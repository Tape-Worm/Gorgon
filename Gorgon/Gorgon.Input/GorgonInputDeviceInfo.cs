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
using Gorgon.Core;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// Input device type.
	/// </summary>
	[Flags]
	public enum InputDeviceType
	{
		/// <summary>
		/// No device type.
		/// </summary>
		None = 0,
		/// <summary>
		/// Keyboard input device.
		/// </summary>
		Keyboard = 1,
		/// <summary>
		/// Pointing input device.
		/// </summary>
		PointingDevice = 2,
		/// <summary>
		/// Joystick/gamepad input device.
		/// </summary>
		Joystick = 4,
		/// <summary>
		/// Generic HID input device.
		/// </summary>
		HID = 8
	}

	/// <summary>
	/// Name of an input device object.
	/// </summary>
	/// <remarks>Devices are often associated by strings, handles, GUIDs, or even integer IDs by the operating system and whatever back end library (Raw Input, DirectInput, WinForms, etc...) is being used, Gorgon uses this object to wrap up the handle and provide 
	/// user friendly information about the device, such as its name, <see cref="Gorgon.Input.GorgonInputDeviceInfo.HIDPath">HID path</see>, and <see cref="Gorgon.Input.GorgonInputDeviceInfo.ClassName">class name</see>.
	/// <para>Implementors of input plug-ins must implement this in the plug-in and return a handle of whatever type is required by the back end input library.  For example, DirectInput uses GUIDs to ID the devices, so the implementor must 
	/// use a <see cref="System.Guid"/> type as a handle.  See the GoronRawInputDeviceName.cs file for an example of how to do this.</para>
	/// </remarks>
	public abstract class GorgonInputDeviceInfo
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
		public virtual string ClassName
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the device is connected or not.
		/// </summary>
		public abstract bool IsConnected
		{
			get;
		}

		/// <summary>
		/// Property to return the type of input device.
		/// </summary>
		public InputDeviceType InputDeviceType
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return Name;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDeviceInfo"/> class.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <param name="deviceType">The type of device.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/>, <paramref name="className"/> or <paramref name="hidPath"/> are NULL or empty.</exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		protected GorgonInputDeviceInfo(string name, InputDeviceType deviceType, string className, string hidPath)
			: base(name)
		{
		    if (string.IsNullOrWhiteSpace(className))
		    {
                throw new ArgumentException(Resources.GORINP_PARAMETER_NULL_EMPTY, "className");
		    }

		    if (string.IsNullOrWhiteSpace(hidPath))
		    {
                throw new ArgumentException(Resources.GORINP_PARAMETER_NULL_EMPTY, "hidPath");
		    }

		    InputDeviceType = deviceType;
			ClassName = className;
			HIDPath = hidPath;
			UUID = Guid.NewGuid();
		}
		#endregion
	}
}
