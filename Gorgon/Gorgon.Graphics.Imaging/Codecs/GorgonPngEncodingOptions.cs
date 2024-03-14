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
// Created: June 28, 2016 10:38:40 PM
// 
#endregion

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;
using SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Filter to apply for compression optimization.
/// </summary>
public enum PngFilter
{
    /// <summary>
    /// The system will chose the best filter based on the image data.
    /// </summary>
    DontCare = PngFilterOption.Unspecified,
    /// <summary>
    /// No filtering.
    /// </summary>
    None = PngFilterOption.None,
    /// <summary>
    /// Sub filtering.
    /// </summary>
    Sub = PngFilterOption.Sub,
    /// <summary>
    /// Up filtering.
    /// </summary>
    Up = PngFilterOption.Up,
    /// <summary>
    /// Average filtering.
    /// </summary>
    Average = PngFilterOption.Average,
    /// <summary>
    /// Paeth filtering.
    /// </summary>
    Paeth = PngFilterOption.Paeth,
    /// <summary>
    /// Adaptive filtering.  The system will choose the best filter based on a per-scanline basis.
    /// </summary>
    Adaptive = PngFilterOption.Adaptive
}

/// <summary>
/// Options used when encoding an image to a stream as a PNG file..
/// </summary>
public sealed class GorgonPngEncodingOptions
    : IGorgonWicEncodingOptions
{
    #region Properties.
    /// <summary>
    /// Property to set or return whether all frames in an image array should be persisted.
    /// </summary>
    /// <remarks>
    /// This flag is not supported by this codec and will always return <b>false</b>.
    /// </remarks>
    bool IGorgonImageCodecEncodingOptions.SaveAllFrames
    {
        get => false;
        set
        {
            // Intentionally left blank.
        }
    }

    /// <summary>
    /// Property to set or return the horizontal dots-per-inch for the encoded image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This information is metadata only, no action is taken with this value.
    /// </para>
    /// <para>
    /// The default value is 72.
    /// </para>
    /// </remarks>
    public double DpiX
    {
        get => Options.GetOptionValue<double>(nameof(DpiX));
        set => Options.SetOptionValue(nameof(DpiX), value);
    }

    /// <summary>
    /// Property to set or return the vertical dots-per-index for the encoded image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This information is metadata only, no action is taken with this value.
    /// </para>
    /// <para>
    /// The default value is 72.
    /// </para>
    /// </remarks>
    public double DpiY
    {
        get => Options.GetOptionValue<double>(nameof(DpiY));
        set => Options.SetOptionValue(nameof(DpiY), value);
    }

    /// <summary>
    /// Property to set or return whether to use interlacing when encoding an image as a PNG file.
    /// </summary>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool Interlacing
    {
        get => Options.GetOptionValue<bool>(nameof(Interlacing));
        set => Options.SetOptionValue(nameof(Interlacing), value);
    }

    /// <summary>
    /// Property to set or return the type of filter to use when when compressing the PNG file.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="PngFilter.DontCare"/>.
    /// </remarks>
    public PngFilter Filter
    {
        get => Options.GetOptionValue<PngFilter>(nameof(Filter));
        set => Options.SetOptionValue(nameof(Filter), value);
    }

    /// <summary>
    /// Property to set or return the type of <see cref="ImageDithering"/> to use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This flag is used to determine which type of dithering algorithm should be used when converting the bit depth for a pixel format to a lower bit depth. If the pixel format of the image is supported 
    /// natively by the codec, then this value will be ignored.
    /// </para> 
    /// <para> 
    /// With dithering applied, the image will visually appear closer to the original by using patterns to simulate a greater number of colors.
    /// </para>
    /// <para>
    /// The default value is <see cref="ImageDithering.None"/>.
    /// </para>
    /// </remarks>
    public ImageDithering Dithering
    {
        get => Options.GetOptionValue<ImageDithering>(nameof(Dithering));
        set => Options.SetOptionValue(nameof(Dithering), value);
    }

    /// <summary>
    /// Property to return the list of options available to the codec.
    /// </summary>
    public IGorgonOptionBag Options
    {
        get;
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonPngEncodingOptions"/> class.
    /// </summary>
    public GorgonPngEncodingOptions() => Options = new GorgonOptionBag(
                                      [
                                          GorgonOption.CreateOption(nameof(Dithering), ImageDithering.None, Resources.GORIMG_OPT_WIC_DITHERING),
                                          GorgonOption.CreateOption(nameof(Filter), PngFilter.None, Resources.GORIMG_OPT_PNG_FILTERING),
                                          GorgonOption.CreateOption(nameof(Interlacing), false, Resources.GORIMG_OPT_PNG_INTERLACED),
                                          GorgonOption.CreateOption(nameof(DpiX), 72, Resources.GORIMG_OPT_WIC_DPIX),
                                          GorgonOption.CreateOption(nameof(DpiY), 72, Resources.GORIMG_OPT_WIC_DPIY)
                                      ]);
    #endregion
}
