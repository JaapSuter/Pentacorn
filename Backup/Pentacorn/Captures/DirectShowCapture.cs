using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Pentacorn.Captures.DirectShow;
using Pentacorn.Captures.DirectShow.Internals;
using FilterInfo = Pentacorn.Captures.DirectShow.FilterInfo;
using Size = System.Drawing.Size;

namespace Pentacorn.Captures
{
    class DirectShowCapture : Capture, ISampleGrabberCB
    {
        public static IList<Capture> Devices { get; private set; }

        public DirectShowCapture(string name, string deviceMoniker)
            : base(name, deviceMoniker, new Size(800, 600))
        {
            Start();
        }

        public override void Close()
        {
            MediaControl.Stop();
            // Todo, Jaap Suter, November 2010, properly release COM objects.
        }

        public override TimeSpan Exposure
        {
            set
            {
                IBaseFilter bf;
                FilterInfo.CreateFilter(this.Uuid, out bf);
                ((IKsPropertySet)bf).SetExposure(value);
                Marshal.ReleaseComObject(bf);
            }
            get
            {
                IBaseFilter bf;
                FilterInfo.CreateFilter(this.Uuid, out bf);
                var ret = ((IKsPropertySet)bf).GetExposure();
                Marshal.ReleaseComObject(bf);
                return ret;
            }
        }

        public int SampleCB(double sampleTime, IMediaSample sample)
        {
            // Sample grabber implementation, not used.
            return 0;
        }

        public int BufferCB(double sampleTime, IntPtr buffer, int len)
        {
            using (var rgba = new Picture<Rgba, byte>(Width, Height))
            using (var gray = new Picture<Gray, byte>(Width, Height))
            {
                if ((len / 3) == (Width * Height))
                {
                    using (var bgr = new Image<Bgr, byte>(Width, Height, Width * 3, buffer))
                    {
                        bgr._Flip(FLIP.VERTICAL);
                        CvInvoke.cvCvtColor(bgr.Ptr, rgba.Emgu.Ptr, COLOR_CONVERSION.CV_BGR2RGBA);
                        CvInvoke.cvCvtColor(bgr.Ptr, gray.Emgu.Ptr, COLOR_CONVERSION.CV_BGR2GRAY);
                    }
                }
                else
                {
                    gray.Errorize();
                    rgba.Errorize();
                }

                OnNext(gray.AddRef(), rgba.AddRef());
            }

            return 0;
        }

        private async void Start()
        {
            await ThreadPoolEx.SwitchTo();

            CapGraphBuilder2 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2)) as ICaptureGraphBuilder2;
            FilterGraph2 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph)) as IFilterGraph2;

            SampleGrabberBaseFilter = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SampleGrabber)) as IBaseFilter;
            SampleGrabber = SampleGrabberBaseFilter as ISampleGrabber;

            CapGraphBuilder2.SetFiltergraph(FilterGraph2 as IGraphBuilder);

            FilterInfo.CreateFilter(this.Uuid, out CaptureSourceBaseFilter);
            CaptureSourceBaseFilter.SetSyncSource(IntPtr.Zero);
            SampleGrabberBaseFilter.SetSyncSource(IntPtr.Zero);

            VideoProcAmp = CaptureSourceBaseFilter as IAMVideoProcAmp;
            CameraControl = CaptureSourceBaseFilter as IAMCameraControl;
            KsPropertySet = CaptureSourceBaseFilter as IKsPropertySet;

            VideoProcAmp.Set(VideoProcAmpProperty.ColorEnable, 1, VideoProcAmpFlags.Manual);
            KsPropertySet.SetExposure(TimeSpan.FromSeconds(1000 / 120));

            FilterGraph2.AddFilter(CaptureSourceBaseFilter, "source");
            FilterGraph2.AddFilter(SampleGrabberBaseFilter, "grabber");

            object streamConfigObj;
            CapGraphBuilder2.FindInterface(PinCategory.Capture, MediaType.Video, CaptureSourceBaseFilter, typeof(IAMStreamConfig).GUID, out streamConfigObj);
            IAMStreamConfig streamConfig = (IAMStreamConfig)streamConfigObj;

            VideoCaps = Pentacorn.Captures.DirectShow.VideoCapabilities.FromStreamConfig(streamConfig);

            var desiredFormat = VideoCaps.Where(vc => vc.FrameSize.Width == this.Width && vc.FrameSize.Height == this.Height)
                                                 .OrderByDescending(vc => vc.MaxFrameRate).First();
            streamConfig.SetFormat(desiredFormat.MediaType);

            var hr = SampleGrabber.SetMediaType(desiredFormat.MediaType);
            if (hr < 0)
                throw new Win32Exception(hr);

            SampleGrabber.SetBufferSamples(true);
            SampleGrabber.SetOneShot(false);
            SampleGrabber.SetCallback(this, 1);

            CapGraphBuilder2.RenderStream(PinCategory.Capture, MediaType.Video, CaptureSourceBaseFilter, null, SampleGrabberBaseFilter);
            if (hr < 0)
                throw new Win32Exception(hr);

            AMMediaType mediaType = new AMMediaType();
            if (SampleGrabber.GetConnectedMediaType(mediaType) >= 0)
            {
                VideoInfoHeader vih = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));

                if (this.Width != vih.BmiHeader.Width)
                    throw new Exception("DirectShow capture width not what's requested.");
                if (this.Height != vih.BmiHeader.Height)
                    throw new Exception("DirectShow capture width not what's requested.");
                mediaType.Dispose();
            }

            MediaControl = (IMediaControl)FilterGraph2;
            MediaControl.Run();
        }

        private void Write(TextWriter tw)
        {
            foreach (var prop in Enum.GetValues(typeof(CameraControlProperty)).Cast<CameraControlProperty>())
            {
                int min, max, delta, deflt, current;
                CameraControlFlags flags, curFlags;
                CameraControl.GetRange(prop, out min, out max, out delta, out deflt, out flags);
                CameraControl.Get(prop, out current, out curFlags);
                tw.WriteLine("{0}: {6} [{1}-{2}, step {3}, dflt {4}, flags {7} [{5}]]", prop.ToString().Truncate(15).PadLeft(15), min, max, delta, deflt, flags, current, curFlags);
            }

            foreach (var prop in Enum.GetValues(typeof(VideoProcAmpProperty)).Cast<VideoProcAmpProperty>())
            {
                int min, max, delta, deflt, current;
                VideoProcAmpFlags flags, curFlags;
                VideoProcAmp.GetRange(prop, out min, out max, out delta, out deflt, out flags);
                VideoProcAmp.Get(prop, out current, out curFlags);
                tw.WriteLine("{0}: {6} [{1}-{2}, step {3}, dflt {4}, flags {7} [{5}]]", prop.ToString().Truncate(15).PadLeft(15), min, max, delta, deflt, flags, current, curFlags);
            }
        }

        static DirectShowCapture()
        {
            var query = from fi in new FilterInfoCollection(FilterCategory.VideoInputDevice).Cast<FilterInfo>()
                        where !fi.Name.Contains("PS3Eye Camera")
                        select new DirectShowCapture(fi.Name, fi.MonikerString);

            Devices = query.ToArray();
        }

        private VideoCapabilities[] VideoCaps;
        private IAMVideoProcAmp VideoProcAmp;
        private IAMCameraControl CameraControl;
        private IKsPropertySet  KsPropertySet;
        private ICaptureGraphBuilder2 CapGraphBuilder2;
        private IFilterGraph2 FilterGraph2;
        private IBaseFilter CaptureSourceBaseFilter;
        private IBaseFilter SampleGrabberBaseFilter;
        private ISampleGrabber SampleGrabber;
        private IMediaControl MediaControl;
    }
}
