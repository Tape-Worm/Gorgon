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
// Created: Sunday, September 13, 2015 12:02:50 PM
// 

using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Input.Devices;
using Gorgon.Input.Native;
using Gorgon.Input.Properties;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Input;

namespace Gorgon.Input;

/// <summary>
/// Functionality to return information about a gaming device.
/// </summary>
/// <param name="DeviceID">The unique identifier for the device.</param>
/// <param name="Handle">The handle to the device.</param>
/// <param name="DeviceSubType">The type of gaming device.</param>
/// <param name="DeviceName">The HID name of the device.</param>
/// <param name="DeviceClass">The class of the device.</param>
/// <param name="VendorID">The vendor ID for the device.</param>
/// <param name="ProductID">The product ID for the device.</param>
/// <param name="Version">The version number for the device.</param>
/// <param name="AxisInfo">The information about the axes on the device.</param>
/// <param name="VibrationMotorRanges">The ranges for the vibration motors on the device.</param>
/// <param name="Capabilities">The capabilities of the device.</param>
/// <param name="Description">The description of the device.</param>
internal record class GamingDeviceInfo(Guid DeviceID,
                                       nint Handle,
                                       GamingDeviceType DeviceSubType,
                                       string DeviceName,
                                       string DeviceClass,
                                       int VendorID,
                                       int ProductID,
                                       int Version,
                                       IReadOnlyDictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> AxisInfo,
                                       IReadOnlyList<GorgonRange<int>> VibrationMotorRanges,
                                       GamingDeviceCapabilityFlags Capabilities,
                                       string Description)
    : IGorgonGamingDeviceInfo
{
    // A list of known product names for vendor/product IDs.
    private static readonly Dictionary<(uint, uint), string> _productNames = new()
    {
        {
            (0x045E, 0x028E), Resources.GORINP_PRODUCT_XBOX_360
        },
        {
            (0x045E, 0x028F), Resources.GORINP_PRODUCT_XBOX_360
        },
        {
            (0x045E, 0x0291), Resources.GORINP_PRODUCT_XBOX_360_WIRELESS
        },
        {
            (0x045E, 0x02a1), Resources.GORINP_PRODUCT_XBOX_360_WIRELESS
        },
        {
            (0x045E, 0x02a9), Resources.GORINP_PRODUCT_XBOX_360_WIRELESS
        },
        {
            (0x045E, 0x0719), Resources.GORINP_PRODUCT_XBOX_360_WIRELESS
        },
        {
            (0x45E, 0x02d1), Resources.GORINP_PRODUCT_XBOX_ONE
        },
        {
            (0x45E, 0x02dd), Resources.GORINP_PRODUCT_XBOX_ONE
        },
        {
            (0x45E, 0x02ff), Resources.GORINP_PRODUCT_XBOX_ONE
        },
        {
            (0x45E, 0x02e0), Resources.GORINP_PRODUCT_XBOX_ONE_S
        },
        {
            (0x45E, 0x02ea), Resources.GORINP_PRODUCT_XBOX_ONE_S
        },
        {
            (0x45E, 0x02fd), Resources.GORINP_PRODUCT_XBOX_ONE_S
        },
        {
            (0x45E, 0x0b20), Resources.GORINP_PRODUCT_XBOX_ONE_S
        },
        {
            (0x45E, 0x02e3), Resources.GORINP_PRODUCT_XBOX_ONE_ELITE
        },
        {
            (0x45E, 0x0b00), Resources.GORINP_PRODUCT_XBOX_ONE_ELITE_2
        },
        {
            (0x45E, 0x0b05), Resources.GORINP_PRODUCT_XBOX_ONE_ELITE_2
        },
        {
            (0x45E, 0x0b22), Resources.GORINP_PRODUCT_XBOX_ONE_ELITE_2
        },
        {
            (0x45E, 0x0b12), Resources.GORINP_PRODUCT_XBOX_SERIES_X
        },
        {
            (0x45E, 0x0b13), Resources.GORINP_PRODUCT_XBOX_SERIES_X
        }
    };

    /// <summary>
    /// A default value for the record.
    /// </summary>
    public static readonly GamingDeviceInfo Empty = new(Guid.Empty,
                                                                     IntPtr.Zero,
                                                                     GamingDeviceType.Unknown,
                                                                     string.Empty,
                                                                     string.Empty,
                                                                     -1,
                                                                     -1,
                                                                     -1,
                                                                     new Dictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo>(),
                                                                     [],
                                                                     GamingDeviceCapabilityFlags.None,
                                                                     string.Empty)
    {
        ButtonMap = [],
        PovMap = [],
        PreparsedData = [],
        HidDataLength = 0
    };

    /// <summary>
    /// Property to return a list of index mappings for the buttons on the device.
    /// </summary>
    internal required ushort[] ButtonMap
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return a list of index mappings for the point of view (POV) hats on the device.
    /// </summary>
    internal required PovMap[] PovMap
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the preparsed data for the device.
    /// </summary>
    internal required byte[] PreparsedData
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the number of items in the report.
    /// </summary>
    internal required int HidDataLength
    {
        get;
        init;
    }

    /// <inheritdoc/>
    public int XInputSlot
    {
        get;
        set;
    } = -1;

    /// <inheritdoc/>
    public int PovCount => PovMap.Length;

    /// <inheritdoc/>
    public int ButtonCount => ButtonMap.Length;

    /// <inheritdoc/>
    public InputDeviceType DeviceType => InputDeviceType.GamingDevice;

    /// <summary>
    /// Function to determine if the device is an XInput device.
    /// </summary>
    /// <param name="hidPath">The HID path of the device to evaluate.</param>
    /// <returns><b>true</b> if the device is an XInput compatible device, <b>false</b> if not.</returns>
    private static bool IsXInputDevice(string hidPath)
    {
        if (string.IsNullOrWhiteSpace(hidPath))
        {
            return false;
        }

        return hidPath.Contains("&IG_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Function to get the human readable name for a gaming device from a <see cref="RID_DEVICE_INFO_HID"/> object.
    /// </summary>
    /// <param name="hidPath">The HID path for the device.</param>
    /// <param name="usagePage">The HID usage page for the device.</param>
    /// <param name="usage">The HID usage for the device.</param>
    /// <returns>A string containing the human readable name for the gaming device, or an empty string if the device name could not be determined.</returns>
    /// <remarks>
    /// <para>
    /// This will retrieve the friendly name for a gaming device from a <see cref="RID_DEVICE_INFO_HID"/> object. If the <see cref="RID_DEVICE_INFO_HID.usUsage"/> is not a <see cref="HIDUsage.Gamepad"/> or 
    /// <see cref="HIDUsage.Joystick"/>.
    /// </para>
    /// <para>
    /// This is meant for raw input gaming human interface devices. As such, if this is used on an XInput controller, then the device will not be given a name and empty will be returned. If 
    /// use of an XInput controller is required, then use the Gorgon XInput driver to provide access to those devices.
    /// </para>
    /// </remarks>
    private static string GetGamingDeviceName(string hidPath, HIDUsagePage usagePage, HIDUsage usage)
    {
        if ((string.IsNullOrWhiteSpace(hidPath))
            || (usagePage != HIDUsagePage.Generic)
            || ((usage is not HIDUsage.Gamepad and not HIDUsage.Joystick)))
        {
            return string.Empty;
        }

        // Take the HID path, and split it out until we get the appropriate sub key name.
        string[] parts = hidPath.Split(['#'], StringSplitOptions.RemoveEmptyEntries);

        if ((parts.Length < 2) || (string.IsNullOrWhiteSpace(parts[1])))
        {
            return string.Empty;
        }

        string subKeyName = parts[1];

        // Find the key and open it.
        // The original example code this is based on uses HKEY_LOCAL_MACHINE instead of CURRENT_USER.  This may be a difference 
        // between operating systems.
        const string regKeyPath = @"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\";

        using RegistryKey? joystickOemKey = Registry.CurrentUser.OpenSubKey(regKeyPath, false);

        if (joystickOemKey is null)
        {
            return string.Empty;
        }

        string joystickDeviceKeyName = joystickOemKey.GetSubKeyNames().FirstOrDefault(item => subKeyName.StartsWith(item, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(joystickDeviceKeyName))
        {
            return string.Empty;
        }

        using RegistryKey? joystickVidKey = joystickOemKey.OpenSubKey(joystickDeviceKeyName, false);
        return joystickVidKey?.GetValue("OEMName")?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Function to retrieve the indices for the axes on the device.
    /// </summary>
    /// <param name="valueCaps">The value caps from the device.</param>
    /// <param name="xInput"><b>true</b> if the axes are for an XInput compatible controller, or <b>false</b> if not.</param>
    /// <param name="log">The log used for debug messages.</param>
    /// <returns>An array of values populated with the various axes indices on the device.</returns>
    private static Dictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> GetAxes(ReadOnlySpan<HIDP_VALUE_CAPS> valueCaps, bool xInput, IGorgonLog log)
    {
        Dictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> axisIndices = [];

        // Grab indices.
        for (int i = 0; i < valueCaps.Length; ++i)
        {
            ref readonly HIDP_VALUE_CAPS valueCap = ref valueCaps[i];

            if (valueCap.IsRange)
            {
                continue;
            }

            HIDUsage usage = (HIDUsage)valueCap.Anonymous.NotRange.Usage;

            if (usage is HIDUsage.None or HIDUsage.HatSwitch)
            {
                continue;
            }

            GamingDeviceAxis axis = usage switch
            {
                HIDUsage.X => GamingDeviceAxis.XAxis,
                HIDUsage.Y => GamingDeviceAxis.YAxis,
                HIDUsage.Z => GamingDeviceAxis.ZAxis,
                HIDUsage.RotationX => GamingDeviceAxis.RotationX,
                HIDUsage.RotationY => GamingDeviceAxis.RotationY,
                HIDUsage.RotationZ => GamingDeviceAxis.RotationZ,
                HIDUsage.Slider => GamingDeviceAxis.Throttle,
                _ => GamingDeviceAxis.None
            };

            if (axis == GamingDeviceAxis.None)
            {
                continue;
            }

            // Determine the size of the report data.
            //
            // We check to see if the logical max value is less than logical min to determine if it's a value that needs to be
            // reinterpreted from signed to unsigned. I have no idea if this is the correct way to do it, and if anyone out
            // there knows better, please get in touch with me. I'd love to know how to do this correctly.
            int min = valueCap.BitSize switch
            {
                >= 0 and <= 8 => valueCap.LogicalMax > valueCap.LogicalMin ? valueCap.LogicalMin : (byte)valueCap.LogicalMin,
                >= 9 and <= 16 => valueCap.LogicalMax > valueCap.LogicalMin ? valueCap.LogicalMin : (ushort)valueCap.LogicalMin,
                _ => valueCap.LogicalMin
            };

            int max = valueCap.BitSize switch
            {
                >= 0 and <= 8 => valueCap.LogicalMax > valueCap.LogicalMin ? valueCap.LogicalMax : (byte)valueCap.LogicalMax,
                >= 9 and <= 16 => valueCap.LogicalMax > valueCap.LogicalMin ? valueCap.LogicalMax : (ushort)valueCap.LogicalMax,
                _ => valueCap.LogicalMax
            };

            int range = ((max - min) + 1) / 2;

            // For throttle ranges, use the device information. For other axes, shift the ranges to center around 0.
            min = axis == GamingDeviceAxis.Throttle ? min : -range;
            max = axis == GamingDeviceAxis.Throttle ? max : (range - 1);

            axisIndices[axis] = new GorgonGamingDeviceAxisInfo(axis, new GorgonRange<int>(min, max), 0, valueCap.Anonymous.NotRange.DataIndex);
        }

        if (xInput)
        {
            axisIndices[GamingDeviceAxis.LeftTrigger] = new GorgonGamingDeviceAxisInfo(GamingDeviceAxis.LeftTrigger, new GorgonRange<int>(0, byte.MaxValue), 0, 0xfffe);
            axisIndices[GamingDeviceAxis.RightTrigger] = new GorgonGamingDeviceAxisInfo(GamingDeviceAxis.RightTrigger, new GorgonRange<int>(0, byte.MaxValue), 0, 0xffff);
        }

        log.Print($"Axis count: {axisIndices.Count}", LoggingLevel.Verbose);

        return axisIndices;
    }

    /// <summary>
    /// Function to retrieve the indices for the point of view hat(s) on the device.
    /// </summary>
    /// <param name="valueCaps">The value caps from the device.</param>
    /// <returns>An array of values populated with the various point of view hat(s) indices on the device.</returns>
    private static PovMap[] GetPovs(ReadOnlySpan<HIDP_VALUE_CAPS> valueCaps)
    {
        int povCount = 0;

        // Find the number of axes.
        for (int i = 0; i < valueCaps.Length; ++i)
        {
            ref readonly HIDP_VALUE_CAPS valueCap = ref valueCaps[i];

            if (valueCap.IsRange)
            {
                continue;
            }

            HIDUsage usage = (HIDUsage)valueCap.Anonymous.NotRange.Usage;

            if (usage is not HIDUsage.HatSwitch)
            {
                continue;
            }

            ++povCount;
        }

        if (povCount == 0)
        {
            return [];
        }

        // Grab indices.
        PovMap[] povIndices = new PovMap[povCount];
        int povIndex = 0;

        for (int i = 0; i < valueCaps.Length; ++i)
        {
            ref readonly HIDP_VALUE_CAPS valueCap = ref valueCaps[i];

            if (valueCap.IsRange)
            {
                continue;
            }

            HIDUsage usage = (HIDUsage)valueCap.Anonymous.NotRange.Usage;

            if (usage is not HIDUsage.HatSwitch)
            {
                continue;
            }

            // Convert the values to degrees.
            int degreeIncrement = (int)((360.0 / ((valueCap.LogicalMax - valueCap.LogicalMin) + 1)) * 100.0);

            povIndices[povIndex++] = new PovMap(valueCap.Anonymous.NotRange.DataIndex, valueCap.LogicalMin, valueCap.LogicalMax, (valueCap.PhysicalMin * 100).Max(0), (valueCap.PhysicalMax * 100).Min(36000), degreeIncrement);
        }

        return povIndices;
    }

    /// <summary>
    /// Function to retrieve the indices for the buttons on the device.
    /// </summary>
    /// <param name="buttonCaps">The button capabilities from the device.</param>
    /// <param name="log">The log used for debug messages.</param>
    /// <returns>An array of values populated with the various button indices on the device.</returns>
    private static ushort[] GetButtonIndices(ReadOnlySpan<HIDP_BUTTON_CAPS> buttonCaps, IGorgonLog log)
    {
        int buttonCount = 0;

        // Find the number of buttons.
        for (int i = 0; i < buttonCaps.Length; ++i)
        {
            ref readonly HIDP_BUTTON_CAPS buttonCap = ref buttonCaps[i];
            HIDUsagePage usagePage = (HIDUsagePage)buttonCap.UsagePage;

            if (usagePage != HIDUsagePage.Button)
            {
                continue;
            }

            if (buttonCap.IsRange)
            {
                buttonCount += (buttonCap.Anonymous.Range.DataIndexMax - buttonCap.Anonymous.Range.DataIndexMin) + 1;
            }
            else
            {
                ++buttonCount;
            }
        }

        log.Print($"Button count: {buttonCount}", LoggingLevel.Verbose);

        if (buttonCount == 0)
        {
            return [];
        }

        // Grab indices.
        ushort[] buttonIndices = new ushort[buttonCount];
        int buttonIndex = 0;

        for (int i = 0; i < buttonCaps.Length; ++i)
        {
            ref readonly HIDP_BUTTON_CAPS buttonCap = ref buttonCaps[i];
            HIDUsagePage usagePage = (HIDUsagePage)buttonCap.UsagePage;

            if (usagePage != HIDUsagePage.Button)
            {
                continue;
            }

            if (!buttonCap.IsRange)
            {
                buttonIndices[buttonIndex++] = buttonCap.Anonymous.NotRange.DataIndex;
                continue;
            }

            int indexCount = 1 + (buttonCap.Anonymous.Range.DataIndexMax - buttonCap.Anonymous.Range.DataIndexMin);

            for (int j = 0; j < indexCount; ++j)
            {
                buttonIndices[buttonIndex++] = (ushort)(buttonCap.Anonymous.Range.DataIndexMin + j);
            }
        }

        return buttonIndices;
    }

    /// <summary>
    /// Function to retrieve the capabilities of the device.
    /// </summary>
    /// <param name="deviceHandle">The handle to the device.</param>
    /// <param name="preparsedDataBuffer">The buffer that will hold the preparsed data for the device.</param>
    /// <param name="xInput"><b>true</b> if the device is an XInput compatible device, <b>false</b> if it's not.</param>
    /// <param name="log">The log used for debug messaging.</param>
    /// <returns>The button indices, axis indices, POV hat indices for the device and the flags for the device.</returns>
    private static (ushort[] ButtonIndices, IReadOnlyDictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> AxisIndices, PovMap[] PovMap, GamingDeviceCapabilityFlags Flags) GetCaps(HANDLE deviceHandle, ReadOnlySpan<byte> preparsedDataBuffer, bool xInput, IGorgonLog log)
    {
        HidApi.GetCaps(preparsedDataBuffer, out HIDP_CAPS caps);

        Span<HIDP_BUTTON_CAPS> buttonCaps = stackalloc HIDP_BUTTON_CAPS[caps.NumberInputButtonCaps];
        Span<HIDP_VALUE_CAPS> valueCaps = stackalloc HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

        HidApi.GetDeviceButtonCaps(preparsedDataBuffer, buttonCaps);
        HidApi.GetDeviceValueCaps(preparsedDataBuffer, valueCaps);

        Dictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> axisIndices = GetAxes(valueCaps, xInput, log);
        ushort[] buttonIndices = GetButtonIndices(buttonCaps, log);
        PovMap[] povMap = GetPovs(valueCaps);

        GamingDeviceCapabilityFlags flags = xInput ? GamingDeviceCapabilityFlags.IsXInputDevice : GamingDeviceCapabilityFlags.None;

        if (povMap.Length > 0)
        {
            log.Print($"POV hat count: {povMap.Length}", LoggingLevel.Verbose);
            flags |= GamingDeviceCapabilityFlags.SupportsPOV;
        }

        if (axisIndices.ContainsKey(GamingDeviceAxis.RotationZ))
        {
            log.Print("Rudder support.", LoggingLevel.Verbose);
            flags |= GamingDeviceCapabilityFlags.SupportsRudder;
        }

        if (axisIndices.ContainsKey(GamingDeviceAxis.Throttle))
        {
            log.Print("Throttle support.", LoggingLevel.Verbose);
            flags |= GamingDeviceCapabilityFlags.SupportsThrottle;
        }

        if (axisIndices.ContainsKey(GamingDeviceAxis.ZAxis))
        {
            log.Print("Z axis support.", LoggingLevel.Verbose);
            flags |= GamingDeviceCapabilityFlags.SupportsZAxis;
        }

        if (xInput)
        {
            log.Print("Vibration support.", LoggingLevel.Verbose);
            flags |= GamingDeviceCapabilityFlags.SupportsVibration;
        }

        return (buttonIndices, axisIndices, povMap, flags);
    }

    /// <summary>
    /// Function to retrieve the stable hash code for a string.
    /// </summary>
    /// <param name="str">The string to compute from.</param>
    /// <returns>The stable hash code value.</returns>
    /// <remarks>
    /// <para>
    /// This code was adapted from https://stackoverflow.com/a/36846609/1045720
    /// </para>
    /// </remarks>
    private static int GetStableHashCode(ReadOnlySpan<char> str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];

                if ((i == str.Length - 1) || (str[i + 1] == '\0'))
                {
                    break;
                }

                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    /// <summary>
    /// Function to generate a unique identifier for a device.
    /// </summary>
    /// <param name="productName">The friendly product name for the device.</param>
    /// <param name="deviceName">The system name for the device.</param>
    /// <param name="productID">The ID of the product.</param>
    /// <param name="vendorID">The ID of the vendor.</param>
    /// <returns>A new GUID generated from the parameters.</returns>
    private static Guid GetDeviceGuid(string productName, string deviceName, uint productID, uint vendorID)
    {
        int deviceNameInt = GetStableHashCode(deviceName.AsSpan(0, deviceName.Length));
        int productInt = GetStableHashCode(productName.AsSpan(0, productName.Length));

        short productNameShort1 = (short)(productInt & 0xFFFF);
        short productNameShort2 = (short)((productInt >> 16) & 0xFFFF);

        byte vendorID1 = (byte)(vendorID & 0xff);
        byte vendorID2 = (byte)((vendorID >> 8) & 0xff);
        byte vendorID3 = (byte)((vendorID >> 16) & 0xff);
        byte vendorID4 = (byte)((vendorID >> 24) & 0xff);

        byte productID1 = (byte)(productID & 0xff);
        byte productID2 = (byte)((productID >> 8) & 0xff);
        byte productID3 = (byte)((productID >> 16) & 0xff);
        byte productID4 = (byte)((productID >> 24) & 0xff);

        return new(deviceNameInt, productNameShort1, productNameShort2, vendorID1, productID4, vendorID2, productID3, vendorID3, productID2, vendorID4, productID1);
    }

    /// <summary>
    /// Function to retrieve the friendly product name for the device.
    /// </summary>
    /// <param name="deviceHandle">The open handle to the device.</param>
    /// <param name="defaultName">The default product name if the name could not be retrieved.</param>
    /// <param name="deviceName">The system device name.</param>
    /// <param name="usagePage">The HID usage page for the device.</param>
    /// <param name="usage">The HID usage for the device.</param>
    /// <param name="productID">The device product ID.</param>
    /// <param name="vendorID">The device vendor ID.</param>
    /// <returns>The friendly product name.</returns>
    private static string GetProductName(HANDLE deviceHandle, string defaultName, string deviceName, HIDUsagePage usagePage, HIDUsage usage, uint vendorID, uint productID)
    {
        if ((_productNames.TryGetValue((vendorID, productID), out string? productName)) && (!string.IsNullOrWhiteSpace(productName)))
        {
            return productName;
        }

        productName = HidApi.GetProductString(deviceHandle);

        if (string.IsNullOrWhiteSpace(productName))
        {
            productName = GetGamingDeviceName(deviceName, usagePage, usage);

            if (string.IsNullOrWhiteSpace(productName))
            {
                productName = defaultName;
            }
        }

        return productName;
    }

    /// <inheritdoc/>
    public async Task RefreshXInputSlotAsync(IGorgonLog? log = null, CancellationToken? cancelToken = null)
    {
        if (((Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
            || (string.IsNullOrWhiteSpace(DeviceName)))
        {
            XInputSlot = -1;
        }

        if (XInputSlot == -1)
        {
            return;
        }

        XInputSlot = await XInputApi.LocateXInputSlotAsync(DeviceName, log ?? GorgonLog.NullLog, cancelToken ?? CancellationToken.None);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RefreshXInputSlot(IGorgonLog? log = null)
    {
        if ((Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
        {
            XInputSlot = -1;
        }

        if (XInputSlot == -1)
        {
            return;
        }

        XInputSlot = XInputApi.LocateXInputSlot(DeviceName, log ?? GorgonLog.NullLog);
    }

    /// <summary>
    /// Function to retrieve the gaming device information from a raw input data structure.
    /// </summary>
    /// <param name="deviceName">The name of the device from raw input.</param>
    /// <param name="className">The name of the device class.</param>
    /// <param name="defaultDescription">The default description of the device if it could not be retrieved.</param>
    /// <param name="device">The device data.</param>
    /// <param name="rawInputData">The information about the device from raw input.</param>
    /// <param name="log">The log used for debug messaging.</param>
    /// <returns>A new object containing information about the gaming input device, or <see cref="Empty"/> if the info can't be retrieved.</returns>
    public static GamingDeviceInfo GetGamingDeviceInfo(string deviceName, string className, string defaultDescription, ref readonly RAWINPUTDEVICELIST device, ref readonly RID_DEVICE_INFO rawInputData, IGorgonLog log)
    {
        ArrayPool<byte> pool = GorgonArrayPools<byte>.GetBestPool(8);
        byte[] byteBuffer = pool.Rent(8);
        HIDUsagePage usagePage = (HIDUsagePage)rawInputData.Anonymous.hid.usUsagePage;
        HIDUsage usage = (HIDUsage)rawInputData.Anonymous.hid.usUsage;
        ushort[] buttonIndices;
        PovMap[] povMap;
        List<GorgonRange<int>> vibrationRanges = [];
        IReadOnlyDictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> axisIndices;
        GamingDeviceCapabilityFlags flags;
        HANDLE fileHandle = HANDLE.Null;

        if (usagePage != HIDUsagePage.Generic)
        {
            return Empty;
        }

        GamingDeviceType deviceType = usage switch
        {
            HIDUsage.Gamepad => GamingDeviceType.Gamepad,
            HIDUsage.Joystick => GamingDeviceType.Joystick,
            _ => GamingDeviceType.Unknown
        };

        if (deviceType == GamingDeviceType.Unknown)
        {
            log.PrintWarning($"The device '{deviceName}' is not a game pad or joystick. Skipping...", LoggingLevel.Intermediate);
            return Empty;
        }

        try
        {
            // Get the preparsed data for the device before anything else.
            int bufferSize = RawInputApi.GetPreparsedDataSize(device.hDevice);

            if (bufferSize == 0)
            {
                log.PrintError($"Could not retrieve the size of the HID preparsed data for device handle 0x{((nint)device.hDevice).FormatHex()}. This device will be skipped.", LoggingLevel.Simple);
                return Empty;
            }
            else
            {
                log.Print($"Preparsed data size: {bufferSize} bytes.", LoggingLevel.Verbose);
            }

            // Let's not create more heap
            byte[] preparsedBuffer = new byte[bufferSize];

            RawInputApi.GetPreparsedDeviceInfoData(device.hDevice, preparsedBuffer);

            // Now let's check if this is an XInput device, just to get it out of the way.
            bool isXInput = IsXInputDevice(deviceName);
            int xInputSlot = -1;

            if (isXInput)
            {
                xInputSlot = XInputApi.LocateXInputSlot(deviceName, log);
            }

            fileHandle = PInvoke.CreateFile(deviceName, GENERIC_ACCESS_RIGHTS.GENERIC_READ | GENERIC_ACCESS_RIGHTS.GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE, FILE_CREATION_DISPOSITION.OPEN_EXISTING, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL);

            string productName = GetProductName(fileHandle, defaultDescription, deviceName, usagePage, usage, rawInputData.Anonymous.hid.dwVendorId, rawInputData.Anonymous.hid.dwProductId);
            string manufacturerName = HidApi.GetManufacturerString(fileHandle);

            if ((!string.IsNullOrWhiteSpace(manufacturerName)) && (!productName.StartsWith(manufacturerName, StringComparison.Ordinal)))
            {
                productName = $"{manufacturerName} {productName}";
            }

            Guid id = GetDeviceGuid(productName, deviceName, rawInputData.Anonymous.hid.dwProductId, rawInputData.Anonymous.hid.dwVendorId);

            log.Print($"HID Device: {deviceType} {deviceName}", LoggingLevel.Intermediate);
            log.Print($"Product Name: {productName}", LoggingLevel.Intermediate);
            log.Print($"Device ID: {id}", LoggingLevel.Verbose);

            if ((isXInput) && (xInputSlot != -1))
            {
                log.Print("XInput compatible: Yes", LoggingLevel.Intermediate);
                log.Print($"XInput slot: {xInputSlot}/3", LoggingLevel.Verbose);
            }
            else
            {
                log.Print("XInput compatible: No", LoggingLevel.Intermediate);
            }

            (buttonIndices, axisIndices, povMap, flags) = GetCaps(device.hDevice, preparsedBuffer, isXInput, log);

            PInvoke.CloseHandle(fileHandle);
            fileHandle = HANDLE.Null;

            int reportItemLength = HidApi.GetReportItemLength(preparsedBuffer);

            if (isXInput)
            {
                vibrationRanges.Add(new GorgonRange<int>(0, ushort.MaxValue));
                vibrationRanges.Add(new GorgonRange<int>(0, ushort.MaxValue));
            }

            return new GamingDeviceInfo(id,
                                        device.hDevice,
                                        deviceType,
                                        deviceName,
                                        className,
                                        (int)rawInputData.Anonymous.hid.dwVendorId,
                                        (int)rawInputData.Anonymous.hid.dwProductId,
                                        (int)rawInputData.Anonymous.hid.dwVersionNumber,
                                        axisIndices,
                                        vibrationRanges,
                                        flags,
                                        productName)
            {
                ButtonMap = buttonIndices,
                PovMap = povMap,
                PreparsedData = preparsedBuffer,
                HidDataLength = reportItemLength,
                XInputSlot = xInputSlot
            };
        }
        catch (Win32Exception wEx)
        {
            // If for some reason we cannot open the file handle to the device, just stop interrogation and move on.
            // This seems to happen sometimes with bluetooth devices.
            if (wEx.NativeErrorCode == (int)WIN32_ERROR.ERROR_FILE_NOT_FOUND)
            {
                log.PrintError($"Device {deviceName} was not found when trying to open. This may be because the device is not ready yet.", LoggingLevel.Simple);
                return Empty;
            }

            throw;
        }
        finally
        {
            if ((fileHandle != HANDLE.Null) && (fileHandle != HANDLE.INVALID_HANDLE_VALUE))
            {
                PInvoke.CloseHandle(fileHandle);
            }

            pool.Return(byteBuffer, true);
        }
    }
}