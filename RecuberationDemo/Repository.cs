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
        public WalkingSystem sysSphereInside;
        public WalkingSystem sysPlane;
        public WalkingSystem sysMetaBall4;
        public WalkingSystem sysMetaBall2;
        public Mesh objCube;
        public Mesh objBanner;
        public Mesh objBannerMhm;

        public void Init() {
            objCube = CreateCube();
            objBanner = CreateBanner();
            objBannerMhm = CreateBanner(Properties.Resources.rcbmhm);
            sysCube1 = CreateEarthCube();
            sysSphere3 = CreateEarthSphere3();
            sysPlane = CreateEarthPlane();
            sysSphereInside = CreateEarthSphereInside();
            sysMetaBall4 = CreateEarthMB4();
            sysMetaBall2 = CreateEarthMB2();
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
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(10, 657, 32, 4, 5);
            return system;
        }
        private WalkingSystem CreateEarthCube() {
            var volume = new SphereVolume(0, 0, 0, 0.8f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(1, 1432, 32, 0);
            return system;
        }
        private WalkingSystem CreateEarthSphere3() {
            var volume = new SphereVolume(0, 0, 0, 3.3f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(7, 8465, 64, 8, 5);
            return system;
        }
        private WalkingSystem CreateEarthSphereInside() {
            var volume = new SphereVolume(0, 0, 0, 7.8f, true);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
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
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(9, 2020, 64, 4, 5);
            return system;
        }
        private WalkingSystem CreateEarthMB2() {
            var volume = new MetaballVolume(new Bounds3D(-10, 10, -10, 10, -10, 10), 1.2f);
            volume.AddBall(3, 3, 1, 2.5f);
            volume.AddBall(-2, -1, -2, 3.4f);
            QuadMap quadMap = new QuadMap();
            quadMap.Build(volume);
            var system = new WalkingSystem(quadMap);
            system.AddRandomWalkers(9, 6345, 64, 8, 4);
            return system;
        }
    }
}
