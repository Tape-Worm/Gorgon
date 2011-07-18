#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, June 24, 2011 10:04:29 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
using Forms = System.Windows.Forms;
using GorgonLibrary.HID;

namespace GorgonLibrary.HID.RawInput
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	internal class RawKeyboard
		: GorgonKeyboard
	{
		#region Variables.
		private MessageFilter _messageFilter = null;	// Window message filter.
		private RAWINPUTDEVICE _device;					// Input device.
		private IntPtr _deviceHandle = IntPtr.Zero;		// Device handle.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse the raw keyboard data.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event argments.</param>
		private void GetRawData(object sender, RawInputEventArgs e)
		{
			int keyCode = 0;		// Virtual key code.
			int secCode = -1;		// Secondary code.
			bool left = false;		// Left modifier.
			bool right = false;		// Right modifier.
			KeyboardKeys modifiers;	// Keyboard modifiers.

			if ((BoundWindow == null) || (BoundWindow.Disposing))
				return;

			if ((e.Data.Header.Type != RawInputType.Keyboard) || ((_deviceHandle != IntPtr.Zero) && (_deviceHandle != e.Handle)))
				return;

			if ((Exclusive) && (!Acquired))
			{
				// Attempt to recapture.
				if (BoundWindow.Focused)
					Acquired = true;
				else
					return;
			}

			// Get the key code.
			modifiers = KeyboardKeys.None;
			keyCode = (int)e.Data.Keyboard.VirtualKey;			

			// Determine right or left, and unifier key.
			switch ((KeyboardKeys)e.Data.Keyboard.VirtualKey)
			{
				case KeyboardKeys.ControlKey:	// CTRL.
					secCode = (int)KeyboardKeys.ControlKey;
					if ((e.Data.Keyboard.Flags & RawKeyboardFlags.KeyE0) != 0)
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
					if ((e.Data.Keyboard.Flags & RawKeyboardFlags.KeyE0) != 0)
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
					if ((e.Data.Keyboard.Flags & RawKeyboardFlags.KeyE0) != 0)
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
			switch (e.Data.Keyboard.Message)
			{
				case WindowMessages.SysKeyDown:
				case WindowMessages.KeyDown:
					KeyStates[(KeyboardKeys)keyCode] = KeyState.Down;
					if (secCode > -1)
                        KeyStates[(KeyboardKeys)secCode] = KeyState.Down;
					OnKeyDown((KeyboardKeys)keyCode, modifiers, e.Data.Keyboard.MakeCode, left, right);
					break;
				case WindowMessages.SysKeyUp:
				case WindowMessages.KeyUp:
                    KeyStates[(KeyboardKeys)keyCode] = KeyState.Up;
					if (secCode > -1)
                        KeyStates[(KeyboardKeys)secCode] = KeyState.Up;
					OnKeyUp((KeyboardKeys)keyCode, modifiers, e.Data.Keyboard.MakeCode, left, right);
					break;
			}
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= new EventHandler<RawInputEventArgs>(GetRawData);
				System.Windows.Forms.Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
			}

			_messageFilter = new MessageFilter();
			_messageFilter.RawInputData += new EventHandler<RawInputEventArgs>(GetRawData);
			System.Windows.Forms.Application.AddMessageFilter(_messageFilter);

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
			if ((AllowBackground) || (Exclusive))
				_device.Flags |= RawInputDeviceFlags.InputSink;

			// Enable exclusive access.
			if (Exclusive)
				_device.Flags |= RawInputDeviceFlags.NoLegacy | RawInputDeviceFlags.AppKeys | RawInputDeviceFlags.NoHotKeys;

			_device.WindowHandle = BoundWindow.Handle;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonResult.DriverError, "Failed to bind the keyboard device.");
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= new EventHandler<RawInputEventArgs>(GetRawData);
				System.Windows.Forms.Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(_device))
				throw new GorgonException(GorgonResult.DriverError, "Failed to unbind the keyboard device.");
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="handle">The handle to the device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		internal RawKeyboard(GorgonRawInputDeviceFactory owner, IntPtr handle, Forms.Control boundWindow)
			: base(owner, "Raw Input Keyboard", boundWindow)
		{
			Gorgon.Log.Print("Raw input keyboard interface created for handle 0x{0}.", GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(handle));
			_deviceHandle = handle;
		}
		#endregion
	}
}
