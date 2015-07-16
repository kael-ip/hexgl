using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HexTex.OpenGL.SimpleDemo {

    class DemoForm : Form {
        DemoBase demo;
        GLGraphics savedGL;
        IntPtr savedHdc;
        Point viewOffset = Point.Empty;
        public DemoForm(DemoBase demo){
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.DoubleBuffered = true;
                        this.MouseMove += new MouseEventHandler(DemoForm_MouseMove);

            this.demo = demo;
        }
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            savedHdc = GLGraphics.GetDC(this.Handle);
            savedGL = new GLGraphics(savedHdc, this.Size);
            Console.WriteLine(string.Join("\n", savedGL.GetInfo()));
            savedGL.Execute(x => {
                demo.Prepare(x);
            });
        }
        protected override void OnHandleDestroyed(EventArgs e) {
            base.OnHandleDestroyed(e);
            savedGL.Dispose();
            GLGraphics.ReleaseDC(this.Handle, savedHdc);
        }
        protected override void OnPaintBackground(PaintEventArgs e) {
            //base.OnPaintBackground(e);
        }
        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            if (savedGL == null) return;
            savedGL.Execute(x => {
                demo.Redraw(x);
            });
            this.Invalidate();
            if (Control.MouseButtons == MouseButtons.Right) {
                this.BeginInvoke(new MethodInvoker(delegate { this.Close(); }));
            }
        }
        void DemoForm_MouseMove(object sender, MouseEventArgs e) {
            var p = this.Bounds.Location;
            p.Offset(-this.Bounds.Width / 2, -this.Bounds.Height / 2);
            p.Offset(e.Location);
            viewOffset = p;
            this.Invalidate();
        }
    }

    class GLGraphics {
        private IntPtr savedHdc;
        private Size size;

        public GLGraphics(IntPtr savedHdc, Size size) {
            this.savedHdc = savedHdc;
            this.size = size;
        }
        internal static IntPtr GetDC(IntPtr intPtr) {
            throw new NotImplementedException();
        }

        internal string[] GetInfo() {
            throw new NotImplementedException();
        }

        internal void Dispose() {
            throw new NotImplementedException();
        }

        internal static void ReleaseDC(IntPtr intPtr, IntPtr savedHdc) {
            throw new NotImplementedException();
        }

        internal void Execute(Action<IGL> action) {
            throw new NotImplementedException();
        }
    }

    abstract class DemoBase {
        public abstract void Prepare(IGL gl);
        public abstract void Redraw(IGL gl);
    }
}
