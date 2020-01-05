using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation.Scenes {

    class WalkerScene : SceneBase {
        protected int period;
        protected bool isLightRotated;
        public WalkingSystem earth;
        public Mesh objCube;
        public float speed;
        public WalkerScene(WalkingSystem earth, float speed, int period, bool isLightRotated) {
            this.earth = earth;
            this.speed = speed;
            this.period = period;
            this.isLightRotated = isLightRotated;
            lightVec[0] = 0;
            lightVec[1] = 0;
            lightVec[2] = 1f;
            camPos[0] = 0;
            camPos[1] = 0;
            camPos[2] = 0;
            camz = 10f;
            clipNear = 3f;
            this.objCube = Repository.Instance.objCube;
        }
        protected override void RenderObjects(Facade g) {
            g.SetAmbient(0.8f);
            g.SetShade(0.3f);
            g.SetObjMatrix(objMat);
            g.SetColorIndex(0);
            g.DrawMesh(earth.Mesh, true, 180);
            foreach(var controller in earth.Controllers) {
                float[] mat = new float[12];
                controller.ReadLocation3x4(mat);
                g.SetObjMatrix(mat);
                g.SetColorIndex(controller.Color);
                g.DrawMesh(objCube, false);
            }
        }
        public override void Update(int frame) {
            base.Update(frame);
            double time = frame * (1.0 / period);
            float tRotation = (float)(Math.PI * 2 * time * speed);
            if(isLightRotated) {
                lightVec[0] = Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation));
                lightVec[1] = Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation));
                lightVec[2] = 0.5f;
            }
            camRotZ = tRotation;
            camRotX = (float)(-Math.PI / 2 * 0.66);
            HexTex.OpenGL.GLMath.Identity3(objMat);
            earth.Advance();
        }
    }
}
