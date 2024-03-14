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
// Created: Wednesday, August 12, 2015 11:04:36 PM
// 
#endregion

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace Gorgon.Native;

/// <summary>
/// Enumeration containing the flags for raw mouse data.
/// </summary>
[Flags]
internal enum RawMouseFlags
    : uint
{
    /// <summary>Relative to the last position.</summary>
    MoveRelative = 0,
    /// <summary>Absolute positioning.</summary>
    MoveAbsolute = 1,
    /// <summary>Coordinate data is mapped to a virtual desktop.</summary>
    VirtualDesktop = 2,
    /// <summary>Attributes for the mouse have changed.</summary>
    AttributesChanged = 4
}

/// <summary>
/// Enumeration containing the button data for raw mouse input.
/// </summary>
[Flags]
internal enum RawMouseButtons
    : ushort
{
    /// <summary>No button.</summary>
    None = 0,
    /// <summary>Left (button 1) down.</summary>
    LeftDown = 0x0001,
    /// <summary>Left (button 1) up.</summary>
    LeftUp = 0x0002,
    /// <summary>Right (button 2) down.</summary>
    RightDown = 0x0004,
    /// <summary>Right (button 2) up.</summary>
    RightUp = 0x0008,
    /// <summary>Middle (button 3) down.</summary>
    MiddleDown = 0x0010,
    /// <summary>Middle (button 3) up.</summary>
    MiddleUp = 0x0020,
    /// <summary>Button 4 down.</summary>
    Button4Down = 0x0040,
    /// <summary>Button 4 up.</summary>
    Button4Up = 0x0080,
    /// <summary>Button 5 down.</summary>
    Button5Down = 0x0100,
    /// <summary>Button 5 up.</summary>
    Button5Up = 0x0200,
    /// <summary>Mouse wheel moved.</summary>
    MouseWheel = 0x0400
}

/// <summary>
/// Mouse device info.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RID_DEVICE_INFO_MOUSE
{
    /// <summary>
    /// Mouse ID.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwId;
    /// <summary>
    /// Number of buttons.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwNumberOfButtons;
    /// <summary>
    /// Sample rate.
    /// </summary>
    [MarshalAs(UnmanagedType.U4)]
    public int dwSampleRate;
    /// <summary>
    /// Flag to indicate a horizontal wheel.
    /// </summary>
    [MarshalAs(UnmanagedType.Bool)]
    public bool fHasHorizontalWheel;
}
