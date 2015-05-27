#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;

namespace Gorgon.IO
{
    #region Enums.
    /// <summary>
    /// Image types.
    /// </summary>
    enum TGAImageType
        : byte
    {
        /// <summary>
        /// No image data.
        /// </summary>
        NoImage = 0,
        /// <summary>
        /// Color mapped image data.
        /// </summary>
        ColorMapped = 1,
        /// <summary>
        /// True color image data.
        /// </summary>
        TrueColor = 2,
        /// <summary>
        /// Black and white image data.
        /// </summary>
        BlackAndWhite = 3,
        /// <summary>
        /// Compressed color mapped image data.
        /// </summary>
        ColorMappedRLE = 9,
        /// <summary>
        /// Compressed true color image data.
        /// </summary>
        TrueColorRLE = 10,
        /// <summary>
        /// Compressed black and white data.
        /// </summary>
        BlackAndWhiteRLE = 11
    }

    /// <summary>
    /// Descriptor flags.
    /// </summary>
    [Flags]
    enum TGADescriptor
        : byte
    {		
		/// <summary>
		/// 16 bit.
		/// </summary>
		RGB555A1 = 0x1,
		/// <summary>
		/// 32 bit
		/// </summary>
		RGB888A8 = 0x8,
        /// <summary>
        /// Invert on the x-axis.
        /// </summary>
        InvertX = 0x10,
        /// <summary>
        /// Invert on the y-axis.
        /// </summary>
        InvertY = 0x20,
        /// <summary>
        /// 2 way interleaved (depreciated).
        /// </summary>
        Interleaved2Way = 0x40,
        /// <summary>
        /// 4 way interleaved (depreciated).
        /// </summary>
        Interleaved4Way = 0x80
    }

    /// <summary>
    /// TGA specific conversion flags.
    /// </summary>
    [Flags]
    enum TGAConversionFlags
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
        /// Swizzle BGR to RGB/RGB to BGR
        /// </summary>
        Swizzle = 0x10000,
        /// <summary>
        /// 24 bit format.
        /// </summary>
        RGB888 = 0x20000
    }
    #endregion

    /// <summary>
	/// A codec to handle reading/writing Truevision TGA files.
	/// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>The codec accepts and returns a <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> type, which is filled from or read into the encoded file.</para>
    /// <para>The TGA encoder has the following limitations:
    /// <list type="bullet">
    ///     <item>
    ///         <description>No color map support.</description>
    ///     </item>
    ///     <item>
    ///         <description>Interleaved files are not supported.</description>
    ///     </item>
    ///     <item>
    ///         <description>Supports the following formats: 8 bit greyscale, 16, 24 and 32 bits per pixel images.</description>
    ///     </item>
    ///     <item>
    ///         <description>Writes uncompressed files only.  RLE is only supported for reading.</description>
    ///     </item>
	///     <item>
	///			<description>Only supports saving the first depth slice, mip level and first array index.  Other depth slices, mip levels and array indices are ignored.</description>
	///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public unsafe sealed class GorgonCodecTGA
		: GorgonImageCodec
	{
		#region Value Types.
        /// <summary>
        /// Header information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TGAHeader
        {
            /// <summary>
            /// Length of the ID.
            /// </summary>
            public readonly byte IDLength;
            /// <summary>
            /// Color map type.
            /// </summary>
            public readonly byte ColorMapType;
            /// <summary>
            /// Image type.
            /// </summary>
            public TGAImageType ImageType;
            /// <summary>
            /// First color map index.
            /// </summary>
            private readonly ushort ColorMapFirst;
            /// <summary>
            /// Length of the color map indices.
            /// </summary>
            public readonly ushort ColorMapLength;
            /// <summary>
            /// Size of the color map.
            /// </summary>
            private readonly byte ColorMapSize;
            /// <summary>
            /// Starting horizontal position.
            /// </summary>
            private readonly ushort XOrigin;
            /// <summary>
            /// Starting vertical position.
            /// </summary>
            private readonly ushort YOrigin;
            /// <summary>
            /// Width of the image.
            /// </summary>
            public ushort Width;
            /// <summary>
            /// Height of the image.
            /// </summary>
            public ushort Height;
            /// <summary>
            /// Bits per pixel.
            /// </summary>
            public byte BPP;
            /// <summary>
            /// Descriptor flag.
            /// </summary>
            public TGADescriptor Descriptor;
        }
		#endregion

        #region Variables.
        // List of supported image formats.
        private readonly BufferFormat[] _supportedFormats =
        {
            // 8 bit grayscale.
            BufferFormat.R8_UIntNormal,
            // 16 bit.
            BufferFormat.B5G5R5A1_UIntNormal,
            // 24 bit.
            BufferFormat.R8G8B8A8_UIntNormal,
			BufferFormat.B8G8R8A8_UIntNormal,
			BufferFormat.B8G8R8X8_UIntNormal,
        };
        #endregion

        #region Properties.
	    /// <summary>
        /// Property to return the data formats for the image.
        /// </summary>
        public override IEnumerable<BufferFormat> SupportedFormats
        {
            get
            {
                return _supportedFormats;
            }
        }

		/// <summary>
		/// Property to return whether the image codec supports block compression.
		/// </summary>
		public override bool SupportsBlockCompression
		{
			get
			{
				return false;
			}
		}

        /// <summary>
		/// Property to set or return whether to set the alpha on the image as opaque if no alpha values are present.
		/// </summary>
		/// <remarks>Setting this to <c>true</c> will set all alpha values to an opaque value if the image does not contain an alpha value greater than 0 for any pixel.
		/// <para>This property only applied to images that are being read.</para>
		/// <para>If the image format does not contain alpha, then this setting is ignored.</para>
		/// <para>The default value is <c>true</c>.</para>
		/// </remarks>
		public bool SetOpaqueIfZeroAlpha
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public override string CodecDescription
		{
			get 
			{
				return Resources.GORGFX_IMAGE_TGA_CODEC_DESC;
			}
		}

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. PNG).
		/// </summary>
		public override string Codec
		{
			get 
			{
				return "TGA";
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
        public override bool SupportsArray
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
        public override bool SupportsMipMaps
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
        public override bool SupportsDepth
        {
            get
            {
                return false;
            }
        }

        /// <summary>
		/// Function to read in the DDS header from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the data.</param>
        /// <param name="conversionFlags">Flags for conversion.</param>
		/// <returns>New image settings.</returns>
        private IImageSettings ReadHeader(GorgonDataStream stream, out TGAConversionFlags conversionFlags)
        {            
            IImageSettings settings = new GorgonTexture2DSettings();

			conversionFlags = TGAConversionFlags.None;

            // Get the header for the file.
            var header = stream.Read<TGAHeader>();

            if ((header.ColorMapType != 0) || (header.ColorMapLength != 0)
				|| (header.Width <= 0) || (header.Height <= 0)
				|| ((header.Descriptor & TGADescriptor.Interleaved2Way) == TGADescriptor.Interleaved2Way)
				|| ((header.Descriptor & TGADescriptor.Interleaved4Way) == TGADescriptor.Interleaved4Way))
            {
                throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
            }

            settings.MipCount = 1;
            settings.ArrayCount = 1;

            switch (header.ImageType)
            {
                case TGAImageType.TrueColor:
                case TGAImageType.TrueColorRLE:
                    switch (header.BPP)
                    {
                        case 16:
                            settings.Format = BufferFormat.B5G5R5A1_UIntNormal;
                            break;
                        case 24:
                        case 32:
                            settings.Format = BufferFormat.R8G8B8A8_UIntNormal;
                            if (header.BPP == 24)
                            {
                                conversionFlags |= TGAConversionFlags.Expand;
                            }
                            break;
                    }

                    if (header.ImageType == TGAImageType.TrueColorRLE)
                    {
                        conversionFlags |= TGAConversionFlags.RLE;
                    }
                    break;
                case TGAImageType.BlackAndWhite:
                case TGAImageType.BlackAndWhiteRLE:
                    if (header.BPP == 8)
                    {
                        settings.Format = BufferFormat.R8_UIntNormal;
                    }
                    else
                    {
						throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, header.ImageType));
                    }

                    if (header.ImageType == TGAImageType.BlackAndWhiteRLE)
                    {
                        conversionFlags |= TGAConversionFlags.RLE;
                    }
                    break;
                default:
					throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, header.ImageType));
            }

            settings.Width = header.Width;
            settings.Height = header.Height;

            if ((header.Descriptor & TGADescriptor.InvertX) == TGADescriptor.InvertX)
            {
                conversionFlags |= TGAConversionFlags.InvertX;
            }

            if ((header.Descriptor & TGADescriptor.InvertY) == TGADescriptor.InvertY)
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
		        stream.ReadByte();
		    }

		    return settings;
        }

		/// <summary>
		/// Function to write out the DDS header to the stream.
		/// </summary>
		/// <param name="settings">Meta data for the image header.</param>
		/// <param name="writer">Writer interface for the stream.</param>
		/// <param name="conversionFlags">Flags for image conversion.</param>
		private void WriteHeader(IImageSettings settings, GorgonBinaryWriter writer, out TGAConversionFlags conversionFlags)
		{
			TGAHeader header = default(TGAHeader);

			conversionFlags = TGAConversionFlags.None;

			if ((settings.Width > 0xFFFF)
				|| (settings.Height > 0xFFFF))
			{
				throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_ENCODER, Codec));
			}

			header.Width = (ushort)(settings.Width);
			header.Height = (ushort)(settings.Height);

			switch (settings.Format)
			{
				case BufferFormat.R8G8B8A8_UIntNormal:
				case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					header.ImageType = TGAImageType.TrueColor;
					header.BPP = 32;
					header.Descriptor = TGADescriptor.InvertY | TGADescriptor.RGB888A8;
					conversionFlags |= TGAConversionFlags.Swizzle;
					break;
				case BufferFormat.B8G8R8A8_UIntNormal:
				case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					header.ImageType = TGAImageType.TrueColor;
					header.BPP = 32;
					header.Descriptor = TGADescriptor.InvertY | TGADescriptor.RGB888A8;
					break;
				case BufferFormat.B8G8R8X8_UIntNormal:
				case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
					header.ImageType = TGAImageType.TrueColor;
					header.BPP = 24;
					header.Descriptor = TGADescriptor.InvertY;
					conversionFlags |= TGAConversionFlags.RGB888;
					break;

				case BufferFormat.R8_UIntNormal:
				case BufferFormat.A8_UIntNormal:
					header.ImageType = TGAImageType.BlackAndWhite;
					header.BPP = 8;
					header.Descriptor = TGADescriptor.InvertY;
					break;

				case BufferFormat.B5G5R5A1_UIntNormal:
					header.ImageType = TGAImageType.TrueColor;
					header.BPP = 16;
					header.Descriptor = TGADescriptor.InvertY | TGADescriptor.RGB555A1;
					break;
				default:
					throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, settings.Format));
			}

			// Persist to stream.
			writer.WriteValue(header);
		}

		/// <summary>
		/// Function to compress a 32 bit scanline to a 24 bit bit scanline.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		private static void Compress24BPPScanLine(void* src, int srcPitch, void* dest, int destPitch)
		{
			var srcPtr = (uint*)src;
			var destPtr = (byte*)dest;
			byte* endPtr = destPtr + destPitch;

			for (int srcCount = 0; srcCount < srcPitch; srcCount += 4)
			{
				uint pixel = *(srcPtr++);

				if (destPtr + 2 > endPtr)
				{
					return;
				}

				*(destPtr++) = (byte)(pixel & 0xFF);			//B
				*(destPtr++) = (byte)((pixel & 0xFF) >> 8);		//G
				*(destPtr++) = (byte)((pixel & 0xFF) >> 16);	//R
			}
		}

	    /// <summary>
	    /// Function to read uncompressed TGA scanline data.
	    /// </summary>
	    /// <param name="src">Pointer to the source data.</param>
	    /// <param name="width">Image width.</param>
	    /// <param name="dest">Destination buffer pointner</param>
	    /// <param name="end">End of the scan line.</param>
	    /// <param name="format">Format of the destination buffer.</param>
	    /// <param name="conversionFlags">Flags used for conversion.</param>
	    private bool ReadCompressed(ref byte *src, int width, byte *dest, byte *end, BufferFormat format, TGAConversionFlags conversionFlags)
		{
			bool setOpaque = true;
			bool flipHorizontal = (conversionFlags & TGAConversionFlags.InvertX) == TGAConversionFlags.InvertX;			

			switch (format)
			{
				case BufferFormat.R8_UIntNormal:
					for (int x = 0; x < width; )
					{
						if (src >= end)
						{
							throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
						}

						int size = ((*src & 0x7F) + 1);

						// Do a repeat run.
						if ((*(src++) & 0x80) != 0)
						{
							if (src >= end)
							{
								throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
							}

							for (; size > 0; --size, ++x)
							{
								if (x >= width)
								{
									throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
								}

								if (!flipHorizontal)
								{
									*(dest++) = *src;
								}
								else
								{
									*(dest--) = *src;
								}
							}

							++src;
						}
						else
						{
							if (src + size > end)
							{
								throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
							}

							for (; size > 0; --size, ++x)
							{
								if (x >= width)
								{
									throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
								}

								if (!flipHorizontal)
								{
									*(dest++) = *(src++);
								}
								else
								{
									*(dest--) = *(src++);
								}
							}
						}
					}
					return false;
				case BufferFormat.B5G5R5A1_UIntNormal:
					{
						var destPtr = (ushort*)dest;

						for (int x = 0; x < width; )
						{
							if (src >= end)
							{
								throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
							}

							int size = ((*src & 0x7F) + 1);							

							// Do a repeat run.
							if ((*(src++) & 0x80) != 0)
							{
								if ((src + 1) >= end)
								{
									throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
								}

								var pixel = (ushort)(*(src++) | (*(src++) << 8));

								if ((pixel & 0x8000) != 0)
								{
									setOpaque = false;
								}

								for (; size > 0; size--, ++x)
								{
									if (x >= width)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
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
							}
							else
							{
								if (src + (size * 2) > end)
								{
									throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
								}

								for (; size > 0; size--, ++x)
								{
									if (x >= width)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
									}

									var pixel = (ushort)(*(src++) | (*(src++) << 8));

									if ((pixel & 0x8000) != 0)
									{
										setOpaque = false;
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
							}							
						}
						return setOpaque;
					}
				case BufferFormat.R8G8B8A8_UIntNormal:
					{
						var destPtr = (uint*)dest;
						
						for (int x = 0; x < width;)
						{
							if (src >= end)
							{
								throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
							}

							uint pixel;
							int size = (*src & 0x7F) + 1;							

							if ((*(src++) & 0x80) != 0)
							{								
								// Do expansion.
								if ((conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand)
								{
									if (src + 2 >= end)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
									}

									pixel = ((uint)(*src << 16) | (uint)(*(src + 1) << 8) | (*(src + 2)) | 0xFF000000);
									src += 3;
									setOpaque = false;
								}
								else
								{
									if (src + 3 >= end)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
									}

									pixel = ((uint)(*src << 16) | (uint)(*(src + 1) << 8) | (*(src + 2)) | (uint)(*(src + 3) << 24));									

									if (*(src + 3) > 0)
									{
										setOpaque = false;
									}

									src += 4;
								}

								for (; size > 0; --size, ++x)
								{
									if (x >= width)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
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
							}
							else
							{
								if ((conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand)
								{
									if (src + (size * 3) > end)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
									}
								}
								else
								{
									if (src + (size * 4) > end)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
									}
								}

								for (; size > 0; --size, ++x)
								{
									if (x >= width)
									{
										throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
									}

									if ((conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand)
									{
										if (src + 2 >= end)
										{
											throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
										}

										pixel = ((uint)(*src << 16) | (uint)(*(src + 1) << 8) | (*(src + 2)) | 0xFF000000);
										src += 3;

										setOpaque = false;
									}
									else
									{
										if (src + 3 >= end)
										{
											throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
										}

										pixel = ((uint)(*src << 16) | (uint)(*(src + 1) << 8) | (*(src + 2)) | (uint)(*(src + 3) << 24));

										if (*(src + 3) > 0)
										{
											setOpaque = false;
										}

										src += 4;
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
							}
						}

						return setOpaque;
					}
			}

			return false;

		}

	    /// <summary>
	    /// Function to read uncompressed TGA scanline data.
	    /// </summary>
	    /// <param name="src">Pointer to the source data.</param>
	    /// <param name="srcPitch">Pitch of the source scan line.</param>
	    /// <param name="dest">Destination buffer pointner</param>
	    /// <param name="format">Format of the destination buffer.</param>
	    /// <param name="conversionFlags">Flags used for conversion.</param>
	    private static bool ReadUncompressed(byte* src, int srcPitch, byte* dest, BufferFormat format, TGAConversionFlags conversionFlags)
		{
			bool setOpaque = true;
			bool flipHorizontal = (conversionFlags & TGAConversionFlags.InvertX) == TGAConversionFlags.InvertX;

			switch (format)
			{
				case BufferFormat.R8_UIntNormal:
					for (int x = 0; x < srcPitch; x++)
					{
						if (!flipHorizontal)
						{
							*(dest++) = *(src++);
						}
						else
						{
							*(dest--) = *(src++);
						}						
					}
					return false;
				case BufferFormat.B5G5R5A1_UIntNormal:
					{
						var destPtr = (ushort*)dest;

						for (int x = 0; x < srcPitch; x += 2)
						{
							var pixel = (ushort)(*(src++) | (*(src++) << 8));

							if ((pixel & 0x8000) != 0)
							{
								setOpaque = false;
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

						return setOpaque;
					}
				case BufferFormat.R8G8B8A8_UIntNormal:
					{
						var destPtr = (uint*)dest;
						int x = 0;

						while (x < srcPitch)
						{
							uint pixel;

							// We need to expand from 24 bit.
							if ((conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand)
							{
								pixel = ((uint)(*(src++) << 16) | (uint)(*(src++) << 8) | (*(src++)) | 0xFF000000);
								setOpaque = false;
								x += 3;
							}
							else
							{
								uint alpha = 0;

								pixel = ((uint)(*(src++) << 16) | (uint)(*(src++) << 8) | (*(src++)) | alpha);
								alpha = (uint)(*(src++) << 24);

								pixel |= alpha;									

								if (alpha != 0)
								{
									setOpaque = false;
								}

								x += 4;
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

						return setOpaque;
					}					
			}

			return false;
		}

		/// <summary>
		/// Function to perform the copying of image data into the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the image data.</param>
		/// <param name="image">Image data.</param>
		/// <param name="conversionFlags">Flags used to convert the image.</param>
		private void CopyImageData(GorgonDataStream stream, GorgonImageData image, TGAConversionFlags conversionFlags)
		{
			var buffer = image.Buffers[0];	        // Get the first buffer only.
			var formatInfo = GorgonBufferFormatInfo.GetInfo(image.Settings.Format);

			GorgonFormatPitch srcPitch = (conversionFlags & TGAConversionFlags.Expand) == TGAConversionFlags.Expand
				                             ? new GorgonFormatPitch(image.Settings.Width * 3, image.Settings.Width * 3 * image.Settings.Height)
				                             : formatInfo.GetPitch(image.Settings.Width, image.Settings.Height, PitchFlags.None);

			// Otherwise, allocate a buffer for conversion.
			var srcPtr = (byte*)stream.PositionPointerUnsafe;
			var destPtr = (byte*)buffer.Data.UnsafePointer;

			// Adjust destination for inverted axes.
			if ((conversionFlags & TGAConversionFlags.InvertX) == TGAConversionFlags.InvertX)
			{
				destPtr += buffer.PitchInformation.RowPitch - formatInfo.SizeInBytes;
			}

			if ((conversionFlags & TGAConversionFlags.InvertY) != TGAConversionFlags.InvertY)
			{
				destPtr += (image.Settings.Height - 1) * buffer.PitchInformation.RowPitch;
			}

			// Get bounds of image memory.
			var scanSize = (int)(stream.Length - stream.Position);
			byte* endScan = (byte*)stream.PositionPointerUnsafe + scanSize;

			int opaqueLineCount = 0;
			for (int y = 0; y < image.Settings.Height; y++)
			{
				bool setOpaque;

				if ((conversionFlags & TGAConversionFlags.RLE) == TGAConversionFlags.RLE)
				{
					setOpaque = ReadCompressed(ref srcPtr, image.Settings.Width, destPtr, endScan, image.Settings.Format, conversionFlags);
				}
				else
				{
					setOpaque = ReadUncompressed(srcPtr, srcPitch.RowPitch, destPtr, image.Settings.Format, conversionFlags);
					srcPtr += srcPitch.RowPitch;
				}

				if ((setOpaque) && (SetOpaqueIfZeroAlpha))
				{
					opaqueLineCount++;
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

			if (opaqueLineCount != image.Settings.Height)
			{
				return;
			}

			// Set the alpha to opaque if we don't have any alpha values (i.e. alpha = 0 for all pixels).
			destPtr = (byte*)buffer.Data.UnsafePointer;
			for (int y = 0; y < image.Settings.Height; y++)
			{
				CopyScanline(destPtr, buffer.PitchInformation.RowPitch, destPtr, buffer.PitchInformation.RowPitch, image.Settings.Format, ImageBitFlags.OpaqueAlpha);
				destPtr += buffer.PitchInformation.RowPitch;
			}
		}

        /// <summary>
        /// Function to load an image from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the data to load.</param>
        /// <param name="size">Size of the data to read, in bytes.</param>
        /// <returns>
        /// The image data that was in the stream.
        /// </returns>
		protected internal override GorgonImageData LoadFromStream(GorgonDataStream stream, int size)
		{
			GorgonImageData imageData = null;
	        TGAConversionFlags flags;

            if (DirectAccess.SizeOf<TGAHeader>() > size)
            {
                throw new EndOfStreamException(Resources.GORGFX_STREAM_EOF);
            }

			// Read the header information.
			IImageSettings settings = ReadHeader(stream, out flags);

			if (ArrayCount > 1)
			{
				settings.ArrayCount = ArrayCount;
			}

            try
            {
                // Create our image data structure.
			    imageData = new GorgonImageData(settings);

				// Copy the data from the stream to the buffer.
				CopyImageData(stream, imageData, flags);
			}
			catch 
			{
				// Clean up any memory allocated if we can't copy the image.
				if (imageData != null)
				{
					imageData.Dispose();
				}

				throw;
			}

			return imageData;
		}

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="Gorgon.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal override void SaveToStream(GorgonImageData imageData, Stream stream)
		{
			// Use a binary writer.
			using (var writer = new GorgonBinaryWriter(stream, true))
			{
				// Write the header for the file.
				TGAConversionFlags conversionFlags;
				WriteHeader(imageData.Settings, writer, out conversionFlags);

				GorgonFormatPitch pitch;
				if ((conversionFlags & TGAConversionFlags.RGB888) == TGAConversionFlags.RGB888)
				{
					pitch = new GorgonFormatPitch(imageData.Settings.Width * 3, imageData.Settings.Width * 3 * imageData.Settings.Height);
				}
				else
				{
					var formatInfo = GorgonBufferFormatInfo.GetInfo(imageData.Settings.Format);
					pitch = formatInfo.GetPitch(imageData.Settings.Width, imageData.Settings.Height, PitchFlags.None);
				}

				// Get the pointer to the first mip/array/depth level.
				var srcPointer = (byte *)imageData.Buffers[0].Data.UnsafePointer;
				var srcPitch = imageData.Buffers[0].PitchInformation;

				// If the two pitches are equal, then just write out the buffer.
				if ((pitch == srcPitch) && (conversionFlags == TGAConversionFlags.None))
				{
					writer.Write(srcPointer, srcPitch.SlicePitch);
					return;
				}
				
				// If we have to do a conversion, create a worker buffer.
				using (var convertBuffer = new GorgonDataStream(pitch.SlicePitch))
				{
					var destPtr = (byte*)convertBuffer.UnsafePointer;

					// Write out each scan line.					
					for (int y = 0; y < imageData.Settings.Height; y++)
					{
						if ((conversionFlags & TGAConversionFlags.RGB888) == TGAConversionFlags.RGB888)
						{
							Compress24BPPScanLine(srcPointer, srcPitch.RowPitch, destPtr, pitch.RowPitch);
						}
						else if ((conversionFlags & TGAConversionFlags.Swizzle) == TGAConversionFlags.Swizzle)
						{
							SwizzleScanline(srcPointer, srcPitch.RowPitch, destPtr, pitch.RowPitch, imageData.Settings.Format, ImageBitFlags.None);
						}
						else
						{
							CopyScanline(srcPointer, srcPitch.RowPitch, destPtr, pitch.RowPitch, imageData.Settings.Format, ImageBitFlags.None);
						}

						destPtr += pitch.RowPitch;
						srcPointer += srcPitch.RowPitch;
					}

					// Persist to the stream.
					writer.Write(convertBuffer.UnsafePointer, pitch.SlicePitch);
				}
			}
		}

		/// <summary>
		/// Function to read file meta data.
		/// </summary>
		/// <param name="stream">Stream used to read the metadata.</param>
		/// <returns>
		/// The image meta data as a <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> value.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
		/// <para>-or-</para>
		/// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override IImageSettings GetMetaData(Stream stream)
		{
            long position = 0;
			int headerSize = DirectAccess.SizeOf<TGAHeader>();

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_STREAM_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_STREAM_NO_SEEK);
            }

            if (stream.Length - stream.Position < sizeof(uint) + DirectAccess.SizeOf<TGAHeader>())
            {
                throw new EndOfStreamException(Resources.GORGFX_STREAM_EOF);
            }

            try
            {
                position = stream.Position;

	            TGAConversionFlags conversion;
	            var gorgonDataStream = stream as GorgonDataStream;
	            if (gorgonDataStream != null)
                {
                    return ReadHeader(gorgonDataStream, out conversion);
                }

                using (var memoryStream = new GorgonDataStream(headerSize))
                {
                    memoryStream.ReadFromStream(stream, headerSize);
					memoryStream.Position = 0;
                    return ReadHeader(memoryStream, out conversion);
                }
            }
            finally
            {
                stream.Position = position;
            }            
		}

		/// <summary>
		/// Function to determine if this codec can read the file or not.
		/// </summary>
		/// <param name="stream">Stream used to read the file information.</param>
		/// <returns>
		/// <c>true</c> if the codec can read the file, <c>false</c> if not.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override bool IsReadable(Stream stream)
        {
			TGAHeader header;
			long position = 0;

			if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_STREAM_READ_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_STREAM_NO_SEEK);
            }
            
            if (stream.Length - stream.Position < DirectAccess.SizeOf<TGAHeader>())
			{
				return false;
			}
            
			try
			{
                position = stream.Position;
				var reader = new GorgonBinaryReader(stream, true); 
				header = reader.ReadValue<TGAHeader>();
			}
			finally
			{
				stream.Position = position;
			}

			if ((header.ColorMapType != 0) || (header.ColorMapLength != 0))
			{
				return false;
			}

			if ((header.Descriptor & (TGADescriptor.Interleaved2Way | TGADescriptor.Interleaved4Way)) != 0)
			{
				return false;
			}

			if ((header.Width <= 0) || (header.Height <= 0))
			{
				return false;
			}

			if ((header.ImageType != TGAImageType.TrueColor) && (header.ImageType != TGAImageType.TrueColorRLE)
				&& (header.ImageType != TGAImageType.BlackAndWhite) && (header.ImageType != TGAImageType.BlackAndWhiteRLE))
			{
				return false;
			}

			return ((header.ImageType != TGAImageType.BlackAndWhite) && (header.ImageType != TGAImageType.BlackAndWhiteRLE)) ||
			       (header.BPP == 8);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecTGA" /> class.
		/// </summary>
		public GorgonCodecTGA()
		{
			SetOpaqueIfZeroAlpha = true;

			CodecCommonExtensions = new[] { "tga", "tpic" };
		}
		#endregion
	}
}
