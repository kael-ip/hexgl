using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation.Scenes {

    class TitleSceneBase : SceneBase {
        protected readonly Mesh banner;
        protected int sizex, sizey;
        protected int period;
        public TitleSceneBase(Mesh banner, int sizex, int sizey, int period) {
            this.banner = banner;
            this.sizex = sizex;
            this.sizey = sizey;
            this.period = period;
            camz = -10f;
            lightVec[0] = 0;
            lightVec[1] = 0;
            lightVec[2] = 1f;
            camPos[0] = 0;
            camPos[1] = 0;
            camPos[2] = 0;
        }
        protected override void RenderObjects(Facade g) {
            g.SetObjMatrix(objMat);
            g.SetColorIndex(0);
            g.DrawMesh(banner, false);
        }
    }

    class TitleScene1 : TitleSceneBase {
        public TitleScene1(Mesh banner, int sizex, int sizey, int period)
            : base(banner, sizex, sizey, period) {
        }
        public override void Update(int frame) {
            base.Update(frame);
            double time = frame * (1.0 / (2 * period));
            double tRotation = Math.PI * (0.5 + time);
            double tts = Math.Sin(tRotation);
            double tt = tts * tts * tts; //* tts * tts;
            Trace.TraceInformation(">>> {0} :: {1:f4}", frame, tt);
            double os = Math.Sin(Math.PI * 0.5 * tt), oc = Math.Cos(Math.PI * 0.5 * tt);
            double fz = 1 - 1 / (1 + Math.Abs(tt) * 1000);
            camz = (float)(-10 * fz - sizex * 25 * (1 - fz));
            clipNear = (float)(2 * fz + 160 * (1 - fz));
            clipFar = 10000f;
            camRotZ = 0;
            camRotX = 0;
            GLMath.Rotate3(objMat, -Math.PI * 0.5 * tt, 1, 0, 0);
            objMat[9] = (float)(-sizex / 2);
            objMat[10] = (float)(-sizey / 2 - 1 - (oc - 1) * 32);
            objMat[11] = (float)(-20);
        }
    }

    class TitleScene2 : TitleSceneBase {
        public TitleScene2(Mesh banner, int sizex, int sizey, int period)
            : base(banner, sizex, sizey, period) {
        }
        public override void Update(int frame) {
            base.Update(frame);
            double time = frame * (1.0 / period);
            double tRotation = Math.PI * (0.5 + time);
            double tts = Math.Sin(tRotation);
            double tt = tts * tts * tts * tts * tts;
            double os = Math.Sin(Math.PI * 0.5 * tt), oc = Math.Cos(Math.PI * 0.5 * tt);
            double fz = 1 - 1 / (1 + Math.Abs(tt) * 1000);
            camz = (float)(-10 * fz - sizex * 25 * (1 - fz));
            clipNear = (float)(2 * fz + 160 * (1 - fz));
            clipFar = 10000f;
            camRotZ = 0;
            camRotX = 0;
            GLMath.Rotate3(objMat, -Math.PI * 0.5 * tt, 0, 1, 0);
            objMat[9] = (float)(oc * -sizex / 2 - 0 + os * 24);
            objMat[10] = -sizey / 2 - 1;
            objMat[11] = (float)(os * -sizex / 2 - oc * 30 + 20);
        }
    }
}
