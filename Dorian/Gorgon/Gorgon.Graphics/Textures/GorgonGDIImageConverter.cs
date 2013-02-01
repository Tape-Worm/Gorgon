#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Friday, February 1, 2013 12:51:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A utility class to convert a GDI+ image object into a Gorgon 2D image object.
	/// </summary>
	static class GorgonGDIImageConverter
	{
		// Formats that can be converted.
		private static Tuple<PixelFormat, BufferFormat>[] _formatConversion = new[] 
		{
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format64bppArgb, BufferFormat.R16G16B16A16_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format32bppArgb, BufferFormat.R8G8B8A8_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppGrayScale, BufferFormat.R16_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppArgb1555, BufferFormat.B5G5R5A1_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppRgb565, BufferFormat.B5G6R5_UIntNormal),
		};

		// Best fit format mapping.
		private static Tuple<PixelFormat, PixelFormat>[] _bestFit = new[]
		{
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format1bppIndexed, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format4bppIndexed, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format8bppIndexed, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format16bppRgb555, PixelFormat.Format16bppArgb1555),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format48bppRgb, PixelFormat.Format64bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format64bppPArgb, PixelFormat.Format64bppArgb)
		};



		/// <summary>
		/// Function to convert a GDI+ bitmap into a Gorgon image data object.
		/// </summary>
		/// <param name="images">The image(s) to convert.</param>
		/// <param name="arrayCount">The number of image array items.</param>
		/// <param name="mipCount">The number of mip map levels.</param>
		/// <returns>The converted image data.</returns>
		public static GorgonImageData<GorgonTexture2DSettings> Convert(Image[] images, int arrayCount, int mipCount)
		{
			GorgonImageData<GorgonTexture2DSettings> imageData = null;

			GorgonTexture2DSettings settings = new GorgonTexture2DSettings()
			{
				Width = images.Max(item => item.Width),
				Height = images.Max(item => item.Height),
				ArrayCount = arrayCount,
				MipCount = mipCount,
				IsTextureCube = false,
				Format = BufferFormat.R8G8B8A8_UIntNormal
			};

			// Create our image data buffer.
			imageData = new GorgonImageData<GorgonTexture2DSettings>(settings);

			return imageData;
		}
	}
}
