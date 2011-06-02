using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Size = System.Drawing.Size;

namespace Pentacorn.Vision
{
    static class Extensions
    {
        public static Rectangle Abs(this Rectangle that)
        {
            if (that.Width < 0)
                that = new Rectangle(that.X - that.Width, -that.Width, that.Y, that.Height);
            if (that.Height < 0)
                that = new Rectangle(that.X, that.Width, that.Y - that.Height, -that.Height);
            
            return that; // Todo, this function is stupid according to Jaap who wrote it.
        }

        public static void Invoke(this Control control, Action action)
        {
            control.Invoke((Delegate)action);
        }

        public static T Invoke<T>(this Control control, Func<T> func)
        {
            return (T)control.Invoke((Delegate)func);
        }

        public static Matrix WorldViewProjectionForPixels2d(this Viewport vp)
        {
            var projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
            var halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            return halfPixelOffset * projection;
        }

        public static Ray ThroughPixel(this BoundingFrustum bf, Vector3 orig, float x, float y, float imw, float imh)
        {
            var fx = x / imw;
            var fy = y / imh;

            var corners = bf.GetCorners();

            var left = Vector3.Lerp(corners[4], corners[7], fy);
            var right = Vector3.Lerp(corners[5], corners[6], fy);

            var pixel = Vector3.Lerp(left, right, fx);

            var dir = Vector3.Normalize((pixel - orig));
            return new Ray(orig, dir);
        }

        public static int Area(this Size s)
        {
            return s.Width * s.Height;
        }

        public static Bgra ToBgra(this Color color)
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
