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
// Created: June 27, 2016 7:53:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using DXGI = SharpDX.DXGI;
using DX = SharpDX;
using SharpDX.Direct2D1;
using WIC = SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs
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
	/// Base class for the WIC based file formats (PNG, JPG, and BMP).
	/// </summary>
	/// <remarks>
	/// <para>
	/// A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// </para>
	/// </remarks>
	public abstract class GorgonCodecWic
		: GorgonImageCodec
	{
		#region Variables.
        // Supported formats.
		private readonly DXGI.Format[] _supportedFormats =
		{
			DXGI.Format.R8G8B8A8_UNorm,
			DXGI.Format.B8G8R8A8_UNorm,
			DXGI.Format.B8G8R8X8_UNorm
		};
		#endregion

		#region Properties
		/// <summary>
		/// Property to set or return the file format that is supported by this codec.
		/// </summary>
		protected Guid SupportedFileFormat
		{
			get;
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
        /// Property to return whether the image codec supports mip maps.
        /// </summary>
	    public override bool SupportsMipMaps => false;

		/// <summary>
        /// Property to return whether the image codec supports a depth component for volume textures.
        /// </summary>
	    public override bool SupportsDepth => false;

		/// <summary>
        /// Property to return the supported pixel formats for this codec.
        /// </summary>
        public override IReadOnlyList<DXGI.Format> SupportedPixelFormats => _supportedFormats;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read multiple frames from a decoder that supports multiple frames.
		/// </summary>
		/// <param name="wic">WIC interface.</param>
		/// <param name="data">Image data to populate.</param>
		/// <param name="decoder">Decoder for the image.</param>
		private void ReadFrames(WicUtilities wic, IGorgonImage data, WIC.BitmapDecoder decoder)
		{
/*			Guid bestPixelFormat = wic.GetGUID(data.Info.Format);

			// Find the best fit pixel format.
			if (bestPixelFormat == Guid.Empty)
			{
				throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, data.Info.Format));
			}

			for (int array = 0; array < data.Info.ArrayCount; array++)
			{
				var buffer = data.Buffers[0, array];

				// Get the frame data.
				using (var frame = decoder.GetFrame(array))
				{
					IntPtr bufferPointer = new IntPtr(buffer.Data.Address);
					Guid frameFormat = frame.PixelFormat;
					int frameWidth = frame.Size.Width;
					int frameHeight = frame.Size.Height;

					// If the formats match, then we don't need to do conversion.
					if (bestPixelFormat == frameFormat)
					{
						frame.CopyPixels(buffer.PitchInformation.RowPitch, bufferPointer, buffer.PitchInformation.SlicePitch);
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

				        if (((frameWidth == data.Info.Width) && (frameHeight == data.Info.Height))
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
				                                    data.Info.Width,
				                                    data.Info.Height,
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
				                                new Rectangle(0, 0, data.Info.Width, data.Info.Height));
				            clipper.CopyPixels(buffer.PitchInformation.RowPitch,
				                                bufferPointer,
				                                buffer.PitchInformation.SlicePitch);
				        }
				    }
				}
			}*/
		}

		/// <summary>
		/// Function to read the data from a frame.
		/// </summary>
		/// <param name="wic">WIC interface.</param>
		/// <param name="data">Image data to populate.</param>
		/// <param name="srcFormat">Source image format.</param>
		/// <param name="convertFormat">Conversion format.</param>
		/// <param name="frame">Frame containing the image data.</param>
		private void ReadFrame(WicUtilities wic, IGorgonImage data, Guid srcFormat, Guid convertFormat, WIC.BitmapFrameDecode frame)
		{
/*			var buffer = data.Buffers[0];

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
			}*/
		}

		/// <summary>
		/// Function to retrieve palette information for indexed images.
		/// </summary>
		/// <param name="wic">The WIC interface.</param>
		/// <param name="bitmap">The bitmap to derive the palette from (only used when encoding).</param>
		/// <returns>A tuple containing the palette data, alpha percentage and the type of palette.  NULL if we're encoding and we want to generate the palette from the frame.</returns>
		internal virtual Tuple<WIC.Palette, double, WIC.BitmapPaletteType> GetPaletteInfo(WicUtilities wic, Bitmap bitmap)
		{
			return new Tuple<WIC.Palette, double, WIC.BitmapPaletteType>(null, 0, WIC.BitmapPaletteType.Custom);
		}

		/// <summary>
		/// Function to add custom metadata to the frame.
		/// </summary>
		/// <param name="encoder">Encoder being used to encode the image.</param>
		/// <param name="frame">Frame to encode.</param>
		/// <param name="frameIndex">Index of the current frame.</param>
		/// <param name="settings">Image data settings.</param>
		/// <param name="paletteColors">Palette colors used to encode the 8 bit images.</param>
		internal virtual void AddCustomMetaData(WIC.BitmapEncoder encoder, WIC.BitmapFrameEncode frame, int frameIndex, GorgonImageInfo settings, GorgonColor[] paletteColors)
		{
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data to read.</param>
		/// <param name="size">The size of the image within the stream, in bytes.</param>
		/// <param name="options">[Optional] Options used for decoding the image data.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the image data from the stream.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>NULL</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write only.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="EndOfStreamException">Thrown when the amount of data requested exceeds the size of the stream minus its current position.</exception>
		/// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
		public override IGorgonImage LoadFromStream(Stream stream, long size, IGorgonImageCodecDecodingOptions options = null)
		{
			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size), Resources.GORIMG_ERR_IMAGE_BYTE_LENGTH_TOO_SHORT);
			}

			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanRead)
			{
				throw new ArgumentException(Resources.GORIMG_ERR_STREAM_IS_WRITEONLY, nameof(stream));
			}

			if (stream.Position + size > stream.Length)
			{
				throw new EndOfStreamException();
			}

			var wic = new WicUtilities();

			try
			{
				IGorgonImage result = wic.DecodeImageData(stream, size, SupportedFileFormat, options as IGorgonCodecWicDecodingOptions);

				if (result == null)
				{
					throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
				}

				return result;
			}
			finally
			{
				wic.Dispose();
			}
		}

		/// <summary>
		/// Function to persist a <see cref="IGorgonImage"/> to a stream.
		/// </summary>
		/// <param name="imageData">A <see cref="IGorgonImage"/> to persist to the stream.</param>
		/// <param name="stream">The stream that will receive the image data.</param>
		/// <param name="encodingOptions">[Optional] Options used to encode the image data when it is persisted to the stream.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="imageData"/> parameter is <b>NULL</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is read only.</exception>
		/// <exception cref="GorgonException">Thrown when the image data in the stream has a pixel format that is unsupported.</exception>
		public override void SaveToStream(IGorgonImage imageData, Stream stream, IGorgonImageCodecEncodingOptions encodingOptions = null)
		{
			if (imageData == null)
			{
				throw new ArgumentNullException(nameof(imageData));
			}

			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanWrite)
			{
				throw new IOException(string.Format(Resources.GORIMG_ERR_STREAM_IS_READONLY));
			}

			var options = encodingOptions as IGorgonCodecWicEncodingOptions;

			var wic = new WicUtilities();

			try
			{
				wic.EncodeImageData(imageData, stream, SupportedFileFormat, options);
			}
			finally
			{
				wic.Dispose();
			}

			/*
			// Wrap the stream so WIC doesn't mess up the position.
			using (var wrapperStream = new GorgonStreamWrapper(stream))
			{
				using (var wic = new GorgonWICImage())
				{
					// Find a compatible format.
					Guid targetFormat = wic.GetGUID(imageData.Info.Format);

					if (targetFormat == Guid.Empty)
					{
						throw new IOException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, imageData.Info.Format));
					}

					Guid actualFormat = targetFormat;

					using (var encoder = new BitmapEncoder(wic.Factory, SupportedFileFormat))
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
							if ((imageData.Info.ArrayCount > 1) && (CodecUseAllFrames) && (encoderInfo.IsMultiframeSupported))
							{
								frameCount = imageData.Info.ArrayCount;
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
			}*/
		}

		/// <summary>
		/// Function to read the meta data for image data within a stream.
		/// </summary>
		/// <param name="stream">The stream containing the metadata to read.</param>
		/// <returns>
		/// The image meta data as a <see cref="GorgonImageInfo"/> value.
		/// </returns>
		/// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.
		/// <para>-or-</para>
		/// <para>Thrown if the file is corrupt or can't be read by the codec.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>NULL</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		/// <remarks>
		/// <para>
		/// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so 
		/// may cause undesirable results.
		/// </para> 
		/// </remarks>
		public override GorgonImageInfo GetMetaData(Stream stream) => GetMetaData(stream, null);

		/// <summary>
		/// Function to read file meta data.
		/// </summary>
		/// <param name="stream">Stream used to read the meta data.</param>
		/// <param name="options">Options used for decoding the meta data.</param>
		/// <returns>
		/// The image meta data as a <see cref="GorgonImageInfo"/> value.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>NULL</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>Thrown when the stream cannot perform seek operations.</para>
		/// </exception>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public GorgonImageInfo GetMetaData(Stream stream, IGorgonCodecWicDecodingOptions options)
		{
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

			var wic = new WicUtilities();

            try
            {
				// Get our WIC interface.				
	            GorgonImageInfo result =  wic.GetImageMetaDataFromStream(stream, SupportedFileFormat, options);

	            if (result.Format == DXGI.Format.Unknown)
	            {
		            throw new IOException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, result.Format));
	            }

	            return result;
            }
			catch(DX.SharpDXException)
			{
				throw new IOException(string.Format(Resources.GORIMG_ERR_FILE_FORMAT_NOT_CORRECT, Codec));
			}
			finally
            {
				wic.Dispose();
            }
		}

		/// <summary>
		/// Function to determine if this codec can read the image data within the stream or not.
		/// </summary>
		/// <param name="stream">The stream that is used to read the image data.</param>
		/// <returns><b>true</b> if the codec can read the file, <b>false</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>NULL</b>.</exception>
		/// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		/// <remarks>
		/// <para>
		/// When overloading this method, the implementor should remember to reset the stream position back to the original position when they are done reading the data.  Failure to do so may cause 
		/// undesirable results or an exception. 
		/// </para>
		/// </remarks>
		public override bool IsReadable(Stream stream)
		{
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

			var wic = new WicUtilities();

			try
			{
				GorgonImageInfo info = wic.GetImageMetaDataFromStream(stream, SupportedFileFormat, null);

				if (info == null)
				{
					return false;
				}

				return info.Format != DXGI.Format.Unknown;
			}
			catch(DX.SharpDXException)
			{
				return false;
			}
			finally
			{
				wic.Dispose();
			}
		}       
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWic" /> class.
		/// </summary>
		/// <param name="codec">Codec name.</param>
		/// <param name="description">Description for the codec.</param>
		/// <param name="extensions">Common extension(s) for the codec.</param>
		/// <param name="containerGUID">GUID for the container format.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is <b>NULL</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
		protected GorgonCodecWic(string codec, string description, IReadOnlyList<string> extensions, Guid containerGUID)
		{
			if (codec == null)
			{
				throw new ArgumentNullException(nameof(codec));
			}

			if (string.IsNullOrWhiteSpace(codec))
			{
				throw new ArgumentException(Resources.GORIMG_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(codec));
			}

			Codec = codec;
			CodecDescription = description ?? string.Empty;
			CodecCommonExtensions = extensions ?? new string[0];
			SupportedFileFormat = containerGUID;			
		}
		#endregion
	}
}
