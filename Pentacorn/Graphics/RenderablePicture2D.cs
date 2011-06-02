
namespace Pentacorn.Graphics
{
    class RenderablePicture2d : Renderable<Picture2D>
    {
        public RenderablePicture2d(RendererImpl rendererImpl)
        {
            RenderableQuad = new RenderableQuad(rendererImpl);
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Picture2D picture2D)
        {
            RenderableQuad.Render(rendererImpl, viewProject, new Quad(picture2D.Rectangle, picture2D.Texture2D, picture2D.Color));
        }

        private RenderableQuad RenderableQuad;
    }
}
