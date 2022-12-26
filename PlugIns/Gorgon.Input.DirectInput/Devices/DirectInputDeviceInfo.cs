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
// Created: Sunday, September 13, 2015 2:09:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Native;
using DI = SharpDX.DirectInput;

namespace Gorgon.Input.DirectInput;

/// <summary>
/// Device information for a DirectInput gaming device.
/// </summary>
internal class DirectInputDeviceInfo
    : IGorgonGamingDeviceInfo
{
    #region Properties.
    /// <summary>
    /// Property to return the <see cref="GorgonGamingDeviceAxisInfo"/> values for each axis on the gaming device.
    /// </summary>
    /// <remarks>
    /// Use this value to retrieve the number of axes the gaming device supports by checking its <see cref="GorgonGamingDeviceAxisList{T}.Count"/> property.
    /// </remarks>
    public IReadOnlyDictionary<GamingDeviceAxis, GorgonGamingDeviceAxisInfo> AxisInfo
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the number of buttons available on the gaming device.
    /// </summary>
    public int ButtonCount
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the capabilities supported by the gaming device.
    /// </summary>
    public GamingDeviceCapabilityFlags Capabilities
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return a human readable description for the gaming device.
    /// </summary>
    public string Description
    {
        get;
    }

    /// <summary>
    /// Property to return the ID for the manufacturer of the gaming device.
    /// </summary>
    public int ManufacturerID
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the ID of the product.
    /// </summary>
    public int ProductID
    {
        get;
        private set;
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
    public IReadOnlyList<GorgonRange> VibrationMotorRanges
    {
        get;
    }

    /// <summary>
    /// Property to return the number of point of view controls on the gaming device.
    /// </summary>
    public int POVCount
    {
        get;
        private set;
    }

    /// <summary>Property to return the unique ID for the device.</summary>
    public Guid DeviceID
    {
        get;
    }
    #endregion

    #region Methods.

    /// <summary>
    /// Function to retrieve the capabilities from the DirectInput joystick.
    /// </summary>
    /// <param name="joystick">The DirectInput joystick to evaluate.</param>
    public IReadOnlyDictionary<GamingDeviceAxis, DI.DeviceObjectId> GetDeviceCaps(DI.Joystick joystick)
    {
        var defaults = new Dictionary<GamingDeviceAxis, int>();
        var axisRanges = new Dictionary<GamingDeviceAxis, GorgonRange>();

        ProductID = joystick.Properties.ProductId;
        ManufacturerID = joystick.Properties.VendorId;
        ButtonCount = joystick.Capabilities.ButtonCount;
        POVCount = joystick.Capabilities.PovCount;
        Capabilities = POVCount > 0 ? GamingDeviceCapabilityFlags.SupportsPOV : GamingDeviceCapabilityFlags.None;

        IList<DI.DeviceObjectInstance> axisInfo = joystick.GetObjects(DI.DeviceObjectTypeFlags.Axis);
        var axisMappings = new Dictionary<GamingDeviceAxis, DI.DeviceObjectId>();

        foreach (DI.DeviceObjectInstance axis in axisInfo)
        {
            var usage = (HIDUsage)axis.Usage;
            DI.ObjectProperties properties = joystick.GetObjectPropertiesById(axis.ObjectId);

            // Skip this axis if retrieving the properties results in failure.
            if (properties is null)
            {
                continue;
            }

            var range = new GorgonRange(properties.Range.Minimum, properties.Range.Maximum);
            int midPoint = ((range.Range + 1) / 2) + range.Minimum;

            switch (usage)
            {
                case HIDUsage.X:
                    axisMappings[GamingDeviceAxis.XAxis] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.XAxis] = range;
                    defaults[GamingDeviceAxis.XAxis] = range.Minimum < 0 ? 0 : midPoint;
                    break;
                case HIDUsage.Y:
                    axisMappings[GamingDeviceAxis.YAxis] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.YAxis] = range;
                    defaults[GamingDeviceAxis.YAxis] = range.Minimum < 0 ? 0 : midPoint;
                    break;
                case HIDUsage.Slider:
                    axisMappings[GamingDeviceAxis.Throttle] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.Throttle] = range;
                    defaults[GamingDeviceAxis.Throttle] = 0;
                    Capabilities |= GamingDeviceCapabilityFlags.SupportsThrottle;
                    break;
                case HIDUsage.Z:
                    axisMappings[GamingDeviceAxis.ZAxis] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.ZAxis] = range;
                    defaults[GamingDeviceAxis.ZAxis] = range.Minimum < 0 ? 0 : midPoint;
                    Capabilities |= GamingDeviceCapabilityFlags.SupportsZAxis;
                    break;
                case HIDUsage.RelativeX:
                    axisMappings[GamingDeviceAxis.XAxis2] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.XAxis2] = range;
                    Capabilities |= GamingDeviceCapabilityFlags.SupportsSecondaryXAxis;
                    defaults[GamingDeviceAxis.XAxis2] = midPoint;
                    break;
                case HIDUsage.RelativeY:
                    axisMappings[GamingDeviceAxis.YAxis2] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.YAxis2] = range;
                    Capabilities |= GamingDeviceCapabilityFlags.SupportsSecondaryYAxis;
                    defaults[GamingDeviceAxis.YAxis2] = midPoint;
                    break;
                case HIDUsage.RelativeZ:
                    axisMappings[GamingDeviceAxis.ZAxis2] = axis.ObjectId;
                    axisRanges[GamingDeviceAxis.ZAxis2] = range;
                    Capabilities |= GamingDeviceCapabilityFlags.SupportsRudder;
                    defaults[GamingDeviceAxis.ZAxis2] = 0;
                    break;
            }
        }

        AxisInfo = axisRanges.Select(item => new GorgonGamingDeviceAxisInfo(item.Key, item.Value, defaults[item.Key])).ToDictionary(k => k.Axis, v => v);

        return axisMappings;
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectInputDeviceInfo"/> class.
    /// </summary>
    /// <param name="devInstance">The DirectInput device instance.</param>
    public DirectInputDeviceInfo(DI.DeviceInstance devInstance)
    {
        VibrationMotorRanges = Array.Empty<GorgonRange>();
        DeviceID = devInstance.InstanceGuid;
        Description = devInstance.ProductName;
    }
    #endregion
}
