using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {
    abstract class SimpleDemoBase2 : SimpleDemoBase {
        protected void DrawMesh(Mesh mesh) {
            var hVertex = GCHandle.Alloc(mesh.VertexBuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(mesh.NormalBuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            _aTexCoord.Set(0, 0);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);

            for(int i = 0, j = 0; i < mesh.PrimitiveCount; i++, j += mesh.PrimitiveLength) {
                if(mesh.GetColor != null) {
                    SetColorIndex(mesh.GetColor(i));
                }
                renderer.DrawTriangleFans(program, j, mesh.PrimitiveLength);
            }

            if(hVertex.IsAllocated)
                hVertex.Free();
            if(hNormal.IsAllocated)
                hNormal.Free();
        }
    }
}
