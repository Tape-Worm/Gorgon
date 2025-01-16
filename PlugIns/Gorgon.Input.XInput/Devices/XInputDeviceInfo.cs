
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
// Created: Sunday, July 5, 2015 3:54:34 PM
// 

using Gorgon.Core;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput;

/// <summary>
/// XInput controller information
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="XInputDeviceInfo"/> class
/// </remarks>
/// <param name="deviceDescription">The description for the game pad controller.</param>
/// <param name="id">The index ID of the device.</param>
internal class XInputDeviceInfo(string deviceDescription, XI.UserIndex id)
        : IGorgonGamingDeviceInfo
{
    /// <summary>
    /// Property to return the list of supported buttons for this controller.
    /// </summary>
    public Dictionary<XI.GamepadButtonFlags, int> SupportedButtons
    {
        get;
    } = new Dictionary<XI.GamepadButtonFlags, int>(new ButtonFlagsEqualityComparer());

    /// <summary>
    /// Property to return the number of buttons available on the gaming device.
    /// </summary>
    public int ButtonCount => SupportedButtons.Count;

    /// <summary>
    /// Property to return the ID for the manufacturer of the gaming device.
    /// </summary>
    public int ManufacturerID
    {
        get;
    } = 0;

    /// <summary>
    /// Property to return the ID of the product.
    /// </summary>
    public int ProductID
    {
        get;
    } = 0;

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
    public IReadOnlyList<GorgonRange<int>> VibrationMotorRanges
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
    /// Property to return a human readable description for the gaming device.
    /// </summary>
    public string Description
    {
        get;
    } = deviceDescription;

    /// <summary>
    /// Property to return the number of point of view controls on the gaming device.
    /// </summary>
    public int POVCount => 1;

    /// <summary>Property to return the unique ID for the device.</summary>
    public Guid DeviceID
    {
        get;
    } = id.ToGuid();

    /// <summary>
    /// Function to retrieve the capabilities of the xinput device.
    /// </summary>
    /// <param name="controller"></param>
    public void GetCaps(XI.Controller controller)
    {
        Capabilities = GamingDeviceCapabilityFlags.None;

        controller.GetCapabilities(XI.DeviceQueryType.Any, out XI.Capabilities capabilities);

        // Get vibration caps.
        List<GorgonRange<int>> vibrationRanges = [];

        if (capabilities.Vibration.LeftMotorSpeed != 0)
        {
            vibrationRanges.Add(new GorgonRange<int>(0, ushort.MaxValue));
            Capabilities |= GamingDeviceCapabilityFlags.SupportsVibration;
        }

        if (capabilities.Vibration.RightMotorSpeed != 0)
        {
            vibrationRanges.Add(new GorgonRange<int>(0, ushort.MaxValue));
            Capabilities |= GamingDeviceCapabilityFlags.SupportsVibration;
        }

        VibrationMotorRanges = vibrationRanges;

        if (((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown)
            || ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp)
            || ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft)
            || ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
        {
            Capabilities |= GamingDeviceCapabilityFlags.SupportsPOV;
        }

        // Get buttons, and remap to the button indices present in the gaming device control panel app.
        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.A) == XI.GamepadButtonFlags.A)
        {
            SupportedButtons[XI.GamepadButtonFlags.A] = 0;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.B) == XI.GamepadButtonFlags.B)
        {
            SupportedButtons[XI.GamepadButtonFlags.B] = 1;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.X) == XI.GamepadButtonFlags.X)
        {
            SupportedButtons[XI.GamepadButtonFlags.X] = 2;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.Y) == XI.GamepadButtonFlags.Y)
        {
            SupportedButtons[XI.GamepadButtonFlags.Y] = 3;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.LeftShoulder) == XI.GamepadButtonFlags.LeftShoulder)
        {
            SupportedButtons[XI.GamepadButtonFlags.LeftShoulder] = 4;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.RightShoulder) == XI.GamepadButtonFlags.RightShoulder)
        {
            SupportedButtons[XI.GamepadButtonFlags.RightShoulder] = 5;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.Back) == XI.GamepadButtonFlags.Back)
        {
            SupportedButtons[XI.GamepadButtonFlags.Back] = 6;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.Start) == XI.GamepadButtonFlags.Start)
        {
            SupportedButtons[XI.GamepadButtonFlags.Start] = 7;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.LeftThumb) == XI.GamepadButtonFlags.LeftThumb)
        {
            SupportedButtons[XI.GamepadButtonFlags.LeftThumb] = 8;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.RightThumb) == XI.GamepadButtonFlags.RightThumb)
        {
            SupportedButtons[XI.GamepadButtonFlags.RightThumb] = 9;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp)
        {
            SupportedButtons[XI.GamepadButtonFlags.DPadUp] = 10;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight)
        {
            SupportedButtons[XI.GamepadButtonFlags.DPadRight] = 11;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown)
        {
            SupportedButtons[XI.GamepadButtonFlags.DPadDown] = 12;
        }

        if ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft)
        {
            SupportedButtons[XI.GamepadButtonFlags.DPadLeft] = 13;
        }

        // Find out the ranges for each axis.
        Dictionary<GamingDeviceAxis, GorgonRange<int>> axes = [];

        if (capabilities.Gamepad.LeftThumbX != 0)
        {
            axes[GamingDeviceAxis.XAxis] = new GorgonRange<int>(short.MinValue, short.MaxValue);
        }

        if (capabilities.Gamepad.LeftThumbY != 0)
        {
            axes[GamingDeviceAxis.YAxis] = new GorgonRange<int>(short.MinValue, short.MaxValue);
        }

        if (capabilities.Gamepad.RightThumbX != 0)
        {
            axes[GamingDeviceAxis.XAxis2] = new GorgonRange<int>(short.MinValue, short.MaxValue);
            Capabilities |= GamingDeviceCapabilityFlags.SupportsSecondaryXAxis;
        }

        if (capabilities.Gamepad.RightThumbY != 0)
        {
            axes[GamingDeviceAxis.YAxis2] = new GorgonRange<int>(short.MinValue, short.MaxValue);
            Capabilities |= GamingDeviceCapabilityFlags.SupportsSecondaryYAxis;
        }

        if (capabilities.Gamepad.LeftTrigger != 0)
        {
            axes[GamingDeviceAxis.LeftTrigger] = new GorgonRange<int>(0, byte.MaxValue);
            Capabilities |= GamingDeviceCapabilityFlags.SupportsRudder;
        }

        if (capabilities.Gamepad.RightTrigger != 0)
        {
            axes[GamingDeviceAxis.RightTrigger] = new GorgonRange<int>(0, byte.MaxValue);
            Capabilities |= GamingDeviceCapabilityFlags.SupportsThrottle;
        }

        AxisInfo = axes.Select(item => new GorgonGamingDeviceAxisInfo(item.Key, item.Value, 0)).ToDictionary(k => k.Axis, v => v);
    }
}
