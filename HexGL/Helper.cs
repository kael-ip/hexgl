using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HexTex.OpenGL {

    public static class Helper {
        public static void WithPinned(object obj, Action<IntPtr> action) {
            GCHandle pinned = GCHandle.Alloc(obj, GCHandleType.Pinned);
            try {
                action(pinned.AddrOfPinnedObject());
            } finally {
                pinned.Free();
            }
        }
    }
}
