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
                throw new ArgumentOutOfRangeException("width");
            if(length < 0)
                throw new ArgumentOutOfRangeException("length");
            this.length = length;
            this.width = width;
        }
        public abstract Type ElementType {
            get;
        }
        internal abstract IntPtr Pointer {
            get;
        }

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
        public override bool Normalized {
            get {
                return normalized;
            }
        }
        public VertexArray(int length, int width, bool normalized)
            : base(length, width) {
            this.data = new T[length * width];
            this.handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            this.normalized = normalized;
        }
        private void Check(int index, bool s) {
            if(index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException("index");
            if(!s)
                throw new InvalidOperationException();
        }
        public void SetVertex(int index, T x, T y, T z, T w) {
            Check(index, Width == 4);
            int i = index * Width;
            data[i++] = x;
            data[i++] = y;
            data[i++] = z;
            data[i] = w;
        }
        public void SetVertex(int index, T x, T y, T z) {
            Check(index, Width == 3);
            int i = index * Width;
            data[i++] = x;
            data[i++] = y;
            data[i] = z;
        }
        public void SetVertex(int index, T x, T y) {
            Check(index, Width == 2);
            int i = index * Width;
            data[i++] = x;
            data[i] = y;
        }
        public void SetVertex(int index, T x) {
            Check(index, Width == 1);
            int i = index * Width;
            data[i] = x;
        }
        public override Type ElementType {
            get {
                return typeof(T);
            }
        }
        internal override IntPtr Pointer {
            get {
                if(!handle.IsAllocated)
                    return IntPtr.Zero;
                return handle.AddrOfPinnedObject();
            }
        }
        protected override void DisposeUnmanaged() {
            base.DisposeUnmanaged();
            if(handle.IsAllocated) {
                handle.Free();
            }
            data = null;
        }
    }
}
