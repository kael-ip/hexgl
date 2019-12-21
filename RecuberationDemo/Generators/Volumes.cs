using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation.Generators {

    class SphereVolume : IBinaryVolume {
        private float x, y, z, r, r2;
        private bool inverse;
        private Bounds3D bounds;
        public SphereVolume(float x, float y, float z, float r, bool inverse = false) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.r = r;
            this.r2 = r * r;
            this.inverse = inverse;
            this.bounds = new Bounds3D() {
                Xmin = (int)Math.Floor(x - r),
                Xmax = (int)Math.Ceiling(x + r),
                Ymin = (int)Math.Floor(y - r),
                Ymax = (int)Math.Ceiling(y + r),
                Zmin = (int)Math.Floor(z - r),
                Zmax = (int)Math.Ceiling(z + r)
            };
        }
        public Bounds3D Bounds {
            get { return bounds; }
        }
        public bool IsOccupied(int x, int y, int z) {
            var s2 = (this.x - x) * (this.x - x) + (this.y - y) * (this.y - y) + (this.z - z) * (this.z - z);
            return inverse ? s2 > r2 : s2 < r2;
        }
    }

    class RandomHeightPlane : IBinaryVolume {
        int xsize, ysize, zmax;
        private int[] heights;
        private Bounds3D bounds;
        public RandomHeightPlane(int xsize, int ysize, int zmax, int seed) {
            this.xsize = xsize;
            this.ysize = ysize;
            this.zmax = zmax;
            var rnd = new Random(seed);
            heights = new int[xsize * ysize];
            int p = 0;
            for(int y = 0; y < ysize; y++) {
                for(int x = 0; x < xsize; x++) {
                    heights[p++] = rnd.Next(zmax);
                }
            }
            bounds = new Bounds3D() {
                Xmin = 0, Xmax = xsize,
                Ymin = 0, Ymax = ysize,
                Zmin = 0, Zmax = zmax
            };
        }
        public Bounds3D Bounds {
            get { return bounds; }
        }
        public bool IsOccupied(int x, int y, int z) {
            if(x < 0 || x >= xsize || y < 0 || y >= ysize || z < 0 || z >= zmax)
                return false;
            return z < heights[y * xsize + x];
        }
    }
}
