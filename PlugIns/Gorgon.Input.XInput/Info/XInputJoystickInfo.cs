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
using Gorgon.Core;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <inheritdoc/>
	class XInputJoystickInfo
		: IXInputJoystickInfo
	{
		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="XInputJoystickInfo"/> class.
		/// </summary>
		/// <param name="uuid">The unique ID for the xinput device.</param>
		/// <param name="name">The name of the xinput device.</param>
		/// <param name="controller">The controller representing the xinput device.</param>
		/// <param name="id">The index ID of the device.</param>
		public XInputJoystickInfo(Guid uuid, string name, XI.Controller controller, int id)
		{
			UUID = uuid;
			Name = name;
			Controller = controller;
			ID = id;

			ManufacturerID = 0;
			ProductID = 0;
		}
		#endregion

		#region IXInputJoystickInfo Members
		#region Properties.
		/// <inheritdoc/>
		public int ID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public XI.Controller Controller
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public void GetCaps()
		{
			Capabilities = JoystickCapabilityFlags.SupportsDiscreetPOV | JoystickCapabilityFlags.SupportsPOV | JoystickCapabilityFlags.SupportsRudder |
						   JoystickCapabilityFlags.SupportsThrottle | JoystickCapabilityFlags.SupportsVibration | JoystickCapabilityFlags.SupportsSecondaryXAxis |
						   JoystickCapabilityFlags.SupportsSecondaryYAxis;

			ButtonCount = Enum.GetValues(typeof(XI.GamepadButtonFlags)).Length;

			// XInput hard codes all of its ranges.
			AxisRanges = new GorgonJoystickAxisRangeList(new []
			                                             {
				                                             new KeyValuePair<int, GorgonRange>((int)JoystickAxis.XAxis, new GorgonRange(-32768, 32767)), 
															 new KeyValuePair<int, GorgonRange>((int)JoystickAxis.YAxis, new GorgonRange(-32768, 32767)), 
															 new KeyValuePair<int, GorgonRange>((int)JoystickAxis.XAxis2, new GorgonRange(-32768, 32767)), 
															 new KeyValuePair<int, GorgonRange>((int)JoystickAxis.YAxis2, new GorgonRange(-32768, 32767)), 
															 new KeyValuePair<int, GorgonRange>((int)JoystickAxis.Throttle, new GorgonRange(0, 255)), 
															 new KeyValuePair<int, GorgonRange>((int)JoystickAxis.Rudder, new GorgonRange(0, 255)), 
			                                             });
			
			VibrationMotorRanges = new[]
			                       {
				                       new GorgonRange(0, 65535),
				                       new GorgonRange(0, 65535)
			                       };
		}
		#endregion
		#endregion

		#region IGorgonJoystickInfo Members
		/// <inheritdoc/>
		public bool IsConnected
		{
			get
			{
				return Controller.IsConnected;
			}
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
			private set;
		}

		/// <inheritdoc/>
		public int ProductID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public IGorgonJoystickAxisRangeList AxisRanges
		{
			get;
			private set;
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
		#endregion

		#region IGorgonInputDeviceInfo Members
		/// <inheritdoc/>
		public Guid UUID
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public string HumanInterfaceDevicePath
		{
			get
			{
				return "GamePad";
			}
		}

		/// <inheritdoc/>
		public string ClassName
		{
			get
			{
				return "Game Pad";
			}
		}

		/// <inheritdoc/>
		public InputDeviceType InputDeviceType
		{
			get
			{
				return InputDeviceType.Joystick;
			}
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <inheritdoc/>
		public string Name
		{
			get;
			private set;
		}
		#endregion
	}
}
