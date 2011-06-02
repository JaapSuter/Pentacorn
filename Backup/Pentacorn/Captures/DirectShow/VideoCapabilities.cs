// AForge Direct Show Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2009
// andrew.kirillov@gmail.com

// ------------------------------------------------------------------
// DirectX.Capture
//
// History:
//	2003-Jan-24		BL		- created
//
// Copyright (c) 2003 Brian Low
// ------------------------------------------------------------------
// Adapted for AForge, Yves Vander Haeghen, 2009
//
// Changed a lot from the original by Andrew Kirillov to fit AForge.NET framework, 2009
//

namespace Pentacorn.Captures.DirectShow
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Pentacorn.Captures.DirectShow.Internals;

    /// <summary>
    /// Capabilities of video device such as frame size and frame rate.
    /// </summary>
    sealed class VideoCapabilities : IDisposable
    {
        /// <summary>
        /// Frame size supported by video device.
        /// </summary>
        public readonly Size FrameSize;

        /// <summary>
        /// Maximum frame rate supported by video device for corresponding <see cref="FrameSize">frame size</see>.
        /// </summary>
        public readonly int MaxFrameRate;

        public readonly AMMediaType MediaType;

        internal VideoCapabilities() { }

        // Retrieve capabilities of a video device
        static internal VideoCapabilities[] FromStreamConfig(IAMStreamConfig videoStreamConfig)
        {
            if (videoStreamConfig == null)
                throw new ArgumentNullException("videoStreamConfig");

            // ensure this device reports capabilities
            int count, size;
            int hr = videoStreamConfig.GetNumberOfCapabilities(out count, out size);

            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            if (count <= 0)
                throw new NotSupportedException("This video device does not report capabilities.");

            if (size > Marshal.SizeOf(typeof(VideoStreamConfigCaps)))
                throw new NotSupportedException("Unable to retrieve video device capabilities. This video device requires a larger VideoStreamConfigCaps structure.");

            var videocapsList = from i in Enumerable.Range(0, count)
                                let vc = new VideoCapabilities(videoStreamConfig, i)
                                // where vc.MediaType.SubType == MediaSubType.RGB24
                                select vc;
            
            return videocapsList.ToArray();
        }

        public override int GetHashCode()
        {
            return (this.FrameSize.Width << 16) ^ this.FrameSize.Height ^ this.MaxFrameRate ^ base.GetHashCode();
        }

        // Retrieve capabilities of a video device
        internal VideoCapabilities(IAMStreamConfig videoStreamConfig, int index)
        {
            AMMediaType mediaType = null;
            VideoStreamConfigCaps caps = new VideoStreamConfigCaps();

            // retrieve capabilities struct at the specified index
            int hr = videoStreamConfig.GetStreamCaps(index, out mediaType, caps);

            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            // extract info
            MediaType = mediaType;
            FrameSize = caps.InputSize;
            MaxFrameRate = (int)(10000000 / caps.MinFrameInterval);
        }

        public void Dispose()
        {
            if (MediaType != null)
                MediaType.Dispose();
        }
    }
}
