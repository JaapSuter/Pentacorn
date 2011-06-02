using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class RenderableCircle2D : Renderable<Circle2D>
    {
        public RenderableCircle2D(RendererImpl rendererImpl)
            : base()
        {
            effect = rendererImpl.Loader.Load<Effect>("Content/Shaders/Circle2d");
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Circle2D circle2D)
        {
            var screenPosition = circle2D.AlreadyInScreenSpace
                               ? circle2D.Position.ToVector2()
                               : rendererImpl.Device.Viewport.Project(circle2D.Position, viewProject.Projection, viewProject.View, Matrix.Identity).ToVector2();

            effect.Parameters["WorldViewProj"].SetValue(effect.GraphicsDevice.Viewport.Size().WorldViewProjectionForPixels2d());
            effect.Parameters["Position"].SetValue(screenPosition);
            effect.Parameters["InnerAndOuterRadius"].SetValue(new Vector2(circle2D.InnerRadius, circle2D.InnerRadius + circle2D.Thickness));
            effect.Parameters["Color"].SetValue(circle2D.Color.ToVector4());
            
            const int inAndOutsideAntiAliasPixels = 2;
            var halfWidth = inAndOutsideAntiAliasPixels + (int)(circle2D.InnerRadius + circle2D.Thickness);
            var halfHeight = inAndOutsideAntiAliasPixels + (int)(circle2D.InnerRadius + circle2D.Thickness);
            var rect = new Rectangle((int)screenPosition.X - halfWidth, (int)screenPosition.Y - halfHeight, halfWidth * 2, halfHeight * 2);

            rendererImpl.SpriteBatch.Begin(default(SpriteSortMode), BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
            rendererImpl.SpriteBatch.Draw(rendererImpl.WhiteTexel, rect, Color.White);
            rendererImpl.SpriteBatch.End();
        }

        Effect effect;
    }
}
