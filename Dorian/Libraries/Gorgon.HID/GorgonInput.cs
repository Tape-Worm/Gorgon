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
// Created: Friday, June 24, 2011 9:48:40 AM
// 
#endregion

using System;
using Forms = System.Windows.Forms;
using GorgonLibrary.Collections;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// Base for the HID device factory object.
	/// </summary>
	public abstract class GorgonHIDDeviceFactory
		: GorgonNamedObject
    {
		#region Properties.
		/// <summary>
		/// Property to return the names of the pointing devices attached to the system.
		/// </summary>
		public GorgonNamedObjectReadOnlyCollection<GorgonDeviceName> PointingDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the keyboard devices attached to the system.
		/// </summary>
		public GorgonNamedObjectReadOnlyCollection<GorgonDeviceName> KeyboardDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the joystick devices attached to the system.
		/// </summary>
		public GorgonNamedObjectReadOnlyCollection<GorgonDeviceName> JoystickDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether devices will auto-reacquire once the owner control gains focus.
		/// </summary>
		public bool AutoReacquireDevices
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected abstract GorgonNamedObjectReadOnlyCollection<GorgonDeviceName> EnumeratePointingDevices();

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected abstract GorgonNamedObjectReadOnlyCollection<GorgonDeviceName> EnumerateKeyboardDevices();

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected abstract GorgonNamedObjectReadOnlyCollection<GorgonDeviceName> EnumerateJoysticksDevices();

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para></remarks>		
		public abstract GorgonKeyboard CreateKeyboard(GorgonDeviceName keyboardName, Forms.Control window);

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">A <see cref="GorgonLibrary.HID.GorgonDeviceName">GorgonDeviceName</see> object containing the keyboard information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing an empty string for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>		
		public GorgonKeyboard CreateKeyboard(string keyboardName, Forms.Control window)
		{
			if (string.IsNullOrEmpty(keyboardName))
				return CreateKeyboard((GorgonDeviceName)null, window);
			else
				return CreateKeyboard(KeyboardDevices[keyboardName], window);
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		public GorgonKeyboard CreateKeyboard(Forms.Control window)
		{
			return CreateKeyboard(string.Empty, window);
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <returns>A new keyboard interface.</returns>
		public GorgonKeyboard CreateKeyboard()
		{
			return CreateKeyboard(string.Empty, null);
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceName">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceName"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>
		public abstract GorgonPointingDevice CreatePointingDevice(GorgonDeviceName pointingDeviceName, Forms.Control window);

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceName">A <see cref="GorgonLibrary.HID.GorgonDeviceName">GorgonDeviceName</see> object containing the pointing device information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing an empty string for <paramref name="pointingDeviceName"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>
		public GorgonPointingDevice CreatePointingDevice(string pointingDeviceName, Forms.Control window)
		{
			if (string.IsNullOrEmpty(pointingDeviceName))
				return CreatePointingDevice((GorgonDeviceName)null, window);
			else
				return CreatePointingDevice(PointingDevices[pointingDeviceName], window);
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>		
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		public GorgonPointingDevice CreatePointingDevice(Forms.Control window)
		{
			return CreatePointingDevice(string.Empty, window);
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <returns>A new pointing device interface.</returns>
		public GorgonPointingDevice CreatePointingDevice()
		{
			return CreatePointingDevice(string.Empty, null);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">Name of the joystick device to use.</param>
		/// <returns>A new joystick interface.</returns>
		/// <exception cref="System.ArgumentException">The <paramRef name="joystickName"/> is empty.</exception>
		/// <exception cref="System.ArgumentNullException">The joystickName is NULL.</exception>
		public GorgonJoystick CreateJoystick(string joystickName)
		{
			return CreateJoystick(joystickName, null);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">Name of the joystick device to use.</param>
		/// <param name="window">Window to bind with.</param>		
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentException">The <paramRef name="joystickName"/> is empty.</exception>
		/// <exception cref="System.ArgumentNullException">The joystickName is NULL.</exception>
		public GorgonJoystick CreateJoystick(string joystickName, Forms.Control window)
		{			
			if (joystickName == null)
				throw new ArgumentNullException("joystickName");
			if (string.IsNullOrEmpty(joystickName))
				throw new ArgumentException("joystickName");		

			return CreateJoystick(JoystickDevices[joystickName], window);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">A <see cref="GorgonLibrary.HID.GorgonDeviceName">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickName"/> is NULL.</exception>
		public abstract GorgonJoystick CreateJoystick(GorgonDeviceName joystickName, Forms.Control window);

		/// <summary>
		/// Function to enumerate devices attached to the system.
		/// </summary>
		public void EnumerateDevices()
		{
			PointingDevices = EnumeratePointingDevices();
			KeyboardDevices = EnumerateKeyboardDevices();
			JoystickDevices = EnumerateJoysticksDevices();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonHIDDeviceFactory"/> class.
		/// </summary>
		/// <param name="name">The name of the device manager.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (or Nothing) in VB.NET.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonHIDDeviceFactory(string name)
			: base(name)
		{
			EnumerateDevices();
			AutoReacquireDevices = true;
		}
		#endregion
	}
}
