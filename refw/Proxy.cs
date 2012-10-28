using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;

namespace refw {
	public static class Proxy {
		public delegate int ConnectDelegate(IntPtr s, IntPtr name, int namelen);

		static LocalHook proxyHook;
		static ConnectDelegate proxyDetour;

		public static bool IsEnabled {
			set {
				if (proxyHook == null && value) {
					var connect_orig = LocalHook.GetProcAddress("Ws2_32", "connect");
					proxyDetour = new ConnectDelegate(reCLR.Loader.ConnectHookWrapper);
					proxyHook = LocalHook.Create(connect_orig, proxyDetour, null);
					proxyHook.ThreadACL.SetExclusiveACL(new int[] { });
				} else if (proxyHook != null && !value) {
					proxyHook.Dispose();
					proxyHook = null;
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
