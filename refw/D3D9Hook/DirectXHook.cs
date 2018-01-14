using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace refw.D3D {
    public abstract class DirectXHook: IDisposable {
        /// <summary>
		/// Event that is invoked every frame, the time since the last frame is passed as an argument.
		/// </summary>
		public event Action<TimeSpan> OnFrame;
		/// <summary>
		/// Invoked only on the first ever OnFrame event for this instance
		/// </summary>
		public event Action OnFirstFrame;
		/// <summary>
		/// Invoked when a WndProc key event occurs, with the key code as an argument. The return value of
		/// the function determines whether the event is passed to the application.
		/// </summary>
		public event Func<int, bool> OnKeyPressed;

		private bool _firstOnFrame = true;

		// Measurements for frame-limiting
		private Stopwatch _stopWatch = new Stopwatch();
		private double _timeDiff = 0;
		private NativeAPI.WndProcDelegate wnd_proc_delegate;
		private IntPtr old_wndproc;

		public IntPtr Hwnd { get; protected set; }

		/// <summary>
		/// If set, limits the frames per second to the set value.
		/// </summary>
		public int? FrameLimit = null;
	    public NativeAPI.Point? EnforceWindowSize = null;

        public uint FrameCounter { get; private set; } = 0;

		public void ArtificialEndScene() {
			long delta_ms = 0;
            if (!_stopWatch.IsRunning) {
                _stopWatch.Reset();
                _stopWatch.Start();
            } else {
                delta_ms = _stopWatch.ElapsedMilliseconds;
                if (FrameLimit.HasValue) {
					double diff = 1000.0 / FrameLimit.Value - delta_ms;
					//_timeDiff += diff;
                    //_timeDiff = Math.Max(0, _timeDiff);
					if (diff > 0) {
                        Thread.Sleep((int)diff);
						_timeDiff = 0;
					}
				}
                _stopWatch.Reset();
                _stopWatch.Start();
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

        protected void SetWindow(IntPtr hwnd) {
            Hwnd = hwnd;
            // Hook WndProc
            old_wndproc = NativeAPI.GetWindowLong(Hwnd, NativeAPI.GWL_WNDPROC);
            wnd_proc_delegate = new NativeAPI.WndProcDelegate(WndProcDetour);
            NativeAPI.SetWindowLong(Hwnd, NativeAPI.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(wnd_proc_delegate));
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
