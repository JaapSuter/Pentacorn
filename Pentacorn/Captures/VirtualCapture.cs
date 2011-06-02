using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Timer = System.Windows.Forms.Timer;

namespace Pentacorn.Captures
{
    class VirtualCapture : Capture
    {
        public Scene Scene { get; set; }

        public VirtualCapture(string name, string uuid, Size size, int fps, int numRadialDistortionCoefficients = 0, Matrix<float> undistortMapX = null, Matrix<float> undistortMapY = null)
            : base(name, uuid, size)
        {
            NumRadialDistortionCoefficients = numRadialDistortionCoefficients;
            UndistortMapX = undistortMapX;
            UndistortMapY = undistortMapY;

            Rgba = new Picture<Rgba, byte>(Width, Height);
            Gray = new Picture<Gray, byte>(Width, Height);
            
            Timer = new Timer();
            Timer.Interval = 1000 / fps;
            Timer.Tick += Update;
            Timer.Start();
        }
        
        public override void Close()
        {
            Timer.Stop();
        }

        private void Update(object sender, EventArgs e)
        {
            if (Scene == null)
                return;

            // Todo
            //      ...
            // using (var rgba = new Picture<Rgba, byte>(Width, Height))
            // using (var gray = new Picture<Gray, byte>(Width, Height))
            // {

            Program.EnsureRendering();
            Program.Renderer.RenderInto(Scene, Rgba);

            Gray.Dispose();
            Gray = new Picture<Gray, byte>(Width, Height);            
            Gray.Emgu.ConvertFrom(Rgba.Emgu);

            if (UndistortMapX != null && UndistortMapY != null)
                Gray.Remap(UndistortMapX, UndistortMapY);

            OnNext(Gray.AddRef(), Rgba.AddRef());
        }

        private Picture<Rgba, byte> Rgba;
        private Picture<Gray, byte> Gray;

        private Matrix<float> UndistortMapX;
        private Matrix<float> UndistortMapY;
        private Timer Timer;
    }
}
