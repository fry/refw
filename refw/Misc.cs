using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using EasyHook;

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

        // GetAdaptersInfo Hook
	    private static LocalHook getAdaptersInfoHook;
	    private static NativeAPI.GetAdaptersInfo GetAdaptersInfo;

	    private static int MyGetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen) {
	        var result = GetAdaptersInfo(pAdapterInfo, ref pBufOutLen);
	        if (result != 0)
	            return result;

	        var entry = pAdapterInfo;

            var random = new Random(Identity.GetHashCode());

	        do {
	            var structure = (NativeAPI.IP_ADAPTER_INFO)Marshal.PtrToStructure(entry, typeof (NativeAPI.IP_ADAPTER_INFO));

                // Randomize the MAC address
                var random_data = new byte[structure.AddressLength];
                random.NextBytes(random_data);
                Array.Copy(random_data, structure.Address, random_data.Length);
                
                Marshal.StructureToPtr(structure, entry, false);
                entry = structure.Next;

	        } while (entry != IntPtr.Zero);

	        return result;
	    }

	    private static string identityStr;

	    public static string Identity {
	        get {
	            return identityStr;
	        }
	        set {
	            identityStr = value;

	            if (value != null) {
	                if (getAdaptersInfoHook == null) {
	                    var addr = LocalHook.GetProcAddress("iphlpapi.dll", "GetAdaptersInfo");
	                    GetAdaptersInfo = refw.Misc.GetDelegate<NativeAPI.GetAdaptersInfo>(addr);
	                    getAdaptersInfoHook = LocalHook.Create(addr, new NativeAPI.GetAdaptersInfo(MyGetAdaptersInfo), null);
	                }
	                getAdaptersInfoHook.ThreadACL.SetExclusiveACL(new Int32[] {});
	            } else {
                    getAdaptersInfoHook.ThreadACL.SetInclusiveACL(new Int32[] { });
	            }
	        }
	    }
	}
}
