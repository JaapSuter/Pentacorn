using System;
using Microsoft.Xna.Framework;

namespace Pentacorn
{
    static class SizeEx
    {        
        public static System.Drawing.RectangleF ToRectF(this System.Drawing.Size that, int x = 0, int y = 0)
        {
            return new System.Drawing.Rectangle(x, y, that.Width, that.Height);
        }

        public static System.Drawing.Point Center(this System.Drawing.Size that)
        {
            return new System.Drawing.Point(that.Width / 2, that.Height / 2);
        }

        public static System.Drawing.PointF CenterF(this System.Drawing.Size that)
        {
            return new System.Drawing.PointF(that.Width / 2.0f, that.Height / 2.0f);
        }

        public static System.Drawing.Rectangle AnchorWithin(this System.Drawing.Size that, System.Drawing.Rectangle there, AnchorPoints anchor)
        {
            return System.Drawing.Rectangle.Truncate(new System.Drawing.SizeF(that.Width, that.Height).AnchorWithin(there, anchor));
        }

        public static System.Drawing.RectangleF AnchorWithin(this System.Drawing.SizeF that, System.Drawing.Rectangle there, AnchorPoints anchor)
        {
            switch (anchor)
            {
                case AnchorPoints.TopLeft:
                    return that.ToRectF(new System.Drawing.Point(there.Left                  , there.Top), anchor);
                case AnchorPoints.TopMiddle:
                    return that.ToRectF(new System.Drawing.Point(there.Left + there.Width / 2, there.Top), anchor);
                case AnchorPoints.TopRight:
                    return that.ToRectF(new System.Drawing.Point(there.Right, there.Top), anchor);
                case AnchorPoints.MiddleLeft:
                    return that.ToRectF(new System.Drawing.Point(there.Left, (there.Top + there.Bottom) / 2), anchor);
                case AnchorPoints.Center:
                    return that.ToRectF(new System.Drawing.Point(there.Left + there.Width / 2, (there.Top + there.Bottom) / 2), anchor);
                case AnchorPoints.MiddleRight:
                    return that.ToRectF(new System.Drawing.Point(there.Right, (there.Top + there.Bottom) / 2), anchor);
                case AnchorPoints.BottomLeft:
                    return that.ToRectF(new System.Drawing.Point(there.Left, there.Bottom), anchor);
                case AnchorPoints.BottomMiddle:
                    return that.ToRectF(new System.Drawing.Point(there.Left + there.Width / 2, there.Bottom), anchor);
                case AnchorPoints.BottomRight:
                    return that.ToRectF(new System.Drawing.Point(there.Right, there.Bottom), anchor);
                default:
                    throw new ArgumentException("Unknown anchor enumeration value.");
            }
        }

        public static System.Drawing.Rectangle ToRect(this System.Drawing.Size that, System.Drawing.Point position, AnchorPoints anchor)
        {
            return that.ToRect(position.X, position.Y, anchor);
        }

        public static System.Drawing.Rectangle ToRect(this System.Drawing.Size that)
        {
            return new System.Drawing.Rectangle(0, 0, that.Width, that.Height);
        }

        public static System.Drawing.Point ToPoint(this System.Drawing.Size that)
        {
            return new System.Drawing.Point(that.Width, that.Height);
        }

        public static Microsoft.Xna.Framework.Vector2 ToVector2(this System.Drawing.Size that)
        {
            return new Microsoft.Xna.Framework.Vector2(that.Width, that.Height);
        }

        public static Microsoft.Xna.Framework.Rectangle ToXnaRect(this System.Drawing.Size that)
        {
            return new Microsoft.Xna.Framework.Rectangle(0, 0, that.Width, that.Height);
        }

        public static System.Drawing.Rectangle ToRect(this System.Drawing.Size that, int x, int y, AnchorPoints anchor)
        {
            return System.Drawing.Rectangle.Round(that.ToRectF(x, y, anchor));
        }
        
        public static System.Drawing.RectangleF ToRectF(this System.Drawing.Size that, System.Drawing.PointF position, AnchorPoints anchor)
        {
            return that.ToRectF(position.X, position.Y, anchor);
        }

        public static System.Drawing.RectangleF ToRectF(this System.Drawing.SizeF that, System.Drawing.PointF position, AnchorPoints anchor)
        {
            return that.ToRectF(position.X, position.Y, anchor);
        }

        public static System.Drawing.RectangleF ToRectF(this System.Drawing.Size that, Vector2 position, AnchorPoints anchor)
        {
            return that.ToRectF(position.X, position.Y, anchor);
        }

        public static System.Drawing.RectangleF ToRectF(this System.Drawing.Size that, float x, float y, AnchorPoints anchor)
        {
            return new System.Drawing.RectangleF(0, 0, that.Width, that.Height).Anchor(x, y, anchor);
        }

        public static System.Drawing.RectangleF ToRectF(this System.Drawing.SizeF that, float x, float y, AnchorPoints anchor)
        {
            return new System.Drawing.RectangleF(0, 0, that.Width, that.Height).Anchor(x, y, anchor);
        }

        public static System.Drawing.RectangleF Anchor(this System.Drawing.RectangleF that, float x, float y, AnchorPoints anchor)
        {
            switch (anchor)
            {
                case AnchorPoints.TopLeft:
                    return new System.Drawing.RectangleF(x, y, that.Width, that.Height);
                case AnchorPoints.TopMiddle:
                    return new System.Drawing.RectangleF(x - that.Width / 2, y, that.Width, that.Height);
                case AnchorPoints.TopRight:
                    return new System.Drawing.RectangleF(x - that.Width, y, that.Width, that.Height);
                case AnchorPoints.MiddleLeft:
                    return new System.Drawing.RectangleF(x, y - that.Height / 2, that.Width, that.Height);
                case AnchorPoints.Center:
                    return new System.Drawing.RectangleF(x - that.Width / 2, y - that.Height / 2, that.Width, that.Height);
                case AnchorPoints.MiddleRight:
                    return new System.Drawing.RectangleF(x - that.Width, y - that.Height / 2, that.Width, that.Height);
                case AnchorPoints.BottomLeft:
                    return new System.Drawing.RectangleF(x, y - that.Height, that.Width, that.Height);
                case AnchorPoints.BottomMiddle:
                    return new System.Drawing.RectangleF(x - that.Width / 2, y - that.Height, that.Width, that.Height);
                case AnchorPoints.BottomRight:
                    return new System.Drawing.RectangleF(x - that.Width, y - that.Height, that.Width, that.Height);
                default:
                    throw new ArgumentException("Unknown anchor enumeration value.");
            }
        }

        public static System.Drawing.RectangleF Anchor(this System.Drawing.RectangleF that, Vector2 position, AnchorPoints anchor)
        {
            return that.Anchor(position.X, position.Y, anchor);
        }

        public static System.Drawing.RectangleF Anchor(this System.Drawing.RectangleF that, System.Drawing.PointF position, AnchorPoints anchor)
        {
            return that.Anchor(position.X, position.Y, anchor);
        }

        public static System.Drawing.Size Scale(this System.Drawing.Size that, float scale)
        {
            return new System.Drawing.Size((int)((float)that.Width * scale), (int)((float)that.Height * scale));
        }

        public static System.Drawing.SizeF Truncate(this System.Drawing.SizeF that, float width, float height)
        {
            return new System.Drawing.SizeF(width > that.Width ? that.Width : width,
                                            height > that.Height ? that.Height : height);
        }

        public static System.Drawing.Size Truncate(this System.Drawing.Size that, int width, int height)
        {
            return new System.Drawing.Size(width > that.Width ? that.Width : width,
                                           height > that.Height ? that.Height : height);
        }

        public static System.Drawing.SizeF LimitProportional(this System.Drawing.SizeF that, float width, float height)
        {
            if (that.Width > width)
            {
                that.Height = width / that.Ratio();
                that.Width = width;
            }

            if (that.Height > height)
            {
                that.Width = height * that.Ratio();
                that.Height = height;                
            }

            return that;
        }

        public static System.Drawing.Size LimitProportional(this System.Drawing.Size that, int width, int height)
        {
            return System.Drawing.Size.Truncate(LimitProportional(new System.Drawing.SizeF(that.Width, that.Height), width, height));
        }

        public static int Area(this System.Drawing.Size s)
        {
            return s.Width * s.Height;
        }

        public static float Ratio(this System.Drawing.Size s)
        {
            return (float)s.Width / (float)s.Height;
        }

        public static float Ratio(this System.Drawing.SizeF s)
        {
            return (float)s.Width / (float)s.Height;
        }
    }
}
