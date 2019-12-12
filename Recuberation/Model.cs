using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {

    public struct Bounds3D {
        public int Xmin { get; set; }
        public int Xmax { get; set; }
        public int Ymin { get; set; }
        public int Ymax { get; set; }
        public int Zmin { get; set; }
        public int Zmax { get; set; }
        public override string ToString() {
            return string.Format("({{{0}, {1}, {2}}}, {{{3}, {4}, {5}}})", Xmin, Ymin, Zmin, Xmax, Ymax, Zmax);
        }
    }

    public interface IBinaryVolume {
        Bounds3D Bounds { get; }
        bool IsOccupied(int x, int y, int z);
    }

    public enum Axis { X = 0, Y = 1, Z = 2 }

    public struct VectorI2D {
        public int X;
        public int Y;
        public VectorI2D(int x, int y) {
            this.X = x;
            this.Y = y;
        }
        public override string ToString() {
            return string.Format("{{{0}, {1}}}", X, Y);
        }
    }

    public struct VectorI3D {
        public int X;
        public int Y;
        public int Z;
        public VectorI3D(int x, int y, int z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public override string ToString() {
            return string.Format("{{{0}, {1}, {2}}}", X, Y, Z);
        }
    }

    public class Quad {
        public Axis NormalAxis { get; set; }
        public bool NormalIsNegative { get; set; }
        public int PlaneValue { get; set; }
        public VectorI2D LocationOnPlane { get; set; } // Z:(x,y) Y:(z,x) X:(y,z)
        public int Color { get; set; }
        public bool IsOccupied { get; set; }
        public object Tag { get; set; }
        private Edge[] edges = new Edge[4];
        public Edge GetEdge(Axis axis, bool sign) {
            if(axis == NormalAxis)
                return null;
            throw new NotImplementedException();
        }
        public Quad() {
        }
        public static bool ccwFront = false;
        public int FillQuadVerts(float[] vbuffer, float[] nbuffer, int offset) { // fill vertex buffer with 4 verts (12 floats)
            Action<int, int, int> put = (x, y, z) => {
                vbuffer[offset + 0] = x;
                vbuffer[offset + 1] = y;
                vbuffer[offset + 2] = z;
                float av = NormalIsNegative ? -1.0f : 1.0f;
                nbuffer[offset + 0] = (NormalAxis == Axis.X) ? av : 0;
                nbuffer[offset + 1] = (NormalAxis == Axis.Y) ? av : 0;
                nbuffer[offset + 2] = (NormalAxis == Axis.Z) ? av : 0;
                offset += 3;
            };
            var ccw = ccwFront ? NormalIsNegative : !NormalIsNegative;
            if(NormalAxis == Axis.Z) {
                put(LocationOnPlane.X, LocationOnPlane.Y, PlaneValue);
                if(ccw) {
                    put(LocationOnPlane.X + 1, LocationOnPlane.Y, PlaneValue);
                    put(LocationOnPlane.X + 1, LocationOnPlane.Y + 1, PlaneValue);
                    put(LocationOnPlane.X, LocationOnPlane.Y + 1, PlaneValue);
                } else {
                    put(LocationOnPlane.X, LocationOnPlane.Y + 1, PlaneValue);
                    put(LocationOnPlane.X + 1, LocationOnPlane.Y + 1, PlaneValue);
                    put(LocationOnPlane.X + 1, LocationOnPlane.Y, PlaneValue);
                }
            } else if(NormalAxis == Axis.Y) {
                put(LocationOnPlane.Y, PlaneValue, LocationOnPlane.X);
                if(ccw) {
                    put(LocationOnPlane.Y, PlaneValue, LocationOnPlane.X + 1);
                    put(LocationOnPlane.Y + 1, PlaneValue, LocationOnPlane.X + 1);
                    put(LocationOnPlane.Y + 1, PlaneValue, LocationOnPlane.X);
                } else {
                    put(LocationOnPlane.Y + 1, PlaneValue, LocationOnPlane.X);
                    put(LocationOnPlane.Y + 1, PlaneValue, LocationOnPlane.X + 1);
                    put(LocationOnPlane.Y, PlaneValue, LocationOnPlane.X + 1);
                }
            } else {
                put(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y);
                if(ccw) {
                    put(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y);
                    put(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y + 1);
                    put(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y + 1);
                } else {
                    put(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y + 1);
                    put(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y + 1);
                    put(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y);
                }
            }
            return offset;
        }
        public int FillTriVerts(float[] vbuffer, float[] nbuffer, int offset) { // fill vertex buffer with 6 verts (18 floats)
            throw new NotSupportedException();
        }
    }

    public class Edge {
        public Axis Axis { get; set; }
        public Quad P { get; set; }
        public Quad Q { get; set; }
        public int Angle { get; set; }
        public int PValue { get; set; }
        public int QValue { get; set; }
    }

    public class QuadMap {
        private List<Quad> quads = new List<Quad>();
        public IList<Quad> GetAllQuads() { // all (renderable) quads
            return quads.AsReadOnly();
        }
        public IList<Quad> GetFreeQuads() { // unoccupied valid quads only
            throw new NotImplementedException();
        }
        public void Build(IBinaryVolume volume) {
            BuildQuads(volume);
            BuildEdges(volume);
        }
        private void BuildQuads(IBinaryVolume volume) {
            quads.Clear();
            for(var z = volume.Bounds.Zmin; z <= volume.Bounds.Zmax; z++) {
                for(var y = volume.Bounds.Ymin; y <= volume.Bounds.Ymax; y++) {
                    for(var x = volume.Bounds.Xmin; x <= volume.Bounds.Xmax; x++) {
                        bool v = volume.IsOccupied(x, y, z);
                        bool e = volume.IsOccupied(x, y, z - 1);
                        if(v && !e) {
                            AddQuad(new Quad() { NormalAxis = Axis.Z, NormalIsNegative = true, PlaneValue = z, LocationOnPlane = new VectorI2D(x, y) });
                        } else if(!v && e) {
                            AddQuad(new Quad() { NormalAxis = Axis.Z, NormalIsNegative = false, PlaneValue = z, LocationOnPlane = new VectorI2D(x, y) });
                        }
                        e = volume.IsOccupied(x, y - 1, z);
                        if(v && !e) {
                            AddQuad(new Quad() { NormalAxis = Axis.Y, NormalIsNegative = true, PlaneValue = y, LocationOnPlane = new VectorI2D(z, x) });
                        } else if(!v && e) {
                            AddQuad(new Quad() { NormalAxis = Axis.Y, NormalIsNegative = false, PlaneValue = y, LocationOnPlane = new VectorI2D(z, x) });
                        }
                        e = volume.IsOccupied(x - 1, y, z);
                        if(v && !e) {
                            AddQuad(new Quad() { NormalAxis = Axis.X, NormalIsNegative = true, PlaneValue = x, LocationOnPlane = new VectorI2D(y, z) });
                        } else if(!v && e) {
                            AddQuad(new Quad() { NormalAxis = Axis.X, NormalIsNegative = false, PlaneValue = x, LocationOnPlane = new VectorI2D(y, z) });
                        }
                    }
                }
            }
        }
        private void AddQuad(Quad quad) {
            quads.Add(quad);
        }
        private void BuildEdges(IBinaryVolume volume) {
        }
    }
}
