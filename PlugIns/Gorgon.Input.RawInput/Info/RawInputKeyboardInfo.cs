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
			KeyboardType = Win32API.KeyboardType;

			switch (KeyboardType)
			{
				case KeyboardType.XT:
					KeyCount = 83;
					IndicatorCount = 3;
					break;
				case KeyboardType.OlivettiICO:
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case KeyboardType.AT:
					KeyCount = 84;
					IndicatorCount = 3;
					break;
				case KeyboardType.Enhanced:
					KeyCount = 102;
					IndicatorCount = 3;
					break;
				case KeyboardType.Nokia1050:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case KeyboardType.Nokia9140:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case KeyboardType.Japanese:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				case KeyboardType.USB:
					KeyCount = -1;
					IndicatorCount = -1;
					break;
				default:
					KeyboardType = KeyboardType.Unknown;
					KeyCount = -1;
					IndicatorCount = -1;
					break;
			}

			FunctionKeyCount = Win32API.FunctionKeyCount;
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
			/*RID_DEVICE_INFO deviceInfo = Win32API.GetDeviceInfo(handle);
			Usage = (HIDUsage)deviceInfo.hid.usUsage;
			UsagePage = (HIDUsagePage)deviceInfo.hid.usUsagePage;

			KeyCount = deviceInfo.keyboard.dwNumberOfKeysTotal;
			IndicatorCount = deviceInfo.keyboard.dwNumberOfIndicators;
			FunctionKeyCount = deviceInfo.keyboard.dwNumberOfFunctionKeys;*/
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
		public InputDeviceType InputDeviceType => InputDeviceType.Keyboard;

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

		#region IGorgonRawInputKeyboardInfo Members
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
