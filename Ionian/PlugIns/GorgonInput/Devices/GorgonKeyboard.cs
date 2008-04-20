#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
			if ((_background) || (_exclusive))
				_device.Flags |= RawInputDeviceFlags.InputSink;

			// Enable exclusive access.
			if (_exclusive)
				_device.Flags |= RawInputDeviceFlags.NoLegacy | RawInputDeviceFlags.AppKeys | RawInputDeviceFlags.NoHotKeys;

			_device.WindowHandle = InputInterface.Window.Handle;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new InputCannotBindKeyboardException();

			_acquired = true;
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
				throw new InputCannotBindKeyboardException();

			_acquired = false;
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
