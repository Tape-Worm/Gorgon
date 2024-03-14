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
// Created: June 20, 2016 10:52:04 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Diagnostics;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// A container for a list of image buffers.
/// </summary>
class ImageBufferList
    : IGorgonImageBufferList
{
    #region Variables.
    // List of buffers.
    private IGorgonImageBuffer[] _buffers;
    // Image that owns this buffer.
    private readonly IGorgonImageInfo _imageInfo;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the offsets of the mip map levels.
    /// </summary>
    internal (int BufferIndex, int MipDepth)[] MipOffsetSize
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    public int Count => _buffers.Length;

    /// <summary>
    /// Gets the element at the specified index in the read-only list.
    /// </summary>
    /// <exception cref="NotSupportedException">This list is read only.</exception>
    IGorgonImageBuffer IReadOnlyList<IGorgonImageBuffer>.this[int index] => _buffers[index];

    /// <summary>
    /// Property to return the buffer for the given mip map level and depth slice.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the array index or the depth slice parameters are larger than their respective boundaries, or less than 0. Only thrown when this assembly is compiled in DEBUG mode.</exception>
    /// <remarks>
    /// <para>
    /// To get the array length, or the mip map count, use the <see cref="P:Gorgon.Graphics.GorgonImageData.Settings">Settings</see> property.
    /// </para>
    /// <para>
    /// To get the depth slice count, use the <see cref="IGorgonImage.GetDepthCount"/> method.
    /// </para>
    /// <para>
    /// The <paramref name="depthSliceOrArrayIndex"/> parameter is used as an array index if the image is 1D or 2D.  If it is a 3D image, then the value indicates a depth slice.
    /// </para>
    /// </remarks>
    public IGorgonImageBuffer this[int mipLevel, int depthSliceOrArrayIndex = 0]
    {
        get
        {
            (int, int) offsetSize;

            mipLevel.ValidateRange(nameof(mipLevel), 0, _imageInfo.MipCount);

            if (_imageInfo.ImageType == ImageType.Image3D)
            {
                depthSliceOrArrayIndex.ValidateRange("arrayIndexDepthSlice", 0, _imageInfo.Depth);
                offsetSize = MipOffsetSize[mipLevel];
                return _buffers[offsetSize.Item1 + depthSliceOrArrayIndex];
            }

            depthSliceOrArrayIndex.ValidateRange("arrayIndexDepthSlice", 0, _imageInfo.ArrayCount);
            offsetSize = MipOffsetSize[mipLevel + (depthSliceOrArrayIndex * _imageInfo.MipCount)];
            return _buffers[offsetSize.Item1];
        }
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to create a list of buffers to use.
    /// </summary>
    /// <param name="data">Data to copy/reference.</param>
    internal void CreateBuffers(in GorgonPtr<byte> data)
    {
        int bufferIndex = 0;
        var formatInfo = new GorgonFormatInfo(_imageInfo.Format);   // Format information.

        // Allocate enough room for the array and mip levels.
        _buffers = new IGorgonImageBuffer[GorgonImage.CalculateDepthSliceCount(_imageInfo.Depth, _imageInfo.MipCount) * _imageInfo.ArrayCount];

        // Offsets for the mip maps.
        MipOffsetSize = new (int BufferIndex, int MipDepth)[_imageInfo.MipCount * _imageInfo.ArrayCount];

        GorgonPtr<byte> dataAddress = data;

        // Enumerate array indices. (For 1D and 2D only, 3D will always be 1)
        for (int array = 0; array < _imageInfo.ArrayCount; array++)
        {
            int mipWidth = _imageInfo.Width;
            int mipHeight = _imageInfo.Height;
            int mipDepth = _imageInfo.Depth;

            // Enumerate mip map levels.
            for (int mip = 0; mip < _imageInfo.MipCount; mip++)
            {
                int arrayIndex = mip + (array * _imageInfo.MipCount);
                GorgonPitchLayout pitchInformation = formatInfo.GetPitchForFormat(mipWidth, mipHeight);

                // Calculate buffer offset by mip.
                MipOffsetSize[arrayIndex] = (bufferIndex, mipDepth);

                // Enumerate depth slices.
                for (int depth = 0; depth < mipDepth; depth++)
                {
                    // Get mip information.						
                    _buffers[bufferIndex] = new GorgonImageBuffer(new GorgonPtr<byte>(dataAddress, pitchInformation.SlicePitch),
                                                                    pitchInformation,
                                                                    mip,
                                                                    array,
                                                                    depth,
                                                                    mipWidth,
                                                                    mipHeight,
                                                                    mipDepth,
                                                                    _imageInfo.Format,
                                                                    formatInfo);

                    dataAddress += pitchInformation.SlicePitch;
                    bufferIndex++;
                }

                if (mipWidth > 1)
                {
                    mipWidth >>= 1;
                }

                if (mipHeight > 1)
                {
                    mipHeight >>= 1;
                }

                if (mipDepth > 1)
                {
                    mipDepth >>= 1;
                }
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}" /> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<IGorgonImageBuffer> GetEnumerator()
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (IGorgonImageBuffer buffer in _buffers)
        {
            yield return buffer;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => _buffers.GetEnumerator();

    /// <summary>
    /// Function to retrieve the index of a given buffer within the list.
    /// </summary>
    /// <param name="buffer">The buffer to look up.</param>
    /// <returns>The index of the buffer within the list, or -1 if not found.</returns>
    public int IndexOf(IGorgonImageBuffer buffer) => buffer is null ? -1 : Array.IndexOf(_buffers, buffer);

    /// <summary>
    /// Function to retrieve the index of a buffer within the list using a mip map level and optional depth slice or array index.
    /// </summary>
    /// <param name="mipLevel">The mip map level to look up.</param>
    /// <param name="depthSliceOrArrayIndex">[Optional] The depth slice (for 3D images) or array index (for 1D or 2D images) to look up.</param>
    /// <returns>The index of the buffer within the list, or -1 if not found.</returns>
    public int IndexOf(int mipLevel, int depthSliceOrArrayIndex = 0)
    {
        (int, int) offsetSize;

        mipLevel = mipLevel.Max(0).Min(_imageInfo.MipCount - 1);

        if (_imageInfo.ImageType == ImageType.Image3D)
        {
            depthSliceOrArrayIndex = depthSliceOrArrayIndex.Max(0).Min(_imageInfo.Depth - 1);
            offsetSize = MipOffsetSize[mipLevel];

            return offsetSize.Item1 + depthSliceOrArrayIndex;
        }

        depthSliceOrArrayIndex = depthSliceOrArrayIndex.Max(0).Min(_imageInfo.ArrayCount - 1);
        offsetSize = MipOffsetSize[mipLevel + (depthSliceOrArrayIndex * _imageInfo.MipCount)];
        return offsetSize.Item1;
    }

    /// <summary>
    /// Function to determine if a buffer with the given mip level and optional depth slice or array index.
    /// </summary>
    /// <param name="mipLevel">The mip map level to look up.</param>
    /// <param name="depthSliceOrArrayIndex">[Optional] The depth slice (for 3D images) or array index (for 1D or 2D images) to look up.</param>
    /// <returns></returns>
    public bool Contains(int mipLevel, int depthSliceOrArrayIndex = 0) => IndexOf(mipLevel, depthSliceOrArrayIndex) != -1;
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBufferList"/> class.
    /// </summary>
    /// <param name="imageInfo">The image information for the image that will hold this list.</param>
    internal ImageBufferList(IGorgonImageInfo imageInfo)
    {
        _imageInfo = imageInfo;
        _buffers = [];
    }
    #endregion
}
