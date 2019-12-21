using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    class Demo3 : SimpleDemoBase {
        Mesh earth;
        Mesh cube;
        List<RollingController> controllers;
        float[][] palette;

        public Demo3() {
            controllers = new List<RollingController>();
            earth = CreateEarth(1);
            cube = CreateCube();
            palette = DemoHelper.GeneratePalette(20);
            palette[0] = new float[] { 1f, 1f, 1f };
        }
        private Mesh CreateCube() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildCube();
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            Mesh mesh = new Mesh(4, quads.Count, true, false);
            int i = 0;
            foreach(var quad in quads) {
                i = quad.FillQuadVerts(mesh.VertexBuffer, mesh.NormalBuffer, i);
            }
            return mesh;
        }
        private Mesh CreateEarth(int cubeCount) {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildCube();
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            var rnd = new Random();
            for(int i = 0; i < cubeCount; i++) {
                var controller = new RollingController();
                Quad quad = null;
                while(quad == null || quad.IsOccupied) {
                    quad = quads[rnd.Next(quads.Count)];
                }
                controller.Setup(quad, rnd.Next(2) == 0 ? Axis.X : Axis.Y, rnd.Next(2) != 0, 128 + (8 << rnd.Next(5)), i + 1);
                controllers.Add(controller);
            }
            Mesh mesh = new Mesh(4, quads.Count, true, false);
            mesh.GetColor = index => quads[index].Color;
            int offset = 0;
            foreach(var quad in quads) {
                offset = quad.FillQuadVerts(mesh.VertexBuffer, mesh.NormalBuffer, offset);
            }
            return mesh;
        }
        protected override void RedrawCore(IGL gl) {
            //_uPerspective.Set(matProjection);
            _tTexture.Set(0);
            _uAmbientLight.Set(0.2f);
            _uShadeLight.Set(0.5f);
            //_uLightVec.Set(iq3, -iq3, iq3);
            _uLightVec.Set(0, 0, 1);
            //_uViewOrigin.Set(0, 0, 500f);
            _uViewOrigin.Set(0, 0, 0);
            //_uViewOrigin.Set(mousePosition.X - viewportSize.Width / 2, mousePosition.Y - viewportSize.Height, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uAngles.Set(angles);
            _uViewAngles.Set(angles);

            var dt = DateTime.Now;
            double tRotation = Math.PI * 2 * ((0.001 * dt.Millisecond) + dt.Second) / 60;
            {
                System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, 0));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.66)));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, -10f));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreatePerspectiveOffCenter(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar));
                _uPerspective.Set(vmat.ToArray());
            }

            //GLMath.Rotate3(angles, tRotation, 0, 0, 1);
            _uOrigin.Set(0, 0, 0);
            //_uObject.Set(System.Numerics.Matrix4x4.Identity.ToArray());
            _aVertexColor.Set(1.0f, 1.0f, 1.0f);
            DrawMesh(earth);

            foreach(var controller in controllers) {
                controller.ReadLocation(_uOrigin, _uAngles);
                //_aVertexColor.Set(1.0f, 0.4f, 0.4f);
                var c = palette[controller.Color % palette.Length];
                _aVertexColor.Set(c[0], c[1], c[2]);
                DrawMesh(cube);
                controller.Advance();
            }
        }
        private void DrawMesh(Mesh mesh) {
            var hVertex = GCHandle.Alloc(mesh.VertexBuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(mesh.NormalBuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            _aTexCoord.Set(0, 0);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);

            for(int i = 0, j = 0; i < mesh.PrimitiveCount; i++, j += mesh.PrimitiveLength) {
                if(mesh.GetColor != null) {
                    var c = palette[mesh.GetColor(i) % palette.Length];
                    _aVertexColor.Set(c[0], c[1], c[2]);
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
