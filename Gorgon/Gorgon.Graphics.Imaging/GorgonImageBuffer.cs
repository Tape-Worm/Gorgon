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
using Gorgon.Graphics.Imaging.Properties;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using Gorgon.Math;
using Gorgon.Native;

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
        public DXGI.Format Format
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
        /// Property to return the depth for the current buffer.
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
        /// Property to return the data stream for the image data.
        /// </summary>
        public IGorgonPointer Data
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
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer" /> parameter is <b>NULL</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="buffer" /> is not the same format as this buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the source region does not fit within the bounds of this buffer.</exception>
		/// <remarks>
		/// <para>
		/// This method will copy the contents of this buffer into another buffer and will provide clipping to handle cases where the buffer or <paramref name="sourceRegion" /> is mismatched with the 
		/// destination size. If this buffer, and the buffer passed to <paramref name="buffer"/> share the same pointer address, then this method will return immediately without making any changes.
		/// </para>
		/// <para>
		/// Users may define an area on this buffer to copy by specifying the <paramref name="sourceRegion" /> parameter. If <b>NULL</b> is passed to this parameter, then the entire buffer will be copied 
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
		public void CopyTo(IGorgonImageBuffer buffer, DX.Rectangle? sourceRegion = null, int destX = 0, int destY = 0)
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

			if (buffer.Data.Size == 0)
			{
				throw new ArgumentException(Resources.GORIMG_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(buffer));
			}

			if (buffer.Format != Format)
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_BUFFER_FORMAT_MISMATCH, Format), nameof(buffer));
			}

			// If we're attempting to copy ourselves into... well, ourselves, then do nothing.
			if ((buffer == this)
				|| (buffer.Data.Address == Data.Address))
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
				srcRegion = new DX.Rectangle
				            {
					            Left = sourceRegion.Value.Left.Max(0).Min(Width - 1),
					            Top = sourceRegion.Value.Top.Max(0).Min(Height - 1),
					            Right = sourceRegion.Value.Right.Max(0).Min(Width - 1),
					            Bottom = sourceRegion.Value.Bottom.Max(0).Min(Height - 1)
				            };
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

			// Adjust in case we're trying to move off the target.
			if (destX < 0)
			{
				srcRegion.Left -= destX;
				srcRegion.Right += destX;
			    destX = 0;
			}

			if (destY < 0)
			{
				srcRegion.Top -= destY;
				srcRegion.Bottom += destY;
			    destY = 0;
			}

			// Ensure that the regions actually fit within their respective buffers.
			var dstRegion = new DX.Rectangle
			                {
				                Left = destX,
				                Top = destY,
				                Right = (destX + (srcRegion.Right - srcRegion.Left)).Min(buffer.Width),
				                Bottom = (destY + (srcRegion.Bottom - srcRegion.Top)).Min(buffer.Height)
			                };

			// If the source/dest region is empty, then we have nothing to copy.
			if ((srcRegion.IsEmpty)
				|| (dstRegion.IsEmpty)
                || ((dstRegion.Right - dstRegion.Left) <= 0)
                || ((dstRegion.Bottom - dstRegion.Top) <= 0))
			{
				return;
			}

			// If the buffers are identical in dimensions and have no offset, then just do a straight copy.
		    if ((srcRegion.Left == dstRegion.Left) 
				&& (srcRegion.Top == dstRegion.Top)
				&& (srcRegion.Right == dstRegion.Right)
				&& (srcRegion.Bottom == dstRegion.Bottom))
		    {
				buffer.Data.CopyFrom(Data, (int)Data.Size);
		        return;
		    }

			unsafe
			{
				byte* dest = (byte*)buffer.Data.Address;
				byte* source = (byte*)Data.Address;

				// Find out how many bytes each pixel occupies.
				int dataSize = PitchInformation.RowPitch / Width;

				// Number of source bytes/scanline.
				int srcLineSize = dataSize * (srcRegion.Right - srcRegion.Left);
				var srcData = source + (srcRegion.Top * PitchInformation.RowPitch) + (srcRegion.Left * dataSize);

				// Number of dest bytes/scanline.
				int dstLineSize = dataSize * (dstRegion.Right - dstRegion.Left);
				var dstData = dest + (dstRegion.Top * buffer.PitchInformation.RowPitch) + (dstRegion.Left * dataSize);

				// Get the smallest line size.
				int minLineSize = dstLineSize.Min(srcLineSize);
				int minHeight = (dstRegion.Bottom - dstRegion.Top).Min(srcRegion.Bottom - srcRegion.Top);

				// Finally, copy our data.
				for (int i = 0; i < minHeight; ++i)
				{
					DirectAccess.MemoryCopy(dstData, srcData, minLineSize);

					srcData += PitchInformation.RowPitch;
					dstData += buffer.PitchInformation.RowPitch;
				}
			}
		}
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
        internal GorgonImageBuffer(GorgonPointerAlias data, GorgonPitchLayout pitchInfo, int mipLevel, int arrayIndex, int sliceIndex, int width, int height, int depth, DXGI.Format format)
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
        }
        #endregion
    }
}