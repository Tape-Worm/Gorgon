
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
// Created: Tuesday, July 23, 2013 7:58:34 PM
// 

using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// An image buffer containing data about a part of a <see cref="IGorgonImage"/>
/// </summary>
public class GorgonImageBuffer
    : IGorgonImageBuffer
{
    // The data buffer owned by the image buffer.
    private GorgonNativeBuffer<byte>? _ownedBuffer;
    // The bounds for the buffer.
    private readonly GorgonRectangle _bounds;

    /// <summary>
    /// An empty image buffer.
    /// </summary>
    public static readonly GorgonImageBuffer Empty = new(GorgonPtr<byte>.NullPtr, GorgonPitchLayout.Empty, 0, 0, 0, 0, 0, 0, new GorgonFormatInfo(BufferFormat.Unknown));

    /// <inheritdoc/>
    public GorgonRectangle Bounds => _bounds;

    /// <inheritdoc/>
    public BufferFormat Format
    {
        get;
    }

    /// <inheritdoc/>
    public GorgonFormatInfo FormatInformation
    {
        get;
    }

    /// <inheritdoc/>
    public long SizeInBytes
    {
        get;
    }

    /// <inheritdoc/>
    public int Width
    {
        get;
    }

    /// <inheritdoc/>
    public int Height
    {
        get;
    }

    /// <inheritdoc/>
    public int Depth
    {
        get;
    }

    /// <inheritdoc/>
    public int MipLevel
    {
        get;
    }

    /// <inheritdoc/>
    public int ArrayIndex
    {
        get;
    }

    /// <inheritdoc/>
    public int DepthSliceIndex
    {
        get;
    }

    /// <inheritdoc/>
    public GorgonPtr<byte> ImageData
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public GorgonPitchLayout PitchInformation
    {
        get;
    }

    /// <inheritdoc/>
    ImageDataType IGorgonImageInfo.ImageType => Height == 1 ? ImageDataType.Image1D : ImageDataType.Image2D;

    /// <inheritdoc/>
    bool IGorgonImageInfo.HasPremultipliedAlpha => false;

    /// <inheritdoc/>
    int IGorgonImageInfo.MipCount => 1;

    /// <inheritdoc/>
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width & (Width - 1)) == 0) && ((Height & (Height - 1)) == 0);

    /// <inheritdoc/>
    int IGorgonImageInfo.ArrayCount => 1;

    /// <summary>
    /// Function to release managed, and unmanaged, resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to release both managed, and unmanaged resources. <b>false</b> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            GorgonNativeBuffer<byte>? data = Interlocked.Exchange(ref _ownedBuffer, null);

            if (data is not null)
            {
                ImageData = GorgonPtr<byte>.NullPtr;
                data.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    public void SetAlpha(float alphaValue, GorgonRange<float>? updateAlphaRange = null, GorgonRectangle? region = null)
    {
        ObjectDisposedException.ThrowIf(ImageData == GorgonPtr<byte>.NullPtr, this);

        // We don't support compressed formats.
        if (FormatInformation.IsCompressed)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
        }

        // If we don't have an alpha channel, then don't do anything.
        if (!FormatInformation.HasAlpha)
        {
            return;
        }

        updateAlphaRange ??= (FormatInformation.IsFloatingPoint || FormatInformation.IsSigned) ? new GorgonRange<float>(-1.0f, 1.0f) : new GorgonRange<float>(0, 1);

        if (region is null)
        {
            region = _bounds;
        }
        else
        {
            region = GorgonRectangle.Intersect(region.Value, _bounds);
        }

        if ((region.Value.Width <= 0) || (region.Value.Height <= 0))
        {
            return;
        }

        GorgonPtr<byte> src = ImageData;

        float alpha;
        float min = 0;
        float max;
        int rowSize = region.Value.Width * FormatInformation.SizeInBytes;

        if (FormatInformation.IsFloatingPoint)
        {
            float floatAlpha = alphaValue;
            float floatMin = updateAlphaRange.Value.Minimum;
            float floatMax = updateAlphaRange.Value.Maximum;

            if (FormatInformation.IsHalf)
            {
                floatAlpha *= (float)Half.MaxValue;
                floatMin *= (float)Half.MinValue;
                floatMax *= (float)Half.MaxValue;
            }
            else
            {
                floatAlpha *= float.MaxValue;
                floatMin *= float.MinValue;
                floatMax *= float.MaxValue;
            }

            alpha = floatAlpha;
            min = floatMin;
            max = floatMax;
        }
        else if (!FormatInformation.IsSigned)
        {
            switch (FormatInformation.SizeInBytes)
            {
                case 2:
                    alpha = alphaValue * 15;
                    max = updateAlphaRange.Value.Maximum * 15;
                    break;
                case 4:
                    alpha = alphaValue * byte.MaxValue;
                    max = updateAlphaRange.Value.Maximum * byte.MaxValue;
                    break;
                case 8:
                    alpha = alphaValue * ushort.MaxValue;
                    max = updateAlphaRange.Value.Maximum * ushort.MaxValue;
                    break;
                case 16:
                    alpha = alphaValue * uint.MaxValue;
                    max = updateAlphaRange.Value.Maximum * uint.MaxValue;
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
            }
        }
        else
        {
            switch (FormatInformation.SizeInBytes)
            {
                case 4:
                    alpha = alphaValue * sbyte.MaxValue;
                    min = updateAlphaRange.Value.Minimum * sbyte.MinValue;
                    max = updateAlphaRange.Value.Maximum * sbyte.MaxValue;
                    break;
                case 8:
                    alpha = alphaValue * short.MaxValue;
                    min = updateAlphaRange.Value.Minimum * short.MinValue;
                    max = updateAlphaRange.Value.Maximum * short.MaxValue;
                    break;
                case 16:
                    alpha = alphaValue * int.MaxValue;
                    min = updateAlphaRange.Value.Minimum * int.MinValue;
                    max = updateAlphaRange.Value.Maximum * int.MaxValue;
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
            }
        }

        for (int y = region.Value.Top; y < region.Value.Bottom; ++y)
        {
            GorgonPtr<byte> horzPtr = src + (region.Value.Left.Max(0) * FormatInformation.SizeInBytes);
            ImageUtilities.SetAlphaScanline(horzPtr, rowSize, Format, alpha, min, max);
            src += PitchInformation.RowPitch;
        }
    }

    /// <inheritdoc/>
    public void CopyTo(IGorgonImageBuffer buffer, GorgonRectangle? sourceRegion = null, GorgonPoint? destination = null)
    {
        ObjectDisposedException.ThrowIf(ImageData == GorgonPtr<byte>.NullPtr, this);

        // We don't support compressed formats.
        if (FormatInformation.IsCompressed)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
        }

        if (buffer.ImageData.SizeInBytes == 0)
        {
            throw new ArgumentEmptyException(nameof(buffer));
        }

        if (buffer.Format != Format)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_BUFFER_FORMAT_MISMATCH, Format), nameof(buffer));
        }

        // Do nothing if we're using an empty buffer.
        // If we're attempting to copy ourselves into... well, ourselves, then do nothing.
        if ((buffer == this) || (buffer == Empty) || (buffer.ImageData == Empty.ImageData) || (buffer.ImageData == ImageData))
        {
            return;
        }

        destination ??= GorgonPoint.Zero;

        if (sourceRegion is not null)
        {
            // Clip the rectangle to the buffer size.
            sourceRegion = GorgonRectangle.Intersect(sourceRegion.Value, _bounds);
        }
        else
        {
            sourceRegion = _bounds;
        }

        // If we've nothing to copy, then leave.
        if ((sourceRegion.Value.IsEmpty)
            || ((sourceRegion.Value.Right - sourceRegion.Value.Left) <= 0)
            || ((sourceRegion.Value.Bottom - sourceRegion.Value.Top) <= 0))
        {
            return;
        }

        // If we try to place this image outside of the target buffer, then do nothing.
        if ((destination.Value.X >= buffer.Width)
            || (destination.Value.Y >= buffer.Height))
        {
            return;
        }

        // Ensure that the regions actually fit within their respective buffers.
        GorgonRectangle dstRegion = GorgonRectangle.Intersect(new GorgonRectangle(destination.Value.X, destination.Value.Y, sourceRegion.Value.Width, sourceRegion.Value.Height),
                                                              _bounds);

        // If the source/dest region is empty, then we have nothing to copy.
        if ((dstRegion.IsEmpty)
            || ((dstRegion.Right - dstRegion.Left) <= 0)
            || ((dstRegion.Bottom - dstRegion.Top) <= 0))
        {
            return;
        }

        // If the buffers are identical in dimensions and have no offset, then just do a straight copy.
        if ((buffer.Width == Width)
            && (buffer.Height == Height)
            && (sourceRegion.Value.Equals(dstRegion)))
        {
            ImageData.CopyTo(buffer.ImageData);
            return;
        }

        // Number of source bytes/scanline.
        int srcLineSize = FormatInformation.SizeInBytes * (sourceRegion.Value.Right - sourceRegion.Value.Left);
        GorgonPtr<byte> srcData = ImageData + (sourceRegion.Value.Top * PitchInformation.RowPitch) + (sourceRegion.Value.Left * FormatInformation.SizeInBytes);

        // Number of dest bytes/scanline.
        int dstLineSize = FormatInformation.SizeInBytes * (dstRegion.Right - dstRegion.Left);
        GorgonPtr<byte> dstData = buffer.ImageData + (dstRegion.Top * buffer.PitchInformation.RowPitch) + (dstRegion.Left * FormatInformation.SizeInBytes);

        // Get the smallest line size.
        int minLineSize = dstLineSize.Min(srcLineSize);
        int minHeight = (dstRegion.Bottom - dstRegion.Top).Min(sourceRegion.Value.Bottom - sourceRegion.Value.Top);

        // Finally, copy our data.
        for (int i = 0; i < minHeight; ++i)
        {
            srcData.Slice(0, minLineSize).CopyTo(dstData.Slice(0, minLineSize));

            srcData += PitchInformation.RowPitch;
            dstData += buffer.PitchInformation.RowPitch;
        }
    }

    /// <inheritdoc/>
    public IGorgonImageBuffer GetRegion(GorgonRectangle clipRegion)
    {
        ObjectDisposedException.ThrowIf(ImageData == GorgonPtr<byte>.NullPtr, this);

        // We don't support compressed formats.
        if (FormatInformation.IsCompressed)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
        }

        GorgonRectangle finalRegion = GorgonRectangle.Intersect(clipRegion, _bounds);

        if ((finalRegion.Width <= 0)
            || (finalRegion.Height <= 0))
        {
            return Empty;
        }

        GorgonImageBuffer result = new(finalRegion.Width, finalRegion.Height, Format);

        CopyTo(result, clipRegion);

        return result;
    }

    /// <inheritdoc/>
    public void Fill(byte value)
    {
        ObjectDisposedException.ThrowIf(ImageData == GorgonPtr<byte>.NullPtr, this);

        ImageData.Fill(value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
    /// <param name="formatInfo">Format information from the parent image.</param>
    internal GorgonImageBuffer(GorgonPtr<byte> data,
                               GorgonPitchLayout pitchInfo,
                               int mipLevel,
                               int arrayIndex,
                               int sliceIndex,
                               int width,
                               int height,
                               int depth,
                               GorgonFormatInfo formatInfo)
    {
        _bounds = new GorgonRectangle(0, 0, width, height);
        ImageData = data;
        PitchInformation = pitchInfo;
        MipLevel = mipLevel;
        ArrayIndex = arrayIndex;
        DepthSliceIndex = sliceIndex;
        Width = width;
        Height = height;
        Depth = depth;
        Format = formatInfo.Format;
        FormatInformation = formatInfo;
        SizeInBytes = pitchInfo.SlicePitch;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImageBuffer"/> class.
    /// </summary>
    /// <param name="width">The width for the buffer.</param>
    /// <param name="height">The height for the buffer.</param>
    /// <param name="format">Format of the buffer.</param>
    /// <remarks>
    /// <para>
    /// This constructor creates a new image buffer that users can use independently of a <see cref="IGorgonImage"/>. It can be used for updating image information periodically and copying it back into a base 
    /// image. Or it can be used for a temporary buffer for a completely separate operation. 
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This type implements <see cref="IDisposable"/>, ensure that the <see cref="IDisposable.Dispose"/> method is called on any instance when you are finished with it. Otherwise, a temporary memory leak 
    /// may occur.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonImageBuffer(int width, int height, BufferFormat format)
    {
        if (format == BufferFormat.Unknown)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
        }

        FormatInformation = new GorgonFormatInfo(format);

        // We don't support compressed formats.
        if (FormatInformation.IsCompressed)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
        }

        long size = ((long)width * FormatInformation.SizeInBytes) * height;

        if (size <= 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_BUFFER_TOO_SMALL);
        }

        _bounds = new GorgonRectangle(0, 0, width, height);
        _ownedBuffer = new GorgonNativeBuffer<byte>(size);
        ImageData = (GorgonPtr<byte>)_ownedBuffer;

        PitchInformation = FormatInformation.GetPitchForFormat(width, height);
        MipLevel = 0;
        ArrayIndex = 0;
        DepthSliceIndex = 0;
        Width = width;
        Height = height;
        Depth = 1;
        Format = format;
        SizeInBytes = size;
    }
}
