using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class Grid : PrimitiveBase<VertexPosition>
    {
        public Grid(GraphicsDevice device, ContentManager content)
        {
            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 2);
            AddIndex(CurrentVertex + 1);

            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 3);
            AddIndex(CurrentVertex + 2);

            var y = 0.0f;
            AddVertex(new VertexPosition(-2, y, -1));
            AddVertex(new VertexPosition(-2, y, 4));
            AddVertex(new VertexPosition(5,  y, 4));
            AddVertex(new VertexPosition(5,  y, -1));

            FinishConstruction(device, content.Load<Effect>("Content/Shaders/Grid"));
        }

        public override void Render(object obj, Matrix view, Matrix projection)
        {
            this.Effect.Parameters["BackColor"].SetValue(Color.LightGray.ToVector4());
            this.Effect.Parameters["LineColor"].SetValue(Color.Black.ToVector4());
            this.Effect.Parameters["LineEvery"].SetValue(1.0f);
            this.Effect.Parameters["LineWidth"].SetValue(0.05f);
            
            base.Render(obj, view, projection);
        }

    }
}
