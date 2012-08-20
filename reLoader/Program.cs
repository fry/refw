using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace reLoader {
	class Program {
		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("Usage: {0} <target> <assembly> [<parameters>] [<show_errors = false>] [<assembly_args>]", Path.GetFileName(Assembly.GetEntryAssembly().Location));
				return;
			}

			var arguments = args.Length >= 3 ? args[2] : "";
			var display_errors = args.Length >= 4 ? args[3] != "false" : false;
			var assembly_args = args.Length >= 5 ? args[4] : "";

			var process_id = refw.Loader.CreateProcessAndInject(args[0], arguments, args[1], assembly_args, display_errors);
			Console.WriteLine("Injected assembly '{0}' into '{1}', process id: {2} with assembly args: '{3}'", args[1], args[0], process_id, assembly_args);
		}
	}
}
