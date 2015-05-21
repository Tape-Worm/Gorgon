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
// Created: Friday, June 24, 2011 10:04:41 AM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Object representing event arguments for the raw input keyboard events.
	/// </summary>
	internal class RawInputKeyboardEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the handle to the device that is receiving the event notification.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the raw input keyboard data.
		/// </summary>
		public RAWINPUTKEYBOARD KeyboardData
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="device">The device that sent the event.</param>
		/// <param name="data">Raw input data to pass.</param>
		public RawInputKeyboardEventArgs(IntPtr device, ref RAWINPUTKEYBOARD data)
		{
			Handle = device;
			KeyboardData = data;
		}
		#endregion
	}

	/// <summary>
	/// Object representing event arguments for the raw input pointing device events.
	/// </summary>
	internal class RawInputPointingDeviceEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the handle to the device that is receiving the event notification.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the raw input pointing device data.
		/// </summary>
		public RAWINPUTMOUSE PointingDeviceData
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="device">The device that sent the event.</param>
		/// <param name="data">Raw input data to pass.</param>
		public RawInputPointingDeviceEventArgs(IntPtr device, ref RAWINPUTMOUSE data)
		{
			Handle = device;
			PointingDeviceData = data;
		}
		#endregion
	}

	/// <summary>
	/// Object representing event arguments for the raw input HID events.
	/// </summary>
	internal class RawInputHIDEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the handle to the device that is receiving the event notification.
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the raw input HID data.
		/// </summary>
		public RAWINPUTHID HIDData
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the HID buffer data.
		/// </summary>
		public byte[] HIDBuffer
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="device">The device that sent the event.</param>
		/// <param name="data">Raw input data to pass.</param>
		/// <param name="hidBuffer">HID buffer data.</param>
		public RawInputHIDEventArgs(IntPtr device, ref RAWINPUTHID data, byte[] hidBuffer)
		{
			Handle = device;
			HIDData = data;
			HIDBuffer = hidBuffer;
		}
		#endregion
	}
}
