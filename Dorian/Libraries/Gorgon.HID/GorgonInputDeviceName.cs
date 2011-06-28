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
// Created: Friday, June 24, 2011 10:03:55 AM
// 
#endregion

using System;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// Name of an input device object.
	/// </summary>
	public class GorgonDeviceName
		: GorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the handle to the device (if applicable).
		/// </summary>
		public IntPtr Handle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the HID path to the device.
		/// </summary>
		public string HIDPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the class name of the device.
		/// </summary>
		public string ClassName
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the GUID for the device (if applicable).
		/// </summary>
		public Guid GUID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the numeric ID of the device (if applicable).
		/// </summary>
		public int ID
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceName"/> struct.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="handle">The device handle (if applicable).</param>
		/// <param name="guid">GUID for the device (if applicable).</param>
		/// <param name="id">Numeric ID for the device (if applicable).</param>
		/// <exception cref="System.ArgumentException">The handle is set to 0.</exception>
		/// <exception cref="System.ArgumentNullException">Either the name, className or hidPath are NULL or empty.</exception>
		public GorgonDeviceName(string name, string className, string hidPath, IntPtr handle, Guid guid, int id)
			: base(name)
		{
			if (string.IsNullOrEmpty(className))
				throw new ArgumentNullException("className");
			if (string.IsNullOrEmpty(hidPath))
				throw new ArgumentNullException("hidPath");

			this.Name = name;
			this.Handle = handle;
			this.ClassName = className;
			this.HIDPath = hidPath;
			this.GUID = guid;
			this.ID = id;
		}
		#endregion
	}
}
