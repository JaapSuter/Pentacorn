using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Capture = Pentacorn.Captures.Capture;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System.Disposables;
using System.Threading;

namespace Pentacorn.Graphics
{
    class CaptureCamera : Camera, IVisible
    {
        public Capture Capture { get; private set; }
        
        public CaptureCamera(Capture capture, int numRadialDistortionCoefficients)
            : this(capture, capture.Size.CenterF(), GetDefaultFocalLength(capture.Size), DefaultNearPlaneDistance, DefaultFarPlaneDistance, numRadialDistortionCoefficients)
        {}

        public CaptureCamera(Capture capture, PointF principalPointEstimate, double focalLength, double nearPlaneDistance, double farPlaneDistance, int numRadialDistortionCoefficients)
            : base(capture.Name, capture.Uuid, capture.Size, principalPointEstimate, focalLength, nearPlaneDistance, farPlaneDistance, numRadialDistortionCoefficients)
        {
            Capture = capture;            
            Quad = new Quad(capture.Size, Capture.RgbaTexture2D) { Color = Color.White.Alpha(0.4f), };
            Quad.World = Matrix.CreateTranslation(0, 0, 0.1f);
        }

        public PointF[] ToWorld(PointF[] captureCameraPoints)
        {
            var worldPoints = captureCameraPoints.ToArray();
            CameraToWorld.ProjectPoints(worldPoints);
            return worldPoints;
        }

        public override void Locate(PointF[] worldPoints, PointF[] imagePoints)
        {
            base.Locate(worldPoints, imagePoints);
            
            var zBias = -0.01f;
            var zBiasTranslate = Matrix.CreateTranslation(0, 0, zBias);
            CameraToWorld = CameraCalibration.FindHomography(imagePoints, worldPoints, HOMOGRAPHY_METHOD.LMEDS, 0);
            Quad.Homography = CameraToWorld.ToMatrixFromHomogeneous3x3() * zBiasTranslate;
        }

        public override void Render(Renderer renderer, IViewProject viewProject)
        {
            renderer.Render(viewProject, Quad);
            base.Render(renderer, viewProject);
        }

        private HomographyMatrix CameraToWorld;
        private Quad Quad;
    }
}
