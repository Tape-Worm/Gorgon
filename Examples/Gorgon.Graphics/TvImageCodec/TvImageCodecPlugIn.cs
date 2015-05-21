#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, November 03, 2014 9:29:01 PM
// 
#endregion

using Gorgon.IO;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// Our entry point into the TV image codec.
    /// </summary>
    /// <remarks>
    /// This plug-in will encode/decode images as 1 pixel per channel.  This will give the image an appearance similar to the line patterns on a CRT TV screen.  Well, somewhat.
    /// </remarks>
    public class TvImageCodecPlugIn
        : GorgonCodecPlugIn
    {
        #region Methods.
        /// <summary>
        /// Function to create an image codec object.
        /// </summary>
        /// <returns>
        /// The custom image codec.
        /// </returns>
        public override GorgonImageCodec CreateCodec()
        {
            return new TvImageCodec();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="TvImageCodecPlugIn"/> class.
        /// </summary>
        public TvImageCodecPlugIn()
            : base("A TV image codec, used for example only.")
        {
        }
        #endregion
    }
}
