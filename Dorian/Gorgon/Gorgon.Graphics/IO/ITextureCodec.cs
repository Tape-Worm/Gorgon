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
// Created: Thursday, January 31, 2013 8:17:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WIC = SharpDX.WIC;
using GorgonLibrary.Graphics;


namespace GorgonLibrary.IO
{
	/// <summary>
	/// Filter to be applied to an image that's been stretched or shrunk.
	/// </summary>
	public enum ImageFilter
	{
		/// <summary>
		/// The output pixel is assigned the value of the pixel that the point falls within. No other pixels are considered.
		/// </summary>
		Point = WIC.BitmapInterpolationMode.NearestNeighbor,
		/// <summary>
		/// The output pixel values are computed as a weighted average of the nearest four pixels in a 2x2 grid.
		/// </summary>
		Linear = WIC.BitmapInterpolationMode.Linear,
		/// <summary>
		/// Destination pixel values are computed as a weighted average of the nearest sixteen pixels in a 4x4 grid.
		/// </summary>
		Cubic = WIC.BitmapInterpolationMode.Cubic,
		/// <summary>
		/// Destination pixel values are computed as a weighted average of the all the pixels that map to the new pixel.
		/// </summary>
		Fant = WIC.BitmapInterpolationMode.Fant
	}

	/// <summary>
	/// Filter for dithering an image when it is downsampled to a lower bit depth.
	/// </summary>
	public enum ImageDithering
	{
		/// <summary>
		/// No dithering.
		/// </summary>
		None = WIC.BitmapDitherType.None,
		/// <summary>
		/// A 4x4 ordered dither algorithm.
		/// </summary>
		Dither4x4 = WIC.BitmapDitherType.Ordered4x4,
		/// <summary>
		/// An 8x8 ordered dither algorithm.
		/// </summary>
		Dither8x8 = WIC.BitmapDitherType.Ordered8x8,
		/// <summary>
		/// A 16x16 ordered dither algorithm.
		/// </summary>
		Dither16x16 = WIC.BitmapDitherType.Ordered16x16,
		/// <summary>
		/// An 4x4 spiral dither algorithm.
		/// </summary>
		Spiral4x4 = WIC.BitmapDitherType.Spiral4x4,
		/// <summary>
		/// An 8x8 spiral dither algorithm.
		/// </summary>
		Spiral8x8 = WIC.BitmapDitherType.Spiral8x8,
		/// <summary>
		/// A 4x4 dual spiral dither algorithm.
		/// </summary>
		DualSpiral4x4 = WIC.BitmapDitherType.DualSpiral4x4,
		/// <summary>
		/// An 8x8 dual spiral dither algorithm.
		/// </summary>
		DualSpiral8x8 = WIC.BitmapDitherType.DualSpiral8x8,
		/// <summary>
		/// An error diffusion algorithm.
		/// </summary>
		ErrorDiffusion = WIC.BitmapDitherType.ErrorDiffusion
	}

    /// <summary>
    /// Defines for loading/saving images to and from a stream.
    /// </summary>
    /// <remarks>An image codec is used to decode and encode image data to and from a data store (such as a <see cref="System.IO.Stream">Stream</see>).  
    /// Using these codecs, we will be able to read/write any image data source as long as there's an implementation for the image format.
    /// <para>Gorgon will have several built-in codecs for PNG, BMP, TGA, DDS, JPG, and WMP.</para>
    /// </remarks>
    public interface IImageCodec
    {
        #region Methods.
        /// <summary>
        /// Function to load a texture from a stream.
        /// </summary>
        /// <typeparam name="T">Type of texture to load.  Must be a GorgonTexture.</typeparam>
        /// <param name="stream">Stream containing the data to load.</param>
        /// <param name="sizeInBytes">The size of the texture, in bytes.</param>
        /// <returns>The texture that was in the stream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="sizeInBytes"/> parameter is less than 1 byte.</exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream parameter is write-only.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt is made to read beyond the end of the stream.</exception>
        T LoadFromStream<T>(Stream stream, int sizeInBytes) where T : GorgonTexture;

        /// <summary>
        /// Function to persist a texture to a stream.
        /// </summary>
        /// <typeparam name="T">Type of texture to load.  Must be a GorgonTexture.</typeparam>
        /// <param name="texture">Texture to persist to the stream.</param>
        /// <param name="stream">Stream that will contain the data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="stream"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream parameter is read-only.</exception>
        void SaveToStream<T>(T texture, Stream stream) where T : GorgonTexture;
        #endregion
    }
}
