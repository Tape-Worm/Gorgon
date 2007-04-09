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
using SharpUtilities.Native.Win32;
using SharpUtilities.Mathematics;
using GorgonLibrary;

namespace GorgonLibrary.Input
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
		public void GetRawData(RAWINPUTKEYBOARD keyboardData)
		{
			int keyCode = 0;		// Virtual key code.
			int secCode = -1;		// Secondary code.
			KeyboardKeys modifiers;	// Keyboard modifiers.

			// If the key is not recognized, then exit.
			if (((int)keyboardData.VirtualKey < 0) || ((int)keyboardData.VirtualKey >= _keys.Length))
				return;

			// Get the key code.
			modifiers = KeyboardKeys.None;
			keyCode = (int)keyboardData.VirtualKey;			

			// Determine right or left, and unifier key.
			switch ((KeyboardKeys)keyboardData.VirtualKey)
			{
				case KeyboardKeys.ControlKey:	// CTRL.
					secCode = (int)KeyboardKeys.ControlKey;
					if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) != 0)
						keyCode = (int)KeyboardKeys.RControlKey;
					else
						keyCode = (int)KeyboardKeys.LControlKey;
					break;
				case KeyboardKeys.Menu:			// ALT.
					secCode = (int)KeyboardKeys.Menu;
					if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) != 0)
						keyCode = (int)KeyboardKeys.RMenu;
					else
						keyCode = (int)KeyboardKeys.LMenu;
					break;
				case KeyboardKeys.ShiftKey:		// Shift.
					secCode = (int)KeyboardKeys.ShiftKey;
					if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) != 0)
						keyCode = (int)KeyboardKeys.RShiftKey;
					else
						keyCode = (int)KeyboardKeys.LShiftKey;
					break;
			}

			// Check for modifiers.
			modifiers = KeyboardKeys.None;
			if (this[KeyboardKeys.ControlKey])
				modifiers |= KeyboardKeys.Control;
			if (this[KeyboardKeys.Menu])
				modifiers |= KeyboardKeys.Alt;
			if (this[KeyboardKeys.ShiftKey])
				modifiers |= KeyboardKeys.Shift;

			// Dispatch the key.
			switch (keyboardData.Message)
			{
				case WindowMessages.SysKeyDown:
				case WindowMessages.KeyDown:
					_keys[keyCode] = true;
					if (secCode > -1)
						_keys[secCode] = true;
					OnKeyDown((KeyboardKeys)keyCode, modifiers, keyboardData.MakeCode);
					break;
				case WindowMessages.SysKeyUp:
				case WindowMessages.KeyUp:
					_keys[keyCode] = false;
					if (secCode > -1)
						_keys[secCode] = false;
					OnKeyUp((KeyboardKeys)keyCode, modifiers, keyboardData.MakeCode);
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
				throw new CannotBindMouseException(null);
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
				throw new CannotBindKeyboardException(null);
		}

		/// <summary>
		/// Function to forcefully bind the device.
		/// </summary>
		public void Bind()
		{
			BindDevice();
			_bound = true;
			_acquired = true;
		}

		/// <summary>
		/// Function to forcefully unbind the device.
		/// </summary>
		public void Unbind()
		{
			UnbindDevice();
			_bound = false;
			_acquired = false;
		}

		/// <summary>
		/// Function to acquire the input device.
		/// </summary>
		public override void Acquire()
		{
			try
			{
				if (_bound)
					BindDevice();
				_acquired = true;
			}
			catch
			{
				throw;
			}			
		}

		/// <summary>
		/// Function to release the input device.
		/// </summary>
		public override void Release()
		{
			try
			{
				if (_bound)
					UnbindDevice();
				_acquired = false;
			}
			catch
			{
				throw;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owning input interface.</param>
		public GorgonKeyboard(InputDevices owner)
			: base(owner)
		{
		}
		#endregion
	}
}
