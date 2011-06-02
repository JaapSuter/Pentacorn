// AForge Direct Show Library
// AForge.NET framework
//
// Copyright © Andrew Kirillov, 2008
// andrew.kirillov@gmail.com
//

namespace Pentacorn.Captures.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// From KSTOPOLOGY_CONNECTION
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct KSTopologyConnection
    {
        public int FromNode;
        public int FromNodePin;
        public int ToNode;
        public int ToNodePin;
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("720D4AC0-7533-11D0-A5D6-28DB04C10000"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IKsTopologyInfo
    {
        [PreserveSig]
        int get_NumCategories(
            [Out] out int pdwNumCategories
            );

        [PreserveSig]
        int get_Category(
            [In] int dwIndex,
            [Out] out Guid pCategory
            );

        [PreserveSig]
        int get_NumConnections(
            [Out] out int pdwNumConnections
            );

        [PreserveSig]
        int get_ConnectionInfo(
            [In] int dwIndex,
            [Out] out KSTopologyConnection pConnectionInfo
            );

        [PreserveSig]
        int get_NodeName(
            [In] int dwNodeId,
            [In] IntPtr pwchNodeName,
            [In] int dwBufSize,
            [Out] out int pdwNameLen
            );

        [PreserveSig]
        int get_NumNodes(
            [Out] out int pdwNumNodes
            );

        [PreserveSig]
        int get_NodeType(
            [In] int dwNodeId,
            [Out] out Guid pNodeType
            );

        [PreserveSig]
        int CreateNodeInstance(
            [In] int dwNodeId,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out Object ppvObject
            );
    }
}
