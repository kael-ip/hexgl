using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    public abstract class VertexArrayBase : IDisposable {
        public abstract int Length { get; }
        public abstract int Width { get; }
        public virtual bool Normalized { get { return false; } }
        public VertexArrayBase() { }
        public abstract Type ElementType { get; }
        public abstract IntPtr Pointer { get; }
        public abstract int Stride { get; }

        #region IDisposable Support
        protected virtual void DisposeUnmanaged() {
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
        }
        private bool isDisposed = false;
        protected virtual void Dispose(bool disposing) {
            if(!isDisposed) {
                if(disposing) {
                    // TODO: dispose managed state (managed objects).
                }
                DisposeUnmanaged();
                isDisposed = true;
            }
        }
        ~VertexArrayBase() {
            Dispose(false);
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class VertexArray<T> : VertexArrayBase where T : struct {
        private T[] data;
        private GCHandle handle;
        private int length;
        private int width;
        private bool normalized;
        private int indexOffset;
        private int indexStride;
        private int byteOffset;
        private int byteStride;
        public VertexArray(T[] data, int width, bool normalized, int stride = 0, int offset = 0) {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            if(width < 1 || width > 4)
                throw new ArgumentOutOfRangeException(nameof(width));
            if(stride < 0 || (stride > 0 && stride < width))
                throw new ArgumentOutOfRangeException(nameof(stride));
            indexStride = stride == 0 ? width : stride;
            if(offset < 0 || offset + width > indexStride)
                throw new ArgumentOutOfRangeException(nameof(offset));
            indexOffset = offset;
            this.width = width;
            if(data.Length % indexStride != 0)
                throw new ArgumentException(nameof(stride));
            this.length = data.Length / indexStride;
            this.data = data;
            this.handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            this.normalized = normalized;
            int sizeofT = Marshal.SizeOf(typeof(T));
            byteStride = (indexStride == width) ? 0 : (indexStride * sizeofT);
            byteOffset = indexOffset * sizeofT;
        }
        public T[] Data { get { return data; } }
        public override int Length { get { return length; } }
        public override int Width { get { return width; } }
        public int IndexStride { get { return indexStride; } }
        public int IndexOffset { get { return indexOffset; } }
        public override bool Normalized { get { return normalized; } }
        private void GuardLength(int index) {
            if(index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
        public void SetVertex(int index, T x, T y = default(T), T z = default(T), T w = default(T)) {
            GuardLength(index);
            int i = index * indexStride + indexOffset;
            data[i] = x;
            if(Width < 2)
                return;
            data[++i] = y;
            if(Width < 3)
                return;
            data[++i] = z;
            if(Width < 4)
                return;
            data[++i] = w;
        }
        public override Type ElementType { get { return typeof(T); } }
        public override IntPtr Pointer {
            get {
                if(!handle.IsAllocated)
                    return IntPtr.Zero;
                return new IntPtr(handle.AddrOfPinnedObject().ToInt64() + byteOffset);
            }
        }
        public override int Stride {
            get { return byteStride; }
        }
        protected override void DisposeUnmanaged() {
            base.DisposeUnmanaged();
            if(handle.IsAllocated) {
                handle.Free();
            }
            data = null;
        }
    }
    public class SimpleVertexArray<T> : VertexArray<T> where T : struct {
        public SimpleVertexArray(int length, int width, bool normalized)
            : base(new T[length * width], width, normalized) {
        }
    }
}
