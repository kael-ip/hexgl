using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {
    public class CachedVolume : IBinaryVolume {
        private bool inverse;
        private Bounds3D bounds;
        private byte[] data;
        public CachedVolume(Bounds3D bounds, bool inverse = false) {
            this.bounds = bounds;
            this.inverse = inverse;
            this.data = new byte[(bounds.Xmax - bounds.Xmin) * (bounds.Ymax - bounds.Ymin) * (bounds.Zmax - bounds.Zmin)];
        }
        public Bounds3D Bounds {
            get { return bounds; }
        }
        public bool IsOccupied(int x, int y, int z) {
            int index = GetIndex(x, y, z);
            if(index < 0)
                return inverse;
            return inverse ? data[index] == 0 : data[index] != 0;
        }
        public void MakeBox(Bounds3D s, bool occupy) {
            for(var z = s.Zmin; z < s.Zmax; z++) {
                for(var y = s.Ymin; y < s.Ymax; y++) {
                    for(var x = s.Xmin; x < s.Xmax; x++) {
                        int index = GetIndex(x, y, z);
                        if(index >= 0) {
                            data[index] = (byte)(occupy ? 1 : 0);
                        }
                    }
                }
            }
        }
        public void MakePyramid(int xo, int yo, int zo, int height, bool upsidedown, bool occupy) {
            for(var z = 0; z < height; z++) {
                for(var y = -z; y <= z; y++) {
                    for(var x = -z; x <= z; x++) {
                        int zz = upsidedown ? height - z - 1 : z;
                        int index = GetIndex(x + xo, y + yo, zz + zo);
                        if(index >= 0) {
                            data[index] = (byte)(occupy ? 1 : 0);
                        }
                    }
                }
            }
        }
        public void MakeRhombamid(int xo, int yo, int zo, int height, bool upsidedown, bool occupy) {
            for(var z = 0; z < height; z++) {
                for(var y = -z; y <= z; y++) {
                    int yabs = y < 0 ? -y : y;
                    for(var x = yabs - z; x <= z - yabs; x++) {
                        int zz = upsidedown ? height - z - 1 : z;
                        int index = GetIndex(x + xo, y + yo, zz + zo);
                        if(index >= 0) {
                            data[index] = (byte)(occupy ? 1 : 0);
                        }
                    }
                }
            }
        }
        private int GetIndex(int x, int y, int z) {
            if(x < bounds.Xmin
            || x >= bounds.Xmax
            || y < bounds.Ymin
            || y >= bounds.Ymax
            || z < bounds.Zmin
            || z >= bounds.Zmax
            )
                return -1;
            return (z - bounds.Zmin) * (bounds.Xmax - bounds.Xmin) * (bounds.Ymax - bounds.Ymin) + (y - bounds.Ymin) * (bounds.Xmax - bounds.Xmin) + (x - bounds.Xmin);
        }
    }
}
