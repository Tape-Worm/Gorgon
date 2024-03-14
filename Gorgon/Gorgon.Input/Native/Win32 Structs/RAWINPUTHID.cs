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
// Created: Wednesday, August 12, 2015 11:52:25 PM
// 
#endregion

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace Gorgon.Native;

/// <summary>
/// Value type for raw input from a HID.
/// </summary>	
[StructLayout(LayoutKind.Explicit)]
internal struct RAWINPUTHID
{
    /// <summary>Size of the HID data in bytes.</summary>
    [FieldOffset(0)]
    public int Size;
    /// <summary>Number of HID in Data.</summary>
    [FieldOffset(4)]
    public int Count;
    /// <summary>HID data.</summary>
    [FieldOffset(8)]
    public nint Data;
}
