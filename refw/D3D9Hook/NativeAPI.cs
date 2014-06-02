using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace refw {
	public static class NativeAPI {
		public const uint SDKVersion = 32;

		public const int EndSceneOffset = 42;
		public const int ResetOffset = 16;

		public const int GWL_WNDPROC = -4;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;
		public const int WM_GETMINMAXINFO = 0x0024;
		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;

		public const int MK_LBUTTON = 0x0001;

		[StructLayout(LayoutKind.Sequential)]
		public struct Point {
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Rect {
			public int Left, Top, Right, Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MinMaxInfo {
			public Point ptReserved;
			public Point ptMaxSize;
			public Point ptMaxPosition;
			public Point ptMinTrackSize;
			public Point ptMaxTrackSize;
		}

		public const int HWND_TOP = 0;
		[DllImport("d3d9.dll")]
		public static extern IntPtr Direct3DCreate9(uint sdkVersion);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string str);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

		[DllImport("user32.dll")]
		public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool GetClientRect(IntPtr hwnd, out Rect rect);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate IntPtr Direct3DCreate9Delegate(uint sdkVersion);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void D3DRelease(IntPtr instance);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int Direct3D9CreateDeviceDelegate(IntPtr instance, uint adapter, uint deviceType,
													 IntPtr focusWindow,
													 uint behaviorFlags,
													 [In] ref PresentParameters presentationParameters,
													 [Out] out IntPtr returnedDeviceInterface);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int Direct3D9ResetDelegate(IntPtr instance, [In] ref PresentParameters presentationParameters);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		public delegate int Direct3D9EndScene(IntPtr instance);

		[StructLayout(LayoutKind.Sequential)]
		public struct PresentParameters {
			public readonly uint BackBufferWidth;
			public readonly uint BackBufferHeight;
			public uint BackBufferFormat;
			public readonly uint BackBufferCount;
			public readonly uint MultiSampleType;
			public readonly uint MultiSampleQuality;
			public uint SwapEffect;
			public readonly IntPtr hDeviceWindow;
			[MarshalAs(UnmanagedType.Bool)]
			public bool Windowed;
			[MarshalAs(UnmanagedType.Bool)]
			public readonly bool EnableAutoDepthStencil;
			public readonly uint AutoDepthStencilFormat;
			public readonly uint Flags;
			public readonly uint FullScreen_RefreshRateInHz;
			public readonly uint PresentationInterval;
		}
	}
}
