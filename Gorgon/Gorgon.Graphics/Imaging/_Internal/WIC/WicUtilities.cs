
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
// Created: June 20, 2016 11:49:00 PM
// 

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Imaging;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.System.Variant;

namespace Gorgon.Graphics.Imaging.Wic;

/// <summary>
/// The type of resize to perform
/// </summary>
public enum ResizeMode
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
public unsafe class WicUtilities
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

    // Native strings for the encoder properties.
    private readonly PWSTR _nativeEncOptInterlacing;
    private readonly PWSTR _nativeEncOptFilter;
    private readonly PWSTR _nativeEncOptImageQuality;

    /// <summary>
    /// A value to hold a WIC to Gorgon buffer format value.
    /// </summary>
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

    /// <summary>
    /// A value to return a color transformer, and source and destination colour contexts.
    /// </summary>
    /// <param name="transformer">The transformer to return.</param>
    /// <param name="sourceContext">The source color context.</param>
    /// <param name="destinationContext">The destination color context.</param>
    private readonly struct WICSrgbConverter(IWICColorTransform* transformer, IWICColorContext* sourceContext, IWICColorContext* destinationContext)
        : IDisposable
    {
        /// <summary>
        /// The bitmap source for the transformer.
        /// </summary>
        public readonly IWICBitmapSource* BitmapSource = (IWICBitmapSource*)transformer;
        /// <summary>
        /// The source colour context.
        /// </summary>
        public readonly IWICColorContext* SourceColorContext = sourceContext;
        /// <summary>
        /// The destination colour context.
        /// </summary>
        public readonly IWICColorContext* DestinationColorContext = destinationContext;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (BitmapSource is not null)
            {
                BitmapSource->Release();
            }

            if (SourceColorContext is not null)
            {
                SourceColorContext->Release();
            }

            if (DestinationColorContext is not null)
            {
                DestinationColorContext->Release();
            }
        }
    }

    /// <summary>
    /// A value to return a color transformer, and source and destination colour contexts.
    /// </summary>
    /// <param name="palette">The WIC palette.</param>
    /// <param name="alpha">The alpha value for transparency.</param>
    private readonly struct WICPaletteInfo(IWICPalette* palette, float alpha)
        : IDisposable
    {
        /// <summary>
        /// The WIC palette.
        /// </summary>
        public readonly IWICPalette* Palette = palette;
        /// <summary>
        /// The alpha value for transparency.
        /// </summary>
        public readonly float Alpha = alpha;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Palette is not null)
            {
                Palette->Release();
            }
        }
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
        new(PixelFormat.Format32bppR10G10B10A2, BufferFormat.R10G10B10A2_UNorm),
        new(PixelFormat.Format32bppR10G10B10A2HDR10, BufferFormat.R10G10B10A2_UNorm),
        new(PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_SharedExp),
        new(PixelFormat.Format16bppBGR565, BufferFormat.B5G6R5_UNorm),
        new(PixelFormat.Format16bppBGRA5551, BufferFormat.B5G5R5A1_UNorm),
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
    private IWICImagingFactory* _factory;
    // The pointer for the factory.
    private nint _factoryPtr;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose managed and unmanaged resources, <b>false</b> to dispose unmanaged resources only.</param>
    private void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _factoryPtr, IntPtr.Zero) == IntPtr.Zero)
        {
            return;
        }

        Marshal.FreeCoTaskMem((nint)(void*)_nativeEncOptInterlacing);
        Marshal.FreeCoTaskMem((nint)(void*)_nativeEncOptImageQuality);
        Marshal.FreeCoTaskMem((nint)(void*)_nativeEncOptFilter);

        if (_factory is not null)
        {
            _factory->Release();
        }

        _factory = null;
    }

    /// <summary>
    /// Function to convert a variant type to an integer value.
    /// </summary>
    /// <param name="prop">The property containing the value to convert.</param>
    /// <returns>The integer value.</returns>
    private static int ConvertVariantToInt(ref PROPVARIANT prop) => prop.Anonymous.Anonymous.vt switch
    {
        VARENUM.VT_I1 => prop.Anonymous.Anonymous.Anonymous.bVal,
        VARENUM.VT_I2 => prop.Anonymous.Anonymous.Anonymous.iVal,
        VARENUM.VT_I4 => prop.Anonymous.Anonymous.Anonymous.lVal,
        VARENUM.VT_I8 => (int)prop.Anonymous.Anonymous.Anonymous.hVal,
        VARENUM.VT_UI1 => prop.Anonymous.Anonymous.Anonymous.bVal,
        VARENUM.VT_UI2 => prop.Anonymous.Anonymous.Anonymous.uiVal,
        VARENUM.VT_UI4 => (int)prop.Anonymous.Anonymous.Anonymous.ulVal,
        VARENUM.VT_UI8 => (int)prop.Anonymous.Anonymous.Anonymous.uhVal,
        VARENUM.VT_R4 => (int)prop.Anonymous.Anonymous.Anonymous.fltVal,
        VARENUM.VT_R8 => (int)prop.Anonymous.Anonymous.Anonymous.dblVal,
        _ => 0
    };

    /// <summary>
    /// Function to read metadata about an image file stored within a stream.
    /// </summary>
    /// <param name="decoder">The decoder that will retrieve the data from the data stream.</param>
    /// <param name="frameDecoder">The decoder for the image frame.</param>
    /// <param name="fileFormat">The file format of the image data.</param>
    /// <param name="options">Options used for decoding the image data.</param>
    /// <returns>A <see cref="GorgonImageInfo"/> containing information about the image data.</returns>
    private (GorgonImageInfo, Guid) GetImageMetaData(IWICBitmapDecoder* decoder, IWICBitmapFrameDecode* frameDecoder, Guid fileFormat, IGorgonWicDecodingOptions options)
    {
        IWICBitmapDecoderInfo* decoderInfo = null;

        try
        {
            decoder->GetContainerFormat(out Guid containerFormat);

            if (containerFormat != fileFormat)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORIMG_ERR_CANNOT_READ_FILE_INVALID_FORMAT);
            }

            decoder->GetDecoderInfo(&decoderInfo);
            decoderInfo->DoesSupportMultiframe(out BOOL readAllFrames);
            frameDecoder->GetPixelFormat(out Guid pixelFormat);

            BufferFormat format = FindBestFormat(pixelFormat, options.Flags, out Guid actualPixelFormat);

            uint arrayCount = 1;

            if ((readAllFrames) && (options.ReadAllFrames))
            {
                decoder->GetFrameCount(out arrayCount);
            }

            frameDecoder->GetSize(out uint frameWidth, out uint frameHeight);

            return (GorgonImageInfo.Create2DImageInfo(format, (int)frameWidth, (int)frameHeight, (int)arrayCount, 1), actualPixelFormat);
        }
        finally
        {
            if (decoderInfo is not null)
            {
                decoderInfo->Release();
            }
        }
    }

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
    private IWICBitmap* GetBitmap(IGorgonImageBuffer imageData, Guid pixelFormat)
    {
        IWICBitmap* result = null;

        _factory->CreateBitmapFromMemory((uint)imageData.Width, (uint)imageData.Height, in pixelFormat, (uint)imageData.PitchInformation.RowPitch, imageData.ImageData, &result);

        return result;
    }

    /// <summary>
    /// Function to retrieve a WIC format converter.
    /// </summary>
    /// <param name="bitmap">The WIC bitmap source to use for the converter.</param>
    /// <param name="targetFormat">The WIC format to convert to.</param>
    /// <param name="dither">The type of dithering to apply to the image data during conversion (if down sampling).</param>
    /// <param name="palette">The 8 bit palette to apply.</param>
    /// <returns>The WIC converter.</returns>
    private IWICBitmapSource* GetFormatConverter(IWICBitmapSource* bitmap, Guid targetFormat, ImageDithering dither, WICPaletteInfo palette)
    {
        WICBitmapPaletteType paletteType = WICBitmapPaletteType.WICBitmapPaletteTypeCustom;
        IWICFormatConverter* result = null;

        _factory->CreateFormatConverter(&result);

        bitmap->GetPixelFormat(out Guid sourceFormat);

        result->CanConvert(in sourceFormat, in targetFormat, out BOOL canConvert);

        if (!canConvert)
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, "WICGuid{" + targetFormat + "}"));
        }

        if (palette.Palette is not null)
        {
            palette.Palette->GetType(out paletteType);

            // Change dithering from ordered to error diffusion when we use 8 bit palettes.
            switch (dither)
            {
                case ImageDithering.Ordered4x4:
                case ImageDithering.Ordered8x8:
                case ImageDithering.Ordered16x16:
                case ImageDithering.Spiral4x4:
                case ImageDithering.Spiral8x8:
                case ImageDithering.DualSpiral4x4:
                case ImageDithering.DualSpiral8x8:
                    if (paletteType == WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
                    {
                        dither = ImageDithering.ErrorDiffusion;
                    }
                    break;
            }
        }

        result->Initialize(bitmap, targetFormat, (WICBitmapDitherType)dither, palette.Palette, palette.Alpha * 100.0, paletteType);

        return (IWICBitmapSource*)result;
    }

    /// <summary>
    /// Function to build the WIC color contexts used to convert to and/or from sRgb pixel formats.
    /// </summary>
    /// <param name="source">The WIC bitmap source to use for sRgb conversion.</param>
    /// <param name="pixelFormat">The pixel format of the image data.</param>
    /// <param name="srcIsSRgb"><b>true</b> if the source data is already in sRgb; otherwise <b>false</b>.</param>
    /// <param name="destIsSRgb"><b>true</b> if the destination data should be converted to sRgb; otherwise <b>false</b>.</param>
    /// <returns>A WIC color transformation object use to convert to/from sRgb.</returns>
    private WICSrgbConverter GetSRgbConverter(IWICBitmapSource* source, Guid pixelFormat, bool srcIsSRgb, bool destIsSRgb)
    {
        IWICColorContext* srcCtx;
        IWICColorContext* destCtx;
        IWICColorTransform* result;

        _factory->CreateColorContext(&srcCtx);
        _factory->CreateColorContext(&destCtx);

        srcCtx->InitializeFromExifColorSpace((uint)(srcIsSRgb ? 1 : 2));
        destCtx->InitializeFromExifColorSpace((uint)(destIsSRgb ? 1 : 2));

        _factory->CreateColorTransformer(&result);
        result->Initialize(source, srcCtx, destCtx, pixelFormat);

        return new(result, srcCtx, destCtx);
    }

    /// <summary>
    /// Function to assign frame encoding options to the frame based on the codec.
    /// </summary>
    /// <param name="options">The list of options to apply.</param>
    /// <param name="properties">The properties to populate.</param>
    private void SetFrameOptions(IGorgonWicEncodingOptions options, IPropertyBag2* properties)
    {
        VARIANT val = new();
        HRESULT result = (HRESULT)0;
        HRESULT err = (HRESULT)0;

        if (options.Options.ContainsName(EncOptInterlacing))
        {
            PROPBAG2 prop = new()
            {
                pstrName = _nativeEncOptInterlacing,
                vt = VARENUM.VT_BOOL
            };

            err = properties->Read(1, &prop, null, &val, &result);

            result.ThrowOnFailure();

            //val.Anonymous.Anonymous.vt = VARENUM.VT_BOOL;
            val.Anonymous.Anonymous.Anonymous.boolVal = options.Options.GetOptionValue<bool>(EncOptInterlacing);
            properties->Write(1, &prop, &val);
        }

        if (options.Options.ContainsName(EncOptFilter))
        {
            PROPBAG2 prop = new()
            {
                pstrName = _nativeEncOptFilter,
                vt = VARENUM.VT_UI1
            };

            err = properties->Read(1, &prop, null, &val, &result);

            result.ThrowOnFailure();

            val.Anonymous.Anonymous.vt = VARENUM.VT_UI1;
            val.Anonymous.Anonymous.Anonymous.bVal = options.Options.GetOptionValue<byte>(EncOptFilter);
            properties->Write(1, &prop, &val);
        }

        if (options.Options.ContainsName(EncOptImageQuality))
        {
            PROPBAG2 prop = new()
            {
                pstrName = _nativeEncOptImageQuality,
                vt = VARENUM.VT_R4
            };

            err = properties->Read(1, &prop, null, &val, &result);

            result.ThrowOnFailure();

            val.Anonymous.Anonymous.vt = VARENUM.VT_R4;
            val.Anonymous.Anonymous.Anonymous.fltVal = options.Options.GetOptionValue<float>(EncOptImageQuality);
            properties->Write(1, &prop, &val);
        }
    }

    /// <summary>
    /// Function to retrieve palette information from an 8 bit indexed image.
    /// </summary>
    /// <param name="options">The list of options used to override.</param>
    /// <returns>The palette information.</returns>
    private unsafe WICPaletteInfo GetDecoderPalette(IGorgonWicDecodingOptions options)
    {
        // If there's no palette option on the decoder, then we do nothing.
        if (!options.Options.ContainsName(DecOptPalette))
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, Resources.GORIMG_ERR_CANNOT_GET_PALETTE_FOR_INDEXED_FRAME);
        }

        IList<GorgonColor> paletteColors = options.Options.GetOptionValue<IList<GorgonColor>>(DecOptPalette) ?? [];
        float alpha = options.Options.GetOptionValue<float>(DecOptAlphaThreshold);

        // If there are no colors set, then extract it from the frame.
        if (paletteColors.Count == 0)
        {
            return default;
        }

        // Generate from our custom palette.
        uint[] colors = ArrayPool<uint>.Shared.Rent(paletteColors.Count);

        try
        {
            for (int i = 0; i < paletteColors.Count; i++)
            {
                GorgonColor color = new(paletteColors[i], paletteColors[i].Alpha >= alpha ? 1.0f : 0.0f);
                colors[i] = (uint)GorgonColor.ToRGBA(color);
            }

            IWICPalette* palette = null;
            _factory->CreatePalette(&palette);

            palette->InitializeCustom(colors.AsSpan());

            return new(palette, alpha);
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(colors, true);
        }
    }

    /// <summary>
    /// Function to retrieve palette information from an 8 bit indexed image.
    /// </summary>
    /// <param name="frame">The bitmap frame that holds the palette info.</param>
    /// <param name="options">The list of options used to override.</param>
    /// <returns>The palette information.</returns>
    private WICPaletteInfo GetEncoderPalette(IWICBitmapSource* frame, IGorgonWicEncodingOptions options)
    {
        // If the user supplied options doesn't contain a encoder option palette, then we can't do anything.
        if (!options.Options.ContainsName(DecOptPalette))
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, Resources.GORIMG_ERR_CANNOT_GET_PALETTE_FOR_INDEXED_FRAME);
        }

        IWICPalette* wicPalette = null;
        IList<GorgonColor> paletteColors = options.Options.GetOptionValue<IList<GorgonColor>>(DecOptPalette) ?? [];
        float alpha = options.Options.GetOptionValue<float>(DecOptAlphaThreshold);

        _factory->CreatePalette(&wicPalette);

        // If there are no colors set, then extract it from the frame.
        if (paletteColors.Count == 0)
        {
            wicPalette->InitializeFromBitmap(frame, 256, !alpha.EqualsEpsilon(0));
        }
        else
        {
            // Generate from our custom palette.
            uint[] colors = ArrayPool<uint>.Shared.Rent(paletteColors.Count);

            for (int i = 0; i < paletteColors.Count; ++i)
            {
                GorgonColor color = paletteColors[i].Alpha >= alpha ? new GorgonColor(paletteColors[i], 1.0f) : GorgonColors.BlackTransparent;
                colors[i] = (uint)GorgonColor.ToARGB(color);
            }

            wicPalette->InitializeCustom(colors);
        }

        return new(wicPalette, alpha);
    }

    /// <summary>
    /// Function to encode metadata into the frame.
    /// </summary>
    /// <param name="metaData">The metadata to encode.</param>
    /// <param name="frameEncoder">The frame being encoded.</param>
    private static void EncodeMetaData(IReadOnlyDictionary<string, object> metaData, IWICBitmapFrameEncode* frameEncoder)
    {
        IWICMetadataQueryWriter* writer = null;

        try
        {
            frameEncoder->GetMetadataQueryWriter(&writer);

            foreach (KeyValuePair<string, object> kvp in metaData)
            {
                Type? valueType = kvp.Value?.GetType();

                PROPVARIANT variant = new();

                if (valueType is null)
                {
                    variant.Anonymous.Anonymous.vt = VARENUM.VT_NULL;
                }
                else
                {
                    switch (Type.GetTypeCode(valueType))
                    {
                        case TypeCode.Boolean:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_BOOL;
                            variant.Anonymous.Anonymous.Anonymous.boolVal = Convert.ToBoolean(kvp.Value);
                            break;
                        case TypeCode.SByte:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_I1;
                            variant.Anonymous.Anonymous.Anonymous.bVal = Convert.ToByte(kvp.Value);
                            break;
                        case TypeCode.Byte:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_UI1;
                            variant.Anonymous.Anonymous.Anonymous.bVal = Convert.ToByte(kvp.Value);
                            break;
                        case TypeCode.Int16:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_I2;
                            variant.Anonymous.Anonymous.Anonymous.iVal = Convert.ToInt16(kvp.Value);
                            break;
                        case TypeCode.UInt16:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_UI2;
                            variant.Anonymous.Anonymous.Anonymous.uiVal = Convert.ToUInt16(kvp.Value);
                            break;
                        case TypeCode.Int32:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_I4;
                            variant.Anonymous.Anonymous.Anonymous.intVal = Convert.ToInt32(kvp.Value);
                            break;
                        case TypeCode.UInt32:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_UI4;
                            variant.Anonymous.Anonymous.Anonymous.uintVal = Convert.ToUInt32(kvp.Value);
                            break;
                        case TypeCode.Int64:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_I8;
                            variant.Anonymous.Anonymous.Anonymous.hVal = Convert.ToInt64(kvp.Value);
                            break;
                        case TypeCode.UInt64:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_UI8;
                            variant.Anonymous.Anonymous.Anonymous.uhVal = Convert.ToUInt64(kvp.Value);
                            break;
                        case TypeCode.Single:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_R4;
                            variant.Anonymous.Anonymous.Anonymous.fltVal = Convert.ToSingle(kvp.Value);
                            break;
                        case TypeCode.Double:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_R8;
                            variant.Anonymous.Anonymous.Anonymous.dblVal = Convert.ToDouble(kvp.Value);
                            break;
                        case TypeCode.Decimal:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_DECIMAL;
                            variant.Anonymous.decVal = Convert.ToDecimal(kvp.Value);
                            break;
                        case TypeCode.String:
                            variant.Anonymous.Anonymous.vt = VARENUM.VT_LPWSTR;
                            variant.Anonymous.Anonymous.Anonymous.pwszVal = new PWSTR(Marshal.StringToCoTaskMemUni(kvp.Value?.ToString()));
                            break;
                        default:
                            throw new InvalidCastException();
                    }
                }

                writer->SetMetadataByName(kvp.Key, in variant);
            }
        }
        finally
        {
            if (writer is not null)
            {
                writer->Release();
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
    private void EncodeFrame(IWICBitmapEncoder* encoder, IGorgonImage imageData, Guid pixelFormat, IGorgonWicEncodingOptions options, int frameIndex, IReadOnlyDictionary<string, object> metaData)
    {
        Guid requestedFormat = pixelFormat;
        IWICBitmapFrameEncode* frameEncoder = null;
        IPropertyBag2* properties = null;
        WICPaletteInfo paletteInfo = default;
        IWICBitmap* bitmap = null;
        IWICBitmapSource* converter = null;

        try
        {
            IGorgonImageBuffer buffer = imageData.Buffers[0, frameIndex];
            encoder->CreateNewFrame(&frameEncoder, &properties);

            // We expect these values to be set to their defaults.  If they are not, then we will have an error.
            // These are for PNG only.
            SetFrameOptions(options, properties);

            frameEncoder->Initialize(properties);
            frameEncoder->SetSize((uint)buffer.Width, (uint)buffer.Height);
            frameEncoder->SetResolution(options.DpiX, options.DpiY);
            frameEncoder->SetPixelFormat(ref pixelFormat);

            if (metaData.Count > 0)
            {
                EncodeMetaData(metaData, frameEncoder);
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
                    paletteInfo = GetEncoderPalette((IWICBitmapSource*)bitmap, options);

                    frameEncoder->SetPalette(paletteInfo.Palette);
                }

                converter = GetFormatConverter((IWICBitmapSource*)bitmap, pixelFormat, options.Dithering, paletteInfo);
                frameEncoder->WriteSource(converter, null);
            }
            else
            {
                frameEncoder->WritePixels((uint)buffer.Height, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);
            }

            frameEncoder->Commit();
        }
        finally
        {
            if (properties is not null)
            {
                properties->Release();
            }

            if (converter is not null)
            {
                converter->Release();
            }

            if (frameEncoder is not null)
            {
                frameEncoder->Release();
            }

            if (bitmap is not null)
            {
                bitmap->Release();
            }

            paletteInfo.Dispose();
        }
    }

    /// <summary>
    /// Function to determine if a format can be converted to any of the requested formats.
    /// </summary>
    /// <param name="sourceFormat">The source format to convert.</param>
    /// <param name="destFormats">The destination formats to convert to.</param>
    /// <returns>A list of formats that the <paramref name="sourceFormat"/> can convert into.</returns>
    public IReadOnlyList<BufferFormat> CanConvertFormats(BufferFormat sourceFormat, IReadOnlyList<BufferFormat> destFormats)
    {
        Guid sourceGuid = GetGUID(sourceFormat);
        IWICFormatConverter* converter = null;
        List<BufferFormat> result = [];

        if (sourceGuid == Guid.Empty)
        {
            return [];
        }

        try
        {
            _factory->CreateFormatConverter(&converter);

            for (int i = 0; i < destFormats.Count; ++i)
            {
                BufferFormat destFormat = destFormats[i];
                Guid destGuid;
                BOOL canConvert;

                // If we've asked for B4G4R4A4, we have to convert using a manual conversion by converting to B8G8R8A8 first and then manually downsampling those pixels.
                if (destFormat == BufferFormat.B4G4R4A4_UNorm)
                {
                    destGuid = GetGUID(BufferFormat.B8G8R8A8_UNorm);

                    if (destGuid != Guid.Empty)
                    {
                        converter->CanConvert(sourceGuid, destGuid, out canConvert);

                        if ((sourceGuid == destGuid) || (canConvert))
                        {
                            result.Add(destFormat);
                        }
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

                converter->CanConvert(sourceGuid, destGuid, out canConvert);

                if (canConvert)
                {
                    result.Add(destFormat);
                }
            }
        }
        finally
        {
            if (converter is not null)
            {
                converter->Release();
            }
        }

        return result;
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
        GorgonComStreamWrapper streamWrapper = new(imageStream, false);
        IStream* iStream = (IStream*)streamWrapper;
        IWICBitmapEncoder* encoder = null;
        IWICBitmapEncoderInfo* encoderInfo = null;
        int frameCount = 1;

        try
        {
            Guid pixelFormat = GetGUID(imageData.Format);

            if (pixelFormat == Guid.Empty)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
            }
            if (imageFileFormat == PInvoke.GUID_ContainerFormatWebp)
            {
                Guid vendor = PInvoke.GUID_VendorMicrosoft;
                encoder = _factory->CreateEncoder(&imageFileFormat, &vendor);
            }
            else
            {
                encoder = _factory->CreateEncoder(&imageFileFormat, null);
            }
            encoder->Initialize(iStream, WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache);

            encoder->GetEncoderInfo(&encoderInfo);
            encoderInfo->DoesSupportMultiframe(out BOOL supportsMultiframe);

            if ((supportsMultiframe) && (imageData.ArrayCount > 1) && (options.SaveAllFrames))
            {
                frameCount = imageData.ArrayCount;
            }

            for (int i = 0; i < frameCount; ++i)
            {
                EncodeFrame(encoder, imageData, pixelFormat, options, i, metaData);
            }

            encoder->Commit();
        }
        finally
        {
            if (encoderInfo is not null)
            {
                encoderInfo->Release();
            }

            if (encoder is not null)
            {
                encoder->Release();
            }

            iStream->Release();
            streamWrapper.Dispose();
        }
    }

    /// <summary>
    /// Function to decode all frames in a multi-frame image.
    /// </summary>
    /// <param name="fileFormat">The format of the encoded data.</param>
    /// <param name="data">The image data that will receive the multi-frame data.</param>
    /// <param name="convertFormat">The destination pixel format.</param>
    /// <param name="stream">The stream containing the encoded data.</param>
    /// <param name="decodingOptions">Options used in decoding the image.</param>
    /// <param name="xOffsetMetadataName">The name of the X offset metadata.</param>
    /// <param name="yOffsetMetadataName">The name of the Y offset metadata.</param>
    private void ReadAllFrames(Guid fileFormat, GorgonImage data, Guid convertFormat, IWICStream* stream, IGorgonWicDecodingOptions decodingOptions, string xOffsetMetadataName, string yOffsetMetadataName)
    {
        IWICBitmap* tempBitmap = null;
        IWICBitmapDecoder* decoder = null;
        IWICBitmapFrameDecode* frameDecoder = null;
        IWICBitmapSource* converter = null;
        WICPaletteInfo palette = default;

        try
        {
            decoder = _factory->CreateDecoder(&fileFormat, null);
            decoder->Initialize((IStream*)stream, WICDecodeOptions.WICDecodeMetadataCacheOnDemand);

            for (uint i = 0; i < data.ArrayCount; ++i)
            {
                IGorgonImageBuffer buffer = data.Buffers[0, (int)i];

                decoder->GetFrame(i, &frameDecoder);
                frameDecoder->GetPixelFormat(out Guid framePixelFormat);

                if ((convertFormat == Guid.Empty) || (convertFormat == framePixelFormat))
                {
                    frameDecoder->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);
                    return;
                }

                if ((framePixelFormat == PixelFormat.Format1bppIndexed) || (framePixelFormat == PixelFormat.Format2bppIndexed)
                    || (framePixelFormat == PixelFormat.Format4bppIndexed) || (framePixelFormat == PixelFormat.Format8bppIndexed))
                {
                    palette = GetDecoderPalette(decodingOptions);
                }

                GorgonPoint offset = (!string.IsNullOrWhiteSpace(xOffsetMetadataName) && !string.IsNullOrWhiteSpace(yOffsetMetadataName)) ? GetFrameOffsetMetadataItems(frameDecoder, xOffsetMetadataName, yOffsetMetadataName)
                                                                                                                                          : GorgonPoint.Zero;

                // Get the pointer to the buffer and adjust its offset to that of the current frame.
                GorgonPtr<byte> bufferPtr = buffer.ImageData + offset.Y * buffer.PitchInformation.RowPitch + offset.X * buffer.FormatInformation.SizeInBytes;

                // Convert the format as necessary.
                if (palette.Palette is not null)
                {
                    _factory->CreateBitmapFromSource((IWICBitmapSource*)frameDecoder, WICBitmapCreateCacheOption.WICBitmapNoCache, &tempBitmap);
                    tempBitmap->SetPalette(palette.Palette);
                    converter = GetFormatConverter((IWICBitmapSource*)tempBitmap, convertFormat, decodingOptions.Dithering, palette);
                }
                else
                {
                    converter = GetFormatConverter((IWICBitmapSource*)frameDecoder, convertFormat, decodingOptions.Dithering, default);
                }

                converter->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);

                if (tempBitmap is not null)
                {
                    tempBitmap->Release();
                    tempBitmap = null;
                }

                palette.Dispose();
                palette = default;
                converter->Release();
                converter = null;
                frameDecoder->Release();
                frameDecoder = null;
            }
        }
        finally
        {
            palette.Dispose();

            if (tempBitmap is not null)
            {
                tempBitmap->Release();
            }

            if (converter is not null)
            {
                converter->Release();
            }

            if (frameDecoder is not null)
            {
                frameDecoder->Release();
            }

            if (decoder is not null)
            {
                decoder->Release();
            }
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
    private void ScaleBitmapData(IWICBitmapSource* bitmap, IGorgonImageBuffer buffer, int width, int height, ImageFilter filter)
    {
        IWICBitmapScaler* scaler = null;
        IWICBitmapSource* converter = null;

        try
        {
            _factory->CreateBitmapScaler(&scaler);
            scaler->Initialize(bitmap, (uint)width, (uint)height, (WICBitmapInterpolationMode)filter);

            bitmap->GetPixelFormat(out Guid sourceFormat);
            scaler->GetPixelFormat(out Guid scalerFromat);

            WICRect rect = new()
            {
                Height = height,
                Width = width
            };

            if (sourceFormat == scalerFromat)
            {
                scaler->CopyPixels(in rect, (uint)buffer.PitchInformation.RowPitch, buffer.ImageData);
                return;
            }

            // There's a chance that, due the filter applied, that the format is now different. 
            // So we'll need to convert.
            converter = GetFormatConverter((IWICBitmapSource*)scaler, sourceFormat, ImageDithering.None, default);
            converter->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);
        }
        finally
        {
            if (converter is not null)
            {
                converter->Release();
            }

            if (scaler is not null)
            {
                scaler->Release();
            }
        }
    }

    /// <summary>
    /// Function to expand the WIC bitmap data to a new size and place it into the buffer provided.
    /// </summary>
    /// <param name="bitmap">The WIC bitmap to scale.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="offsetX">The horizontal offset to start cropping at.</param>
    /// <param name="offsetY">The vertical offset to start cropping at.</param>
    private static void ExpandBitmapData(IWICBitmap* bitmap, IGorgonImageBuffer buffer, int offsetX, int offsetY)
    {
        int pixelStride = buffer.FormatInformation.SizeInBytes;
        GorgonPtr<byte> data = buffer.ImageData + (offsetY * buffer.PitchInformation.RowPitch + offsetX * pixelStride);

        IWICBitmapLock* bitmapLock = null;

        try
        {
            bitmap->GetSize(out uint width, out uint height);

            GorgonRectangle sourceRect = new(offsetX, offsetY, (int)width, (int)height);
            GorgonRectangle destRect = new(0, 0, buffer.Width, buffer.Height);

            if ((offsetX == 0) && (offsetY == 0))
            {
                bitmap->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);
                return;
            }

            GorgonRectangle finalRect = GorgonRectangle.Intersect(destRect, sourceRect);

            if ((finalRect.Width <= 0) || (finalRect.Height <= 0))
            {
                return;
            }

            int rowStride = buffer.PitchInformation.RowPitch.Min(finalRect.Width * buffer.FormatInformation.SizeInBytes);

            bitmap->Lock(null, (int)WICBitmapLockFlags.WICBitmapLockRead, &bitmapLock);

            byte* sourceData;
            byte* destData = (byte*)buffer.ImageData;
            bitmapLock->GetDataPointer(out uint bufferSize, &sourceData);
            bitmapLock->GetStride(out uint sourceStride);

            for (int y = 0; y < finalRect.Height; ++y)
            {
                byte* srcPtr = sourceData + (y * sourceStride);
                byte* destPtr = destData + ((y + finalRect.Top) * buffer.PitchInformation.RowPitch) + (finalRect.Left * buffer.FormatInformation.SizeInBytes);

                Unsafe.CopyBlock(destPtr, srcPtr, (uint)rowStride);
            }
        }
        finally
        {
            if (bitmapLock is not null)
            {
                bitmapLock->Release();
            }
        }
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
    private void CropBitmapData(IWICBitmapSource* bitmap, IGorgonImageBuffer buffer, int offsetX, int offsetY, int width, int height)
    {
        IWICBitmapClipper* clipper = null;

        try
        {
            _factory->CreateBitmapClipper(&clipper);

            bitmap->GetSize(out uint srcWidth, out uint srcHeight);

            GorgonRectangle rect = GorgonRectangle.Intersect(new GorgonRectangle(0, 0, (int)srcWidth, (int)srcHeight),
                                                             new GorgonRectangle(offsetX, offsetY, width, height));

            if ((rect.Width <= 0) || (rect.Height <= 0))
            {
                return;
            }

            WICRect srcRect = new()
            {
                Width = rect.Width,
                Height = rect.Height,
                X = rect.X,
                Y = rect.Y
            };

            clipper->Initialize(bitmap, srcRect);
            clipper->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);
        }
        finally
        {
            if (clipper is not null)
            {
                clipper->Release();
            }
        }
    }

    /// <summary>
    /// Function to retrieve metadata items from a stream containing an image.
    /// </summary>
    /// <param name="frame">The frame containing the metadata.</param>
    /// <param name="xOffsetMetadataName">The metadata name of the x offset for the frame.</param>
    /// <param name="yOffsetMetadataName">The metadata name of the y offset for the frame.</param>
    /// <returns>The offset for the frame.</returns>
    private static GorgonPoint GetFrameOffsetMetadataItems(IWICBitmapFrameDecode* frame, string xOffsetMetadataName, string yOffsetMetadataName)
    {
        IWICMetadataQueryReader* reader = null;
        PROPVARIANT xProp = new();
        PROPVARIANT yProp = new();

        try
        {
            frame->GetMetadataQueryReader(&reader);
            reader->GetMetadataByName(xOffsetMetadataName, ref xProp);
            reader->GetMetadataByName(xOffsetMetadataName, ref yProp);

            return new(ConvertVariantToInt(ref xProp), ConvertVariantToInt(ref yProp));
        }
        finally
        {
            if (reader is not null)
            {
                reader->Release();
            }
        }
    }

    /// <summary>
    /// Function to retrieve metadata items from a stream containing an image.
    /// </summary>
    /// <param name="stream">The stream used to read the data.</param>
    /// <param name="fileFormat">The file format to use.</param>
    /// <param name="xOffsetMetadataName">The name of the X offset metadata.</param>
    /// <param name="yOffsetMetadataName">The name of the Y offset metadata.</param>
    /// <returns>A list of frame offsets.</returns>
    public IReadOnlyList<GorgonPoint> GetFrameOffsetMetadata(Stream stream, Guid fileFormat, string xOffsetMetadataName, string yOffsetMetadataName)
    {
        long oldPosition = stream.Position;
        GorgonStreamSlice wrapper = new(stream, allowWrite: false);
        GorgonComStreamWrapper comWrapper = new(stream, false);
        IStream* iStream = (IStream*)comWrapper;
        IWICBitmapDecoder* decoder = null;
        IWICBitmapFrameDecode* frameDecoder = null;
        IWICBitmapDecoderInfo* decoderInfo = null;
        IWICStream* wicStream = null;

        try
        {
            if ((string.IsNullOrWhiteSpace(yOffsetMetadataName)) || (string.IsNullOrWhiteSpace(yOffsetMetadataName)))
            {
                return [];
            }

            _factory->CreateStream(&wicStream);
            wicStream->InitializeFromIStream(iStream);

            decoder = _factory->CreateDecoder(&fileFormat, null);
            decoder->Initialize((IStream*)wicStream, WICDecodeOptions.WICDecodeMetadataCacheOnDemand);

            decoder->GetContainerFormat(out Guid actualFileFormat);

            if (actualFileFormat != fileFormat)
            {
                return [];
            }

            decoder->GetDecoderInfo(&decoderInfo);
            decoderInfo->DoesSupportMultiframe(out BOOL supportsMultiframe);

            if (!supportsMultiframe)
            {
                return [];
            }

            decoder->GetFrameCount(out uint frameCount);

            GorgonPoint[] result = new GorgonPoint[frameCount];

            for (uint i = 0; i < result.Length; ++i)
            {
                decoder->GetFrame(i, &frameDecoder);

                result[i] = GetFrameOffsetMetadataItems(frameDecoder, xOffsetMetadataName, yOffsetMetadataName);

                frameDecoder->Release();
                frameDecoder = null;
            }

            return result;
        }
        finally
        {
            stream.Position = oldPosition;

            if (wicStream is not null)
            {
                wicStream->Release();
            }

            iStream->Release();

            if (decoderInfo is not null)
            {
                decoderInfo->Release();
            }

            if (frameDecoder is not null)
            {
                frameDecoder->Release();
            }

            if (decoder is not null)
            {
                decoder->Release();
            }

            comWrapper.Dispose();
        }
    }

    /// <summary>
    /// Function to retrieve the metadata for an image from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the image data.</param>
    /// <param name="fileFormat">The file format of the data in the stream.</param>
    /// <param name="options">Options for image decoding.</param>
    /// <returns>The image metadata from the stream and the file format for the file in the stream.</returns>
    public GorgonImageInfo GetImageMetaDataFromStream(Stream stream, Guid fileFormat, IGorgonWicDecodingOptions options)
    {
        long oldPosition = stream.Position;
        GorgonStreamSlice wrapper = new(stream, allowWrite: false);
        GorgonComStreamWrapper comStream = new(wrapper, false);
        IStream* iStream = (IStream*)comStream;
        IWICStream* wicStream = null;
        IWICBitmapDecoder* decoder = null;
        IWICBitmapFrameDecode* frameDecoder = null;

        try
        {
            _factory->CreateStream(&wicStream);
            wicStream->InitializeFromIStream(iStream);

            decoder = _factory->CreateDecoder(&fileFormat, null);
            decoder->Initialize((IStream*)wicStream, WICDecodeOptions.WICDecodeMetadataCacheOnDemand);

            (GorgonImageInfo imageInfo, Guid _) = GetImageMetaData(decoder, frameDecoder, fileFormat, options);
            return imageInfo;
        }
        finally
        {
            if (frameDecoder is not null)
            {
                frameDecoder->Release();
            }

            if (decoder is not null)
            {
                decoder->Release();
            }

            if (wicStream is not null)
            {
                wicStream->Release();
            }

            iStream->Release();
            wrapper.Dispose();
            stream.Position = oldPosition;
        }
    }

    /// <summary>
    /// Function to decode image data from a file within a stream.
    /// </summary>
    /// <param name="stream">The stream containing the image data to decode.</param>
    /// <param name="length">The size of the image data to read, in bytes.</param>
    /// <param name="imageFileFormat">The file format for the image data in the stream.</param>
    /// <param name="decodingOptions">Options used for decoding the image data.</param>
    /// <param name="xOffsetMetadataName">The name of the X offset metadata.</param>
    /// <param name="yOffsetMetadataName">The name of the Y offset metadata.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the decoded image file data. Or <b>null</b> if the decoding fails.</returns>
    public IGorgonImage DecodeImageData(Stream stream, long length, Guid imageFileFormat, IGorgonWicDecodingOptions decodingOptions, string xOffsetMetadataName, string yOffsetMetadataName)
    {
        _ = length;

        GorgonComStreamWrapper comWrapper = new(stream, false);
        IStream* iStream = (IStream*)comWrapper;
        GorgonImage? result = null;
        IWICStream* wicStream = null;
        IWICBitmap* tempBitmap = null;
        IWICBitmapDecoder* decoder = null;
        IWICBitmapFrameDecode* firstFrameDecoder = null;
        IWICBitmapFrameDecode* frameDecoder = null;
        IWICBitmapSource* converter = null;
        WICPaletteInfo palette = default;

        try
        {
            _factory->CreateStream(&wicStream);
            wicStream->InitializeFromIStream(iStream);

            decoder = _factory->CreateDecoder(&imageFileFormat, null);
            decoder->Initialize((IStream*)wicStream, WICDecodeOptions.WICDecodeMetadataCacheOnDemand);

            decoder->GetFrame(0, &firstFrameDecoder);

            (GorgonImageInfo imageInfo, Guid pixelFormat) = GetImageMetaData(decoder, firstFrameDecoder, imageFileFormat, decodingOptions);

            if (imageInfo.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported, Resources.GORIMG_ERR_CANNOT_READ_FILE_INVALID_FORMAT);
            }

            // Build the image.
            result = new GorgonImage(imageInfo);

            for (uint i = 0; i < result.ArrayCount; ++i)
            {
                IGorgonImageBuffer buffer = result.Buffers[0, (int)i];

                decoder->GetFrame(i, &frameDecoder);
                frameDecoder->GetPixelFormat(out Guid framePixelFormat);

                if ((pixelFormat == Guid.Empty) || (pixelFormat == framePixelFormat))
                {
                    frameDecoder->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)buffer.ImageData);

                    if (frameDecoder is not null)
                    {
                        frameDecoder->Release();
                        frameDecoder = null;
                    }
                    continue;
                }

                if ((framePixelFormat == PixelFormat.Format1bppIndexed) || (framePixelFormat == PixelFormat.Format2bppIndexed)
                    || (framePixelFormat == PixelFormat.Format4bppIndexed) || (framePixelFormat == PixelFormat.Format8bppIndexed))
                {
                    palette = GetDecoderPalette(decodingOptions);
                }

                GorgonPoint offset = (!string.IsNullOrWhiteSpace(xOffsetMetadataName) && !string.IsNullOrWhiteSpace(yOffsetMetadataName))
                                    ? GetFrameOffsetMetadataItems(frameDecoder, xOffsetMetadataName, yOffsetMetadataName)
                                    : GorgonPoint.Zero;

                // Get the pointer to the buffer and adjust its offset to that of the current frame.
                GorgonPtr<byte> bufferPtr = buffer.ImageData + offset.Y * buffer.PitchInformation.RowPitch + offset.X * buffer.FormatInformation.SizeInBytes;

                // Convert the format as necessary.
                if (palette.Palette is not null)
                {
                    _factory->CreateBitmapFromSource((IWICBitmapSource*)frameDecoder, WICBitmapCreateCacheOption.WICBitmapNoCache, &tempBitmap);
                    tempBitmap->SetPalette(palette.Palette);
                    converter = GetFormatConverter((IWICBitmapSource*)tempBitmap, pixelFormat, decodingOptions.Dithering, palette);
                }
                else
                {
                    converter = GetFormatConverter((IWICBitmapSource*)frameDecoder, pixelFormat, decodingOptions.Dithering, default);
                }

                converter->CopyPixels(null, (uint)buffer.PitchInformation.RowPitch, (uint)buffer.PitchInformation.SlicePitch, (byte*)bufferPtr);

                palette.Dispose();
                palette = default;

                if (tempBitmap is not null)
                {
                    tempBitmap->Release();
                    tempBitmap = null;
                }

                converter->Release();
                converter = null;
                frameDecoder->Release();
                frameDecoder = null;
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
            palette.Dispose();

            if (tempBitmap is not null)
            {
                tempBitmap->Release();
            }

            if (converter is not null)
            {
                converter->Release();
            }

            if (frameDecoder is not null)
            {
                frameDecoder->Release();
            }

            if (firstFrameDecoder is not null)
            {
                firstFrameDecoder->Release();
            }

            if (decoder is not null)
            {
                decoder->Release();
            }

            if (wicStream is not null)
            {
                wicStream->Release();
            }

            iStream->Release();
            comWrapper.Dispose();
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

        IWICBitmap* bitmap = null;

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
                        IGorgonImageBuffer sourceBuffer = destImageData.Buffers[0, destImageData.ImageType == ImageDataType.Image3D ? destImageData.Depth / mipDepth * depth : array];
                        IGorgonImageBuffer destBuffer = destImageData.Buffers[mipLevel, destImageData.ImageType == ImageDataType.Image3D ? depth : array];

                        bitmap = GetBitmap(sourceBuffer, pixelFormat);

                        ScaleBitmapData((IWICBitmapSource*)bitmap, destBuffer, destBuffer.Width, destBuffer.Height, filter);

                        bitmap->Release();
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
            bitmap->Release();
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
    public unsafe GorgonImage Resize(GorgonImage imageData, int offsetX, int offsetY, int newWidth, int newHeight, int newDepth, int calculatedMipLevels, ImageFilter scaleFilter, ResizeMode resizeMode)
    {
        GorgonImage workingImage = imageData;

        // If we have a 4 bit per channel image, then we need to convert it back to 8 bit per channel (WIC doesn't like 4 bit per channel it seems).
        if (imageData.Format == BufferFormat.B4G4R4A4_UNorm)
        {
            workingImage = ConvertToFormat(workingImage, BufferFormat.R8G8B8A8_UNorm, ImageDithering.None, false, false);
        }

        Guid pixelFormat = GetGUID(workingImage.Format);

        if (pixelFormat == Guid.Empty)
        {
            throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Format));
        }

        // Duplicate the settings, and update the format.
        GorgonImageInfo imageInfo = imageData.ImageType switch
        {
            ImageDataType.Image1D => GorgonImageInfo.Create1DImageInfo(imageData.Format, newWidth, imageData.ArrayCount, calculatedMipLevels, imageData.HasPremultipliedAlpha),
            ImageDataType.Image2D => GorgonImageInfo.Create2DImageInfo(imageData.Format, newWidth, newHeight, imageData.ArrayCount, calculatedMipLevels, imageData.HasPremultipliedAlpha),
            ImageDataType.ImageCube => GorgonImageInfo.CreateCubeImageInfo(imageData.Format, newWidth, newHeight, imageData.ArrayCount / 6, calculatedMipLevels, imageData.HasPremultipliedAlpha),
            ImageDataType.Image3D => GorgonImageInfo.Create3DImageInfo(imageData.Format, newWidth, newHeight, newDepth, calculatedMipLevels),
            _ => throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_IMAGE_TYPE_UNKNOWN, imageData.ImageType)),
        };

        GorgonImage result = new(imageInfo);
        IWICBitmap* bitmap = null;

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
                                ScaleBitmapData((IWICBitmapSource*)bitmap, destBuffer, destBuffer.Width, destBuffer.Height, scaleFilter);
                                break;
                            case ResizeMode.Crop:
                                CropBitmapData((IWICBitmapSource*)bitmap, destBuffer, offsetX, offsetY, destBuffer.Width, destBuffer.Height);
                                break;
                            case ResizeMode.Expand:
                                ExpandBitmapData(bitmap, destBuffer, offsetX, offsetY);
                                break;
                        }

                        bitmap->Release();
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

            if (bitmap is not null)
            {
                bitmap->Release();
            }
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
        GorgonImageInfo resultInfo = imageData.ImageType switch
        {
            ImageDataType.Image1D => GorgonImageInfo.Create1DImageInfo(format, imageData.Width, imageData.ArrayCount, imageData.MipCount, imageData.HasPremultipliedAlpha),
            ImageDataType.Image2D => GorgonImageInfo.Create2DImageInfo(format, imageData.Width, imageData.Height, imageData.ArrayCount, imageData.MipCount, imageData.HasPremultipliedAlpha),
            ImageDataType.ImageCube => GorgonImageInfo.CreateCubeImageInfo(format, imageData.Width, imageData.Height, imageData.ArrayCount / 6, imageData.MipCount, imageData.HasPremultipliedAlpha),
            ImageDataType.Image3D => GorgonImageInfo.Create3DImageInfo(format, imageData.Width, imageData.Height, imageData.Depth, imageData.MipCount),
            _ => throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_IMAGE_TYPE_UNKNOWN, imageData.ImageType)),
        };

        GorgonImage result = new(resultInfo);

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

                        IWICBitmap* bitmap = null;
                        IWICBitmapSource* formatConverter = null;
                        WICSrgbConverter converter = new();

                        try
                        {
                            // Create a WIC bitmap so we have a source for conversion.
                            bitmap = GetBitmap(srcBuffer, sourceFormat);

                            // If we have an sRgb conversion, then apply that after converting formats.
                            if ((isSrcSRgb) || (isDestSRgb))
                            {
                                converter = GetSRgbConverter((IWICBitmapSource*)bitmap, isDestSRgb ? destFormat : sourceFormat, isSrcSRgb, isDestSRgb);
                                formatConverter = GetFormatConverter(converter.BitmapSource, destFormat, dithering, default);
                            }
                            else
                            {
                                formatConverter = GetFormatConverter((IWICBitmapSource*)bitmap, destFormat, dithering, default);
                            }

                            formatConverter->CopyPixels(null, (uint)destBuffer.PitchInformation.RowPitch, (uint)destBuffer.PitchInformation.SlicePitch, (byte*)destBuffer.ImageData);
                        }
                        finally
                        {
                            converter.Dispose();

                            if (formatConverter is not null)
                            {
                                formatConverter->Release();
                                formatConverter = null;
                            }

                            if (bitmap is not null)
                            {
                                bitmap->Release();
                                bitmap = null;
                            }
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
        GorgonComStreamWrapper istreamWrapper = new(stream, false);
        IStream* iStream = (IStream*)istreamWrapper;
        IWICStream* wicStream = null;
        IWICBitmapDecoder* decoder = null;
        IWICBitmapFrameDecode* frame = null;
        IWICBitmapDecoderInfo* decoderInfo = null;
        IWICMetadataQueryReader* queryReader = null;

        try
        {
            _factory->CreateStream(&wicStream);
            wicStream->InitializeFromIStream(iStream);

            decoder = _factory->CreateDecoderFromStream(iStream, in decoderFormat, WICDecodeOptions.WICDecodeMetadataCacheOnDemand);
            decoder->GetContainerFormat(out Guid actualDecoderFormat);
            decoder->GetDecoderInfo(&decoderInfo);

            if (decoderInfo is null)
            {
                return [];
            }

            decoderInfo->DoesSupportMultiframe(out BOOL hasMultipleFrames);

            if ((!hasMultipleFrames) || (actualDecoderFormat != decoderFormat))
            {
                return [];
            }

            decoder->GetFrameCount(out uint frameCount);

            int[] result = new int[frameCount];

            for (uint i = 0; i < result.Length; ++i)
            {
                decoder->GetFrame(i, &frame);

                if (frame is null)
                {
                    continue;
                }

                frame->GetMetadataQueryReader(&queryReader);

                if (queryReader is not null)
                {
                    PROPVARIANT property = default;

                    queryReader->GetMetadataByName(delayMetaDataName, ref property);

                    result[i] = ConvertVariantToInt(ref property);

                    queryReader->Release();
                    queryReader = null;
                }

                frame->Release();
                frame = null;
            }

            return result;
        }
        finally
        {
            stream.Position = oldPosition;

            if (wicStream is not null)
            {
                wicStream->Release();
            }

            if (queryReader is not null)
            {
                queryReader->Release();
                queryReader = null;
            }

            if (frame is not null)
            {
                frame->Release();
            }

            if (decoder is not null)
            {
                decoder->Release();
            }

            if (decoderInfo is not null)
            {
                decoderInfo->Release();
            }

            iStream->Release();
            istreamWrapper.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalize for the object, used to dispose of unmanaged objects.
    /// </summary>
    ~WicUtilities() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="WicUtilities"/> class.
    /// </summary>
    /// <exception cref="Exception">Thrown if the Windows Imaging Comonents system could not be initialized.</exception>
    public WicUtilities()
    {
        _nativeEncOptInterlacing = new PWSTR(Marshal.StringToCoTaskMemUni("InterlaceOption"));
        _nativeEncOptImageQuality = new PWSTR(Marshal.StringToCoTaskMemUni("ImageQuality"));
        _nativeEncOptFilter = new PWSTR(Marshal.StringToCoTaskMemUni("FilterOption"));

        PInvoke.CoCreateInstance(in PInvoke.CLSID_WICImagingFactory1, null, CLSCTX.CLSCTX_INPROC_SERVER, out IWICImagingFactory* wicFactory)
               .ThrowOnFailure();

        _factory = wicFactory;
        _factoryPtr = (nint)_factory;
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    static WicUtilities()
    {
    }
}
