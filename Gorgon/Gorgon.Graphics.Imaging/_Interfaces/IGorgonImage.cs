using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Native;
using DX = SharpDX;
using WIC = SharpDX.WIC;
using BCn = BCnEncoder.Encoder;

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
        None = WIC.BitmapDitherType.None,
        /// <summary>
        /// <para>
        /// An error diffusion algorithm.
        /// </para>
        /// <para>
        /// This should be used with images that make use of a color palette, such as 8 bit images.
        /// </para>
        /// </summary>
        ErrorDiffusion = WIC.BitmapDitherType.ErrorDiffusion,
        /// <summary>
        /// <para>
        /// A 4x4 ordered dither algorithm.
        /// </para>
        /// <para>
        /// This should be used with images that use 5 or 6 bits of color depth for their color channels.
        /// </para>
        /// </summary>
        Ordered4x4 = WIC.BitmapDitherType.Ordered4x4,
        /// <summary>
        /// An 8x8 ordered dither algorithm.
        /// </summary>
        Ordered8x8 = WIC.BitmapDitherType.Ordered8x8,
        /// <summary>
        /// A 16x16 ordered dither algorithm.
        /// </summary>
        Ordered16x16 = WIC.BitmapDitherType.Ordered16x16
    }

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
    /// The levels of quality to apply to the block compression functionality. The higher the quality, the slower the compression routine will be.
    /// </summary>
    public enum BcCompressionQuality
    {
        /// <summary>
        /// Fast compression performance, poorer image quality.
        /// </summary>        
        Fast = BCn.CompressionQuality.Fast,
        /// <summary>
        /// Balanced compression performance, better image quality.
        /// </summary>
        Balanced = BCn.CompressionQuality.Balanced,
        /// <summary>
        /// Slowest compression performance, best image quality.
        /// </summary>
        Quality = BCn.CompressionQuality.BestQuality
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
        GorgonPtr<byte> ImageData
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
        IReadOnlyList<BufferFormat> CanConvertToFormats(IReadOnlyList<BufferFormat> destFormats);

        /// <summary>
        /// Function to copy an image into this image.
        /// </summary>
        /// <param name="image">The image that will copied into this image.</param>
        /// <remarks>
        /// <para>
        /// This will clone the <paramref name="image"/> into this image. All information in the current image will be discarded and replaced with a duplicate of the data present in the source 
        /// <paramref name="image"/>. If copying parts of an image into a new image is required, then see the <see cref="IGorgonImageBuffer"/>.<see cref="IGorgonImageBuffer.CopyTo"/> 
        /// method.
        /// </para>
        /// </remarks>
        void Copy(IGorgonImage image);

        /// <summary>
        /// Function to decompress an image containing block compressed data.
        /// </summary>
        /// <param name="useBC1Alpha">[Optional] <b>true</b> if the image is compressed with BC1 (DXT1) compression, and the contains alpha, <b>false</b> if no alpha is in the image.</param>
        /// <returns>The fluent interface for modifying the image.</returns>
        /// <remarks>
        /// <para>
        /// This method will decompress an image containing image data that has been compressed with one of the standard block compression formats. The <see cref="BufferFormat"/> enum contains 7 levels of 
        /// block compression named BC1 - BC7. The features of each compression level are documented at <a target="_blank" href="https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11"/>.
        /// </para>
        /// <para>
        /// The decompressed image data will result in 32 bit RGBA data in the format of <see cref="BufferFormat.R8G8B8A8_UNorm"/>.
        /// </para>
        /// <para>
        /// Block compression is, by nature, a lossy compression format. Thus some fidelity will be lost when the image data is compressed, it is recommended that images be compressed as a last stage in 
        /// processing. Because block compression lays the image data out differently than standard image data, the functionality provided for modifying an image (e.g. <see cref="IGorgonImageUpdateFluent.Resize"/>) will not 
        /// work and will throw an exception if used on block compressed data, this method will allow users to make alterations with the image modification functionality.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Because block compressed data is lossy, it is not recommended that images be decompressed and compressed over and over as it will degrade the image fidelity severely. 
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// If the image data was compressed with BC1 compression, optional 1-bit alpha channel data may be stored with the image data. The developer must specify whether to use the alpha data or not via 
        /// the <paramref name="useBC1Alpha"/> parameter.
        /// </para>
        /// <para>
        /// This method returns the <see cref="IGorgonImageUpdateFluent"/> interface to allow users to modify the image modification after decompression. This means that this method calls 
        /// <see cref="BeginUpdate"/> implicitly after execution.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonImageUpdateFluent.Compress"/>
        /// <seealso cref="BeginUpdate"/>
        IGorgonImageUpdateFluent Decompress(bool useBC1Alpha = false);

        /// <summary>
        /// Function to begin updating the image.
        /// </summary>
        /// <returns>The fluent interface for editing the image.</returns>
        /// <remarks>
        /// <para>
        /// This begins an update to the current image instance by returning a fluent interface (<see cref="IGorgonImageUpdateFluent"/>) that will provide operations that can be performed on the image 
        /// in place. 
        /// </para>
        /// <para>
        /// If the image data is compressed using block compression, this method will throw an exception. Check the <see cref="FormatInfo"/> property to determine if the image has block compressed image 
        /// data. If the image data is block compressed, call the <see cref="Decompress"/> method instead.
        /// </para>
        /// <para>
        /// Once done updating the image, call the <see cref="IGorgonImageUpdateFinalize.EndUpdate"/> method to apply or cancel the changes to the image data. This method must be called if <c>BeginUpdate</c>
        /// is to be called again. Calling <c>BeginUpdate</c> more than once without calling <see cref="IGorgonImageUpdateFinalize.EndUpdate"/> will throw an exception.
        /// </para>
        /// </remarks>
        /// <seealso cref="Decompress"/>
        /// <seealso cref="IGorgonImageUpdateFinalize.EndUpdate"/>
        IGorgonImageUpdateFluent BeginUpdate();
    }
}