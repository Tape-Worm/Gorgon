
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
// Created: Saturday, July 4, 2015 2:20:46 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Input.Devices;

/// <summary>
/// The type of gaming device.
/// </summary>
public enum GamingDeviceType
{
    /// <summary>
    /// Unknown device type.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Device is a joystick.
    /// </summary>
    Joystick = 1,
    /// <summary>
    /// Device is a gamepad.
    /// </summary>
    Gamepad = 2
}

/// <summary>
/// Flags to indicate which extra capabilities are supported by the gaming device
/// </summary>
/// <remarks>
/// The values in this enumeration may be OR'd together to indicate support for multiple items, or it will return <see cref="None"/> if the device does not have any special capabilities
/// </remarks>
[Flags]
public enum GamingDeviceCapabilityFlags
{
    /// <summary>
    /// No extra capabilities.  This flag is mutually exclusive.
    /// </summary>
    None = 0,
    /// <summary>
    /// Supports point of view controls.
    /// </summary>
    SupportsPOV = 1,
    /// <summary>
    /// Supports a rudder control.
    /// </summary>
    SupportsRudder = 2,
    /// <summary>
    /// Supports a throttle control.
    /// </summary>
    SupportsThrottle = 4,
    /// <summary>
    /// Supports vibration.
    /// </summary>
    SupportsVibration = 8,
    /// <summary>
    /// Supports a Z axis.  This is sometimes used in place of the <see cref="SupportsThrottle"/> flag.
    /// </summary>
    SupportsZAxis = 16,
    /// <summary>
    /// The device is an XInput compatible device.
    /// </summary>
    IsXInputDevice = 32
}

/// <summary>
/// Contains values for common, well known, gaming device axes
/// </summary>
public enum GamingDeviceAxis
{
    /// <summary>
    /// No axis.
    /// </summary>
    None = 0,
    /// <summary>
    /// The primary horizontal axis.
    /// </summary>
    XAxis = 1,
    /// <summary>
    /// The primary vertical axis.
    /// </summary>
    YAxis = 2,
    /// <summary>
    /// The primary z axis or rudder.
    /// </summary>
    ZAxis = 3,
    /// <summary>
    /// The throttle axis.
    /// </summary>
    Throttle = 4,
    /// <summary>
    /// Rotation about the X axis. Typically assigned to the right stick on a game pad.
    /// </summary>
    RotationX = 5,
    /// <summary>
    /// Rotation about the Y axis. Typically assigned to the right stick on a game pad.
    /// </summary>
    RotationY = 6,
    /// <summary>
    /// Rotation about the Z axis. Typically assigned to the right stick on a game pad.
    /// </summary>
    RotationZ = 7,
    /// <summary>
    /// The left trigger on an XInput compatible device. For devices with triggers that are not XInput compatible, use the <see cref="ZAxis"/> value.
    /// </summary>
    LeftTrigger = 8,
    /// <summary>
    /// The right trigger on an XInput compatible device. For devices with triggers that are not XInput compatible, use the <see cref="ZAxis"/> value.
    /// </summary>
    RightTrigger = 9
}

/// <summary>
/// Contains information for a gaming device (gaming device, game pad, etc...)
/// </summary>
public interface IGorgonGamingDeviceInfo
    : IGorgonInputDeviceInfo
{
    /// <summary>
    /// Property to return the XInput slot for the device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the device is a XInput compatible device, then this value will be set to the slot number (as indicated by the LED on the device) minus 1 (e.g. if the device is slot 1, then this value will be 0). 
    /// </para>
    /// <para>
    /// If the device is not an XInput compatible device, or is disconnected, then this value will return -1.
    /// </para>
    /// </remarks>
    int XInputSlot
    {
        get;
    }

    /// <summary>
    /// Property to return the unique ID for the device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a stable, unique ID for the device. This value will remain the same across application or system restarts, so users can store the value for later use if needed. 
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// If the device is plugged into a different USB port, then this value will change.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    Guid DeviceID
    {
        get;
    }

    /// <summary>
    /// Property to return the number of point of view controls on the gaming device.
    /// </summary>
    int PovCount
    {
        get;
    }

    /// <summary>
    /// Property to return the number of buttons available on the gaming device.
    /// </summary>
    int ButtonCount
    {
        get;
    }

    /// <summary>
    /// Property to return the <see cref="GorgonGamingDeviceAxisInfo"/> values for each axis on the gaming device.
    /// </summary>
    IReadOnlyDictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> AxisInfo
    {
        get;
    }

    /// <summary>
    /// Property to return the tolerances for each of the vibration motors in the gaming device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to XInput compatible devices at this time. This list will be empty for non XInput devices.
    /// </para>
    /// <para>
    /// Check the <see cref="Capabilities"/> property to determine if the device is XInput compatible.
    /// </para>
    /// </remarks>
    IReadOnlyList<GorgonRange<int>> VibrationMotorRanges
    {
        get;
    }

    /// <summary>
    /// Property to return the capabilities supported by the gaming device.
    /// </summary>
    GamingDeviceCapabilityFlags Capabilities
    {
        get;
    }

    /// <summary>
    /// Property to return the sub type for the gaming device.
    /// </summary>
    GamingDeviceType DeviceSubType
    {
        get;
    }

    /// <summary>
    /// Function to asynchronously refresh the XInput slot if it's not assigned.
    /// </summary>
    /// <param name="log">[Optional] The log used for logging debug messages.</param>
    /// <param name="cancelToken">[Optional] The cancellation token to cancel the operation.</param>
    /// <remarks>
    /// <para>
    /// Use this method to update the XInput controller's slot index if it has become disconnected or it was not assigned (<see cref="XInputSlot"/> will be -1).
    /// </para>
    /// <para>
    /// If the device is in the process of connecting (e.g. wireless devices), then this method will block for up to 10 seconds or until the slot is found.
    /// </para>
    /// <para>
    /// If the device is not an XInput compatiable device, this then method will do nothing.
    /// </para>
    /// </remarks>
    /// <seealso cref="XInputSlot"/>
    Task RefreshXInputSlotAsync(IGorgonLog? log = null, CancellationToken? cancelToken = null);

    /// <summary>
    /// Function to refresh the XInput slot if it's not assigned.
    /// </summary>
    /// <param name="log">[Optional] The log used for logging debug messages.</param>
    /// <remarks>
    /// <para>
    /// Use this method to update the XInput controller's slot index if it has become disconnected or it was not assigned (<see cref="XInputSlot"/> will be -1).
    /// </para>
    /// <para>
    /// If the device is in the process of connecting (e.g. wireless devices), then this method will block for up to 10 seconds or until the slot is found.
    /// </para>
    /// <para>
    /// If the device is not an XInput compatiable device, this then method will do nothing.
    /// </para>
    /// </remarks>
    /// <seealso cref="XInputSlot"/>
    void RefreshXInputSlot(IGorgonLog? log = null);
}
