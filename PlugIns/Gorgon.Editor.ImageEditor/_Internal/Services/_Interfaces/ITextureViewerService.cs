#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: January 9, 2019 1:59:21 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The service used to view the image as a texture.
    /// </summary>
    internal interface ITextureViewerService
        : IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the current zoom level.
        /// </summary>
        ZoomLevels ZoomLevel
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the texture parameters for rendering.
        /// </summary>
        /// <param name="image">The image to retrieve the parameters from.</param>
        void UpdateTextureParameters(IImageContent image);

        /// <summary>
        /// Function to draw the texture.
        /// </summary>
        void Draw(IImageContent image);

        /// <summary>
        /// Function to retrieve the size, in pixels, if the current mip level.
        /// </summary>
        /// <param name="image">The image to retrieve mip information from.</param>
        /// <returns>The width and height of the mip level.</returns>
        DX.Size2 GetMipSize(IImageContent image);

        /// <summary>
        /// Function to create the resources required for the viewer.
        /// </summary>
        /// <param name="backgroundImage">The image used for display in the background.</param>
        void CreateResources(GorgonTexture2DView backgroundImage);

        /// <summary>
        /// Function to update the texture.
        /// </summary>
        /// <param name="image">The image to upload to the texture.</param>
        void UpdateTexture(IImageContent image);
        #endregion
    }
}