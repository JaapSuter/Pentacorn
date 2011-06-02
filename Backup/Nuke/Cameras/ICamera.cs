using Microsoft.Xna.Framework;

namespace Pentacorn.Vision.Cameras
{
    interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }
    }
}
