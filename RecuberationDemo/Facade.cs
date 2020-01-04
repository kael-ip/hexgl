using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    class Facade {
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
        protected Sampler _tLightTable;

        protected float hheight = 2.0f;
        //protected float clipNear = 3.0f;
        //protected float clipFar = 1000f;
        protected float aspect = 2.0f;
        private int viewportWidth;
        private int viewportHeight;

        public void Init(IGL gl) {
            renderer = new Renderer(gl);
            BuildShaders(gl);
            LoadTextures(gl);
        }
        public void SetViewport(int viewportWidth, int viewportHeight) {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            aspect = (float)viewportWidth / viewportHeight;
        }
        protected virtual void BuildShaders(IGL gl) {
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
attribute float aVertexColor;
varying vec2 vTexCoord;
varying float vLightDot;
varying float vVertexColor;
void main(void)
{
	//vec3 position = uViewAngles * (uAngles * aPoint.xyz + uOrigin - uViewOrigin);
    vec3 position = uViewAngles * (uAngles * aPoint.xyz + uOrigin) - uViewOrigin;
    gl_Position = uPerspective * vec4(position.xyz, 1.0);
	vTexCoord = aTexCoord;
    //vLightDot = dot(uViewAngles * (uAngles * aLightNormal), uLightVec);
    vLightDot = dot((uAngles * aLightNormal), uLightVec);
    vVertexColor = aVertexColor;
}
";
            var fshaderSource = @"
precision mediump float;
uniform float uAmbientLight;
uniform float uShadeLight;
uniform sampler2D tTexture;
uniform sampler2D tPalette;
uniform sampler2D tLightTable;
varying vec2 vTexCoord;
varying float vLightDot;
varying float vVertexColor;
void main(void)
{
	vec4 texture = texture2D(tTexture, vTexCoord);
    float lightness = mix(0, vLightDot * uShadeLight + uAmbientLight, texture.a);
    vec2 index = vec2(vVertexColor, clamp(lightness, 0, 0.999));
    gl_FragColor = texture2D(tPalette, vec2(0.5, texture2D(tLightTable, index).a));
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
            program.Uniforms.Add(_tPalette = new Sampler("tPalette"));
            program.Uniforms.Add(_tLightTable = new Sampler("tLightTable"));
            program.Attributes.Add(_aPoint = new AttributeFloat("aPoint", 3));
            program.Attributes.Add(_aLightNormal = new AttributeFloat("aLightNormal", 3));
            program.Attributes.Add(_aTexCoord = new AttributeFloat("aTexCoord", 2));
            program.Attributes.Add(_aVertexColor = new AttributeFloat("aVertexColor", 1));
            renderer.BuildAll();
        }
        protected virtual void LoadTextures(IGL gl) {
            //var p = Palette.CreateGradient(0xffffff);
            //var p = Palette.Create16x16();
            var p = Palette.CreateSmart();
            uint[] palette = p.GetPalette();
            byte[] lightTable = p.GetLightTable();
            uint[] textures = new uint[3];
            gl.GenTextures(3, textures);
            uint[] data = new uint[] { 0xffffffff };
            LoadTexture(gl, textures[0], 0, 0, 0, data);
            LoadTexture(gl, textures[1], 1, 0, 8, palette);
            LoadTextureL(gl, textures[2], 2, 8, 8, lightTable);
        }
        protected void LoadTexture(IGL gl, uint id, uint unit, int pw, int ph, Array data) {
            gl.ActiveTexture(GL.TEXTURE0 + unit);
            gl.BindTexture(GL.TEXTURE_2D, id);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP_TO_EDGE);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP_TO_EDGE);
            Helper.WithPinned(data, ptr => {
                gl.TexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, 1 << pw, 1 << ph, 0, GL.RGBA, GL.UNSIGNED_BYTE, ptr);
            });
        }
        private void LoadTextureL(IGL gl, uint id, uint unit, int pw, int ph, Array data) {
            gl.ActiveTexture(GL.TEXTURE0 + unit);
            gl.BindTexture(GL.TEXTURE_2D, id);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP_TO_EDGE);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP_TO_EDGE);
            Helper.WithPinned(data, ptr => {
                gl.TexImage2D(GL.TEXTURE_2D, 0, GL.ALPHA, 1 << pw, 1 << ph, 0, GL.ALPHA, GL.UNSIGNED_BYTE, ptr);
            });
        }
        public void Redraw(IGL gl, Action<Facade> painter) {
            gl.ClearColor(0, 0, 0, 0);
            gl.ClearDepthf(float.MaxValue);
            gl.Clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT | GL.STENCIL_BUFFER_BIT);
            //
            gl.Enable(GL.CULL_FACE);
            gl.FrontFace(GL.CCW);
            gl.CullFace(GL.BACK);
            gl.Enable(GL.DEPTH_TEST);
            gl.DepthFunc(GL.LESS);

            gl.Viewport(0, 0, viewportWidth, viewportHeight);

            _tTexture.Set(0);
            _tPalette.Set(1);
            _tLightTable.Set(2);
            _uAmbientLight.Set(0.01f);
            _uShadeLight.Set(1.0f);
            _aVertexColor.Set(1f);

            if(painter != null) {
                painter(this);
            }

            gl.Flush();
            gl.Finish();
        }

        //
        // Facade API
        //

        public void SetColorIndex(int c) {
            _aVertexColor.Set((c & 255) / 256f + 1 / 512f);
        }
        public void SetObjMatrix(float[] mat) { //3x4
            _uAngles.Set(mat, 0, 9);
            _uOrigin.Set(mat, 9, 3);
        }
        public void SetCamMatrix(float[] mat) { //3x4
            _uViewAngles.Set(mat, 0, 9);
            _uViewOrigin.Set(mat, 9, 3);
        }
        public void SetLightVector(float[] vec) {
            _uLightVec.Set(vec);
        }
        public void SetProjection(float clipNear, float clipFar) {
            var projection = GLMath.Frustum(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar);
            _uPerspective.Set(projection);
        }
        public void DrawMesh(Mesh mesh, bool colored) {
            var hVertex = GCHandle.Alloc(mesh.VertexBuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(mesh.NormalBuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            _aTexCoord.Set(0, 0);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);

            for(int i = 0, j = 0; i < mesh.PrimitiveCount; i++, j += mesh.PrimitiveLength) {
                if(colored) {
                    SetColorIndex(((QMesh)mesh).Quads[i].Color);
                }
                renderer.DrawTriangleFans(program, j, mesh.PrimitiveLength);
            }

            if(hVertex.IsAllocated)
                hVertex.Free();
            if(hNormal.IsAllocated)
                hNormal.Free();
        }
    }

    abstract class FacadeDemoBase : DemoBase {
        private Facade facade;
        public FacadeDemoBase() {
            facade = new Facade();
        }
        public override void Prepare(IGL gl) {
            facade.Init(gl);
        }
        public override void Redraw(IGL gl) {
            facade.Redraw(gl, OnPaint);
        }
        public override void SetViewportSize(System.Drawing.Size size) {
            base.SetViewportSize(size);
            facade.SetViewport(size.Width, size.Height);
        }
        protected abstract void OnPaint(Facade g);
    }
}
