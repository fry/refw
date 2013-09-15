using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace refw {
	public class MemoryPatch: IDisposable {
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
		   uint flNewProtect, out uint lpflOldProtect);

		IntPtr address;
		byte[] patch;
		byte[] backup;
		bool isEnabled = false;

		public MemoryPatch(IntPtr addr, byte[] patch) {
			this.address = addr;
			this.patch = patch;
			this.backup = new byte[patch.Length];
		}

		~MemoryPatch() {
			Dispose();
		}

		public bool Enabled {
			get {
				return isEnabled;
			}

			set {
				if (value != isEnabled) {
					// Unprotect and reprotect the memory area to patch
					uint old_protect;
					VirtualProtect(address, (uint)patch.Length, 0x04, out old_protect);

					if (value && !isEnabled) {
						// Save backup and patch the bytes
						Marshal.Copy(address, backup, 0, patch.Length);
						Marshal.Copy(patch, 0, address, patch.Length);
					} else if (!value && isEnabled) {
						// Restore backup
						Marshal.Copy(backup, 0, address, patch.Length);
					}

					VirtualProtect(address, (uint)patch.Length, old_protect, out old_protect);
				}

				isEnabled = value;
			}
		}

		public void Dispose() {
			Enabled = false;
		}
	}
}
