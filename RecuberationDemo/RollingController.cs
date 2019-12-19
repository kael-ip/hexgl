using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;

namespace HexTex.Recuberation {

    class RollingController {
        private Quad quad, next;
        private Axis dirAxis;
        private bool dirIsNegative;
        private Edge edge;
        private bool rIsNegative;
        private System.Numerics.Matrix4x4 omat, mat;
        private int frame, period, rq;
        private int color;
        public int Period { get; set; }
        public int Color { get { return color; } }
        public RollingController() {
            this.omat = this.mat = System.Numerics.Matrix4x4.Identity;
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
            System.Numerics.Matrix4x4 rmat = System.Numerics.Matrix4x4.Identity;
            System.Numerics.Vector3 rvec = System.Numerics.Vector3.Zero;
            float angle = (float)(frame * Math.PI / 2 / period);
            if(rIsNegative) {
                angle = -angle;
            }
            float e = 100;// arbitrary value
            if(edge != null) {
                if(edge.Axis == Axis.X) {
                    if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && rIsNegative) {
                        rvec = new System.Numerics.Vector3(e, 0, 0);
                    } else if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.Y && quad.NormalIsNegative && !rIsNegative) {
                        rvec = new System.Numerics.Vector3(e, 1, 0);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.Y && quad.NormalIsNegative && rIsNegative) {
                        rvec = new System.Numerics.Vector3(e, 1, 1);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && !rIsNegative) {
                        rvec = new System.Numerics.Vector3(e, 0, 1);
                    }
                    rmat = System.Numerics.Matrix4x4.CreateRotationX(angle, rvec);
                } else if(edge.Axis == Axis.Y) {
                    if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && !rIsNegative) {
                        rvec = new System.Numerics.Vector3(0, e, 0);
                    } else if(quad.NormalAxis == Axis.Z && !quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && rIsNegative) {
                        rvec = new System.Numerics.Vector3(1, e, 0);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && rIsNegative) {
                        rvec = new System.Numerics.Vector3(0, e, 1);
                    } else if(quad.NormalAxis == Axis.Z && quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && !rIsNegative) {
                        rvec = new System.Numerics.Vector3(1, e, 1);
                    }
                    rmat = System.Numerics.Matrix4x4.CreateRotationY(angle, rvec);
                } else {// edge.Axis == Axis.Z
                    if(quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && !rIsNegative) {
                        rvec = new System.Numerics.Vector3(1, 0, e);
                    } else if(quad.NormalAxis == Axis.Y && !quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && rIsNegative) {
                        rvec = new System.Numerics.Vector3(0, 0, e);
                    } else if(quad.NormalAxis == Axis.Y && quad.NormalIsNegative && !rIsNegative || quad.NormalAxis == Axis.X && quad.NormalIsNegative && rIsNegative) {
                        rvec = new System.Numerics.Vector3(1, 1, e);
                    } else if(quad.NormalAxis == Axis.Y && quad.NormalIsNegative && rIsNegative || quad.NormalAxis == Axis.X && !quad.NormalIsNegative && !rIsNegative) {
                        rvec = new System.Numerics.Vector3(0, 1, e);
                    }
                    rmat = System.Numerics.Matrix4x4.CreateRotationZ(angle, rvec);
                }
            }
            mat = System.Numerics.Matrix4x4.Multiply(omat, rmat);
            System.Numerics.Vector3 tvec = new System.Numerics.Vector3(quad.Location.X, quad.Location.Y, quad.Location.Z);
            if(quad.NormalIsNegative) {
                switch(quad.NormalAxis) {
                    case Axis.X:
                        tvec.X -= 1;
                        break;
                    case Axis.Y:
                        tvec.Y -= 1;
                        break;
                    case Axis.Z:
                        tvec.Z -= 1;
                        break;
                }
            }
            mat = System.Numerics.Matrix4x4.Multiply(mat, System.Numerics.Matrix4x4.CreateTranslation(tvec));
        }
        public void ReadLocation(UniformFloat position, UniformMatrix rotation) {
            position.Set(mat.M41, mat.M42, mat.M43);
            rotation.Set(mat.GetRotationMatrixAsArray());
        }
        private void ReAnimate() {
            //TODO: mark quad occupation and check if it is not occupied
            quad.IsOccupied = false;
            quad = next;
            quad.IsOccupied = true;
            next = null;
            edge = null;
            int attempt = 0;
            while(next == null || next.IsOccupied) {
                if(attempt > 10) {//abort
                    next = quad;
                    rq = 0;
                    break;
                }
                if(attempt > 0) {
                    if(attempt % 2 == 1) {
                        dirAxis = (Axis)(((int)dirAxis + 1) % 3);
                    } else {
                        dirIsNegative = !dirIsNegative;
                    }
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
                }
                attempt++;
            }
            next.IsOccupied = true;
            quad.Color = color;
            //TODO: occupy quads based on volume cells and consider self-occupation for 0-moves
            System.Diagnostics.Trace.TraceInformation("Direction: {1}{0}", dirAxis, dirIsNegative ? "-" : "+");
            //if actual angle == 0, repeat, limit retries
        }
    }
}
