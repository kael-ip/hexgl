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
        private Edge[] edges = new Edge[6]; // TODO: 4 is enough!
        public Edge GetEdge(Axis axis, bool neg) {
            if(axis == NormalAxis)
                return null;
            var index = (int)axis + (neg ? 3 : 0);
            return edges[index];
        }
        public void SetEdge(Axis axis, bool neg, Edge edge) {
            //if(axis == NormalAxis)
            //    throw new NotSupportedException();
            var index = (int)axis + (neg ? 3 : 0);
            if(edges[index] != null)
                throw new Exception("Edge already set");
            edges[index] = edge;
        }
        public VectorI3D Location {
            get {
                switch(NormalAxis) {
                    case Axis.X:
                        return new VectorI3D(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y);
                    case Axis.Y:
                        return new VectorI3D(LocationOnPlane.Y, PlaneValue, LocationOnPlane.X);
                    case Axis.Z:
                        return new VectorI3D(LocationOnPlane.X, LocationOnPlane.Y, PlaneValue);
                }
                return default(VectorI3D);
            }
        }
        public Quad() {
        }
        public static bool ccwFront = false;
        public int FillQuadVerts(float[] vbuffer, float[] nbuffer, int offset) { // fill vertex buffer with 4 verts (12 floats)
            int a = (int)NormalAxis;
            Action<int, int, int> puta = (x, y, z) => {
                vbuffer[offset + (0 + a) % 3] = x;
                vbuffer[offset + (1 + a) % 3] = y;
                vbuffer[offset + (2 + a) % 3] = z;
                float av = NormalIsNegative ? -1.0f : 1.0f;
                nbuffer[offset + 0] = (a == 0) ? av : 0;
                nbuffer[offset + 1] = (a == 1) ? av : 0;
                nbuffer[offset + 2] = (a == 2) ? av : 0;
                offset += 3;
            };
            var ccw = ccwFront ? NormalIsNegative : !NormalIsNegative;
            puta(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y);
            if(ccw) {
                puta(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y);
            } else {
                puta(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y + 1);
            }
            puta(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y + 1);
            if(ccw) {
                puta(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y + 1);
            } else {
                puta(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y);
            }
            return offset;
        }
        public int FillTriVerts(float[] vbuffer, float[] nbuffer, int offset) { // fill vertex buffer with 6 verts (18 floats)
            throw new NotSupportedException();
        }
    }

    public class Edge {
        public Axis Axis { get; set; }
        public int Angle { get; set; } //unsigned (when axis points outside, CW is positive for Q0->Q1)
        public Quad Q0 { get; set; }
        public Quad Q1 { get; set; }
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
                            AddQuad(Axis.Z, true, z, x, y);
                        } else if(!v && e) {
                            AddQuad(Axis.Z, false, z, x, y);
                        }
                        e = volume.IsOccupied(x, y - 1, z);
                        if(v && !e) {
                            AddQuad(Axis.Y, true, y, z, x);
                        } else if(!v && e) {
                            AddQuad(Axis.Y, false, y, z, x);
                        }
                        e = volume.IsOccupied(x - 1, y, z);
                        if(v && !e) {
                            AddQuad(Axis.X, true, x, y, z);
                        } else if(!v && e) {
                            AddQuad(Axis.X, false, x, y, z);
                        }
                    }
                }
            }
        }
        private Quad AddQuad(Axis normalAxis, bool normalIsNegative, int planeValue, int x, int y) {
            var quad = new Quad() { NormalAxis = normalAxis, NormalIsNegative = normalIsNegative, PlaneValue = planeValue, LocationOnPlane = new VectorI2D(x, y) };
            quads.Add(quad);
            return quad;
        }
        private void BuildEdges(IBinaryVolume volume) {
        }

        public int CheckConnectivity() {
            var groups = new int[quads.Count];
            int currentGroup = 0;
            for(int i = 0; i < quads.Count; i++) {
                if(groups[i] == 0) {
                    currentGroup++;
                    Collect(quads[i], groups, currentGroup);
                }
            }
            return currentGroup;
        }
        private void Collect(Quad quad, int[] groups, int currentGroup) {
            var i = quads.IndexOf(quad);
            if(groups[i] == currentGroup)
                return;
            if(groups[i] != 0)
                throw new Exception("Ambiguous quad0 group");
            groups[i] = currentGroup;
            Collect(quad.GetEdge(Axis.X, false), quad, groups, currentGroup);
            Collect(quad.GetEdge(Axis.X, true), quad, groups, currentGroup);
            Collect(quad.GetEdge(Axis.Y, false), quad, groups, currentGroup);
            Collect(quad.GetEdge(Axis.Y, true), quad, groups, currentGroup);
            Collect(quad.GetEdge(Axis.Z, false), quad, groups, currentGroup);
            Collect(quad.GetEdge(Axis.Z, true), quad, groups, currentGroup);
        }
        private void Collect(Edge edge, Quad quad, int[] groups, int currentGroup) {
            if(edge == null)
                return;
            Quad q = edge.Q0 == quad ? edge.Q1 : edge.Q0;
            Collect(q, groups, currentGroup);
        }
    }
}
