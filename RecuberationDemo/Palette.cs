using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    class Palette {
        int length;
        int shades;
        List<byte[]> palette;
        byte[] lightTable;
        private Palette(int length, int shades) {
            this.length = length;
            this.shades = shades;
            palette = new List<byte[]>();
        }
        private int AddColor(byte r, byte g, byte b) {
            palette.Add(new byte[] { r, g, b });
            return palette.Count - 1;
        }
        private int AddColor(uint color) {
            byte r, g, b;
            UnpackColor(color, out r, out g, out b);
            return AddColor(r, g, b);
        }
        public uint[] GetPalette() {
            uint[] data = new uint[length];
            for(int i = 0; i < length; i++) {
                if(i < palette.Count) {
                    var c = palette[i];
                    data[i] = PackColor(c[0], c[1], c[2]);
                } else {
                    data[i] = 0;
                }
            }
            return data;
        }
        public byte[] GetLightTable() {
            if(lightTable == null) {
                lightTable = CreateLightTable();
            }
            return lightTable;
        }
        public static uint PackColor(byte r, byte g, byte b) {
            return (uint)(0xff000000 | ((uint)b << 16) | ((uint)g << 8) | ((uint)r << 0));
        }
        public static void UnpackColor(uint color, byte[] rgb) {
            for(int i = 0; i < 3; i++) {
                rgb[i] = (byte)(color & 255);
                color = color >> 8;
            }
        }
        public static void UnpackColor(uint color, out byte r, out byte g, out byte b) {
            r = (byte)((color >> 0) & 255);
            g = (byte)((color >> 8) & 255);
            b = (byte)((color >> 16) & 255);
        }
        public static void UnpackColorScaled(uint color, out int r, out int g, out int b, int n, int d) {
            r = (((int)color >> 0) & 255) * n / d;
            g = (((int)color >> 8) & 255) * n / d;
            b = (((int)color >> 16) & 255) * n / d;
        }
        public void CreateGradient(uint color, int n) {
            if(this.length - palette.Count < n)
                throw new ArgumentOutOfRangeException();
            byte[] rgb = new byte[3];
            UnpackColor(color, rgb);
            for(int i = 0; i < n; i++) {
                byte[] rgbs = new byte[3];
                for(int j = 0; j < 3; j++) {
                    rgbs[j] = Scale(rgb[j], n - i - 1, n - 1);
                }
                palette.Add(rgbs);
            }
        }
        public static Palette CreateGradient(uint color) {
            var p = new Palette(256, 256);
            p.CreateGradient(color, 256);
            return p;
        }
        static uint[] colors = new uint[]{
            //0xffffff,

            //NES light gray
            0xBCBCBC,

            //NES row 1
            0x0078F8,
            0x0058F8,
            0x6844FC,
            0xD800CC,
            0xE40058,
            0xF83800,
            0xE45C10,
            //0xAC7C00,
            0x00B800,
            0x00A800,
            //0x00A844,
            //0x008888,

            //NES row 2
            //0x3CBCFC,
            //0x6888FC,
            //0x9878F8,
            //0xF878F8,
            0xF85898,
            //0xF87858,
            //0xFCA044,
            0xF8B800,
            0xB8F818,
            //0x58D854,
            0x58F898,
            0x00E8D8,

            0xD8D828 //extra

            //0xff0000,
            //0x00ff00,
            //0x0000ff,
            //0x00ffff,
            //0xff00ff,
            //0xffff00,

            //0x770000,
            //0x007700,
            //0x000077,
            //0x0077ff,
            //0x00ff77,
            //0x7700ff,
            //0xff0077,
            //0x77ff00,
            //0xff7700,
        };
        public static Palette Create16x16() {
            var p = new Palette(256, 256);
            byte[] rgb = new byte[3];
            for(var s = 0; s < 16; s++) {
                for(var j = 0; j < 16; j++) {
                    UnpackColor(colors[j], rgb);
                    byte[] rgbs = new byte[3];
                    for(int i = 0; i < 3; i++) {
                        rgbs[i] = Scale(rgb[i], 15 - s, 15);
                    }
                    p.palette.Add(rgbs);
                }
            }
            return p;
        }
        static byte Scale(byte c, int n, int d) {
            return (byte)(c * n / d);
        }
        static uint Scale(uint color, int n, int d) {
            int r, g, b;
            UnpackColorScaled(color, out r, out g, out b, n, d);
            return PackColor((byte)r, (byte)g, (byte)b);
        }
        byte[] CreateLightTable() {
            var tbl = new byte[length * shades];
            int[] color = new int[3];
            for(int c = 0; c < length; c++) {
                for(int s = 0; s < shades; s++) {
                    if(c < palette.Count) {
                        byte[] rgb = palette[c];
                        for(int i = 0; i < 3; i++) {
                            color[i] = rgb[i] * s / (shades - 1);
                        }
                        int delta;
                        int index = FindNearestColor(color, out delta);
                        if(index > 255)
                            throw new InvalidOperationException();
                        tbl[c + s * length] = (byte)index;
                    }
                }
            }
            return tbl;
        }
        //static int[] lumaWeights = new int[] { 299, 587, 114 };
        static int[] lumaWeights = new int[] { 76, 149, 29 }; // w*255/1000
        int FindNearestColor(int[] c, out int delta) {
            int bestIndex = -1;
            int dmin = int.MaxValue;
            int bestIndexLuma = -1;
            int ldmin = int.MaxValue;
            for(int index = 0; index < palette.Count; index++) {
                int d = 0;
                int ld = 0;
                byte[] rgb = palette[index];
                for(int i = 0; i < 3; i++) {
                    int v = absdiff(c[i], rgb[i]);
                    d += v * v;
                    ld += absdiff(c[i], rgb[i]) * lumaWeights[i];
                }
                if(d == 0) {
                    delta = dmin;
                    return index;
                }
                if(d < dmin) {
                    dmin = d;
                    bestIndex = index;
                }
                if(ld < ldmin) {
                    ldmin = ld;
                    bestIndexLuma = index;
                }
            }
            delta = dmin;
            if(ldmin * 3 < dmin * 255) {
                return bestIndexLuma;
            }
            //return bestIndexLuma;
            return bestIndex;
        }
        static int absdiff(int a, int b) {
            return a > b ? a - b : b - a;
        }
        public static Palette CreateSmart() {
            var p = new Palette(256, 256);
            p.AddColor(0xffffffff);
            //p.AddColor(0xBCBCBC);
            //NES row 1
            p.AddColor(0x0078F8);
            p.AddColor(0x0058F8);
            p.AddColor(0x6844FC);
            p.AddColor(0xD800CC);
            p.AddColor(0xE40058);
            p.AddColor(0xF83800);
            p.AddColor(0xE45C10);
            p.AddColor(0xAC7C00);
            p.AddColor(0x00B800);
            p.AddColor(0x00A800);
            p.AddColor(0x00A844);
            p.AddColor(0x008888);
            //
            p.AddColor(0);
            p.AddShades(0, 64, 64);
            for(int i = 0; i < 12; i++) {
                int last = p.AddShades(i + 1, 4, 24);
                last = p.AddShades(last, 4, 20);
                last = p.AddShades(last, 4, 16);
                last = p.AddShades(last, 4, 12);
                last = p.AddShades(last, 4, 8);
            }
            //for(int i = 0; i < 12; i++) {
            //    p.AddWhites(i + 1, 8, 8);
            //}
            System.Diagnostics.Debug.Assert(p.palette.Count <= 256);
            return p;
        }
        private int AddShades(int id, int maxshades, int numshades) {
            int index = 0;
            for(int j = 0; j < maxshades; j++) {
                var rgb = palette[id];
                var rgbs = new byte[3];
                for(int i = 0; i < 3; i++) {
                    rgbs[i] = Scale(rgb[i], numshades - j - 1, numshades - 1);
                }
                index = FindColor(rgbs);
                if(index < 0) {
                    index = palette.Count;
                    palette.Add(rgbs);
                }
            }
            return index;
        }
        private int FindColor(byte[] rgb) {
            for(int j = 0; j < palette.Count; j++) {
                var rgbp = palette[j];
                int d = 0;
                for(int i = 0; i < 3; i++) {
                    d += absdiff(rgbp[i], rgb[i]);
                }
                if(d == 0)
                    return j;
            }
            return -1;
        }
        private void AddWhites(int id, int maxshades, int numshades) {
            for(int j = 0; j < maxshades; j++) {
                var rgb = palette[id];
                var rgbs = new byte[3];
                for(int i = 0; i < 3; i++) {
                    rgbs[i] = (byte)(255 - Scale((byte)(255 - rgb[i]), numshades - j - 1, numshades - 1));
                }
                var index = FindColor(rgbs);
                if(index < 0) {
                    palette.Add(rgbs);
                }
            }
        }
    }
}
