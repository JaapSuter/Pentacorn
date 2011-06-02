using System.Drawing;
using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class ScreenCamera : IViewProject
    {
        public ScreenCamera(Window window)
        {
            window.Resize += (s, e)
                => Refresh(window.ClientSize.Width, window.ClientSize.Height);

            Refresh(window.ClientSize.Width, window.ClientSize.Height);
        }

        private void Refresh(int width, int height)
        {
            View = Matrix.Identity;
            Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
        }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }        
    }
}
