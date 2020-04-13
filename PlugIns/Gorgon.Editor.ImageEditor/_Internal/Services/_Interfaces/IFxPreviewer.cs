#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: March 1, 2020 8:36:39 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A previewer for specific effects that support previewing.
    /// </summary>
    internal interface IFxPreviewer
    {
        #region Properties.
        /// <summary>
        /// Property to return the texture holding the unalterd image.
        /// </summary>
        GorgonTexture2DView OriginalTexture
        {
            get;
        }

        /// <summary>
        /// Property to return the texture that contains the blurred preview image.
        /// </summary>
        GorgonTexture2DView PreviewTexture
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to generate a blurred image preview.
        /// </summary>
        /// <param name="blurAmount">The amount to blur.</param>
        void GenerateBlurPreview(int blurAmount);

        /// <summary>
        /// Function to generate a sharpen or emboss image preview.
        /// </summary>
        /// <param name="amount">The amount to sharpen or emboss.</param>
        /// <param name="emboss"><b>true</b> to use the emboss effect, <b>false</b> to use sharpening.</param>
        void GenerateSharpenEmbossPreview(int amount, bool emboss);

        /// <summary>
        /// Function to generate an edge detection preview.
        /// </summary>
        /// <param name="threshold">The threshold for the detection.</param>
        /// <param name="offset">The offset for the edge lines.</param>
        /// <param name="color">The color of the edge lines.</param>
        /// <param name="overlay"><b>true</b> to overlay the edges on the original image, <b>false</b> to replace the image with edges.</param>
        void GenerateEdgeDetectPreview(int threshold, float offset, GorgonColor color, bool overlay);

        /// <summary>
        /// Function to generate a one bit effect preview.
        /// </summary>
        /// <param name="range">The threshold range of colors to consider as "on".</param>
        /// <param name="invert"><b>true</b> to invert the colors, <b>false</b> to leave as-is.</param>
        void GenerateOneBitPreview(GorgonRangeF range, bool invert);

        /// <summary>
        /// Function to generate a posterize effect preview.
        /// </summary>
        /// <param name="amount">The amount to posterize.</param>
        void GeneratePosterizePreview(int amount);
        #endregion
    }
}
