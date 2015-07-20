using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL.SimpleDemo {

    class Demo1 : DemoBase {
        uint[] textures;
        uint _fshader;
        uint _vshader;
        uint _program;
        int _uOrigin;
        int _uAngles;
        int _uViewOrigin;
        int _uViewAngles;
        int _uPerspective;
        int _uLightVec;
        int _aPoint;
        int _aLightNormal;
        int _aTexCoord;
        int _uAmbientLight;
        int _uShadeLight;
        int _tTexture;
        static float iq2 = (float)(1 / Math.Sqrt(2));
        static float iq3 = (float)(1 / Math.Sqrt(3));
        SimpleCube2 cube;
        float[] matProjection;
        public override void Prepare(IGL gl) {
            BuildShaders(gl);
            LoadTextures(gl);
            cube = new SimpleCube2(100, false, true, true);
            matProjection = GLMath.Frustum(-200, 200, 100, -100, 100, 100000);
        }
        private void BuildShaders(IGL gl) {
            var vshaderSource = @"
uniform vec3 uOrigin;
uniform mat3 uAngles;
uniform vec3 uViewOrigin;
uniform mat3 uViewAngles;
uniform mat4 uPerspective;
uniform vec3 uLightVec;
attribute vec3 aPoint;
attribute vec3 aLightNormal;
attribute vec2 aTexCoord;
varying vec2 vTexCoord;
varying float vLightDot;
void main(void)
{
	vec3 position = uViewAngles * (uAngles * aPoint.xyz + uOrigin - uViewOrigin);
    gl_Position = uPerspective * vec4(position.xyz, 1.0);
	vTexCoord = aTexCoord;
    vLightDot = dot(uViewAngles * (uAngles * aLightNormal), uLightVec);
}
";
            var fshaderSource = @"
precision mediump float;
uniform float uAmbientLight;
uniform float uShadeLight;
uniform sampler2D tTexture;
varying vec2 vTexCoord;
varying float vLightDot;
void main(void)
{
	vec4 texture = texture2D(tTexture, vTexCoord);
	gl_FragColor = vec4(texture.rgb * mix(1.0, vLightDot * uShadeLight + uAmbientLight, texture.a), 1.0);
}
";
            _vshader = gl.CreateShader(GL.VERTEX_SHADER);
            LoadShader(gl, _vshader, vshaderSource);
            _fshader = gl.CreateShader(GL.FRAGMENT_SHADER);
            LoadShader(gl, _fshader, fshaderSource);
            _program = gl.CreateProgram();
            gl.AttachShader(_program, _vshader);
            gl.AttachShader(_program, _fshader);
            gl.LinkProgram(_program);
            //
            _uOrigin = gl.GetUniformLocation(_program, "uOrigin");
            _uAngles = gl.GetUniformLocation(_program, "uAngles");
            _uViewOrigin = gl.GetUniformLocation(_program, "uViewOrigin");
            _uViewAngles = gl.GetUniformLocation(_program, "uViewAngles");
            _uPerspective = gl.GetUniformLocation(_program, "uPerspective");
            _uLightVec = gl.GetUniformLocation(_program, "uLightVec");
            _uAmbientLight = gl.GetUniformLocation(_program, "uAmbientLight");
            _uShadeLight = gl.GetUniformLocation(_program, "uShadeLight");
            _tTexture = gl.GetUniformLocation(_program, "tTexture");
            _aPoint = gl.GetAttribLocation(_program, "aPoint");
            _aLightNormal = gl.GetAttribLocation(_program, "aLightNormal");
            _aTexCoord = gl.GetAttribLocation(_program, "aTexCoord");
        }
        private void LoadShader(IGL gl, uint name, string source) {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(source);
            IntPtr ptr2 = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(ptr2, ptr);
            gl.ShaderSource(name, 1, ptr2, IntPtr.Zero);
            gl.CompileShader(name);
            Marshal.FreeHGlobal(ptr2);
            Marshal.FreeHGlobal(ptr);
        }
        private void LoadTextures(IGL gl) {
            int pw = 4, ph = 4;
            var bitmap = TexureHelper.Generate(pw, ph);
            var bitmapData = TexureHelper.ToRGBA(bitmap);
            textures = new uint[1];
            gl.GenTextures(1, textures);
            gl.ActiveTexture(GL.TEXTURE0);
            gl.BindTexture(GL.TEXTURE_2D, textures[0]);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR_MIPMAP_LINEAR);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.REPEAT);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.REPEAT);
            Helper.WithPinned(bitmapData, data => {
                gl.TexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, 1 << pw, 1 << ph, 0, GL.RGBA, GL.UNSIGNED_BYTE, data);
            });
            gl.GenerateMipmap(GL.TEXTURE_2D);

        }
        public override void Redraw(IGL gl) {
            gl.ClearColor(0, 0, 0, 0);
            gl.ClearDepthf(float.MaxValue);
            gl.Clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT | GL.STENCIL_BUFFER_BIT);
            //
            gl.Enable(GL.CULL_FACE);
            gl.FrontFace(GL.CCW);
            gl.CullFace(GL.BACK);
            gl.Enable(GL.DEPTH_TEST);
            gl.DepthFunc(GL.LESS);
            //
            gl.UseProgram(_program);

            Helper.WithPinned(matProjection, ptr => {
                gl.UniformMatrix4fv((uint)_uPerspective, 1, false, ptr);
            });

            gl.Uniform1i((uint)_tTexture, 0);
            gl.Uniform1f((uint)_uAmbientLight, 0.5f);
            gl.Uniform1f((uint)_uShadeLight, 0.5f);
            gl.Uniform3f((uint)_uLightVec, iq3, -iq3, iq3);

            gl.Uniform3f((uint)_uViewOrigin, 0, 0, 500f);

            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            Helper.WithPinned(angles, ptr => {
                gl.UniformMatrix3fv((uint)_uViewAngles, 1, false, ptr);
            });

            var hVertex = cube.VertexArray.PinData();
            var hTexCoord = cube.TexCoordArray.PinData();
            var hNormal = cube.NormalArray.PinData();

            gl.VertexAttribPointer((uint)_aPoint, cube.VertexArray.Width, GL.FLOAT, false, 0, hVertex.AddrOfPinnedObject());
            gl.EnableVertexAttribArray((uint)_aPoint);
            gl.VertexAttribPointer((uint)_aTexCoord, cube.TexCoordArray.Width, GL.FLOAT, false, 0, hTexCoord.AddrOfPinnedObject());
            gl.EnableVertexAttribArray((uint)_aTexCoord);
            gl.VertexAttribPointer((uint)_aLightNormal, cube.NormalArray.Width, GL.FLOAT, false, 0, hNormal.AddrOfPinnedObject());
            gl.EnableVertexAttribArray((uint)_aLightNormal);

            var dt = DateTime.Now;
            double tRotation = Math.PI*2*((0.001 * dt.Millisecond) + dt.Second) / 60;
            GLMath.Rotate3(angles, tRotation, 0, iq2, iq2);
            Helper.WithPinned(angles, ptr => {
                gl.UniformMatrix3fv((uint)_uAngles, 1, false, ptr);
            });

            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    for (int k = 0; k < 250; k++) {
                        gl.Uniform3f((uint)_uOrigin, 200 * i - 500, 200 * j - 500, -200 * k);
                        gl.DrawArrays(GL.TRIANGLES, 0, cube.Count);
                    }
                }
            }

            gl.Flush();
            gl.Finish();

            if(hVertex.IsAllocated) hVertex.Free();
            if(hTexCoord.IsAllocated)hTexCoord.Free();
            if(hNormal.IsAllocated)hNormal.Free();
        }

    }

}
