#region MIT
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Sunday, September 13, 2015 12:02:50 PM
// 
#endregion

using System;
using System.Linq;
using Gorgon.Native;
using Microsoft.Win32;

namespace Gorgon.Input
{
    /// <summary>
    /// Functionality to return information about a gaming device from the registry.
    /// </summary>
    internal static class GamingDeviceRegistryInfo
    {
        /// <summary>
        /// Function to get the human readable name for a gaming device from a <see cref="GorgonRawHIDInfo"/> object.
        /// </summary>
        /// <param name="hidDeviceInfo">The human interface device information to evaluate.</param>
        /// <returns>A string containing the human readable name for the gaming device, or <b>null</b> if the device name could not be determined.</returns>
        /// <remarks>
        /// <para>
        /// This will retrieve the friendly name for a gaming device from a <see cref="GorgonRawHIDInfo"/> object. If the <see cref="GorgonRawHIDInfo.Usage"/> is not a <see cref="HIDUsage.Gamepad"/> or 
        /// <see cref="HIDUsage.Joystick"/>, or it does not have a <see cref="GorgonRawHIDInfo.HIDPath"/>, this method will return <b>null</b>.
        /// </para>
        /// <para>
        /// This is meant for raw input gaming human interface devices. As such, if this is used on an XInput controller, then the device will not be given a name and <b>null</b> will be returned. If 
        /// use of an XInput controller is required, then use the Gorgon XInput driver to provide access to those devices.
        /// </para>
        /// </remarks>
        public static string GetGamingDeviceName(GorgonRawHIDInfo hidDeviceInfo)
        {
            if (string.IsNullOrWhiteSpace(hidDeviceInfo?.HIDPath)
                || (hidDeviceInfo.UsagePage != HIDUsagePage.Generic)
                || ((hidDeviceInfo.Usage != HIDUsage.Gamepad)
                    && (hidDeviceInfo.Usage != HIDUsage.Joystick)))
            {
                return null;
            }


            // Take the HID path, and split it out until we get the appropriate sub key name.
            string[] parts = hidDeviceInfo.HIDPath.Split(new[]
                                                         {
                                                             '#'
                                                         },
                                                         StringSplitOptions.RemoveEmptyEntries);

            if ((parts.Length < 2) || (string.IsNullOrWhiteSpace(parts[1])))
            {
                return null;
            }

            string subKeyName = parts[1];

            // The XBOX 360 controller has an &IG_ at the end of its PID, then ignore it.
            // Use the XInput driver.  All other game devices through HID will be handled by us, but not the XInput devices.
            while (subKeyName.Count(item => item == '&') > 1)
            {
                int lastAmp = subKeyName.LastIndexOf("&", StringComparison.OrdinalIgnoreCase);

                if (lastAmp == -1)
                {
                    return null;
                }

                subKeyName = subKeyName[lastAmp..];

                if (subKeyName.IndexOf("&IG_", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(subKeyName))
            {
                return string.Empty;
            }

            // Find the key and open it.
            // The original example code this is based on uses HKEY_LOCAL_MACHINE instead of CURRENT_USER.  This may be a difference 
            // between operating systems.
            const string regKeyPath = @"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\";

            using RegistryKey joystickOemKey = Registry.CurrentUser.OpenSubKey(regKeyPath, false);
            string joystickDeviceKeyName = joystickOemKey?.GetSubKeyNames().First(item => parts[1].StartsWith(item, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(joystickDeviceKeyName))
            {
                return string.Empty;
            }

            using RegistryKey joystickVidKey = joystickOemKey.OpenSubKey(subKeyName, false);
            object value = joystickVidKey?.GetValue("OEMName");

            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Function to get the human readable name for a gaming device from a registry key and WMM joystick ID.
        /// </summary>
        /// <param name="registryKey">The registry key to use.</param>
        /// <param name="joystickID">The WMM ID of the joystick to retrieve data for.</param>
        /// <returns>The name of the gaming device.</returns>
        /// <remarks>
        /// This is a helper method that will get the device name from the registry when using Windows Multimedia Joysticks (joyPosEx).
        /// </remarks>
        public static string GetGamingDeviceName(string registryKey, int joystickID)
        {
            RegistryKey rootKey = null;         // Root key.
            RegistryKey lookup = null;          // Look up key.
            RegistryKey nameKey = null;         // Name key.

            try
            {
                // Default name.
                rootKey = Registry.CurrentUser;

                // Get the device ID.				
                lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + registryKey + @"\CurrentJoystickSettings");

                // Try the local machine key as a root if that lookup failed.
                if (lookup is null)
                {
                    rootKey.Close();
                    rootKey = Registry.LocalMachine;
                    lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + registryKey + @"\CurrentJoystickSettings");
                }

                if (lookup is not null)
                {
                    // Key name.
                    string key = lookup.GetValue("Joystick" + (joystickID + 1) + "OEMName", string.Empty).ToString();

                    // If we have no name, then build one.
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        return null;
                    }

                    // Get the name.
                    nameKey =
                        rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" +
                                           key);

                    return nameKey?.GetValue("OEMName", null)?.ToString();
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                nameKey?.Close();
                lookup?.Close();
                rootKey?.Close();
            }
        }
    }
}
