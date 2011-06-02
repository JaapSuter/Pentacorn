using Microsoft.Xna.Framework;

namespace Pentacorn.Vision
{
    static class Skreen
    {
        // Using...
        //
        //  http://www.projectorcentral.com/Optoma-EW1610-projection-calculator-pro.htm

        public const int PixelWidth = 800;
        public const int PixelHeight = 1280;

        public const float Width = 0.72f;
        public const float Height = Width / 10.0f * 16.0f;
        public const float Distance = 1.80f;

        public static readonly Vector2 Center = new Vector2((Left + Right) / 2.0f, (Top + Bottom) / 2.0f);

        public const float Bottom = 0.0f;
        public const float Left = 0.055f;
        public const float Top = Bottom + Height;
        public const float Right = Left + Width;

    }
}
