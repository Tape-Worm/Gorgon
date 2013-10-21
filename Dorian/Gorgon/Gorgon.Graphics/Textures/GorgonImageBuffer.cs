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

using System.Drawing;
using System.Drawing.Imaging;
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// An image buffer containing data about the image.
    /// </summary>
    public unsafe class GorgonImageBuffer
    {
        #region Properties.
        /// <summary>
        /// Property to return the format of the buffer.
        /// </summary>
        public BufferFormat Format
        {
            get;
            private set;
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
        internal GorgonImageBuffer(void* dataStart, GorgonFormatPitch pitchInfo, int mipLevel, int arrayIndex, int sliceIndex, int width, int height, int depth, BufferFormat format)
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