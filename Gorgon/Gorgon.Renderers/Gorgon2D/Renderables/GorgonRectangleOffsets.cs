#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 8, 2017 7:22:29 PM
// 
#endregion
using System;
using DX = SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Defines the offsets for each corner of a rectangle.
	/// </summary>
	public class GorgonRectangleOffsets
		: IEquatable<GorgonRectangleOffsets>
	{
		#region Variables.
		// The color of the upper left corner.
		private DX.Vector3 _upperLeft;
		// The color of the upper right corner.
		private DX.Vector3 _upperRight;
		// The color of the lower left corner.
		private DX.Vector3 _lowerLeft;
		// The color of the lower right corner.
		private DX.Vector3 _lowerRight;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the offsets in this list have changed or not.
		/// </summary>
		internal bool HasChanged
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset of the upper left corner.
		/// </summary>
		public DX.Vector3 UpperLeft
		{
			get => _upperLeft;
			set
			{
				if (_upperLeft.Equals(ref value))
				{
					return;
				}

				_upperLeft = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the upper right corner.
		/// </summary>
		public DX.Vector3 UpperRight
		{
			get => _upperRight;
			set
			{
				if (_upperRight.Equals(ref value))
				{
					return;
				}

				_upperRight = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the lower left corner.
		/// </summary>
		public DX.Vector3 LowerLeft
		{
			get => _lowerLeft;
			set
			{
				if (_lowerLeft.Equals(ref value))
				{
					return;
				}

				_lowerLeft = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the lower right corner.
		/// </summary>
		public DX.Vector3 LowerRight
		{
			get => _lowerRight;
			set
			{
				if (_lowerRight.Equals(ref value))
				{
					return;
				}

				_lowerRight = value;
				HasChanged = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to assign a single offset to all corners.
		/// </summary>
		/// <param name="offset">The offset to assign.</param>
		public void SetAll(DX.Vector3 offset)
		{
			LowerLeft = LowerRight = UpperLeft = UpperRight = offset;
		}

		/// <summary>
		/// Function to copy the offsets into the specified destination.
		/// </summary>
		/// <param name="destination">The destination that will receive the copy of the offsets.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
		public void CopyTo(GorgonRectangleOffsets destination)
		{
			if (destination == null)
			{
				throw new ArgumentNullException(nameof(destination));
			}

			destination.LowerLeft = LowerLeft;
			destination.LowerRight = LowerRight;
			destination.UpperRight = UpperRight;
			destination.UpperLeft = UpperLeft;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		public bool Equals(GorgonRectangleOffsets other)
		{
			if (other == null)
			{
				return false;
			}

			return other._lowerLeft.Equals(ref _lowerLeft)
			       && other._lowerRight.Equals(ref _lowerRight)
			       && other._upperLeft.Equals(ref _upperLeft)
			       && other._upperRight.Equals(ref _upperRight);
		}
		#endregion
	}
}
