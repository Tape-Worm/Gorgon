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
// Created: Thursday, June 30, 2011 6:35:52 AM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The Raw Input implementation of mouse information.
	/// </summary>
	class RawInputMouseInfo
		: IRawInputMouseInfo
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputMouseInfo"/> class.
		/// </summary>
		/// <param name="uuid">Unique identifier for the pointing device.</param>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="handle">Handle to the device.</param>
		public RawInputMouseInfo(Guid uuid, string name, string className, string hidPath, IntPtr handle)
		{
			Name = name;
			ClassName = className;
			HumanInterfaceDevicePath = hidPath;
			UUID = uuid;

			Handle = handle;

			// Get the usage information for the device.			
			if (handle != IntPtr.Zero)
			{
				RID_DEVICE_INFO deviceInfo = Win32API.GetDeviceInfo(handle);
				Usage = (HIDUsage)deviceInfo.hid.usUsage;
				UsagePage = (HIDUsagePage)deviceInfo.hid.usUsagePage;

				ButtonCount = deviceInfo.mouse.dwNumberOfButtons;
				SamplingRate = deviceInfo.mouse.dwSampleRate;
				HasHorizontalWheel = deviceInfo.mouse.fHasHorizontalWheel;
			}
			else
			{
				ButtonCount = SystemInformation.MouseButtons;
				SamplingRate = 0;
				HasHorizontalWheel = false;
			}
		}
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType
		{
			get
			{
				return InputDeviceType.Keyboard;
			}
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion

		#region IGorgonRawInputKeyboardInfo Members
		/// <summary>
		/// Property to return the handle to the device.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the HID usage.
		/// </summary>
		public HIDUsage Usage
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the HID usage page.
		/// </summary>
		public HIDUsagePage UsagePage
		{
			get;
			private set;
		}
		#endregion

		#region IGorgonMouseInfo Members
		/// <inheritdoc/>
		public int ButtonCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int SamplingRate
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public bool HasHorizontalWheel
		{
			get;
			private set;
		}
		#endregion
	}
}
