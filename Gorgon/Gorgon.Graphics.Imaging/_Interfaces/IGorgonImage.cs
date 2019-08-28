using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Native;
using SharpDX.WIC;

namespace Gorgon.Graphics.Imaging
{
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
        /// <para>
        /// An error diffusion algorithm.
        /// </para>
        /// <para>
        /// This should be used with images that make use of a color palette, such as 8 bit images.
        /// </para>
        /// </summary>
        ErrorDiffusion = BitmapDitherType.ErrorDiffusion,
        /// <summary>
        /// <para>
        /// A 4x4 ordered dither algorithm.
        /// </para>
        /// <para>
        /// This should be used with images that use 5 or 6 bits of color depth for their color channels.
        /// </para>
        /// </summary>
        Ordered4x4 = BitmapDitherType.Ordered4x4,
        /// <summary>
        /// An 8x8 ordered dither algorithm.
        /// </summary>
        Ordered8x8 = BitmapDitherType.Ordered8x8,
        /// <summary>
        /// A 16x16 ordered dither algorithm.
        /// </summary>
        Ordered16x16 = BitmapDitherType.Ordered16x16
    }

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
    /// Holds raw data that is used to represent an image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="GorgonImage"/> object will hold a blob of data and represent that data as a series of pixels to be displayed, or manipulated. This image type is capable of representing standard 2D 
    /// images, but can also represent 1D and 3D images. And, depending on the type of image, there is also support for mip map levels, and arrayed images.
    /// </para>
    /// <para>
    /// Images can access their data directly through a <see cref="GorgonNativeBuffer{T}"/> interface that allows safe access to raw, unmanaged memory where the image data is stored. In cases where images have 
    /// multiple parts like depth slices for a 3D image, or an array for 2D images, this object will provide access through a series of buffers that will point to the individual locations for depth slices, 
    /// array indices, and mip map levels. These buffers will also provide their own <see cref="GorgonNativeBuffer{T}"/> that will allow safe and direct access to the native memory where the buffer is located.
    /// </para>
    /// <para>
    /// Because this object stored data in native memory instead of on the heaps provided by .NET, this object should be disposed by calling its <see cref="IDisposable.Dispose"/> method when it is no 
    /// longer required. Failure to do so might cause a memory leak until the garbage collector can deal with it.
    /// </para>
    /// </remarks>
    public interface IGorgonImage
        : IDisposable, IGorgonCloneable<IGorgonImage>, IGorgonImageInfo
    {
        /// <summary>
        /// Property to return the pointer to the beginning of the internal buffer.
        /// </summary>
        GorgonNativeBuffer<byte> ImageData
        {
            get;
        }

        /// <summary>
        /// Property to return information about the pixel format for this image.
        /// </summary>
        GorgonFormatInfo FormatInfo
        {
            get;
        }

        /// <summary>
        /// Property to return the number of bytes, in total, that this image occupies.
        /// </summary>
        int SizeInBytes
        {
            get;
        }

        /// <summary>
        /// Property to return the list of image buffers for this image.
        /// </summary>
        IGorgonImageBufferList Buffers
        {
            get;
        }

        /// <summary>
        /// Function to return the number of depth slices for a given mip map slice.
        /// </summary>
        /// <param name="mipLevel">The mip map level to look up.</param>
        /// <returns>The number of depth slices for the given mip map level.</returns>
        /// <remarks>
        /// <para>
        /// For 1D and 2D images, the mip level will always return 1.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds the number of mip maps for the image or is less than 0.</exception>
        int GetDepthCount(int mipLevel);

        /// <summary>
        /// Function to determine if the pixel format for this image can be converted to another pixel format.
        /// </summary>
        /// <param name="format">The pixel format to convert to.</param>
        /// <returns><b>true</b> if the the current pixel format and the requested pixel format can be converted, <b>false</b> if not.</returns>
        bool CanConvertToFormat(BufferFormat format);

        /// <summary>
        /// Function to determine if the source format can convert to any of the formats in the destination list.
        /// </summary>
        /// <param name="destFormats">List of destination formats to compare.</param>
        /// <returns>A list of formats that the source format can be converted into, or an empty array if no conversion is possible.</returns>
        IReadOnlyList<BufferFormat> CanConvertToFormats(BufferFormat[] destFormats);

        /// <summary>
        /// Function to copy another <see cref="IGorgonImage"/> into this image object.
        /// </summary>
        /// <param name="source">The image that will be copied into this image.</param>
        /// <remarks>
        /// <para>
        /// This will clone the <paramref name="source"/> image into this one . All information in this image will be replaced with the image data present in <paramref name="source"/>. If copying parts of 
        /// an image into a new image is required, then see the <see cref="IGorgonImageBuffer"/>.<see cref="IGorgonImageBuffer.CopyTo"/> method.
        /// </para>
        /// </remarks>
        void CopyFrom(IGorgonImage source);

        /// <summary>
	    /// Function to copy this image into another image object.
	    /// </summary>
	    /// <param name="destination">The image that will receive the contents of this image.</param>
	    /// <remarks>
	    /// <para>
	    /// This will clone this image into the <paramref name="destination"/> . All information in the destination image will be replaced with the image data present in this image. If copying parts of an 
	    /// image into a new image is required, then see the <see cref="IGorgonImageBuffer"/>.<see cref="IGorgonImageBuffer.CopyTo"/> method.
	    /// </para>
	    /// </remarks>
		void CopyTo(IGorgonImage destination);
    }
}