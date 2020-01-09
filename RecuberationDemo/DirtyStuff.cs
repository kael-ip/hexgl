using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    class DirtyStuff {
        public static void WriteC(int w, int h, byte[] data, string fileName) {
            using(var fs = File.OpenWrite(fileName)) {
                WriteByte(fs, (byte)w);
                fs.WriteByte((byte)',');
                WriteByte(fs, (byte)h);
                fs.WriteByte((byte)',');
                for(var i = 0; i < data.Length; i++) {
                    if(i % 8 == 0) {
                        fs.WriteByte((byte)0x0d);
                        fs.WriteByte((byte)0x0a);
                    }
                    WriteByte(fs, data[i]);
                    if(i < data.Length - 1) {
                        fs.WriteByte((byte)',');
                    }
                }
            }
        }
        private static void WriteByte(FileStream fs, byte v) {
            var buf = Encoding.ASCII.GetBytes(string.Format("0x{0:X2}", v));
            fs.Write(buf, 0, buf.Length);
        }
    }
}
