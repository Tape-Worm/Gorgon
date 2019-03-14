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
// Created: March 2, 2019 2:09:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Content view model for a sprite.
    /// </summary>
    internal class SpriteContent
        : EditorContentCommon<SpriteContentParameters>, ISpriteContent
    {
        #region Constants.
        /// <summary>
        /// The attribute key name for the sprite codec attribute.
        /// </summary>
        public const string CodecAttr = "SpriteCodec";
        #endregion

        #region Variables.
        // The sprite being edited.
        private GorgonSprite _sprite;
        // The undo service.
        private IUndoService _undoService;
        // The texture file associated with the sprite.
        private IContentFile _textureFile;
        // The file manager used to access external content files.
        private IContentFileManager _contentFiles;
        // The file system for temporary working data.
        private IGorgonFileSystemWriter<Stream> _scratchArea;        
        #endregion

        #region Properties.
        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => SpriteEditorCommonConstants.ContentType;

        /// <summary>
        /// Property to set or return the texture coordinates used by the sprite.
        /// </summary>
        public DX.RectangleF TextureCoordinates
        {
            get => _sprite.TextureRegion;
            set
            {
                if (_sprite.TextureRegion.Equals(ref value))
                {
                    return;
                }

                OnPropertyChanging();
                _sprite.TextureRegion = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the texture associated with the sprite.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get => _sprite.Texture;
            private set
            {
                if (_sprite.Texture == value)
                {
                    return;
                }

                OnPropertyChanging();
                _sprite.Texture = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set up a texture file that is associated with the sprite.
        /// </summary>
        /// <param name="textureFile"></param>
        private void SetupTextureFile(IContentFile textureFile)
        {
            if (_textureFile != null)
            {
                _textureFile.Deleted -= TextureFile_Deleted;
                _textureFile.IsOpen = false;
            }

            _textureFile = textureFile;

            if (_textureFile == null)
            {
                return;
            }

            _textureFile.Deleted += TextureFile_Deleted;
            _textureFile.IsOpen = true;
        }

        /// <summary>Handles the Deleted event of the TextureFile control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextureFile_Deleted(object sender, EventArgs e)
        {
            Texture?.Dispose();
            Texture = null;
        }

        /// <summary>Function to initialize the content.</summary>
        /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
        protected override void OnInitialize(SpriteContentParameters injectionParameters)
        {
            base.OnInitialize(injectionParameters);

            _contentFiles = injectionParameters.ContentFileManager ?? throw new ArgumentMissingException(nameof(injectionParameters.ContentFileManager), nameof(injectionParameters));
            _sprite = injectionParameters.Sprite ?? throw new ArgumentMissingException(nameof(injectionParameters.Sprite), nameof(injectionParameters));
            _undoService = injectionParameters.UndoService ?? throw new ArgumentMissingException(nameof(injectionParameters.UndoService), nameof(injectionParameters));
            _scratchArea = injectionParameters.ScratchArea ?? throw new ArgumentMissingException(nameof(injectionParameters.ScratchArea), nameof(injectionParameters));

            SetupTextureFile(injectionParameters.SpriteTextureFile);
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        public override void OnLoad()
        {
            base.OnLoad();

            // Mark this file as open in the editor.
            if (_textureFile != null)
            {
                _textureFile.IsOpen = true;
            }            
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            SetupTextureFile(_textureFile);
            _sprite?.Texture?.Dispose();

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
