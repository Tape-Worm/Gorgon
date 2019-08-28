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
// Created: Saturday, July 18, 2015 4:38:03 PM
// 
#endregion

using System;
using System.Windows.Forms;

namespace Gorgon.Input
{
    /// <summary>
    /// Flags to help determine the type of data within the <see cref="GorgonRawKeyboardData"/> interface.
    /// </summary>
    [Flags]
    public enum KeyboardDataFlags
    {
        /// <summary>
        /// No key state.
        /// </summary>
        None = 0,
        /// <summary>
        /// The key is being held down.
        /// </summary>
        KeyDown = 1,
        /// <summary>
        /// The key has been released.
        /// </summary>
        KeyUp = 2,
        /// <summary>
        /// The left version of the key (for Alt, Control, Shift, etc...)
        /// </summary>
        LeftKey = 4,
        /// <summary>
        /// The right version of the key (for Alt, Control, Shift, etc...)
        /// </summary>
        RightKey = 8
    }

    /// <summary>
    /// A representation of the Raw Input data received from <c>WM_INPUT</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a Gorgon friendly representation of the data received from the <c>WM_INPUT</c> window message. The data from Raw Input is parsed and placed in an instance of this type and sent to the 
    /// appropriate <see cref="GorgonRawKeyboard"/> device object to be turned into state for that device. 
    /// </para>
    /// <para>
    /// This type is not intended for use by applications.
    /// </para>
    /// </remarks>
    public struct GorgonRawKeyboardData
    {
        /// <summary>
        /// Property to return the scan code for the key.
        /// </summary>
        public int ScanCode;

        /// <summary>
        /// Property to return flags for the scan code information.
        /// </summary>
        public KeyboardDataFlags Flags;

        /// <summary>
        /// Property to return the key being used.
        /// </summary>
        public Keys Key;
    }
}
