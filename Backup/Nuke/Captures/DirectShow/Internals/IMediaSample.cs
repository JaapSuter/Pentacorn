// AForge Direct Show Library
// AForge.NET framework
//
// Copyright © Andrew Kirillov, 2007
// andrew.kirillov@gmail.com
//

namespace Pentacorn.Vision.Captures.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMediaSample
    {
        [PreserveSig]
        int GetPointer([Out] out IntPtr ppBuffer); // BYTE **

        [PreserveSig]
        int GetSize();

        [PreserveSig]
        int GetTime(
            [Out] out long pTimeStart,
            [Out] out long pTimeEnd
            );

        [PreserveSig]
        int SetTime(
            [In, MarshalAs(UnmanagedType.LPStruct)] ulong pTimeStart,
            [In, MarshalAs(UnmanagedType.LPStruct)] ulong pTimeEnd
            );

        [PreserveSig]
        int IsSyncPoint();

        [PreserveSig]
        int SetSyncPoint([In, MarshalAs(UnmanagedType.Bool)] bool bIsSyncPoint);

        [PreserveSig]
        int IsPreroll();

        [PreserveSig]
        int SetPreroll([In, MarshalAs(UnmanagedType.Bool)] bool bIsPreroll);

        [PreserveSig]
        int GetActualDataLength();

        [PreserveSig]
        int SetActualDataLength([In] int len);

        /// <summary>
        /// Returned object must be released with DsUtils.FreeAMMediaType()
        /// </summary>
        [PreserveSig]
        int GetMediaType([Out] out AMMediaType ppMediaType);

        [PreserveSig]
        int SetMediaType([In] AMMediaType pMediaType);

        [PreserveSig]
        int IsDiscontinuity();

        [PreserveSig]
        int SetDiscontinuity([In, MarshalAs(UnmanagedType.Bool)] bool bDiscontinuity);

        [PreserveSig]
        int GetMediaTime(
            [Out] out long pTimeStart,
            [Out] out long pTimeEnd
            );

        [PreserveSig]
        int SetMediaTime(
            [In, MarshalAs(UnmanagedType.LPStruct)] ulong pTimeStart,
            [In, MarshalAs(UnmanagedType.LPStruct)] ulong pTimeEnd
            );
    }
}
