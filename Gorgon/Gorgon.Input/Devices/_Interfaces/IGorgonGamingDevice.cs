
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
// Created: Wednesday, September 16, 2015 11:20:25 PM
// 

using Gorgon.Graphics;

namespace Gorgon.Input.Devices;

/// <summary>
/// Enumeration for point-of-view hat directions
/// </summary>
[Flags]
public enum POVDirection
{
    /// <summary>
    /// Axis is centered.
    /// </summary>
    Center = 0,
    /// <summary>
    /// Axis is up.
    /// </summary>
    Up = 1,
    /// <summary>
    /// Axis is down.
    /// </summary>
    Down = 2,
    /// <summary>
    /// Axis is left.
    /// </summary>
    Left = 4,
    /// <summary>
    /// Axis is right.
    /// </summary>
    Right = 8
}

/// <summary>
/// The battery level state for a gaming device.
/// </summary>
public enum GamingDeviceBatteryLevel
{
    /// <summary>
    /// The device was disconnected.
    /// </summary>
    Disconnected = 0,
    /// <summary>
    /// There was an error reading the battery level.
    /// </summary>
    Error = 1,
    /// <summary>
    /// Device is a wired device and the battery is not used or does not exist.
    /// </summary>
    Wired = 2,
    /// <summary>
    /// The device type is not supported.
    /// </summary>
    NotSupported = 3,
    /// <summary>
    /// The battery has no charge.
    /// </summary>
    Empty = 4,
    /// <summary>
    /// The battery has a low charge.
    /// </summary>
    Low = 5,
    /// <summary>
    /// The battery has a medium charge.
    /// </summary>
    Medium = 6,
    /// <summary>
    /// The battery is fully charged.
    /// </summary>
    Full = 7,
    /// <summary>
    /// Unknown value.
    /// </summary>
    Unknown = 8
}

/// <summary>
/// A data structure that represents the state of a joystick or gamepad.
/// </summary>
public interface IGorgonGamingDevice
{
    /// <summary>
    /// Property to return information about the gaming device.
    /// </summary>
    IGorgonGamingDeviceInfo Info
    {
        get;
    }

    /// <summary>
    /// Property to return the list of axes for the gaming device.
    /// </summary>
    GorgonGamingDeviceAxisList Axes
    {
        get;
    }

    /// <summary>
    /// Property to return the Point of View hat value.
    /// </summary>
    /// <remarks>
    /// This returns a read only span with the point of view hat value for a specific point of view hat which is accessed by an index. This value is measured in degrees * 100 (e.g. 45 degrees is 4500, 180 degrees is 18000, etc...)
    /// </remarks>
    ReadOnlySpan<int> Povs
    {
        get;
    }

    /// <summary>
    /// Property to return the Point of View hat direction.
    /// </summary>
    /// <remarks>
    /// This returns a read only span with the point of view hat direction for a specific point of view hat which is accessed by an index. 
    /// </remarks>
    ReadOnlySpan<POVDirection> PovDirections
    {
        get;
    }

    /// <summary>
    /// Property to return whether a button is pressed or not.
    /// </summary>
    /// <remarks>
    /// This returns <b>true</b> when a button at a specified index was pressed.
    /// </remarks>
    ReadOnlySpan<bool> Buttons
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the axis value, constrained to the value outside of the dead zone.
    /// </summary>
    /// <param name="axis">The axis to evaluate.</param>
    /// <returns>The constrained value for the axis.</returns>
    /// <remarks>
    /// <para>
    /// This will return the value for the specified <paramref name="axis"/>, but constrained to only return a value when outside of the <see cref="IGorgonGamingDeviceAxis.DeadZone"/> value.
    /// </para>
    /// <para>
    /// If the <see cref="IGorgonGamingDeviceAxis.DeadZone"/> value is 0 or less, then the value for the axis is returned, unchanged.
    /// </para>
    /// <para>
    /// This method will scale the value the so that the values will still fall within the range of the axis as described in <see cref="IGorgonGamingDeviceInfo.AxisInfo"/>. For example, if the dead zone 
    /// was set to 256 and the device has a max axis range of 1024, then up until the joystick reaches 256, it will return 0, and when pushed to its maximum range, it will still return 1024.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonGamingDeviceAxis"/>
    int ConstrainLinearScaled(GamingDeviceAxis axis);

    /// <summary>
    /// Function to retrieve the axis value, constrained to the value outside of the dead zone.
    /// </summary>
    /// <param name="axisValue">The value for the axis.</param>
    /// <param name="deadZone">The dead zone for the axis.</param>
    /// <returns>The constrained value for the axis.</returns>
    /// <remarks>
    /// <para>
    /// This will return the value, but constrained to only return a value when outside of the <paramref name="deadZone"/> value.
    /// </para>
    /// <para>
    /// This method does not apply scaling. To get a linear constrained value with scaling to the device axis range, use the <see cref="ConstrainLinearScaled(GamingDeviceAxis)"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="ConstrainLinearScaled(GamingDeviceAxis)"/>
    int ConstrainLinear(int axisValue, int deadZone);

    /// <summary>
    /// Function to retrieve the x and y axis values, constrained to the value outside of the dead zone.
    /// </summary>
    /// <param name="xAxis">The x axis to retrieve.</param>
    /// <param name="yAxis">The y axis to retrieve.</param>
    /// <returns>The constrained values for the axes.</returns>
    /// <remarks>
    /// <para>
    /// This will return the values for the specified axes, but constrained to only return a value when outside of the <see cref="IGorgonGamingDeviceAxis.DeadZone"/> value. This method uses a 
    /// circular magnitude algorithm (implemented by Chuck Walburn https://github.com/microsoft/DirectXTK/blob/main/Src/GamePad.cpp) to provide a more natural falloff for the dead zone area.
    /// </para>
    /// <para>
    /// This method will scale the values the so that the values will still fall within the range of the axis as described in <see cref="IGorgonGamingDeviceInfo.AxisInfo"/>. For example, if the dead zone 
    /// was set to 256 and the device has a max axis range of 1024, then up until the joystick reaches 256, it will return 0, and when pushed to its maximum range, it will still return 1024.
    /// </para>
    /// </remarks>
    GorgonPoint ConstrainCircular(GamingDeviceAxis xAxis, GamingDeviceAxis yAxis);

    /// <summary>
    /// Function to retrieve the battery level for the gaming device.
    /// </summary>
    /// <returns>The <see cref="GamingDeviceBatteryLevel"/> information for the device.</returns>
    /// <remarks>
    /// <para>
    /// This method will only return information for devices that are XInput compatible at this time and the return value will be <see cref="GamingDeviceBatteryLevel.NotSupported"/> for non XInput devices. 
    /// To determine if the device is XInput compatible, check the <see cref="IGorgonGamingDeviceInfo.Capabilities"/> on the <see cref="Info"/> property. 
    /// </para>
    /// </remarks>
    /// <seealso cref="Info"/>
    /// <seealso cref="GamingDeviceCapabilityFlags"/>
    GamingDeviceBatteryLevel GetBatteryLevel();

    /// <summary>
    /// Function to return the current vibration value for the motors on the device.
    /// </summary>
    /// <param name="motorIndex">The index of the motor.</param>
    /// <returns>The current vibration motor value.</returns>
    /// <remarks>
    /// <para>
    /// This will return the current vibration motor value for the motor at the <paramref name="motorIndex"/>. This value will be a number from 0 (stopped) to a maximum value on the 
    /// <see cref="IGorgonGamingDeviceInfo.VibrationMotorRanges"/> value on the <see cref="Info"/> property.
    /// </para>
    /// <para>
    /// This method will only return a value for devices that are XInput compatible at this time and the return value will be 0 for non XInput devices. To determine if the device is XInput compatible, check 
    /// the <see cref="IGorgonGamingDeviceInfo.Capabilities"/> on the <see cref="Info"/> property. 
    /// </para>
    /// </remarks>
    /// <seealso cref="Info"/>
    int GetVibration(int motorIndex);

    /// <summary>
    /// Function to send a value to the vibration motors on the device.
    /// </summary>
    /// <param name="motorIndex">The index of the motor.</param>
    /// <param name="value">The value to send.</param>
    /// <remarks>
    /// <para>
    /// This will start a vibration motor by sending a <paramref name="value"/> that is greater than 0, or stop it by sending 0. The <paramref name="value"/> should be a number between 0 and the maximum 
    /// value on the <see cref="IGorgonGamingDeviceInfo.VibrationMotorRanges"/> value on the <see cref="Info"/> property.
    /// </para>
    /// <para>
    /// This method will only set the value for devices that are XInput compatible at this time and will do nothing for non XInput devices. To determine if the device is XInput compatible, check the 
    /// <see cref="IGorgonGamingDeviceInfo.Capabilities"/> on the <see cref="Info"/> property. 
    /// </para>
    /// </remarks>
    /// <seealso cref="Info"/>
    void SetVibration(int motorIndex, int value);

    /// <summary>
    /// Function to reset the data for the gaming device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should only be called when the application loses focus.
    /// </para>
    /// </remarks>
    void Reset();

    /// <summary>
    /// Function to parse the input events from the <see cref="GorgonInput.GetInput"/> method.
    /// </summary>
    /// <param name="inputEvent">The list of event containing the data to parse.</param>
    /// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Users can use this overload to parse a single event from the <see cref="GorgonInput.GetInput"/> method. Using the gaming device data this way allows 
    /// applications to process a series of gaming device input events in a custom, application-specific way. Since all <see cref="GorgonInputEvent"/> values include a <see cref="GorgonInputEvent.TimeStamp"/> value, 
    /// applications can sort the inputs in any way they see fit.
    /// </para>
    /// <para>
    /// If the <paramref name="inputEvent"/> is not a gaming device event, then this method will return <b>false</b>, and the data will be reset to default values.
    /// </para>
    /// <para>
    /// If the event is not for this device, then <b>false</b> will be returned, but the data will be not changed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInput.GetInput"/>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="ParseData(GorgonInputEventBuffer)"/>"/>    
    bool ParseData(GorgonInputEvent inputEvent);

    /// <summary>
    /// Function to parse the input events from the <see cref="GorgonInput.GetInput"/> method.
    /// </summary>
    /// <param name="inputEvents">The list of events containing the data to parse.</param>
    /// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="inputEvents"/> parameter is received by calling the <see cref="GorgonInput.GetInput"/> method. This will take the series of events 
    /// and build up to the most current event. All gaming device events will be processed by this method, so no data will be missed, and aggregrated to the current state. If the user requires custom processing 
    /// of the gaming device events, they can use the <see cref="ParseData(GorgonInputEvent?)"/> overload.
    /// </para>
    /// <para>
    /// If the <paramref name="inputEvents"/> is empty, then this method will return <b>false</b> and the data will be untouched.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonInput.GetInput"/>
    /// <seealso cref="GorgonInputEvent"/>
    /// <seealso cref="ParseData(GorgonInputEvent?)"/>
    /// <seealso cref="GorgonInputEventBuffer"/>
    bool ParseData(GorgonInputEventBuffer inputEvents);
}
