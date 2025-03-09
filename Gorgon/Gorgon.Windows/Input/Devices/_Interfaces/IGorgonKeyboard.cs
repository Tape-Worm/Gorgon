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

namespace Gorgon.Windows.Input.Devices;

/// <summary>
/// Key states
/// </summary>
public enum KeyState
{
    /// <summary>
    /// Key is not pressed.
    /// </summary>
    Up = 0,
    /// <summary>
    /// Key is pressed.
    /// </summary>
    Down = 1
}

/// <summary>
/// A data structure that represents the state of the keyboard.
/// </summary>
public interface IGorgonKeyboard
{
    /// <summary>
    /// Property to return whether the key specified is currently pressed.
    /// </summary>
    bool this[VirtualKeys key]
    {
        get;
    }

    /// <summary>
    /// Property to return the information about this specific keyboard device.
    /// </summary>
    IGorgonKeyboardInfo Info
    {
        get;
    }

    /// <summary>
    /// Property to return the number of keys currently pressed.
    /// </summary>
    int PressedKeyCount
    {
        get;
    }

    /// <summary>
    /// Function to retrieve a list of modifier keys that are currently pressed.
    /// </summary>
    /// <returns>A read only span containing the modifier keys that are pressed.</returns>
    /// <remarks>
    /// <para>
    /// This method is not thread safe. 
    /// </para>
    /// </remarks>
    ReadOnlySpan<VirtualKeys> GetModifiers();

    /// <summary>
    /// Function to retrieve the keys that are currently pressed.
    /// </summary>
    /// <returns>A read only span containing the keys that are pressed.</returns>
    /// <remarks>
    /// <para>
    /// This method is not thread safe.
    /// </para>
    /// </remarks>
    ReadOnlySpan<VirtualKeys> GetPressedKeys();

    /// <summary>
    /// Function to convert a keyboard key into a character (if applicable).
    /// </summary>
    /// <param name="key">The key to convert into a character.</param>
    /// <param name="modifier">The modifier for that key.</param>
    /// <returns>The character representation for the key. If no representation is available, an empty string is returned.</returns>
    /// <remarks>
    /// <para>
    /// Use this to retrieve the character associated with a keyboard key. For example, if <see cref="VirtualKeys.A"/> is pressed, then 'a' will be returned. A <paramref name="modifier"/> can be 
    /// passed with the <see cref="VirtualKeys.Shift"/> to return 'A'. 
    /// </para>
    /// <para>
    /// This method also supports the AltGr key which is represented by a combination of the <see cref="VirtualKeys.Control"/> | <see cref="VirtualKeys.Alt"/> keys.
    /// </para>
    /// <para>
    /// This method only returns characters for the currently active keyboard layout (i.e. the system keyboard layout). If this keyboard interface represents another keyboard attached to the computer 
    /// then it will default to using the system keyboard to retrieve the character.
    /// </para>
    /// </remarks>
    string KeyToCharacter(VirtualKeys key, VirtualKeys modifier);

    /// <summary>
    /// Function to reset the data for the keyboard.
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
    /// <param name="inputEvent">The list of event containing the data to parse.</param>
    /// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Users can use this overload to parse a single event from the <see cref="GorgonInput.GetInput"/> method. Using the keyboard data this way allows 
    /// applications to process a series of keyboard input events in a custom, application-specific way. Since all <see cref="GorgonInputEvent"/> values include a <see cref="GorgonInputEvent.TimeStamp"/> value, 
    /// applications can sort the inputs in any way they see fit.
    /// </para>
    /// <para>
    /// If the <paramref name="inputEvent"/> is not a keyboard event, then this method will return <b>false</b>, and the data will be reset to default values.
    /// </para>
    /// <para>
    /// If this object is bound to a specific device, and the event is not for this device, then <b>false</b> will be returned, but the data will be not changed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInput.GetInput"/>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="ParseData(GorgonInputEventBuffer)"/>
    bool ParseData(GorgonInputEvent inputEvent);

    /// <summary>
    /// Function to parse the input events from the <see cref="GorgonInput.GetInput"/> method.
    /// </summary>
    /// <param name="inputEvents">The list of events containing the data to parse.</param>
    /// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="inputEvents"/> parameter is received by calling the <see cref="GorgonInput.GetInput"/> method. This will take the series of events 
    /// and build up to the most current event. All keyboard events will be processed by this method, so no data will be missed, and aggregrated to the current state. If the user requires custom processing 
    /// of the keyboard events, they can use the <see cref="ParseData(GorgonInputEvent?)"/> overload.
    /// </para>
    /// <para>
    /// If the <paramref name="inputEvents"/> is empty, then this method will return <b>false</b> and the data will be untouched.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInput.GetInput"/>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="ParseData(GorgonInputEvent?)"/>
    /// <seealso cref="GorgonInputEventBuffer"/>
    bool ParseData(GorgonInputEventBuffer inputEvents);
}