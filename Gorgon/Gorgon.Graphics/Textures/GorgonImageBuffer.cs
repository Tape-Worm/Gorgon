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
using System.Drawing;
using System.Drawing.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics
{
    /// <summary>
    /// An image buffer containing data about the image.
    /// </summary>
    public class GorgonImageBuffer
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
        /// Property to return the width for the current buffer.
        /// </summary>
        public int Width
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the height for the current buffer.
        /// </summary>
        /// <remarks>This is only valid for 2D and 3D images.</remarks>
        public int Height
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the depth for the current buffer.
        /// </summary>
        /// <remarks>This is only valid for 3D images.</remarks>
        public int Depth
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the mip map level this buffer represents.
        /// </summary>
        public int MipLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the array this buffer represents.
        /// </summary>
        /// <remarks>For 3D images, this will always be 0.</remarks>
        public int ArrayIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the depth slice index.
        /// </summary>
        /// <remarks>For 1D or 2D images, this will always be 0.</remarks>
        public int SliceIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the data stream for the image data.
        /// </summary>
        public GorgonDataStream Data
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return information about the pitch of the data for this buffer.
        /// </summary>
        public GorgonFormatPitch PitchInformation
        {
            get;
            protected set;
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="buffer" /> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="buffer" /> is not the same format as this buffer.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the source region does not fit within the bounds of this buffer.</exception>
		/// <remarks>
		/// This method will copy the contents of this buffer into another buffer and will provide clipping to handle cases where the buffer or <paramref name="sourceRegion" /> is mismatched with the
		/// destination size.
		/// <para>Users may define an area on this buffer to copy by specifying the <paramref name="sourceRegion" /> parameter.  If NULL (<i>Nothing</i> in VB.Net) is passed to this parameter, then the
		/// entire buffer will be copied to the destination.</para><para>An offset into the destination buffer may also be specified to relocate the data copied from this buffer into the destination.  Clipping will be applied if the offset pushes the
		/// source data outside of the boundaries of the destination buffer.</para><para>The destination buffer must be the same format as the source buffer.  If it is not, then an exception will be thrown.</para>
		/// <para>The <paramref name="buffer"/> parameter must not be the same as the this buffer.  An exception will be thrown if an attempt to copy this buffer into itself is made.</para>
		/// <para>If the source region does not fit within the bounds of this buffer, then an exception will be thrown.</para>
		/// </remarks>
	    public unsafe void CopyTo(GorgonImageBuffer buffer, Rectangle? sourceRegion = null, int destX = 0, int destY = 0)
	    {
            var sourceBufferDims = new Rectangle(0, 0, Width, Height);
            Rectangle srcRegion = sourceRegion != null ? sourceRegion.Value : sourceBufferDims;

			if ((buffer == null)
			    || (buffer.Data == null)
			    || (buffer.Data.Length == 0))
			{
				throw new ArgumentNullException(nameof(buffer));	
			}

			if ((buffer == this)
				|| (buffer.Data.BasePointer == Data.BasePointer))
			{
				throw new ArgumentException(Resources.GORGFX_IMAGE_BUFFER_CANT_BE_SAME, nameof(buffer));
			}

			if (buffer.Format != Format)
			{
				throw new ArgumentException(Resources.GORGFX_IMAGE_MUST_BE_SAME_FORMAT, nameof(buffer));
			}

            if (!sourceBufferDims.Contains(srcRegion))
            {
                throw new ArgumentOutOfRangeException(nameof(sourceRegion));
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
				srcRegion.X -= destX;
				srcRegion.Width += destX;
			    destX = 0;
			}

			if (destY < 0)
			{
				srcRegion.Y -= destY;
				srcRegion.Height += destY;
			    destY = 0;
			}

			// Ensure that the regions actually fit within their respective buffers.
			srcRegion = Rectangle.FromLTRB(srcRegion.X.Max(0), srcRegion.Y.Max(0), srcRegion.Right.Min(Width), srcRegion.Bottom.Min(Height));
			Rectangle dstRegion = Rectangle.FromLTRB(destX, destY, (destX + buffer.Width).Min(buffer.Width), (destY + buffer.Height).Min(buffer.Height));

			// If the source/dest region is empty, then we have nothing to copy.
			if ((srcRegion.IsEmpty)
				|| (dstRegion.IsEmpty)
                || (dstRegion.Right < 0)
                || (dstRegion.Bottom < 0))
			{
				return;
			}

			// If the buffers are identical in dimensions and have no offset, then just do a straight copy.
		    if (srcRegion == dstRegion)
		    {
		        DirectAccess.MemoryCopy(buffer.Data.BasePointer, Data.BasePointer, (int)Data.Length);
		        return;
		    }

			// Find out how many bytes each pixel occupies.
			int dataSize = PitchInformation.RowPitch / Width;

			int srcLineSize = dataSize * srcRegion.Width;	// Number of source bytes/scanline.
			var srcData = ((byte*)Data.BasePointer) + (srcRegion.Y * PitchInformation.RowPitch) + (srcRegion.X * dataSize);

			int dstLineSize = dataSize * dstRegion.Width;	// Number of dest bytes/scanline.
			var dstData = ((byte*)buffer.Data.BasePointer) + (dstRegion.Y * buffer.PitchInformation.RowPitch) + (dstRegion.X * dataSize);

			// Get the smallest line size.
			int minLineSize = dstLineSize.Min(srcLineSize);
			int minHeight = dstRegion.Height.Min(srcRegion.Height);

			// Finally, copy our data.
			for (int i = 0; i < minHeight; ++i)
			{
				DirectAccess.MemoryCopy(dstData, srcData, minLineSize);

				srcData += PitchInformation.RowPitch;
				dstData += buffer.PitchInformation.RowPitch;
			}
	    }

        /// <summary>
        /// Function to convert this image buffer into a <see cref="System.Drawing.Image"/> object.
        /// </summary>
        /// <returns>The System.Drawing.Image object containing the image in the buffer.</returns>
        public Image ToGDIImage()
        {
            using(var wic = new GorgonWICImage())
            {
                using(var wicBitmap = wic.CreateWICBitmapFromImageBuffer(this))
                {
                    return wic.CreateGDIImageFromWICBitmap(wicBitmap, PixelFormat.Format32bppArgb);
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageBuffer"/> class.
        /// </summary>
        /// <param name="mipLevel">Mip map level.</param>
        /// <param name="arrayIndex">Array index.</param>
        /// <param name="sliceIndex">Slice index.</param>
        protected GorgonImageBuffer(int mipLevel, int arrayIndex, int sliceIndex)
        {
            MipLevel = mipLevel;
            ArrayIndex = arrayIndex;
            SliceIndex = sliceIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageBuffer" /> class.
        /// </summary>
        /// <param name="dataStart">The data start.</param>
        /// <param name="pitchInfo">The pitch info.</param>
        /// <param name="mipLevel">Mip map level.</param>
        /// <param name="arrayIndex">Array index.</param>
        /// <param name="sliceIndex">Slice index.</param>
        /// <param name="width">The width for the buffer.</param>
        /// <param name="height">The height for the buffer.</param>
        /// <param name="depth">The depth for the buffer.</param>
        /// <param name="format">Format of the buffer.</param>
        internal unsafe GorgonImageBuffer(void* dataStart, GorgonFormatPitch pitchInfo, int mipLevel, int arrayIndex, int sliceIndex, int width, int height, int depth, BufferFormat format)
        {
            Data = new GorgonDataStream(dataStart, pitchInfo.SlicePitch);
            PitchInformation = pitchInfo;
            MipLevel = mipLevel;
            ArrayIndex = arrayIndex;
            SliceIndex = sliceIndex;
            Width = width;
            Height = height;
            Depth = depth;
            Format = format;
        }
        #endregion
    }
}