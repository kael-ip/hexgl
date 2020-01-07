using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

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

    class PinRotorScene : SceneBase {
        protected int period;
        protected bool isLightRotated;
        public float speed;
        float[] objMat1 = new float[12];
        float[] objMat2 = new float[12];
        float[] objMat3 = new float[12];
        public PinRotorScene(float z, float speed, int period, bool isLightRotated) {
            this.speed = speed;
            this.period = period;
            this.isLightRotated = isLightRotated;
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
            g.SetColorIndex(0);
            g.DrawMesh(Repository.Instance.objAntiCube, false);

            g.SetObjMatrix(objMat1);
            g.SetColorIndex(11);
            g.DrawMesh(Repository.Instance.objPin, false);

            g.SetObjMatrix(objMat2);
            g.SetColorIndex(8);
            g.DrawMesh(Repository.Instance.objPin, false);

            g.SetObjMatrix(objMat3);
            g.SetColorIndex(1);
            g.DrawMesh(Repository.Instance.objPin, false);
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
            GLMath.Identity3(objMat);
            GLMath.Identity3(objMat1);
            objMat1[11] = 3 * (float)Math.Sin(tRotation);
            GLMath.Rotate3(objMat2, Math.PI * 0.5, 1, 0, 0);
            objMat2[10] = 3 * (float)Math.Sin(tRotation + Math.PI / 3);
            GLMath.Rotate3(objMat3, Math.PI * 0.5, 0, 1, 0);
            objMat3[9] = 3 * (float)Math.Sin(tRotation + Math.PI * 2 / 3);
        }
    }

    class SlabRotorScene : SceneBase {
        protected int period;
        protected bool isLightRotated;
        public float speed;
        float[][] objMats;
        int numobjs;
        public SlabRotorScene(float z, float speed, int period, bool isLightRotated) {
            this.speed = speed;
            this.period = period;
            this.isLightRotated = isLightRotated;
            lightVec[0] = 0;
            lightVec[1] = 0;
            lightVec[2] = 1f;
            camPos[0] = 0;
            camPos[1] = 0;
            camPos[2] = 0;
            camz = z;
            clipNear = 3f;
            numobjs = 12;
            objMats = new float[numobjs][];
            for(var i = 0; i < numobjs; i++) {
                objMats[i] = new float[12];
            }
        }
        protected override void RenderObjects(Facade g) {
            g.SetAmbient(0.8f);
            g.SetShade(0.3f);

            for(var i = 0; i < numobjs; i++) {
                g.SetObjMatrix(objMats[i]);
                g.SetColorIndex(i + 1);
                g.DrawMesh(Repository.Instance.objSlab, false);
            }
        }
        public override void Update(int frame) {
            base.Update(frame);
            double time = frame * (1.0 / period);
            float tRotation = -(float)(Math.PI * 2 * time);
            if(isLightRotated) {
                lightVec[0] = Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation));
                lightVec[1] = Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation));
                lightVec[2] = 0.5f;
            }
            camRotZ = 0;
            camRotX = (float)(-Math.PI / 2);

            for(var i = 0; i < numobjs; i++) {
                GLMath.Rotate3(objMats[i], Math.PI * i / 12 + tRotation * speed * (i + 100) / 100, 0, 0, 1);
                objMats[i][11] = i - numobjs / 2f;
            }
        }
    }
}
