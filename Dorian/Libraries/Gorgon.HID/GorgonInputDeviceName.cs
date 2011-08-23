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

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Name of an input device object.
	/// </summary>
	/// <remarks>Devices are often associated by strings, handles, GUIDs, or even integer IDs by the operating system and whatever back end library (Raw Input, DirectInput, WinForms, etc...) is being used, Gorgon uses this object to wrap up the handle and provide 
	/// user friendly information about the device, such as its name, <see cref="P:GorgonLibrary.Input.GorgonInputDeviceName.HIDPath">HID path</see>, and <see cref="P:GorgonLibrary.Input.GorgonInputDeviceName.ClassName">class name</see>.
	/// <para>Implementors of input plug-ins must implement this in the plug-in and return a handle of whatever type is required by the back end input library.  For example, DirectInput uses GUIDs to ID the devices, so the implementor must 
	/// use a <see cref="System.Guid"/> type as a handle.  See the GoronRawInputDeviceName.cs file for an example of how to do this.</para>
	/// </remarks>
	public abstract class GorgonInputDeviceName
		: GorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the internal ID for the factory.
		/// </summary>
		internal Guid UUID
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
		/// Property to return whether the device is connected or not.
		/// </summary>
		public virtual bool IsConnected
		{
			get;
			protected set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDeviceName"/> class.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="connected">TRUE if the device is presently connected and operating, FALSE if it is not.</param>
		/// <exception cref="System.ArgumentException">The handle is set to 0.</exception>
		/// <exception cref="System.ArgumentNullException">Either the name, className or hidPath are NULL or empty.</exception>
		protected GorgonInputDeviceName(string name, string className, string hidPath, bool connected)
			: base(name)
		{
			if (string.IsNullOrEmpty(className))
				throw new ArgumentNullException("className");
			if (string.IsNullOrEmpty(hidPath))
				throw new ArgumentNullException("hidPath");

			this.Name = name;
			this.ClassName = className;
			this.HIDPath = hidPath;
			this.UUID = Guid.NewGuid();
			this.IsConnected = connected;
		}
		#endregion
	}
}
