using System;
using Microsoft.Xna.Framework;

namespace Pentacorn.Vision.Cameras
{
    class VirtualOrbitCamera : IUpdateable, ICamera
    {
        public VirtualOrbitCamera(float fieldOfView, float aspectRatio)
        {
            this.Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, near, far);
            this.Update(TimeSpan.Zero, Vector2.Zero, 0);
        }

        public void Update(TimeSpan ts)
        {
            Update(ts, Input.Global.LeftButton ? Input.Global.MousePositionDelta : Vector2.Zero, Input.Global.MouseScrollDelta);
        }

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        private void Update(TimeSpan ts, Vector2 deltaMousePosition, float deltaMouseScroll)
        {
            var dt = (float)ts.TotalSeconds;

            const float ad = 0.1f;
            yaw += dt * ad * deltaMousePosition.X;
            pitch += dt * ad * deltaMousePosition.Y;

            const float maxPitch = (float)(60.0f / 180 * Math.PI);
            const float minPitch = (float)(-40.0f / 180 * Math.PI);
            pitch = MathHelper.Clamp(pitch, minPitch, maxPitch);

            const float dd = -0.03f;
            const float min = 0.3f;
            const float max = 42.0f;
            distance = MathHelper.Clamp(distance + dt * dd * deltaMouseScroll, min, max);

            Matrix rotateX = Matrix.CreateFromYawPitchRoll(yaw, 0, 0);
            Matrix rotateY = Matrix.CreateFromYawPitchRoll(0, pitch, 0);
            Matrix translate = Matrix.CreateTranslation(-distance * Vector3.UnitZ);

            View = rotateX * rotateY * translate * Matrix.CreateTranslation(-center);
        }

        private const float near = 0.1f;
        private const float far = 7.4f;

        private Vector3 center = Vector3.Zero;
        private float distance = (far + near) / 2;
        private float yaw = MathHelper.ToRadians(10.0f);
        private float pitch = MathHelper.ToRadians(30.0f);
    }
}
