#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Monday, October 09, 2006 12:41:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Forms = System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.InputDevices.Internal;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	public class GorgonKeyboard
		: Keyboard
	{
		#region Variables.
		private RAWINPUTDEVICE _device;			// Input device.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse the raw keyboard data.
		/// </summary>
		/// <param name="keyboardData">Data to examine.</param>
		internal void GetRawData(RAWINPUTKEYBOARD keyboardData)
		{
			int keyCode = 0;		// Virtual key code.
			int secCode = -1;		// Secondary code.
			bool left = false;		// Left modifier.
			bool right = false;		// Right modifier.
			KeyboardKeys modifiers;	// Keyboard modifiers.

			// Get the key code.
			modifiers = KeyboardKeys.None;
			keyCode = (int)keyboardData.VirtualKey;			

			// Determine right or left, and unifier key.
			switch ((KeyboardKeys)keyboardData.VirtualKey)
			{
				case KeyboardKeys.ControlKey:	// CTRL.
					secCode = (int)KeyboardKeys.ControlKey;
					if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) != 0)
					{
						right = true;
						keyCode = (int)KeyboardKeys.RControlKey;
					}
					else
					{
						left = true;
						keyCode = (int)KeyboardKeys.LControlKey;
					}
					break;
				case KeyboardKeys.Menu:			// ALT.
					secCode = (int)KeyboardKeys.Menu;
					if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) != 0)
					{
						right = true;
						keyCode = (int)KeyboardKeys.RMenu;
					}
					else
					{
						left = true;
						keyCode = (int)KeyboardKeys.LMenu;						
					}
					break;
				case KeyboardKeys.ShiftKey:		// Shift.
					secCode = (int)KeyboardKeys.ShiftKey;
					if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) != 0)
					{
						right = true;
						keyCode = (int)KeyboardKeys.RShiftKey;
					}
					else
					{
						left = true;
						keyCode = (int)KeyboardKeys.LShiftKey;
					}
					break;
			}

			// Check for modifiers.
			modifiers = KeyboardKeys.None;
			if (KeyStates[KeyboardKeys.ControlKey] == KeyState.Down)
			{
				if (KeyStates[KeyboardKeys.LControlKey] == KeyState.Down)
					left = true;
				if (KeyStates[KeyboardKeys.RControlKey] == KeyState.Down)
					right = true;
				modifiers |= KeyboardKeys.Control;
			}
			if (KeyStates[KeyboardKeys.Menu] == KeyState.Down)
			{
				if (KeyStates[KeyboardKeys.LMenu] == KeyState.Down)
					left = true;
				if (KeyStates[KeyboardKeys.RMenu] == KeyState.Down)
					right = true;
				modifiers |= KeyboardKeys.Alt;
			}
			if (KeyStates[KeyboardKeys.ShiftKey] == KeyState.Down)
			{
				if (KeyStates[KeyboardKeys.LShiftKey] == KeyState.Down)
					left = true;
				if (KeyStates[KeyboardKeys.RShiftKey] == KeyState.Down)
					right = true;
				modifiers |= KeyboardKeys.Shift;
			}

			// Dispatch the key.
			switch (keyboardData.Message)
			{
				case WindowMessages.SysKeyDown:
				case WindowMessages.KeyDown:
					KeyStates[(KeyboardKeys)keyCode] = KeyState.Down;
					if (secCode > -1)
                        KeyStates[(KeyboardKeys)secCode] = KeyState.Down;
					OnKeyDown((KeyboardKeys)keyCode, modifiers, keyboardData.MakeCode, left, right);
					break;
				case WindowMessages.SysKeyUp:
				case WindowMessages.KeyUp:
                    KeyStates[(KeyboardKeys)keyCode] = KeyState.Up;
					if (secCode > -1)
                        KeyStates[(KeyboardKeys)secCode] = KeyState.Up;
					OnKeyUp((KeyboardKeys)keyCode, modifiers, keyboardData.MakeCode, left, right);
					break;
			}
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
			if ((AllowBackground) || (Exclusive))
				_device.Flags |= RawInputDeviceFlags.InputSink;

			// Enable exclusive access.
			if (Exclusive)
				_device.Flags |= RawInputDeviceFlags.NoLegacy | RawInputDeviceFlags.AppKeys | RawInputDeviceFlags.NoHotKeys;

			_device.WindowHandle = InputInterface.Window.Handle;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonErrors.CannotBindInputDevice, "Failed to bind the keyboard device.");
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonErrors.CannotBindInputDevice, "Failed to bind the keyboard device.");
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Input device interface that owns this object.</param>
		public GorgonKeyboard(Input owner)
			: base(owner)
		{
		}
		#endregion
	}
}
