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
// Created: Thursday, October 12, 2006 3:43:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object that will represent joystick data.
	/// </summary>
	public abstract class Joystick
		: InputDevice 
	{
		#region Variables.
		private string _name = string.Empty;		// Name of the joystick.

		/// <summary>Axis values.</summary>
		protected float[] _axisValues = null;
		/// <summary>Axis directions.</summary>
		protected JoystickDirections[] _axisDirections = null;
		/// <summary>Values for POV hat.</summary>
		protected int[] _povValues = null;
		/// <summary>Directions for POV hat.</summary>
		protected JoystickDirections[] _povDirections = null;
		/// <summary>Button values.</summary>
		protected bool[] _buttons = null;
		/// <summary>Number of POV hats.</summary>
		protected int _povCount;
		/// <summary>Range of motion for the joystick axes.</summary>
		protected MinMaxRange[] _axisRanges = null;
		/// <summary>Number of axes on the joystick.</summary>
		protected int _axisCount;
		/// <summary>Number of buttons the joystick.</summary>
		protected int _buttonCount;
		/// <summary>Manufacturer ID.</summary>
		protected int _manufacturerID;
		/// <summary>Product ID.</summary>
		protected int _productID;
		/// <summary>Flag to indicate whether the joystick has a rudder or not.</summary>
		protected bool _hasRudder;
		/// <summary>Flag to indicate whether the joystick has a POV hat or not.</summary>
		protected bool _hasPOV;
		/// <summary>Flag to indicate whether the joystick has an unrestricted POV or not.</summary>
		protected bool _unrestrictedPOV;
		/// <summary>Flag to indicate whether the joystick has a Z axis or not.</summary>
		protected bool _hasZ;
		/// <summary>Point of view hat only has 4 directions.</summary>
		protected bool _POV4Directions;
		/// <summary>Dead zone range for each axis.</summary>
		protected MinMaxRange[] _deadZone = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of POV hats.
		/// </summary>
		public int POVCount
		{
			get
			{
				return _povCount;
			}
		}

		/// <summary>
		/// Property to return the dead zone range.
		/// </summary>
		/// <remarks>The dead zone should be within the range of the same axis.  That is, if the axis range is 
		/// between 0..32767, then the dead zone range should be within 0..32767.  Likewise if the axis has a 
		/// range of -32767..32767 then the dead zone value should correspond to the range of -32767..32767.  
		/// <para>Please note that by setting the dead zone to the exact size of the axis range the joystick 
		/// will not register any movement because the entire range is considered 'dead'.</para></remarks>
		public virtual MinMaxRange[] DeadZone
		{
			get
			{
				return _deadZone;
			}
		}

		/// <summary>
		/// Property to return a value for an axis.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.InputDevices.Joystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the axis range will be returned (center position of the axis).</remarks>
		public float[] Axes
		{
			get
			{
				PollJoystick();
				return _axisValues;
			}
		}

		/// <summary>
		/// Property to return the direction that an axis is pointed towards.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.InputDevices.Joystick.DeadZone">DeadZone</see> 
		/// property.  If the axis position is within the dead-zone range only the 
		/// <see cref="GorgonLibrary.InputDevices.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
		public JoystickDirections[] AxisDirection
		{
			get
			{
				return _axisDirections;
			}
		}

		/// <summary>
		/// Property to return the point of view values.
		/// </summary>
		public int[] POV
		{
			get
			{
				return _povValues;
			}
		}

		/// <summary>
		/// Property to return the POV direction.
		/// </summary>
		public JoystickDirections[] POVDirection
		{
			get
			{
				return _povDirections;
			}
		}

		/// <summary>
		/// Property to return a button state.
		/// </summary>
		public bool[] Button
		{
			get
			{
				PollJoystick();
				return _buttons;
			}
		}

		/// <summary>
		/// Property to return the x coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.InputDevices.Joystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the X axis range will be returned (center position of the axis).</remarks>
		public abstract float X
		{
			get;
		}

		/// <summary>
		/// Property to return the y coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.InputDevices.Joystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Y axis range will be returned (center position of the axis).</remarks>
		public abstract float Y
		{
			get;
		}

		/// <summary>
		/// Property to return the z coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.InputDevices.Joystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Z axis range will be returned (center position of the axis).</remarks>
		public abstract float Z
		{
			get;
		}

		/// <summary>
		/// Property to return the rudder coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.InputDevices.Joystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the rudder range will be returned (center position of the axis).</remarks>
		public abstract float Rudder
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the joystick.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// Property to return the number of axes on the joystick.
		/// </summary>
		public int AxisCount
		{
			get
			{
				return _axisCount;
			}
		}

		/// <summary>
		/// Property to return the number of buttons on the joystick.
		/// </summary>
		public int ButtonCount
		{
			get
			{
				return _buttonCount;
			}
		}

		/// <summary>
		/// Property to return the manufacturer ID.
		/// </summary>
		public int ManufacturerID
		{
			get
			{
				return _manufacturerID;
			}
		}

		/// <summary>
		/// Property to return the product ID.
		/// </summary>
		public int ProductID
		{
			get
			{
				return _productID;
			}
		}

		/// <summary>
		/// Property to return a range for particular axis.
		/// </summary>
		public MinMaxRange[] AxisRanges
		{
			get
			{
				return _axisRanges;
			}
		}

		/// <summary>
		/// Property to return whether the device has a rudder or not.
		/// </summary>
		public bool HasRudder
		{
			get
			{
				return _hasRudder;
			}
		}

		/// <summary>
		/// Property to return whether the device has a POV hat or not.
		/// </summary>
		public bool HasPOV
		{
			get
			{
				return _hasPOV;
			}
		}

		/// <summary>
		/// Property to return if the POV hat only has 4 directions or not.
		/// </summary>
		public bool POVHas4Directions
		{
			get
			{
				return _POV4Directions;
			}
		}

		/// <summary>
		/// Property to return whether the POV hat is limited to 5 positions or is free floating.
		/// </summary>
		public bool UnrestrictedPOV
		{
			get
			{
				return _unrestrictedPOV;
			}
		}

		/// <summary>
		/// Property to return whether the joystick has a Z axis or not.
		/// </summary>
		public bool HasZAxis
		{
			get
			{
				return _hasZ;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected abstract void PollJoystick();

		/// <summary>
		/// Function to set or return the dead zone range.
		/// </summary>
		/// <param name="axis">Axis to set.</param>
		/// <param name="deadzoneRange">Range to set for the joystick dead zone.</param>
		public void SetDeadzone(int axis, int deadzoneRange)
		{
			int range = _axisRanges[axis].Range / 2;		// Axis range.
			DeadZone[axis] = new MinMaxRange(range - deadzoneRange, range + deadzoneRange);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the joystick.</param>
		/// <param name="owner">Input interface that owns this joystick.</param>
		protected internal Joystick(string name, Input owner)
			: base(owner)
		{
			_name = name;
		}
		#endregion
	}
}
