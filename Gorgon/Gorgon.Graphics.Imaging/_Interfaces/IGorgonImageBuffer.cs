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
// Created: June 29, 2016 9:43:51 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics.Imaging
{
    /// <summary>
    /// An image buffer containing data about a part of a <see cref="IGorgonImage"/>.
    /// </summary>
    public interface IGorgonImageBuffer
    {
        /// <summary>
        /// Property to return the format of the buffer.
        /// </summary>
        BufferFormat Format
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
        /// Property to return the width for the current buffer.
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// Property to return the height for the current buffer.
        /// </summary>
        /// <remarks>This is only valid for 2D and 3D images.</remarks>
        int Height
        {
            get;
        }

        /// <summary>
        /// Property to return the total depth for the <see cref="IGorgonImage"/> that this buffer is associated with.
        /// </summary>
        /// <remarks>This is only valid for 3D images.</remarks>
        int Depth
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
        /// Property to return the array this buffer represents.
        /// </summary>
        /// <remarks>For 3D images, this will always be 0.</remarks>
        int ArrayIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the depth slice index.
        /// </summary>
        /// <remarks>For 1D or 2D images, this will always be 0.</remarks>
        int DepthSliceIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the native memory buffer holding the data for this image buffer.
        /// </summary>
        GorgonPtr<byte> Data
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
        /// <param name="alphaValue">The value to set.</param>
        /// <param name="updateAlphaRange">[Optional] The range of alpha values in the buffer that will be updated.</param>
        /// <param name="region">[Optional] The region in the buffer to update.</param>
        /// <remarks>
        /// <para>
        /// This will set the alpha channel for the image data in the buffer> to a discrete value specified by <paramref name="alphaValue"/>. 
        /// </para>
        /// <para>
        /// If the <paramref name="updateAlphaRange"/> parameter is set, then the alpha values in the buffer will be examined and if the alpha value is less than the minimum range or 
        /// greater than the maximum range, then the <paramref name="alphaValue"/> will <b>not</b> be set on the alpha channel.
        /// </para>
        /// <para>
        /// If the <paramref name="region"/> is not specified, then the entire buffer is updated, otherwise only the values within the <paramref name="region"/> are updated. 
        /// </para>
        /// </remarks>
        void SetAlpha(float alphaValue, GorgonRangeF? updateAlphaRange = null, DX.Rectangle? region = null);

        /// <summary>
        /// Function to copy the image buffer data from this buffer into another.
        /// </summary>
        /// <param name="buffer">The buffer to copy into.</param>
        /// <param name="sourceRegion">[Optional] The region in the source to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset in the destination buffer.</param>
        /// <param name="destY">[Optional] Vertical offset in the destination buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="buffer" /> is not the same format as this buffer.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the source region does not fit within the bounds of this buffer.</exception>
        /// <remarks>
        /// <para>
        /// This method will copy the contents of this buffer into another buffer and will provide clipping to handle cases where the buffer or <paramref name="sourceRegion" /> is mismatched with the 
        /// destination size. If this buffer, and the buffer passed to <paramref name="buffer"/> share the same pointer address, then this method will return immediately without making any changes.
        /// </para>
        /// <para>
        /// Users may define an area on this buffer to copy by specifying the <paramref name="sourceRegion" /> parameter. If <b>null</b> is passed to this parameter, then the entire buffer will be copied 
        /// to the destination.
        /// </para>
        /// <para>
        /// An offset into the destination buffer may also be specified to relocate the data copied from this buffer into the destination.  Clipping will be applied if the offset pushes the source data 
        /// outside of the boundaries of the destination buffer.
        /// </para>
        /// <para>
        /// The destination buffer must be the same format as the source buffer.  If it is not, then an exception will be thrown.
        /// </para>
        /// </remarks>
        void CopyTo(IGorgonImageBuffer buffer, in DX.Rectangle? sourceRegion = null, int destX = 0, int destY = 0);

        /// <summary>
        /// Function to create a sub region from the current image data contained within this buffer.
        /// </summary>
        /// <param name="clipRegion">The region of the buffer to clip.</param>
        /// <returns>A new <see cref="IGorgonImageBuffer"/> containing the sub region of this buffer, or <b>null</b> if the clipped region is empty.</returns> 
        /// <remarks>
        /// <para>
        /// This method is used to create a smaller sub region from the current buffer based on the <paramref name="clipRegion"/> specified. This region value is clipped to the size of the buffer.
        /// </para>
        /// <para>
        /// The resulting image buffer that is returned will share the same memory as the parent buffer (which, in turn, shares its buffer with the <see cref="IGorgonImage"/> it's created from). Because of
        /// this, the <see cref="Format"/>, <see cref="MipLevel"/>, <see cref="ArrayIndex"/>, <see cref="DepthSliceIndex"/> and
        /// the <see cref="Depth"/> will the be same as the buffer it was created from. 
        /// </para>
        /// <para>
        /// Because this buffer references a subsection of the same memory as the parent buffer, care must be taken when accessing the memory directly. Even though the <see cref="GorgonNativeBuffer{T}"/>
        /// object takes precautions to avoid out of bounds reads/writes on memory, it cannot address memory in a rectangular region like that of an image. If a write that extends beyond the width of the
        /// buffer occurs, it will appear on the parent buffer, but may not appear on the resulting buffer. To handle accessing memory properly, use of the values in <see cref="PitchInformation"/> is
        /// required so that data will be read and written within the region defined by the resulting buffer.
        /// </para>
        /// <para>
        /// If the width and/or height of the clip region is 0, then the image is empty and this method will return <b>null</b>.
        /// </para>
        /// <para>
        /// Please note that the returned buffer will <b>not</b> be appended to the list of <see cref="IGorgonImage.Buffers"/> in the <see cref="IGorgonImage"/>.
        /// </para> 
        /// </remarks>
        /// <seealso cref="IGorgonImage"/>
	    IGorgonImageBuffer GetRegion(in DX.Rectangle clipRegion);

        /// <summary>
        /// Function to fill the entire buffer with the specified byte value.
        /// </summary>
        /// <param name="value">The byte value used to fill the buffer.</param>
	    void Fill(byte value);
    }
}