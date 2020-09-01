#region MIT.
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
// Created: Tuesday, February 12, 2013 8:49:57 PM
// 

// This code was adapted from:
// SharpDX by Alexandre Mutel (http://sharpdx.org)
// DirectXTex by Chuck Walburn (http://directxtex.codeplex.com)

#region SharpDX/DirectXTex licenses
// Copyright (c) 2010-2016 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTex http://directxtex.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.
#endregion
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging.Codecs
{
    /// <summary>
    /// TGA specific conversion flags.
    /// </summary>
    [Flags]
    internal enum TGAConversionFlags
    {
        /// <summary>
        /// No conversion.
        /// </summary>
        None = 0,
        /// <summary>
        /// Expand to a wider bit format.
        /// </summary>
        Expand = 0x1,
        /// <summary>
        /// Scanlines are right to left.
        /// </summary>
        InvertX = 0x2,
        /// <summary>
        /// Scanelines are bottom to top.
        /// </summary>
        InvertY = 0x4,
        /// <summary>
        /// Run length encoded.
        /// </summary>
        RLE = 0x8,
        /// <summary>
        /// Convert alpha to opaque if all scanlines have 0 alpha.
        /// </summary>
        SetOpaqueAlpha = 0x10,
        /// <summary>
        /// Swizzle BGR to RGB/RGB to BGR
        /// </summary>
        Swizzle = 0x10000,
        /// <summary>
        /// 24 bit format.
        /// </summary>
        RGB888 = 0x20000
    }

    /// <summary>
    /// A codec to handle reading/writing Truevision TGA files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This codec will read RLE compressed and uncompressed files, and write compressed files using the Truevision Targa (TGA) format.
    /// </para>
    /// <para>
    /// Most 16/24/32 bit TGA files will be readable using this codec, however the following limitations may keep the file from being decoded by this codec:
    /// <list type="bullet">
    ///     <item>
    ///         <description>No color map support.</description>
    ///     </item>
    ///     <item>
    ///         <description>Interleaved files are not supported.</description>
    ///     </item>
    ///     <item>
    ///         <description>Supports the following formats: 8 bit grayscale, 16, 24 and 32 bits per pixel images.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class GorgonCodecTga
        : GorgonImageCodec<IGorgonImageCodecEncodingOptions, GorgonTgaDecodingOptions>
    {
        #region Variables.
        // List of supported image formats.
        private readonly BufferFormat[] _supportedFormats =
        {
			// 8 bit grayscale.
			BufferFormat.R8_UNorm,
            BufferFormat.A8_UNorm,
			// 16 bit (only supports 5 bit color and 1 bit alpha).
			BufferFormat.B5G5R5A1_UNorm,
			// 24/32 bit.
			BufferFormat.R8G8B8A8_UNorm,
            BufferFormat.B8G8R8A8_UNorm,
            BufferFormat.B8G8R8X8_UNorm
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the pixel formats supported by the codec.
        /// </summary>
        public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _supportedFormats;

        /// <summary>
        /// Property to return whether the image codec supports block compression.
        /// </summary>
        public override bool SupportsBlockCompression => false;

        /// <summary>
        /// Property to return the friendly description of the format.
        /// </summary>
        public override string CodecDescription => Resources.GORIMG_DESC_TGA_CODEC;

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. PNG).
        /// </summary>
        public override string Codec => "TGA";

        /// <summary>
        /// Property to return whether the codec supports decoding/encoding multiple frames or not.
        /// </summary>
        public override bool SupportsMultipleFrames => false;

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
        public override bool SupportsMipMaps => false;

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        public override bool SupportsDepth => false;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to read in the TGA header from a stream.
        /// </summary>
        /// <param name="reader">The reader used to read the stream containing the data.</param>
        /// <param name="conversionFlags">Flags for conversion.</param>
        /// <returns>New image settings.</returns>
        private static IGorgonImageInfo ReadHeader(GorgonBinaryReader reader, out TGAConversionFlags conversionFlags)
        {
            conversionFlags = TGAConversionFlags.None;

            // Get the header for the file.
            TgaHeader header = reader.ReadValue<TgaHeader>();

            if ((header.ColorMapType != 0) || (header.ColorMapLength != 0)
                || (header.Width <= 0) || (header.Height <= 0)
                || ((header.Descriptor & TgaDescriptor.Interleaved2Way) == TgaDescriptor.Interleaved2Way)
                || ((header.Descriptor & TgaDescriptor.Interleaved4Way) == TgaDescriptor.Interleaved4Way))
            {
                throw new NotSupportedException(Resources.GORIMG_ERR_TGA_TYPE_NOT_SUPPORTED);
            }

            BufferFormat pixelFormat = BufferFormat.Unknown;

            switch (header.ImageType)
            {
                case TgaImageType.TrueColor:
                case TgaImageType.TrueColorRLE:
                    switch (header.BPP)
                    {
                        case 16:
                            pixelFormat = BufferFormat.B5G5R5A1_UNorm;
                            break;
                        case 24:
                        case 32:
                            pixelFormat = BufferFormat.R8G8B8A8_UNorm;
                            if (header.BPP == 24)
                            {
                                conversionFlags |= TGAConversionFlags.Expand;
                            }
                            break;
                    }

                    if (header.ImageType == TgaImageType.TrueColorRLE)
                    {
                        conversionFlags |= TGAConversionFlags.RLE;
                    }
                    break;
                case TgaImageType.BlackAndWhite:
                case TgaImageType.BlackAndWhiteRLE:
                    if (header.BPP == 8)
                    {
                        pixelFormat = BufferFormat.R8_UNorm;
                    }
                    else
                    {
                        throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, header.ImageType));
                    }

                    if (header.ImageType == TgaImageType.BlackAndWhiteRLE)
                    {
                        conversionFlags |= TGAConversionFlags.RLE;
                    }
                    break;
                default:
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, header.ImageType));
            }

            var settings = new GorgonImageInfo(ImageType.Image2D, pixelFormat)
            {
                MipCount = 1,
                ArrayCount = 1,
                Width = header.Width,
                Height = header.Height
            };

            if ((header.Descriptor & TgaDescriptor.InvertX) == TgaDescriptor.InvertX)
            {
                conversionFlags |= TGAConversionFlags.InvertX;
            }

            if ((header.Descriptor & TgaDescriptor.InvertY) == TgaDescriptor.InvertY)
            {
                conversionFlags |= TGAConversionFlags.InvertY;
            }

            if (header.IDLength <= 0)
            {
                return settings;
            }

            // Skip these bytes.
            for (int i = 0; i < header.IDLength; i++)
            {
                reader.ReadByte();
            }

            return settings;
        }

        /// <summary>
        /// Function to write out the DDS header to the stream.
        /// </summary>
        /// <param name="settings">Meta data for the image header.</param>
        /// <param name="conversionFlags">Flags required for image conversion.</param>
        /// <returns>A TGA header value, populated with the correct settings.</returns>
        private TgaHeader GetHeader(IGorgonImageInfo settings, out TGAConversionFlags conversionFlags)
        {
            TgaHeader header = default;

            conversionFlags = TGAConversionFlags.None;

            if ((settings.Width > 0xFFFF)
                || (settings.Height > 0xFFFF))
            {
                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
            }

            header.Width = (ushort)(settings.Width);
            header.Height = (ushort)(settings.Height);

            switch (settings.Format)
            {
                case BufferFormat.R8G8B8A8_UNorm:
                case BufferFormat.R8G8B8A8_UNorm_SRgb:
                    header.ImageType = TgaImageType.TrueColor;
                    header.BPP = 32;
                    header.Descriptor = TgaDescriptor.InvertY | TgaDescriptor.RGB888A8;
                    conversionFlags |= TGAConversionFlags.Swizzle;
                    break;
                case BufferFormat.B8G8R8A8_UNorm:
                case BufferFormat.B8G8R8A8_UNorm_SRgb:
                    header.ImageType = TgaImageType.TrueColor;
                    header.BPP = 32;
                    header.Descriptor = TgaDescriptor.InvertY | TgaDescriptor.RGB888A8;
                    break;
                case BufferFormat.B8G8R8X8_UNorm:
                case BufferFormat.B8G8R8X8_UNorm_SRgb:
                    header.ImageType = TgaImageType.TrueColor;
                    header.BPP = 24;
                    header.Descriptor = TgaDescriptor.InvertY;
                    conversionFlags |= TGAConversionFlags.RGB888;
                    break;

                case BufferFormat.R8_UNorm:
                case BufferFormat.A8_UNorm:
                    header.ImageType = TgaImageType.BlackAndWhite;
                    header.BPP = 8;
                    header.Descriptor = TgaDescriptor.InvertY;
                    break;

                case BufferFormat.B5G5R5A1_UNorm:
                    header.ImageType = TgaImageType.TrueColor;
                    header.BPP = 16;
                    header.Descriptor = TgaDescriptor.InvertY | TgaDescriptor.RGB555A1;
                    break;
                default:
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, settings.Format));
            }

            // Persist to stream.
            return header;
        }

        /// <summary>
        /// Function to decode an uncompressed run of pixel data.
        /// </summary>
        /// <param name="reader">The reader used to read the data in the stream.</param>
        /// <param name="dest">The destination buffer pointer.</param>
        /// <param name="x">The current horizontal position in the scanline.</param>
        /// <param name="runLength">The size of the run, in pixels.</param>
        /// <param name="width">The total width of the run.</param>
        /// <param name="expand"><b>true</b> to expand a 24bpp scanline to 32bpp, or <b>false</b> if no expansion is needed.</param>
        /// <param name="flipHorizontal"><b>true</b> to decode the pixels from right to left, or <b>false</b> to decode from left to right.</param>
        /// <param name="format">The pixel format.</param>
        /// <returns><b>true</b> if the run contains entirely transparent pixels, or <b>false</b> if not.</returns>
        private unsafe bool DecodeRleEncodedRun(GorgonBinaryReader reader, ref byte* dest, ref int x, int runLength, int width, bool expand, bool flipHorizontal, BufferFormat format)
        {
            bool result = true;

            switch (format)
            {
                case BufferFormat.R8_UNorm:
                    for (; runLength > 0; --runLength, ++x)
                    {
                        if (x >= width)
                        {
                            throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                        }

                        if (!flipHorizontal)
                        {
                            *(dest++) = reader.ReadByte();
                        }
                        else
                        {
                            *(dest--) = reader.ReadByte();
                        }
                    }
                    break;
                case BufferFormat.B5G5R5A1_UNorm:
                    {
                        ushort* destPtr = (ushort*)dest;
                        ushort pixel = reader.ReadUInt16();

                        if ((pixel & 0x8000) != 0)
                        {
                            result = false;
                        }

                        for (; runLength > 0; runLength--, ++x)
                        {
                            if (x >= width)
                            {
                                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                            }

                            if (!flipHorizontal)
                            {
                                *(destPtr++) = pixel;
                            }
                            else
                            {
                                *(destPtr--) = pixel;
                            }
                        }

                        dest = (byte*)destPtr;
                    }
                    return result;
                case BufferFormat.R8G8B8A8_UNorm:
                    {
                        uint pixel;

                        // Do expansion.
                        if (expand)
                        {
                            pixel = (uint)((reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte() | 0xFF000000);
                            result = false;
                        }
                        else
                        {
                            pixel = reader.ReadUInt32();

                            if ((pixel & 0xFF000000) != 0)
                            {
                                result = false;
                            }
                        }

                        uint* destPtr = (uint*)dest;

                        for (; runLength > 0; --runLength, ++x)
                        {
                            if (x >= width)
                            {
                                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                            }

                            if (!flipHorizontal)
                            {
                                *(destPtr++) = pixel;
                            }
                            else
                            {
                                *(destPtr--) = pixel;
                            }
                        }

                        dest = (byte*)destPtr;
                    }
                    return result;
            }

            return false;
        }

        /// <summary>
        /// Function to decode an uncompressed run of pixel data.
        /// </summary>
        /// <param name="reader">The reader used to read the data from the stream.</param>
        /// <param name="dest">The destination buffer pointer.</param>
        /// <param name="x">The current horizontal position in the scanline.</param>
        /// <param name="runLength">The size of the run, in pixels.</param>
        /// <param name="width">The total width of the run.</param>
        /// <param name="expand"><b>true</b> to expand a 24bpp scanline to 32bpp, or <b>false</b> if no expansion is needed.</param>
        /// <param name="flipHorizontal"><b>true</b> to decode the pixels from right to left, or <b>false</b> to decode from left to right.</param>
        /// <param name="format">The pixel format.</param>
        /// <returns><b>true</b> if the run contains entirely transparent pixels, or <b>false</b> if not.</returns>
        private unsafe bool DecodeUncompressedRun(GorgonBinaryReader reader, ref byte* dest, ref int x, int runLength, int width, bool expand, bool flipHorizontal, BufferFormat format)
        {
            bool result = true;

            switch (format)
            {
                case BufferFormat.R8_UNorm:
                    for (; runLength > 0; --runLength, ++x)
                    {
                        if (x >= width)
                        {
                            throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                        }

                        if (!flipHorizontal)
                        {
                            *(dest++) = reader.ReadByte();
                        }
                        else
                        {
                            *(dest--) = reader.ReadByte();
                        }
                    }
                    break;
                case BufferFormat.B5G5R5A1_UNorm:
                    {
                        ushort* destPtr = (ushort*)dest;

                        for (; runLength > 0; runLength--, ++x)
                        {
                            if (x >= width)
                            {
                                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                            }

                            ushort pixel = reader.ReadUInt16();

                            if ((pixel & 0x8000) != 0)
                            {
                                result = false;
                            }

                            if (!flipHorizontal)
                            {
                                *(destPtr++) = pixel;
                            }
                            else
                            {
                                *(destPtr--) = pixel;
                            }
                        }

                        // Send the updated destination address back to the calling function.
                        // This is kind of ugly, but too lazy to make it nice.
                        dest = (byte*)destPtr;
                    }
                    return result;
                case BufferFormat.R8G8B8A8_UNorm:
                    {
                        uint* destPtr = (uint*)dest;

                        for (; runLength > 0; --runLength, ++x)
                        {
                            if (x >= width)
                            {
                                throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                            }

                            uint pixel;

                            if (expand)
                            {
                                pixel = (uint)((reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte() | 0xFF000000);
                                result = false;
                            }
                            else
                            {
                                pixel = reader.ReadUInt32();

                                if ((pixel & 0xFF000000) != 0)
                                {
                                    result = false;
                                }
                            }

                            if (!flipHorizontal)
                            {
                                *(destPtr++) = pixel;
                            }
                            else
                            {
                                *(destPtr--) = pixel;
                            }
                        }

                        // Send the updated destination address back to the calling function.
                        // This is kind of ugly, but too lazy to make it nice.
                        dest = (byte*)destPtr;
                    }
                    return result;
            }

            return false;
        }

        /// <summary>
        /// Function to read uncompressed TGA scanline data.
        /// </summary>
        /// <param name="reader">The reader used to read in the data from the source stream.</param>
        /// <param name="width">Image width.</param>
        /// <param name="dest">Destination buffer pointner</param>
        /// <param name="format">Format of the destination buffer.</param>
        /// <param name="conversionFlags">Flags used for conversion.</param>
        private unsafe bool ReadCompressed(GorgonBinaryReader reader, int width, byte* dest, BufferFormat format, TGAConversionFlags conversionFlags)
        {
            bool setOpaque = true;
            bool flipHorizontal = (conversionFlags & TGAConversionFlags.InvertX) == TGAConversionFlags.InvertX;
            bool expand = (conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand;

            for (int x = 0; x < width;)
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                {
                    throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
                }

                byte rleBlock = reader.ReadByte();
                int size = (rleBlock & 0x7F) + 1;
                if ((rleBlock & 0x80) != 0)
                {
                    if (!DecodeRleEncodedRun(reader, ref dest, ref x, size, width, expand, flipHorizontal, format))
                    {
                        setOpaque = false;
                    }
                    continue;
                }

                if (!DecodeUncompressedRun(reader, ref dest, ref x, size, width, expand, flipHorizontal, format))
                {
                    setOpaque = false;
                }
            }

            return setOpaque;
        }

        /// <summary>
        /// Function to read uncompressed TGA scanline data.
        /// </summary>
        /// <param name="src">The pointer to the buffer containing the source data.</param>
        /// <param name="srcPitch">Pitch of the source scan line.</param>
        /// <param name="dest">Destination buffer pointner</param>
        /// <param name="format">Format of the destination buffer.</param>
        /// <param name="conversionFlags">Flags used for conversion.</param>
        private static unsafe bool ReadUncompressed(byte* src, int srcPitch, byte* dest, BufferFormat format, TGAConversionFlags conversionFlags)
        {
            bool flipHorizontal = (conversionFlags & TGAConversionFlags.InvertX) == TGAConversionFlags.InvertX;

            switch (format)
            {
                case BufferFormat.R8_UNorm:
                case BufferFormat.B5G5R5A1_UNorm:
                    return ImageUtilities.CopyScanline(src, srcPitch, dest, format, flipHorizontal);
                case BufferFormat.R8G8B8A8_UNorm:
                    if ((conversionFlags & TGAConversionFlags.Expand) != TGAConversionFlags.Expand)
                    {
                        return ImageUtilities.CopyScanline(src, srcPitch, dest, format, flipHorizontal);
                    }

                    ImageUtilities.Expand24BPPScanLine(src, srcPitch, dest, flipHorizontal);

                    // We're already opaque by virtue of being 24 bit.
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Function to perform the copying of image data into the buffer.
        /// </summary>
        /// <param name="reader">A reader used to read the data from the source stream.</param>
        /// <param name="image">Image data.</param>
        /// <param name="conversionFlags">Flags used to convert the image.</param>
        private void CopyImageData(GorgonBinaryReader reader, IGorgonImage image, TGAConversionFlags conversionFlags)
        {
            // TGA only supports 1 array level, and 1 mip level, so we only need to get the first buffer.
            IGorgonImageBuffer buffer = image.Buffers[0];

            // Determine how large a row is, in bytes.
            var formatInfo = new GorgonFormatInfo(image.Format);

            GorgonPitchLayout srcPitch = (conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand
                                             ? new GorgonPitchLayout(image.Width * 3, image.Width * 3 * image.Height)
                                             : formatInfo.GetPitchForFormat(image.Width, image.Height);

            unsafe
            {
                // Otherwise, allocate a buffer for conversion.
                byte* destPtr = (byte*)buffer.Data;

                // Adjust destination for inverted axes.
                if ((conversionFlags & TGAConversionFlags.InvertX) == TGAConversionFlags.InvertX)
                {
                    destPtr += buffer.PitchInformation.RowPitch - formatInfo.SizeInBytes;
                }

                if ((conversionFlags & TGAConversionFlags.InvertY) != TGAConversionFlags.InvertY)
                {
                    destPtr += (image.Height - 1) * buffer.PitchInformation.RowPitch;
                }

                // Used to counter the number of lines to force as opaque.
                int opaqueLineCount = 0;
                // The buffer used to hold an uncompressed scanline.
                GorgonNativeBuffer<byte> lineBuffer = null;

                try
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        // Indicates that the scanline has an alpha of 0 for the entire run.
                        bool lineHasZeroAlpha;

                        if ((conversionFlags & TGAConversionFlags.RLE) == TGAConversionFlags.RLE)
                        {
                            lineHasZeroAlpha = ReadCompressed(reader, image.Width, destPtr, image.Format, conversionFlags);
                        }
                        else
                        {
                            // Read the current scanline into memory.
                            if (lineBuffer == null)
                            {
                                lineBuffer = new GorgonNativeBuffer<byte>(srcPitch.RowPitch);
                            }

                            reader.ReadRange(lineBuffer, count: srcPitch.RowPitch);

                            lineHasZeroAlpha = ReadUncompressed((byte*)lineBuffer, srcPitch.RowPitch, destPtr, image.Format, conversionFlags);
                        }

                        if ((lineHasZeroAlpha) && ((conversionFlags & TGAConversionFlags.SetOpaqueAlpha) == TGAConversionFlags.SetOpaqueAlpha))
                        {
                            opaqueLineCount++;
                        }

                        // The components of the pixel data in a TGA file need swizzling for 32 bit.
                        if (formatInfo.BitDepth == 32)
                        {
                            ImageUtilities.SwizzleScanline(destPtr,
                                                           buffer.PitchInformation.RowPitch,
                                                           destPtr,
                                                           buffer.PitchInformation.RowPitch,
                                                           image.Format,
                                                           ImageBitFlags.None);
                        }

                        if ((conversionFlags & TGAConversionFlags.InvertY) != TGAConversionFlags.InvertY)
                        {
                            destPtr -= buffer.PitchInformation.RowPitch;
                        }
                        else
                        {
                            destPtr += buffer.PitchInformation.RowPitch;
                        }
                    }
                }
                finally
                {
                    lineBuffer?.Dispose();
                }

                if (opaqueLineCount != image.Height)
                {
                    return;
                }

                // Set the alpha to opaque if we don't have any alpha values (i.e. alpha = 0 for all pixels).
                destPtr = (byte*)buffer.Data;
                for (int y = 0; y < image.Height; y++)
                {
                    ImageUtilities.CopyScanline(destPtr,
                                                buffer.PitchInformation.RowPitch,
                                                destPtr,
                                                buffer.PitchInformation.RowPitch,
                                                image.Format,
                                                ImageBitFlags.OpaqueAlpha);
                    destPtr += buffer.PitchInformation.RowPitch;
                }
            }
        }

        /// <summary>
        /// Function to load an image from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the image data to read.</param>
        /// <param name="size">The size of the image within the stream, in bytes.</param>
        /// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
        /// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
        protected override IGorgonImage OnDecodeFromStream(Stream stream, long size)
        {
            if (Unsafe.SizeOf<TgaHeader>() >= size)
            {
                throw new EndOfStreamException();
            }

            using (var reader = new GorgonBinaryReader(stream, true))
            {
                IGorgonImageInfo info = ReadHeader(reader, out TGAConversionFlags flags);

                IGorgonImage image = new GorgonImage(info);

                if (DecodingOptions?.SetZeroAlphaAsOpaque ?? true)
                {
                    flags |= TGAConversionFlags.SetOpaqueAlpha;
                }

                CopyImageData(reader, image, flags);

                stream.Position = size;

                return image;
            }
        }

        /// <summary>
        /// Function to persist a <see cref="IGorgonImage"/> to a stream.
        /// </summary>
        /// <param name="imageData">A <see cref="IGorgonImage"/> to persist to the stream.</param>
        /// <param name="stream">The stream that will receive the image data.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="imageData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is read only.</exception>
        /// <exception cref="NotSupportedException">Thrown when the image data in the stream has a pixel format that is unsupported by the codec.</exception>
        /// <remarks>
        /// <para>
        /// When persisting image data via a codec, the image must have a format that the codec can recognize. This list of supported formats is provided by the <see cref="SupportedPixelFormats"/> 
        /// property. Applications may convert their image data a supported format before saving the data using a codec.
        /// </para>
        /// </remarks>
        public override void Save(IGorgonImage imageData, Stream stream)
        {
            // Ensure that we can actually read this format.  We do not perform total pixel conversion on behalf of the user, they are responsible for that.
            // We will, however, support swizzling and pixel compression (e.g. 32 -> 24 bit).
            if (Array.IndexOf(_supportedFormats, imageData.Format) == -1)
            {
                throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
            }

            using (var writer = new GorgonBinaryWriter(stream, true))
            {
                // Write the header for the file before we dump the file contents.
                TgaHeader header = GetHeader(imageData, out TGAConversionFlags conversionFlags);

                GorgonPitchLayout destPitch;

                if ((conversionFlags & TGAConversionFlags.RGB888) == TGAConversionFlags.RGB888)
                {
                    destPitch = new GorgonPitchLayout(imageData.Width * 3, imageData.Width * 3 * imageData.Height);
                }
                else
                {
                    var formatInfo = new GorgonFormatInfo(imageData.Format);
                    destPitch = formatInfo.GetPitchForFormat(imageData.Width, imageData.Height);
                }

                GorgonPitchLayout srcPitch = imageData.Buffers[0].PitchInformation;

                // If the two pitches are equal and we have no conversion requirements, then just write out the buffer.
                if ((destPitch == srcPitch) && (conversionFlags == TGAConversionFlags.None))
                {
                    writer.WriteValue(ref header);
                    writer.WriteRange(imageData.Buffers[0].Data, count: srcPitch.SlicePitch);
                    return;
                }

                unsafe
                {
                    // Get the pointer to the first mip/array/depth level.
                    byte* srcPointer = (byte*)imageData.Buffers[0].Data;
                    var lineBuffer = new GorgonNativeBuffer<byte>(srcPitch.RowPitch);

                    try
                    {
                        // Persist the working buffer to the stream.
                        writer.WriteValue(ref header);

                        // Write out each scan line.					
                        for (int y = 0; y < imageData.Height; y++)
                        {
                            byte* destPtr = (byte*)lineBuffer;

                            if ((conversionFlags & TGAConversionFlags.RGB888) == TGAConversionFlags.RGB888)
                            {
                                ImageUtilities.Compress24BPPScanLine(srcPointer,
                                                                     srcPitch.RowPitch,
                                                                     destPtr,
                                                                     destPitch.RowPitch,
                                                                     (conversionFlags & TGAConversionFlags.Swizzle) == TGAConversionFlags.Swizzle);
                            }
                            else if ((conversionFlags & TGAConversionFlags.Swizzle) == TGAConversionFlags.Swizzle)
                            {
                                ImageUtilities.SwizzleScanline(srcPointer, srcPitch.RowPitch, destPtr, destPitch.RowPitch, imageData.Format, ImageBitFlags.None);
                            }
                            else
                            {
                                ImageUtilities.CopyScanline(srcPointer, srcPitch.RowPitch, destPtr, destPitch.RowPitch, imageData.Format, ImageBitFlags.None);
                            }

                            srcPointer += srcPitch.RowPitch;

                            writer.WriteRange(lineBuffer, count: destPitch.RowPitch);
                        }
                    }
                    finally
                    {
                        lineBuffer?.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Function to read the meta data for image data within a stream.
        /// </summary>
        /// <param name="stream">The stream containing the metadata to read.</param>
        /// <returns>
        /// The image meta data as a <see cref="IGorgonImageInfo"/> value.
        /// </returns>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
        /// <para>-or-</para>
        /// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>
        /// <para>
        /// Ensure that the file can be read by calling the <see cref="IsReadable"/> method prior to calling this method.
        /// </para> 
        /// </remarks>
        /// <seealso cref="IsReadable"/>
        public override IGorgonImageInfo GetMetaData(Stream stream)
        {
            int headerSize = Unsafe.SizeOf<TgaHeader>();
            long position = 0;


            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
            }

            if (stream.Length - stream.Position < sizeof(uint) + headerSize)
            {
                throw new EndOfStreamException();
            }

            GorgonBinaryReader reader = null;

            try
            {
                position = stream.Position;
                reader = new GorgonBinaryReader(stream, true);
                return ReadHeader(reader, out _);
            }
            finally
            {
                stream.Position = position;
                reader?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if this codec can read the image data within the stream or not.
        /// </summary>
        /// <param name="stream">The stream that is used to read the image data.</param>
        /// <returns><b>true</b> if the codec can read the file, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        public override bool IsReadable(Stream stream)
        {
            TgaHeader header;
            long position = 0;

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORIMG_ERR_STREAM_CANNOT_SEEK);
            }

            if (stream.Length - stream.Position < Unsafe.SizeOf<TgaHeader>())
            {
                return false;
            }

            try
            {
                position = stream.Position;
                var reader = new GorgonBinaryReader(stream, true);
                header = reader.ReadValue<TgaHeader>();
            }
            finally
            {
                stream.Position = position;
            }

            if ((header.ColorMapType != 0) || (header.ColorMapLength != 0))
            {
                return false;
            }

            if ((header.Descriptor & (TgaDescriptor.Interleaved2Way | TgaDescriptor.Interleaved4Way)) != 0)
            {
                return false;
            }

            if ((header.Width <= 0) || (header.Height <= 0))
            {
                return false;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((header.ImageType != TgaImageType.TrueColor) && (header.ImageType != TgaImageType.TrueColorRLE)
                && (header.ImageType != TgaImageType.BlackAndWhite) && (header.ImageType != TgaImageType.BlackAndWhiteRLE))
            {
                return false;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return ((header.ImageType != TgaImageType.BlackAndWhite) && (header.ImageType != TgaImageType.BlackAndWhiteRLE)) ||
                   (header.BPP == 8);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecTga" /> class.
        /// </summary>
        /// <param name="decodingOptions">[Optional] Codec specific options to use when decoding image data.</param>
        public GorgonCodecTga(GorgonTgaDecodingOptions decodingOptions = null)
            : base(null, decodingOptions) => CodecCommonExtensions = new[] { "tga", "tpic" };
        #endregion
    }
}
