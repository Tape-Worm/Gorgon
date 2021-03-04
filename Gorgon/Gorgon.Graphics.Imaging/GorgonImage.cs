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
// Created: June 20, 2016 8:37:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;
using BCnDecode = BCnEncoder.Decoder;

namespace Gorgon.Graphics.Imaging
{
    /// <summary>
    /// Holds raw data that is used to represent an image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="GorgonImage"/> object will hold a blob of data in native (unmanaged) memory and represent that data as a series of pixels to be displayed, or manipulated. This image type is capable 
    /// of representing standard 2D images, but can also represent 1D and 3D images. And, depending on the type of image, there is also support for mip map levels, and arrayed images.
    /// </para>
    /// <para>
    /// Images can access their data directly through a <see cref="GorgonNativeBuffer{T}"/> interface that allows safe access to raw, unmanaged memory where the image data is stored. In cases where images have 
    /// multiple parts like depth slices for a 3D image, or an array for 2D images, this object will provide access through a series of buffers that will point to the individual locations for depth slices, 
    /// array indices, and mip map levels. These buffers will also provide their own <see cref="GorgonNativeBuffer{T}"/> that will allow safe and direct access to the native memory where the buffer is located.
    /// </para>
    /// <para>
    /// Because this object stored data in native memory instead of on the heaps provided by .NET, this object should be disposed by calling its <see cref="IDisposable.Dispose"/> method when it is no 
    /// longer required. Failure to do so might cause a memory leak until the garbage collector can deal with it.
    /// </para>
    /// </remarks>
    public partial class GorgonImage
        : IGorgonImage
    {
        #region Variables.
        // Information used to create the image.
        private GorgonImageInfo _imageInfo;
        // The list of image buffers.
        private ImageBufferList _imageBuffers;
        // The backing image data store.
        private GorgonNativeBuffer<byte> _imageData;
        // The pointer to the image buffer.
        private GorgonPtr<byte> _imagePtr;
        // Flag to indicate that the image is in an editing state.
        private bool _isEditing;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the pointer to the beginning of the internal buffer.
        /// </summary>
        public GorgonPtr<byte> ImageData => _imagePtr;

        /// <summary>
        /// Property to return information about the pixel format for this image.
        /// </summary>
        public GorgonFormatInfo FormatInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of bytes, in total, that this image occupies.
        /// </summary>
        public int SizeInBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the list of image buffers for this image.
        /// </summary>
        public IGorgonImageBufferList Buffers => _imageBuffers;

        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        public ImageType ImageType => _imageInfo.ImageType;

        /// <summary>
        /// Property to return the width of an image, in pixels.
        /// </summary>
        public int Width => _imageInfo.Width;

        /// <summary>
        /// Property to return the height of an image, in pixels.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This applies to 2D and 3D images only.  This parameter will be set to a value of 1 for a 1D image.
        /// </para>
        /// </remarks>
        public int Height => _imageInfo.Height;

        /// <summary>
        /// Property to return the depth of an image, in pixels.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This applies to 3D images only.  This parameter will be set to a value of 1 for a 1D or 2D image.
        /// </para>
        /// </remarks>
        public int Depth => _imageInfo.Depth;

        /// <summary>
        /// Property to return the pixel format for an image.
        /// </summary>
        public BufferFormat Format => _imageInfo.Format;

        /// <summary>
        /// Property to return whether the image data is using premultiplied alpha.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Premultiplied alpha is used to display correct alpha blending. This flag indicates that the data in the image has already been transformed to use premultiplied alpha.
        /// </para>
        /// <para>
        /// For more information see: <a href="https://blogs.msdn.microsoft.com/shawnhar/2009/11/06/premultiplied-alpha/">Shawn Hargreaves Blog</a>
        /// </para>
        /// </remarks>
        public bool HasPreMultipliedAlpha => _imageInfo.HasPreMultipliedAlpha;

        /// <summary>
        /// Property to return the number of mip map levels in the image.
        /// </summary>
        public int MipCount => _imageInfo.MipCount;

        /// <summary>
        /// Property to return whether the size of the texture is a power of 2 or not.
        /// </summary>
        public bool IsPowerOfTwo => _imageInfo.IsPowerOfTwo;

        /// <summary>
        /// Property to return the total number of images there are in an image array.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
        /// </para>
        /// </remarks>
        public int ArrayCount => _imageInfo.ArrayCount;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the image data.
        /// </summary>
        /// <param name="data">Pre-existing data to use.</param>
        private void Initialize(ReadOnlySpan<byte> data)
        {
            _imagePtr = _imageData = new GorgonNativeBuffer<byte>(SizeInBytes);

            if (!data.IsEmpty)
            {
                data.CopyTo(_imagePtr);
            }            

            _imageBuffers = new ImageBufferList(this);
            _imageBuffers.CreateBuffers(in _imagePtr);
        }

        /// <summary>
        /// Function to sanitize the image information.
        /// </summary>
        /// <param name="info">Information used to create this image.</param>
        private void SanitizeInfo(IGorgonImageInfo info)
        {
            var newInfo = new GorgonImageInfo(info)
            {
                Height = info.Height.Max(1),
                Depth = info.Depth.Max(1)
            };

            // Validate the array size.
            newInfo.ArrayCount = newInfo.ImageType == ImageType.Image3D ? 1 : newInfo.ArrayCount.Max(1);

            // Validate depth count.
            newInfo.Depth = newInfo.ImageType != ImageType.Image3D ? 1 : newInfo.Depth.Max(1);

            // Limit to the lesser value.
            int maxMipCount = CalculateMaxMipCount(info);
            newInfo.MipCount = newInfo.MipCount.Min(maxMipCount);

            if (newInfo.MipCount <= 0)
            {
                newInfo.MipCount = maxMipCount;
            }

            if (newInfo.ImageType == ImageType.ImageCube)
            {
                // If we're an image cube, and we don't have an array count that's a multiple of 6, then up size until we do.
                while ((newInfo.ArrayCount % 6) != 0)
                {
                    newInfo.ArrayCount++;
                }
            }

            _imageInfo = newInfo;
        }

        /// <summary>
        /// Function to return the size of an image in bytes.
        /// </summary>
        /// <param name="imageType">The type of image.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="arrayCountOrDepth">The number of array indices for <see cref="ImageType.Image1D"/> or <see cref="ImageType.Image2D"/> images, or the number of depth slices for a <see cref="ImageType.Image3D"/>.</param>
        /// <param name="format">Format of the image.</param>
        /// <param name="mipCount">[Optional] Number of mip-map levels in the image.</param>
        /// <param name="pitchFlags">[Optional] Flags used to influence the row pitch of the image.</param>
        /// <returns>The number of bytes for the image.</returns>
        /// <exception cref="GorgonException">Thrown when the <paramref name="format"/> is not supported.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="pitchFlags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
        /// </para>
        /// </remarks>
        public static int CalculateSizeInBytes(ImageType imageType, int width, int height, int arrayCountOrDepth, BufferFormat format, int mipCount = 1, PitchFlags pitchFlags = PitchFlags.None)
        {
            if (format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
            }

            width = 1.Max(width);
            height = 1.Max(height);
            arrayCountOrDepth = 1.Max(arrayCountOrDepth);
            mipCount = 1.Max(mipCount);
            var formatInfo = new GorgonFormatInfo(format);
            int result = 0;

            if (formatInfo.SizeInBytes == 0)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
            }

            int arrayCount = imageType == ImageType.Image3D ? 1 : arrayCountOrDepth;
            int depthCount = imageType == ImageType.Image3D ? arrayCountOrDepth : 1;

            for (int array = 0; array < arrayCount; ++array)
            {
                int mipWidth = width;
                int mipHeight = height;

                for (int mip = 0; mip < mipCount; mip++)
                {
                    GorgonPitchLayout pitchInfo = formatInfo.GetPitchForFormat(mipWidth, mipHeight, pitchFlags);
                    result += pitchInfo.SlicePitch * depthCount;

                    if (mipWidth > 1)
                    {
                        mipWidth >>= 1;
                    }

                    if (mipHeight > 1)
                    {
                        mipHeight >>= 1;
                    }

                    if (depthCount > 1)
                    {
                        depthCount >>= 1;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Function to return the size, in bytes, of an image with the given <see cref="IGorgonImageInfo"/>.
        /// </summary>
        /// <param name="info">The <see cref="IGorgonImageInfo"/> used to describe the image.</param>
        /// <param name="pitchFlags">[Optional] Flags to influence the size of the row pitch.</param>
        /// <returns>The number of bytes for the image.</returns>
        /// <exception cref="ArgumentException">Thrown when the format value of the <paramref name="info"/> parameter is not supported.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="pitchFlags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
        /// </para>
        /// </remarks>
        public static int CalculateSizeInBytes(IGorgonImageInfo info, PitchFlags pitchFlags = PitchFlags.None) => info is null
                ? 0
                : CalculateSizeInBytes(info.ImageType,
                                  info.Width,
                                  info.Height,
                                  info.ImageType == ImageType.Image3D ? info.Depth : info.ArrayCount,
                                  info.Format,
                                  info.MipCount,
                                  pitchFlags);

        /// <summary>
        /// Function to return the maximum number of mip levels supported given the specified settings.
        /// </summary>
        /// <param name="info">The <see cref="IGorgonImageInfo"/> used to describe the image.</param>
        /// <returns>The number of possible mip-map levels in the image.</returns>
        public static int CalculateMaxMipCount(IGorgonImageInfo info) => info is null ? 0 : CalculateMaxMipCount(info.Width, info.Height, info.Depth);

        /// <summary>
        /// Function to return the maximum number of mip levels supported in for an image.
        /// </summary>
        /// <param name="width">Width of the proposed image.</param>
        /// <param name="height">Height of the proposed image.</param>
        /// <param name="depth">Depth of the proposed image.</param>
        /// <returns>The number of possible mip-map levels in the image.</returns>
        public static int CalculateMaxMipCount(int width, int height, int depth)
        {
            int result = 1;
            width = 1.Max(width);
            height = 1.Max(height);
            depth = 1.Max(depth);

            while ((width > 1) || (height > 1) || (depth > 1))
            {
                if (width > 1)
                {
                    width >>= 1;
                }
                if (height > 1)
                {
                    height >>= 1;
                }
                if (depth > 1)
                {
                    depth >>= 1;
                }

                result++;
            }

            return result;
        }

        /// <summary>
        /// Function to return the number of depth slices for a 3D image with the given number of mip maps.
        /// </summary>
        /// <param name="maximumSlices">The maximum desired number of depth slices to return.</param>
        /// <param name="mipCount">The number of mip maps to use.</param>
        /// <returns>The actual number of depth slices.</returns>
        public static int CalculateDepthSliceCount(int maximumSlices, int mipCount)
        {
            if (mipCount < 2)
            {
                return maximumSlices;
            }

            int bufferCount = 0;
            int depth = maximumSlices;

            for (int i = 0; i < mipCount; i++)
            {
                bufferCount += depth;

                if (depth > 1)
                {
                    depth >>= 1;
                }
            }

            return bufferCount;
        }

        /// <summary>
        /// Function to return the number of depth slices available for a given mip map level.
        /// </summary>
        /// <param name="mipLevel">The mip map level to look up.</param>
        /// <returns>The number of depth slices for the given mip map level.</returns>
        /// <remarks>
        /// <para>
        /// For 1D and 2D images, the mip level will always return 1.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds the number of mip maps for the image or is less than 0.</exception>
        public int GetDepthCount(int mipLevel) => (mipLevel < 0) || (mipLevel >= _imageInfo.MipCount)
                ? throw new ArgumentOutOfRangeException(nameof(mipLevel), mipLevel, string.Format(Resources.GORIMG_ERR_INDEX_OUT_OF_RANGE, 0, _imageInfo.MipCount))
                : (_imageInfo.Depth <= 1 ? 1 : _imageBuffers.MipOffsetSize[mipLevel].MipDepth);

        /// <summary>
        /// Function to determine if the pixel format for this image can be converted to another pixel format.
        /// </summary>
        /// <param name="format">The pixel format to convert to.</param>
        /// <returns><b>true</b> if the the current pixel format and the requested pixel format can be converted, <b>false</b> if not.</returns>
        public bool CanConvertToFormat(BufferFormat format)
        {
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

            using var wic = new WicUtilities();
            return wic.CanConvertFormats(sourceFormat,
                                         new[]
                                         {
                                                 format
                                         }).Count > 0;
        }

        /// <summary>
        /// Function to determine if the source format can convert to any of the formats in the destination list.
        /// </summary>
        /// <param name="destFormats">List of destination formats to compare.</param>
        /// <returns>A list of formats that the source format can be converted into, or an empty array if no conversion is possible.</returns>
        public IReadOnlyList<BufferFormat> CanConvertToFormats(IReadOnlyList<BufferFormat> destFormats)
        {
            if ((destFormats is null)
                || (destFormats.Count == 0))
            {
                return Array.Empty<BufferFormat>();
            }

            // If we're converting from B4G4R4A4, then we need to use another path.
            if (_imageInfo.Format == BufferFormat.B4G4R4A4_UNorm)
            {
                using var wic = new WicUtilities();
                return wic.CanConvertFormats(BufferFormat.B8G8R8A8_UNorm, destFormats);
            }

            using (var wic = new WicUtilities())
            {
                return wic.CanConvertFormats(_imageInfo.Format, destFormats);
            }
        }

        /// <summary>
        /// Function to copy an image into this image.
        /// </summary>
        /// <param name="image">The image that will copied into this image.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will clone the <paramref name="image"/> into this image. All information in the current image will be discarded and replaced with a duplicate of the data present in the source 
        /// <paramref name="image"/>. If copying parts of an image into a new image is required, then see the <see cref="IGorgonImageBuffer"/>.<see cref="IGorgonImageBuffer.CopyTo"/> 
        /// method.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonImageBuffer"/>
        public void Copy(IGorgonImage image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var info = new GorgonImageInfo(image);
            var formatInfo = new GorgonFormatInfo(info.Format);
            var data = new GorgonNativeBuffer<byte>(image.SizeInBytes);
            GorgonPtr<byte> ptr = data.Pointer;

            image.ImageData.CopyTo(ptr);

            var buffers = new ImageBufferList(info);
            buffers.CreateBuffers(in ptr);

            _imageData.Dispose();
            _imageData = data;
            _imagePtr = ptr;
            _imageBuffers = buffers;
            SizeInBytes = data.SizeInBytes;
            _imageInfo = info;
            FormatInfo = formatInfo;
        }

        /// <summary>
        /// Function to make a clone of this image.
        /// </summary>
        /// <returns>A new <see cref="IGorgonImage"/> that contains an identical copy of this image and its data.</returns>
        public IGorgonImage Clone() => new GorgonImage(this);

        /// <summary>Function to decompress an image containing block compressed data.</summary>
        /// <param name="useBC1Alpha">[Optional] <b>true</b> if the image is compressed with BC1 (DXT1) compression, and the contains alpha, <b>false</b> if no alpha is in the image.</param>
        /// <returns>The image with decompressed image data.</returns>
        /// <exception cref="NotSupportedException">Thrown if the image data is not block compressed, or contains typeless data.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the image is already being edited.</exception>
        /// <remarks>
        /// <para>
        /// This method will decompress an image containing image data that has been compressed with one of the standard block compression formats. The <see cref="BufferFormat"/> enum contains 7 levels of 
        /// block compression named BC1 - BC7. The features of each compression level are documented at <a target="_blank" href="https://docs.microsoft.com/en-us/windows/win32/direct3d11/texture-block-compression-in-direct3d-11"/>.
        /// </para>
        /// <para>
        /// The decompressed image data will result in 32 bit RGBA data in the format of <see cref="BufferFormat.R8G8B8A8_UNorm"/>.
        /// </para>
        /// <para>
        /// Block compression is, by nature, a lossy compression format. Thus some fidelity will be lost when the image data is compressed, it is recommended that images be compressed as a last stage in 
        /// processing. Because block compression lays the image data out differently than standard image data, the functionality provided for modifying an image (e.g. 
        /// <see cref="IGorgonImageUpdateFluent.Resize"/>) will not work and will throw an exception if used on block compressed data, this method will allow users to make alterations with the image 
        /// modification functionality.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Because block compressed data is lossy, it is not recommended that images be decompressed and compressed over and over as it will degrade the image fidelity severely. 
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// If the image data was compressed with BC1 compression, optional 1-bit alpha channel data may be stored with the image data. The developer must specify whether to use the alpha data or not via 
        /// the <paramref name="useBC1Alpha"/> parameter.
        /// </para>
        /// <para>
        /// This method returns the <see cref="IGorgonImageUpdateFluent"/> interface to allow users to modify the image modification after decompression. This means that this method calls 
        /// <see cref="BeginUpdate"/> implicitly after execution.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonImageUpdateFluent.Compress"/>
        /// <seealso cref="BeginUpdate"/>
        public IGorgonImageUpdateFluent Decompress(bool useBC1Alpha)
        {
            if ((_isEditing) || (_wic is not null))
            {
                throw new InvalidOperationException(Resources.GORIMG_ERR_ALREADY_EDITING);
            }

            if ((!FormatInfo.IsCompressed) || (FormatInfo.IsTypeless))
            {
                throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
            }

            // Performs the actual decompression.
            void DoDecompress()
            {
                var decoder = new BCnDecode.BcDecoder
                {
                    LuminanceAsRed = true
                };

                var info = new GorgonImageInfo(ImageType, BufferFormat.R8G8B8A8_UNorm)
                {
                    MipCount = MipCount,
                    ArrayCount = ArrayCount,
                    Depth = Depth,
                    HasPreMultipliedAlpha = HasPreMultipliedAlpha,
                    Width = Width,
                    Height = Height
                };

                ImageBufferList bufferList;
                var decoded = new GorgonNativeBuffer<byte>(CalculateSizeInBytes(info));

                try
                {
                    bufferList = new ImageBufferList(info);
                    bufferList.CreateBuffers(decoded.Pointer);

                    for (int arrayIndex = 0; arrayIndex < ArrayCount; ++arrayIndex)
                    {
                        for (int mip = 0; mip < MipCount; ++mip)
                        {
                            for (int depthSlice = 0; depthSlice < GetDepthCount(mip); ++depthSlice)
                            {
                                int depthArrayIndex = ImageType != ImageType.Image3D ? arrayIndex : depthSlice;
                                IGorgonImageBuffer buffer = Buffers[mip, depthArrayIndex];

                                using GorgonNativeBuffer<byte> decompressedData = decoder.Decode(buffer.Data, buffer.Width, buffer.Height, useBC1Alpha, Format);
                                // Replace the data in the source image buffer with our newly compressed data.
                                decompressedData.CopyTo((Span<byte>)bufferList[mip, depthArrayIndex].Data);
                            }
                        }
                    }
                }
                catch
                {
                    decoded?.Dispose();
                    throw;
                }

                _imageData?.Dispose();
                _imagePtr = _imageData = null;

                SanitizeInfo(info);
                FormatInfo = new GorgonFormatInfo(info.Format);
                SizeInBytes = decoded.SizeInBytes;
                _imagePtr = _imageData = decoded;
                _imageBuffers = bufferList;
            }

            _isEditing = true;
            _wic = new WicUtilities();

            _commands.Enqueue(DoDecompress);

            return this;
        }

        /// <summary>
        /// Function to begin updating the image.
        /// </summary>
        /// <returns>The fluent interface for editing the image.</returns>
        /// <exception cref="NotSupportedException">Thrown if the image cannot be updated because of its format.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the image is already being edited.</exception>
        /// <remarks>
        /// <para>
        /// This begins an update to the current image instance by returning a fluent interface (<see cref="IGorgonImageUpdateFluent"/>) that will provide operations that can be performed on the image 
        /// in place. 
        /// </para>
        /// <para>
        /// OTHER STUFF TO BE DONE HERE.
        /// </para>
        /// <para>
        /// If the image data is compressed using block compression, this method will throw an exception. Check the <see cref="FormatInfo"/> property to determine if the image has block compressed image 
        /// data.
        /// </para>
        /// <para>
        /// Once done updating the image, call the <see cref="IGorgonImageUpdateFinalize.EndUpdate"/> method to apply or cancel the changes to the image data. This method must be called if <c>BeginUpdate</c>
        /// is to be called again. Calling <c>BeginUpdate</c> more than once without calling <see cref="IGorgonImageUpdateFinalize.EndUpdate"/> will throw an exception.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonImageUpdateFinalize.EndUpdate"/>
        public IGorgonImageUpdateFluent BeginUpdate() 
        {
            if ((FormatInfo.IsCompressed) || (FormatInfo.IsTypeless))
            {
                throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, Format));
            }

            if ((_isEditing) || (_wic is not null))
            {
                throw new InvalidOperationException(Resources.GORIMG_ERR_ALREADY_EDITING);
            }

            _isEditing = true;
            _wic = new WicUtilities();

            return this;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // If we were editing, shut it down.
            if (_isEditing)
            {
                ((IGorgonImageUpdateFluent)this).EndUpdate(true);
            }

            GorgonNativeBuffer<byte> imageData = Interlocked.Exchange(ref _imageData, null);
            _imagePtr = GorgonPtr<byte>.NullPtr;
            imageData?.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>Initializes a new instance of the <see cref="GorgonImage" /> class.</summary>
        /// <param name="source">The source image to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a copy constructor for an image object. The new object will contain a duplicate of all data provided by the <paramref name="source"/>.
        /// </para>
        /// </remarks>
        public GorgonImage(IGorgonImage source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _imageInfo = new GorgonImageInfo(source);
            FormatInfo = new GorgonFormatInfo(_imageInfo.Format);
            SizeInBytes = CalculateSizeInBytes(_imageInfo);
            Initialize(source.ImageData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImage" /> class.
        /// </summary>
        /// <param name="info">A <see cref="IGorgonImageInfo"/> containing information used to create the image.</param>
        /// <param name="data">[Optional] A read only span of byte data that points to a blob of existing image data.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="data"/> parameter is omitted, then a new, empty, image will be created, otherwise the data within the span will be copied into this image. The <paramref name="data"/> passed 
        /// to this image must be large enough to accomodate the size of the image described by <paramref name="info"/>, otherwise an exception will be thrown. To determine how large the image size will be, in 
        /// bytes, use the static <see cref="CalculateSizeInBytes(IGorgonImageInfo,PitchFlags)"/> method to determine the potential size of an image prior to creation.
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
        public GorgonImage(IGorgonImageInfo info, ReadOnlySpan<byte> data = default)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (info.Width < 1)
            {
                throw new ArgumentException(Resources.GORIMG_ERR_IMAGE_WIDTH_TOO_SMALL, nameof(info));
            }

            if ((info.Height < 1) &&
                ((info.ImageType == ImageType.ImageCube)
                || (info.ImageType == ImageType.Image2D)
                || (info.ImageType == ImageType.Image3D)))
            {
                throw new ArgumentException(Resources.GORIMG_ERR_IMAGE_HEIGHT_TOO_SMALL, nameof(info));
            }

            if ((info.Depth < 1) && (info.ImageType == ImageType.Image3D))
            {
                throw new ArgumentException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(info));
            }

            FormatInfo = new GorgonFormatInfo(info.Format);

            // Create a copy of the settings so outside forces don't change it.
            SanitizeInfo(info);
            SizeInBytes = CalculateSizeInBytes(_imageInfo);

            // Validate the image size.
            if ((!data.IsEmpty) && (SizeInBytes > data.Length))
            {
                throw new ArgumentException(string.Format(Resources.GORIMG_ERR_IMAGE_SIZE_MISMATCH, SizeInBytes, data.Length), nameof(data));
            }

            Initialize(data);
        }
        #endregion
    }
}
