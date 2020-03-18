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
    //struct PolyVertex {
    //    public int vi, ni;
    //    public override string ToString() {
    //        return string.Format("(vi={0}, ni={1})", vi, ni);
    //    }
    //}
    class Polygon {
        public List<int> vs { get; private set; }
        public int ni;
        public int tag;
        public Polygon(int ni, params int[] vs) {
            this.ni = ni;
            this.vs = new List<int>(vs);
        }
        public Polygon(Polygon p) {
            this.ni = p.ni;
            this.vs = new List<int>(p.vs);
            this.tag = p.tag;
        }
        public int SafeGet(int index) {
            while(index < 0)
                index += vs.Count;
            while(index >= vs.Count)
                index -= vs.Count;
            return vs[index];
        }
        public override string ToString() {
            return string.Format("(ni={1}, vi={0})", string.Join(",", vs), ni);
        }
    }
    public class Geom {
        List<Vertex> vb = new List<Vertex>();
        List<Vertex> nb = new List<Vertex>();
        List<Polygon> ib = new List<Polygon>();
        public int PolyCount { get { return ib.Count; } }
        public int GetTrisCount() {
            return ib.Sum(p => p.vs.Count - 2);
        }
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
        private int currentTag;
        public void SetPolyTag(int tag) {
            this.currentTag = tag;
        }
        public int AddTriangle(int v0, int v1, int v2) {
            return AddPoly(CalcNormal(v0, v1, v2), v0, v1, v2);
        }
        private int CalcNormal(int v0, int v1, int v2) {
            throw new NotImplementedException();
        }
        public int AddPoly(int nv, params int[] va) {
            var p = new Polygon(nv, va);
            p.tag = currentTag;
            ib.Add(p);
            return ib.Count - 1;
        }
        private static void FillVertex(List<float> list, List<Vertex> b, int idx) {
            var vertex = b[idx];
            list.Add(vertex.x);
            list.Add(vertex.y);
            list.Add(vertex.z);
        }
        private void FillVertex(List<float> vblist, List<float> nblist, int vi, int ni) {
            FillVertex(vblist, vb, vi);
            FillVertex(nblist, nb, ni);
        }
        public int Fill(out float[] vbuffer, out float[] nbuffer, IList<int> taglist) {
            // triangulate polys
            var vblist = new List<float>();
            var nblist = new List<float>();
            for(var i = 0; i < ib.Count; i++) {
                var poly = ib[i];
                var vz = poly.vs[0];
                for(var j = 2; j < poly.vs.Count; j++) {
                    FillVertex(vblist, nblist, vz, poly.ni);
                    FillVertex(vblist, nblist, poly.vs[j - 1], poly.ni);
                    FillVertex(vblist, nblist, poly.vs[j], poly.ni);
                    if(taglist != null) {
                        taglist.Add(poly.tag);
                    }
                }
            }
            vbuffer = vblist.ToArray();
            nbuffer = nblist.ToArray();
            return 3;
        }
        private bool TryCoalesce(Polygon poly, Polygon poly2, float n) {
            for(int pvi = 0; pvi < poly.vs.Count; pvi++) {
                var pv = poly.vs[pvi];
                for(int p2vi = 0; p2vi < poly2.vs.Count; p2vi++) {
                    var p2v = poly2.vs[p2vi];
                    if(pv == p2v) { // touching
                        var pvnext = poly.SafeGet(pvi + 1);
                        var p2vprev = poly2.SafeGet(p2vi - 1);
                        if(pvnext == p2vprev) { // anti-collinear matching edges
                            var pvprev = poly.SafeGet(pvi - 1);
                            var pvnextnext = poly.SafeGet(pvi + 2);
                            var p2vnext = poly2.SafeGet(p2vi + 1);
                            var p2vprevprev = poly2.SafeGet(p2vi - 2);
                            var cpa = CalcNormalValue(pvprev, pv, p2vnext);
                            var cpb = CalcNormalValue(p2vprevprev, pvnext, pvnextnext);
                            if(IsConvex(n, cpa) && IsConvex(n, cpb)) { // both resulting corners are not concave
                                //merge polys
                                for(var k = 0; k < poly2.vs.Count - 2; k++) {
                                    poly.vs.Insert((pvi + 1 + k),
                                        poly2.vs[(p2vi + 1 + k) % poly2.vs.Count]);
                                }
                                if(cpa == 0) { //remove flat vertex
                                    poly.vs.Remove(pv);
                                }
                                if(cpb == 0) { //remove flat vertex
                                    poly.vs.Remove(pvnext);
                                }
                                System.Diagnostics.Debug.Assert(IsConvex(poly), string.Format("Not convex: {0}", poly));
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool TryCoalesce(List<Polygon> polys) {
            bool success = false;
            for(int i = 0; i < polys.Count; i++) {
                var poly = polys[i];
                var n = GetNormalValue(poly.ni);
                for(int j = i + 1; j < polys.Count; j++) {
                    var poly2 = polys[j];
                    if(poly2.ni != poly.ni)
                        continue; // not coplanar
                    if(TryCoalesce(poly, poly2, n)) {
                        polys.RemoveAt(j);
                        j--;
                        success = true;
                    }
                }
            }
            return success;
        }
        private bool IsConvex(float n, float nv) {
            return (nv == 0 || nv < 0 && n < 0 || nv > 0 && n > 0);
        }
        private bool IsConvex(float n, int v0, int v1, int v2) {
            var nv = CalcNormalValue(v0, v1, v2);
            return IsConvex(n, nv);
        }
        private bool IsConvex(Polygon p) {
            var n = GetNormalValue(p.ni);
            for(int i = 0; i < p.vs.Count; i++) {
                var v0 = p.vs[i];
                var v1 = p.SafeGet(i + 1);
                var v2 = p.SafeGet(i + 2);
                var nv = CalcNormalValue(v0, v1, v2);
                if(!IsConvex(n, v0, v1, v2))
                    return false;
            }
            return true;
        }
        public Geom CoalescePolys() {
            var polys = new List<Polygon>();
            // validate all polys are convex
            foreach(var p in ib) {
                System.Diagnostics.Debug.Assert(IsConvex(p), string.Format("Not convex: {0}", p));
                polys.Add(new Polygon(p));
            }
            // coalesce all
            while(TryCoalesce(polys)) { }
            // output removing unused verts
            var g = new Geom();
            foreach(var p in polys) {
                var nv = nb[p.ni];
                var rni = g.AddNormal(nv.x, nv.y, nv.z);
                var rvs = new List<int>();
                for(int pvi = 0; pvi < p.vs.Count; pvi++) {
                    var v = vb[p.vs[pvi]];
                    var rvi = g.AddVertex(v.x, v.y, v.z);
                    rvs.Add(rvi);
                }
                g.SetPolyTag(p.tag);
                g.AddPoly(rni, rvs.ToArray());
            }
            return g;
        }
        private Vertex CrossProduct(int vi0, int vi1, int vi2) {
            var v0 = vb[vi0];
            var v1 = vb[vi1];
            var v2 = vb[vi2];
            var b = new Vertex() { x = v1.x - v0.x, y = v1.y - v0.y, z = v1.z - v0.z };
            var a = new Vertex() { x = v1.x - v2.x, y = v1.y - v2.y, z = v1.z - v2.z };
            return new Vertex() { x = (a.y * b.z - a.z * b.y), y = (a.z * b.x - a.x * b.z), z = (a.x * b.y - a.y * b.x) };
        }
        public float CalcNormalValue(int vi0, int vi1, int vi2) {
            var v = CrossProduct(vi0, vi1, vi2);
            return v.x + v.y + v.z;
        }
        private float GetNormalValue(int ni) {
            var n = nb[ni];
            return n.x + n.y + n.z;
        }
    }
}
