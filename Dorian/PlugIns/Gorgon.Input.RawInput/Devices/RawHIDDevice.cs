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
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Input.Raw.Properties;
using GorgonLibrary.Native;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// A raw input generic HID device object.
	/// </summary>
	internal class RawHIDDevice
		: GorgonCustomHID
	{
		#region Variables.
		private readonly GorgonRawInputDeviceInfo _deviceData;		// Device data.
		private MessageFilter _messageFilter;				        // Window message filter.
		private RAWINPUTDEVICE _device;								// Input device.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse the raw keyboard data.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event argments.</param>
		private void GetRawData(object sender, RawInputEventArgs e)
		{
		    if ((BoundControl == null) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    if ((e.Data.Header.Type != RawInputType.HID) || (_deviceData.Handle != e.Handle))
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

			SetData("BinaryData", e.Data.HIDData);
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			if (_messageFilter != null)
			{
				_messageFilter.RawInputData -= GetRawData;
				Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
			}

			_messageFilter = new MessageFilter();
			_messageFilter.RawInputData += GetRawData;
			Application.AddMessageFilter(_messageFilter);

			_device.UsagePage = _deviceData.UsagePage;
			_device.Usage = (ushort)_deviceData.Usage;
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
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_BIND_HID);
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
				Application.RemoveMessageFilter(_messageFilter);
				_messageFilter.Dispose();
				_messageFilter = null;
			}

			_device.UsagePage = _deviceData.UsagePage;
			_device.Usage = (ushort)_deviceData.Usage;
			_device.Flags = RawInputDeviceFlags.None;
			_device.WindowHandle = IntPtr.Zero;

			// Attempt to register the device.
		    if (!Win32API.RegisterRawInputDevices(_device))
		    {
		        throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_CANNOT_UNBIND_HID);
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawHIDDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceData">The HID device name object.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		internal RawHIDDevice(GorgonInputFactory owner, GorgonRawInputDeviceInfo deviceData, Control boundWindow)
			: base(owner, deviceData.Name, boundWindow)
		{
			Gorgon.Log.Print("Raw input HID interface created for handle 0x{0}.", LoggingLevel.Verbose, deviceData.Handle.FormatHex());

			_deviceData = deviceData;
			SetData("BinaryData", new byte[0]);
		}
		#endregion
	}
}
