// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: May 6, 2025 11:11:02 PM
//

using Gorgon.Native;
using Windows.Win32.Graphics.Imaging;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// Flags to control how pixel conversion should be handled
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
/// Filter for dithering an image when it is downsampled to a lower bit depth
/// </summary>
public enum ImageDithering
{
    /// <summary>
    /// No dithering.
    /// </summary>
    None = WICBitmapDitherType.WICBitmapDitherTypeNone,
    /// <summary>
    /// <para>
    /// An error diffusion algorithm.
    /// </para>
    /// <para>
    /// This should be used with images that make use of a color palette, such as 8 bit images.
    /// </para>
    /// </summary>
    ErrorDiffusion = WICBitmapDitherType.WICBitmapDitherTypeErrorDiffusion,
    /// <summary>
    /// <para>
    /// A 4x4 ordered dither algorithm.
    /// </para>
    /// <para>
    /// This should be used with images that use 5 or 6 bits of color depth for their color channels.
    /// </para>
    /// </summary>
    Ordered4x4 = WICBitmapDitherType.WICBitmapDitherTypeOrdered4x4,
    /// <summary>
    /// An 8x8 ordered dither algorithm.
    /// </summary>
    Ordered8x8 = WICBitmapDitherType.WICBitmapDitherTypeOrdered8x8,
    /// <summary>
    /// A 16x16 ordered dither algorithm.
    /// </summary>
    Ordered16x16 = WICBitmapDitherType.WICBitmapDitherTypeOrdered16x16,
    /// <summary>
    /// A 4x4 spiral dither algorithm.
    /// </summary>
    Spiral4x4 = WICBitmapDitherType.WICBitmapDitherTypeSpiral4x4,
    /// <summary>
    /// An 8x8 spiral dither algorithm.
    /// </summary>
    Spiral8x8 = WICBitmapDitherType.WICBitmapDitherTypeSpiral8x8,
    /// <summary>
    /// A 4x4 dual spiral dither algorithm.
    /// </summary>
    DualSpiral4x4 = WICBitmapDitherType.WICBitmapDitherTypeDualSpiral4x4,
    /// <summary>
    /// An 8x8 dual spiral dither algorithm.
    /// </summary>
    DualSpiral8x8 = WICBitmapDitherType.WICBitmapDitherTypeDualSpiral8x8,
}

/// <summary>
/// Filter to be applied to an image that's been stretched or shrunk
/// </summary>
public enum ImageFilter
{
    /// <summary>
    /// The output pixel is assigned the value of the pixel that the point falls within. No other pixels are considered.
    /// </summary>
    Point = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor,
    /// <summary>
    /// The output pixel values are computed as a weighted average of the nearest four pixels in a 2x2 grid.
    /// </summary>
    Linear = WICBitmapInterpolationMode.WICBitmapInterpolationModeLinear,
    /// <summary>
    /// Destination pixel values are computed as a weighted average of the nearest sixteen pixels in a 4x4 grid.
    /// </summary>
    Cubic = WICBitmapInterpolationMode.WICBitmapInterpolationModeCubic,
    /// <summary>
    /// Destination pixel values are computed as a weighted average of the all the pixels that map to the new pixel.
    /// </summary>
    Fant = WICBitmapInterpolationMode.WICBitmapInterpolationModeFant,
    /// <summary>
    /// Destination pixel values are computed using a much denser sampling kernel than regular cubic. The kernel is resized in response to the scale factor, making it suitable for downscaling by factors greater than 2.
    /// </summary>
    CubicHighQuality = WICBitmapInterpolationMode.WICBitmapInterpolationModeHighQualityCubic,
}

/// <summary>
/// Holds raw data that is used to represent an image
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="GorgonImage"/> object will hold a blob of data in native (unmanaged) memory and represent that data as a series of pixels to be displayed, or manipulated. This image type is capable 
/// of representing standard 2D images, but can also represent 1D and 3D images. And, depending on the type of image, there is also support for mip map levels, and arrayed images (e.g. cube maps).
/// </para>
/// <para>
/// Images can access their data directly through a <see cref="GorgonPtr{T}"/> interface that allows safe access to raw, unmanaged memory where the image data is stored. In cases where images have 
/// multiple parts like depth slices for a 3D image, or an array for 2D images, this object will provide access through a series of buffers that will point to the individual locations for depth slices, 
/// array indices, and mip map levels. These buffers will also provide their own <see cref="GorgonPtr{T}"/> that will allow safe and direct access to the native memory where the buffer is located
/// </para>
/// <para>
/// Because this object stored data in native memory instead of on the heaps provided by .NET, this object should be disposed by calling its <see cref="IDisposable.Dispose"/> method when it is no 
/// longer required. Failure to do so might cause a temporary memory leak until the garbage collector can deal with it.
/// </para>
/// </remarks>
public interface IGorgonImage
    : IDisposable, IGorgonImageInfo
{
    /// <summary>
    /// Property to return the pointer to the native memory holding the pixel data for the entire image.
    /// </summary>
    /// <remarks>
    /// The memory pointed at by this pointer contains all mip levels, depth slices and array indices of the image. To access each mip level, depth slice or array index, use the <see cref="Buffers"/> 
    /// collection to retrieve a <see cref="IGorgonImageBuffer"/> and use its <see cref="IGorgonImageBuffer.ImageData"/> pointer.
    /// </remarks>
    /// <seealso cref="GorgonPtr{T}"/>
    /// <seealso cref="IGorgonImageBuffer"/>
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
    long SizeInBytes
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds, or equals the number of mip map levels for the image or is less than 0.</exception>
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
    /// Function to copy this image into a new image.
    /// </summary>
    /// <returns>The new image containing and exact duplicate of its data and properties.</returns>
    IGorgonImage Copy();

    /// <summary>
    /// Function to begin updating the image.
    /// </summary>
    /// <returns>The fluent interface for editing the image.</returns>
    /// <exception cref="NotSupportedException">Thrown if the image cannot be updated because of its format.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the image is already being edited.</exception>
    /// <remarks>
    /// <para>
    /// This begins an update to the current image instance by returning a fluent interface (<see cref="IGorgonImageUpdateFluent"/>) that will provide operations that can be performed on the image 
    /// in place. 
    /// </para>
    /// <para>
    /// If the image data is compressed using block compression, this method will throw an exception. Check the <see cref="FormatInfo"/> property to determine if the image has block compressed image 
    /// data.
    /// </para>
    /// <para>
    /// Once done updating the image, call the <see cref="IGorgonImageUpdateFluent.EndUpdate"/> method to apply or cancel the changes to the image data. This method must be called if <c>BeginUpdate</c>
    /// is to be called again. Calling <c>BeginUpdate</c> more than once without calling <see cref="IGorgonImageUpdateFluent.EndUpdate"/> will throw an exception.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImageUpdateFluent.EndUpdate"/>
    IGorgonImageUpdateFluent BeginUpdate();
}
