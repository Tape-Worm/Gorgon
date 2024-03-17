
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 2, 2019 2:09:04 AM
// 


using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Content view model for a sprite
/// </summary>
internal class SpriteContent
    : ContentEditorViewModelBase<SpriteContentParameters>, ISpriteContent
{

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
        /// The path to the current texture file associated with the sprite.
        /// </summary>
        public string CurrentTexturePath;
        /// <summary>
        /// The current texture array index.
        /// </summary>
        public int ArrayIndex;
        /// <summary>
        /// The color of the sprite
        /// </summary>
        public IReadOnlyList<GorgonColor> VertexColor;
        /// <summary>
        /// The offsets of the sprite vertices.
        /// </summary>
        public IReadOnlyList<Vector3> VertexOffset;
        /// <summary>
        /// The anchor position for the sprite.
        /// </summary>
        public Vector2 Anchor;
        /// <summary>
        /// The current sampler state.
        /// </summary>
        public GorgonSamplerState SamplerState;
    }



    // The default color for a sprite.
    private static readonly GorgonColor[] _defaultColor =
    [
        GorgonColor.White,
        GorgonColor.White,
        GorgonColor.White,
        GorgonColor.White
    ];

    // The sprite content services.
    private SpriteContentServices _contentServices;
    // The sprite being edited.
    private GorgonSprite _sprite;
    // The texture file associated with the sprite.
    private IContentFile _textureFile;
    // The codec used to read/write sprite data.
    private IGorgonSpriteCodec _spriteCodec;
    // The original texture.
    private IContentFile _originalTexture;
    // The currently active panel.
    private IHostedPanelViewModel _currentPanel;



    /// <summary>
    /// Property to return the sprite color editor.
    /// </summary>
    public ISpriteColorEdit ColorEditor
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the sprite anchor editor.
    /// </summary>
    public ISpriteAnchorEdit AnchorEditor
    {
        get;
        private set;
    }

    /// <summary>Property to return whether the sprite will use nearest neighbour filtering, or bilinear filtering.</summary>
    public bool IsPixellated => (_sprite.TextureSampler is not null) && (_sprite.TextureSampler.Filter == SampleFilter.MinMagMipPoint);

    /// <summary>
    /// Property to return the current sampler state for the sprite.
    /// </summary>
    public GorgonSamplerState SamplerState => _sprite.TextureSampler ?? GorgonSamplerState.Default;

    /// <summary>Property to return the type of content.</summary>
    public override string ContentType => CommonEditorContentTypes.SpriteType;

    /// <summary>
    /// Property to return the view model for the plug in settings.
    /// </summary>
    public ISettings Settings
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the context for the sprite clipper.
    /// </summary>
    public ISpriteClipContext SpriteClipContext
    {
        get;
        private set;
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

            NotifyPropertyChanging(nameof(SpriteInfo));
            NotifyPropertyChanging(nameof(ArrayIndex));
            NotifyPropertyChanging(nameof(ArrayCount));
            OnPropertyChanging();
            _sprite.TextureRegion = value;
            OnPropertyChanged();
            NotifyPropertyChanged(nameof(ArrayCount));
            NotifyPropertyChanged(nameof(ArrayIndex));
            NotifyPropertyChanged(nameof(SpriteInfo));
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

            NotifyPropertyChanging(nameof(SupportsArrayChange));
            OnPropertyChanging();
            _sprite.Texture = value;
            OnPropertyChanged();
            NotifyPropertyChanged(nameof(SupportsArrayChange));

            ContentState = ContentState.Modified;
        }
    }

    /// <summary>Property to set or return the color of each sprite vertex.</summary>
    public IReadOnlyList<GorgonColor> VertexColors
    {
        get => _sprite.CornerColors;
        set
        {
            if (value == _sprite.CornerColors)
            {
                return;
            }

            value ??= _defaultColor;

            if (_sprite.CornerColors.SequenceEqual(value))
            {
                return;
            }

            OnPropertyChanging();
            for (int i = 0; i < _sprite.CornerColors.Count; ++i)
            {
                _sprite.CornerColors[i] = i >= value.Count ? GorgonColor.White : value[i];
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return offsets for each vertex in the sprite.
    /// </summary>
    public IReadOnlyList<Vector3> VertexOffsets
    {
        get => _sprite.CornerOffsets;
        private set
        {
            if (value == _sprite.CornerOffsets)
            {
                return;
            }

            if (_sprite.CornerOffsets.SequenceEqual(value))
            {
                return;
            }

            OnPropertyChanging();
            for (int i = 0; i < _sprite.CornerOffsets.Count; ++i)
            {
                _sprite.CornerOffsets[i] = i >= value.Count ? Vector3.Zero : value[i];
            }
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the anchor position for for the sprite.</summary>
    public Vector2 Anchor
    {
        get => _sprite.Anchor;
        private set
        {
            if (_sprite.Anchor.Equals(value))
            {
                return;
            }

            OnPropertyChanging();
            _sprite.Anchor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return whether the currently loaded texture supports array changes.
    /// </summary>
    public bool SupportsArrayChange => (Texture is not null) && (Texture.Texture.ArrayCount > 1) && ((CommandContext == SpriteClipContext));// || (CurrentTool == SpriteEditTool.SpriteClip));

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

            NotifyPropertyChanging(nameof(SpriteInfo));
            OnPropertyChanging();
            _sprite.Size = value;
            OnPropertyChanged();
            NotifyPropertyChanged(nameof(SpriteInfo));
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
    /// Property to return the command used to show the sprite texture wrapping editor.
    /// </summary>
    public IEditorCommand<object> ShowWrappingEditorCommand
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
    /// Property to return the command to execute when clipping a sprite.
    /// </summary>
    public IEditorCommand<object> SpriteClipCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute when creating a new sprite.
    /// </summary>
    public IEditorAsyncCommand<object> NewSpriteCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute when toggling the manual vertex editing.
    /// </summary>
    public IEditorCommand<object> ToggleManualVertexEditCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to show the sprite color editor.
    /// </summary>
    public IEditorCommand<object> ShowColorEditorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to show the sprite anchor editor.
    /// </summary>
    public IEditorCommand<object> ShowAnchorEditorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to execut when adjusting the sprite vertex offsets.
    /// </summary>
    public IEditorCommand<object> SpriteVertexOffsetCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to apply the vertex offsets to the sprite.
    /// </summary>
    public IEditorCommand<IReadOnlyList<Vector3>> SetVertexOffsetsCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to apply texture filtering to the sprite.
    /// </summary>
    public IEditorCommand<SampleFilter> SetTextureFilteringCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the texture on the sprite.
    /// </summary>
    public IEditorAsyncCommand<SetTextureArgs> SetTextureCommand
    {
        get;
    }

    /// <summary>Property to return the currently active panel.</summary>
    public IHostedPanelViewModel CurrentPanel
    {
        get => _currentPanel;
        set
        {
            if (_currentPanel == value)
            {
                return;
            }

            if (_currentPanel is not null)
            {
                _currentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
                _currentPanel.IsActive = false;
            }

            OnPropertyChanging();
            _currentPanel = value;
            OnPropertyChanged();

            if (_currentPanel is not null)
            {
                _currentPanel.IsActive = true;
                _currentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
            }
        }
    }

    /// <summary>Property to return the total number of array indices in the sprite texture.</summary>
    public int ArrayCount => Texture?.Texture.ArrayCount ?? 0;

    /// <summary>Property to return information about the sprite.</summary>
    public string SpriteInfo
    {
        get
        {
            if (Texture is null)
            {
                return string.Empty;
            }

            DX.Rectangle rect = Texture.ToPixel(TextureCoordinates);
            return string.Format(Resources.GORSPR_TEXT_SPRITE_INFO, rect.Left, rect.Top, rect.Right, rect.Bottom, rect.Width, rect.Height);
        }
    }

    /// <summary>Property to return the context for the sprite picker.</summary>
    public ISpritePickContext SpritePickContext
    {
        get;
        private set;
    }

    /// <summary>Property to return the view model for the vertex editor interface.</summary>
    public ISpriteVertexEditContext SpriteVertexEditContext
    {
        get;
        private set;
    }

    /// <summary>Property to return the editor used to modify the texture wrapping state for a sprite.</summary>
    public ISpriteTextureWrapEdit WrappingEditor
    {
        get;
        private set;
    }



    /// <summary>Handles the PropertyChanged event of the ColorEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void ColorEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpriteColorEdit.IsActive):
                CurrentPanel = ColorEditor.IsActive ? ColorEditor : null;
                break;
        }
    }

    /// <summary>
    /// Function to set up a texture file that is associated with the sprite.
    /// </summary>
    /// <param name="spriteFile">The file containing the sprite data.</param>
    /// <param name="textureFile">The current texture file.</param>
    private void SetupTextureFile(IContentFile spriteFile, IContentFile textureFile)
    {
        if (_textureFile is not null)
        {
            _textureFile.IsOpen = false;
        }

        _textureFile = textureFile;

        if (_textureFile is null)
        {
            return;
        }

        spriteFile.LinkContent(_textureFile);

        _textureFile.IsOpen = true;
    }

    /// <summary>
    /// Function to determine if a texture can be set on the sprite.
    /// </summary>
    /// <param name="args">The command arguments.</param>
    /// <returns><b>true</b> if the texture can be assigned, <b>false</b> if not.</returns>
    private bool CanSetTexture(SetTextureArgs args)
    {
        if ((string.IsNullOrWhiteSpace(args.TextureFilePath))
            || (CommandContext is not null)
            || (!ContentFileManager.FileExists(args.TextureFilePath)))
        {
            args.Cancel = true;
            return false;
        }

        IContentFile file = ContentFileManager.GetFile(args.TextureFilePath);

        if (!_contentServices.TextureService.IsContentImage(file))
        {
            args.Cancel = true;
            return false;
        }

        IGorgonImageInfo metadata = _contentServices.TextureService.GetImageMetadata(file);

        args.Cancel = metadata.ImageType is not ImageDataType.Image2D and not ImageDataType.ImageCube;

        return !args.Cancel;
    }

    /// <summary>
    /// Function to assign a new texture to the sprite.
    /// </summary>
    /// <param name="spriteFile">The current sprite file.</param>
    /// <param name="textureFile">The texture file to load.</param>
    /// <param name="arrayIndex">The texture array index to use.</param>
    /// <returns><b>true</b> if the texture loaded successfully, <b>false</b> if not.</returns>
    private async Task<bool> SetTextureAsync(IContentFile spriteFile, IContentFile textureFile, int arrayIndex)
    {
        GorgonTexture2DView texture;
        IGorgonImageInfo imageInfo;

        imageInfo = _contentServices.TextureService.GetImageMetadata(textureFile);

        if (imageInfo is null)
        {
            HostServices.MessageDisplay.ShowError(string.Format(Resources.GORSPR_ERR_NOT_AN_IMAGE, textureFile.Path));
            return false;
        }

        if (imageInfo.ImageType is ImageDataType.Image1D or ImageDataType.Image3D)
        {
            HostServices.MessageDisplay.ShowError(Resources.GORSPR_ERR_NOT_2D_IMAGE);
            return false;
        }

        texture = await _contentServices.TextureService.LoadTextureAsync(textureFile);

        Texture?.Dispose();
        Texture = texture;

        // Copy the image data.
        if ((SpritePickContext?.GetImageDataCommand is not null) && (SpritePickContext.GetImageDataCommand.CanExecute(null)))
        {
            await SpritePickContext.GetImageDataCommand.ExecuteAsync(null);
        }

        spriteFile?.UnlinkContent(_textureFile);

        SetupTextureFile(spriteFile, textureFile);

        if (File.Metadata.Attributes.ContainsKey(CommonEditorConstants.IsNewAttr))
        {
            File.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);
        }

        // Store the sprite array index as this may change based on the texture array count.
        if (arrayIndex == -1)
        {
            arrayIndex = ArrayIndex.Min(texture.Texture.ArrayCount - 1);
        }

        ArrayIndex = arrayIndex;

        // Adjust the texture coordinates so that they appear in the correct place on the texture.
        TextureCoordinates = texture.ToTexel(new DX.Rectangle((int)(TextureCoordinates.X * texture.Width), (int)(TextureCoordinates.Y * texture.Height),
                                                              (int)Size.Width, (int)Size.Height));

        return true;
    }

    /// <summary>
    /// Function to assign a texture to the sprite.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSetTexture(SetTextureArgs args)
    {
        SpriteUndoArgs textureUndoArgs = null;
        SpriteUndoArgs textureRedoArgs = null;

        async Task<bool> AssignTextureAsync(SpriteUndoArgs textureArgs)
        {
            IContentFile newTextureFile = ContentFileManager.GetFile(textureArgs.CurrentTexturePath);

            ShowWaitPanel(string.Format(Resources.GORSPR_TEXT_LOADING_IMAGE, newTextureFile.Path));

            try
            {
                if (!await SetTextureAsync(File, newTextureFile, textureArgs.ArrayIndex))
                {
                    return false;
                }

                if (textureArgs.ArrayIndex != ArrayIndex)
                {
                    textureArgs.ArrayIndex = ArrayIndex;
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_TEXTURE_REPLACE);
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
            CurrentTexturePath = _textureFile?.Path,
            ArrayIndex = ArrayIndex
        };
        textureRedoArgs = new SpriteUndoArgs
        {
            CurrentTexturePath = args.TextureFilePath,
            ArrayIndex = -1
        };

        if (!await AssignTextureAsync(textureRedoArgs))
        {
            return;
        }

        // If we initially don't have a texture, then don't record the action.
        if (!string.IsNullOrWhiteSpace(textureUndoArgs?.CurrentTexturePath))
        {
            _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_TEXTURE, UndoAction, RedoAction, textureUndoArgs, textureRedoArgs);
            // Need to call this so the UI can register our updated undo stack.
            NotifyPropertyChanged(nameof(UndoCommand));
        }
    }

    /// <summary>
    /// Function to determine if an undo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
    private bool CanUndo() => (_contentServices.UndoService.CanUndo) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to determine if a redo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
    private bool CanRedo() => (_contentServices.UndoService.CanRedo) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function called when a redo operation is requested.
    /// </summary>
    private async void DoRedoAsync()
    {
        try
        {
            await _contentServices.UndoService.Redo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_REDO);
        }
    }

    /// <summary>
    /// Function called when an undo operation is requested.
    /// </summary>
    private async void DoUndoAsync()
    {
        try
        {
            await _contentServices.UndoService.Undo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UNDO);
        }
    }

    /// <summary>
    /// Function to determine if a new sprite can be created.
    /// </summary>
    /// <returns><b>true</b> if a new sprite can be created, <b>false</b> if not.</returns>
    private bool CanCreateSprite() => (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to create a new sprite based on the current sprite.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoCreateSpriteAsync()
    {
        Stream outStream = null;

        void SaveSprite() => _spriteCodec.Save(_sprite, outStream);

        try
        {
            // If this content is currently in a modified state, ask if we want to save first.
            if (ContentState != ContentState.Unmodified)
            {
                MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_CLOSE, File.Name), allowCancel: true);

                switch (response)
                {
                    case MessageResponse.Yes:
                        ShowWaitPanel(Resources.GORSPR_TEXT_SAVING);
                        File.IsOpen = false;
                        outStream = ContentFileManager.OpenStream(File.Path, FileMode.Create);
                        await Task.Run(SaveSprite);
                        outStream.Dispose();
                        File.IsOpen = true;
                        HideWaitPanel();
                        break;
                    case MessageResponse.Cancel:
                        return;
                }
            }

            (string newName, IContentFile textureFile, DX.Size2F size) = _contentServices.NewSpriteService.GetNewSpriteName(File, _textureFile, _sprite.Size);

            if (newName is null)
            {
                return;
            }

            string spriteDirectory = Path.GetDirectoryName(File.Path).FormatDirectory('/');

            if (string.IsNullOrWhiteSpace(spriteDirectory))
            {
                spriteDirectory = "/";
            }

            string spritePath = spriteDirectory + newName.FormatFileName();

            IContentFile existingFile = ContentFileManager.GetFile(spritePath);
            if (existingFile is not null)
            {
                // We cannot overwrite a file that's already open for editing.
                if (existingFile.IsOpen)
                {
                    HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_ERR_FILESYSTEM_ITEM_EXISTS, spritePath));
                    return;
                }

                if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_SPRITE_EXISTS, spritePath)) != MessageResponse.Yes)
                {
                    return;
                }
            }

            // Copy the current sprite state into a new file.
            ShowWaitPanel(Resources.GORSPR_TEXT_SAVING);

            // Create the file first.
            outStream = ContentFileManager.OpenStream(spritePath, FileMode.Create);
            await Task.Run(SaveSprite);
            outStream.Dispose();

            IContentFile file = ContentFileManager.GetFile(spritePath);

            if (file is null)
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GORSPR_ERR_SPRITE_NOT_FOUND, newName));
                await CloseContentCommand.ExecuteAsync(new CloseContentArgs(false));
                return;
            }

            ContentState = ContentState.Unmodified;

            // Update the sprite size.
            if (Size != size)
            {
                Size = size;
                TextureCoordinates = new DX.RectangleF(TextureCoordinates.X, TextureCoordinates.Y, size.Width / _sprite.Texture.Width, size.Height / _sprite.Texture.Height);
                ContentState = ContentState.Modified;
            }

            // Load the texture if we've changed it.
            if (!string.Equals(textureFile.Path, _textureFile.Path, StringComparison.OrdinalIgnoreCase))
            {
                if (await SetTextureAsync(file, textureFile, -1))
                {
                    // Set the new texture as the original.
                    _originalTexture = _textureFile;
                    ContentState = ContentState.Modified;
                }
            }

            // Write the updated data.
            if (ContentState == ContentState.Modified)
            {
                outStream = ContentFileManager.OpenStream(spritePath, FileMode.Create);
                await Task.Run(SaveSprite);
                outStream.Dispose();

                ContentState = ContentState.Unmodified;
            }

            File = file;

            // Remove all undo states since we're now working with a new file.
            _contentServices.UndoService.ClearStack();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_CREATE_SPRITE);
            // If we fail to create the new sprite, we need to shut the editor down because our state
            // could be corrupt.
            await CloseContentCommand.ExecuteAsync(new CloseContentArgs(false));
        }
        finally
        {
            HideWaitPanel();
        }
    }


    /// <summary>
    /// Function to determine if the current color can be comitted.
    /// </summary>
    /// <returns><b>true</b> if the color can be comitted, <b>false</b> if not.</returns>
    private bool CanCommitColorChange() => (ColorEditor is not null) && (!ColorEditor.SpriteColor.SequenceEqual(ColorEditor.OriginalSpriteColor));

    /// <summary>
    /// Function called to change the color of the sprite.
    /// </summary>
    private void DoCommitColorChange()
    {

        bool SetColor(IReadOnlyList<GorgonColor> colors)
        {
            try
            {
                VertexColors = colors;
                ContentState = ContentState.Modified;

                CurrentPanel = null;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetColor(undoArgs.VertexColor);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetColor(redoArgs.VertexColor);
            return Task.CompletedTask;
        }

        SpriteUndoArgs colorUndoArgs = new()
        {
            VertexColor = [.. _sprite.CornerColors]
        };

        SpriteUndoArgs colorRedoArgs = new()
        {
            VertexColor = [.. ColorEditor.SpriteColor]
        };

        if (!SetColor(colorRedoArgs.VertexColor))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_COLOR, UndoAction, RedoAction, colorUndoArgs, colorRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if the current vertex offset changes can be comitted.
    /// </summary>
    /// <returns><b>true</b> if the vertex offset changes can be comitted, <b>false</b> if not.</returns>
    private bool CanCommitVertexOffsets() => (CurrentPanel is null)
                                          && (!_sprite.CornerOffsets.Select(item => new Vector2(item.X, item.Y))
                                                                    .SequenceEqual(SpriteVertexEditContext.Vertices));

    /// <summary>
    /// Function called to change the vertex offsets of the sprite.
    /// </summary>
    private void DoCommitVertexOffsets()
    {
        bool SetVertices(IReadOnlyList<Vector3> offsets)
        {
            try
            {
                VertexOffsets = offsets;
                ContentState = ContentState.Modified;

                CommandContext = null;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetVertices(undoArgs.VertexOffset);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetVertices(redoArgs.VertexOffset);
            return Task.CompletedTask;
        }

        SpriteUndoArgs vtxUndoArgs = new()
        {
            VertexOffset = [.. _sprite.CornerOffsets]
        };

        Vector3[] verts = SpriteVertexEditContext.Vertices.Select(item => new Vector3(item.X, item.Y, 0)).ToArray();

        SpriteUndoArgs vtxRedoArgs = new()
        {
            VertexOffset = verts
        };

        if (!SetVertices(verts))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_CORNER_OFFSET, UndoAction, RedoAction, vtxUndoArgs, vtxRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if the current content can be saved.
    /// </summary>
    /// <param name="saveReason">The reason why the content is being saved.</param>
    /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
    private bool CanSaveSprite(SaveReason saveReason) => (ContentState != ContentState.Unmodified)
                                                        && (((CommandContext is null) && (CurrentPanel is null)) || (saveReason != SaveReason.UserSave));

    /// <summary>
    /// Function to save the sprite back to the project file system.
    /// </summary>
    /// <param name="saveReason">The reason why the content is being saved.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSaveSpriteTask(SaveReason saveReason)
    {
        Stream outStream = null;

        ShowWaitPanel(Resources.GORSPR_TEXT_SAVING);

        void SaveSprite() => _spriteCodec.Save(_sprite, outStream);

        try
        {
            // Ensure that the texture is linked.
            File.LinkContent(_textureFile);

            // This file is no longer "new".
            File.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);

            File.IsOpen = false;
            outStream = ContentFileManager.OpenStream(File.Path, FileMode.Create);
            await Task.Run(SaveSprite);
            outStream.Dispose();
            File.IsOpen = true;

            File.Refresh();

            _originalTexture = _textureFile;

            ContentState = ContentState.Unmodified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_SAVE_SPRITE);
        }
        finally
        {
            File.IsOpen = true;
            outStream?.Dispose();

            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if the current editor picking coordinates can be applied to the sprite or not.
    /// </summary>
    /// <returns><b>true</b> if the coordinates can be applied, <b>false</b> if not.</returns>
    private bool CanApplyPick()
    {
        if ((Texture is null) || (CurrentPanel is not null) || (CommandContext != SpritePickContext))
        {
            return false;
        }

        DX.RectangleF texCoords = Texture.ToTexel(SpritePickContext.SpriteRectangle.ToRectangle());

        return ((!_sprite.TextureRegion.Equals(ref texCoords)) || (_sprite.TextureArrayIndex != SpritePickContext.ArrayIndex));
    }

    /// <summary>
    /// Function to apply the pick coordinates to the sprite.
    /// </summary>
    private void DoApplyPick()
    {
        bool SetTextureCoordinates(DX.RectangleF coordinates, int index, IReadOnlyList<Vector3> vertexOffsets)
        {
            try
            {
                DX.Rectangle textureRect = coordinates.ToRectangle();
                textureRect.Inflate(SpritePickContext.Padding, SpritePickContext.Padding);
                TextureCoordinates = Texture.ToTexel(textureRect);
                Size = new DX.Size2F((int)coordinates.Size.Width, (int)coordinates.Size.Height);
                ArrayIndex = index;
                VertexOffsets = vertexOffsets;

                CommandContext = null;
                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetTextureCoordinates(undoArgs.TextureCoordinates, undoArgs.ArrayIndex, undoArgs.VertexOffset);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetTextureCoordinates(redoArgs.TextureCoordinates, redoArgs.ArrayIndex, redoArgs.VertexOffset);
            return Task.CompletedTask;
        }

        SpriteUndoArgs texCoordUndoArgs = new()
        {
            TextureCoordinates = Texture.ToPixel(TextureCoordinates).ToRectangleF(),
            ArrayIndex = ArrayIndex,
            VertexOffset = [.. _sprite.CornerOffsets]
        };
        SpriteUndoArgs texCoordRedoArgs = new()
        {
            TextureCoordinates = SpritePickContext.SpriteRectangle,
            ArrayIndex = SpritePickContext.ArrayIndex,
            VertexOffset = new Vector3[_sprite.CornerOffsets.Count]
        };

        if (!SetTextureCoordinates(texCoordRedoArgs.TextureCoordinates, texCoordRedoArgs.ArrayIndex, texCoordRedoArgs.VertexOffset))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_CLIP, UndoAction, RedoAction, texCoordUndoArgs, texCoordRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if the current editor clipping coordinates can be applied to the sprite or not.
    /// </summary>
    /// <returns><b>true</b> if the coordinates can be applied, <b>false</b> if not.</returns>
    private bool CanApplyClip()
    {
        if ((Texture is null) || (CurrentPanel is not null) || (CommandContext != SpriteClipContext))
        {
            return false;
        }

        DX.RectangleF texCoords = Texture.ToTexel(SpriteClipContext.SpriteRectangle.ToRectangle());

        return ((!_sprite.TextureRegion.Equals(ref texCoords)) || (_sprite.TextureArrayIndex != SpriteClipContext.ArrayIndex));
    }

    /// <summary>
    /// Function to apply the clipping coordinates to the sprite.
    /// </summary>
    private void DoApplyClip()
    {
        bool SetTextureCoordinates(DX.RectangleF coordinates, int index, IReadOnlyList<Vector3> vertexOffsets)
        {
            try
            {
                TextureCoordinates = Texture.ToTexel(coordinates.ToRectangle());
                Size = new DX.Size2F((int)coordinates.Size.Width, (int)coordinates.Size.Height);
                ArrayIndex = index;
                VertexOffsets = vertexOffsets;

                CommandContext = null;
                ContentState = ContentState.Modified;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetTextureCoordinates(undoArgs.TextureCoordinates, undoArgs.ArrayIndex, undoArgs.VertexOffset);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetTextureCoordinates(redoArgs.TextureCoordinates, redoArgs.ArrayIndex, redoArgs.VertexOffset);
            return Task.CompletedTask;
        }

        SpriteUndoArgs texCoordUndoArgs = new()
        {
            TextureCoordinates = Texture.ToPixel(TextureCoordinates).ToRectangleF(),
            ArrayIndex = ArrayIndex,
            VertexOffset = [.. _sprite.CornerOffsets]
        };
        SpriteUndoArgs texCoordRedoArgs = new()
        {
            TextureCoordinates = SpriteClipContext.SpriteRectangle,
            ArrayIndex = SpriteClipContext.ArrayIndex,
            VertexOffset = new Vector3[_sprite.CornerOffsets.Count]
        };

        if (!SetTextureCoordinates(texCoordRedoArgs.TextureCoordinates, texCoordRedoArgs.ArrayIndex, texCoordRedoArgs.VertexOffset))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_CLIP, UndoAction, RedoAction, texCoordUndoArgs, texCoordRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if the sprite picker can activate or not.
    /// </summary>
    /// <returns><b>true</b> if it can activate, <b>false</b> if not.</returns>
    private bool CanSpritePick() => ((CommandContext is null) || (CommandContext == SpritePickContext))
                                        && (Texture is not null)
                                        && (CurrentPanel is null);

    /// <summary>
    /// Function to activate (or deactivate) the sprite picker tool.
    /// </summary>
    private async void DoSpritePick()
    {
        try
        {
            if (CommandContext == SpritePickContext)
            {
                CommandContext = null;
                return;
            }

            // Let the user know that performance may be an issue here with a large texture (32 bit 4096x4096 image should be around 67 MB).
            if ((Settings.ShowImageSizeWarning) && (Texture.Texture.SizeInBytes > 67108864))
            {
                HostServices.MessageDisplay.ShowWarning(string.Format(Resources.GORSPR_WRN_LARGE_IMAGE, Texture.Width, Texture.Height));
            }

            // If the image data is unloaded from the image picker service, then we'll need to upload it.
            if ((SpritePickContext?.GetImageDataCommand != null) && (SpritePickContext.ImageData is null) && (SpritePickContext.GetImageDataCommand.CanExecute(null)))
            {
                ShowWaitPanel(Resources.GORSPR_TEXT_LOADING);
                await SpritePickContext.GetImageDataCommand.ExecuteAsync(null);
            }

            CommandContext = SpritePickContext;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORSPR_ERR_TOOL_CHANGE, SpritePickContext.Name));
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if a sprite can be clipped.
    /// </summary>
    /// <returns><b>true</b> if the sprite can be clipped, <b>false</b> if not.</returns>
    private bool CanSpriteClip() => ((CommandContext is null) || (CommandContext == SpriteClipContext))
                                        && (Texture is not null)
                                        && (CurrentPanel is null);

    /// <summary>
    /// Function to activate (or deactivate) the sprite clipper tool.
    /// </summary>
    private void DoSpriteClip()
    {
        try
        {
            if (CommandContext == SpriteClipContext)
            {
                CommandContext = null;
                return;
            }

            CommandContext = SpriteClipContext;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORSPR_ERR_TOOL_CHANGE, SpriteClipContext.Name));
        }
    }

    /// Function to determine if the sprite vertex offsets can be adjusted.
    /// </summary>
    /// <returns><b>true</b> if the vertices can be adjusted, <b>false</b> if not.</returns>
    private bool CanSpriteVertexOffset() => (Texture is not null) && ((CommandContext is null) || (CommandContext == SpriteVertexEditContext)) && (CurrentPanel is null);

    /// <summary>
    /// Function to activate the sprite vertex editing functionality.
    /// </summary>
    private void DoSpriteVertexOffset()
    {
        try
        {
            if (CommandContext == SpriteVertexEditContext)
            {
                CommandContext = null;
                return;
            }

            CommandContext = SpriteVertexEditContext;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GORSPR_ERR_TOOL_CHANGE, SpriteVertexEditContext.Name));
        }
    }

    /// <summary>
    /// Function to determine if the color editor panel can be shown.
    /// </summary>
    /// <returns><b>true</b> if the color editor panel can be shown, <b>false</b> if not.</returns>
    private bool CanShowColorEditor() => (Texture is not null) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to show or hide the sprite color editor.
    /// </summary>
    private void DoShowSpriteColorEditor()
    {
        try
        {
            ColorEditor.OriginalSpriteColor = ColorEditor.SpriteColor = _sprite.CornerColors;
            CurrentPanel = ColorEditor;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
        }
    }

    /// <summary>
    /// Function to determine if the anchor editor panel can be shown.
    /// </summary>
    /// <returns><b>true</b> if the anchor editor can be shown, or <b>false</b> if not.</returns>
    private bool CanShowAnchorEditor() => (Texture is not null) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to show or hide the sprite anchor editor.
    /// </summary>
    private void DoShowSpriteAnchorEditor()
    {
        try
        {
            CurrentPanel = AnchorEditor;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
        }
    }

    /// <summary>
    /// Function to determine if the sprite texture wrapping editor panel can be shown.
    /// </summary>
    /// <returns><b>true</b> if the texture wrapping editor panel can be shown, <b>false</b> if not.</returns>
    private bool CanShowWrappingEditor() => (Texture is not null) && (CommandContext is null) && (CurrentPanel is null);

    /// <summary>
    /// Function to show or hide the sprite texture wrapping editor.
    /// </summary>
    private void DoShowWrappingEditor()
    {
        try
        {
            WrappingEditor.CurrentSampler = SamplerState;
            CurrentPanel = WrappingEditor;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
        }
    }

    /// <summary>
    /// Function to determine if the changes to sprite texture wrapping can be committed back to the sprite.
    /// </summary>
    /// <returns><b>true</b> if the changes can be committed, <b>false</b> if not.</returns>
    private bool CanCommitWrappingChange() => ((WrappingEditor is not null) && (Texture is not null)
        && ((SamplerState.WrapU != WrappingEditor.HorizontalWrapping)
             || (SamplerState.WrapV != WrappingEditor.VerticalWrapping)
             || (!SamplerState.BorderColor.Equals(WrappingEditor.BorderColor))));

    /// <summary>
    /// Function to commit the texture wrapping changes back to the sprite.
    /// </summary>
    private void DoCommitWrappingChange()
    {
        bool SetWrapping(GorgonSamplerState state)
        {
            try
            {
                _sprite.TextureSampler = state == GorgonSamplerState.Default ? null : state;

                NotifyPropertyChanged(nameof(SamplerState));

                ContentState = ContentState.Modified;
                CurrentPanel = null;
                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetWrapping(undoArgs.SamplerState);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetWrapping(redoArgs.SamplerState);
            return Task.CompletedTask;
        }

        SpriteUndoArgs wrapUndoArgs = new()
        {
            SamplerState = SamplerState
        };

        SpriteUndoArgs wrapRedoArgs = new()
        {
            SamplerState = WrappingEditor.CurrentSampler
        };

        if (!SetWrapping(wrapRedoArgs.SamplerState))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_WRAP, UndoAction, RedoAction, wrapUndoArgs, wrapRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if the current anchor value can be comitted.
    /// </summary>
    /// <returns><b>true</b> if the anchor value can be comitted, <b>false</b> if not.</returns>
    private bool CanCommitAnchorChange()
    {
        if ((AnchorEditor is null) || (Texture is null))
        {
            return false;
        }

        Vector2 halfSprite = new(Size.Width * 0.5f, Size.Height * 0.5f);
        Vector2 anchorPosition = new Vector2(_sprite.Anchor.X * Size.Width - halfSprite.X,
                                                   _sprite.Anchor.Y * Size.Height - halfSprite.Y).Truncate();
        return (!AnchorEditor.Anchor.Equals(anchorPosition));
    }

    /// <summary>
    /// Function called to change the anchor value of the sprite.
    /// </summary>
    private void DoCommitAnchorChange()
    {
        Vector2 halfSprite = new(Size.Width * 0.5f, Size.Height * 0.5f);

        bool SetAnchor(Vector2 anchor)
        {
            try
            {
                Anchor = anchor;
                ContentState = ContentState.Modified;

                CurrentPanel = null;

                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetAnchor(undoArgs.Anchor);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetAnchor(redoArgs.Anchor);
            return Task.CompletedTask;
        }

        SpriteUndoArgs anchorUndoArgs = new()
        {
            Anchor = _sprite.Anchor
        };

        SpriteUndoArgs anchorRedoArgs = new()
        {
            Anchor = new Vector2((AnchorEditor.Anchor.X + halfSprite.X) / Size.Width,
                                    (AnchorEditor.Anchor.Y + halfSprite.Y) / Size.Height)
        };

        if (!SetAnchor(anchorRedoArgs.Anchor))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_ANCHOR, UndoAction, RedoAction, anchorUndoArgs, anchorRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if a texture filter can be set on the sprite or not.
    /// </summary>
    /// <param name="state">The sampler state.</param>
    /// <returns><b>true</b> if the filter can be set, <b>false</b> if not.</returns>
    private bool CanSetTextureFilter(SampleFilter state) => (CurrentPanel is null) && (CommandContext is null);

    /// <summary>
    /// Function to set the texture sampler filter on the sprite.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    private void DoSetTextureFilter(SampleFilter filter)
    {
        bool SetState(GorgonSamplerState samplerState)
        {
            try
            {
                _sprite.TextureSampler = samplerState;

                NotifyPropertyChanged(nameof(SamplerState));
                NotifyPropertyChanged(nameof(IsPixellated));

                ContentState = ContentState.Modified;
                return true;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
                return false;
            }
        }

        Task UndoAction(SpriteUndoArgs undoArgs, CancellationToken cancelToken)
        {
            SetState(undoArgs.SamplerState);
            return Task.CompletedTask;
        }

        Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
        {
            SetState(redoArgs.SamplerState);
            return Task.CompletedTask;
        }

        GorgonSamplerState newState = null;

        switch (filter)
        {
            case SampleFilter.MinMagMipPoint:
                if ((SamplerState == GorgonSamplerState.Default) || (SamplerState is null))
                {
                    newState = GorgonSamplerState.PointFiltering;
                }
                else
                {
                    newState = _contentServices.SampleStateBuilder.ResetTo(SamplerState)
                                                                  .Filter(SampleFilter.MinMagMipPoint)
                                                                  .Build();
                }
                break;
        }

        SpriteUndoArgs anchorUndoArgs = new()
        {
            SamplerState = SamplerState
        };

        SpriteUndoArgs anchorRedoArgs = new()
        {
            SamplerState = newState
        };

        if (!SetState(newState))
        {
            return;
        }

        _contentServices.UndoService.Record(Resources.GORSPR_UNDO_DESC_SAMPLER, UndoAction, RedoAction, anchorUndoArgs, anchorRedoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>Handles the PropertyChanged event of the CurrentPanel control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void CurrentPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IHostedPanelViewModel.IsActive):
                if (CurrentPanel is not null)
                {
                    CurrentPanel = null;
                }
                else
                {
                    CurrentPanel = (IHostedPanelViewModel)sender;
                }
                break;
        }
    }

    /// <summary>Function to initialize the content.</summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    protected override void OnInitialize(SpriteContentParameters injectionParameters)
    {
        base.OnInitialize(injectionParameters);

        Settings = injectionParameters.Settings;
        _sprite = injectionParameters.Sprite;
        _contentServices = injectionParameters.ContentServices;
        _spriteCodec = injectionParameters.SpriteCodec;
        _originalTexture = injectionParameters.SpriteTextureFile;
        SpriteClipContext = injectionParameters.SpriteClipContext;
        SpritePickContext = injectionParameters.SpritePickContext;
        SpriteVertexEditContext = injectionParameters.SpriteVertexEditContext;
        ColorEditor = injectionParameters.ColorEditor;
        AnchorEditor = injectionParameters.AnchorEditor;
        WrappingEditor = injectionParameters.TextureWrappingEditor;

        SetupTextureFile(File, injectionParameters.SpriteTextureFile);

        SpriteClipContext.ApplyCommand = new EditorCommand<object>(DoApplyClip, CanApplyClip);
        SpritePickContext.ApplyCommand = new EditorCommand<object>(DoApplyPick, CanApplyPick);
        SpriteVertexEditContext.ApplyCommand = new EditorCommand<object>(DoCommitVertexOffsets, CanCommitVertexOffsets);
        ColorEditor.OkCommand = new EditorCommand<object>(DoCommitColorChange, CanCommitColorChange);
        AnchorEditor.OkCommand = new EditorCommand<object>(DoCommitAnchorChange, CanCommitAnchorChange);
        WrappingEditor.OkCommand = new EditorCommand<object>(DoCommitWrappingChange, CanCommitWrappingChange);
    }

    /// <summary>
    /// Function to initialize the view model in-place.
    /// </summary>
    /// <param name="sprite">The sprite used to update the view model.</param>
    /// <param name="file">The new file.</param>
    /// <param name="undoService">The undo service to use when correcting mistakes.</param>
    public void Initialize(GorgonSprite sprite, IContentFile file, IUndoService undoService)
    {
        NotifyAllPropertiesChanging();

        // We must replace our undo service, otherwise we'll be undoing into the previous file.
        _contentServices.UndoService = undoService;
        File = file;
        _sprite = sprite;

        SpriteClipContext spriteClipContext = new();
        SpritePickContext spritePickContext = new();
        SpriteVertexEditContext spriteVertexEditContext = new();

        spriteClipContext.Initialize(new SpriteClipContextParameters(this, HostServices));
        spritePickContext.Initialize(new SpritePickContextParameters(this, SpritePickContext.SpritePickMaskEditor, _contentServices.TextureService, HostServices));
        spriteVertexEditContext.Initialize(new SpriteVertexEditContextParameters(this, HostServices));

        SpriteClipContext = spriteClipContext;
        SpritePickContext = spritePickContext;
        SpriteVertexEditContext = spriteVertexEditContext;

        SpriteClipContext.ApplyCommand = new EditorCommand<object>(DoApplyClip, CanApplyClip);
        SpritePickContext.ApplyCommand = new EditorCommand<object>(DoApplyPick, CanApplyPick);
        SpriteVertexEditContext.ApplyCommand = new EditorCommand<object>(DoCommitVertexOffsets, CanCommitVertexOffsets);

        NotifyAllPropertiesChanged();
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        // Mark this file as open in the editor.
        if (_textureFile is not null)
        {
            _textureFile.IsOpen = true;
        }

        SpriteClipContext?.Load();
        SpritePickContext?.Load();
        SpriteVertexEditContext?.Load();
        ColorEditor?.Load();
        AnchorEditor?.Load();
        WrappingEditor?.Load();
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    protected override void OnUnload()
    {
        WrappingEditor?.Unload();
        AnchorEditor?.Unload();
        ColorEditor?.Unload();
        SpriteVertexEditContext?.Unload();
        SpritePickContext?.Unload();
        SpriteClipContext?.Unload();

        if (CurrentPanel is not null)
        {
            CurrentPanel = null;
        }

        if (SpriteClipContext is not null)
        {
            SpriteClipContext.ApplyCommand = null;
        }

        if (SpritePickContext is not null)
        {
            SpritePickContext.ApplyCommand = null;
        }

        CurrentPanel = null;

        if (AnchorEditor is not null)
        {
            AnchorEditor.OkCommand = null;
        }

        if (ColorEditor is not null)
        {
            ColorEditor.OkCommand = null;
        }

        CommandContext = null;

        // If a texture was assigned, but not saved, then remove the link.
        if (_originalTexture != _textureFile)
        {
            File.UnlinkContent(_textureFile);
            File.LinkContent(_originalTexture);
        }

        SetupTextureFile(File, null);
        _sprite?.Texture?.Dispose();

        base.OnUnload();
    }

    /// <summary>Function to determine the action to take when this content is closing.</summary>
    /// <returns>
    ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>PlugIn authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
    protected override async Task<bool> OnCloseContentTaskAsync()
    {
        if (ContentState == ContentState.Unmodified)
        {
            return true;
        }

        MessageResponse response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_CLOSE, File.Name), allowCancel: true);

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



    /// <summary>Initializes a new instance of the <see cref="SpriteContent"/> class.</summary>
    public SpriteContent()
    {
        SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveSpriteTask, CanSaveSprite);
        UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
        RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
        SpriteClipCommand = new EditorCommand<object>(DoSpriteClip, CanSpriteClip);
        SpritePickCommand = new EditorCommand<object>(DoSpritePick, CanSpritePick);
        NewSpriteCommand = new EditorAsyncCommand<object>(DoCreateSpriteAsync, CanCreateSprite);
        ShowColorEditorCommand = new EditorCommand<object>(DoShowSpriteColorEditor, CanShowColorEditor);
        SpriteVertexOffsetCommand = new EditorCommand<object>(DoSpriteVertexOffset, CanSpriteVertexOffset);
        ShowAnchorEditorCommand = new EditorCommand<object>(DoShowSpriteAnchorEditor, CanShowAnchorEditor);
        ShowWrappingEditorCommand = new EditorCommand<object>(DoShowWrappingEditor, CanShowWrappingEditor);
        SetTextureFilteringCommand = new EditorCommand<SampleFilter>(DoSetTextureFilter, CanSetTextureFilter);
        SetTextureCommand = new EditorAsyncCommand<SetTextureArgs>(DoSetTexture, CanSetTexture);
    }

}
