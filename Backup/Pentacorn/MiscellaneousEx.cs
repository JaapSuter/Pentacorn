using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;

namespace Pentacorn
{
    static class MiscelaneousEx
    {
        public static void Run<K, V>(this IDictionary<K, V> dict, Action<K, V> fun)
        {
            foreach (var kvp in dict)
                fun(kvp.Key, kvp.Value);
        }

        public static U[] ToArrayOf<T, U>(this IEnumerable<T> es, Func<T, U> conv)
        {
            return es.Select(e => conv(e)).ToArray();
        }

        public static T[] TakeQuadCorners<T>(this T[] es, Size dim)
        {
            Debug.Assert(es.Length == dim.Area());
            return new[] { es[0], es[dim.Width - 1], es[dim.Area() - dim.Width], es[dim.Area() - 1], };
        }

        public static float SquaredDistanceTo(this PointF that, PointF other)
        {
            var d = new PointF(other.X - that.X, other.Y - that.Y);
            return d.X * d.X + d.Y * d.Y;
        }

        public static float NextFloat(this Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static PointF GridOrigin(this PointF[] that, Size size) { return that[0]; }
        public static PointF GridAxisMostX(this PointF[] that, Size size) { return that[size.Width - 1]; }
        public static PointF GridAxisMostY(this PointF[] that, Size size) { return that[size.Area() - size.Width]; }
        public static PointF GridAxisMostBoth(this PointF[] that, Size size) { return that[size.Area() - 1]; }

        public static void ToCoordinateSystem(this ExtrinsicCameraParameters that, out Vector3 position, out Vector3 x, out Vector3 y, out Vector3 z)
        {
            var rot = that.RotationVector.RotationMatrix.ToMatrixFrom3x3();

            position = that.TranslationVector.ToVector3();
            x = rot.Left;
            y = rot.Up;
            z = rot.Forward;
        }

        static Vector3 sgn = new Vector3(1, 1, 1);
        static bool inv2 = false;
        static bool inv = false;
        static float rx = 1;
        static float ry = 1;
        static float rz = 1;

        public static Matrix ToViewMatrix(this ExtrinsicCameraParameters that)
        {
            // return that.ExtrinsicMatrix.ToMatrixFrom4x3();
            //*
            var position = that.TranslationVector.ToVector3() * sgn;
            var unrot = that.RotationVector.RotationMatrix.ToMatrixFrom3x3();

            if (inv)
                unrot = Matrix.Invert(unrot);

            unrot.M11 *= rx;
            unrot.M12 *= rx;
            unrot.M13 *= rx;
            unrot.M14 *= rx;
            unrot.M21 *= ry;
            unrot.M22 *= ry;
            unrot.M23 *= ry;
            unrot.M24 *= ry;
            unrot.M31 *= rz;
            unrot.M32 *= rz;
            unrot.M33 *= rz;
            unrot.M34 *= rz;

            var forward = Vector3.Transform(Vector3.Forward, unrot);
            var up = Vector3.Transform(Vector3.Up, unrot);

            var mat = Matrix.CreateWorld(position, forward, up);

            if (inv2)
                mat = Matrix.Invert(mat);

            return mat;
            // return unrot * Matrix.CreateTranslation(position);
            // return that.ExtrinsicMatrix.ToMatrixFrom4x3();*/
        }

        public static System.Drawing.PointF[] ToQuadCorners(this Size that)
        {
            return new[] {
                    new System.Drawing.PointF(0, 0),
                    new System.Drawing.PointF(that.Width, 0),
                    new System.Drawing.PointF(0, that.Height),
                    new System.Drawing.PointF(that.Width, that.Height),                    
                };
        }

        public static System.Drawing.PointF[] ToQuadCorners(this Microsoft.Xna.Framework.Rectangle that)
        {
            return new[] {
                    new System.Drawing.PointF(that.Left, that.Top),
                    new System.Drawing.PointF(that.Right, that.Top),
                    new System.Drawing.PointF(that.Left, that.Bottom),
                    new System.Drawing.PointF(that.Right, that.Bottom),                    
                };
        }

        public static Vector3 Truncate3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        
        public static Color Alpha(this Color that, float alpha)
        {
            return Color.Multiply(that, alpha);
        }

        public static Color Lerp(this Color that, Color other, float amount)
        {
            var v = Vector4.Lerp(that.ToVector4(), other.ToVector4(), amount);
            return new Color(v);
        }

        public static IEnumerable<IEnumerable<T>> Chunkify<T>(this IEnumerable<T> source, int chunkSize)
        {
            for (int i = 0; i < source.Count(); i += chunkSize)
                yield return source.Skip(i).Take(chunkSize);
        }

        public static IEnumerable<T> TakeNthPlusM<T>(this IEnumerable<T> source, int n, int m)
        {
            for (int i = 0; (i * n + m) < source.Count(); ++i)
                yield return source.Skip(i * n).Skip(m).First();
        }

        public static string FormatWith(this string fmt, params object[] args)
        {
            return String.Format(fmt, args);
        }

        public static Size Size(this Viewport vp)
        {
            return new Size(vp.Width, vp.Height);
        }

        public static Matrix WorldViewProjectionForPixels2d(this Size size) { return size.WorldViewProjectionForPixels2d(Matrix.Identity); }
        public static Matrix WorldViewProjectionForPixels2d(this Size size, Matrix world)
        {
            var projection = Matrix.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
            var halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            return world * halfPixelOffset * projection;
        }

        public static Plane SweepVerticalThroughX(this BoundingFrustum bf, Vector3 orig, float x, float imageWidth)
        {
            var fx = x / imageWidth;

            var corners = bf.GetCorners();

            var top = Vector3.Lerp(corners[4], corners[5], fx);
            var bottom = Vector3.Lerp(corners[7], corners[6], fx);

            return Plane.Normalize(new Plane(orig, top, bottom));
        }

        public static Quad QuadVerticalThroughX(this BoundingFrustum bf, Vector3 orig, float x, float imageWidth)
        {
            var fx = x / imageWidth;
            
            var corners = bf.GetCorners();

            var top = Vector3.Lerp(corners[4], corners[5], fx);
            var bottom = Vector3.Lerp(corners[7], corners[6], fx);

            return new Quad(orig, top, bottom, orig, Program.Renderer.LeaseWhiteTexel(), Palette.Get((int)x));
        }

        public static Ray ThroughPixel(this BoundingFrustum bf, Vector3 orig, float x, float y, float imw, float imh)
        {
            var fx = x / imw;
            var fy = y / imh;

            var corners = bf.GetCorners();

            var top = Vector3.Lerp(corners[4], corners[5], fx);
            var bottom = Vector3.Lerp(corners[7], corners[6], fx);

            var pixel = Vector3.Lerp(top, bottom, fy);

            var dir = Vector3.Normalize((pixel - orig));
            return new Ray(orig, dir);
        }

        public static Plane SweepHorizontalThroughY(this BoundingFrustum bf, Vector3 orig, float y, float imageHeight)
        {
            var fy = y / imageHeight;

            var corners = bf.GetCorners();

            var left = Vector3.Lerp(corners[4], corners[7], fy);
            var right = Vector3.Lerp(corners[5], corners[6], fy);

            return Plane.Normalize(new Plane(orig, left, right));
        }

        public static Quad QuadHorizontalThroughY(this BoundingFrustum bf, Vector3 orig, float y, float imageHeight)
        {
            var fy = y / imageHeight;

            var corners = bf.GetCorners();

            var top = Vector3.Lerp(corners[4], corners[7], fy);
            var bottom = Vector3.Lerp(corners[5], corners[6], fy);

            return new Quad(orig, top, bottom, orig, Program.Renderer.LeaseWhiteTexel(), Color.Yellow.Alpha(0.4f));
        }

        public static Bgra ToBgra(this Color color)
        {
            return new Bgra(color.B, color.G, color.R, color.A);
        }

        public static Bgra ToBgra(this System.Drawing.Color color)
        {
            return new Bgra(color.B, color.G, color.R, color.A);
        }

        public static string ToSanitizedFileName(this string s)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]+", invalidChars);
            return Regex.Replace(s, invalidReStr, "_");
        }

        public static string Truncate(this string s, int maxLength)
        {
            if (string.IsNullOrEmpty(s) || maxLength <= 0)
                return string.Empty;
            else if (s.Length > (maxLength - 3))
                return s.Substring(0, maxLength - 3) + "...";
            else
                return s;
        }
    }
}
