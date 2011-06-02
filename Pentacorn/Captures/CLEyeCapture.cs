using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Emgu.CV.Structure;
using Color = Microsoft.Xna.Framework.Color;
using Size = System.Drawing.Size;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Pentacorn.Captures
{
    class CLEyeCapture : Capture
    {
        public static IList<Capture> Devices { get; private set; }

        public Color WhiteBalance
        {
            get
            {
                return new Color(CLEyeGetCameraParameter(Handle, Param.CLEYE_WHITEBALANCE_RED_0_255),
                                 CLEyeGetCameraParameter(Handle, Param.CLEYE_WHITEBALANCE_BLUE_0_255),
                                 CLEyeGetCameraParameter(Handle, Param.CLEYE_WHITEBALANCE_GREEN_0_255));
            }         
            private set
            {
                CLEyeSetCameraParameter(Handle, Param.CLEYE_WHITEBALANCE_RED_0_255, value.R);
                CLEyeSetCameraParameter(Handle, Param.CLEYE_WHITEBALANCE_BLUE_0_255, value.G);
                CLEyeSetCameraParameter(Handle, Param.CLEYE_WHITEBALANCE_GREEN_0_255, value.B);
            }
        }
        
        public int Gain
        {
            get { return CLEyeGetCameraParameter(Handle, Param.CLEYE_GAIN_0_79); }
            private set { CLEyeSetCameraParameter(Handle, Param.CLEYE_GAIN_0_79, value); }
        }
        
        public override TimeSpan Exposure
        {
            // Taken from the CLEye forums...
            //
            // Exposure depends on the capture resolution. Assuming that you are capturing at 640x480 the exposure is as following:
            //
            //      Texposure = ExposureValue * Trow
            //
            // There are total of 510 rows between two VSync periods. So for example if you are capturing at 30fps this time amounts to:
            //
            // Trow = (1/30) / 510 = 6.5359e-5 s
            // 
            // So for your desired exposure time of 1/250, the ExposureValue would be:
            // 
            // ExposureValue = (1/250) / Trow = 61.2 ~ 61
            //
            // Therefore if you capture at 30fps at 640x480 resolution, in order to get
            // exposure time of 1/250 s you would set the sensor exposure parameter value ExposureValue=61.
            // 
            // The thing to remember is that at:
            //      1. 640x480 there are total of 510*Trow between two consecutive Vsync periods.
            //      2. 320x480 there are total of 278*Trow between two consecutive Vsync periods.
            get
            {
                var ev = CLEyeGetCameraParameter(Handle, Param.CLEYE_EXPOSURE_0_511);
                return TimeSpan.FromSeconds(ev * ExpTeeRow);                
            }
            set
            {
                var ev = value.TotalSeconds / ExpTeeRow;
                CLEyeSetCameraParameter(Handle, Param.CLEYE_EXPOSURE_0_511, (int)ev.Clamp(0, 511));
            }
        }

        private int ExpRows { get { return Resolution == ResolutionMode.CLEYE_QVGA ? 278 : 510; } }
        private double ExpTeeRow { get { return (1.0 / FPS) / ExpRows; } }

        public override void Close()
        {
            Running = false;
            if (Thread.IsAlive)
                Thread.Join();
        }

        private static Size ResolutionToSize(ResolutionMode rm) { return rm == ResolutionMode.CLEYE_QVGA ? new Size(320, 240) : new Size(640, 480); }

        private CLEyeCapture(Guid guid)
            : base("CLEye Camera", guid.ToString(), ResolutionToSize(Resolution))
        {
            Handle = CLEyeCreateCamera(guid, Mode, Resolution, FPS);
            if (Handle == IntPtr.Zero)
                throw new Exception("Unable to obtain Handle to CLEye camera device.");

            int w = 0, h = 0;
            CLEyeCameraGetFrameDimensions(Handle, ref w, ref h);

            Thread = new Thread(this.Runner);
            Thread.IsBackground = true;
            Thread.Start();
        }

        private void Runner()
        {
            Exposure = TimeSpan.FromSeconds(0.9 / FPS);
            WhiteBalance = Color.White;
            Gain = 0;

            CLEyeSetCameraParameter(Handle, Param.CLEYE_AUTO_EXPOSURE_0_1, 0);
            CLEyeSetCameraParameter(Handle, Param.CLEYE_AUTO_GAIN_0_1, 0);
            CLEyeSetCameraParameter(Handle, Param.CLEYE_AUTO_WHITEBALANCE_0_1, 0);
            
            if (Global.No)
            {
                CLEyeSetCameraParameter(Handle, Param.CLEYE_AUTO_WHITEBALANCE_0_1, 1);
                CLEyeSetCameraParameter(Handle, Param.CLEYE_AUTO_GAIN_0_1, 1);
                CLEyeSetCameraParameter(Handle, Param.CLEYE_AUTO_EXPOSURE_0_1, 1);
            }

            CLEyeCameraStart(Handle);

            while (Running)
            {
                using (var rgba = new Picture<Rgba, byte>(this.Width, this.Height))
                using (var gray = new Picture<Gray, byte>(this.Width, this.Height))
                using (var bayer = new Picture<Gray, byte>(this.Width, this.Height))
                using (var bgr  = new Picture<Bgr, byte>(this.Width, this.Height))
                {
                    var waitTimeOutMs = 2000;
                    var ok = NumChannels == 1
                           ? CLEyeCameraGetFrame(Handle, bayer.Ptr, waitTimeOutMs)
                           : false;
                    if (!ok)
                        bayer.Errorize();

                    CvInvoke.cvCvtColor(bayer.Emgu.Ptr, bgr.Emgu.Ptr, COLOR_CONVERSION.CV_BayerGB2BGR_VNG);
                    CvInvoke.cvCvtColor(bgr.Emgu.Ptr, rgba.Emgu.Ptr, COLOR_CONVERSION.CV_BGR2RGBA);
                    CvInvoke.cvCvtColor(bgr.Emgu.Ptr, gray.Emgu.Ptr, COLOR_CONVERSION.CV_BGR2GRAY);
                    
                    OnNext(gray.AddRef(), rgba.AddRef());
                }
            }

            if (Handle != IntPtr.Zero)
            {
                CLEyeCameraStop(Handle);
                CLEyeDestroyCamera(Handle);
                Handle = IntPtr.Zero;
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

        private enum ResolutionMode
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
        private static extern IntPtr CLEyeCreateCamera(Guid camUUID, ColorMode mode, ResolutionMode res, float frameRate);
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

        private readonly int FPS = 30;
        private const int NumChannels = (Mode == ColorMode.CLEYE_COLOR_PROCESSED || Mode == ColorMode.CLEYE_COLOR_RAW) ? 4 : 1;
        private const ColorMode Mode = ColorMode.CLEYE_BAYER_RAW;
        private const ResolutionMode Resolution = ResolutionMode.CLEYE_VGA;
        private IntPtr Handle;
        private Thread Thread;
        private volatile bool Running = true;
    }
}
