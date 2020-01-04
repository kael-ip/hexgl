using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    class DemoPalette : FacadeDemoBase {
        QMesh banner;
        bool isRotated;

        public DemoPalette() {
            var map = new QuadMap();
            map.BuildPlane(16, 16);
            banner = new QMesh(map.Quads);
            foreach(var q in banner.Quads) {
                q.Color = q.LocationOnPlane.X + q.LocationOnPlane.Y * 16;
            }
        }
        public override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == System.Windows.Forms.Keys.Enter) {
                isRotated = !isRotated;
            }
        }

        static float[] unitZ = new float[] { 0, 0, 1 };
        static float[] identity = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        protected override void OnPaint(Facade g) {
            var dt = DateTime.Now;
            float time = ((0.001f * dt.Millisecond) + dt.Second) / 60;
            float angle = ((float)Math.PI) * (Fraction(time * 5) - 0.5f);

            g.SetLightVector(unitZ);

            System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(8, 8, 100));
            var amat = vmat.ToMatrix3x4Array();
            g.SetCamMatrix(amat);

            g.SetProjection(23f, 1000f);

            if(isRotated) {
                vmat = System.Numerics.Matrix4x4.CreateTranslation(-8, 0, 0);
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationY(angle));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(8, 0, 0));
                g.SetObjMatrix(vmat.ToMatrix3x4Array());
            } else {
                g.SetObjMatrix(identity);
            }

            g.SetColorIndex(0);
            g.DrawMesh(banner, true);
        }
        private static float Fraction(float v) {
            return v - (float)Math.Floor(v);
        }
    }
}
