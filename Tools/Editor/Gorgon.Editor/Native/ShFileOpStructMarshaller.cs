// Gorgon
// Copyright (C) 2024 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 14, 2024 9:07:42 AM
//

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Gorgon.Editor.Native;

/// <summary>
/// Marshaller for converting the <see cref="SHFILEOPSTRUCT"/> type to an unmanaged type
/// </summary>
[CustomMarshaller(typeof(SHFILEOPSTRUCT), MarshalMode.ManagedToUnmanagedRef, typeof(ShFileOpStructMarshaller))]
internal unsafe static class ShFileOpStructMarshaller
{
    /// <summary>
    /// SHFILEOPSTRUCT for SHFileOperation from COM
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SHFILEOPSTRUCTUnmanaged
    {
        public nint hwnd;
        public uint wFunc;
        public byte* pFrom;
        public byte* pTo;
        public ushort fFlags;
        public byte fAnyOperationsAborted;
        public nint hNameMappings;
        public byte* lpszProgressTitle;
    }

    /// <summary>
    /// Function to convert a managed SHFILEOPSTRUCT to an unmanaged one.
    /// </summary>
    /// <param name="managed">The managed value to convert.</param>
    /// <returns>The unmanaged value.</returns>
    public static SHFILEOPSTRUCTUnmanaged ConvertToUnmanaged(SHFILEOPSTRUCT managed) => new()
    {
        hwnd = managed.hwnd,
        wFunc = (uint)managed.wFunc,
        pFrom = PczzWStrMarshaller.ConvertToUnmanaged(managed.pFrom),
        pTo = PczzWStrMarshaller.ConvertToUnmanaged(managed.pTo),
        fFlags = (ushort)managed.fFlags,
        fAnyOperationsAborted = Convert.ToByte(managed.fAnyOperationsAborted),
        hNameMappings = managed.hNameMappings,
        lpszProgressTitle = PczzWStrMarshaller.ConvertToUnmanaged(managed.lpszProgressTitle)
    };

    /// <summary>
    /// Function to convert an unmanaged SHFILEOPSTRUCTUnmanaged to a managed one.
    /// </summary>
    /// <param name="unmanaged">The unmanaged value to convert.</param>
    /// <returns>The managed value.</returns>
    public static SHFILEOPSTRUCT ConvertToManaged(SHFILEOPSTRUCTUnmanaged unmanaged) => new()
    {
        hwnd = unmanaged.hwnd,
        wFunc = (FileOperationType)unmanaged.wFunc,
        pFrom = PczzWStrMarshaller.ConvertToManaged(unmanaged.pFrom),
        pTo = PczzWStrMarshaller.ConvertToManaged(unmanaged.pTo),
        fFlags = (FileOperationFlags)unmanaged.fFlags,
        fAnyOperationsAborted = Convert.ToBoolean(unmanaged.fAnyOperationsAborted),
        hNameMappings = unmanaged.hNameMappings,
        lpszProgressTitle = PczzWStrMarshaller.ConvertToManaged(unmanaged.lpszProgressTitle)
    };

    /// <summary>
    /// Function to free the unmanaged memory used by the SHFILEOPSTRUCTUnmanaged.
    /// </summary>
    /// <param name="unmanaged">The unmanaged value.</param>
    public static void Free(SHFILEOPSTRUCTUnmanaged unmanaged)
    {
        PczzWStrMarshaller.Free(unmanaged.pFrom);
        PczzWStrMarshaller.Free(unmanaged.pTo);
        PczzWStrMarshaller.Free(unmanaged.lpszProgressTitle);
    }
}
