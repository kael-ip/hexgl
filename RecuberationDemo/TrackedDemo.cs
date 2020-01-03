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
        protected override void SetupTracker(Tracker tracker) {
            base.SetupTracker(tracker);
            tracker.RowRate = 3;
            tracker.Add(new Tracker.CommandLabel());
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new TitleScene1(repository.objBannerMhm, 64, 32, 64 * t.RowRate);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64*2));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new TitleScene2(repository.objBanner, 32, 17, 64 * t.RowRate);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysPlane, 1f / 8, 64 * t.RowRate, false);
                scene.camPos[0] = -5f;
                scene.camPos[1] = -5f;
                scene.camPos[2] = 0;
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysCube1, 1, 64 * t.RowRate, false);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysSphere3, 1, 64 * t.RowRate, true);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysMetaBall4, -3, 64 * t.RowRate, true);
                scene.camz = -12f;
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysMetaBall2, 3, 64 * t.RowRate, true);
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
            tracker.Add(new Tracker.CommandCall(t => {
                scene = new WalkerScene(repository.sysSphereInside, -1f / 16, 64 * t.RowRate, true);
                scene.camz = -6f;
                frame0 = t.Frame;
            }));
            tracker.Add(new Tracker.CommandDelay(64));
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
