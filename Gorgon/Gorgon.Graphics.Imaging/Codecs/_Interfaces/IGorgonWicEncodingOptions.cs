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
// Created: June 28, 2016 10:41:11 PM
// 
#endregion

namespace Gorgon.Graphics.Imaging.Codecs
{
    /// <summary>
    /// Provides options used when encoding a <see cref="IGorgonImage"/> for persistence.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This particular interface provides common WIC (Windows Imaging Component) specific options for use when encoding an image across multiple image formats.
    /// </para>
    /// </remarks>
    public interface IGorgonWicEncodingOptions
        : IGorgonImageCodecEncodingOptions
    {
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
        ImageDithering Dithering
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the horizontal dots-per-inch for the encoded image.
        /// </summary>
        double DpiX
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the vertical dots-per-index for the encoded image.
        /// </summary>
        double DpiY
        {
            get;
            set;
        }
    }
}
