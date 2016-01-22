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
using DXGI = SharpDX.DXGI;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Indicates the method used to create an image on a display surface.
	/// </summary>
	public enum VideoModeScanlineOrder
	{
		/// <summary>
		/// No scanline ordering is specified.
		/// </summary>
		Unspecified = DXGI.DisplayModeScanlineOrder.Unspecified,
		/// <summary>
		/// The image is created from the first scanline to the last without skipping.
		/// </summary>
		Progressive = DXGI.DisplayModeScanlineOrder.Progressive,
		/// <summary>
		/// The image is created starting with the upper field.
		/// </summary>
		UpperFieldFirst = DXGI.DisplayModeScanlineOrder.UpperFieldFirst,
		/// <summary>
		/// The image is created starting with the lower field.
		/// </summary>
		LowerFieldFirst = DXGI.DisplayModeScanlineOrder.LowerFieldFirst
	}

	/// <summary>
	/// Indicates how an image is stretched to fit the specified video mode resolution.
	/// </summary>
	public enum VideoModeDisplayModeScaling
	{
		/// <summary>
		/// No scaling was specified.
		/// </summary>
		Unspecified = DXGI.DisplayModeScaling.Unspecified,
		/// <summary>
		/// <para>
		/// No scaling is performed. 
		/// </para>
		/// <para>
		/// The image is centered on the display. This is common for fixed dot pitch displays such as LED displays.
		/// </para>
		/// </summary>
		Centered = DXGI.DisplayModeScaling.Centered,
		/// <summary>
		/// The image is stretched to fill the display.
		/// </summary>
		Stretched = DXGI.DisplayModeScaling.Stretched
	}

	/// <summary>
	/// Contains information about a full screen video mode.
	/// </summary>
	public struct GorgonVideoMode
		: IEquatable<GorgonVideoMode>
	{
		#region Variables.
		/// <summary>
		/// Width of the mode in pixels.
		/// </summary>
		public readonly int Width;
		/// <summary>
		/// Height of the mode in pixels.
		/// </summary>
		public readonly int Height;								
		/// <summary>
		/// Format of the video mode.
		/// </summary>
		public readonly BufferFormat Format;					
		/// <summary>
		/// Refresh rate numerator.
		/// </summary>
		public readonly GorgonRationalNumber RefreshRate;
		/// <summary>
		/// The scanline order for this video mode.
		/// </summary>
		public readonly VideoModeScanlineOrder ScanlineOrdering;
		/// <summary>
		/// The scaling mode for the video mode.
		/// </summary>
		public readonly VideoModeDisplayModeScaling Scaling;
		#endregion

		#region Methods.
		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="mode1">A video mode to compare.</param>
		/// <param name="mode2">A video mode to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonVideoMode mode1, GorgonVideoMode mode2)
		{
			return Equals(ref mode1, ref mode2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="mode1">A video mode to compare.</param>
		/// <param name="mode2">A video mode to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonVideoMode mode1, GorgonVideoMode mode2)
		{
			return !Equals(ref mode1, ref mode2);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
		    return string.Format(Resources.GORGFX_TOSTR_VIDEOMODE,
		                         Width,
		                         Height,
		                         RefreshRate.Numerator,
		                         RefreshRate.Denominator,
		                         Format);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Width).GenerateHash(Height).GenerateHash(RefreshRate).GenerateHash(Format);
		}

		/// <summary>
		/// Determines whether the specified <see cref="object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<b>true</b> if the specified <see cref="object"/> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
		    if (obj is GorgonVideoMode)
		    {
		        return Equals((GorgonVideoMode)obj);
		    }

		    return base.Equals(obj);
		}

		/// <summary>
		/// Function to determine if two video modes are equal.
		/// </summary>
		/// <param name="left">Left video mode to compare.</param>
		/// <param name="right">Right video mode to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonVideoMode left, ref GorgonVideoMode right)
		{
			return (left.Width == right.Width) && (left.Height == right.Height) && (left.Format == right.Format) &&
			       (left.RefreshRate.Equals(right.RefreshRate) && (left.Scaling == right.Scaling) &&
			        (left.ScanlineOrdering == right.ScanlineOrdering));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="modeDesc">The DXGI mode description used to initialize this value.</param>
		internal GorgonVideoMode(DXGI.ModeDescription modeDesc)
		{
			Width = modeDesc.Width;
			Height = modeDesc.Height;
			RefreshRate = modeDesc.RefreshRate.FromRational();
			Format = (BufferFormat)modeDesc.Format;
			Scaling = (VideoModeDisplayModeScaling)modeDesc.Scaling;
			ScanlineOrdering = (VideoModeScanlineOrder)modeDesc.ScanlineOrdering;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="width">The width of the video mode.</param>
		/// <param name="height">The height of the video mode.</param>
		/// <param name="format">The format for the video mode.</param>
		/// <param name="refreshRate">The refresh rate for the full screen video mode.</param>
		/// <param name="scaling">[Optional] The type of image scaling to use.</param>
		/// <param name="scanlineOrder">[Optional] The type of scanline ordering to use.</param>
		public GorgonVideoMode(int width,
		                       int height,
		                       BufferFormat format,
		                       GorgonRationalNumber refreshRate,
		                       VideoModeDisplayModeScaling scaling = VideoModeDisplayModeScaling.Unspecified,
		                       VideoModeScanlineOrder scanlineOrder = VideoModeScanlineOrder.Unspecified)
		{
			Width = width;
			Height = height;
			RefreshRate = refreshRate;
			Format = format;
			ScanlineOrdering = scanlineOrder;
			Scaling = scaling;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoMode"/> struct.
		/// </summary>
		/// <param name="width">The width of the video mode.</param>
		/// <param name="height">The height of the video mode.</param>
		/// <param name="format">The format for the video mode.</param>
		/// <param name="scaling">[Optional] The type of image scaling to use.</param>
		/// <param name="scanlineOrder">[Optional] The type of scanline ordering to use.</param>
		public GorgonVideoMode(int width,
		                       int height,
		                       BufferFormat format,
		                       VideoModeDisplayModeScaling scaling = VideoModeDisplayModeScaling.Unspecified,
		                       VideoModeScanlineOrder scanlineOrder = VideoModeScanlineOrder.Unspecified)
			: this(width, height, format, GorgonRationalNumber.Empty, scaling, scanlineOrder)
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
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
