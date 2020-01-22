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
// Created: April 24, 2019 11:05:37 PM
// 
#endregion

using System;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// Parameters to pass to the <see cref="IExtract"/> view model.
    /// </summary>
    internal class ExtractParameters
        : ViewModelInjection
    {
        /// <summary>
        /// Property to return the plug in settings.
        /// </summary>
        public ExtractSpriteToolSettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the data structure that will hold the information used to extract sprites.
        /// </summary>
        public SpriteExtractionData Data
        {
            get;
        }

        /// <summary>
        /// Property to return the file containing the texture data.
        /// </summary>
        public OLDE_IContentFile TextureFile
        {
            get;
        }

        /// <summary>
        /// Property to return the extractor used to build the sprites.
        /// </summary>
        public IExtractorService Extractor
        {
            get;
        }

        /// <summary>
        /// Property to return the color picker service.
        /// </summary>
        public IColorPickerService ColorPicker
        {
            get;
        }

        /// <summary>
        /// Property to return the project file system folder browser.
        /// </summary>
        public IFileSystemFolderBrowseService FolderBrowser
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="ExtractParameters"/> class.</summary>
        /// <param name="settings">The plug in settings.</param>
        /// <param name="data">The data structure that will hold the information used to extract sprites.</param>
        /// <param name="textureFile">The file used as the texture.</param>
        /// <param name="extractor">The extractor used to build the sprites.</param>
        /// <param name="colorPicker">The color picker service.</param>
        /// <param name="folderBrowser">The project file system folder browser service.</param>
        /// <param name="commonServices">The common services.</param>
        /// <exception cref="ArgumentNullException">Thrown when the any parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the <see cref="SpriteExtractionData.Texture"/> property of the <paramref name="data"/> parameter is <b>null</b>.</exception>
        public ExtractParameters(ExtractSpriteToolSettings settings, SpriteExtractionData data, OLDE_IContentFile textureFile, IExtractorService extractor, IColorPickerService colorPicker, IFileSystemFolderBrowseService folderBrowser, IViewModelInjection commonServices)
            : base(commonServices)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            ColorPicker = colorPicker ?? throw new ArgumentNullException(nameof(colorPicker));
            FolderBrowser = folderBrowser ?? throw new ArgumentNullException(nameof(folderBrowser));
            TextureFile = textureFile ?? throw new ArgumentNullException(nameof(textureFile));

            if (data.Texture == null)
            {
                throw new ArgumentMissingException(nameof(data.Texture), nameof(data));
            }
        }
    }
}
