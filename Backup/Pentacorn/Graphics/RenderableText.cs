using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System;
using System.Linq;
using Point = System.Drawing.Point;
using Emgu.CV.UI;
using System.Threading;

namespace Pentacorn.Graphics
{
    class RenderableText : Renderable<Text>
    {
        public RenderableText(RendererImpl rendererImpl)
        {
            SpriteFont = rendererImpl.Loader.Load<SpriteFont>("Content/Fonts/Custom");
            BasicEffect = new BasicEffect(rendererImpl.Device)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Text text)
        {
            rendererImpl.PostOps.Add(() => RenderDelayed(rendererImpl, viewProject, text));
        }

        private void RenderDelayed(RendererImpl rendererImpl, IViewProject viewProject, Text text)
        {
            BasicEffect.World = text.AlreadyInScreenSpace
                              ?         Matrix.CreateTranslation(text.Position)
                              : FlipY * Matrix.CreateTranslation(text.Position);
            BasicEffect.View = viewProject.View;
            BasicEffect.Projection = viewProject.Projection;
            
            var anchor = text.AlreadyInScreenSpace ? Vector2.Zero : rendererImpl.DebugFont.MeasureString(text.String) / 2;
            var scale = text.AlreadyInScreenSpace ? 0.2f : 0.01f;
            
            rendererImpl.SpriteBatch.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, BasicEffect);
            rendererImpl.SpriteBatch.DrawString(SpriteFont, text.String, Vector2.Zero, text.FillColor, 0, anchor, scale, 0, 0);
            rendererImpl.SpriteBatch.End();
        }

        private void InitializeDistanceField(RendererImpl rendererImpl)
        {
            // Todo, shame on me...
            var width = 1305;
            var height = 746;
            
            using (var src = new Image<Gray, byte>(Path.Combine(Global.ExeDir, "Content/Fonts/Distance.png")))
            {
                src._ThresholdBinaryInv(new Gray(1), new Gray(255));
                using (var dst = src.Convert<Gray, float>())
                {
                    CvInvoke.cvDistTransform(src.Ptr, dst.Ptr, DIST_TYPE.CV_DIST_L2, 5, null, IntPtr.Zero);
                    double[] min, max;
                    Point[] minp, maxp;
                    dst.MinMax(out min, out max, out minp, out maxp);
                    dst._Mul(255.0f / max.First());
                     
                    using (var picture = new Picture<Gray, byte>(dst.Width, dst.Height))
                    {
                        picture.Emgu.ConvertFrom(dst);                        
                        var DistanceField = new Texture2D(rendererImpl.Device, width, height, false, SurfaceFormat.Alpha8);
                        DistanceField.SetData(picture.Bytes);
                    }
                }
            }
        }

        private SpriteFont SpriteFont;
        private BasicEffect BasicEffect;
        private static readonly Matrix FlipY = Matrix.CreateScale(1, -1, 1);
    }
}
