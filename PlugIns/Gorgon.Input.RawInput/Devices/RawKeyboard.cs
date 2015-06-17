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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	internal class RawKeyboard
		: GorgonKeyboard
	{
		#region Variables.
		private readonly MessageFilter _messageFilter;	                // Window message filter.
		private RAWINPUTDEVICE _device;									// Input device.
		private readonly IntPtr _deviceHandle = IntPtr.Zero;			// Device handle.
		private bool _isBound;											// Flag to indicate that we're bound.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse the raw keyboard data.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event argments.</param>
		private void GetRawData(object sender, RawInputKeyboardEventArgs e)
		{
		    var state = KeyState.Down;				// Key state.

		    if ((BoundControl == null) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    if ((_deviceHandle != IntPtr.Zero) && (_deviceHandle != e.Handle))
		    {
		        return;
		    }

		    if ((Exclusive) && (!Acquired))
			{
				// Attempt to recapture.
			    if (BoundControl.Focused)
			    {
			        Acquired = true;
			    }
			    else
			    {
			        return;
			    }
			}

			// Get the key code.
			var keyCode = (KeyboardKeys)e.KeyboardData.VirtualKey;

			// Check for left/right versions.
			KeyboardKeys version = ((e.KeyboardData.Flags & RawKeyboardFlags.KeyE0) == RawKeyboardFlags.KeyE0) ? KeyboardKeys.RightVersion : KeyboardKeys.LeftVersion;

		    if ((e.KeyboardData.Message == WindowMessages.KeyUp) || (e.KeyboardData.Message == WindowMessages.SysKeyUp) ||
		        (e.KeyboardData.Message == WindowMessages.IMEKeyUp))
		    {
		        state = KeyState.Up;
		    }

		    // Determine right or left, and unifier key.
			switch (keyCode)
			{
				case KeyboardKeys.ControlKey:	// CTRL.
			        keyCode = (version & KeyboardKeys.RightVersion) == KeyboardKeys.RightVersion
			                      ? KeyboardKeys.RControlKey
			                      : KeyboardKeys.LControlKey;

			        KeyStates[KeyboardKeys.ControlKey] = state;
					break;
				case KeyboardKeys.Menu:			// ALT.
			        keyCode = (version & KeyboardKeys.RightVersion) == KeyboardKeys.RightVersion
			                      ? KeyboardKeys.RMenu
			                      : KeyboardKeys.LMenu;

					KeyStates[KeyboardKeys.Menu] = state;
					break;
				case KeyboardKeys.ShiftKey:		// Shift.
			        keyCode = e.KeyboardData.MakeCode == 0x36 ? KeyboardKeys.RShiftKey : KeyboardKeys.LShiftKey;

					KeyStates[KeyboardKeys.ShiftKey] = state;
					break;
			}

			// Dispatch the key.
			KeyStates[keyCode] = state;

		    if (state == KeyState.Down)
		    {
		        OnKeyDown(keyCode, e.KeyboardData.MakeCode);
		    }
		    else
		    {
		        OnKeyUp(keyCode, e.KeyboardData.MakeCode);
		    }
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			if (_isBound)
			{
				return;
			}

			UnbindDevice();

			if (_messageFilter != null)
			{
				_messageFilter.RawInputKeyboardData += GetRawData;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
		    if (AllowBackground)
		    {
		        _device.Flags |= RawInputDeviceFlags.InputSink;
		    }

		    _device.WindowHandle = BoundControl.Handle;

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_BIND_KEYBOARD);
		    }

			_isBound = true;
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputKeyboardData -= GetRawData;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.Remove;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_UNBIND_KEYBOARD);
		    }

			_isBound = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the device.</param>
		/// <param name="handle">The handle to the device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		internal RawKeyboard(GorgonRawInputService owner, string deviceName, IntPtr handle)
			: base(owner, deviceName)
		{
			GorgonApplication.Log.Print("Raw input keyboard interface created for handle 0x{0}.", LoggingLevel.Verbose, handle.FormatHex());

			_deviceHandle = handle;
			_messageFilter = owner.MessageFilter;
		}
		#endregion
	}
}
