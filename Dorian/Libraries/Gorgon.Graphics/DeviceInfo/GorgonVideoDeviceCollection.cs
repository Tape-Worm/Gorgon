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
		/// Function to add a list of devices to the collection.
		/// </summary>
		/// <param name="devices">Devices to add to the collection.</param>
		internal void AddRange(IEnumerable<GorgonVideoDevice> devices)
		{
			if (devices == null)
				throw new ArgumentNullException("device");

			if (devices.Count() == 0)
				throw new ArgumentException("Must have at least one device.", "devices");

			AddItems(devices);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		internal void Clear()
		{
			foreach (GorgonVideoDevice device in this)
				device.Dispose();

			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDeviceCollection"/> class.
		/// </summary>
		internal GorgonVideoDeviceCollection()
			: base(false)
		{
		}
		#endregion
	}
}
