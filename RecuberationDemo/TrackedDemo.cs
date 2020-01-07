using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;
using HexTex.Recuberation.Scenes;

namespace HexTex.Recuberation {

    class TrackedDemo : TrackedDemoBase {
        const int patrows = 48;
        protected override void SetupTracker(Tracker tracker) {
            base.SetupTracker(tracker);
            tracker.RowRate = 3;
            tracker.Add(new Tracker.CommandLabel());
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new TitleScene1(repository.objBannerMhm, 64, 32, patrows * t.RowRate);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 2));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new RotorScene(repository.objFirTree, 25f, 0.2f, patrows * t.RowRate, true, -1) {
                    zoff = -5, tilt = 0.8f
                };
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 4));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new SlabRotorScene(15, 0.2f, patrows * t.RowRate, true);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 4));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new PinRotorScene(15, 1, patrows * t.RowRate, true);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 4));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new RotorScene(repository.sysCuboid1.Mesh, 25, 1, patrows * t.RowRate, true, 2);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 2));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new RotorScene(repository.sysCuboid2.Mesh, 25, 1, patrows * t.RowRate, true, 7);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 2));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new RotorScene(repository.sysCuboid3.Mesh, 25, 1, patrows * t.RowRate, true, 4);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows * 2));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new TitleScene2(repository.objBanner, 32, 17, patrows * t.RowRate);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysPlane, 1f / 8, patrows * t.RowRate, false);
                scene.camPos[0] = 5f;
                scene.camPos[1] = 5f;
                scene.camPos[2] = 0;
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysCube1, 1, patrows * t.RowRate, false);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysSphere3, 1, patrows * t.RowRate, true);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysMetaBall4, -3, patrows * t.RowRate, true);
                scene.camz = 12f;
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysMetaBall2, 3, patrows * t.RowRate, true);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysSphereInside, -1f / 16, patrows * t.RowRate, true);
                scene.camz = 6f;
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(patrows));
            tracker.Add(new Tracker.CommandLoop(0, 1));
            tracker.FrameHandler = OnFrame;
        }

        //
        //
        //

        Repository repository;
        SceneBase scene;
        int frame0;

        protected override void Init() {
            repository = new Repository();
            repository.Init();
        }
        protected override void OnPaint(Facade g) {
            if(scene != null) {
                scene.Render(g);
            }
        }
        private void OnFrame(Tracker tracker) {
            Trace.TraceInformation(">>> {0}", (tracker.Frame - frame0));
            if(scene != null) {
                scene.Update(tracker.Frame - frame0);
            }
        }
    }
}
