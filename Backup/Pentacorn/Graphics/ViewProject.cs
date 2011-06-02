using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class ViewProject : IViewProject
    {
        public ViewProject(Matrix view, Matrix projection)
        {
            View = view;
            Projection = projection;
        }

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public static ViewProject Identity = new ViewProject(Matrix.Identity, Matrix.Identity);
    }
}
