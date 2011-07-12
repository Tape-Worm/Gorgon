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
// Created: Tuesday, July 12, 2011 1:36:11 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Math;
using GorgonLibrary.HID;
using XI = SlimDX.XInput;

namespace GorgonLibrary.HID.XInput
{
	/// <summary>
	/// XInput XBOX 360 controller device.
	/// </summary>
	internal class XInputController
		: GorgonJoystick
	{
		#region Variables.
		private int _controllerID = 0;								// Controller ID.
		private XI.Controller _controller = null;					// Controller instance.
		private XI.Capabilities _caps = default(XI.Capabilities);	// Device capabilities.
		private XI.GamepadButtonFlags[] _button = null;				// Button flags.
		private bool _deviceLost = false;							// Flag to indicate that the device is lost.
		private bool _connected = false;							// Flag to indicate that the device is connected.
		private uint _lastPacket = int.MaxValue;					// Last packet number.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return flag to indicate that the device is in a lost state.
		/// </summary>
		internal bool DeviceLost
		{
			get
			{
				if (!AllowBackground)
					return _deviceLost;
				else
					return false;
			}
			set
			{
				if (AllowBackground)
					_deviceLost = false;					
				else
					_deviceLost = value;
			}
		}

		/// <summary>
		/// Property to return whether the device is acquired or not.
		/// </summary>		
		public override bool Acquired
		{
			get
			{
				return base.Acquired;
			}
			set
			{
				base.Acquired = value;

				if (value)
					_deviceLost = false;
			}
		}

		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.  For joysticks, this is always TRUE.
		/// </summary>		
		public override bool Exclusive
		{
			get
			{
				return true;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to return the x coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override float X
		{
			get 
			{
				return Axes[0];
			}
		}

		/// <summary>
		/// Property to return the y coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override float Y
		{
			get 
			{
				return Axes[1];
			}
		}

		/// <summary>
		/// Property to return the secondary X axis for the joystick (if applicable).
		/// </summary>
		public override float SecondaryX
		{
			get
			{
				return Axes[3];
			}
		}

		/// <summary>
		/// Property to return the secondary Y axis for the joystick (if applicable).
		/// </summary>
		public override float SecondaryY
		{
			get
			{
				return Axes[4];
			}
		}

		/// <summary>
		/// Property to return the z coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override float Z
		{
			get 
			{
				if (HasZAxis)
					return Axes[2];
				else
					return float.NaN;
			}
		}
				
		/// <summary>
		/// Property to return the rudder coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override float Rudder
		{
			get 
			{
				if (HasRudder)
					return Axes[5];
				else
					return float.NaN;
			}
		}

		/// <summary>
		/// Property to return whether the joystick is connected.
		/// </summary>
		public override bool IsConnected
		{
			get
			{
				return _connected;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set dead zone limited axis data.
		/// </summary>
		/// <param name="axis">Axis to work with.</param>
		/// <param name="joyData">Joystick data for the axis.</param>
		private void DeadZoneAxis(int axis, int joyData)
		{
			float midrange = AxisRanges[axis].Minimum + (AxisRanges[axis].Range / 2);		// Mid range value.

			// The dead zone range needs to be within the range of the axis.
			if (!DeadZone[axis].Contains(joyData))
			{
				SetAxisData(axis, joyData);
				AxisDirection[axis] = JoystickDirections.Center;

				// Get direction.
				switch (axis)
				{
					case 0:
					case 3:
						if (joyData < midrange)
							AxisDirection[axis] |= JoystickDirections.Left;
						else
							AxisDirection[axis] |= JoystickDirections.Right;
						break;
					case 1:
					case 4:
						if (joyData > midrange)
							AxisDirection[axis] |= JoystickDirections.Up;
						else
							AxisDirection[axis] |= JoystickDirections.Down;
						break;
					default:
						if (joyData < midrange)
							AxisDirection[axis] |= JoystickDirections.LessThanCenter;
						else
							AxisDirection[axis] |= JoystickDirections.MoreThanCenter;
						break;
				}
			}
			else
			{
				SetAxisData(axis, midrange);
				AxisDirection[axis] = JoystickDirections.Center;
			}
		}

		/// <summary>
		/// Function to retrieve the POV data.
		/// </summary>
		/// <param name="buttonState">Button data to parse.</param>
		private void GetPOVData(XI.GamepadButtonFlags buttonState)
		{
			// Get POV, which is on the D-Pad and default to a center value.
			POV[0] = -1;
			POVDirection[0] = JoystickDirections.Center;

			// Determine direction and set POV value which is -1 (for center) or 0..35900.  Divide by 100 to retrieve angle.
			if (((buttonState & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown) && ((buttonState & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
			{
				POV[0] = 13500;
				POVDirection[0] = JoystickDirections.Down | JoystickDirections.Right;
			}
			if (((buttonState & XI.GamepadButtonFlags.DPadDown) == XI.GamepadButtonFlags.DPadDown) && ((buttonState & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft))
			{
				POV[0] = 22500;
				POVDirection[0] = JoystickDirections.Down | JoystickDirections.Left;
			}
			if (((buttonState & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp) && ((buttonState & XI.GamepadButtonFlags.DPadLeft) == XI.GamepadButtonFlags.DPadLeft))
			{
				POV[0] = 31500;
				POVDirection[0] = JoystickDirections.Up | JoystickDirections.Left;
			}
			if (((buttonState & XI.GamepadButtonFlags.DPadUp) == XI.GamepadButtonFlags.DPadUp) && ((buttonState & XI.GamepadButtonFlags.DPadRight) == XI.GamepadButtonFlags.DPadRight))
			{
				POV[0] = 4500;
				POVDirection[0] = JoystickDirections.Up | JoystickDirections.Right;
			}

			if (buttonState == XI.GamepadButtonFlags.DPadDown)
			{
				POV[0] = 18000;
				POVDirection[0] = JoystickDirections.Down;
			}

			if (buttonState == XI.GamepadButtonFlags.DPadUp)
			{
				POV[0] = 0;
				POVDirection[0] = JoystickDirections.Up;
			}

			if (buttonState == XI.GamepadButtonFlags.DPadRight)
			{
				POV[0] = 9000;
				POVDirection[0] = JoystickDirections.Right;
			}

			if (buttonState == XI.GamepadButtonFlags.DPadLeft)
			{
				POV[0] = 27000;
				POVDirection[0] = JoystickDirections.Left;
			}
		}

		/// <summary>
		/// Function to initalize the data for the joystick.
		/// </summary>
		protected override void InitializeData()
		{
			// Don't try this on a disconnected controller.
			if (!_controller.IsConnected)
			{
				Gorgon.Log.Print("XInput Controller {0} disconnected.", GorgonLoggingLevel.Verbose, _controllerID);
				_connected = false;
				ResetData();
				return;
			}

			_caps = _controller.GetCapabilities(XI.DeviceQueryType.Any);
			_connected = true;

			// Default to the standard XBOX 360 controller.
			// If we have other types, then we may have to alter this.
			AxisCount = 6;
			ButtonCount = Enum.GetValues(typeof(XI.GamepadButtonFlags)).Length - 1;
			POVCount = 1;
			Axes = new float[AxisCount];
			AxisRanges = new GorgonMinMax[AxisCount];
			AxisDirection = new JoystickDirections[AxisCount];
			DeadZone = new GorgonMinMax[AxisCount];
			POVDirection = new JoystickDirections[POVCount];
			POV = new int[POVCount];
			Button = new bool[ButtonCount];

			for (int i = 0; i < AxisCount; i++)
			{
				Axes[i] = 0.0f;
				// The ranges are always between -32768 and 32767				
				AxisRanges[i] = new GorgonMinMax(-32768, 32767);
				AxisDirection[i] = JoystickDirections.Center;
				switch (i)
				{
					case 0:
					case 1:
						DeadZone[i] = new GorgonMinMax(-XI.Gamepad.GamepadLeftThumbDeadZone, XI.Gamepad.GamepadLeftThumbDeadZone);
						break;
					case 3:
					case 4:
						DeadZone[i] = new GorgonMinMax(-XI.Gamepad.GamepadRightThumbDeadZone, XI.Gamepad.GamepadRightThumbDeadZone);
						break;
					case 5:
						DeadZone[i] = new GorgonMinMax(0, XI.Gamepad.GamepadTriggerThreshold);
						break;
				}
				DeadZone[i] = GorgonMinMax.Empty;
			}

			// Set the triggers to range between 0 and 255.
			AxisRanges[2] = new GorgonMinMax(0, 255);
			AxisRanges[5] = new GorgonMinMax(0, 255);

			for (int i = 0; i < POVCount; i++)
			{
				POV[0] = -1;
				POVDirection[i] = JoystickDirections.Center;
			}

			for (int i = 0; i < ButtonCount; i++)
				Button[i] = false;

			// Get the individual values for the buttons.
			_button = ((XI.GamepadButtonFlags[])Enum.GetValues(typeof(XI.GamepadButtonFlags))).Where((item) => item != XI.GamepadButtonFlags.None).OrderBy((item) => item.ToString()).ToArray();
			
			HasZAxis = true;
			HasRudder = true;
			HasPOV = true;
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
			XI.State state = default(XI.State);

			if ((DeviceLost) && (!BoundWindow.Focused))
				return;
			else
				_deviceLost = false;

			if ((!Enabled) || (!Acquired))
				return;
			if (!_controller.IsConnected)
			{
				Gorgon.Log.Print("XInput Controller {0} disconnected.", GorgonLoggingLevel.Verbose, _controllerID);
				_connected = false;
				ResetData();
				return;
			}
			else
			{
				// If we weren't connected before, then get the caps for the device.
				if (!_connected)
				{
					_caps = _controller.GetCapabilities(XI.DeviceQueryType.Any);
					InitializeData();
					Gorgon.Log.Print("XInput Controller {0} (ID:{1}) re-connected.", GorgonLoggingLevel.Verbose, _caps.Subtype.ToString(), _controllerID);
				}
			}

			_connected = true;
			state = _controller.GetState();

			// Do nothing if the data has not changed since the last poll.
			if (_lastPacket == state.PacketNumber)
				return;

			// Get axis data.
			DeadZoneAxis(0, state.Gamepad.LeftThumbX);
			DeadZoneAxis(1, state.Gamepad.LeftThumbY);
			DeadZoneAxis(2, state.Gamepad.LeftTrigger);
			DeadZoneAxis(3, state.Gamepad.RightThumbX);
			DeadZoneAxis(4, state.Gamepad.RightThumbY);
			DeadZoneAxis(5, state.Gamepad.RightTrigger);

			// Get button info.
			for (int i = 0; i < ButtonCount; i++)
			{
				if ((_button[i] != XI.GamepadButtonFlags.None) && (state.Gamepad.Buttons != XI.GamepadButtonFlags.None))
					SetButtonValue(i, ((state.Gamepad.Buttons & _button[i]) != 0));
				else
					SetButtonValue(i, false);
			}

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
		/// <param name="ID">The ID of the joystick.</param>
		/// <param name="name">The name of the joystick.</param>
		/// <param name="boundWindow">The window to bind the joystick with.</param>
		/// <param name="controller">Controller instance to bind to this joystick.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		internal XInputController(GorgonXInputDeviceFactory owner, int ID, string name, Control boundWindow, XI.Controller controller)
			: base(owner, name, boundWindow)
		{
			if (controller == null)
				throw new ArgumentNullException("controller");

			_controller = controller;
			_controllerID = ID;
			if (controller.IsConnected)
			{
				InitializeData();
				Gorgon.Log.Print("XInput XBOX 360 controller device {0} interface created (ID:{1}).", GorgonLoggingLevel.Simple, _caps.Subtype.ToString(), ID);
			}
			else
			{
				Gorgon.Log.Print("Disconnected XInput XBOX 360 controller device #{0} interface created.", GorgonLoggingLevel.Simple, ID);
				_connected = false;				
			}
		}
		#endregion
	}
}
