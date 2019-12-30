using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    class DemoData {
        public static int[] Banner1 = new int[]{
            //0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,1,1,1,1,1,0,0,0,0,1,1,1,1,0,0,0,0,1,1,1,1,1,0,0,1,1,0,0,1,1,0,
            0,1,1,0,0,1,1,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,1,1,0,
            0,1,1,1,1,1,0,0,0,1,1,1,1,1,1,0,0,1,1,0,0,0,0,0,0,1,1,0,0,1,1,0,
            0,1,1,0,0,1,1,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,1,1,0,
            0,1,1,0,0,0,1,0,0,0,1,1,1,1,0,0,0,0,1,1,1,1,1,0,0,0,1,1,1,1,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,1,1,1,1,1,0,0,0,0,1,1,1,1,0,0,0,1,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
            0,1,1,0,0,1,1,0,0,1,1,0,0,0,0,0,0,1,1,0,0,1,1,0,0,0,0,0,0,1,1,0,
            0,1,1,1,1,1,0,0,0,1,1,1,1,1,1,0,0,1,1,1,1,1,0,0,0,0,1,1,1,1,1,0,
            0,1,1,0,0,1,1,0,0,1,1,0,0,0,0,0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,
            0,1,1,1,1,1,0,0,0,0,1,1,1,1,0,0,0,1,1,0,0,0,1,0,0,0,1,1,1,1,1,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,1,1,1,1,1,1,0,0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,0,1,0,0,0,1,1,0,
            0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,
            0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0,0,1,1,1,1,1,1,0,
            0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,
            0,0,0,1,1,0,0,0,0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,0,1,1,0,0,0,1,0,
            //0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        };
    }

    class PixelMap {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Values { get; private set; }
        public byte this[int column, int row] {
            get {
                if(column < 0 || column >= Width || row < 0 || row >= Height)
                    throw new ArgumentOutOfRangeException();
                return Values[row * Width + column];
            }
        }
        private PixelMap(int width, int height) {
            this.Width = width;
            this.Height = height;
            this.Values = new byte[width * height];
        }
        public static PixelMap Load(Bitmap bitmap) {
            var result = new PixelMap(bitmap.Width, bitmap.Height);
            int i=0;
            for(var y = 0; y < bitmap.Height; y++) {
                for(var x = 0; x < bitmap.Width; x++) {
                    var c = bitmap.GetPixel(x, y);
                    result.Values[i++] = (byte)(c.GetBrightness() * 255);
                }
            }
            return result;
        }
    }
}
