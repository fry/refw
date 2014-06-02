#include "reCLR.h"

#define WIN32_LEAN_AND_MEAN
#include <Winsock2.h>
#include <string>
#include <vector>
#include <cassert>
#include <map>
#include <Wininet.h>
#include <mswsock.h>

#include <msclr\marshal_cppstd.h>
#include <msclr\marshal.h>

using namespace msclr::interop;
using namespace System;
using namespace System::Windows::Forms;

struct ServerChoicePacket {
	unsigned char version;
	unsigned char method;
};

struct ServerAuthenticationResponsePacket {
	unsigned char version;
	unsigned char status;
};

// Wrapper for socket name. We still need to allocate this on the heap, since
// we don't implement copy/move assignment operators.
struct SocknameWrapper {
	sockaddr* name;
	int namelen;

	SocknameWrapper(const sockaddr* name, int namelen) {
		this->namelen = namelen;
		this->name = (sockaddr*)malloc(namelen);
		memcpy(this->name, name, namelen);
	}

	~SocknameWrapper() {
		if (name != nullptr) {
			free(name);
		}
	}
};

std::string g_proxyHost;
int g_proxyPort;
sockaddr_in g_proxyService;
bool g_allow_user_pass_auth = false;
LPFN_CONNECTEX g_ConnectEx;
std::string g_username;
std::string g_password;
std::map<SOCKET, std::shared_ptr<SocknameWrapper>> g_real_peernames;

int getpeername_hook(SOCKET s, sockaddr *name, int *namelen) {
	auto iter = g_real_peernames.find(s);
	if (iter != g_real_peernames.end()) {
		auto name_override = iter->second;

		// Output buffer is too small
		if (*namelen < name_override->namelen) {
			WSASetLastError(WSAEFAULT);
			return SOCKET_ERROR;
		}

		// Copy name override to output buffer
		const sockaddr_in* name_in = (const sockaddr_in*)name_override->name;

		memcpy(name, name_override->name, name_override->namelen);
		*namelen = name_override->namelen;
		return 0;
	}

	return getpeername(s, name, namelen);
}

enum ConnectFunc {
	CF_CONNECT, CF_WSACONNECT, CF_CONNECTEX
};

int proxy_negotiate(ConnectFunc func, SOCKET s, const sockaddr *name, int namelen,
					LPWSABUF lpCallerData, LPWSABUF lpCalleeData, LPQOS lpSQOS, LPQOS lpGQOS,
					/*PVOID lpSendBuffer, DWORD dwSendDataLength, */ LPDWORD lpdwBytesSent, LPOVERLAPPED lpOverlapped) {
	assert(name->sa_family == AF_INET);
	assert(func == CF_CONNECT || func == CF_WSACONNECT || CF_CONNECTEX);
	const sockaddr_in* name_in = (const sockaddr_in*)name;

	char* addr_str = inet_ntoa(name_in->sin_addr);
	unsigned short port = ntohs(name_in->sin_port);

	// don't detour localhost loopback
	if (    name_in->sin_addr.S_un.S_addr == inet_addr("127.0.0.1")
		|| (name_in->sin_addr.S_un.S_addr == inet_addr(g_proxyHost.c_str()) && port == g_proxyPort)
		|| name_in->sin_addr.S_un.S_addr == inet_addr("0.0.0.0")) {
		if (func == CF_CONNECT)
			return connect(s, name, namelen);
		else if (func == CF_WSACONNECT)
			return WSAConnect(s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS);
		else if (func == CF_CONNECTEX)
			return g_ConnectEx(s, name, namelen, NULL, 0, lpdwBytesSent, lpOverlapped);
	}

	unsigned long blocking = 0;

	// Keep waiting for connect to finish if this is a non-blocking socket
	int wsa_error_code = 0;
	int error_code = 0;
	bool first_try = true;

	// Windows offers no way to check a socket's blocking state, since /normally/
	// you would already know this. We don't, so we have to handle a possible
	// WSAEWOULDBLOCK
	while (true) {
		if (func == CF_CONNECT)
			error_code = connect(s, (sockaddr*)&g_proxyService, sizeof(g_proxyService));
		else if (func == CF_WSACONNECT)
			error_code = WSAConnect(s, (sockaddr*)&g_proxyService, sizeof(g_proxyService), lpCallerData, lpCalleeData, lpSQOS, lpGQOS);
		else if (func == CF_CONNECTEX) {
			bool success = g_ConnectEx(s, (sockaddr*)&g_proxyService, sizeof(g_proxyService), NULL, 0, lpdwBytesSent, lpOverlapped);
			if (!success && WSAGetLastError() == WSA_IO_PENDING) {
				DWORD transferred;
				success = GetOverlappedResult((HANDLE)s, lpOverlapped, &transferred, TRUE);
				error_code = 0;
				break;
			}

		}

		if (error_code != 0) {
			wsa_error_code = WSAGetLastError();
			// Socket discovered to be blocking -> set to non-blocking & remember old state
			if (wsa_error_code == WSAEWOULDBLOCK) {
				ioctlsocket(s, FIONBIO, &blocking);
				blocking = 1;
			}
			// Wait until connection attempt finishes
			else if (wsa_error_code != WSAEINVAL && wsa_error_code != WSAEALREADY)
				break;
			first_try = false;
		}
	}
	
	// Only when we get WSAEISCONN on the first connect attempt is this an
	// actual error, otherwise we just finished the non-blocking connection
	// attempt
	if (error_code != 0 && (wsa_error_code != WSAEISCONN && !first_try)) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, String::Format("Connection to proxy failed: {0}, {1}", error_code, wsa_error_code));
		return error_code;
	}
	
	// send identify request
	std::vector<unsigned char> data_identify;
	// SOCKS version
	data_identify.push_back(0x05);
	// number of methods
	if (g_allow_user_pass_auth)
		data_identify.push_back(2);
	else
		data_identify.push_back(1);
	// methods
	data_identify.push_back(0x00);
	if (g_allow_user_pass_auth)
		data_identify.push_back(0x02);

	error_code = send(s, (char*)&data_identify.front(), data_identify.size(), 0);

	if (error_code == SOCKET_ERROR) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, "Send 1 failed");
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	// receive server's choice of method
	ServerChoicePacket server_choice_packet;
	error_code = recv(s, (char*)&server_choice_packet, sizeof(ServerChoicePacket), MSG_WAITALL);

	if (error_code == SOCKET_ERROR) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, "Recv 1 failed");
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	if (server_choice_packet.method == 0xFF) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::AuthFailed, "SOCKS5 proxy does not support authentication methods 0 or 2");
		closesocket(s);
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	// authenticate
	if (server_choice_packet.method == 0x02) {
		// send authentication request
		std::vector<unsigned char> data;
		data.push_back(0x01);
		data.push_back(g_username.size());
		data.insert(data.end(), g_username.begin(), g_username.end());
		data.push_back(g_password.size());
		data.insert(data.end(), g_password.begin(), g_password.end());

		error_code = send(s, (char*)&data.front(), data.size(), 0);
		if (error_code == SOCKET_ERROR) {
			if (reCLR::Loader::OnProxyError != nullptr)
				reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, "Auth Send 1 failed");
			WSASetLastError(WSAENETDOWN);
			return SOCKET_ERROR;
		}

		// receive response
		ServerAuthenticationResponsePacket auth_response_packet;
		error_code = recv(s, (char*)&auth_response_packet, sizeof(ServerAuthenticationResponsePacket), MSG_WAITALL);

		if (error_code == SOCKET_ERROR) {
			if (reCLR::Loader::OnProxyError != nullptr)
				reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, "Auth Recv 1 failed");
			WSASetLastError(WSAENETDOWN);
			return SOCKET_ERROR;
		}

		if (auth_response_packet.status != 0x00) {
			if (reCLR::Loader::OnProxyError != nullptr)
				reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::AuthFailed, "SOCKS5 proxy authentication failed");
			closesocket(s);
			WSASetLastError(WSAENETDOWN);
			return SOCKET_ERROR;
		}
	}

	// send connection request
	std::vector<unsigned char> data_request;
	// version
	data_request.push_back(0x05);
	// command code
	data_request.push_back(0x01);
	data_request.push_back(0x00);
	// address type
	data_request.push_back(0x01);
	// address
	data_request.push_back(name_in->sin_addr.S_un.S_un_b.s_b1);
	data_request.push_back(name_in->sin_addr.S_un.S_un_b.s_b2);
	data_request.push_back(name_in->sin_addr.S_un.S_un_b.s_b3);
	data_request.push_back(name_in->sin_addr.S_un.S_un_b.s_b4);
	// port (it's already stored in network byte order)
	data_request.push_back((unsigned char)name_in->sin_port);
	data_request.push_back((unsigned char)(name_in->sin_port >> 8));

	error_code = send(s, (char*)&data_request.front(), data_request.size(), 0);

	if (error_code == SOCKET_ERROR) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, "Send 2 failed"); 
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	std::vector<unsigned char> data_response;
	data_response.resize(10);
	error_code = recv(s, (char*)&data_response.front(), data_response.size(), MSG_WAITALL);

	if (error_code == SOCKET_ERROR) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, "Recv 2 failed");
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}	

	// SOCKS5
	assert(data_response[0] == 0x05);
	// IPv4 address
	assert(data_response[3] == 0x01);

	if (data_response[1] != 0x00) {
		if (reCLR::Loader::OnProxyError != nullptr)
			reCLR::Loader::OnProxyError(reCLR::ProxyErrorType::ConnectFailed, String::Format("SOCKS5 proxy connection request failed: error code {0}", data_response[1]));
		closesocket(s);
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	ioctlsocket(s, FIONBIO, &blocking);

	// Remember original socket name. We are kind of leaking memory here until
	// we start hooking closesocket() and deallocating, but socket number
	// reuse and low number of sockets shouldn't make this matter anyway
	g_real_peernames[s] = std::shared_ptr<SocknameWrapper>(new SocknameWrapper(name, namelen));

	return 0;
}

bool PASCAL ConnectEx_wrapper(SOCKET s, const struct sockaddr *name, int namelen, PVOID lpSendBuffer, DWORD dwSendDataLength,
					   LPDWORD lpdwBytesSent, LPOVERLAPPED lpOverlapped) {
	//bool result = g_ConnectEx(s, name, namelen, lpSendBuffer, dwSendDataLength, lpdwBytesSent, lpOverlapped);
	int result = proxy_negotiate(CF_CONNECTEX, s, name, namelen, NULL, NULL, NULL, NULL, lpdwBytesSent, lpOverlapped);
	if (result != 0)
		return false;

	if (dwSendDataLength > 0) {
		WSABUF buf;
		buf.buf = (CHAR*)lpSendBuffer;
		buf.len = dwSendDataLength;

		DWORD bytesSent;
		int result = WSASend(s, &buf, 1, &bytesSent, 0, NULL, NULL);
		*lpdwBytesSent += bytesSent;
		return result == 0;
	}

	WSASetLastError(WSA_IO_PENDING);
	return false;
}

int WSAIoctl_hook(SOCKET s, DWORD dwIoControlCode, LPVOID lpvInBuffer, DWORD cbInBuffer, LPVOID lpvOutBuffer, DWORD cbOutBuffer,
			 LPDWORD lpcbBytesReturned, LPWSAOVERLAPPED lpOverlapped, LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine) {
	int result = WSAIoctl(s, dwIoControlCode, lpvInBuffer, cbInBuffer, lpvOutBuffer, cbOutBuffer, lpcbBytesReturned, lpOverlapped, lpCompletionRoutine);

	if (result == 0 && dwIoControlCode == SIO_GET_EXTENSION_FUNCTION_POINTER) {
		GUID guid = WSAID_CONNECTEX;
		GUID* buffer_guid = (GUID*)lpvInBuffer;
		if (memcmp(lpvInBuffer, &guid, cbInBuffer) == 0) {
			// Save original function
			g_ConnectEx = *(LPFN_CONNECTEX*)lpvOutBuffer;
			// Return our wrapper
			*(DWORD*)lpvOutBuffer = (DWORD)&ConnectEx_wrapper;
		}
	}
	return result;
}

/*SOCKET WSASocket_hook(int af, int type, int protocol, LPWSAPROTOCOL_INFO lpProtocolInfo, GROUP g, DWORD dwFlags) {
	SOCKET s = WSASocket(af, type, protocol, lpProtocolInfo, g, dwFlags);
	return s;
}*/

/*HINTERNET InternetOpen_hook(LPCTSTR lpszAgent, DWORD dwAccessType, LPCTSTR lpszProxyName, LPCTSTR lpszProxyBypass, DWORD dwFlags) {
	marshal_context context;

	dwAccessType = INTERNET_OPEN_TYPE_PROXY;
	String^ proxy_str = String::Format("socks={0}:{1}", gcnew String(g_proxyHost.c_str()), g_proxyPort);

	auto inet = InternetOpen(lpszAgent, dwAccessType, context.marshal_as<const char*>(proxy_str), lpszProxyBypass, dwFlags);
	InternetSetOption(inet, INTERNET_OPTION_PROXY_USERNAME, (void*)g_username.c_str(), g_username.length());
	InternetSetOption(inet, INTERNET_OPTION_PROXY_PASSWORD, (void*)g_password.c_str(), g_password.length());

	return inet;
}*/

int WSAConnect_hook(SOCKET s, const sockaddr *name, int namelen, LPWSABUF lpCallerData, LPWSABUF lpCalleeData, LPQOS lpSQOS, LPQOS lpGQOS) {
	return proxy_negotiate(CF_WSACONNECT, s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS, NULL, NULL);
}

int connect_hook(SOCKET s, const sockaddr *name, int namelen) {
	return proxy_negotiate(CF_CONNECT, s, name, namelen, NULL, NULL, NULL, NULL, NULL, NULL);
}

/*IntPtr reCLR::Loader::InternetOpenHookWrapper(IntPtr lpszAgent, IntPtr dwAccessType, IntPtr lpszProxyName, IntPtr lpszProxyBypass, IntPtr dwFlags) {
	return (IntPtr)InternetOpen_hook((LPCTSTR)lpszAgent.ToInt32(), (DWORD)dwAccessType.ToInt32(), (LPCTSTR)lpszProxyName.ToInt32(), (LPCTSTR)lpszProxyBypass.ToInt32(), (DWORD)dwFlags.ToInt32());
}*/

int reCLR::Loader::WSAConnectHookWrapper(IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData, IntPtr lpCalleeData, IntPtr lpSQOS, IntPtr lpGQOS) {
	return WSAConnect_hook((SOCKET)s.ToInt32(), (sockaddr*)name.ToInt32(), namelen,
		(LPWSABUF)lpCallerData.ToInt32(), (LPWSABUF)lpCalleeData.ToInt32(), (LPQOS)lpSQOS.ToInt32(), (LPQOS)lpGQOS.ToInt32());
}

int reCLR::Loader::ConnectHookWrapper(IntPtr s, IntPtr name, int namelen) {
	return connect_hook((SOCKET)s.ToInt32(), (sockaddr*)name.ToInt32(), namelen);
}

int reCLR::Loader::GetpeernameHookWrapper(IntPtr s, IntPtr name, IntPtr namelen) {
	return getpeername_hook((SOCKET)s.ToInt32(), (sockaddr*)name.ToInt32(), (int*)namelen.ToPointer());
}

int reCLR::Loader::WSAIoctlWrapper(IntPtr s, IntPtr dwIoControlCode, IntPtr lpvInBuffer, IntPtr cbInBuffer, IntPtr lpvOutBuffer, IntPtr cbOutBuffer,
			 IntPtr lpcbBytesReturned, IntPtr lpOverlapped, IntPtr lpCompletionRoutine) {
	return WSAIoctl_hook((SOCKET)s.ToInt32(), (DWORD)dwIoControlCode.ToInt32(), (LPVOID)lpvInBuffer, (DWORD)cbInBuffer.ToInt32(),
		(LPVOID)lpvOutBuffer, (DWORD)cbOutBuffer.ToInt32(), (LPDWORD)lpcbBytesReturned.ToInt32(), (LPWSAOVERLAPPED)lpOverlapped.ToInt32(),
		(LPWSAOVERLAPPED_COMPLETION_ROUTINE)lpCompletionRoutine.ToInt32());
}

void reCLR::Loader::SetProxy(String^ addr, int port) {
	marshal_context context;

	g_proxyHost = context.marshal_as<std::string>(addr);
	g_proxyPort = port;

	g_proxyService.sin_family = AF_INET;
	g_proxyService.sin_addr.s_addr = inet_addr(context.marshal_as<const char*>(addr));
	g_proxyService.sin_port = htons(port);
}

void reCLR::Loader::SetProxyAuth(String^ username, String^ password) {
	marshal_context context;

	if (username != nullptr && password != nullptr) {
		g_allow_user_pass_auth = true;
		g_username = context.marshal_as<const char*>(username);
		g_password = context.marshal_as<const char*>(password);
	} else {
		g_allow_user_pass_auth = false;
	}
}
