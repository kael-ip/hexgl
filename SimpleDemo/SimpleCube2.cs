﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HexTex.OpenGL {
    public class SimpleCube2 : IDisposable {
        VertexArray<float> aVertex;
        VertexArray<float> aTexCoord;
        VertexArray<byte> aColor;
        VertexArray<float> aNormal;
        public int Count {
            get {
                return SimpleCubeBuilder.VertexCount;
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
            int vcount = SimpleCubeBuilder.VertexCount;
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
