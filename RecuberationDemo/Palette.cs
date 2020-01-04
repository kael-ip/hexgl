using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    class Palette {
        int length;
        int shades;
        int usedColors;
        byte[] palette;
        byte[] lightTable;
        private Palette(int length, int shades) {
            this.length = length;
            this.shades = shades;
            palette = new byte[length * 3];
            this.usedColors = 0;
        }
        public uint[] GetPalette() {
            uint[] data = new uint[length];
            for(int i = 0, j = 0; i < length; i++) {
                byte r = palette[j++];
                byte g = palette[j++];
                byte b = palette[j++];
                data[i] = PackColor(r, g, b);
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
            if(this.length - usedColors < n)
                throw new ArgumentOutOfRangeException();
            byte r, g, b;
            UnpackColor(color, out r, out g, out b);
            for(int i = 0, j = usedColors * 3; i < n; i++) {
                palette[j++] = Scale(r, n - i - 1, n - 1);
                palette[j++] = Scale(g, n - i - 1, n - 1);
                palette[j++] = Scale(b, n - i - 1, n - 1);
            }
            usedColors += n;
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
            int i = 0;
            for(var s = 0; s < 16; s++) {
                for(var j = 0; j < 16; j++) {
                    byte r, g, b;
                    UnpackColor(colors[j], out r, out g, out b);
                    p.palette[i++] = Scale(r, 15 - s, 15);
                    p.palette[i++] = Scale(g, 15 - s, 15);
                    p.palette[i++] = Scale(b, 15 - s, 15);
                }
            }
            p.usedColors = 256;
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
            for(int c = 0, j = 0; c < length; c++, j += 3) {
                for(int s = 0; s < shades; s++) {
                    for(int i = 0; i < 3; i++) {
                        color[i] = palette[j + i] * s / (shades - 1);
                    }
                    tbl[c + s * length] = FindNearestColor(color);
                }
            }
            return tbl;
        }
        byte FindNearestColor(int[] c) {
            int bestIndex = -1;
            int dmin = int.MaxValue;
            for(int index = 0, j = 0; index < this.length; index++) {
                int d = 0;
                for(int i = 0; i < 3; i++) {
                    int v = absdiff(c[i], palette[j++]);
                    d += v * v;
                }
                if(d == 0)
                    return (byte)index;
                if(d < dmin) {
                    dmin = d;
                    bestIndex = index;
                }
            }
            return (byte)bestIndex;
        }
        static int absdiff(int a, int b) {
            return a > b ? a - b : b - a;
        }
    }
}
