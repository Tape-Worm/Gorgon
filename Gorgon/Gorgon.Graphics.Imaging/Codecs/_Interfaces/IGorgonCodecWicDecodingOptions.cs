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
// Created: June 29, 2016 10:27:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Graphics.Imaging.Codecs
{
	/// <summary>
	/// Provides options used when decoding a <see cref="IGorgonImage"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This particular interface provides common WIC (Windows Imaging Component) specific options for use when encoding an image across multiple image formats.
	/// </para>
	/// </remarks>
	public interface IGorgonCodecWicDecodingOptions
		: IGorgonImageCodecDecodingOptions
	{
		/// <summary>
		/// Property to set or return flags used to determine how to handle bit depth conversion for specific formats.
		/// </summary>
		WICFlags Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of dithering to use if the codec needs to reduce the bit depth for a pixel format.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This flag is used to determine which type of dithering algorithm should be used when converting the bit depth for a pixel format to a lower bit depth. If the pixel format of the image is supported 
		/// natively by the codec, then this value will be ignored.
		/// </para> 
		/// <para> 
		/// With dithering applied, the image will visually appear closer to the original by using patterns to simulate a greater number of colors.
		/// </para>
		/// </remarks>
		ImageDithering Dithering
		{
			get;
			set;
		}
	}
}
