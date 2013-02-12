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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GorgonLibrary.Math;
using GorgonLibrary.Native;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
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
    [Flags()]
    enum TGADescriptor
        : byte
    {
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
    [Flags()]
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
    /// <para>The codec accepts and returns a <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> type, which is filled from or read into the encoded file.</para>
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
    ///         <description>Writes uncompressed files.  RLE is only supported for reading.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public unsafe sealed class GorgonCodecTGA
		: GorgonImageCodec
	{
		#region Constants.
		private const string MagicNumber = "TRUEVISION-XFILE.";		// The TGA file magic string.
		#endregion

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
            public byte IDLength;
            /// <summary>
            /// Color map type.
            /// </summary>
            public byte ColorMapType;
            /// <summary>
            /// Image type.
            /// </summary>
            public TGAImageType ImageType;
            /// <summary>
            /// First color map index.
            /// </summary>
            public ushort ColorMapFirst;
            /// <summary>
            /// Length of the color map indices.
            /// </summary>
            public ushort ColorMapLength;
            /// <summary>
            /// Size of the color map.
            /// </summary>
            public byte ColorMapSize;
            /// <summary>
            /// Starting horizontal position.
            /// </summary>
            public ushort XOrigin;
            /// <summary>
            /// Starting vertical position.
            /// </summary>
            public ushort YOrigin;
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

        /// <summary>
        /// Extension information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct TGAExtension
        {
            /// <summary>
            /// Size of the extension.
            /// </summary>
            public ushort Size;
            /// <summary>
            /// Author.
            /// </summary>
            public fixed byte AuthorName[41];
            /// <summary>
            /// Comment.
            /// </summary>
            public fixed byte AuthorComment[324];
            /// <summary>
            /// Timestamp month.
            /// </summary>
            public ushort StampMonth;
            /// <summary>
            /// Timestamp day.
            /// </summary>
            public ushort StampDay;
            /// <summary>
            /// Timestamp year.
            /// </summary>
            public ushort StampYear;
            /// <summary>
            /// Timestamp hour.
            /// </summary>
            public ushort StampHour;
            /// <summary>
            /// Timestamp minute.
            /// </summary>
            public ushort StampMinute;
            /// <summary>
            /// Timestamp second.
            /// </summary>
            public ushort StampSecond;
            /// <summary>
            /// Job name.
            /// </summary>
            public fixed byte JobName[41];
            /// <summary>
            /// Job hour.
            /// </summary>
            public ushort JobHour;
            /// <summary>
            /// Job minute.
            /// </summary>
            public ushort JobMinute;
            /// <summary>
            /// Job second.
            /// </summary>
            public ushort JobSecond;
            /// <summary>
            /// Software ID.
            /// </summary>
            public fixed byte SoftwareId[41];
            /// <summary>
            /// Version number.
            /// </summary>
            public ushort VersionNumber;
            /// <summary>
            /// Version letter.
            /// </summary>
            public byte VersionLetter;
            /// <summary>
            /// Key color.
            /// </summary>
            public uint KeyColor;
            /// <summary>
            /// Pixel numerator.
            /// </summary>
            public ushort PixelNumerator;
            /// <summary>
            /// Pixel denominator.
            /// </summary>
            public ushort PixelDenominator;
            /// <summary>
            /// Gamma numerator.
            /// </summary>
            public ushort GammaNumerator;
            /// <summary>
            /// Gamme denominator.
            /// </summary>
            public ushort GammaDenominator;
            /// <summary>
            /// Offset.
            /// </summary>
            public uint ColorOffset;
            /// <summary>
            /// Offset.
            /// </summary>
            public uint StampOffset;
            /// <summary>
            /// Offset.
            /// </summary>
            public uint ScanOffset;
            /// <summary>
            /// Attribute type.
            /// </summary>
            public byte AttributesType;
        }

        /// <summary>
        /// Footer information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct TGAFooter
        {
            /// <summary>
            /// Offset in the file for the extension.
            /// </summary>
            public ushort ExtensionOffset;
            /// <summary>
            /// Offset in the file for the developer.
            /// </summary>
            public ushort DeveloperOffset;
            /// <summary>
            /// Signature.
            /// </summary>
            public fixed byte Signature[18];
        }
		#endregion

		#region Variables.
        private BufferFormat[] _formats = null;         // Buffer formats.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public override string CodecDescription
		{
			get 
			{
				return "Truevision Targa";
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
		/// Function to read in the DDS header from a stream.
		/// </summary>
		/// <param name="reader">Reader interface for the stream.</param>
        /// <param name="conversionFlags">Flags for conversion.</param>
		/// <returns>New image settings.</returns>
        private IImageSettings ReadHeader(GorgonBinaryReader reader, out TGAConversionFlags conversionFlags)
        {            
            IImageSettings settings = new GorgonTexture2DSettings();
            TGAHeader header = default(TGAHeader);

            conversionFlags = TGAConversionFlags.None;

            // Get the header for the file.
            header = reader.ReadValue<TGAHeader>();

            if ((header.ColorMapType != 0) || (header.ColorMapLength != 0) 
                || (header.ImageType == TGAImageType.ColorMapped) 
                || (header.ImageType == TGAImageType.ColorMappedRLE))
            {
                throw new System.IO.IOException("Cannot decode the TGA file from the stream. Color mapped TGA files are not supported.");
            }

            if ((header.Descriptor & (TGADescriptor.Interleaved2Way | TGADescriptor.Interleaved4Way)) != 0)
            {
                throw new System.IO.IOException("Cannot decode the TGA file from the stream. Color mapped TGA files are not supported.");
            }

            if ((header.Width <= 0) || (header.Height <= 0))
            {
                throw new System.IO.IOException("Cannot decode the TGA file from the stream. Invalid width [" + header.Width.ToString() + "] or height [" + header.Height.ToString () + "].");
            }

            settings.MipCount = 1;
            settings.ArrayCount = 1;
            settings.Depth = 1;

            switch (header.ImageType)
            {
                case TGAImageType.TrueColor:
                case TGAImageType.TrueColorRLE:
                    switch (header.BPP)
                    {
                        case 16:
                            settings.Format = BufferFormat.B5G6R5_UIntNormal;
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
                        throw new System.IO.IOException("Cannot decode the TGA file from the stream. Only 8bpp grayscale images are supported.");
                    }

                    if (header.ImageType == TGAImageType.BlackAndWhiteRLE)
                    {
                        conversionFlags |= TGAConversionFlags.RLE;
                    }
                    break;
                default:
                    throw new System.IO.IOException("Cannot decode the TGA file from the stream. The image type '" + header.ImageType.ToString() + "' is not supported.");
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

            // Skip these bytes.
            if (header.IDLength > 0)
            {
                for (int i = 0; i < header.IDLength; i++)
                {
                    reader.ReadByte();
                }
            }

            return settings;
        }

		/// <summary>
		/// Function to write out the DDS header to the stream.
		/// </summary>
		/// <param name="settings">Meta data for the image header.</param>
		/// <param name="writer">Writer interface for the stream.</param>
		private void WriteHeader(IImageSettings settings, GorgonBinaryWriter writer)
		{
		}

		/// <summary>
		/// Function to perform the copying of image data into the buffer.
		/// </summary>
		/// <param name="reader">Reader interface for the stream.</param>
		/// <param name="image">Image data.</param>
		/// <param name="pitchFlags">Flags used to determine pitch when expanding pixels.</param>
		/// <param name="palette">Palette used in indexed conversion.</param>
		private void CopyImageData(GorgonBinaryReader reader, GorgonImageData image, PitchFlags pitchFlags, uint[] palette)
		{
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the data to load.</param>
		/// <returns>
		/// The image data that was in the stream.
		/// </returns>
		protected internal override GorgonImageData LoadFromStream(System.IO.Stream stream)
		{
			GorgonImageData imageData = null;
			IImageSettings settings = null;
			uint[] palette = null;

			using (var reader = new GorgonBinaryReader(stream, true))
			{
				// Read the header information.
				settings = ReadHeader(reader);

				// Create our image data structure.
				imageData = new GorgonImageData(settings);

				try
				{
					// Copy the data from the stream to the buffer.
					CopyImageData(reader, imageData, PitchFlags.None, palette);
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
			}

			return imageData;
		}

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="GorgonLibrary.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal override void SaveToStream(GorgonImageData imageData, System.IO.Stream stream)
		{
			// Use a binary writer.
			using (GorgonBinaryWriter writer = new GorgonBinaryWriter(stream, true))
			{
				// Write the header for the file.
				WriteHeader(imageData.Settings, writer);

				// Write image data.
/*				switch (imageData.Settings.ImageType)
				{
					case ImageType.Image1D:
					case ImageType.Image2D:
					case ImageType.ImageCube:
						for (int array = 0; array < imageData.Settings.ArrayCount; array++)
						{
							for (int mipLevel = 0; mipLevel < imageData.Settings.MipCount; mipLevel++)
							{
								var buffer = imageData[array, mipLevel];
								
								writer.Write(buffer.Data.UnsafePointer, buffer.PitchInformation.SlicePitch);
							}
						}
						break;
					case ImageType.Image3D:
						int depth = imageData.Settings.Depth;
						for (int mipLevel = 0; mipLevel < imageData.Settings.MipCount; mipLevel++)
						{							
							for (int slice = 0; slice < depth; slice++)
							{
								var buffer = imageData[0, mipLevel, slice];
								writer.Write(buffer.Data.UnsafePointer, buffer.PitchInformation.SlicePitch);
							}

							if (depth > 1)
							{
								depth >>= 1;
							}
						}
						break;
				}*/
			}
		}

		/// <summary>
		/// Function to read file meta data.
		/// </summary>
		/// <param name="stream">Stream used to read the metadata.</param>
		/// <returns>
		/// The image meta data as a <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> value.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
		/// <para>-or-</para>
		/// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override IImageSettings GetMetaData(System.IO.Stream stream)
		{
			using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
			{
				return this.ReadHeader(reader);
			}
		}

		/// <summary>
		/// Function to determine if this codec can read the file or not.
		/// </summary>
		/// <param name="stream">Stream used to read the file information.</param>
		/// <returns>
		/// TRUE if the codec can read the file, FALSE if not.
		/// </returns>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override bool CanBeRead(System.IO.Stream stream)
        {
            uint magicNumber = 0;
            long position = stream.Position;

            if (!stream.CanRead)
            {
                throw new System.IO.IOException("Stream is write-only.");
            }

            if (!stream.CanSeek)
            {
                throw new System.IO.IOException("The stream cannot perform seek operations.");
            }

            using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
            {
                magicNumber = reader.ReadUInt32();
            }

            stream.Position = position;
            return magicNumber == MagicNumber;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecTGA" /> class.
		/// </summary>
		internal GorgonCodecTGA()
		{
			this.CodecCommonExtensions = new string[] { "TGA" };
            _formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));
		}
		#endregion
	}
}
