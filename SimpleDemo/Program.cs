using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HexTex.OpenGL.SimpleDemo {
    static class Program {
        static bool fullScreen = false;
        static Type[] sequence = new Type[]{
            typeof(Demo1),
        };
        static int current = 0, next = 0;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Idle += Application_Idle;
            Application.Run();
        }
        static void Application_Idle(object sender, EventArgs e) {
            Application.Idle -= Application_Idle;
            Run();
        }
        static void Run() {
            if(next < 0) {
                Application.Exit();
                return;
            }
            else {
                current = next;
            }
            var demo = (DemoBase)Activator.CreateInstance(sequence[current]);
            next = -1;
            var form = new DemoForm(demo, fullScreen);
            form.FormClosed += form_FormClosed;
            form.KeyUp += form_KeyUp;
            form.ShowDialog();
        }
        static void form_FormClosed(object sender, FormClosedEventArgs e) {
            Application.Idle += Application_Idle;
        }
        static void form_KeyUp(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Space) {
                next = (current + 1) % sequence.Length;
                ((Form)sender).Close();
            }
            else if(e.KeyCode == Keys.Z) {
                next = current;
                ((Form)sender).Close();
            }
            else if(e.KeyCode == Keys.Escape) {
                next = -1;
                ((Form)sender).Close();
            }
            else if(e.KeyCode == Keys.F) {
                next = current;
                fullScreen = !fullScreen;
                ((Form)sender).Close();
            }
        }
    }
}

