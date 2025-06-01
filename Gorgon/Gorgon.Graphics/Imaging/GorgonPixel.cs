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
// Created: June 2, 2025 3:01:33 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// A base class to define how to encode and decode pixel values for a given format.
/// </summary>
/// <typeparam name="T">The type of data that the pixel is stored as in an <see cref="IGorgonImageBuffer"/>.</typeparam>
/// <remarks>
/// <para>
/// This type provides functionality to encode and/or decode a pixel format from its raw native data to/from a <see cref="GorgonColor"/>. It also provides a means of writing (and reading) a pixel to 
/// (and from) a <see cref="IGorgonImageBuffer"/> which allows a limited form of drawing.
/// </para>
/// <para>
/// If an image has an uncommon pixel format, then users can inherit from this class and define their own encoding and decoding. They may even extend the functionality to draw more than simple pixels if that 
/// is what's required.
/// </para>
/// <para>
/// If using a common image format, such as 32bpp RGBA, use the <see cref="GorgonPixels"/> static class to use one of the predefined pixel types. 
/// </para>
/// </remarks>
/// <seealso cref="GorgonColor"/>
/// <seealso cref="GorgonPixels"/>
/// <seealso cref="IGorgonImageBuffer"/>
public abstract class GorgonPixel<T>()
    where T : unmanaged
{
    /// <summary>
    /// Function to determine if the requested format is supported by this pixel codec.
    /// </summary>
    public abstract bool SupportsFormat(BufferFormat format);

    /// <summary>
    /// Function to encode a color as the defined type for the pixel format.
    /// </summary>
    /// <param name="color">The color to encode.</param>
    /// <returns>The type of data stored in the image buffer.</returns>
    public abstract T Encode(GorgonColor color);

    /// <summary>
    /// Function to decode a color from 
    /// </summary>
    /// <param name="value">The value from an image buffer to decode.</param>
    /// <returns>The color represented by the value.</returns>
    public abstract GorgonColor Decode(T value);

    /// <summary>
    /// Function to set a pixel at a specified coordinate with the specified encoded value.
    /// </summary>
    /// <param name="buffer">The buffer that will receive the pixel.</param>
    /// <param name="x">The horizontal position of the pixel.</param>
    /// <param name="y">The vertical position of the pixel.</param>
    /// <param name="value">The encoded value to store in the image buffer.</param>
    /// <remarks>
    /// <para>
    /// This method is used to quickly assign a value to the image buffer. It does this without doing any bounds checking or error checking.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Use this method with extreme caution, no bounds checking is performed and writing outside of the image buffer <see cref="IGorgonImageBuffer.Bounds"/> can lead to corruption or crashes. Ensure that 
    /// the <paramref name="x"/> and <paramref name="y"/> values are within the bounds prior to calling this method.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected unsafe void SetPixelUnsafe(IGorgonImageBuffer buffer, int x, int y, T value) => *((T*)((byte*)(buffer.ImageData + (y * buffer.PitchInformation.RowPitch + x * buffer.FormatInformation.SizeInBytes)))) = value;

    /// <summary>
    /// Function to retrieve a pixel value at a specified coordinate.
    /// </summary>
    /// <param name="buffer">The buffer that contains the pixel.</param>
    /// <param name="x">The horizontal position of the pixel.</param>
    /// <param name="y">The vertical position of the pixel.</param>
    /// <returns>The encoded value of the pixel.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to quickly retrieve a value from the image buffer. It does this without doing any bounds checking or error checking.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Use this method with extreme caution, no bounds checking is performed and reading outside of the image buffer <see cref="IGorgonImageBuffer.Bounds"/> can lead to crashes. Ensure that the 
    /// <paramref name="x"/> and <paramref name="y"/> values are within the bounds prior to calling this method.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected unsafe T GetPixelUnsafe(IGorgonImageBuffer buffer, int x, int y) => *((T*)((byte*)(buffer.ImageData + (y * buffer.PitchInformation.RowPitch + x * buffer.FormatInformation.SizeInBytes))));

    /// <summary>
    /// Function to set a pixel at a specified coordinate with the specified color.
    /// </summary>
    /// <param name="buffer">The buffer that will receive the pixel.</param>
    /// <param name="point">The location of the pixel within the buffer width (and for 2D images, height).</param>
    /// <param name="color">The color for the pixel.</param>
    /// <exception cref="NotSupportedException">Thrown if this pixel type does not support the format of the buffer.</exception>
    /// <remarks>
    /// <para>
    /// This is a standard utility function allows an application to assign a pixel to a point within the buffer. For a 1D image, this is within the width of the image, and for a 2D image this is within the 
    /// height of the image as well as width. 3D images are separated by depth slices, and can have individual pixels set per slice by accessing the slice from an <see cref="IGorgonImage.Buffers"/> list.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="GorgonColor"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(IGorgonImageBuffer buffer, GorgonPoint point, GorgonColor color)
    {
        if (!SupportsFormat(buffer.Format))
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, buffer.Format));
        }

        GorgonRectangle bounds = new(0, 0, buffer.Bounds.Width - 1, buffer.Bounds.Height - 1);

        if (!bounds.Contains(point))
        {
            return;
        }

        SetPixelUnsafe(buffer, point.X, point.Y, Encode(color));
    }

    /// <summary>
    /// Function to retrieve the colour of a pixel at a specified coordinate.
    /// </summary>
    /// <param name="buffer">The buffer containing the pixel to evaluate.</param>
    /// <param name="point">The location of the pixel within the buffer width (and for 2D images, height).</param>
    /// <returns>A <see cref="GorgonColor"/> representing the color if the pixel at the specified point.</returns>
    /// <exception cref="NotSupportedException">Thrown if this pixel type does not support the format of the buffer.</exception>
    /// <remarks>
    /// <para>
    /// This is a standard utility function allows an application to retrieve a pixel color from a point within the buffer. For a 1D image, this is within the width of the image, and for a 2D image this is 
    /// within the height of the image as well as width. 3D images are separated by depth slices, and can have individual pixels set per slice by accessing the slice from an 
    /// <see cref="IGorgonImage.Buffers"/> list.
    /// </para>
    /// <para>
    /// If the pixel <paramref name="point"/> is outside of the <see cref="IGorgonImageBuffer.Bounds"/> of the <paramref name="buffer"/>, then <see cref="GorgonColors.BlackTransparent"/> is returned.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="GorgonColor"/>
    /// <seealso cref="IGorgonImageBuffer"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonColor GetPixel(IGorgonImageBuffer buffer, GorgonPoint point)
    {
        if (!SupportsFormat(buffer.Format))
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, buffer.Format));
        }

        GorgonRectangle bounds = new(0, 0, buffer.Bounds.Width - 1, buffer.Bounds.Height - 1);

        if (bounds.Contains(point))
        {
            return GorgonColors.BlackTransparent;
        }

        return Decode(GetPixelUnsafe(buffer, point.X, point.Y));
    }
}
