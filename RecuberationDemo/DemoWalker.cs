using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {
    class DemoWalker : FacadeDemoBase {
        static float q3 = (float)Math.Sqrt(3);
        Repository repository;

        class Preset {
            public WalkingSystem System;
            public float[] Offset;
            public float BackStep;
            public float Speed;
            public Preset(WalkingSystem system, float ox, float oy, float oz, float bs, float speed) {
                this.System = system;
                this.Offset = new float[] { ox, oy, oz };
                this.BackStep = bs;
                this.Speed = speed;
            }
        }

        Preset[] presets;
        int pIndex;

        public DemoWalker() {
            repository = new Repository();
            repository.Init();
            presets = new Preset[]{
                new Preset(repository.sysLensHedge, 0,0,0,24,1),
                new Preset(repository.sysSphere3x, 0,0,0,10,1),
                new Preset(repository.sysCuboid3, 0,0,0,25,3),
                new Preset(repository.sysCuboid0, 0,0,0,15,3),
                new Preset(repository.sysCube1, 0,0,0,10,1),
                new Preset(repository.sysPlane, 5,5,0,10,1),
                new Preset(repository.sysSphere3, 0,0,0,10,1),
                new Preset(repository.sysMetaBall4, 0,0,0,12,3),
                new Preset(repository.sysMetaBall2, 0,0,0,10,3),
                new Preset(repository.sysSphereInside, 0,0,0,6,1f/16)
            };
            pIndex = 0;
        }
        public override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == System.Windows.Forms.Keys.Enter) {
                pIndex = (pIndex + 1) % presets.Length;
            }
        }

        static float[] unitZ = new float[] { 0, 0, 1 };
        static float[] identity = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        protected override void OnPaint(Facade g) {
            Preset preset = presets[pIndex];
            
            g.SetAmbient(0.8f);
            g.SetShade(0.3f);

            var dt = DateTime.Now;
            double time = ((0.001 * dt.Millisecond) + dt.Second) / 60;
            double tRotation = Math.PI * 2 * time * preset.Speed;

            float[] lightVec = new float[] { Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation)), Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation)), 0.5f };
            g.SetLightVector(lightVec);

            float[] mat = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            float[] rmat = new float[12];
            GLMath.Translate3(rmat, preset.Offset[0], preset.Offset[1], preset.Offset[2]);
            GLMath.MatrixMultiply(mat, mat, rmat);
            GLMath.Rotate3(rmat, (float)tRotation, 0, 0, 1);
            GLMath.MatrixMultiply(mat, mat, rmat);
            GLMath.Rotate3(rmat, (float)(-Math.PI / 2 * 0.66), 1, 0, 0);
            GLMath.MatrixMultiply(mat, mat, rmat);
            GLMath.Translate3(rmat, 0, 0, preset.BackStep);
            GLMath.MatrixMultiply(mat, mat, rmat);
            g.SetCamMatrix(mat);

            g.SetProjection(3f, 1000f);

            g.SetObjMatrix(identity);
            g.SetColorIndex(0);
            g.DrawMesh(preset.System.Mesh, true, 200);

            foreach(var controller in preset.System.Controllers) {
                controller.ReadLocation3x4(mat);
                g.SetObjMatrix(mat);
                g.SetColorIndex(controller.Color);
                g.DrawMesh(repository.objCube, false);
                controller.Advance();
            }
        }
    }
}
