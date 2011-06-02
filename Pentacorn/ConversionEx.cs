using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;

namespace Pentacorn
{
    static class ConversionEx
    {
        public static Vector3 ToVector3(this Matrix<double> that)
        {
            if (that.Height > 3)
            {
                var w = that[3, 0];
                if (Math.Abs(w) > double.Epsilon)
                    return new Vector3((float)(that[0, 0] / w), (float)(that[1, 0] / w), (float)(that[2, 0] / w));
            }
            
            return new Vector3((float)that[0, 0], (float)that[1, 0], (float)that[2, 0]);
        }

        public static Vector2 ToVector2(this MCvPoint2D64f that)
        {
            return new Vector2((float)that.x, (float)that.y);
        }

        public static Matrix ToMatrixFromHomogeneous3x3(this Matrix<double> m) { return m.Convert<float>().ToMatrixFromHomogeneous3x3(); }
        public static Matrix ToMatrixFromHomogeneous3x3(this Matrix<float> m)
        {
            if (m.Height != 3 && m.Width != 3)
                throw new ArgumentException();

            return new Matrix(m[0, 0], m[1, 0], 0.0f, m[2, 0],
                              m[0, 1], m[1, 1], 0.0f, m[2, 1],
                                 0.0f,    0.0f, 1.0f, 0.0f,
                              m[0, 2], m[1, 2], 0.0f, 1.0f);
        }

        public static Matrix ToMatrixFrom3x3(this Matrix<double> m) { return m.Convert<float>().ToMatrixFrom3x3(); }
        public static Matrix ToMatrixFrom3x3(this Matrix<float> m)
        {
            if (m.Height != 3 && m.Width != 3)
                throw new ArgumentException();

            return new Matrix(m[0, 0], m[1, 0], m[2, 0], 0.0f, 
                              m[0, 1], m[1, 1], m[2, 1], 0.0f, 
                              m[0, 2], m[1, 2], m[2, 2], 0.0f,
                                 0.0f,    0.0f,    0.0f, 1.0f);
        }

        public static Matrix ToMatrixFrom3x4(this Matrix<double> m) { return m.Convert<float>().ToMatrixFrom3x4(); }
        public static Matrix ToMatrixFrom3x4(this Matrix<float> m)
        {
            if (m.Height != 3 && m.Width != 4)
                throw new ArgumentException();

            return new Matrix(m[0, 0], m[1, 0], m[2, 0], 0,
                              m[0, 1], m[1, 1], m[2, 1], 0,
                              m[0, 2], m[1, 2], m[2, 2], 0,
                              m[0, 3], m[1, 3], m[2, 3], 1);
        }

        public static Matrix<double> FromMatrixTo3x4(this Matrix m)
        {
            return new Matrix<double>(new double[,] { { m.M11, m.M21, m.M31, m.M41 },
                                                      { m.M12, m.M22, m.M32, m.M42 },
                                                      { m.M13, m.M23, m.M33, m.M43 }, });
        }

        public static Matrix<double> FromMatrixTo4x4(this Matrix m)
        {
            return new Matrix<double>(new double[,] { { m.M11, m.M21, m.M31, m.M41 },
                                                      { m.M12, m.M22, m.M32, m.M42 },
                                                      { m.M13, m.M23, m.M33, m.M43 },
                                                      { m.M14, m.M24, m.M34, m.M44 }, });
        }

        public static Matrix ToMatrix(this Matrix<double> m) { return m.Convert<float>().ToMatrix(); } 
        public static Matrix ToMatrix(this Matrix<float> m)
        {
            if (m.Height != 4 && m.Width != 4)
                throw new ArgumentException();

            return new Matrix((float)m[0, 0], (float)m[1, 0], (float)m[2, 0], (float)m[3, 0],
                              (float)m[0, 1], (float)m[1, 1], (float)m[2, 1], (float)m[3, 1],
                              (float)m[0, 2], (float)m[1, 2], (float)m[2, 2], (float)m[3, 2],
                              (float)m[0, 3], (float)m[1, 3], (float)m[2, 3], (float)m[3, 3]);
        }

        public static System.Drawing.PointF ToPointF(this Vector2 that) { return new System.Drawing.PointF(that.X, that.Y); }
        public static System.Drawing.PointF ToPointF(this Vector3 that) { return new System.Drawing.PointF(that.X, that.Y); }

        public static Vector2 ToVector2(this               Vector3 that) { return new Microsoft.Xna.Framework.Vector2(that.X, that.Y); }
        public static Vector2 ToVector2(this System.Drawing.Point  that) { return new Microsoft.Xna.Framework.Vector2(that.X, that.Y); }
        public static Vector2 ToVector2(this System.Drawing.PointF that) { return new Microsoft.Xna.Framework.Vector2(that.X, that.Y); }
        public static Vector3 ToVector3(this System.Drawing.PointF that, float z = 0) { return new Microsoft.Xna.Framework.Vector3(that.X, that.Y, z); }

        public static MCvPoint3D32f ToMCvPoint3D32f(this System.Drawing.PointF that) { return new MCvPoint3D32f(that.X, that.Y, 0); }
    }
}
