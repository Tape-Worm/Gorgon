
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
// Created: August 4, 2016 11:32:12 PM
// 

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// Options used when encoding an image to a stream as a JPEG file.
/// </summary>
public sealed class GorgonJpegEncodingOptions
    : IGorgonWicEncodingOptions
{
    // The quality of the compressed image.
    private readonly GorgonRangedOption<float> _imageQuality = GorgonOption.CreateSingleOption(nameof(ImageQuality), 1.0f, Resources.GORIMG_OPT_JPG_QUALITY, 0.0f, 1.0f);
    // The horizontal dots per inch.
    private readonly GorgonRangedOption<double> _dpiX = GorgonOption.CreateDoubleOption(nameof(DpiX), 72.0, Resources.GORIMG_OPT_WIC_DPIX);
    // The vertical dots per inch.
    private readonly GorgonRangedOption<double> _dpiY = GorgonOption.CreateDoubleOption(nameof(DpiY), 72.0, Resources.GORIMG_OPT_WIC_DPIY);

    /// <summary>
    /// The default encoding options for JPEG files.
    /// </summary>
    public static readonly GorgonJpegEncodingOptions Default = new();

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
    /// Property to set or return the quality of an image compressed with lossy compression.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this property to control the fidelity of an image compressed with lossy compression. A value of 0.0f will give the lowest quality and 1.0f will give the highest. Values outside of this range are 
    /// clamped.
    /// </para>
    /// <para>
    /// The default value is 1.0f.
    /// </para>
    /// </remarks>
    public float ImageQuality
    {
        get => _imageQuality.Value;
        set => _imageQuality.Value = value;
    }

    /// <inheritdoc/>
    public IGorgonOptionBag Options
    {
        get;
    }

    /// <inheritdoc/>
    ImageDithering IGorgonWicEncodingOptions.Dithering
    {
        get => ImageDithering.None;
        set
        {
            // Intentionally left blank.
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonJpegEncodingOptions"/> class.
    /// </summary>
    public GorgonJpegEncodingOptions() => Options = new GorgonOptionBag([_imageQuality, _dpiX, _dpiY]);

}
