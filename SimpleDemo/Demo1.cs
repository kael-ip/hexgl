using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    class Demo1 : DemoBase {
        Renderer renderer;
        uint[] textures;
        Program program;
        UniformFloat _uOrigin;
        UniformMatrix _uAngles;
        UniformFloat _uViewOrigin;
        UniformMatrix _uViewAngles;
        UniformMatrix _uPerspective;
        UniformFloat _uLightVec;
        AttributeFloat _aPoint;
        AttributeFloat _aLightNormal;
        AttributeFloat _aTexCoord;
        UniformFloat _uAmbientLight;
        UniformFloat _uShadeLight;
        Sampler _tTexture;
        static float iq2 = (float)(1 / Math.Sqrt(2));
        static float iq3 = (float)(1 / Math.Sqrt(3));
        SimpleCube2 cube;
        float[] matProjection;
        public override void Prepare(IGL gl) {
            renderer = new Renderer(gl);
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
            var vsh = new VertexShader() { Source = vshaderSource };
            renderer.Shaders.Add(vsh);
            var fsh = new FragmentShader() { Source = fshaderSource };
            renderer.Shaders.Add(fsh);
            program = new HexTex.OpenGL.Program();
            renderer.Programs.Add(program);
            program.VertexShader = vsh;
            program.FragmentShader = fsh;
            program.Uniforms.Add(_uOrigin = new UniformFloat("uOrigin", 3));
            program.Uniforms.Add(_uAngles = new UniformMatrix("uAngles", 3));
            program.Uniforms.Add(_uViewOrigin = new UniformFloat("uViewOrigin", 3));
            program.Uniforms.Add(_uViewAngles = new UniformMatrix("uViewAngles", 3));
            program.Uniforms.Add(_uPerspective = new UniformMatrix("uPerspective", 4));
            program.Uniforms.Add(_uLightVec = new UniformFloat("uLightVec", 3));
            program.Uniforms.Add(_uAmbientLight = new UniformFloat("uAmbientLight", 1));
            program.Uniforms.Add(_uShadeLight = new UniformFloat("uShadeLight", 1));
            program.Uniforms.Add(_tTexture = new Sampler("tTexture"));
            program.Attributes.Add(_aPoint = new AttributeFloat("aPoint", 3));
            program.Attributes.Add(_aLightNormal = new AttributeFloat("aLightNormal", 3));
            program.Attributes.Add(_aTexCoord = new AttributeFloat("aTexCoord", 2));
            renderer.BuildAll();
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

            _uPerspective.Set(matProjection);
            _tTexture.Set(0);
            _uAmbientLight.Set(0.5f);
            _uShadeLight.Set(0.5f);
            _uLightVec.Set(iq3, -iq3, iq3);
            _uViewOrigin.Set(0, 0, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uViewAngles.Set(angles);

            var hVertex = cube.VertexArray.PinData();
            var hTexCoord = cube.TexCoordArray.PinData();
            var hNormal = cube.NormalArray.PinData();

            _aPoint.Set(hVertex.AddrOfPinnedObject(), cube.VertexArray.Width);
            _aTexCoord.Set(hTexCoord.AddrOfPinnedObject(), cube.TexCoordArray.Width);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), cube.NormalArray.Width);

            var dt = DateTime.Now;
            double tRotation = Math.PI * 2 * ((0.001 * dt.Millisecond) + dt.Second) / 60;
            GLMath.Rotate3(angles, tRotation, 0, iq2, iq2);
            _uAngles.Set(angles);

            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    for (int k = 0; k < 250; k++) {
                        _uOrigin.Set(200 * i - 500, 200 * j - 500, -200 * k);
                        renderer.DrawTriangles(program, 0, cube.Count);
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
