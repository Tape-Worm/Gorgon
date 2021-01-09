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
// Created: Sunday, September 13, 2015 8:59:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Input.DirectInput.Properties;
using Gorgon.UI;
using DX = SharpDX;
using DI = SharpDX.DirectInput;

namespace Gorgon.Input.DirectInput
{
    /// <summary>
    /// DirectInput gaming device.
    /// </summary>
    internal class DirectInputDevice
        : GorgonGamingDevice
    {
        #region Variables.
        // The direct input joystick interface.
        private Lazy<DI.Joystick> _joystick;
        // Direct input device.
        private readonly DI.DirectInput _directInput;
        // Device information.
        private readonly IGorgonGamingDeviceInfo _info;
        // Device state.
        private DI.JoystickState _state = new DI.JoystickState();
        // The axis to Direct Input property mappings.
        private readonly IReadOnlyDictionary<GamingDeviceAxis, DI.DeviceObjectId> _axisMappings;
        #endregion

        #region Properties.
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
        public override bool IsConnected => _directInput.IsDeviceAttached(_info.DeviceID);
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
        /// application should immediately check after it regains foreground focus to see whether the device is acquired or not. For a winforms application this can be achieved with the <see cref="Form.Activated"/> 
        /// event. When that happens, set this property to <b>true</b>.
        /// </para>
        /// <para>
        /// Note that some devices from other input providers (like XInput) will always be in an acquired state. Regardless, it is best to check this flag when the application becomes active.
        /// </para>
        /// </remarks>
        protected override bool OnAcquire(bool acquireState)
        {
            try
            {
                if (acquireState)
                {
                    _joystick.Value.Acquire();
                }
                else
                {
                    _joystick.Value.Unacquire();
                }

                return acquireState;
            }
            catch (DX.SharpDXException)
            {
                // If we fail acquisition, then an exception is typically thrown.
                // Just handle it here and tell the user that we don't have acquisition.
                return false;
            }
        }

        /// <summary>
        /// Function to perform the creation of the DirectInput joystick object.
        /// </summary>
        /// <param name="directInput">The direct input interface.</param>
        /// <param name="deviceInfo">The device information for the gaming device to use.</param>
        /// <returns>The DirectInput joystick object.</returns>
        private DI.Joystick CreateJoystick(DI.DirectInput directInput, IGorgonGamingDeviceInfo deviceInfo)
        {
            var result = new DI.Joystick(directInput, deviceInfo.DeviceID);

            IntPtr mainWindow = FindMainApplicationWindow();

            if (mainWindow == IntPtr.Zero)
            {
                // We have no main window, and DI requires one, so we need to kill this.
                throw new InvalidOperationException(Resources.GORINP_ERR_DI_COULD_NOT_FIND_APP_WINDOW);
            }

            result.SetCooperativeLevel(mainWindow, DI.CooperativeLevel.Foreground | DI.CooperativeLevel.NonExclusive);

            result.Properties.AxisMode = DI.DeviceAxisMode.Absolute;

            // Set up dead zones.
            foreach (IGorgonGamingDeviceAxis axis in Axis)
            {
                // Skip the throttle.  Dead zones on the throttle don't work too well for regular joysticks.
                // Game pads may be another story, but the user can manage those settings if required.
                if (axis.Axis == GamingDeviceAxis.Throttle)
                {
                    continue;
                }

                GorgonGamingDeviceAxisInfo info = Info.AxisInfo[axis.Axis];
                DI.ObjectProperties properties = result.GetObjectPropertiesById(_axisMappings[axis.Axis]);

                if (properties == null)
                {
                    continue;
                }

                // Set a 0 dead zone.
                properties.DeadZone = 0;

                // Make the dead zone 10% of the range for all axes.
                float deadZone = axis.Axis == GamingDeviceAxis.Throttle ? 0.02f : 0.10f;
                int deadZoneActual = (int)(info.Range.Range * deadZone);

                axis.DeadZone = new GorgonRange(info.DefaultValue - deadZoneActual, info.DefaultValue + deadZoneActual);
            }

            return result;
        }

        /// <summary>
        /// Function to attempt to locate the main application window handle.
        /// </summary>
        /// <returns>The window handle, if found. Or <see cref="IntPtr.Zero"/> if not.</returns>
        private static IntPtr FindMainApplicationWindow()
        {
            // Try to get the window handle from the GorgonApplication class.
            if (GorgonApplication.MainForm != null)
            {
                return GorgonApplication.MainForm.Handle;
            }

            // If this is a windows forms application, then attempt to use the Application object to find the 
            // main form.
            if (Application.OpenForms.Count > 0)
            {
                // First, attempt to locate the window with keyboard focus.
                Form window = Form.ActiveForm;

                if (window != null)
                {
                    return window.Handle;
                }

                // If we have open forms, then assume the first in the list is the primary form.
                window = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;

                if (window != null)
                {
                    return window.Handle;
                }
            }

            // If we're at this point, we're either using WPF, or we can't determine the main window handle 
            // by any other means.  So just get the handle from the process.
            Process currentProcess = null;

            try
            {
                currentProcess = Process.GetCurrentProcess();
                return currentProcess.MainWindowHandle;
            }
            finally
            {
                currentProcess?.Close();
            }
        }

        /// <summary>
        /// Function to retrieve data from the provider of the physical device.
        /// </summary>
        /// <remarks>
        /// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in must implement this and format their data to populate the values of this object with correct state information.
        /// </remarks>
        protected override void OnGetData()
        {
            if (!IsAcquired)
            {
                IsAcquired = true;

                // If we still don't have acquisition, then get out and try again later.
                if (!IsAcquired)
                {
                    return;
                }
            }

            try
            {
                _joystick.Value.GetCurrentState(ref _state);
            }
            catch (DX.SharpDXException)
            {
                // If we can't get the state, then it's likely that our joystick has become unacquired.
                // So mark the device as unacquired and leave.
                IsAcquired = false;
                return;
            }

            // Update button states.
            for (int i = 0; i < Button.Length; ++i)
            {
                Button[i] = _state.Buttons[i] ? GamingDeviceButtonState.Down : GamingDeviceButtonState.Up;
            }

            // Update axes.
            foreach (IGorgonGamingDeviceAxis axis in Axis)
            {
                switch (axis.Axis)
                {
                    case GamingDeviceAxis.XAxis:
                        axis.Value = _state.X;
                        break;
                    case GamingDeviceAxis.YAxis:
                        axis.Value = _state.Y;
                        break;
                    case GamingDeviceAxis.ZAxis:
                        axis.Value = _state.Z;
                        break;
                    case GamingDeviceAxis.Throttle:
                        axis.Value = _state.Sliders[0];
                        break;
                    case GamingDeviceAxis.XAxis2:
                        axis.Value = _state.RotationX;
                        break;
                    case GamingDeviceAxis.YAxis2:
                        axis.Value = _state.RotationY;
                        break;
                    case GamingDeviceAxis.ZAxis2:
                        axis.Value = _state.RotationZ;
                        break;
                }
            }

            // Get point of view hat values.
            if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsPOV) != GamingDeviceCapabilityFlags.SupportsPOV)
            {
                return;
            }

            for (int i = 0; i < POV.Length; ++i)
            {
                int povValue = _state.PointOfViewControllers[i];

                if ((povValue == -1) || ((povValue & 0xffff) == 0xffff))
                {
                    POV[i] = -1.0f;
                    continue;
                }

                POV[i] = _state.PointOfViewControllers[i] / 100.0f;
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
        public override void Dispose()
        {
            if (_joystick.IsValueCreated)
            {
                _joystick.Value.Unacquire();
                _joystick.Value.Dispose();
                _joystick = null;
            }

            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectInputDevice"/> class.
        /// </summary>
        /// <param name="deviceInfo">The gaming device information for the specific device to use.</param>
        /// <param name="directInput">The direct input interface to use when creating the object.</param>
        /// <param name="axisMappings">The mappings for axes and Direct Input properties.</param>
        public DirectInputDevice(IGorgonGamingDeviceInfo deviceInfo, DI.DirectInput directInput, IReadOnlyDictionary<GamingDeviceAxis, DI.DeviceObjectId> axisMappings)
            : base(deviceInfo)
        {
            _directInput = directInput;
            _info = deviceInfo;
            _joystick = new Lazy<DI.Joystick>(() => CreateJoystick(directInput, deviceInfo), true);
            _axisMappings = axisMappings;
        }
        #endregion
    }
}
