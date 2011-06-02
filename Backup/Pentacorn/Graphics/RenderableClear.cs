
namespace Pentacorn.Graphics
{
    class RenderableClear : Renderable<Clear>
    {
        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Clear clear)
        {
            rendererImpl.Device.Clear(clear.Color);
        }
    }
}
