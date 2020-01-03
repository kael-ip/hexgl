using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HexTex.OpenGL;
using NUnit.Framework;

namespace HexTex.Recuberation {
    [TestFixture]
    public class GLMathTests {
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(2.34f)]
        [TestCase(-2.34f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        [TestCase(4.56f)]
        [TestCase(-4.56f)]
        public void TestMatrixRotationX(float angle) {
            var smat = System.Numerics.Matrix4x4.CreateRotationX(angle).ToMatrix3x4Array();
            var tmat = new float[12];
            GLMath.Rotate3(tmat, angle, 1, 0, 0);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(2.34f)]
        [TestCase(-2.34f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        [TestCase(4.56f)]
        [TestCase(-4.56f)]
        public void TestMatrixRotationY(float angle) {
            var smat = System.Numerics.Matrix4x4.CreateRotationY(angle).ToMatrix3x4Array();
            var tmat = new float[12];
            GLMath.Rotate3(tmat, angle, 0, 1, 0);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(2.34f)]
        [TestCase(-2.34f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        [TestCase(4.56f)]
        [TestCase(-4.56f)]
        public void TestMatrixRotationZ(float angle) {
            var smat = System.Numerics.Matrix4x4.CreateRotationZ(angle).ToMatrix3x4Array();
            var tmat = new float[12];
            GLMath.Rotate3(tmat, angle, 0, 0, 1);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        public void TestMartixTranslation() {
            var smat = System.Numerics.Matrix4x4.CreateTranslation(3, 5, 7).ToMatrix3x4Array();
            var tmat = new float[12];
            GLMath.Translate3(tmat, 3, 5, 7);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        public void TestMartixMultiplicationZ1(float a) {
            float[] smat;
            {
                var m1 = System.Numerics.Matrix4x4.CreateTranslation(3, 5, 7);
                var m2 = System.Numerics.Matrix4x4.CreateRotationZ(a);
                var m = System.Numerics.Matrix4x4.Multiply(m1, m2);
                smat = m.ToMatrix3x4Array();
            }
            var tm1 = new float[12];
            var tm2 = new float[12];
            var tmat = new float[12];
            GLMath.Translate3(tm1, 3, 5, 7);
            GLMath.Rotate3(tm2, a, 0, 0, 1);
            GLMath.MatrixMultiply(tmat, tm1, tm2);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(2.34f)]
        [TestCase(-2.34f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        public void TestMartixMultiplicationZ2(float a) {
            float[] smat;
            {
                var m1 = System.Numerics.Matrix4x4.CreateTranslation(3, 5, 7);
                var m2 = System.Numerics.Matrix4x4.CreateRotationZ(a);
                var m = System.Numerics.Matrix4x4.Multiply(m2, m1);
                smat = m.ToMatrix3x4Array();
            }
            var tm1 = new float[12];
            var tm2 = new float[12];
            var tmat = new float[12];
            GLMath.Translate3(tm1, 3, 5, 7);
            GLMath.Rotate3(tm2, a, 0, 0, 1);
            GLMath.MatrixMultiply(tmat, tm2, tm1);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        public void TestMartixMultiplicationX1(float a) {
            float[] smat;
            {
                var m1 = System.Numerics.Matrix4x4.CreateTranslation(3, 5, 7);
                var m2 = System.Numerics.Matrix4x4.CreateRotationX(a);
                var m = System.Numerics.Matrix4x4.Multiply(m1, m2);
                smat = m.ToMatrix3x4Array();
            }
            var tm1 = new float[12];
            var tm2 = new float[12];
            var tmat = new float[12];
            GLMath.Translate3(tm1, 3, 5, 7);
            GLMath.Rotate3(tm2, a, 1, 0, 0);
            GLMath.MatrixMultiply(tmat, tm1, tm2);
            Assert.AreEqual(smat, tmat);
        }
        [Test]
        [TestCase(1.23f)]
        [TestCase(-1.23f)]
        [TestCase(3.45f)]
        [TestCase(-3.45f)]
        public void TestMartixMultiplicationX2(float a) {
            float[] smat;
            {
                var m1 = System.Numerics.Matrix4x4.CreateTranslation(3, 5, 7);
                var m2 = System.Numerics.Matrix4x4.CreateRotationX(a);
                var m = System.Numerics.Matrix4x4.Multiply(m2, m1);
                smat = m.ToMatrix3x4Array();
            }
            var tm1 = new float[12];
            var tm2 = new float[12];
            var tmat = new float[12];
            GLMath.Translate3(tm1, 3, 5, 7);
            GLMath.Rotate3(tm2, a, 1, 0, 0);
            GLMath.MatrixMultiply(tmat, tm2, tm1);
            Assert.AreEqual(smat, tmat);
        }

        
        [Test]
        [TestCase(1.23f, 1.23f)]
        [TestCase(-1.23f, 1.23f)]
        [TestCase(-1.23f, -1.23f)]
        [TestCase(1.23f, -1.23f)]
        [TestCase(1.23f, 3.45f)]
        [TestCase(3.45f, 1.23f)]
        [TestCase(-1.23f, -3.45f)]
        [TestCase(-3.45f, -1.23f)]
        [TestCase(-1.23f, 3.45f)]
        [TestCase(3.45f, -1.23f)]
        [TestCase(1.23f, -3.45f)]
        [TestCase(-3.45f, 1.23f)]
        public void TestMartixMultiplication3(float az, float ax) {
            float[] smat1;
            float[] smat2;
            {
                var m2 = System.Numerics.Matrix4x4.CreateRotationZ(az);
                var m3 = System.Numerics.Matrix4x4.CreateRotationX(ax);
                var m = System.Numerics.Matrix4x4.Multiply(m2, m3);
                smat1 = m.ToMatrix3x4Array();
                m = System.Numerics.Matrix4x4.Multiply(m3, m2);
                smat2 = m.ToMatrix3x4Array();
            }
            var tm1 = new float[12];
            var tm2 = new float[12];
            var tm3 = new float[12];
            var tmat1 = new float[12];
            var tmat2 = new float[12];
            GLMath.Rotate3(tm2, az, 0, 0, 1);
            GLMath.Rotate3(tm3, ax, 1, 0, 0);
            GLMath.MatrixMultiply(tmat1, tm2, tm3);
            GLMath.MatrixMultiply(tmat2, tm3, tm2);
            Assert.AreEqual(smat1, tmat1);
            Assert.AreEqual(smat2, tmat2);
        }
        /*
        [Test]
        [TestCase(1.23f, 1.23f)]
        [TestCase(-1.23f, 1.23f)]
        [TestCase(-1.23f, -1.23f)]
        [TestCase(1.23f, -1.23f)]
        [TestCase(1.23f, 3.45f)]
        [TestCase(3.45f, 1.23f)]
        [TestCase(-1.23f, -3.45f)]
        [TestCase(-3.45f, -1.23f)]
        [TestCase(-1.23f, 3.45f)]
        [TestCase(3.45f, -1.23f)]
        [TestCase(1.23f, -3.45f)]
        [TestCase(-3.45f, 1.23f)]
        public void TestMartixMultiplication3a(float az, float ax) {
            float[] smat1;
            float[] smat2;
            {
                var m1 = System.Numerics.Matrix4x4.CreateTranslation(3, 5, 7);
                var m2 = System.Numerics.Matrix4x4.CreateRotationZ(az);
                var m3 = System.Numerics.Matrix4x4.CreateRotationX(ax);
                var m = System.Numerics.Matrix4x4.Multiply(m1, m2);
                smat1 = m.ToMatrix3x4Array();
                m = System.Numerics.Matrix4x4.Multiply(m3, m2);
                smat2 = m.ToMatrix3x4Array();
            }
            var tm1 = new float[12];
            var tm2 = new float[12];
            var tm3 = new float[12];
            var tmat1 = new float[12];
            var tmat2 = new float[12];
            GLMath.Translate3(tm1, 3, 5, 7);
            GLMath.Rotate3(tm2, az, 0, 0, 1);
            GLMath.Rotate3(tm3, ax, 1, 0, 0);
            GLMath.MatrixMultiply(tmat1, tm1, tm2);
            GLMath.MatrixMultiply(tmat2, tm3, tm2);
            Assert.AreEqual(smat1, tmat1);
            Assert.AreEqual(smat2, tmat2);
        }*/
    }
}
