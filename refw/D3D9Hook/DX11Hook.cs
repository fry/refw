using EasyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace refw.D3D {
    public class DX11Hook: DirectXHook {
        private const int DXGI_FORMAT_R8G8B8A8_UNORM = 0x1C;
        private const int DXGI_USAGE_RENDER_TARGET_OUTPUT = 0x20;
        private const int D3D11_SDK_VERSION = 7;
        private const int D3D_DRIVER_TYPE_HARDWARE = 1;

        [DllImport("d3d11.dll")]
        private static extern unsafe int D3D11CreateDeviceAndSwapChain(void* pAdapter, int driverType, void* Software,
            int flags, void* pFeatureLevels,
            int FeatureLevels, int SDKVersion,
            void* pSwapChainDesc, void* ppSwapChain,
            void* ppDevice, void* pFeatureLevel,
            void* ppImmediateContext);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rational {
            private readonly int Numerator;
            private readonly int Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ModeDescription {
            private readonly int Width;
            private readonly int Height;
            private readonly Rational RefreshRate;
            public int Format;
            private readonly int ScanlineOrdering;
            private readonly int Scaling;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SampleDescription {
            public int Count;
            private readonly int Quality;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SwapChainDescription {
            public ModeDescription ModeDescription;
            public SampleDescription SampleDescription;
            public int Usage;
            public int BufferCount;
            public IntPtr OutputHandle;
            [MarshalAs(UnmanagedType.Bool)]
            public bool IsWindowed;

            private readonly int SwapEffect;
            private readonly int Flags;
        }

        public const int DXGISwapChainPresent = 8;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate IntPtr PresentF(IntPtr pThis, uint SyncInterval, uint flags);

        LocalHook presentHook;
        PresentF presentDelegateOrig;
        PresentF presentDelegate;

        public DX11Hook() {
            // TODO: Determine main window
        }

        public void SetupDetour() {
            var form = new Form();
            var scd = new SwapChainDescription {
                BufferCount = 1,
                ModeDescription = new ModeDescription { Format = DXGI_FORMAT_R8G8B8A8_UNORM },
                Usage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription { Count = 1 },
                IsWindowed = true
            };

            unsafe
            {
                var pSwapChain = IntPtr.Zero;
                var pDevice = IntPtr.Zero;
                var pImmediateContext = IntPtr.Zero;
                var ret = D3D11CreateDeviceAndSwapChain((void*)IntPtr.Zero, D3D_DRIVER_TYPE_HARDWARE,
                    (void*)IntPtr.Zero, 0, (void*)IntPtr.Zero, 0, D3D11_SDK_VERSION, &scd, &pSwapChain, &pDevice,
                    (void*)IntPtr.Zero, &pImmediateContext);

                var func = Misc.GetVTableFunction(pSwapChain, DXGISwapChainPresent);

                presentDelegateOrig = Misc.GetDelegate<PresentF>(func);
                presentDelegate = new PresentF(MyPresent);
                presentHook = LocalHook.Create(func, presentDelegate, null);
                presentHook.ThreadACL.SetExclusiveACL(new int[] { });
            }

            form.Dispose();
        }

        IntPtr MyPresent(IntPtr swapChain, uint syncInterval, uint flags) {
            ArtificialEndScene();
            return presentDelegateOrig(swapChain, syncInterval, flags);
        }
    }
}
