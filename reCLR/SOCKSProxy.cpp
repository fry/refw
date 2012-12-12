#include "reCLR.h"

#define WIN32_LEAN_AND_MEAN
#include <Winsock2.h>
#include <string>
#include <vector>
#include <cassert>
#include <map>

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

sockaddr_in g_proxyService;
bool g_allow_user_pass_auth = false;
std::string g_username;
std::string g_password;
std::map<SOCKET, std::shared_ptr<SocknameWrapper>> g_real_socknames;

int getsockname_hook(SOCKET s, sockaddr *name, int *namelen) {
	auto iter = g_real_socknames.find(s);
	if (iter != g_real_socknames.end()) {
		auto name_override = iter->second;

		// Output buffer is too small
		if (*namelen < name_override->namelen) {
			WSASetLastError(WSAEFAULT);
			return SOCKET_ERROR;
		}

		// Copy name override to outbut buffer
		const sockaddr_in* name_in = (const sockaddr_in*)name_override->name;
		memcpy(name, name_override->name, name_override->namelen);
		*namelen = name_override->namelen;
		return 0;
	}

	return getsockname(s, name, namelen);
}

int connect_hook(SOCKET s, const sockaddr *name, int namelen) {
	assert(name->sa_family == AF_INET);
	const sockaddr_in* name_in = (const sockaddr_in*)name;

	char* addr_str = inet_ntoa(name_in->sin_addr);
	unsigned short port = ntohs(name_in->sin_port);

	// don't detour localhost loopback
	if (name_in->sin_addr.S_un.S_addr == inet_addr("127.0.0.1")) {
		return connect(s, name, namelen);
	}

	// Remember original socket name. We are kind of leaking memory here until
	// we start hooking closesocket() and deallocating, but socket number
	// reuse and low number of sockets shouldn't make this matter anyway
	g_real_socknames[s] = std::shared_ptr<SocknameWrapper>(new SocknameWrapper(name, namelen));

	unsigned long blocking = 0;

	// Keep waiting for connect to finish if this is a non-blocking socket
	int wsa_error_code = 0;
	int error_code = 0;
	bool first_try = true;

	// Windows offers no way to check a socket's blocking state, since /normally/
	// you would already know this. We don't, so we have to handle a possible
	// WSAEWOULDBLOCK
	while (true) {
		error_code = connect(s, (sockaddr*)&g_proxyService, sizeof(g_proxyService));
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
		MessageBox::Show(String::Format("Connection to proxy failed: {0}, {1}", error_code, wsa_error_code));
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
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	// receive server's choice of method
	ServerChoicePacket server_choice_packet;
	error_code = recv(s, (char*)&server_choice_packet, sizeof(ServerChoicePacket), MSG_WAITALL);

	if (error_code == SOCKET_ERROR) {
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	if (server_choice_packet.method == 0xFF) {
		MessageBox::Show("SOCKS5 proxy does not support authentication methods 0 or 2");
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
			WSASetLastError(WSAENETDOWN);
			return SOCKET_ERROR;
		}

		// receive response
		ServerAuthenticationResponsePacket auth_response_packet;
		error_code = recv(s, (char*)&auth_response_packet, sizeof(ServerAuthenticationResponsePacket), MSG_WAITALL);

		if (error_code == SOCKET_ERROR) {
			WSASetLastError(WSAENETDOWN);
			return SOCKET_ERROR;
		}

		if (auth_response_packet.status != 0x00) {
			MessageBox::Show("SOCKS5 proxy authentication failed");
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
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	std::vector<unsigned char> data_response;
	data_response.resize(10);
	error_code = recv(s, (char*)&data_response.front(), data_response.size(), MSG_WAITALL);

	if (error_code == SOCKET_ERROR) {
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}	

	// SOCKS5
	assert(data_response[0] == 0x05);
	// IPv4 address
	assert(data_response[3] == 0x01);

	if (data_response[1] != 0x00) {
		MessageBox::Show(String::Format("SOCKS5 proxy connection request failed: error code {0}", data_response[1]));
		closesocket(s);
		WSASetLastError(WSAENETDOWN);
		return SOCKET_ERROR;
	}

	ioctlsocket(s, FIONBIO, &blocking);

	return 0;
}

int reCLR::Loader::ConnectHookWrapper(IntPtr s, IntPtr name, int namelen) {
	return connect_hook((SOCKET)s.ToInt32(), (sockaddr*)name.ToInt32(), namelen);
}

int reCLR::Loader::GetsocknameHookWrapper(IntPtr s, IntPtr name, IntPtr namelen) {
	return getsockname_hook((SOCKET)s.ToInt32(), (sockaddr*)name.ToInt32(), (int*)namelen.ToPointer());
}

void reCLR::Loader::SetProxy(String^ addr, int port) {
	marshal_context context;

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
