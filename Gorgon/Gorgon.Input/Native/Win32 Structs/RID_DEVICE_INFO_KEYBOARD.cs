
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, August 12, 2015 11:02:23 PM
// 

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace Gorgon.Native;

/// <summary>
/// Keyboard device information
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RID_DEVICE_INFO_KEYBOARD
{
    /// <summary>
    /// Type.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwType;
    /// <summary>
    /// Subtype.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwSubType;
    /// <summary>
    /// Keyboard mode.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwKeyboardMode;
    /// <summary>
    /// Number of function keys.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwNumberOfFunctionKeys;
    /// <summary>
    /// Number of indicators.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwNumberOfIndicators;
    /// <summary>
    /// Number of total keys.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwNumberOfKeysTotal;
}
