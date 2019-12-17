using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HexTex.OpenGL;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {

    class Demo2 : DemoBase {
        Renderer renderer;
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
        UniformMatrix _uObject;

        static float iq2 = (float)(1 / Math.Sqrt(2));
        static float iq3 = (float)(1 / Math.Sqrt(3));

        float vpheight = 2.0f;
        float aspect = 2.0f;
        float[] matProjection;
        Size viewportSize;
        Point mousePosition;

        Mesh earth;
        Mesh cube;
        System.Numerics.Matrix4x4 cmat = System.Numerics.Matrix4x4.Identity;
        float tween = 0, twoff = 0;

        public Demo2() {
            Quad.ccwFront = false;
            earth = CreateEarth();
            cube = CreateCube();
        }
        private Mesh CreateCube() {
            IBinaryVolume volume = new SphereVolume(0, 0, 0, 1);
            //IBinaryVolume volume = new SphereVolume(0, 0, 0, 9);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                ;//Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            Mesh mesh = new Mesh(4, quads.Count, true, false);
            int i = 0;
            foreach(var quad in quads) {
                i = quad.FillQuadVerts(mesh.VertexBuffer, mesh.NormalBuffer, i);
            }
            return mesh;
        }
        private Mesh CreateEarth() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildPlane(10, 10);
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            Mesh mesh = new Mesh(4, quads.Count, true, false);
            int i = 0;
            foreach(var quad in quads) {
                i = quad.FillQuadVerts(mesh.VertexBuffer, mesh.NormalBuffer, i);
            }
            return mesh;
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
varying vec2 vTexCoord;
varying float vLightDot;
void main(void)
{
	vec3 position = uViewAngles * (uAngles * aPoint.xyz + uOrigin - uViewOrigin);
    gl_Position = uPerspective * vec4(position.xyz, 1.0);
    //vec4 position4 = uObject * vec4(aPoint.xyz, 1.0);
    //gl_Position = uPerspective * position4;
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
            program.Uniforms.Add(_uObject = new UniformMatrix("uObject", 4));
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
            //_uViewOrigin.Set(0, 0, 500f);
            _uViewOrigin.Set(0, 0, 0);
            //_uViewOrigin.Set(mousePosition.X - viewportSize.Width / 2, mousePosition.Y - viewportSize.Height, 500f);
            float[] angles = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            _uAngles.Set(angles);
            _uViewAngles.Set(angles);

            var dt = DateTime.Now;
            double tRotation = Math.PI * 2 * ((0.001 * dt.Millisecond) + dt.Second) / 60;
            {
                System.Numerics.Matrix4x4 vmat = System.Numerics.Matrix4x4.Identity;
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.9)));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, -500f));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreatePerspectiveOffCenter(-vpheight * aspect, vpheight * aspect, -vpheight, vpheight, 100, 1000));
                _uPerspective.Set(vmat.ToArray());
            }

            //GLMath.Rotate3(angles, tRotation, 0, 0, 1);
            _uOrigin.Set(0, 0, 0);
            //_uObject.Set(System.Numerics.Matrix4x4.Identity.ToArray());
            DrawMesh(earth);
            
            {
                System.Numerics.Matrix4x4 mat = cmat;
                mat = System.Numerics.Matrix4x4.Multiply(mat, System.Numerics.Matrix4x4.CreateRotationY((float)(tween * Math.PI / 2), new System.Numerics.Vector3(1, 0, 0)));
                mat = System.Numerics.Matrix4x4.Multiply(mat, System.Numerics.Matrix4x4.CreateTranslation(twoff, 0, 0));
                //_uObject.Set(mat.ToArray());
                _uOrigin.Set(mat.M41, mat.M42, mat.M43);
                _uAngles.Set(mat.GetRotationMatrixAsArray());
                tween += 1f / 256;
                if(tween >= 1) {
                    tween -= 1;
                    twoff += 1f;
                    if(twoff >= 10) {
                        twoff -= 10;
                    }
                }
            }
            DrawMesh(cube);

            gl.Flush();
            gl.Finish();
        }

        private void DrawMesh(Mesh mesh) {
            var hVertex = GCHandle.Alloc(mesh.VertexBuffer, GCHandleType.Pinned);
            var hNormal = GCHandle.Alloc(mesh.NormalBuffer, GCHandleType.Pinned);

            _aPoint.Set(hVertex.AddrOfPinnedObject(), 3);
            _aTexCoord.Set(0, 0);
            _aLightNormal.Set(hNormal.AddrOfPinnedObject(), 3);

            for(int i = 0, j = 0; i < mesh.PrimitiveCount; i++, j += mesh.PrimitiveLength) {
                renderer.DrawTriangleFans(program, j, mesh.PrimitiveLength);
            }           

            if(hVertex.IsAllocated)
                hVertex.Free();
            if(hNormal.IsAllocated)
                hNormal.Free();
        }
    }

    public static class NumericsHelper {
        public static float[] ToArray(this System.Numerics.Matrix4x4 m) {
            var a = new float[16];
            a[0] = m.M11;
            a[1] = m.M12;
            a[2] = m.M13;
            a[3] = m.M14;
            a[4] = m.M21;
            a[5] = m.M22;
            a[6] = m.M23;
            a[7] = m.M24;
            a[8] = m.M31;
            a[9] = m.M32;
            a[10] = m.M33;
            a[11] = m.M34;
            a[12] = m.M41;
            a[13] = m.M42;
            a[14] = m.M43;
            a[15] = m.M44;
            return a;
        }
        public static float[] GetRotationMatrixAsArray(this System.Numerics.Matrix4x4 m) {
            var a = new float[9];
            a[0] = m.M11;
            a[1] = m.M12;
            a[2] = m.M13;
            a[3] = m.M21;
            a[4] = m.M22;
            a[5] = m.M23;
            a[6] = m.M31;
            a[7] = m.M32;
            a[8] = m.M33;
            return a;
        }
    }
}
