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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        #endregion
    }
}
