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
using System.Collections.Generic;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	sealed class GorgonJoystickAxisValueList
		: IGorgonJoystickAxisValueList
	{
		#region Variables.
		// The list of ranges.
		private readonly int[] _ranges;
		// The list of axes associated with the ranges.
		private readonly Dictionary<JoystickAxis, int> _axes = new Dictionary<JoystickAxis, int>(new GorgonJoystickAxisEqualityComparer());
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickAxisValueList"/> class.
		/// </summary>
		/// <param name="axesValues">A list of initial values for axes.</param>
		public GorgonJoystickAxisValueList(IList<KeyValuePair<int, int>> axesValues)
		{
			Type enumType = typeof(JoystickAxis);

			_ranges = new int[axesValues.Count];

			for (int index = 0; index < axesValues.Count; index++)
			{
				KeyValuePair<int, int> range = axesValues[index];

				if (Enum.IsDefined(enumType, range.Key))
				{
					_axes.Add((JoystickAxis)range.Key, index);
				}

				_ranges[index] = range.Value;
			}
		}
		#endregion

		#region IGorgonJoystickAxisValueList Members
		/// <inheritdoc/>
		public int this[JoystickAxis axis]
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

		#region IList<int> Members
		/// <inheritdoc/>
		int IList<int>.IndexOf(int item)
		{
			return Array.IndexOf(_ranges, item);
		}

		/// <inheritdoc/>
		void IList<int>.Insert(int index, int item)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		void IList<int>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets or sets the value for a gaming device axis at the specified index.
		/// </summary>
		public int this[int index]
		{
			get
			{
#if DEBUG
				if ((index < 0) || (index >= _ranges.Length))
				{
					throw new ArgumentOutOfRangeException("index", Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif

				return _ranges[index];
			}
			set
			{
#if DEBUG
				if ((index < 0) || (index >= _ranges.Length))
				{
					throw new ArgumentOutOfRangeException("index", Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif

				_ranges[index] = value;
			}
		}
		#endregion

		#region ICollection<int> Members
		/// <inheritdoc/>
		void ICollection<int>.Add(int item)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		void ICollection<int>.Clear()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		bool ICollection<int>.Contains(int item)
		{
			return Array.IndexOf(_ranges, item) != -1;
		}

		/// <inheritdoc/>
		void ICollection<int>.CopyTo(int[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Property to return the number of axis values in this list.
		/// </summary>
		public int Count
		{
			get
			{
				return _ranges.Length;
			}
		}

		/// <inheritdoc/>
		bool ICollection<int>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <inheritdoc/>
		bool ICollection<int>.Remove(int item)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region IEnumerable<int> Members
		/// <inheritdoc/>
		public IEnumerator<int> GetEnumerator()
		{
			return ((IEnumerable<int>)_ranges).GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		/// <inheritdoc/>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _ranges.GetEnumerator();
		}
		#endregion
	}
}
