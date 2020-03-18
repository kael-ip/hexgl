using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {

    public struct Bounds3D {
        public int Xmin;
        public int Xmax;
        public int Ymin;
        public int Ymax;
        public int Zmin;
        public int Zmax;
        public Bounds3D(int Xmin, int Xmax, int Ymin, int Ymax, int Zmin, int Zmax) {
            this.Xmin = Xmin;
            this.Xmax = Xmax;
            this.Ymin = Ymin;
            this.Ymax = Ymax;
            this.Zmin = Zmin;
            this.Zmax = Zmax;
        }
        public override string ToString() {
            return string.Format("({{{0}, {1}, {2}}}, {{{3}, {4}, {5}}})", Xmin, Ymin, Zmin, Xmax, Ymax, Zmax);
        }
    }

    public interface IBinaryVolume {
        Bounds3D Bounds { get; }
        bool IsOccupied(int x, int y, int z);
    }

    public interface IBinaryVolumeWritable : IBinaryVolume {
        void SetIsOccupied(int x, int y, int z, bool yes);
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
        public void Offset(Axis axis, int value) {
            switch(axis) {
                case Axis.X:
                    X += value;
                    break;
                case Axis.Y:
                    Y += value;
                    break;
                case Axis.Z:
                    Z += value;
                    break;
            }
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
            if(axis == NormalAxis)
                throw new NotSupportedException();
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
        public override string ToString() {
            return string.Format("{1}{0} {2}:{3} {4}", NormalAxis, NormalIsNegative ? "-" : "+", PlaneValue, LocationOnPlane, Location);
        }
        internal int Debug_EdgeCount { get { return edges.Count(e => e != null); } }
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
        public void FillQuadVerts(Geom geom) {
            int nv;
            int[] vis;
            FillVerts(geom, out nv, out vis);
            geom.AddPoly(nv, vis);
        }
        public void FillTriVerts(Geom geom) {
            int nv;
            int[] vis;
            FillVerts(geom, out nv, out vis);
            geom.AddPoly(nv, vis[0], vis[1], vis[2]);
            geom.AddPoly(nv, vis[0], vis[2], vis[3]);
        }
        public void FillVerts(Geom geom, out int nv, out int[] vis) {
            int a = (int)NormalAxis;
            Func<int, int, int, int> putv = (x, y, z) => {
                float[] xyz = new float[3];
                xyz[(0 + a) % 3] = x;
                xyz[(1 + a) % 3] = y;
                xyz[(2 + a) % 3] = z;
                return geom.AddVertex(xyz[0], xyz[1], xyz[2]);
            };
            var ccw = ccwFront ? NormalIsNegative : !NormalIsNegative;
            vis = new int[4];
            vis[0] = putv(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y);
            if(ccw) {
                vis[1] = putv(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y);
            } else {
                vis[1] = putv(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y + 1);
            }
            vis[2] = putv(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y + 1);
            if(ccw) {
                vis[3] = putv(PlaneValue, LocationOnPlane.X, LocationOnPlane.Y + 1);
            } else {
                vis[3] = putv(PlaneValue, LocationOnPlane.X + 1, LocationOnPlane.Y);
            }
            float av = NormalIsNegative ? -1.0f : 1.0f;
            nv = geom.AddNormal((a == 0) ? av : 0, (a == 1) ? av : 0, (a == 2) ? av : 0);
        }
    }

    public class Edge {
        public Axis Axis { get; set; }
        public int Angle { get; set; } //unsigned (when axis points outside, CW is positive for Q0->Q1)
        public Quad Q0 { get; set; }
        public Quad Q1 { get; set; }
        public override string ToString() {
            return string.Format("{0}{1} ({2})->({3})", Axis, Angle, Q0, Q1);
        }
    }

    public class QuadMap {
        private List<Quad> quads = new List<Quad>();
        public IList<Quad> Quads { get { return quads.AsReadOnly(); } }
        public void Build(IBinaryVolume volume) {
            BuildQuads(volume);
            BuildEdges();
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
        private void BuildEdges() {
            foreach(var quad in quads) {
                Axis ax = (Axis)(((int)quad.NormalAxis + 1) % 3);
                Axis ay = (Axis)(((int)quad.NormalAxis + 2) % 3);
                bool nn = quad.NormalIsNegative;
                int px = quad.LocationOnPlane.X;
                int py = quad.LocationOnPlane.Y;
                int pi = nn ? 1 : 0;
                TryConnect(quad, !nn, ay, 2, ax, nn, ax, !nn, quad.NormalAxis, nn, quad.PlaneValue, px + 1, py);
                TryConnect(quad, nn, ay, 2, ax, nn, ax, !nn, quad.NormalAxis, nn, quad.PlaneValue, px - 1, py);
                TryConnect(quad, !nn, ax, 2, ay, !nn, ay, nn, quad.NormalAxis, nn, quad.PlaneValue, px, py - 1);
                TryConnect(quad, nn, ax, 2, ay, !nn, ay, nn, quad.NormalAxis, nn, quad.PlaneValue, px, py + 1);
                // 108
                if(!quad.NormalIsNegative) {
                    TryConnect(quad, false, ay, 1, quad.NormalAxis, true, ax, true, ax, false, px, py, quad.PlaneValue);//left inner 82 >*
                    TryConnect(quad, true, ay, 1, ax, false, quad.NormalAxis, true, ax, true, px + 1, py, quad.PlaneValue);//right inner 78 (+78 >48)
                    TryConnect(quad, false, ax, 1, quad.NormalAxis, true, ay, false, ay, true, py + 1, quad.PlaneValue, px);//top inner 78 (+78 >48)
                    TryConnect(quad, true, ax, 1, ay, true, quad.NormalAxis, true, ay, false, py, quad.PlaneValue, px);//bottom inner 82 >*

                    TryConnect(quad, false, ay, 3, quad.NormalAxis, false, ax, true, ax, true, px, py, quad.PlaneValue - 1);//left outer 63 (+63 >25)
                    TryConnect(quad, true, ay, 3, ax, false, quad.NormalAxis, false, ax, false, px + 1, py, quad.PlaneValue - 1);//right outer 73 >*
                    TryConnect(quad, false, ax, 3, quad.NormalAxis, false, ay, false, ay, false, py + 1, quad.PlaneValue - 1, px);//top outer 73 >*
                    TryConnect(quad, true, ax, 3, ay, true, quad.NormalAxis, false, ay, true, py, quad.PlaneValue - 1, px);//bottom outer 63 (+63 >25)
                }// >10
                if(quad.NormalIsNegative) {// -X > ax=Y, ay=Z // !false > nn, !true > !nn
                    TryConnect(quad, false, ay, 1, quad.NormalAxis, false, ax, false, ax, true, px + 1, py, quad.PlaneValue - pi);//left inner 82 >*
                    TryConnect(quad, true, ay, 1, ax, true, quad.NormalAxis, false, ax, false, px, py, quad.PlaneValue - pi);//right inner 78 (+78 >48)
                    TryConnect(quad, false, ax, 1, quad.NormalAxis, false, ay, true, ay, false, py, quad.PlaneValue - pi, px);//bottom inner 78 (+78 >48)
                    TryConnect(quad, true, ax, 1, ay, false, quad.NormalAxis, false, ay, true, py + 1, quad.PlaneValue - pi, px);//top inner 82 >*

                    TryConnect(quad, false, ay, 3, quad.NormalAxis, true, ax, false, ax, false, px + pi, py, quad.PlaneValue);//left outer 63 (+63 >25)
                    TryConnect(quad, true, ay, 3, ax, true, quad.NormalAxis, true, ax, true, px, py, quad.PlaneValue);//right outer 73 >*
                    TryConnect(quad, false, ax, 3, quad.NormalAxis, true, ay, true, ay, true, py, quad.PlaneValue, px);//bottom outer 73 >*
                    TryConnect(quad, true, ax, 3, ay, false, quad.NormalAxis, true, ay, false, py + pi, quad.PlaneValue, px);//top outer 63 63 (+63 >25)
                }// >10
            }
        }
        private void TryConnect(Quad quad, bool is0, Axis edgeAxis, int edgeAngle, Axis dir0Axis, bool dir0Neg, Axis dir1Axis, bool dir1Neg, Axis qAxis, bool qNeg, int qLevel, int qU, int qV) {
            Quad other = null;
            if(is0) {
                if(quad.GetEdge(dir0Axis, dir0Neg) != null)
                    return;
                other = FindQuad(qAxis, qNeg, qLevel, qU, qV);
                if(other != null)
                    Connect(quad, other, edgeAxis, edgeAngle, dir0Axis, dir0Neg, dir1Axis, dir1Neg);
            } else {
                if(quad.GetEdge(dir1Axis, dir1Neg) != null)
                    return;
                other = FindQuad(qAxis, qNeg, qLevel, qU, qV);
                if(other != null)
                    Connect(other, quad, edgeAxis, edgeAngle, dir0Axis, dir0Neg, dir1Axis, dir1Neg);
            }
        }
        private Quad FindQuad(Axis qAxis, bool qNeg, int qLevel, int qU, int qV) {
            foreach(var quad in quads) {
                if(quad.NormalAxis == qAxis && quad.NormalIsNegative == qNeg && quad.PlaneValue == qLevel && quad.LocationOnPlane.X == qU && quad.LocationOnPlane.Y == qV)
                    return quad;
            }
            return null;
        }
        public void BuildPlane(int xsize, int ysize) {
            var row = new Quad[xsize];
            for(int y = 0; y < ysize; y++) {
                for(int x = 0; x < xsize; x++) {
                    var quad = AddQuad(Axis.Z, false, 0, x, y);
                    if(y > 0) {
                        var prev = row[x];
                        Connect(quad, prev, Axis.X, 2, Axis.Y, true, Axis.Y, false);
                    }
                    if(x > 0) {
                        var prev = row[x - 1];
                        Connect(prev, quad, Axis.Y, 2, Axis.X, true, Axis.X, false);
                    }
                    row[x] = quad;
                }
            }
        }
        public void BuildCube() {
            var quadN = AddQuad(Axis.Z, false, 1, 0, 0);
            var quadF = AddQuad(Axis.Z, true, 0, 0, 0);
            var quadT = AddQuad(Axis.Y, false, 1, 0, 0);
            var quadB = AddQuad(Axis.Y, true, 0, 0, 0);
            var quadR = AddQuad(Axis.X, false, 1, 0, 0);
            var quadL = AddQuad(Axis.X, true, 0, 0, 0);
            Connect(quadT, quadN, Axis.X, 3, Axis.Z, false, Axis.Y, false);
            Connect(quadF, quadT, Axis.X, 3, Axis.Y, false, Axis.Z, true);
            Connect(quadB, quadF, Axis.X, 3, Axis.Z, true, Axis.Y, true);
            Connect(quadN, quadB, Axis.X, 3, Axis.Y, true, Axis.Z, false);

            Connect(quadN, quadR, Axis.Y, 3, Axis.X, false, Axis.Z, false);
            Connect(quadR, quadF, Axis.Y, 3, Axis.Z, true, Axis.X, false);
            Connect(quadF, quadL, Axis.Y, 3, Axis.X, true, Axis.Z, true);
            Connect(quadL, quadN, Axis.Y, 3, Axis.Z, false, Axis.X, true);

            Connect(quadR, quadT, Axis.Z, 3, Axis.Y, false, Axis.X, false);
            Connect(quadT, quadL, Axis.Z, 3, Axis.X, true, Axis.Y, false);
            Connect(quadL, quadB, Axis.Z, 3, Axis.Y, true, Axis.X, true);
            Connect(quadB, quadR, Axis.Z, 3, Axis.X, false, Axis.Y, true);
        }
        private void Connect(Quad quad0, Quad quad1, Axis edgeAxis, int edgeAngle, Axis dir0Axis, bool dir0Neg, Axis dir1Axis, bool dir1Neg) {
            var edge = new Edge();
            edge.Axis = edgeAxis;
            edge.Angle = edgeAngle;
            edge.Q0 = quad0;
            edge.Q1 = quad1;
            quad0.SetEdge(dir0Axis, dir0Neg, edge);
            quad1.SetEdge(dir1Axis, dir1Neg, edge);
        }
        public void BuildHeightPlane(int[] map, int xsize, int ysize, int maskLevel, bool invy) {
            BuildHeightPlane(xsize, ysize, (x, y) => map[x + y * xsize], maskLevel, invy);
        }
        public void BuildHeightPlane(int xsize, int ysize, Func<int,int,int> source, int maskLevel, bool invy) {
            for(int y = 0; y < ysize; y++) {
                for(int x = 0; x < xsize; x++) {
                    var v = source(x, y);
                    if(v != maskLevel) {
                        var yy = invy ? ysize - y : y;
                        var quad = AddQuad(Axis.Z, false, v, x, yy);
                    }
                }
            }
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
            List<Quad>[] groupQuads = new List<Quad>[currentGroup];
            int[] qe = new int[7];
            for(int i = 0; i < quads.Count; i++) {
                var g = groupQuads[groups[i] - 1];
                if(g == null) {
                    g = new List<Quad>();
                    groupQuads[groups[i] - 1] = g;
                }
                var quad = quads[i];
                g.Add(quad);
                qe[quad.Debug_EdgeCount]++;
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
