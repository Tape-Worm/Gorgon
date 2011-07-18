#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Monday, July 11, 2005 5:42:11 PM
// 
#endregion

using System;

namespace GorgonLibrary
{
	/// <summary>
	/// Structure for holding information about a video mode.
	/// </summary>
	/// <remarks>
	/// Since an adapter can have a whole boatload of video modes, we should
	/// have a system of keeping track of the available video modes.
	/// </remarks>
	public struct VideoMode
	{
		#region Variables.
		/// <summary>Width of this video mode.</summary>
		public int Width;
		/// <summary>Height of this video mode.</summary>
		public int Height;
		/// <summary>Refresh Rate of this video mode in Hz.</summary>
		public int RefreshRate;
		/// <summary>Buffer format for this video mode.</summary>
		public BackBufferFormats Format;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the bits per pixel of the video mode.
		/// </summary>
		public int Bpp
		{
			get
			{
				switch (Format)
				{
				case BackBufferFormats.BufferRGB101010A2:
				case BackBufferFormats.BufferRGB888:
					return 32;
				case BackBufferFormats.BufferRGB555:
				case BackBufferFormats.BufferRGB565:
					return 16;
				case BackBufferFormats.BufferP8:
					return 8;
				default:
					return 0;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if another object is equal to this one.
		/// </summary>
		/// <param name="obj">Object to test.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if (obj is VideoMode)
			{
				if (((VideoMode)obj) == this)
					return true;
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Function to return a hash code for this object.
		/// </summary>
		/// <returns>Hash code of the object.</returns>
		public override int GetHashCode()
		{
			return Width.GetHashCode() ^ Height.GetHashCode() ^ Format.GetHashCode() ^ RefreshRate.GetHashCode();
		}

		/// <summary>
		/// Function to return a string representation of this object.
		/// </summary>
		/// <returns>String representation.</returns>
		public override string ToString()
		{
			string result = string.Empty;		// Resultant string.

			result = "Video mode:\n";
			result += "\t" + Width.ToString() + "x" + Height.ToString() + "x" + Bpp.ToString() + "\n";
			result += "\tFormat: [" + Format + "]\n";			
			result += "\tRefreshRate Rate: " + RefreshRate.ToString() + "Hz.";

			return result;
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to test for equality.
		/// </summary>
		/// <param name="left">Left operand to test.</param>
		/// <param name="right">Right operand to test.</param>
		/// <returns>TRUE if items are identical, FALSE if not.</returns>
		public static bool operator ==(VideoMode left, VideoMode right)
		{
			if ((left.Width == right.Width) && (left.Height == right.Height) && (left.Format == right.Format) && (left.RefreshRate == right.RefreshRate))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Operator to test for inequality.
		/// </summary>
		/// <param name="left">Left operand to test.</param>
		/// <param name="right">Right operand to test.</param>
		/// <returns>FALSE if items are identical, TRUE if not.</returns>
		public static bool operator !=(VideoMode left, VideoMode right)
		{
			return !(left == right);
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Overloaded constructor.
		/// </summary>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="refresh">RefreshRate rate of the video mode.</param>
		/// <param name="format">D3D Format of the video mode.</param>
		public VideoMode(int width,int height,int refresh,BackBufferFormats format)
		{
			Width = width;
			Height = height;			
			RefreshRate = refresh;
			Format = format;
		}
		#endregion
	}
}
