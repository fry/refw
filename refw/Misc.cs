using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace refw {
	public class Misc {
		/// <summary>
		/// Turn a native function pointer into a callable managed delegate.
		/// </summary>
		/// <typeparam name="T">type of the delegate</typeparam>
		/// <param name="offset">address of the function</param>
		/// <returns>an instance of the delegate for the passed address</returns>
		public static T GetDelegate<T>(uint offset) where T: class {
			return GetDelegate<T>(new IntPtr(offset));
		}

		/// <summary>
		/// Turn a native function pointer into a callable managed delegate.
		/// </summary>
		/// <typeparam name="T">type of the delegate</typeparam>
		/// <param name="offset">address of the function</param>
		/// <returns>an instance of the delegate for the passed address</returns>
		public static T GetDelegate<T>(int offset) where T: class {
			return GetDelegate<T>(new IntPtr(offset));
		}

		/// <summary>
		/// Turn a native function pointer into a callable managed delegate.
		/// </summary>
		/// <typeparam name="T">type of the delegate</typeparam>
		/// <param name="offset">address of the function</param>
		/// <returns>an instance of the delegate for the passed address</returns>
		public static T GetDelegate<T>(IntPtr offset) where T: class {
			return Marshal.GetDelegateForFunctionPointer(offset, typeof(T)) as T;
		}

		public static unsafe IntPtr GetVTableFunction(IntPtr obj, int index) {
			return Marshal.ReadIntPtr(Marshal.ReadIntPtr(obj), index * 4);
		}

		/// <summary>
		/// Rebase an offset by the process's main module address.
		/// </summary>
		/// <param name="addr">address relative to the main module</param>
		/// <returns>address rebased by the main module's address</returns>
		public static IntPtr GetAddress(uint addr) {
			var base_address = (uint)Process.GetCurrentProcess().MainModule.BaseAddress;
			return new IntPtr(base_address + addr);
		}
	}
}
