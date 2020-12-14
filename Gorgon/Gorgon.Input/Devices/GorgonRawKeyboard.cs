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
// Created: Tuesday, September 07, 2015 2:27:10 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Input.Properties;
using Gorgon.Native;

namespace Gorgon.Input
{
    /// <summary>
    /// Provides events and state for keyboard data returned from Raw Input.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows a user to read keyboard data from Raw Input for all keyboard devices, or an individual device depending on what is passed to the constructor. 
    /// </para>
    /// </remarks>
    public class GorgonRawKeyboard
        : IGorgonKeyboard
    {
        #region Events.
        /// <summary>
        /// Event fired when a key is pressed on the keyboard.
        /// </summary>
        public event EventHandler<GorgonKeyboardEventArgs> KeyDown;

        /// <summary>
        /// Event fired when a key is released on the keyboard.
        /// </summary>
        public event EventHandler<GorgonKeyboardEventArgs> KeyUp;
        #endregion

        #region Variables.
        // The character buffer to hold the characters represented by a key press.
        private static readonly char[] _characterBuffer = new char[1];
        // The state values for the keys on the keyboard when translating a key press to a character.
        private static readonly byte[] _charStates = new byte[256];
        // The device handle.
        private readonly IntPtr _deviceHandle;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the handle for the device.
        /// </summary>
        IntPtr IGorgonRawInputDevice.Handle => _deviceHandle;

        /// <summary>
        /// Property to return the type of device.
        /// </summary>
        RawInputType IGorgonRawInputDevice.DeviceType => RawInputType.Keyboard;

        /// <summary>
        /// Property to return the HID usage code for this device.
        /// </summary>
        HIDUsage IGorgonRawInputDevice.DeviceUsage => HIDUsage.Keyboard;

        /// <summary>
        /// Property to return information about this keyboard.
        /// </summary>
        public IGorgonKeyboardInfo Info
        {
            get;
        }

        /// <summary>
        /// Property to return the states for each of the <see cref="Keys"/>.
        /// </summary>
        public GorgonKeyStateCollection KeyStates
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve information for the system keyboard if no specific keyboard is defined on creation.
        /// </summary>
        /// <returns>The device information for the system keyboard.</returns>
        private static RawKeyboardInfo GetSysKeyboardInfo()
        {
            var rawInfo = new RID_DEVICE_INFO_KEYBOARD
            {
                dwNumberOfFunctionKeys = UserApi.FunctionKeyCount,
                dwKeyboardMode = 0,
                dwSubType = 0
            };

            KeyboardType keyboardType = UserApi.KeyboardType;
            rawInfo.dwType = (int)keyboardType;

            switch (UserApi.KeyboardType)
            {
                case KeyboardType.XT:
                    rawInfo.dwNumberOfKeysTotal = 83;
                    rawInfo.dwNumberOfIndicators = 3;
                    break;
                case KeyboardType.OlivettiICO:
                    rawInfo.dwNumberOfKeysTotal = 102;
                    rawInfo.dwNumberOfIndicators = 3;
                    break;
                case KeyboardType.AT:
                    rawInfo.dwNumberOfKeysTotal = 84;
                    rawInfo.dwNumberOfIndicators = 3;
                    break;
                case KeyboardType.Enhanced:
                    rawInfo.dwNumberOfKeysTotal = 102;
                    rawInfo.dwNumberOfIndicators = 3;
                    break;
                case KeyboardType.Nokia1050:
                    rawInfo.dwNumberOfKeysTotal = -1;
                    rawInfo.dwNumberOfIndicators = -1;
                    break;
                case KeyboardType.Nokia9140:
                    rawInfo.dwNumberOfKeysTotal = -1;
                    rawInfo.dwNumberOfIndicators = -1;
                    break;
                case KeyboardType.Japanese:
                    rawInfo.dwNumberOfKeysTotal = -1;
                    rawInfo.dwNumberOfIndicators = -1;
                    break;
                case KeyboardType.USB:
                    rawInfo.dwNumberOfKeysTotal = -1;
                    rawInfo.dwNumberOfIndicators = -1;
                    break;
                default:
                    rawInfo.dwNumberOfKeysTotal = -1;
                    rawInfo.dwNumberOfIndicators = -1;
                    break;
            }

            return new RawKeyboardInfo(IntPtr.Zero, "System Keyboard", "Keyboard", Resources.GORINP_RAW_DESC_SYS_KEYBOARD, rawInfo);
        }

        /// <summary>
        /// Function to retrieve the modifier keys.
        /// </summary>
        /// <returns>The modifier keys.</returns>
        private Keys GetModifiers()
        {
            Keys result = Keys.None;

            if (KeyStates[Keys.ControlKey] == KeyState.Down)
            {
                if (KeyStates[Keys.LControlKey] == KeyState.Down)
                {
                    result |= Keys.Control | Keys.LControlKey;
                }

                if (KeyStates[Keys.RControlKey] == KeyState.Down)
                {
                    result |= Keys.Control | Keys.RControlKey;
                }
            }

            if (KeyStates[Keys.Menu] == KeyState.Down)
            {
                if (KeyStates[Keys.LMenu] == KeyState.Down)
                {
                    result |= Keys.Alt | Keys.LMenu;
                }

                if (KeyStates[Keys.RMenu] == KeyState.Down)
                {
                    result |= Keys.Alt | Keys.RMenu;
                }

            }

            if (KeyStates[Keys.ShiftKey] != KeyState.Down)
            {
                return result;
            }

            if (KeyStates[Keys.LShiftKey] == KeyState.Down)
            {
                result |= Keys.Shift | Keys.LShiftKey;
            }

            if (KeyStates[Keys.RShiftKey] == KeyState.Down)
            {
                result |= Keys.Shift | Keys.RShiftKey;
            }

            return result;
        }

        /// <summary>
        /// Function to fire the key down event.
        /// </summary>
        /// <param name="key">Key that's pressed.</param>
        /// <param name="scan">Scan code data.</param>
        private void OnKeyDown(Keys key, int scan) => KeyDown?.Invoke(this, new GorgonKeyboardEventArgs(key, GetModifiers(), scan));

        /// <summary>
        /// Function to fire the key up event.
        /// </summary>
        /// <param name="key">Key that's pressed.</param>
        /// <param name="scan">Scan code data.</param>
        private void OnKeyUp(Keys key, int scan) => KeyUp?.Invoke(this, new GorgonKeyboardEventArgs(key, GetModifiers(), scan));

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
        /// <para>
        /// This method is not thread safe. Invalid data will be returned if multiple thread access this method.
        /// </para>
        /// <para>
        /// This method was derived from the answer at <a href="http://stackoverflow.com/questions/6929275/how-to-convert-a-virtual-key-code-to-a-character-according-to-the-current-keyboa"/>.
        /// </para>
        /// </remarks>
        public string KeyToCharacter(Keys key, Keys modifier)
        {
            const int shiftKey = (int)Keys.ShiftKey;
            const int ctrlKey = (int)Keys.ControlKey;
            const int menuKey = (int)Keys.Menu;

            // Shift key modifier
            if ((modifier & Keys.Shift) == Keys.Shift)
            {
                _charStates[shiftKey] = 0xff;
            }
            else
            {
                _charStates[shiftKey] = 0;
            }

            // AltGr modifiers
            if ((modifier & (Keys.Control | Keys.Menu)) == (Keys.Control | Keys.Menu))
            {
                _charStates[ctrlKey] = 0xff;
                _charStates[menuKey] = 0xff;
            }
            else
            {
                _charStates[ctrlKey] = 0;
                _charStates[menuKey] = 0;
            }

            int result = UserApi.ToUnicode((uint)key, 0, _charStates, _characterBuffer, _characterBuffer.Length, 0);

            switch (result)
            {
                case -1:
                case 0:
                    return string.Empty;
                case 1:
                    return new string(_characterBuffer, 0, 1);
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Function to process the Gorgon raw input data into device state data and appropriate events.
        /// </summary>
        /// <param name="rawInputData">The data to process.</param>
        void IGorgonRawInputDeviceData<GorgonRawKeyboardData>.ProcessData(in GorgonRawKeyboardData rawInputData)
        {
            // Get the key code.
            Keys keyCode = rawInputData.Key;

            KeyState state = ((rawInputData.Flags & KeyboardDataFlags.KeyUp) == KeyboardDataFlags.KeyUp) ? KeyState.Up : KeyState.Down;

            // Determine right or left, and unifier key.
            switch (keyCode)
            {
                case Keys.ControlKey:   // CTRL.
                    keyCode = ((rawInputData.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? Keys.LControlKey : Keys.RControlKey;
                    KeyStates[Keys.ControlKey] = state;
                    break;
                case Keys.Menu:         // ALT.
                    keyCode = ((rawInputData.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? Keys.LMenu : Keys.RMenu;
                    KeyStates[Keys.Menu] = state;
                    break;
                case Keys.ShiftKey:     // Shift.
                    keyCode = ((rawInputData.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? Keys.LShiftKey : Keys.RShiftKey;
                    KeyStates[Keys.ShiftKey] = state;
                    break;
            }

            // Dispatch the key.
            KeyStates[keyCode] = state;

            if (state == KeyState.Down)
            {
                OnKeyDown(keyCode, rawInputData.ScanCode);
            }
            else
            {
                OnKeyUp(keyCode, rawInputData.ScanCode);
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRawKeyboard"/> class.
        /// </summary>
        /// <param name="keyboardInfo">[Optional] The keyboard information used to determine which keyboard to use.</param>
        /// <exception cref="InvalidCastException">Thrown if the <paramref name="keyboardInfo"/> is not the expected type.</exception>
        /// <remarks>
        /// When the <paramref name="keyboardInfo"/> is set to <b>null</b>, the system keyboard (that is, all keyboards attached to the computer) will be used. No differentiation between 
        /// keyboard devices is made. To specify an individual keyboard, pass an appropriate <see cref="IGorgonKeyboardInfo"/> obtained from the <see cref="GorgonRawInput.EnumerateKeyboards"/> method.
        /// </remarks>
        public GorgonRawKeyboard(IGorgonKeyboardInfo keyboardInfo = null)
        {
            if (keyboardInfo == null)
            {
                keyboardInfo = GetSysKeyboardInfo();
            }

            _deviceHandle = keyboardInfo.Handle;
            Info = keyboardInfo;
            KeyStates = new GorgonKeyStateCollection();
            KeyStates.Reset();
        }
        #endregion
    }
}
