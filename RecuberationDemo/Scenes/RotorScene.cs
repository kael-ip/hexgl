using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation.Scenes {

    class RotorScene : SceneBase {
        protected int period;
        protected bool isLightRotated;
        protected int color;
        public Mesh obj;
        public float speed;
        public RotorScene(Mesh obj, float z, float speed, int period, bool isLightRotated, int color) {
            this.obj = obj;
            this.speed = speed;
            this.period = period;
            this.isLightRotated = isLightRotated;
            this.color = color;
            lightVec[0] = 0;
            lightVec[1] = 0;
            lightVec[2] = 1f;
            camPos[0] = 0;
            camPos[1] = 0;
            camPos[2] = 0;
            camz = z;
            clipNear = 3f;
        }
        protected override void RenderObjects(Facade g) {
            g.SetAmbient(0.8f);
            g.SetShade(0.3f);
            g.SetObjMatrix(objMat);
            g.SetColorIndex(color);
            g.DrawMesh(obj, false);
        }
        public override void Update(int frame) {
            base.Update(frame);
            double time = frame * (1.0 / period);
            float tRotation = (float)(Math.PI * 2 * time);
            if(isLightRotated) {
                lightVec[0] = Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation));
                lightVec[1] = Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation));
                lightVec[2] = 0.5f;
            }
            camRotZ = tRotation * speed;
            camRotX = (float)(-Math.PI / 2 * 0.66);
            HexTex.OpenGL.GLMath.Identity3(objMat);
        }
    }
}
