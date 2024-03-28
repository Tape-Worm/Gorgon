
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, September 07, 2015 2:15:24 PM
// 

using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Gorgon.Native;
using Microsoft.Win32;

namespace Gorgon.Input;

/// <summary>
/// Retrieves device information from the windows registry for a given device name
/// </summary>
internal static class RawInputDeviceRegistryInfo
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

        regValue[0] = regValue[0][4..];

        // Don't add RDP devices.
        if ((log is not null) &&
            (regValue.Length > 0) &&
            (regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
        {
            log.PrintWarning("This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
            return string.Empty;
        }

        using RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{regValue[0]}\{regValue[1]}\{regValue[2]}",
                                                                        false);
        if (deviceKey?.GetValue("DeviceDesc") is null)
        {
            return string.Empty;
        }

        regValue = deviceKey.GetValue("DeviceDesc").ToString().Split(';');

        return regValue[^1];
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

        regValue[0] = regValue[0][4..];

        // Don't add RDP devices.
        if ((log is not null) &&
            (regValue.Length > 0) &&
            (regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
        {
            log.PrintWarning("This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
            return string.Empty;
        }

        using RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{regValue[0]}\{regValue[1]}\{regValue[2]}",
                                                                        false);
        if (deviceKey?.GetValue("DeviceDesc") is null)
        {
            return string.Empty;
        }

        if (deviceKey.GetValue("Class") is not null)
        {
            return deviceKey.GetValue("Class").ToString();
        }

        // Windows 8 no longer has a "Class" value in this area, so we need to go elsewhere to get it.
        if (deviceKey.GetValue("ClassGUID") is null)
        {
            return string.Empty;
        }

        string classGUID = deviceKey.GetValue("ClassGUID").ToString();

        if (string.IsNullOrWhiteSpace(classGUID))
        {
            return string.Empty;
        }

        using RegistryKey classKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Control\Class\{classGUID}");
        return classKey?.GetValue("Class") is null ? string.Empty : classKey.GetValue("Class").ToString();
    }
}
