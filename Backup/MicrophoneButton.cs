using System;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn.Graphics;
using System.Threading.Tasks;
using System.Threading;
using Capture = Pentacorn.Captures.Capture;
using Rectangle = System.Drawing.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using System.Concurrency;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Pentacorn
{
    class MicrophoneButton
    {
        public static IEnumerable<MicrophoneButton> Devices = Microphone.All.Where(m => m == Microphone.Default).Select(m => new MicrophoneButton(m));

        private MicrophoneButton(Microphone microphone)
        {
            FrameworkDispatcher.Update();
                        
            var minSupportedBufferDurationMs = 100;
            var duration = TimeSpan.FromMilliseconds(minSupportedBufferDurationMs);

            Microphone = microphone;
            Microphone.BufferDuration = duration;

            Buffer = new byte[Microphone.GetSampleSizeInBytes(duration)];
            Buffer2D = new Color[TexWidth * byte.MaxValue];

            Texture2D = Texture2D ?? Program.Renderer.LeaseFor<Gray, byte>(TexWidth, byte.MaxValue);
            Texture2D.SetData(Buffer2D);

            Microphone.BufferReady += OnBufferReady;

            Microphone.Start();            
        }

        private void OnBufferReady(object s, EventArgs e)
        {
            var available = Microphone.GetData(Buffer);
            var len = Math.Min(TexWidth, available);
            for (int y = 0; y < byte.MaxValue; ++y)
                for (int x = 0; x < len; ++x)
                    Buffer2D[x + (byte.MaxValue - y - 1) * TexWidth] = y < Buffer[available - len + x] ? Color.Red : Color.Blue;

            Texture2D.SetData(Buffer2D);
        }

        public Texture2D Texture2D;
        private int TexWidth = 1680;
        private Microphone Microphone;
        private byte[] Buffer;
        private Color[] Buffer2D;
    }
}
