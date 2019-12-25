using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    abstract class TrackedDemoBase : SimpleDemoBase2 {
        Stopwatch stopWatch;
        long trackerTime = 0;
        int trackerFrameSpan = 20; // 20ms --> 50Hz
        Tracker tracker;
        public TrackedDemoBase() {
            tracker = new Tracker();
            SetupTracker(tracker);
            Init();
            tracker.Reset();
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }
        protected virtual void Init() { }
        protected virtual void SetupTracker(Tracker tracker) { }
        public override void Redraw(IGL gl) {
            while(trackerTime < stopWatch.ElapsedMilliseconds) {
                trackerTime += trackerFrameSpan;
                tracker.Advance();
            }
            base.Redraw(gl);
        }
    }
}
