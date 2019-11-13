#region MIT
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

using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides functionality to update an image.
    /// </summary>
    internal class ImageUpdaterService : IImageUpdaterService
    {
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

            resizeImage.Resize(newSize.Width, newSize.Height, resizeImage.Depth, filter);
        }

        /// <summary>
        /// Function to update the depth slice count for a 3D image, or the array index count for a 2D/cube image.
        /// </summary>
        /// <param name="sourceImage">The image to update.</param>
        /// <param name="arrayOrDepthCount">The new depth or array count.</param>
        /// <returns>A new image with the specified depth/array count, or the same image if no changes were made.</returns>
        public IGorgonImage ChangeArrayOrDepthCount(IGorgonImage sourceImage, int arrayOrDepthCount)
        {
            int currentArrayOrDepth = sourceImage.ImageType == ImageType.Image3D ? sourceImage.Depth : sourceImage.ArrayCount;
            int depthCount = sourceImage.ImageType == ImageType.Image3D ? arrayOrDepthCount : 1;
            int arrayCount = sourceImage.ImageType != ImageType.Image3D ? arrayOrDepthCount : 1;

            if (currentArrayOrDepth == arrayOrDepthCount)
            {
                return sourceImage;
            }

            IGorgonImage newImage = new GorgonImage(new GorgonImageInfo(sourceImage)
            {
                Depth = depthCount,
                ArrayCount = arrayCount
            });

            for (int array = 0; array < arrayCount.Min(sourceImage.ArrayCount); ++array)
            {
                for (int mip = 0; mip < sourceImage.MipCount; ++mip)
                {
                    for (int depth = 0; depth < depthCount.Min(sourceImage.GetDepthCount(mip)); ++depth)
                    {
                        IGorgonImageBuffer srcBuffer = sourceImage.Buffers[mip, sourceImage.ImageType == ImageType.Image3D ? depth : array];
                        IGorgonImageBuffer destBuffer = newImage.Buffers[mip, sourceImage.ImageType == ImageType.Image3D ? depth : array];
                        srcBuffer.CopyTo(destBuffer);
                    }

                    depthCount >>= 1;

                    if (depthCount < 1)
                    {
                        depthCount = 1;
                    }
                }
            }

            return newImage;
        }

        /// <summary>
        /// Function to retrieve an image based on the current mip level.
        /// </summary>
        /// <param name="sourceImage">The image to extract the mip level from.</param>
        /// <param name="currentMipLevel">The mip level to extract.</param>
        /// <returns>A new image with the specified mip level.</returns>
        public IGorgonImage GetMipLevelAsImage(IGorgonImage sourceImage, int currentMipLevel)
        {
            if (sourceImage.MipCount == 1)
            {
                return sourceImage;
            }

            int depthCount = 1;

            if (sourceImage.ImageType == ImageType.Image3D)
            {
                depthCount = sourceImage.GetDepthCount(currentMipLevel);
            }

            IGorgonImage result = new GorgonImage(new GorgonImageInfo(sourceImage)
            {
                Width = sourceImage.Buffers[currentMipLevel].Width,
                Height = sourceImage.Buffers[currentMipLevel].Height,
                Depth = depthCount,
                MipCount = 1
            });

            for (int array = 0; array < sourceImage.ArrayCount; ++array)
            {
                for (int depth = 0; depth < result.Depth; ++depth)
                {
                    IGorgonImageBuffer srcBuffer = sourceImage.Buffers[currentMipLevel, sourceImage.ImageType == ImageType.Image3D ? depth : array];
                    IGorgonImageBuffer destBuffer = result.Buffers[0, sourceImage.ImageType == ImageType.Image3D ? depth : array];

                    srcBuffer.CopyTo(destBuffer);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to retrieve an image based on the current depth slice.
        /// </summary>
        /// <param name="sourceImage">The image to extract the depth slice from.</param>
        /// <param name="currentDepthSlice">The depth slice to extract.</param>
        /// <returns>A new image with the specified depth slice.</returns>
        public IGorgonImage GetDepthSliceAsImage(IGorgonImage sourceImage, int currentDepthSlice)
        {
            if (sourceImage.MipCount == 1)
            {
                return sourceImage;
            }

            IGorgonImage result = new GorgonImage(new GorgonImageInfo(sourceImage, ImageType.Image2D)
            {
                Depth = 1
            });

            for (int mip = 0; mip < result.MipCount; ++mip)
            {
                int depthCount = sourceImage.GetDepthCount(mip);

                if (currentDepthSlice >= depthCount)
                {
                    return result;
                }

                IGorgonImageBuffer srcBuffer = sourceImage.Buffers[mip, currentDepthSlice];
                IGorgonImageBuffer destBuffer = result.Buffers[mip];

                srcBuffer.CopyTo(destBuffer);
            }

            return result;
        }

        /// <summary>
        /// Function to retrieve an image based on the array index.
        /// </summary>
        /// <param name="sourceImage">The image to extract the array index from.</param>
        /// <param name="currentArrayIndex">The array index to extract.</param>
        /// <returns>A new image with the specified array index.</returns>
        public IGorgonImage GetArrayIndexAsImage(IGorgonImage sourceImage, int currentArrayIndex)
        {
            if (sourceImage.ArrayCount == 1)
            {
                return sourceImage;
            }

            IGorgonImage result = new GorgonImage(new GorgonImageInfo(sourceImage)
            {
                ArrayCount = 1
            });

            for (int mip = 0; mip < result.MipCount; ++mip)
            {
                IGorgonImageBuffer srcBuffer = sourceImage.Buffers[mip, currentArrayIndex];
                IGorgonImageBuffer destBuffer = result.Buffers[mip, 0];

                srcBuffer.CopyTo(destBuffer);
            }

            return result;
        }

        /// <summary>
        /// Function to update the number of mip levels on an image.
        /// </summary>
        /// <param name="sourceImage">The source image to update.</param>
        /// <param name="newMipCount">The new number of mip levels for the resulting image.</param>
        /// <returns>The updated image, or the same image if no changes were made.</returns>
        public IGorgonImage ChangeMipCount(IGorgonImage sourceImage, int newMipCount)
        {
            if (sourceImage.MipCount == newMipCount)
            {
                return sourceImage;
            }

            int maxDepth = sourceImage.Depth;
            int minMipCount = newMipCount
                            .Min(sourceImage.MipCount)
                            .Min(GorgonImage.CalculateMaxMipCount(sourceImage.Width, sourceImage.Height, sourceImage.Depth));

            IGorgonImage result = new GorgonImage(new GorgonImageInfo(sourceImage)
            {
                MipCount = newMipCount
            });

            for (int array = 0; array < sourceImage.ArrayCount; ++array)
            {
                for (int mip = 0; mip < minMipCount; ++mip)
                {
                    for (int depth = 0; depth < maxDepth; ++depth)
                    {
                        int depthOrArray = sourceImage.ImageType == ImageType.Image3D ? depth : array;
                        IGorgonImageBuffer src = sourceImage.Buffers[mip, depthOrArray];
                        IGorgonImageBuffer dest = result.Buffers[mip, depthOrArray];
                        src.CopyTo(dest);
                    }

                    maxDepth >>= 1;
                    if (maxDepth < 1)
                    {
                        maxDepth = 1;
                    }
                }
            }

            return result;
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
            int mipCount = destImage.MipCount - startMip;
            int arrayCount = destImage.ArrayCount - (destImage.ImageType == ImageType.Image3D ? 0 : startArrayOrDepth);

            int minMipCount = mipCount.Min(srcImage.MipCount);
            int minArrayCount = arrayCount.Min(srcImage.ArrayCount);

            var size = new DX.Size2(srcImage.Width, srcImage.Height);

            for (int array = 0; array < minArrayCount; ++array)
            {
                for (int mip = 0; mip < minMipCount; ++mip)
                {
                    int destDepthCount = destImage.GetDepthCount(mip + startMip);
                    int minDepth = destDepthCount.Min(srcImage.GetDepthCount(mip));

                    for (int depth = 0; depth < minDepth; ++depth)
                    {
                        int destOffset;
                        if (destImage.ImageType == ImageType.Image3D)
                        {
                            destOffset = depth + startArrayOrDepth;

                            // We're at the end of the destination buffer, skip the rest of the slices.
                            if (destOffset >= destDepthCount)
                            {
                                break;
                            }
                        }
                        else
                        {
                            destOffset = array + startArrayOrDepth;
                        }

                        IGorgonImageBuffer srcBuffer = srcImage.Buffers[mip, srcImage.ImageType == ImageType.Image3D ? depth : array];
                        IGorgonImageBuffer destBuffer = destImage.Buffers[mip + startMip, destOffset];

                        // Clear the destination buffer before copying.
                        destBuffer.Data.Fill(0);

                        int minWidth = destBuffer.Width.Min(srcBuffer.Width);
                        int minHeight = destBuffer.Height.Min(srcBuffer.Height);
                        var copyRegion = new DX.Rectangle(0, 0, minWidth, minHeight);

                        DX.Point startLoc = GetAnchorStart(new DX.Size2(minWidth, minHeight), ref size, alignment);

                        srcBuffer.CopyTo(destBuffer, copyRegion, startLoc.X, startLoc.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Function to set the alpha value for an image.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="currentMipLevel">The current mip map level.</param>
        /// <param name="currentArrayOrDepth">The current array index or depth slice.</param>
        /// <param name="value">The value to assign.</param>
        /// <param name="inclusionRange">The range of alpha values to update.</param>
        /// <returns>A new image with the updated alpha.</returns>
        public IGorgonImage SetAlphaValue(IGorgonImage sourceImage, int currentMipLevel, int currentArrayOrDepth, int value, GorgonRange inclusionRange)
        {
            IGorgonImage result = sourceImage.Clone();

            result.Buffers[currentMipLevel, currentArrayOrDepth]
                  .SetAlpha(value / 255.0f, new GorgonRangeF(inclusionRange.Minimum / 255.0f, inclusionRange.Maximum / 255.0f));

            return result;
        }
        #endregion
    }
}
