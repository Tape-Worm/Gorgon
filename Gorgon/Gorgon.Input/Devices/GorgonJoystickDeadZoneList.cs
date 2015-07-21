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
using Gorgon.Core;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	sealed class GorgonJoystickDeadZoneList
		: IGorgonJoystickDeadZoneList
	{
		#region Variables.
		// The list of ranges.
		private readonly GorgonRange[] _ranges;
		// The list of axes associated with the ranges.
		private readonly Dictionary<JoystickAxis, int> _axes = new Dictionary<JoystickAxis, int>(new GorgonJoystickAxisEqualityComparer());
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickAxisRangeList"/> class.
		/// </summary>
		/// <param name="axesRanges">A list of ranges for axes.</param>
		public GorgonJoystickDeadZoneList(IList<KeyValuePair<int, GorgonRange>> axesRanges)
		{
			Type enumType = typeof(JoystickAxis);

			_ranges = new GorgonRange[axesRanges.Count];

			for (int index = 0; index < axesRanges.Count; index++)
			{
				KeyValuePair<int, GorgonRange> range = axesRanges[index];

				if (Enum.IsDefined(enumType, range.Key))
				{
					_axes.Add((JoystickAxis)range.Key, index);
				}

				_ranges[index] = range.Value;
			}
		}
		#endregion

		#region IGorgonJoystickAxisRangeList Members
		/// <inheritdoc/>
		public GorgonRange this[JoystickAxis axis]
		{
			get
			{
				int index;

#if DEBUG
				if (!_axes.TryGetValue(axis, out index))
				{
					throw new KeyNotFoundException(Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif
				return _ranges[index];
			}
			set
			{
				int index;

#if DEBUG
				if (!_axes.TryGetValue(axis, out index))
				{
					throw new KeyNotFoundException(Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif
				_ranges[index] = value;
			}
		}
		#endregion

		#region IList<GorgonRange> Members
		/// <inheritdoc/>
		int IList<GorgonRange>.IndexOf(GorgonRange item)
		{
			return Array.IndexOf(_ranges, item);
		}

		/// <inheritdoc/>
		void IList<GorgonRange>.Insert(int index, GorgonRange item)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		void IList<GorgonRange>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Property to set or return a dead zone for the axis at the given index.
		/// </summary>
		public GorgonRange this[int index]
		{
			get
			{
#if DEBUG
				if ((index < 0) || (index >= _ranges.Length))
				{
					throw new ArgumentOutOfRangeException(nameof(index), Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif

				return _ranges[index];
			}
			set
			{
#if DEBUG
				if ((index < 0) || (index >= _ranges.Length))
				{
					throw new ArgumentOutOfRangeException(nameof(index), Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif
				_ranges[index] = value;
			}
		}

		#endregion

		#region ICollection<GorgonRange> Members
		/// <inheritdoc/>
		void ICollection<GorgonRange>.Add(GorgonRange item)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		void ICollection<GorgonRange>.Clear()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public bool Contains(GorgonRange item)
		{
			return Array.IndexOf(_ranges, item) != -1;
		}

		/// <inheritdoc/>
		void ICollection<GorgonRange>.CopyTo(GorgonRange[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Property to return the number of dead zone axes.
		/// </summary>
		public int Count => _ranges.Length;

		/// <inheritdoc/>
		bool ICollection<GorgonRange>.IsReadOnly => true;

		/// <inheritdoc/>
		bool ICollection<GorgonRange>.Remove(GorgonRange item)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region IEnumerable<GorgonRange> Members
		/// <inheritdoc/>
		public IEnumerator<GorgonRange> GetEnumerator()
		{
			return ((IEnumerable<GorgonRange>)_ranges).GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _ranges.GetEnumerator();
		}
		#endregion
	}
}
