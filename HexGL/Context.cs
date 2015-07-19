using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    public class Context : IDisposable {
        private static CodeGenerator generator;
        private static Dictionary<System.Reflection.MethodInfo, Type> glDelegates;
        private static Type implType;
        static Context() {
            generator = new CodeGenerator("OpenGL", "OpenGL");
            glDelegates = generator.DefineDelegates(typeof(IGL));
            implType = generator.CreateImplementor("GL", typeof(IGL), glDelegates, typeof(IImplProvider));
        }
        private static Context current;
        public static Context Create(IntPtr hwnd) {
            return new Context(hwnd);
        }
        IntPtr hwnd;
        IntPtr hdc;
        IntPtr hglrc;
        //bool isWindowDC;
        bool isDoubleBuffered;
        IGL gl;
        private Context(IntPtr hwnd) {
            this.hwnd = hwnd;
            //isWindowDC = true;
            hdc = WGL.GetDC(hwnd);
            if (hdc == IntPtr.Zero) throw new InvalidOperationException("HDC failed");
            SelectPixelFormatAuto();
            //SelectPixelFormat(24, 0, 24, 8, true, false);
            //SelectPixelFormat(24, 0, 24, 8, false, false);
            hglrc = WGL.CreateContext(hdc);
            if (hglrc == IntPtr.Zero) throw new InvalidOperationException("HGLRC failed");
            gl = CreateBindingImplementor();
        }
        private void SelectPixelFormatAuto() {
            WGL.PIXELFORMATDESCRIPTOR pfd = new WGL.PIXELFORMATDESCRIPTOR();
            pfd.dwFlags = WGL.PFD_SUPPORT_OPENGL | WGL.PFD_GENERIC_ACCELERATED | WGL.PFD_STEREO_DONTCARE;
            int pixelFormat = WGL.ChoosePixelFormat(hdc, pfd);
            WGL.DescribePixelFormat(hdc, pixelFormat, Marshal.SizeOf(pfd), pfd);
            bool ok = WGL.SetPixelFormat(hdc, pixelFormat, pfd);
            if (!ok) throw new InvalidOperationException("SetPixelFormat failed");
            isDoubleBuffered = (pfd.dwFlags & WGL.PFD_DOUBLEBUFFER) != 0;
        }
        private void SelectPixelFormat(byte minColorBits, byte minAlphaBits, byte minDepthBits, byte minStencilBits, bool needSwapExchange, bool needSwapCopy) {
            WGL.PIXELFORMATDESCRIPTOR pfd = new WGL.PIXELFORMATDESCRIPTOR();
            int count = WGL.DescribePixelFormat(hdc, 0, Marshal.SizeOf(typeof(WGL.PIXELFORMATDESCRIPTOR)), null);
            if (count == 0) throw new InvalidOperationException("No PixelFormat available");
            int pixelFormat = 0;
            for (int i = 0; i < count; i++) {
                int c = WGL.DescribePixelFormat(hdc, i + 1, Marshal.SizeOf(pfd), pfd);
                if (c != count) throw new InvalidOperationException("DescribePixelFormat failed");
                if ((pfd.dwFlags & WGL.PFD_SUPPORT_OPENGL) == 0) continue;
                //if ((pfd.dwFlags & WGL.PFD_GENERIC_ACCELERATED) == 0) continue;
                if (pfd.iPixelType != WGL.PFD_TYPE_RGBA) continue;
                if (pfd.cDepthBits < minDepthBits) continue;
                if (pfd.cStencilBits < minStencilBits) continue;
                if (pfd.cColorBits < minColorBits) continue;
                if (pfd.cAlphaBits < minAlphaBits) continue;
                if ((needSwapCopy || needSwapExchange) && ((pfd.dwFlags & WGL.PFD_DOUBLEBUFFER) == 0)) continue;
                if (needSwapExchange && ((pfd.dwFlags & WGL.PFD_SWAP_EXCHANGE) == 0)) continue;
                if ((needSwapCopy || needSwapExchange) && ((pfd.dwFlags & WGL.PFD_DOUBLEBUFFER) == 0)) continue;
                pixelFormat = i + 1;
                break;
            }
            if (pixelFormat == 0) throw new InvalidOperationException("No PixelFormat available");
            WGL.DescribePixelFormat(hdc, pixelFormat, Marshal.SizeOf(pfd), pfd);
            bool ok = WGL.SetPixelFormat(hdc, pixelFormat, pfd);
            if (!ok) throw new InvalidOperationException("SetPixelFormat failed");
            isDoubleBuffered = (pfd.dwFlags & WGL.PFD_DOUBLEBUFFER) != 0;
        }
        private static void MakeCurrent(Context context) {
            if (context != null) {
                WGL.MakeCurrent(context.hdc, context.hglrc);
            } else {
                WGL.MakeCurrent(IntPtr.Zero, IntPtr.Zero);
            }
            Context.current = context;
        }
        public bool IsCurrent { get { return Context.current == this; } }
        public bool IsInitialized { get { return hdc != IntPtr.Zero && hglrc != IntPtr.Zero; } }
        public void Execute(Action<IGL> procedure) {
            if (!IsInitialized) throw new InvalidOperationException();
            MakeCurrent(this);
            procedure(gl);
            MakeCurrent(null);
        }
        public void SwapBuffers() {
            if (isDoubleBuffered)
                WGL.SwapBuffers(hdc);
        }
        public void Dispose() {
            WGL.DeleteContext(hglrc);
            WGL.ReleaseDC(hwnd, hdc);
        }
        private IGL CreateBindingImplementor() {
            var provider = new GLMethodProvider(this, glDelegates);
            var impl = (IGL)Activator.CreateInstance(implType, provider);
            return impl;
        }

        public class GLMethodProvider : IImplProvider {
            private Context owner;
            private Dictionary<System.Reflection.MethodInfo, Type> delegateTypes;
            private Dictionary<Type, object> delegates = new Dictionary<Type, object>();
            public GLMethodProvider(Context owner, Dictionary<System.Reflection.MethodInfo, Type> dict) {
                this.owner = owner;
                this.delegateTypes = dict;
            }
            public object Invoke(Type dtype, object[] args) {
                var d = GetDelegate(dtype);
                return d.DynamicInvoke(args);
            }
            T GetDelegate<T>() {
                return (T)(object)GetDelegate(typeof(T));
            }
            Delegate GetDelegate(Type type) {
                object d = null;
                if (!delegates.TryGetValue(type, out d)) {
                    var name = string.Concat("gl", type.Name);
                    var ptr = WGL.GetProcAddress(name);
                    if (ptr == IntPtr.Zero || ptr == new IntPtr(1) || ptr == new IntPtr(2) || ptr == new IntPtr(3) || ptr == new IntPtr(-1)) {
                        ptr = WGL.GetModuleProcAddress(name);
                        if (ptr == IntPtr.Zero) throw new NotSupportedException(name);
                    }
                    d = Marshal.GetDelegateForFunctionPointer(ptr, type);
                    delegates.Add(type, d);
                }
                return (Delegate)d;
            }
        }
    }

}
