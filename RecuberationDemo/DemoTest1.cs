using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    class DemoTest1 : SimpleDemoBase {

        float[] vbuffer;
        float[] nbuffer;
        float[] tbuffer;

        public DemoTest1() {
            vbuffer = new float[]{
                3,7,1,
                4,7,1,
                4,8,1,
                3,8,1
            };
            //vbuffer = new float[]{ //CW
            //    3,7,1,
            //    3,8,1,
            //    4,8,1,
            //    4,7,1
            //};
            nbuffer = new float[]{
                0,0,1,
                0,0,1,
                0,0,1,
                0,0,1
            };
            tbuffer = new float[]{
                0,0,
                1,0,
                1,1,
                0,1
            };
        }
        protected override void LoadTextures(IGL gl) {
            int pw = 2, ph = 2;
            uint e = 0xffcc22ee;
            var bitmapData = new uint[]{
                e,0,0,e,
                e,e,e,0,
                e,0,e,0,
                e,e,0,0
            };
            textures = new uint[1];
            gl.GenTextures(1, textures);
            LoadTexture(gl, textures[0], GL.TEXTURE0, pw, ph, bitmapData);
        }
        private void LoadTexture(IGL gl, uint id, uint slot, int pw, int ph, Array data) {
            gl.ActiveTexture(slot);
            gl.BindTexture(GL.TEXTURE_2D, id);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR_MIPMAP_LINEAR);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.REPEAT);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.REPEAT);
            Helper.WithPinned(data, ptr => {
                gl.TexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, 1 << pw, 1 << ph, 0, GL.RGBA, GL.UNSIGNED_BYTE, ptr);
            });
            gl.GenerateMipmap(GL.TEXTURE_2D);
        }
        protected override void RedrawCore(IGL gl) {
            _uPerspective.Set(matProjection);
            _tTexture.Set(0);
            _uAmbientLight.Set(0.5f);
            _uShadeLight.Set(0.5f);
            _uLightVec.Set(0, 0, 1);
            _uViewOrigin.Set(0, 0, 5f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uViewAngles.Set(angles);

            var hVertex = GCHandle.Alloc(vbuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(nbuffer, GCHandleType.Pinned);
            var hTexUV = GCHandle.Alloc(tbuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            //_aTexCoord.Set(0, 0);
            _aTexCoord.Set(hTexUV.AddrOfPinnedObject(), 2);
            //_aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);
            _aLightNormal.Set(0, 0, 1);

            _uAngles.Set(angles);

            _uOrigin.Set(-3.5f, -7.5f, -1);

            for(int i = 0; i < vbuffer.Length / 3; i += 4) {
                renderer.DrawTriangleFans(program, i, 4);
            }

            if(hVertex.IsAllocated)
                hVertex.Free();
            if(hNormal.IsAllocated)
                hNormal.Free();
            if(hTexUV.IsAllocated)
                hTexUV.Free();
        }
    }
}
