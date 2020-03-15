using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {

    struct Vertex {
        public float x, y, z;
    }
    struct PolyVertex {
        public int vi, ni;
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
                    System.Diagnostics.Debug.Assert(n == poly.Length);
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
    }
}
