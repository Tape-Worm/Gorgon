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
using System.IO;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX;
using SharpDX.WIC;
using Rectangle = System.Drawing.Rectangle;

namespace Gorgon.IO
{
	/// <summary>
	/// Filter to be applied to an image that's been stretched or shrunk.
	/// </summary>
	public enum ImageFilter
	{
		/// <summary>
		/// The output pixel is assigned the value of the pixel that the point falls within. No other pixels are considered.
		/// </summary>
		Point = BitmapInterpolationMode.NearestNeighbor,
		/// <summary>
		/// The output pixel values are computed as a weighted average of the nearest four pixels in a 2x2 grid.
		/// </summary>
		Linear = BitmapInterpolationMode.Linear,
		/// <summary>
		/// Destination pixel values are computed as a weighted average of the nearest sixteen pixels in a 4x4 grid.
		/// </summary>
		Cubic = BitmapInterpolationMode.Cubic,
		/// <summary>
		/// Destination pixel values are computed as a weighted average of the all the pixels that map to the new pixel.
		/// </summary>
		Fant = BitmapInterpolationMode.Fant
	}

	/// <summary>
	/// Filter for dithering an image when it is downsampled to a lower bit depth.
	/// </summary>
	public enum ImageDithering
	{
		/// <summary>
		/// No dithering.
		/// </summary>
		None = BitmapDitherType.None,
		/// <summary>
		/// An error diffusion algorithm.
		/// </summary>
		ErrorDiffusion = BitmapDitherType.ErrorDiffusion
	}

	/// <summary>
	/// Flags to control how pixel conversion should be handled.
	/// </summary>
	[Flags]
	public enum ImageBitFlags
	{
		/// <summary>
		/// No modifications to conversion process.
		/// </summary>
		None = 0,
		/// <summary>
		/// Set a known opaque alpha value in the alpha channel.
		/// </summary>
		OpaqueAlpha = 1,
		/// <summary>
		/// Enables specific legacy format conversion cases.
		/// </summary>
		Legacy = 2
	}

	/// <summary>
	/// A codec to reading and/or writing image data.
	/// </summary>
	/// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// <para>The codec accepts and returns a <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> type, which is filled from or read into the encoded file.</para>
	/// </remarks>
	public abstract class GorgonImageCodec
		: INamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the array count for the image.
		/// </summary>
		/// <remarks>Use this to control the array count of the image.  If this value is set to a value larger than 1, then any depth information will be ignored in the image.
		/// <para>For most codecs, there will only be 1 array index, so a setting of 0 is the same as a setting of 1.  Array image data is usually only found in DDS files and in files that support multiple frames 
		/// like GIF files.</para>
		/// <para>This property only applies to decoding image data.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of the image.
		/// </summary>
		/// <remarks>Use this to clip or scale the width of the image.  To clip, ensure that the <see cref="Gorgon.IO.GorgonImageCodec.Clip">Clip</see> property is set to <c>true</c>.  To scale, ensure that 
		/// the property is set to <c>false</c>.  Filtering may be applied to scaled images by the <see cref="Gorgon.IO.GorgonImageCodec.Filter">Filter</see> property.
		/// <para>Set this value to 0 to use the width in the file.</para>
		/// <para>This property only applies to decoding image data.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the image.
		/// </summary>
		/// <remarks>Use this to clip or scale the height of the image.  To clip, ensure that the <see cref="Gorgon.IO.GorgonImageCodec.Clip">Clip</see> property is set to <c>true</c>.  To scale, ensure that 
		/// the property is set to <c>false</c>.  Filtering may be applied to scaled images by the <see cref="Gorgon.IO.GorgonImageCodec.Filter">Filter</see> property.
		/// <para>Set this value to 0 to use the height in the file.</para>
		/// <para>This property only applies to decoding image data.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format to convert to.
		/// </summary>
		/// <remarks>Use this to convert an image to another format.  Some formats are unsupported, and in those cases the image will be returned with its original format.
		/// <para>Set this value to Unknown to use the format of the image as it appears in the file.</para>
		/// <para>This property only applies to decoding image data.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the mip map level count.
		/// </summary>
		/// <remarks>Use this to override the number of mip maps in the image.  When this value is set, Gorgon will attempt to fill out as many of the requested 
		/// mip maps as it can (depending on width, height and depth of the image).  
		/// <para>Set this value to 0 to use the number of mip maps that are present in the file.</para>
		/// <para>This property only applies to decoding image data.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the codec supports decoding/encoding multiple frames or not.
		/// </summary>
		public virtual bool SupportsMultipleFrames
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Property to set or return the usage for textures loaded with this codec.
		/// </summary>
		/// <remarks>This will assign the usage pattern for a texture that is loaded by this codc.
		/// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture">textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> it is ignored.</para>
		/// <para>This property is only applied when decoding an image, otherwise it is ignored.</para>
		/// <para>The default value is Default.</para>
		/// </remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the shader view format for a texture loaded with this codec.
		/// </summary>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  When this value is set to Unknown the view format is taken from the texture format.
		/// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture">textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> it is ignored.</para>
		/// <para>This property is only applied when decoding an image, otherwise it is ignored.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat ViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow an unordered access view of this texture.
		/// </summary>
		/// <remarks>This allows a texture to be accessed via an unordered access view in a shader.
		/// <para>Textures using an unordered access view can only use a typed (e.g. int, uint, float) format that belongs to the same group as the format assigned to the texture, 
		/// or R32_UInt/Int/Float (but only if the texture format is 32 bit).  Any other format will raise an exception.  Note that if the format is not set to R32_UInt/Int/Float, 
		/// then write-only access will be given to the UAV.</para> 
		/// <para>To check to see if a format is supported for UAV, use the <see cref="Gorgon.Graphics.GorgonVideoDevice.SupportsUnorderedAccessViewFormat">GorgonVideoDevice.SupportsUnorderedAccessViewFormat</see> 
		/// Function to determine if the format is supported.</para>
        /// <para>If this value is set to <c>true</c>, it will automatically change the format of the texture to the equivalent typeless format.  This is necessary because UAVs cannot be 
        /// used with typed texture resources.</para>
        /// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture">textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> it is ignored.</para>
		/// <para>This property is only applied when decoding an image, otherwise it is ignored.</para>
		/// <para>The default value is <c>false</c>.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the multisampling applied to the texture.
        /// </summary>
        /// <remarks>
        /// Set this value to apply multisampling to the texture.  
        /// <para>Note that if multisampling is applied, then the mip-map count and array count must be set to 1.  If these values are not set to 1, then this value will be ignored.</para>
        /// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture2D">2D textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> or other texture types it is ignored.</para>
        /// <para>The default value is a count of 1 and a quality of 0 (No multisampling).</para>
        /// </remarks>
        public GorgonMultisampling Multisampling
        {
            get;
            set;
        }
		
		/// <summary>
		/// Property to set or return whether to clip the image or to scale it if the size is mismatched.
		/// </summary>
		/// <remarks>Setting this value to <c>false</c> will scale the image to match the image data buffer size.  Setting it to <c>true</c> will clip it to the buffer size.
		/// <para>This applies to decoding images only and only when the image width or height is smaller than the width or height in the image file.</para>
		/// <para>The default value is <c>true</c>.</para>
		/// </remarks>
		public bool Clip
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the dithering type to apply to images that lose bit depth information.
		/// </summary>
		/// <remarks>This will allow the image to be dithered to cover up lost information due downsampling to a lower color depth.
		/// <para>This applies to decoding images only and only when images are down sampled in bit depth (e.g. 32 bpp -> 16 bpp).</para>
		/// <para>The default is None.</para>
		/// </remarks>
		public ImageDithering Dithering
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the filter to apply when an image file is scaled up or down to match the image data buffer.
		/// </summary>
		/// <remarks>This will apply filtering to smooth out the image file data if it needs to be scaled up or down.
		/// <para>This applies to decoding images only and only when images are scaled.</para>
		/// <para>The default value is Fant.</para>
		/// </remarks>
		public ImageFilter Filter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the common file name extension(s) for a codec.
		/// </summary>
		public IEnumerable<string> CodecCommonExtensions
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

        /// <summary>
        /// Property to return the data formats supported by the codec.
        /// </summary>
	    public abstract IEnumerable<BufferFormat> SupportedFormats
	    {
	        get;
	    }

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
	    public abstract bool SupportsDepth
	    {
	        get;
	    }

        /// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
	    public abstract bool SupportsArray
	    {
	        get;
	    }

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
	    public abstract bool SupportsMipMaps
	    {
	        get;
	    }

		/// <summary>
		/// Property to return whether the image codec supports block compression.
		/// </summary>
		public abstract bool SupportsBlockCompression
		{
			get;
		}
	    #endregion

		#region Methods.
		/// <summary>
		/// Function to copy (or update in-place) a line with opaque alpha substituion (if required).
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="format">Format of the destination buffer.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.
		/// <para>This overload is for languages that don't support unsafe code (e.g. Visual Basic .NET).</para>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is IntPtr.Zero.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// </remarks>
		protected unsafe void CopyScanline(IntPtr src, int srcPitch, IntPtr dest, int destPitch, BufferFormat format, ImageBitFlags bitFlags)
		{
			CopyScanline(src.ToPointer(), srcPitch, dest.ToPointer(), destPitch, format, bitFlags);
		}

		/// <summary>
		/// Function to expand a 16BPP line in an image to a 32BPP RGBA line.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="srcFormat">Format to convert from.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="srcFormat"/> is not a 16 BPP format.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is IntPtr.Zero.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>Use this to expand a 16 BPP (B5G6R5 or B5G5R5A1 format) into a 32 BPP R8G8B8A8 (normalized unsigned integer) format.
		/// <para>This overload is for languages that don't support unsafe code (e.g. Visual Basic .NET).</para>
		/// </remarks>
		protected unsafe void Expand16BPPScanline(IntPtr src, int srcPitch, BufferFormat srcFormat, IntPtr dest, int destPitch, ImageBitFlags bitFlags)
		{
			Expand16BPPScanline(src.ToPointer(), srcPitch, srcFormat, dest.ToPointer(), destPitch, bitFlags);
		}

		/// <summary>
		/// Function to expand a 16BPP scan line in an image to a 32BPP RGBA line.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="srcFormat">Format to convert from.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="srcFormat" /> is not a 16 BPP format.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is NULL.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>
		/// Use this to expand a 16 BPP (B5G6R5 or B5G5R5A1 format) into a 32 BPP R8G8B8A8 (normalized unsigned integer) format.
		/// </remarks>
		protected unsafe void Expand16BPPScanline(void* src, int srcPitch, BufferFormat srcFormat, void* dest, int destPitch, ImageBitFlags bitFlags)
		{
			var srcPtr = (ushort*)src;
			var destPtr = (uint*)dest;

			if ((srcFormat != BufferFormat.B5G5R5A1_UIntNormal) && (srcFormat != BufferFormat.B5G6R5_UIntNormal))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_IMAGE_NOT_16BPP, srcFormat), "srcFormat");
			}

			if (src == null)
			{
				throw new ArgumentNullException("src");
			}

			if (dest == null)
			{
				throw new ArgumentNullException("dest");
			}

			for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
			{
				ushort srcPixel = *(srcPtr++);
				uint R = 0, G = 0, B = 0, A = 0;

				switch (srcFormat)
				{
					case BufferFormat.B5G6R5_UIntNormal:
						R = (uint)((srcPixel & 0xF800) >> 11);
						G = (uint)((srcPixel & 0x07E0) >> 5);
						B = (uint)(srcPixel & 0x001F);
						R = ((R << 3) | (R >> 2));
						G = ((G << 2) | (G >> 4)) << 8;
						B = ((B << 3) | (B >> 2)) << 16;
						A = 0xFF000000;
						break;
					case BufferFormat.B5G5R5A1_UIntNormal:
						R = (uint)((srcPixel & 0x7C00) >> 10);
						G = (uint)((srcPixel & 0x03E0) >> 5);
						B = (uint)(srcPixel & 0x001F);
						R = ((R << 3) | (R >> 2));
						G = ((G << 3) | (G >> 2)) << 8;
						B = ((B << 3) | (B >> 2)) << 16;
						A = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
							    ? 0xFF000000
							    : (((srcPixel & 0x8000) != 0) ? 0xFF000000 : 0);
						break;
				}

				*(destPtr++) = R | G | B | A;
			}
		}

		/// <summary>
		/// Function to copy (or update in-place) with bits swizzled to match another format.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="format">Format of the destination buffer.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is NULL.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>Use this method to copy a single scanline and swizzle the bits of an image and (optionally) set an opaque constant alpha value.</remarks>
		protected unsafe void SwizzleScanline(IntPtr src, int srcPitch, IntPtr dest, int destPitch, BufferFormat format, ImageBitFlags bitFlags)
		{
			SwizzleScanline(src.ToPointer(), srcPitch, dest.ToPointer(), destPitch, format, bitFlags);
		}

		/// <summary>
		/// Function to copy (or update in-place) with bits swizzled to match another format.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="format">Format of the destination buffer.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is NULL.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
        /// <remarks>Use this method to copy a single scanline and swizzle the bits of an image and (optionally) set an opaque constant alpha value.</remarks>
		protected unsafe void SwizzleScanline(void* src, int srcPitch, void* dest, int destPitch, BufferFormat format, ImageBitFlags bitFlags)
		{
            int size = srcPitch.Min(destPitch);
            uint r, g, b, a, pixel;

			if (src == null)
			{
				throw new ArgumentNullException("src");
			}

			if (dest == null)
			{
				throw new ArgumentNullException("dest");
			}

            if (format == BufferFormat.Unknown)
            {
	            throw new ArgumentException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, BufferFormat.Unknown),
	                                        "format");
            }

            var srcPtr = (uint*)src;
            var destPtr = (uint*)dest;

            switch (format)
            {
                case BufferFormat.R10G10B10A2:
                case BufferFormat.R10G10B10A2_UInt:
                case BufferFormat.R10G10B10A2_UIntNormal:
                case BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal:
                    for (int i = 0; i < size; i += 4)
                    {
                        if (src != dest)
                        {
                            pixel = *(srcPtr++);
                        }
                        else
                        {
                            pixel = *(destPtr);
                        }

                        r = ((pixel & 0x3FF00000) >> 20);
                        g = (pixel & 0x000FFC00);
                        b = ((pixel & 0x000003FF) << 20);
                        a = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xC0000000 : pixel & 0xC0000000;

                        *destPtr = r | g | b | a;
                        destPtr++;
                    }
                    return;
                case BufferFormat.R8G8B8A8:
                case BufferFormat.R8G8B8A8_UIntNormal:
                case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
                case BufferFormat.B8G8R8A8_UIntNormal:
                case BufferFormat.B8G8R8X8_UIntNormal:
                case BufferFormat.B8G8R8A8:
                case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
                case BufferFormat.B8G8R8X8:
                case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                    for (int i = 0; i < size; i += 4)
                    {
                        if (src != dest)
                        {
                            pixel = *(srcPtr++);
                        }
                        else
                        {
                            pixel = *(destPtr);
                        }

                        r = ((pixel & 0xFF0000) >> 16);
                        g = (pixel & 0x00FF00);
                        b = ((pixel & 0x0000FF) << 16);
                        a = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : pixel & 0xFF000000;

                        *destPtr = r | g | b | a;
                        destPtr++;
                    }
                    return;
            }

            if (src != dest)
            {
                DirectAccess.MemoryCopy(dest, src, size);
            }
        }


		/// <summary>
		/// Function to copy (or update in-place) a line with opaque alpha substituion (if required).
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="format">Format of the destination buffer.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is NULL.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.</remarks>
		protected unsafe void CopyScanline(void* src, int srcPitch, void* dest, int destPitch, BufferFormat format, ImageBitFlags bitFlags)
		{
			if (src == null)
			{
				throw new ArgumentNullException("src");
			}

			if (dest == null)
			{
				throw new ArgumentNullException("dest");
			}

			if (format == BufferFormat.Unknown)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, BufferFormat.Unknown),
											"format");
			}

			int size = (src == dest) ? destPitch : (srcPitch.Min(destPitch));

			if ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
			{
				// Do a straight copy.
				switch (format)
				{
					case BufferFormat.R32G32B32A32:
					case BufferFormat.R32G32B32A32_Float:
					case BufferFormat.R32G32B32A32_UInt:
					case BufferFormat.R32G32B32A32_Int:
						{
							uint alpha = (format == BufferFormat.R32G32B32_Float) ? 0x3F800000
												: ((format == BufferFormat.R32G32B32_Int) ? 0x7FFFFFFF : 0xFFFFFFFF);

							var srcPtr = (uint*)src;
							var destPtr = (uint*)dest;

							for (int i = 0; i < size; i += 16)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = *srcPtr;
									srcPtr += 4;
								}

								*destPtr += 3;
								*(destPtr++) = alpha;
							}
						}
						return;
					case BufferFormat.R16G16B16A16:
					case BufferFormat.R16G16B16A16_Float:
					case BufferFormat.R16G16B16A16_UIntNormal:
					case BufferFormat.R16G16B16A16_UInt:
					case BufferFormat.R16G16B16A16_IntNormal:
					case BufferFormat.R16G16B16A16_Int:
						{
							ushort alpha = 0xFFFF;

							switch (format)
							{
								case BufferFormat.R16G16B16A16_Float:
									alpha = 0x3C00;
									break;
								case BufferFormat.R16G16B16A16_Int:
								case BufferFormat.R16G16B16A16_IntNormal:
									alpha = 0x7FFF;
									break;
							}

							var srcPtr = (ushort*)src;
							var destPtr = (ushort*)dest;

							for (int i = 0; i < size; i += 8)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = *srcPtr;
									srcPtr += 4;
								}
								*destPtr += 3;
								*(destPtr++) = alpha;
							}
						}
						return;
					case BufferFormat.R10G10B10A2:
					case BufferFormat.R10G10B10A2_UIntNormal:
					case BufferFormat.R10G10B10A2_UInt:
					case BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal:
						{
							var srcPtr = (uint*)src;
							var destPtr = (uint*)dest;

							for (int i = 0; i < size; i += 4)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = (*srcPtr) & 0x3FFFFFFF;
									srcPtr++;
								}
								*destPtr |= 0xC0000000;
								destPtr++;
							}
						}
						return;
					case BufferFormat.R8G8B8A8:
					case BufferFormat.R8G8B8A8_UIntNormal:
					case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					case BufferFormat.R8G8B8A8_UInt:
					case BufferFormat.R8G8B8A8_IntNormal:
					case BufferFormat.R8G8B8A8_Int:
					case BufferFormat.B8G8R8A8_UIntNormal:
					case BufferFormat.B8G8R8A8:
					case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
						{
							uint alpha = ((format == BufferFormat.R8G8B8A8_Int) || (format == BufferFormat.R8G8B8A8_IntNormal)) ? 0x7F000000 : 0xFF000000;

							var srcPtr = (uint*)src;
							var destPtr = (uint*)dest;

							for (int i = 0; i < size; i += 4)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = (*srcPtr) & 0xFFFFFF;
									srcPtr++;
								}
								*destPtr |= alpha;
								destPtr++;
							}
						}
						return;
					case BufferFormat.B5G5R5A1_UIntNormal:
						{
							var srcPtr = (ushort*)src;
							var destPtr = (ushort*)dest;

							for (int i = 0; i < size; i += 2)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*(destPtr++) = (ushort)((*srcPtr++) | 0x8000);
								}
								else
								{
									*(destPtr++) |= 0x8000;
								}
							}
						}
						return;
					case BufferFormat.A8_UIntNormal:
						DirectAccess.FillMemory(dest, 0xFF, size);
						return;
				}
			}

			// Copy if not doing an in-place update.
			if (dest != src)
			{
				DirectAccess.MemoryCopy(dest, src, size);
			}
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the data to load.</param>
        /// <param name="size">Size of the data to read, in bytes.</param>
		/// <returns>The image data that was in the stream.</returns>
		protected internal abstract GorgonImageData LoadFromStream(GorgonDataStream stream, int size);

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="Gorgon.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal abstract void SaveToStream(GorgonImageData imageData, Stream stream);

		/// <summary>
		/// Function to perform any post processing on image data.
		/// </summary>
		internal void PostProcess(GorgonImageData data)
		{
			IImageSettings destSettings = data.Settings.Clone();
			int width = Width > 0 ? Width : data.Settings.Width;
			int height = Height > 0 ? Height : data.Settings.Height;
			int mipCount = MipCount > 0 ? MipCount : data.Settings.MipCount;
			BufferFormat format = (Format != BufferFormat.Unknown) ? Format : data.Settings.Format;
			Rectangle newSize = Rectangle.Empty;
			int mipStart = 0;
			var sourceInfo = GorgonBufferFormatInfo.GetInfo(data.Settings.Format);
			var destInfo = GorgonBufferFormatInfo.GetInfo(format);

			// First, confirm whether we can perform format conversions.
			using (var wic = new GorgonWICImage())
			{
				Guid srcPixelFormat = wic.GetGUID(data.Settings.Format);
				Guid destPixelFormat = wic.GetGUID(format);

				// Do nothing if we can't do anything with the source format.
				if (srcPixelFormat == Guid.Empty)
				{
					return;
				}

				// Cancel conversion if we're using the same format.
				if (srcPixelFormat == destPixelFormat)
				{
					destPixelFormat = Guid.Empty;
				}

				// Get the new size.
				if ((width != data.Settings.Width) || (height != data.Settings.Height))
				{
					newSize = new Rectangle(0, 0, width, height);
				}

				// Set up destination buffer settings.
				destSettings.Format = format;
				destSettings.Width = width;
				destSettings.Height = height;
				destSettings.MipCount = mipCount;

				// Ensure we don't go over the maximum.
				int maxMips = GorgonImageData.GetMaxMipCount(destSettings);

				if (mipCount > maxMips)
				{
					mipCount = maxMips;
				}

				// Nothing's going to happen here, so leave.
				if ((destPixelFormat == Guid.Empty) && (newSize == Rectangle.Empty) && (mipCount == 1))
				{
					return;
				}

				// Create our worker buffer.
				GorgonImageData destData;
				using(destData = new GorgonImageData(destSettings))
				{
					// The first step is to convert and resize our images:
					if ((destPixelFormat != Guid.Empty) || (newSize != Rectangle.Empty))
					{
						for (int array = 0; array < destSettings.ArrayCount; array++)
						{
							// We're not going to copy mip levels at this point, that will come in the next step.
							for (int depth = 0; depth < destSettings.Depth; depth++)
							{
								// Get our source/destination buffers.
								var sourceBuffer = data.Buffers[0, data.Settings.ImageType == ImageType.Image3D ? depth : array];
                                var destBuffer = destData.Buffers[0, data.Settings.ImageType == ImageType.Image3D ? depth : array];

								var dataRect = new DataRectangle(sourceBuffer.Data.BasePointer, sourceBuffer.PitchInformation.RowPitch);

								// Create a temporary WIC bitmap to work with.
								using (var bitmap = new Bitmap(wic.Factory, sourceBuffer.Width, sourceBuffer.Height, srcPixelFormat, dataRect, sourceBuffer.PitchInformation.SlicePitch))
								{
									wic.TransformImageData(bitmap,
									                       destBuffer.Data.BasePointer,
									                       destBuffer.PitchInformation.RowPitch,
									                       destBuffer.PitchInformation.SlicePitch,
									                       destPixelFormat,
									                       sourceInfo.IssRGB,
									                       destInfo.IssRGB,
									                       Dithering,
									                       newSize,
									                       Clip,
									                       Filter);
								}
							}
						}

						// Adjust the mip map starting point.
						mipStart = 1;
					}

					// Next, we need to build mip maps.
					if (destSettings.MipCount > 1)
					{
						// The first step is to convert and resize our images:
						for (int array = 0; array < destSettings.ArrayCount; array++)
						{
							int mipDepth = destSettings.Depth;

							for (int mip = mipStart; mip < destSettings.MipCount; mip++)
							{
								// We're not going to copy mip levels at this point, that will come in the next step.
								for (int depth = 0; depth < mipDepth; depth++)
								{
									// Get our source/destination buffers.
								    var sourceBuffer = destData.Buffers[0, data.Settings.ImageType == ImageType.Image3D ? (destSettings.Depth / mipDepth) * depth : array];
									var destBuffer = destData.Buffers[mip, data.Settings.ImageType == ImageType.Image3D ? depth : array];

									var dataRect = new DataRectangle(sourceBuffer.Data.BasePointer, sourceBuffer.PitchInformation.RowPitch);

									// Create a temporary WIC bitmap to work with.
									using (var bitmap = new Bitmap(wic.Factory, sourceBuffer.Width, sourceBuffer.Height, srcPixelFormat, dataRect, sourceBuffer.PitchInformation.SlicePitch))
									{
										wic.TransformImageData(bitmap, destBuffer.Data.BasePointer, destBuffer.PitchInformation.RowPitch, destBuffer.PitchInformation.SlicePitch,
																Guid.Empty, false, false, ImageDithering.None, new Rectangle(0, 0, destBuffer.Width, destBuffer.Height), false, Filter);
									}
								}

								if (mipDepth > 1)
								{
									mipDepth >>= 1;
								}
							}
						}
					}

					// Update our data.
					data.TakeOwnership(destData);
				}
			}
		}

        /// <summary>
        /// Function to determine if this codec can read the file or not.
        /// </summary>
        /// <param name="stream">Stream used to read the file information.</param>
        /// <returns>
        /// <c>true</c> if the codec can read the file, <c>false</c> if not.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so 
        /// may cause undesirable results.</remarks>
        public abstract bool IsReadable(Stream stream);

		/// <summary>
        /// Function to read file meta data.
        /// </summary>
        /// <param name="stream">Stream used to read the metadata.</param>
        /// <returns>
        /// The image meta data as a <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> value.
        /// </returns>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
		/// <para>-or-</para>
		/// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
		/// </exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		/// <remarks>When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so 
		/// may cause undesirable results.</remarks>
		public abstract IImageSettings GetMetaData(Stream stream);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
	        return string.Format(Resources.GORGFX_IMAGE_CODEC_TOSTR, Codec);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageCodec" /> class.
		/// </summary>
		protected GorgonImageCodec()
		{
			Width = 0;
			Height = 0;
			Format = BufferFormat.Unknown;
			MipCount = 0;

			Clip = true;
			Filter = ImageFilter.Fant;
			Dithering = ImageDithering.None;
			CodecCommonExtensions = new string[] { };
			ViewFormat = BufferFormat.Unknown;
			AllowUnorderedAccess = false;
			Usage = BufferUsage.Default;
		    Multisampling = GorgonMultisampling.NoMultiSampling;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return Codec;
			}
		}
		#endregion
	}
}
