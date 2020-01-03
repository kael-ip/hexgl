using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HexTex.OpenGL;

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
            m[1] = (float)(x * y * (1 - c) + z * s);
            m[2] = (float)(x * z * (1 - c) - y * s);
            m[3] = (float)(x * y * (1 - c) - z * s);
            m[4] = (float)(c + y * y * (1 - c));
            m[5] = (float)(y * z * (1 - c) + x * s);
            m[6] = (float)(x * z * (1 - c) + y * s);
            m[7] = (float)(y * z * (1 - c) - x * s);
            m[8] = (float)(c + z * z * (1 - c));
            if(m.Length >= 12) {
                m[9] = 0;
                m[10] = 0;
                m[11] = 0;
            }
        }
        public static void Translate3(float[] m, float x, float y, float z) {
            m[0] = 1;
            m[1] = 0;
            m[2] = 0;
            m[3] = 0;
            m[4] = 1;
            m[5] = 0;
            m[6] = 0;
            m[7] = 0;
            m[8] = 1;
            m[9] = x;
            m[10] = y;
            m[11] = z;
        }
        public static void MatrixMultiply(float[] m, float[] b, float[] a) {
            float t11 = a[0] * b[0] + a[3] * b[1] + a[6] * b[2];
            float t12 = a[0] * b[3] + a[3] * b[4] + a[6] * b[5];
            float t13 = a[0] * b[6] + a[3] * b[7] + a[6] * b[8];
            float t21 = a[1] * b[0] + a[4] * b[1] + a[7] * b[2];
            float t22 = a[1] * b[3] + a[4] * b[4] + a[7] * b[5];
            float t23 = a[1] * b[6] + a[4] * b[7] + a[7] * b[8];
            float t31 = a[2] * b[0] + a[5] * b[1] + a[8] * b[2];
            float t32 = a[2] * b[3] + a[5] * b[4] + a[8] * b[5];
            float t33 = a[2] * b[6] + a[5] * b[7] + a[8] * b[8];
            if(a.Length >= 12 && b.Length >= 12 && m.Length >= 12) {
                float t41 = b[9] * a[0] + b[10] * a[3] + b[11] * a[6] + a[9];
                float t42 = b[9] * a[1] + b[10] * a[4] + b[11] * a[7] + a[10];
                float t43 = b[9] * a[2] + b[10] * a[5] + b[11] * a[8] + a[11];
                m[9] = t41;
                m[10] = t42;
                m[11] = t43;
            }
            m[0] = t11;
            m[1] = t21;
            m[2] = t31;
            m[3] = t12;
            m[4] = t22;
            m[5] = t32;
            m[6] = t13;
            m[7] = t23;
            m[8] = t33;
        }
    }
}

namespace HexTex.Recuberation {

    public static class NumericsHelper {
        public static float[] ToArray(this System.Numerics.Matrix4x4 m) {
            var a = new float[16];
            a[0] = m.M11;
            a[1] = m.M12;
            a[2] = m.M13;
            a[3] = m.M14;
            a[4] = m.M21;
            a[5] = m.M22;
            a[6] = m.M23;
            a[7] = m.M24;
            a[8] = m.M31;
            a[9] = m.M32;
            a[10] = m.M33;
            a[11] = m.M34;
            a[12] = m.M41;
            a[13] = m.M42;
            a[14] = m.M43;
            a[15] = m.M44;
            return a;
        }
        public static float[] GetRotationMatrixAsArray(this System.Numerics.Matrix4x4 m) {
            var a = new float[9];
            a[0] = m.M11;
            a[1] = m.M12;
            a[2] = m.M13;
            a[3] = m.M21;
            a[4] = m.M22;
            a[5] = m.M23;
            a[6] = m.M31;
            a[7] = m.M32;
            a[8] = m.M33;
            return a;
        }
        public static float[] ToMatrix3x4Array(this System.Numerics.Matrix4x4 m) {
            var a = new float[12];
            a[0] = m.M11;
            a[1] = m.M12;
            a[2] = m.M13;
            a[3] = m.M21;
            a[4] = m.M22;
            a[5] = m.M23;
            a[6] = m.M31;
            a[7] = m.M32;
            a[8] = m.M33;
            a[9] = m.M41;
            a[10] = m.M42;
            a[11] = m.M43;
            return a;
        }
    }

    class DemoHelper {
        public static float[][] GeneratePalette(int count, float luma = 0.7f, float z = 0.4f) {
            var rnd = new PRNG();
            var palette = new float[count][];
            for(var i = 0; i < count; i++) {
                var rgb = new float[3];
                var a = rnd.Next(1 << 15) * 3.0 / (1 << 15);
                var ii = Math.Floor(a);
                var j = (int)ii;
                var c0 = (1 - z) * luma;
                var c1 = a - ii;
                var c2 = 1 - c1;
                c1 = (c1 * z + 1 - z) * luma;
                c2 = (c2 * z + 1 - z) * luma;
                rgb[(j + 0) % 3] = (float)(c0);
                rgb[(j + 1) % 3] = (float)(c1);
                rgb[(j + 2) % 3] = (float)(c2);
                palette[i] = rgb;
            }
            return palette;
        }
    }

    abstract class DemoBase {
        public abstract void Prepare(IGL gl);
        public abstract void Redraw(IGL gl);
        public virtual void SetViewportSize(Size size) { }
        public virtual void OnMouseMove(Point point, bool leftButtonPressed, bool rightButtonPressed) { }
        public virtual void OnKeyDown(System.Windows.Forms.KeyEventArgs e) { }
        public virtual void OnKeyUp(System.Windows.Forms.KeyEventArgs e) { }
    }

    class Mesh {
        public float[] VertexBuffer { get; private set; }
        public float[] NormalBuffer { get; private set; }
        public float[] TexUVBuffer { get; private set; }
        public int PrimitiveCount { get; private set; }
        public int PrimitiveLength { get; private set; }
        public Mesh(int length, int count, bool useNormal, bool useTexUV) {
            PrimitiveLength = length;
            PrimitiveCount = count;
            VertexBuffer = new float[3 * length * count];
            if(useNormal) {
                NormalBuffer = new float[3 * length * count];
            }
            if(useTexUV) {
                TexUVBuffer = new float[2 * length * count];
            }
        }
    }

    class QMesh : Mesh {
        private IList<Quad> quads;
        public IList<Quad> Quads { get { return quads; } }
        public QMesh(IList<Quad> quads)
            : base(4, quads.Count, true, false) {
            this.quads = quads;
            int offset = 0;
            foreach(var quad in quads) {
                offset = quad.FillQuadVerts(VertexBuffer, NormalBuffer, offset);
            }
        }
    }

    class PRNG {
        const int a = 1103515245, c = 12345, m = (int)((1L << 31) - 1); //glibc
        static PRNG shared = new PRNG(1);
        int seed;
        public PRNG(int seed) {
            this.seed = seed;
        }
        public PRNG() : this(shared.Next()) { }
        public int Next() {
            return seed = (seed * a + c) & m;
        }
        public int Next(int max) {
            return (int)(((((long)Next()) * max) >> 31) & m);
        }
    }    
}
