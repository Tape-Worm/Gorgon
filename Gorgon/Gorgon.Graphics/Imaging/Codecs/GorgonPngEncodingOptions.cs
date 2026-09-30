
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
// Created: June 28, 2016 10:38:40 PM
// 

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;
using Windows.Win32.Graphics.Imaging;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Filter to apply for compression optimization.
/// </summary>
public enum PngFilter
{
    /// <summary>
    /// The system will choose the best filter based on the image data.
    /// </summary>
    DontCare = WICPngFilterOption.WICPngFilterUnspecified,
    /// <summary>
    /// No filtering.
    /// </summary>
    None = WICPngFilterOption.WICPngFilterNone,
    /// <summary>
    /// Sub filtering.
    /// </summary>
    Sub = WICPngFilterOption.WICPngFilterSub,
    /// <summary>
    /// Up filtering.
    /// </summary>
    Up = WICPngFilterOption.WICPngFilterUp,
    /// <summary>
    /// Average filtering.
    /// </summary>
    Average = WICPngFilterOption.WICPngFilterAverage,
    /// <summary>
    /// Paeth filtering.
    /// </summary>
    Paeth = WICPngFilterOption.WICPngFilterPaeth,
    /// <summary>
    /// Adaptive filtering. The system will choose the best filter on a per-scanline basis.
    /// </summary>
    Adaptive = WICPngFilterOption.WICPngFilterAdaptive
}

/// <summary>
/// Options used when encoding an image to a stream as a PNG file.
/// </summary>
public sealed class GorgonPngEncodingOptions
    : IGorgonWicEncodingOptions
{
    // The dithering to apply when converting formats.
    private readonly GorgonOption<ImageDithering> _dithering = GorgonOption.CreateOption(nameof(Dithering), ImageDithering.None, Resources.GORIMG_OPT_WIC_DITHERING);
    // The filter used to improve compression.
    private readonly GorgonOption<PngFilter> _filter = GorgonOption.CreateOption(nameof(Filter), PngFilter.None, Resources.GORIMG_OPT_PNG_FILTERING);
    // Whether to interlace the image.
    private readonly GorgonOption<bool> _interlacing = GorgonOption.CreateOption(nameof(Interlacing), false, Resources.GORIMG_OPT_PNG_INTERLACED);
    // The horizontal dots per inch.
    private readonly GorgonRangedOption<double> _dpiX = GorgonOption.CreateDoubleOption(nameof(DpiX), 72.0, Resources.GORIMG_OPT_WIC_DPIX);
    // The vertical dots per inch.
    private readonly GorgonRangedOption<double> _dpiY = GorgonOption.CreateDoubleOption(nameof(DpiY), 72.0, Resources.GORIMG_OPT_WIC_DPIY);

    /// <summary>
    /// The default encoding options for PNG files.
    /// </summary>
    public static readonly GorgonPngEncodingOptions Default = new();

    /// <inheritdoc/>
    bool IGorgonImageCodecEncodingOptions.SaveAllFrames
    {
        get => false;
        set
        {
            // Intentionally left blank.
        }
    }

    /// <inheritdoc cref="IGorgonWicEncodingOptions.DpiX" path="/summary"/>
    /// <remarks>
    /// <para>
    /// The default value is 72.0.
    /// </para>
    /// </remarks>
    public double DpiX
    {
        get => _dpiX.Value;
        set => _dpiX.Value = value;
    }

    /// <inheritdoc cref="IGorgonWicEncodingOptions.DpiY" path="/summary"/>
    /// <remarks>
    /// <para>
    /// The default value is 72.0.
    /// </para>
    /// </remarks>
    public double DpiY
    {
        get => _dpiY.Value;
        set => _dpiY.Value = value;
    }

    /// <summary>
    /// Property to set or return whether to use interlacing when encoding an image as a PNG file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool Interlacing
    {
        get => _interlacing.Value;
        set => _interlacing.Value = value;
    }

    /// <summary>
    /// Property to set or return the type of filter to use when compressing the PNG file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="PngFilter.None"/>.
    /// </para>
    /// </remarks>
    public PngFilter Filter
    {
        get => _filter.Value;
        set => _filter.Value = value;
    }

    /// <inheritdoc cref="IGorgonWicEncodingOptions.Dithering" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonWicEncodingOptions.Dithering" path="/remarks/para"/>
    /// <para>
    /// The default value is <see cref="ImageDithering.None"/>.
    /// </para>
    /// </remarks>
    public ImageDithering Dithering
    {
        get => _dithering.Value;
        set => _dithering.Value = value;
    }

    /// <inheritdoc/>
    public IGorgonOptionBag Options
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonPngEncodingOptions"/> class.
    /// </summary>
    public GorgonPngEncodingOptions() => Options = new GorgonOptionBag([_dithering, _filter, _interlacing, _dpiX, _dpiY]);

}
