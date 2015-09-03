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
// Created: Sunday, July 5, 2015 1:22:12 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	public sealed class GorgonJoystickAxisList
		: IReadOnlyList<GorgonJoystickAxis>
	{
		#region Variables.
		// The list of available axes.
		private readonly List<JoystickAxis> _axisList = new List<JoystickAxis>();
		// The list of axis values.
		private readonly Dictionary<JoystickAxis, GorgonJoystickAxis> _axes = new Dictionary<JoystickAxis, GorgonJoystickAxis>(new GorgonJoystickAxisEqualityComparer());
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of axes supported on this joystick.
		/// </summary>
		public int Count => _axisList.Count;

		/// <summary>
		/// Property to return a <see cref="GorgonJoystickAxis"/> for a given axis.
		/// </summary>
		public GorgonJoystickAxis this[JoystickAxis axis] => _axes[axis];

		/// <summary>
		/// Property to return a <see cref="GorgonJoystickAxis"/> at the specified index.
		/// </summary>
		public GorgonJoystickAxis this[int index]
		{
			get
			{
#if DEBUG
				if ((index < 0) || (index >= _axisList.Count))
				{
					throw new ArgumentOutOfRangeException(nameof(index), Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif
				return _axes[_axisList[index]];
			}
		}
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public IEnumerator<GorgonJoystickAxis> GetEnumerator()
		{
			return _axes.Values.GetEnumerator();
		}


		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_axes.Values).GetEnumerator();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickAxisList"/> class.
		/// </summary>
		/// <param name="info">Information about the joystick.</param>
		internal GorgonJoystickAxisList(IGorgonJoystickInfo2 info)
		{
			foreach (GorgonJoystickAxisInfo axis in info.AxisInfo)
			{
				_axisList.Add(axis.Axis);
				_axes[axis.Axis] = new GorgonJoystickAxis(axis.Axis);
			}
		}
		#endregion
	}
}
