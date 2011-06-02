using System;
using System.Collections.Concurrent;
using System.Disposables;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Captures
{
    abstract class Capture
    {
        public string Uuid { get; private set; }
        public string Name { get; private set; }

        public Size Size { get; private set; }

        public int Width { get { return Size.Width; } }
        public int Height { get { return Size.Height; } }

        public Texture2D GrayTexture2D { get; private set; }
        public Texture2D RgbaTexture2D { get; private set; }

        public int NumRadialDistortionCoefficients { get; protected set; }

        public virtual TimeSpan Exposure
        {
            get { { return TimeSpan.Zero; } }
            set { }
        }

        public virtual void Close() { }

        public Capture(string name, string uuid, Size resolution)            
        {
            Name = name;
            Uuid = uuid;
            Size = resolution;
            NumRadialDistortionCoefficients = 2;
            GrayTexture2D = Program.Renderer.LeaseFor(this, colorize: false);
            RgbaTexture2D = Program.Renderer.LeaseFor(this, colorize: true);
        }
        
        public Task<Picture<Gray, byte>> NextGray()
        {
            var tcs = new TaskCompletionSource<Picture<Gray, byte>>();
            GrayReq.Enqueue(tcs);
            return tcs.Task;
        }

        public Task<Picture<Rgba, byte>> NextRgba()
        {
            var tcs = new TaskCompletionSource<Picture<Rgba, byte>>();
            RgbaReq.Enqueue(tcs);
            return tcs.Task;
        }

        protected void OnNext(Picture<Gray, byte> gray, Picture<Rgba, byte> rgba)
        {
            UpdateTexture2D(gray.AddRef(), rgba.AddRef());

            using (gray)
            {
                GrayReq.Enqueue(null);
                TaskCompletionSource<Picture<Gray, byte>> req = null;
                while (GrayReq.TryDequeue(out req) && req != null)
                    req.SetResult(gray.AddRef());
            }

            using (rgba)
            {
                RgbaReq.Enqueue(null);
                TaskCompletionSource<Picture<Rgba, byte>> ceq = null;
                while (RgbaReq.TryDequeue(out ceq) && ceq != null)
                    ceq.SetResult(rgba.AddRef());                
            }
        }

        private async void UpdateTexture2D(Picture<Gray, byte> gray, Picture<Rgba, byte> rgba)
        {
            await Program.SwitchToRender();
                
            using (rgba)
            using (gray)
            {
                GrayTexture2D.SetData(gray.Bytes);
                RgbaTexture2D.SetData(rgba.Bytes);
            }
        }
        
        private ConcurrentQueue<TaskCompletionSource<Picture<Gray, byte>>> GrayReq = new ConcurrentQueue<TaskCompletionSource<Picture<Gray, byte>>>();
        private ConcurrentQueue<TaskCompletionSource<Picture<Rgba, byte>>> RgbaReq = new ConcurrentQueue<TaskCompletionSource<Picture<Rgba, byte>>>();        
    }
}
