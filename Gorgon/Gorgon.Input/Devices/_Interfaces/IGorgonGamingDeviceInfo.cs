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
// Created: Saturday, July 4, 2015 2:20:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;

namespace Gorgon.Input;

/// <summary>
/// Flags to indicate which extra capabilities are supported by the gaming device.
/// </summary>
/// <remarks>
/// The values in this enumeration may be OR'd together to indicate support for multiple items, or it will return <see cref="None"/> if the device does not have any special capabilities.
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
    /// Supports a throttle (or Z-axis) control.
    /// </summary>
    SupportsThrottle = 4,
    /// <summary>
    /// Supports vibration.
    /// </summary>
    SupportsVibration = 8,
    /// <summary>
    /// Supports a secondary X axis.
    /// </summary>
    SupportsSecondaryXAxis = 16,
    /// <summary>
    /// Supports a secondary Y axis.
    /// </summary>
    SupportsSecondaryYAxis = 32,
    /// <summary>
    /// Supports a Z axis.  This is sometimes used in place of the <see cref="SupportsThrottle"/> flag.
    /// </summary>
    SupportsZAxis = 64
}

/// <summary>
/// Contains values for common, well known, gaming device axes.
/// </summary>
public enum GamingDeviceAxis
{
    /// <summary>
    /// The primary horizontal axis.
    /// </summary>
    XAxis = 0,
    /// <summary>
    /// The primary vertical axis.
    /// </summary>
    YAxis = 1,
    /// <summary>
    /// The primary z axis.
    /// </summary>
    ZAxis = 2,
    /// <summary>
    /// The secondary horizontal axis.
    /// </summary>
    XAxis2 = 3,
    /// <summary>
    /// The secondary vertical axis.
    /// </summary>
    YAxis2 = 4,
    /// <summary>
    /// The secondary z axis.
    /// </summary>
    ZAxis2 = 5,
    /// <summary>
    /// The throttle axis.
    /// </summary>
    Throttle = 6,
    /// <summary>
    /// The rudder axis. This maps to the <see cref="ZAxis2"/> value.
    /// </summary>
    Rudder = ZAxis2,
    /// <summary>
    /// Left analog game pad stick horizontal axis. This maps to the <see cref="XAxis"/>.
    /// </summary>
    LeftStickX = XAxis,
    /// <summary>
    /// Left analog game pad stick vertical axis. This maps to the <see cref="YAxis"/>.
    /// </summary>
    LeftStickY = YAxis,
    /// <summary>
    /// Right analog game pad stick horizontal axis. This maps to the <see cref="XAxis2"/>.
    /// </summary>
    RightStickX = XAxis2,
    /// <summary>
    /// Right analog game pad stick vertical axis. This maps to the <see cref="YAxis2"/>.
    /// </summary>
    RightStickY = YAxis2,
    /// <summary>
    /// The right trigger axis for xbox controllers. This maps to the <see cref="ZAxis"/>.
    /// </summary>
    RightTrigger = Throttle,
    /// <summary>
    /// The left trigger axis for xbox controllers. This maps to the <see cref="ZAxis2"/>.
    /// </summary>
    LeftTrigger = Rudder
}

/// <summary>
/// Contains information for a gaming device (gaming device, game pad, etc...).
/// </summary>
public interface IGorgonGamingDeviceInfo
{
    /// <summary>
    /// Property to return the unique ID for the device.
    /// </summary>
    Guid DeviceID
    {
        get;
    }

    /// <summary>
    /// Property to return the number of point of view controls on the gaming device.
    /// </summary>
    int POVCount
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
    /// Property to return the ID for the manufacturer of the gaming device.
    /// </summary>
    int ManufacturerID
    {
        get;
    }

    /// <summary>
    /// Property to return the ID of the product.
    /// </summary>
    int ProductID
    {
        get;
    }

    /// <summary>
    /// Property to return the <see cref="GorgonGamingDeviceAxisInfo"/> values for each axis on the gaming device.
    /// </summary>
    /// <remarks>
    /// Use this value to retrieve the number of axes the gaming device supports by checking its <see cref="GorgonGamingDeviceAxisList{T}.Count"/> property.
    /// </remarks>
    IReadOnlyDictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> AxisInfo
    {
        get;
    }

    /// <summary>
    /// Property to return the tolerances for each of the vibration motors in the gaming device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this value to retrieve the number of vibration motors the gaming device supports by checking its <see cref="IReadOnlyCollection{T}.Count"/> property.
    /// </para>
    /// <para>
    /// If the device does not support vibration, then this list will be empty.
    /// </para>
    /// </remarks>
    IReadOnlyList<GorgonRange> VibrationMotorRanges
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
    /// Property to return a human readable description for the gaming device.
    /// </summary>
    string Description
    {
        get;
    }
}
