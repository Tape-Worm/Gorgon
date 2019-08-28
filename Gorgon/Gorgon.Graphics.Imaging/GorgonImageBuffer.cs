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
// Created: Tuesday, July 23, 2013 7:58:34 PM
// 
#endregion

using System;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics.Imaging
{
    /// <summary>
    /// An image buffer containing data about a part of a <see cref="IGorgonImage"/>.
    /// </summary>
    public class GorgonImageBuffer
        : IGorgonImageBuffer
    {
        #region Properties.
        /// <summary>
        /// Property to return the format of the buffer.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return the format information for the buffer.
        /// </summary>
        public GorgonFormatInfo FormatInformation
        {
            get;
        }

        /// <summary>
        /// Property to return the width for the current buffer.
        /// </summary>
        public int Width
        {
            get;
        }

        /// <summary>
        /// Property to return the height for the current buffer.
        /// </summary>
        /// <remarks>This is only valid for 2D and 3D images.</remarks>
        public int Height
        {
            get;
        }

        /// <summary>
        /// Property to return the total depth for the <see cref="IGorgonImage"/> that this buffer is associated with.
        /// </summary>
        /// <remarks>This is only valid for 3D images.</remarks>
        public int Depth
        {
            get;
        }

        /// <summary>
        /// Property to return the mip map level this buffer represents.
        /// </summary>
        public int MipLevel
        {
            get;
        }

        /// <summary>
        /// Property to return the array this buffer represents.
        /// </summary>
        /// <remarks>For 3D images, this will always be 0.</remarks>
        public int ArrayIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the depth slice index.
        /// </summary>
        /// <remarks>For 1D or 2D images, this will always be 0.</remarks>
        public int DepthSliceIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the native memory buffer holding the data for this image buffer.
        /// </summary>
        public GorgonNativeBuffer<byte> Data
        {
            get;
        }

        /// <summary>
        /// Property to return information about the pitch of the data for this buffer.
        /// </summary>
        public GorgonPitchLayout PitchInformation
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy the image buffer data from this buffer into another.
        /// </summary>
        /// <param name="buffer">The buffer to copy into.</param>
        /// <param name="sourceRegion">[Optional] The region in the source to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset in the destination buffer.</param>
        /// <param name="destY">[Optional] Vertical offset in the destination buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="buffer"/> has no data.</exception>
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
        public void CopyTo(IGorgonImageBuffer buffer, in DX.Rectangle? sourceRegion = null, int destX = 0, int destY = 0)
        {
            var sourceBufferDims = new DX.Rectangle
            {
                Left = 0,
                Top = 0,
                Right = Width,
                Bottom = Height
            };

            if (buffer?.Data == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Data.SizeInBytes == 0)
            {
                throw new ArgumentEmptyException(nameof(buffer));
            }

            if (buffer.Format != Format)
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_BUFFER_FORMAT_MISMATCH, Format), nameof(buffer));
            }

            // If we're attempting to copy ourselves into... well, ourselves, then do nothing.
            if ((buffer == this)
                || (buffer.Data == Data))
            {
                return;
            }

            DX.Rectangle srcRegion;

            if (sourceRegion == null)
            {
                srcRegion = sourceBufferDims;
            }
            else
            {
                // Clip the rectangle to the buffer size.
                srcRegion = DX.Rectangle.Intersect(sourceRegion.Value, sourceBufferDims);
            }

            // If we've nothing to copy, then leave.
            if (((srcRegion.Right - srcRegion.Left) <= 0)
                || (srcRegion.Bottom - srcRegion.Top) <= 0)
            {
                return;
            }

            // If we try to place this image outside of the target buffer, then do nothing.
            if ((destX >= buffer.Width)
                || (destY >= buffer.Height))
            {
                return;
            }

            // Ensure that the regions actually fit within their respective buffers.
            var dstRegion = DX.Rectangle.Intersect(new DX.Rectangle(destX, destY, srcRegion.Width, srcRegion.Height),
                                                    new DX.Rectangle(0, 0, buffer.Width, buffer.Height));

            // If the source/dest region is empty, then we have nothing to copy.
            if ((srcRegion.IsEmpty)
                || (dstRegion.IsEmpty)
                || ((dstRegion.Right - dstRegion.Left) <= 0)
                || ((dstRegion.Bottom - dstRegion.Top) <= 0))
            {
                return;
            }

            // If the buffers are identical in dimensions and have no offset, then just do a straight copy.
            if ((buffer.Width == Width)
                && (buffer.Height == Height)
                && (srcRegion.Equals(ref dstRegion)))
            {
                Data.CopyTo(buffer.Data);
                return;
            }

            unsafe
            {
                byte* dest = (byte*)buffer.Data;
                byte* source = (byte*)Data;

                // Find out how many bytes each pixel occupies.
                int dataSize = PitchInformation.RowPitch / Width;

                // Number of source bytes/scanline.
                int srcLineSize = dataSize * (srcRegion.Right - srcRegion.Left);
                byte* srcData = source + (srcRegion.Top * PitchInformation.RowPitch) + (srcRegion.Left * dataSize);

                // Number of dest bytes/scanline.
                int dstLineSize = dataSize * (dstRegion.Right - dstRegion.Left);
                byte* dstData = dest + (dstRegion.Top * buffer.PitchInformation.RowPitch) + (dstRegion.Left * dataSize);

                // Get the smallest line size.
                int minLineSize = dstLineSize.Min(srcLineSize);
                int minHeight = (dstRegion.Bottom - dstRegion.Top).Min(srcRegion.Bottom - srcRegion.Top);

                // Finally, copy our data.
                for (int i = 0; i < minHeight; ++i)
                {
                    Unsafe.CopyBlock(dstData, srcData, (uint)minLineSize);

                    srcData += PitchInformation.RowPitch;
                    dstData += buffer.PitchInformation.RowPitch;
                }
            }
        }

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
        /// this, the <see cref="IGorgonImageBuffer.Format"/>, <see cref="IGorgonImageBuffer.MipLevel"/>, <see cref="IGorgonImageBuffer.ArrayIndex"/>, <see cref="IGorgonImageBuffer.DepthSliceIndex"/> and
        /// the <see cref="IGorgonImageBuffer.Depth"/> will the be same as the buffer it was created from. 
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
        public IGorgonImageBuffer GetRegion(in DX.Rectangle clipRegion)
        {
            var finalRegion = DX.Rectangle.Intersect(clipRegion, new DX.Rectangle(0, 0, Width, Height));

            if ((finalRegion.Width <= 0)
                || (finalRegion.Height <= 0))
            {
                return null;
            }

            unsafe
            {
                byte* regionStart = ((byte*)Data) + (finalRegion.Top * PitchInformation.RowPitch) + (finalRegion.Left * FormatInformation.SizeInBytes);
                byte* regionEnd = ((byte*)Data) + (finalRegion.Bottom * PitchInformation.RowPitch) + (finalRegion.Right * FormatInformation.SizeInBytes);
                var pitch = new GorgonPitchLayout(PitchInformation.RowPitch, (int)(regionEnd - regionStart));

                return new GorgonImageBuffer(new GorgonNativeBuffer<byte>(regionStart, pitch.SlicePitch),
                                             pitch,
                                             MipLevel,
                                             ArrayIndex,
                                             DepthSliceIndex,
                                             finalRegion.Width,
                                             finalRegion.Height,
                                             Depth,
                                             Format,
                                             FormatInformation);
            }
        }

        /// <summary>
        /// Function to fill the entire buffer with the specified byte value.
        /// </summary>
        /// <param name="value">The byte value used to fill the buffer.</param>
        public void Fill(byte value) => Data.Fill(value);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageBuffer" /> class.
        /// </summary>
        /// <param name="data">The aliased pointer to the data for this buffer.</param>
        /// <param name="pitchInfo">The pitch info.</param>
        /// <param name="mipLevel">Mip map level.</param>
        /// <param name="arrayIndex">Array index.</param>
        /// <param name="sliceIndex">Slice index.</param>
        /// <param name="width">The width for the buffer.</param>
        /// <param name="height">The height for the buffer.</param>
        /// <param name="depth">The depth for the buffer.</param>
        /// <param name="format">Format of the buffer.</param>
        /// <param name="formatInfo">Format information from the parent image.</param>
        internal GorgonImageBuffer(GorgonNativeBuffer<byte> data,
                                   GorgonPitchLayout pitchInfo,
                                   int mipLevel,
                                   int arrayIndex,
                                   int sliceIndex,
                                   int width,
                                   int height,
                                   int depth,
                                   BufferFormat format,
                                   GorgonFormatInfo formatInfo)
        {
            Data = data;
            PitchInformation = pitchInfo;
            MipLevel = mipLevel;
            ArrayIndex = arrayIndex;
            DepthSliceIndex = sliceIndex;
            Width = width;
            Height = height;
            Depth = depth;
            Format = format;
            FormatInformation = formatInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageBuffer" /> class.
        /// </summary>
        /// <param name="data">The aliased pointer to the data for this buffer.</param>
        /// <param name="width">The width for the buffer.</param>
        /// <param name="height">The height for the buffer.</param>
        /// <param name="format">Format of the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
        public GorgonImageBuffer(GorgonNativeBuffer<byte> data, int width, int height, BufferFormat format)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));

            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
            }

            FormatInformation = new GorgonFormatInfo(format);
            PitchInformation = FormatInformation.GetPitchForFormat(width, height);
            MipLevel = 0;
            ArrayIndex = 0;
            DepthSliceIndex = 0;
            Width = width;
            Height = height;
            Depth = 1;
            Format = format;
        }
        #endregion
    }
}