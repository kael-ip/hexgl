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
