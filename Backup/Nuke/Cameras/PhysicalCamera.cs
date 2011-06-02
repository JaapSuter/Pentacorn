using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn.Vision.Graphics;
using Pentacorn.Vision.Markers;
using Capture = Pentacorn.Vision.Captures.Capture;
using Color = Microsoft.Xna.Framework.Color;
using Primitives = Pentacorn.Vision.Graphics.Primitives;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pentacorn.Vision.Cameras
{
    class PhysicalCamera : ICamera, IVisible, IUpdateable
    {
        public bool Calibrated { get { return chessboard.Calibrated; } }

        public PhysicalCamera(Capture capture)
        {
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50), (float)capture.Width / (float)capture.Height, 0.02f, 0.2f);
            this.World = Matrix.CreateTranslation(-2, -2, -2);

            chessboard = new Markers.Chessboard(Projector.M, Projector.N, capture.Uuid, capture.Width, capture.Height);
            cap = capture;

            font.thickness = 8;
        }

        public void Update(TimeSpan dt)
        {
            var unwrapped = Projector.Unwrapped;
            if (unwrapped == null)
                return;

            if (!this.Localized)
                return;

            if (Cloud != null)
                return;

            var frustum = new BoundingFrustum(this.View * this.Projection);
            var orig = this.World.Translation;
            var corners = frustum.GetCorners();

            var cld = new Vector3[cap.Width * cap.Height];

            var vals = unwrapped.Data;

            for (int y = 0; y < cap.Height; ++y)
            {
                var fy = (float)y / (float)cap.Height;
                var left = Vector3.Lerp(corners[4], corners[7], fy);
                var right = Vector3.Lerp(corners[5], corners[6], fy);

                for (int x = 0; x < cap.Width; ++x)
                {
                    var fx = (float)x / (float)cap.Width;
                    var pixel = Vector3.Lerp(left, right, fx);

                    var dir = Vector3.Normalize((pixel - orig));
                    var ray = new Ray(orig, dir);

                    var spi = unwrapped.Data[y, x, 0];
                    if (spi < 0)
                        continue;
                    if (spi >= Projector.SweptPlanes.Length)
                        continue;

                    var sp = Projector.SweptPlanes[spi];

                    float? d = ray.Intersects(sp);
                    if (!d.HasValue)
                        d = ray.Intersects(frustum.Far);
                    if (!d.HasValue)
                        d = -1000.0f;

                    var pos = orig + d.Value * dir;
                    cld[y * cap.Width + x] = pos;
                }
            }

            Cloud = cld;
        }

        public static List<KeyValuePair<Vector3, Vector3>> rays = new List<KeyValuePair<Vector3, Vector3>>();

        public static Picture Texture = null;
        public static Vector3[] Cloud;
        
        public void See(Frame frame)
        {
            Picture p = cap.TryDequeue();
            if (p != null)
            {
                using (var bayer = new Picture(p.Width, p.Height))
                {
                    CvInvoke.cvCvtColor(p.Bgr.Ptr, bayer.Bgr.Ptr, COLOR_CONVERSION.CV_YUV2RGB);
                    p.Bgra.ConvertFrom(bayer.Bgr);
                    // bgra.Bgra.ConvertFrom<Bgr, byte>(bgr.Bgr);
                    // bayer.Bgra.
                    // bayer.Bgra.ConvertFrom(p.Bgr);
                    // bayer.Bgra.CopyTo(p.Bgra);
                    /*
                    for (int i = 0; i < bayer.Width * bayer.Height; ++i)
                        bayer.Bytes[i] = (byte)((((int)p.Bytes[i * 3])
                            | ((((int)p.Bytes[i * 3 + 1]) & 3) << 8)) / 4); ;

                    using (var demosaicked = bayer.DemosaickBayer8(COLOR_CONVERSION.CV_BayerBG2BGR))
                        demosaicked.Bgra.CopyTo(p.Bgra);
                    */
                }

                if (Global.No)
                {
                    if (chessboard.Calibrated)
                        chessboard.MaybeUndistort(p);
                    else
                        p.Bgra.Draw(String.Format("{0,2}/{1,2}", chessboard.Count, chessboard.Max), ref font, new System.Drawing.Point(10, 480 - 20), Color.AliceBlue.ToBgra());
                }

                if (pic != null)
                    pic.Release();
                pic = p;
            }

            if (chessboard.Calibrated)
            {
                var frustum = new BoundingFrustum(this.View * this.Projection);
                DebugShapeRenderer.AddBoundingFrustum(frustum, Color.HotPink);

                var orig = this.World.Translation;
                var corners = frustum.GetCorners();
                DebugShapeRenderer.AddTriangle(orig, Vector3.Lerp(corners[4], corners[7], 0.25f), Vector3.Lerp(corners[5], corners[6], 0.75f), Color.Green);
                DebugShapeRenderer.AddTriangle(orig, Vector3.Lerp(corners[4], corners[7], 0.75f), Vector3.Lerp(corners[5], corners[6], 0.25f), Color.DarkBlue);

                if (Global.No)
                    foreach (var kvp in rays.ToArray())
                        DebugShapeRenderer.AddLine(kvp.Key, kvp.Value, new Color(20, 20, 20, 80));

                frame.Add<DebugShapeRenderer>(Matrix.Identity);
                frame.Add<Primitives.Axes>(this.World);
            }

            if (pic != null)
            {
                pic.AddRef();
                var pct = 100;
                var rect = new Rectangle(this.cap.Name.Contains("CLEye") ? 0 : 640, 0, pic.Width * pct / 100, pic.Height * pct / 100);
                frame.Add<Primitives.Snapshot>(new Primitives.Snapshot.Context() { Picture = pic, Rect = rect, });
            }
        }

        private void DrawCalibratedFrustum(GraphicsDevice device, Matrix view, Matrix projection)
        {
            var plane = new Plane(Vector3.UnitZ, -0.01f);
            var frustum = new BoundingFrustum(this.World * this.View * this.Projection);
            frustum.GetCorners(corners);

            for (int i = 0; i < 4; ++i)
            {
                var ray = new Ray(corners[i], Vector3.Normalize(corners[i + 4] - corners[i]));
                float? isect;
                ray.Intersects(ref plane, out isect);
                if (isect.HasValue)
                {
                    corners[i] = ray.Position + isect.Value * ray.Direction;
                    DebugShapeRenderer.AddLine(this.World.Translation, corners[i], Color.Green);
                }
            }

            for (int i = 0; i < 4; ++i)
                DebugShapeRenderer.AddLine(corners[i], corners[(i + 1) % 4], Color.Green);
            DebugShapeRenderer.AddBoundingFrustum(frustum, Color.HotPink);
        }

        public void Draw(GraphicsDevice device, SpriteBatch spriteBatch, Rectangle rect)
        {
            if (texture2D == null)
                texture2D = new Texture2D(device, cap.Width, cap.Height, false, SurfaceFormat.Color);

            if (calib2D == null)
                calib2D = new Texture2D(device, cap.Width, cap.Height, false, SurfaceFormat.Color);

            spriteBatch.Draw(texture2D, rect, new Color(255, 255, 255, 255));
        }

        public Texture2D calib2D;
        public Picture calib2E;
        
        public Matrix View { get { return Matrix.Invert(this.World); } }
        public Matrix World { get; set; }
        public Matrix Projection { get; set; }
        public bool Localized = false;

        private Vector3[] corners = new Vector3[8];
        private Capture cap;
        private Picture pic;
        private Texture2D texture2D;
        private Chessboard chessboard;

        private static MCvFont font= new MCvFont(FONT.CV_FONT_HERSHEY_DUPLEX, 4.0f, 4.0f);
    }
}
