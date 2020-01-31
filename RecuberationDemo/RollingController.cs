using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    class RollingController {
        private Quad quad, next;
        private IBinaryVolumeWritable volume;
        private Axis dirAxis;
        private bool dirIsNegative;
        private Edge edge;
        private bool rIsNegative;
        private float[] omat, mat;
        private int frame, period, rq;
        private int color;
        public int Period { get; set; }
        public int Color { get { return color; } }
        public RollingController(IBinaryVolumeWritable volume) {
            this.omat = new float[12];
            this.mat = new float[12];
            GLMath.Identity3(omat);
            GLMath.Identity3(mat);
            this.volume = volume;
        }
        public void Setup(Quad quad, Axis dirAxis, bool dirIsNegative, int period, int color) {
            if(quad.IsOccupied) {
                throw new ArgumentException();
            }
            quad.IsOccupied = true;
            this.quad = quad;
            this.next = quad;
            this.dirAxis = dirAxis;
            this.dirIsNegative = dirIsNegative;
            this.Period = this.period = period;
            this.frame = 0;
            this.rq = 0;
            this.color = color;
            UpdateLocation();
        }
        public void Advance() {
            if(quad == null)
                return;
            if(frame >= rq * period) {
                frame = 0;
                if(Period <= 0) {
                    Period = 256;
                }
                period = Period;
                ReAnimate();
            } else {
                frame++;
            }
            UpdateLocation();
        }
        private void UpdateLocation() {
            float[] rmat = new float[12];
            float[] rvec = new float[3];
            float angle = (float)(frame * Math.PI / 2 / period);
            if(rIsNegative) {
                angle = -angle;
            }
            float e = 100;// arbitrary value
            if(edge != null) {
                if(edge.Axis == Axis.X) {
                    if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && rIsNegative) {
                        SetVector(rvec, e, 0, 0);
                    } else if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.Y && quad.NormalIsNegative && !rIsNegative) {
                        SetVector(rvec, e, 1, 0);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.Y && quad.NormalIsNegative && rIsNegative) {
                        SetVector(rvec, e, 1, 1);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && !rIsNegative) {
                        SetVector(rvec, e, 0, 1);
                    }
                    RotateAbout(rmat, angle, 1, 0, 0, rvec);
                } else if(edge.Axis == Axis.Y) {
                    if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && !rIsNegative) {
                        SetVector(rvec, 0, e, 0);
                    } else if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && rIsNegative) {
                        SetVector(rvec, 1, e, 0);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && rIsNegative) {
                        SetVector(rvec, 0, e, 1);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && !rIsNegative) {
                        SetVector(rvec, 1, e, 1);
                    }
                    RotateAbout(rmat, angle, 0, 1, 0, rvec);
                } else {// edge.Axis == Axis.Z
                    if(quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && !rIsNegative) {
                        SetVector(rvec, 1, 0, e);
                    } else if(quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && rIsNegative) {
                        SetVector(rvec, 0, 0, e);
                    } else if(quad.NormalAxis == Axis.Y && quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && rIsNegative) {
                        SetVector(rvec, 1, 1, e);
                    } else if(quad.NormalAxis == Axis.Y && quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && !rIsNegative) {
                        SetVector(rvec, 0, 1, e);
                    }
                    RotateAbout(rmat, angle, 0, 0, 1, rvec);
                }
            }
            GLMath.MatrixMultiply(mat, omat, rmat);
            float[] tvec = new float[] { quad.Location.X, quad.Location.Y, quad.Location.Z };
            if(quad.NormalIsNegative) {
                tvec[(int)quad.NormalAxis] -= 1;
            }
            GLMath.Translate3(rmat, tvec[0], tvec[1], tvec[2]);
            GLMath.MatrixMultiply(mat, mat, rmat);
        }
        private void SetVector(float[] v, float x, float y, float z) {
            v[0] = x;
            v[1] = y;
            v[2] = z;
        }
        private void RotateAbout(float[] m, float angle, float ax, float ay, float az, float[] origin) {
            float[] tm = new float[12];
            GLMath.Translate3(tm, -origin[0], -origin[1], -origin[2]);
            GLMath.Rotate3(m, angle, ax, ay, az);
            GLMath.MatrixMultiply(m, tm, m);
            GLMath.Translate3(tm, origin[0], origin[1], origin[2]);
            GLMath.MatrixMultiply(m, m, tm);
        }
        public void ReadLocation3x4(float[] m) {
            Array.Copy(mat, m, 12);
        }
        private void ReAnimate() {
            SetIsOccupied(quad, false, next);
            quad = next;
            SetIsOccupied(quad, true, null);
            next = null;
            edge = null;
            int attempt = 0;
            while(true) {
                if(attempt > 5) {//abort
                    next = quad;
                    rq = 0;
                    break;
                }
                edge = quad.GetEdge(dirAxis, dirIsNegative);
                if(edge != null) {
                    if(edge.Q0 == quad) {
                        next = edge.Q1;
                        rIsNegative = false;
                    } else {
                        next = edge.Q0;
                        rIsNegative = true;
                    }
                    rq = edge.Angle - 1;
                    if(!IsOccupied(next, quad)) {
                        break;
                    }
                }
                if((attempt % 2) == 0) {
                    dirAxis = (Axis)(((int)dirAxis + 1) % 3);
                }
                if(attempt == 0) {
                    dirIsNegative = (new PRNG().Next() & (1 << 5)) == 0;
                } else {
                    dirIsNegative = !dirIsNegative;
                }
                attempt++;
            }
            SetIsOccupied(next, true, quad);
            quad.Color = color;
            System.Diagnostics.Trace.TraceInformation("Direction: {1}{0}", dirAxis, dirIsNegative ? "-" : "+");
            //if actual angle == 0, repeat, limit retries
        }
        private void SetIsOccupied(Quad q, bool yes, Quad prev) {
            q.IsOccupied = yes;
            if(volume != null) {
                VectorI3D vp = GetVoxelLocation(q, 0);
                volume.SetIsOccupied(vp.X, vp.Y, vp.Z, yes);
                vp = GetVoxelLocation(q, 1);
                volume.SetIsOccupied(vp.X, vp.Y, vp.Z, yes);
                if(prev != null) {
                    vp = GetVoxelLocation(q, 0, prev);
                    volume.SetIsOccupied(vp.X, vp.Y, vp.Z, yes);
                }
            }
        }
        private bool IsOccupied(Quad q, Quad prev) {
            if(q.IsOccupied)
                return true;
            if(volume != null) {
                VectorI3D vp = GetVoxelLocation(q, 0);
                if(volume.IsOccupied(vp.X, vp.Y, vp.Z))
                    return true;
                vp = GetVoxelLocation(q, 1);
                if(volume.IsOccupied(vp.X, vp.Y, vp.Z))
                    return true;
                if(prev != null) {
                    vp = GetVoxelLocation(q, 0, prev);
                    if(volume.IsOccupied(vp.X, vp.Y, vp.Z))
                        return true;
                }
            }
            return false;
        }
        private VectorI3D GetVoxelLocation(Quad q, int off) {
            VectorI3D vp = q.Location;
            if(q.NormalIsNegative) {
                off = -off - 1;
            }
            switch(q.NormalAxis) {
                case Axis.X:
                    vp.X += off;
                    break;
                case Axis.Y:
                    vp.Y += off;
                    break;
                case Axis.Z:
                    vp.Z += off;
                    break;
            }
            return vp;
        }
        private VectorI3D GetVoxelLocation(Quad q, int off, Quad prev) {
            VectorI3D vp = q.Location;
            if(q.NormalIsNegative) {
                off = -off - 1;
            }
            int poff = prev.NormalIsNegative ? -1 : 1;
            switch(prev.NormalAxis) {
                case Axis.X:
                    vp.X += poff;
                    break;
                case Axis.Y:
                    vp.Y += poff;
                    break;
                case Axis.Z:
                    vp.Z += poff;
                    break;
            }
            switch(q.NormalAxis) {
                case Axis.X:
                    vp.X += off;
                    break;
                case Axis.Y:
                    vp.Y += off;
                    break;
                case Axis.Z:
                    vp.Z += off;
                    break;
            }
            return vp;
        }
    }

    class WalkingSystem {
        private QuadMap map;
        private IBinaryVolumeWritable volume;
        private QMesh mesh;
        private List<RollingController> controllers;
        public WalkingSystem(QuadMap map, IBinaryVolumeWritable volume) {
            this.map = map;
            this.volume = volume;
            controllers = new List<RollingController>();
            Trace.TraceInformation("Quads count = {0}", map.Quads.Count);
            Trace.TraceInformation("Quad groups = {0}", map.CheckConnectivity());
            this.mesh = new QMesh(map.Quads);
        }
        public void AddRandomWalkers(int cubeCount, int seed, int stepFramesMin, int stepFramesMag = 8, int stepFramesVar = 5) {
            var rnd = new PRNG(seed);
            for(int i = 0; i < cubeCount; i++) {
                var controller = new RollingController(volume);
                Quad quad = null;
                while(quad == null || quad.IsOccupied) {
                    quad = map.Quads[rnd.Next(map.Quads.Count)];
                }
                controller.Setup(quad, (Axis)rnd.Next(3), (rnd.Next() & 1) != 0, stepFramesMin + (stepFramesMag << rnd.Next(stepFramesVar)), i % 12 + 1);
                controllers.Add(controller);
            }
        }
        public Mesh Mesh { get { return mesh; } }
        public List<RollingController> Controllers { get { return controllers; } }
        public void Advance() {
            foreach(var controller in controllers) {
                controller.Advance();
            }
        }
    }
}
