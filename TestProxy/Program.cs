using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace TestProxy {
	public class Program {
		[STAThread]
		public static void Main() {
			refw.Proxy.SetProxy("207.181.205.199", 15374);
			refw.Proxy.EnableSOCKS5();

			refw.Loader.WakeUpProcess();
		}
	}
}
