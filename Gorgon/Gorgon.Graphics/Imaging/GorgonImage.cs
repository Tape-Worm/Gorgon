// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: May 8, 2025 1:31:13 PM
//

using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Graphics.Imaging.Wic;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging;

/// <inheritdoc cref="IGorgonImage"/>
public partial class GorgonImage
    : IGorgonImage
{
    // The information about the image data.
    private GorgonImageInfo _imageInfo;
    // The main buffer for the image data.
    private GorgonNativeBuffer<byte>? _imageBuffer;
    // The log used for debug messaging.
    private readonly IGorgonLog _log;
    // The list of buffers that make up the image.
    private ImageBufferList _buffers = [];

    /// <inheritdoc/>
    public GorgonFormatInfo FormatInfo
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public ImageDataType ImageType => _imageInfo.ImageType;

    /// <inheritdoc/>
    public BufferFormat Format => _imageInfo.Format;

    /// <inheritdoc/>
    public int Width => _imageInfo.Width;

    /// <inheritdoc/>
    public int Height => _imageInfo.Height;

    /// <inheritdoc/>
    public int Depth => _imageInfo.Depth;

    /// <inheritdoc/>
    public int ArrayCount => _imageInfo.ArrayCount;

    /// <inheritdoc/>
    public int MipCount => _imageInfo.MipCount;

    /// <inheritdoc/>
    public long SizeInBytes
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public GorgonPtr<byte> ImageData
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public IGorgonImageBufferList Buffers => _buffers;

    /// <inheritdoc/>
    public bool HasPremultipliedAlpha => _imageInfo.IsPremultiplied;

    /// <inheritdoc/>
    public bool IsPowerOfTwo => _imageInfo.IsPowerOfTwo;

    /// <summary>
    /// Function to validate the format, width, height, depth, array count and mip count values for an image.
    /// </summary>
    /// <param name="imageType">The type of image data.</param>
    /// <param name="format">The pixel format of the image.</param>
    /// <param name="width">The width of the image, in pixels.</param>
    /// <param name="height">The height of the image, in pixels.</param>
    /// <param name="depth">The depth of the image, in depth slices.</param>
    /// <param name="arrayCount">The number of array indices.</param>
    /// <param name="mipCount">The number of mip map levels.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the <paramref name="width"/>, <paramref name="height"/>, <paramref name="depth"/>, <paramref name="arrayCount"/>, or <paramref name="mipCount"/> 
    /// values is less than 1.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <paramref name="format"/> is <see cref="BufferFormat.Unknown"/> or the <paramref name="imageType"/> is set to 
    /// <see cref="ImageDataType.Unknown"/>.</exception>
    private static void ValidateImageInfo(ImageDataType imageType, BufferFormat format, int width, int height, int depth, int arrayCount, int mipCount)
    {
        if (imageType is ImageDataType.Unknown)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_IMAGE_TYPE_UNKNOWN, imageType));
        }

        if (format is BufferFormat.Unknown)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);

        switch (imageType)
        {
            case ImageDataType.Image1D:
                ArgumentOutOfRangeException.ThrowIfLessThan(arrayCount, 1);
                break;
            case ImageDataType.ImageCube or ImageDataType.Image2D:
                ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
                ArgumentOutOfRangeException.ThrowIfLessThan(arrayCount, 1);
                break;
            case ImageDataType.Image3D:
                ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
                ArgumentOutOfRangeException.ThrowIfLessThan(depth, 1);
                break;
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(mipCount, 1);
    }

    /// <summary>
    /// Function to sanitize the image information.
    /// </summary>
    /// <param name="info">Information used to create this image.</param>
    /// <returns>The sanitized image information.</returns>
    private GorgonImageInfo SanitizeInfo(GorgonImageInfo info)
    {
        _log.Print($"Creating {info.ImageType} image with dimensions: {info.Width}x{info.Height}x{info.Depth}", LoggingLevel.Simple);
        _log.Print($"Array count: {info.ArrayCount}", LoggingLevel.Intermediate);
        _log.Print($"Mip level count: {info.MipCount}", LoggingLevel.Intermediate);
        _log.Print($"Will contain premultiplied alpha: {info.IsPremultiplied}", LoggingLevel.Intermediate);

        int mipCount = info.MipCount.Min(GorgonImageInfo.GetMaximumMipCount(info.Width, info.Height, info.Depth));
        int depth = info.Depth.Min(GorgonImageInfo.GetMaximumDepthSliceCount(info.Depth, mipCount));

        GorgonImageInfo newInfo = info with
        {
            Depth = depth,
            MipCount = mipCount
        };

        if (info != newInfo)
        {
            _log.PrintWarning("Image info updated:", LoggingLevel.Verbose);
            if (info.Depth != newInfo.Depth)
            {
                _log.Print($"New depth: {newInfo.Depth}", LoggingLevel.Verbose);
            }
            if (info.MipCount != newInfo.MipCount)
            {
                _log.Print($"New mip level count: {newInfo.MipCount}", LoggingLevel.Verbose);
            }
        }

        return newInfo;
    }

    /// <summary>
    /// Function to initialize the image data.
    /// </summary>
    /// <param name="data">Pre-existing data to use.</param>
    /// <returns>A pointer to the image buffer.</returns>    
    private GorgonPtr<byte> Initialize(GorgonPtr<byte> data)
    {
        _imageBuffer = new GorgonNativeBuffer<byte>(SizeInBytes);

        if (data != GorgonPtr<byte>.NullPtr)
        {
            data.CopyTo(_imageBuffer);
        }

        _buffers = new ImageBufferList(_imageInfo, FormatInfo, _imageBuffer);

        return _imageBuffer;
    }

    /// <summary>
    /// Function to dispose of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose both managed and native resources, <b>false</b> to only dispose unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // If we were editing, shut it down.
            if (_isEditing)
            {
                ((IGorgonImageUpdateFluent)this).EndUpdate(true);
            }

            _buffers.Dispose();
            GorgonNativeBuffer<byte>? data = Interlocked.Exchange(ref _imageBuffer, null);
            data?.Dispose();
        }

        ImageData = GorgonPtr<byte>.NullPtr;
    }

    /// <summary>
    /// Function to return the size, in bytes, of an image with the given <see cref="IGorgonImageInfo"/>.
    /// </summary>
    /// <param name="info">The <see cref="IGorgonImageInfo"/> used to describe the image.</param>
    /// <param name="pitchFlags">[Optional] Flags to influence the size of the row pitch.</param>
    /// <returns>The number of bytes for the image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the <see cref="GorgonImageInfo.Width"/>, <see cref="GorgonImageInfo.Height"/>, <see cref="GorgonImageInfo.Depth"/>, 
    /// <see cref="GorgonImageInfo.ArrayCount"/>, or <see cref="GorgonImageInfo.MipCount"/> in the <paramref name="info"/> parameter is less than 1.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonImageInfo.Format"/> value of the <paramref name="info"/> parameter is set to <see cref="BufferFormat.Unknown"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="pitchFlags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
    /// </para>
    /// </remarks>
    public static long CalculateSizeInBytes(GorgonImageInfo info, PitchFlags pitchFlags = PitchFlags.None) => GorgonImageInfo.CalculateSizeInBytes(info.ImageType,
                              info.Width,
                              info.Height,
                              info.ImageType == ImageDataType.Image3D ? info.Depth : info.ArrayCount,
                              info.Format,
                              info.MipCount,
                              pitchFlags);

    /// <inheritdoc/>
    public int GetDepthCount(int mipLevel)
    {
        if ((mipLevel < 0) || (mipLevel > MipCount))
        {
            throw new ArgumentOutOfRangeException(nameof(mipLevel), mipLevel, string.Format(Resources.GORIMG_ERR_INDEX_OUT_OF_RANGE, 0, _imageInfo.MipCount));
        }

        return ImageType == ImageDataType.Image3D ? _buffers.MipOffsetSize[mipLevel].MipDepth : 1;
    }

    /// <inheritdoc/>
    public bool CanConvertToFormat(BufferFormat format)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if (format == BufferFormat.Unknown)
        {
            return false;
        }

        if (format == _imageInfo.Format)
        {
            return true;
        }

        BufferFormat sourceFormat = _imageInfo.Format;

        // If we want to convert from B4G4R4A4 to another format, then we first have to upsample to B8R8G8A8.
        if (sourceFormat == BufferFormat.B4G4R4A4_UNorm)
        {
            sourceFormat = BufferFormat.B8G8R8A8_UNorm;
        }

        using WicUtilities wic = new();

        return wic.CanConvertFormats(sourceFormat, [format]).Count > 0;
    }

    /// <inheritdoc/>
    public IReadOnlyList<BufferFormat> CanConvertToFormats(IReadOnlyList<BufferFormat> destFormats)
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);

        if ((destFormats is null)
            || (destFormats.Count == 0))
        {
            return [];
        }

        BufferFormat sourceFormat = _imageInfo.Format;

        using WicUtilities wic = new();

        // If we're converting from B4G4R4A4, then we need to use another path.
        if (_imageInfo.Format == BufferFormat.B4G4R4A4_UNorm)
        {
            sourceFormat = BufferFormat.B8G8R8X8_UNorm;
        }

        return wic.CanConvertFormats(sourceFormat, destFormats);
    }

    /// <inheritdoc/>
    public IGorgonImage Copy()
    {
        ObjectDisposedException.ThrowIf(_imageBuffer is null, this);
        return new GorgonImage(this);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initialzies a new instance of the <see cref="GorgonImage"/> class.
    /// </summary>
    /// <param name="image">The image to copy.</param>
    /// <param name="log">[Optional] The log used for debug messages.</param>
    /// <remarks>
    /// <para>
    /// This is a copy constructor used to create a new image that is identical, but separate from the <paramref name="image"/> parameter.
    /// </para>
    /// </remarks>
    public GorgonImage(IGorgonImage image, IGorgonLog? log = null)
    {
        _log = log ?? GorgonLog.NullLog;

        _imageInfo = new GorgonImageInfo(image);

        // Create a copy of the settings so outside forces don't change it.        
        FormatInfo = new GorgonFormatInfo(_imageInfo.Format);
        SizeInBytes = CalculateSizeInBytes(_imageInfo);
        ImageData = Initialize(image.ImageData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImage" /> class.
    /// </summary>
    /// <param name="info">A <see cref="GorgonImageInfo"/> containing information used to create the image.</param>    
    /// <param name="data">[Optional] A pointer to byte data that points to a blob of existing image data.</param>
    /// <param name="log">[Optional] The log used for debug messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the <see cref="GorgonImageInfo.Width"/>, <see cref="GorgonImageInfo.Height"/>, <see cref="GorgonImageInfo.Depth"/>, 
    /// <see cref="GorgonImageInfo.ArrayCount"/>, or <see cref="GorgonImageInfo.MipCount"/> in the <paramref name="info"/> parameter is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="data"/> is too small, or too large for the image.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonImageInfo.Format"/> value of the <paramref name="info"/> parameter is set to <see cref="BufferFormat.Unknown"/>.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="data"/> parameter is omitted, then a new, empty, image will be created, otherwise the data within the span will be copied into this image. The <paramref name="data"/> passed 
    /// to this image must be the same size, in bytes as the image described by <paramref name="info"/>, otherwise an exception will be thrown. To determine how large the image size will be, in 
    /// bytes, use the static <see cref="GorgonImageInfo.CalculateSizeInBytes(ImageDataType, int, int, int, BufferFormat, int, PitchFlags)"/> or the 
    /// <see cref="CalculateSizeInBytes(GorgonImageInfo, PitchFlags)"/> method to determine the potential size of an image prior to creation.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The <paramref name="data"/>, if not omitted, is <b>copied</b>, not wrapped. This ensures that the lifetime of the data passed in remains the responsibility of the caller and does not affect the 
    /// image object integrity.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonImageInfo.CalculateSizeInBytes(ImageDataType, int, int, int, BufferFormat, int, PitchFlags)"/>
    /// <seealso cref="CalculateSizeInBytes(GorgonImageInfo, PitchFlags)"/>
    public GorgonImage(GorgonImageInfo info, GorgonPtr<byte>? data = null, IGorgonLog? log = null)
    {
        _log = log ?? GorgonLog.NullLog;
        data ??= GorgonPtr<byte>.NullPtr;

        ValidateImageInfo(info.ImageType, info.Format, info.Width, info.Height, info.Depth, info.ArrayCount, info.MipCount);

        FormatInfo = new GorgonFormatInfo(info.Format);

        // Create a copy of the settings so outside forces don't change it.
        _imageInfo = SanitizeInfo(info);
        SizeInBytes = CalculateSizeInBytes(_imageInfo);

        // Validate the image size.
        if ((data != GorgonPtr<byte>.NullPtr) && (SizeInBytes != data.Value.SizeInBytes))
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_IMAGE_SIZE_MISMATCH, SizeInBytes, data.Value.SizeInBytes), nameof(data));
        }

        ImageData = Initialize(data.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImage" /> class.
    /// </summary>
    /// <param name="info">A <see cref="GorgonImageInfo"/> containing information used to create the image.</param>    
    /// <param name="data">A read only span for a blob of existing image data.</param>
    /// <param name="log">[Optional] The log used for debug messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the <see cref="GorgonImageInfo.Width"/>, <see cref="GorgonImageInfo.Height"/>, <see cref="GorgonImageInfo.Depth"/>, 
    /// <see cref="GorgonImageInfo.ArrayCount"/>, or <see cref="GorgonImageInfo.MipCount"/> in the <paramref name="info"/> parameter is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="data"/> is too small, or too large for the image.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonImageInfo.Format"/> value of the <paramref name="info"/> parameter is set to <see cref="BufferFormat.Unknown"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="data"/> passed to this image must be the same size, in bytes as the image described by <paramref name="info"/>, otherwise an exception will be thrown. To determine how large the 
    /// image size will be, in bytes, use the static <see cref="GorgonImageInfo.CalculateSizeInBytes(ImageDataType, int, int, int, BufferFormat, int, PitchFlags)"/> or the 
    /// <see cref="CalculateSizeInBytes(GorgonImageInfo, PitchFlags)"/> method to determine the potential size of an image prior to creation.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The <paramref name="data"/>, is <b>copied</b>, not wrapped. This ensures that the lifetime of the data passed in remains the responsibility of the caller and does not affect the image object 
    /// integrity.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonImageInfo.CalculateSizeInBytes(ImageDataType, int, int, int, BufferFormat, int, PitchFlags)"/>
    /// <seealso cref="CalculateSizeInBytes(GorgonImageInfo, PitchFlags)"/>
    public GorgonImage(GorgonImageInfo info, ReadOnlySpan<byte> data, IGorgonLog? log = null)
    {
        _log = log ?? GorgonLog.NullLog;

        ValidateImageInfo(info.ImageType, info.Format, info.Width, info.Height, info.Depth, info.ArrayCount, info.MipCount);

        FormatInfo = new GorgonFormatInfo(info.Format);

        // Create a copy of the settings so outside forces don't change it.
        _imageInfo = SanitizeInfo(info);
        SizeInBytes = CalculateSizeInBytes(_imageInfo);

        // Validate the image size.
        if (SizeInBytes != data.Length)
        {
            throw new ArgumentException(string.Format(Resources.GORIMG_ERR_IMAGE_SIZE_MISMATCH, SizeInBytes, data.Length), nameof(data));
        }

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                ImageData = Initialize(new GorgonPtr<byte>(dataPtr, data.Length));
            }
        }
    }
}
