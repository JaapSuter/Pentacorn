using System;
using Microsoft.Xna.Framework;

namespace Pentacorn
{
    static class Palette
    {
        public static Color Get(int i)
        {
            var m = i % Base.Length;
            var s = 1.0f - Math.Abs(i % 255) / 255.0f;
            return Base[m].Alpha(s);
        }

        public static Color GetSolid(int i)
        {
            var m = i % Base.Length;
            return Base[m] ;
        }

        private static Color[] Base = new[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Cyan,
            Color.Fuchsia,
        };
    }
}
