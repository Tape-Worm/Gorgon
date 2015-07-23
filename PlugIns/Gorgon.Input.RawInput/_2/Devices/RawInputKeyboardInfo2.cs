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
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The Raw Input implementation of keyboard information.
	/// </summary>
	class RawInputKeyboardInfo2
		: IGorgonKeyboardInfo2
	{
		#region Variables.
		// Device description.
		private readonly string _deviceDescription;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the handle for the device.
		/// </summary>
		public IntPtr Handle
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and parse out the device information settings for a raw input keyboard.
		/// </summary>
		/// <param name="deviceInfo">Raw input device information.</param>
		public void AssignRawInputDeviceInfo(ref RID_DEVICE_INFO_KEYBOARD deviceInfo)
		{
			KeyCount = deviceInfo.dwNumberOfKeysTotal;
			IndicatorCount = deviceInfo.dwNumberOfIndicators;
			FunctionKeyCount = deviceInfo.dwNumberOfFunctionKeys;

			if (Enum.IsDefined(typeof(KeyboardType), deviceInfo.dwType))
			{
				KeyboardType = (KeyboardType)deviceInfo.dwType;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputKeyboardInfo2"/> class.
		/// </summary>
		/// <param name="deviceDescription">The device description.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="handle">Handle to the device.</param>
		public RawInputKeyboardInfo2(string deviceDescription, string className, string hidPath, IntPtr handle)
		{
			_deviceDescription = deviceDescription;
			ClassName = className;
			HumanInterfaceDevicePath = hidPath;
			Handle = handle;

			KeyboardType = Win32API.KeyboardType;
			FunctionKeyCount = Win32API.FunctionKeyCount;

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
					KeyCount = -1;
					IndicatorCount = -1;
					break;
			}
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
		public string Description => string.Format(Resources.GORINP_RAW_DESC_KEYBOARD, _deviceDescription, KeyCount, KeyboardType);

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Keyboard;
		#endregion
	}
}
