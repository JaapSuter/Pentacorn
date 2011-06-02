using System;
using System.Collections.Generic;
using System.Disposables;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Pentacorn.Graphics
{
    abstract class Camera : IViewProject, IVisible
    {
        public Matrix World { get; protected set; }
        public Matrix View { get; protected set; }
        public Matrix Projection { get { return Intrinsics.Projection; } }
        public Frustum Frustum { get; protected set; }
        public Intrinsics Intrinsics { get; private set; }
        public Color Color { get { return Frustum.Color; } set { Frustum.Color = value; } }

        public bool Highlight { get; set; }

        public string Name { get; private set; }
        public string Uuid { get; private set; }

        public Camera(string name, string uuid, Size imageSize)
            : this(name, uuid, imageSize, imageSize.CenterF(), DefaultNearPlaneDistance, DefaultFarPlaneDistance, GetDefaultFocalLength(imageSize), DefaultNumRadialDistortionCoefficients)
        {}

        public Camera(string name, string uuid, Size imageSize, PointF principalPointEstimate, double focalLength, double nearPlaneDistance, double farPlaneDistance, int numRadialDistortionCoefficients)
        {
            PathToIntrinsics = GetPathToIntrinsics(name, uuid);
            Name = name;
            Uuid = uuid;
            NumRadialDistortionCoefficients = numRadialDistortionCoefficients;

            World = Matrix.Identity;
            View = Matrix.Identity;

            if (!String.IsNullOrWhiteSpace(PathToIntrinsics))
                if (File.Exists(PathToIntrinsics))
                    using (var fs = File.OpenRead(PathToIntrinsics))
                        Intrinsics = new Intrinsics(imageSize, nearPlaneDistance, farPlaneDistance, fs);
            
            if (Intrinsics == null)
                Intrinsics = new Intrinsics(imageSize, principalPointEstimate, focalLength, nearPlaneDistance, farPlaneDistance, numRadialDistortionCoefficients);

            Frustum = new Frustum(imageSize, this);
            Text = new Text(Name, World.Translation, Color.Black);
        }

        public virtual void Render(Renderer renderer, IViewProject viewProject)
        {
            Frustum.Update(this);

            renderer.Render(viewProject, new Sphere(World.Translation, 1, Color));
            renderer.Render(viewProject, Frustum);

            if (Highlight)
                renderer.Render(viewProject, new Circle2D(World.Translation, 60, 20, Color));
        }

        public virtual void Locate(PointF[] worldPoints, PointF[] imagePoints)
        {
            View = Intrinsics.LocateExtrinsics(worldPoints, imagePoints);
            World = Matrix.Invert(View);
        }

        public async Task CalibrateAsync(IEnumerable<PointF[]> listOfWorldPoints, IEnumerable<PointF[]> listOfImagePoints)
        {
            await Program.SwitchToCompute();            
            
            Intrinsics.Recalibrate(Intrinsics.ImageSize, listOfWorldPoints, listOfImagePoints, NumRadialDistortionCoefficients);            
        }

        public void Save()
        {
            using (var fs = File.Create(PathToIntrinsics))
                Intrinsics.Save(Name, Uuid, fs);
        }

        public override string ToString()
        {
            var fmt = "{0}\n" +
                      "Resolution: ({1}x{2})\n" +
                      "Principal: ({3:0.00}, {4:0.00})\n" +
                      "Focal Length: {5:0.00}\n" +
                      "H/V Fov: {6:0.00} {7:0.00}\n" +
                      "Reprojection Error: {8:0.00}"; 
            
            return fmt.FormatWith(Name,
                        Intrinsics.ImageSize.Width, Intrinsics.ImageSize.Height,
                        Intrinsics.PrincipalPoint.X, Intrinsics.PrincipalPoint.Y,
                        Intrinsics.FocalLength,
                        Intrinsics.HorizontalFov, Intrinsics.VerticalFov,
                        Intrinsics.ReprojectionError);
        }

        private static string GetPathToIntrinsics(string name, string uuid)
        {
            var dir = Path.Combine(Global.DatDir, name, uuid.ToSanitizedFileName());

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!Directory.Exists(dir)) throw new Exception(String.Format("Unable to create directory at '{0}'.", dir));

            var fileName = "intrinsics.txt";
            var file = Path.Combine(dir, fileName);

            return file;
        }
                
        private int NumRadialDistortionCoefficients;
        private string PathToIntrinsics;
        private Text Text;

        protected static double GetDefaultFocalLength(Size imageSize)
        {
            return 1.2 * Math.Max(imageSize.Width, imageSize.Height);
        }

        protected const double DefaultNearPlaneDistance = 1;
        protected const double DefaultFarPlaneDistance = 100;
        protected const int DefaultNumRadialDistortionCoefficients = 1;
    }
}
