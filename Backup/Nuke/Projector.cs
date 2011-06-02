using System.Linq;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Pentacorn.Vision.Graphics;


namespace Pentacorn.Vision
{
    class Projector : IVisible
    {
        public static Image<Gray, int> Unwrapped = null;

        public const int M = 6;
        public const int N = 11;

        public static readonly Vector2 Size = new Vector2(0.06f, 0.06f);
        public static readonly Vector2 TopLeftCornerRelativeToCenter = new Vector2(M - 1, -N) * 0.5f * Size;
        public static readonly Vector2 Origin = Skreen.Center - TopLeftCornerRelativeToCenter;

        public const float ScreenWidth = Skreen.Width;
        public const float ScreenHeight = Skreen.Height;
        public const float ScreenOffset = Skreen.Bottom;
        public const float ScreenDistance = Skreen.Distance;

        public Projector()
            : base()
        {
            const float near = 0.2f;
            const float far = 2.3f;
            const float scale = 1.0f / ScreenDistance * near;
            projection = Matrix.CreatePerspectiveOffCenter(
                scale * Skreen.Left, scale * Skreen.Right,
                scale * -Skreen.Height / 2, scale * Skreen.Height / 2,
                near, far);
            view = Matrix.CreateTranslation(0.0f, -Skreen.Height / 2, -ScreenDistance);

            // this.Width = Screen.PixelWidth;
            // this.Height = Screen.PixelHeight;
            // this.Left = WinScreen.AllScreens.Where(s => s != WinScreen.PrimaryScreen).First().Bounds.Left;
            // this.WindowState = FormWindowState.Maximized;
            // this.FormBorderStyle = FormBorderStyle.None;
            
            var num = Skreen.PixelHeight;
            var frustum = new BoundingFrustum(view * projection);
            var corners = frustum.GetCorners();
            var tpos = corners[4];
            var tray = (corners[7] - corners[4]) / (float)num;
            var bpos = corners[5];
            var bray = (corners[6] - corners[5]) / (float)num;

            SweptPlanes = new Plane[num];
            var projectorPos = Vector3.Zero;//.Invert(this.View).Translation;
            for (int i = 0; i < num; ++i)
                SweptPlanes[i] = new Plane(projectorPos, tpos + (float)i * tray, bpos + (float)i * bray);
        }

        public void See(Frame frame)
        {
            // DebugShapeRenderer.AddBoundingFrustum(new BoundingFrustum(), Color.HotPink);
            // frame.Add<Primitives.Axes>(Matrix.Invert(this.View));
            // frame.Add<Primitives.GrayCodeSweep>(0);
        }

        public static MCvPoint3D32f[][] GetObjectPointsCopy(int num, int m, int n, float w, float h)
        {
            MCvPoint3D32f[][] ops = new MCvPoint3D32f[num][];
            
            ops[0] = new MCvPoint3D32f[m * n];
            for (int y = 0; y < n; ++y)
                for (int x = 0; x < m; ++x)
                    ops[0][y * m + x] = new MCvPoint3D32f(x * w, -y * h, 0);

            for (int i = 1; i < num; ++i)
                ops[i] = ops[0].ToArray();
            
            return ops;
        }

        public static MCvPoint3D32f[] GetObjectPointsCopy()
        {
            var ops = new MCvPoint3D32f[M * N];
            for (int y = 0; y < N; ++y)
                for (int x = 0; x < M; ++x)
                    ops[y * M + x] = new MCvPoint3D32f(Origin.X + x * Size.X, Origin.Y - (y * Size.Y), 0);
            return ops;
        }

        public static Plane[] SweptPlanes { get; private set; }

        private Matrix view;
        private Matrix projection;

    }
}
