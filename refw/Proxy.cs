using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;

namespace refw {
	public static class Proxy {
		public delegate int ConnectDelegate(IntPtr s, IntPtr name, int namelen);
        public delegate int GetsocknameDelegate(IntPtr s, IntPtr name, IntPtr namelen);

		static LocalHook proxyHook;
        static LocalHook proxyNameHook;
		static ConnectDelegate proxyDetour;
        static GetsocknameDelegate proxyNameDetour;

		public static bool IsEnabled {
			set {
				if (proxyHook == null && value) {
					var connect_orig = LocalHook.GetProcAddress("Ws2_32", "connect");
					proxyDetour = new ConnectDelegate(reCLR.Loader.ConnectHookWrapper);
					proxyHook = LocalHook.Create(connect_orig, proxyDetour, null);
					proxyHook.ThreadACL.SetExclusiveACL(new int[] { });

                    var getsockname_orig = LocalHook.GetProcAddress("Ws2_32", "getsockname");
                    proxyNameDetour = new GetsocknameDelegate(reCLR.Loader.GetsocknameHookWrapper);
                    proxyNameHook = LocalHook.Create(getsockname_orig, proxyNameDetour, null);
                    proxyNameHook.ThreadACL.SetExclusiveACL(new int[] { });
				} else if (proxyHook != null && !value) {
					proxyHook.Dispose();
					proxyHook = null;

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
	}
}
