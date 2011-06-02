using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Graphics
{
    class RenderableGrid : RenderablePrimitiveBase<VertexPosition, Grid>
    {
        public RenderableGrid(RendererImpl rendererImpl)
        {
            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 1);
            AddIndex(CurrentVertex + 2);

            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 2);
            AddIndex(CurrentVertex + 3);

            var rect = new Rectangle(-2, -2, 20, 10);
            AddVertex(new VertexPosition(rect.X, rect.Y, 0));
            AddVertex(new VertexPosition(rect.X + rect.Width, rect.Y, 0));
            AddVertex(new VertexPosition(rect.X + rect.Width, rect.Y + rect.Height, 0));
            AddVertex(new VertexPosition(rect.X, rect.Y + rect.Height, 0));

            Effect = rendererImpl.Loader.Load<Effect>("Content/Shaders/Grid");
            FinishConstruction(rendererImpl.Device);
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Grid grid)
        {
            Effect.Parameters["FillColor"].SetValue(grid.FillColor.ToVector4());
            Effect.Parameters["LineColor"].SetValue(grid.LineColor.ToVector4());
            Effect.Parameters["LineEvery"].SetValue(grid.LineEvery);
            Effect.Parameters["LineWidth"].SetValue(grid.LineThickness);
            Effect.Parameters["WorldViewProj"].SetValue(grid.World * viewProject.View * viewProject.Projection);

            rendererImpl.Device.DepthStencilState = DepthStencilState.None;
            base.Render(rendererImpl, Effect);
            rendererImpl.Device.DepthStencilState = DepthStencilState.Default;
        }

        private Effect Effect;
    }
}
