using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace refw {
	public static class Loader {
		/// <summary>
		/// Create a new process and inject a managed DLL into it without passing any arguments to the assembly. The process's main thread is
		/// launched in a suspended status and the injected assembly is required to call WakeUpProcess once it finished its initialization routine.
		/// After injection the normal entry point routine of the assembly will be invoked.
		/// </summary>
		/// <param name="process_name">file name of the executable to launch</param>
		/// <param name="command_line">command line arguments to the process, preformatted</param>
		/// <param name="assembly">file name of the assembly to inject</param>
		/// <param name="display_errors">whether to display a message box when an error happens (useful for debugging injection errors)</param>
		/// <returns></returns>
		public static int CreateProcessAndInject(string process_name, string command_line, string assembly, bool display_errors = false) {
			return reCLR.Loader.CreateProcessAndInject(process_name, command_line, assembly, display_errors);
		}

		/// <summary>
		/// Create a new process and inject a managed DLL into it. The process's main thread is
		/// launched in a suspended status and the injected assembly is required to call WakeUpProcess once it finished its initialization routine.
		/// After injection the normal entry point routine of the assembly will be invoked.
		/// </summary>
		/// <param name="process_name">file name of the executable to launch</param>
		/// <param name="command_line">command line arguments to the process, preformatted</param>
		/// <param name="assembly">file name of the assembly to inject</param>
		/// <param name="assembly_args">command line arguments to the injected assembly, preformatted</param>
		/// <param name="display_errors">whether to display a message box when an error happens (useful for debugging injection errors)</param>
		/// <returns></returns>
		public static int CreateProcessAndInject(string process_name, string command_line, string assembly, string assembly_args, bool display_errors = false) {
			return reCLR.Loader.CreateProcessAndInject(process_name, command_line, assembly, assembly_args, display_errors);
		}

		/// <summary>
		/// Create a new process and inject a managed DLL into it. The process's main thread is
		/// launched in a suspended status and the injected assembly is required to call WakeUpProcess once it finished its initialization routine.
		/// After injection the normal entry point routine of the assembly will be invoked.
		/// </summary>
		/// <param name="process_name">file name of the executable to launch</param>
		/// <param name="args">array of command line arguments to the process</param>
		/// <param name="assembly">file name of the assembly to inject</param>
		/// <param name="assembly_args">array of command line arguments to the injecteed assembly</param>
		/// <param name="display_errors">whether to display a message box when an error happens (useful for debugging injection errors)</param>
		/// <returns></returns>
		public static int CreateProcessAndInject(string process_name, string[] args, string assembly, string[] assembly_args, bool display_errors = false) {
			return reCLR.Loader.CreateProcessAndInject(process_name, BuildCommandLine(args), assembly, BuildCommandLine(assembly_args), display_errors);
		}

		static string BuildCommandLine(string[] args) {
			return String.Join(" ", args.Select(s => String.Format("\"{0}\"", s)));
		}

		/// <summary>
		/// Used by injected assemblies to wake up the main thread after assembly initialization is finished. This is required so no race-conditions
		/// turn up during launch if the injected assembly needs to, for example, hook startup functions of the process.
		/// </summary>
		[DllImport("reCLR.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void WakeUpProcess();
	}
}
