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

using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <inheritdoc/>
	class XInputDeviceInfo
		: IGorgonGamingDeviceInfo
	{
		#region Properties.
		/// <summary>
		/// Property to return the list of supported buttons for this controller.
		/// </summary>
		public Dictionary<XI.GamepadButtonFlags, int> SupportedButtons
		{
			get;
		}

		/// <summary>
		/// Property to return the ID of the device.
		/// </summary>
		public XI.UserIndex ID
		{
			get;
		}

		/// <inheritdoc/>
		public int ButtonCount => SupportedButtons.Count;

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
		public GamingDeviceCapabilityFlags Capabilities
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public GorgonGamingDeviceAxisList<GorgonGamingDeviceAxisInfo> AxisInfo
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string Description
		{
			get;
		}

		/// <inheritdoc/>
		public int POVCount => 1;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public void GetCaps(XI.Controller controller)
		{
			Capabilities = GamingDeviceCapabilityFlags.None;

			XI.Capabilities capabilities;

			controller.GetCapabilities(XI.DeviceQueryType.Any, out capabilities);

			// Get vibration caps.
			var vibrationRanges = new List<GorgonRange>();

			if (capabilities.Vibration.LeftMotorSpeed != 0)
			{
				vibrationRanges.Add(new GorgonRange(0, ushort.MaxValue));
				Capabilities |= GamingDeviceCapabilityFlags.SupportsVibration;
			}

			if (capabilities.Vibration.RightMotorSpeed != 0)
			{
				vibrationRanges.Add(new GorgonRange(0, ushort.MaxValue));
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
			var axes = new Dictionary<GamingDeviceAxis, GorgonRange>(new GorgonGamingDeviceAxisEqualityComparer());

			if (capabilities.Gamepad.LeftThumbX != 0)
			{
				axes[GamingDeviceAxis.XAxis] = new GorgonRange(short.MinValue, short.MaxValue);
			}

			if (capabilities.Gamepad.LeftThumbY != 0)
			{
				axes[GamingDeviceAxis.YAxis] = new GorgonRange(short.MinValue, short.MaxValue);
			}

			if (capabilities.Gamepad.RightThumbX != 0)
			{
				axes[GamingDeviceAxis.XAxis2] = new GorgonRange(short.MinValue, short.MaxValue);
				Capabilities |= GamingDeviceCapabilityFlags.SupportsSecondaryXAxis;
			}

			if (capabilities.Gamepad.RightThumbY != 0)
			{
				axes[GamingDeviceAxis.YAxis2] = new GorgonRange(short.MinValue, short.MaxValue);
				Capabilities |= GamingDeviceCapabilityFlags.SupportsSecondaryYAxis;
			}

			if (capabilities.Gamepad.LeftTrigger != 0)
			{
				axes[GamingDeviceAxis.LeftTrigger] = new GorgonRange(0, byte.MaxValue);
				Capabilities |= GamingDeviceCapabilityFlags.SupportsRudder;
			}

			if (capabilities.Gamepad.RightTrigger != 0)
			{
				axes[GamingDeviceAxis.RightTrigger] = new GorgonRange(0, byte.MaxValue);
				Capabilities |= GamingDeviceCapabilityFlags.SupportsThrottle;
			}

			AxisInfo = new GorgonGamingDeviceAxisList<GorgonGamingDeviceAxisInfo>(axes.Select(item => new GorgonGamingDeviceAxisInfo(item.Key, item.Value, 0)));
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="XInputDeviceInfo"/> class.
		/// </summary>
		/// <param name="deviceDescription">The description for the game pad controller.</param>
		/// <param name="id">The index ID of the device.</param>
		public XInputDeviceInfo(string deviceDescription, XI.UserIndex id)
		{
			SupportedButtons = new Dictionary<XI.GamepadButtonFlags, int>(new ButtonFlagsEqualityComparer());
			Description = deviceDescription;
			ID = id;
			ManufacturerID = 0;
			ProductID = 0;
		}
		#endregion
	}
}
