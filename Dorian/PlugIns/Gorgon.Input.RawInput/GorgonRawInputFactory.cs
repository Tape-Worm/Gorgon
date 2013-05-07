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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Input.Raw.Properties;
using GorgonLibrary.Native;
using Microsoft.Win32;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	internal class GorgonRawInputFactory
		: GorgonInputFactory
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
			RegistryKey deviceKey = null;
			RegistryKey classKey = null;

		    if (Win32API.GetRawInputDeviceInfo(deviceHandle, (int)RawInputDeviceInfo.DeviceName, IntPtr.Zero, ref dataSize) < 0)
		    {
		        throw new Win32Exception();
		    }
		    
		    IntPtr data = Marshal.AllocHGlobal(dataSize * 2);

		    try
		    {
		        if (Win32API.GetRawInputDeviceInfo(deviceHandle, (int)RawInputCommand.DeviceName, data, ref dataSize) < 0)
		        {
		            throw new Win32Exception();
		        }

                if (data == IntPtr.Zero)
		        {
		            throw new Win32Exception();
		        }

		        string regPath = Marshal.PtrToStringAnsi(data);

		        if (regPath == null)
		        {
		            throw new Win32Exception();
		        }

		        string[] regValue = regPath.Split('#');
		        string className = string.Empty;

		        regValue[0] = regValue[0].Substring(4);

		        // Don't add RDP devices.
		        if ((regValue.Length > 0) &&
		            (regValue[1].StartsWith("RDP_", StringComparison.CurrentCultureIgnoreCase)))
		        {
		            return;
		        }

		        deviceKey =
		            Registry.LocalMachine.OpenSubKey(
		                string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", regValue[0], regValue[1],
		                                regValue[2]),
		                false);

		        if (deviceKey == null)
		        {
		            return;
		        }

		        int counter = 0;

		        regValue = deviceKey.GetValue("DeviceDesc").ToString().Split(';');

		        if (deviceKey.GetValue("Class") == null)
		        {
		            // Windows 8 no longer has a "Class" value in this area, so we need to go elsewhere to get it.
		            if (deviceKey.GetValue("ClassGUID") != null)
		            {
		                var classGUID = deviceKey.GetValue("ClassGUID").ToString();

		                if (!string.IsNullOrWhiteSpace(classGUID))
		                {
		                    classKey =
		                        Registry.LocalMachine.OpenSubKey(
		                            string.Format(@"System\CurrentControlSet\Control\Class\{0}", classGUID));

		                    if (classKey != null)
		                    {
		                        if (classKey.GetValue("Class") != null)
		                        {
		                            className = classKey.GetValue("Class").ToString();
		                        }
		                    }
		                }
		            }
		        }
		        else
		        {
		            className = deviceKey.GetValue("Class").ToString();
		        }

		        string baseName;
		        string name = baseName = regValue[regValue.Length - 1];

		        while (deviceList.Find(item => String.Compare(item.Name, name, StringComparison.OrdinalIgnoreCase) == 0) != null)
		        {
		            counter++;
		            name = baseName + " #" + counter;
		        }

		        deviceList.Add(new GorgonRawInputDeviceInfo(name, deviceType, className, regPath, deviceHandle));
		    }
		    finally
		    {
		        if (deviceKey != null)
		        {
		            deviceKey.Dispose();
		        }

		        if (classKey != null)
		        {
		            classKey.Dispose();
		        }

		        if (data != IntPtr.Zero)
		        {
		            Marshal.FreeHGlobal(data);
		        }
		    }
		}

		/// <summary>
		/// Function to retrieve a joystick name from the registry.
		/// </summary>
		/// <param name="joystickData">Joystick capability data.</param>
		/// <param name="joystickID">ID of the joystick to retrieve data for.</param>
		/// <returns>The name of the joystick.</returns>
		private string GetJoystickName(JOYCAPS joystickData, int joystickID)
		{
			RegistryKey rootKey = null;			// Root registry key.
			RegistryKey lookup = null;			// Look up key.
			RegistryKey nameKey = null;			// Name key.

		    try
			{
				string defaultName = joystickData.AxisCount + "-axis, " + joystickData.ButtonCount + "-button joystick.";	// Default name.
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
                    // Key name.
			        string key = lookup.GetValue("Joystick" + (joystickID + 1) + "OEMName", string.Empty).ToString();
			            
			        // If we have no name, then build one.
			        if (string.IsNullOrWhiteSpace(key))
			        {
			            return defaultName;
			        }

			        // Get the name.
			        nameKey =
			            rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" +
			                               key);

			        return nameKey != null ? nameKey.GetValue("OEMName", defaultName).ToString() : defaultName;
			    }
			    else
			    {
			        return defaultName;
			    }
			}
			finally
			{
			    if (nameKey != null)
			    {
			        nameKey.Close();
			    }
			    if (lookup != null)
			    {
			        lookup.Close();
			    }
			    if (rootKey != null)
			    {
			        rootKey.Close();
			    }
			}
		}

		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumeratePointingDevices()
		{
		    IEnumerable<RAWINPUTDEVICELIST> devices = Win32API.EnumerateInputDevices().Where(item => item.DeviceType == RawInputType.Mouse);
			var result = new List<GorgonRawInputDeviceInfo>();

		    foreach (var pointingDevice in devices)
		    {
		        GetRawInputDeviceInfo(pointingDevice.Device, InputDeviceType.PointingDevice, result);
		    }

		    return result;
		}

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumerateKeyboardDevices()
		{
		    IEnumerable<RAWINPUTDEVICELIST> devices = Win32API.EnumerateInputDevices().Where(item => item.DeviceType == RawInputType.Keyboard);
			var result = new List<GorgonRawInputDeviceInfo>();

		    foreach (var keyboardDevice in devices)
		    {
		        GetRawInputDeviceInfo(keyboardDevice.Device, InputDeviceType.Keyboard, result);
		    }

		    return result;
		}

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected override IEnumerable<GorgonInputDeviceInfo> EnumerateJoysticksDevices()
		{
		    var capabilities = new JOYCAPS();	    // Joystick capabilities.
		    int nameCount = 0;						// Name counter.

			var result = new List<GorgonMultimediaDeviceInfo>();
			int deviceCount = Win32API.joyGetNumDevs();

			Gorgon.Log.Print("Enumerating joysticks...", LoggingLevel.Intermediate);

			for (int i = 0; i < deviceCount; i++)
			{
			    int error = Win32API.joyGetDevCaps(i, ref capabilities, Marshal.SizeOf(typeof(JOYCAPS)));							// Error code.

			    // If the joystick has no registry key, then skip it.
			    if ((string.IsNullOrEmpty(capabilities.RegistryKey)) || (string.IsNullOrEmpty(capabilities.Name)))
			    {
			        continue;
			    }

			    // Check for error, stop enumeration.
			    if (error > 0)
			    {
			        throw new GorgonException(GorgonResult.DriverError,
			                                  string.Format(Resources.GORINP_RAW_CANNOT_GET_JOYSTICK_CAPS, i, error.FormatHex()));
			    }

			    // Get the name.
			    string name = GetJoystickName(capabilities, i);				// Name of the joystick.

			    if (string.IsNullOrWhiteSpace(name))
			    {
			        continue;
			    }

			    string keyName = name;

			    while (result.Find(item => String.Compare(item.Name, keyName, StringComparison.OrdinalIgnoreCase) == 0) != null)
			    {
			        nameCount++;
			        keyName = name + " " + nameCount;
			    }

			    result.Add(new GorgonMultimediaDeviceInfo(keyName, "Game device", "N/A", i));
			}

		    Gorgon.Log.Print("{0} joysticks found.", LoggingLevel.Intermediate, result.Count);

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
		    IEnumerable<RAWINPUTDEVICELIST> devices =
		        Win32API.EnumerateInputDevices()
		                .Where(item => item.DeviceType != RawInputType.Keyboard && item.DeviceType != RawInputType.Mouse);
			var result = new List<GorgonRawInputDeviceInfo>();

		    foreach (var hidDevice in devices)
		    {
		        GetRawInputDeviceInfo(hidDevice.Device, InputDeviceType.HID, result);
		    }

		    return result;
		}

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="hidInfo">A <see cref="GorgonLibrary.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the HID information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>
		/// A new custom HID interface.
		/// </returns>
		protected override GorgonCustomHID CreateCustomHIDImpl(System.Windows.Forms.Control window, GorgonInputDeviceInfo hidInfo)
		{
            var rawInfo = hidInfo as GorgonRawInputDeviceInfo;

		    if (rawInfo == null)
		    {
                throw new InvalidCastException(Resources.GORINP_RAW_HIDINFO_NOT_RAW);
		    }

		    var hidDevice = new RawHIDDevice(this, rawInfo, window)
		        {
		            Enabled = true
		        };

		    return hidDevice;
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardInfo">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardInfo"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para></remarks>
		protected override GorgonKeyboard CreateKeyboardImpl(System.Windows.Forms.Control window, GorgonInputDeviceInfo keyboardInfo)
		{
		    RawKeyboard keyboard = keyboardInfo == null
		                               ? new RawKeyboard(this, "System Keyboard", IntPtr.Zero, window)
		                               : new RawKeyboard(this, keyboardInfo.Name, ((GorgonRawInputDeviceInfo)keyboardInfo).Handle, window);

			keyboard.Enabled = true;

			return keyboard;
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceInfo">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceInfo"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para>
		/// </remarks>
		protected override GorgonPointingDevice CreatePointingDeviceImpl(System.Windows.Forms.Control window, GorgonInputDeviceInfo pointingDeviceInfo)
		{
		    RawPointingDevice mouse = pointingDeviceInfo == null
		                                  ? new RawPointingDevice(this, "System Mouse", IntPtr.Zero, window)
		                                  : new RawPointingDevice(this, pointingDeviceInfo.Name,
		                                                          ((GorgonRawInputDeviceInfo)pointingDeviceInfo).Handle,
		                                                          window);
			mouse.Enabled = true;

			return mouse;
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickInfo">A <see cref="GorgonLibrary.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickInfo"/> is NULL.</exception>
		protected override GorgonJoystick CreateJoystickImpl(System.Windows.Forms.Control window, GorgonInputDeviceInfo joystickInfo)
		{
		    var mmJoystick = joystickInfo as GorgonMultimediaDeviceInfo;

		    if (mmJoystick == null)
		    {
                throw new InvalidCastException(Resources.GORINP_RAW_NOT_MM_JOYSTICK);
		    }

		    var joystick = new MultimediaJoystick(this, mmJoystick.JoystickID, joystickInfo.Name, window)
		        {
		            Enabled = true
		        };

		    return joystick;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawInputFactory"/> class.
		/// </summary>
		public GorgonRawInputFactory()
			: base("Gorgon Raw Input")
		{
		}
		#endregion
	}
}
