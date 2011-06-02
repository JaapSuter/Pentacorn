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

        public static Color GetBanded(int i)
        {
            var m = i % Banded.Length;
            return Banded[m];
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

        private static Color[] Banded = new[]
        {
            Color.Maroon,
            Color.DarkRed,
            Color.Red,
            Color.Firebrick,
            Color.Brown,
            Color.IndianRed,
            Color.RosyBrown,
            Color.LightCoral,
            Color.Snow,
            Color.MistyRose,
            Color.Salmon,
            Color.Tomato,
            Color.DarkSalmon,
            Color.Coral,
            Color.OrangeRed,
            Color.LightSalmon,
            Color.Sienna,
            Color.SeaShell,
            Color.SaddleBrown,
            Color.Chocolate,
            Color.SandyBrown,
            Color.PeachPuff,
            Color.Peru,
            Color.Linen,
            Color.Bisque,
            Color.DarkOrange,
            Color.BurlyWood,
            Color.Tan,
            Color.AntiqueWhite,
            Color.NavajoWhite,
            Color.BlanchedAlmond,
            Color.PapayaWhip,
            Color.Moccasin,
            Color.Orange,
            Color.Wheat,
            Color.OldLace,
            Color.FloralWhite,
            Color.DarkGoldenrod,
            Color.Goldenrod,
            Color.Cornsilk,
            Color.Gold,
            Color.Khaki,
            Color.LemonChiffon,
            Color.PaleGoldenrod,
            Color.DarkKhaki,
            Color.Olive,
            Color.Yellow,
            Color.Beige,
            Color.LightGoldenrodYellow,
            Color.LightYellow,
            Color.Ivory,
            Color.OliveDrab,
            Color.YellowGreen,
            Color.DarkOliveGreen,
            Color.GreenYellow,
            Color.Chartreuse,
            Color.LawnGreen,
            Color.DarkSeaGreen,
            Color.DarkGreen,
            Color.Green,
            Color.ForestGreen,
            Color.LimeGreen,
            Color.Lime,
            Color.LightGreen,
            Color.PaleGreen,
            Color.Honeydew,
            Color.SeaGreen,
            Color.MediumSeaGreen,
            Color.SpringGreen,
            Color.MintCream,
            Color.MediumSpringGreen,
            Color.MediumAquamarine,
            Color.Aquamarine,
            Color.Turquoise,
            Color.LightSeaGreen,
            Color.MediumTurquoise,
            Color.DarkSlateGray,
            Color.Teal,
            Color.DarkCyan,
            Color.Aqua,
            Color.Cyan,
            Color.PaleTurquoise,
            Color.LightCyan,
            Color.Azure,
            Color.DarkTurquoise,
            Color.CadetBlue,
            Color.PowderBlue,
            Color.LightBlue,
            Color.DeepSkyBlue,
            Color.SkyBlue,
            Color.LightSkyBlue,
            Color.SteelBlue,
            Color.AliceBlue,
            Color.DodgerBlue,
            Color.SlateGray,
            Color.LightSlateGray,
            Color.LightSteelBlue,
            Color.CornflowerBlue,
            Color.RoyalBlue,
            Color.Navy,
            Color.DarkBlue,
            Color.MediumBlue,
            Color.Blue,
            Color.MidnightBlue,
            Color.Lavender,
            Color.GhostWhite,
            Color.SlateBlue,
            Color.DarkSlateBlue,
            Color.MediumSlateBlue,
            Color.MediumPurple,
            Color.BlueViolet,
            Color.Indigo,
            Color.DarkOrchid,
            Color.DarkViolet,
            Color.MediumOrchid,
            Color.Purple,
            Color.DarkMagenta,
            Color.Fuchsia,
            Color.Magenta,
            Color.Violet,
            Color.Plum,
            Color.Thistle,
            Color.Orchid,
            Color.MediumVioletRed,
            Color.DeepPink,
            Color.HotPink,
            Color.LavenderBlush,
            Color.PaleVioletRed,
            Color.Crimson,
            Color.Pink,
            Color.LightPink,
        };
    }
}
