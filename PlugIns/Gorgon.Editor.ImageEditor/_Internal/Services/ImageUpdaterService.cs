﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: January 21, 2019 8:31:11 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Graphics.Imaging;
using Gorgon.UI;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides functionality to update an image.
    /// </summary>
    internal class ImageUpdaterService : IImageUpdaterService
    {
        #region Variables.

        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the starting point for an operaiton based on an alignment.
        /// </summary>
        /// <param name="srcSize">The size of the image being cropped/resized.</param>
        /// <param name="destSize">The new size for the image.</param>
        /// <param name="alignment">The alignment of the image for the operation.</param>
        /// <returns></returns>
        private DX.Point GetAnchorStart(DX.Size2 srcSize, ref DX.Size2 destSize, Alignment alignment)
        {
            // Limit the extents if the destination is larger in one direction than the source.
            if (srcSize.Height < destSize.Height)
            {
                destSize.Height = srcSize.Height;
            }

            if (srcSize.Width < destSize.Width)
            {
                destSize.Width = srcSize.Width;
            }

            switch (alignment)
            {
                case Alignment.UpperCenter:
                    return new DX.Point(srcSize.Width / 2 - destSize.Width / 2, 0);
                case Alignment.UpperRight:
                    return new DX.Point(srcSize.Width - destSize.Width, 0);
                case Alignment.CenterLeft:
                    return new DX.Point(0, srcSize.Height / 2 - destSize.Height / 2);
                case Alignment.Center:
                    return new DX.Point(srcSize.Width / 2 - destSize.Width / 2, srcSize.Height / 2 - destSize.Height / 2);
                case Alignment.CenterRight:
                    return new DX.Point(srcSize.Width - destSize.Width, srcSize.Height / 2 - destSize.Height / 2);
                case Alignment.LowerLeft:
                    return new DX.Point(0, srcSize.Height - destSize.Height);
                case Alignment.LowerCenter:
                    return new DX.Point(srcSize.Width / 2 - destSize.Width / 2, srcSize.Height - destSize.Height);
                case Alignment.LowerRight:
                    return new DX.Point(srcSize.Width - destSize.Width, srcSize.Height - destSize.Height);
                default:
                    return DX.Point.Zero;
            }
        }

        /// <summary>
        /// Function to convert the specified image into a 2D image.
        /// </summary>
        /// <param name="image">The image to convert.</param>
        /// <param name="isCubeMap"><b>true</b> if the image should be a cube map, or <b>false</b> if not.</param>
        /// <returns>The converted image.</returns>
        public IGorgonImage ConvertTo2D(IGorgonImage image, bool isCubeMap)
        {
            IGorgonImage result = null;

            try
            {
                var info = new GorgonImageInfo(image, isCubeMap ? ImageType.ImageCube : ImageType.Image2D)
                {
                    ArrayCount = image.ArrayCount,
                    Depth = 1
                };

                if (isCubeMap)
                {
                    while ((info.ArrayCount % 6) != 0)
                    {
                        ++info.ArrayCount;
                    }
                }

                result = new GorgonImage(info);

                // Copy the mip levels and array indices (for cube -> 2D and 2D -> cube, 3D always has an array count of 1).
                for (int array = 0; array < image.ArrayCount; ++array)
                {
                    for (int mip = 0; mip < result.MipCount; ++mip)
                    {
                        image.Buffers[mip, array].CopyTo(result.Buffers[mip, array]);
                    }
                }
            }
            catch
            {
                result?.Dispose();
                throw;
            }

            return result;
        }

        /// <summary>Function to convert the specified image into a volume image.</summary>
        /// <param name="image">The image to convert.</param>
        /// <returns>The converted image.</returns>
        public IGorgonImage ConvertToVolume(IGorgonImage image)
        {
            IGorgonImage result = null;

            try
            {
                result = new GorgonImage(new GorgonImageInfo(image, ImageType.Image3D)
                {
                    ArrayCount = 1,
                    Depth = 1
                });

                // Copy the mip levels.
                for (int i = 0; i < result.MipCount; ++i)
                {
                    image.Buffers[i].CopyTo(result.Buffers[i]);
                }
            }
            catch
            {
                result?.Dispose();
                throw;
            }

            return result;
        }

        /// <summary>
        /// Function to crop an image.
        /// </summary>
        /// <param name="cropImage">The image to crop.</param>
        /// <param name="destSize">The new size of the image.</param>
        /// <param name="alignment">The location to start cropping from.</param>
        public void CropTo(IGorgonImage cropImage, DX.Size2 destSize, Alignment alignment)
        {
            DX.Point startLoc = GetAnchorStart(new DX.Size2(cropImage.Width, cropImage.Height), ref destSize, alignment);
            cropImage.Crop(new DX.Rectangle(startLoc.X, startLoc.Y, destSize.Width, destSize.Height), cropImage.Depth);
        }

        /// <summary>Function to resize the image to fit within the width and height specified.</summary>
        /// <param name="resizeImage">The image tor resize.</param>
        /// <param name="newSize">The new size for the image.</param>
        /// <param name="filter">The filter to apply when resizing.</param>
        /// <param name="preserveAspect"><b>true</b> to preserve the aspect ratio of the image, <b>false</b> to ignore it.</param>
        public void Resize(IGorgonImage resizeImage, DX.Size2 newSize, ImageFilter filter, bool preserveAspect)
        {
            if (preserveAspect)
            {
                float imageScale = ((float)newSize.Width / resizeImage.Width).Min((float)newSize.Height / resizeImage.Height);
                newSize = new DX.Size2((int)(resizeImage.Width * imageScale), (int)(resizeImage.Height * imageScale));
            }

            resizeImage.Resize(newSize.Width, newSize.Height, 1, filter);
        }

        /// <summary>
        /// Function to copy an image onto another image, using the supplied alignment.
        /// </summary>
        /// <param name="srcImage">The image to copy.</param>
        /// <param name="destImage">The destination image.</param>
        /// <param name="startMip">The starting mip map level to copy.</param>
        /// <param name="startArrayOrDepth">The starting array index for 2D images, or depth slice for 3D images.</param>
        /// <param name="alignment">The alignment of the image, relative to the source image.</param>
        public void CopyTo(IGorgonImage srcImage, IGorgonImage destImage, int startMip, int startArrayOrDepth, Alignment alignment)
        {
            int depthCount = destImage.Depth - (destImage.ImageType == ImageType.Image3D ? startArrayOrDepth : 0);
            int mipCount = destImage.MipCount - startMip;
            int arrayCount = destImage.ArrayCount - (destImage.ImageType == ImageType.Image3D ? 0 : startArrayOrDepth);

            int minDepth = depthCount.Min(srcImage.Depth);
            int minMipCount = mipCount.Min(srcImage.MipCount);
            int minArrayCount = arrayCount.Min(srcImage.ArrayCount);

            var size = new DX.Size2(srcImage.Width, srcImage.Height);
            DX.Point startLoc = GetAnchorStart(new DX.Size2(destImage.Width, destImage.Height), ref size, alignment);

            for (int array = 0; array < minArrayCount; ++array)
            {
                for (int mip = 0; mip < minMipCount; ++mip)
                {
                    for (int depth = 0; depth < minDepth; ++depth)
                    {
                        IGorgonImageBuffer srcBuffer = srcImage.Buffers[mip, srcImage.ImageType == ImageType.Image3D ? depth : array];
                        IGorgonImageBuffer destBuffer = destImage.Buffers[mip + startMip, destImage.ImageType == ImageType.Image3D ? (depth + startArrayOrDepth) : (array + startArrayOrDepth)];

                        // Clear the destination buffer before copying.
                        destBuffer.Data.Fill(0);

                        int minWidth = destBuffer.Width.Min(srcBuffer.Width);
                        int minHeight = destBuffer.Height.Min(srcBuffer.Height);
                        var copyRegion = new DX.Rectangle(0, 0, minWidth, minHeight);

                        srcBuffer.CopyTo(destBuffer, copyRegion, startLoc.X, startLoc.Y);
                    }
                }
            }
        }
        #endregion
    }
}
