using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {

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

        float vpheight = 2.0f;
        float aspect = 2.0f;
        float[] matProjection;
        Size viewportSize;
        Point mousePosition;

        IBinaryVolume volume;
        QuadMap quadMap;
        float[] vbuffer;
        float[] nbuffer;

        public Demo1() {
            Quad.ccwFront = false;
            //volume = new RandomHeightPlane(12, 12, 9, 1);
            volume = new SphereVolume(5, 3, 7, 9);
            //volume = new SphereVolume(5, 3, 7, 1);
            //volume = new SphereVolume(0, 0, 0, 1);
            quadMap = new QuadMap();
            quadMap.Build(volume);
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            vbuffer = new float[quads.Count * 4 * 3];
            nbuffer = new float[quads.Count * 4 * 3];
            int i = 0;
            foreach(var quad in quads) {
                i = quad.FillQuadVerts(vbuffer, nbuffer, i);
            }
        }

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
            matProjection = GLMath.Frustum(-vpheight * aspect, vpheight * aspect, -vpheight, vpheight, 100, 1000);
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
	//vec4 texture = texture2D(tTexture, vTexCoord);
    vec4 texture = texture2D(tTexture, vTexCoord)*0.00001 + vec4(1.0, 1.0, 1.0, 1.0);
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
            _uAmbientLight.Set(0.2f);
            _uShadeLight.Set(0.5f);
            //_uLightVec.Set(iq3, -iq3, iq3);
            _uLightVec.Set(0, 0, 1);
            _uViewOrigin.Set(0, 0, 500f);
            //_uViewOrigin.Set(mousePosition.X - viewportSize.Width / 2, mousePosition.Y - viewportSize.Height, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uViewAngles.Set(angles);

            var hVertex = GCHandle.Alloc(vbuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(nbuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            _aTexCoord.Set(0, 0);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);

            var dt = DateTime.Now;
            double tRotation = Math.PI * 2 * ((0.001 * dt.Millisecond) + dt.Second) / 60;
            GLMath.Rotate3(angles, tRotation, 0, iq2, iq2);
            _uAngles.Set(angles);

            _uOrigin.Set(-5, -3, -7);
            //_uOrigin.Set(0, 0, 0);

            for(int i = 0; i < vbuffer.Length / 3; i += 4) {
                renderer.DrawTriangleFans(program, i, 4);
            }

            gl.Flush();
            gl.Finish();

            if(hVertex.IsAllocated) hVertex.Free();
            if(hNormal.IsAllocated) hNormal.Free();
        }

    }
}
