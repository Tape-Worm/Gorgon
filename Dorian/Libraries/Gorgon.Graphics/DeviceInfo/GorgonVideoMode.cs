#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, July 25, 2011 8:08:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A video mode information record.
	/// </summary>
	public struct GorgonVideoMode
	{
		#region Variables.
		private int _width;									// Width of the mode in pixels.
		private int _height;								// Height of the mode in pixels.
		private GorgonBufferFormat _format;					// Format of the video mode.
		private int _refreshNumerator;						// Refresh rate numerator.
		private int _refreshDenominator;					// Refresh rate denominator.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the width of the video mode in pixels.
		/// </summary>
		public int Width
		{
			get
			{
				return _width;
			}
		}

		/// <summary>
		/// Property to return the height of the video mode in pixels.
		/// </summary>
		public int Height
		{
			get
			{
				return _height;
			}
		}

		/// <summary>
		/// Property to return the format of the video mode.
		/// </summary>
		public GorgonBufferFormat Format
		{
			get
			{
				return _format;
			}
		}

		/// <summary>
		/// Property to return the numerator of the refresh rate.
		/// </summary>
		public int RefreshRateNumerator
		{
			get
			{
				return _refreshNumerator;
			}
		}

		/// <summary>
		/// Property to return the denominator of the refresh rate.
		/// </summary>
		public int RefreshRateDenominator
		{
			get
			{
				return _refreshDenominator;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="mode1">A video mode to compare.</param>
		/// <param name="mode2">A video mode to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonVideoMode mode1, GorgonVideoMode mode2)
		{
			return mode1.Equals(mode2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="mode1">A video mode to compare.</param>
		/// <param name="mode2">A video mode to compare.</param>
		/// <returns>TRUE if not equal, FALSE if equal.</returns>
		public static bool operator !=(GorgonVideoMode mode1, GorgonVideoMode mode2)
		{
			return !(mode1 == mode2);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Gorgon Video Mode: {0}x{1} Refresh Num/Denom: {2}/{3} Format: {4}", Width, Height, RefreshRateNumerator, RefreshRateDenominator, Format.ToString());
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Width.GetHashCode() ^ Height.GetHashCode() ^ RefreshRateNumerator.GetHashCode() ^ RefreshRateDenominator.GetHashCode() ^ Format.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonVideoMode)
			{
				GorgonVideoMode videoMode = (GorgonVideoMode)obj;

				return ((videoMode.Width == Width) && (videoMode.Height == Height) && (videoMode.RefreshRateNumerator == RefreshRateNumerator) && (videoMode.RefreshRateDenominator == RefreshRateDenominator) && (videoMode.Format == Format));
			}

			return false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="width">The width of the video mode.</param>
		/// <param name="height">The height of the video mode.</param>
		/// <param name="refreshNumerator">The refresh rate numerator.</param>
		/// <param name="refreshDenominator">The refresh rate denominator.</param>
		/// <param name="format">The format for the video mode.</param>
		public GorgonVideoMode(int width, int height, int refreshNumerator, int refreshDenominator, GorgonBufferFormat format)
		{
			_width = width;
			_height = height;
			_refreshNumerator = refreshNumerator;
			_refreshDenominator = refreshDenominator;
			_format = format;
		}
		#endregion
	}
}
