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
// Created: Tuesday, September 07, 2015 1:51:39 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <inheritdoc cref="IGorgonRawInputDeviceInfo"/>
	class RawMouseInfo
		: IGorgonMouseInfo, IGorgonRawInputDeviceInfo
	{
		#region Properties.
		/// <inheritdoc/>
		public string Description
		{
			get;
		}

		/// <inheritdoc/>
		public string HIDPath
		{
			get;
		}

		/// <inheritdoc/>
		public string Class
		{
			get;
		}

		/// <inheritdoc/>
		public IntPtr Handle
		{
			get;
		}

		/// <inheritdoc/>
		public int SampleRate
		{
			get;
		}

		/// <inheritdoc/>
		public int ButtonCount
		{
			get;
		}

		/// <inheritdoc/>
		public bool HasHorizontalWheel
		{
			get;
		}

		/// <inheritdoc/>
		public int MouseID
		{
			get;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawMouseInfo"/> class.
		/// </summary>
		/// <param name="deviceHandle">The device handle.</param>
		/// <param name="hidPath">The human interface device path.</param>
		/// <param name="className">The class of device.</param>
		/// <param name="description">The human readable description of this device.</param>
		/// <param name="deviceInfo">The data about the device.</param>
		public RawMouseInfo(IntPtr deviceHandle, string hidPath, string className, string description, RID_DEVICE_INFO_MOUSE deviceInfo)
		{
			Handle = deviceHandle;
			Description = description;
			HIDPath = hidPath;
			Class = className;

			ButtonCount = deviceInfo.dwNumberOfButtons;
			SampleRate = deviceInfo.dwSampleRate;
			HasHorizontalWheel = deviceInfo.fHasHorizontalWheel;
			MouseID = deviceInfo.dwId;
		}
		#endregion
	}
}
