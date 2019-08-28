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
// Created: June 27, 2016 11:18:56 PM
// 
#endregion


using System.Collections.Generic;
using Gorgon.Graphics.Imaging.Properties;
using SharpDX.WIC;

namespace Gorgon.Graphics.Imaging.Codecs
{
    /// <summary>
    /// A codec to handle read/writing of PNG files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This codec will read and write lossless compressed files using the Portable Network Graphics (PNG) format.
    /// </para>
    /// <para>
    /// This codec supports the following pixel formats:
    /// <list type="bullet">
    ///		<item>
    ///			<description><c>R8G8B8A8_UNorm</c></description>
    ///		</item>
    ///		<item>
    ///			<description><c>B8G8R8A8_UNorm</c></description>
    ///		</item>
    ///		<item>
    ///			<description><c>B8G8R8X8_UNorm</c></description>
    ///		</item>
    ///		<item>
    ///			<description><c>R16G16B16A16_UNorm</c></description>
    ///		</item>
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
    public sealed class GorgonCodecPng
        : GorgonCodecWic<GorgonPngEncodingOptions, IGorgonWicDecodingOptions>
    {
        #region Variables.
        // Supported formats.
        private readonly BufferFormat[] _supportedPixelFormats =
        {
            BufferFormat.R8G8B8A8_UNorm,
            BufferFormat.B8G8R8A8_UNorm,
            BufferFormat.B8G8R8X8_UNorm,
            BufferFormat.R16G16B16A16_UNorm
        };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the supported pixel formats for this codec.
        /// </summary>
        public override IReadOnlyList<BufferFormat> SupportedPixelFormats => _supportedPixelFormats;
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecPng"/> class.
        /// </summary>
        /// <param name="encodingOptions">[Optional] Options used when encoding the image data.</param>
        public GorgonCodecPng(GorgonPngEncodingOptions encodingOptions = null)
            : base("PNG", Resources.GORIMG_DESC_PNG_CODEC, new[] { "png" }, ContainerFormatGuids.Png, encodingOptions, null)
        {
        }
        #endregion
    }
}
