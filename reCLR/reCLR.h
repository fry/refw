#pragma once

#include "CLRParameters.h"
#include "DomainWorker.h"

using namespace System;

namespace reCLR {
	public enum ProxyErrorType {
		ConnectFailed,
		AuthFailed
	};

	public delegate void OnProxyErrorDelegate(ProxyErrorType type, String^ message);

	public ref class Loader {
	public:
		static AppDomain^ RefwDomain;
		static String^ LoadedAssemblyPath;
		static String^ LoadedAssemblyArgs;

		static OnProxyErrorDelegate^ OnProxyError;
		static bool ProxyEnabled;

		static void Inject(int process_id, String^ assembly, String^ assembly_args, bool display_errors, int wakeup_thread_id);
		static int CreateProcessAndInject(String^ process_name, String^ command_line, String^ assembly, bool display_errors);
		static int CreateProcessAndInject(String^ process_name, String^ command_line, String^ assembly, String^ assembly_args, bool display_errors);
		static DomainWorker^ LoadAssemblyInDomain(String^ assembly_path, String^ assembly_args);

		static void UnloadAndReloadDomain();

		//static IntPtr InternetOpenHookWrapper(IntPtr lpszAgent, IntPtr dwAccessType, IntPtr lpszProxyName, IntPtr lpszProxyBypass, IntPtr dwFlags);
		static int WSAIoctlWrapper(IntPtr s, IntPtr dwIoControlCode, IntPtr lpvInBuffer, IntPtr cbInBuffer, IntPtr lpvOutBuffer, IntPtr cbOutBuffer,
			IntPtr lpcbBytesReturned, IntPtr lpOverlapped, IntPtr lpCompletionRoutine);
		static int WSAConnectHookWrapper(IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData, IntPtr lpCalleeData, IntPtr lpSQOS, IntPtr lpGQOS);
		static int ConnectHookWrapper(IntPtr s, IntPtr name, int namelen);
		static int GetpeernameHookWrapper(IntPtr s, IntPtr name, IntPtr namelen);
		static void SetProxy(String^ addr, int port);
		static void SetProxyAuth(String^ username, String^ password);

		static void test();
	};
}
