﻿#region MIT
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
// Created: Tuesday, September 07, 2015 1:51:39 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <summary>
	/// Provides information about a Raw Input keyboard device.
	/// </summary>
	class RawKeyboardInfo
		: IGorgonRawInputDeviceInfo, IGorgonKeyboardInfo
	{
		#region Properties.
		/// <summary>
		/// Property to return a human friendly description of the device.
		/// </summary>
		public string Description
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
		/// Property to return the device class name.
		/// </summary>
		public string Class
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
		/// Property to return the total number of keys present on the keyboard.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value may or may not be accurate depending on the implementation. That is, for some systems, this will be an estimate, and for others this will be accurate.
		/// </para> 
		/// </remarks>
		public int KeyCount
		{
			get;
		}

		/// <summary>
		/// Property to return the number of LED indicators on the keyboard.
		/// </summary>
		public int IndicatorCount
		{
			get;
		}

		/// <summary>
		/// Property to return the number of function keys on the keyboard.
		/// </summary>
		public int FunctionKeyCount
		{
			get;
		}

		/// <summary>
		/// Property to return the type of keyboard.
		/// </summary>
		public KeyboardType KeyboardType
		{
			get;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawKeyboardInfo"/> class.
		/// </summary>
		/// <param name="deviceHandle">The device handle.</param>
		/// <param name="hidPath">The human interface device path.</param>
		/// <param name="className">The class of device.</param>
		/// <param name="description">The human readable description of this device.</param>
		/// <param name="deviceInfo">The data about the device.</param>
		public RawKeyboardInfo(IntPtr deviceHandle, string hidPath, string className, string description, RID_DEVICE_INFO_KEYBOARD deviceInfo)
		{
			Handle = deviceHandle;
			Description = description;
			HIDPath = hidPath;
			Class = className;

			FunctionKeyCount = deviceInfo.dwNumberOfFunctionKeys;
			IndicatorCount = deviceInfo.dwNumberOfIndicators;
			KeyCount = deviceInfo.dwNumberOfKeysTotal;
			KeyboardType = (KeyboardType)deviceInfo.dwType;
		}
		#endregion
	}
}
