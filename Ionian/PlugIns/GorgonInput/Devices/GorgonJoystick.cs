#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, October 12, 2006 3:45:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.InputDevices.Internal;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing joystick data.
	/// </summary>
	public class GorgonJoystick
		: Joystick
	{
		#region Variables.
		private int _joystickID = 0;			// ID of the joystick.
		private bool _deviceLost = false;		// Flag to indicate that the device is lost.
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
					return Axes[3];
				else
					return float.NaN;
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
			if (!DeadZone[axis].InRange(joyData))
			{
				SetAxisData(axis, joyData);
				AxisDirection[axis] = JoystickDirections.Center;

				// Get direction.
				switch (axis)
				{
					case 0:
						if (joyData < midrange)
							AxisDirection[axis] |= JoystickDirections.Left;
						else
							AxisDirection[axis] |= JoystickDirections.Right;
						break;
					case 1:
						if (joyData < midrange)
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
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
			JOYINFOEX joyInfo = new JOYINFOEX();		// Joystick information.
			int error = 0;								// Error code.

			if ((DeviceLost) && (!InputInterface.Window.Focused))
				return;
			else
				_deviceLost = false;

			if ((!Enabled) || (!Acquired))
				return;

			// Set up joystick info.
			joyInfo.Size = Marshal.SizeOf(typeof(JOYINFOEX));
			joyInfo.Flags = JoystickInfoFlags.ReturnButtons | JoystickInfoFlags.ReturnX | JoystickInfoFlags.ReturnY;

			if (HasZAxis)
				joyInfo.Flags |= JoystickInfoFlags.ReturnZ;
			if (AxisCount >= 4)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis5;
			if (AxisCount >= 5)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis6;
			if (HasRudder)
				joyInfo.Flags |= JoystickInfoFlags.ReturnRudder;
			if (HasPOV)
			{
				joyInfo.Flags |= JoystickInfoFlags.ReturnPOV;
				if (UnrestrictedPOV)
					joyInfo.Flags |= JoystickInfoFlags.ReturnPOVContinuousDegreeBearings;
			}
			error = Win32API.joyGetPosEx(_joystickID, ref joyInfo);
			if (error > 0)
				return;

			// Get X & Y values.
			DeadZoneAxis(0, joyInfo.X);
			DeadZoneAxis(1, joyInfo.Y);

			// Get additional axes.
			if (HasZAxis)
				DeadZoneAxis(2, joyInfo.Z);
			if (HasRudder)
				DeadZoneAxis(3, joyInfo.Rudder);
			if (AxisCount > 4)
				DeadZoneAxis(4, joyInfo.Axis5);
			if (AxisCount > 5)
				DeadZoneAxis(5, joyInfo.Axis6);

			if (HasPOV)
			{
				// Get POV data.
				POV[0] = joyInfo.POV;

				if (joyInfo.POV == -1)
					POVDirection[0] = JoystickDirections.Center;

				// Determine direction.
				if ((joyInfo.POV < 18000) && (joyInfo.POV > 9000))
					POVDirection[0] = JoystickDirections.Down | JoystickDirections.Right;
				if ((joyInfo.POV > 18000) && (joyInfo.POV < 27000))
					POVDirection[0] = JoystickDirections.Down | JoystickDirections.Left;
				if ((joyInfo.POV > 27000) && (joyInfo.POV < 36000))
					POVDirection[0] = JoystickDirections.Up | JoystickDirections.Left;
				if ((joyInfo.POV > 0) && (joyInfo.POV < 9000))
					POVDirection[0] = JoystickDirections.Up | JoystickDirections.Right;

				if (joyInfo.POV == 18000)
					POVDirection[0] = JoystickDirections.Down;
				if (joyInfo.POV == 0)
					POVDirection[0] = JoystickDirections.Up;
				if (joyInfo.POV == 9000)
					POVDirection[0] = JoystickDirections.Right;
				if (joyInfo.POV == 27000)
					POVDirection[0] = JoystickDirections.Left;
			}

			// Update buttons.
			for (int i = 0; i < ButtonCount; i++)
			{
				if ((joyInfo.Buttons & (JoystickButtons)(1 << i)) != 0)
					SetButtonValue(i, true);
				else
					SetButtonValue(i, false);
			}
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
		/// Constructor.
		/// </summary>
		/// <param name="ID">ID of the joystick to bind with.</param>
		/// <param name="name">Name of the joystick.</param>
		/// <param name="caps">Capabilities of the joystick.</param>
		/// <param name="threshold">Threshold of the joystick deadzone.</param>
		/// <param name="owner">Input interface that owns this joystick.</param>
		internal GorgonJoystick(int ID, string name, JOYCAPS caps, int threshold, Input owner)
			: base(name, owner)
		{
			// Get joystick info.
			_joystickID = ID;
			AxisCount = (int)caps.AxisCount;
			AxisRanges = new MinMaxRange[AxisCount];

			// Initialize the ranges.
			for (int i = 0; i < AxisCount; i++)
				AxisRanges[i] = MinMaxRange.Empty;

			// X axis range.
			AxisRanges[0] = new MinMaxRange((int)caps.MinimumX, (int)caps.MaximumX);
			// Y axis range.
			AxisRanges[1] = new MinMaxRange((int)caps.MinimumY, (int)caps.MaximumY);
			// Buttons.
			ButtonCount = (int)caps.ButtonCount;

			// Manufacturuer/product.
			ManufacturerID = caps.ManufacturerID;
			ProductID = caps.ProductID;

			// Determine capabilities.
			if ((caps.Capabilities & JoystickCapabilities.HasRudder) != 0)
			{
				AxisRanges[3] = new MinMaxRange((int)caps.MinimumRudder, (int)caps.MaximumRudder);
				HasRudder = true;
			}
			if ((caps.Capabilities & JoystickCapabilities.HasPOV) != 0)
			{
				HasPOV = true;
				if ((caps.Capabilities & JoystickCapabilities.POVContinuousDegreeBearings) != 0)
					UnrestrictedPOV = true;
				if ((caps.Capabilities & JoystickCapabilities.POV4Directions) != 0)
					POVHas4Directions = true;
			}
			if ((caps.Capabilities & JoystickCapabilities.HasZ) != 0)
			{
				HasZAxis = true;
				AxisRanges[2] = new MinMaxRange((int)caps.MinimumZ, (int)caps.MaximumZ);
			}
			if ((caps.Capabilities & JoystickCapabilities.HasU) != 0)
				AxisRanges[4] = new MinMaxRange((int)caps.Axis5Minimum, (int)caps.Axis5Maximum);
			if ((caps.Capabilities & JoystickCapabilities.HasV) != 0)
				AxisRanges[5] = new MinMaxRange((int)caps.Axis6Minimum, (int)caps.Axis6Maximum);

			// Create value arrays.
			Axes = new float[AxisCount];
			AxisDirection = new JoystickDirections[AxisCount];
			DeadZone = new MinMaxRange[AxisCount];
			for (int i = 0; i < AxisCount; i++)
			{
				AxisDirection[i] = JoystickDirections.Center;
				DeadZone[i] = MinMaxRange.Empty;
			}
			POVDirection = new JoystickDirections[1];
			POVCount = 1;
			POV = new int[1];
			POVDirection[0] = JoystickDirections.Center;			
			Button = new bool[ButtonCount];
		}
		#endregion
	}
}
