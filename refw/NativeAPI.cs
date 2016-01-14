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
        public const int WM_WINDOWPOSCHANGING = 70;
		public const int WM_KEYUP = 0x0101;
		public const int WM_GETMINMAXINFO = 0x0024;
		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
	    public const int WM_MOUSEMOVE = 0x0200;

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

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPos {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        } 

		public const int HWND_TOP = 0;
		[DllImport("d3d9.dll")]
		public static extern IntPtr Direct3DCreate9(uint sdkVersion);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

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

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

        [DllImport("User32.Dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.Dll")]
        public static extern bool SetCapture(IntPtr hWnd);

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
		public delegate int Direct3D9ResetDelegate(IntPtr instance, ref PresentParameters presentationParameters);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		public delegate int Direct3D9EndScene(IntPtr instance);

        public delegate int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

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

        const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        const int ERROR_BUFFER_OVERFLOW = 111;
        const int MAX_ADAPTER_NAME_LENGTH = 256;
        const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        const int MIB_IF_TYPE_OTHER = 1;
        const int MIB_IF_TYPE_ETHERNET = 6;
        const int MIB_IF_TYPE_TOKENRING = 9;
        const int MIB_IF_TYPE_FDDI = 15;
        const int MIB_IF_TYPE_PPP = 23;
        const int MIB_IF_TYPE_LOOPBACK = 24;
        const int MIB_IF_TYPE_SLIP = 28;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDRESS_STRING {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDR_STRING {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public Int32 Context;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO {
            public IntPtr Next;
            public Int32 ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string AdapterDescription;
            public UInt32 AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;
            public Int32 Index;
            public UInt32 Type;
            public UInt32 DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public Int32 LeaseObtained;
            public Int32 LeaseExpires;
        }
	}
}
