using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    class CubesDemo1 : CubesDemoBase {
        protected override SimpleCube2 CreateCube() {
            return new SimpleCube2(100, false, true, true);
        }
    }
    class CubesDemo2 : CubesDemoBase {
        protected override SimpleCube2 CreateCube() {
            return new SimpleCube2(100, false, false, true);
        }
    }
    class CubesDemo3 : CubesDemoBase {
        protected override SimpleCube2 CreateCube() {
            ambient = 1f;
            return new SimpleCube2(100, false, false, false);
        }
    }
    class CubesDemo4 : CubesDemoBase {
        protected override SimpleCube2 CreateCube() {
            ambient = 1f;
            return new SimpleCube2(100, true, false, false);
        }
    }

    abstract class CubesDemoBase : DemoBase {
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
        AttributeFloat _aColor;
        UniformFloat _uAmbientLight;
        UniformFloat _uShadeLight;
        Sampler _tTexture;
        static float iq2 = (float)(1 / Math.Sqrt(2));
        static float iq3 = (float)(1 / Math.Sqrt(3));
        SimpleCube2 cube;
        protected float ambient = 0.5f;
        float aspect;
        float vpheight = 100;
        float[] matProjection;
        Size viewportSize;
        Point mousePosition;
        public CubesDemoBase() {
            cube = CreateCube();
        }
        protected abstract SimpleCube2 CreateCube();
        public override void Prepare(IGL gl) {
            renderer = new Renderer(gl);
            BuildShaders(gl);
            LoadTextures(gl);
            SetProjection();
        }
        public override void SetViewportSize(Size size) {
            base.SetViewportSize(size);
            this.viewportSize = size;
            aspect = (float)size.Width / size.Height;
            SetProjection();
        }
        private void SetProjection() {
            matProjection = GLMath.Frustum(-vpheight * aspect, vpheight * aspect, vpheight, -vpheight, 100, 100000);
        }
        public override void OnMouseMove(Point point, bool leftButtonPressed, bool rightButtonPressed) {
            base.OnMouseMove(point, leftButtonPressed, rightButtonPressed);
            mousePosition = point;
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
attribute vec4 aColor;
varying vec2 vTexCoord;
varying float vLightDot;
varying vec3 vColor;
void main(void)
{
	vec3 position = uViewAngles * (uAngles * aPoint.xyz + uOrigin - uViewOrigin);
    gl_Position = uPerspective * vec4(position.xyz, 1.0);
	vTexCoord = aTexCoord;
    vLightDot = dot(uViewAngles * (uAngles * aLightNormal), uLightVec);
    vColor = aColor.rgb;
}
";
            var fshaderSource = @"
precision mediump float;
uniform float uAmbientLight;
uniform float uShadeLight;
uniform sampler2D tTexture;
varying vec2 vTexCoord;
varying float vLightDot;
varying vec3 vColor;
void main(void)
{
	vec4 texture = texture2D(tTexture, vTexCoord);
    float lightness = mix(1.0, vLightDot * uShadeLight + uAmbientLight, texture.a);
  if(uAmbientLight==1.0){
	gl_FragColor = vec4(vColor, 1.0);
  }else{
	gl_FragColor = vec4(texture.rgb * lightness, 1.0);
  }
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
            program.Attributes.Add(_aColor = new AttributeFloat("aColor", 4));
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

            gl.Viewport(0, 0, viewportSize.Width, viewportSize.Height);

            _uPerspective.Set(matProjection);
            _tTexture.Set(0);
            _uAmbientLight.Set(ambient);
            _uShadeLight.Set(0.5f);
            _uLightVec.Set(iq3, -iq3, iq3);
            //_uViewOrigin.Set(0, 0, 500f);
            _uViewOrigin.Set((viewportSize.Width / 2 - mousePosition.X), (viewportSize.Height / 2 - mousePosition.Y), 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uViewAngles.Set(angles);

            SetVertexAttribArray(_aPoint, cube.VertexArray);
            if(cube.TexCoordArray != null) {
                SetVertexAttribArray(_aTexCoord, cube.TexCoordArray);
            }
            if(cube.NormalArray != null) {
                SetVertexAttribArray(_aLightNormal, cube.NormalArray);
            }
            if(cube.ColorArray != null) {
                SetVertexAttribArray(_aColor, cube.ColorArray);
            }

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
        }
        private void SetVertexAttribArray(AttributeFloat attrib, VertexArrayBase array) {
            attrib.Set(array.Pointer, array.Width, array.Stride, array.Normalized, typeof(byte).IsAssignableFrom(array.ElementType) ? GL.UNSIGNED_BYTE : GL.FLOAT);
        }
        public override void Dispose() {
            base.Dispose();
            cube.Dispose();
        }
    }
}
