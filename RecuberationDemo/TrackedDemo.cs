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
                bullet = objBanner2;
                camz = -10f;
                lightVec[0] = 0;
                lightVec[1] = 0;
                lightVec[2] = 1f;
                camPos[0] = 0;
                camPos[1] = 0;
                camPos[2] = 0;
                bsize[0] = 64;
                bsize[1] = 32;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV5;
            }));
            tracker.Add(new Tracker.CommandDelay(64*2));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = null;
                bullet = repository.objBanner;
                camz = -10f;
                lightVec[0] = 0;
                lightVec[1] = 0;
                lightVec[2] = 1f;
                camPos[0] = 0;
                camPos[1] = 0;
                camPos[2] = 0;
                bsize[0] = 32;
                bsize[1] = 17;
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
                earth = repository.sysPlane;
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
                earth = repository.sysCube1;
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
                earth = repository.sysSphere3;
                camz = -10f;
                speed1 = 1;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = repository.sysMetaBall4;
                camz = -12f;
                speed1 = -3;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = repository.sysMetaBall2;
                camz = -10f;
                speed1 = 3;
                frame0 = t.Frame;
                t.FrameHandler = OnFrame_UpdateStateV1;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                earth = repository.sysSphereInside;
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
        Mesh objBanner2;
        Repository repository;

        // runtime:
        WalkingSystem earth;
        Mesh bullet;
        double tRotation;
        float speed1;
        float clipNear = 3f, clipFar = 1000f;
        float camz, camRotZ, camRotX;
        float[] camPos = new float[3];
        float[] lightVec = new float[3];
        float[] objMat = new float[12];
        int[] bsize = new int[2];
        float[] identityMat = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        int frame0;

        protected override void Init() {
            repository = new Repository();
            repository.Init();
            objBanner2 = CreateBanner(Properties.Resources.rcbmhm);
        }
        private QMesh CreateBanner(System.Drawing.Bitmap bitmap) {
            var pixelMap = PixelMap.Load(bitmap);
            QuadMap quadMap = new QuadMap();
            var rnd = new PRNG();
            quadMap.BuildHeightPlane(pixelMap.Width, pixelMap.Height,
                //(x, y) => pixelMap[x, y] * (rnd.Next(12) + 1), 0, true);
                (x, y) => pixelMap[x, y] * 32 / 256, 0, true);
            var quads = quadMap.Quads;
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            QMesh mesh = new QMesh(quads);
            return mesh;
        }
        protected override void OnPaint(Facade g) {
            g.SetLightVector(lightVec);

            System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(-camPos[0], -camPos[1], -camPos[2]));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ(-camRotZ));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX(camRotX));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, -camz));
            var amat = vmat.ToMatrix3x4Array();
            g.SetCamMatrix(amat);

            g.SetProjection(clipNear, clipFar);


            g.SetObjMatrix(objMat);
            g.SetColorIndex(0);
            if(earth != null) {
                g.DrawMesh(earth.Mesh, true);
                foreach(var controller in earth.Controllers) {
                    float[] mat = new float[12];
                    controller.ReadLocation3x4(mat);
                    g.SetObjMatrix(mat);
                    g.SetColorIndex(controller.Color);
                    g.DrawMesh(repository.objCube, false);
                }
            }
            if(bullet != null) {
                g.DrawMesh(bullet, false);
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
            camz = (float)(-10 * fz - bsize[0] * 25 * (1 - fz));
            clipNear = (float)(2 * fz + 160 * (1 - fz));
            clipFar = 10000f;
            camRotZ = 0;
            camRotX = 0;
            GLMath.Rotate3(objMat, Math.PI * 0.5 * tt, 0, 1, 0);
            objMat[9] = (float)(oc * -bsize[0] / 2 - 0 + os * 24);
            objMat[10] = -bsize[1] / 2 - 1;
            objMat[11] = (float)(os * -bsize[0] / 2 - oc * 30 + 20);
        }
        private void OnFrame_UpdateStateV5(Tracker tracker) {
            double time = (tracker.Frame - frame0) * (1.0 / (64 * 2 * tracker.RowRate));
            tRotation = Math.PI * (0.5 + time);
            double tts = Math.Sin(tRotation);
            double tt = tts * tts * tts; //* tts * tts;
            Trace.TraceInformation(">>> {0} :: {1:f4}", (tracker.Frame - frame0), tt);
            double os = Math.Sin(Math.PI * 0.5 * tt), oc = Math.Cos(Math.PI * 0.5 * tt);
            double fz = 1 - 1 / (1 + Math.Abs(tt) * 1000);
            camz = (float)(-10 * fz - bsize[0] * 25 * (1 - fz));
            clipNear = (float)(2 * fz + 160 * (1 - fz));
            clipFar = 10000f;
            camRotZ = 0;
            camRotX = 0;
            GLMath.Rotate3(objMat, Math.PI * 0.5 * tt, 1, 0, 0);
            objMat[9] = (float)(-bsize[0] / 2);
            objMat[10] = (float)(-bsize[1] / 2 - 1 - (oc - 1) * 32);
            objMat[11] = (float)(-20);
        }
    }
}
