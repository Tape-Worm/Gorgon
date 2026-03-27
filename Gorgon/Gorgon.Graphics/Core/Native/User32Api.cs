// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: July 12, 2025 4:41:12 PM
//

using System.Runtime.InteropServices;

namespace Gorgon.Native;

/// <summary>
/// Flags for the <see cref="User32Api.MonitorFromWindow"/> method.
/// </summary>
internal enum MonitorFlags
{
    /// <summary>
    /// Returns a handle to the display monitor that is nearest to the window. 
    /// </summary>
    DefaultToNearest = 0,
    /// <summary>
    /// Returns NULL. 
    /// </summary>
    DefaultToNull = 1,
    /// <summary>
    /// Returns a handle to the primary display monitor. 
    /// </summary>
    DefaultToPrimary = 2
}

/// <summary>
/// User32 functions.
/// </summary>
internal static partial class User32Api
{
    /// <summary>
    /// Function to retrieve the handle for a monitor based on the physical portion occupied by a window.
    /// </summary>
    /// <param name="hwnd">The window handle.</param>
    /// <param name="flags">The flags used to determine how to default the value when the monitor could not be determined.</param>
    /// <returns>The handle for the monitor.</returns>
    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial nint MonitorFromWindow(nint hwnd, MonitorFlags flags);
}
