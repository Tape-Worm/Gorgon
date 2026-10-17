
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 29, 2016 9:43:51 PM
// 

using Gorgon.Core;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// An image buffer containing the data for a part of a <see cref="IGorgonImage"/>.
/// </summary>
/// <remarks>
/// <para>
/// A buffer retrieved from the <see cref="IGorgonImage.Buffers"/> of an image points into the memory of that image, and does not own it, so disposing that buffer does nothing. A buffer returned by 
/// <see cref="GetRegion"/>, or created with the <see cref="GorgonImageBuffer(int, int, BufferFormat)"/> constructor, owns its memory, and must be disposed when it is no longer needed.
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// A buffer retrieved from the <see cref="IGorgonImage.Buffers"/> of an image is detached when that image is disposed, or when an update to that image (see <see cref="IGorgonImage.BeginUpdate"/>) replaces its 
/// data. A detached buffer has its <see cref="ImageData"/> set to <see cref="GorgonPtr{T}.NullPtr"/>, and its methods throw an <see cref="ObjectDisposedException"/>. Retrieve the buffers again from the image 
/// after an update.
/// </para>
/// </note>
/// </para>
/// </remarks>
public interface IGorgonImageBuffer
    : IGorgonImageInfo, IDisposable
{
    /// <summary>
    /// Property to return the boundaries for the buffer.
    /// </summary>
    GorgonRectangle Bounds
    {
        get;
    }

    /// <summary>
    /// Property to return the format information for the buffer.
    /// </summary>
    GorgonFormatInfo FormatInformation
    {
        get;
    }

    /// <summary>
    /// Property to return the size of the buffer, in bytes.
    /// </summary>
    public long SizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to return the mip map level this buffer represents.
    /// </summary>
    int MipLevel
    {
        get;
    }

    /// <summary>
    /// Property to return the array index this buffer represents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For 3D images, this will always be 0.
    /// </para>
    /// </remarks>
    int ArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the depth slice index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For 1D, 2D or cube images, this will always be 0.
    /// </para>
    /// </remarks>
    int DepthSliceIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the pointer to the native memory holding the pixel data for this image buffer.
    /// </summary>
    GorgonPtr<byte> ImageData
    {
        get;
    }

    /// <summary>
    /// Property to return information about the pitch of the data for this buffer.
    /// </summary>
    GorgonPitchLayout PitchInformation
    {
        get;
    }

    /// <summary>
    /// Function to set the alpha channel for a specific buffer in the image.
    /// </summary>
    /// <param name="alphaValue">The normalized alpha value to set.</param>
    /// <param name="updateAlphaRange">[Optional] The range of normalized alpha values in the buffer that will be updated.</param>
    /// <param name="region">[Optional] The region in the buffer to update.</param>
    /// <exception cref="NotSupportedException">Thrown when the buffer uses a block compressed format.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed, or detached from its image.</exception>
    /// <remarks>
    /// <para>
    /// This will set the alpha channel for the image data in the buffer to a discrete value specified by <paramref name="alphaValue"/>.
    /// </para>
    /// <para>
    /// The <paramref name="alphaValue"/> and <paramref name="updateAlphaRange"/> are normalized values, regardless of the buffer format: 0 to 1 for unsigned formats, and -1 to 1 for 
    /// signed formats. These values are scaled to the range of the alpha channel for the format (e.g. 1.0 is written as 255 for <see cref="BufferFormat.R8G8B8A8_UNorm"/>, 3 for 
    /// <see cref="BufferFormat.R10G10B10A2_UNorm"/>, and 1.0 for <see cref="BufferFormat.R32G32B32A32_Float"/>). Values outside of the range for the format are clamped.
    /// </para>
    /// <para>
    /// If the <paramref name="updateAlphaRange"/> parameter is set, then the alpha values in the buffer will be examined and if the alpha value is less than the minimum range or 
    /// greater than the maximum range, then the <paramref name="alphaValue"/> will <b>not</b> be set on the alpha channel. If it is not set, then the range covers every alpha value 
    /// for the format (0 to 1 for unsigned formats, and -1 to 1 for signed and floating point formats).
    /// </para>
    /// <para>
    /// If the <paramref name="region"/> is not specified, then the entire buffer is updated, otherwise only the values within the <paramref name="region"/> are updated.
    /// </para>
    /// <para>
    /// If the buffer format does not have an alpha channel, then this method does nothing.
    /// </para>
    /// </remarks>
    void SetAlpha(float alphaValue, GorgonRange<float>? updateAlphaRange = null, GorgonRectangle? region = null);

    /// <summary>
    /// Function to copy the image buffer data from this buffer into another.
    /// </summary>
    /// <param name="buffer">The buffer to copy into.</param>
    /// <param name="sourceRegion">[Optional] The region in the source to copy.</param>
    /// <param name="destination">[Optional] The destination offset within the receiving image.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="buffer" /> has no image data (e.g. <see cref="GorgonImageBuffer.Empty"/>).</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="buffer" /> is not the same format as this buffer.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this buffer has been disposed, or detached from its image.</exception>
    /// <remarks>
    /// <para>
    /// This method will copy the contents of this buffer into another buffer and will provide clipping to handle cases where the buffer or <paramref name="sourceRegion" /> is mismatched with the 
    /// destination size. If this buffer, and the buffer passed to <paramref name="buffer"/> share the same pointer address, then this method will return immediately without making any changes.
    /// </para>
    /// <para>
    /// Users may define an area on this buffer to copy by specifying the <paramref name="sourceRegion" /> parameter. If <b>null</b> is passed to this parameter, then the entire buffer will be copied 
    /// to the destination. The <paramref name="sourceRegion"/> is clipped to the bounds of this buffer.
    /// </para>
    /// <para>
    /// An offset into the destination buffer may also be specified to relocate the data copied from this buffer into the destination. Clipping will be applied if the offset pushes the source data 
    /// outside of the boundaries of the destination buffer, including negative offsets, where the part of the source that lands inside the destination is copied.
    /// </para>
    /// <para>
    /// If the clipped region does not overlap the destination buffer, then this method will return without making any changes.
    /// </para>
    /// <para>
    /// When this buffer uses a <see cref="GorgonFormatInfo.IsCompressed">block compressed</see> format, the data is copied in whole 4x4 blocks of pixels, because a block cannot be split. To do this, the 
    /// region being copied is expanded to the block grid: its left and top edges, and the destination offset, are rounded down to the nearest multiple of 4, and its right and bottom edges are rounded up. As a 
    /// result, more data than the <paramref name="sourceRegion"/> requested may be copied, and the data may be placed up to 3 pixels to the left of, and above, the <paramref name="destination"/> offset.
    /// </para>
    /// <para>
    /// To avoid copying more data than intended, it is preferred that the position and size of the <paramref name="sourceRegion"/>, and the <paramref name="destination"/> offset, are multiples of 4. Only the 
    /// right and bottom edges of a buffer whose width or height is not a multiple of 4 are exempt from this.
    /// </para>
    /// <para>
    /// Blocks that do not fit within the destination buffer are not copied. If the width or height of a buffer is not a multiple of 4, then the partially filled blocks along its right and bottom edges are 
    /// copied whole.
    /// </para>
    /// <para>
    /// The destination buffer must be the same format as the source buffer. If it is not, then an exception will be thrown.
    /// </para>
    /// </remarks>
    void CopyTo(IGorgonImageBuffer buffer, GorgonRectangle? sourceRegion = null, GorgonPoint? destination = null);

    /// <summary>
    /// Function to create a sub region from the current image data contained within this buffer.
    /// </summary>
    /// <param name="clipRegion">The region of the buffer to clip.</param>
    /// <returns>A new <see cref="IGorgonImageBuffer"/> containing a copy of the sub region of this buffer, or <see cref="GorgonImageBuffer.Empty"/> if the clipped region is empty.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed, or detached from its image.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to create a smaller sub region from the current buffer based on the <paramref name="clipRegion"/> specified. This region value is clipped to the size of the buffer.
    /// </para>
    /// <para>
    /// The pixel data in the region is <b>copied</b> into a new buffer with the same <see cref="IGorgonImageInfo.Format"/> as this buffer. The new buffer does not share memory with this buffer, so 
    /// changes to one are not seen by the other. Its <see cref="PitchInformation"/> describes the size of the region, not the size of this buffer.
    /// </para>
    /// <para>
    /// If the clipped region has no width or height, then <see cref="GorgonImageBuffer.Empty"/> is returned.
    /// </para>
    /// <para>
    /// When this buffer uses a <see cref="GorgonFormatInfo.IsCompressed">block compressed</see> format, the region is expanded to whole 4x4 blocks of pixels, because a block cannot be split. Its left and top 
    /// edges are rounded down to the nearest multiple of 4, and its right and bottom edges are rounded up, but not past the edges of this buffer. The returned buffer may therefore be larger than the 
    /// <paramref name="clipRegion"/> requested, and its top-left pixel corresponds to the rounded-down position in this buffer, not to the position of the <paramref name="clipRegion"/>.
    /// </para>
    /// <para>
    /// To avoid receiving more data than intended, it is preferred that the position and size of the <paramref name="clipRegion"/> are multiples of 4. Only the right and bottom edges of a buffer whose width 
    /// or height is not a multiple of 4 are exempt from this.
    /// </para>
    /// <para>
    /// If the region includes the partially filled blocks along the right or bottom edge of this buffer, then the returned buffer has the same partial size. For example, a region at (4, 4) in a 6x6 buffer 
    /// returns a 2x2 buffer.
    /// </para>
    /// <para>
    /// The returned buffer is <b>not</b> added to the <see cref="IGorgonImage.Buffers"/> list of the <see cref="IGorgonImage"/> that this buffer belongs to.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The returned buffer owns its memory, so it must be disposed by calling its <see cref="IDisposable.Dispose"/> method when it is no longer needed.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    IGorgonImageBuffer GetRegion(GorgonRectangle clipRegion);

    /// <summary>
    /// Function to fill the entire buffer with the specified byte value.
    /// </summary>
    /// <param name="value">The byte value used to fill the buffer.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer has been disposed, or detached from its image.</exception>
    void Fill(byte value);
}
