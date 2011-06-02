// AForge Direct Show Library
// AForge.NET framework
//
// Copyright © Andrew Kirillov, 2008
// andrew.kirillov@gmail.com
//

namespace Pentacorn.Vision.Captures.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [Flags]
    enum KSPropertySupport
    {
        Get = 1,
        Set = 2
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("31EFAC30-515C-11d0-A9AA-00AA0061BE93"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IKsPropertySet
    {

        [PreserveSig]
        int Set(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
            [In] int dwPropID,
            [In] IntPtr pInstanceData,
            [In] int cbInstanceData,
            [In] IntPtr pPropData,
            [In] int cbPropData
            );

        [PreserveSig]
        int Get(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
            [In] int dwPropID,
            [In] IntPtr pInstanceData,
            [In] int cbInstanceData,
            [In, Out] IntPtr pPropData,
            [In] int cbPropData,
            [Out] out int pcbReturned
            );

        [PreserveSig]
        int QuerySupported(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
            [In] int dwPropID,
            [Out] out KSPropertySupport pTypeSupport
            );
    }
}
