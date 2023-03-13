using System;
using System.Collections.Generic;
using System.Text;

namespace HexTex.OpenGL {
    public abstract class Variable {
        public readonly string Name;
        public Variable(string name) {
            this.Name = name;
        }
        internal abstract void Setup(IGL gl, uint location);
        public bool IsDirty {
            get; protected set;
        }
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
            get {
                return values[index];
            }
            set {
                values[index] = value;
                IsDirty = true;
            }
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
                }
                else if(width == 2) {
                    gl.Uniform2f(location, values[0], values[1]);
                }
                else if(width == 3) {
                    gl.Uniform3f(location, values[0], values[1], values[2]);
                }
                else if(width == 4) {
                    gl.Uniform4f(location, values[0], values[1], values[2], values[3]);
                }
            }
            else {
                if(width == 1) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform1fv(location, length, ptr);
                    });
                }
                else if(width == 2) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform2fv(location, length, ptr);
                    });
                }
                else if(width == 3) {
                    Helper.WithPinned(values, ptr => {
                        gl.Uniform3fv(location, length, ptr);
                    });
                }
                else if(width == 4) {
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
            get {
                return values[index];
            }
            set {
                values[index] = value;
                IsDirty = true;
            }
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
            }
            else if(width == 3) {
                Helper.WithPinned(values, ptr => {
                    gl.UniformMatrix3fv(location, length, false, ptr);
                });
            }
            else if(width == 4) {
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
            }
            else {
                gl.DisableVertexAttribArray(location);
                if(width == 1) {
                    gl.VertexAttrib1f(location, values[0]);
                }
                else if(width == 2) {
                    gl.VertexAttrib2f(location, values[0], values[1]);
                }
                else if(width == 3) {
                    gl.VertexAttrib3f(location, values[0], values[1], values[2]);
                }
                else if(width == 4) {
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
}
