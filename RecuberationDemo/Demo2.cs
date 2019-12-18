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
        AttributeFloat _aVertexColor;
        UniformFloat _uAmbientLight;
        UniformFloat _uShadeLight;
        Sampler _tTexture;
        UniformMatrix _uObject;

        static float iq2 = (float)(1 / Math.Sqrt(2));
        static float iq3 = (float)(1 / Math.Sqrt(3));

        float hheight = 2.0f;
        float clipNear = 3.0f;
        float clipFar = 1000f;
        float aspect = 2.0f;
        float[] matProjection;
        Size viewportSize;
        Point mousePosition;

        Mesh earth;
        Mesh cube;
        List<RollingController> controllers;

        public Demo2() {
            Quad.ccwFront = false;
            controllers = new List<RollingController>();
            earth = CreateEarth(5);
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
        private Mesh CreateEarth(int cubeCount) {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildPlane(10, 10);
            var quads = quadMap.GetAllQuads();
            Trace.TraceInformation("Quads count = {0}", quads.Count);
            try {
                Trace.TraceInformation("Quad groups = {0}", quadMap.CheckConnectivity());
            } catch(Exception ex) {
                Trace.TraceError(ex.Message);
            }
            var rnd = new Random();
            for(int i = 0; i < cubeCount; i++) {
                var controller = new RollingController();
                Quad quad = null;
                while(quad == null || quad.IsOccupied) {
                    quad = quads[rnd.Next(quads.Count)];
                }
                controller.Setup(quad, rnd.Next(2) == 0 ? Axis.X : Axis.Y, rnd.Next(2) != 0, 256, i + 1);
                controllers.Add(controller);
            }
            Mesh mesh = new Mesh(4, quads.Count, true, false);
            int offset = 0;
            foreach(var quad in quads) {
                offset = quad.FillQuadVerts(mesh.VertexBuffer, mesh.NormalBuffer, offset);
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
            //matProjection = GLMath.Frustum(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar);
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
attribute vec3 aVertexColor;
varying vec2 vTexCoord;
varying float vLightDot;
varying vec3 vVertexColor;
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
varying vec2 vTexCoord;
varying float vLightDot;
varying vec3 vVertexColor;
void main(void)
{
	//vec4 texture = texture2D(tTexture, vTexCoord);
    vec4 texture = vec4(vVertexColor.rgb, 1.0);
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
            program.Attributes.Add(_aVertexColor = new AttributeFloat("aVertexColor", 3));
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

            //_uPerspective.Set(matProjection);
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
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(-5, -5, 0));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationZ((float)tRotation));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2 * 0.66)));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreateTranslation(0, 0, -10f));
                vmat = System.Numerics.Matrix4x4.Multiply(vmat, System.Numerics.Matrix4x4.CreatePerspectiveOffCenter(-hheight * aspect, hheight * aspect, -hheight, hheight, clipNear, clipFar));
                _uPerspective.Set(vmat.ToArray());
            }

            //GLMath.Rotate3(angles, tRotation, 0, 0, 1);
            _uOrigin.Set(0, 0, 0);
            //_uObject.Set(System.Numerics.Matrix4x4.Identity.ToArray());
            _aVertexColor.Set(1.0f, 1.0f, 1.0f);
            DrawMesh(earth);

            foreach(var controller in controllers) {
                controller.ReadLocation(_uOrigin, _uAngles);
                _aVertexColor.Set(1.0f, 0.4f, 0.4f);
                DrawMesh(cube);
                controller.Advance();
            }

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

    class RollingController {
        private Quad quad, next;
        private Axis dirAxis;
        private bool dirIsNegative;
        private Edge edge;
        private bool rIsNegative;
        private System.Numerics.Matrix4x4 omat, mat;
        private int frame, period, rq;
        private int color;
        public int Period { get; set; }
        public RollingController() {
            this.omat = this.mat = System.Numerics.Matrix4x4.Identity;
        }
        public void Setup(Quad quad, Axis dirAxis, bool dirIsNegative, int period, int color) {
            if(quad.IsOccupied) {
                throw new ArgumentException();
            }
            quad.IsOccupied = true;
            this.quad = quad;
            this.next = quad;
            this.dirAxis = dirAxis;
            this.dirIsNegative = dirIsNegative;
            this.Period = this.period = period;
            this.frame = 0;
            this.rq = 0;
            this.color = color;
        }
        public void Advance() {
            if(quad == null)
                return;
            if(frame >= rq * period) {
                frame = 0;
                if(Period <= 0) {
                    Period = 256;
                }
                period = Period;
                ReAnimate();
            } else {
                frame++;
            }
            UpdateLocation();
        }
        private void UpdateLocation() {
            System.Numerics.Matrix4x4 rmat = System.Numerics.Matrix4x4.Identity;
            System.Numerics.Vector3 rvec = System.Numerics.Vector3.Zero;
            float angle = (float)(frame * Math.PI / 2 / period);
            if(rIsNegative) {
                angle = -angle;
            }
            if(edge.Axis == Axis.X) {
                rvec = rIsNegative ? new System.Numerics.Vector3(0, 1, 0) : new System.Numerics.Vector3(0, 0, 0);
                rmat = System.Numerics.Matrix4x4.CreateRotationX(angle, rvec);
            } else if(edge.Axis == Axis.Y) {
                rvec = rIsNegative ? new System.Numerics.Vector3(0, 0, 0) : new System.Numerics.Vector3(1, 0, 0);
                rmat = System.Numerics.Matrix4x4.CreateRotationY(angle, rvec);
            } else {
                rmat = System.Numerics.Matrix4x4.CreateRotationZ(angle, rvec);
            }
            mat = System.Numerics.Matrix4x4.Multiply(omat, rmat);
            mat = System.Numerics.Matrix4x4.Multiply(mat, 
                System.Numerics.Matrix4x4.CreateTranslation(quad.Location.X, quad.Location.Y, quad.Location.Z));
        }
        public void ReadLocation(UniformFloat position, UniformMatrix rotation) {
            position.Set(mat.M41, mat.M42, mat.M43);
            rotation.Set(mat.GetRotationMatrixAsArray());
        }
        private void ReAnimate() {
            //TODO: mark quad occupation and check if it is not occupied
            quad.IsOccupied = false;
            quad = next;
            quad.IsOccupied = true;
            next = null;
            edge = null;
            int attempt = 0;
            while(next == null || next.IsOccupied) {
                if(attempt > 0) {
                    if(attempt % 2 == 1) {
                        dirAxis = (Axis)(((int)dirAxis + 1) % 3);
                    } else {
                        dirIsNegative = !dirIsNegative;
                    }
                }
                edge = quad.GetEdge(dirAxis, dirIsNegative);
                if(edge != null) {
                    if(edge.Q0 == quad) {
                        next = edge.Q1;
                        rIsNegative = false;
                    } else {
                        next = edge.Q0;
                        rIsNegative = true;
                    }
                    rq = edge.Angle - 1;
                }
                attempt++;
            }
            next.IsOccupied = true;
            next.Color = color;
            //TODO: occupy quads based on volume cells and consider self-occupation for 0-moves
            System.Diagnostics.Trace.TraceInformation("Direction: {1}{0}", dirAxis, dirIsNegative ? "-" : "+");
            //if actual angle == 0, repeat, limit retries
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
