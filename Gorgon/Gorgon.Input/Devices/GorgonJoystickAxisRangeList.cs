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
	public sealed class GorgonJoystickAxisRangeList
		: IGorgonJoystickAxisRangeList
	{
		#region Variables.
		// The list of ranges.
		private readonly List<GorgonRange> _ranges = new List<GorgonRange>();
		// The list of axes associated with the ranges.
		private readonly Dictionary<JoystickAxis, int> _axes = new Dictionary<JoystickAxis, int>(new GorgonJoystickAxisEqualityComparer());
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystickAxisRangeList"/> class.
		/// </summary>
		/// <param name="axesRanges">A list of ranges for axes.</param>
		public GorgonJoystickAxisRangeList(IList<KeyValuePair<int, GorgonRange>> axesRanges)
		{
			Type enumType = typeof(JoystickAxis);

			for (int index = 0; index < axesRanges.Count; index++)
			{
				KeyValuePair<int, GorgonRange> range = axesRanges[index];

				if (Enum.IsDefined(enumType, range.Key))
				{
					_axes.Add((JoystickAxis)range.Key, index);
				}

				_ranges.Add(range.Value);
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
		}
		#endregion

		#region IReadOnlyList<GorgonRange> Members
		/// <inheritdoc/>
		public GorgonRange this[int index]
		{
			get
			{
#if DEBUG
				if ((index < 0) || (index >= _ranges.Count))
				{
					throw new ArgumentOutOfRangeException(nameof(index), Resources.GORINP_ERR_JOYSTICK_AXES_INVALID);
				}
#endif

				return _ranges[index];
			}
		}
		#endregion

		#region IReadOnlyCollection<GorgonRange> Members
		/// <inheritdoc/>
		public int Count => _ranges.Count;

		#endregion

		#region IEnumerable<GorgonRange> Members
		/// <inheritdoc/>
		public IEnumerator<GorgonRange> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			return _ranges.GetEnumerator();
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
