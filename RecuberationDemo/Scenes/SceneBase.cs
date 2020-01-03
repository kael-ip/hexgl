using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation.Scenes {
    abstract class SceneBase {
        public static readonly float q3 = (float)Math.Sqrt(3);
        public float camz, camRotZ, camRotX;
        public float[] camPos = new float[3];
        public float[] lightVec = new float[3];
        public float[] objMat = new float[12];
        public float clipNear = 10f, clipFar = 1000f;
        public float[] identityMat = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        protected void SetIdentity(float[] mat) {
            Array.Copy(identityMat, mat, 9);
            mat[9] = 0;
            mat[10] = 0;
            mat[11] = 0;
        }

        public virtual void Update(int frame) { }
        public virtual void Render(Facade g) {
            SetupCamera(g);
            g.SetLightVector(lightVec);
            g.SetProjection(clipNear, clipFar);
            RenderObjects(g);
        }
        protected virtual void SetupCamera(Facade g) {
            float[] mat = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            float[] rmat = new float[12];
            GLMath.Translate3(rmat, camPos[0], camPos[1], camPos[2]);
            GLMath.MatrixMultiply(mat, mat, rmat);
            GLMath.Rotate3(rmat, -camRotZ, 0, 0, 1);
            GLMath.MatrixMultiply(mat, mat, rmat);
            GLMath.Rotate3(rmat, camRotX, 1, 0, 0);
            GLMath.MatrixMultiply(mat, mat, rmat);
            GLMath.Translate3(rmat, 0, 0, camz);
            GLMath.MatrixMultiply(mat, mat, rmat);
            g.SetCamMatrix(mat);
        }
        protected abstract void RenderObjects(Facade g);
    }
}
