using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Pentacorn.Vision.Captures;
using Pentacorn.Vision.Captures.DirectShow;
using Rectangle = System.Drawing.Rectangle;

namespace Pentacorn.Vision.Captures
{
    class AForgeCapture : Capture, ICloseable
    {
        public override Task CloseAsync()
        {
            var steal = Interlocked.Exchange(ref device, null);
            if (steal == null)
                return Global.Done;
            
                steal.SignalToStop();                
            return TaskEx.Run(() =>
            {
                steal.WaitForStop();
                base.Close();
            });
        }

        private AForgeCapture(string name, string deviceMoniker)
        {
            this.Name = name;
            this.Uuid = deviceMoniker;

            var width = Picture.Width;
            var height = Picture.Height;

            device = new VideoCaptureDevice(deviceMoniker);
            var caps = device.VideoCapabilities
                             .Where(c => c.FrameSize.Width == width && c.FrameSize.Height == height)
                             .OrderByDescending(c => c.FrameSize.Width * c.FrameSize.Height * c.MaxFrameRate).First();

            this.Width = caps.FrameSize.Width;
            this.Height = caps.FrameSize.Height;
            device.DesiredFrameRate = caps.MaxFrameRate;
            device.DesiredFrameSize = caps.FrameSize;
            device.NewFrame += new NewFrameEventHandler(OnNewFrameEvent);
            device.Start();
        }

        private void OnNewFrameEvent(object sender, NewFrameEventArgs eventArgs)
        {
            var bmp = eventArgs.Frame;
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            var picture = Picture.Alloc(this.Width, this.Height);
            Marshal.Copy(data.Scan0, picture.Bytes, 0, picture.Bytes.Length / 4 * 3);            
            bmp.UnlockBits(data);
            Enqueue(ref picture);
        }

        private VideoCaptureDevice device;
    }
}
