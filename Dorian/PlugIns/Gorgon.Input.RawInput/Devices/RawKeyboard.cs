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
using Forms = System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
using GorgonLibrary.Input.Raw.Properties;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	internal class RawKeyboard
		: GorgonKeyboard
	{
		#region Variables.
		private MessageFilter _messageFilter;	                // Window message filter.
		private RAWINPUTDEVICE _device;					        // Input device.
		private readonly IntPtr _deviceHandle = IntPtr.Zero;	// Device handle.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse the raw keyboard data.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event argments.</param>
		private void GetRawData(object sender, RawInputEventArgs e)
		{
		    var state = KeyState.Down;				// Key state.

		    if ((BoundControl == null) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    if ((e.Data.Header.Type != RawInputType.Keyboard) ||
		        ((_deviceHandle != IntPtr.Zero) && (_deviceHandle != e.Handle)))
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
			var keyCode = (KeyboardKeys)e.Data.Keyboard.VirtualKey;

			// Check for left/right versions.
			KeyboardKeys version = ((e.Data.Keyboard.Flags & RawKeyboardFlags.KeyE0) == RawKeyboardFlags.KeyE0) ? KeyboardKeys.RightVersion : KeyboardKeys.LeftVersion;

		    if ((e.Data.Keyboard.Message == WindowMessages.KeyUp) || (e.Data.Keyboard.Message == WindowMessages.SysKeyUp) ||
		        (e.Data.Keyboard.Message == WindowMessages.IMEKeyUp))
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
			        keyCode = e.Data.Keyboard.MakeCode == 0x36 ? KeyboardKeys.RShiftKey : KeyboardKeys.LShiftKey;

					KeyStates[KeyboardKeys.ShiftKey] = state;
					break;
			}

			// Dispatch the key.
			KeyStates[keyCode] = state;

		    if (state == KeyState.Down)
		    {
		        OnKeyDown(keyCode, e.Data.Keyboard.MakeCode);
		    }
		    else
		    {
		        OnKeyUp(keyCode, e.Data.Keyboard.MakeCode);
		    }
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= GetRawData;
				Forms.Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
			}

			_messageFilter = new MessageFilter();
			_messageFilter.RawInputData += GetRawData;
			Forms.Application.AddMessageFilter(_messageFilter);

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
		    if ((AllowBackground) || (Exclusive))
		    {
		        _device.Flags |= RawInputDeviceFlags.InputSink;
		    }

		    // Enable exclusive access.
		    if (Exclusive)
		    {
		        _device.Flags |= RawInputDeviceFlags.NoLegacy | RawInputDeviceFlags.AppKeys | RawInputDeviceFlags.NoHotKeys;
		    }

		    _device.WindowHandle = BoundControl.Handle;

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_BIND_KEYBOARD);
		    }
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= GetRawData;
				Forms.Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
				_messageFilter = null;
			}

			_device.UsagePage = HIDUsagePage.Generic;
			_device.Usage = (ushort)HIDUsage.Keyboard;
			_device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_UNBIND_KEYBOARD);
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the device.</param>
		/// <param name="handle">The handle to the device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		internal RawKeyboard(GorgonInputFactory owner, string deviceName, IntPtr handle, Forms.Control boundWindow)
			: base(owner, deviceName, boundWindow)
		{
			Gorgon.Log.Print("Raw input keyboard interface created for handle 0x{0}.", LoggingLevel.Verbose, handle.FormatHex());

			_deviceHandle = handle;
		}
		#endregion
	}
}
