using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {

    class Demo4 : FacadeDemoBase {
        static float q3 = (float)Math.Sqrt(3);
        Mesh earth;
        Mesh cube;
        List<RollingController> controllers;

        public Demo4() {
            controllers = new List<RollingController>();
            earth = CreateEarth(9);
            cube = CreateCube();
        }
        private Mesh CreateCube() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildCube();
            var quads = quadMap.Quads;
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
            IBinaryVolume volume = new SphereVolume(0, 0, 0, 3.3f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var quads = quadMap.Quads;
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            var rnd = new PRNG();
            for(int i = 0; i < cubeCount; i++) {
                var controller = new RollingController();
                Quad quad = null;
                while(quad == null || quad.IsOccupied) {
                    quad = quads[rnd.Next(quads.Count)];
                }
                controller.Setup(quad, rnd.Next(2) == 0 ? Axis.X : Axis.Y, rnd.Next(2) != 0, 128 + (8 << rnd.Next(5)), i + 1);
                controllers.Add(controller);
            }
            Mesh mesh = new QMesh(quads);
            return mesh;
        }

        static float[] unitZ = new float[] { 0, 0, 1 };
        static float[] identity = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        protected override void OnPaint(Facade g) {
            var dt = DateTime.Now;
            double time = ((0.001 * dt.Millisecond) + dt.Second) / 60;
            double tRotation = Math.PI * 2 * time;

            float[] lightVec = new float[] { Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation)), Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation)), 0.5f };
            g.SetLightVector(lightVec);

            System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, 0));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.66)));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, 10f));
            var amat = vmat.ToMatrix3x4Array();
            g.SetCamMatrix(amat);

            g.SetProjection(3f, 1000f);

            g.SetObjMatrix(identity);
            g.SetColorIndex(0);
            g.DrawMesh(earth, true);

            foreach(var controller in controllers) {
                float[] mat = new float[12];
                controller.ReadLocation3x4(mat);
                g.SetObjMatrix(mat);
                g.SetColorIndex(controller.Color);
                g.DrawMesh(cube, false);
                controller.Advance();
            }
        }
    }
}
