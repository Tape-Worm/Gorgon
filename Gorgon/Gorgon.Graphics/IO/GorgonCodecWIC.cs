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
// Created: Wednesday, February 13, 2013 10:15:28 PM
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
using Gorgon.Graphics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using SharpDX;
using SharpDX.WIC;
using Bitmap = SharpDX.WIC.Bitmap;
using Color = SharpDX.Color;
using Point = System.Drawing.Point;
using Rectangle = SharpDX.Rectangle;

namespace Gorgon.IO
{
	/// <summary>
	/// Special case flags for decoding images.
	/// </summary>
	[Flags]
	public enum WICFlags
	{
		/// <summary>
		/// No special flags.
		/// </summary>
		None = 0,
		/// <summary>
		/// Loads BGR formats as R8G8B8A8_UNorm.
		/// </summary>
		ForceRGB = 0x1,
		/// <summary>
		/// Loads R10G10B10_XR_BIAS_A2_UNorm as R10G10B10A2_UNorm.
		/// </summary>
		NoX2Bias = 0x2,
		/// <summary>
		/// Loads 565, 5551, and 4444 as R8G8B8A8_UNorm.
		/// </summary>
		No16BPP = 0x4,
		/// <summary>
		/// Loads 1-bit monochrome as 8 bit grayscale.
		/// </summary>
		AllowMono = 0x8
	}

	/// <summary>
	/// Base class for the WIC based file formats (PNG, JPG, BMP, WMP, TIF).
	/// </summary>
	/// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// </remarks>
	public abstract class GorgonCodecWIC
		: GorgonImageCodec
	{
		#region Variables.

		private int _actualArrayCount;							// Array count.

        // Supported formats.
	    private readonly BufferFormat[] _supportedFormats = {
                                                                BufferFormat.R8G8B8A8_UNorm,
																BufferFormat.B8G8R8A8_UNorm,
																BufferFormat.B8G8R8X8_UNorm
	                                                        };
		#endregion

		#region Properties
		/// <summary>
		/// Property to set or return the format that is supported by this codec.
		/// </summary>
		protected Guid SupportedFormat
		{
			get;
		}

		/// <summary>
		/// Property to set or return whether all frames in a multi-frame image should be encoded/decoded or not.
		/// </summary>
		/// <remarks>This property will encode or decode multiple frames from or into an array.  Note that this is only supported on codecs that support multiple frames (e.g. animated Gif).  
		/// Images that do not support multiple frames will ignore this flag.
		/// <para>This property applies to both encoding and decoding of image data.</para>
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		protected bool CodecUseAllFrames
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the flags used for decoding the image.
		/// </summary>
		/// <remarks>This will alter the conversion process for an image when changing formats.
		/// <para>This property applied to decoding of image data only.</para>
		/// <para>The default value is None.</para>
		/// </remarks>
		public WICFlags DecodeFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. PNG).
		/// </summary>
		public override string Codec
		{
			get;
		}

		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public override string CodecDescription
		{
			get;
		}

		/// <summary>
		/// Property to return whether the image codec supports block compression.
		/// </summary>
		public override bool SupportsBlockCompression => false;

		/// <summary>
        /// Property to return whether the image codec supports image arrays.
        /// </summary>
	    public override bool SupportsArray => false;

		/// <summary>
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
	    public override bool SupportsMipMaps => false;

		/// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
	    public override bool SupportsDepth => false;

		/// <summary>
        /// Property to return the data formats for the image.
        /// </summary>
        public override IEnumerable<BufferFormat> SupportedFormats => _supportedFormats;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to read multiple frames from a decoder that supports multiple frames.
		/// </summary>
		/// <param name="wic">WIC interface.</param>
		/// <param name="data">Image data to populate.</param>
		/// <param name="decoder">Decoder for the image.</param>
		private void ReadFrames(GorgonWICImage wic, GorgonImageData data, BitmapDecoder decoder)
		{
			Guid bestPixelFormat = wic.GetGUID(data.Settings.Format);

			// Find the best fit pixel format.
			if (bestPixelFormat == Guid.Empty)
			{
				throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, data.Settings.Format));
			}

			// Use the image array as the frames.
			int arrayCount = _actualArrayCount.Min(data.Settings.ArrayCount);

			for (int array = 0; array < arrayCount; array++)
			{
				var buffer = data.Buffers[0, array];

				// Get the frame data.
				using (var frame = decoder.GetFrame(array))
				{
					IntPtr bufferPointer = new IntPtr(buffer.Data.Address);
					Guid frameFormat = frame.PixelFormat;
					int frameWidth = frame.Size.Width;
					int frameHeight = frame.Size.Height;
					var frameOffset = GetFrameOffset(frame);

					// Calculate the pointer offset if we have an offset from the frame.  Only offset if we're clipping the image though.
					if (((frameOffset.Y != 0) || (frameOffset.X != 0)) && (Clip))
					{
						bufferPointer = new IntPtr(buffer.Data.Address) + (frameOffset.Y * buffer.PitchInformation.RowPitch) + (frameOffset.X * (buffer.PitchInformation.RowPitch / buffer.Width));
					}

					// Confirm that we actually need to perform clipping.
					bool needsSizeAdjust = (frameWidth + frameOffset.X > data.Settings.Width) || (frameHeight + frameOffset.Y > data.Settings.Height);

					// If the formats match, then we don't need to do conversion.
					if (bestPixelFormat == frameFormat)
					{
						// If the width and height are the same then we can just do a straight copy into the buffer.
						if (((frameWidth == data.Settings.Width) && (frameHeight == data.Settings.Height)) || ((!needsSizeAdjust) && (Clip)))
						{
							frame.CopyPixels(buffer.PitchInformation.RowPitch, bufferPointer, buffer.PitchInformation.SlicePitch);

						    continue;
						}

						// We need to scale the image up/down to the size of our image data.
						if (!Clip)
						{
							using (var scaler = new BitmapScaler(wic.Factory))
							{
								scaler.Initialize(frame, data.Settings.Width, data.Settings.Height, (BitmapInterpolationMode)Filter);
								scaler.CopyPixels(buffer.PitchInformation.RowPitch, bufferPointer, buffer.PitchInformation.SlicePitch);
							}
						    continue;
						}

						using (var clipper = new BitmapClipper(wic.Factory))
						{
							clipper.Initialize(frame, new Rectangle(0, 0, data.Settings.Width, data.Settings.Height));
							clipper.CopyPixels(buffer.PitchInformation.RowPitch, bufferPointer, buffer.PitchInformation.SlicePitch);
						}

					    continue;
					}

				    // Poop.  We need to convert this image.
				    using (var converter = new FormatConverter(wic.Factory))
                    {
				        converter.Initialize(frame,
				                                bestPixelFormat,
				                                (BitmapDitherType)Dithering,
				                                null,
				                                0.0,
				                                BitmapPaletteType.Custom);

				        if (((frameWidth == data.Settings.Width) && (frameHeight == data.Settings.Height))
				            || ((!needsSizeAdjust) && (Clip)))
				        {
				            converter.CopyPixels(buffer.PitchInformation.RowPitch,
				                                 bufferPointer,
				                                 buffer.PitchInformation.SlicePitch);
				            continue;
				        }

				        // And we need to scale the image.
				        if (!Clip)
				        {
				            using(var scaler = new BitmapScaler(wic.Factory))
				            {
				                scaler.Initialize(converter,
				                                    data.Settings.Width,
				                                    data.Settings.Height,
				                                    (BitmapInterpolationMode)Filter);
				                scaler.CopyPixels(buffer.PitchInformation.RowPitch,
				                                    bufferPointer,
				                                    buffer.PitchInformation.SlicePitch);
				            }

				            continue;
				        }

				        using(var clipper = new BitmapClipper(wic.Factory))
				        {
				            clipper.Initialize(frame,
				                                new Rectangle(0, 0, data.Settings.Width, data.Settings.Height));
				            clipper.CopyPixels(buffer.PitchInformation.RowPitch,
				                                bufferPointer,
				                                buffer.PitchInformation.SlicePitch);
				        }
				    }
				}
			}
		}

		/// <summary>
		/// Function to read the data from a frame.
		/// </summary>
		/// <param name="wic">WIC interface.</param>
		/// <param name="data">Image data to populate.</param>
		/// <param name="srcFormat">Source image format.</param>
		/// <param name="convertFormat">Conversion format.</param>
		/// <param name="frame">Frame containing the image data.</param>
		private void ReadFrame(GorgonWICImage wic, GorgonImageData data, Guid srcFormat, Guid convertFormat, BitmapFrameDecode frame)
		{
			var buffer = data.Buffers[0];

			// We don't need to convert, so just leave.
			if ((convertFormat == Guid.Empty) || (srcFormat == convertFormat))
			{
				frame.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
				return;
			}

			// Perform conversion.
			using (var converter = new FormatConverter(wic.Factory))
			{
				bool isIndexed = ((frame.PixelFormat == PixelFormat.Format8bppIndexed)
							|| (frame.PixelFormat == PixelFormat.Format4bppIndexed)
							|| (frame.PixelFormat == PixelFormat.Format2bppIndexed)
							|| (frame.PixelFormat == PixelFormat.Format1bppIndexed));
				Tuple<Palette, double, BitmapPaletteType> paletteInfo = null;

				try
				{
					// If the pixel format is indexed, then retrieve a palette.
					if (isIndexed)
					{
						paletteInfo = GetPaletteInfo(wic, null);
					}

					// If we've defined a palette for an indexed image, then copy it to a bitmap and set its palette.
					if (paletteInfo?.Item1 != null)
					{
						using (var tempBitmap = new Bitmap(wic.Factory, frame, BitmapCreateCacheOption.CacheOnDemand))
						{
							tempBitmap.Palette = paletteInfo.Item1;
							converter.Initialize(tempBitmap, convertFormat, (BitmapDitherType)Dithering, paletteInfo.Item1, paletteInfo.Item2, paletteInfo.Item3);
							converter.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
						}

						return;
					}

					// Only apply palettes to indexed image data.
					converter.Initialize(frame, convertFormat, (BitmapDitherType)Dithering, null, 0.0, BitmapPaletteType.Custom);
					converter.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
				}
				finally
				{
					paletteInfo?.Item1?.Dispose();
				}
			}
		}

        /// <summary>
        /// Function to retrieve meta data from the image.
        /// </summary>
        /// <param name="wic">WIC interface.</param>
        /// <param name="decoder">Decoder for the image.</param>
        /// <param name="frame">Frame in the image to decode.</param>
        /// <param name="bestFormatMatch">The best match for the pixel format.</param>
        /// <returns>Settings for the new image.</returns>
        internal IImageSettings ReadMetaData(GorgonWICImage wic, BitmapDecoder decoder, BitmapFrameDecode frame, out Guid bestFormatMatch)
        {
	        return new GorgonTexture2DSettings
	        {
                Width = frame.Size.Width,
                Height = frame.Size.Height,
                MipCount = 1,
                ArrayCount = (CodecUseAllFrames && SupportsMultipleFrames) ? decoder.FrameCount : 1,
                Format = wic.FindBestFormat(frame.PixelFormat, DecodeFlags, out bestFormatMatch)
            };
        }

		/// <summary>
		/// Function to retrieve the offset for the frame being decoded.
		/// </summary>
		/// <param name="frame">Frame to decode.</param>
		/// <returns>The position of the offset.</returns>
		internal virtual Point GetFrameOffset(BitmapFrameDecode frame)
		{
			return Point.Empty;
		}

		/// <summary>
		/// Function to retrieve palette information for indexed images.
		/// </summary>
		/// <param name="wic">The WIC interface.</param>
		/// <param name="bitmap">The bitmap to derive the palette from (only used when encoding).</param>
		/// <returns>A tuple containing the palette data, alpha percentage and the type of palette.  NULL if we're encoding and we want to generate the palette from the frame.</returns>
		internal virtual Tuple<Palette, double, BitmapPaletteType> GetPaletteInfo(GorgonWICImage wic, Bitmap bitmap)
		{
			return new Tuple<Palette, double, BitmapPaletteType>(null, 0, BitmapPaletteType.Custom);
		}

		/// <summary>
		/// Function to add custom metadata to the frame.
		/// </summary>
		/// <param name="encoder">Encoder being used to encode the image.</param>
		/// <param name="frame">Frame to encode.</param>
		/// <param name="frameIndex">Index of the current frame.</param>
		/// <param name="settings">Image data settings.</param>
		/// <param name="paletteColors">Palette colors used to encode the 8 bit images.</param>
		internal virtual void AddCustomMetaData(BitmapEncoder encoder, BitmapFrameEncode frame, int frameIndex, IImageSettings settings, Color[] paletteColors)
		{
		}

        /// <summary>
        /// Function to set custom encoding options.
        /// </summary>
        /// <param name="frame">Frame encoder to use.</param>
        internal virtual void SetFrameOptions(BitmapFrameEncode frame)
        {
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
			GorgonImageData result = null;

		    // Get our WIC interface.
		    var wrapperStream = new GorgonStreamWrapper(stream);

			using (var wic = new GorgonWICImage())
			{
				using (var decoder = new BitmapDecoder(wic.Factory, SupportedFormat))
				{
					using (var wicStream = new WICStream(wic.Factory, wrapperStream))
					{
						try
						{
							decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);
						}
						catch (SharpDXException)
						{
							// Repackage this exception to keep in line with our API.
							throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, Codec));
						}

						using (var frame = decoder.GetFrame(0))
						{
						    Guid bestFormat;
						    var settings = ReadMetaData(wic, decoder, frame, out bestFormat);

							if (settings.Format == BufferFormat.Unknown)
							{
								throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, settings.Format));
							}

							// Create our image data.
							try
							{
								_actualArrayCount = settings.ArrayCount;
								if (ArrayCount > 0)
								{
									settings.ArrayCount = ArrayCount;
								}

								result = new GorgonImageData(settings);

								if ((settings.ArrayCount > 1) && (_actualArrayCount > 1))
								{
									ReadFrames(wic, result, decoder);
								}
								else
								{
									ReadFrame(wic, result, frame.PixelFormat, bestFormat, frame);
								}

								// If we've not read the full length of the data (WIC seems to ignore the CRC on the IEND chunk for PNG files for example),
								// then we need to move the pointer up by however many bytes we've missed.
								if (wrapperStream.Position < size)
								{
									wrapperStream.Position = size;
								}
							}
							catch
							{
								// If we run into a problem, dump the memory buffer.
								result?.Dispose();

								throw;
							}
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="Gorgon.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal override void SaveToStream(GorgonImageData imageData, Stream stream)
		{
			int frameCount = 1;

			// Wrap the stream so WIC doesn't mess up the position.
			using (var wrapperStream = new GorgonStreamWrapper(stream))
			{
				using (var wic = new GorgonWICImage())
				{
					// Find a compatible format.
					Guid targetFormat = wic.GetGUID(imageData.Settings.Format);

					if (targetFormat == Guid.Empty)
					{
						throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, imageData.Settings.Format));
					}

					Guid actualFormat = targetFormat;

					using (var encoder = new BitmapEncoder(wic.Factory, SupportedFormat))
					{
						try
						{
							encoder.Initialize(wrapperStream);
							AddCustomMetaData(encoder, null, 0, imageData.Settings, null);
						}
						catch (SharpDXException)
						{
							// Repackage this exception to keep in line with our API.
							throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_ENCODER, Codec));
						}

						using (var encoderInfo = encoder.EncoderInfo)
						{
							if ((imageData.Settings.ArrayCount > 1) && (CodecUseAllFrames) && (encoderInfo.IsMultiframeSupported))
							{
								frameCount = imageData.Settings.ArrayCount;
							}							

							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
							{
								using (var frame = new BitmapFrameEncode(encoder))
								{
									var buffer = imageData.Buffers[0, frameIndex];

									frame.Initialize();
									frame.SetSize(buffer.Width, buffer.Height);
									frame.SetResolution(72, 72);
									frame.SetPixelFormat(ref actualFormat);

                                    SetFrameOptions(frame);

									// If the image encoder doesn't like the format we've chosen, then we'll need to convert to 
									// the best format for the codec.
									if (targetFormat != actualFormat)
									{
										var rect = new DataRectangle(new IntPtr(buffer.Data.Address), buffer.PitchInformation.RowPitch);
										using (var bitmap = new Bitmap(wic.Factory, buffer.Width, buffer.Height, targetFormat, rect))
										{
											// If we're using a codec that supports 8 bit indexed data, then get the palette info.									
											var paletteInfo = GetPaletteInfo(wic, bitmap);

											if (paletteInfo == null)
											{
												throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_ENCODER, Codec));
											}

											try
											{
												using (var converter = new FormatConverter(wic.Factory))
												{
													converter.Initialize(bitmap, actualFormat, (BitmapDitherType)Dithering, paletteInfo.Item1, paletteInfo.Item2, paletteInfo.Item3);
													if (paletteInfo.Item1 != null)
													{
														frame.Palette = paletteInfo.Item1;
													}

													AddCustomMetaData(encoder, frame, frameIndex, imageData.Settings, paletteInfo.Item1?.GetColors<Color>());
													frame.WriteSource(converter);													
												}
											}
											finally
											{
												paletteInfo.Item1?.Dispose();
											}
										}
									}
									else
									{
										// No conversion was needed, just dump as-is.										
										AddCustomMetaData(encoder, frame, frameIndex, imageData.Settings, null);
										frame.WritePixels(buffer.Height, new IntPtr(buffer.Data.Address), buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
									}									

									frame.Commit();
								}
							}
						}

						encoder.Commit();
					}
				}
			}
		}

		/// <summary>
		/// Function to read file meta data.
		/// </summary>
		/// <param name="stream">Stream used to read the meta data.</param>
		/// <returns>
		/// The image meta data as a <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> value.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>Thrown when the stream cannot perform seek operations.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override IImageSettings GetMetaData(Stream stream)
		{
		    if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_STREAM_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_STREAM_NO_SEEK);
            }

            long position = stream.Position;

            try
            {
				// Wrap the stream so WIC doesn't mess up the position.
                var wrapperStream = new GorgonStreamWrapper(stream);

				// Get our WIC interface.				
				using (var wic = new GorgonWICImage())
				{
					using (var decoder = new BitmapDecoder(wic.Factory, SupportedFormat))
					{
						using (var wicStream = new WICStream(wic.Factory, wrapperStream))
						{
							try
							{
								decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);
							}
							catch (SharpDXException)
							{
								throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_ENCODER, Codec));
							}

							using (var frame = decoder.GetFrame(0))
							{
							    Guid bestFormat;
							    var settings = ReadMetaData(wic, decoder, frame, out bestFormat);

								if (settings.Format == BufferFormat.Unknown)
								{
									throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, settings.Format));
								}

								return settings;
							}
						}
					}
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
		/// <b>true</b> if the codec can read the file, <b>false</b> if not.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>Thrown when the stream cannot perform seek operations.</para>
		/// </exception>		
		public override bool IsReadable(Stream stream)
		{
		    if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_STREAM_READ_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_STREAM_EOF);
            }

			long position = stream.Position;

			try
			{
				// Get our WIC interface.
				// Wrap the stream so WIC doesn't mess up the position.
			    var wrapperStream = new GorgonStreamWrapper(stream);

				using (var wic = new GorgonWICImage())
				{
					using (var decoder = new BitmapDecoder(wic.Factory, SupportedFormat))
					{
						using (var wicStream = new WICStream(wic.Factory, wrapperStream))
						{
							try
							{
								decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);
							}
							catch (SharpDXException)
							{
								return false;
							}

							// Only load supported WIC formats.
							if (SupportedFormat != decoder.ContainerFormat)
							{
								return false;
							}

							using (var frame = decoder.GetFrame(0))
							{
							    Guid bestFormat;
							    var settings = ReadMetaData(wic, decoder, frame, out bestFormat);

								return (settings.Format != BufferFormat.Unknown);
							}
						}
					}
				}
			}
			finally
			{
				stream.Position = position;
			}
		}       
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWIC" /> class.
		/// </summary>
		/// <param name="codec">Codec name.</param>
		/// <param name="description">Description for the codec.</param>
		/// <param name="extensions">Common extension(s) for the codec.</param>
		/// <param name="containerGUID">GUID for the container format.</param>
		internal GorgonCodecWIC(string codec, string description, IEnumerable<string> extensions, Guid containerGUID)
		{
			DecodeFlags = WICFlags.None;
			CodecUseAllFrames = true;
			Codec = codec;
			CodecDescription = description;
			CodecCommonExtensions = extensions;
			SupportedFormat = containerGUID;			
		}
		#endregion
	}
}
