using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;

namespace refw {
	public static class Proxy {
        // Proxy callback enums repeated from reCLR so users of the library don't have to reference reCLR
        public enum ProxyErrorType {
            ConnectFailed,
            AuthFailed
        }

        public delegate void OnProxyErrorDelegate(ProxyErrorType type, string message);

        public static event OnProxyErrorDelegate OnProxyError;

		public delegate int ConnectDelegate(IntPtr s, IntPtr name, int namelen);
		public delegate int WSAConnectDelegate(IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData, IntPtr lpCalleeData, IntPtr lpSQOS, IntPtr lpGQOS);
        public delegate int GetsocknameDelegate(IntPtr s, IntPtr name, IntPtr namelen);

		static LocalHook proxyHook;
		static LocalHook proxyHookWSA;
        static LocalHook proxyNameHook;
		static ConnectDelegate proxyDetour;
		static WSAConnectDelegate proxyDetourWSA;
        static GetsocknameDelegate proxyNameDetour;

		public static bool IsEnabled {
			set {
				if (proxyHook == null && value) {
					var connect_orig = LocalHook.GetProcAddress("Ws2_32", "connect");
					proxyDetour = new ConnectDelegate(reCLR.Loader.ConnectHookWrapper);
					proxyHook = LocalHook.Create(connect_orig, proxyDetour, null);
					proxyHook.ThreadACL.SetExclusiveACL(new int[] { });

					var wsa_connect_orig = LocalHook.GetProcAddress("Ws2_32", "WSAConnect");
					proxyDetourWSA = new WSAConnectDelegate(reCLR.Loader.WSAConnectHookWrapper);
					proxyHookWSA = LocalHook.Create(wsa_connect_orig, proxyDetourWSA, null);
					proxyHookWSA.ThreadACL.SetExclusiveACL(new int[] { });

                    var getpeerame_orig = LocalHook.GetProcAddress("Ws2_32", "getpeername");
                    proxyNameDetour = new GetsocknameDelegate(reCLR.Loader.GetpeernameHookWrapper);
                    proxyNameHook = LocalHook.Create(getpeerame_orig, proxyNameDetour, null);
                    proxyNameHook.ThreadACL.SetExclusiveACL(new int[] { });

                    reCLR.Loader.OnProxyError = OnProxyErrorCallback;
				} else if (proxyHook != null && !value) {
					proxyHook.Dispose();
					proxyHook = null;

					proxyHookWSA.Dispose();
					proxyHookWSA = null;

                    proxyNameHook.Dispose();
                    proxyNameHook = null;
				}
			}

			get {
				return proxyHook != null;
			}
		}

		public static void SetProxy(string addr, int port) {
			reCLR.Loader.SetProxy(addr, port);
		}

		public static void SetProxyAuth(string username, string password) {
			reCLR.Loader.SetProxyAuth(username, password);
		}

        static void OnProxyErrorCallback(reCLR.ProxyErrorType error, string msg) {
            if (OnProxyError == null)
                return;

            OnProxyError((ProxyErrorType)error, msg);
        }
	}
}
