﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    public abstract class VertexArrayBase : IDisposable {
        private int length;
        private int width;
        public int Length {
            get {
                return length;
            }
        }
        public int Width {
            get {
                return width;
            }
        }
        public virtual bool Normalized {
            get {
                return false;
            }
        }
        public VertexArrayBase(int length, int width) {
            if(width < 1 || width > 4)
                throw new ArgumentOutOfRangeException(nameof(width));
            if(length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            this.length = length;
            this.width = width;
        }
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
        private bool normalized;
        public VertexArray(T[] data, int width, bool normalized)
            : base(data.Length / width, width) {
            if(data.Length % width != 0)
                throw new ArgumentException(nameof(width));
            this.data = data;
            this.handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            this.normalized = normalized;
        }
        public T[] Data { get { return data; } }
        public override bool Normalized {
            get {
                return normalized;
            }
        }
        private void GuardLength(int index) {
            if(index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
        public void SetVertex(int index, T x, T y = default(T), T z = default(T), T w = default(T)) {
            GuardLength(index);
            int i = index * Width;
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
        public override Type ElementType {
            get {
                return typeof(T);
            }
        }
        public override IntPtr Pointer {
            get {
                if(!handle.IsAllocated)
                    return IntPtr.Zero;
                return handle.AddrOfPinnedObject();
            }
        }
        public override int Stride {
            get { return 0; }
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
