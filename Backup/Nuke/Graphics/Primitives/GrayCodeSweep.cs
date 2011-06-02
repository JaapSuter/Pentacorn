using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class GrayCodeSweep : IRenderable
    {
        public enum Dir
        {
            Horizontal,
            Vertical
        };

        public struct Context
        {
            public SafeGrayCode Sgc;
            public int Bit;
            public Dir Dir;
            public int Sweep;
        };

        public GrayCodeSweep(GraphicsDevice device)
        {
            sb = new SpriteBatch(device);
            white = new Texture2D(device, 1, 1);
            white.SetData(new Color[] { Color.White });
        }
        
        /*
        private IEnumerable<Texture2D> Patterns(GraphicsDevice device)
        {
            var 
            tex = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            tex.SetData(new Color[] { Color.Black });
            yield return tex;
            
            tex = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            tex.SetData(new Color[] { new Color(0.5f, 0.5f, 0.5f, 1.0f) });
            yield return tex;

            tex = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            tex.SetData(new Color[] { Color.White });
            yield return tex;

            for (int b = 0; b < horizontal.NumBits; ++b)
            {
                tex = new Texture2D(device, Width, 1, false, SurfaceFormat.Color);
                var data = horizontal.ToFromStep(b).Select(gray => gray ? Color.White : Color.Black).ToArray();
                tex.SetData(data);
                yield return tex;
            }

            for (int b = 0; b < vertical.NumBits; ++b)
            {
                tex = new Texture2D(device, Width, Height, false, SurfaceFormat.Color);
                var data = new Color[Width * Height];
                int y = 0;
                foreach (var gray in vertical.ToFromStep(b))
                {
                    for (int x = 0; x < Width; ++x)
                        data[y * Width + x] = gray ? Color.White : Color.Black;
                    ++y;
                }
                tex.SetData(data);
                yield return tex;
            }
        }
        */

        public void Render(object obj, Matrix view, Matrix projection)
        {
            var ctx = (Context)obj;
            var bws = ctx.Sgc.ToFromStep(ctx.Bit).Select(bw => bw ? Color.White : Color.Black).ToArray();

            sb.Begin(SpriteSortMode.Texture, BlendState.Opaque);
            if (ctx.Dir == Dir.Horizontal)
                for (int i = 0; i < ctx.Sgc.Count; ++i)
                    sb.Draw(white, new Rectangle(i, 0, 1, ctx.Sweep), bws[i]);
            else if (ctx.Dir == Dir.Vertical)
                    for (int i = 0; i < ctx.Sgc.Count; ++i)
                        sb.Draw(white, new Rectangle(0, i, ctx.Sweep, 1), bws[i]);
            sb.End();
        }

        /*
        public void ByTheWay(Picture picture)
        {
            if (grays.Count >= numBits)
                return;

            if (waitCount == 0)
            {
                NextTexture();
                waitCount = waitDelay;
                return;
            }
            else
                --waitCount;

            if (waitCount != waitTrigger)
                return;

            var pict = picture.Bgra.Convert<Gray, byte>();

            if (bit == blackBit)
            {
                black = pict;
                black.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.black.png", grays.Count)));
            }
            else if (bit == whiteBit)
            {
                white = pict;
                white.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.white.png", grays.Count)));
                offset = (white - black) / 2;
                offset.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.offset.png", grays.Count)));
                middle = black + offset;
                middle.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.middle.png", grays.Count)));

                PhysicalCamera.Texture = new Picture(picture.Width, picture.Height);
                picture.Bgra.CopyTo(PhysicalCamera.Texture.Bgra);
            }
            else
            {
                var gray = pict;
                gray.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.png", grays.Count)));
                gray = gray - black;
                gray.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.offset.png", grays.Count)));
                gray._ThresholdToZero(new Gray(10));
                gray.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.truncated.png", grays.Count)));
                gray = gray.Cmp(offset, Emgu.CV.CvEnum.CMP_TYPE.CV_CMP_GT);
                gray.Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + String.Format(".{0}.binary.png", grays.Count)));

                grays.Add(gray);

                if (grays.Count == numBits)
                    Interpret();
            }
        }
        
        private void Interpret()
        {
            var accum = new Image<Gray, int>(width, height);

            for (int g = 0; g < numBits; ++g)
                accum._Or((grays[g].Convert<Gray, int>() / 255) * (1 << g));

            var vals = accum.Data;
            for (int y = 0; y < width; ++y)
                for (int x = 0; x < height; ++x)
                    vals[y, x, 0] = GrayToBin(vals[y, x, 0]);
            accum.Data = vals;

            var scale = 255.0 / (double)(1 << numBits);
            accum.Mul(scale).Convert<Gray, byte>().Save(Path.Combine(Global.TmpDir, DateTime.Now.TimeOfDay.ToString().ToSanitizedFileName() + ".unwrapped.png"));

            double[] min, max;
            System.Drawing.Point[] pmin, pmax;
            accum.MinMax(out min, out max, out pmin, out pmax);
            Console.WriteLine("Min {0}, Max {1}, Offset {2}", min[0], max[0], margin);

            Projector.Unwrapped = accum;
        }
        */

        private Texture2D white;
        private SpriteBatch sb;
    }
}
