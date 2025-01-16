
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
// Created: Saturday, August 1, 2015 1:03:26 PM
// 

using Gorgon.Graphics;

namespace Gorgon.Input;

/// <summary>
/// The current state for the mouse buttons
/// </summary>
/// <remarks>
/// This is used by the <see cref="GorgonRawMouseData"/> value type to determine if a specific button is being held down, or released. It is not intended for use in general applications
/// </remarks>
[Flags]
public enum MouseButtonState
    : short
{
    /// <summary>
    /// No button has a state.
    /// </summary>
    None = 0,
    /// <summary>
    /// The left button was held down (button 1).
    /// </summary>
    ButtonLeftDown = 1,
    /// <summary>
    /// The right button was held down (button 2).
    /// </summary>
    ButtonRightDown = 2,
    /// <summary>
    /// The middle button was held down (button 3).
    /// </summary>
    ButtonMiddleDown = 4,
    /// <summary>
    /// The fifth button was held down.
    /// </summary>
    Button4Down = 8,
    /// <summary>
    /// The fourth button was held down.
    /// </summary>
    Button5Down = 16,
    /// <summary>
    /// The left button was released (button 1).
    /// </summary>
    ButtonLeftUp = 32,
    /// <summary>
    /// The right button was released (button 2).
    /// </summary>
    ButtonRightUp = 64,
    /// <summary>
    /// The middle button was released (button 3).
    /// </summary>
    ButtonMiddleUp = 128,
    /// <summary>
    /// The fourth button was released.
    /// </summary>
    Button4Up = 256,
    /// <summary>
    /// The fifth button was released.
    /// </summary>
    Button5Up = 512
}

/// <summary>
/// A representation of the Raw Input data received from <c>WM_INPUT</c>
/// </summary>
/// <remarks>
/// <para>
/// This is a Gorgon friendly representation of the data received from the <c>WM_INPUT</c> window message. The data from Raw Input is parsed and placed in an instance of this type and sent to the 
/// appropriate <see cref="GorgonRawMouse"/> object to be turned into state for that device. 
/// </para>
/// <para>
/// This type is not intended for use by applications
/// </para>
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="GorgonRawMouseData" /> struct.</remarks>
/// <param name="position">The position of the mouse.</param>
/// <param name="mouseWheel">The mouse wheel delta value.</param>
/// <param name="mouseButton">The mouse buttons that are held down.</param>
/// <param name="relative"><b>true</b> if the positioning is relative, <b>false</b> if absolute.</param>
public readonly struct GorgonRawMouseData(GorgonPoint position, short mouseWheel, MouseButtonState mouseButton, bool relative)
{
    /// <summary>
    /// The current position of the mouse.
    /// </summary>
    /// <remarks>
    /// If the <see cref="IsRelative"/> value is set to <b>true</b>, then this value will be a relative value based on the last known position of the mouse. Otherwise, this will return the absolute 
    /// position of the mouse.
    /// </remarks>
    public readonly GorgonPoint Position = position;

    /// <summary>
    /// The change in the mouse wheel since the last event.
    /// </summary>
    public readonly short MouseWheelDelta = mouseWheel;

    /// <summary>
    /// The state of the mouse button
    /// </summary>
    public readonly MouseButtonState ButtonState = mouseButton;

    /// <summary>
    /// Flag to indicate whether the <see cref="Position"/> value is relative or not.
    /// </summary>
    public readonly bool IsRelative = relative;
}
