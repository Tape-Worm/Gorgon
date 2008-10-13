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
// Created: Thursday, October 12, 2006 3:43:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object that will represent joystick data.
	/// </summary>
	public abstract class Joystick
		: InputDevice 
	{
		#region Variables.
		private float[] _axisValues = null;			// Axis values.
		private bool[] _buttons = null;				// Button values.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of POV hats.
		/// </summary>
		public int POVCount
		{
			get;
			protected set;
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
			get;
			protected set;
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
			protected set
			{
				_axisValues = value;
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
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the point of view values.
		/// </summary>
		public int[] POV
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the POV direction.
		/// </summary>
		public JoystickDirections[] POVDirection
		{
			get;
			protected set;
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
			protected set
			{
				_buttons = value;
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
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the number of axes on the joystick.
		/// </summary>
		public int AxisCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the number of buttons on the joystick.
		/// </summary>
		public int ButtonCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the manufacturer ID.
		/// </summary>
		public int ManufacturerID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the product ID.
		/// </summary>
		public int ProductID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return a range for particular axis.
		/// </summary>
		public MinMaxRange[] AxisRanges
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the device has a rudder or not.
		/// </summary>
		public bool HasRudder
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the device has a POV hat or not.
		/// </summary>
		public bool HasPOV
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return if the POV hat only has 4 directions or not.
		/// </summary>
		public bool POVHas4Directions
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the POV hat is limited to 5 positions or is free floating.
		/// </summary>
		public bool UnrestrictedPOV
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the joystick has a Z axis or not.
		/// </summary>
		public bool HasZAxis
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the data for a specific axis.
		/// </summary>
		/// <param name="index">Index of the axis to set.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>Axis data.</returns>
		protected void SetAxisData(int index, float value)
		{
			_axisValues[index] = value;
		}

		/// <summary>
		/// Function to set the pressed value for a specific button.
		/// </summary>
		/// <param name="index">Index of the button to set.</param>
		/// <param name="value">Value to set.</param>
		protected void SetButtonValue(int index, bool value)
		{
			_buttons[index] = value;
		}

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
			int range = AxisRanges[axis].Range / 2;		// Axis range.
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
			Name = name;
		}
		#endregion
	}
}
