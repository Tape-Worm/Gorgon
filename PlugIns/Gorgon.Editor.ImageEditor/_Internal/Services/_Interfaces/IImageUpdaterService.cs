﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 21, 2019 8:58:05 AM
// 

using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Provides functionality to update an image
/// </summary>
internal interface IImageUpdaterService
{
    /// <summary>
    /// Function to copy an image onto another image, using the supplied alignment.
    /// </summary>
    /// <param name="srcImage">The image to copy.</param>
    /// <param name="destImage">The destination image.</param>
    /// <param name="startMip">The starting mip map level to copy.</param>
    /// <param name="startArrayOrDepth">The starting array index for 2D images, or depth slice for 3D images.</param>
    /// <param name="alignment">The alignment of the image, relative to the source image.</param>
    /// <param name="clearDestination"><b>true</b> to clear the destination, <b>false</b> to keep the image contents.</param>
    void CopyTo(IGorgonImage srcImage, IGorgonImage destImage, int startMip, int startArrayOrDepth, Alignment alignment, bool clearDestination = true);

    /// <summary>
    /// Function to resize the image to fit within the width and height specified.
    /// </summary>
    /// <param name="resizeImage">The image tor resize.</param>
    /// <param name="newSize">The new size for the image.</param>
    /// <param name="filter">The filter to apply when resizing.</param>
    /// <param name="preserveAspect"><b>true</b> to preserve the aspect ratio of the image, <b>false</b> to ignore it.</param>
    void Resize(IGorgonImage resizeImage, GorgonPoint newSize, ImageFilter filter, bool preserveAspect);

    /// <summary>
    /// Function to crop an image.
    /// </summary>
    /// <param name="cropImage">The image to crop.</param>
    /// <param name="destSize">The new size of the image.</param>
    /// <param name="alignment">The location to start cropping from.</param>
    void CropTo(IGorgonImage cropImage, GorgonPoint destSize, Alignment alignment);

    /// <summary>
    /// Function to update the number of mip levels on an image.
    /// </summary>
    /// <param name="sourceImage">The source image to update.</param>
    /// <param name="newMipCount">The new number of mip levels for the resulting image.</param>
    /// <returns>The updated image, or the same image if no changes were made.</returns>
    IGorgonImage ChangeMipCount(IGorgonImage sourceImage, int newMipCount);

    /// <summary>
    /// Function to update the depth slice count for a 3D image, or the array index count for a 2D/cube image.
    /// </summary>
    /// <param name="sourceImage">The image to update.</param>
    /// <param name="arrayOrDepthCount">The new depth or array count.</param>
    /// <returns>A new image with the specified depth/array count, or the same image if no changes were made.</returns>
    IGorgonImage ChangeArrayOrDepthCount(IGorgonImage sourceImage, int arrayOrDepthCount);

    /// <summary>
    /// Function to convert the specified image into a volume image.
    /// </summary>
    /// <param name="image">The image to convert.</param>
    /// <returns>The converted image.</returns>
    IGorgonImage ConvertToVolume(IGorgonImage image);

    /// <summary>
    /// Function to convert the specified image into a 2D image.
    /// </summary>
    /// <param name="image">The image to convert.</param>
    /// <param name="isCubeMap"><b>true</b> if the image should be a cube map, or <b>false</b> if not.</param>
    /// <returns>The converted image.</returns>
    IGorgonImage ConvertTo2D(IGorgonImage image, bool isCubeMap);

    /// <summary>
    /// Function to retrieve an image based on the current mip level.
    /// </summary>
    /// <param name="sourceImage">The image to extract the mip level from.</param>
    /// <param name="currentMipLevel">The mip level to extract.</param>
    /// <returns>A new image with the specified mip level.</returns>
    IGorgonImage GetMipLevelAsImage(IGorgonImage sourceImage, int currentMipLevel);

    /// <summary>
    /// Function to retrieve an image based on the array index.
    /// </summary>
    /// <param name="sourceImage">The image to extract the mip level from.</param>
    /// <param name="currentArrayIndex">The array index to extract.</param>
    /// <returns>A new image with the specified mip level.</returns>
    IGorgonImage GetArrayIndexAsImage(IGorgonImage sourceImage, int currentArrayIndex);

    /// <summary>
    /// Function to retrieve an image based on the current depth slice.
    /// </summary>
    /// <param name="sourceImage">The image to extract the depth slice from.</param>
    /// <param name="currentDepthSlice">The depth slice to extract.</param>
    /// <returns>A new image with the specified depth slice.</returns>
    IGorgonImage GetDepthSliceAsImage(IGorgonImage sourceImage, int currentDepthSlice);

    /// <summary>
    /// Function to set the alpha value for an image.
    /// </summary>
    /// <param name="sourceImage">The source image.</param>
    /// <param name="currentMipLevel">The current mip map level.</param>
    /// <param name="currentArrayOrDepth">The current array index or depth slice.</param>
    /// <param name="value">The value to assign.</param>
    /// <param name="inclusionRange">The range of alpha values to update.</param>
    /// <returns>A new image with the updated alpha.</returns>
    IGorgonImage SetAlphaValue(IGorgonImage sourceImage, int currentMipLevel, int currentArrayOrDepth, int value, GorgonRange<int> inclusionRange);
}
