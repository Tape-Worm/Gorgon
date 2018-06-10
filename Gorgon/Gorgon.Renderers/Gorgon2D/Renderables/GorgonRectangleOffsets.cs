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
	{
		#region Variables.
        // The renderable object to update.
	    private readonly BatchRenderable _renderable;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the offset of the upper left corner.
		/// </summary>
		public DX.Vector3 UpperLeft
		{
			get => _renderable.UpperLeftOffset;
			set
			{
				if (_renderable.UpperLeftOffset.Equals(ref value))
				{
					return;
				}

			    _renderable.UpperLeftOffset = value;
				_renderable.HasTransformChanges = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the upper right corner.
		/// </summary>
		public DX.Vector3 UpperRight
		{
			get => _renderable.UpperRightOffset;
			set
			{
				if (_renderable.UpperRightOffset.Equals(ref value))
				{
					return;
				}

			    _renderable.UpperRightOffset = value;
				_renderable.HasTransformChanges = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the lower left corner.
		/// </summary>
		public DX.Vector3 LowerLeft
		{
			get => _renderable.LowerLeftOffset;
			set
			{
				if (_renderable.LowerLeftOffset.Equals(ref value))
				{
					return;
				}

			    _renderable.LowerLeftOffset = value;
				_renderable.HasTransformChanges = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the lower right corner.
		/// </summary>
		public DX.Vector3 LowerRight
		{
			get => _renderable.LowerRightOffset;
			set
			{
				if (_renderable.LowerRightOffset.Equals(ref value))
				{
					return;
				}

			    _renderable.LowerRightOffset = value;
				_renderable.HasTransformChanges = true;
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
		    if ((offset.Equals(ref _renderable.LowerLeftOffset))
		        && (offset.Equals(ref _renderable.LowerRightOffset))
		        && (offset.Equals(ref _renderable.UpperRightOffset))
		        && (offset.Equals(ref _renderable.UpperLeftOffset)))
		    {
		        return;
		    }

		    _renderable.LowerLeftOffset = _renderable.LowerRightOffset = _renderable.UpperRightOffset = _renderable.UpperLeftOffset;
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
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRectangleOffsets"/> class.
        /// </summary>
        /// <param name="renderable">The renderable to update.</param>
        internal GorgonRectangleOffsets(BatchRenderable renderable)
	    {
	        _renderable = renderable;
	    }
        #endregion
	}
}
