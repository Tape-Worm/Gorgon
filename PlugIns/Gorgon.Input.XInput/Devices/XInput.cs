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
using System.Collections.ObjectModel;
using Gorgon.Core;
using Gorgon.Diagnostics;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// XInput XBOX 360 controller device.
	/// </summary>
	internal class XInputController
		: GorgonJoystick
	{
		#region Classes.
		/// <summary>
		/// Defines the buttons for the controller.
		/// </summary>
		public class XInputButtons
			: JoystickButtons
		{
			#region Methods.
			/// <summary>
			/// Function to set the state for a button.
			/// </summary>
			/// <param name="name">Name of the button.</param>
			/// <param name="state">State of the button.</param>
			public void SetButtonState(XI.GamepadButtonFlags name, bool state)
			{
				string enumName = name.ToString();
				int index = IndexOf(enumName);

			    if (index != -1)
			    {
			        this[index] = new JoystickButtonState(enumName, state);
			    }
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="XInputButtons"/> class.
			/// </summary>
			public XInputButtons()
			{
				var buttons = (XI.GamepadButtonFlags[])Enum.GetValues(typeof(XI.GamepadButtonFlags));

				foreach (XI.GamepadButtonFlags button in buttons)
				{
				    if (button != XI.GamepadButtonFlags.None)
				    {
				        DefineButton(button.ToString());
				    }
				}
			}
			#endregion
		}

		/// <summary>
		/// Defines the capabilities of the XInput device.
		/// </summary>
		public class XInputCapabilties
			: JoystickCapabilities
		{
			#region Methods.
		    /// <summary>
		    /// Function to retrieve the capabilities of the controller device.
		    /// </summary>
		    /// <param name="buttonCount">Button count.</param>
		    private void GetCaps(int buttonCount)
			{
				// Default to the standard XBOX 360 controller.
				// If we have other types, then we may have to alter this.
				AxisCount = 6;
				ButtonCount = buttonCount;

				// Get ranges.
				SecondaryYAxisRange = SecondaryXAxisRange = YAxisRange = XAxisRange = new GorgonRange(-32768, 32767);
				ThrottleAxisRange = RudderAxisRange = new GorgonRange(0, 255);

			    VibrationMotorRanges = new ReadOnlyCollection<GorgonRange>(new[]
				    {
					    new GorgonRange(0, 65535), new GorgonRange(0, 65535)
				    });

				// BUG: As of this writing, there's a bug in XInputGetCapabilities in DirectX that's returning 255 even though
				// they've documented the ranges (and I've tested these) to be 0..65535.
				//VibrationMotorRanges[0] = new GorgonMinMax(0, caps.Vibration.LeftMotorSpeed);
				//VibrationMotorRanges[1] = new GorgonMinMax(0, caps.Vibration.RightMotorSpeed);

				ExtraCapabilities = JoystickCapabilityFlags.SupportsDiscreetPOV | JoystickCapabilityFlags.SupportsPOV | JoystickCapabilityFlags.SupportsRudder | JoystickCapabilityFlags.SupportsThrottle | JoystickCapabilityFlags.SupportsVibration | JoystickCapabilityFlags.SupportsSecondaryXAxis | JoystickCapabilityFlags.SupportsSecondaryYAxis;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="XInputCapabilties"/> class.
			/// </summary>
			/// <param name="controller">The controller to enumerate.</param>
			/// <param name="buttonCount">The button count.</param>
			public XInputCapabilties(XI.Controller controller, int buttonCount)
			{
			    if (!controller.IsConnected)
			    {
			        return;
			    }

			    GetCaps(buttonCount);
			}
			#endregion			
		}
		#endregion

        #region Constants.
        private const uint LastPacket = int.MaxValue;       // Last packet number.
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

		#region Variables.
		private readonly int _controllerID;					// Controller ID.
		private readonly XI.Controller _controller;			// Controller instance.
		private XI.GamepadButtonFlags[] _button;			// Button flags.
	    private XInputButtons _buttonList;					// List of buttons for the controller.
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

		    if (!_controller.IsConnected)
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

		    _controller.SetVibration(vibeData);
		}

		/// <summary>
		/// Function to retrieve the capabilities of the joystick/gamepad.
		/// </summary>
		/// <returns>
		/// The capabilities data for the joystick/gamepad.
		/// </returns>
		/// <remarks>Implementors must implement this method so the object can determine constraints about the device.</remarks>
		protected override JoystickCapabilities GetCapabilities()
		{
			// Get the individual values for the buttons.
			_button = (XI.GamepadButtonFlags[])Enum.GetValues(typeof(XI.GamepadButtonFlags));
			return new XInputCapabilties(_controller, _button.Length - 1);
		}

		/// <summary>
		/// Function to retrieve the buttons for the joystick/gamepad.
		/// </summary>
		/// <returns>
		/// The list of buttons for the joystick/gamepad.
		/// </returns>
		/// <remarks>Implementors must implement this method so the object can get the list of buttons for the device.</remarks>
		protected override JoystickButtons GetButtons()
		{
			_buttonList = new XInputButtons();
			return _buttonList;
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
		    if (!_controller.IsConnected)
			{
				GorgonApplication.Log.Print("XInput Controller {0} disconnected.", LoggingLevel.Verbose, _controllerID);
				IsConnected = false;				
				return;
			}

		    // If we weren't connected before, then get the caps for the device.
		    if (!IsConnected)
		    {
		        var previousDeadZone = DeadZone;

		        Initialize();					
		        IsConnected = true;

		        // Restore the dead zone.
		        DeadZone.Rudder = new GorgonRange(previousDeadZone.Rudder);
		        DeadZone.Throttle = new GorgonRange(previousDeadZone.Throttle);
		        DeadZone.X = new GorgonRange(previousDeadZone.X);
		        DeadZone.Y = new GorgonRange(previousDeadZone.Y);
		        DeadZone.SecondaryX = new GorgonRange(previousDeadZone.SecondaryX);
		        DeadZone.SecondaryY = new GorgonRange(previousDeadZone.SecondaryY);
#if DEBUG
		        XI.Capabilities caps = _controller.GetCapabilities(XI.DeviceQueryType.Any);
		        GorgonApplication.Log.Print("XInput Controller {0} (ID:{1}) re-connected.", LoggingLevel.Verbose, caps.SubType.ToString(), _controllerID);
#endif
		    }

		    XI.State state = _controller.GetState();

			// Do nothing if the data has not changed since the last poll.
		    if (LastPacket == state.PacketNumber)
		    {
		        return;
		    }

		    // Get axis data.
			X = state.Gamepad.LeftThumbX;
			Y = state.Gamepad.LeftThumbY;
			SecondaryX = state.Gamepad.RightThumbX;
			SecondaryY = state.Gamepad.RightThumbY;
			Throttle = state.Gamepad.RightTrigger;
			Rudder = state.Gamepad.LeftTrigger;

			// Get button info.
			if (state.Gamepad.Buttons != XI.GamepadButtonFlags.None)
			{
				// ReSharper disable once ForCanBeConvertedToForeach
				for (int i = 0; i < _button.Length; i++)
				{
				    _buttonList.SetButtonState(_button[i],
				                               (_button[i] != XI.GamepadButtonFlags.None) &&
				                               ((state.Gamepad.Buttons & _button[i]) == _button[i]));
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
		/// <param name="joystickID">The ID of the joystick.</param>
		/// <param name="name">The name of the joystick.</param>
		/// <param name="controller">Controller instance to bind to this joystick.</param>
		internal XInputController(GorgonInputService owner, int joystickID, string name, XI.Controller controller)
			: base(owner, name)
		{
		    _controller = controller;
			_controllerID = joystickID;
			if (controller.IsConnected)
			{
				IsConnected = true;
				
#if DEBUG
				XI.Capabilities caps = controller.GetCapabilities(XI.DeviceQueryType.Any);
				GorgonApplication.Log.Print("XInput XBOX 360 controller device {0} interface created (ID:{1}).", LoggingLevel.Simple, caps.SubType.ToString(), joystickID);
#endif
			}
			else
			{
				GorgonApplication.Log.Print("Disconnected XInput XBOX 360 controller device #{0} interface created.", LoggingLevel.Simple, joystickID);
				IsConnected = false;				
			}
		}
		#endregion
	}
}
