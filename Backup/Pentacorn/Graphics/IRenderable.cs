using System;

namespace Pentacorn.Graphics
{
    interface IRenderable
    {
        Type AbleToRender { get; }
        void Render(RendererImpl rendererImpl, IViewProject viewProject, object obj);
    }
}
