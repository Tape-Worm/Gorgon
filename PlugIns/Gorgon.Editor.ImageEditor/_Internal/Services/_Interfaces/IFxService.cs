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
// Created: March 1, 2020 8:20:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The service used to apply effects and generate previews for effects.
    /// </summary>
    internal interface IFxService
        : IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to return the image that will contain the effect output.
        /// </summary>
        IGorgonImage EffectImage
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to apply the current effect that is using a preview.
        /// </summary>
        void ApplyPreviewedEffect();

        /// <summary>
        /// Function to apply the invert effect.
        /// </summary>
        void ApplyInvert();

        /// <summary>
        /// Function to apply the grayscale effect.
        /// </summary>
        void ApplyGrayScale();

        /// <summary>
        /// Function to assign an image for editing.
        /// </summary>
        /// <param name="image">The image to edit.</param>        
        /// <param name="arrayDepth">The selected array index or depth slice (volume textures).</param>
        /// <param name="mipLevel">The currently selected mip map level.</param>
        void SetImage(IGorgonImage image, int arrayDepth, int mipLevel);
        #endregion
    }
}
