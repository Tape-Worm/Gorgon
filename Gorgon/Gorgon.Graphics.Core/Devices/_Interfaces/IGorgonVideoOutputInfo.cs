#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 29, 2016 10:41:14 PM
// 
#endregion

using Gorgon.Core;
using SharpDX.DXGI;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An enumeration that indicates how the back buffers should be rotated to fit the physical rotation of a monitor.
/// </summary>
public enum RotationMode
{
    /// <summary>
    /// No rotation mode specified.
    /// </summary>
    Unspecified = DisplayModeRotation.Unspecified,
    /// <summary>
    /// No rotation.
    /// </summary>
    Identity = DisplayModeRotation.Identity,
    /// <summary>
    /// Rotated 90 degrees.
    /// </summary>
    Rotate90 = DisplayModeRotation.Rotate90,
    /// <summary>
    /// Rotated 180 degrees.
    /// </summary>
    Rotate180 = DisplayModeRotation.Rotate180,
    /// <summary>
    /// Rotated 270 degrees.
    /// </summary>
    Rotate270 = DisplayModeRotation.Rotate270
}

/// <summary>
/// Provides information about an output on a <see cref="IGorgonVideoAdapterInfo"/>.
/// </summary>
/// <remarks>
/// An output is typically a physical connection between the video adapter and another device.
/// </remarks>
public interface IGorgonVideoOutputInfo
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the index of the output.
    /// </summary>
    int Index
    {
        get;
    }

    /// <summary>
    /// Property to return the adapter that owns this output.
    /// </summary>
    IGorgonVideoAdapterInfo Adapter
    {
        get;
    }

    /// <summary>
    /// Property to return the handle to the monitor that is attached to the output.
    /// </summary>
    nint MonitorHandle
    {
        get;
    }

    /// <summary>
    /// Property to return the bounds of the output in desktop coordinates.
    /// </summary>
    /// <remarks>
    /// The desktop coordinates depend on the dots per inch (DPI) of the desktop. For more information about writing DPI-aware Win32 applications, see <a target="_blank" href="https://msdn.microsoft.com/en-us/library/bb173068.aspx">High DPI</a>.
    /// </remarks>
    DX.Rectangle DesktopBounds
    {
        get;
    }

    /// <summary>
    /// Property to return whether the output is attached to the desktop or not.
    /// </summary>
    bool IsAttachedToDesktop
    {
        get;
    }

    /// <summary>
    /// Property to return how the display image is rotated by the output.
    /// </summary>
    RotationMode Rotation
    {
        get;
    }

    /// <summary>
    /// Property to return the list of video modes supported by this output.
    /// </summary>
    IReadOnlyList<GorgonVideoMode> VideoModes
    {
        get;
    }
}
