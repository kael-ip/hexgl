using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HexTex.OpenGL.SimpleDemo {

    class DemoForm : Form {
        DemoBase demo;
        Context glContext;
        Point viewOffset = Point.Empty;
        public DemoForm(DemoBase demo, bool fullScreen = false) {
            this.demo = demo;
            if(fullScreen) {
                this.FormBorderStyle = FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            this.TopMost = true;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.DoubleBuffered = true;
            this.MouseMove += new MouseEventHandler(DemoForm_MouseMove);
        }
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            demo.SetViewportSize(this.ClientSize);
            glContext = Context.Create(this.Handle, true);
            glContext.Execute(gl => {
                Console.WriteLine(string.Join("\n", GetInfo(gl)));
                demo.Prepare(gl);
            });
        }
        protected override void OnHandleDestroyed(EventArgs e) {
            base.OnHandleDestroyed(e);
            glContext.Dispose();
            glContext = null;
        }
        protected override void OnPaintBackground(PaintEventArgs e) {
            //base.OnPaintBackground(e);
        }
        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            if (glContext == null) return;
            var sw = Stopwatch.StartNew();
            glContext.Execute(gl => {
                demo.Redraw(gl);
            });
            sw.Stop();
            Console.WriteLine("GL redraw: {0} ms", sw.ElapsedMilliseconds);
            glContext.SwapBuffers();
            this.Invalidate();
            System.Threading.Thread.Sleep(0);
            if (Control.MouseButtons == MouseButtons.Right) {
                this.BeginInvoke(new MethodInvoker(delegate { this.Close(); }));
            }
        }
        private string[] GetInfo(IGL gl) {
            return new string[]{
                GetString(gl, GL.VENDOR),
                GetString(gl, GL.RENDERER),
                GetString(gl, GL.VERSION),
                GetString(gl, GL.EXTENSIONS)
            };
        }
        void DemoForm_MouseMove(object sender, MouseEventArgs e) {
            var p = this.Bounds.Location;
            p.Offset(-this.Bounds.Width / 2, -this.Bounds.Height / 2);
            p.Offset(e.Location);
            viewOffset = p;
            this.Invalidate();
        }
        public static string GetString(IGL gl, uint name) {
            IntPtr ptr = gl.GetString(name);
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }
        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            demo.SetViewportSize(this.ClientSize);
        }
    }
}
