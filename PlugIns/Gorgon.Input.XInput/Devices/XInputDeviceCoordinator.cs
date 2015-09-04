#region MIT
// 
// Gorgon.
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
// Created: Thursday, September 3, 2015 11:39:43 PM
// 
#endregion

using System;
using System.Collections.Generic;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// An XInput specific concrete implementation of the <see cref="IGorgonInputDeviceCoordinator"/>
	/// </summary>
	class XInputDeviceCoordinator
		: IGorgonInputDeviceCoordinator
	{
		#region Classes.
		/// <summary>
		/// Holds state data for the xinput controller.
		/// </summary>
		private class DeviceState
		{
			/// <summary>
			/// Property to set or return the controller for the device state.
			/// </summary>
			public XI.Controller Controller
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the last packet ID received.
			/// </summary>
			public int? LastPacket
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the last state received.
			/// </summary>
			public GorgonJoystickData LastState
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the vibration value.
			/// </summary>
			public XI.Vibration Vibration
			{
				get;
				set;
			}
		}
		#endregion

		#region Variables.
		// The list of xinput controllers.
		private readonly Dictionary<IGorgonInputDevice, DeviceState> _controllers = new Dictionary<IGorgonInputDevice, DeviceState>();
		#endregion

		#region Methods.
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

			if (state == XI.GamepadButtonFlags.DPadDown)
			{
				pov = 180.0f;
			}

			if (state == XI.GamepadButtonFlags.DPadUp)
			{
				pov = 0;
			}

			if (state == XI.GamepadButtonFlags.DPadRight)
			{
				pov = 90.0f;
			}

			if (state == XI.GamepadButtonFlags.DPadLeft)
			{
				pov = 270.0f;
			}

			return pov;
		}

		/// <summary>
		/// Function to retrieve the current state of the buttons.
		/// </summary>
		/// <param name="state">The XInput state for the buttons.</param>
		private static int GetButtonState(XI.GamepadButtonFlags state)
		{
			int result = 0;

			if ((state & XI.GamepadButtonFlags.A) == XI.GamepadButtonFlags.A)
			{
				result |= 1;
			}

			if ((state & XI.GamepadButtonFlags.B) == XI.GamepadButtonFlags.B)
			{
				result |= 2;
			}

			if ((state & XI.GamepadButtonFlags.X) == XI.GamepadButtonFlags.X)
			{
				result |= 4;
			}

			if ((state & XI.GamepadButtonFlags.Y) == XI.GamepadButtonFlags.Y)
			{
				result |= 8;
			}

			if ((state & XI.GamepadButtonFlags.LeftShoulder) == XI.GamepadButtonFlags.LeftShoulder)
			{
				result |= 16;
			}

			if ((state & XI.GamepadButtonFlags.RightShoulder) == XI.GamepadButtonFlags.RightShoulder)
			{
				result |= 32;
			}

			if ((state & XI.GamepadButtonFlags.LeftThumb) == XI.GamepadButtonFlags.LeftThumb)
			{
				result |= 64;
			}

			if ((state & XI.GamepadButtonFlags.RightThumb) == XI.GamepadButtonFlags.RightThumb)
			{
				result |= 128;
			}

			if ((state & XI.GamepadButtonFlags.Start) == XI.GamepadButtonFlags.Start)
			{
				result |= 256;
			}

			if ((state & XI.GamepadButtonFlags.Back) == XI.GamepadButtonFlags.Back)
			{
				result |= 512;
			}

			return result;
		}

		/// <summary>
		/// Function to update the values for the axes on the joystick.
		/// </summary>
		/// <param name="state">The current state of the controller.</param>
		/// <param name="capabilities">The capabilities of the controller.</param>
		/// <param name="values">The values to populate.</param>
		private static void UpdateAxisValues(XI.Gamepad state, JoystickCapabilityFlags capabilities, Dictionary<JoystickAxis, int> values)
		{
			values[JoystickAxis.XAxis] = state.LeftThumbX;
			values[JoystickAxis.YAxis] = state.LeftThumbY;

			if ((capabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
			{
				values[JoystickAxis.XAxis2] = state.RightThumbX;
			}

			if ((capabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
			{
				values[JoystickAxis.YAxis2] = state.RightThumbY;
			}

			if ((capabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
			{
				values[JoystickAxis.Throttle] = state.RightTrigger;
			}

			if ((capabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
			{
				values[JoystickAxis.Rudder] = state.LeftTrigger;
			}
		}

		/// <inheritdoc/>
		bool IGorgonInputDeviceCoordinator.DispatchEvent<T>(IGorgonInputEventDrivenDevice<T> eventDevice, ref T deviceData)
		{
			// This is never used from this plug in.
			return false;
		}

		/// <inheritdoc/>
		public GorgonJoystickData GetJoystickStateData(GorgonJoystick2 device)
		{
			DeviceState deviceState;
			XI.State state;

			if (!_controllers.TryGetValue(device, out deviceState))
			{
				_controllers[device] = deviceState = new DeviceState
				                                    {
					                                    Controller = new XI.Controller(((XInputJoystickInfo)device.Info).ID),
					                                    LastPacket = null,
					                                    LastState = null
				                                    };
			}
			
			if (deviceState.LastState == null)
			{
				deviceState.LastState = new GorgonJoystickData();
			}

			// Check for disconnected state.
			if ((!device.IsAcquired)
			    || (!deviceState.Controller.IsConnected)
			    || (!deviceState.Controller.GetState(out state))
			    || ((deviceState.LastPacket.HasValue)
			        && (deviceState.LastPacket == state.PacketNumber)))
			{
				deviceState.LastState.IsConnected = deviceState.Controller.IsConnected;
				deviceState.LastPacket = null;

				return deviceState.LastState;
			}

			deviceState.LastState.IsConnected = true;

			deviceState.LastState.ButtonState = GetButtonState(state.Gamepad.Buttons);

			if ((device.Info.Capabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
			{
				deviceState.LastState.POVValue = GetPOVState(state.Gamepad.Buttons);
			}

			UpdateAxisValues(state.Gamepad, device.Info.Capabilities, deviceState.LastState.AxisValues);

			deviceState.LastPacket = state.PacketNumber;

			return deviceState.LastState;
		}

		/// <inheritdoc/>
		public void SendVibrationData(GorgonJoystick2 device, int vibrationMotor, int speed)
		{
			DeviceState deviceState;

			if (!_controllers.TryGetValue(device, out deviceState))
			{
				_controllers[device] = deviceState = new DeviceState
				{
					Controller = new XI.Controller(((XInputJoystickInfo)device.Info).ID),
					LastPacket = null,
					LastState = null
				};
			}

			if (!deviceState.Controller.IsConnected)
			{
				return;
			}

			deviceState.Vibration = new XI.Vibration
			                        {
				                        LeftMotorSpeed = vibrationMotor == 0 ? (ushort)speed : deviceState.Vibration.LeftMotorSpeed,
				                        RightMotorSpeed = vibrationMotor == 1 ? (ushort)speed : deviceState.Vibration.RightMotorSpeed
			                        };

			deviceState.Controller.SetVibration(deviceState.Vibration);
		}
		#endregion
	}
}
