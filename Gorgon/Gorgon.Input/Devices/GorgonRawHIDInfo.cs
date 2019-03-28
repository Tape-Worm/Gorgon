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
// Created: Thursday, September 10, 2015 10:53:11 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <summary>
	/// Provides capability information about a Raw Input Human Interface Device.
	/// </summary>
	public class GorgonRawHIDInfo
		: IGorgonRawHIDInfo
	{
		#region Properties.
		/// <summary>
		/// Property to return the device class name.
		/// </summary>
		public string DeviceClass
		{
			get;
		}

		/// <summary>
		/// Property to return a human friendly description of the device.
		/// </summary>
		public string Description
		{
			get;
		}

		/// <summary>
		/// Property to return the device handle.
		/// </summary>
		public IntPtr Handle
		{
			get;
		}

		/// <summary>
		/// Property to return human interface device path for the device.
		/// </summary>
		public string HIDPath
		{
			get;
		}

		/// <summary>
		/// Property to return the product ID for the device.
		/// </summary>
		public int ProductID
		{
			get;
		}

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		public int VendorID
		{
			get;
		}

		/// <summary>
		/// Property to return the version number for the device.
		/// </summary>
		public int Version
		{
			get;
		}

		/// <summary>
		/// Property to return the top level collection usage value for this device.
		/// </summary>
		public HIDUsage Usage
		{
			get;
		}

		/// <summary>
		/// Property to return the top level collection usage page value for this device.
		/// </summary>
		public HIDUsagePage UsagePage
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawHIDInfo"/> class.
		/// </summary>
		/// <param name="deviceHandle">The device handle.</param>
		/// <param name="hidPath">The hid path.</param>
		/// <param name="className">Name of the class.</param>
		/// <param name="description">The description.</param>
		/// <param name="deviceInfo">The device information.</param>
		internal GorgonRawHIDInfo(IntPtr deviceHandle, string hidPath, string className, string description, RID_DEVICE_INFO_HID deviceInfo)
		{
			Handle = deviceHandle;
			HIDPath = hidPath;
			DeviceClass = className;
			Description = description;

			ProductID = deviceInfo.dwProductId;
			VendorID = deviceInfo.dwVendorId;
			Version = deviceInfo.dwVersionNumber;
			Usage = (HIDUsage)deviceInfo.usUsage;
			UsagePage = (HIDUsagePage)deviceInfo.usUsagePage;
		}
		#endregion
	}
}
