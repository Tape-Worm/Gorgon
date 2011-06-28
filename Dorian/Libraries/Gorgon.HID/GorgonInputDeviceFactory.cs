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
using System.Collections.Generic;
using System.Linq;
using Forms = System.Windows.Forms;
using GorgonLibrary.Collections;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// Base for the input device factory object.
	/// </summary>
	/// <remarks>This object is responsible for creating and maintaining the various input devices available to the system.
	/// <para>This object is capable of creating multiple interfaces for each keyboard and pointing device attached to the system.  
	/// If the user has more than one of these devices attached, they will be enumerated here and will be available as distinct object instances.</para>
	/// </remarks>
	public abstract class GorgonInputDeviceFactory
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of created input devices.
		/// </summary>
		internal IDictionary<Guid, GorgonInputDevice> Devices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the pointing devices attached to the system.
		/// </summary>
		public GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> PointingDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the keyboard devices attached to the system.
		/// </summary>
		public GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> KeyboardDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the joystick devices attached to the system.
		/// </summary>
		public GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> JoystickDevices
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
		/// Function to retrieve the factory UUID for the device.
		/// </summary>
		/// <param name="name">Name of the device.</param>
		/// <returns>The UUID for the device.</returns>
		private Guid GetDeviceUUID(GorgonInputDeviceName name)
		{
			Guid result = Guid.Empty;

			if (name != null)
				result = name.UUID;

			return result;
		}

		/// <summary>
		/// Function to destroy any outstanding device instance.
		/// </summary>
		private void DestroyDevices()
		{
			// Destroy any existing device references.
			var devices = Devices.ToArray<KeyValuePair<Guid, GorgonInputDevice>>();
			foreach (KeyValuePair<Guid, GorgonInputDevice> device in devices)
				device.Value.Dispose();
			Devices.Clear();
		}

		/// <summary>
		/// Function to retrieve an existing input device.
		/// </summary>
		/// <typeparam name="T">Type name of the device.</typeparam>
		/// <param name="name">Name of the device.</param>
		/// <returns>The input device if it was previously created, NULL (Nothing in VB.Net) if not.</returns>
		private T GetInputDevice<T>(GorgonInputDeviceName name) where T : GorgonInputDevice
		{
			Guid UUID = GetDeviceUUID(name);
			T device = null;

			if (Devices.ContainsKey(UUID))
			{
				device = Devices[UUID] as T;

				if (device == null)
					throw new ArgumentException("The device requested already exists and is not of the type '" + typeof(T).FullName + "'.", "name");
			}

			return device;
		}

		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected abstract GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumeratePointingDevices();

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected abstract GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumerateKeyboardDevices();

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected abstract GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumerateJoysticksDevices();

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para></remarks>		
		protected abstract GorgonKeyboard CreateKeyboardImpl(GorgonInputDeviceName keyboardName, Forms.Control window);

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceName">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceName"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>
		protected abstract GorgonPointingDevice CreatePointingDeviceImpl(GorgonInputDeviceName pointingDeviceName, Forms.Control window);

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">A <see cref="GorgonLibrary.HID.GorgonInputDeviceName">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickName"/> is NULL.</exception>
		protected abstract GorgonJoystick CreateJoystickImpl(GorgonInputDeviceName joystickName, Forms.Control window);

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para></remarks>		
		/// <exception cref="System.ArgumentException">Thrown when the device requested was already created but was not of the correct type.</exception>
		public GorgonKeyboard CreateKeyboard(GorgonInputDeviceName keyboardName, Forms.Control window)
		{
			GorgonKeyboard keyboard = GetInputDevice<GorgonKeyboard>(keyboardName);

			if (keyboard == null)
			{
				keyboard = CreateKeyboardImpl(keyboardName, window);				
				keyboard.UUID = GetDeviceUUID(keyboardName);
				Devices.Add(keyboard.UUID, keyboard);
			}

			return keyboard;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">A <see cref="GorgonLibrary.HID.GorgonInputDeviceName">GorgonDeviceName</see> object containing the keyboard information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing an empty string for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>		
		public GorgonKeyboard CreateKeyboard(string keyboardName, Forms.Control window)
		{
			if (string.IsNullOrEmpty(keyboardName))
				return CreateKeyboard((GorgonInputDeviceName)null, window);
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
		public GorgonPointingDevice CreatePointingDevice(GorgonInputDeviceName pointingDeviceName, Forms.Control window)
		{
			GorgonPointingDevice pointingDevice = GetInputDevice<GorgonPointingDevice>(pointingDeviceName);

			if (pointingDevice == null)
			{
				pointingDevice = CreatePointingDeviceImpl(pointingDeviceName, window);
				pointingDevice.UUID = GetDeviceUUID(pointingDeviceName);
				Devices.Add(pointingDevice.UUID, pointingDevice);
			}

			return pointingDevice;
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceName">A <see cref="GorgonLibrary.HID.GorgonInputDeviceName">GorgonDeviceName</see> object containing the pointing device information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing an empty string for <paramref name="pointingDeviceName"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>
		public GorgonPointingDevice CreatePointingDevice(string pointingDeviceName, Forms.Control window)
		{
			if (string.IsNullOrEmpty(pointingDeviceName))
				return CreatePointingDeviceImpl((GorgonInputDeviceName)null, window);
			else
				return CreatePointingDeviceImpl(PointingDevices[pointingDeviceName], window);
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

			return CreateJoystickImpl(JoystickDevices[joystickName], window);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">A <see cref="GorgonLibrary.HID.GorgonInputDeviceName">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickName"/> is NULL.</exception>
		public GorgonJoystick CreateJoystick(GorgonInputDeviceName joystickName, Forms.Control window)
		{
			GorgonJoystick joystickDevice = GetInputDevice<GorgonJoystick>(joystickName);

			if (joystickDevice == null)
			{
				joystickDevice = CreateJoystickImpl(joystickName, window);
				joystickDevice.UUID = GetDeviceUUID(joystickName);
				Devices.Add(joystickDevice.UUID, joystickDevice);
			}

			return joystickDevice;
		}

		/// <summary>
		/// Function to enumerate devices attached to the system.
		/// </summary>
		/// <remarks>Calling this function will invalidate any existing device objects created by this factory, use with care.</remarks>
		public void EnumerateDevices()
		{
			DestroyDevices();
			PointingDevices = EnumeratePointingDevices();
			KeyboardDevices = EnumerateKeyboardDevices();
			JoystickDevices = EnumerateJoysticksDevices();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDeviceFactory"/> class.
		/// </summary>
		/// <param name="name">The name of the device manager.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (or Nothing) in VB.NET.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonInputDeviceFactory(string name)
			: base(name)
		{
			Devices = new Dictionary<Guid, GorgonInputDevice>();
			EnumerateDevices();
			AutoReacquireDevices = true;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Destroy any outstanding device instances.
					DestroyDevices();
				}
			}
			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
