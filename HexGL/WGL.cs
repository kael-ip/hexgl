using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    class WGL {
        public const uint PFD_DOUBLEBUFFER = 0x00000001;
        public const uint PFD_STEREO = 0x00000002;
        public const uint PFD_DRAW_TO_WINDOW = 0x00000004;
        public const uint PFD_DRAW_TO_BITMAP = 0x00000008;
        public const uint PFD_SUPPORT_GDI = 0x00000010;
        public const uint PFD_SUPPORT_OPENGL = 0x00000020;
        public const uint PFD_GENERIC_FORMAT = 0x00000040;
        public const uint PFD_NEED_PALETTE = 0x00000080;
        public const uint PFD_NEED_SYSTEM_PALETTE = 0x00000100;
        public const uint PFD_SWAP_EXCHANGE = 0x00000200;
        public const uint PFD_SWAP_COPY = 0x00000400;
        public const uint PFD_SWAP_LAYER_BUFFERS = 0x00000800;
        public const uint PFD_GENERIC_ACCELERATED = 0x00001000;
        public const uint PFD_SUPPORT_DIRECTDRAW = 0x00002000;
        public const uint PFD_DEPTH_DONTCARE = 0x20000000;
        public const uint PFD_DOUBLEBUFFER_DONTCARE = 0x40000000;
        public const uint PFD_STEREO_DONTCARE = 0x80000000;

        public const byte PFD_TYPE_RGBA = 0;
        public const byte PFD_TYPE_COLORINDEX = 1;

        public const byte PFD_MAIN_PLANE = 0;
        public const byte PFD_OVERLAY_PLANE = 1;
        public const byte PFD_UNDERLAY_PLANE = 255;

        [StructLayout(LayoutKind.Sequential)]
        public class PIXELFORMATDESCRIPTOR {
            public short nSize;
            public short nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public int dwLayerMask;
            public int dwVisibleMask;
            public int dwDamageMask;
            public PIXELFORMATDESCRIPTOR() {
                nSize = (short)Marshal.SizeOf(this);
                nVersion = 1;
                iPixelType = WGL.PFD_TYPE_RGBA;
                cColorBits = 24;
                cDepthBits = 32;
                iLayerType = WGL.PFD_MAIN_PLANE;
            }
            public override string ToString() {
                return string.Format("Flags=[{0}], PixelType={1}, ColorBits={2}, AplhaBits={3}, AccumBits={4}, AccumAlphaBits={5}, DepthBits={6}, StencilBits={7}, AuxBuffers={8}, LayerType={9}"
                    , FlagsToString(), iPixelType == WGL.PFD_TYPE_RGBA ? "RGBA" : (iPixelType == WGL.PFD_TYPE_COLORINDEX ? "INDEX" : "?")
                    , cColorBits, cAlphaBits, cAccumBits, cAccumAlphaBits, cDepthBits, cStencilBits, cAuxBuffers
                    , iLayerType == WGL.PFD_MAIN_PLANE ? "MAIN" : (iLayerType == WGL.PFD_OVERLAY_PLANE ? "OVERLAY" : (iLayerType == WGL.PFD_UNDERLAY_PLANE ? "UNDERLAY" : "?")));
            }
            private string FlagsToString() {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                if ((dwFlags & WGL.PFD_DOUBLEBUFFER) != 0) list.Add("DOUBLEBUFFER");
                if ((dwFlags & WGL.PFD_STEREO) != 0) list.Add("STEREO");
                if ((dwFlags & WGL.PFD_DRAW_TO_WINDOW) != 0) list.Add("DRAW_TO_WINDOW");
                if ((dwFlags & WGL.PFD_DRAW_TO_BITMAP) != 0) list.Add("DRAW_TO_BITMAP");
                if ((dwFlags & WGL.PFD_SUPPORT_GDI) != 0) list.Add("SUPPORT_GDI");
                if ((dwFlags & WGL.PFD_SUPPORT_OPENGL) != 0) list.Add("SUPPORT_OPENGL");
                if ((dwFlags & WGL.PFD_GENERIC_FORMAT) != 0) list.Add("GENERIC_FORMAT");
                if ((dwFlags & WGL.PFD_NEED_PALETTE) != 0) list.Add("NEED_PALETTE");
                if ((dwFlags & WGL.PFD_NEED_SYSTEM_PALETTE) != 0) list.Add("NEED_SYSTEM_PALETTE");
                if ((dwFlags & WGL.PFD_SWAP_EXCHANGE) != 0) list.Add("SWAP_EXCHANGE");
                if ((dwFlags & WGL.PFD_SWAP_COPY) != 0) list.Add("SWAP_COPY");
                if ((dwFlags & WGL.PFD_SWAP_LAYER_BUFFERS) != 0) list.Add("SWAP_LAYER_BUFFERS");
                if ((dwFlags & WGL.PFD_GENERIC_ACCELERATED) != 0) list.Add("GENERIC_ACCELERATED");
                if ((dwFlags & WGL.PFD_SUPPORT_DIRECTDRAW) != 0) list.Add("SUPPORT_DIRECTDRAW");
                if ((dwFlags & WGL.PFD_DEPTH_DONTCARE) != 0) list.Add("DEPTH_DONTCARE");
                if ((dwFlags & WGL.PFD_DOUBLEBUFFER_DONTCARE) != 0) list.Add("DOUBLEBUFFER_DONTCARE");
                if ((dwFlags & WGL.PFD_STEREO_DONTCARE) != 0) list.Add("STEREO_DONTCARE");
                return string.Join("|", list.ToArray());
            }
        }

        public const string LibraryName = "opengl32.dll";
        private static IntPtr hModuleOpenGL;

        static WGL() {
            try {
                Finish();//otherwise GL context creation may fail
            } catch { }
            hModuleOpenGL = WGL.GetModuleHandle(WGL.LibraryName);
        }

        [DllImport("gdi32.dll")]
        public static extern int DescribePixelFormat(IntPtr hdc, int iPixelFormat, int nBytes, [In, Out] PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")]
        public static extern int ChoosePixelFormat(IntPtr hdc, [In] PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")]
        public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")]
        public static extern bool SwapBuffers(IntPtr hdc);

        [DllImport(LibraryName, EntryPoint = "wglCreateContext")]
        public static extern IntPtr CreateContext(IntPtr hdc);
        [DllImport(LibraryName, EntryPoint = "wglDeleteContext")]
        public static extern bool DeleteContext(IntPtr hglrc);
        [DllImport(LibraryName, EntryPoint = "wglMakeCurrent")]
        public static extern bool MakeCurrent(IntPtr hdc, IntPtr hglrc);
        [DllImport(LibraryName, EntryPoint = "wglGetProcAddress")]
        public static extern IntPtr GetProcAddress(String name);
        [DllImport(WGL.LibraryName, EntryPoint = "glFinish")]
        private static extern void Finish();
        [DllImport(WGL.LibraryName, EntryPoint = "glGetError")]
        internal static extern uint GetError();

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        public static IntPtr GetModuleProcAddress(string procName) {
            return GetProcAddress(hModuleOpenGL, procName);
        }
    }
}
