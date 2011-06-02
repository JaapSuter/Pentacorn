using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class Circle2d : IRenderable
    {
        public class Context
        {
            public Vector2 Position;
            public float InnerRadius;
            public float OuterRadius;
            public Color Color;
        }

        public Circle2d(GraphicsDevice device, ContentManager content)
            : base()
        {
            sb = new SpriteBatch(device);            
            white = new Texture2D(device, 1, 1);
            white.SetData(new Color[] { Color.White });

            effect = content.Load<Effect>("Content/Shaders/Circle2d");            
        }

        public void Render(object obj, Matrix view, Matrix projection)
        {
            var ctx = (Context)obj;

            effect.Parameters["WorldViewProj"].SetValue(effect.GraphicsDevice.Viewport.WorldViewProjectionForPixels2d());
            effect.Parameters["Position"].SetValue(ctx.Position);
            effect.Parameters["InnerAndOuterRadius"].SetValue(new Vector2(ctx.InnerRadius, ctx.OuterRadius));
            effect.Parameters["Color"].SetValue(ctx.Color.ToVector4());            
            
            sb.Begin(default(SpriteSortMode), BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
            sb.Draw(white, effect.GraphicsDevice.Viewport.Bounds, Color.Red);
            sb.End();
        }

        SpriteBatch sb;
        Effect effect;
        Texture2D white;
    }
}
