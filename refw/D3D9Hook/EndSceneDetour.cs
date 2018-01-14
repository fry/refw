using System;
using System.Diagnostics;
using EasyHook;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace refw.D3D {
	/// <summary>
	/// A DirectX 9 hook class that can be used to ínvoke callbacks during each frame or key presses.
	/// </summary>
	public class EndSceneDetour: DirectXHook, IDisposable {
		/// <summary>
		/// Invoken when the device is created
		/// </summary>
		public event Action OnCreateDevice;
		/// <summary>
		/// Invoked when the DirectX device is reset.
		/// </summary>
		public event Action<IntPtr> OnResetDevice;

		private NativeAPI.Direct3DCreate9Delegate Direct3DCreate9;
		private NativeAPI.Direct3D9CreateDeviceDelegate Direct3D9CreateDevice;
		private NativeAPI.Direct3D9EndScene EndScene;
		private NativeAPI.Direct3D9ResetDelegate Reset;

		private LocalHook hook_d3d_create9 = null;
		private LocalHook hook_d3d9_create_device = null;
		private LocalHook hook_end_scene = null;
		private LocalHook hook_reset = null;

		public IntPtr Direct3D9 { get; private set; }
		public IntPtr Direct3DDevice9 { get; private set; }

		private int EndSceneCallback(IntPtr instance) {
			ArtificialEndScene();

			return (int)EndScene(instance);
		}

		/// <summary>
		/// Start hooking DirectX, this needs to happen during initialization before the process executes.
		/// </summary>
		public void SetupDetour(string dll_name = "d3d9.dll") {
			// Retrieve Direct3DCreate9 function
			var lib_d3d9 = EasyHook.NativeAPI.LoadLibrary(dll_name);
			var create_func = EasyHook.NativeAPI.GetProcAddress(lib_d3d9, "Direct3DCreate9");
			Direct3DCreate9 = Misc.GetDelegate<NativeAPI.Direct3DCreate9Delegate>(create_func);

			// Hook function
			hook_d3d_create9 = LocalHook.Create(create_func, new NativeAPI.Direct3DCreate9Delegate(D3DCreate9Detour), this);
			hook_d3d_create9.ThreadACL.SetExclusiveACL(new Int32[] { });
		}

		IntPtr D3DCreate9Detour(uint sdkVersion) {
			//hook_d3d_create9.Dispose();

			// Retrieve created D3D9 object
			Direct3D9 = Direct3DCreate9(sdkVersion);

			if (hook_d3d9_create_device == null) {
				// Retrieve pointer to Direct3D9CreateDevice function and hook it
				var create_device_func = Misc.GetVTableFunction(Direct3D9, 16);

				Direct3D9CreateDevice = Misc.GetDelegate<NativeAPI.Direct3D9CreateDeviceDelegate>(create_device_func);
				hook_d3d9_create_device = LocalHook.Create(create_device_func, new NativeAPI.Direct3D9CreateDeviceDelegate(D3D9CreateDeviceDetour), this);
				hook_d3d9_create_device.ThreadACL.SetExclusiveACL(new Int32[] { });
			}

			return Direct3D9;
		}

		int D3D9CreateDeviceDetour(IntPtr instance, uint adapter, uint deviceType, IntPtr focusWindow, uint behaviorFlags, ref NativeAPI.PresentParameters presentationParameters, out IntPtr returnedDeviceInterface) {
			// Invoke original create device function and retrieve the created device
			var result = Direct3D9CreateDevice(instance, adapter, deviceType, focusWindow, behaviorFlags, ref presentationParameters, out returnedDeviceInterface);
			Direct3DDevice9 = returnedDeviceInterface;
			if (hook_end_scene == null) {
                SetWindow(focusWindow);

				// Hook EndScene
				var end_scene_pointer = Misc.GetVTableFunction(Direct3DDevice9, NativeAPI.EndSceneOffset);
				EndScene = Misc.GetDelegate<NativeAPI.Direct3D9EndScene>(end_scene_pointer);
				hook_end_scene = LocalHook.Create(end_scene_pointer, new NativeAPI.Direct3D9EndScene(EndSceneCallback), this);
				hook_end_scene.ThreadACL.SetExclusiveACL(new Int32[] { });
			}


			if (hook_reset == null) {
				// Hook Reset
				var reset_pointer = Misc.GetVTableFunction(Direct3DDevice9, NativeAPI.ResetOffset);
				Reset = Misc.GetDelegate<NativeAPI.Direct3D9ResetDelegate>(reset_pointer);
				hook_reset = LocalHook.Create(reset_pointer, new NativeAPI.Direct3D9ResetDelegate(D3D9ResetDetour), this);
				hook_reset.ThreadACL.SetExclusiveACL(new Int32[] { });
			}

			try {
				if (OnCreateDevice != null)
					OnCreateDevice();
			} catch (Exception e) {
				MessageBox.Show(e.ToString());
			}

			return result;
		}

		private int D3D9ResetDetour(IntPtr instance, ref NativeAPI.PresentParameters presentationParameters) {
			if (OnResetDevice != null)
				OnResetDevice(instance);

			var ret = Reset(instance, ref presentationParameters);
		    return ret;
		}

		public new void Dispose() {
            base.Dispose();

			if (hook_d3d_create9 != null)
				hook_d3d_create9.Dispose();
			if (hook_d3d9_create_device != null)
				hook_d3d9_create_device.Dispose();
			if (hook_end_scene != null)
				hook_end_scene.Dispose();
		}
	}
}
