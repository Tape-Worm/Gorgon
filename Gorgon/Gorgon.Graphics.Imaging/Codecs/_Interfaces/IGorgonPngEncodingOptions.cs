#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 28, 2016 8:03:41 PM
// 
#endregion

using WIC = SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Filter to apply for compression optimization.
	/// </summary>
	public enum PNGFilter
	{
		/// <summary>
		/// The system will chose the best filter based on the image data.
		/// </summary>
		DontCare = WIC.PngFilterOption.Unspecified,
		/// <summary>
		/// No filtering.
		/// </summary>
		None = WIC.PngFilterOption.None,
		/// <summary>
		/// Sub filtering.
		/// </summary>
		Sub = WIC.PngFilterOption.Sub,
		/// <summary>
		/// Up filtering.
		/// </summary>
		Up = WIC.PngFilterOption.Up,
		/// <summary>
		/// Average filtering.
		/// </summary>
		Average = WIC.PngFilterOption.Average,
		/// <summary>
		/// Paeth filtering.
		/// </summary>
		Paeth = WIC.PngFilterOption.Paeth,
		/// <summary>
		/// Adaptive filtering.  The system will choose the best filter based on a per-scanline basis.
		/// </summary>
		Adaptive = WIC.PngFilterOption.Adaptive
	}


	/// <summary>
	/// Provides options used when encoding an image 
	/// </summary>
	public interface IGorgonPngEncodingOptions
		: IGorgonWicEncodingOptions
	{
		/// <summary>
		/// Property to set or return whether to use interlacing when encoding an image as a PNG file.
		/// </summary>
		bool Interlacing
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of filter to use when when compressing the PNG file.
		/// </summary>
		PNGFilter Filter
		{
			get;
			set;
		}
	}
}
