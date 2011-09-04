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
// Created: Thursday, July 21, 2011 3:15:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Native;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of video devices.
	/// </summary>
	public class GorgonVideoDeviceCollection
		: GorgonBaseNamedObjectCollection<GorgonVideoDevice>
	{
		#region Properties.
		/// <summary>
		/// Property to return a video device by its index.
		/// </summary>
		public GorgonVideoDevice this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a video device by its name.
		/// </summary>
		public GorgonVideoDevice this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the capabilities of each device in the collection.
		/// </summary>
		internal void GetCapabilities()
		{
			foreach (var device in this)
				device.GetDeviceCapabilities();
		}

		/// <summary>
		/// Function to retrieve a specific video output from a device by the placement of a control.
		/// </summary>
		/// <param name="control">Control to find.</param>
		internal GorgonVideoOutput GetOutputFromControl(System.Windows.Forms.Control control)
		{
			IntPtr monitorHandle = Win32API.GetMonitor(control);

			if (monitorHandle == IntPtr.Zero)
				throw new ArgumentException("Could not find the monitor that the control '0x" + control.Handle.FormatHex() + "' is located on.", "control");

			// Find the correct video output.
			var videoOutput = (from device in this
							   from output in device.Outputs
							   where output.Handle == monitorHandle
							   select output).SingleOrDefault();

			if (videoOutput == null)
				throw new ArgumentException("Could not find the monitor that the control '0x" + control.Handle.FormatHex() + "' is located on.", "control");

			return videoOutput;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDeviceCollection"/> class.
		/// </summary>
		/// <param name="devices">Video devices to add.</param>
		internal GorgonVideoDeviceCollection(IEnumerable<KeyValuePair<string, GorgonVideoDevice>> devices)
			: base(false)
		{
			string deviceName = string.Empty;

			if (devices == null)
				throw new ArgumentNullException("device");

			if (devices.Count() == 0)
				throw new ArgumentException("Must have at least one device.", "devices");

			var deviceList = from device in devices
							 select device.Value;

			AddItems(deviceList);
		}
		#endregion
	}
}
