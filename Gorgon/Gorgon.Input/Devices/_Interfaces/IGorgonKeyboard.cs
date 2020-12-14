#region MIT
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, September 10, 2015 10:12:40 PM
// 
#endregion

using System;
using System.Windows.Forms;

namespace Gorgon.Input
{
    /// <summary>
    /// Key states.
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
    /// The type of keyboard.
    /// </summary>
    public enum KeyboardType
    {
        /// <summary>
        /// Unknown keyboard type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// An IBM compatible PC/XT 83 key keyboard.
        /// </summary>
        XT = 1,
        /// <summary>
        /// An Olivetti "ICO" 102 key keyboard.
        /// </summary>
        OlivettiICO = 2,
        /// <summary>
        /// An IBM compatible PC/AT 84 key keyboard.
        /// </summary>
        AT = 3,
        /// <summary>
        /// An IBM enhanced 101 or 102 key keyboard.
        /// </summary>
        Enhanced = 4,
        /// <summary>
        /// Nokia 1050 and similar keyboards.
        /// </summary>
        Nokia1050 = 5,
        /// <summary>
        /// Nokia 9140 and similar keyboards.
        /// </summary>
        Nokia9140 = 6,
        /// <summary>
        /// A japanese keyboard.
        /// </summary>
        Japanese = 7,
        /// <summary>
        /// A standard USB keyboard.
        /// </summary>
        USB = 81
    }

    /// <summary>
    /// Provides events and state for keyboard data from an input provider.
    /// </summary>

    public interface IGorgonKeyboard
        : IGorgonRawInputDevice, IGorgonRawInputDeviceData<GorgonRawKeyboardData>
    {
        #region Events.
        /// <summary>
        /// Event fired when a key is pressed on the keyboard.
        /// </summary>
        event EventHandler<GorgonKeyboardEventArgs> KeyDown;

        /// <summary>
        /// Event fired when a key is released on the keyboard.
        /// </summary>
        event EventHandler<GorgonKeyboardEventArgs> KeyUp;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return information about this keyboard.
        /// </summary>
        IGorgonKeyboardInfo Info
        {
            get;
        }

        /// <summary>
        /// Property to return the states for each of the <see cref="Keys"/>.
        /// </summary>
        GorgonKeyStateCollection KeyStates
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to convert a keyboard key into a character (if applicable).
        /// </summary>
        /// <param name="key">The key to convert into a character.</param>
        /// <param name="modifier">The modifier for that key.</param>
        /// <returns>The character representation for the key. If no representation is available, an empty string is returned.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve the character associated with a keyboard key. For example, if <see cref="Keys.A"/> is pressed, then 'a' will be returned. A <paramref name="modifier"/> can be 
        /// passed with the <see cref="Keys.ShiftKey"/> to return 'A'. 
        /// </para>
        /// <para>
        /// This method also supports the AltGr key which is represented by a combination of the <see cref="Keys.ControlKey"/> | <see cref="Keys.Menu"/> keys.
        /// </para>
        /// <para>
        /// This method only returns characters for the currently active keyboard layout (i.e. the system keyboard layout). If this keyboard interface represents another keyboard attached to the computer 
        /// then it will default to using the system keyboard to retrieve the character.
        /// </para>
        /// </remarks>
        string KeyToCharacter(Keys key, Keys modifier);
        #endregion
    }
}