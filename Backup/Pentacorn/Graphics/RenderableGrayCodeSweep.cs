using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn;
using System.Collections.Generic;
using Size = System.Drawing.Size;
using System;

namespace Pentacorn.Graphics
{
    class RenderableGrayCodeSweep : Renderable<GrayCodeSweep>
    {
        public RenderableGrayCodeSweep(RendererImpl rendererImpl)
        {            
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, GrayCodeSweep grayCodeSweep)
        {
            var black = grayCodeSweep.BlackColor;
            var white = grayCodeSweep.WhiteColor;

            var bws = grayCodeSweep.Sgc.BitsForStep(grayCodeSweep.Bit).Select(bw => bw ? white : black).ToArray();

            var whiteTexel = rendererImpl.WhiteTexel;
            var spriteBatch = rendererImpl.SpriteBatch;

            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);
            if (grayCodeSweep.Dir == GrayCodeSweep.Direction.Horizontal)
                for (int i = 0; i < grayCodeSweep.Sgc.Count; ++i)
                    spriteBatch.Draw(whiteTexel, new Rectangle(i, 0, 1, grayCodeSweep.SweepLength), bws[i]);
            else if (grayCodeSweep.Dir == GrayCodeSweep.Direction.Vertical)
                for (int i = 0; i < grayCodeSweep.Sgc.Count; ++i)
                    spriteBatch.Draw(whiteTexel, new Rectangle(0, i, grayCodeSweep.SweepLength, 1), bws[i]);
            spriteBatch.End();            
        }

        private IEnumerable<Texture2D> Patterns(RendererImpl rendererImpl, Size size)
        {
            var horizontal = new SafeGrayCode(size.Width);
            var vertical = new SafeGrayCode(size.Height);

            var 
            tex = new Texture2D(rendererImpl.Device, 1, 1, false, SurfaceFormat.Color);
            tex.SetData(new Color[] { Color.Black });
            yield return tex;

            tex.Dispose();
            tex = new Texture2D(rendererImpl.Device, 1, 1, false, SurfaceFormat.Color);
            tex.SetData(new Color[] { new Color(0.5f, 0.5f, 0.5f, 1.0f) });
            yield return tex;

            tex.Dispose();
            tex = new Texture2D(rendererImpl.Device, 1, 1, false, SurfaceFormat.Color);
            tex.SetData(new Color[] { Color.White });
            yield return tex;

            for (int b = 0; b < horizontal.NumBits; ++b)
            {
                tex.Dispose();
                tex = new Texture2D(rendererImpl.Device, size.Width, 1, false, SurfaceFormat.Color);
                var data = horizontal.BitsForStep(b).Select(gray => gray ? Color.White : Color.Black).ToArray();
                tex.SetData(data);
                yield return tex;
            }

            for (int b = 0; b < vertical.NumBits; ++b)
            {
                tex.Dispose();
                tex = new Texture2D(rendererImpl.Device, size.Width, size.Height, false, SurfaceFormat.Color);
                var data = new Color[size.Area()];
                int y = 0;
                foreach (var gray in vertical.BitsForStep(b))
                {
                    for (int x = 0; x < size.Width; ++x)
                        data[y * size.Width + x] = gray ? Color.White : Color.Black;
                    ++y;
                }

                tex.SetData(data);
                yield return tex;
            }
        }
    }
}
