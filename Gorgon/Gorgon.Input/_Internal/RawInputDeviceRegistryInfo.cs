
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
            log.PrintError("There is no device name. Cannot retrieve the HID description with a device name.", LoggingLevel.Verbose);
            return string.Empty;
        }

        string[] regValue = deviceName.Split('#');

        if (regValue.Length < 3)
        {
            log.PrintError($"Could not retrieve description location from the device name '{deviceName}'.", LoggingLevel.Verbose);
            return string.Empty;
        }

        // Our entries should be under the HID key.
        if (!regValue[0].StartsWith(@"\\?\HID", StringComparison.OrdinalIgnoreCase))
        {
            log.PrintError($"Device name does not contain a HID registry root key: '{regValue[0]}'.", LoggingLevel.Verbose);
            return string.Empty;
        }

        string keyPath = $@"System\CurrentControlSet\Enum\HID\{regValue[1]}\{regValue[2]}";
        using RegistryKey? deviceKey = Registry.LocalMachine.OpenSubKey(keyPath, false);

        if (deviceKey is null)
        {
            log.PrintError($"Unable to find the registry key for the device {deviceName}. Path: {keyPath}", LoggingLevel.Verbose);
            return string.Empty;
        }

        string deviceDesc = deviceKey?.GetValue("DeviceDesc")?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(deviceDesc))
        {
            return string.Empty;
        }

        regValue = deviceDesc.Split(';');

        if (regValue.Length == 0)
        {
            return string.Empty;
        }

        return regValue[^1];
    }

    /// <summary>
    /// Function to return the class name for the device.
    /// </summary>
    /// <param name="deviceName">The name of the device from Raw Input device information.</param>
    /// <param name="log">The debug log file to use when logging issues.</param>
    /// <returns>The device class name.</returns>
    public static string GetDeviceClass(string deviceName, IGorgonLog log)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            log.PrintError("There is no device name. Cannot retrieve the HID class information with a device name.", LoggingLevel.Verbose);
            return string.Empty;
        }

        string[] regValue = deviceName.Split('#');

        if (regValue.Length < 3)
        {
            log.PrintError($"Could not retrieve class information from the device name '{deviceName}'.", LoggingLevel.Verbose);
            return string.Empty;
        }

        // Our entries should be under the HID key.
        if (!regValue[0].StartsWith(@"\\?\HID", StringComparison.OrdinalIgnoreCase))
        {
            log.PrintError($"Device name does not contain a HID registry root key: '{regValue[0]}'.", LoggingLevel.Verbose);
            return string.Empty;
        }

        // Strip off the first 4 characters.
        string keyPath = $@"System\CurrentControlSet\Enum\HID\{regValue[1]}\{regValue[2]}";
        using RegistryKey? deviceKey = Registry.LocalMachine.OpenSubKey(keyPath, false);

        if (deviceKey is null)
        {
            log.PrintError($"Unable to find the registry key for the device {deviceName}. Path: {keyPath}", LoggingLevel.Verbose);
            return string.Empty;
        }

        string classGUID = deviceKey.GetValue("ClassGUID")?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(classGUID))
        {
            log.PrintError($"Unable to find the registry string value for the device {deviceName}. Path: {keyPath}\\ClassGUID", LoggingLevel.Verbose);
            return string.Empty;
        }

        keyPath = $@"System\CurrentControlSet\Control\Class\{classGUID}";
        using RegistryKey? classKey = Registry.LocalMachine.OpenSubKey(keyPath, false);

        if (classKey is null)
        {
            log.PrintError($"Unable to find the device class registry key for the device {deviceName}. Path: {keyPath}", LoggingLevel.Verbose);
            return string.Empty;
        }

        return classKey.GetValue("Class")?.ToString() ?? string.Empty;
    }
}
