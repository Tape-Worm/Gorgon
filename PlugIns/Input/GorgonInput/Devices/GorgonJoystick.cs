#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Thursday, October 12, 2006 3:45:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SharpUtilities;
using SharpUtilities.Native.Win32;
using GorgonLibrary;

namespace GorgonLibrary.Input
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
				if (!_background)
					return _deviceLost;
				else
					return false;
			}
			set
			{
				if (_background)
					_deviceLost = false;					
				else
					_deviceLost = value;
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
				if (_hasZ)
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
				if (_hasRudder)
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
			int range = _axisRanges[axis].Range / 2;		// Half the range.

			if (!_deadZone[axis].InRange(joyData))
			{
				_axisValues[axis] = joyData - range;
				_axisDirections[axis] = JoystickDirection.Center;

				// Get direction.
				switch(axis)
				{
					case 0:
						if (_axisValues[axis] < 0)
							_axisDirections[axis] |= JoystickDirection.Left;
						else
							_axisDirections[axis] |= JoystickDirection.Right;
						break;
					case 1:
						if (_axisValues[axis] < 0)
							_axisDirections[axis] |= JoystickDirection.Up;
						else
							_axisDirections[axis] |= JoystickDirection.Down;
						break;
					default:
						if (_axisValues[axis] < 0)
							_axisDirections[axis] |= JoystickDirection.LessThanCenter;
						else
							_axisDirections[axis] |= JoystickDirection.MoreThanCenter;
						break;
				}
			}
			else
			{
				_axisValues[axis] = 0;
				_axisDirections[axis] = JoystickDirection.Center;
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

			if ((!_bound) || (!_acquired))
				return;

			// Set up joystick info.
			joyInfo.Size = Marshal.SizeOf(typeof(JOYINFOEX));
			joyInfo.Flags = JoystickInfoFlags.ReturnButtons | JoystickInfoFlags.ReturnX | JoystickInfoFlags.ReturnY;

			if (_hasZ)
				joyInfo.Flags |= JoystickInfoFlags.ReturnZ;
			if (_axisCount >= 4)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis5;
			if (_axisCount >= 5)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis6;
			if (_hasRudder)
				joyInfo.Flags |= JoystickInfoFlags.ReturnRudder;
			if (_hasPOV)
			{
				joyInfo.Flags |= JoystickInfoFlags.ReturnPOV;
				if (_unrestrictedPOV)
					joyInfo.Flags |= JoystickInfoFlags.ReturnPOVContinuousDegreeBearings;
			}
			error = Win32API.joyGetPosEx(_joystickID, ref joyInfo);
			if (error > 0)
				return;

			// Get X & Y values.
			DeadZoneAxis(0, joyInfo.X);
			DeadZoneAxis(1, joyInfo.Y);

			// Get additional axes.
			if (_hasZ)
				DeadZoneAxis(2, joyInfo.Z);
			if (_hasRudder)
				DeadZoneAxis(3, joyInfo.Rudder);
			if (_axisCount > 4)
				DeadZoneAxis(4, joyInfo.Axis5);
			if (_axisCount > 5)
				DeadZoneAxis(5, joyInfo.Axis6);

			if (_hasPOV)
			{
				// Get POV data.
				_povValues[0] = joyInfo.POV;

				if (joyInfo.POV == -1)
					_povDirections[0] = JoystickDirection.Center;

				// Determine direction.
				if ((joyInfo.POV < 18000) && (joyInfo.POV > 9000))
					_povDirections[0] = JoystickDirection.Down | JoystickDirection.Right;
				if ((joyInfo.POV > 18000) && (joyInfo.POV < 27000))
					_povDirections[0] = JoystickDirection.Down | JoystickDirection.Left;
				if ((joyInfo.POV > 27000) && (joyInfo.POV < 36000))
					_povDirections[0] = JoystickDirection.Up | JoystickDirection.Left;
				if ((joyInfo.POV > 0) && (joyInfo.POV < 9000))
					_povDirections[0] = JoystickDirection.Up | JoystickDirection.Right;

				if (joyInfo.POV == 18000)
					_povDirections[0] = JoystickDirection.Down;
				if (joyInfo.POV == 0)
					_povDirections[0] = JoystickDirection.Up;
				if (joyInfo.POV == 9000)
					_povDirections[0] = JoystickDirection.Right;
				if (joyInfo.POV == 27000)
					_povDirections[0] = JoystickDirection.Left;
			}

			// Update buttons.
			for (int i = 0; i < _buttonCount; i++)
			{
				if ((joyInfo.Buttons & (JoystickButtons)(1 << i)) != 0)
					_buttons[i] = true;
				else
					_buttons[i] = false;
			}
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			_bound = false;
			_acquired = false;
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			// Default the deadzone threshold.
			_bound = true;
		}

		/// <summary>
		/// Function to release the input device.
		/// </summary>
		public override void Release()
		{
			_acquired = false;
		}

		/// <summary>
		/// Function to acquire the input device.
		/// </summary>
		public override void Acquire()
		{
			_acquired = true;
			_deviceLost = false;
		}

		/// <summary>
		/// Function to forcefully bind the device.
		/// </summary>
		public void Bind()
		{
			BindDevice();
			_bound = true;
			_acquired = true;
		}

		/// <summary>
		/// Function to forcefully unbind the device.
		/// </summary>
		public void Unbind()
		{
			UnbindDevice();
			_bound = false;
			_acquired = false;
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
		public GorgonJoystick(int ID, string name, JOYCAPS caps, int threshold, InputDevices owner)
			: base(name, owner)
		{
			// Get joystick info.
			_joystickID = ID;
			_axisCount = (int)caps.AxisCount;
			_axisRanges = new MinMaxRange[_axisCount];

			// Initialize the ranges.
			for (int i = 0; i < _axisCount; i++)
				_axisRanges[i] = MinMaxRange.Empty;

			// X axis range.
			_axisRanges[0] = new MinMaxRange((int)caps.MinimumX, (int)caps.MaximumX);
			// Y axis range.
			_axisRanges[1] = new MinMaxRange((int)caps.MinimumY, (int)caps.MaximumY);
			// Buttons.
			_buttonCount = (int)caps.ButtonCount;

			// Manufacturuer/product.
			_manufacturerID = caps.ManufacturerID;
			_productID = caps.ProductID;

			// Determine capabilities.
			if ((caps.Capabilities & JoystickCapabilities.HasRudder) != 0)
			{
				_axisRanges[3] = new MinMaxRange((int)caps.MinimumRudder, (int)caps.MaximumRudder);
				_hasRudder = true;
			}
			if ((caps.Capabilities & JoystickCapabilities.HasPOV) != 0)
			{
				_hasPOV = true;
				if ((caps.Capabilities & JoystickCapabilities.POVContinuousDegreeBearings) != 0)
					_unrestrictedPOV = true;
				if ((caps.Capabilities & JoystickCapabilities.POV4Directions) != 0)
					_POV4Directions = true;
			}
			if ((caps.Capabilities & JoystickCapabilities.HasZ) != 0)
			{
				_hasZ = true;
				_axisRanges[2] = new MinMaxRange((int)caps.MinimumZ, (int)caps.MaximumZ);
			}
			if ((caps.Capabilities & JoystickCapabilities.HasU) != 0)
				_axisRanges[4] = new MinMaxRange((int)caps.Axis5Minimum, (int)caps.Axis5Maximum);
			if ((caps.Capabilities & JoystickCapabilities.HasV) != 0)
				_axisRanges[5] = new MinMaxRange((int)caps.Axis6Minimum, (int)caps.Axis6Maximum);

			// Create value arrays.
			_axisValues = new float[_axisCount];
			_axisDirections = new JoystickDirection[_axisCount];
			_deadZone = new MinMaxRange[_axisCount];
			for (int i = 0; i < _axisCount; i++)
			{
				_axisDirections[i] = JoystickDirection.Center;
				_deadZone[i] = MinMaxRange.Empty;
			}
			_povDirections = new JoystickDirection[1];
			_povCount = 1;
			_povValues = new int[1];
			_povDirections[0] = JoystickDirection.Center;			
			_buttons = new bool[_buttonCount];
		}
		#endregion
	}
}
