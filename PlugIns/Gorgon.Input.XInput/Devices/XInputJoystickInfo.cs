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
// Created: Sunday, July 5, 2015 3:54:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <inheritdoc/>
	class XInputJoystickInfo
		: IGorgonJoystickInfo2
	{
		#region Properties.
		/// <summary>
		/// Property to return the ID of the device.
		/// </summary>
		public XI.UserIndex ID
		{
			get;
		}

		/// <inheritdoc/>
		public int ButtonCount
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int ManufacturerID
		{
			get;
		}

		/// <inheritdoc/>
		public int ProductID
		{
			get;
		}

		/// <inheritdoc/>
		public IReadOnlyList<GorgonRange> VibrationMotorRanges
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public JoystickCapabilityFlags Capabilities
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath => "GamePad";

		/// <inheritdoc/>
		public string ClassName => "Game Pad";

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType => InputDeviceType.Joystick;

		/// <inheritdoc/>
		public GorgonJoystickAxisInfoList AxisInfo
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string Description
		{
			get;
		}
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public void GetCaps(XI.Controller controller)
		{
			Capabilities = JoystickCapabilityFlags.None;

			var buttons = (XI.GamepadButtonFlags[])Enum.GetValues(typeof(XI.GamepadButtonFlags));
			XI.Capabilities capabilities;

			controller.GetCapabilities(XI.DeviceQueryType.Any, out capabilities);

			// Get vibration caps.
			var vibrationRanges = new List<GorgonRange>();

			if (capabilities.Vibration.LeftMotorSpeed != 0)
			{
				vibrationRanges.Add(new GorgonRange(0, ushort.MaxValue));
				Capabilities |= JoystickCapabilityFlags.SupportsVibration;
			}

			if (capabilities.Vibration.RightMotorSpeed != 0)
			{
				vibrationRanges.Add(new GorgonRange(0, ushort.MaxValue));
				Capabilities |= JoystickCapabilityFlags.SupportsVibration;
			}

			VibrationMotorRanges = vibrationRanges;

			if (((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown)
			    || ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp)
			    || ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft)
			    || ((capabilities.Gamepad.Buttons & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
			{
				Capabilities |= JoystickCapabilityFlags.SupportsPOV | JoystickCapabilityFlags.SupportsDiscreetPOV;
			}

			// Get the button count.
			foreach (XI.GamepadButtonFlags button in buttons)
			{
				// The D-Pad on the controller is considered a button.
				if ((button == XI.GamepadButtonFlags.DPadDown)
				    || (button == XI.GamepadButtonFlags.DPadLeft)
				    || (button == XI.GamepadButtonFlags.DPadUp)
				    || (button == XI.GamepadButtonFlags.DPadRight)
					|| (button == XI.GamepadButtonFlags.None))
				{
					continue;
				}

				if ((capabilities.Gamepad.Buttons & button) == button)
				{
					ButtonCount++;
				}
			}

			// Find out the ranges for each axis.
			var axes = new Dictionary<JoystickAxis, GorgonRange>(new GorgonJoystickAxisEqualityComparer());

			if (capabilities.Gamepad.LeftThumbX != 0)
			{
				axes[JoystickAxis.XAxis] = new GorgonRange(short.MinValue, short.MaxValue);
			}

			if (capabilities.Gamepad.LeftThumbY != 0)
			{
				axes[JoystickAxis.YAxis] = new GorgonRange(short.MinValue, short.MaxValue);
			}

			if (capabilities.Gamepad.RightThumbX != 0)
			{
				axes[JoystickAxis.XAxis2] = new GorgonRange(short.MinValue, short.MaxValue);
				Capabilities |= JoystickCapabilityFlags.SupportsSecondaryXAxis;
			}

			if (capabilities.Gamepad.RightThumbY != 0)
			{
				axes[JoystickAxis.YAxis2] = new GorgonRange(short.MinValue, short.MaxValue);
				Capabilities |= JoystickCapabilityFlags.SupportsSecondaryYAxis;
			}

			if (capabilities.Gamepad.LeftTrigger != 0)
			{
				axes[JoystickAxis.LeftTrigger] = new GorgonRange(0, byte.MaxValue);
				Capabilities |= JoystickCapabilityFlags.SupportsRudder;
			}

			if (capabilities.Gamepad.RightTrigger != 0)
			{
				axes[JoystickAxis.RightTrigger] = new GorgonRange(0, byte.MaxValue);
				Capabilities |= JoystickCapabilityFlags.SupportsThrottle;
			}

			AxisInfo = new GorgonJoystickAxisInfoList(axes.Select(item => new GorgonJoystickAxisInfo(item.Key, item.Value)));
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="XInputJoystickInfo"/> class.
		/// </summary>
		/// <param name="deviceDescription">The description for the game pad controller.</param>
		/// <param name="id">The index ID of the device.</param>
		public XInputJoystickInfo(string deviceDescription, XI.UserIndex id)
		{
			Description = deviceDescription;
			ID = id;
			ManufacturerID = 0;
			ProductID = 0;
		}
		#endregion
	}
}
