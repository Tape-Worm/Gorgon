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
using GI = SharpDX.DXGI;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A video mode information record.
	/// </summary>
	public struct GorgonVideoMode
		: IEquatable<GorgonVideoMode>
	{
		#region Variables.
		/// <summary>
		/// Width of the mode in pixels.
		/// </summary>
		public int Width;
		/// <summary>
		/// Height of the mode in pixels.
		/// </summary>
		public int Height;								
		/// <summary>
		/// Format of the video mode.
		/// </summary>
		public BufferFormat Format;					
		/// <summary>
		/// Refresh rate numerator.
		/// </summary>
		public int RefreshRateNumerator;						
		/// <summary>
		/// Refresh rate denominator.
		/// </summary>
		public int RefreshRateDenominator;					
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the video mode width and height as a .NET Size value.
		/// </summary>
		public System.Drawing.Size Size
		{
			get
			{
				return new System.Drawing.Size(Width, Height);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Converts between a DXGI mode description and a GorgonVideoMode.
		/// </summary>
		/// <param name="mode">The mode to convert.</param>
		/// <returns>The DXGI mode.</returns>
		internal static GI.ModeDescription Convert(GorgonVideoMode mode)
		{
			return new GI.ModeDescription(mode.Width, mode.Height, new GI.Rational(mode.RefreshRateNumerator, mode.RefreshRateDenominator), (GI.Format)mode.Format);
		}

		/// <summary>
		/// Converts between a DXGI mode description and a GorgonVideoMode.
		/// </summary>
		/// <param name="mode">The mode to convert.</param>
		/// <returns>The DXGI mode.</returns>
		internal static GorgonVideoMode Convert(GI.ModeDescription mode)
		{
			return new GorgonVideoMode(mode.Width, mode.Height, (BufferFormat)mode.Format, mode.RefreshRate.Numerator, mode.RefreshRate.Denominator);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="mode1">A video mode to compare.</param>
		/// <param name="mode2">A video mode to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonVideoMode mode1, GorgonVideoMode mode2)
		{
			return GorgonVideoMode.Equals(ref mode1, ref mode2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="mode1">A video mode to compare.</param>
		/// <param name="mode2">A video mode to compare.</param>
		/// <returns>TRUE if not equal, FALSE if equal.</returns>
		public static bool operator !=(GorgonVideoMode mode1, GorgonVideoMode mode2)
		{
			return !GorgonVideoMode.Equals(ref mode1, ref mode2);
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
			unchecked
			{
				return 281.GenerateHash(Width).GenerateHash(Height).GenerateHash(RefreshRateDenominator).GenerateHash(RefreshRateNumerator).GenerateHash(Format);
			}
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
				return Equals((GorgonVideoMode)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Function to determine if two video modes are equal.
		/// </summary>
		/// <param name="left">Left video mode to compare.</param>
		/// <param name="right">Right video mode to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonVideoMode left, ref GorgonVideoMode right)
		{
			return (left.Width == right.Width) && (left.Height == right.Height) && (left.Format == right.Format) &&
					(left.RefreshRateDenominator == right.RefreshRateDenominator) && (left.RefreshRateNumerator == right.RefreshRateNumerator);
		}

		/// <summary>
		/// Method to set the width and height of the video mode.
		/// </summary>
		/// <param name="size">Width and height of the video mode, in pixels.</param>
		public void SetSize(System.Drawing.Size size)
		{
			Width = size.Width;
			Height = size.Height;
		}

		/// <summary>
		/// Method to set the width and height of the video mode.
		/// </summary>
		/// <param name="width">Width of the video mode, in pixels.</param>
		/// <param name="height">Height of the video mode, in pixels.</param>
		public void SetSize(int width, int height)
		{
			Width = width;
			Height = height;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="width">The width of the video mode.</param>
		/// <param name="height">The height of the video mode.</param>
		/// <param name="format">The format for the video mode.</param>
		/// <param name="refreshNumerator">The refresh rate numerator.</param>
		/// <param name="refreshDenominator">The refresh rate denominator.</param>
		public GorgonVideoMode(int width, int height, BufferFormat format, int refreshNumerator, int refreshDenominator)
		{
			Width = width;
			Height = height;
			RefreshRateNumerator = refreshNumerator;
			RefreshRateDenominator = refreshDenominator;
			Format = format;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="width">The width of the video mode.</param>
		/// <param name="height">The height of the video mode.</param>
		/// <param name="format">The format for the video mode.</param>
		public GorgonVideoMode(int width, int height, BufferFormat format)
			: this(width, height, format, 0, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="width">The width of the new video mode.</param>
		/// <param name="height">The height of the new video mode.</param>
		/// <param name="mode">The previous mode to copy settings from.</param>
		/// <remarks>Use this to create a new video mode with the specified with and height.</remarks>
		public GorgonVideoMode(int width, int height, GorgonVideoMode mode)
			: this(width, height, mode.Format, mode.RefreshRateNumerator, mode.RefreshRateDenominator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="size">The size of the new video mode.</param>
		/// <param name="mode">The previous mode to copy settings from.</param>
		/// <remarks>Use this to create a new video mode with the specified with and height.</remarks>
		public GorgonVideoMode(System.Drawing.Size size, GorgonVideoMode mode)
			: this(size.Width, size.Height, mode.Format, mode.RefreshRateNumerator, mode.RefreshRateDenominator)
		{
		}
		#endregion

		#region IEquatable<GorgonVideoMode> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonVideoMode other)
		{
			return GorgonVideoMode.Equals(ref this, ref other);
		}
		#endregion
	}
}
