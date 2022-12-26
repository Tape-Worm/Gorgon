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
// Created: August 4, 2016 11:32:12 PM
// 
#endregion

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Options used when encoding an image to a stream as a JPEG file.
/// </summary>
public sealed class GorgonJpegEncodingOptions
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
    /// Property to set or return the quality of an image compressed with lossy compression.
    /// </summary>
    /// <remarks>
    /// Use this property to control the fidelity of an image compressed with lossy compression. A value of 0.0f will give the lowest quality and 1.0f will give the highest.
    /// </remarks>
    public float ImageQuality
    {
        get => Options.GetOptionValue<float>(nameof(ImageQuality));
        set
        {
            if (value < 0.0f)
            {
                value = 0.0f;
            }
            if (value > 1.0f)
            {
                value = 1.0f;
            }

            Options.SetOptionValue(nameof(ImageQuality), value);
        }
    }

    /// <summary>
    /// Property to return the list of options available to the codec.
    /// </summary>
    public IGorgonOptionBag Options
    {
        get;
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
    /// </remarks>
    ImageDithering IGorgonWicEncodingOptions.Dithering
    {
        get => ImageDithering.None;

        set
        {
            // Intentionally left blank.
        }
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonJpegEncodingOptions"/> class.
    /// </summary>
    public GorgonJpegEncodingOptions() => Options = new GorgonOptionBag(new[]
                                      {
                                          GorgonOption.CreateSingleOption(nameof(ImageQuality), 1.0f, Resources.GORIMG_OPT_JPG_QUALITY, 0, 1.0f),
                                          GorgonOption.CreateDoubleOption(nameof(DpiX), 72.0, Resources.GORIMG_OPT_WIC_DPIX),
                                          GorgonOption.CreateDoubleOption(nameof(DpiY), 72.0, Resources.GORIMG_OPT_WIC_DPIY)
                                      });
    #endregion
}
