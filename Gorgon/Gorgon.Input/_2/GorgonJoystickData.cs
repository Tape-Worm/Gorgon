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
// Created: Wednesday, August 12, 2015 10:11:24 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Input
{
	/// <summary>
	/// Data received from the device layer and transformed into a common data set so that the <see cref="GorgonInputService2"/> can decipher it.
	/// </summary>
	public class GorgonJoystickData
	{
		/// <summary>
		/// Property to indicate whether the joystick is connected or not.
		/// </summary>
		public bool IsConnected
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the bit mask containing the buttons that are currently pressed.
		/// </summary>
		/// <remarks>
		/// Each button is indicated by a single bit in the mask. For example, if the 3rd button is down, then bit #3 is set.
		/// </remarks>
		public int ButtonState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the values for the axes on the joystick.
		/// </summary>

		public Dictionary<JoystickAxis, int> AxisValues
		{
			get;
		}

		/// <summary>
		/// Property to set or return the point of view hat values.
		/// </summary>
		/// <remarks>
		/// This must be a value from 0 - 359.99999f, to get the bearing in degrees, divide by 100.0f.
		/// </remarks>
		public float POVValue
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickData"/> class.
		/// </summary>
		public GorgonJoystickData()
		{
			AxisValues = new Dictionary<JoystickAxis, int>(new GorgonJoystickAxisEqualityComparer());
			POVValue = 1.0f;
		}
	}
}
