
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
// Created: June 29, 2016 10:46:00 PM
// 

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Options used when decoding an image from a stream as a GIF file
/// </summary>
/// <remarks>
/// <para>
/// When decoding a GIF file into a <see cref="GorgonImage"/>, the <see cref="IGorgonWicDecodingOptions.Dithering"/> property is ignored
/// </para>
/// </remarks>
public class GorgonGifDecodingOptions
    : IGorgonWicDecodingOptions
{
    /// <summary>
    /// The default decoding options for GIF files.
    /// </summary>
    public static readonly GorgonGifDecodingOptions Default = new();

    /// <inheritdoc/>
    WICFlags IGorgonWicDecodingOptions.Flags
    {
        get => WICFlags.None;
        set
        {
            // Intentionally left blank.
        }
    }

    /// <inheritdoc/>
    public bool ReadAllFrames
    {
        get => Options.GetOptionValue<bool>(nameof(ReadAllFrames));
        set => Options.SetOptionValue(nameof(ReadAllFrames), value);
    }

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
    /// Use this to define a new palette for the 8 bit indexed image in the GIF file during decoding.
    /// </para>
    /// <para>
    /// This value does not remap the pixel values to the corresponding palette color, therefore, palettes assigned to the image may not give the desired results without careful palette selection.
    /// </para>
    /// <para>
    /// This value is ignored when the GIF file has multiple frames of animation.
    /// </para>
    /// <para>
    /// The default value is an empty list.
    /// </para>
    /// </remarks>
    public IList<GorgonColor> Palette
    {
        get => Options.GetOptionValue<IList<GorgonColor>>(nameof(Palette)) ?? [];
        set => Options.SetOptionValue(nameof(Palette), value);
    }

    /// <summary>
    /// Property to set or return the alpha threshold percentage for this codec.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this to determine what percentage of alpha channel values should be considered transparent for the GIF.  A value of 0.5f will mean that colors with an alpha component less than 0.5f will 
    /// be considered transparent.
    /// </para>
    /// <para>
    /// This value does not apply to GIF files with multiple frames.
    /// </para>
    /// <para>
    /// The default value is 0.0f.
    /// </para>
    /// </remarks>
    public float AlphaThreshold
    {
        get => Options.GetOptionValue<float>(nameof(AlphaThreshold));
        set => Options.SetOptionValue(nameof(AlphaThreshold), value);
    }

    /// <inheritdoc/>
    ImageDithering IGorgonWicDecodingOptions.Dithering
    {
        get => ImageDithering.None;
        set
        {
            // Intentionally left blank.
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGifDecodingOptions"/> class.
    /// </summary>
    public GorgonGifDecodingOptions() => Options = new GorgonOptionBag(
                                      [
                                          GorgonOption.CreateOption(nameof(ReadAllFrames), false, Resources.GORIMG_OPT_READ_ALL_FRAMES),
                                          GorgonOption.CreateOption<IList<GorgonColor>>(nameof(Palette), []),
                                          GorgonOption.CreateSingleOption(nameof(AlphaThreshold), 0.0f, Resources.GORIMG_OPT_GIF_ALPHA_THRESHOLD, 0.0f, 1.0f)
                                      ]);

}
