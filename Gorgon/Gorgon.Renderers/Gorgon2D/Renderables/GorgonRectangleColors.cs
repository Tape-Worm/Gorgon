﻿#region MIT
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
	{
		#region Variables.
        // The renderable that owns this object.
	    private readonly BatchRenderable _renderable;
		#endregion 

		#region Properties.
		/// <summary>
		/// Property to set or return the color of the upper left corner.
		/// </summary>
		public GorgonColor UpperLeft
		{
			get => _renderable.UpperLeftColor;
			set
			{
				if (GorgonColor.Equals(in value, in _renderable.UpperLeftColor))
				{
					return;
				}

				_renderable.UpperLeftColor = value;
				_renderable.HasVertexColorChanges = true;
			}
		}

		/// <summary>
		/// Property to set or return the color of the upper right corner.
		/// </summary>
		public GorgonColor UpperRight
		{
			get => _renderable.UpperRightColor;
			set
			{
				if (GorgonColor.Equals(in value, in _renderable.UpperRightColor))
				{
					return;
				}

			    _renderable.UpperRightColor = value;
			    _renderable.HasVertexColorChanges = true;
			}
		}

		/// <summary>
		/// Property to set or return the color of the lower left corner.
		/// </summary>
		public GorgonColor LowerLeft
		{
			get => _renderable.LowerLeftColor;
			set
			{
				if (GorgonColor.Equals(in value, in _renderable.LowerLeftColor))
				{
					return;
				}

				_renderable.LowerLeftColor = value;
				_renderable.HasVertexColorChanges = true;
			}
		}

		/// <summary>
		/// Property to set or return the color of the lower right corner.
		/// </summary>
		public GorgonColor LowerRight
		{
			get => _renderable.LowerRightColor;
			set
			{
				if (GorgonColor.Equals(in value, in _renderable.LowerRightColor))
				{
					return;
				}

			    _renderable.LowerRightColor = value;
				_renderable.HasVertexColorChanges = true;
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
		    if ((_renderable.LowerLeftColor.Equals(in color))
		        && (_renderable.LowerRightColor.Equals(in color))
		        && (_renderable.UpperLeftColor.Equals(in color))
		        && (_renderable.UpperRightColor.Equals(in color)))
		    {
		        return;
		    }

		    _renderable.LowerLeftColor = _renderable.LowerRightColor = _renderable.UpperRightColor = _renderable.UpperLeftColor = color;
		    _renderable.HasVertexColorChanges = true;
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
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRectangleColors"/> class.
        /// </summary>
        /// <param name="defaultColor">The default color for the corners.</param>
        /// <param name="renderable">The renderable that owns this object.</param>
        internal GorgonRectangleColors(GorgonColor defaultColor, BatchRenderable renderable)
        {
            _renderable = renderable;
			SetAll(defaultColor);
		}
		#endregion
	}
}