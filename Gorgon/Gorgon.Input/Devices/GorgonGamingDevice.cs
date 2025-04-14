
// 
// Gorgon
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, June 24, 2011 10:05:35 AM
// 

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Input.Devices;
using Gorgon.Input.Native;
using Gorgon.Input.Properties;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Foundation;

namespace Gorgon.Input;

/// <inheritdoc cref="IGorgonGamingDevice"/>
public class GorgonGamingDevice
    : IGorgonGamingDevice
{
    // Dead zone constants for XInput controllers.
    private const int XInputGamepadLeftThumbDeadZone = 7849;
    private const int XInputGamepadRightThumbDeadZone = 8689;
    private const int XInputGamepadTriggerThreshold = 30;

    // POV hat data.
    private readonly int[] _povs = [];
    private readonly POVDirection[] _povDirections = [];

    // Button states.
    private readonly bool[] _buttons = [];

    // The vibration motor values.
    private readonly int[] _motorValues = [];

    /// <summary>
    /// Property to return whether the device being used is an XInput capable device.
    /// </summary>
    private bool IsXInputDevice => ((Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) == GamingDeviceCapabilityFlags.IsXInputDevice) && (Info.XInputSlot > -1);

    /// <inheritdoc/>
    public IGorgonGamingDeviceInfo Info
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public GorgonGamingDeviceAxisList Axes
    {
        get;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<int> Povs => _povs;

    /// <inheritdoc/>
    public ReadOnlySpan<POVDirection> PovDirections => _povDirections;

    /// <inheritdoc/>
    public ReadOnlySpan<bool> Buttons => _buttons;

    /// <summary>
    /// Function to map Z axis data to the correct XInput triggers for XInput controllers.
    /// </summary>
    private void MapXInputTriggers()
    {
        Axes.TryGetValue(GamingDeviceAxis.LeftTrigger, out IGorgonGamingDeviceAxis? leftTrigger);
        Axes.TryGetValue(GamingDeviceAxis.RightTrigger, out IGorgonGamingDeviceAxis? rightTrigger);

        (int left, int right) = XInputApi.GetTriggerValues(Info.XInputSlot);

        if (leftTrigger is not null)
        {
            leftTrigger.Value = left;
        }

        if (rightTrigger is not null)
        {
            rightTrigger.Value = right;
        }
    }

    /// <summary>
    /// Function to return the POV direction of the POV hat.
    /// </summary>
    /// <param name="value">The current POV value.</param>
    /// <param name="povMap">The POV mapping.</param>
    /// <returns>The POV direction and value.</returns>
    private static (POVDirection Direction, int Value) GetPOV(int value, PovMap povMap)
    {
        if ((value < povMap.LogicalMin) || (value > povMap.LogicalMax))
        {
            return (POVDirection.Center, -1);
        }

        int degrees = ((value - povMap.LogicalMin) * povMap.UnitDegree).Min(povMap.PhysicalMax).Max(povMap.PhysicalMin);

        return (degrees switch
        {
            >= 0 and < 4500 => POVDirection.Up,
            >= 4500 and < 9000 => POVDirection.Up | POVDirection.Right,
            >= 9000 and < 13500 => POVDirection.Right,
            >= 13500 and < 18000 => POVDirection.Down | POVDirection.Right,
            >= 18000 and < 22500 => POVDirection.Down,
            >= 22500 and < 27000 => POVDirection.Down | POVDirection.Left,
            >= 27000 and < 31500 => POVDirection.Left,
            >= 31500 and < 36000 => POVDirection.Up | POVDirection.Left,
            _ => POVDirection.Center
        }, degrees);
    }

    /// <summary>
    /// Function to locate the index of a specific data item within the HID report data.
    /// </summary>
    /// <param name="dataIndex">The index of the data item to locate.</param>
    /// <param name="hidData">The data to search through.</param>
    /// <returns>The index of the item if found, or -1 if not.</returns>
    private static int GetHIDDataIndex(ushort dataIndex, ReadOnlySpan<HIDP_DATA> hidData)
    {
        if ((dataIndex < hidData.Length) && (hidData[dataIndex].DataIndex == dataIndex))
        {
            return dataIndex;
        }

        for (int i = 0; i < hidData.Length; ++i)
        {
            if (dataIndex == hidData[i].DataIndex)
            {
                return i;
            }
        }

        return -1;
    }

    /// <inheritdoc/>
    public int ConstrainLinear(int axisValue, int deadZone)
    {
        if (deadZone <= 0)
        {
            return axisValue;
        }

        if (axisValue > deadZone)
        {
            return axisValue - deadZone;
        }
        else if (axisValue < -deadZone)
        {
            return axisValue + deadZone;
        }

        return 0;
    }

    /// <inheritdoc/>
    public GorgonPoint ConstrainCircular(GamingDeviceAxis xAxis, GamingDeviceAxis yAxis)
    {
        if ((!Axes.TryGetValue(xAxis, out IGorgonGamingDeviceAxis? deviceXAxis))
            || (!Info.AxisInfo.TryGetValue(xAxis, out GorgonGamingDeviceAxisInfo? xAxisInfo))
            || (!Axes.TryGetValue(yAxis, out IGorgonGamingDeviceAxis? deviceYAxis))
            || (!Info.AxisInfo.TryGetValue(yAxis, out GorgonGamingDeviceAxisInfo? yAxisInfo)))
        {
            return GorgonPoint.Zero;
        }

        int deadZone = deviceXAxis.DeadZone.Max(deviceYAxis.DeadZone);

        if (deadZone <= 0)
        {
            return new GorgonPoint(deviceXAxis.Value, deviceYAxis.Value);
        }

        Vector2 position = new(deviceXAxis.Value, deviceYAxis.Value);
        float maxRange = xAxisInfo.Range.Range.Max(yAxisInfo.Range.Range) * 0.5f;

        float dist = (position.X * position.X + position.Y * position.Y).Sqrt();
        float wanted = (ConstrainLinear((int)dist.FastFloor(), deadZone) / (maxRange - deadZone)).Min(1);
        float scale = wanted > 0 ? wanted / dist : 0;
        float x = (position.X * scale) * maxRange;
        float y = (position.Y * scale) * maxRange;

        return new GorgonPoint((int)x, (int)y);
    }

    /// <inheritdoc/>
    public int ConstrainLinearScaled(GamingDeviceAxis axis)
    {
        if ((!Axes.TryGetValue(axis, out IGorgonGamingDeviceAxis? deviceAxis))
            || (!Info.AxisInfo.TryGetValue(axis, out GorgonGamingDeviceAxisInfo? axisInfo)))
        {
            return 0;
        }

        if (deviceAxis.DeadZone <= 0)
        {
            return deviceAxis.Value;
        }

        int newValue = ConstrainLinear(deviceAxis.Value, deviceAxis.DeadZone);
        int axisRange = axis switch
        {
            GamingDeviceAxis.LeftTrigger or GamingDeviceAxis.RightTrigger => IsXInputDevice ? axisInfo.Range.Range : axisInfo.Range.Range / 2,
            _ => (axisInfo.Range.Range / 2)
        };

        int maxRange = axisRange - deviceAxis.DeadZone;

        if (newValue == 0)
        {
            return 0;
        }

        float normalizedValue = (float)newValue / maxRange;

        return (int)(normalizedValue * axisRange);
    }

    /// <inheritdoc/>
    public int GetVibration(int motorIndex)
    {
        if ((Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
        {
            return 0;
        }

        // If we've not determined the slot for the device yet, then try to do so now.
        Info.RefreshXInputSlot();

        if (Info.XInputSlot == -1)
        {
            return 0;
        }

        if ((motorIndex < 0) || (motorIndex >= _motorValues.Length))
        {
            return 0;
        }

        return _motorValues[motorIndex];
    }

    /// <inheritdoc/>
    public void SetVibration(int motorIndex, int value)
    {
        if ((Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
        {
            return;
        }

        // If we've not determined the slot for the device yet, then try to do so now.
        Info.RefreshXInputSlot();

        if (Info.XInputSlot == -1)
        {
            return;
        }

        if ((motorIndex < 0) || (motorIndex >= _motorValues.Length))
        {
            return;
        }

        GorgonRange<int> motorRange = Info.VibrationMotorRanges[motorIndex];

        value = value.Min(motorRange.Maximum).Max(motorRange.Minimum);
        _motorValues[motorIndex] = value;

        if (!XInputApi.SetVibrationMotor(Info.XInputSlot, _motorValues[0], _motorValues.Length > 0 ? _motorValues[1] : 0))
        {
            _motorValues[motorIndex] = 0;
        }
    }

    /// <inheritdoc/>
    public GamingDeviceBatteryLevel GetBatteryLevel()
    {
        if ((Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) != GamingDeviceCapabilityFlags.IsXInputDevice)
        {
            return GamingDeviceBatteryLevel.NotSupported;
        }

        // If we've not determined the slot for the device yet, then try to do so now.
        Info.RefreshXInputSlot();

        if (Info.XInputSlot == -1)
        {
            return GamingDeviceBatteryLevel.Disconnected;
        }

        return XInputApi.GetBatteryLevel(Info.XInputSlot);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        for (int i = 0; i < _motorValues.Length; i++)
        {
            SetVibration(i, 0);
        }

        foreach (IGorgonGamingDeviceAxis axis in Axes)
        {
            axis.Value = Info.AxisInfo[axis.Axis].DefaultValue;
        }

        Array.Clear(_povs, 0, _povs.Length);
        Array.Clear(_povDirections, 0, _povs.Length);
        Array.Clear(_buttons, 0, _buttons.Length);
    }

    /// <inheritdoc/>
    public bool ParseData(GorgonInputEvent inputEvent)
    {
        if (Info.Handle != inputEvent.DeviceHandle)
        {
            return false;
        }

        if (inputEvent.DeviceType != InputDeviceType.GamingDevice)
        {
            Reset();
            return false;
        }

        // If the device is an XInput device, and we've not determined the slot for the device, then it was probably disconnected before we got here.
        // If that's the case, then try to get the slot again, and if that fails, move on.
        if (((Info.Capabilities & GamingDeviceCapabilityFlags.IsXInputDevice) == GamingDeviceCapabilityFlags.IsXInputDevice) && (Info.XInputSlot == -1))
        {
            Reset();
            Info.RefreshXInputSlot();

            if (Info.XInputSlot == -1)
            {
                return false;
            }
        }

        ArrayPool<HIDP_DATA> pool = GorgonArrayPools<HIDP_DATA>.GetBestPool(inputEvent.HidDataLength);
        HIDP_DATA[] reportItemsBuffer = pool.Rent(inputEvent.HidDataLength);
        Span<HIDP_DATA> reportItems = reportItemsBuffer.AsSpan(0, inputEvent.HidDataLength);

        try
        {
            WIN32_ERROR error = HidApi.ReadDeviceData(inputEvent.DeviceHandle, inputEvent.PreparsedData, inputEvent.RawInputHidReport, ref reportItems);

            if (error == WIN32_ERROR.ERROR_DEVICE_NOT_CONNECTED)
            {
                Reset();
                return false;
            }

            if (error != WIN32_ERROR.ERROR_SUCCESS)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
            }

            // Capture button state.                
            for (int b = 0; b < _buttons.Length; ++b)
            {
                int index = GetHIDDataIndex(inputEvent.HidButtonIndices[b], reportItems);

                _buttons[b] = index != -1 ? reportItems[index].Anonymous.On : false;
            }

            // Capture POV state.
            for (int p = 0; p < _povs.Length; ++p)
            {
                PovMap map = inputEvent.HidHatSwitchIndices[p];

                int index = GetHIDDataIndex(map.Index, reportItems);

                (_povDirections[p], _povs[p]) = index > -1 ? GetPOV((int)reportItems[index].Anonymous.RawValue, map) : (POVDirection.Center, -1);
            }

            // Capture axis state.
            for (int a = 0; a < Axes.Count; ++a)
            {
                IGorgonGamingDeviceAxis axis = Axes[a];
                GorgonGamingDeviceAxisInfo axisInfo = Info.AxisInfo[axis.Axis];

                // Anything above 127 is a fake axis (e.g. XBox controller triggers), so says I.
                if (axisInfo.AxisIndex > 0x7f)
                {
                    continue;
                }

                int index = GetHIDDataIndex(axisInfo.AxisIndex, reportItems);

                if (index == -1)
                {
                    continue;
                }

                if ((IsXInputDevice) && (axis.Axis == GamingDeviceAxis.ZAxis))
                {
                    MapXInputTriggers();
                }

                axis.Value = (int)reportItems[index].Anonymous.RawValue + axisInfo.Range.Minimum;
            }

            return true;
        }
        finally
        {
            pool.Return(reportItemsBuffer);
        }
    }

    /// <inheritdoc/>
    public bool ParseData(GorgonInputEventBuffer inputEvents)
    {
        if (inputEvents.GamingDeviceEventCount == 0)
        {
            return false;
        }

        for (int i = 0; i < inputEvents.GamingDeviceEventCount; ++i)
        {
            ParseData(inputEvents.GetGamingDeviceEvent(i));
        }

        return true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGamingDevice"/> class.
    /// </summary>
    /// <param name="info">The info for the gaming device to monitor.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="info"/> is required because there is no aggregate device for a joystick/gamepad due to the wide variety of functionality available for those devices. The parameter is used to 
    /// filter the data supplied to the device we've passed in so that no data from other devices gets intermixed with this one.
    /// </para>
    /// <para>
    /// Deadzones are automatically created for the most used axes on the device. If the object is recreated, and the user has previously supplied dead zone information for an axis, then that deadzone value 
    /// must be set again.
    /// </para>
    /// <para>
    /// This object will have to be recreated if the <see cref="IGorgonInput.Enable"/> method is called.
    /// </para>
    /// </remarks>
    public GorgonGamingDevice(IGorgonGamingDeviceInfo info)
    {
        Info = info;
        _buttons = new bool[info.ButtonCount];
        _povs = new int[info.PovCount];
        _povDirections = new POVDirection[info.PovCount];
        if (IsXInputDevice)
        {
            _motorValues = new int[info.VibrationMotorRanges.Count];
        }
        Axes = new GorgonGamingDeviceAxisList(info.AxisInfo.Values.Select(item =>
        {
            GamingDeviceAxisProperties result = new(item);

            if (!IsXInputDevice)
            {
                result.DeadZone = item.Axis switch
                {
                    GamingDeviceAxis.XAxis or GamingDeviceAxis.YAxis or GamingDeviceAxis.ZAxis => (int)((item.Range.Range * 0.5) * 0.24),
                    GamingDeviceAxis.RotationX or GamingDeviceAxis.RotationY or GamingDeviceAxis.RotationZ => (int)((item.Range.Range * 0.5) * 0.27),
                    _ => 0
                };

                return result;
            }

            result.DeadZone = item.Axis switch
            {
                GamingDeviceAxis.LeftTrigger or GamingDeviceAxis.RightTrigger => XInputGamepadTriggerThreshold,
                GamingDeviceAxis.XAxis or GamingDeviceAxis.YAxis => XInputGamepadLeftThumbDeadZone,
                GamingDeviceAxis.RotationX or GamingDeviceAxis.RotationY => XInputGamepadRightThumbDeadZone,
                _ => 0
            };

            return result;
        }));
    }
}
