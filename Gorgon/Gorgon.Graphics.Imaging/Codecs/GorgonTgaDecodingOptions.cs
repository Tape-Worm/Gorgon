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
// Created: August 15, 2016 11:30:16 PM
// 
#endregion

using Gorgon.Configuration;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging.Codecs
{
    /// <summary>
    /// Options used when decoding an image from a stream as a TGA file.
    /// </summary>
    public class GorgonTgaDecodingOptions
        : IGorgonImageCodecDecodingOptions
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
        /// Property to set or return whether to read all frames from an image.
        /// </summary>
        /// <remarks>
        /// Some codecs do not use multiple frames in their image data. For those codecs, this flag will be ignored.
        /// </remarks>
        bool IGorgonImageCodecDecodingOptions.ReadAllFrames
        {
            get => false;
            set
            {
                // Intentionally left blank.
            }
        }

        /// <summary>
        /// Property to set or return whether the to force alpha values of 0 in the image to be fully opaque.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some TGA encoded images write out 32 bit images with an alpha value of 0 even though the image has fully opaque color data. This causes an image to appear to be completely transparent when it 
        /// shouldn't be. Use this flag to force the image to set the alpha channel to fully opaque in such cases.
        /// </para>
        /// <para>
        /// Note that this only works on images where all the alpha values are set to 0. If there is a mix of alpha values in the image, then this option will have no effect.
        /// </para>
        /// <para>
        /// The default value is <b>true</b>.
        /// </para>
        /// </remarks>
        public bool SetZeroAlphaAsOpaque
        {
            get => Options.GetOptionValue<bool>(nameof(SetZeroAlphaAsOpaque));

            set => Options.SetOptionValue(nameof(SetZeroAlphaAsOpaque), value);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTgaDecodingOptions"/> class.
        /// </summary>
        public GorgonTgaDecodingOptions() => Options = new GorgonOptionBag(new[]
                                          {
                                              GorgonOption.CreateOption(nameof(SetZeroAlphaAsOpaque), true, Resources.GORIMG_OPT_TGA_SETZEROALPHA_OPAQUE)
                                          });
        #endregion
    }
}
