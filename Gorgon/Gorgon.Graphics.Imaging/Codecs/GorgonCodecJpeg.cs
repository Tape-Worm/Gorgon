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
// Created: August 4, 2016 11:27:19 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Graphics.Imaging.Properties;
using SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs;

/// <summary>
/// A codec to handle read/writing of JPEG files.
/// </summary>
/// <remarks>
/// <para>
/// This codec will read and write lossy compression files using the Joint Photographics Experts Group (JPEG) format.
/// </para>
/// <para>
/// This codec supports the following pixel formats:
/// <list type="bullet">
///		<item>
///			<description><see cref="BufferFormat.B8G8R8X8_UNorm"/></description>
///		</item>
///		<item>
///			<description><see cref="BufferFormat.B8G8R8A8_UNorm"/> (Alpha channel is ignored)</description>
///		</item>
///        <item>
///            <description><see cref="BufferFormat.R8G8B8A8_UNorm"/> (Alpha channel is ignored)</description>
///        </item>
/// </list>
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// This codec requires the Windows Imaging Components (WIC) to be installed for the operating system.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonCodecJpeg" /> class.
/// </remarks>
/// <param name="encodingOptions">[Optional] Options to use when encoding a JPEG image.</param>
public sealed class GorgonCodecJpeg(GorgonJpegEncodingOptions encodingOptions = null)
        : GorgonCodecWic<GorgonJpegEncodingOptions, IGorgonWicDecodingOptions>("JPEG", Resources.GORIMG_DESC_JPG_CODEC, ["jpg", "jpeg", "jpe", "jif", "jfif", "jfi"], ContainerFormatGuids.Jpeg, encodingOptions, null)
{
    #region Variables.
    // Supported formats.
    private readonly BufferFormat[] _supportedFormats =
    [
        BufferFormat.R8G8B8A8_UNorm,
        BufferFormat.B8G8R8A8_UNorm,
        BufferFormat.B8G8R8X8_UNorm
    ];

    // Image quality for lossy compressed images.
    private float _imageQuality = 1.0f;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the supported pixel formats for this codec.
    /// </summary>
    public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _supportedFormats;

    /// <summary>
    /// Property to set or return the quality of an image compressed with lossy compression.
    /// </summary>
    /// <remarks>
    /// Use this property to control the fidelity of an image compressed with lossy compression.  0.0f will give the 
    /// lowest quality and 1.0f will give the highest.
    /// </remarks>
    public float ImageQuality
    {
        get => _imageQuality;
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

            _imageQuality = value;
        }
    }

    #endregion
    #region Constructor/Destructor.
    #endregion
}
