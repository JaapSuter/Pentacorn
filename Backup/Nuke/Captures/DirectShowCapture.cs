using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Pentacorn.Vision.Captures.DirectShow;
using Pentacorn.Vision.Captures.DirectShow.Internals;
using FilterInfo = Pentacorn.Vision.Captures.DirectShow.FilterInfo;

namespace Pentacorn.Vision.Captures
{
    class DirectShowCapture : Capture, ISampleGrabberCB
    {
        public static IList<Capture> Devices { get; private set; }

        public DirectShowCapture(string name, string deviceMoniker)
        {
            this.Name = name;
            this.Uuid = deviceMoniker;
            this.Width = 1600;
            this.Height = 1200;

            captureGraphBuilder2 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2)) as ICaptureGraphBuilder2;
            filterGraph2 = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph)) as IFilterGraph2;

            sampleGrabberBaseFilter = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SampleGrabber)) as IBaseFilter;
            sampleGrabber = sampleGrabberBaseFilter as ISampleGrabber;

            captureGraphBuilder2.SetFiltergraph(filterGraph2 as IGraphBuilder);

            FilterInfo.CreateFilter(this.Uuid, out captureSourceBaseFilter);
            captureSourceBaseFilter.SetSyncSource(IntPtr.Zero);
            sampleGrabberBaseFilter.SetSyncSource(IntPtr.Zero);

            videoProcAmp = captureSourceBaseFilter as IAMVideoProcAmp;
            cameraControl = captureSourceBaseFilter as IAMCameraControl;
            ksPropertySet = captureSourceBaseFilter as IKsPropertySet;

            videoProcAmp.Set(VideoProcAmpProperty.ColorEnable, 1, VideoProcAmpFlags.Manual);
            ksPropertySet.SetExposure(TimeSpan.FromSeconds(1000 / 120));

            filterGraph2.AddFilter(captureSourceBaseFilter, "source");
            filterGraph2.AddFilter(sampleGrabberBaseFilter, "grabber");

            object streamConfigObj;
            captureGraphBuilder2.FindInterface(PinCategory.Capture, MediaType.Video, captureSourceBaseFilter, typeof(IAMStreamConfig).GUID, out streamConfigObj);
            IAMStreamConfig streamConfig = (IAMStreamConfig)streamConfigObj;

            videoCapabilities = Pentacorn.Vision.Captures.DirectShow.VideoCapabilities.FromStreamConfig(streamConfig);

            var desiredFormat = videoCapabilities.Where(vc => vc.FrameSize.Width == this.Width && vc.FrameSize.Height == this.Height)
                                                 .OrderByDescending(vc => vc.MaxFrameRate).First();
            streamConfig.SetFormat(desiredFormat.MediaType);

            var hr = sampleGrabber.SetMediaType(desiredFormat.MediaType);
            if (hr < 0)
                throw new Win32Exception(hr);

            sampleGrabber.SetBufferSamples(true);
            sampleGrabber.SetOneShot(false);
            sampleGrabber.SetCallback(this, 1);

            captureGraphBuilder2.RenderStream(PinCategory.Capture, MediaType.Video, captureSourceBaseFilter, null, sampleGrabberBaseFilter);
            if (hr < 0)
                throw new Win32Exception(hr);

            AMMediaType mediaType = new AMMediaType();
            if (sampleGrabber.GetConnectedMediaType(mediaType) >= 0)
            {
                VideoInfoHeader vih = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));

                if (this.Width != vih.BmiHeader.Width)
                    throw new Exception("DirectShow capture width not what's requested.");
                if (this.Height != vih.BmiHeader.Height)
                    throw new Exception("DirectShow capture width not what's requested.");
                mediaType.Dispose();
            }

            mediaControl = (IMediaControl)filterGraph2;
            mediaControl.Run();
        }

        public void Close()
        {
            mediaControl.Stop();
            // Todo, Jaap Suter, November 2010, properly release COM objects.
        }

        public int SampleCB(double sampleTime, IMediaSample sample)
        {
            return 0;
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
        }

        public int BufferCB(double sampleTime, IntPtr buffer, int len)
        {
            using (var bgra = new Picture(Width, Height))
            {
                if (len <= pic.Bytes.Length)
                {
                    using (var bgr = new Picture(Width, Height))
                    {
                        Marshal.Copy(buffer, bgr.Bytes, 0, len);
                        bgra.AddRef();
                        bgra.Bgra.ConvertFrom(bgr.Bgr);
                        Enqueue(bgra.AddRef());
                    }
                }
                else
                {
                    bgra.Bgra.SetValue(Microsoft.Xna.Framework.Color.Fuchsia.ToBgra());
                    Enqueue(bgra.AddRef());
                }
            }

            return 0;
        }

        private void Write(TextWriter tw)
        {
            foreach (var prop in Enum.GetValues(typeof(CameraControlProperty)).Cast<CameraControlProperty>())
            {
                int min, max, delta, deflt, current;
                CameraControlFlags flags, curFlags;
                cameraControl.GetRange(prop, out min, out max, out delta, out deflt, out flags);
                cameraControl.Get(prop, out current, out curFlags);
                tw.WriteLine("{0}: {6} [{1}-{2}, step {3}, dflt {4}, flags {7} [{5}]]", prop.ToString().Truncate(15).PadLeft(15), min, max, delta, deflt, flags, current, curFlags);
            }

            foreach (var prop in Enum.GetValues(typeof(VideoProcAmpProperty)).Cast<VideoProcAmpProperty>())
            {
                int min, max, delta, deflt, current;
                VideoProcAmpFlags flags, curFlags;
                videoProcAmp.GetRange(prop, out min, out max, out delta, out deflt, out flags);
                videoProcAmp.Get(prop, out current, out curFlags);
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

        private VideoCapabilities[] videoCapabilities;
        private IAMVideoProcAmp videoProcAmp;
        private IAMCameraControl cameraControl;
        private IKsPropertySet ksPropertySet;
        private ICaptureGraphBuilder2 captureGraphBuilder2;
        private IFilterGraph2   filterGraph2;
        private IBaseFilter     captureSourceBaseFilter;
        private IBaseFilter     sampleGrabberBaseFilter;
        private ISampleGrabber  sampleGrabber;
        private IMediaControl   mediaControl;
    }
}
