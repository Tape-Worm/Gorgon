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
// Created: Thursday, June 30, 2011 1:24:52 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// A raw input generic HID device object.
	/// </summary>
	internal class RawHIDDevice
		: GorgonCustomHID
	{
		#region Variables.
		// Device data.
		private readonly IRawInputHumanInterfaceDeviceInfo _deviceData;
		// Window message filter.;		
		private readonly MessageFilter _messageFilter;
		// Input device.
		private RAWINPUTDEVICE _device;
		// Flag to indicate that the device was bound.
		private bool _isBound;										
		#endregion

		#region Methods.
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
				
			}

			_device.UsagePage = _deviceData.UsagePage;
			_device.Usage = (ushort)_deviceData.Usage;
			_device.Flags = RawInputDeviceFlags.None;

			// Enable background access.
		    if (AllowBackground)
		    {
		        _device.Flags |= RawInputDeviceFlags.InputSink;
		    }
 
		    _device.WindowHandle = BoundControl.Handle;

			// Attempt to register the device.

			_isBound = true;
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			if (_messageFilter != null)
			{
				
			}

			_device.UsagePage = _deviceData.UsagePage;
			_device.Usage = (ushort)_deviceData.Usage;
			_device.Flags = RawInputDeviceFlags.Remove;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.

			_isBound = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <inheritdoc/>
		internal RawHIDDevice(GorgonRawInputService owner, IRawInputHumanInterfaceDeviceInfo deviceInfo)
			: base(owner, deviceInfo)
		{
			_deviceData = deviceInfo;
			_messageFilter = owner.MessageFilter;
			SetData("BinaryData", new byte[0]);
		}
		#endregion
	}
}
