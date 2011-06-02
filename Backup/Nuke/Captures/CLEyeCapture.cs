using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;

namespace Pentacorn.Vision.Captures
{
    sealed class CLEyeCapture : Capture
    {
        public static IList<Capture> Devices { get; private set; }

        public void Close()
        {
            close = true;
            if (thread.IsAlive)
                thread.Join();
        }

        private CLEyeCapture(Guid guid)
        {
            this.Name = "CLEye";
            this.Uuid = guid.ToString();

            var fps = 20;
            var mode = ColorMode.CLEYE_COLOR_RAW;
            var resolution = Resolution.CLEYE_VGA;

            cam = CLEyeCreateCamera(guid, mode, resolution, fps);
            if (cam == IntPtr.Zero)
                throw new Exception("Unable to obtain cam to CLEye camera device.");

            int w = 0, h = 0;
            CLEyeCameraGetFrameDimensions(cam, ref w, ref h);

            this.Width = w;
            this.Height = h;

            thread = new Thread(this.Runner);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Runner()
        {
            CLEyeCameraLED(cam, false);

            var exp = 50;
            var wb = 255;
            var gain = 0;

            CLEyeSetCameraParameter(cam, Param.CLEYE_AUTO_EXPOSURE_0_1, 0);
            CLEyeSetCameraParameter(cam, Param.CLEYE_EXPOSURE_0_511, exp);

            CLEyeSetCameraParameter(cam, Param.CLEYE_AUTO_GAIN_0_1, 0);
            CLEyeSetCameraParameter(cam, Param.CLEYE_GAIN_0_79, gain);

            CLEyeSetCameraParameter(cam, Param.CLEYE_AUTO_WHITEBALANCE_0_1, 0);
            CLEyeSetCameraParameter(cam, Param.CLEYE_WHITEBALANCE_RED_0_255, wb);
            CLEyeSetCameraParameter(cam, Param.CLEYE_WHITEBALANCE_BLUE_0_255, wb);
            CLEyeSetCameraParameter(cam, Param.CLEYE_WHITEBALANCE_GREEN_0_255, wb);

            if (Global.No)
            {
                CLEyeSetCameraParameter(cam, Param.CLEYE_AUTO_WHITEBALANCE_0_1, 1);
                CLEyeSetCameraParameter(cam, Param.CLEYE_AUTO_GAIN_0_1, 1);
                CLEyeSetCameraParameter(cam, Param.CLEYE_AUTO_EXPOSURE_0_1, 1);
            }

            CLEyeCameraStart(cam);

            while (!close)
                Update();

            if (cam != IntPtr.Zero)
            {
                CLEyeCameraStop(cam);
                CLEyeDestroyCamera(cam);
                cam = IntPtr.Zero;
            }
        }

        public override TimeSpan Exposure
        {
            set
            {
                // Todo, 
            }
        }
        
        private void Update()
        {
            var waitTimeOutMs = 2000;
            using (var pict = new Picture(this.Width, this.Height))
            {
                var ok = CLEyeCameraGetFrame(cam, pict.Ptr, waitTimeOutMs);
                if (!ok)
                    pict.Bgra.SetValue(Color.Green.ToBgra());

                Enqueue(pict);
                // using (var demosaicked = pict.DemosaickBayer8(COLOR_CONVERSION.CV_BayerGB2BGR_VNG))
                    // Enqueue(demosaicked);
            }
        }

        private enum ColorMode
        {
            CLEYE_MONO_PROCESSED,
            CLEYE_COLOR_PROCESSED,
            CLEYE_MONO_RAW,
            CLEYE_COLOR_RAW,
            CLEYE_BAYER_RAW
        };

        private enum Resolution
        {
            CLEYE_QVGA,
            CLEYE_VGA
        };

        private enum Param
        {
            CLEYE_AUTO_GAIN_0_1,
            CLEYE_GAIN_0_79,
            CLEYE_AUTO_EXPOSURE_0_1,
            CLEYE_EXPOSURE_0_511,
            CLEYE_AUTO_WHITEBALANCE_0_1,
            CLEYE_WHITEBALANCE_RED_0_255,
            CLEYE_WHITEBALANCE_GREEN_0_255,
            CLEYE_WHITEBALANCE_BLUE_0_255,
            CLEYE_HFLIP_0_1,
            CLEYE_VFLIP_0_1,
            CLEYE_HKEYSTONE_PLUS_MINUS_500,
            CLEYE_VKEYSTONE_PLUS_MINUS_500,
            CLEYE_XOFFSET_PLUS_MINUS_500,
            CLEYE_YOFFSET_PLUS_MINUS_500,
            CLEYE_ROTATION_PLUS_MINUS_500,
            CLEYE_ZOOM_PLUS_MINUS_500,
            CLEYE_LENSCORRECTION1_PLUS_MINUS_500,
            CLEYE_LENSCORRECTION2_PLUS_MINUS_500,
            CLEYE_LENSCORRECTION3_PLUS_MINUS_500,
            CLEYE_LENSBRIGHTNESS_PLUS_MINUS_500
        };

        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CLEyeGetCameraCount();
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Guid CLEyeGetCameraUUID(int camId);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CLEyeCreateCamera(Guid camUUID, ColorMode mode, Resolution res, float frameRate);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeDestroyCamera(IntPtr camera);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeCameraStart(IntPtr camera);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeCameraStop(IntPtr camera);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeCameraLED(IntPtr camera, bool on);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeSetCameraParameter(IntPtr camera, Param param, int value);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CLEyeGetCameraParameter(IntPtr camera, Param param);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeCameraGetFrameDimensions(IntPtr camera, ref int width, ref int height);
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool CLEyeCameraGetFrame(IntPtr camera, IntPtr pData, int waitTimeout);

        static CLEyeCapture()
        {
            var count = CLEyeGetCameraCount();
            Devices = new CLEyeCapture[count];
            for (int i = 0; i < count; ++i)
                Devices[i] = new CLEyeCapture(CLEyeGetCameraUUID(i));
        }

        private IntPtr cam;
        private Thread thread;
        private volatile bool close;
    }
}