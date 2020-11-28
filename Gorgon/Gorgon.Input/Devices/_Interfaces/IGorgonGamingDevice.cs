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
// Created: Wednesday, September 16, 2015 11:20:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gorgon.Input
{
    /// <summary>
    /// Enumeration for point-of-view hat directions.
    /// </summary>
    [Flags]
    public enum POVDirection
    {
        /// <summary>
        /// Axis is centered.
        /// </summary>
        Center = 1,
        /// <summary>
        /// Axis is up.
        /// </summary>
        Up = 2,
        /// <summary>
        /// Axis is down.
        /// </summary>
        Down = 4,
        /// <summary>
        /// Axis is left.
        /// </summary>
        Left = 8,
        /// <summary>
        /// Axis is right.
        /// </summary>
        Right = 16
    }

    /// <summary>
    /// The state of a gaming device button.
    /// </summary>
    public enum GamingDeviceButtonState
    {
        /// <summary>
        /// The gaming device button is not pressed.
        /// </summary>
        Up = 0,
        /// <summary>
        /// The gaming device button is pressed.
        /// </summary>
        Down = 1
    }

    /// <summary>
    /// Provides state for gaming device data from a physical gaming device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gaming devices (such as a joystick or game pad) and provided via a driver system using the <see cref="GorgonGamingDeviceDriver"/> object. These drivers may be loaded via a plug in interface through the 
    /// <see cref="GorgonGamingDeviceDriverFactory"/> object. Once a driver is loaded, it can be used to create an object of this type.
    /// </para>
    /// <para>
    /// The gaming devices can support a variable number of axes, and this is reflected within the <see cref="Axis"/> property to make available only those axes which are supported by the device. For example, 
    /// if the device supports a single horizontal axis and that axis is mapped to the secondary axis (<see cref="GamingDeviceAxis.XAxis2"/>), then the <see cref="Axis"/> collection will only contain a member 
    /// for the <see cref="GamingDeviceAxis.XAxis2"/> value.  Likewise, if it supports many axes, then all of those axes will be made available. To determine which axes are supported, use the <see cref="GorgonGamingDeviceAxisList{T}.Contains"/>
    /// method on the <see cref="Info"/>.<see cref="IGorgonGamingDeviceInfo.AxisInfo"/> property or the on the <see cref="Axis"/> property.
    /// </para>
    /// <para>
    /// <see cref="GorgonGamingDevice"/> objects require that the device be polled via a call to the <see cref="Poll"/> method. This will capture the latest state from the physical device and store it within 
    /// this type for use by an application.
    /// </para>
    /// <para>
    /// Some gaming devices (such as XInput controllers) offer special functionality like vibration. This object supports sending data to the physical device to activate the special functionality. If the device 
    /// does not support the functionality, an exception is typically thrown. Use the <see cref="IGorgonGamingDeviceInfo.Capabilities"/> property to determine if these special functions are supported by the 
    /// device before calling.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in must inherit this type in order to expose functionality from a native provider (e.g. XInput).
    /// </para>
    /// </remarks>
    public interface IGorgonGamingDevice
        : IDisposable
    {
        /// <summary>
        /// Property to return the current direction for the point-of-view hat.
        /// </summary>
        /// <remarks>
        /// If the gaming device does not support a point-of-view axis, then this value will always return <see cref="POVDirection.Center"/>.
        /// </remarks>
        IReadOnlyList<POVDirection> POVDirection
        {
            get;
        }

        /// <summary>
        /// Property to return information about the gaming device.
        /// </summary>
        IGorgonGamingDeviceInfo Info
        {
            get;
        }

        /// <summary>
        /// Property to return the list of axes available to this gaming device.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is used to return the current position and dead zone for a given axis. 
        /// </para>
        /// <para>
        /// <see cref="GorgonGamingDeviceDriver"/> plug in implementors must set this value when device data is retrieved.
        /// </para>
        /// </remarks>
        /// <example>
        /// This example shows how to use this property to get the current gaming device X axis position:
        /// <code language="csharp">
        /// <![CDATA[
        /// IGorgonGamingDevice _device;
        /// int _currentXPosition;
        /// 
        /// void SetupDevices()
        /// {
        ///     // Do set up in here to retrieve a value for _device.
        /// }
        /// 
        /// void UpdateJoystickPosition()
        /// {
        ///		_device.Poll();
        ///		
        ///		_currentXPosition = _device.Axis[GamingDeviceAxis.XAxis].Value;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        GorgonGamingDeviceAxisList<GorgonGamingDeviceAxis> Axis
        {
            get;
        }

        /// <summary>
        /// Property to return the point of view value for discrete or continuous bearings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return a <see cref="float"/> value of -1.0f for center, or 0 to 359.9999f to indicate the direction, in degrees, of the POV hat.
        /// </para>
        /// <para>
        /// <see cref="GorgonGamingDeviceDriver"/> plug in implementors must set this value when device data is retrieved.
        /// </para>
        /// </remarks>
        float[] POV
        {
            get;
        }

        /// <summary>
        /// Property to return a list of button states.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return a list of the available buttons on the gaming device and their corresponding state represented by a <see cref="GamingDeviceButtonState"/> enumeration.
        /// </para>
        /// <para>
        /// <see cref="GorgonGamingDeviceDriver"/> plug in implementors must set this value when device data is retrieved.
        /// </para>
        /// </remarks>
        GamingDeviceButtonState[] Button
        {
            get;
        }

        /// <summary>
        /// Property to return whether the gaming device is connected or not.
        /// </summary> 
        /// <remarks>
        /// <para>
        /// Gaming devices may be registered with the system, and appear in the enumeration list provided by <see cref="GorgonGamingDeviceDriver.EnumerateGamingDevices"/>, but they may not be physically connected 
        /// to the system at the time of enumeration. Thus, we have this property to ensure that we know when a gaming device is connected to the system or not. 
        /// </para>
        /// <para>
        /// <see cref="GorgonGamingDeviceDriver"/> plug in implementors must ensure that this property will update itself when a gaming device is connected or disconnected.
        /// </para>
        /// </remarks>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the device is acquired or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set this value to <b>true</b> to acquire the device for the application, or <b>false</b> to unacquire it.
        /// </para>
        /// <para>
        /// Some input providers, like Direct Input, have the concept of device acquisition. When a device is created it may not be usable until it has been acquired by the application. When a device is 
        /// acquired, it will be made available for use by the application. However, in some circumstances, another application may have exclusive control over the device, and as such, acquisition is not 
        /// possible. 
        /// </para>
        /// <para>
        /// A device may lose acquisition when the application goes into the background, and as such, the application will no longer receive information from the device. When this is the case, the 
        /// application should immediately set this value to <b>false</b> during deactivation (during the WinForms <see cref="Form.Deactivate"/>, or WPF <see cref="E:System.Windows.Window.Deactivated"/> events). 
        /// When the application is activated (during the WinForms <see cref="Form.Activated"/>, or WPF <see cref="E:System.Windows.Window.Activated"/> events), it should set this value to <b>true</b> in order to 
        /// start capturing data again. 
        /// </para>
        /// <para>
        /// Note that some devices from other input providers (like XInput) will always be in an acquired state. Regardless, it is best to update this flag when the application becomes active or inactive.
        /// </para>
        /// </remarks>
        bool IsAcquired
        {
            get;
            set;
        }

        /// <summary>
        /// Function to reset the various gaming device axis, button and POV states to default values.
        /// </summary>
        void Reset();

        /// <summary>
        /// Function to perform vibration on the gaming device, if supported.
        /// </summary>
        /// <param name="motorIndex">The index of the motor to start or stop.</param>
        /// <param name="value">The speed of the motor.</param>
        /// <remarks>
        /// <para>
        /// This will activate the vibration motor(s) in the gaming device.  The <paramref name="motorIndex"/> should be within the <see cref="IGorgonGamingDeviceInfo.VibrationMotorRanges"/> count, or else 
        /// an exception will be thrown. 
        /// </para>
        /// <para>
        /// To determine if the device supports vibration, check the <see cref="IGorgonGamingDeviceInfo.Capabilities"/> property for the <see cref="GamingDeviceCapabilityFlags.SupportsVibration"/> flag.
        /// </para>
        /// </remarks>
        void Vibrate(int motorIndex, int value);

        /// <summary>
        /// Function to read the gaming device state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to capture the current state of the gaming device. When the state is captured, its data is propagated to the properties for this object.
        /// </para>
        /// <para>
        /// If this method is not called before checking state, that state will be invalid and/or out of date. Ensure that this method is called frequently to capture the most current state of the physical 
        /// device.
        /// </para>
        /// </remarks>
        void Poll();
    }
}