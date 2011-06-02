using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pentacorn.Vision.Captures
{
    abstract class Capture
    {
        public static IEnumerable<Capture> AllDevices { get { return DirectShowCapture.Devices.Concat(CLEyeCapture.Devices); } }

        public string Uuid { get; protected set; }
        public string Name { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public abstract TimeSpan Exposure { set; }

        public Picture TryDequeue()
        {
            Statistics.Update("Capture.TryDequeue[" + this.Name + "-" + this.Uuid + "]");
            return Interlocked.Exchange(ref latest, null);
        }

        public void Enqueue(Picture bgra)
        {
            bgra.AddRef();
            var steal = Interlocked.Exchange(ref latest, bgra);
            if (steal != null)
                steal.Release();

            Statistics.Update("Capture.Enqueue[" + this.Name + "-" + this.Uuid + "]");
        }

        private Picture latest;
    }
}
