using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    abstract class SimpleDemoBase : DemoBase {
        protected Renderer renderer;
        protected Program program;
        protected UniformFloat _uOrigin;
        protected UniformMatrix _uAngles;
        protected UniformFloat _uViewOrigin;
        protected UniformMatrix _uViewAngles;
        protected UniformMatrix _uPerspective;
        protected UniformFloat _uLightVec;
        protected AttributeFloat _aPoint;
        protected AttributeFloat _aLightNormal;
        protected AttributeFloat _aTexCoord;
        protected AttributeFloat _aVertexColor;
        protected UniformFloat _uAmbientLight;
        protected UniformFloat _uShadeLight;
        protected Sampler _tTexture;
        protected Sampler _tPalette;
        protected UniformMatrix _uObject;

        protected static float iq2 = (float)(1 / Math.Sqrt(2));
        protected static float iq3 = (float)(1 / Math.Sqrt(3));

        protected float hheight = 2.0f;
        protected float clipNear = 3.0f;
        protected float clipFar = 1000f;
        protected float aspect = 2.0f;
        protected float[] matProjection;
        protected Size viewportSize;
        protected Point mousePosition;

        private uint[] palette;

        public SimpleDemoBase() {
            Quad.ccwFront = false;
        }
        public override void Prepare(IGL gl) {
            renderer = new Renderer(gl);
            palette = CreatePalette();
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
        protected virtual void SetProjection() {
            matProjection = GLMath.Frustum(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar);
        }
        public override void OnMouseMove(Point point, bool leftButtonPressed, bool rightButtonPressed) {
            base.OnMouseMove(point, leftButtonPressed, rightButtonPressed);
            mousePosition = point;
        }
        protected virtual void BuildShaders(IGL gl) {
            var vshaderSource = @"
uniform mat4 uObject;
uniform vec3 uOrigin;
uniform mat3 uAngles;
uniform vec3 uViewOrigin;
uniform mat3 uViewAngles;
uniform mat4 uPerspective;
uniform vec3 uLightVec;
attribute vec3 aPoint;
attribute vec3 aLightNormal;
attribute vec2 aTexCoord;
attribute float aVertexColor;
varying vec2 vTexCoord;
varying float vLightDot;
varying float vVertexColor;
void main(void)
{
	vec3 position = uViewAngles * (uAngles * aPoint.xyz + uOrigin - uViewOrigin);
    gl_Position = uPerspective * vec4(position.xyz, 1.0);
    //vec4 position4 = uObject * vec4(aPoint.xyz, 1.0);
    //gl_Position = uPerspective * position4;
	vTexCoord = aTexCoord;
    vLightDot = dot(uViewAngles * (uAngles * aLightNormal), uLightVec);
    vVertexColor = aVertexColor;
}
";
            var fshaderSource = @"
precision mediump float;
uniform float uAmbientLight;
uniform float uShadeLight;
uniform sampler2D tTexture;
uniform sampler2D tPalette;
varying vec2 vTexCoord;
varying float vLightDot;
varying float vVertexColor;
void main(void)
{
	vec4 texture = texture2D(tTexture, vTexCoord);
    float lightness = mix(0, vLightDot * uShadeLight + uAmbientLight, texture.a);
    vec2 index = vec2(vVertexColor, clamp(lightness, 0, 1));
    gl_FragColor = texture2D(tPalette, index);
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
            program.Uniforms.Add(_uObject = new UniformMatrix("uObject", 4));
            program.Attributes.Add(_aPoint = new AttributeFloat("aPoint", 3));
            program.Attributes.Add(_aLightNormal = new AttributeFloat("aLightNormal", 3));
            program.Attributes.Add(_aTexCoord = new AttributeFloat("aTexCoord", 2));
            program.Attributes.Add(_aVertexColor = new AttributeFloat("aVertexColor", 1));
            program.Uniforms.Add(_tPalette = new Sampler("tPalette"));
            renderer.BuildAll();

            _aVertexColor.Set(1f);
        }
        protected virtual void LoadTextures(IGL gl) {
            uint[] textures = new uint[2];
            gl.GenTextures(2, textures);
            uint[] data = new uint[] { 0xffffffff };
            LoadTexture(gl, textures[0], 0, 0, 0, data);
            LoadTexture(gl, textures[1], 1, 0, 8, palette);
        }
        protected void LoadTexture(IGL gl, uint id, uint unit, int pw, int ph, Array data, bool genMipMap = false, bool repeat = false) {
            gl.ActiveTexture(GL.TEXTURE0 + unit);
            gl.BindTexture(GL.TEXTURE_2D, id);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            if(genMipMap) {
                gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR_MIPMAP_LINEAR);
            } else {
                gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            }
            if(repeat) {
                gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.REPEAT);
                gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.REPEAT);
            } else {
                gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP_TO_EDGE);
                gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP_TO_EDGE);
            }
            Helper.WithPinned(data, ptr => {
                gl.TexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, 1 << pw, 1 << ph, 0, GL.RGBA, GL.UNSIGNED_BYTE, ptr);
            });
            if(genMipMap) {
                gl.GenerateMipmap(GL.TEXTURE_2D);
            }
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

            _tTexture.Set(0);
            _tPalette.Set(1);
            RedrawCore(gl);

            gl.Flush();
            gl.Finish();
        }
        protected abstract void RedrawCore(IGL gl);
        protected virtual uint[] CreatePalette() {
            return CreatePalette256(0xffffffff);
        }
        private uint[] CreatePalette256(uint color) {
            var palette = new uint[256];
            for(var i = 0; i < 256; i++) {
                palette[i] = Scale(color, i, 255);
            }
            return palette;
        }
        private uint Scale(uint color, int n, int d) {
            var r = ((color >> 0) & 255) * n / d;
            var g = ((color >> 8) & 255) * n / d;
            var b = ((color >> 16) & 255) * n / d;
            return (uint)(0xff000000 | (b << 16) | (g << 8) | (r << 0));
        }
    }
}
