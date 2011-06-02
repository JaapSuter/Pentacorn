using System;
using System.Collections.Generic;

namespace Pentacorn.Vision
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
            Margin = 0;
        }

        public IEnumerable<bool> To(int i)
        {
            var g = BinToGray(i + Margin);
            for (int b = 0; b < NumBits; ++b)
                yield return 0 != ((1 << b) & g);
        }

        public IEnumerable<bool> ToFromStep(int b)
        {
            for (int i = 0; i < Count; ++i)
                yield return 0 != ((1 << b) & BinToGray(i + Margin));
        }

        public int From(IEnumerable<bool> bits)
        {
            int b = 0;
            int g = 0;
            foreach (var bit in bits)
                if (bit)
                    g |= 1 << b++;
                else
                    ++b;

            return GrayToBin(g) - Margin;
        }

        private static int BinToGray(int n)
        {
            return (n >> 1) ^ n;
        }

        private static int GrayToBin(int g)
        {
            g ^= (g >> 16);
            g ^= (g >> 8);
            g ^= (g >> 4);
            g ^= (g >> 2);
            g ^= (g >> 1);
            return g;
        }

        private static int NumBitsNeededFor(int num)
        {
            return (int)Math.Ceiling(Math.Log(num) / Math.Log(2));
        }

        private int Margin;
    }
}
