using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Pentacorn
{
    class SafeGrayCode
    {
        public int NumBits { get; private set; }
        public int Count { get; private set; }

        public SafeGrayCode(int count)
        {
            Count = count;
            NumBits = NumBitsNeededFor(count);
            Margin = ((1 << NumBits) - Count) / 2;
        }

        public IEnumerable<bool> BitsForStep(int step)
        {
            for (int i = 0; i < Count; ++i)
                yield return 0 != ((1 << step) & BinToGray((ushort)(i + Margin)));
        }

        public int ToBinary(int gray)
        {
            return GrayToBin(gray) - Margin;
        }

        private int BinToGray(int n)
        {
            return (n >> 1) ^ n;
        }

        private static int GrayToBin(int g)
        {
            var t = g;
            t ^= (g >> 16);
            t ^= (t >> 8);
            t ^= (t >> 4);
            t ^= (t >> 2);
            t ^= (t >> 1);
            return t;
        }

        private static int NumBitsNeededFor(int num)
        {
            return (int)Math.Ceiling(Math.Log(num) / Math.Log(2));
        }

        public static void Test(int count)
        {
            var g = new SafeGrayCode(count);

            var pics = Enumerable.Range(0, g.NumBits).Select(b => g.BitsForStep(b).ToArray()).ToArray();

            var accs = new int[count];

            for (var i = 0; i < g.NumBits; ++i)
                for (var x = 0; x < count; ++x)
                    accs[x] |= pics[i][x] ? (1 << i) : 0;

            for (var x = 0; x < count; ++x)
                accs[x] = g.ToBinary(accs[x]);

            Debug.Assert(accs.SequenceEqual(Enumerable.Range(0, count)));
        }

        private int Margin;
    }
}
