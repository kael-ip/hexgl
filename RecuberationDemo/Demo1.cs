using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {

    class Demo1 : SimpleDemoBase {

        Mesh earth;

        public Demo1() {
            Quad.ccwFront = false;
            clipNear = 100;
            clipFar = 1000;
            //IBinaryVolume volume = new RandomHeightPlane(12, 12, 9, 1);
            IBinaryVolume volume = new SphereVolume(5, 3, 7, 9);
            //IBinaryVolume volume = new SphereVolume(5, 3, 7, 1);
            //IBinaryVolume volume = new SphereVolume(0, 0, 0, 1);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            earth = new Mesh(4, quads.Count, true, false);
            int i = 0;
            foreach(var quad in quads) {
                i = quad.FillQuadVerts(earth.VertexBuffer, earth.NormalBuffer, i);
            }
        }
        protected override void RedrawCore(IGL gl) {
            _uPerspective.Set(matProjection);
            _tTexture.Set(0);
            //_uLightVec.Set(iq3, -iq3, iq3);
            _uLightVec.Set(0, 0, 1);
            _uViewOrigin.Set(0, 0, 500f);
            //_uViewOrigin.Set(mousePosition.X - viewportSize.Width / 2, mousePosition.Y - viewportSize.Height, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uViewAngles.Set(angles);

            var dt = DateTime.Now;
            double tRotation = Math.PI * 2 * ((0.001 * dt.Millisecond) + dt.Second) / 60;
            GLMath.Rotate3(angles, tRotation, 0, iq2, iq2);
            _uAngles.Set(angles);

            _uOrigin.Set(-5, -3, -7);
            //_uOrigin.Set(0, 0, 0);

            DrawMesh(earth);
        }
        private void DrawMesh(Mesh mesh) {
            var hVertex = GCHandle.Alloc(mesh.VertexBuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(mesh.NormalBuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            _aTexCoord.Set(0, 0);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);

            for(int i = 0, j = 0; i < mesh.PrimitiveCount; i++, j += mesh.PrimitiveLength) {
                renderer.DrawTriangleFans(program, j, mesh.PrimitiveLength);
            }

            if(hVertex.IsAllocated)
                hVertex.Free();
            if(hNormal.IsAllocated)
                hNormal.Free();
        }
    }
}
