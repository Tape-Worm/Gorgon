
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 20, 2016 11:49:00 PM
// 


using Gorgon.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.WIC;
using DX = SharpDX;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// The type of resize to perform
/// </summary>
enum ResizeMode
{
    /// <summary>
    /// Scale the image.
    /// </summary>
    Scale = 0,
    /// <summary>
    /// Crop the image data.
    /// </summary>
    Crop = 1,
    /// <summary>
    /// Expand the image data.
    /// </summary>
    Expand = 2
}

/// <summary>
/// Utilities that use WIC (Windows Imaging Component) to perform image manipulation operations
/// </summary> 
class WicUtilities
    : IDisposable
{

    // Encoding option for interlacing.
    private const string EncOptInterlacing = "Interlacing";
    // Encoding option for filtering.
    private const string EncOptFilter = "Filter";
    // Encoding option for image quality.
    private const string EncOptImageQuality = "ImageQuality";
    // Decoding option for palette.
    private const string DecOptPalette = "Palette";
    // Decoding option for the alpha threshold.
    private const string DecOptAlphaThreshold = "AlphaThreshold";



    /// <summary>
    /// A value to hold a WIC to Gorgon buffer format value.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="WICGorgonFormat" /> struct.
    /// </remarks>
    /// <param name="guid">The GUID.</param>
    /// <param name="format">The format.</param>
    private readonly struct WICGorgonFormat(Guid guid, BufferFormat format)
    {
        /// <summary>
        /// WIC GUID to convert from/to.
        /// </summary>
        public readonly Guid WICGuid = guid;
        /// <summary>
        /// Gorgon buffer format to convert from/to.
        /// </summary>
        public readonly BufferFormat Format = format;
    }

    /// <summary>
    /// A value to hold a nearest supported format conversion.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="WICNearest" /> struct.
    /// </remarks>
    /// <param name="source">The source.</param>
    /// <param name="dest">The destination.</param>
    private readonly struct WICNearest(Guid source, Guid dest)
    {
        /// <summary>
        /// Source format to convert from.
        /// </summary>
        public readonly Guid Source = source;
        /// <summary>
        /// Destination format to convert to.
        /// </summary>
        public readonly Guid Destination = dest;
    }

    // Formats for conversion between Gorgon and WIC.
    private readonly WICGorgonFormat[] _wicDXGIFormats =
    [
        new(PixelFormat.Format128bppRGBAFloat, BufferFormat.R32G32B32A32_Float),
        new(PixelFormat.Format64bppRGBAHalf, BufferFormat.R16G16B16A16_Float),
        new(PixelFormat.Format64bppRGBA, BufferFormat.R16G16B16A16_UNorm),
        new(PixelFormat.Format32bppRGBA, BufferFormat.R8G8B8A8_UNorm),
        new(PixelFormat.Format32bppBGRA, BufferFormat.B8G8R8A8_UNorm),
        new(PixelFormat.Format32bppBGR, BufferFormat.B8G8R8X8_UNorm),
        new(PixelFormat.Format32bppRGBA1010102XR,BufferFormat.R10G10B10_Xr_Bias_A2_UNorm),
        new(PixelFormat.Format32bppRGBA1010102, BufferFormat.R10G10B10A2_UNorm),
        new(PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_Sharedexp),
        new(PixelFormat.Format16bppBGR565, BufferFormat.B5G6R5_UNorm),
        new(PixelFormat.Format16bppBGRA5551, BufferFormat.B5G5R5A1_UNorm),
        new(PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_Sharedexp),
        new(PixelFormat.Format32bppGrayFloat, BufferFormat.R32_Float),
        new(PixelFormat.Format16bppGrayHalf, BufferFormat.R16_Float),
        new(PixelFormat.Format16bppGray, BufferFormat.R16_UNorm),
        new(PixelFormat.Format8bppGray, BufferFormat.R8_UNorm),
        new(PixelFormat.Format8bppAlpha, BufferFormat.A8_UNorm)
    ];

    // Best fit for supported format conversions.
    private readonly WICNearest[] _wicBestFitFormat =
    [
        new(PixelFormat.Format1bppIndexed, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format2bppIndexed, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format4bppIndexed, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format8bppIndexed, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format2bppGray, PixelFormat.Format8bppGray),
        new(PixelFormat.Format4bppGray, PixelFormat.Format8bppGray),
        new(PixelFormat.Format16bppGrayFixedPoint, PixelFormat.Format16bppGrayHalf),
        new(PixelFormat.Format32bppGrayFixedPoint, PixelFormat.Format32bppGrayFloat),
        new(PixelFormat.Format16bppBGR555, PixelFormat.Format16bppBGRA5551),
        new(PixelFormat.Format32bppBGR101010, PixelFormat.Format32bppRGBA1010102),
        new(PixelFormat.Format24bppBGR, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format24bppRGB, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format32bppPBGRA, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format32bppPRGBA, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format48bppRGB, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format48bppBGR, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format64bppBGRA, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format64bppPRGBA, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format64bppPBGRA, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format48bppRGBFixedPoint, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format48bppBGRFixedPoint, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format64bppRGBAFixedPoint, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format64bppBGRAFixedPoint, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format64bppRGBFixedPoint, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format64bppRGBHalf, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format48bppRGBHalf, PixelFormat.Format64bppRGBAHalf),
        new(PixelFormat.Format128bppPRGBAFloat, PixelFormat.Format128bppRGBAFloat),
        new(PixelFormat.Format128bppRGBFloat, PixelFormat.Format128bppRGBAFloat),
        new(PixelFormat.Format128bppRGBAFixedPoint, PixelFormat.Format128bppRGBAFloat),
        new(PixelFormat.Format128bppRGBFixedPoint, PixelFormat.Format128bppRGBAFloat),
        new(PixelFormat.Format32bppCMYK, PixelFormat.Format32bppRGBA),
        new(PixelFormat.Format64bppCMYK, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format40bppCMYKAlpha, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format80bppCMYKAlpha, PixelFormat.Format64bppRGBA),
        new(PixelFormat.Format96bppRGBFixedPoint, PixelFormat.Format128bppRGBAFloat)
    ];



    // The WIC factory.
    private readonly ImagingFactory _factory;



    /// <summary>
    /// Function to find the best buffer format for a given pixel format.
    /// </summary>
    /// <param name="sourcePixelFormat">Pixel format to translate.</param>
    /// <param name="flags">Flags to apply to the pixel format conversion.</param>
    /// <param name="updatedPixelFormat">The updated pixel format after flags are applied.</param>
    /// <returns>The buffer format, or Unknown if the format couldn't be converted.</returns>
    private BufferFormat FindBestFormat(Guid sourcePixelFormat, WICFlags flags, out Guid updatedPixelFormat)
    {
        BufferFormat result = _wicDXGIFormats.FirstOrDefault(item => item.WICGuid == sourcePixelFormat).Format;
        updatedPixelFormat = Guid.Empty;

        if (result == BufferFormat.Unknown)
        {
            if (sourcePixelFormat == PixelFormat.Format96bppRGBFixedPoint)
            {
                updatedPixelFormat = PixelFormat.Format128bppRGBAFloat;
                result = BufferFormat.R32G32B32A32_Float;
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
                    if (result == BufferFormat.Unknown)
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
            case BufferFormat.B8G8R8A8_UNorm:
            case BufferFormat.B8G8R8X8_UNorm:
                if ((flags & WICFlags.ForceRGB) == WICFlags.ForceRGB)
                {
                    result = BufferFormat.R8G8B8A8_UNorm;
                    updatedPixelFormat = PixelFormat.Format32bppRGBA;
                }
                break;
            case BufferFormat.R10G10B10_Xr_Bias_A2_UNorm:
                if ((flags & WICFlags.NoX2Bias) == WICFlags.NoX2Bias)
                {
                    result = BufferFormat.R10G10B10A2_UNorm;
                    updatedPixelFormat = PixelFormat.Format32bppRGBA1010102;
                }
                break;
            case BufferFormat.B5G5R5A1_UNorm:
            case BufferFormat.B5G6R5_UNorm:
                if ((flags & WICFlags.No16BPP) == WICFlags.No16BPP)
                {
                    result = BufferFormat.R8G8B8A8_UNorm;
                    updatedPixelFormat = PixelFormat.Format32bppRGBA;
                }
                break;
            case BufferFormat.R1_UNorm:
                if ((flags & WICFlags.AllowMono) != WICFlags.AllowMono)
                {
                    result = BufferFormat.R1_UNorm;
                    updatedPixelFormat = PixelFormat.Format8bppGray;
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
    private Guid GetGUID(BufferFormat format)
    {
        for (int i = 0; i < _wicDXGIFormats.Length; i++)
        {
            if (_wicDXGIFormats[i].Format == format)
            {
                return _wicDXGIFormats[i].WICGuid;
            }
        }

        return format switch
        {
            BufferFormat.R8G8B8A8_UNorm_SRgb => PixelFormat.Format32bppRGBA,
            BufferFormat.D32_Float => PixelFormat.Format32bppGrayFloat,
            BufferFormat.D16_UNorm => PixelFormat.Format16bppGray,
            BufferFormat.B8G8R8A8_UNorm_SRgb => PixelFormat.Format32bppBGRA,
            BufferFormat.B8G8R8X8_UNorm_SRgb => PixelFormat.Format32bppBGR,
            _ => Guid.Empty,
        };
    }

    /// <summary>
    /// Function to convert a <see cref="IGorgonImageBuffer"/> into a WIC bitmap.
    /// </summary>
    /// <param name="imageData">The image data buffer to convert.</param>
    /// <param name="pixelFormat">The WIC pixel format of the data in the buffer.</param>
    /// <returns>The WIC bitmap pointing to the data stored in <paramref name="imageData"/>.</returns>
    private Bitmap GetBitmap(IGorgonImageBuffer imageData, Guid pixelFormat)
    {
        var dataRect = new DX.DataRectangle(imageData.Data, imageData.PitchInformation.RowPitch);
        return new Bitmap(_factory, imageData.Width, imageData.Height, pixelFormat, dataRect);
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
    private FormatConverter GetFormatConverter(BitmapSource bitmap, Guid targetFormat, ImageDithering dither, Palette palette, float alpha8Bit)
    {
        BitmapPaletteType paletteType = BitmapPaletteType.Custom;
        var result = new FormatConverter(_factory);

        if (!result.CanConvert(bitmap.PixelFormat, targetFormat))
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, "WICGuid{" + targetFormat + "}"));
        }

        if (palette is not null)
        {
            // Change dithering from ordered to error diffusion when we use 8 bit palettes.
            switch (dither)
            {
                case ImageDithering.Ordered4x4:
                case ImageDithering.Ordered8x8:
                case ImageDithering.Ordered16x16:
                    if (palette.TypeInfo == BitmapPaletteType.Custom)
                    {
                        dither = ImageDithering.ErrorDiffusion;
                    }
                    break;
            }

            paletteType = palette.TypeInfo;
        }

        result.Initialize(bitmap, targetFormat, (BitmapDitherType)dither, palette, alpha8Bit * 100.0, paletteType);

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
    private BitmapSource GetSRgbTransform(BitmapSource source, Guid pixelFormat, bool srcIsSRgb, bool destIsSRgb, out ColorContext srcContext, out ColorContext destContext)
    {
        srcContext = new ColorContext(_factory);
        destContext = new ColorContext(_factory);

        srcContext.InitializeFromExifColorSpace(srcIsSRgb ? 1 : 2);
        destContext.InitializeFromExifColorSpace(destIsSRgb ? 1 : 2);

        var result = new ColorTransform(_factory);
        result.Initialize(source, srcContext, destContext, pixelFormat);

        return result;
    }

    /// <summary>
    /// Function to assign frame encoding options to the frame based on the codec.
    /// </summary>
    /// <param name="frame">The frame that holds the options to set.</param>
    /// <param name="options">The list of options to apply.</param>
    private static void SetFrameOptions(BitmapFrameEncode frame, IGorgonWicEncodingOptions options)
    {
        if (options.Options.Contains(EncOptInterlacing))
        {
            frame.Options.InterlaceOption = options.Options[EncOptInterlacing].GetValue<bool>();
        }

        if (options.Options.Contains(EncOptFilter))
        {
            frame.Options.FilterOption = options.Options[EncOptFilter].GetValue<PngFilterOption>();
        }

        if (options.Options.Contains(EncOptImageQuality))
        {
            frame.Options.ImageQuality = options.Options[EncOptImageQuality].GetValue<float>();
        }
    }

    /// <summary>
    /// Function to retrieve palette information from an 8 bit indexed image.
    /// </summary>
    /// <param name="frame">The bitmap frame that holds the palette info.</param>
    /// <param name="options">The list of options used to override.</param>
    /// <returns>The palette information.</returns>
    private (Palette Palette, float Alpha)? GetDecoderPalette(BitmapFrameDecode frame, IGorgonWicDecodingOptions options)
    {
        // If there's no palette option on the decoder, then we do nothing.
        if ((options is not null) && (!options.Options.Contains(DecOptPalette)))
        {
            return null;
        }

        if (frame is null)
        {
            return null;
        }

        IList<GorgonColor> paletteColors = options?.Options[DecOptPalette].GetValue<IList<GorgonColor>>() ?? [];
        float alpha = options?.Options[DecOptAlphaThreshold].GetValue<float>() ?? 0.0f;

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
                dxColors[i] = new GorgonColor(color, 1.0f);
            }
            else
            {
                dxColors[i] = GorgonColor.BlackTransparent;
            }
        }

        var wicPalette = new Palette(_factory);
        wicPalette.Initialize(dxColors);

        return (wicPalette, alpha);
    }

    /// <summary>
    /// Function to retrieve palette information from an 8 bit indexed image.
    /// </summary>
    /// <param name="frame">The bitmap frame that holds the palette info.</param>
    /// <param name="options">The list of options used to override.</param>
    /// <returns>The palette information.</returns>
    private (Palette Palette, float Alpha)? GetEncoderPalette(Bitmap frame, IGorgonWicEncodingOptions options)
    {
        // If there's no palette option on the decoder, then we do nothing.
        if ((options is not null) && (!options.Options.Contains(DecOptPalette)))
        {
            return null;
        }

        if (frame is null)
        {
            return null;
        }

        IList<GorgonColor> paletteColors = options?.Options[DecOptPalette].GetValue<IList<GorgonColor>>() ?? [];
        float alpha = options?.Options[DecOptAlphaThreshold].GetValue<float>() ?? 0.0f;
        Palette wicPalette;

        // If there are no colors set, then extract it from the frame.
        if (paletteColors.Count == 0)
        {
            wicPalette = new Palette(_factory);
            wicPalette.Initialize(frame, 256, !alpha.EqualsEpsilon(0));

            return (wicPalette, alpha);
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
                dxColors[i] = new GorgonColor(color, 1.0f);
            }
            else
            {
                dxColors[i] = GorgonColor.BlackTransparent;
            }
        }

        wicPalette = new Palette(_factory);
        wicPalette.Initialize(dxColors);

        return (wicPalette, alpha);
    }

    /// <summary>
    /// Function to encode metadata into the frame.
    /// </summary>
    /// <param name="metaData">The metadata to encode.</param>
    /// <param name="encoderFrame">The frame being encoded.</param>
    private static void EncodeMetaData(IReadOnlyDictionary<string, object> metaData, BitmapFrameEncode encoderFrame)
    {
        using MetadataQueryWriter writer = encoderFrame.MetadataQueryWriter;
        foreach (KeyValuePair<string, object> item in metaData)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                continue;
            }

            writer.SetMetadataByName(item.Key, item.Value);
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
    private void EncodeFrame(BitmapEncoder encoder, IGorgonImage imageData, Guid pixelFormat, IGorgonWicEncodingOptions options, int frameIndex, IReadOnlyDictionary<string, object> metaData)
    {
        Guid requestedFormat = pixelFormat;
        BitmapFrameEncode frame = null;
        (Palette Palette, float Alpha)? paletteInfo = null;
        Bitmap bitmap = null;

        try
        {
            IGorgonImageBuffer buffer = imageData.Buffers[0, frameIndex];

            frame = new BitmapFrameEncode(encoder);

            frame.Initialize();
            frame.SetSize(buffer.Width, buffer.Height);
            frame.SetResolution(options?.DpiX ?? 72, options?.DpiY ?? 72);
            frame.SetPixelFormat(ref pixelFormat);

            // We expect these values to be set to their defaults.  If they are not, then we will have an error.
            // These are for PNG only.
            if (options is not null)
            {
                SetFrameOptions(frame, options);
            }

            if ((metaData is not null) && (metaData.Count > 0))
            {
                EncodeMetaData(metaData, frame);
            }

            // If there's a disparity between what we asked for, and what we actually support, then convert to the correct format.
            if (requestedFormat != pixelFormat)
            {
                bitmap = GetBitmap(buffer, requestedFormat);

                // If we're using indexed pixel format(s), then get the palette.
                if ((pixelFormat == PixelFormat.Format8bppIndexed)
                    || (pixelFormat == PixelFormat.Format4bppIndexed)
                    || (pixelFormat == PixelFormat.Format2bppIndexed)
                    || (pixelFormat == PixelFormat.Format1bppIndexed))
                {
                    paletteInfo = GetEncoderPalette(bitmap, options);
                    frame.Palette = paletteInfo?.Palette;
                }

                using BitmapSource converter = GetFormatConverter(bitmap, pixelFormat, options?.Dithering ?? ImageDithering.None, paletteInfo?.Palette, paletteInfo?.Alpha ?? 0.0f);
                frame.WriteSource(converter);
            }
            else
            {
                frame.WritePixels(buffer.Height, buffer.Data, buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
            }

            frame.Commit();
        }
        finally
        {
            paletteInfo?.Palette?.Dispose();
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
    public IReadOnlyList<BufferFormat> CanConvertFormats(BufferFormat sourceFormat, IEnumerable<BufferFormat> destFormats)
    {
        Guid sourceGuid = GetGUID(sourceFormat);

        if (sourceGuid == Guid.Empty)
        {
            return [];
        }

        var result = new List<BufferFormat>();

        using (var converter = new FormatConverter(_factory))
        {
            foreach (BufferFormat destFormat in destFormats)
            {
                Guid destGuid;

                // If we've asked for B4G4R4A4, we have to convert using a manual conversion by converting to B8G8R8A8 first and then manually downsampling those pixels.
                if (destFormat == BufferFormat.B4G4R4A4_UNorm)
                {
                    destGuid = GetGUID(BufferFormat.B8G8R8A8_UNorm);

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
    /// <returns>A <see cref="GorgonImageInfo"/> containing information about the image data.</returns>
    private (GorgonImageInfo, BitmapFrameDecode, BitmapDecoder, WICStream, Guid) GetImageMetaData(Stream stream, Guid fileFormat, IGorgonWicDecodingOptions options)
    {
        var wicStream = new WICStream(_factory, stream);

        var decoder = new BitmapDecoder(_factory, fileFormat);
        decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);

        if (decoder.ContainerFormat != fileFormat)
        {
            return default;
        }

        BitmapFrameDecode frame = decoder.GetFrame(0);
        BufferFormat format = FindBestFormat(frame.PixelFormat, options?.Flags ?? WICFlags.None, out Guid actualPixelFormat);

        int arrayCount = 1;
        bool readAllFrames = decoder.DecoderInfo.IsMultiframeSupported;

        if ((readAllFrames)
            && (options is not null))
        {
            readAllFrames = options.ReadAllFrames;
        }

        if (readAllFrames)
        {
            arrayCount = decoder.FrameCount.Max(1);
        }

        return (new GorgonImageInfo(ImageDataType.Image2D, format)
        {
            Width = frame.Size.Width,
            Height = frame.Size.Height,
            ArrayCount = arrayCount,
            Depth = 1,
            MipCount = 1
        }, frame, decoder, wicStream, actualPixelFormat);
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
        BitmapEncoder encoder = null;
        BitmapEncoderInfo encoderInfo = null;
        int frameCount = 1;

        try
        {
            Guid pixelFormat = GetGUID(imageData.Format);

            if (pixelFormat == Guid.Empty)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
            }

            encoder = new BitmapEncoder(_factory, imageFileFormat);
            encoder.Initialize(imageStream);

            encoderInfo = encoder.EncoderInfo;

            bool saveAllFrames = encoderInfo.IsMultiframeSupported && imageData.ArrayCount > 1;

            if ((saveAllFrames)
                && (options is not null)
                && (options.Options.Contains(nameof(IGorgonWicEncodingOptions.SaveAllFrames))))
            {
                saveAllFrames = options.SaveAllFrames;
            }

            if (saveAllFrames)
            {
                frameCount = imageData.ArrayCount;
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
    private void ReadAllFrames(IGorgonImage data, Guid srcFormat, Guid convertFormat, BitmapDecoder decoder, IGorgonWicDecodingOptions decodingOptions, IReadOnlyList<string> frameOffsetMetadataItems)
    {
        ImageDithering dithering = decodingOptions?.Dithering ?? ImageDithering.None;
        BitmapFrameDecode frame = null;
        FormatConverter converter = null;

        try
        {
            for (int i = 0; i < data.ArrayCount; ++i)
            {
                IGorgonImageBuffer buffer = data.Buffers[0, i];

                frame?.Dispose();
                frame = decoder.GetFrame(i);
                DX.Point offset = frameOffsetMetadataItems?.Count > 0 ? GetFrameOffsetMetadataItems(frame, frameOffsetMetadataItems) : new DX.Point(0, 0);

                // Get the pointer to the buffer and adjust its offset to that of the current frame.
                GorgonPtr<byte> bufferPtr = buffer.Data + (offset.Y * buffer.PitchInformation.RowPitch) + (offset.X * buffer.PitchInformation.RowPitch / buffer.Width);

                BitmapSource bitmapSource = frame;

                // Convert the format as necessary.
                if (srcFormat != convertFormat)
                {
                    converter = new FormatConverter(_factory);
                    converter.Initialize(frame, convertFormat, (BitmapDitherType)dithering, null, 0.0, BitmapPaletteType.Custom);
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
    /// <param name="convertFormat">Conversion format.</param>
    /// <param name="frame">Frame containing the image data.</param>
    /// <param name="decodingOptions">Options used to decode the image data.</param>
    private void ReadFrame(IGorgonImage data, Guid convertFormat, BitmapFrameDecode frame, IGorgonWicDecodingOptions decodingOptions)
    {
        IGorgonImageBuffer buffer = data.Buffers[0];

        // We don't need to convert, so just leave.
        if ((convertFormat == Guid.Empty) || (frame.PixelFormat == convertFormat))
        {
            frame.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data, buffer.PitchInformation.SlicePitch);
            return;
        }

        ImageDithering dither = decodingOptions?.Dithering ?? ImageDithering.None;
        Bitmap tempBitmap = null;
        BitmapSource formatConverter = null;
        BitmapSource sourceBitmap = frame;
        (Palette Palette, float Alpha)? paletteInfo = null;

        try
        {
            // Perform conversion.
            if ((frame.PixelFormat == PixelFormat.Format8bppIndexed)
                || (frame.PixelFormat == PixelFormat.Format4bppIndexed)
                || (frame.PixelFormat == PixelFormat.Format2bppIndexed)
                || (frame.PixelFormat == PixelFormat.Format1bppIndexed))
            {
                paletteInfo = GetDecoderPalette(frame, decodingOptions);
            }

            if (paletteInfo is not null)
            {
                // Create a temporary bitmap to convert our indexed image.
                tempBitmap = new Bitmap(_factory, frame, BitmapCreateCacheOption.NoCache)
                {
                    Palette = paletteInfo.Value.Palette
                };
                formatConverter = GetFormatConverter(tempBitmap, convertFormat, ImageDithering.None, paletteInfo.Value.Palette, paletteInfo.Value.Alpha);
            }
            else
            {
                formatConverter = GetFormatConverter(sourceBitmap, convertFormat, dither, null, 0.0f);
            }

            formatConverter.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data, buffer.PitchInformation.SlicePitch);
        }
        finally
        {
            tempBitmap?.Dispose();
            paletteInfo?.Palette?.Dispose();
            formatConverter?.Dispose();
            sourceBitmap.Dispose();
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
    private void ScaleBitmapData(BitmapSource bitmap, IGorgonImageBuffer buffer, int width, int height, ImageFilter filter)
    {
        using var scaler = new BitmapScaler(_factory);
        scaler.Initialize(bitmap, width, height, (BitmapInterpolationMode)filter);

        if (bitmap.PixelFormat == scaler.PixelFormat)
        {
            scaler.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data, buffer.PitchInformation.SlicePitch);
            return;
        }

        // There's a chance that, due the filter applied, that the format is now different. 
        // So we'll need to convert.
        using FormatConverter converter = GetFormatConverter(scaler, bitmap.PixelFormat, ImageDithering.None, null, 0);
        converter.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data, buffer.PitchInformation.SlicePitch);
    }

    /// <summary>
    /// Function to expand the WIC bitmap data to a new size and place it into the buffer provided.
    /// </summary>
    /// <param name="bitmap">The WIC bitmap to scale.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="offsetX">The horizontal offset to start cropping at.</param>
    /// <param name="offsetY">The vertical offset to start cropping at.</param>
    private static void ExpandBitmapData(BitmapSource bitmap, IGorgonImageBuffer buffer, int offsetX, int offsetY)
    {
        int pixelStride = buffer.PitchInformation.RowPitch / buffer.Width;
        GorgonPtr<byte> data = buffer.Data + ((offsetY * buffer.PitchInformation.RowPitch) + (offsetX * pixelStride));
        bitmap.CopyPixels(buffer.PitchInformation.RowPitch, data, buffer.PitchInformation.SlicePitch);
    }

    /// <summary>
    /// Function to crop the WIC bitmap data to a new size and place it into the buffer provided.
    /// </summary>
    /// <param name="bitmap">The WIC bitmap to scale.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="offsetX">The horizontal offset to start cropping at.</param>
    /// <param name="offsetY">The vertical offset to start cropping at.</param>
    /// <param name="width">The new width of the image data.</param>
    /// <param name="height">The new height of the image data.</param>
    private void CropBitmapData(BitmapSource bitmap, IGorgonImageBuffer buffer, int offsetX, int offsetY, int width, int height)
    {
        using var clipper = new BitmapClipper(_factory);
        var rect = DX.Rectangle.Intersect(new DX.Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height),
                                                   new DX.Rectangle(offsetX, offsetY, width, height));

        if (rect.IsEmpty)
        {
            return;
        }

        // Intersect our clipping rectangle with the buffer size.
        clipper.Initialize(bitmap, rect);
        clipper.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data, buffer.PitchInformation.SlicePitch);
    }

    /// <summary>
    /// Function to retrieve metadata items from a stream containing an image.
    /// </summary>
    /// <param name="frame">The frame containing the metadata.</param>
    /// <param name="metadataNames">The names of the metadata items to read.</param>
    /// <returns>The offset for the frame.</returns>
    private static DX.Point GetFrameOffsetMetadataItems(BitmapFrameDecode frame, IReadOnlyList<string> metadataNames)
    {
        using MetadataQueryReader reader = frame.MetadataQueryReader;

        reader.TryGetMetadataByName(metadataNames[0], out object xValue);
        reader.TryGetMetadataByName(metadataNames[1], out object yValue);

        xValue ??= 0;

        yValue ??= 0;

        return new DX.Point(Convert.ToInt32(xValue), Convert.ToInt32(yValue));
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
        BitmapDecoder decoder = null;
        WICStream wicStream = null;
        BitmapFrameDecode frame = null;

        try
        {
            wicStream = new WICStream(_factory, wrapper);

            decoder = new BitmapDecoder(_factory, fileFormat);
            decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);

            if (decoder.ContainerFormat != fileFormat)
            {
                return null;
            }

            if (!decoder.DecoderInfo.IsMultiframeSupported)
            {
                return [];
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
        (GorgonImageInfo ImageInfo,
            BitmapFrameDecode FrameDecoder,
            BitmapDecoder Decoder,
            WICStream Stream,
            Guid) result = default;

        try
        {
            result = GetImageMetaData(wrapper, fileFormat, options as IGorgonWicDecodingOptions);
            return result.ImageInfo;
        }
        finally
        {
            stream.Position = oldPosition;

            result.Stream?.Dispose();
            result.Decoder?.Dispose();
            result.FrameDecoder?.Dispose();
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
        _ = length;

        (GorgonImageInfo ImageInfo,
            BitmapFrameDecode FrameDecoder,
            BitmapDecoder Decoder,
            WICStream Stream,
            Guid PixelFormat) metaData = default;
        IGorgonImage result = null;

        try
        {
            metaData = GetImageMetaData(stream, imageFileFormat, decodingOptions);

            if (metaData.ImageInfo is null)
            {
                return null;
            }

            if (metaData.ImageInfo.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, metaData.ImageInfo.Format));
            }

            // Build the image.
            result = new GorgonImage(metaData.ImageInfo);

            // Read a single frame of data. This value will be set larger than 1 if the decoder supports multi-frame images, and the options for the codec 
            // specify that all frames are to be read (true is the default if no options are specified).
            if (metaData.ImageInfo.ArrayCount > 1)
            {
                ReadAllFrames(result, metaData.FrameDecoder.PixelFormat, metaData.PixelFormat, metaData.Decoder, decodingOptions, frameOffsetMetadataItems);
            }
            else
            {
                // For some reason, if we don't dispose of this here, we get and an A/V when trying to dispose it below.
                // Could possibly be because the native object is destroyed before we exit the method?
                metaData.Decoder?.Dispose();
                metaData = (metaData.ImageInfo, metaData.FrameDecoder, null, metaData.Stream, metaData.PixelFormat);
                ReadFrame(result, metaData.PixelFormat, metaData.FrameDecoder, decodingOptions);
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
            metaData.FrameDecoder?.Dispose();
            metaData.Stream?.Dispose();
            metaData.Decoder?.Dispose();
        }
    }

    /// <summary>
    /// Function to generate mip map images.
    /// </summary>
    /// <param name="destImageData">The image that will receive the mip levels.</param>
    /// <param name="filter">The filter to apply when resizing the buffers.</param>
    public void GenerateMipImages(IGorgonImage destImageData, ImageFilter filter)
    {
        Guid pixelFormat = GetGUID(destImageData.Format);

        if (pixelFormat == Guid.Empty)
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, destImageData.Format));
        }

        Bitmap bitmap = null;

        try
        {
            // Begin scaling.
            for (int array = 0; array < destImageData.ArrayCount; ++array)
            {
                int mipDepth = destImageData.Depth;

                // Start at 1 because we're copying from the first mip level..
                for (int mipLevel = 1; mipLevel < destImageData.MipCount; ++mipLevel)
                {
                    for (int depth = 0; depth < mipDepth; ++depth)
                    {
                        IGorgonImageBuffer sourceBuffer = destImageData.Buffers[0, destImageData.ImageType == ImageDataType.Image3D ? (destImageData.Depth / mipDepth) * depth : array];
                        IGorgonImageBuffer destBuffer = destImageData.Buffers[mipLevel, destImageData.ImageType == ImageDataType.Image3D ? depth : array];

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
    /// <param name="resizeMode">The type of resize to perform.</param>
    /// <returns>A new <see cref="IGorgonImage"/> containing the resized data.</returns>
    public GorgonImage Resize(GorgonImage imageData, int offsetX, int offsetY, int newWidth, int newHeight, int newDepth, int calculatedMipLevels, ImageFilter scaleFilter, ResizeMode resizeMode)
    {
        IGorgonImage workingImage = imageData;

        // If we have a 4 bit per channel image, then we need to convert it back to 8 bit per channel (WIC doesn't like 4 bit per channel it seems).
        if (imageData.Format == BufferFormat.B4G4R4A4_UNorm)
        {
            workingImage = imageData.Clone()
                                    .BeginUpdate()
                                    .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm)
                                    .EndUpdate();
        }

        Guid pixelFormat = GetGUID(workingImage.Format);

        if (pixelFormat == Guid.Empty)
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
        }

        var imageInfo = new GorgonImageInfo(workingImage.ImageType, workingImage.Format)
        {
            Width = newWidth,
            Height = newHeight,
            Depth = newDepth.Max(1),
            ArrayCount = workingImage.ArrayCount,
            MipCount = calculatedMipLevels
        };

        var result = new GorgonImage(imageInfo);
        Bitmap bitmap = null;

        try
        {
            for (int array = 0; array < imageInfo.ArrayCount; ++array)
            {
                for (int mip = 0; mip < calculatedMipLevels.Min(workingImage.MipCount); ++mip)
                {
                    int mipDepth = result.GetDepthCount(mip).Min(workingImage.Depth);

                    for (int depth = 0; depth < mipDepth; ++depth)
                    {
                        IGorgonImageBuffer destBuffer = result.Buffers[mip, workingImage.ImageType == ImageDataType.Image3D ? depth : array];
                        IGorgonImageBuffer srcBuffer = workingImage.Buffers[mip, workingImage.ImageType == ImageDataType.Image3D ? depth : array];

                        bitmap = GetBitmap(srcBuffer, pixelFormat);

                        switch (resizeMode)
                        {
                            case ResizeMode.Scale:
                                ScaleBitmapData(bitmap, destBuffer, destBuffer.Width, destBuffer.Height, scaleFilter);
                                break;
                            case ResizeMode.Crop:
                                CropBitmapData(bitmap, destBuffer, offsetX, offsetY, destBuffer.Width, destBuffer.Height);
                                break;
                            case ResizeMode.Expand:
                                ExpandBitmapData(bitmap, destBuffer, offsetX, offsetY);
                                break;
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
            if (imageData.Format == BufferFormat.B4G4R4A4_UNorm)
            {
                workingImage?.Dispose();
            }

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
    public GorgonImage ConvertToFormat(GorgonImage imageData, BufferFormat format, ImageDithering dithering, bool isSrcSRgb, bool isDestSRgb)
    {
        Guid sourceFormat = GetGUID(imageData.Format);
        Guid destFormat = GetGUID(format);

        // Duplicate the settings, and update the format.
        var resultInfo = new GorgonImageInfo(imageData.ImageType, format)
        {
            Width = imageData.Width,
            Height = imageData.Height,
            ArrayCount = imageData.ArrayCount,
            Depth = imageData.Depth,
            MipCount = imageData.MipCount
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
                        IGorgonImageBuffer destBuffer = result.Buffers[mip, imageData.ImageType == ImageDataType.Image3D ? depth : array];
                        IGorgonImageBuffer srcBuffer = imageData.Buffers[mip, imageData.ImageType == ImageDataType.Image3D ? depth : array];
                        var rect = new DX.DataRectangle(srcBuffer.Data, srcBuffer.PitchInformation.RowPitch);

                        Bitmap bitmap = null;
                        BitmapSource formatConverter = null;
                        BitmapSource sRgbConverter = null;
                        ColorContext srcColorContext = null;
                        ColorContext destColorContext = null;

                        try
                        {
                            // Create a WIC bitmap so we have a source for conversion.
                            bitmap = new Bitmap(_factory, srcBuffer.Width, srcBuffer.Height, sourceFormat, rect, srcBuffer.PitchInformation.SlicePitch);

                            // If we have an sRgb conversion, then apply that after converting formats.
                            if ((isSrcSRgb) || (isDestSRgb))
                            {
                                sRgbConverter = GetSRgbTransform(bitmap, isDestSRgb ? destFormat : sourceFormat, isSrcSRgb, isDestSRgb, out srcColorContext, out destColorContext);
                                formatConverter = GetFormatConverter(sRgbConverter, destFormat, dithering, null, 0);
                            }
                            else
                            {
                                formatConverter = GetFormatConverter(bitmap, destFormat, dithering, null, 0);
                            }

                            formatConverter.CopyPixels(destBuffer.PitchInformation.RowPitch,
                                                        destBuffer.Data,
                                                        destBuffer.PitchInformation.SlicePitch);
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
        BitmapDecoder decoder = null;
        WICStream wicStream = null;
        BitmapFrameDecode frame = null;

        try
        {
            // We don't be needing this.
            wicStream = new WICStream(_factory, wrapper);

            decoder = new BitmapDecoder(_factory, decoderFormat);
            decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);

            if (decoder.ContainerFormat != decoderFormat)
            {
                return null;
            }

            if (!decoder.DecoderInfo.IsMultiframeSupported)
            {
                return [];
            }

            int[] result = new int[decoder.FrameCount];

            for (int i = 0; i < result.Length; ++i)
            {
                frame?.Dispose();
                frame = decoder.GetFrame(i);


                frame.MetadataQueryReader.TryGetMetadataByName(delayMetaDataName, out object value);

                if (value is null)
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
    public void Dispose() => _factory?.Dispose();



    /// <summary>
    /// Initializes a new instance of the <see cref="WicUtilities"/> class.
    /// </summary>
    public WicUtilities() => _factory = new ImagingFactory();

}
