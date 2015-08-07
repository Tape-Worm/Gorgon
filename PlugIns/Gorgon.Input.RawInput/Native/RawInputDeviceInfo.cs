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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;
using Microsoft.Win32;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Retrieves information about a raw input device.
	/// </summary>
	class RawInputDeviceInfo
	{
		#region Variables.
		// The logging interface to use for debugging.
		private readonly IGorgonLog _log;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve information about a specific device.
		/// </summary>
		/// <param name="device">The device to retrieve information about.</param>
		/// <returns>The device information for the device.</returns>
		public RID_DEVICE_INFO GetDeviceInfo(ref RAWINPUTDEVICELIST device)
		{
			int dataSize = 0;
			int errCode = Win32API.GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceInfo, IntPtr.Zero, ref dataSize);

			if ((errCode != -1) && (errCode != 0))
			{
				int win32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
			}

			if (errCode == -1)
			{
				throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, dataSize));
			}

			unsafe
			{
				byte* data = stackalloc byte[dataSize];
				errCode = Win32API.GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceInfo, (IntPtr)data, ref dataSize);

				if (errCode < -1)
				{
					int win32Error = Marshal.GetLastWin32Error();
					throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
				}

				if (errCode == -1)
				{
					throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, dataSize));
				}

				RID_DEVICE_INFO result = *((RID_DEVICE_INFO*)data);

				return result;
			}
		}

		/// <summary>
		/// Function to retrieve the description of the raw input device from the registry.
		/// </summary>
		/// <param name="deviceName">Path to the registry key that holds the device description.</param>
		/// <returns>The device description.</returns>
		public string GetDeviceDescription(string deviceName)
		{
			if (string.IsNullOrWhiteSpace(deviceName))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, nameof(deviceName));
			}

			string[] regValue = deviceName.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				_log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
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
		/// <param name="deviceName">The name of the device from <see cref="GetDeviceName"/>.</param>
		/// <returns>The device class name.</returns>
		public string GetDeviceClass(string deviceName)
		{
			if (string.IsNullOrWhiteSpace(deviceName))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, nameof(deviceName));
			}

			string[] regValue = deviceName.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				_log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
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
		/// Function to retrieve the name for the specified device.
		/// </summary>
		/// <param name="device">Device to retrieve the name for.</param>
		/// <returns>A string containing the device name.</returns>
		public string GetDeviceName(ref RAWINPUTDEVICELIST device)
		{
			int dataSize = 0;

			if (Win32API.GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceName, IntPtr.Zero, ref dataSize) < 0)
			{
				int win32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
			}

			// Do nothing if we have no data.
			if (dataSize < 4)
			{
				return string.Empty;
			}

			unsafe
			{
				char* data = stackalloc char[dataSize];

				if (Win32API.GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceName, (IntPtr)data, ref dataSize) < 0)
				{
					int win32Error = Marshal.GetLastWin32Error();
					throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
				}

				// The strings that come back from native land will end with a NULL terminator, so crop that off.
				return new string(data, 0, dataSize - 1);
			}
		}
		#endregion

		#region Constructor/Finalizer
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputDeviceInfo" /> class.
		/// </summary>
		/// <param name="log">The log to use for debug logging.</param>
		public RawInputDeviceInfo(IGorgonLog log)
		{
			_log = log;
		}
		#endregion
	}
}
