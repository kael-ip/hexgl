using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {

    class DemoTitle : SimpleDemoBase2 {
        static float q3 = (float)Math.Sqrt(3);
        Mesh banner;

        public DemoTitle() {
            banner = CreateBanner();
        }
        private Mesh CreateBanner() {
            QuadMap quadMap = new QuadMap();
            int[] data = new int[DemoData.Banner1.Length];
            var rnd = new PRNG();
            for(var i = 0; i < data.Length; i++) {
                data[i] = DemoData.Banner1[i] * (rnd.Next(12) + 1);
            }
            quadMap.BuildHeightPlane(data, 32, 17, 0, true);
            var quads = quadMap.Quads;
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            Mesh mesh = new QMesh(quads);
            return mesh;
        }
        protected override void RedrawCore(IGL gl) {
            var dt = DateTime.Now;
            double time = ((0.001 * dt.Millisecond) + dt.Second) / 60;
            double tRotation = Math.PI * 2 * time;
            //_uPerspective.Set(matProjection);
            //_uLightVec.Set(iq3, -iq3, iq3);
            //_uLightVec.Set(Convert.ToSingle(q3 * 0.5f * Math.Cos(-tRotation)), Convert.ToSingle(q3 * 0.5f * Math.Sin(-tRotation)), 0.5f);
            _uLightVec.Set(0, 0, 1);
            //_uViewOrigin.Set(0, 0, 500f);
            _uViewOrigin.Set(0, 0, 0);
            //_uViewOrigin.Set(mousePosition.X - viewportSize.Width / 2, mousePosition.Y - viewportSize.Height, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uAngles.Set(angles);
            _uViewAngles.Set(angles);

            double tts = Math.Sin(tRotation * 3);
            double tt = tts * tts * tts * tts * tts;
            double os = Math.Sin(Math.PI * 0.5 * tt), oc = Math.Cos(Math.PI * 0.5 * tt);

            //set0: (old default)
            //float camz = -20f;
            //clipNear = 3f;
            //set1: (nearly parallel)
            //float camz = -800f;
            //clipNear = 160;
            //set2: (wide fov)
            //float camz = -10f;
            //clipNear = 2f;

            //double fz = Math.Abs(Math.Sin(Math.PI * tt));
            //double fz = 1, aaa = 0.999;
            //if(Math.Abs(os) < aaa) {
            //    fz = Math.Abs(os) / aaa;
            //}
            double fz = 1 - 1 / (1 + Math.Abs(tt) * 1000);
            //Math.Abs(os);

            float camz = (float)(-10 * fz - 800 * (1 - fz));
            clipNear = (float)(2 * fz + 160 * (1 - fz));
            {
                System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, 0));
                //vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
                //vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.66)));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, camz));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreatePerspectiveOffCenter(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar));
                _uPerspective.Set(vmat.ToArray());
            }

            GLMath.Rotate3(angles, Math.PI * 0.5 * tt, 0, 1, 0);
            //GLMath.Rotate3(angles, Math.PI * 0.5 * 0.9, 0, 1, 0);
            _uAngles.Set(angles);
            //GLMath.Rotate3(angles, tRotation, 0, 0, 1);
            //_uOrigin.Set(-16, -8, 0);//-0.9
            //_uOrigin.Set(0, -8, 0);//+0.9

            //_uOrigin.Set((float)(oc * -16 - 0 + os * 20), -9.5f, (float)(os * -16 - oc * 30 + 20));
            //_uOrigin.Set((float)(oc * -16 - 0 - os * 32), -9.5f, (float)(os * -16 - oc * 30 + 20));
            _uOrigin.Set((float)(oc * -16 - 0 + os * 24), -9.5f, (float)(os * -16 - oc * 30 + 20));
            //_uObject.Set(System.Numerics.Matrix4x4.Identity.ToArray());
            SetColorIndex(0);
            DrawMesh(banner, false);
        }
    }
}
