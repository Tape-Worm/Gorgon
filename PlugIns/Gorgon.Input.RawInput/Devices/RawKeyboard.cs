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
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	class RawKeyboard
		: GorgonKeyboard
	{
		#region Variables.
		// Window message filter.
		private readonly MessageFilter _messageFilter;	                
		// Input device.
		private RAWINPUTDEVICE _device;									
		// Device handle.
		private readonly IntPtr _deviceHandle;
		// Flag to indicate that we're bound.
		private bool _isBound;											
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse the raw keyboard data.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		private void GetRawData(RawInputKeyboardEventArgs e)
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
			var keyCode = (KeyboardKey)e.KeyboardData.VirtualKey;

			// Check for left/right versions.
			KeyboardKey version = ((e.KeyboardData.Flags & RawKeyboardFlags.KeyE0) == RawKeyboardFlags.KeyE0) ? KeyboardKey.RightVersion : KeyboardKey.LeftVersion;

		    if ((e.KeyboardData.Message == WindowMessages.KeyUp) || (e.KeyboardData.Message == WindowMessages.SysKeyUp) ||
		        (e.KeyboardData.Message == WindowMessages.IMEKeyUp))
		    {
		        state = KeyState.Up;
		    }

		    // Determine right or left, and unifier key.
			switch (keyCode)
			{
				case KeyboardKey.ControlKey:	// CTRL.
			        keyCode = (version & KeyboardKey.RightVersion) == KeyboardKey.RightVersion
			                      ? KeyboardKey.RControlKey
			                      : KeyboardKey.LControlKey;

			        KeyStates[KeyboardKey.ControlKey] = state;
					break;
				case KeyboardKey.Menu:			// ALT.
			        keyCode = (version & KeyboardKey.RightVersion) == KeyboardKey.RightVersion
			                      ? KeyboardKey.RMenu
			                      : KeyboardKey.LMenu;

					KeyStates[KeyboardKey.Menu] = state;
					break;
				case KeyboardKey.ShiftKey:		// Shift.
			        keyCode = e.KeyboardData.MakeCode == 0x36 ? KeyboardKey.RShiftKey : KeyboardKey.LShiftKey;

					KeyStates[KeyboardKey.ShiftKey] = state;
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
				_messageFilter.RawInputKeyboardData = GetRawData;
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
			if (!Win32API.RegisterRawInputDevices(new[]
			                                      {
				                                      _device
			                                      },
			                                      1,
			                                      DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_BIND_KEYBOARD);
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
				_messageFilter.RawInputKeyboardData = null;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.Remove;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(new[]
			                                      {
				                                      _device
			                                      },
			                                      1,
			                                      DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_UNBIND_KEYBOARD);
			}

			_isBound = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <inheritdoc/>
		internal RawKeyboard(GorgonRawInputService owner, IRawInputKeyboardInfo info)
			: base(owner, info)
		{
			_deviceHandle = info.Handle;
			_messageFilter = owner.MessageFilter;
		}
		#endregion
	}
}
