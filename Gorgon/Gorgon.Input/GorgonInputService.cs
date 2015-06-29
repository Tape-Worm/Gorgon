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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Input.Properties;
using Gorgon.UI;

namespace Gorgon.Input
{
	/// <summary>
	/// Base for the input device factory object.
	/// </summary>
	/// <remarks>This object is responsible for creating and maintaining the various input devices available to the system.
	/// <para>This object is capable of creating multiple interfaces for each keyboard and pointing device attached to the system.  
	/// If the user has more than one of these devices attached, they will be enumerated here and will be available as distinct object instances.</para>
	/// </remarks>
	public abstract class GorgonInputService
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;											
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
		/// Property to return when a pointing device has been flagged as exclusive.
		/// </summary>
		protected internal InputDeviceType ExclusiveDevices
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the names of the pointing devices attached to the system.
		/// </summary>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> PointingDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the keyboard devices attached to the system.
		/// </summary>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> KeyboardDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the names of the joystick devices attached to the system.
		/// </summary>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> JoystickDevices
		{
			get;
			private set;
		}

        // ReSharper disable InconsistentNaming
		/// <summary>
		/// Property to return the names of custom HIDs.
		/// </summary>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> CustomHIDs
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
		private static string GetDeviceUUID(GorgonInputDeviceInfo name, Type deviceType)
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

			ExclusiveDevices = InputDeviceType.None;

		    Devices.Clear();
		}

		/// <summary>
		/// Function to retrieve an existing input device.
		/// </summary>
		/// <typeparam name="T">Type name of the device.</typeparam>
		/// <param name="name">Name of the device.</param>
		/// <returns>The input device if it was previously created, NULL (<i>Nothing</i> in VB.Net) if not.</returns>
		private T GetInputDevice<T>(GorgonInputDeviceInfo name) where T : GorgonInputDevice
		{
		    Type devType = typeof(T);
			string uuid = GetDeviceUUID(name, devType);

		    if (!Devices.ContainsKey(uuid))
		    {
		        return null;
		    }

		    var device = Devices[uuid] as T;

		    if (device == null)
		    {
		        throw new ArgumentException(string.Format(Resources.GORINP_DEVICE_ALREADY_EXISTS_TYPE_MISMATCH, devType.FullName), "name");
		    }

		    return device;
		}

		/// <summary>
		/// Function to unbind all devices.
		/// </summary>
		protected void UnbindAllDevices()
		{
			foreach (var device in Devices)
			{
				device.Value.Enabled = false;
			}
		}

		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumeratePointingDevices();

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateKeyboardDevices();

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateJoysticksDevices();

        // ReSharper disable InconsistentNaming
        /// <summary>
		/// Function to enumerate device types for which there is no class wrapper and will return data in a custom property collection.
		/// </summary>		
		/// <returns>A list of custom HID types.</returns>
		/// <remarks>Custom devices are devices that are unknown to Gorgon.  The user can provide a subclass that will take the data returned from the
		/// device and parse it out and provide properties depending on the device.</remarks>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateCustomHIDs();
        // ReSharper restore InconsistentNaming

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="keyboardInfo">Name of the keyboard device to create.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardInfo"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para></remarks>		
		protected abstract GorgonKeyboard CreateKeyboardImpl(Control window, GorgonInputDeviceInfo keyboardInfo);

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="pointingDeviceInfo">Name of the pointing device device to create.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceInfo"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>
		protected abstract GorgonPointingDevice CreatePointingDeviceImpl(Control window, GorgonInputDeviceInfo pointingDeviceInfo);

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="joystickInfo">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickInfo"/> is NULL.</exception>
		protected abstract GorgonJoystick CreateJoystickImpl(Control window, GorgonInputDeviceInfo joystickInfo);

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="hidInfo">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the HID information.</param>
		/// <returns>A new custom HID interface.</returns>
		/// <remarks>Implementors must implement this method if they wish to return data from a undefined (custom) device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="hidInfo"/> is NULL.</exception>
		protected abstract GorgonCustomHID CreateCustomHIDImpl(Control window, GorgonInputDeviceInfo hidInfo);

		/// <summary>
		/// Function called before enumeration begins.
		/// </summary>
		/// <remarks>Implementors can use this method to cache enumeration data.</remarks>
		protected virtual void OnBeforeEnumerate()
		{
		}

		/// <summary>
		/// Function called after enumeration ends.
		/// </summary>
		/// <remarks>Implementors can use this method to clean up any cached enumeration data.</remarks>
		protected virtual void OnAfterEnumerate()
		{
		}

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="hidName">Name of the HID to use.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new custom HID interface.</returns>
		/// <remarks>Data from a custom HID will be returned via the <see cref="P:GorgonLibrary.Input.GorgonCustomHID.Data">Data</see> property.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</para>
		/// </remarks>		
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="hidName"/> is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="hidName"/> is empty.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when a device with the <paramref name="hidName"/> could not be found.</exception>
		public GorgonCustomHID CreateCustomHID(Control window, string hidName)
		{
			if (hidName == null)
			{
			    throw new ArgumentNullException("hidName");
			}

            if (string.IsNullOrWhiteSpace(hidName))
            {
                throw new ArgumentException(Resources.GORINP_PARAMETER_EMPTY, "hidName");
            }

		    GorgonInputDeviceInfo deviceInfo;
		    if (!CustomHIDs.TryGetValue(hidName, out deviceInfo))
		    {
                throw new KeyNotFoundException(string.Format(Resources.GORINP_HID_NOT_FOUND, hidName));
		    }

			var customHID = GetInputDevice<GorgonCustomHID>(deviceInfo);

			if (customHID == null)
			{
				customHID = CreateCustomHIDImpl(window, deviceInfo);
				customHID.UUID = GetDeviceUUID(deviceInfo, customHID.GetType());
				customHID.DeviceType = InputDeviceType.HID;
				Devices.Add(customHID.UUID, customHID);
			}

			customHID.ClearData();
			customHID.Bind(window);
			customHID.Enabled = true;
			customHID.Exclusive = (ExclusiveDevices & InputDeviceType.HID) == InputDeviceType.HID;

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
		public GorgonKeyboard CreateKeyboard(Control window, string keyboardName)
		{
			GorgonInputDeviceInfo deviceInfo = null;

		    if ((!string.IsNullOrWhiteSpace(keyboardName)) && (!KeyboardDevices.TryGetValue(keyboardName, out deviceInfo)))
			{
                throw new ArgumentException(string.Format(Resources.GORINP_KEYBOARD_NOT_FOUND, keyboardName), "keyboardName");
			}

			var keyboardDevice = GetInputDevice<GorgonKeyboard>(deviceInfo);

			if (keyboardDevice == null)
			{
				keyboardDevice = CreateKeyboardImpl(window, deviceInfo);
				keyboardDevice.DeviceType = InputDeviceType.Keyboard;
				keyboardDevice.UUID = GetDeviceUUID(deviceInfo, keyboardDevice.GetType());
				Devices.Add(keyboardDevice.UUID, keyboardDevice);
			}

			keyboardDevice.GetDefaultKeyMapping();
			keyboardDevice.KeyStateResetMode = KeyStateResetMode.ResetAll;
			keyboardDevice.KeyStates.Reset();
			keyboardDevice.Bind(window);
			keyboardDevice.Enabled = true;
			keyboardDevice.Exclusive = (ExclusiveDevices & InputDeviceType.Keyboard) == InputDeviceType.Keyboard;

			return keyboardDevice;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		public GorgonKeyboard CreateKeyboard(Control window)
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
		public GorgonPointingDevice CreatePointingDevice(Control window, string pointingDeviceName)
		{
			GorgonInputDeviceInfo deviceInfo = null;

		    if ((!string.IsNullOrWhiteSpace(pointingDeviceName)) && (!PointingDevices.TryGetValue(pointingDeviceName, out deviceInfo)))
			{
                throw new ArgumentException(string.Format(Resources.GORINP_POINTINGDEVICE_NOT_FOUND, pointingDeviceName), "pointingDeviceName");
			}

			// Only reset this once.
			if (!Devices.Any(item => item.Value is GorgonPointingDevice))
			{
				GorgonPointingDevice.ResetCursor();
			}

			var pointingDevice = GetInputDevice<GorgonPointingDevice>(deviceInfo);

			if (pointingDevice == null)
			{
				pointingDevice = CreatePointingDeviceImpl(window, deviceInfo);
				pointingDevice.DeviceType = InputDeviceType.PointingDevice;
				pointingDevice.UUID = GetDeviceUUID(deviceInfo, pointingDevice.GetType());
				Devices.Add(pointingDevice.UUID, pointingDevice);
			}

			// Default to the last position for the mouse cursor.
			pointingDevice.Position = window.PointToClient(Cursor.Position);
			pointingDevice.PositionRange = RectangleF.Empty;
			pointingDevice.WheelRange = Point.Empty;
			pointingDevice.ResetButtons();
			pointingDevice.Bind(window);
			pointingDevice.Enabled = true;
			pointingDevice.Exclusive = (ExclusiveDevices & InputDeviceType.PointingDevice) == InputDeviceType.PointingDevice;

			if (!pointingDevice.Exclusive)
			{
				pointingDevice.ShowCursor();
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
		public GorgonPointingDevice CreatePointingDevice(Control window)
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="joystickName"/> is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when a joystick with the <paramref name="joystickName"/> could not be found.</exception>
		public GorgonJoystick CreateJoystick(Control window, string joystickName)
		{
            if (joystickName == null)
            {
                throw new ArgumentNullException("joystickName");
            }

            if (string.IsNullOrWhiteSpace(joystickName))
            {
                throw new ArgumentException(Resources.GORINP_PARAMETER_EMPTY, "joystickName");
            }

			GorgonInputDeviceInfo deviceInfo;

            if (!JoystickDevices.TryGetValue(joystickName, out deviceInfo))
            {
                throw new ArgumentException(string.Format(Resources.GORINP_JOYSTICK_NOT_FOUND, joystickName), "joystickName");
            }

			var joystickDevice = GetInputDevice<GorgonJoystick>(deviceInfo);

			if (joystickDevice == null)
			{
				joystickDevice = CreateJoystickImpl(window, deviceInfo);
				joystickDevice.UUID = GetDeviceUUID(deviceInfo, joystickDevice.GetType());
				joystickDevice.DeviceType = InputDeviceType.Joystick;
				Devices.Add(joystickDevice.UUID, joystickDevice);
			}

			joystickDevice.Initialize();
			joystickDevice.Bind(window);
			joystickDevice.Enabled = true;
			joystickDevice.Exclusive = (ExclusiveDevices & InputDeviceType.Joystick) == InputDeviceType.Joystick;

			return joystickDevice;
		}

		/// <summary>
		/// Function to enumerate devices attached to the system.
		/// </summary>
		/// <remarks>Calling this method will invalidate any existing device objects created by this factory, use with care.</remarks>
		public void EnumerateDevices()
		{
			DestroyDevices();

			OnBeforeEnumerate();

			try
			{
				PointingDevices = EnumeratePointingDevices();
				KeyboardDevices = EnumerateKeyboardDevices();
				JoystickDevices = EnumerateJoysticksDevices();
				CustomHIDs = EnumerateCustomHIDs();
			}
			finally
			{
				OnAfterEnumerate();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputService"/> class.
		/// </summary>
		/// <param name="name">The name of the device manager.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonInputService(string name)
			: base(name)
		{
			ExclusiveDevices = InputDeviceType.None;
			Devices = new Dictionary<string, GorgonInputDevice>();
			AutoReacquireDevices = true;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					GorgonApplication.RemoveTrackedObject(this);

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
