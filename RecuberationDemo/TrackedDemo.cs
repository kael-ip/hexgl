using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {

    class TrackedDemo : TrackedDemoBase {
        protected override void SetupTracker(Tracker tracker) {
            base.SetupTracker(tracker);
            tracker.RowRate = 3;
            tracker.Add(new Tracker.CommandLabel());
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysPlane;
                camz = -10f;
                lightVec[0] = 0;
                lightVec[1] = 0;
                lightVec[2] = 1f;
                camPos[0] = -5f;
                camPos[1] = -5f;
                camPos[2] = 0;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = UpdateStateV3;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                camPos[0] = 0;
                camPos[1] = 0;
                camPos[2] = 0;
            }));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysCube1;
                camz = -10f;
                lightVec[0] = 0;
                lightVec[1] = 0;
                lightVec[2] = 1f;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = UpdateStateV3;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysSphere3;
                camz = -10f;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = UpdateState;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysMetaBall4;
                camz = -12f;
                speed1 = -5;
                frame0 = t.Frame;
                t.FrameHandler = UpdateState;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysMetaBall2;
                camz = -10f;
                speed1 = 5;
                frame0 = t.Frame;
                t.FrameHandler = UpdateState;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysSphereInside;
                camz = -6f;
                speed1 = -1;
                frame0 = t.Frame;
                t.FrameHandler = UpdateState;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandLoop(0, 1));
        }

        //
        //
        //

        static float q3 = (float)Math.Sqrt(3);
        WalkingSystem sysCube1;
        WalkingSystem sysSphere3;
        WalkingSystem sysSphereInside;
        WalkingSystem sysPlane;
        WalkingSystem sysMetaBall4;
        WalkingSystem sysMetaBall2;
        WalkingSystem earth;
        Mesh objCube;
        double tRotation;
        float speed1;
        float camz;
        float[] camPos = new float[3];
        float[] lightVec = new float[3];
        int frame0;

        protected override void Init() {
            objCube = CreateCube();
            sysCube1 = CreateEarthCube();
            sysSphere3 = CreateEarthSphere3();
            sysPlane = CreateEarthPlane();
            sysSphereInside = CreateEarthSphereInside();
            sysMetaBall4 = CreateEarthMB4();
            sysMetaBall2 = CreateEarthMB2();
        }
        private Mesh CreateCube() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildCube();
            var quads = quadMap.Quads;
            Mesh mesh = new Mesh(4, quads.Count, true, false);
            int i = 0;
            foreach(var quad in quads) {
                i = quad.FillQuadVerts(mesh.VertexBuffer, mesh.NormalBuffer, i);
            }
            return mesh;
        }
        private WalkingSystem CreateEarthPlane() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildPlane(10, 10);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(10, 657, 32, 4, 5);
            return system;
        }
        private WalkingSystem CreateEarthCube() {
            var volume = new SphereVolume(0, 0, 0, 0.8f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(1, 1432, 32, 0);
            return system;
        }
        private WalkingSystem CreateEarthSphere3() {
            var volume = new SphereVolume(0, 0, 0, 3.3f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(7, 8465, 64, 8, 5);
            return system;
        }
        private WalkingSystem CreateEarthSphereInside() {
            var volume = new SphereVolume(0, 0, 0, 7.8f, true);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(37, 8465, 64, 16, 6);
            return system;
        }
        private WalkingSystem CreateEarthMB4() {
            //var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            //volume.AddBall(-2, 0, 1, 5.3f);
            //volume.AddBall(1, -4, 3, -1.3f);
            //volume.AddBall(3, 3, -2, 2.4f);
            var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            volume.AddBall(-3, 2, -3, 2.3f);
            volume.AddBall(3, 2, 3, 2.3f);
            volume.AddBall(2, -2, -2, 2.3f);
            volume.AddBall(-2, -2, 2, 2.3f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(9, 2020, 64, 4, 5);
            return system;
        }
        private WalkingSystem CreateEarthMB2() {
            var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            volume.AddBall(3, 3, 1, 2.5f);
            volume.AddBall(-2, -1, -2, 3.4f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(9, 6345, 64, 8, 4);
            return system;
        }
        protected override void RedrawCore(IGL gl) {
            //_uPerspective.Set(matProjection);
            //_uLightVec.Set(iq3, -iq3, iq3);
            _uLightVec.Set(lightVec);
            //_uViewOrigin.Set(0, 0, 500f);
            _uViewOrigin.Set(0, 0, 0);
            //_uViewOrigin.Set(mousePosition.X - viewportSize.Width / 2, mousePosition.Y - viewportSize.Height, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uAngles.Set(angles);
            _uViewAngles.Set(angles);

            {
                System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(camPos[0], camPos[1], camPos[2]));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.66)));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, camz));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreatePerspectiveOffCenter(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar));
                _uPerspective.Set(vmat.ToArray());
            }

            //GLMath.Rotate3(angles, tRotation, 0, 0, 1);
            _uOrigin.Set(0, 0, 0);
            //_uObject.Set(System.Numerics.Matrix4x4.Identity.ToArray());
            SetColorIndex(0);
            if(earth != null) {
                DrawMesh(earth.Mesh, true);
                foreach(var controller in earth.Controllers) {
                    controller.ReadLocation(_uOrigin, _uAngles);
                    SetColorIndex(controller.Color);
                    DrawMesh(objCube, false);
                }
            }
        }
        private void UpdateState(Tracker tracker) {
            Trace.TraceInformation(">>> {0}", (tracker.Frame - frame0));
            //double time = (tracker.Frame - frame0) * (1 / 640.0);
            double time = (tracker.Frame - frame0) * (1 / 700.0);
            tRotation = Math.PI * 2 * time * speed1;
            lightVec[0] = Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation));
            lightVec[1] = Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation));
            lightVec[2] = 0.5f;
            if(earth != null) {
                earth.Advance();
            }
        }
        private void UpdateStateV3(Tracker tracker) {
            Trace.TraceInformation(">>> {0}", (tracker.Frame - frame0));
            double time = (tracker.Frame - frame0) * (1 / 700.0);
            tRotation = Math.PI * 2 * time * speed1;
            if(earth != null) {
                earth.Advance();
            }
        }
    }
}
