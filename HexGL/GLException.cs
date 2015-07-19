using System;
using System.Collections.Generic;
using System.Text;

namespace HexTex.OpenGL {

    public class GLException : Exception {
        public readonly uint Code;
        public GLException(uint code)
            : base() {
            this.Code = code;
        }
        public GLException(string message) : base(message) { }
        public override string Message {
            get {
                switch (Code) {
                    case GL.NO_ERROR: return base.Message;
                    case GL.INVALID_ENUM: return "Invalid Enum";
                    case GL.INVALID_VALUE: return "Invalid value";
                    case GL.INVALID_OPERATION: return "Invalid operation";
                    case GL.OUT_OF_MEMORY: return "Out of memory";
                    default: return string.Format("Unknown error 0x{0:x}", Code);
                }
            }
        }
    }
}
