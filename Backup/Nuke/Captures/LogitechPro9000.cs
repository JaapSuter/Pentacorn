using System;
using System.Runtime.InteropServices;
using Pentacorn.Vision.Captures.DirectShow.Internals;

namespace Pentacorn.Vision.Captures
{
    static class LogitechPro9000
    {
        public static void SetExposure(this IKsPropertySet ksPropertySet, TimeSpan exposureTime)
        {
            var ptr = IntPtr.Zero;
            var exposureSupport = new KSPropertySupport();

            const int ksPropId = (int)KSPROPERTY_LP1_PROPERTY.EXPOSURE_TIME;
            var hr = ksPropertySet.QuerySupported(PROPSETID_LOGITECH_PUBLIC1, ksPropId, out exposureSupport);
            if (hr < 0 || !exposureSupport.HasFlag(KSPropertySupport.Set))
                return;

            var expData= new KSPROPERTY_LP1_EXPOSURE_TIME_S()
            {
                Header = new KSPROPERTY_LP1_HEADER() { Flags = KSPROPERTY_CAMERACONTROL_FLAGS.MANUAL, },
                ulExposureTime = (uint)(exposureTime.TotalMilliseconds * 10), // Step size is 100us.
            };

            try
            {
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(expData));
                Marshal.StructureToPtr(expData, ptr, true);
                hr = ksPropertySet.Set(PROPSETID_LOGITECH_PUBLIC1, ksPropId, IntPtr.Zero, 0, ptr, Marshal.SizeOf(expData));
                if (hr < 0)
                    return; // Todo, Jaap Suter, November 2010, don't care.
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }

        public static void EnableRawBayer(this IKsPropertySet ksPropertySet)
        {
            var ptr = IntPtr.Zero;
            var support = new KSPropertySupport();

            var hr = ksPropertySet.QuerySupported(PROPSETID_LOGITECH_VIDEO_XU, LXU_COLOR_PROCESSING_DISABLE_CONTROL, out support);
            if (hr < 0 || !support.HasFlag(KSPropertySupport.Set))
                return;

            hr = ksPropertySet.QuerySupported(PROPSETID_LOGITECH_VIDEO_XU, LXU_RAW_DATA_BIT_PER_PIXEL_CONTROL, out support);
            if (hr < 0 || !support.HasFlag(KSPropertySupport.Set))
                return;

            try
            {
                var kn = new KSP_NODE()
                {
                    Flags = (uint)(KSPropertyType.Set | KSPropertyType.Topology),
                    Id = 0,
                    Set = PROPSETID_LOGITECH_VIDEO_XU,
                    NodeId = 0,
                };

                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(kn));
                Marshal.StructureToPtr(kn, ptr, true);
                hr = ksPropertySet.Set(PROPSETID_LOGITECH_VIDEO_XU, LXU_COLOR_PROCESSING_DISABLE_CONTROL, IntPtr.Zero, 0, ptr, Marshal.SizeOf(kn));
                if (hr < 0)
                    return; // Todo, Jaap Suter, November 2010, don't care.
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }

        // 0x5070, 0x49AB, 0xB8, 0xCC, 0xB3, 0x85, 0x5E, 0x8D, 0x22, 0x50);
        private static readonly Guid PROPSETID_LOGITECH_VIDEO_XU = new Guid(0x63610682, 0x5070, 0x49AB, 0xB8, 0xCC, 0xB3, 0x85, 0x5E, 0x8D, 0x22, 0x1E);
        private static readonly Guid PROPSETID_LOGITECH_PUBLIC1 = new Guid(0xCAAE4966, 0x272C, 0x44A9, 0xB7, 0x92, 0x71, 0x95, 0x3F, 0x89, 0xDB, 0x2B);

        private static readonly int LXU_COLOR_PROCESSING_DISABLE_CONTROL	= 0x05;
        private static readonly int LXU_RAW_DATA_BIT_PER_PIXEL_CONTROL	= 0x08;

        [Flags]
        enum KSPropertyType : uint
        {
            Get = 1,
            Set = 2,
            Topology = 0x10000000,
        }

        private enum KSPROPERTY_LP1_PROPERTY : int
        {
            VERSION = 0,
            DIGITAL_PAN,
            DIGITAL_TILT,
            DIGITAL_ZOOM,
            DIGITAL_PANTILTZOOM,
            EXPOSURE_TIME,
            FACE_TRACKING,
            LED,
            FINDFACE,

            LAST = FINDFACE
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KSPROPERTY_LP1_HEADER
        {
            public KSPROPERTY_CAMERACONTROL_FLAGS Flags;
            public int Reserved1;
            public int Reserved2;
        }

        [Flags]
        private enum KSPROPERTY_CAMERACONTROL_FLAGS : int
        {
            AUTO = 0X0001,          //Equivalent to CameraControl_Flags_Auto.
            MANUAL = 0X0002,        // Equivalent to CameraControl_Flags_Manual.
            ABSOLUTE = 0X0000,      // The camera supports absolute units for this setting.
            RELATIVE = 0X0010,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KSPROPERTY_LP1_EXPOSURE_TIME_S
        {
            public KSPROPERTY_LP1_HEADER Header;
            public uint ulExposureTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KSP_NODE
        {
            public Guid Set;
            public uint Id;
            public uint Flags;
            public uint NodeId;
            private uint Reserved;
        }
    }
}
