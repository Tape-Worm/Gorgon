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
// Created: May 7, 2019 12:12:47 AM
// 
#endregion

using System;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.Renderers.Services;

namespace Gorgon.Editor.TextureAtlasTool
{
    /// <summary>
    /// The parameters for the <see cref="ITextureAtlas"/> view model.
    /// </summary>
    internal class TextureAtlasParameters
        : ViewModelInjection
    {
        /// <summary>
        /// Property to return the project file system folder browser.
        /// </summary>
        public IFileSystemFolderBrowseService FolderBrowser
        {
            get;
        }

        /// <summary>
        /// Property to return the atlas generation service.
        /// </summary>
        public IGorgonTextureAtlasService AtlasGenerator
        {
            get;
        }

        /// <summary>
        /// Property to return the sprite file manager.
        /// </summary>
        public ISpriteFiles SpriteFiles
        {
            get;
        }

        /// <summary>
        /// Property to return the settings for the plug in.
        /// </summary>
        public TextureAtlasSettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the file I/O service.
        /// </summary>
        public IFileIOService FileManager
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="TextureAtlasParameters"/> class.</summary>
        /// <param name="settings">The settings for the plug in.</param>
        /// <param name="spriteFiles">The sprite file manager.</param>
        /// <param name="atlasGen">The atlas generation service.</param>
        /// <param name="fileService">The service used for file I/O operations.</param>
        /// <param name="folderBrowser">The folder browser.</param>
        /// <param name="commonServices">The common services for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public TextureAtlasParameters(TextureAtlasSettings settings, ISpriteFiles spriteFiles, IGorgonTextureAtlasService atlasGen, IFileIOService fileService, IFileSystemFolderBrowseService folderBrowser, IViewModelInjection commonServices)
            : base(commonServices)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SpriteFiles = spriteFiles ?? throw new ArgumentNullException(nameof(spriteFiles));
            AtlasGenerator = atlasGen ?? throw new ArgumentNullException(nameof(atlasGen));
            FolderBrowser = folderBrowser ?? throw new ArgumentNullException(nameof(folderBrowser));
            FileManager = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }
    }
}
