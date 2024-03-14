#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, August 12, 2015 11:52:01 PM
// 
#endregion

using System.Runtime.InteropServices;
using Gorgon.Input;

// ReSharper disable InconsistentNaming
namespace Gorgon.Native;

/// <summary>
/// Value type for a raw input header.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUTHEADER
{
    /// <summary>Type of device the input is coming from.</summary>
    public RawInputType Type;
    /// <summary>Size of the packet of data.</summary>
    public int Size;
    /// <summary>Handle to the device sending the data.</summary>
    public nint Device;
    /// <summary>wParam from the window message.</summary>
    public nint wParam;
}
