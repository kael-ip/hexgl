using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.Recuberation.Generators;

namespace HexTex.Recuberation {
    class Repository {
        private static Lazy<Repository> instance = new Lazy<Repository>(() =>{
            var repository = new Repository();
            repository.Init();
            return repository;
        });
        public static Repository Instance { get { return instance.Value; } }

        public WalkingSystem sysCube1;
        public WalkingSystem sysSphere3;
        public WalkingSystem sysSphere3x;
        public WalkingSystem sysSphereInside;
        public WalkingSystem sysPlane;
        public WalkingSystem sysMetaBall4;
        public WalkingSystem sysMetaBall2;
        public WalkingSystem sysCuboid0;
        public WalkingSystem sysCuboid1;
        public WalkingSystem sysCuboid2;
        public WalkingSystem sysCuboid3;
        public Mesh objCube;
        public Mesh objBanner;
        public Mesh objBannerMhm;
        public Mesh objSlab;
        public Mesh objPin;
        public Mesh objAntiCube;
        public Mesh objPyramid1;
        public Mesh objPyramid2;
        public Mesh objPyramid3;
        public Mesh objFirTree;

        public void Init() {
            objCube = CreateCube();
            objBanner = CreateBanner();
            objBannerMhm = CreateBanner(Properties.Resources.rcbmhm);
            sysCube1 = CreateEarthCube();
            sysSphere3 = CreateEarthSphere3();
            sysSphere3x = CreateEarthSphere3x();
            sysPlane = CreateEarthPlane();
            sysSphereInside = CreateEarthSphereInside();
            sysMetaBall4 = CreateEarthMB4();
            sysMetaBall2 = CreateEarthMB2();
            sysCuboid0 = CreateEarthCuboid0();
            sysCuboid1 = CreateEarthCuboidRandom(743);
            sysCuboid2 = CreateEarthCuboidRandom(8942);
            sysCuboid3 = CreateEarthCuboidRandom(604);
            objSlab = CreateSlab();
            objPin = CreatePin();
            objAntiCube = CreateAntiCube();
            objPyramid1 = CreatePyramid(3);
            objPyramid2 = CreatePyramid(4);
            objPyramid3 = CreatePyramid(5);
            objFirTree = CreateFirTree();
        }
        private Mesh CreateCube() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildCube();
            return new QMesh(quadMap.Quads);
        }
        private QMesh CreateBanner() {
            QuadMap quadMap = new QuadMap();
            int[] data = new int[DemoData.Banner1.Length];
            var rnd = new PRNG();
            for(var i = 0; i < data.Length; i++) {
                data[i] = DemoData.Banner1[i] * (rnd.Next(12) + 1);
            }
            quadMap.BuildHeightPlane(data, 32, 17, 0, true);
            return new QMesh(quadMap.Quads);
        }
        private QMesh CreateBanner(System.Drawing.Bitmap bitmap) {
            var pixelMap = PixelMap.Load(bitmap);
            QuadMap quadMap = new QuadMap();
            quadMap.BuildHeightPlane(pixelMap.Width, pixelMap.Height,
                (x, y) => pixelMap[x, y] * 32 / 256, 0, true);
            return new QMesh(quadMap.Quads);
        }
        private WalkingSystem CreateEarthPlane() {
            QuadMap quadMap = new QuadMap();
            quadMap.BuildPlane(10, 10);
            var system = new WalkingSystem(quadMap, null);
            system.AddRandomWalkers(10, 657, 32, 4, 5);
            return system;
        }
        private WalkingSystem CreateSystem(IBinaryVolume volume) {
            QuadMap quadMap = new QuadMap();
            var cv = new CachedVolume(volume);
            quadMap.Build(cv);
            var system = new WalkingSystem(quadMap, cv);
            return system;
        }
        private WalkingSystem CreateEarthCube() {
            var volume = new SphereVolume(0, 0, 0, 0.8f);
            var system = CreateSystem(volume);
            system.AddRandomWalkers(1, 1432, 32, 0);
            return system;
        }
        private WalkingSystem CreateEarthSphere3() {
            var volume = new SphereVolume(0, 0, 0, 3.3f);
            var system = CreateSystem(volume);
            system.AddRandomWalkers(7, 8465, 64, 8, 5);
            return system;
        }
        private WalkingSystem CreateEarthSphere3x() {
            var volume = new SphereVolume(0, 0, 0, 3.3f);
            var system = CreateSystem(volume);
            system.AddRandomWalkers(21, 8465, 64, 8, 5);
            return system;
        }
        private WalkingSystem CreateEarthSphereInside() {
            var volume = new SphereVolume(0, 0, 0, 7.8f, true);
            var system = CreateSystem(volume);
            system.AddRandomWalkers(37, 8465, 64, 16, 6);
            return system;
        }
        private WalkingSystem CreateEarthMB4() {
            //var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            //volume.AddBall(-2, 0, 1, 5.3f);
            //volume.AddBall(1, -4, 3, -1.3f);
            //volume.AddBall(3, 3, -2, 2.4f);
            var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            volume.AddBall(-3, 2, -3, 2.3f);
            volume.AddBall(3, 2, 3, 2.3f);
            volume.AddBall(2, -2, -2, 2.3f);
            volume.AddBall(-2, -2, 2, 2.3f);
            var system = CreateSystem(volume);
            system.AddRandomWalkers(9, 2020, 64, 4, 5);
            return system;
        }
        private WalkingSystem CreateEarthMB2() {
            var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            volume.AddBall(3, 3, 1, 2.5f);
            volume.AddBall(-2, -1, -2, 3.4f);
            var system = CreateSystem(volume);
            system.AddRandomWalkers(9, 6345, 64, 8, 4);
            return system;
        }
        private WalkingSystem CreateEarthCuboid0() {
            var volume = new CachedVolume(new Bounds3D(-10, 10, -10, 10, -10, 10));
            volume.MakeBox(new Bounds3D(-5, 3, -5, 3, -5, 3), true);
            volume.MakeBox(new Bounds3D(-1, 5, -1, 5, -1, 5), true);
            volume.MakeBox(new Bounds3D(2, 5, 2, 5, 2, 5), false);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap, volume);
            system.AddRandomWalkers(12, 4537, 64, 8, 4);
            return system;
        }
        private WalkingSystem CreateEarthCuboidRandom(int seed) {
            var volume = new CachedVolume(new Bounds3D(-10, 10, -10, 10, -10, 10));
            var rnd = new PRNG(seed);
            int maxs = 10;
            for(int i = 0; i < 30; i++) {
                int xs = rnd.Next(maxs) + 1;
                int ys = rnd.Next(maxs) + 1;
                int zs = rnd.Next(maxs) + 1;
                int x = rnd.Next(20 - xs) - 10;
                int y = rnd.Next(20 - ys) - 10;
                int z = rnd.Next(20 - zs) - 10;
                bool occupy = (rnd.Next() & 12) != 0;
                volume.MakeBox(new Bounds3D(x, x + xs, y, y + ys, z, z + zs), occupy);
            }
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap, volume);
            system.AddRandomWalkers(12, 4537, 64, 8, 4);
            return system;
        }
        private Mesh CreateSlab() {
            int s = 5;
            var volume = new CachedVolume(new Bounds3D(-s, s, -s, s, 0, 1));
            volume.MakeBox(new Bounds3D(-s, s, -s, s, 0, 1), true);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            return new QMesh(quadMap.Quads);
        }
        private Mesh CreatePin() {
            int s = 3;
            int e = 1;
            var volume = new CachedVolume(new Bounds3D(-e, e, -e, e, -s, s));
            volume.MakeBox(new Bounds3D(-e, e, -e, e, -s, s), true);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            return new QMesh(quadMap.Quads);
        }
        private Mesh CreateAntiCube() {
            int s = 3;
            int e = 1;
            var volume = new CachedVolume(new Bounds3D(-s, s, -s, s, -s, s));
            volume.MakeBox(new Bounds3D(-s, s, -s, s, -s, s), true);
            volume.MakeBox(new Bounds3D(-e, e, -e, e, -s, s), false);
            volume.MakeBox(new Bounds3D(-e, e, -s, s, -e, e), false);
            volume.MakeBox(new Bounds3D(-s, s, -e, e, -e, e), false);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            return new QMesh(quadMap.Quads);
        }
        private Mesh CreatePyramid(int s) {
            var volume = new CachedVolume(new Bounds3D(-s, s, -s, s, 0, s));
            volume.MakePyramid(0, 0, 0, s, true, true);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            return new QMesh(quadMap.Quads);
        }
        private Mesh CreateFirTree() {
            int th = 5;
            int p1h = 9, p2h = 5, p3h = 3;
            int s = p1h + 1;
            var volume = new CachedVolume(new Bounds3D(-s, s, -s, s, 0, th + 16));
            //volume.MakePyramid(0, 0, th + 11, 3, true, true);
            volume.MakeRhombamid(0, 0, th + 10, 5, true, true);
            //volume.MakePyramid(0, 0, th + 7, 5, true, true);
            volume.MakeRhombamid(0, 0, th + 5, 7, true, true);
            //volume.MakePyramid(0, 0, th + 3, 7, true, true);
            volume.MakeRhombamid(0, 0, th, 9, true, true);
            volume.MakeBox(new Bounds3D(-0, 1, -0, 1, 0, th), true);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var rnd = new PRNG(78487);
            for(var i = 0; i < 240; i++) {
                int index = rnd.Next(quadMap.Quads.Count);
                var q = quadMap.Quads[index];
                if(q.Location.Z >= th) {
                    q.Color = rnd.Next(12) + 1;
                }
            }
            return new QMesh(quadMap.Quads);
        }
    }
}
