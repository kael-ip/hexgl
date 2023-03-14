using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    static class GLMath {

        public static float[] Frustum(double l, double r, double b, double t, double n, double f) {
            if (l == r || b == t || n == f || n <= 0 || f <= 0) throw new ArgumentException();
            return new float[]{
                (float)(2*n/(r-l)),0,0,0,
                0,(float)(2*n/(t-b)),0,0,
                (float)((r+l)/(r-l)), (float)((t+b)/(t-b)), (float)(-(f+n)/(f-n)), -1,
                0,0,(float)(-2*f*n/(f-n)),0
            };
        }
        public static float[] Ortho(double l, double r, double b, double t, double n, double f) {
            if (l == r || b == t || n == f) throw new ArgumentException();
            return new float[]{
                (float)(2/(r-l)),0,0,0,
                0,(float)(2/(t-b)),0,0,
                0,0,(float)(-2/(f-n)),0,
                (float)(-(r+l)/(r-l)), (float)(-(t+b)/(t-b)), (float)(-(f+n)/(f-n)),1
            };
        }
        public static void Rotate3(float[] m, double angle, double x, double y, double z) {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[0] = (float)(c + x * x * (1 - c));
            m[1] = (float)(x * y * (1 - c) - z * s);
            m[2] = (float)(x * z * (1 - c) + y * s);
            m[3] = (float)(x * y * (1 - c) + z * s);
            m[4] = (float)(c + y * y * (1 - c));
            m[5] = (float)(y * z * (1 - c) - x * s);
            m[6] = (float)(x * z * (1 - c) - y * s);
            m[7] = (float)(y * z * (1 - c) + x * s);
            m[8] = (float)(c + z * z * (1 - c));
        }
    }

    static class TexureHelper {
        public static Bitmap Generate(int widthPower, int heightPower) {
            Bitmap image = new Bitmap(1 << widthPower, 1 << heightPower);
            Random rnd = new Random();
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    image.SetPixel(x, y, Color.FromArgb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256)));
                }
            }
            return image;
        }
        public static int[] ToRGBA(Bitmap image) {
            int[] pixels = new int[image.Width * image.Height];
            int i = 0;
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    pixels[i++] = image.GetPixel(x, y).ToArgb();
                }
            }
            return pixels;
        }
    }

    abstract class DemoBase {
        public abstract void Prepare(IGL gl);
        public abstract void Redraw(IGL gl);
        public virtual void SetViewportSize(Size size) { }
        public virtual void OnMouseMove(Point point, bool leftButtonPressed, bool rightButtonPressed) { }
    }

    class EmptyDemo : DemoBase {
        public override void Prepare(IGL gl) {
        }
        public override void Redraw(IGL gl) {
            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(GL.COLOR_BUFFER_BIT);
            gl.Finish();
        }
    }

}
