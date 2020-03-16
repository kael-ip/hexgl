using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {

    struct Vertex {
        public float x, y, z;
        public override string ToString() {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }
    struct PolyVertex {
        public int vi, ni;
        public override string ToString() {
            return string.Format("(vi={0}, ni={1})", vi, ni);
        }
    }
    public class Geom {
        List<Vertex> vb = new List<Vertex>();
        List<Vertex> nb = new List<Vertex>();
        List<PolyVertex[]> ib = new List<PolyVertex[]>();
        public int PolyCount { get { return ib.Count; } }
        public int AddVertex(float x, float y, float z) {
            return AddVertex(vb, x, y, z);
        }
        public int AddNormal(float x, float y, float z) {
            return AddVertex(nb, x, y, z);
        }
        private int AddVertex(List<Vertex> b, float x, float y, float z) {
            int idx;
            for(idx = 0; idx < b.Count; idx++) {
                var v = b[idx];
                if(v.x == x && v.y == y && v.z == z) {
                    return idx;
                }
            }
            b.Add(new Vertex() { x = x, y = y, z = z });
            return idx;
        }
        public int AddTriangle(int v0, int v1, int v2) {
            return AddPoly(0, v0, v1, v2);
        }
        public int AddTriangle(int v0, int v1, int v2, int nv) {
            return AddPoly(nv, v0, v1, v2);
        }
        public int AddTriangle(int v0, int v1, int v2, int nv0, int nv1, int nv2) {
            var poly = new PolyVertex[3];
            poly[0].vi = v0;
            poly[1].vi = v1;
            poly[2].vi = v2;
            poly[0].ni = nv0;
            poly[1].ni = nv1;
            poly[2].ni = nv2;
            ib.Add(poly);
            return ib.Count - 1;
        }
        public int AddPoly(int nv, params int[] va) {
            var poly = new PolyVertex[va.Length];
            for(int i = 0; i < va.Length; i++) {
                poly[i].vi = va[i];
                poly[i].ni = nv;
            }
            ib.Add(poly);
            return ib.Count - 1;
        }
        private void AddPoly(PolyVertex[] poly) {
            ib.Add(poly);
        }
        private void FillVertex(List<float> vbi, List<Vertex> b, int idx) {
            var vertex = b[idx];
            vbi.Add(vertex.x);
            vbi.Add(vertex.y);
            vbi.Add(vertex.z);
        }
        public int Fill(out float[] vbuffer, out float[] nbuffer) {
            var vbi = new List<float>();
            var nbi = new List<float>();
            int n = 0;
            for(var i = 0; i < ib.Count; i++) {
                var poly = ib[i];
                if(i == 0) {
                    n = poly.Length;
                } else {
                    System.Diagnostics.Debug.Assert(n == poly.Length, "Ragged primitives");
                }
                for(var j = 0; j < n; j++) {
                    FillVertex(vbi, vb, poly[j].vi);
                    FillVertex(nbi, nb, poly[j].ni);
                }
            }
            vbuffer = vbi.ToArray();
            nbuffer = nbi.ToArray();
            return n;
        }
        public Geom CoalescePolys() {
            var g = new Geom();
            bool[] used = new bool[ib.Count];
            for(int i = 0; i < ib.Count; i++) {
                if(used[i])
                    continue;
                used[i] = true;
                var poly = new List<PolyVertex>(ib[i]);
                var cs0 = CrossProduct(poly[0], poly[1], poly[2]);
                bool cs0IsNegative = cs0 < 0;
                for(int pvi = 0; pvi < poly.Count; pvi++) {
                    var pv = poly[pvi];
                    for(int j = i + 1; j < ib.Count; j++) {
                        if(used[j])
                            continue;
                        var poly2 = ib[j];
                        for(int p2vi = 0; p2vi < poly2.Length; p2vi++) {
                            var p2v = poly2[p2vi];
                            if(pv.vi == p2v.vi && pv.ni == p2v.ni) {//coplanar and touching
                                var pvnext = poly[(pvi + 1) % poly.Count];
                                var p2vprev = poly2[(p2vi + poly2.Length - 1) % poly2.Length];
                                if(pvnext.vi == p2vprev.vi) {//assume normals are per face
                                    System.Diagnostics.Debug.Assert(pvnext.ni == p2vprev.ni, "Different normals");
                                    //rpoly.Add(poly2[(p2vi + 1) % poly2.Length]);
                                    poly.Insert(pvi + 1, poly2[(p2vi + 1) % poly2.Length]);
                                    used[j] = true;
                                }
                            }
                        }
                    }
                }
                //remove duplicated verts
                bool retry = true;
                while(retry) {
                    retry = false;
                    for(int pvi = 0; pvi < poly.Count; pvi++) {
                        int inext = (pvi + 1) % poly.Count;
                        int iprev = (pvi + poly.Count - 1) % poly.Count;
                        if(poly[inext].vi == poly[iprev].vi) {
                            poly.RemoveAt(Math.Max(inext, pvi));
                            poly.RemoveAt(Math.Min(inext, pvi));
                            retry = true;
                            break;
                        }
                    }
                }
                //remove mid verts
                for(int pvi = 0; pvi < poly.Count; pvi++) {
                    int inext = (pvi + 1) % poly.Count;
                    int iprev = (pvi + poly.Count - 1) % poly.Count;
                    if(CrossProduct(poly[iprev], poly[pvi], poly[inext]) == 0) {
                        poly.RemoveAt(pvi);
                        pvi--;
                    }
                }
                //intern
                var rpoly = new PolyVertex[poly.Count];
                for(int pvi = 0; pvi < poly.Count; pvi++) {
                    var v = vb[poly[pvi].vi];
                    var nv = nb[poly[pvi].ni];
                    var rvi = g.AddVertex(v.x, v.y, v.z);
                    var rni = g.AddNormal(nv.x, nv.y, nv.z);
                    rpoly[pvi].vi = rvi;
                    rpoly[pvi].ni = rni;
                }
                //g.AddPoly(rpoly);
                //re-triangulate
                for(int pvi = 0; pvi < rpoly.Length - 2; pvi++) {
                    var vs = rpoly[0];
                    var v0 = rpoly[pvi];
                    var v1 = rpoly[pvi + 1];
                    var v2 = rpoly[pvi + 2];
                    var cp = g.CrossProduct(v0, v1, v2);
                    System.Diagnostics.Debug.Assert(cp != 0, "Empty triangle");
                    if(cp < 0 && !cs0IsNegative || cp > 0 && cs0IsNegative) {
                        //???
                    } else {
                        g.AddTriangle(vs.vi, v1.vi, v2.vi, vs.ni, v1.ni, v2.ni);
                    }
                }
            }
            return g;
        }
        private float CrossProduct(PolyVertex pv0, PolyVertex pv1, PolyVertex pv2) {
            var v0 = vb[pv0.vi];
            var v1 = vb[pv1.vi];
            var v2 = vb[pv2.vi];
            var a = new Vertex() { x = v1.x - v0.x, y = v1.y - v0.y, z = v1.z - v0.z };
            var b = new Vertex() { x = v1.x - v2.x, y = v1.y - v2.y, z = v1.z - v2.z };
            var r = (a.x * b.y - a.y * b.x) + (a.y * b.z - a.z * b.y) + (a.z * b.x - a.x * b.z);
            return r;
        }
    }
}
