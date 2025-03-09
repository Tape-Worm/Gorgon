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
// Created: February 2, 2025 4:40:17 PM
//

using System.ComponentModel;
using System.Runtime.InteropServices;
using Gorgon.Windows.Input.Properties;
using Windows.Win32;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Foundation;

namespace Gorgon.Windows.Input.Native;

/// <summary>
/// Functions to interact with the Human Interface Device (HID) API.
/// </summary>
internal static class HidApi
{
    /// <summary>
    /// Function to return the capabilities of the HID.
    /// </summary>
    /// <param name="preparsedData">The buffer containing the HID preparsed data.</param>
    /// <param name="caps">The data structure that will be populated with device information.</param>
    /// <exception cref="Win32Exception">Throw if the preparsed data is invalid.</exception>
    public static unsafe void GetCaps(ReadOnlySpan<byte> preparsedData, out HIDP_CAPS caps)
    {
        if (preparsedData.IsEmpty)
        {
            caps = default;
            return;
        }

        fixed (byte* ptr = preparsedData)
        {
            PHIDP_PREPARSED_DATA dataPtr = new((nint)ptr);
            NTSTATUS status = PInvoke.HidP_GetCaps(dataPtr, out caps);

            if (status.SeverityCode != NTSTATUS.Severity.Success)
            {
                throw new Win32Exception(status.Value, Resources.GORINP_RAW_ERR_HID_PREPARSED_INVALID);
            }
        }
    }

    /// <summary>
    /// Function to retrieve the capabilities for the values on the device.
    /// </summary>
    /// <param name="preparsedData">The preparsed HID data.</param>
    /// <param name="valueCaps">The buffer that will receive the values.</param>
    /// <exception cref="Win32Exception">Thrown if the preparsed data is invalid.</exception>
    public static unsafe void GetDeviceValueCaps(ReadOnlySpan<byte> preparsedData, Span<HIDP_VALUE_CAPS> valueCaps)
    {
        if ((preparsedData.IsEmpty) || (valueCaps.IsEmpty))
        {
            return;
        }

        fixed (byte* ptr = preparsedData)
        fixed (HIDP_VALUE_CAPS* valueCapsPtr = valueCaps)
        {
            PHIDP_PREPARSED_DATA dataPtr = new((nint)ptr);
            HIDP_VALUE_CAPS* buttonCapsData = stackalloc HIDP_VALUE_CAPS[valueCaps.Length];
            ushort buttonLength = (ushort)valueCaps.Length;

            NTSTATUS status = PInvoke.HidP_GetValueCaps(HIDP_REPORT_TYPE.HidP_Input, valueCapsPtr, ref buttonLength, dataPtr);

            if (status.SeverityCode != NTSTATUS.Severity.Success)
            {
                throw new Win32Exception(status.Value, Resources.GORINP_RAW_ERR_HID_PREPARSED_INVALID);
            }
        }
    }

    /// <summary>
    /// Function to retrieve the capabilities for the buttons on the device.
    /// </summary>
    /// <param name="preparsedData">The preparsed HID data.</param>
    /// <param name="buttonCaps">The buffer that will receive the button capabilties.</param>
    /// <exception cref="Win32Exception">Throw if the preparsed data is invalid.</exception>
    public static unsafe void GetDeviceButtonCaps(ReadOnlySpan<byte> preparsedData, Span<HIDP_BUTTON_CAPS> buttonCaps)
    {
        if ((preparsedData.IsEmpty) || (buttonCaps.IsEmpty))
        {
            return;
        }

        fixed (byte* ptr = preparsedData)
        fixed (HIDP_BUTTON_CAPS* buttonCapsPtr = buttonCaps)
        {
            PHIDP_PREPARSED_DATA dataPtr = new((nint)ptr);
            HIDP_BUTTON_CAPS* buttonCapsData = stackalloc HIDP_BUTTON_CAPS[buttonCaps.Length];
            ushort buttonLength = (ushort)buttonCaps.Length;

            NTSTATUS status = PInvoke.HidP_GetButtonCaps(HIDP_REPORT_TYPE.HidP_Input, buttonCapsPtr, ref buttonLength, dataPtr);

            if (status.SeverityCode != NTSTATUS.Severity.Success)
            {
                throw new Win32Exception(status.Value, Resources.GORINP_RAW_ERR_HID_PREPARSED_INVALID);
            }
        }
    }

    /// <summary>
    /// Function to get the product name for an HID device.
    /// </summary>
    /// <param name="fileHandle">The file handle to the HID device.</param>
    /// <returns>The plain text, friendly product name for the device.</returns>
    /// <exception cref="Win32Exception">Thrown if the name could not be retrieved.</exception>
    public static unsafe string GetProductString(HANDLE fileHandle)
    {
        if ((fileHandle.IsNull) || (fileHandle == HANDLE.INVALID_HANDLE_VALUE))
        {
            // 6 - is invalid handle.
            throw new Win32Exception(6, Resources.GORINP_ERR_INVALID_WINDOW_HANDLE);
        }

        Span<byte> buffer = stackalloc byte[4093];

        unsafe
        {
            fixed (byte* bufferPtr = buffer)
            {
                if (!PInvoke.HidD_GetProductString(fileHandle, bufferPtr, (uint)buffer.Length))
                {
                    WIN32_ERROR err = (WIN32_ERROR)Marshal.GetLastWin32Error();

                    // If we can't retrieve the product string, but we receive a success code, then return an empty string so 
                    // we can try other methods.
                    if (err == WIN32_ERROR.ERROR_SUCCESS)
                    {
                        return string.Empty;
                    }

                    throw new Win32Exception((int)err, Resources.GORINP_WIN32_CANNOT_RETRIEVE_PRODUCT_DATA);
                }

                return Marshal.PtrToStringUni((nint)bufferPtr) ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Function to get the manufacturer name for an HID device.
    /// </summary>
    /// <param name="fileHandle">The file handle to the HID device.</param>
    /// <returns>The plain text, friendly manufacturer name for the device.</returns>
    /// <exception cref="Win32Exception">Thrown if the name could not be retrieved.</exception>
    public static unsafe string GetManufacturerString(HANDLE fileHandle)
    {
        if ((fileHandle.IsNull) || (fileHandle == HANDLE.INVALID_HANDLE_VALUE))
        {
            // 6 - is invalid handle.
            throw new Win32Exception(6);
        }

        Span<byte> buffer = stackalloc byte[4093];

        unsafe
        {
            fixed (byte* bufferPtr = buffer)
            {
                if (!PInvoke.HidD_GetManufacturerString(fileHandle, bufferPtr, (uint)buffer.Length))
                {
                    WIN32_ERROR lastError = (WIN32_ERROR)Marshal.GetLastWin32Error();

                    // The call was succesful, but the HID call failed, so just return an empty string.
                    // This happens with XBOX controllers.
                    if (lastError == WIN32_ERROR.ERROR_SUCCESS)
                    {
                        return string.Empty;
                    }

                    throw new Win32Exception((int)lastError, Resources.GORINP_WIN32_CANNOT_RETRIEVE_MANUFACTURER_DATA);
                }

                return Marshal.PtrToStringUni((nint)bufferPtr) ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Function to retrieve a reading for the device.
    /// </summary>
    /// <param name="deviceHandle">The handle for the device.</param>
    /// <param name="preparsedData">The preparsed data for the device.</param>
    /// <param name="report">HID Report data.</param>
    /// <param name="hidBuffer">The buffer to populate with HID report items.</param>
    /// <returns>The WIN32 error code to indicate success or failure.</returns>    
    public static unsafe WIN32_ERROR ReadDeviceData(nint deviceHandle, ReadOnlySpan<byte> preparsedData, ReadOnlySpan<byte> report, ref Span<HIDP_DATA> hidBuffer)
    {
        uint dbSize = (uint)hidBuffer.Length;

        fixed (byte* ptr = preparsedData, dataPtr = report)
        fixed (HIDP_DATA* hidPtr = hidBuffer)
        {
            NTSTATUS r = PInvoke.HidP_GetData(HIDP_REPORT_TYPE.HidP_Input, hidPtr, ref dbSize, new PHIDP_PREPARSED_DATA((nint)ptr), new PSTR(dataPtr), (uint)report.Length);

            if (r.SeverityCode != NTSTATUS.Severity.Success)
            {
                return (WIN32_ERROR)Marshal.GetLastWin32Error();
            }

            hidBuffer = hidBuffer[..(int)dbSize];

            return WIN32_ERROR.ERROR_SUCCESS;
        }
    }

    /// <summary>
    /// Function to retrieve the size of the required buffer for a device report list.
    /// </summary>
    /// <param name="preparsedBuffer">The buffer holding the preparsed data.</param>
    /// <returns>The number of HIDP_DATA structures for the device. Or 0 if the size was not able to be retrieved.</returns>
    public static unsafe int GetReportItemLength(ReadOnlySpan<byte> preparsedBuffer)
    {
        if (preparsedBuffer.Length == 0)
        {
            return 0;
        }

        fixed (byte* ptr = preparsedBuffer)
        {
            return (int)PInvoke.HidP_MaxDataListLength(HIDP_REPORT_TYPE.HidP_Input, new PHIDP_PREPARSED_DATA((nint)ptr));
        }
    }
}
