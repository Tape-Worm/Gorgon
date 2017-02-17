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
// Created: June 20, 2016 11:49:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using Gorgon.Math;
using DX = SharpDX;
using WIC = SharpDX.WIC;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Imaging
{
	/// <summary>
	/// Utilities that use WIC (Windows Imaging Component) to perform image manipulation operations.
	/// </summary> 
	class WicUtilities
		: IDisposable
	{
		#region Value Types.
		/// <summary>
		/// A value to hold a WIC to Gorgon buffer format value.
		/// </summary>
		private struct WICGorgonFormat
		{
			/// <summary>
			/// WIC GUID to convert from/to.
			/// </summary>
			public readonly Guid WICGuid;
			/// <summary>
			/// Gorgon buffer format to convert from/to.
			/// </summary>
			public readonly DXGI.Format Format;

			/// <summary>
			/// Initializes a new instance of the <see cref="WICGorgonFormat" /> struct.
			/// </summary>
			/// <param name="guid">The GUID.</param>
			/// <param name="format">The format.</param>
			public WICGorgonFormat(Guid guid, DXGI.Format format)
			{
				WICGuid = guid;
				Format = format;
			}
		}

		/// <summary>
		/// A value to hold a nearest supported format conversion.
		/// </summary>
		private struct WICNearest
		{
			/// <summary>
			/// Source format to convert from.
			/// </summary>
			public readonly Guid Source;
			/// <summary>
			/// Destination format to convert to.
			/// </summary>
			public readonly Guid Destination;

			/// <summary>
			/// Initializes a new instance of the <see cref="WICNearest" /> struct.
			/// </summary>
			/// <param name="source">The source.</param>
			/// <param name="dest">The destination.</param>
			public WICNearest(Guid source, Guid dest)
			{
				Source = source;
				Destination = dest;
			}
		}
		#endregion

		#region Conversion Tables.
		// Formats for conversion between Gorgon and WIC.
		private readonly WICGorgonFormat[] _wicDXGIFormats =
		{
			new WICGorgonFormat(WIC.PixelFormat.Format128bppRGBAFloat, DXGI.Format.R32G32B32A32_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format64bppRGBAHalf, DXGI.Format.R16G16B16A16_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format64bppRGBA, DXGI.Format.R16G16B16A16_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA, DXGI.Format.R8G8B8A8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppBGRA, DXGI.Format.B8G8R8A8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppBGR, DXGI.Format.B8G8R8X8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA1010102XR,DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA1010102, DXGI.Format.R10G10B10A2_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBE, DXGI.Format.R9G9B9E5_Sharedexp),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppBGR565, DXGI.Format.B5G6R5_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppBGRA5551, DXGI.Format.B5G5R5A1_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBE, DXGI.Format.R9G9B9E5_Sharedexp),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppGrayFloat, DXGI.Format.R32_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppGrayHalf, DXGI.Format.R16_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppGray, DXGI.Format.R16_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format8bppGray, DXGI.Format.R8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format8bppAlpha, DXGI.Format.A8_UNorm)
		};

		// Best fit for supported format conversions.
		private readonly WICNearest[] _wicBestFitFormat =
		{
			new WICNearest(WIC.PixelFormat.Format1bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format2bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format4bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format8bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format2bppGray, WIC.PixelFormat.Format8bppGray),
			new WICNearest(WIC.PixelFormat.Format4bppGray, WIC.PixelFormat.Format8bppGray),
			new WICNearest(WIC.PixelFormat.Format16bppGrayFixedPoint, WIC.PixelFormat.Format16bppGrayHalf),
			new WICNearest(WIC.PixelFormat.Format32bppGrayFixedPoint, WIC.PixelFormat.Format32bppGrayFloat),
			new WICNearest(WIC.PixelFormat.Format16bppBGR555, WIC.PixelFormat.Format16bppBGRA5551),
			new WICNearest(WIC.PixelFormat.Format32bppBGR101010, WIC.PixelFormat.Format32bppRGBA1010102),
			new WICNearest(WIC.PixelFormat.Format24bppBGR, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format24bppRGB, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format32bppPBGRA, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format32bppPRGBA, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format48bppRGB, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format48bppBGR, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppBGRA, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppPRGBA, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppPBGRA, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format48bppRGBFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format48bppBGRFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppRGBAFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppBGRAFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppRGBFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppRGBHalf, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format48bppRGBHalf, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format128bppPRGBAFloat, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format128bppRGBFloat, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format128bppRGBAFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format128bppRGBFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format32bppCMYK, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppCMYK, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format40bppCMYKAlpha, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format80bppCMYKAlpha, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format96bppRGBFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat)
		};
		#endregion

		#region Variables.
		// The WIC factory.
		private readonly WIC.ImagingFactory _factory;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to find the best buffer format for a given pixel format.
		/// </summary>
		/// <param name="sourcePixelFormat">Pixel format to translate.</param>
		/// <param name="flags">Flags to apply to the pixel format conversion.</param>
		/// <param name="updatedPixelFormat">The updated pixel format after flags are applied.</param>
		/// <returns>The buffer format, or Unknown if the format couldn't be converted.</returns>
		private DXGI.Format FindBestFormat(Guid sourcePixelFormat, WICFlags flags, out Guid updatedPixelFormat)
		{
			DXGI.Format result = _wicDXGIFormats.FirstOrDefault(item => item.WICGuid == sourcePixelFormat).Format;
			updatedPixelFormat = Guid.Empty;

			if (result == DXGI.Format.Unknown)
			{
				if (sourcePixelFormat == WIC.PixelFormat.Format96bppRGBFixedPoint)
				{
					updatedPixelFormat = WIC.PixelFormat.Format128bppRGBAFloat;
					result = DXGI.Format.R32G32B32A32_Float;
				}
				else
				{
					// Find the best fit format if we couldn't find an exact match.
					for (int i = 0; i < _wicBestFitFormat.Length; i++)
					{
						if (_wicBestFitFormat[i].Source != sourcePixelFormat)
						{
							continue;
						}

						Guid bestFormat = _wicBestFitFormat[i].Destination;
						result = _wicDXGIFormats.FirstOrDefault(item => item.WICGuid == bestFormat).Format;
						
						// We couldn't find the format, bail out.
						if (result == DXGI.Format.Unknown)
						{
							return result;
						}

						updatedPixelFormat = bestFormat;
						break;
					}
				}
			}

			switch (result)
			{
				case DXGI.Format.B8G8R8A8_UNorm:
				case DXGI.Format.B8G8R8X8_UNorm:
					if ((flags & WICFlags.ForceRGB) == WICFlags.ForceRGB)
					{
						result = DXGI.Format.R8G8B8A8_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format32bppRGBA;
					}
					break;
				case DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm:
					if ((flags & WICFlags.NoX2Bias) == WICFlags.NoX2Bias)
					{
						result = DXGI.Format.R10G10B10A2_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format32bppRGBA1010102;
					}
					break;
				case DXGI.Format.B5G5R5A1_UNorm:
				case DXGI.Format.B5G6R5_UNorm:
					if ((flags & WICFlags.No16BPP) == WICFlags.No16BPP)
					{
						result = DXGI.Format.R8G8B8A8_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format32bppRGBA;
					}
					break;
				case DXGI.Format.R1_UNorm:
					if ((flags & WICFlags.AllowMono) != WICFlags.AllowMono)
					{
						result = DXGI.Format.R1_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format8bppGray;
					}
					break;
			}

			return result;
		}

		/// <summary>
		/// Function to retrieve a WIC equivalent format GUID based on the Gorgon buffer format.
		/// </summary>
		/// <param name="format">Format to look up.</param>
		/// <returns>The GUID for the format, or <b>null</b> if not found.</returns>
		private Guid GetGUID(DXGI.Format format)
		{
			for (int i = 0; i < _wicDXGIFormats.Length; i++)
			{
				if (_wicDXGIFormats[i].Format == format)
				{
					return _wicDXGIFormats[i].WICGuid;
				}
			}

			switch (format)
			{
				case DXGI.Format.R8G8B8A8_UNorm_SRgb:
					return WIC.PixelFormat.Format32bppRGBA;
				case DXGI.Format.D32_Float:
					return WIC.PixelFormat.Format32bppGrayFloat;
				case DXGI.Format.D16_UNorm:
					return WIC.PixelFormat.Format16bppGray;
				case DXGI.Format.B8G8R8A8_UNorm_SRgb:
					return WIC.PixelFormat.Format32bppBGRA;
				case DXGI.Format.B8G8R8X8_UNorm_SRgb:
					return WIC.PixelFormat.Format32bppBGR;
			}

			return Guid.Empty;
		}

		/// <summary>
		/// Function to convert a <see cref="IGorgonImageBuffer"/> into a WIC bitmap.
		/// </summary>
		/// <param name="imageData">The image data buffer to convert.</param>
		/// <param name="pixelFormat">The WIC pixel format of the data in the buffer.</param>
		/// <returns>The WIC bitmap pointing to the data stored in <paramref name="imageData"/>.</returns>
		private WIC.Bitmap GetBitmap(IGorgonImageBuffer imageData, Guid pixelFormat)
		{
			var dataRect = new DX.DataRectangle(new IntPtr(imageData.Data.Address), imageData.PitchInformation.RowPitch);
			return new WIC.Bitmap(_factory, imageData.Width, imageData.Height, pixelFormat, dataRect);
		}

		/// <summary>
		/// Function to retrieve a WIC format converter.
		/// </summary>
		/// <param name="bitmap">The WIC bitmap source to use for the converter.</param>
		/// <param name="targetFormat">The WIC format to convert to.</param>
		/// <param name="dither">The type of dithering to apply to the image data during conversion (if down sampling).</param>
		/// <param name="palette">The 8 bit palette to apply.</param>
		/// <param name="alpha8Bit">Percentage of alpha for 8 bit paletted images.</param>
		/// <returns>The WIC converter.</returns>
		private WIC.FormatConverter GetFormatConverter(WIC.BitmapSource bitmap, Guid targetFormat, ImageDithering dither, WIC.Palette palette, float alpha8Bit)
		{
			WIC.BitmapPaletteType paletteType = WIC.BitmapPaletteType.Custom;
			var result = new WIC.FormatConverter(_factory);

			if (!result.CanConvert(bitmap.PixelFormat, targetFormat))
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, "WICGuid{" + targetFormat + "}"));
			}

			if (palette != null)
			{
				// Change dithering from ordered to error diffusion when we use 8 bit palettes.
				switch (dither)
				{
					case ImageDithering.Ordered4x4:
					case ImageDithering.Ordered8x8:
					case ImageDithering.Ordered16x16:
						if (palette.TypeInfo == WIC.BitmapPaletteType.Custom)
						{
							dither = ImageDithering.ErrorDiffusion;
						}
						break;
				}

				paletteType = palette.TypeInfo;
			}

			result.Initialize(bitmap, targetFormat, (WIC.BitmapDitherType)dither, palette, alpha8Bit * 100.0 , paletteType);

			return result;
		}

		/// <summary>
		/// Function to build the WIC color contexts used to convert to and/or from sRgb pixel formats.
		/// </summary>
		/// <param name="source">The WIC bitmap source to use for sRgb conversion.</param>
		/// <param name="pixelFormat">The pixel format of the image data.</param>
		/// <param name="srcIsSRgb"><b>true</b> if the source data is already in sRgb; otherwise <b>false</b>.</param>
		/// <param name="destIsSRgb"><b>true</b> if the destination data should be converted to sRgb; otherwise <b>false</b>.</param>
		/// <param name="srcContext">The WIC color context for the source image.</param>
		/// <param name="destContext">The WIC color context for the destination image.</param>
		/// <returns>A WIC color transformation object use to convert to/from sRgb.</returns>
		private WIC.BitmapSource GetSRgbTransform(WIC.BitmapSource source, Guid pixelFormat, bool srcIsSRgb, bool destIsSRgb, out WIC.ColorContext srcContext, out WIC.ColorContext destContext)
		{
			srcContext = new WIC.ColorContext(_factory);
			destContext = new WIC.ColorContext(_factory);

			srcContext.InitializeFromExifColorSpace(srcIsSRgb ? 1 : 2);
			destContext.InitializeFromExifColorSpace(destIsSRgb ? 1 : 2);

			var result = new WIC.ColorTransform(_factory);
			result.Initialize(source, srcContext, destContext, pixelFormat);

			return result;
		}

		/// <summary>
		/// Function to assign frame encoding options to the frame based on the codec.
		/// </summary>
		/// <param name="frame">The frame that holds the options to set.</param>
		/// <param name="options">The list of options to apply.</param>
		private static void SetFrameOptions(WIC.BitmapFrameEncode frame, IGorgonWicEncodingOptions options)
		{
			if (options.Options.Contains("Interlacing"))
			{
				frame.Options.InterlaceOption = options.Options["Interlacing"].GetValue<bool>();
			}

			if (options.Options.Contains("Filter"))
			{
				frame.Options.FilterOption = options.Options["Filter"].GetValue<WIC.PngFilterOption>();
			}

			if (options.Options.Contains("ImageQuality"))
			{
				frame.Options.ImageQuality = options.Options["ImageQuality"].GetValue<float>();
			}
		}

		/// <summary>
		/// Function to retrieve palette information from an 8 bit indexed image.
		/// </summary>
		/// <param name="frame">The bitmap frame that holds the palette info.</param>
		/// <param name="options">The list of options used to override.</param>
		/// <returns>The palette information.</returns>
		private Tuple<WIC.Palette, float> GetDecoderPalette(WIC.BitmapFrameDecode frame, IGorgonWicDecodingOptions options)
		{
			// If there's no palette option on the decoder, then we do nothing.
			if ((options != null) && (!options.Options.Contains("Palette")))
			{
				return null;
			}

			if (frame == null)
			{
				return null;
			}

			IList<GorgonColor> paletteColors = options?.Options["Palette"].GetValue<IList<GorgonColor>>() ?? new GorgonColor[0];
			float alpha = options?.Options["AlphaThreshold"].GetValue<float>() ?? 0.0f;

			// If there are no colors set, then extract it from the frame.
			if (paletteColors.Count == 0)
			{
				return null;
			}

			// Generate from our custom palette.
			var dxColors = new DX.Color[paletteColors.Count];
			int size = paletteColors.Count.Min(dxColors.Length);

			for (int i = 0; i < size; i++)
			{
				GorgonColor color = paletteColors[i];

				if (color.Alpha >= alpha)
				{
					// We set the alpha to 1.0 because we only get opaque or transparent colors for 8 bit.
					dxColors[i] = new DX.Color(color.ToVector3(), 1.0f);
				}
				else
				{
					dxColors[i] = new DX.Color(0, 0, 0, 0);
				}
			}

			var wicPalette = new WIC.Palette(_factory);
			wicPalette.Initialize(dxColors);
			
			return new Tuple<WIC.Palette, float>(wicPalette, alpha);
		}

		/// <summary>
		/// Function to retrieve palette information from an 8 bit indexed image.
		/// </summary>
		/// <param name="frame">The bitmap frame that holds the palette info.</param>
		/// <param name="options">The list of options used to override.</param>
		/// <returns>The palette information.</returns>
		private Tuple<WIC.Palette, float> GetEncoderPalette(WIC.Bitmap frame, IGorgonWicEncodingOptions options)
		{
			// If there's no palette option on the decoder, then we do nothing.
			if ((options != null) && (!options.Options.Contains("Palette")))
			{
				return null;
			}

			if (frame == null)
			{
				return null;
			}

			IList<GorgonColor> paletteColors = options?.Options["Palette"].GetValue<IList<GorgonColor>>() ?? new GorgonColor[0];
			float alpha = options?.Options["AlphaThreshold"].GetValue<float>() ?? 0.0f;
			WIC.Palette wicPalette;

			// If there are no colors set, then extract it from the frame.
			if (paletteColors.Count == 0)
			{
				wicPalette = new WIC.Palette(_factory);
				wicPalette.Initialize(frame, 256, !alpha.EqualsEpsilon(0));

				return new Tuple<WIC.Palette, float>(wicPalette, alpha);
			}

			// Generate from our custom palette.
			var dxColors = new DX.Color[paletteColors.Count];
			int size = paletteColors.Count.Min(dxColors.Length);

			for (int i = 0; i < size; i++)
			{
				GorgonColor color = paletteColors[i];

				if (color.Alpha >= alpha)
				{
					// We set the alpha to 1.0 because we only get opaque or transparent colors for 8 bit.
					dxColors[i] = new DX.Color(color.ToVector3(), 1.0f);
				}
				else
				{
					dxColors[i] = new DX.Color(0, 0, 0, 0);
				}
			}

			wicPalette = new WIC.Palette(_factory);
			wicPalette.Initialize(dxColors);

			return new Tuple<WIC.Palette, float>(wicPalette, alpha);
		}

		/// <summary>
		/// Function to encode metadata into the frame.
		/// </summary>
		/// <param name="metaData">The metadata to encode.</param>
		/// <param name="encoderFrame">The frame being encoded.</param>
		private static void EncodeMetaData(IReadOnlyDictionary<string, object> metaData, WIC.BitmapFrameEncode encoderFrame)
		{
			using (WIC.MetadataQueryWriter writer = encoderFrame.MetadataQueryWriter)
			{
				foreach (KeyValuePair<string, object> item in metaData)
				{
					if (string.IsNullOrWhiteSpace(item.Key))
					{
						continue;
					}
					
					writer.SetMetadataByName(item.Key, item.Value);
				}
			}
		}

		/// <summary>
		/// Function to encode image data into a single frame for a WIC bitmap image.
		/// </summary>
		/// <param name="encoder">The WIC encoder to use.</param>
		/// <param name="imageData">The image data to encode.</param>
		/// <param name="pixelFormat">The pixel format for the image data.</param>
		/// <param name="options">Optional encoding options (depends on codec).</param>
		/// <param name="frameIndex">The current frame index.</param>
		/// <param name="metaData">Optional meta data used to encode the image data.</param>
		private void EncodeFrame(WIC.BitmapEncoder encoder, IGorgonImage imageData, Guid pixelFormat, IGorgonWicEncodingOptions options, int frameIndex, IReadOnlyDictionary<string, object> metaData)
		{
			Guid requestedFormat = pixelFormat;
			WIC.BitmapFrameEncode frame = null;
			Tuple<WIC.Palette, float> paletteInfo = null;
			WIC.Bitmap bitmap = null;

			try
			{
				IGorgonImageBuffer buffer = imageData.Buffers[0, frameIndex];

				frame = new WIC.BitmapFrameEncode(encoder);

				frame.Initialize();
				frame.SetSize(buffer.Width, buffer.Height);
				frame.SetResolution(options?.DpiX ?? 72, options?.DpiY ?? 72);
				frame.SetPixelFormat(ref pixelFormat);

				// We expect these values to be set to their defaults.  If they are not, then we will have an error.
				// These are for PNG only.
				if (options != null)
				{
					SetFrameOptions(frame, options);
				}

				if ((metaData != null) && (metaData.Count > 0))
				{
					EncodeMetaData(metaData, frame);
				}

				// If there's a disparity between what we asked for, and what we actually support, then convert to the correct format.
				if (requestedFormat != pixelFormat)
				{
					bitmap = GetBitmap(buffer, requestedFormat);

					// If we're using indexed pixel format(s), then get the palette.
					if ((pixelFormat == WIC.PixelFormat.Format8bppIndexed)
						|| (pixelFormat == WIC.PixelFormat.Format4bppIndexed)
						|| (pixelFormat == WIC.PixelFormat.Format2bppIndexed)
						|| (pixelFormat == WIC.PixelFormat.Format1bppIndexed))
					{
						paletteInfo = GetEncoderPalette(bitmap, options);
						frame.Palette = paletteInfo?.Item1;
					}

					using (WIC.BitmapSource converter = GetFormatConverter(bitmap, pixelFormat, options?.Dithering ?? ImageDithering.None, paletteInfo?.Item1, paletteInfo?.Item2 ?? 0.0f))
					{
						frame.WriteSource(converter);
					}
				}
				else
				{
					frame.WritePixels(buffer.Height, new IntPtr(buffer.Data.Address), buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
				}

				frame.Commit();
			}
			finally
			{
				paletteInfo?.Item1?.Dispose();
				bitmap?.Dispose();
				frame?.Dispose();
			}
		}

		/// <summary>
		/// Function to determine if a format can be converted to any of the requested formats.
		/// </summary>
		/// <param name="sourceFormat">The source format to convert.</param>
		/// <param name="destFormats">The destination formats to convert to.</param>
		/// <returns>A list of formats that the <paramref name="sourceFormat"/> can convert into.</returns>
		public IReadOnlyList<DXGI.Format> CanConvertFormats(DXGI.Format sourceFormat, IEnumerable<DXGI.Format> destFormats)
		{
			Guid sourceGuid = GetGUID(sourceFormat);

			if (sourceGuid == Guid.Empty)
			{
				return new DXGI.Format[0];
			}

			var result = new List<DXGI.Format>();

			using (var converter = new WIC.FormatConverter(_factory))
			{
				foreach (DXGI.Format destFormat in destFormats)
				{
					Guid destGuid;
					
					// If we've asked for B4G4R4A4, we have to convert using a manual conversion by converting to B8G8R8A8 first and then manually downsampling those pixels.
					if (destFormat == DXGI.Format.B4G4R4A4_UNorm)
					{
						destGuid = GetGUID(DXGI.Format.B8G8R8A8_UNorm);

						if ((destGuid != Guid.Empty) && ((sourceGuid == destGuid) || (converter.CanConvert(sourceGuid, destGuid))))
						{
							result.Add(destFormat);
						}
						continue;
					}

					destGuid = GetGUID(destFormat);

					if (destGuid == Guid.Empty)
					{
						continue;
					}

					if (destGuid == sourceGuid)
					{
						result.Add(destFormat);
						continue;
					}

					if (converter.CanConvert(sourceGuid, destGuid))
					{
						result.Add(destFormat);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to read metadata about an image file stored within a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image file data.</param>
		/// <param name="fileFormat">The file format of the image data.</param>
		/// <param name="options">Options used for decoding the image data.</param>
		/// <param name="frame">The WIC frame decoder used to read the image data.</param>
		/// <param name="decoder">The WIC decoder used to read the file data.</param>
		/// <param name="wicStream">The WIC stream containing the file data.</param>
		/// <param name="actualPixelFormat">The actual pixel format of the image data, used when conversion is necessary.</param>
		/// <returns>A <see cref="GorgonImageInfo"/> containing information about the image data.</returns>
		private GorgonImageInfo GetImageMetaData(Stream stream, Guid fileFormat, IGorgonWicDecodingOptions options, out WIC.BitmapFrameDecode frame, out WIC.BitmapDecoder decoder, out WIC.WICStream wicStream, out Guid actualPixelFormat)
		{
			wicStream = new WIC.WICStream(_factory, stream);

			decoder = new WIC.BitmapDecoder(_factory, fileFormat);
			decoder.Initialize(wicStream, WIC.DecodeOptions.CacheOnDemand);

			if (decoder.ContainerFormat != fileFormat)
			{
				actualPixelFormat = Guid.Empty;
				frame = null;
				return null;
			}

			frame = decoder.GetFrame(0);
			DXGI.Format format = FindBestFormat(frame.PixelFormat, options?.Flags ?? WICFlags.None, out actualPixelFormat);

			int arrayCount = 1;
			bool readAllFrames = decoder.DecoderInfo.IsMultiframeSupported;

			if ((readAllFrames) 
				&& (options != null))
			{
				readAllFrames = options.ReadAllFrames;
			}

			if (readAllFrames)
			{
				arrayCount = decoder.FrameCount.Max(1);
			}

			return new GorgonImageInfo(ImageType.Image2D, format)
			{
				Width = frame.Size.Width,
				Height = frame.Size.Height,
				ArrayCount = arrayCount,
				Depth = 1,
				MipCount = 1
			};
		}

		/// <summary>
		/// Function to encode a <see cref="IGorgonImage"/> into a known WIC file format.
		/// </summary>
		/// <param name="imageData">The image to encode.</param>
		/// <param name="imageStream">The stream that will contain the encoded data.</param>
		/// <param name="imageFileFormat">The file format to use for encoding.</param>
		/// <param name="options">The encoding options for the codec performing the encode operation.</param>
		/// <param name="metaData">Optional metadata to further describe the encoding process.</param>
		public void EncodeImageData(IGorgonImage imageData, Stream imageStream, Guid imageFileFormat, IGorgonWicEncodingOptions options, IReadOnlyDictionary<string, object> metaData)
		{
			WIC.BitmapEncoder encoder = null;
			WIC.BitmapEncoderInfo encoderInfo = null;
			int frameCount = 1;

			try
			{
				Guid pixelFormat = GetGUID(imageData.Info.Format);

				if (pixelFormat == Guid.Empty)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Info.Format));
				}

				encoder = new WIC.BitmapEncoder(_factory, imageFileFormat);
				encoder.Initialize(imageStream);

				encoderInfo = encoder.EncoderInfo;

				bool saveAllFrames = encoderInfo.IsMultiframeSupported && imageData.Info.ArrayCount > 1;

				if ((saveAllFrames) 
					&& (options != null) 
					&& (options.Options.Contains(nameof(IGorgonWicEncodingOptions.SaveAllFrames))))
				{
					saveAllFrames = options.SaveAllFrames;
				}

				if (saveAllFrames)
				{
					frameCount = imageData.Info.ArrayCount;
				}

				for (int i = 0; i < frameCount; ++i)
				{
					EncodeFrame(encoder, imageData, pixelFormat, options, i, metaData);
				}

				encoder.Commit();
			}
			finally
			{
				encoderInfo?.Dispose();
				encoder?.Dispose();
			}
		}

		/// <summary>
		/// Function to decode all frames in a multi-frame image.
		/// </summary>
		/// <param name="data">The image data that will receive the multi-frame data.</param>
		/// <param name="srcFormat">The source pixel format.</param>
		/// <param name="convertFormat">The destination pixel format.</param>
		/// <param name="decoder">The decoder used to read the image data.</param>
		/// <param name="decodingOptions">Options used in decoding the image.</param>
		/// <param name="frameOffsetMetadataItems">Names used to look up metadata describing the offset of each frame.</param>
		private void ReadAllFrames(IGorgonImage data, Guid srcFormat, Guid convertFormat, WIC.BitmapDecoder decoder, IGorgonWicDecodingOptions decodingOptions, IReadOnlyList<string> frameOffsetMetadataItems)
		{
			ImageDithering dithering = decodingOptions?.Dithering ?? ImageDithering.None;
			WIC.BitmapFrameDecode frame = null;
			WIC.FormatConverter converter = null;

			try
			{
				for (int i = 0; i < data.Info.ArrayCount; ++i)
				{
					IGorgonImageBuffer buffer = data.Buffers[0, i];

					frame?.Dispose();
					frame = decoder.GetFrame(i);
					DX.Point offset = frameOffsetMetadataItems?.Count > 0 ? GetFrameOffsetMetadataItems(frame, frameOffsetMetadataItems) : new DX.Point(0, 0);

					// Get the pointer to the buffer and adjust its offset to that of the current frame.
					IntPtr bufferPtr = new IntPtr(buffer.Data.Address) + (offset.Y * buffer.PitchInformation.RowPitch) + (offset.X * buffer.PitchInformation.RowPitch / buffer.Width);
					
					WIC.BitmapSource bitmapSource = frame;

					// Convert the format as necessary.
					if (srcFormat != convertFormat)
					{
						converter = new WIC.FormatConverter(_factory);
						converter.Initialize(frame, convertFormat, (WIC.BitmapDitherType)dithering, null, 0.0, WIC.BitmapPaletteType.Custom);
						bitmapSource = converter;
					}

					bitmapSource.CopyPixels(buffer.PitchInformation.RowPitch, bufferPtr, buffer.PitchInformation.SlicePitch);
				}
			}
			finally
			{
				converter?.Dispose();
				frame?.Dispose();
			}
		}

		/// <summary>
		/// Function to read the data from a frame.
		/// </summary>
		/// <param name="data">Image data to populate.</param>
		/// <param name="srcFormat">Source image format.</param>
		/// <param name="convertFormat">Conversion format.</param>
		/// <param name="frame">Frame containing the image data.</param>
		/// <param name="decodingOptions">Options used to decode the image data.</param>
		private void ReadFrame(IGorgonImage data, Guid srcFormat, Guid convertFormat, WIC.BitmapFrameDecode frame, IGorgonWicDecodingOptions decodingOptions)
		{
			var buffer = data.Buffers[0];

			// We don't need to convert, so just leave.
			if ((convertFormat == Guid.Empty) || (srcFormat == convertFormat))
			{
				frame.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
				return;
			}

			ImageDithering dither = decodingOptions?.Dithering ?? ImageDithering.None;
			WIC.Bitmap tempBitmap = null;
			WIC.BitmapSource formatConverter = null;
			WIC.BitmapSource sourceBitmap = frame;
			Tuple<WIC.Palette, float> paletteInfo = null;

			try
			{
				// Perform conversion.
				if ((frame.PixelFormat == WIC.PixelFormat.Format8bppIndexed)
				    || (frame.PixelFormat == WIC.PixelFormat.Format4bppIndexed)
				    || (frame.PixelFormat == WIC.PixelFormat.Format2bppIndexed)
				    || (frame.PixelFormat == WIC.PixelFormat.Format1bppIndexed))
				{
					paletteInfo = GetDecoderPalette(frame, decodingOptions);
				}

				if (paletteInfo != null)
				{ 
					// Create a temporary bitmap to convert our indexed image.
					tempBitmap = new WIC.Bitmap(_factory, frame, WIC.BitmapCreateCacheOption.NoCache)
					             {
						             Palette = paletteInfo.Item1
					             };
					formatConverter = GetFormatConverter(tempBitmap, convertFormat, ImageDithering.None, paletteInfo.Item1, paletteInfo.Item2);
				}
				else
				{
					formatConverter = GetFormatConverter(sourceBitmap, convertFormat, dither, null, 0.0f);
				}

				formatConverter.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
			}
			finally
			{
				tempBitmap?.Dispose();
				paletteInfo?.Item1?.Dispose();
				formatConverter?.Dispose();
				sourceBitmap?.Dispose();
			}
		}

		/// <summary>
		/// Function to scale the WIC bitmap data to a new size and place it into the buffer provided.
		/// </summary>
		/// <param name="bitmap">The WIC bitmap to scale.</param>
		/// <param name="buffer">The buffer that will receive the data.</param>
		/// <param name="width">The new width of the image data.</param>
		/// <param name="height">The new height of the image data.</param>
		/// <param name="filter">The filter to apply when smoothing the image during scaling.</param>
		private void ScaleBitmapData(WIC.BitmapSource bitmap, IGorgonImageBuffer buffer, int width, int height, ImageFilter filter)
		{
			using (WIC.BitmapScaler scaler = new WIC.BitmapScaler(_factory))
			{
				scaler.Initialize(bitmap, width, height, (WIC.BitmapInterpolationMode)filter);

				if (bitmap.PixelFormat == scaler.PixelFormat)
				{
					scaler.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
					return;
				}

				// There's a chance that, due the filter applied, that the format is now different. 
				// So we'll need to convert.
				using (WIC.FormatConverter converter = GetFormatConverter(scaler, bitmap.PixelFormat, ImageDithering.None, null, 0))
				{
					converter.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
				}
			}
		}

		/// <summary>
		/// Function to scale the WIC bitmap data to a new size and place it into the buffer provided.
		/// </summary>
		/// <param name="bitmap">The WIC bitmap to scale.</param>
		/// <param name="buffer">The buffer that will receive the data.</param>
		/// <param name="offsetX">The horizontal offset to start cropping at.</param>
		/// <param name="offsetY">The vertical offset to start cropping at.</param>
		/// <param name="width">The new width of the image data.</param>
		/// <param name="height">The new height of the image data.</param>
		private void CropBitmapData(WIC.BitmapSource bitmap, IGorgonImageBuffer buffer, int offsetX, int offsetY, int width, int height)
		{
			using (WIC.BitmapClipper clipper = new WIC.BitmapClipper(_factory))
			{
				DX.Rectangle rect = DX.Rectangle.Intersect(new DX.Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height),
				                                           new DX.Rectangle(offsetX, offsetY, width, height));

				if (rect.IsEmpty)
				{
					return;
				}

				// Intersect our clipping rectangle with the buffer size.
				clipper.Initialize(bitmap, rect);
				clipper.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
			}
		}

		/// <summary>
		/// Function to retrieve metadata items from a stream containing an image.
		/// </summary>
		/// <param name="frame">The frame containing the metadata.</param>
		/// <param name="metadataNames">The names of the metadata items to read.</param>
		/// <returns>The offset for the frame.</returns>
		private static DX.Point GetFrameOffsetMetadataItems(WIC.BitmapFrameDecode frame, IReadOnlyList<string> metadataNames)
		{
			using (WIC.MetadataQueryReader reader = frame.MetadataQueryReader)
			{
				object xValue;
				object yValue;

				reader.TryGetMetadataByName(metadataNames[0], out xValue);
				reader.TryGetMetadataByName(metadataNames[1], out yValue);

				if (xValue == null)
				{
					xValue = 0;
				}

				if (yValue == null)
				{
					yValue = 0;
				}

				return new DX.Point(Convert.ToInt32(xValue), Convert.ToInt32(yValue));
			}
		}

		/// <summary>
		/// Function to retrieve metadata items from a stream containing an image.
		/// </summary>
		/// <param name="stream">The stream used to read the data.</param>
		/// <param name="fileFormat">The file format to use.</param>
		/// <param name="metadataNames">The names of the metadata items to read.</param>
		/// <returns>A list of frame offsets.</returns>
		public IReadOnlyList<DX.Point> GetFrameOffsetMetadata(Stream stream, Guid fileFormat, IReadOnlyList<string> metadataNames)
		{
			long oldPosition = stream.Position;
			var wrapper = new GorgonStreamWrapper(stream, stream.Position);
			WIC.BitmapDecoder decoder = null;
			WIC.WICStream wicStream = null;
			WIC.BitmapFrameDecode frame = null;

			try
			{
				// We don't be needing this.
				wicStream = new WIC.WICStream(_factory, wrapper);

				decoder = new WIC.BitmapDecoder(_factory, fileFormat);
				decoder.Initialize(wicStream, WIC.DecodeOptions.CacheOnDemand);

				if (decoder.ContainerFormat != fileFormat)
				{
					return null;
				}

				if (!decoder.DecoderInfo.IsMultiframeSupported)
				{
					return new DX.Point[0];
				}

				var result = new DX.Point[decoder.FrameCount];

				for (int i = 0; i < result.Length; ++i)
				{
					frame?.Dispose();
					frame = decoder.GetFrame(i);

					result[i] = GetFrameOffsetMetadataItems(frame, metadataNames);
				}

				return result;
			}
			finally
			{
				stream.Position = oldPosition;

				wicStream?.Dispose();
				decoder?.Dispose();
				frame?.Dispose();
			}
		}

		/// <summary>
		/// Function to retrieve the metadata for an image from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data.</param>
		/// <param name="fileFormat">The file format of the data in the stream.</param>
		/// <param name="options">Options for image decoding.</param>
		/// <returns>The image metadata from the stream and the file format for the file in the stream.</returns>
		public GorgonImageInfo GetImageMetaDataFromStream(Stream stream, Guid fileFormat, IGorgonImageCodecDecodingOptions options)
		{
			long oldPosition = stream.Position;
			var wrapper = new GorgonStreamWrapper(stream, stream.Position);
			WIC.BitmapDecoder decoder = null;
			WIC.WICStream wicStream = null;
			WIC.BitmapFrameDecode frame = null;
			
			try
			{
				// We don't be needing this.
				Guid dummy;

				return GetImageMetaData(wrapper,
				                        fileFormat,
										options as IGorgonWicDecodingOptions, 
				                        out frame,
				                        out decoder,
				                        out wicStream,
				                        out dummy);
			}
			finally
			{
				stream.Position = oldPosition;

				wicStream?.Dispose();
				decoder?.Dispose();
				frame?.Dispose();
			}
		}

		/// <summary>
		/// Function to decode image data from a file within a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data to decode.</param>
		/// <param name="length">The size of the image data to read, in bytes.</param>
		/// <param name="imageFileFormat">The file format for the image data in the stream.</param>
		/// <param name="decodingOptions">Options used for decoding the image data.</param>
		/// <param name="frameOffsetMetadataItems">Names used to look up metadata describing the offset of each frame.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the decoded image file data.</returns>
		public IGorgonImage DecodeImageData(Stream stream, long length, Guid imageFileFormat, IGorgonWicDecodingOptions decodingOptions, IReadOnlyList<string> frameOffsetMetadataItems)
		{
			WIC.BitmapDecoder decoder = null;
			WIC.BitmapFrameDecode frame = null;
			WIC.WICStream decoderStream = null;
			IGorgonImage result = null;

			try
			{
				Guid pixelFormat;
				GorgonImageInfo info = GetImageMetaData(stream, imageFileFormat, decodingOptions, out frame, out decoder, out decoderStream, out pixelFormat);

				if (info == null)
				{
					return null;
				}

				if (info.Format == DXGI.Format.Unknown)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, info.Format));
				}

				// Build the image.
				result = new GorgonImage(info);

				// Read a single frame of data. This value will be set larger than 1 if the decoder supports multi-frame images, and the options for the codec 
				// specify that all frames are to be read (true is the default if no options are specified).
				if (info.ArrayCount > 1)
				{
					ReadAllFrames(result, frame.PixelFormat, pixelFormat, decoder, decodingOptions, frameOffsetMetadataItems);
				}
				else
				{
					ReadFrame(result, frame.PixelFormat, pixelFormat, frame, decodingOptions);
				}

				return result;
			}
			catch
			{
				result?.Dispose();
				throw;
			}
			finally
			{
				frame?.Dispose();
				decoderStream?.Dispose();
				decoder?.Dispose();
			}
		}

		/// <summary>
		/// Function to generate mip map images.
		/// </summary>
		/// <param name="destImageData">The image that will receive the mip levels.</param>
		/// <param name="filter">The filter to apply when resizing the buffers.</param>
		public void GenerateMipImages(IGorgonImage destImageData, ImageFilter filter)
		{
			Guid pixelFormat = GetGUID(destImageData.Info.Format);

			if (pixelFormat == Guid.Empty)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, destImageData.Info.Format));
			}

			WIC.Bitmap bitmap = null;

			try
			{
				// Begin scaling.
				for (int array = 0; array < destImageData.Info.ArrayCount; ++array)
				{
					int mipDepth = destImageData.Info.Depth;

					// Start at 1 because we're copying from the first mip level..
					for (int mipLevel = 1; mipLevel < destImageData.Info.MipCount; ++mipLevel)
					{
						for (int depth = 0; depth < mipDepth; ++depth)
						{
							var sourceBuffer = destImageData.Buffers[0, destImageData.Info.ImageType == ImageType.Image3D ? (destImageData.Info.Depth / mipDepth) * depth : array];
							var destBuffer = destImageData.Buffers[mipLevel, destImageData.Info.ImageType == ImageType.Image3D ? depth : array];

							bitmap = GetBitmap(sourceBuffer, pixelFormat);

							ScaleBitmapData(bitmap, destBuffer, destBuffer.Width, destBuffer.Height, filter);

							bitmap.Dispose();
						}

						// Scale the depth.
						if (mipDepth > 1)
						{
							mipDepth >>= 1;
						}
					}
				}
			}
			finally
			{
				bitmap?.Dispose();
			}
		}

		/// <summary>
		/// Function to resize an image to the specified dimensions.
		/// </summary>
		/// <param name="imageData">The image data to resize.</param>
		/// <param name="offsetX">The horizontal offset to start at for cropping (ignored when crop = false).</param>
		/// <param name="offsetY">The vertical offset to start at for cropping (ignored when crop = false).</param>
		/// <param name="newWidth">The new width for the image.</param>
		/// <param name="newHeight">The new height for the image.</param>
		/// <param name="newDepth">The new depth for the image.</param>
		/// <param name="calculatedMipLevels">The number of mip levels to support.</param>
		/// <param name="scaleFilter">The filter to apply when smoothing the image during scaling.</param>
		/// <param name="crop"><b>true</b> to crop the image, <b>false</b> to scale it.</param>
		/// <returns>A new <see cref="IGorgonImage"/> containing the resized data.</returns>
		public IGorgonImage Resize(IGorgonImage imageData, int offsetX, int offsetY, int newWidth, int newHeight, int newDepth, int calculatedMipLevels, ImageFilter scaleFilter, bool crop)
		{
			Guid pixelFormat = GetGUID(imageData.Info.Format);

			if (pixelFormat == Guid.Empty)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Info.Format));
			}

			var imageInfo = new GorgonImageInfo(imageData.Info.ImageType, imageData.Info.Format)
			                {
				                Width = newWidth,
				                Height = newHeight,
				                Depth = newDepth.Max(1),
				                ArrayCount = imageData.Info.ArrayCount,
				                MipCount = calculatedMipLevels
			                };

			var result = new GorgonImage(imageInfo);
			WIC.Bitmap bitmap = null;

			try
			{
				for (int array = 0; array < imageInfo.ArrayCount; ++array)
				{
					for (int mip = 0; mip < calculatedMipLevels.Min(imageData.Info.MipCount); ++mip)
					{
						int mipDepth = result.GetDepthCount(mip).Min(imageData.Info.Depth);

						for (int depth = 0; depth < mipDepth; ++depth)
						{
							IGorgonImageBuffer destBuffer = result.Buffers[mip, imageData.Info.ImageType == ImageType.Image3D ? depth : array];
							IGorgonImageBuffer srcBuffer = imageData.Buffers[mip, imageData.Info.ImageType == ImageType.Image3D ? depth : array];
							
							bitmap = GetBitmap(srcBuffer, pixelFormat);

							if (!crop)
							{
								ScaleBitmapData(bitmap, destBuffer, newWidth, newHeight, scaleFilter);
							}
							else
							{
								CropBitmapData(bitmap, destBuffer, offsetX, offsetY, newWidth, newHeight);
							}

							bitmap.Dispose();
							bitmap = null;
						}
					}
				}

				return result;
			}
			catch
			{
				result.Dispose();
				throw;
			}
			finally
			{
				bitmap?.Dispose();
			}
		}

		/// <summary>
		/// Function to convert from one pixel format to another.
		/// </summary>
		/// <param name="imageData">The image data to convert.</param>
		/// <param name="format">The format to convert to.</param>
		/// <param name="dithering">The type of dithering to apply if the image bit depth has to be reduced.</param>
		/// <param name="isSrcSRgb"><b>true</b> if the image data uses sRgb; otherwise <b>false</b>.</param>
		/// <param name="isDestSRgb"><b>true</b> if the resulting image data should use sRgb; otherwise <b>false</b>.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the converted image data.</returns>
		public IGorgonImage ConvertToFormat(IGorgonImage imageData, DXGI.Format format, ImageDithering dithering, bool isSrcSRgb, bool isDestSRgb)
		{
			Guid sourceFormat = GetGUID(imageData.Info.Format);
			Guid destFormat = GetGUID(format);

			// Duplicate the settings, and update the format.
			GorgonImageInfo resultInfo = new GorgonImageInfo(imageData.Info.ImageType, format)
			                             {
				                             Width = imageData.Info.Width,
				                             Height = imageData.Info.Height,
				                             ArrayCount = imageData.Info.ArrayCount,
				                             Depth = imageData.Info.Depth,
				                             MipCount = imageData.Info.MipCount
			                             };

			var result = new GorgonImage(resultInfo);

			try
			{
				for (int array = 0; array < resultInfo.ArrayCount; array++)
				{
					for (int mip = 0; mip < resultInfo.MipCount; mip++)
					{
						int depthCount = result.GetDepthCount(mip);

						for (int depth = 0; depth < depthCount; depth++)
						{
							// Get the array/mip/depth buffer.
							IGorgonImageBuffer destBuffer = result.Buffers[mip, resultInfo.ImageType == ImageType.Image3D ? depth : array];
							IGorgonImageBuffer srcBuffer = imageData.Buffers[mip, resultInfo.ImageType == ImageType.Image3D ? depth : array];
							var rect = new DX.DataRectangle(new IntPtr(srcBuffer.Data.Address), srcBuffer.PitchInformation.RowPitch);

							WIC.Bitmap bitmap = null;
							WIC.BitmapSource formatConverter = null;
							WIC.BitmapSource sRgbConverter = null;
							WIC.ColorContext srcColorContext = null;
							WIC.ColorContext destColorContext = null;

							try
							{
								// Create a WIC bitmap so we have a source for conversion.
								bitmap = new WIC.Bitmap(_factory, srcBuffer.Width, srcBuffer.Height, sourceFormat, rect, srcBuffer.PitchInformation.SlicePitch);
								WIC.BitmapSource converterSource = formatConverter = GetFormatConverter(bitmap, destFormat, dithering, null, 0);

								// If we have an sRgb conversion, then apply that after converting formats.
								if ((isSrcSRgb) || (isDestSRgb))
								{
									converterSource = sRgbConverter = GetSRgbTransform(formatConverter, destFormat, isSrcSRgb, isDestSRgb, out srcColorContext, out destColorContext);
								}

								converterSource.CopyPixels(destBuffer.PitchInformation.RowPitch, new IntPtr(destBuffer.Data.Address), destBuffer.PitchInformation.SlicePitch);
							}
							finally
							{
								srcColorContext?.Dispose();
								destColorContext?.Dispose();
								sRgbConverter?.Dispose();
								formatConverter?.Dispose();
								bitmap?.Dispose();
							}
						}
					}
				}

				return result;
			}
			catch
			{
				result.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Function to retrieve frame delays for a multi-frame image.
		/// </summary>
		/// <param name="stream">The stream containing the image.</param>
		/// <param name="decoderFormat">The format of the data being decoded</param>
		/// <param name="delayMetaDataName">The name of the metadata to look up.</param>
		/// <returns>A list of codec specific time delays.</returns>
		public int[] GetFrameDelays(Stream stream, Guid decoderFormat, string delayMetaDataName)
		{
			long oldPosition = stream.Position;
			var wrapper = new GorgonStreamWrapper(stream, stream.Position);
			WIC.BitmapDecoder decoder = null;
			WIC.WICStream wicStream = null;
			WIC.BitmapFrameDecode frame = null;
			
			try
			{
				// We don't be needing this.
				wicStream = new WIC.WICStream(_factory, wrapper);

				decoder = new WIC.BitmapDecoder(_factory, decoderFormat);
				decoder.Initialize(wicStream, WIC.DecodeOptions.CacheOnDemand);

				if (decoder.ContainerFormat != decoderFormat)
				{
					return null;
				}

				if (!decoder.DecoderInfo.IsMultiframeSupported)
				{
					return new int[0];
				}

				var result = new int[decoder.FrameCount];

				for (int i = 0; i < result.Length; ++i)
				{
					frame?.Dispose();
					frame = decoder.GetFrame(i);

					object value;

					frame.MetadataQueryReader.TryGetMetadataByName(delayMetaDataName, out value);

					if (value == null)
					{
						continue;
					}
					
					result[i] = Convert.ToInt32(value);
				}

				return result;
			}
			finally
			{
				stream.Position = oldPosition;

				wicStream?.Dispose();
				decoder?.Dispose();
				frame?.Dispose();
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_factory?.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="WicUtilities"/> class.
		/// </summary>
		public WicUtilities()
		{
			_factory = new WIC.ImagingFactory();
		}
		#endregion
	}
}
