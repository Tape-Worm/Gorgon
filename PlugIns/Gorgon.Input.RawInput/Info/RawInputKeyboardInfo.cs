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
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The Raw Input implementation of keyboard information.
	/// </summary>
	class RawInputKeyboardInfo
		: IRawInputKeyboardInfo
	{
		#region Methods.
		/// <summary>
		/// Function to get keyboard information for the system keyboard.
		/// </summary>
		private void GetSystemKeyboardInfo()
		{
			int keyboardType = Win32API.GetKeyboardType(0);

			switch (keyboardType)
			{
				case 1:
					KeyboardType = KeyboardType.XT;
					KeyCount = 83;
					IndicatorCount = 3;
					break;
				case 2:
					KeyboardType = KeyboardType.OlivettiICO;
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case 3:
					KeyboardType = KeyboardType.AT;
					KeyCount = 84;
					IndicatorCount = 3;
					break;
				case 4:
					KeyboardType = KeyboardType.Enhanced;
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case 5:
					KeyboardType = KeyboardType.Nokia1050;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case 6:
					KeyboardType = KeyboardType.Nokia9140;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case 7:
					KeyboardType = KeyboardType.Japanese;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case 81:
					KeyboardType = KeyboardType.USB;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				default:
					KeyboardType = KeyboardType.Unknown;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
			}

			FunctionKeyCount = Win32API.GetKeyboardType(2);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputKeyboardInfo"/> class.
		/// </summary>
		/// <param name="uuid">Unique identifier for the keyboard device.</param>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="handle">Handle to the device.</param>
		public RawInputKeyboardInfo(Guid uuid, string name, string className, string hidPath, IntPtr handle)
		{
			Name = name;
			ClassName = className;
			HumanInterfaceDevicePath = hidPath;
			UUID = uuid;

			Handle = handle;

			GetSystemKeyboardInfo();

			// Get the usage information for the device.			
			if (handle == IntPtr.Zero)
			{
				return;
			}
			RID_DEVICE_INFO deviceInfo = Win32API.GetDeviceInfo(handle);
			Usage = (HIDUsage)deviceInfo.hid.usUsage;
			UsagePage = (HIDUsagePage)deviceInfo.hid.usUsagePage;

			KeyCount = deviceInfo.keyboard.dwNumberOfKeysTotal;
			IndicatorCount = deviceInfo.keyboard.dwNumberOfIndicators;
			FunctionKeyCount = deviceInfo.keyboard.dwNumberOfFunctionKeys;
		}
		#endregion

		#region IGorgonKeyboardInfo Members
		/// <inheritdoc/>
		public int KeyCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int IndicatorCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int FunctionKeyCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public KeyboardType KeyboardType
		{
			get;
			private set;
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
		/// <inheritdoc/>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public HIDUsage Usage
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public HIDUsagePage UsagePage
		{
			get;
			private set;
		}
		#endregion
	}
}
