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
// Created: Friday, June 24, 2011 10:05:11 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
using GorgonLibrary.Input;
using Forms = System.Windows.Forms;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	internal class GorgonRawInputDeviceFactory
		: GorgonInputDeviceFactory
	{
		#region Methods.
		/// <summary>
		/// Function to retrieve the device name.
		/// </summary>
		/// <param name="deviceHandle">Handle to the device.</param>
		/// <param name="deviceType">Type of device.</param>
		/// <param name="deviceList">List of devices.</param>
		/// <returns>A device name structure.</returns>
		private void GetRawInputDeviceInfo(IntPtr deviceHandle, InputDeviceType deviceType, List<GorgonRawInputDeviceInfo> deviceList)
		{
			int dataSize = 0;			

			if (Win32API.GetRawInputDeviceInfo(deviceHandle, (int)RawInputDeviceInfo.DeviceName, IntPtr.Zero, ref dataSize) >= 0)
			{
				IntPtr data = Marshal.AllocHGlobal(dataSize * 2);

				try
				{					
					if (Win32API.GetRawInputDeviceInfo(deviceHandle, (int)RawInputCommand.DeviceName, data, ref dataSize) >= 0)
					{
						string regPath = Marshal.PtrToStringAnsi(data);
						string[] regValue = regPath.Split('#');
						RegistryKey deviceKey = null;
						string baseName = string.Empty;
						string className = string.Empty;

						regValue[0] = regValue[0].Substring(4);
						deviceKey = Registry.LocalMachine.OpenSubKey(string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", regValue[0], regValue[1], regValue[2]), false);
						
						if (deviceKey != null)
						{
							int counter = 0;
							string name = string.Empty;

							regValue = deviceKey.GetValue("DeviceDesc").ToString().Split(';');
							className = deviceKey.GetValue("Class").ToString();
							name = baseName = regValue[regValue.Length - 1];

							while (deviceList.Find((item) => string.Compare(item.Name, name, true) == 0) != null)
							{
								counter++;
								name = baseName + " #" + counter.ToString();
							}

							deviceList.Add(new GorgonRawInputDeviceInfo(name, deviceType,className, regPath, deviceHandle));
						}
					}
					else
						throw new System.ComponentModel.Win32Exception();
				}
				finally
				{
					if (data != IntPtr.Zero)
						Marshal.FreeHGlobal(data);
				}
			}
			else
				throw new System.ComponentModel.Win32Exception();
		}

		/// <summary>
		/// Function to retrieve a joystick name from the registry.
		/// </summary>
		/// <param name="joystickData">Joystick capability data.</param>
		/// <param name="ID">ID of the joystick to retrieve data for.</param>
		/// <returns>The name of the joystick.</returns>
		private string GetJoystickName(JOYCAPS joystickData, int ID)
		{
			RegistryKey rootKey = null;			// Root registry key.
			RegistryKey lookup = null;			// Look up key.
			RegistryKey nameKey = null;			// Name key.
			string key = string.Empty;			// Key name.
			string defaultName = string.Empty;	// Default name.

			try
			{
				defaultName = joystickData.AxisCount.ToString() + "-axis, " + joystickData.ButtonCount.ToString() + "-button joystick.";
				rootKey = Registry.CurrentUser;

				// Get the device ID.				
				lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + joystickData.RegistryKey + @"\CurrentJoystickSettings");

				// Try the local machine key as a root if that lookup failed.
				if (lookup == null)
				{
					rootKey.Close();
					rootKey = Registry.LocalMachine;
					lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + joystickData.RegistryKey + @"\CurrentJoystickSettings");
				}

				if (lookup != null)
				{
					key = lookup.GetValue("Joystick" + (ID + 1) + "OEMName", string.Empty).ToString();

					// If we have no name, then build one.
					if (key == string.Empty)
						return defaultName;

					// Get the name.
					nameKey = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + key);
					return nameKey.GetValue("OEMName", defaultName).ToString();
				}
				else
					return defaultName;
			}
			finally
			{
				if (nameKey != null)
					nameKey.Close();
				if (lookup != null)
					lookup.Close();
				if (rootKey != null)
					rootKey.Close();
			}
		}

		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumeratePointingDevices()
		{
			List<GorgonRawInputDeviceInfo> result = null;
			IEnumerable<RAWINPUTDEVICELIST> devices = null;

			devices = Win32API.EnumerateInputDevices().Where((item) => item.DeviceType == RawInputType.Mouse);
			result = new List<GorgonRawInputDeviceInfo>();
			
			foreach (var pointingDevice in devices)
				this.GetRawInputDeviceInfo(pointingDevice.Device, InputDeviceType.PointingDevice, result);

			return result;
		}

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumerateKeyboardDevices()
		{
			List<GorgonRawInputDeviceInfo> result = null;
			IEnumerable<RAWINPUTDEVICELIST> devices = null;

			devices = Win32API.EnumerateInputDevices().Where((item) => item.DeviceType == RawInputType.Keyboard);
			result = new List<GorgonRawInputDeviceInfo>();

			foreach (var keyboardDevice in devices)
				this.GetRawInputDeviceInfo(keyboardDevice.Device, InputDeviceType.Keyboard, result);

			return result;
		}

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumerateJoysticksDevices()
		{
			List<GorgonWMMDeviceName> result = null;
			JOYCAPS capabilities = new JOYCAPS();	// Joystick capabilities.
			string name = string.Empty;				// Name of the joystick.
			int error = 0;							// Error code.
			int deviceCount = 0;					// Number of devices.
			int nameCount = 0;						// Name counter.

			result = new List<GorgonWMMDeviceName>();
			deviceCount = Win32API.joyGetNumDevs();

			Gorgon.Log.Print("Enumerating joysticks...", GorgonLoggingLevel.Intermediate);
			for (int i = 0; i < deviceCount; i++)
			{
				error = Win32API.joyGetDevCaps(i, ref capabilities, Marshal.SizeOf(typeof(JOYCAPS)));

				// If the joystick has no registry key, then skip it.
				if ((!string.IsNullOrEmpty(capabilities.RegistryKey)) && (!string.IsNullOrEmpty(capabilities.Name)))
				{
					// Check for error, stop enumeration.
					if (error > 0)
						throw new GorgonException(GorgonResult.DriverError, "Cannot get joystick information from index " + i.ToString() + "\nError code: 0x" + error.ToString("x"));

					// Get the name.
					name = GetJoystickName(capabilities, i);

					if (!string.IsNullOrEmpty(name))
					{
						string keyName = name;

						while (result.Find((item) => string.Compare(item.Name, keyName, true) == 0) != null)
						{
							nameCount++;
							keyName = name + " " + nameCount.ToString();
						}

						result.Add(new GorgonWMMDeviceName(keyName, "Game device", "N/A", i));
					}
				}
			}
			Gorgon.Log.Print("{0} joysticks found.", GorgonLoggingLevel.Intermediate, result.Count);

			return result;
		}

		/// <summary>
		/// Function to enumerate device types for which there is no class wrapper and will return data in a custom property collection.
		/// </summary>
		/// <returns>
		/// A list of custom HID types.
		/// </returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumerateCustomHIDs()
		{
			List<GorgonRawInputDeviceInfo> result = null;
			IEnumerable<RAWINPUTDEVICELIST> devices = null;

			devices = Win32API.EnumerateInputDevices().Where((item) => item.DeviceType != RawInputType.Keyboard && item.DeviceType != RawInputType.Mouse);
			result = new List<GorgonRawInputDeviceInfo>();

			foreach (var hidDevice in devices)
				this.GetRawInputDeviceInfo(hidDevice.Device, InputDeviceType.HID, result);

			return result;
		}

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="hidName">A <see cref="GorgonLibrary.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the HID information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>
		/// A new custom HID interface.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="hidName"/> is NULL.</exception>
		protected override GorgonCustomHID CreateCustomHIDImpl(GorgonInputDeviceInfo hidName, Forms.Control window)
		{
			RawHIDDevice hidDevice = null;

			if (hidName == null)
				throw new ArgumentNullException("hidName");

			hidDevice = new RawHIDDevice(this, ((GorgonRawInputDeviceInfo)hidName), window);
			hidDevice.Enabled = true;

			return hidDevice;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para></remarks>
		protected override GorgonKeyboard CreateKeyboardImpl(GorgonInputDeviceInfo keyboardName, Forms.Control window)
		{
			RawKeyboard keyboard = null;

			if (keyboardName == null)
				keyboard = new RawKeyboard(this, IntPtr.Zero, window);
			else
				keyboard = new RawKeyboard(this, ((GorgonRawInputDeviceInfo)keyboardName).Handle, window);
			keyboard.Enabled = true;

			return keyboard;
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceName">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceName"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para>
		/// </remarks>
		protected override GorgonPointingDevice CreatePointingDeviceImpl(GorgonInputDeviceInfo pointingDeviceName, Forms.Control window)
		{
			RawPointingDevice mouse = null;

			if (pointingDeviceName == null)
				mouse = new RawPointingDevice(this, IntPtr.Zero, window);
			else
				mouse = new RawPointingDevice(this, ((GorgonRawInputDeviceInfo)pointingDeviceName).Handle, window);
			mouse.Enabled = true;

			return mouse;
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">A <see cref="GorgonLibrary.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickName"/> is NULL.</exception>
		protected override GorgonJoystick CreateJoystickImpl(GorgonInputDeviceInfo joystickName, Forms.Control window)
		{
			WMMJoystick joystick = null;

			if (joystickName == null)
				throw new ArgumentNullException("joystickName");

			joystick = new WMMJoystick(this, ((GorgonWMMDeviceName)joystickName).JoystickID, joystickName.Name, window);
			joystick.Enabled = true;

			return joystick;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawInputDeviceFactory"/> class.
		/// </summary>
		public GorgonRawInputDeviceFactory()
			: base("Gorgon.RawInput")
		{
		}
		#endregion
	}
}
