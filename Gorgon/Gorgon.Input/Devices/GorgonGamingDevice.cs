#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Friday, June 24, 2011 10:05:35 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Input.Properties;
using Gorgon.Math;

namespace Gorgon.Input
{
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
    public abstract class GorgonGamingDevice
        : IGorgonGamingDevice
    {
        #region Variables.
        // The POV of view control directions.
        private readonly POVDirection[] _povDirections;
        // Flag to indicate whether the gaming device has been acquired or not.
        private bool _isAcquired;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the current direction for the point-of-view hat.
        /// </summary>
        /// <remarks>
        /// If the gaming device does not support a point-of-view axis, then this value will always return <see cref="POVDirection.Center"/>.
        /// </remarks>
        public IReadOnlyList<POVDirection> POVDirection => _povDirections;

        /// <summary>
        /// Property to return information about the gaming device.
        /// </summary>
        public IGorgonGamingDeviceInfo Info
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
        public GorgonGamingDeviceAxisList<IGorgonGamingDeviceAxis> Axis
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
        public float[] POV
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
        public GamingDeviceButtonState[] Button
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
        public abstract bool IsConnected
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
        public bool IsAcquired
        {
            get => _isAcquired;
            set
            {
                if (_isAcquired == value)
                {
                    return;
                }

                _isAcquired = OnAcquire(value);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the direction flag for the POV.
        /// </summary>
        /// <param name="povIndex">The index of the point of view control to use.</param>
        private void UpdatePOVDirection(int povIndex)
        {
            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsPOV) != GamingDeviceCapabilityFlags.SupportsPOV)
            {
                return;
            }

            int pov = (int)((POV[povIndex] * 100.0f).Max(-1.0f));

            // Wrap POV if it's higher than 359.99 degrees.
            if (pov > 35999)
            {
                pov = -1;
            }

            // Get POV direction.
            if (pov != -1)
            {
                if ((pov < 18000) && (pov > 9000))
                {
                    _povDirections[povIndex] = Input.POVDirection.Down | Input.POVDirection.Right;
                }
                if ((pov > 18000) && (pov < 27000))
                {
                    _povDirections[povIndex] = Input.POVDirection.Down | Input.POVDirection.Left;
                }
                if ((pov > 27000) && (pov < 36000))
                {
                    _povDirections[povIndex] = Input.POVDirection.Up | Input.POVDirection.Left;
                }
                if ((pov > 0) && (pov < 9000))
                {
                    _povDirections[povIndex] = Input.POVDirection.Up | Input.POVDirection.Right;
                }
            }

            switch (pov)
            {
                case 18000:
                    _povDirections[povIndex] = Input.POVDirection.Down;
                    break;
                case 0:
                    _povDirections[povIndex] = Input.POVDirection.Up;
                    break;
                case 9000:
                    _povDirections[povIndex] = Input.POVDirection.Right;
                    break;
                case 27000:
                    _povDirections[povIndex] = Input.POVDirection.Left;
                    break;
                case -1:
                    _povDirections[povIndex] = Input.POVDirection.Center;
                    break;
            }
        }

        /// <summary>
        /// Function to acquire a gaming device.
        /// </summary>
        /// <param name="acquireState"><b>true</b> to acquire the device, <b>false</b> to unacquire it.</param>
        /// <returns><b>true</b> if the device was acquired successfully, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// Implementors of a <see cref="GorgonGamingDeviceDriver"/> should implement this on the concrete <see cref="GorgonGamingDevice"/>, even if the device does not use acquisition (in such a case, 
        /// always return <b>true</b> from this method).
        /// </para>
        /// <para>
        /// Some input providers, like Direct Input, have the concept of device acquisition. When a device is created it may not be usable until it has been acquired by the application. When a device is 
        /// acquired, it will be made available for use by the application. However, in some circumstances, another application may have exclusive control over the device, and as such, acquisition is not 
        /// possible. 
        /// </para>
        /// <para>
        /// A device may lose acquisition when the application goes into the background, and as such, the application will no longer receive information from the device. When this is the case, the 
        /// application should immediately check after it regains foreground focus to see whether the device is acquired or not. For a winforms application this can be achieved with the <see cref="Form.Activated"/> 
        /// event. When that happens, set this property to <b>true</b>.
        /// </para>
        /// <para>
        /// Note that some devices from other input providers (like XInput) will always be in an acquired state. Regardless, it is best to check this flag when the application becomes active.
        /// </para>
        /// </remarks>
        protected abstract bool OnAcquire(bool acquireState);

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
        /// <para>
        /// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in should ensure that devices that support vibration implement this method. Otherwise, if the device does not support the functionality 
        /// then this method can be left alone.
        /// </para>
        /// </remarks>
        protected virtual void OnVibrate(int motorIndex, int value)
        {
        }

        /// <summary>
        /// Function to retrieve data from the provider of the physical device.
        /// </summary>
        /// <remarks>
        /// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in must implement this and format their data to populate the values of this object with correct state information.
        /// </remarks>
        protected abstract void OnGetData();

        /// <summary>
        /// Function to reset the various gaming device axis, button and POV states to default values.
        /// </summary>
        public virtual void Reset()
        {
            for (int i = 0; i < _povDirections.Length; ++i)
            {
                POV[i] = -1.0f;
                _povDirections[i] = Input.POVDirection.Center;
            }

            foreach (GamingDeviceAxisProperties axis in Axis)
            {
                axis.Value = Info.AxisInfo[axis.Axis].DefaultValue;
            }

            for (int i = 0; i < Button.Length; ++i)
            {
                Button[i] = GamingDeviceButtonState.Up;
            }
        }

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
        public void Vibrate(int motorIndex, int value)
        {
            if (((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsVibration) != GamingDeviceCapabilityFlags.SupportsVibration)
                || (motorIndex < 0)
                || (motorIndex >= Info.VibrationMotorRanges.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(motorIndex), string.Format(Resources.GORINP_ERR_JOYSTICK_MOTOR_NOT_FOUND, motorIndex));
            }

            if (Info.VibrationMotorRanges[motorIndex].Contains(value))
            {
                OnVibrate(motorIndex, value);
            }
        }

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
        public void Poll()
        {
            if (!IsConnected)
            {
                return;
            }

            OnGetData();

            for (int i = 0; i < POV.Length; ++i)
            {
                UpdatePOVDirection(i);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some gaming devices use native resources to communicate with the physical device. Because of this, it is necessary to call this method to ensure those resources are freed.
        /// </para>
        /// <para>
        /// For implementors of a <see cref="GorgonGamingDeviceDriver"/>, this method should be overridden to free up any native resources required by the device. If the device does 
        /// not use any native resources, then it is safe to leave this method alone.
        /// </para>
        /// </remarks>
        public virtual void Dispose() => GC.SuppressFinalize(this);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGamingDevice"/> class.
        /// </summary>
        /// <param name="deviceInfo">Information about the specific gaming device to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="deviceInfo"/> parameter is <b>null</b>.</exception>
        protected GorgonGamingDevice(IGorgonGamingDeviceInfo deviceInfo)
        {
            if (deviceInfo == null)
            {
                throw new ArgumentNullException(nameof(deviceInfo));
            }

            _povDirections =
                new POVDirection[((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.SupportsPOV) == GamingDeviceCapabilityFlags.SupportsPOV) ? deviceInfo.POVCount : 0];
            POV = new float[_povDirections.Length];
            Button = new GamingDeviceButtonState[deviceInfo.ButtonCount];
            Info = deviceInfo;
            Axis = new GorgonGamingDeviceAxisList<IGorgonGamingDeviceAxis>(deviceInfo.AxisInfo.Select(item => new GamingDeviceAxisProperties(item.Value)));
        }
        #endregion
    }
}
