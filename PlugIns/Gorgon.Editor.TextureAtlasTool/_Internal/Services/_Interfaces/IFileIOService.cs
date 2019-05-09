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
// Created: May 8, 2019 11:45:34 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.Content;
using Gorgon.Renderers;
using Gorgon.Renderers.Services;

namespace Gorgon.Editor.TextureAtlasTool
{
    /// <summary>
    /// The service used for file operations.
    /// </summary>
    internal interface IFileIOService
    {
        /// <summary>
        /// Function to determine if a directory exists or not.
        /// </summary>
        /// <param name="path">The path to the directory/</param>
        /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Function to load the sprites for atlas generation.
        /// </summary>
        /// <param name="files">The files that hold the sprite data.</param>
        /// <returns>A list of sprites and their associated files.</returns>
        IReadOnlyDictionary<IContentFile, GorgonSprite> LoadSprites(IEnumerable<IContentFile> files);

        /// <summary>
        /// Function to save the atlas data.
        /// </summary>
        /// <param name="spriteFiles">The original sprite files.</param>
        /// <param name="atlas">The atlas data to save.</param>
        void SaveAtlas(IReadOnlyDictionary<IContentFile, GorgonSprite> spriteFiles, GorgonTextureAtlas atlas);

        /// <summary>
        /// Function to determine if there are any existing files that match the texture names or sprite names.
        /// </summary>
        /// <param name="atlas">The texture atlas to evaluate.</param>
        /// <returns><b>true</b> if there are any sprites or textures with the same name in the output area, or <b>false</b> if not.</returns>
        bool HasExistingFiles(GorgonTextureAtlas atlas);
    }
}
