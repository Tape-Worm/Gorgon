
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
// Created: Thursday, September 10, 2015 10:00:48 PM
// 


using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Input;

/// <summary>
/// Enumeration for mouse buttons
/// </summary>
[Flags]
public enum MouseButtons
{
    /// <summary>
    /// No pointing device button pressed.
    /// </summary>
    None = 0,
    /// <summary>
    /// Left pointing device button pressed.
    /// </summary>
    Left = 1,
    /// <summary>
    /// Right pointing device button pressed.
    /// </summary>
    Right = 2,
    /// <summary>
    /// Middle pointing device button pressed.
    /// </summary>
    Middle = 4,
    /// <summary>
    /// Left pointing device button pressed (same as <see cref="Left"/>).
    /// </summary>
    Button1 = Left,
    /// <summary>
    /// Right pointing device button pressed (same as <see cref="Right"/>).
    /// </summary>
    Button2 = Right,
    /// <summary>
    /// Middle pointing device button pressed (same as <see cref="Middle"/>).
    /// </summary>
    Button3 = Middle,
    /// <summary>
    /// Fourth pointing device button pressed.
    /// </summary>
    Button4 = 8,
    /// <summary>
    /// Fifth pointing device button pressed.
    /// </summary>
    Button5 = 16
}

/// <summary>
/// Provides events and state for mouse data from a physical mouse
/// </summary>
public interface IGorgonMouse
    : IGorgonRawInputDevice, IGorgonRawInputDeviceData<GorgonRawMouseData>
{

    /// <summary>
    /// Event triggered when the mouse is moved.
    /// </summary>
    event EventHandler<GorgonMouseEventArgs> MouseMove;

    /// <summary>
    /// Event triggered when a mouse button is held down.
    /// </summary>
    event EventHandler<GorgonMouseEventArgs> MouseButtonDown;

    /// <summary>
    /// Event triggered when a mouse button is released.
    /// </summary>
    event EventHandler<GorgonMouseEventArgs> MouseButtonUp;

    /// <summary>
    /// Event triggered when a mouse wheel (if present) is moved.
    /// </summary>
    event EventHandler<GorgonMouseEventArgs> MouseWheelMove;

    /// <summary>
    /// Event triggered when a double click is performed on a mouse button.
    /// </summary>
    event EventHandler<GorgonMouseEventArgs> MouseDoubleClicked;



    /// <summary>
    /// Property to set or return the last reported relative position movement offset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value represents the number of units that the mouse moved since its last <see cref="MouseMove"/> event.
    /// </para>
    /// <para>
    /// Users should reset this value when they are done with it. Otherwise, it will not be reset until the next <see cref="MouseWheelMove"/> event.
    /// </para>
    /// </remarks>
    GorgonPoint RelativePositionOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last reported relative wheel movement delta.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value represents the number of units that the mouse wheel moved since its last <see cref="MouseWheelMove"/> event.
    /// </para>
    /// <para>
    /// Users should reset this value when they are done with it. Otherwise, it will not be reset until the next <see cref="MouseWheelMove"/> event.
    /// </para>
    /// </remarks>
    int RelativeWheelDelta
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return information about this mouse.
    /// </summary>
    IGorgonMouseInfo Info
    {
        get;
    }

    /// <summary>
    /// Property to set or return the delay between button clicks, in milliseconds, for a double click event.
    /// </summary>
    int DoubleClickDelay
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the <c>Rectangle</c> used to constrain the mouse <see cref="Position"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will constrain the value of the <see cref="Position"/> within the specified <c>Rectangle</c>. This means that a cursor positioned at 320x200 with a region located at 330x210 with a width 
    /// and height of 160x160 will make the <see cref="Position"/> property return 330x210. If the cursor was positioned at 500x400, the <see cref="Position"/> property would return 480x360.
    /// </para>
    /// <para>
    /// Passing <c>Rectangle.Empty</c> to this property will remove the constraint on the position.
    /// </para>
    /// </remarks>
    GorgonRectangle PositionConstraint
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the <see cref="GorgonRange{T}"/> used to constrain the mouse <see cref="WheelPosition"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a mouse wheel exists on the device, this will constrain the value of the <see cref="WheelPosition"/> within the specified <see cref="GorgonRange{T}"/>. This means that a wheel with a position of  
    /// 160, with a constraint of 180-190 will make the <see cref="WheelPosition"/> property return 180.
    /// </para>
    /// <para>
    /// Passing <see cref="GorgonRange{T}.Empty"/> to this property will remove the constraint on the position.
    /// </para>
    /// </remarks>
    GorgonRange<int> WheelConstraint
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the <c>Size</c> of the area, in pixels, surrounding the cursor that represents a valid double click area.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is set, and a mouse button is double clicked, this value is checked to see if the mouse <see cref="Position"/> falls within -<c>value.Size.Width</c> to <c>value.Size.Width</c>, 
    /// and -<c>value.Size.Height"</c> to <c>value.Size.Height</c> on the second click. If the <see cref="Position"/> is within this area, then the double click event will be triggered. Otherwise, it will 
    /// not.
    /// </para>
    /// <para>
    /// Passing <c>Size.Empty</c> to this property will disable double clicking.
    /// </para>
    /// </remarks>
    GorgonPoint DoubleClickSize
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the position of the mouse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is affected by the <see cref="PositionConstraint"/> value.
    /// </para>
    /// </remarks>
    GorgonPoint Position
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the pointing device wheel position.
    /// </summary>
    /// <remarks>
    /// This property is affected by the <see cref="WheelConstraint"/> value.
    /// </remarks>
    int WheelPosition
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the pointing device button(s) that are currently down.
    /// </summary>
    MouseButtons Buttons
    {
        get;
        set;
    }

}
