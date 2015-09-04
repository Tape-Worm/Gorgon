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
// Created: Wednesday, August 12, 2015 9:29:01 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Input
{
	/// <summary>
	/// A representation of a joystick axis.
	/// </summary>
	public class GorgonJoystickAxis
	{
		#region Variables.
		// The value for the axis.
		private int _value;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the identifier for the joystick axis.
		/// </summary>
		public JoystickAxis Axis
		{
			get;
		}

		/// <summary>
		/// Property to return the value representing the position of a joystick axis.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value will be constrained by the <see cref="GorgonJoystickAxisInfo.Range"/>, which will indicate the physical value range for the axis.
		/// </para>
		/// <para>
		/// When a <see cref="DeadZone"/> is applied to the axis, the value will remain at 0 (i.e. centered) until it exits the dead zone range.
		/// </para>
		/// </remarks>
		public int Value
		{
			get
			{
				return !DeadZone.Contains(_value) ? _value : 0;
			}
			set
			{
				_value = value;
			}
		}

		/// <summary>
		/// Property to set or return the dead zone value for the axis.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will apply a dead zone area for the axis. If the <see cref="Value"/> for the axis position is within this dead zone range, the <see cref="Value"/> property will return 0 until it 
		/// exceeds the range specified. 
		/// </para>
		/// <para>
		/// Specify <see cref="GorgonRange.Empty"/> to disable the dead zone on the axis.
		/// </para>
		/// </remarks>
		public GorgonRange DeadZone
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickAxis"/> class.
		/// </summary>
		/// <param name="axis">The name of the axis on the joystick.</param>
		internal GorgonJoystickAxis(JoystickAxis axis)
		{
			Axis = axis;
		}
		#endregion
	}
}
