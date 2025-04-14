// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 13, 2025 10:38:09 PM
//

using System.ComponentModel;
using System.Runtime.InteropServices;
using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Windows.Win32.Devices.DeviceAndDriverInstallation;
using Windows.Win32.Devices.Properties;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

namespace Windows.Win32;

/// <summary>
/// Win32 native code.
/// </summary>
internal static partial class ConfigManager
{
    // The request code to access the LED on the device.
    private const uint Ioctl_XUSB_Get_LED_State = 0x8000E008;

    // The request to access the LED on the device (if present)
    private static readonly byte[] _ledRequest = [0x01, 0x01, 0x00];

    /// <summary>
    /// Function to retrieve the property value for a device interface.
    /// </summary>
    /// <param name="deviceInterface">The device interface path.</param>
    /// <param name="log">The logging. interface.</param>
    /// <returns>The value for the property.</returns>
    private static unsafe string GetDeviceInstanceID(string deviceInterface, IGorgonLog log)
    {
        DEVPROPKEY propKey = PInvoke.DEVPKEY_Device_InstanceId;
        uint bufferSize = 0;

        CONFIGRET result = PInvoke.CM_Get_Device_Interface_Property(deviceInterface, in propKey, out _, null, ref bufferSize, 0);

        if (result is not CONFIGRET.CR_SUCCESS and not CONFIGRET.CR_BUFFER_SMALL)
        {
            log.PrintError($"There was an error retrieving the buffer size for the device instance ID: {result}", LoggingLevel.Verbose);
            return string.Empty;
        }

        byte* bufferPtr = stackalloc byte[(int)bufferSize];
        result = PInvoke.CM_Get_Device_Interface_Property(deviceInterface, in propKey, out _, bufferPtr, ref bufferSize, 0);

        if (result != CONFIGRET.CR_SUCCESS)
        {
            log.PrintError($"There was an error retrieving the device instance ID: {result}", LoggingLevel.Verbose);
            return string.Empty;
        }

        return Marshal.PtrToStringUni((nint)bufferPtr) ?? string.Empty;
    }

    /// <summary>
    /// Function to retrieve the parent node for the device instance ID.
    /// </summary>
    /// <param name="deviceInstanceID">The device instance ID to evaluate.</param>
    /// <param name="log">The logging interface.</param>
    /// <returns>The parent path.</returns>
    private unsafe static string GetDeviceParentNode(string deviceInstanceID, IGorgonLog log)
    {
        fixed (char* deviceInstPtr = deviceInstanceID)
        {
            CONFIGRET result = PInvoke.CM_Locate_DevNode(out uint handle, new PWSTR(deviceInstPtr), 0);

            if (result != CONFIGRET.CR_SUCCESS)
            {
                log.PrintError($"There was an error retrieving the device node: {result}", LoggingLevel.Verbose);
                return string.Empty;
            }

            DEVPROPKEY propKey = PInvoke.DEVPKEY_Device_Parent;
            uint bufferSize = 0;

            result = PInvoke.CM_Get_DevNode_Property(handle, in propKey, out _, null, ref bufferSize, 0);

            if (result is not CONFIGRET.CR_SUCCESS and not CONFIGRET.CR_BUFFER_SMALL)
            {
                log.PrintError($"There was an error retrieving the device node property buffer size: {result}", LoggingLevel.Verbose);
                return string.Empty;
            }

            byte* bufferPtr = stackalloc byte[(int)bufferSize];
            result = PInvoke.CM_Get_DevNode_Property(handle, in propKey, out _, bufferPtr, ref bufferSize, 0);

            if (result != CONFIGRET.CR_SUCCESS)
            {
                log.PrintError($"There was an error retrieving the device node property buffer: {result}", LoggingLevel.Verbose);
                return string.Empty;
            }

            return Marshal.PtrToStringUni((nint)bufferPtr) ?? string.Empty;
        }
    }

    /// <summary>
    /// Function to retrieve the device interface name for a device instance ID.
    /// </summary>
    /// <param name="guid">The device class GUID.</param>
    /// <param name="deviceInstanceID">The device instance ID.</param>
    /// <param name="log">The logging interface.</param>
    /// <returns>The device interface name.</returns>
    private static unsafe string GetDeviceInterfaceName(Guid guid, string deviceInstanceID, IGorgonLog log)
    {
        fixed (char* devInstPtr = deviceInstanceID)
        {
            CONFIGRET result = PInvoke.CM_Get_Device_Interface_List_Size(out uint size, in guid, new PWSTR(devInstPtr), CM_GET_DEVICE_INTERFACE_LIST_FLAGS.CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

            if (result is not CONFIGRET.CR_SUCCESS)
            {
                log.PrintError($"There was an error retrieving the device interface list size: {result}", LoggingLevel.Verbose);
                return string.Empty;
            }

            // If the size is 1 byte, then we can't make a valid string from it, so move on.
            if (size < sizeof(char))
            {
                return string.Empty;
            }

            char* charBuffer = stackalloc char[(int)size];

            result = PInvoke.CM_Get_Device_Interface_List(in guid, new PWSTR(devInstPtr), new PZZWSTR(charBuffer), size, CM_GET_DEVICE_INTERFACE_LIST_FLAGS.CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

            if (result != CONFIGRET.CR_SUCCESS)
            {
                log.PrintError($"There was an error retrieving the device interface list: {result}", LoggingLevel.Verbose);
                return string.Empty;
            }

            // This should return the first string in the list (which is the one we want).
            return Marshal.PtrToStringUni((nint)charBuffer) ?? string.Empty;
        }
    }

    /// <summary>
    /// Function to return the device interface path for a specific device name.
    /// </summary>
    /// <param name="deviceName">The device name to locate.</param>
    /// <param name="log">The logging interface.</param>
    /// <returns>The device interface path or empty if none could be located.</returns>
    private unsafe static string GetDeviceInterface(string deviceName, IGorgonLog log)
    {
        Guid classGuid = new("EC87F1E3-C13B-4100-B5F7-8B84D54260CB");

        string deviceInstanceID = GetDeviceInstanceID(deviceName, log);

        if (string.IsNullOrWhiteSpace(deviceInstanceID))
        {
            return string.Empty;
        }

        string devicePath = GetDeviceInterfaceName(classGuid, deviceInstanceID, log);

        if (!string.IsNullOrWhiteSpace(devicePath))
        {
            return devicePath;
        }

        fixed (char* deviceNamePtr = deviceInstanceID)
        {
            string parentNode = GetDeviceParentNode(deviceInstanceID, log);

            while (!string.IsNullOrWhiteSpace(parentNode))
            {
                devicePath = GetDeviceInterfaceName(classGuid, parentNode, log);

                if (!string.IsNullOrWhiteSpace(devicePath))
                {
                    return devicePath;
                }

                parentNode = GetDeviceParentNode(parentNode, log);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Function to retrieve the state of the LED on the device.
    /// </summary>
    /// <param name="deviceName">The name of the device from Raw Input.</param>
    /// <param name="log">The log for the application debug messages.</param>
    /// <returns>The LED state for the device as a bit field, or 0xff if LED state cannot be determined or the device is disconnected.</returns>
    /// <exception cref="Win32Exception">Thrown if the data cannot be read.</exception>
    /// <remarks>
    /// <para>
    /// This code was adapted from https://github.com/DJm00n/RawInputDemo/blob/master/RawInputLib/RawInputDeviceHid.cpp#L275-L339
    /// </para>
    /// </remarks>
    public static unsafe byte GetLEDState(string deviceName, IGorgonLog log)
    {
        DEVPROPKEY propKey = PInvoke.DEVPKEY_Device_InstanceId;
        string devicePath = GetDeviceInterface(deviceName, log);

        if (string.IsNullOrWhiteSpace(devicePath))
        {
            log.PrintError($"Cannot find the device interface for the device {deviceName}.", LoggingLevel.Verbose);
            return 0xff;
        }

        HANDLE deviceHandle = HANDLE.Null;

        try
        {
            deviceHandle = PInvoke.CreateFile(devicePath, GENERIC_ACCESS_RIGHTS.GENERIC_READ | GENERIC_ACCESS_RIGHTS.GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE, FILE_CREATION_DISPOSITION.OPEN_EXISTING, 0);

            if (deviceHandle == HANDLE.INVALID_HANDLE_VALUE)
            {
                log.PrintError($"The device {deviceName} failed to open with a valid handle.", LoggingLevel.Verbose);
                return 0xff;
            }

            byte* ledPtr = stackalloc byte[3];
            fixed (byte* requestPtr = _ledRequest)
            {
                uint returned = 0;

                if (!PInvoke.DeviceIoControl(deviceHandle, Ioctl_XUSB_Get_LED_State, requestPtr, 3, ledPtr, 3, &returned, null))
                {
                    WIN32_ERROR error = (WIN32_ERROR)Marshal.GetLastWin32Error();

                    if (error == WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
                    {
                        log.PrintError($"Device {deviceName} is no longer connected to the system.", LoggingLevel.Verbose);
                        return 0xff;
                    }

                    throw new Win32Exception((int)error, Resources.GORINP_WIN32_CANNOT_GET_IO_DATA);
                }

                return ledPtr[2] switch
                {
                    0 => 0x7f,
                    2 or 6 => 0,
                    3 or 7 => 1,
                    4 or 8 => 2,
                    5 or 9 => 3,
                    _ => 0xff
                };
            }
        }
        catch (Win32Exception wEx)
        {
            WIN32_ERROR error = (WIN32_ERROR)wEx.NativeErrorCode;

            if (error == WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
            {
                log.PrintError($"Device {deviceName} is no longer connected to the system.", LoggingLevel.Verbose);
                return 0xff;
            }

            throw;
        }
        finally
        {
            if ((deviceHandle != HANDLE.Null) && (deviceHandle != HANDLE.INVALID_HANDLE_VALUE))
            {
                PInvoke.CloseHandle(deviceHandle);
            }
        }
    }
}
