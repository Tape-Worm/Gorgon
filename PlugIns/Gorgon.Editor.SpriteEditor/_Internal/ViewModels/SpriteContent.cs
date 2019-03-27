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
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Math;
using System.Threading;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Content view model for a sprite.
    /// </summary>
    internal class SpriteContent
        : EditorContentCommon<SpriteContentParameters>, ISpriteContent
    {
        #region Classes.
        /// <summary>
        /// Arguments for an undo/redo operation.
        /// </summary>
        private class SpriteUndoArgs
        {
            /// <summary>
            /// The sprite texture coordinates, in pixel space.
            /// </summary>
            public DX.RectangleF TextureCoordinates;
            /// <summary>
            /// The current texture file associated with the sprite.
            /// </summary>
            public IContentFile CurrentTexture;
            /// <summary>
            /// The current texture array index.
            /// </summary>
            public int ArrayIndex;
        }
        #endregion

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
        // The sprite texture service.
        private ISpriteTextureService _textureService;
        // The codec used to read/write sprite data.
        private IGorgonSpriteCodec _spriteCodec;
        // The original texture.
        private IContentFile _originalTexture;
        // The currently active tool for editing the sprite.
        private SpriteEditTool _currentTool = SpriteEditTool.None;
        // The currently active editor sub panel.
        private EditorSubPanel _subPanel = EditorSubPanel.None;
        // The image data for the sprite texture.
        private IGorgonImage _imageData;
        #endregion

        #region Properties.
        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => SpriteEditorCommonConstants.ContentType;

        /// <summary>
        /// Property to return the view model for the plug in settings.
        /// </summary>
        public ISettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the manual input interface.
        /// </summary>
        public IManualRectInputVm ManualInput
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the sub panel to make active.
        /// </summary>
        public EditorSubPanel ActiveSubPanel
        {
            get => _subPanel;
            set
            {
                if (_subPanel == value)
                {
                    return;
                }

                OnPropertyChanging();
                _subPanel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the currently active tool for editing the sprite.
        /// </summary>
        public SpriteEditTool CurrentTool
        {
            get => _currentTool;
            set
            {
                if (_currentTool == value)
                {
                    return;
                }

                OnPropertyChanging();
                _currentTool = value;
                OnPropertyChanged();

                ActiveSubPanel = EditorSubPanel.None;

                NotifyPropertyChanged(nameof(SupportsArrayChange));
            }
        }

        /// <summary>
        /// Property to set or return the texture coordinates used by the sprite.
        /// </summary>
        public DX.RectangleF TextureCoordinates
        {
            get => _sprite.TextureRegion;
            private set
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
        /// Property to return the index of the texture array that the sprite uses.
        /// </summary>
        public int ArrayIndex
        {
            get => _sprite.TextureArrayIndex;
            private set
            {
                if (_sprite.TextureArrayIndex == value)
                {
                    return;
                }

                OnPropertyChanging();
                _sprite.TextureArrayIndex = value.Min((Texture?.Texture.ArrayCount ?? 1) - 1).Max(0);
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(ImageData));
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

                ContentState = ContentState.Modified;
                NotifyPropertyChanged(nameof(SupportsArrayChange));
            }
        }

        /// <summary>
        /// Property to return the buffer that contains the image data for the <see cref="Texture"/>, at the <see cref="ArrayIndex"/> associated with the sprite.
        /// </summary>
        public IGorgonImageBuffer ImageData => _imageData?.Buffers[0, _sprite.TextureArrayIndex.Max(0).Min(_imageData.ArrayCount - 1)];

        /// <summary>
        /// Property to return whether the currently loaded texture supports array changes.
        /// </summary>
        public bool SupportsArrayChange => (Texture != null) && (Texture.Texture.ArrayCount > 1) && ((CurrentTool == SpriteEditTool.SpritePick) || (CurrentTool == SpriteEditTool.SpriteClip));

        /// <summary>
        /// Property to set or return the size of the sprite.
        /// </summary>
        public DX.Size2F Size
        {
            get => _sprite.Size;
            private set
            {
                if (_sprite.Size.Equals(value))
                {
                    return;
                }

                OnPropertyChanging();
                _sprite.Size = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command used to undo an action.
        /// </summary>
        public IEditorCommand<object> UndoCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to redo an action.
        /// </summary>
        public IEditorCommand<object> RedoCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when applying texture coordinates to the sprite.
        /// </summary>
        /// <remarks>
        /// The command takes a tuple containing the texture coordinate values, and the texture array index to use.
        /// </remarks>
        public IEditorCommand<(DX.RectangleF, int)> SetTextureCoordinatesCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when picking a sprite.
        /// </summary>
        public IEditorCommand<object> SpritePickCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when creating a new sprite.
        /// </summary>
        public IEditorCommand<object> NewSpriteCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to reset the state of the view model.
        /// </summary>
        private void ResetState()
        {
            // TODO (In the command): 
            // 1. Prior to calling this, prompt for a sprite name.
            //    a. If OK, continue to 2
            //    b. If cancel, then stop.
            // 2. Call this method.
            // 3. Create a new content file with the name we specified, and save the empty sprite to it.
            // 4. Set content file.Metadata.Attributes[CommonEditorConstants.IsNewAttr] = "true"; <-- This value doesn't matter.
            // 5. Assign new content file to File property.
            //
            // If this messes up, we'll probably have corrupt state on our hands, so, if we can close the view, we should. May need to add some means of doing that.
            Texture?.Dispose();
            _imageData?.Dispose();
            SetupTextureFile(null);
            _originalTexture = null;

            // TODO: This should be done in the command, after the file is created.

            // Reset all state.
            _undoService.ClearStack();
            CurrentTool = SpriteEditTool.None;

            _sprite = new GorgonSprite();

#warning Do not forget to add all properties here.
            NotifyPropertyChanged(nameof(ArrayIndex));
            NotifyPropertyChanged(nameof(TextureCoordinates));
            NotifyPropertyChanged(nameof(ImageData));
            NotifyPropertyChanged(nameof(Texture));

            ContentState = ContentState.Unmodified;
        }

        /// <summary>
        /// Function to set up a texture file that is associated with the sprite.
        /// </summary>
        /// <param name="textureFile">The current texture file.</param>
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
            SetupTextureFile(null);
            Texture?.Dispose();
            Texture = null;            
        }


        /// <summary>
        /// Function to determine if an undo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
        private bool CanUndo() => _undoService.CanUndo && _currentTool == SpriteEditTool.None;

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => _undoService.CanRedo && _currentTool == SpriteEditTool.None;

        /// <summary>
        /// Function called when a redo operation is requested.
        /// </summary>
        private async void DoRedoAsync()
        {
            try
            {
                await _undoService.Redo();
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_REDO);
            }
        }

        /// <summary>
        /// Function called when an undo operation is requested.
        /// </summary>
        private async void DoUndoAsync()
        {
            try
            {
                await _undoService.Undo();
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UNDO);
            }
        }

        /// <summary>
        /// Function to determine if the current content can be saved.
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
        private bool CanSaveSprite(SaveReason saveReason) => (ContentState != ContentState.Unmodified) && ((CurrentTool == SpriteEditTool.None) || (saveReason != SaveReason.UserSave));

        /// <summary>
        /// Function to save the sprite back to the project file system.
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private Task DoSaveSpriteTask(SaveReason saveReason)
        {
            Stream outStream = null;

            BusyState.SetBusy();

            try
            {
                outStream = File.OpenWrite();
                _spriteCodec.Save(_sprite, outStream);
                outStream.Dispose();

                // This file is no longer "new".
                File.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);
                File.Refresh();
                File.SaveMetadata();

                _originalTexture = _textureFile;
                ContentState = ContentState.Unmodified;                
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_SAVE_SPRITE);
            }
            finally
            {
                outStream?.Dispose();
                BusyState.SetIdle();
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Function to determine if the texture coordinates can be updated for the sprite.
        /// </summary>
        /// <param name="coords">The texture coordinates and array index to use.</param>
        /// <returns><b>true</b> if the texture coordinates can be applied, <b>false</b> if not.</returns>
        private bool CanSetTextureCoordinates((DX.RectangleF textureCoords, int arrayIndex) coords) => (Texture != null)
            && (!coords.textureCoords.IsEmpty)
            && (coords.arrayIndex >= 0)
            && (coords.arrayIndex <= Texture.Texture.ArrayCount - 1)
            && ((!coords.textureCoords.ToRectangle().Equals(Texture.ToPixel(TextureCoordinates))) || (coords.arrayIndex != ArrayIndex));

        /// <summary>
        /// Function called to update the texture coordinates for the sprite.
        /// </summary>
        /// <param name="coords">The texture coordinates and array index to use.</param>
        private void DoSetTextureCoordinates((DX.RectangleF textureCoords, int arrayIndex) coords)
        {
            bool SetTextureCoordinates(DX.RectangleF coordinates, int index)
            {
                try
                {
                    ActiveSubPanel = EditorSubPanel.None;

                    TextureCoordinates = Texture.ToTexel(coordinates.ToRectangle());                    
                    Size = new DX.Size2F((int)coordinates.Size.Width, (int)coordinates.Size.Height);
                    ArrayIndex = index;
                    
                    ContentState = ContentState.Modified;

                    return true;
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                    return false;
                }
            }

            Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
            {
                SetTextureCoordinates(undoArgs.TextureCoordinates, undoArgs.ArrayIndex);
                return Task.CompletedTask;
            }

            Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
            {
                SetTextureCoordinates(redoArgs.TextureCoordinates, redoArgs.ArrayIndex);
                return Task.CompletedTask;
            }

            var texCoordUndoArgs = new SpriteUndoArgs
            {
                TextureCoordinates = Texture.ToPixel(TextureCoordinates).ToRectangleF(),
                ArrayIndex = ArrayIndex
            };
            var texCoordRedoArgs = new SpriteUndoArgs
            {
                TextureCoordinates = coords.textureCoords,
                ArrayIndex = coords.arrayIndex
            };

            if (!SetTextureCoordinates(coords.textureCoords, coords.arrayIndex))
            {
                return;                
            }            

            _undoService.Record(Resources.GORSPR_UNDO_DESC_CLIP, UndoAction, RedoAction, texCoordUndoArgs, texCoordRedoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }

        /// <summary>
        /// Function to determine if the sprite picker can activate or not.
        /// </summary>
        /// <returns><b>true</b> if it can activate, <b>false</b> if not.</returns>
        private bool CanSpritePick() => ((CurrentTool == SpriteEditTool.None) || (CurrentTool == SpriteEditTool.SpritePick))
                                            && (ImageData != null)
                                            && (ImageData.Format == BufferFormat.R8G8B8A8_UNorm);

        /// <summary>
        /// Function to activate (or deactivate) the sprite picker tool.
        /// </summary>
        private void DoSpritePick()
        {
            try
            {
                if (CurrentTool == SpriteEditTool.SpritePick)
                {
                    CurrentTool = SpriteEditTool.None;
                    return;
                }

                // Let the user know that performance may be an issue here with a large texture (32 bit 4096x4096 image should be around 67 MB).
                if ((Settings.ShowImageSizeWarning) && (ImageData.Data.SizeInBytes > 67108864))
                {
                    MessageDisplay.ShowWarning(string.Format(Resources.GORSPR_WRN_LARGE_IMAGE, ImageData.Width, ImageData.Height));
                }

                CurrentTool = SpriteEditTool.SpritePick;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, string.Format(Resources.GORSPR_ERR_TOOL_CHANGE, SpriteEditTool.SpritePick));
            }
        }

        /// <summary>
        /// Function to determine if the <see cref="ArrayIndex"/> can be updated.
        /// </summary>
        /// <param name="index">The new index.</param>
        /// <returns><b>true</b> if the index can be updated, <b>false</b> if not.</returns>
        private bool CanSetArrayIndex(int index) => ((CurrentTool == SpriteEditTool.SpriteClip) || (CurrentTool == SpriteEditTool.SpritePick)) 
            && (Texture != null) 
            && (Texture.Texture.ArrayCount > 0) 
            && (index >= 0) 
            && (index <= Texture.Texture.ArrayCount - 1);

        /// <summary>
        /// Function to set the new array index for the sprite.
        /// </summary>
        /// <param name="index">The new index to set.</param>
        private void DoSetArrayIndex(int index)
        {
            try
            {
                ArrayIndex = index;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>Function to determine the action to take when this content is closing.</summary>
        /// <returns>
        ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
        /// <remarks>Plugin authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
        protected override async Task<bool> OnCloseContentTask()
        {
            if (ContentState == ContentState.Unmodified)
            {
                return true;
            }

            MessageResponse response = MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_CLOSE, File.Name), allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    await DoSaveSpriteTask(SaveReason.ContentShutdown);
                    return true;
                case MessageResponse.Cancel:
                    return false;
                default:
                    return true;
            }
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
            _textureService = injectionParameters.TextureService ?? throw new ArgumentMissingException(nameof(injectionParameters.TextureService), nameof(injectionParameters));
            _spriteCodec = injectionParameters.SpriteCodec ?? throw new ArgumentMissingException(nameof(injectionParameters.SpriteCodec), nameof(injectionParameters));
            ManualInput = injectionParameters.ManualInput ?? throw new ArgumentMissingException(nameof(injectionParameters.ManualInput), nameof(injectionParameters));
            Settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));

            _originalTexture = injectionParameters.SpriteTextureFile;

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
            // If a texture was assigned, but not saved, then remove the link.
            // TODO: When undoing this, we will need to reset the linkages at each undo level (store in params).
            if (_originalTexture != _textureFile)
            {
                _textureFile?.UnlinkContent(File);
                _originalTexture?.LinkContent(File);
            }

            _imageData?.Dispose();

            SetupTextureFile(null);
            _sprite?.Texture?.Dispose();

            base.OnUnload();
        }

        /// <summary>Function to determine if an object can be dropped.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns>
        ///   <b>true</b> if the data can be dropped, <b>false</b> if not.</returns>
        public bool CanDrop(IContentFileDragData dragData)
        {
            // Don't open the same file, it's already loaded, or is not a content image, or a tool is active.            
            if (((_textureFile != null)  && (string.Equals(_textureFile.Path, dragData.File.Path, StringComparison.OrdinalIgnoreCase))) 
                || (!_textureService.IsContentImage(dragData.File))
                || (_currentTool != SpriteEditTool.None))
            {
                dragData.Cancel = true;
                return false;
            }

            return true;
        }

        /// <summary>Function to drop the payload for a drag drop operation.</summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        public async void Drop(IContentFileDragData dragData, Action afterDrop = null)
        {
            SpriteUndoArgs textureUndoArgs = null;
            SpriteUndoArgs textureRedoArgs = null;

            async Task<bool> AssignTextureAsync(SpriteUndoArgs args)
            {
                IContentFile newTextureFile = args.CurrentTexture;

                ShowWaitPanel(string.Format(Resources.GORSPR_TEXT_LOADING_IMAGE, newTextureFile.Path));

                try
                {
                    
                    GorgonTexture2DView texture;
                    IGorgonImageInfo imageInfo;

                    imageInfo = _textureService.GetImageMetadata(newTextureFile);

                    if (imageInfo == null)
                    {
                        MessageDisplay.ShowError(string.Format(Resources.GORSPR_ERR_NOT_AN_IMAGE, newTextureFile.Path));
                        return false;
                    }

                    if ((imageInfo.ImageType == ImageType.Image1D) || (imageInfo.ImageType == ImageType.Image3D))
                    {
                        MessageDisplay.ShowError(Resources.GORSPR_ERR_NOT_2D_IMAGE);
                        return false;
                    }

                    texture = await _textureService.LoadTextureAsync(newTextureFile);

                    Texture?.Dispose();
                    Texture = texture;

                    // Copy the image data.
                    await ExtractImageDataAsync();

                    _textureFile?.UnlinkContent(File);

                    SetupTextureFile(newTextureFile);

                    _textureFile.LinkContent(File);

                    if (File.Metadata.Attributes.ContainsKey(CommonEditorConstants.IsNewAttr))
                    {
                        // TODO: Set the size property on the view model.
                        Size = new DX.Size2F(texture.Width, texture.Height);
                        File.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);
                    }

                    // Store the sprite array index as this may change based on the texture array count.
                    if (args.ArrayIndex == -1)
                    {
                        args.ArrayIndex = ArrayIndex.Min(texture.Texture.ArrayCount - 1);
                    }

                    ArrayIndex = args.ArrayIndex;
                    NotifyPropertyChanged(nameof(ImageData));
                }
                catch (Exception ex)
                {
                    MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_TEXTURE_REPLACE);
                    return false;
                }
                finally
                {
                    HideWaitPanel();
                }

                return true;
            }

            async Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken) => await AssignTextureAsync(undoArgs);

            async Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken) => await AssignTextureAsync(redoArgs);

            textureUndoArgs = new SpriteUndoArgs
            {
                CurrentTexture = _textureFile,
                ArrayIndex = ArrayIndex
            };
            textureRedoArgs = new SpriteUndoArgs
            {
                CurrentTexture = dragData.File,
                ArrayIndex = -1
            };

            if (!await AssignTextureAsync(textureRedoArgs))
            {
                return;
            }
                
            // If we initially don't have a texture, then don't record the action.
            if (textureUndoArgs?.CurrentTexture != null)
            {
                _undoService.Record(Resources.GORSPR_UNDO_DESC_TEXTURE, UndoAction, RedoAction, textureUndoArgs, textureRedoArgs);
                // Need to call this so the UI can register our updated undo stack.
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to extract the image data from a sprite.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        public async Task ExtractImageDataAsync()
        {
            IGorgonImage image = Interlocked.Exchange(ref _imageData, null);
            image?.Dispose();

            if (_sprite.Texture == null)
            {
                return;
            }                        
            
            image = await _textureService.GetSpriteTextureImageDataAsync(_sprite.Texture, _sprite.TextureArrayIndex);

            Interlocked.Exchange(ref _imageData, image);

            NotifyPropertyChanged(nameof(ImageData));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContent"/> class.</summary>
        public SpriteContent()
        {
            SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveSpriteTask, CanSaveSprite);
            UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
            RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
            SetTextureCoordinatesCommand = new EditorCommand<(DX.RectangleF, int)>(DoSetTextureCoordinates, CanSetTextureCoordinates);
            SpritePickCommand = new EditorCommand<object>(DoSpritePick, CanSpritePick);
        }
        #endregion
    }
}
