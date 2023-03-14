using System;
using System.Collections.Generic;
using System.Text;

namespace HexTex.OpenGL {
    static class SimpleCubeBuilder {
        static int[] loop = new int[] { 0, 1, 2, 2, 3, 0 };
        static int[] faces = new int[]{
            4,5,7,6,//near
            0,2,3,1,//far
            2,6,7,3,//bottom
            0,1,5,4,//top
            1,3,7,5,//left
            0,4,6,2//right
        };
        const int vcount = 36;
        public static void Build(double size,
            VertexArray<float> aVertex,
            VertexArray<float> aTexCoord,
            VertexArray<byte> aColor,
            VertexArray<float> aNormal,
            bool vcolors) {
            float hsize = (float)size / 2;
            for(int f = 0; f < 6; f++) {
                int si = f * 6;
                for(int fi = 0; fi < 6; fi++) {
                    int vi = faces[f * 4 + loop[fi]];
                    {
                        var x = getV8(vi, 0) * hsize;
                        var y = getV8(vi, 1) * hsize;
                        var z = getV8(vi, 2) * hsize;
                        aVertex.SetVertex(si, x, y, z);
                    }
                    if(aColor != null) {
                        if(vcolors) {
                            var r = getC8(vi, 0);
                            var g = getC8(vi, 1);
                            var b = getC8(vi, 2);
                            var aa = (byte)255;
                            aColor.SetVertex(si, r, g, b, aa);
                        }
                        else {
                            var r = getC6(f, 0);
                            var g = getC6(f, 1);
                            var b = getC6(f, 2);
                            var aa = (byte)255;
                            aColor.SetVertex(si, r, g, b, aa);
                        }
                    }
                    if(aTexCoord != null) {
                        float s = getT86(vi, f, 0);
                        float t = getT86(vi, f, 1);
                        aTexCoord.SetVertex(si, s, t);
                    }
                    if(aNormal != null) {
                        var x = getN6(f, 0);
                        var y = getN6(f, 1);
                        var z = getN6(f, 2);
                        aNormal.SetVertex(si, x, y, z);
                    }
                    si++;
                }
            }
        }
        static float getV8(int idx, int part) {
            return (((idx >> part) & 1) == 1) ? -1.0f : 1.0f;
        }
        static byte getC8(int idx, int part) {
            return (byte)((((idx >> part) & 1) == 1) ? 255 : 0);
        }
        static byte getC6(int idx, int part) {
            return (byte)(((((idx + 1) >> part) & 1) == 1) ? 255 : 0);
        }
        static float getT86(int idx, int fid, int part) {
            switch(fid >> 1) {
                case 0:
                    idx = idx & 3;
                    break;
                case 1:
                    idx = (idx & 1) | ((idx >> 1) & 2);
                    break;
                case 2:
                    idx = (idx >> 1) & 3;
                    break;
            }
            float e = (((idx >> part) & 1) == 1) ? 1.0f : 0;
            return e;
        }
        static float getN6(int idx, int part) {
            float e = ((idx >> 1) == (2 - part)) ? 1 : 0;
            return (idx & 1) == 0 ? -e : e;
        }
    }
    public class SimpleCube2 : IDisposable {
        const int vcount = 36;
        VertexArray<float> aVertex;
        VertexArray<float> aTexCoord;
        VertexArray<byte> aColor;
        VertexArray<float> aNormal;
        public int Count {
            get {
                return vcount;
            }
        }
        public VertexArrayBase VertexArray {
            get {
                return aVertex;
            }
        }
        public VertexArrayBase TexCoordArray {
            get {
                return aTexCoord;
            }
        }
        public VertexArrayBase ColorArray {
            get {
                return aColor;
            }
        }
        public VertexArrayBase NormalArray {
            get {
                return aNormal;
            }
        }
        public SimpleCube2(double size) : this(size, true, true) { }
        public SimpleCube2(double size, bool vcolors, bool tex) : this(size, vcolors, tex, false) { }
        public SimpleCube2(double size, bool vcolors, bool tex, bool normals) {
            aVertex = new SimpleVertexArray<float>(vcount, 3, false);
            aTexCoord = tex ? new SimpleVertexArray<float>(vcount, 2, false) : null;
            aColor = new SimpleVertexArray<byte>(vcount, 4, false);
            aNormal = normals ? new SimpleVertexArray<float>(vcount, 3, false) : null;
            SimpleCubeBuilder.Build(size, aVertex, aTexCoord, aColor, aNormal, vcolors);
        }

        public void Dispose() {
            aVertex?.Dispose();
            aTexCoord?.Dispose();
            aColor?.Dispose();
            aNormal?.Dispose();
        }
    }
}
