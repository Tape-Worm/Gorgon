
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

/// <inheritdoc cref="IGorgonImageBuffer"/>
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
    /// <remarks>
    /// <para>
    /// This buffer has no image data and is treated as a disposed buffer. Calling any of its methods that read or write image data will throw an <see cref="ObjectDisposedException"/>.
    /// </para>
    /// </remarks>
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

        GorgonRectangle updateRegion = region is null ? _bounds : GorgonRectangle.Intersect(region.Value, _bounds);

        if ((updateRegion.Width <= 0) || (updateRegion.Height <= 0))
        {
            return;
        }

        int rowSize = updateRegion.Width * FormatInformation.SizeInBytes;
        GorgonPtr<byte> rowPtr = ImageData + (updateRegion.Top * PitchInformation.RowPitch) + (updateRegion.Left * FormatInformation.SizeInBytes);

        for (int y = 0; y < updateRegion.Height; ++y)
        {
            ImageUtilities.SetAlphaScanline(rowPtr, rowSize, Format, alphaValue, updateAlphaRange.Value.Minimum, updateAlphaRange.Value.Maximum);
            rowPtr += PitchInformation.RowPitch;
        }    
    }

    /// <inheritdoc/>
    public void CopyTo(IGorgonImageBuffer buffer, GorgonRectangle? sourceRegion = null, GorgonPoint? destination = null)
    {
        ObjectDisposedException.ThrowIf(ImageData == GorgonPtr<byte>.NullPtr, this);

        if (buffer.ImageData.Equals(GorgonPtr<byte>.NullPtr))
        {
            throw new ArgumentEmptyException(nameof(buffer));
        }

        if (buffer.Format != Format)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_BUFFER_FORMAT_MISMATCH, Format), nameof(buffer));
        }

        // If we're attempting to copy ourselves into... well, ourselves, then do nothing.
        if ((buffer == this) || (buffer.ImageData == ImageData))
        {
            return;
        }

        GorgonPoint destOffset = destination ?? GorgonPoint.Zero;
        GorgonRectangle srcRect = sourceRegion is null ? _bounds : GorgonRectangle.Intersect(sourceRegion.Value, _bounds);

        GorgonRectangle dstRect = GorgonRectangle.Intersect(new GorgonRectangle(destOffset.X, destOffset.Y,srcRect.Width, srcRect.Height),
                                                            new GorgonRectangle(0, 0, buffer.Width, buffer.Height));

        // If nothing lands in the destination, then there's nothing to copy.
        if ((dstRect.Width <= 0) || (dstRect.Height <= 0))
        {
            return;
        }

        // Move the source start by whatever was clipped off the destination's left/top edges.
        int srcX = srcRect.X + (dstRect.X - destOffset.X);
        int srcY = srcRect.Y + (dstRect.Y - destOffset.Y);

        if ((srcX == 0)
            && (srcY == 0)
            && (dstRect.Equals(_bounds))
            && (buffer.Width == Width)
            && (buffer.Height == Height)
            && (buffer.PitchInformation.RowPitch == PitchInformation.RowPitch))
        {
            ImageData.CopyTo(buffer.ImageData);
            return;
        }

        GorgonPtr<byte> srcData;
        GorgonPtr<byte> dstData;

        int lineSize = dstRect.Width * FormatInformation.SizeInBytes;
        int destHeight = dstRect.Height;
        srcData = ImageData + (srcY * PitchInformation.RowPitch) + (srcX * FormatInformation.SizeInBytes);
        dstData = buffer.ImageData + (dstRect.Y * buffer.PitchInformation.RowPitch) + (dstRect.X * FormatInformation.SizeInBytes);

        // For compressed textures we need to copy using 4x4 blocks instead of pixels.
        if (FormatInformation.IsCompressed)
        {
            int srcBlockX = srcX >> 2;
            int srcBlockY = srcY >> 2;
            int destBlockX = dstRect.X >> 2;
            int destBlockY = dstRect.Y >> 2;
            int destWidth = ((srcX + dstRect.Width + 3) >> 2) - srcBlockX;
            destHeight = ((srcY + dstRect.Height + 3) >> 2) - srcBlockY;

            destWidth = destWidth.Min(((buffer.Width + 3) >> 2) - destBlockX);
            destHeight = destHeight.Min(((buffer.Height + 3) >> 2) - destBlockY);

            lineSize = destWidth * FormatInformation.SizeInBytes;
            srcData = ImageData + (srcBlockY * PitchInformation.RowPitch) + (srcBlockX * FormatInformation.SizeInBytes);
            dstData = buffer.ImageData + (destBlockY * buffer.PitchInformation.RowPitch) + (destBlockX * FormatInformation.SizeInBytes);
        }        

        for (int y = 0; y < destHeight; ++y)
        {
            srcData.Slice(0, lineSize).CopyTo(dstData.Slice(0, lineSize));

            srcData += PitchInformation.RowPitch;
            dstData += buffer.PitchInformation.RowPitch;
        }
    }

    /// <inheritdoc/>
    public IGorgonImageBuffer GetRegion(GorgonRectangle clipRegion)
    {
        ObjectDisposedException.ThrowIf(ImageData == GorgonPtr<byte>.NullPtr, this);

        GorgonRectangle finalRegion = GorgonRectangle.Intersect(clipRegion, _bounds);

        if (FormatInformation.IsCompressed)
        {
            int srcX = finalRegion.X >> 2;
            int srcY = finalRegion.Y >> 2;
            int srcWidth = ((finalRegion.Right + 3) >> 2) - srcX;
            int srcHeight = ((finalRegion.Bottom + 3) >> 2) - srcY;

            finalRegion = new GorgonRectangle(srcX << 2, srcY << 2, (srcWidth << 2).Min(Width - (srcX << 2)), (srcHeight << 2).Min(Height - (srcY <<2)));
        }

        if ((finalRegion.Width <= 0)
            || (finalRegion.Height <= 0))
        {
            return Empty;
        }

        GorgonImageBuffer result = new(finalRegion.Width, finalRegion.Height, Format);

        CopyTo(result, finalRegion);

        return result;
    }

    // Function to detach this buffer from the image memory that it points into, once the image has freed or replaced that memory.
    internal void Detach() => ImageData = GorgonPtr<byte>.NullPtr;

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
    /// <exception cref="NotSupportedException">Thrown when the <paramref name="format"/> is <see cref="BufferFormat.Unknown"/>.</exception>
    /// <exception cref="GorgonException">Thrown when the <paramref name="width"/> or the <paramref name="height"/> is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// This constructor creates a new image bufferthat users can use independently of a <see cref="IGorgonImage"/>. It can be used for updating image information periodically and copying it back into a base 
    /// image. Or it can be used for a temporary buffer for a completely separate operation. 
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This type implements <see cref="IDisposable"/>, so ensure that the <see cref="IDisposable.Dispose"/> method is called on any instance when you are finished with it. Otherwise, a temporary memory leak 
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
        PitchInformation = FormatInformation.GetPitchForFormat(width, height);

        if (PitchInformation.SlicePitch <= 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORIMG_ERR_BUFFER_TOO_SMALL);
        }

        _bounds = new GorgonRectangle(0, 0, width, height);
        _ownedBuffer = new GorgonNativeBuffer<byte>(PitchInformation.SlicePitch);
        ImageData = (GorgonPtr<byte>)_ownedBuffer;
                
        MipLevel = 0;
        ArrayIndex = 0;
        DepthSliceIndex = 0;
        Width = width;
        Height = height;
        Depth = 1;
        Format = format;
        SizeInBytes = PitchInformation.SlicePitch;
    }
}
