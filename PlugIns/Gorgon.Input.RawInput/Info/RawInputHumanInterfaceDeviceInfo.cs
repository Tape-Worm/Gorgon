#region MIT
// 
// Gorgon.
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
// Created: Tuesday, July 7, 2015 1:59:31 AM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The Raw Input implementation of human interface device information.
	/// </summary>
	class RawInputHumanInterfaceDeviceInfo
		: IRawInputHumanInterfaceDeviceInfo
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputHumanInterfaceDeviceInfo"/> class.
		/// </summary>
		/// <param name="uuid">Unique identifier for the keyboard device.</param>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="handle">Handle to the device.</param>
		public RawInputHumanInterfaceDeviceInfo(Guid uuid, string name, string className, string hidPath, IntPtr handle)
		{
			Name = name;
			ClassName = className;
			HumanInterfaceDevicePath = hidPath;
			UUID = uuid;

			Handle = handle;

			RID_DEVICE_INFO deviceInfo = Win32API.GetDeviceInfo(handle);
			Usage = (HIDUsage)deviceInfo.hid.usUsage;
			UsagePage = (HIDUsagePage)deviceInfo.hid.usUsagePage;

			ProductID = deviceInfo.hid.dwProductId;
			VendorID = deviceInfo.hid.dwVendorId;
			Version = deviceInfo.hid.dwVersionNumber;
		}
		#endregion

		#region IGorgonHumanInterfaceDeviceInfo Members
		/// <inheritdoc/>
		public int ProductID
		{
			get;
		}

		/// <inheritdoc/>
		public int VendorID
		{
			get;
		}

		/// <inheritdoc/>
		public int Version
		{
			get;
		}
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get;
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get;
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.HumanInterfaceDevice;

		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public string Name
		{
			get;
		}
		#endregion

		#region IGorgonRawInputHumanInterfaceDeviceInfo Members
		/// <inheritdoc/>
		public IntPtr Handle
		{
			get;
		}

		/// <inheritdoc/>
		public HIDUsage Usage
		{
			get;
		}

		/// <inheritdoc/>
		public HIDUsagePage UsagePage
		{
			get;
		}
		#endregion
	}
}
