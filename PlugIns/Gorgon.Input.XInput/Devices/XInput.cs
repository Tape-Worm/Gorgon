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

using System;
using Gorgon.Diagnostics;
using Gorgon.UI;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// XInput XBOX 360 controller device.
	/// </summary>
	internal class XInputController
		: GorgonJoystick
	{
		#region Constants.
		// Last packet number.
        private const int LastPacket = int.MaxValue;       
        #endregion

		#region Variables.
		// Joystick information.
		private readonly IXInputJoystickInfo _info;
		// Flag to indicate that the device is now ready to be polled.
		private bool _isReady;
		// Button flags.
		private readonly XI.GamepadButtonFlags[] _button = (XI.GamepadButtonFlags[])Enum.GetValues(typeof(XI.GamepadButtonFlags));
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.
		/// </summary>
		public override bool Exclusive
		{
			get
			{
				return false;
			}
			set
			{
				// Intentionally left blank.
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the POV data.
		/// </summary>
		/// <param name="buttonState">Button data to parse.</param>
		private void GetPOVData(XI.GamepadButtonFlags buttonState)
		{
			// Get POV, which is on the D-Pad and default to a center value.
			POV = -1;

			// Determine direction and set POV value which is -1 (for center) or 0..35900.  Divide by 100 to retrieve angle.
		    if (((buttonState & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown) &&
		        ((buttonState & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
		    {
		        POV = 13500;
		    }

		    if (((buttonState & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown) &&
		        ((buttonState & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft))
		    {
		        POV = 22500;
		    }

		    if (((buttonState & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp) &&
		        ((buttonState & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft))
		    {
		        POV = 31500;
		    }

		    if (((buttonState & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp) &&
		        ((buttonState & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
		    {
		        POV = 4500;
		    }

		    if (buttonState == XI.GamepadButtonFlags.DPadDown)
		    {
		        POV = 18000;
		    }

		    if (buttonState == XI.GamepadButtonFlags.DPadUp)
		    {
		        POV = 0;
		    }

		    if (buttonState == XI.GamepadButtonFlags.DPadRight)
		    {
		        POV = 9000;
		    }

		    if (buttonState == XI.GamepadButtonFlags.DPadLeft)
		    {
		        POV = 27000;
		    }
		}

        /// <summary>
        /// Function called when the <see cref="GorgonInputDevice.Acquired" /> property changes its value.
        /// </summary>
	    protected override void OnAcquisitionChanged()
	    {
            if (!Acquired)
            {
                return;
            }

            DeviceLost = false;
	    }

	    /// <summary>
		/// Function to perform device vibration.
		/// </summary>
		/// <param name="motorIndex">Index of the motor to start.</param>
		/// <param name="value">Value to set.</param>
		/// <remarks>Implementors should implement this method if the device supports vibration.</remarks>
		protected override void VibrateDevice(int motorIndex, int value)
		{
			var vibeData = new XI.Vibration();

		    if (!_info.Controller.IsConnected)
		    {
		        return;
		    }

		    if (motorIndex == 0)
		    {
		        vibeData.LeftMotorSpeed = (ushort)value;
		    }

		    if (motorIndex == 1)
		    {
		        vibeData.RightMotorSpeed = (ushort)value;
		    }

		    _info.Controller.SetVibration(vibeData);
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
		    if (!_info.IsConnected)
			{
				GorgonApplication.Log.Print("XInput Controller {0} disconnected.", LoggingLevel.Verbose, _info.ID);
				_isReady = false;
				return;
			}
			
			// Reset our data if we've reconnected as it may be incorrect.
		    if (!_isReady)
		    {
		        Reset();					
				
				_isReady = true;

		        XI.Capabilities caps = _info.Controller.GetCapabilities(XI.DeviceQueryType.Any);
		        GorgonApplication.Log.Print("XInput Controller ID:{0} re-connected.", LoggingLevel.Verbose, caps.SubType.ToString(), _info.ID);
		    }

		    XI.State state = _info.Controller.GetState();

			// Do nothing if the data has not changed since the last poll.
		    if (LastPacket == state.PacketNumber)
		    {
		        return;
		    }

		    // Get axis data.
			Axis[JoystickAxis.XAxis] = state.Gamepad.LeftThumbX;
			Axis[JoystickAxis.YAxis] = state.Gamepad.LeftThumbY;
			Axis[JoystickAxis.XAxis2] = state.Gamepad.RightThumbX;
			Axis[JoystickAxis.YAxis2] = state.Gamepad.RightThumbY;
			Axis[JoystickAxis.Throttle] = state.Gamepad.RightTrigger;
			Axis[JoystickAxis.Rudder] = state.Gamepad.LeftTrigger;

			// Get button info.
			if (state.Gamepad.Buttons != XI.GamepadButtonFlags.None)
			{
				// ReSharper disable once ForCanBeConvertedToForeach
				for (int i = 0; i < Button.Count; i++)
				{
					Button[i] = ((_button[i] != XI.GamepadButtonFlags.None) &&
					             ((state.Gamepad.Buttons & _button[i]) == _button[i]))
						            ? JoystickButtonState.Down
						            : JoystickButtonState.Up;
				}
			}

			// Get POV values.
			GetPOVData(state.Gamepad.Buttons);
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="XInputController"/> class.
		/// </summary>
		/// <param name="owner">The input factory that owns this device.</param>
		/// <param name="deviceInfo">Information about the device.</param>
		internal XInputController(GorgonInputService owner, IXInputJoystickInfo deviceInfo)
			: base(owner, deviceInfo)
		{
			_info = deviceInfo;

			GorgonApplication.Log.Print(
			                            _info.IsConnected
				                            ? "XInput controller device interface created (ID:{0})."
				                            : "Disconnected XInput controller device #{0} interface created.",
			                            LoggingLevel.Simple,
			                            deviceInfo.ID);
		}

		#endregion
	}
}
