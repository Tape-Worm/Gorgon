
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 29, 2016 10:46:00 PM
// 

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Options used when encoding an image to a stream as a GIF file.
/// </summary>
public class GorgonGifEncodingOptions
    : IGorgonWicEncodingOptions
{
    // The dithering to apply when converting to an indexed format.
    private readonly GorgonOption<ImageDithering> _dithering = GorgonOption.CreateOption(nameof(Dithering), ImageDithering.None, Resources.GORIMG_OPT_WIC_DITHERING);
    // Whether to save every array index as a frame.
    private readonly GorgonOption<bool> _saveAllFrames = GorgonOption.CreateOption(nameof(IGorgonImageCodecEncodingOptions.SaveAllFrames), true, Resources.GORIMG_OPT_SAVE_ALL_FRAMES);
    // The horizontal dots per inch.
    private readonly GorgonRangedOption<double> _dpiX = GorgonOption.CreateDoubleOption(nameof(DpiX), 72.0, Resources.GORIMG_OPT_WIC_DPIX);
    // The vertical dots per inch.
    private readonly GorgonRangedOption<double> _dpiY = GorgonOption.CreateDoubleOption(nameof(DpiY), 72.0, Resources.GORIMG_OPT_WIC_DPIY);
    // The custom palette.
    private readonly GorgonOption<IReadOnlyList<GorgonColor>> _palette = GorgonOption.CreateOption<IReadOnlyList<GorgonColor>>(nameof(Palette));
    // The alpha value below which a color is transparent.
    private readonly GorgonRangedOption<float> _alphaThreshold = GorgonOption.CreateSingleOption(nameof(AlphaThreshold), 1.0f, Resources.GORIMG_OPT_GIF_ALPHA_THRESHOLD, 0.0f, 1.0f);
    // The delay for each frame of animation.
    private readonly GorgonOption<IReadOnlyList<int>> _frameDelays = GorgonOption.CreateOption<IReadOnlyList<int>>(nameof(FrameDelays));

    /// <summary>
    /// The default encoding options for GIF files.
    /// </summary>
    public static readonly GorgonGifEncodingOptions Default = new();

    /// <inheritdoc/>
    public IGorgonOptionBag Options
    {
        get;
    }

    /// <summary>
    /// Property to set or return a custom color palette to apply to the GIF file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this to define a new palette for the 8 bit indexed image in the GIF file. This will be used to find a "best fit" set of colors when downsampling from a higher bit depth. 
    /// </para>
    /// <para>
    /// The default value is an empty list.
    /// </para>
    /// </remarks>
    public IReadOnlyList<GorgonColor> Palette
    {
        get => _palette.Value ?? [];
        set => _palette.Value = value;
    }

    /// <summary>
    /// Property to set or return the alpha threshold percentage for this codec.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this to determine what percentage of alpha channel values should be considered transparent for the GIF. A value of 0.5f will mean that colors with an alpha component less than 0.5f will 
    /// be considered transparent.
    /// </para>
    /// <para>
    /// The default value is 1.0f.
    /// </para>
    /// </remarks>
    public float AlphaThreshold
    {
        get => _alphaThreshold.Value;
        set => _alphaThreshold.Value = value;
    }

    /// <summary>
    /// Property to set or return a list of delays for a multi-frame GIF file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This specifies the delay, in 1/100 of a second, between each frame of animation for an animated GIF file. If this value is empty, then no delay will be applied between the 
    /// animation frames.
    /// </para>
    /// <para>
    /// This is used when the GIF file is an animated GIF, and its source <see cref="IGorgonImage"/> uses an array to store frames of animation. For a single frame GIF (i.e. a <see cref="IGorgonImage"/> 
    /// with an array count of 1), this value is ignored.
    /// </para> 
    /// <para>
    /// The default value is an empty list.
    /// </para>
    /// </remarks>
    public IReadOnlyList<int> FrameDelays
    {
        get => _frameDelays.Value ?? [];
        set => _frameDelays.Value = value;
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

    /// <inheritdoc cref="IGorgonImageCodecEncodingOptions.SaveAllFrames" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonImageCodecEncodingOptions.SaveAllFrames" path="/remarks/para"/>
    /// <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    public bool SaveAllFrames
    {
        get => _saveAllFrames.Value;
        set => _saveAllFrames.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGifEncodingOptions"/> class.
    /// </summary>
    public GorgonGifEncodingOptions() => Options = new GorgonOptionBag([_dithering, _saveAllFrames, _dpiX, _dpiY, _palette, _alphaThreshold, _frameDelays]);

}
