// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 14, 2024 1:17:35 AM
//

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Gorgon.Native;

/// <summary>
/// Marshaller for the <see cref="HDITEM"/> structure.
/// </summary>
[CustomMarshaller(typeof(HDITEM), MarshalMode.ManagedToUnmanagedRef, typeof(HdItemMarshaller))]
internal unsafe static class HdItemMarshaller
{
    /// <summary>
    /// Header item structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct HDITEMUnmanaged
    {
        public uint mask;
        public int cxy;
        public byte* pszText;
        public nint hbm;
        public int cchTextMax;
        public int fmt;
        public nint lParam;
        public int iImage;
        public int iOrder;
        public uint type;
        public nint pvFilter;
        public uint state;
    }

    /// <summary>
    /// Function to convert an HDITEM structure to an unmanaged value.
    /// </summary>
    /// <param name="managed">The managed item to convert.</param>
    /// <returns>The pointer to the unmanaged item.</returns>
    public static HDITEMUnmanaged ConvertToUnmanaged(HDITEM managed) => new()
    {
        mask = managed.mask,
        cxy = managed.cxy,
        pszText = Utf8StringMarshaller.ConvertToUnmanaged(managed.pszText),
        hbm = managed.hbm,
        cchTextMax = managed.cchTextMax,
        fmt = managed.fmt,
        lParam = managed.lParam,
        iImage = managed.iImage,
        iOrder = managed.iOrder,
        type = managed.type,
        pvFilter = managed.pvFilter,
        state = managed.state
    };

    /// <summary>
    /// Function to convert an unmanaged value to a HDITEM structure.
    /// </summary>
    /// <param name="unmanaged">The unmanaged pointer.</param>
    /// <returns>The HDITEM value.</returns>
    public static HDITEM ConvertToManaged(HDITEMUnmanaged unmanaged) => new()
    {
        mask = unmanaged.mask,
        cxy = unmanaged.cxy,
        pszText = Utf8StringMarshaller.ConvertToManaged(unmanaged.pszText),
        hbm = unmanaged.hbm,
        cchTextMax = unmanaged.cchTextMax,
        fmt = unmanaged.fmt,
        lParam = unmanaged.lParam,
        iImage = unmanaged.iImage,
        iOrder = unmanaged.iOrder,
        type = unmanaged.type,
        pvFilter = unmanaged.pvFilter,
        state = unmanaged.state
    };

    /// <summary>
    /// Function to free any allocated data when marshalling.
    /// </summary>
    /// <param name="unmanaged">The data structure containing the unmanaged data to free.</param>
    public static void Free(HDITEMUnmanaged unmanaged) => Utf8StringMarshaller.Free(unmanaged.pszText);
}
