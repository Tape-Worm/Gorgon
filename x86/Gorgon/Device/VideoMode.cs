#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
				return GetBpp(Format);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the BPP of a specific format for a video mode.
		/// </summary>
		/// <param name="format">Format of the video mode.</param>
		/// <returns>Bits per pixel for the video mode.</returns>
		private int GetBpp(BackBufferFormats format)
		{
			switch(format)
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
			result += "\t" + Width.ToString() + "x" + Height.ToString() + "x" + GetBpp(Format).ToString() + "\n";
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
