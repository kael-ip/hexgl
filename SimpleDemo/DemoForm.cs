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
        }
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            demo.SetViewportSize(this.ClientSize);
            glContext = Context.Create(this.Handle, true);
            glContext.Execute(gl => {
                Trace.TraceInformation(string.Join("\n", GetInfo(gl)));
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
            if(glContext == null)
                return;
            var sw = Stopwatch.StartNew();
            glContext.Execute(gl => {
                demo.Redraw(gl);
            });
            sw.Stop();
            glContext.SwapBuffers();
            var text1 = string.Format("GL redraw: {0} ms", sw.ElapsedMilliseconds);
            this.Text = text1;
            this.Invalidate();
            System.Threading.Thread.Sleep(0);
            if(Control.MouseButtons == MouseButtons.Right) {
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
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            demo.OnMouseMove(e.Location, (e.Button & System.Windows.Forms.MouseButtons.Left) != 0, (e.Button & System.Windows.Forms.MouseButtons.Right) != 0);
        }
        public static string GetString(IGL gl, uint name) {
            IntPtr ptr = gl.GetString(name);
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }
        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            demo.SetViewportSize(this.ClientSize);
        }
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            demo.Dispose();
        }
    }
}
