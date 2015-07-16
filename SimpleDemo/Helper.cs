using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL.SimpleDemo {

    public abstract class VertexArrayBase {
        private int length;
        private int width;
        public int Length { get { return length; } }
        public int Width { get { return width; } }
        public virtual bool Normalized { get { return false; } }
        public VertexArrayBase(int length, int width) {
            if (width < 1 || width > 4) throw new ArgumentOutOfRangeException("width");
            if (length < 0) throw new ArgumentOutOfRangeException("length");
            this.length = length;
            this.width = width;
        }
        public abstract Type ElementType { get; }
        internal abstract GCHandle PinData();
    }
    public class VertexArray<T> : VertexArrayBase where T : struct {
        private T[] data;
        private bool normalized;
        public override bool Normalized { get { return normalized; } }
        public VertexArray(int length, int width, bool normalized)
            : base(length, width) {
            this.data = new T[length * width];
            this.normalized = normalized;
        }
        private void Check(int index, bool s) {
            if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException("index");
            if (!s) throw new InvalidOperationException();
        }
        public void SetVertex(int index, T x, T y, T z, T w) {
            Check(index, Width == 4);
            int i = index * Width;
            data[i++] = x;
            data[i++] = y;
            data[i++] = z;
            data[i] = w;
        }
        public void SetVertex(int index, T x, T y, T z) {
            Check(index, Width == 3);
            int i = index * Width;
            data[i++] = x;
            data[i++] = y;
            data[i] = z;
        }
        public void SetVertex(int index, T x, T y) {
            Check(index, Width == 2);
            int i = index * Width;
            data[i++] = x;
            data[i] = y;
        }
        public void SetVertex(int index, T x) {
            Check(index, Width == 1);
            int i = index * Width;
            data[i] = x;
        }
        internal override GCHandle PinData() {
            return GCHandle.Alloc(data, GCHandleType.Pinned);
        }
        public override Type ElementType {
            get { return typeof(T); }
        }
    }

    public class SimpleCube2 {
        static int[] loop = new int[] { 0, 1, 2, 2, 3, 0 };
        const int vcount = 36;
        VertexArray<float> aVertex;
        VertexArray<float> aTexCoord;
        VertexArray<byte> aColor;
        VertexArray<float> aNormal;
        public int Count { get { return vcount; } }
        public VertexArrayBase VertexArray { get { return aVertex; } }
        public VertexArrayBase TexCoordArray { get { return aTexCoord; } }
        public VertexArrayBase ColorArray { get { return aColor; } }
        public VertexArrayBase NormalArray { get { return aNormal; } }
        public SimpleCube2(double size) : this(size, true, true) { }
        public SimpleCube2(double size, bool vcolors, bool tex) : this(size, vcolors, tex, false) { }
        public SimpleCube2(double size, bool vcolors, bool tex, bool normals) {
            float hsize = (float)size / 2;
            aVertex = new VertexArray<float>(vcount, 3, false);
            aTexCoord = new VertexArray<float>(vcount, 2, false);//if tex?
            aColor = new VertexArray<byte>(vcount, 4, false);
            aNormal = normals ? new VertexArray<float>(vcount, 3, false) : null;

            for (int f = 0; f < 6; f++) {
                int si = f * 6;
                for (int fi = 0; fi < 6; fi++) {
                    int vi = faces[f * 4 + loop[fi]];
                    {
                        var x = getV8(vi, 0) * hsize;
                        var y = getV8(vi, 1) * hsize;
                        var z = getV8(vi, 2) * hsize;
                        aVertex.SetVertex(si, x, y, z);
                    }
                    {
                        if (vcolors) {
                            var r = getC8(vi, 0);
                            var g = getC8(vi, 1);
                            var b = getC8(vi, 2);
                            var aa = (byte)255;
                            aColor.SetVertex(si, r, g, b, aa);
                        } else {
                            var r = getC6(f, 0);
                            var g = getC6(f, 1);
                            var b = getC6(f, 2);
                            var aa = (byte)255;
                            aColor.SetVertex(si, r, g, b, aa);
                        }
                    }
                    if (tex) {
                        float s = getT8(vi, 0);
                        float t = getT8(vi, 1);
                        aTexCoord.SetVertex(si, s, t);
                    }
                    if (normals) {
                        var x = getN6(f, 0);
                        var y = getN6(f, 1);
                        var z = getN6(f, 2);
                        aNormal.SetVertex(si, x, y, z);
                    }
                    si++;
                }
            }
        }

        float getV8(int idx, int part) {
            return (((idx >> part) & 1) == 1) ? -1.0f : 1.0f;
        }
        byte getC8(int idx, int part) {
            return (byte)((((idx >> part) & 1) == 1) ? 255 : 0);
        }
        byte getC6(int idx, int part) {
            return (byte)(((((idx + 1) >> part) & 1) == 1) ? 255 : 0);
        }
        float getT8(int idx, int part) {
            float e = (((idx >> part) & 1) == 1) ? 1.0f : 0;
            return e;
        }
        float getN6(int idx, int part) {
            float e = ((idx >> 1) == (2 - part)) ? 1 : 0;
            return (idx & 1) == 0 ? -e : e;
        }
        int[] faces = new int[]{
            4,5,7,6,//near
            0,2,3,1,//far
            2,6,7,3,//bottom
            0,1,5,4,//top
            1,3,7,5,//left
            0,4,6,2//right
        };
    }

}
