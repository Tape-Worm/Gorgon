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
using System.Linq;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	public sealed class GorgonJoystickAxisInfoList
		: IGorgonJoystickAxisInfoList
	{
		#region Variables.
		// The list of available axes.
		private readonly List<JoystickAxis> _axisList = new List<JoystickAxis>();
		// The list of information about each axis.
		private readonly Dictionary<JoystickAxis, GorgonJoystickAxisInfo> _infoList = new Dictionary<JoystickAxis, GorgonJoystickAxisInfo>(new GorgonJoystickAxisEqualityComparer());
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an axis to the list.
		/// </summary>
		/// <param name="axis">Axis to add to the list.</param>
		internal void Add(GorgonJoystickAxisInfo axis)
		{
			if (_infoList.ContainsKey(axis.Axis))
			{
				return;
			}

			_axisList.Add(axis.Axis);
			_infoList[axis.Axis] = axis;
		}
		#endregion

		#region IGorgonJoystickAxisInfoList Members
		/// <inheritdoc/>
		public GorgonJoystickAxisInfo this[JoystickAxis axis] => _infoList[axis];

		#endregion

		#region IReadOnlyList<GorgonJoystickAxisInfo> Members
		/// <summary>
		/// Gets the <see cref="GorgonJoystickAxisInfo"/> at the specified index in the axis list.
		/// </summary>
		public GorgonJoystickAxisInfo this[int index]
		{
			get
			{
#if DEBUG
				if ((index < 0) || (index >= _axisList.Count))
				{
					throw new ArgumentOutOfRangeException(nameof(index), Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif
				return _infoList[_axisList[index]];
			}
		}
		#endregion

		#region IReadOnlyCollection<GorgonJoystickAxisInfo> Members
		/// <inheritdoc/>
		public int Count => _axisList.Count;

		#endregion

		#region IEnumerable<GorgonJoystickAxisInfo> Members
		/// <inheritdoc/>
		public IEnumerator<GorgonJoystickAxisInfo> GetEnumerator()
		{
			return _infoList.Select(item => item.Value).GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_infoList.Values).GetEnumerator();
		}
		#endregion
	}
}
