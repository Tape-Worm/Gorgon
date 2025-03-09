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
// Created: January 19, 2025 11:55:54 PM
//

using Gorgon.Graphics;

namespace Gorgon.Windows.Input.Devices;

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
    /// First pointing device button pressed.
    /// </summary>
    Button1 = 1,
    /// <summary>
    /// Second pointing device button pressed.
    /// </summary>
    Button2 = 2,
    /// <summary>
    /// Third pointing device button pressed.
    /// </summary>
    Button3 = 4,
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
/// A data structure that represents the state of the mouse.
/// </summary>
public interface IGorgonMouse
{
    /// <summary>
    /// Property to set or return whether the mouse cursor is visible.
    /// </summary>
    /// <remarks>
    /// This is a convenience property that will show or hide the mouse cursor. 
    /// at a constant value. 
    /// </remarks>
    public bool CursorVisible
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the state of the mouse buttons for the event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is returned as a bit mask of the <see cref="MouseButtons"/> enumeration. If the bit for the specific button is set, then the button is currently being held down.
    /// </para>
    /// <para>
    /// For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// if ((_mouse & MouseButtons.Button1) == MouseButtons.Button1)
    /// {
    ///   // Left button is held down.
    /// }
    /// else
    /// {
    ///   // Left button is not held down.
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    MouseButtons Buttons
    {
        get;
    }

    /// <summary>
    /// Property to return the delta of the last mouse movement.
    /// </summary>
    GorgonPoint DeltaPosition
    {
        get;
    }

    /// <summary>
    /// Property to return the information about this specific mouse device.
    /// </summary>
    IGorgonMouseInfo Info
    {
        get;
    }

    /// <summary>
    /// Property to return the relative position of the mouse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value represents the total relative movement of the mouse since the creation of the <see cref="GorgonInput"/> object. This allows an application to get a current snapshot of the mouse's relative 
    /// position without having to accumulate the previous delta values throughout a series of events.
    /// </para>
    /// <para>
    /// This position is not in any specific "space", and does not represent the screen coordinates of the mouse.
    /// </para>
    /// </remarks>
    GorgonPoint Position
    {
        get;
    }

    /// <summary>
    /// Property to return the cumulative relative vertical wheel delta.
    /// </summary>
    /// <remarks>
    /// This value is in multiples of 120. For example, if the wheel is moved one notch, then this value will be 120. If the wheel is moved two notches, then this value will be 240, and so on.
    /// </remarks>
    int RelativeHorizontalWheel
    {
        get;
    }

    /// <summary>
    /// Property to return the cumulative relative horizontal wheel delta.
    /// </summary>
    /// <remarks>
    /// This value is in multiples of 120. For example, if the wheel is moved one notch, then this value will be 120. If the wheel is moved two notches, then this value will be 240, and so on.
    /// </remarks>
    int RelativeVerticalWheel
    {
        get;
    }

    /// <summary>
    /// Property to return the vertical wheel delta value.
    /// </summary>
    int DeltaVerticalWheel
    {
        get;
    }

    /// <summary>
    /// Property to return the horizontal wheel delta value.
    /// </summary>
    int DeltaHorizontalWheel
    {
        get;
    }

    /// <summary>
    /// Function to reset the data for the mouse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should only be called when the application loses focus.
    /// </para>
    /// </remarks>
    void Reset();

    /// <summary>
    /// Function to parse the input events from the <see cref="GorgonInput.GetInput"/> method.
    /// </summary>
    /// <param name="inputEvents">The list of events containing the data to parse.</param>
    /// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="inputEvents"/> parameter is received by calling the <see cref="GorgonInput.GetInput"/> method. This will take the series of events 
    /// and build up to the most current event. All mouse events will be processed by this method, so no data will be missed. If the user requires custom processing of the mouse events, they can use the 
    /// <see cref="ParseData(GorgonInputEvent?)"/> overload.
    /// </para>
    /// <para>
    /// If the <paramref name="inputEvents"/> is empty, then this method will return <b>false</b> and the data will be untouched.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInput.GetInput"/>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="ParseData(GorgonInputEvent?)"/>
    bool ParseData(GorgonInputEventBuffer inputEvents);

    /// <summary>
    /// Function to parse a single input event from the <see cref="GorgonInput.GetInput"/> method.
    /// </summary>
    /// <param name="inputEvent">The event containing the data to parse.</param>
    /// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Users can use this overload to parse a single event from the <see cref="GorgonInput.GetInput"/> method. Using the mouse data this way allows 
    /// applications to process a series of mouse input events in a custom, application-specific way. Since all <see cref="GorgonInputEvent"/> values include a <see cref="GorgonInputEvent.TimeStamp"/> value, 
    /// applications can sort the inputs in any way they see fit.
    /// </para>
    /// <para>
    /// If the <paramref name="inputEvent"/> is not a mouse event, then this method will return <b>false</b>, and the data will be reset to default values.
    /// </para>
    /// <para>
    /// If this object is bound to a specific device, and the event is not for this device, then <b>false</b> will be returned, but the data will be not changed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInput.GetInput"/>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="ParseData(GorgonInputEventBuffer)"/>
    bool ParseData(GorgonInputEvent inputEvent);
}