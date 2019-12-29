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
                earth = null;
                bullet = objBanner;
                camz = -10f;
                lightVec[0] = 0;
                lightVec[1] = 0;
                lightVec[2] = 1f;
                camPos[0] = 0;
                camPos[1] = 0;
                camPos[2] = 0;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV4;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                bullet = null;
                clipNear = 3f;
                t.FrameHandler = null;
            }));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysPlane;
                camz = -10f;
                lightVec[0] = 0;
                lightVec[1] = 0;
                lightVec[2] = 1f;
                camPos[0] = -5f;
                camPos[1] = -5f;
                camPos[2] = 0;
                speed1 = 1f / 8;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV3;
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
                t.FrameHandler = OnFrame_UpdateStateV3;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysSphere3;
                camz = -10f;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysMetaBall4;
                camz = -12f;
                speed1 = -3;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysMetaBall2;
                camz = -10f;
                speed1 = 3;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = sysSphereInside;
                camz = -6f;
                speed1 = -1f / 16;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandLoop(0, 1));
        }

        //
        //
        //

        // preloaded:
        static float q3 = (float)Math.Sqrt(3);
        WalkingSystem sysCube1;
        WalkingSystem sysSphere3;
        WalkingSystem sysSphereInside;
        WalkingSystem sysPlane;
        WalkingSystem sysMetaBall4;
        WalkingSystem sysMetaBall2;
        Mesh objCube;
        Mesh objBanner;

        // runtime:
        WalkingSystem earth;
        Mesh bullet;
        double tRotation;
        float speed1;
        float camz, camRotZ, camRotX;
        float[] camPos = new float[3];
        float[] lightVec = new float[3];
        float[] objMat = new float[12];
        float[] identityMat = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        int frame0;

        protected override void Init() {
            objCube = CreateCube();
            objBanner = CreateBanner();
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
        private Mesh CreateBanner() {
            QuadMap quadMap = new QuadMap();
            int[] data = new int[DemoData.Banner1.Length];
            var rnd = new PRNG();
            for(var i = 0; i < data.Length; i++) {
                data[i] = DemoData.Banner1[i] * (rnd.Next(12) + 1);
            }
            quadMap.BuildHeightPlane(data, 32, 17, 0, true);
            var quads = quadMap.Quads;
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            Mesh mesh = new QMesh(quads);
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
            _uLightVec.Set(lightVec);
            _uViewOrigin.Set(0, 0, 0);
            _uViewAngles.Set(identityMat, 0, 9);

            var projection = GLMath.Frustum(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar);
            _uPerspective.Set(projection);

            System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, -camz));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX(-camRotX));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ(-camRotZ));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(-camPos[0], -camPos[1], -camPos[2]));
            var amat = System.Numerics.Matrix4x4.Transpose(vmat).GetRotationMatrixAsArray();
            _uViewAngles.Set(amat, 0, 9);
            var omat = vmat.ToArray();
            _uViewOrigin.Set(omat, 12, 3);

            _uAngles.Set(objMat, 0, 9);
            _uOrigin.Set(objMat, 9, 3);

            SetColorIndex(0);
            if(earth != null) {
                DrawMesh(earth.Mesh, true);
                foreach(var controller in earth.Controllers) {
                    float[] mat = new float[12];
                    controller.ReadLocation3x4(mat);
                    _uAngles.Set(mat, 0, 9);
                    _uOrigin.Set(mat, 9, 3);
                    SetColorIndex(controller.Color);
                    DrawMesh(objCube, false);
                }
            }
            if(bullet != null) {
                DrawMesh(bullet, false);
            }
        }
        private void SetObjMatIdentity() {
            Array.Copy(identityMat, objMat, 9);
            objMat[9] = 0;
            objMat[10] = 0;
            objMat[11] = 0;
        }
        private void OnFrame_UpdateStateV1(Tracker tracker) {
            Trace.TraceInformation(">>> {0}", (tracker.Frame - frame0));
            double time = (tracker.Frame - frame0) * (1.0 / (64 * tracker.RowRate));
            tRotation = Math.PI * 2 * time * speed1;
            lightVec[0] = Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation));
            lightVec[1] = Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation));
            lightVec[2] = 0.5f;
            camRotZ = (float)tRotation;
            camRotX = (float)(-Math.PI / 2 * 0.66);
            SetObjMatIdentity();
            if(earth != null) {
                earth.Advance();
            }
        }
        private void OnFrame_UpdateStateV3(Tracker tracker) {
            Trace.TraceInformation(">>> {0}", (tracker.Frame - frame0));
            double time = (tracker.Frame - frame0) * (1.0 / (64 * tracker.RowRate));
            tRotation = Math.PI * 2 * time * speed1;
            camRotZ = (float)tRotation;
            camRotX = (float)(-Math.PI / 2 * 0.66);
            SetObjMatIdentity();
            if(earth != null) {
                earth.Advance();
            }
        }
        private void OnFrame_UpdateStateV4(Tracker tracker) {
            Trace.TraceInformation(">>> {0}", (tracker.Frame - frame0));
            double time = (tracker.Frame - frame0) * (1.0 / (64 * tracker.RowRate));
            tRotation = Math.PI * (0.5 + time);
            double tts = Math.Sin(tRotation);
            double tt = tts * tts * tts * tts * tts;
            double os = Math.Sin(Math.PI * 0.5 * tt), oc = Math.Cos(Math.PI * 0.5 * tt);
            double fz = 1 - 1 / (1 + Math.Abs(tt) * 1000);
            camz = (float)(-10 * fz - 800 * (1 - fz));
            clipNear = (float)(2 * fz + 160 * (1 - fz));
            camRotZ = 0;
            camRotX = 0;
            GLMath.Rotate3(objMat, Math.PI * 0.5 * tt, 0, 1, 0);
            objMat[9] = (float)(oc * -16 - 0 + os * 24);
            objMat[10] = -9.5f;
            objMat[11] = (float)(os * -16 - oc * 30 + 20);
        }
    }
}
