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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Events;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// The types of available input devices.
	/// </summary>
	[Flags]
	public enum InputDeviceExclusivity
	{
		/// <summary>
		/// No input device is exclusive.
		/// </summary>
		None = 0,
		/// <summary>
		/// Keyboard is exclusive.
		/// </summary>
		Keyboard = 1,
		/// <summary>
		/// Pointing device is exclusive
		/// </summary>
		Mouse = 2,
		/// <summary>
		/// Gaming device is exclusive.
		/// </summary>
		Joystick = 4,
		/// <summary>
		/// Human Interface Device is exclusive.
		/// </summary>
		HumanInterfaceDevice = 8
	}

	// TODO: Note that the dispose method must be called or else weirdness will happen.
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
		internal ConcurrentDictionary<Guid, GorgonInputDevice> Devices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the log file for debugging.
		/// </summary>
		protected internal IGorgonLog Log
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return when a pointing device has been flagged as exclusive.
		/// </summary>
		protected internal InputDeviceExclusivity ExclusiveDevices
		{
			get;
			internal set;
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
		/// Handles the BeforeUnbind event of the Keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonBeforeInputUnbindEventArgs"/> instance containing the event data.</param>
		private void Keyboard_BeforeUnbind(object sender, GorgonBeforeInputUnbindEventArgs e)
		{
			GorgonKeyboard keyboard = sender as GorgonKeyboard;

			if ((keyboard == null) || (!Devices.ContainsKey(keyboard.Info.UUID)))
			{
				return;
			}

			GorgonInputDevice device;

			Devices.TryRemove(keyboard.Info.UUID, out device);

			// Check the collection for other devices that are exclusive.
			if (!Devices.Any(item => item.Value.Exclusive && item.Value is GorgonKeyboard))
			{
				ExclusiveDevices &= ~InputDeviceExclusivity.Keyboard;
			}
		}

		/// <summary>
		/// Handles the BeforeUnbind event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonBeforeInputUnbindEventArgs"/> instance containing the event data.</param>
		private void Mouse_BeforeUnbind(object sender, GorgonBeforeInputUnbindEventArgs e)
		{
			GorgonPointingDevice mouse = sender as GorgonPointingDevice;

			if ((mouse == null) || (!Devices.ContainsKey(mouse.Info.UUID)))
			{
				return;
			}

			GorgonInputDevice device;

			Devices.TryRemove(mouse.Info.UUID, out device);

			// Check the collection for other devices that are exclusive.
			if (!Devices.Any(item => item.Value.Exclusive && item.Value is GorgonPointingDevice))
			{
				ExclusiveDevices &= ~InputDeviceExclusivity.Mouse;
			}
		}

		/// <summary>
		/// Handles the BeforeUnbind event of the Joystick control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonBeforeInputUnbindEventArgs"/> instance containing the event data.</param>
		private void Joystick_BeforeUnbind(object sender, GorgonBeforeInputUnbindEventArgs e)
		{
			GorgonJoystick joystick = sender as GorgonJoystick;

			if ((joystick == null) || (!Devices.ContainsKey(joystick.Info.UUID)))
			{
				return;
			}

			GorgonInputDevice device;

			Devices.TryRemove(joystick.Info.UUID, out device);

			// Check the collection for other devices that are exclusive.
			if (!Devices.Any(item => item.Value.Exclusive && item.Value is GorgonJoystick))
			{
				ExclusiveDevices &= ~InputDeviceExclusivity.Joystick;
			}
		}

		/// <summary>
		/// Handles the BeforeUnbind event of the Human Interface Device control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonBeforeInputUnbindEventArgs"/> instance containing the event data.</param>
		private void HumanInterfaceDevice_BeforeUnbind(object sender, GorgonBeforeInputUnbindEventArgs e)
		{
			GorgonCustomHID hid = sender as GorgonCustomHID;

			if ((hid == null) || (!Devices.ContainsKey(hid.Info.UUID)))
			{
				return;
			}

			GorgonInputDevice device;

			Devices.TryRemove(hid.Info.UUID, out device);

			// Check the collection for other devices that are exclusive.
			if (!Devices.Any(item => item.Value.Exclusive && item.Value is GorgonCustomHID))
			{
				ExclusiveDevices &= ~InputDeviceExclusivity.HumanInterfaceDevice;
			}
		}

		/// <summary>
		/// Function to retrieve a input device instance.
		/// </summary>
		/// <typeparam name="T">Type name of the device.</typeparam>
		/// <param name="deviceInfo">Device information.</param>
		/// <returns>The input device if it was previously created, <b>null</b> (<i>Nothing</i> in VB.Net) if not.</returns>
		private T GetInputDevice<T>(IGorgonInputDeviceInfo deviceInfo) 
			where T : GorgonInputDevice
		{
			Type devType = typeof(T);
			GorgonInputDevice device;
			Guid guid = deviceInfo == null ? Guid.Empty : deviceInfo.UUID;

			if (!Devices.TryGetValue(guid, out device))
			{
				return null;
			}

			T result = device as T;

			if (result == null)
			{
				throw new ArgumentException(string.Format(Resources.GORINP_DEVICE_ALREADY_EXISTS_TYPE_MISMATCH, devType.FullName), "deviceInfo");
			}

			return result;
		}

		/// <summary>
		/// Function to unbind all devices.
		/// </summary>
		public void UnbindAllDevices()
		{
			// Unbind all the devices from their respective controls.
			foreach (KeyValuePair<Guid, GorgonInputDevice> device in Devices)
			{
				device.Value.Enabled = false;
				device.Value.Unbind();

				switch (device.Value.DeviceType)
				{
					case InputDeviceType.Keyboard:
						device.Value.BeforeUnbind -= Keyboard_BeforeUnbind;
						break;
					case InputDeviceType.Mouse:
						device.Value.BeforeUnbind -= Mouse_BeforeUnbind;
						break;
					case InputDeviceType.Joystick:
						device.Value.BeforeUnbind -= Joystick_BeforeUnbind;
						break;
					case InputDeviceType.HumanInterfaceDevice:
						device.Value.BeforeUnbind -= HumanInterfaceDevice_BeforeUnbind;
						break;
				}
			}

			ExclusiveDevices = InputDeviceExclusivity.None;
			Devices.Clear();
		}

		/// <summary>
		/// Function to enumerate the pointing devices such as mice, trackballs, etc... attached to the system.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonMouseInfo"/> items.</returns>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<IGorgonMouseInfo> OnEnumerateMice();

		/// <summary>
		/// Function to perform the enumeration of keyboard devices attached to the system.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonKeyboardInfo"/> items.</returns>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<IGorgonKeyboardInfo> OnEnumerateKeyboards();

		/// <summary>
		/// Function to enumerate the available keyboards attached to the system.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonKeyboardInfo"/> items.</returns>
		/// <exception cref="GorgonException">Thrown when an attempt to enumerate keyboard devices is made, but there are already keyboard devices active.</exception>
		public IGorgonNamedObjectReadOnlyDictionary<IGorgonKeyboardInfo> EnumerateKeyboards()
		{
			Log.Print("Enumerating keyboard devices.", LoggingLevel.Simple);

			if (Devices.Any(item => item.Value is GorgonKeyboard))
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORINP_ERR_CANNOT_ENUM_DEVICES_PRESENT);
			}

			return OnEnumerateKeyboards();
		}

		/// <summary>
		/// Function to enumerate the available pointing devices (mice, track pads, etc...) attached to the system.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonMouseInfo"/> items.</returns>
		/// <exception cref="GorgonException">Thrown when an attempt to enumerate pointing devices is made, but there are already pointing devices active.</exception>
		public IGorgonNamedObjectReadOnlyDictionary<IGorgonMouseInfo> EnumerateMice()
		{
			Log.Print("Enumerating pointing devices.", LoggingLevel.Simple);

			if (Devices.Any(item => item.Value is GorgonPointingDevice))
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORINP_ERR_CANNOT_ENUM_DEVICES_PRESENT);
			}

			return OnEnumerateMice();
		}

		/// <summary>
		/// Function to enumerate the available gaming devices (joysticks, game pads, etc...) registered to the system.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonJoystickInfo"/> items.</returns>
		/// <exception cref="GorgonException">Thrown when an attempt to enumerate gaming devices is made, but there are already gaming devices active.</exception>
		public IGorgonNamedObjectReadOnlyDictionary<IGorgonJoystickInfo> EnumerateJoysticks()
		{
			Log.Print("Enumerating pointing devices.", LoggingLevel.Simple);

			if (Devices.Any(item => item.Value is GorgonJoystick))
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORINP_ERR_CANNOT_ENUM_DEVICES_PRESENT);
			}

			return OnEnumerateJoysticks();
		}

		/// <summary>
		/// Function to enumerate Human Interface Devices. 
		/// </summary>		
		/// <returns>A list of <see cref="IGorgonHumanInterfaceDeviceInfo"/> items.</returns> 
		/// <exception cref="GorgonException">Thrown when an attempt to enumerate pointing devices is made, but there are already pointing devices active.</exception>
		/// <remarks>
		/// <para>
		/// Human Interface Devices are unknown to Gorgon. And, as such, Gorgon cannot parse the data for the devices in a meaningful way.  Therefore, it will be up to the input service plugin implementor to 
		/// make use of the data provided by the <see cref="IGorgonHumanInterfaceDevice"/> objects created by the input service.
		/// </para>
		/// </remarks>
		public IGorgonNamedObjectReadOnlyDictionary<IGorgonHumanInterfaceDeviceInfo> EnumerateHumanInterfaceDevices()
		{
			Log.Print("Enumerating human interface devices.", LoggingLevel.Simple);

			if (Devices.Any(item => item.Value is GorgonCustomHID))
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORINP_ERR_CANNOT_ENUM_DEVICES_PRESENT);
			}

			return OnEnumerateHumanInterfaceDevices();
		}

		/// <summary>
		/// Function to enumerate the gaming devices such as joysticks, game pads, etc... registered with the system.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonJoystickInfo"/> items.</returns>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<IGorgonJoystickInfo> OnEnumerateJoysticks();

        /// <summary>
		/// Function to enumerate Human Interface Devices. 
		/// </summary>		
		/// <returns>A list of <see cref="IGorgonHumanInterfaceDeviceInfo"/> items.</returns> 
		/// <remarks>
		/// <para>
		/// Human Interface Devices are unknown to Gorgon. And, as such, Gorgon cannot parse the data for the devices in a meaningful way.  Therefore, it will be up to the input service plugin implementor to 
		/// make use of the data provided by the <see cref="IGorgonHumanInterfaceDevice"/> objects created by the input service.
		/// </para>
		/// </remarks>
		protected abstract IGorgonNamedObjectReadOnlyDictionary<IGorgonHumanInterfaceDeviceInfo> OnEnumerateHumanInterfaceDevices();

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">The application window to bind with.</param>
		/// <param name="keyboardInfo">Information about the keyboard to bind.</param>
		/// <returns>A new <see cref="IGorgonKeyboard"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This method is for implementing functionality that will return a new <see cref="IGorgonKeyboard"/> interface. 
		/// </para>
		/// <para>
		/// When implementing this method, developers must handle the case where the <paramref name="keyboardInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net). This usually means that a system 
		/// keyboard (i.e. on Windows, all input from all keyboards) should be used instead of a specific keyboard. In cases where there is no system keyboard, the application should return the first 
		/// keyboard that it knows of.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Do <b>not</b> throw an exception if the <paramref name="keyboardInfo"/> parameter is <b>null</b>. This will anything depending on an interface to be returned from this method.
		/// </para>
		/// </note>
		/// </para> 
		/// </remarks>
		protected abstract GorgonKeyboard OnCreateKeyboard(Control window, IGorgonKeyboardInfo keyboardInfo);

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">The application window to bind with.</param>
		/// <param name="mouseInfo">Information about the pointing device to bind.</param>
		/// <returns>A new <see cref="IGorgonMouse"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This method is for implementing functionality that will return a new <see cref="IGorgonMouse"/> interface. 
		/// </para>
		/// <para>
		/// When implementing this method, developers must handle the case where the <paramref name="mouseInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net). This usually means that a system 
		/// pointing device (i.e. on Windows, all input from all pointing devices) should be used instead of a specific pointing device. In cases where there is no system keyboard, the application should 
		/// return the first pointing device that it knows of.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Do <b>not</b> throw an exception if the <paramref name="mouseInfo"/> parameter is <b>null</b>. This will anything depending on an interface to be returned from this method.
		/// </para>
		/// </note>
		/// </para> 
		/// </remarks>
		protected abstract GorgonPointingDevice OnCreateMouse(Control window, IGorgonMouseInfo mouseInfo);

		/// <summary>
		/// Function to create a gaming device interface.
		/// </summary>
		/// <param name="window">The application window to bind with.</param>
		/// <param name="joystickInfo">Information about the gaming device to bind.</param>
		/// <returns>A new <see cref="IGorgonJoystick"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This method is for implementing functionality that will return a new <see cref="IGorgonJoystick"/> interface. 
		/// </para>
		/// </remarks>
		protected abstract GorgonJoystick OnCreateJoystick(Control window, IGorgonJoystickInfo joystickInfo);

		/// <summary>
		/// Function to create a human interface device interface.
		/// </summary>
		/// <param name="window">The application to bind with.</param>
		/// <param name="deviceInfo">A <see cref="IGorgonHumanInterfaceDeviceInfo"/> object containing the device information.</param>
		/// <returns>A new <see cref="IGorgonHumanInterfaceDevice"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This method is for implementing functionality that will return a new <see cref="IGorgonHumanInterfaceDevice"/> interface.
		/// </para>
		/// <para>
		/// Human Interface Devices are not recognized by Gorgon. That is, Gorgon cannot parse their data in any meaningful way. Therefore, it is up to the implementor of a <see cref="IGorgonHumanInterfaceDevice"/> to 
		/// take the data and parse it into a meaningful context.
		/// </para>
		/// </remarks>
		protected abstract GorgonCustomHID OnCreateHumanInterfaceDevice(Control window, IGorgonHumanInterfaceDeviceInfo deviceInfo);

		/// <summary>
		/// Function to create a Human Interface Device interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="deviceInfo">The <see cref="IGorgonHumanInterfaceDeviceInfo"/> to provide information when creating the device.</param>
		/// <returns>A new <see cref="IGorgonHumanInterfaceDevice"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// Data from a Human Interface Device will be returned via the <see cref="GorgonCustomHID.Data"/>. Implementors of the <see cref="IGorgonHumanInterfaceDevice"/> object will be required to parse the 
		/// data into a meaningful representation.
		/// </para>
		/// </remarks>		
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="window"/>, or the <paramref name="deviceInfo"/> parameters are <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonCustomHID CreateHumanInterfaceDevice(Control window, IGorgonHumanInterfaceDeviceInfo deviceInfo)
		{
			if (deviceInfo == null)
			{
			    throw new ArgumentNullException("deviceInfo");
			}

			var customHID = GetInputDevice<GorgonCustomHID>(deviceInfo);

			if (customHID == null)
			{
				customHID = OnCreateHumanInterfaceDevice(window, deviceInfo);
				customHID.DeviceType = InputDeviceType.HumanInterfaceDevice;
				customHID.BeforeUnbind += HumanInterfaceDevice_BeforeUnbind;
				Devices.TryAdd(Guid.NewGuid(), customHID);
			}

			customHID.ClearData();
			customHID.Bind(window);
			customHID.Enabled = true;
			customHID.Exclusive = (ExclusiveDevices & InputDeviceExclusivity.HumanInterfaceDevice) == InputDeviceExclusivity.HumanInterfaceDevice;

			return customHID;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="window">The application window to bind with.</param>
		/// <param name="keyboardInfo">[Optional] Information about a specific keyboard to bind.</param>
		/// <returns>A new <see cref="IGorgonKeyboard"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This will return a new <see cref="IGorgonKeyboard"/> interface to allow processing of key presses either through polling or events. 
		/// </para>
		/// <para>
		/// When passing the <paramref name="keyboardInfo"/> for a keyboard interface that was already created, that existing keyboard interface will be returned instead of a new one.
		/// </para>
		/// <para>
		/// If <b>null</b> (<i>Nothing</i> in VB.Net) is passed to the <paramref name="keyboardInfo"/> parameter, then the system keyboard (i.e. on Windows, all input from all keyboards) will be used. 
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="window"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonKeyboard CreateKeyboard(Control window, IGorgonKeyboardInfo keyboardInfo = null)
		{
			if (window == null)
			{
				throw new ArgumentNullException("window");
			}

			var keyboardDevice = GetInputDevice<GorgonKeyboard>(keyboardInfo);

			if (keyboardDevice == null)
			{
				keyboardDevice = OnCreateKeyboard(window, keyboardInfo);
				keyboardDevice.DeviceType = InputDeviceType.Keyboard;
				keyboardDevice.BeforeUnbind += Keyboard_BeforeUnbind;
				Devices.TryAdd(keyboardDevice.Info.UUID, keyboardDevice);
			}

			keyboardDevice.GetDefaultKeyMapping();
			keyboardDevice.KeyStateResetMode = KeyStateResetMode.ResetAll;
			keyboardDevice.KeyStates.Reset();
			keyboardDevice.Bind(window);
			keyboardDevice.Enabled = true;
			keyboardDevice.Exclusive = (ExclusiveDevices & InputDeviceExclusivity.Keyboard) == InputDeviceExclusivity.Keyboard;

			return keyboardDevice;
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="window">The application window to bind with.</param>
		/// <param name="pointingDeviceInfo">[Optional] Information about a specific pointing device to bind.</param>
		/// <returns>A new <see cref="IGorgonMouse"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This will return a new <see cref="IGorgonMouse"/> interface to allow processing of pointing device actions either through polling or events. A pointing device can be a mouse, track pad, etc... But 
		/// for simplicity and easy of typing, the Gorgon Input API refers to these devices as mice.
		/// </para>
		/// <para>
		/// When passing the <paramref name="pointingDeviceInfo"/> for a pointing device interface that was already created, that existing pointing device interface will be returned instead of a new one.
		/// </para>
		/// <para>
		/// If <b>null</b> (<i>Nothing</i> in VB.Net) is passed to the <paramref name="pointingDeviceInfo"/> parameter, then the system pointing device (i.e. on Windows, all input from all pointing devices) will 
		/// be used. 
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="window"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonPointingDevice CreateMouse(Control window, IGorgonMouseInfo pointingDeviceInfo = null)
		{
		    if (window == null)
			{
                throw new ArgumentNullException("window");
			}

			// If this is the first pointing device that we've created, then reset the mouse cursor so that it's visible.
			if (!Devices.Any(item => item.Value is GorgonPointingDevice))
			{
				GorgonPointingDevice.ResetCursor();
			}

			var pointingDevice = GetInputDevice<GorgonPointingDevice>(pointingDeviceInfo);

			if (pointingDevice == null)
			{
				pointingDevice = OnCreateMouse(window, pointingDeviceInfo);
				pointingDevice.DeviceType = InputDeviceType.Mouse;
				pointingDevice.BeforeUnbind += Mouse_BeforeUnbind;
				Devices.TryAdd(pointingDevice.Info.UUID, pointingDevice);
			}

			// Default to the last position for the mouse cursor.
			pointingDevice.Position = window.PointToClient(Cursor.Position);
			pointingDevice.PositionRange = RectangleF.Empty;
			pointingDevice.WheelRange = Point.Empty;
			pointingDevice.ResetButtons();
			pointingDevice.Bind(window);
			pointingDevice.Enabled = true;
			pointingDevice.Exclusive = (ExclusiveDevices & InputDeviceExclusivity.Mouse) == InputDeviceExclusivity.Mouse;

			if (!pointingDevice.Exclusive)
			{
				pointingDevice.ShowCursor();
			}

			return pointingDevice;
		}

		/// <summary>
		/// Function to create a gaming device interface.
		/// </summary>
		/// <param name="window">The application window to bind with.</param>
		/// <param name="joystickDeviceInfo">Information about a specific gaming device to bind.</param>
		/// <returns>A new <see cref="IGorgonJoystick"/> interface.</returns>
		/// <remarks>
		/// <para>
		/// This will return a new <see cref="IGorgonJoystick"/> interface to allow processing of gaming device actions through polling. A gaming device can be a joystick, game pad, etc... But 
		/// for simplicity and easy of typing, the Gorgon Input API refers to these devices as joysticks.
		/// </para>
		/// <para>
		/// When passing the <paramref name="joystickDeviceInfo"/> for a gaming device interface that was already created, that existing gaming device interface will be returned instead of a new one.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="window"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="joystickDeviceInfo"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</para>
		/// </exception>
		public GorgonJoystick CreateJoystick(Control window, IGorgonJoystickInfo joystickDeviceInfo)
		{
			if (window == null)
			{
				throw new ArgumentNullException("window");
			}
            if (joystickDeviceInfo == null)
            {
                throw new ArgumentNullException("joystickDeviceInfo");
            }

			var joystickDevice = GetInputDevice<GorgonJoystick>(joystickDeviceInfo);

			if (joystickDevice == null)
			{
				joystickDevice = OnCreateJoystick(window, joystickDeviceInfo);
				joystickDevice.DeviceType = InputDeviceType.Joystick;
				joystickDevice.BeforeUnbind += Joystick_BeforeUnbind;
				Devices.TryAdd(joystickDeviceInfo.UUID, joystickDevice);
			}

			joystickDevice.Reset();
			joystickDevice.Bind(window);
			joystickDevice.Enabled = true;
			joystickDevice.Exclusive = (ExclusiveDevices & InputDeviceExclusivity.Joystick) == InputDeviceExclusivity.Joystick;

			return joystickDevice;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputService"/> class.
		/// </summary>
		/// <param name="name">The name of the device manager.</param> 
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonInputService(string name)
			: base(name)
		{
			ExclusiveDevices = InputDeviceExclusivity.None;
			Devices = new ConcurrentDictionary<Guid, GorgonInputDevice>();
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
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// Destroy any outstanding device instances.
				UnbindAllDevices();
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
