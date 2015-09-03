#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, July 22, 2015 1:09:22 AM
// 
#endregion

using System;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;
using Microsoft.Win32;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Retrieves information from the registry about a raw input devices.
	/// </summary>
	static class RawInputDeviceRegistryInfo
	{
		/// <summary>
		/// Function to retrieve the description of the raw input device from the registry.
		/// </summary>
		/// <param name="deviceName">Path to the registry key that holds the device description.</param>
		/// <param name="log">The debug log file to use when logging issues.</param>
		/// <returns>The device description.</returns>
		public static string GetDeviceDescription(string deviceName, IGorgonLog log)
		{
			if (string.IsNullOrWhiteSpace(deviceName))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, nameof(deviceName));
			}

			string[] regValue = deviceName.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((log != null) &&
				(regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
				return string.Empty;
			}

			using (RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{regValue[0]}\{regValue[1]}\{regValue[2]}",
																			false))
			{
				if (deviceKey?.GetValue("DeviceDesc") == null)
				{
					return string.Empty;
				}

				regValue = deviceKey.GetValue("DeviceDesc").ToString().Split(';');

				return regValue[regValue.Length - 1];
			}
		}

		/// <summary>
		/// Function to return the class name for the device.
		/// </summary>
		/// <param name="deviceName">The name of the device from <see cref="RawInputApi.GetDeviceName"/>.</param>
		/// <param name="log">The debug log file to use when logging issues.</param>
		/// <returns>The device class name.</returns>
		public static string GetDeviceClass(string deviceName, IGorgonLog log)
		{
			if (string.IsNullOrWhiteSpace(deviceName))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, nameof(deviceName));
			}

			string[] regValue = deviceName.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((log != null) &&
				(regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
				return string.Empty;
			}

			using (RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{regValue[0]}\{regValue[1]}\{regValue[2]}",
																			false))
			{
				if (deviceKey?.GetValue("DeviceDesc") == null)
				{
					return string.Empty;
				}

				if (deviceKey.GetValue("Class") != null)
				{
					return deviceKey.GetValue("Class").ToString();
				}

				// Windows 8 no longer has a "Class" value in this area, so we need to go elsewhere to get it.
				if (deviceKey.GetValue("ClassGUID") == null)
				{
					return string.Empty;
				}

				string classGUID = deviceKey.GetValue("ClassGUID").ToString();

				if (string.IsNullOrWhiteSpace(classGUID))
				{
					return string.Empty;
				}

				using (RegistryKey classKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Control\Class\{classGUID}"))
				{
					return classKey?.GetValue("Class") == null ? string.Empty : classKey.GetValue("Class").ToString();
				}
			}
		}

		/// <summary>
		/// Function to get the real name for a joystick.
		/// </summary>
		/// <param name="rawInputDeviceName">The raw input device name for the joystick.</param>
		/// <returns>The real name for the joystick.</returns>
		public static string GetJoystickName(string rawInputDeviceName)
		{
			if (string.IsNullOrWhiteSpace(rawInputDeviceName))
			{
				return string.Empty;
			}

			// Take the HID path, and split it out until we get the appropriate sub key name.
			string[] parts = rawInputDeviceName.Split(new[]
			                                          {
				                                          '#'
			                                          },
			                                          StringSplitOptions.RemoveEmptyEntries);

			if ((parts.Length < 2) || (string.IsNullOrWhiteSpace(parts[1])))
			{
				return null;
			}

			string subKeyName = parts[1];

			// The XBOX 360 controller has an &IG_ at the end of its PID, strip it off.
			while (subKeyName.Count(item => item == '&') > 1)
			{
				int lastAmp = subKeyName.LastIndexOf("&", StringComparison.OrdinalIgnoreCase);

				if (lastAmp == -1)
				{
					return null;
				}

				subKeyName = subKeyName.Substring(0, lastAmp);
			}

			if (string.IsNullOrWhiteSpace(subKeyName))
			{
				return string.Empty;
			}

			// Find the key and open it.
			// The original example code this is based on uses HKEY_LOCAL_MACHINE instead of CURRENT_USER.  This may be a difference 
			// between operating systems.
			const string regKeyPath = @"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\";

            using (RegistryKey joystickOemKey = Registry.CurrentUser.OpenSubKey(regKeyPath, false))
			{
				string joystickDeviceKeyName = joystickOemKey?.GetSubKeyNames().First(item => parts[1].StartsWith(item, StringComparison.OrdinalIgnoreCase));

				if (string.IsNullOrWhiteSpace(joystickDeviceKeyName))
				{
					return string.Empty;
				}

				using (RegistryKey joystickVidKey = joystickOemKey.OpenSubKey(subKeyName, false))
				{
					object value = joystickVidKey?.GetValue("OEMName");

					return value?.ToString() ?? string.Empty;
				}
			}
		}
	}
}
