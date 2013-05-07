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
using GorgonLibrary.Input.Properties;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Base for the input device factory object.
	/// </summary>
	/// <remarks>This object is responsible for creating and maintaining the various input devices available to the system.
	/// <para>This object is capable of creating multiple interfaces for each keyboard and pointing device attached to the system.  
	/// If the user has more than one of these devices attached, they will be enumerated here and will be available as distinct object instances.</para>
	/// </remarks>
	public abstract class GorgonInputFactory
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed;											// Flag to indicate that the object was disposed.
	    #endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of created input devices.
		/// </summary>
		internal IDictionary<string, GorgonInputDevice> Devices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the pointing devices attached to the system.
		/// </summary>
		public GorgonInputDeviceInfoCollection PointingDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the keyboard devices attached to the system.
		/// </summary>
		public GorgonInputDeviceInfoCollection KeyboardDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the joystick devices attached to the system.
		/// </summary>
		public GorgonInputDeviceInfoCollection JoystickDevices
		{
			get;
			private set;
		}

        // ReSharper disable InconsistentNaming
		/// <summary>
		/// Property to return the names of custom HIDs.
		/// </summary>
		public GorgonInputDeviceInfoCollection CustomHIDs
		{
			get;
			private set;
		}
        // ReSharper restore InconsistentNaming

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
		/// <param name="deviceType">Type of input device.</param>
		/// <returns>The UUID for the device.</returns>
		private string GetDeviceUUID(GorgonInputDeviceInfo name, Type deviceType)
		{
			string result = Guid.Empty.ToString();

		    if (name != null)
		    {
		        result = name.UUID.ToString();
		    }

		    result += "_" + deviceType.FullName;

			return result;
		}

		/// <summary>
		/// Function to destroy any outstanding device instance.
		/// </summary>
		private void DestroyDevices()
		{
			// Destroy any existing device references.
			var devices = Devices.ToArray();

		    foreach (var device in devices)
		    {
		        device.Value.Dispose();
		    }

		    Devices.Clear();
		}

		/// <summary>
		/// Function to retrieve an existing input device.
		/// </summary>
		/// <typeparam name="T">Type name of the device.</typeparam>
		/// <param name="name">Name of the device.</param>
		/// <returns>The input device if it was previously created, NULL (Nothing in VB.Net) if not.</returns>
		private T GetInputDevice<T>(GorgonInputDeviceInfo name) where T : GorgonInputDevice
		{
		    Type devType = typeof(T);
			string uuid = GetDeviceUUID(name, devType);
			T device = null;

			if (Devices.ContainsKey(uuid))
			{
				device = Devices[uuid] as T;

			    if (device == null)
			    {
			        throw new ArgumentException(string.Format(Resources.GORINP_DEVICE_ALREADY_EXISTS_TYPE_MISMATCH, devType.FullName), "name");
			    }
			}

			return device;
		}

		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected abstract IEnumerable<GorgonInputDeviceInfo> EnumeratePointingDevices();

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected abstract IEnumerable<GorgonInputDeviceInfo> EnumerateKeyboardDevices();

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected abstract IEnumerable<GorgonInputDeviceInfo> EnumerateJoysticksDevices();

        // ReSharper disable InconsistentNaming
        /// <summary>
		/// Function to enumerate device types for which there is no class wrapper and will return data in a custom property collection.
		/// </summary>		
		/// <returns>A list of custom HID types.</returns>
		/// <remarks>Custom devices are devices that are unknown to Gorgon.  The user can provide a subclass that will take the data returned from the
		/// device and parse it out and provide properties depending on the device.</remarks>
        protected abstract IEnumerable<GorgonInputDeviceInfo> EnumerateCustomHIDs();
        // ReSharper restore InconsistentNaming

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="keyboardInfo">Name of the keyboard device to create.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardInfo"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para></remarks>		
		protected abstract GorgonKeyboard CreateKeyboardImpl(Forms.Control window, GorgonInputDeviceInfo keyboardInfo);

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="pointingDeviceInfo">Name of the pointing device device to create.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceInfo"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>
		protected abstract GorgonPointingDevice CreatePointingDeviceImpl(Forms.Control window, GorgonInputDeviceInfo pointingDeviceInfo);

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="joystickInfo">A <see cref="GorgonLibrary.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickInfo"/> is NULL.</exception>
		protected abstract GorgonJoystick CreateJoystickImpl(Forms.Control window, GorgonInputDeviceInfo joystickInfo);

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="hidInfo">A <see cref="GorgonLibrary.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the HID information.</param>
		/// <returns>A new custom HID interface.</returns>
		/// <remarks>Implementors must implement this method if they wish to return data from a undefined (custom) device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="hidInfo"/> is NULL.</exception>
		protected abstract GorgonCustomHID CreateCustomHIDImpl(Forms.Control window, GorgonInputDeviceInfo hidInfo);

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="hidName">Name of the HID to use.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new custom HID interface.</returns>
		/// <remarks>Data from a custom HID will be returned via the <see cref="P:GorgonLibrary.Input.GorgonCustomHID.Data">Data</see> property.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>		
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="hidName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="hidName"/> is empty.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when a device with the <paramref name="hidName"/> could not be found.</exception>
		public GorgonCustomHID CreateCustomHID(Forms.Control window, string hidName)
		{
			if (hidName == null)
			{
			    throw new ArgumentNullException("hidName");
			}

            if (string.IsNullOrWhiteSpace(hidName))
            {
                throw new ArgumentException(Resources.GORINP_PARAMETER_EMPTY, "hidName");
            }

		    if (!CustomHIDs.Contains(hidName))
			{
			    throw new KeyNotFoundException(string.Format(Resources.GORINP_HID_NOT_FOUND, hidName));
			}

			GorgonInputDeviceInfo deviceInfo = CustomHIDs[hidName];
			var customHID = GetInputDevice<GorgonCustomHID>(deviceInfo);

			if (customHID == null)
			{
				customHID = CreateCustomHIDImpl(window, deviceInfo);
				customHID.UUID = GetDeviceUUID(deviceInfo, customHID.GetType());
				Devices.Add(customHID.UUID, customHID);
			}

			return customHID;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="keyboardName">The name of the keyboard to use.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing an empty string for <paramref name="keyboardName"/> will use the system keyboard (i.e. data from all keyboards will be tracked by the same interface).
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>		
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when a keyboard with the <paramref name="keyboardName"/> could not be found.</exception>
		public GorgonKeyboard CreateKeyboard(Forms.Control window, string keyboardName)
		{
			GorgonInputDeviceInfo deviceInfo = null;

		    if (!string.IsNullOrWhiteSpace(keyboardName))
			{
			    if (!KeyboardDevices.Contains(keyboardName))
			    {
                    throw new ArgumentException(string.Format(Resources.GORINP_KEYBOARD_NOT_FOUND, keyboardName), "keyboardName");
			    }

			    deviceInfo = KeyboardDevices[keyboardName];
			}

			var keyboardDevice = GetInputDevice<GorgonKeyboard>(deviceInfo);

			if (keyboardDevice == null)
			{
				keyboardDevice = CreateKeyboardImpl(window, deviceInfo);
				keyboardDevice.UUID = GetDeviceUUID(deviceInfo, keyboardDevice.GetType());
				Devices.Add(keyboardDevice.UUID, keyboardDevice);
			}

			return keyboardDevice;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		public GorgonKeyboard CreateKeyboard(Forms.Control window)
		{
			return CreateKeyboard(window, string.Empty);
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="pointingDeviceName">The name of the pointing device to use.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing an empty string for <paramref name="pointingDeviceName"/> will use the system pointing device (i.e. data from all pointing devices will be tracked by the same interface).
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when a pointing device with the <paramref name="pointingDeviceName"/> could not be found.</exception>
		public GorgonPointingDevice CreatePointingDevice(Forms.Control window, string pointingDeviceName)
		{
			GorgonInputDeviceInfo deviceInfo = null;

		    if (!string.IsNullOrWhiteSpace(pointingDeviceName))
			{
			    if (!PointingDevices.Contains(pointingDeviceName))
			    {
                    throw new ArgumentException(string.Format(Resources.GORINP_POINTINGDEVICE_NOT_FOUND, pointingDeviceName), "pointingDeviceName");
			    }

			    deviceInfo = PointingDevices[pointingDeviceName];				
			}

			var pointingDevice = GetInputDevice<GorgonPointingDevice>(deviceInfo);

			if (pointingDevice == null)
			{
				pointingDevice = CreatePointingDeviceImpl(window, deviceInfo);
				pointingDevice.UUID = GetDeviceUUID(deviceInfo, pointingDevice.GetType());
				Devices.Add(pointingDevice.UUID, pointingDevice);
			}

			return pointingDevice;
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>		
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the pointing device could not be found.</exception>
		public GorgonPointingDevice CreatePointingDevice(Forms.Control window)
		{
			return CreatePointingDevice(window, string.Empty);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="joystickName">Name of the joystick to use.</param>		
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="joystickName"/> is empty.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="joystickName"/> is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when a joystick with the <paramref name="joystickName"/> could not be found.</exception>
		public GorgonJoystick CreateJoystick(Forms.Control window, string joystickName)
		{
            if (joystickName == null)
            {
                throw new ArgumentNullException("joystickName");
            }

            if (string.IsNullOrWhiteSpace(joystickName))
            {
                throw new ArgumentException(Resources.GORINP_PARAMETER_EMPTY, "joystickName");
            }

		    if (!JoystickDevices.Contains(joystickName))
			{
                throw new ArgumentException(string.Format(Resources.GORINP_JOYSTICK_NOT_FOUND, joystickName), "joystickName");
			}

			GorgonInputDeviceInfo deviceInfo = JoystickDevices[joystickName];
			var joystickDevice = GetInputDevice<GorgonJoystick>(deviceInfo);

			if (joystickDevice == null)
			{
				joystickDevice = CreateJoystickImpl(window, deviceInfo);
				joystickDevice.UUID = GetDeviceUUID(deviceInfo, joystickDevice.GetType());
				Devices.Add(joystickDevice.UUID, joystickDevice);
			}

			return joystickDevice;
		}

		/// <summary>
		/// Function to enumerate devices attached to the system.
		/// </summary>
		/// <remarks>Calling this method will invalidate any existing device objects created by this factory, use with care.</remarks>
		public void EnumerateDevices()
		{
			DestroyDevices();
			PointingDevices = new GorgonInputDeviceInfoCollection(EnumeratePointingDevices());
			KeyboardDevices = new GorgonInputDeviceInfoCollection(EnumerateKeyboardDevices());
			JoystickDevices = new GorgonInputDeviceInfoCollection(EnumerateJoysticksDevices());
			CustomHIDs = new GorgonInputDeviceInfoCollection(EnumerateCustomHIDs());
		}

		/// <summary>
		/// Function to return a new input device factory object.
		/// </summary>
		/// <param name="plugInType">Type name of the input device factory plug-in.</param>
		/// <returns>The input device factory object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugInType"/> parameter was NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="plugInType"/> parameter is empty.</exception>
		/// <exception cref="System.InvalidCastException">Thrown when the input plug-in was not found or was not the correct type.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the input factory could not be created.</exception>
		public static GorgonInputFactory CreateInputFactory(string plugInType)
		{
		    if (plugInType == null)
			{
			    throw new ArgumentNullException("plugInType");
			}

            if (string.IsNullOrWhiteSpace(plugInType))
            {
                throw new ArgumentException(Resources.GORINP_PARAMETER_EMPTY, "plugInType");
            }

		    var plugIn =
		        Gorgon.PlugIns.FirstOrDefault(
		            item => string.Compare(item.Name, plugInType, StringComparison.OrdinalIgnoreCase) == 0) as
		        GorgonInputPlugIn;

            if (plugIn == null)
            {
                throw new InvalidCastException(string.Format(Resources.GORINP_PLUGIN_NOT_FOUND, plugInType));
            }

		    GorgonInputFactory factory = plugIn.GetFactory();

            if (factory == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORINP_CANNOT_CREATE, plugInType));
            }

		    Gorgon.AddTrackedObject(factory);

			return factory;
		}

		/// <summary>
		/// Function to return a new input device factory object.
		/// </summary>
		/// <param name="plugInType">Type of the input device factory plug-in.</param>
		/// <returns>The input device factory object.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="plugInType"/> parameter is empty or NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the input device factory plug-in type was not found.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the input device factory plug-in requested is not an input device factory.</para>
		/// </exception>
		public static GorgonInputFactory CreateInputFactory(Type plugInType)
		{
			return CreateInputFactory(plugInType.FullName);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputFactory"/> class.
		/// </summary>
		/// <param name="name">The name of the device manager.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonInputFactory(string name)
			: base(name)
		{
			Devices = new Dictionary<string, GorgonInputDevice>();
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
					Gorgon.RemoveTrackedObject(this);

					// Destroy any outstanding device instances.
					DestroyDevices();
				}
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
