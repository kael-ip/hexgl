using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var dt = DateTime.Now;
            double time = ((0.001 * dt.Millisecond) + dt.Second) / 60;
            double tRotation = Math.PI * 2 * time * preset.Speed;

            float[] lightVec = new float[] { Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation)), Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation)), 0.5f };
            g.SetLightVector(lightVec);

            System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(preset.Offset[0], preset.Offset[1], preset.Offset[2]));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.66)));
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, preset.BackStep));
            var amat = vmat.ToMatrix3x4Array();
            g.SetCamMatrix(amat);

            g.SetProjection(3f, 1000f);

            g.SetObjMatrix(identity);
            g.SetColorIndex(0);
            g.DrawMesh(preset.System.Mesh, true);

            foreach(var controller in preset.System.Controllers) {
                float[] mat = new float[12];
                controller.ReadLocation3x4(mat);
                g.SetObjMatrix(mat);
                g.SetColorIndex(controller.Color);
                g.DrawMesh(repository.objCube, false);
                controller.Advance();
            }
        }
    }
}
