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
// Created: June 20, 2016 10:53:06 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// A container for a list of image buffers.
/// </summary>
public interface IGorgonImageBufferList
    : IReadOnlyList<IGorgonImageBuffer>
{
    /// <summary>
    /// Property to return the buffer for the given mip map level and depth slice.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the array index or the depth slice parameters are larger than their respective boundaries, or less than 0. Only thrown when this assembly is compiled in DEBUG mode.</exception>
    /// <remarks>
    /// <para>
    /// To get the array length, or the mip map count, use the <see cref="IGorgonImage"/>.<see cref="IGorgonImageInfo.ArrayCount"/>, or <see cref="IGorgonImage"/>.<see cref="IGorgonImageInfo.MipCount"/> property.
    /// </para>
    /// <para>
    /// To get the depth slice count, use the <see cref="IGorgonImage.GetDepthCount"/> method.
    /// </para>
    /// <para>
    /// The <paramref name="depthSliceOrArrayIndex"/> parameter is used as an array index if the image is 1D or 2D.  If it is a 3D image, then the value indicates a depth slice.
    /// </para>
    /// </remarks>
    IGorgonImageBuffer this[int mipLevel, int depthSliceOrArrayIndex = 0]
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the index of a given buffer within the list.
    /// </summary>
    /// <param name="buffer">The buffer to look up.</param>
    /// <returns>The index of the buffer within the list, or -1 if not found.</returns>
    int IndexOf(IGorgonImageBuffer buffer);

    /// <summary>
    /// Function to retrieve the index of a buffer within the list using a mip map level and optional depth slice or array index.
    /// </summary>
    /// <param name="mipLevel">The mip map level to look up.</param>
    /// <param name="depthSliceOrArrayIndex">[Optional] The depth slice (for 3D images) or array index (for 1D or 2D images) to look up.</param>
    /// <returns>The index of the buffer within the list, or -1 if not found.</returns>
    int IndexOf(int mipLevel, int depthSliceOrArrayIndex = 0);

    /// <summary>
    /// Function to determine if a buffer with the given mip level and optional depth slice or array index.
    /// </summary>
    /// <param name="mipLevel">The mip map level to look up.</param>
    /// <param name="depthSliceOrArrayIndex">[Optional] The depth slice (for 3D images) or array index (for 1D or 2D images) to look up.</param>
    /// <returns><b>true</b> if the mip and/or depth slice/array index are in this list.</returns>
    bool Contains(int mipLevel, int depthSliceOrArrayIndex = 0);
}