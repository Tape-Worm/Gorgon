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
// Created: January 21, 2025 6:49:03 PM
//

using System.Runtime.InteropServices;
using Gorgon.Graphics;

namespace Gorgon.Input.Devices;

/// <summary>
/// The current state of a mouse.
/// </summary>
/// <param name="position">The position of the mouse.</param>
/// <param name="buttons">The buttons on the mouse that are currently pressed.</param>
/// <param name="wheelDelta">The vertical scroll wheel data.</param>
/// <param name="hWheelDelta">The horizontal scroll wheel data.</param>
/// <param name="absolute">Flag to indicate whether the position data is absolute or not.</param>
[StructLayout(LayoutKind.Sequential)]
internal readonly struct RawMouseData(GorgonPoint position, uint buttons, short wheelDelta, short hWheelDelta, bool absolute)
{
    /// <summary>
    /// The position of the mouse.
    /// </summary>
    public readonly GorgonPoint Position = position;
    /// <summary>
    /// The buttons on the mouse that are currently pressed.
    /// </summary>
    public readonly uint Buttons = buttons;
    /// <summary>
    /// The vertical scroll wheel data.
    /// </summary>
    public readonly short WheelDelta = wheelDelta;
    /// <summary>
    /// The horizontal scroll wheel data.
    /// </summary>
    public readonly short HWheelDelta = hWheelDelta;
    /// <summary>
    /// Flag to return whether the position data is absolute.
    /// </summary>
    public readonly bool IsAbsoute = absolute;
}
