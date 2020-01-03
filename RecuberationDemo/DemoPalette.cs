using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    class DemoPalette : FacadeDemoBase {
        QMesh banner;

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
            }
        }

        static float[] unitZ = new float[] { 0, 0, 1 };
        static float[] identity = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
        protected override void OnPaint(Facade g) {
            g.SetLightVector(unitZ);

            System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
            vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(8, 8, 13));
            var amat = vmat.ToMatrix3x4Array();
            g.SetCamMatrix(amat);

            g.SetProjection(3f, 1000f);

            g.SetObjMatrix(identity);
            g.SetColorIndex(0);
            g.DrawMesh(banner, true);
        }
    }
}
