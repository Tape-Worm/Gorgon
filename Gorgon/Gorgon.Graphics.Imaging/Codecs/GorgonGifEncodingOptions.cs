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
// Created: June 29, 2016 10:46:00 PM
// 
#endregion

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Options used when decoding an image from a stream as a GIF file.
/// </summary>
public class GorgonGifEncodingOptions
    : IGorgonWicEncodingOptions
{
    #region Properties.
    /// <summary>
    /// Property to return the list of options available to the codec.
    /// </summary>
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
    /// This value is ignored when the GIF file has multiple frames of animation.
    /// </para>
    /// <para>
    /// The default value is an empty list.
    /// </para>
    /// </remarks>
    public IList<GorgonColor> Palette
    {
        get => Options.GetOptionValue<IList<GorgonColor>>(nameof(Palette));
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
        set => Options.SetOptionValue(nameof(AlphaThreshold), value.Max(0).Min(1));
    }

    /// <summary>
    /// Property to set or return a list of delays for a multi-frame GIF file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This specifies the delay, in 1/100 of a second, between each frame of animation for an animated GIF file. If this value is <b>null</b>, or empty, then no delay will be applied between the 
    /// animation frames.
    /// </para>
    /// <para>
    /// This is used when the GIF file is an animated GIF, and its source <see cref="IGorgonImage"/> uses an array to store frames of animation. For a single frame GIF (i.e. a <see cref="IGorgonImage"/> 
    /// with an array count of 1), this value is ignored.
    /// </para> 
    /// </remarks>
    public IList<int> FrameDelays
    {
        get => Options.GetOptionValue<IList<int>>(nameof(FrameDelays));
        set => Options.SetOptionValue(nameof(FrameDelays), value);
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
    /// For the <see cref="GorgonCodecGif"/> codec, only the <see cref="ImageDithering.ErrorDiffusion"/> dithering type is supported when there are values assigned to the <see cref="Palette"/> property.
    /// </para>
    /// </remarks>
    public ImageDithering Dithering
    {
        get => Options.GetOptionValue<ImageDithering>(nameof(Dithering));
        set => Options.SetOptionValue(nameof(Dithering), value);
    }

    /// <summary>
    /// Property to set or return the horizontal dots-per-inch for the encoded image.
    /// </summary>
    public double DpiX
    {
        get => Options.GetOptionValue<double>(nameof(DpiX));
        set => Options.SetOptionValue(nameof(DpiX), value);
    }

    /// <summary>
    /// Property to set or return the vertical dots-per-index for the encoded image.
    /// </summary>
    public double DpiY
    {
        get => Options.GetOptionValue<double>(nameof(DpiY));
        set => Options.SetOptionValue(nameof(DpiY), value);
    }

    /// <summary>
    /// Property to set or return whether all frames in an image array should be persisted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This flag only applies when the <see cref="GorgonImage"/> being saved as a GIF file has more than 1 array level.
    /// </para>
    /// <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    public bool SaveAllFrames
    {
        get => Options.GetOptionValue<bool>(nameof(IGorgonImageCodecEncodingOptions.SaveAllFrames));
        set => Options.SetOptionValue(nameof(IGorgonImageCodecEncodingOptions.SaveAllFrames), value);
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGifEncodingOptions"/> class.
    /// </summary>
    public GorgonGifEncodingOptions() => Options = new GorgonOptionBag(
                                      [
                                          GorgonOption.CreateOption(nameof(Dithering), ImageDithering.None, Resources.GORIMG_OPT_WIC_DITHERING),
                                          GorgonOption.CreateOption(nameof(SaveAllFrames), true, Resources.GORIMG_OPT_SAVE_ALL_FRAMES),
                                          GorgonOption.CreateDoubleOption(nameof(DpiX), 72.0, Resources.GORIMG_OPT_WIC_DPIX),
                                          GorgonOption.CreateDoubleOption(nameof(DpiY), 72.0, Resources.GORIMG_OPT_WIC_DPIY),
                                          GorgonOption.CreateOption(nameof(Palette), new List<GorgonColor>()),
                                          GorgonOption.CreateSingleOption(nameof(AlphaThreshold), 1.0f, Resources.GORIMG_OPT_GIF_ALPHA_THRESHOLD, 0.0f, 1.0f),
                                          GorgonOption.CreateOption(nameof(FrameDelays), new List<int>())
                                      ]);
    #endregion
}
