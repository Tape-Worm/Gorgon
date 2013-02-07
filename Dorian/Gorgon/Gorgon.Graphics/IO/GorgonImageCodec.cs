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
// Created: Wednesday, February 6, 2013 5:59:03 PM
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
	/// A codec to reading and/or writing image data.
	/// </summary>
	/// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// <para>The codec accepts and returns a <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> type, which is filled from or read into the encoded file.</para>
	/// </remarks>
	public abstract class GorgonImageCodec
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the common file name extension(s) for a codec.
		/// </summary>
		public IList<string> CodecCommonExtensions
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public abstract string CodecDescription
		{
			get;
		}

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. PNG).
		/// </summary>
		public abstract string Codec
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the data to load.</param>
		/// <param name="sizeInBytes">The size of the image data, in bytes.</param>
		/// <returns>The image data that was in the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="sizeInBytes"/> parameter is less than 1 byte.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is write-only.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt is made to read beyond the end of the stream.</exception>
		protected internal abstract GorgonImageData LoadFromStream(Stream stream, int sizeInBytes);

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="GorgonLibrary.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="imageData"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="stream"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is read-only.</exception>
		protected internal abstract void SaveToStream(GorgonImageData imageData, Stream stream);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageCodec" /> class.
		/// </summary>
		protected GorgonImageCodec()
		{
			CodecCommonExtensions = new string[] { };
		}
		#endregion
	}
}
