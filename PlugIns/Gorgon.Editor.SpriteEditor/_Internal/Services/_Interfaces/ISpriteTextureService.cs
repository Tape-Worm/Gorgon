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
// Created: March 18, 2019 11:40:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Functionality for handling an associated sprite texture.
    /// </summary>
    internal interface ISpriteTextureService
    {
        /// <summary>
        /// Function to determine if the content file passed in is image content as supported by this editor.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns><b>true</b> if the file is an image, supported by this editor, or <b>false</b> if not.</returns>
        bool IsContentImage(IContentFile file);

        /// <summary>
        /// Function to retrieve the image data for a sprite texture as a 32 bit RGBA pixel data.
        /// </summary>
        /// <param name="texture">The texture to extract the data from.</param>
        /// <param name="arrayIndex">The index in the array to use.</param>
        /// <returns>The image data for the texture.</returns>
        Task<IGorgonImage> GetSpriteTextureImageDataAsync(GorgonTexture2DView texture, int arrayIndex);

        /// <summary>
        /// Function to load a texture from the file system.
        /// </summary>
        /// <param name="file">The content file for the texture.</param>
        /// <returns>The texture from the file system.</returns>
        Task<GorgonTexture2DView> LoadTextureAsync(IContentFile file);

        /// <summary>
        /// Function to load an associated sprite texture for sprite content.
        /// </summary>
        /// <param name="spriteContent">The sprite content file to use.</param>        
        /// <returns>The texture associated with the sprite, and the content file associated with that texture, or <b>null</b> if no sprite texture was found.</returns>
        Task<(GorgonTexture2DView, IContentFile)> LoadFromSpriteContentAsync(IContentFile spriteContent);

        /// <summary>
        /// Function to retrieve the image metadata for a content file.
        /// </summary>
        /// <param name="file">The file to retrieve metadata from.</param>
        /// <returns>The metadata for the file, or <b>null</b> if the file is not an image.</returns>
        IGorgonImageInfo GetImageMetadata(IContentFile file);
    }
}
