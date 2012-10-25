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
	public class EndSceneDetour: IDisposable {
		/// <summary>
		/// Event that is invoked every frame, the time since the last frame is passed as an argument.
		/// </summary>
		public event Action<TimeSpan> OnFrame;
		/// <summary>
		/// Invoken when the device is created
		/// </summary>
		public event Action OnCreateDevice;
		/// <summary>
		/// Invoked when a WndProc key event occurs, with the key code as an argument. The return value of
		/// the function determines whether the event is passed to the application.
		/// </summary>
		public event Func<int, bool> OnKeyPressed;
		/// <summary>
		/// Invoked when the DirectX device is reset.
		/// </summary>
		public event Action<IntPtr> OnResetDevice;

		// Measurements for frame-limiting
		private Stopwatch _stopWatch = new Stopwatch();
		private double _timeDiff = 0;

		private NativeAPI.Direct3DCreate9Delegate Direct3DCreate9;
		private NativeAPI.Direct3D9CreateDeviceDelegate Direct3D9CreateDevice;
		private NativeAPI.Direct3D9EndScene EndScene;
		private NativeAPI.Direct3D9ResetDelegate Reset;

		private NativeAPI.WndProcDelegate wnd_proc_delegate;

		private LocalHook hook_d3d_create9 = null;
		private LocalHook hook_d3d9_create_device = null;
		private LocalHook hook_end_scene = null;
		private LocalHook hook_reset = null;

		private IntPtr old_wndproc;

		public IntPtr Hwnd { get; private set; }

		public IntPtr Direct3D9 { get; private set; }
		public IntPtr Direct3DDevice9 { get; private set; }

		/// <summary>
		/// If set, limits the frames per second to the set value.
		/// </summary>
		public int? FrameLimit = null;

		private int EndSceneCallback(IntPtr instance) {
			long delta_ms = 0;
			if (FrameLimit.HasValue) {
				if (!_stopWatch.IsRunning) {
					_stopWatch.Reset();
					_stopWatch.Start();
				} else {
					delta_ms = _stopWatch.ElapsedMilliseconds;
					double diff = 1000.0 / FrameLimit.Value - delta_ms;
					_timeDiff += diff;
					if (_timeDiff > 0) {
						Thread.Sleep((int)_timeDiff);
						_timeDiff = 0;
					}
					_stopWatch.Reset();
					_stopWatch.Start();
				}
			}

			if (OnFrame != null)
				OnFrame(TimeSpan.FromMilliseconds(delta_ms));

			return (int)EndScene(instance);
		}

		/// <summary>
		/// Start hooking DirectX, this needs to happen during initialization before the process executes.
		/// </summary>
		public void SetupDetour() {
			// Retrieve Direct3DCreate9 function
			var lib_d3d9 = EasyHook.NativeAPI.LoadLibrary("d3d9.dll");
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
			Hwnd = focusWindow;
			if (hook_end_scene == null) {
				// Hook WndProc
				old_wndproc = NativeAPI.GetWindowLong(focusWindow, NativeAPI.GWL_WNDPROC);
				wnd_proc_delegate = new NativeAPI.WndProcDelegate(WndProcDetour);
				NativeAPI.SetWindowLong(focusWindow, NativeAPI.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(wnd_proc_delegate));

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

			return Reset(instance, ref presentationParameters);
		}

		unsafe IntPtr WndProcDetour(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam) {
			Hwnd = hwnd;
			if (msg == NativeAPI.WM_KEYDOWN) {
				if (OnKeyPressed != null) {
					if (OnKeyPressed(wparam.ToInt32()))
						return NativeAPI.DefWindowProc(hwnd, msg, wparam, lparam);
				}
			}

			return NativeAPI.CallWindowProc(old_wndproc, hwnd, msg, wparam, lparam);
		}

		public void Dispose() {
			if (hook_d3d_create9 != null)
				hook_d3d_create9.Dispose();
			if (hook_d3d9_create_device != null)
				hook_d3d9_create_device.Dispose();
			if (hook_end_scene != null)
				hook_end_scene.Dispose();

			NativeAPI.SetWindowLong(Hwnd, NativeAPI.GWL_WNDPROC, old_wndproc);
		}

		public bool SetWindowText(string name) {
			return NativeAPI.SetWindowText(Hwnd, name);
		}

		public bool MoveWindow(int x, int y, int width, int height) {
			var result = NativeAPI.MoveWindow(Hwnd, x, y, width, height, true);
			NativeAPI.SendMessage(Hwnd, 0x232, IntPtr.Zero, IntPtr.Zero);
			return result;
		}

		public void LeftClick(int x, int y) {
			var wparam = (IntPtr)NativeAPI.MK_LBUTTON;
			var lparam = (IntPtr)(y << 16 | x);
			NativeAPI.SendMessage(Hwnd, NativeAPI.WM_LBUTTONDOWN, wparam, lparam);
			NativeAPI.SendMessage(Hwnd, NativeAPI.WM_LBUTTONUP, wparam, lparam);
		}

		public refw.NativeAPI.Rect? GetClientRect() {
			refw.NativeAPI.Rect rect;
			if (!NativeAPI.GetClientRect(Hwnd, out rect))
				return null;
			return rect;
		}
	}
}
