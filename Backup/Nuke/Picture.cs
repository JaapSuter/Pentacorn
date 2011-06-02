using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;

namespace Pentacorn.Vision
{
    sealed class Picture : InteropBuffer
    {
        public int Width { get { return bgra.Width; } }
        public int Height { get { return bgra.Height; } }

        public Image<Bgra, byte> Bgra { get { return bgra; } }
        public Image<Gray, byte> Gray { get { return gray; } }
        public Image<Bgr, byte> Bgr { get { return bgr; } }

        public Picture(int width, int height)
            : base(width * height * maxChannels)
        {
            var grayChannels = 1;
            var bgrChannels = 3;
            var bgraChannels = 4;
            var size = width * height * bgraChannels;

            gray = new Image<Gray, byte>(width, height, width * grayChannels, Ptr);
            bgr = new Image<Bgr, byte>(width, height, width * bgrChannels, Ptr);
            bgra = new Image<Bgra, byte>(width, height, width * bgraChannels, Ptr);

            if (Global.DebugThorough)
                this.Bgra.SetValue(Color.Magenta.ToBgra());
        }

        public Picture Histogram()
        {
            var chans = 3;
            var bins = 256;
            var range = new RangeF(0, 255);
            var hist = new DenseHistogram(bins, range);
            var split = this.bgra.Split();
            var colors = new Bgra[]
            {
                new Bgra(255, 0, 0, 255),
                new Bgra(0, 255, 0, 255),
                new Bgra(0, 0, 255, 255),
            };

            var hip = new Picture(bins * chans, bins + 1); // Todo, plus one Jaap, really? Tssssk... wrote Jaap to himself.
            hip.Bgra.SetValue(Color.Black.ToBgra());

            for (int chan = 0; chan < chans; ++chan)
            {
                hist.Calculate<byte>(new Image<Gray, byte>[] { split[chan] }, false, null);

                // Todo, Jaap, December 2010, hist.Normalize(bins - 1);
                float min, max;
                int[] minLoc, maxLoc;
                hist.MinMax(out min, out max, out minLoc, out maxLoc);
                if (max == min)
                    continue;

                var scale = 255.0f / (max - min);

                for (int x = 0; x < bins; ++x)
                {
                    var n = hip.Height - (int)(hist[x] * scale);
                    for (int y = hip.Height - 1; y > n; --y)
                        hip.Bgra[y, x + chan * bins] = colors[chan];
                }
            }

            foreach (var c in split)
                c.Dispose();

            return hip;
        }

        public Picture DemosaickBayer8(COLOR_CONVERSION cc)
        {
            using (var bgr = new Picture(Width, Height))
            {
                var bgra = new Picture(Width, Height);

                CvInvoke.cvCvtColor(this.Gray.Ptr, bgr.Bgr.Ptr, cc);
                bgra.Bgra.ConvertFrom<Bgr, byte>(bgr.Bgr);

                return bgra;
            }
        }

        public static void ExchangeAndFree(ref Picture location, ref Picture value)
        {
            var previous = Interlocked.Exchange(ref location, value);
            if (previous != null)
                previous.Dispose();
            value = null;
        }

        private const int maxChannels = 4;
        private Image<Bgra, byte> bgra;
        private Image<Bgr, byte>  bgr;
        private Image<Gray, byte> gray;
    }
}
