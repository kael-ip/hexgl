using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    public class Renderer : IDisposable {

        class GLShader {
            public readonly uint id;
            public string source;
            public GLShader(uint id) {
                this.id = id;
            }
        }

        class GLUniform {
            public readonly uint location;
            public GLUniform(uint location) {
                this.location = location;
            }
        }

        class GLAttribute {
            public readonly uint location;
            public GLAttribute(uint location) {
                this.location = location;
            }
        }

        class GLProgram {
            public readonly uint id;
            public VertexShader vshader;
            public FragmentShader fshader;
            public Dictionary<Uniform, GLUniform> uniforms = new Dictionary<Uniform,GLUniform>();
            public Dictionary<Attribute, GLAttribute> attributes = new Dictionary<Attribute, GLAttribute>();
            public GLProgram(uint id) {
                this.id = id;
            }
        }

        private IGL gl;
        private List<Shader> shaders = new List<Shader>();
        private List<Program> programs = new List<Program>();
        private Dictionary<Shader, GLShader> glShaders = new Dictionary<Shader, GLShader>();
        private Dictionary<Program, GLProgram> glPrograms = new Dictionary<Program, GLProgram>();
        public bool ThrowOnMissingVariable = false;
        public Renderer(IGL gl) {
            this.gl = gl;
        }
        public IList<Shader> Shaders { get { return shaders; } }
        public IList<Program> Programs { get { return programs; } }
        public void BuildAll() {
            foreach(var shader in shaders) {
                Build(shader);
            }
            foreach(var program in programs) {
                Build(program);
            }
            // TODO: Free unused objects
        }
        private GLShader Build(Shader shader) {
            GLShader obj;
            if(!glShaders.TryGetValue(shader, out obj)) {
                uint id = gl.CreateShader(shader.GLType);
                obj = new GLShader(id);
                glShaders.Add(shader, obj);
            }
            if(obj.source != shader.Source) {
                obj.source = shader.Source;
                IntPtr ptr = Marshal.StringToHGlobalAnsi(obj.source);
                IntPtr ptr2 = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(ptr2, ptr);
                gl.ShaderSource(obj.id, 1, ptr2, IntPtr.Zero);
                gl.CompileShader(obj.id);
                Marshal.FreeHGlobal(ptr2);
                Marshal.FreeHGlobal(ptr);
            }
            return obj;
        }
        private GLProgram Build(Program program) {
            GLProgram obj;
            if(!glPrograms.TryGetValue(program, out obj)) {
                uint id = gl.CreateProgram();
                obj = new GLProgram(id);
                glPrograms.Add(program, obj);
            }
            if(obj.vshader != program.VertexShader) {
                if(obj.vshader != null) {
                    GLShader glShader;
                    if(glShaders.TryGetValue(obj.vshader, out glShader)) {
                        gl.DetachShader(obj.id, glShader.id);
                    }
                    obj.vshader = null;
                }
                obj.vshader = program.VertexShader;
                if(obj.vshader != null) {
                    GLShader glShader = Build(obj.vshader);
                    gl.AttachShader(obj.id, glShader.id);
                }
                program.IsDirty = true;
            }
            // TODO: Refactor similar code
            if(obj.fshader != program.FragmentShader) {
                if(obj.fshader != null) {
                    GLShader glShader;
                    if(glShaders.TryGetValue(obj.fshader, out glShader)) {
                        gl.DetachShader(obj.id, glShader.id);
                    }
                    obj.fshader = null;
                }
                obj.fshader = program.FragmentShader;
                if(obj.fshader != null) {
                    GLShader glShader = Build(obj.fshader);
                    gl.AttachShader(obj.id, glShader.id);
                }
                program.IsDirty = true;
            }
            // TODO: On any change in the shader, mark it as dirty.
            if(program.IsDirty) {
                obj.uniforms.Clear();
                obj.attributes.Clear();
                gl.LinkProgram(obj.id);
                foreach(var uniform in program.Uniforms) {
                    int location = gl.GetUniformLocation(obj.id, uniform.Name);
                    if(location < 0) {
                        if(ThrowOnMissingVariable)
                            throw new GLException("Invalid uniform name");
                    } else {
                        GLUniform glUniform = new GLUniform((uint)location);
                        obj.uniforms.Add(uniform, glUniform);
                    }
                }
                foreach(var attribute in program.Attributes) {
                    int location = gl.GetAttribLocation(obj.id, attribute.Name);
                    if(location < 0) {
                        if(ThrowOnMissingVariable)
                            throw new GLException("Invalid attribute name");
                    } else {
                        GLAttribute glAttribute = new GLAttribute((uint)location);
                        obj.attributes.Add(attribute, glAttribute);
                    }
                }
                program.IsDirty = false;
            }
            return obj; 
        }
        private void Setup(Program program) {
            GLProgram obj = Build(program);
            gl.UseProgram(obj.id);
            foreach(var uniform in program.Uniforms) {
                if(uniform.IsDirty) {
                    GLUniform glUniform;
                    if(obj.uniforms.TryGetValue(uniform, out glUniform)) {
                        uniform.Setup(gl, glUniform.location);
                    }
                }
            }
            foreach(var attribute in program.Attributes) {
                if(attribute.IsDirty) {
                    GLAttribute glAttribute;
                    if(obj.attributes.TryGetValue(attribute, out glAttribute)) {
                        attribute.Setup(gl, glAttribute.location);
                    }
                }
            }
        }
        public void DrawTriangles(Program program, int first, int count) {
            Setup(program);
            gl.DrawArrays(GL.TRIANGLES, first, count);
        }
        public void DrawTriangleFans(Program program, int first, int count) {
            Setup(program);
            gl.DrawArrays(GL.TRIANGLE_FAN, first, count);
        }
        public void DrawTriangleStrips(Program program, int first, int count) {
            Setup(program);
            gl.DrawArrays(GL.TRIANGLE_STRIP, first, count);
        }

        #region IDisposable Members

        public void Dispose() {
            throw new NotImplementedException();
        }

        #endregion
    }
    public abstract class Shader {
        public string Source { get; set; }
        protected Shader() { }
        internal abstract uint GLType { get; }
    }
    public class VertexShader : Shader {
        internal override uint GLType {
            get { return GL.VERTEX_SHADER; }
        }
    }
    public class FragmentShader : Shader {
        internal override uint GLType {
            get { return GL.FRAGMENT_SHADER; }
        }
    }
    public class Program {
        private List<Uniform> uniforms = new List<Uniform>();
        private List<Attribute> attributes = new List<Attribute>();
        public VertexShader VertexShader { get; set; }
        public FragmentShader FragmentShader { get; set; }
        public IList<Uniform> Uniforms { get { return uniforms; } }
        public IList<Attribute> Attributes { get { return attributes; } }
        public bool IsDirty { get; set; }
        public Program() {
            IsDirty = true;
        }
    }
}
