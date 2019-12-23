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
    public abstract class Variable {
        public readonly string Name;
        public Variable(string name) {
            this.Name = name;
        }
        internal abstract void Setup(IGL gl, uint location);
        public bool IsDirty { get; protected set; }
    }
    public abstract class Uniform : Variable {
        public Uniform(string name) : base(name) { }
    }
    public class UniformFloat : Uniform {
        static int[] validWidths = new int[] { 1, 2, 3, 4 };
        private int width;
        private int length;
        private float[] values;
        public UniformFloat(string name, int width, int length = 1)
            : base(name) {
            if(Array.IndexOf(validWidths, width) < 0)
                throw new ArgumentOutOfRangeException("width");
            if(length < 1)
                throw new ArgumentOutOfRangeException("length");
            this.width = width;
            this.length = length;
            this.values = new float[width * length];
        }
        public float this[int index] {
            get { return values[index]; }
            set { values[index] = value; IsDirty = true; }
        }
        public void Set(float v0) {
            this.values[0] = v0;
            IsDirty = true;
        }
        public void Set(float v0, float v1) {
            this.values[0] = v0;
            this.values[1] = v1;
            IsDirty = true;
        }
        public void Set(float v0, float v1, float v2) {
            this.values[0] = v0;
            this.values[1] = v1;
            this.values[2] = v2;
            IsDirty = true;
        }
        public void Set(float v0, float v1, float v2, float v3) {
            this.values[0] = v0;
            this.values[1] = v1;
            this.values[2] = v2;
            this.values[3] = v3;
            IsDirty = true;
        }
        public void Set(params float[] values) {
            Array.Copy(values, this.values, values.Length);
            IsDirty = true;
        }
        public void Set(float[] values, int offset, int length) {
            Array.Copy(values, offset, this.values, 0, length);
            IsDirty = true;
        }
        internal override void Setup(IGL gl, uint location) {
            if(length == 1) {
                if(width == 1) {
                    gl.Uniform1f(location, values[0]);
                } else if(width == 2) {
                    gl.Uniform2f(location, values[0], values[1]);
                } else if(width == 3) {
                    gl.Uniform3f(location, values[0], values[1], values[2]);
                } else if(width == 4) {
                    gl.Uniform4f(location, values[0], values[1], values[2], values[3]);
                }
            } else {
                if(width == 1) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform1fv(location, length, ptr);
                    });
                } else if(width == 2) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform2fv(location, length, ptr);
                    });
                } else if(width == 3) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform3fv(location, length, ptr);
                    });
                } else if(width == 4) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform4fv(location, length, ptr);
                    });
                }
            }
            IsDirty = false;
        }
    }
    public class UniformMatrix : Uniform {
        static int[] validWidths = new int[] { 2, 3, 4 };
        private int width;
        private int length;
        private float[] values;
        public UniformMatrix(string name, int width, int length = 1)
            : base(name) {
            if(Array.IndexOf(validWidths, width) < 0)
                throw new ArgumentOutOfRangeException("width");
            if(length < 1)
                throw new ArgumentOutOfRangeException("length");
            this.width = width;
            this.length = length;
            this.values = new float[width * width * length];
        }
        public float this[int index] {
            get { return values[index]; }
            set { values[index] = value; IsDirty = true; }
        }
        public void Set(params float[] values) {
            Array.Copy(values, this.values, values.Length);
            IsDirty = true;
        }
        public void Set(float[] values, int offset, int length) {
            Array.Copy(values, offset, this.values, 0, length);
            IsDirty = true;
        }
        internal override void Setup(IGL gl, uint location) {
            if(width == 2) {
                Helper.WithPinned(values, ptr => {
                    gl.UniformMatrix2fv(location, length, false, ptr);
                });
            } else if(width == 3) {
                Helper.WithPinned(values, ptr => {
                    gl.UniformMatrix3fv(location, length, false, ptr);
                });
            } else if(width == 4) {
                Helper.WithPinned(values, ptr => {
                    gl.UniformMatrix4fv(location, length, false, ptr);
                });
            }
            IsDirty = false;
        }
    }
    public abstract class Attribute : Variable {
        public Attribute(string name) : base(name) { }
    }
    public class AttributeFloat : Attribute {
        static int[] validWidths = new int[] { 1, 2, 3, 4 };
        private int width;
        private int stride;// byte offset between elements if not zero
        private IntPtr ptr;
        private float[] values;
        private bool isArray = false;
        public AttributeFloat(string name, int width) : base(name) {
            if(Array.IndexOf(validWidths, width) < 0)
                throw new ArgumentOutOfRangeException("width");
            this.width = width;
            this.values = new float[4];
        }
        public void Set(params float[] values) {
            Array.Copy(values, this.values, values.Length);
            isArray = false;
            IsDirty = true;
        }
        public void Set(IntPtr ptr, int width, int stride = 0) {
            this.ptr = ptr;
            this.width = width;
            this.stride = stride;
            isArray = true;
            IsDirty = true;
        }
        internal override void Setup(IGL gl, uint location) {
            if(isArray) {
                gl.VertexAttribPointer(location, width, GL.FLOAT, false, stride, ptr);
                gl.EnableVertexAttribArray(location);
            } else {
                gl.DisableVertexAttribArray(location);
                if(width == 1) {
                    gl.VertexAttrib1f(location, values[0]);
                } else if(width == 2) {
                    gl.VertexAttrib2f(location, values[0], values[1]);
                } else if(width == 3) {
                    gl.VertexAttrib3f(location, values[0], values[1], values[2]);
                } else if(width == 4) {
                    gl.VertexAttrib4f(location, values[0], values[1], values[2], values[3]);
                }
            }
            IsDirty = false;
        }
    }
    public class Sampler : Uniform {
        private int textureUnit;
        public Sampler(string name) : base(name) { }
        public void Set(int unit) {
            this.textureUnit = unit;
            IsDirty = true;
        }
        internal override void Setup(IGL gl, uint location) {
            gl.Uniform1i(location, textureUnit);
            IsDirty = false;
        }
    }
    public abstract class Shader {
        public string Source { get; set; }
        protected Shader() { }
        public abstract uint GLType { get; }
    }
    public class VertexShader : Shader {
        public override uint GLType {
            get { return GL.VERTEX_SHADER; }
        }
    }
    public class FragmentShader : Shader {
        public override uint GLType {
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
