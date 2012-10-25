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

		public static void EnableSOCKS5() {
			var connect_orig = LocalHook.GetProcAddress("Ws2_32", "connect");
			proxyDetour = new ConnectDelegate(reCLR.Loader.ConnectHookWrapper);
			proxyHook = LocalHook.Create(connect_orig, proxyDetour, null);
			proxyHook.ThreadACL.SetExclusiveACL(new int[] { });
		}

		public static void SetProxy(string addr, int port) {
			reCLR.Loader.SetProxy(addr, port);
		}
	}
}
