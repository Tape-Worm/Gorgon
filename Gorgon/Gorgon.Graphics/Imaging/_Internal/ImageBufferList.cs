
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: June 20, 2016 10:52:04 PM
// 

using System.Buffers;
using System.Collections;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// A container for a list of image buffers
/// </summary>
sealed class ImageBufferList
    : IGorgonImageBufferList, IDisposable
{
    // List of buffers.
    private IGorgonImageBuffer[] _buffers = [];
    // The number of buffers.
    private int _bufferCount;
    // The number of array indices.
    private int _arrayCount;
    // The number of mip levels.
    private int _mipCount;
    // The number of depth slices.
    private int _depth;
    // The type of image.
    private ImageDataType _imageType;

    /// <summary>
    /// Property to return the offsets of the mip map levels.
    /// </summary>
    internal (int BufferIndex, int MipDepth)[] MipOffsetSize
    {
        get;
        private set;
    } = [];

    /// <inheritdoc/>
    public int Count => _bufferCount;

    /// <inheritdoc/>
    IGorgonImageBuffer IReadOnlyList<IGorgonImageBuffer>.this[int index]
    {
        get
        {
            if ((index < 0) || (index >= _bufferCount))
            {
                throw new IndexOutOfRangeException();
            }

            return _buffers[index];
        }
    }

    /// <inheritdoc/>
    public IGorgonImageBuffer this[int mipLevel, int depthSliceOrArrayIndex = 0]
    {
        get
        {
            (int, int) offsetSize;

            if (_bufferCount == 0)
            {
                throw new IndexOutOfRangeException();
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(mipLevel, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mipLevel, _mipCount);

            if (_imageType == ImageDataType.Image3D)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(depthSliceOrArrayIndex, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(depthSliceOrArrayIndex, _depth);

                offsetSize = MipOffsetSize[mipLevel];
                return _buffers[offsetSize.Item1 + depthSliceOrArrayIndex];
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(depthSliceOrArrayIndex, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(depthSliceOrArrayIndex, _arrayCount);

            offsetSize = MipOffsetSize[mipLevel + (depthSliceOrArrayIndex * _mipCount)];
            return _buffers[offsetSize.Item1];
        }
    }

    /// <summary>
    /// Function to dispose of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose of managed and unmanaged resources, <b>false</b> to dispose of unmanaged resources only.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _arrayCount = _bufferCount = _mipCount = _depth = 0;
            _imageType = ImageDataType.Unknown;

            IGorgonImageBuffer[] buffers = Interlocked.Exchange(ref _buffers, []);
            if (buffers.Length > 0)
            {
                ArrayPool<IGorgonImageBuffer>.Shared.Return(buffers, true);
            }
        }
    }

    /// <summary>
    /// Function to create a list of buffers to use.
    /// </summary>
    /// <param name="imageWidth">The width of the image, in pixels.</param>
    /// <param name="imageHeight">The height of the image, in pixels.</param>
    /// <param name="formatInfo">The information about the image pixel format.</param>
    /// <param name="imageDataPtr">The pointer to the image data.</param>
    /// <returns>The actual count and list of image buffers.</returns>
    private (int count, IGorgonImageBuffer[] buffers) CreateBuffers(int imageWidth, int imageHeight, GorgonFormatInfo formatInfo, GorgonPtr<byte> imageDataPtr)
    {
        int bufferIndex = 0;

        // Allocate enough room for the array and mip levels.
        int bufferCount = GorgonImageInfo.GetMaximumDepthSliceCount(_depth, _mipCount) * _arrayCount;
        IGorgonImageBuffer[] buffers = ArrayPool<IGorgonImageBuffer>.Shared.Rent(bufferCount);

        // Offsets for the mip maps.
        MipOffsetSize = new (int BufferIndex, int MipDepth)[_mipCount * _arrayCount];

        GorgonPtr<byte> dataAddress = imageDataPtr;

        // Enumerate array indices. (For 1D and 2D only, 3D will always be 1)
        for (int array = 0; array < _arrayCount; array++)
        {
            int mipWidth = imageWidth;
            int mipHeight = imageHeight;
            int mipDepth = _depth;

            // Enumerate mip map levels.
            for (int mip = 0; mip < _mipCount; mip++)
            {
                int arrayIndex = mip + (array * _mipCount);
                GorgonPitchLayout pitchInformation = formatInfo.GetPitchForFormat(mipWidth, mipHeight);

                // Calculate buffer offset by mip.
                MipOffsetSize[arrayIndex] = (bufferIndex, mipDepth);

                // Enumerate depth slices.
                for (int depth = 0; depth < mipDepth; depth++)
                {
                    // Get mip information.						
                    buffers[bufferIndex] = new GorgonImageBuffer(dataAddress.Slice(0, pitchInformation.SlicePitch),
                                                                    pitchInformation,
                                                                    mip,
                                                                    array,
                                                                    depth,
                                                                    mipWidth,
                                                                    mipHeight,
                                                                    mipDepth,
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

        return (bufferCount, buffers);
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

        if (_bufferCount == 0)
        {
            return -1;
        }

        mipLevel = mipLevel.Max(0).Min(_mipCount - 1);

        if (_imageType == ImageDataType.Image3D)
        {
            depthSliceOrArrayIndex = depthSliceOrArrayIndex.Max(0).Min(_depth - 1);
            offsetSize = MipOffsetSize[mipLevel];

            return offsetSize.Item1 + depthSliceOrArrayIndex;
        }

        depthSliceOrArrayIndex = depthSliceOrArrayIndex.Max(0).Min(_arrayCount - 1);
        offsetSize = MipOffsetSize[mipLevel + (depthSliceOrArrayIndex * _mipCount)];
        return offsetSize.Item1;
    }

    /// <summary>
    /// Function to determine if a buffer with the given mip level and optional depth slice or array index.
    /// </summary>
    /// <param name="mipLevel">The mip map level to look up.</param>
    /// <param name="depthSliceOrArrayIndex">[Optional] The depth slice (for 3D images) or array index (for 1D or 2D images) to look up.</param>
    /// <returns></returns>
    public bool Contains(int mipLevel, int depthSliceOrArrayIndex = 0) => IndexOf(mipLevel, depthSliceOrArrayIndex) != -1;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBufferList"/> class.
    /// </summary>
    /// <param name="info">Information about the image.</param>
    /// <param name="formatInfo">Information about the pixel format.</param>
    /// <param name="data">The pointer to the image data.</param>
    internal ImageBufferList(GorgonImageInfo info, GorgonFormatInfo formatInfo, GorgonPtr<byte> data)
    {
        _imageType = info.ImageType;
        _depth = info.Depth;
        _arrayCount = info.ArrayCount;
        _mipCount = info.MipCount;

        (_bufferCount, _buffers) = CreateBuffers(info.Width, info.Height, formatInfo, data);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBufferList"/> class.
    /// </summary>
    internal ImageBufferList()
    {
    }
}
