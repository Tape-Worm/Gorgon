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
// Created: Friday, July 15, 2011 6:22:41 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Core;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
    /// <summary>
    /// XInput XBOX 360 controller device.
    /// </summary>
    internal class XInputDevice
        : GorgonGamingDevice
    {
        #region Variables.
        // Last packet number.
        private int _lastPacket = int.MaxValue;
        // The XInput controller.
        private readonly XI.Controller _controller;
        // Current vibration values.
        private XI.Vibration _currentVibration;
        // The strongly typed device information data.
        private readonly XInputDeviceInfo _info;
        #endregion

        #region Properties.
        public override bool IsConnected => _controller.IsConnected;
        #endregion

        #region Methods.
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
        /// application should immediately check after it regains foreground focus to see whether the device is acquired or not. For a winforms application this can be achieved with the <see cref="System.Windows.Forms.Form.Activated"/> 
        /// event. When that happens, set this property to <b>true</b>.
        /// </para>
        /// <para>
        /// Note that some devices from other input providers (like XInput) will always be in an acquired state. Regardless, it is best to check this flag when the application becomes active.
        /// </para>
        /// </remarks>
        protected override bool OnAcquire(bool acquireState) => true;

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
        protected override void OnVibrate(int motorIndex, int value)
        {
            _currentVibration = new XI.Vibration
            {
                LeftMotorSpeed = motorIndex == 0 ? (ushort)value : _currentVibration.LeftMotorSpeed,
                RightMotorSpeed = motorIndex == 1 ? (ushort)value : _currentVibration.RightMotorSpeed
            };

            _controller.SetVibration(_currentVibration);
        }

        /// <summary>
        /// Function to update the values for the axes on the joystick.
        /// </summary>
        /// <param name="state">The current state of the controller.</param>
        private void UpdateAxisValues(XI.Gamepad state)
        {

            if (Axis.TryGetValue(GamingDeviceAxis.LeftStickX, out GorgonGamingDeviceAxis xAxis))
            {
                xAxis.Value = state.LeftThumbX;
            }

            if (Axis.TryGetValue(GamingDeviceAxis.LeftStickY, out GorgonGamingDeviceAxis yAxis))
            {
                yAxis.Value = state.LeftThumbY;
            }

            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsSecondaryXAxis) == GamingDeviceCapabilityFlags.SupportsSecondaryXAxis)
            {
                Axis[GamingDeviceAxis.RightStickX].Value = state.RightThumbX;
            }

            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsSecondaryYAxis) == GamingDeviceCapabilityFlags.SupportsSecondaryYAxis)
            {
                Axis[GamingDeviceAxis.RightStickY].Value = state.RightThumbY;
            }

            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsThrottle) == GamingDeviceCapabilityFlags.SupportsThrottle)
            {
                Axis[GamingDeviceAxis.RightTrigger].Value = state.RightTrigger;
            }

            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsRudder) == GamingDeviceCapabilityFlags.SupportsRudder)
            {
                Axis[GamingDeviceAxis.LeftTrigger].Value = state.LeftTrigger;
            }
        }

        /// <summary>
        /// Function to retrieve the current point of view hat state.
        /// </summary>
        /// <param name="state">The state of the d-pad (pov hat).</param>
        /// <returns>An angle, in degrees representing the direction of the d-pad.</returns>
        private static float GetPOVState(XI.GamepadButtonFlags state)
        {
            // Get POV, which is on the D-Pad and default to a center value.
            float pov = -1.0f;

            // Determine direction.
            if (((state & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown) &&
                ((state & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
            {
                pov = 135.0f;
            }

            if (((state & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown) &&
                ((state & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft))
            {
                pov = 225.0f;
            }

            if (((state & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp) &&
                ((state & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft))
            {
                pov = 315.0f;
            }

            if (((state & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp) &&
                ((state & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
            {
                pov = 45.0f;
            }

            switch (state)
            {
                case XI.GamepadButtonFlags.DPadDown:
                    pov = 180.0f;
                    break;
                case XI.GamepadButtonFlags.DPadUp:
                    pov = 0;
                    break;
                case XI.GamepadButtonFlags.DPadRight:
                    pov = 90.0f;
                    break;
                case XI.GamepadButtonFlags.DPadLeft:
                    pov = 270.0f;
                    break;
            }

            return pov;
        }

        /// <summary>
        /// Function to retrieve data from the provider of the physical device.
        /// </summary>
        /// <remarks>
        /// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in must implement this and format their data to populate the values of this object with correct state information.
        /// </remarks>
        protected override void OnGetData()
        {

            if ((!_controller.GetState(out XI.State state)) || (_lastPacket == state.PacketNumber))
            {
                return;
            }

            _lastPacket = state.PacketNumber;

            // Retrieve button state.
            foreach (KeyValuePair<XI.GamepadButtonFlags, int> button in _info.SupportedButtons)
            {
                Button[button.Value] = (state.Gamepad.Buttons & button.Key) == button.Key ? GamingDeviceButtonState.Down : GamingDeviceButtonState.Up;
            }

            // Get point of view from the D-Pad.
            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsPOV) == GamingDeviceCapabilityFlags.SupportsPOV)
            {
                POV[0] = GetPOVState(state.Gamepad.Buttons);
            }

            UpdateAxisValues(state.Gamepad);
        }

        /// <summary>
        /// Function to reset the various gaming device axis, button and POV states to default values.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            _lastPacket = int.MaxValue;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="XInputDevice" /> class.
        /// </summary>
        /// <param name="deviceInfo">The device information.</param>
        public XInputDevice(XInputDeviceInfo deviceInfo)
                    : base(deviceInfo)
        {
            // XInput devices don't lose acquisition when the application loses focus.
            IsAcquired = true;
            _info = deviceInfo;
            _controller = new XI.Controller(deviceInfo.ID);


            if (Axis.TryGetValue(GamingDeviceAxis.XAxis, out GorgonGamingDeviceAxis _))
            {
                Axis[GamingDeviceAxis.XAxis].DeadZone = new GorgonRange(-XI.Gamepad.LeftThumbDeadZone, XI.Gamepad.LeftThumbDeadZone);
            }

            if (Axis.TryGetValue(GamingDeviceAxis.YAxis, out GorgonGamingDeviceAxis _))
            {
                Axis[GamingDeviceAxis.YAxis].DeadZone = new GorgonRange(-XI.Gamepad.LeftThumbDeadZone, XI.Gamepad.LeftThumbDeadZone);
            }

            if ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.SupportsSecondaryXAxis) == GamingDeviceCapabilityFlags.SupportsSecondaryXAxis)
            {
                Axis[GamingDeviceAxis.XAxis2].DeadZone = new GorgonRange(-XI.Gamepad.RightThumbDeadZone, XI.Gamepad.RightThumbDeadZone);
            }

            if ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.SupportsSecondaryYAxis) == GamingDeviceCapabilityFlags.SupportsSecondaryYAxis)
            {
                Axis[GamingDeviceAxis.YAxis2].DeadZone = new GorgonRange(-XI.Gamepad.RightThumbDeadZone, XI.Gamepad.RightThumbDeadZone);
            }

            if ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.SupportsThrottle) == GamingDeviceCapabilityFlags.SupportsThrottle)
            {
                Axis[GamingDeviceAxis.RightTrigger].DeadZone = new GorgonRange(0, XI.Gamepad.TriggerThreshold);
            }

            if ((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.SupportsRudder) == GamingDeviceCapabilityFlags.SupportsRudder)
            {
                Axis[GamingDeviceAxis.LeftTrigger].DeadZone = new GorgonRange(0, XI.Gamepad.TriggerThreshold);
            }
        }
        #endregion
    }
}
