using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    interface IViewProject
    {
        Matrix View { get; }
        Matrix Projection { get; }
    }
}
