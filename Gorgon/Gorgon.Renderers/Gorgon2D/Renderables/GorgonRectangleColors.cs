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
using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Defines the colors for each corner of a rectangle.
	/// </summary>
	public class GorgonRectangleColors
		: IEquatable<GorgonRectangleColors>, IGorgonEquatableByRef<GorgonColor>
	{
		#region Variables.
		// The color of the upper left corner.
		private GorgonColor _upperLeft;
		// The color of the upper right corner.
		private GorgonColor _upperRight;
		// The color of the lower left corner.
		private GorgonColor _lowerLeft;
		// The color of the lower right corner.
		private GorgonColor _lowerRight;
		#endregion 

		#region Properties.
		/// <summary>
		/// Property to set or return whether the colors in this list have changed.
		/// </summary>
		internal bool HasChanged
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color of the upper left corner.
		/// </summary>
		public GorgonColor UpperLeft
		{
			get => _upperLeft;
			set
			{
				if (GorgonColor.Equals(in value, in _upperLeft))
				{
					return;
				}

				_upperLeft = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the color of the upper right corner.
		/// </summary>
		public GorgonColor UpperRight
		{
			get => _upperRight;
			set
			{
				if (GorgonColor.Equals(in value, in _upperRight))
				{
					return;
				}

				_upperRight = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the color of the lower left corner.
		/// </summary>
		public GorgonColor LowerLeft
		{
			get => _lowerLeft;
			set
			{
				if (GorgonColor.Equals(in value, in _lowerLeft))
				{
					return;
				}

				_lowerLeft = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the color of the lower right corner.
		/// </summary>
		public GorgonColor LowerRight
		{
			get => _lowerRight;
			set
			{
				if (GorgonColor.Equals(in value, in _lowerRight))
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
		/// Function to assign a single color to all corners.
		/// </summary>
		/// <param name="color">The color to assign.</param>
		public void SetAll(in GorgonColor color)
		{
			LowerLeft = LowerRight = UpperLeft = UpperRight = color;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		public bool Equals(GorgonRectangleColors other)
		{
			if (other == null)
			{
				return false;
			}

			return GorgonColor.Equals(in other._lowerLeft, in _lowerLeft)
			       && GorgonColor.Equals(in other._lowerRight, in _lowerRight)
			       && GorgonColor.Equals(in other._upperLeft, in _upperLeft)
			       && GorgonColor.Equals(in other._upperRight, in _upperRight);
		}

		/// <summary>
		/// Function to copy the colors into the specified destination.
		/// </summary>
		/// <param name="destination">The destination that will receive the copy of the colors.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
		public void CopyTo(GorgonRectangleColors destination)
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
		/// Function to compare this instance with another.
		/// </summary>
		/// <param name="other">The other instance to use for comparison.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(GorgonColor other)
		{
			return Equals(in other);
		}

		/// <summary>
		/// Function to compare this instance with another.
		/// </summary>
		/// <param name="other">The other instance to use for comparison.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(in GorgonColor other)
		{
			return GorgonColor.Equals(in other, in _lowerLeft)
				   && GorgonColor.Equals(in other, in _lowerRight)
				   && GorgonColor.Equals(in other, in _upperLeft)
				   && GorgonColor.Equals(in other, in _upperRight);
		}
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRectangleColors"/> class.
        /// </summary>
        /// <param name="defaultColor">The default color for the corners.</param>
        public GorgonRectangleColors(GorgonColor defaultColor)
		{
			SetAll(defaultColor);
			HasChanged = true;
		}
		#endregion
	}
}
