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
		/// Invoked only on the first ever OnFrame event for this instance
		/// </summary>
		public event Action OnFirstFrame;
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

		private bool _firstOnFrame = true;

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
	    public NativeAPI.Point? EnforceWindowSize = null;

        public uint FrameCounter { get; private set; }

		public void ArtificialEndScene() {
			long delta_ms = 0;
			if (FrameLimit.HasValue) {
				if (!_stopWatch.IsRunning) {
					_stopWatch.Reset();
					_stopWatch.Start();
				} else {
					delta_ms = _stopWatch.ElapsedMilliseconds;
					double diff = 1000.0 / FrameLimit.Value - delta_ms;
					//_timeDiff += diff;
                    //_timeDiff = Math.Max(0, _timeDiff);
					if (diff > 0) {
                        Thread.Sleep((int)diff);
						_timeDiff = 0;
					}
					_stopWatch.Reset();
					_stopWatch.Start();
				}
			}

			if (_firstOnFrame) {
				_firstOnFrame = false;
				if (OnFirstFrame != null)
					OnFirstFrame();
			}

			if (OnFrame != null)
				OnFrame(TimeSpan.FromMilliseconds(delta_ms));

            // Increase frame counter with wrap-around
            FrameCounter = (FrameCounter + 1) % uint.MaxValue;
		}

		private int EndSceneCallback(IntPtr instance) {
			ArtificialEndScene();

			return (int)EndScene(instance);
		}

        public EndSceneDetour() {
            FrameCounter = 0;
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

			var ret = Reset(instance, ref presentationParameters);
		    return ret;
		}

		unsafe IntPtr WndProcDetour(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam) {
			Hwnd = hwnd;
			if (msg == NativeAPI.WM_KEYDOWN) {
				if (OnKeyPressed != null) {
					if (OnKeyPressed(wparam.ToInt32()))
						return NativeAPI.DefWindowProc(hwnd, msg, wparam, lparam);
				}
			} else if (msg == NativeAPI.WM_GETMINMAXINFO) {
			    var minmax = (NativeAPI.MinMaxInfo*) lparam;
                
			    minmax->ptMinTrackSize.X = 100;
                minmax->ptMinTrackSize.Y = 100;

			    return IntPtr.Zero;
            } else if (msg == NativeAPI.WM_WINDOWPOSCHANGING) {
                var wndpos = (NativeAPI.WindowPos*) lparam;
                var result = NativeAPI.CallWindowProc(old_wndproc, hwnd, msg, wparam, lparam);
                if (EnforceWindowSize.HasValue) {
                    wndpos->cx = Math.Min(wndpos->cx, EnforceWindowSize.Value.X);
                    wndpos->cy = Math.Min(wndpos->cx, EnforceWindowSize.Value.Y);
                }
                return result;
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
            NativeAPI.SetWindowPos(Hwnd, IntPtr.Zero, x, y, width, height, 0x0002);
			return result;
		}

		public void LeftClick(int x, int y) {
			var wparam = (IntPtr)NativeAPI.MK_LBUTTON;
			var lparam = (IntPtr)(y << 16 | x);
			NativeAPI.SendMessage(Hwnd, NativeAPI.WM_LBUTTONDOWN, wparam, lparam);
			NativeAPI.SendMessage(Hwnd, NativeAPI.WM_LBUTTONUP, wparam, lparam);
		}

        public void MoveMouse(int x, int y) {
            var pos = new NativeAPI.Point();
            pos.X = x;
            pos.Y = y;
            NativeAPI.ClientToScreen(Hwnd, ref pos);
            NativeAPI.SetCursorPos(pos.X, pos.Y);
            var lparam = (IntPtr)(y << 16 | x);
            NativeAPI.SendMessage(Hwnd, NativeAPI.WM_MOUSEMOVE, IntPtr.Zero, lparam);

        }

        public refw.NativeAPI.Rect? GetClientRect() {
            refw.NativeAPI.Rect rect;
            if (!NativeAPI.GetClientRect(Hwnd, out rect))
                return null;
            return rect;
        }

        public refw.NativeAPI.Rect? GetDesktopRect() {
            var hwnd = NativeAPI.GetDesktopWindow();
            refw.NativeAPI.Rect rect;
            if (!NativeAPI.GetClientRect(hwnd, out rect))
                return null;
            return rect;
        }

	    public void BringToFront() {
            NativeAPI.SetCapture(Hwnd);
            NativeAPI.SetForegroundWindow(Hwnd);
	    }
	}
}
