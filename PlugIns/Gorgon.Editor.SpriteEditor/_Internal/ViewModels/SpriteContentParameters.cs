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
// Created: March 14, 2019 8:35:00 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.IO;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Parameters for the <see cref="ISpriteContent"/> view model.
    /// </summary>
    internal class SpriteContentParameters
        : OLDE_ContentViewModelInjectionCommon
    {
        /// <summary>
        /// Property to return the sprite being edited.
        /// </summary>
        public GorgonSprite Sprite
        {
            get;
        }

        /// <summary>
        /// Property to return the file manager used to access external content.
        /// </summary>
        public OLDE_IContentFileManager ContentFileManager
        {
            get;
        }

        /// <summary>
        /// Property to return the content file for the sprite texture.
        /// </summary>
        public OLDE_IContentFile SpriteTextureFile
        {
            get;
        }


        /// <summary>
        /// Property to return the undo service for the editor.
        /// </summary>
        public IUndoService UndoService
        {
            get;
        }

        /// <summary>
        /// Property to return the file system used to write out temporary working data.
        /// </summary>
        public IGorgonFileSystemWriter<Stream> ScratchArea
        {
            get;
        }

        /// <summary>
        /// Property to return the texture service used to read sprite texture data.
        /// </summary>
        public ISpriteTextureService TextureService
        {
            get;
        }

        /// <summary>
        /// Property to return the codec to use when reading/writing sprite data.
        /// </summary>
        public IGorgonSpriteCodec SpriteCodec
        {
            get;
        }

        /// <summary>
        /// Property to return the manual rectangle editor view model.
        /// </summary>
        public IManualRectangleEditor ManualRectangleEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the manual vertex editor view model.
        /// </summary>
        public IManualVertexEditor ManualVertexEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the sprite picker mask color editor.
        /// </summary>
        public ISpritePickMaskEditor SpritePickMaskEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the sprite texture wrapping state editor.
        /// </summary>
        public ISpriteWrappingEditor SpriteWrappingEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the settings view model.
        /// </summary>
        public IEditorPlugInSettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the factory used to build sprite content data.
        /// </summary>
        public ISpriteContentFactory Factory
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the sprite color editor.
        /// </summary>
        public ISpriteColorEdit ColorEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the sprite anchor editor.
        /// </summary>
        public ISpriteAnchorEdit AnchorEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the sampler builder service.
        /// </summary>
        public ISamplerBuildService SamplerBuilder
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentParameters"/> class.</summary>
        /// <param name="factory">The factory for building sprite content data.</param>
        /// <param name="spriteFile">The sprite file.</param>
        /// <param name="spriteTextureFile">The sprite texture file.</param>
        /// <param name="fileManager">The file manager used to access external content.</param>
        /// <param name="textureService">The texture service to use when reading sprite texture data.</param>
        /// <param name="sprite">The sprite being edited.</param>
        /// <param name="codec">The codec to use when reading/writing sprite data.</param>
        /// <param name="manualRectEdit">The manual rectangle editor view model.</param>
        /// <param name="manualVertexEdit">The manual vertex editor view model.</param>
        /// <param name="spritePickMaskEditor">The sprite picker mask color editor.</param>
        /// <param name="colorEditor">The color editor for the sprite.</param>
        /// <param name="anchorEditor">The anchor editor for the sprite.</param>
        /// <param name="wrapEditor">The texture wrapping state editor for the sprite.</param>
        /// <param name="settings">The plug in settings view model.</param>
        /// <param name="undoService">The undo service.</param>
        /// <param name="scratchArea">The file system used to write out temporary working data.</param>
        /// <param name="commonServices">Common application services.</param>
        public SpriteContentParameters(
            ISpriteContentFactory factory,
            OLDE_IContentFile spriteFile,
            OLDE_IContentFile spriteTextureFile,
            OLDE_IContentFileManager fileManager,
            ISpriteTextureService textureService,
            GorgonSprite sprite,
            IGorgonSpriteCodec codec,
            IManualRectangleEditor manualRectEdit,
            IManualVertexEditor manualVertexEdit,
            ISpritePickMaskEditor spritePickMaskEditor,
            ISpriteColorEdit colorEditor,
            ISpriteAnchorEdit anchorEditor,
            ISpriteWrappingEditor wrapEditor,
            ISamplerBuildService samplerBuilder,
            IEditorPlugInSettings settings,
            IUndoService undoService,
            IGorgonFileSystemWriter<Stream> scratchArea,
            IViewModelInjection commonServices)
            : base(spriteFile, commonServices)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
            UndoService = undoService ?? throw new ArgumentNullException(nameof(undoService));
            ContentFileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            ScratchArea = scratchArea ?? throw new ArgumentNullException(nameof(scratchArea));
            TextureService = textureService ?? throw new ArgumentNullException(nameof(textureService));
            SpriteCodec = codec ?? throw new ArgumentNullException(nameof(codec));
            ManualRectangleEditor = manualRectEdit ?? throw new ArgumentNullException(nameof(manualRectEdit));
            ManualVertexEditor = manualVertexEdit ?? throw new ArgumentNullException(nameof(manualVertexEdit));
            SpritePickMaskEditor = spritePickMaskEditor ?? throw new ArgumentNullException(nameof(spritePickMaskEditor));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ColorEditor = colorEditor ?? throw new ArgumentNullException(nameof(colorEditor));
            AnchorEditor = anchorEditor ?? throw new ArgumentNullException(nameof(anchorEditor));
            SpriteWrappingEditor = wrapEditor ?? throw new ArgumentNullException(nameof(wrapEditor));
            SamplerBuilder = samplerBuilder ?? throw new ArgumentNullException(nameof(samplerBuilder));
            SpriteTextureFile = spriteTextureFile;
        }
    }
}
