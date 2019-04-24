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
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.Graphics.Imaging;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Math;
using System.Threading;
using Gorgon.Graphics;
using System.ComponentModel;
using Gorgon.Diagnostics;

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
            /// <summary>
            /// The color of the sprite
            /// </summary>
            public IReadOnlyList<GorgonColor> VertexColor;
            /// <summary>
            /// The offsets of the sprite vertices.
            /// </summary>
            public IReadOnlyList<DX.Vector3> VertexOffset;
            /// <summary>
            /// The anchor position for the sprite.
            /// </summary>
            public DX.Vector2 Anchor;
			/// <summary>
            /// The texture filter for the sprite.
            /// </summary>
            public SampleFilter Filter;
			/// <summary>
            /// The horizontal texture wrapping state.
            /// </summary>
            public TextureWrap HorizontalWrap;
			/// <summary>
            /// The vertical texture wrapping state.
            /// </summary>
            public TextureWrap VerticalWrap;
			/// <summary>
            /// The color of the border while in <see cref="TextureWrap.Border"/> mode.
            /// </summary>
            public GorgonColor BorderColor;
        }
        #endregion

        #region Constants.
        /// <summary>
        /// The attribute key name for the sprite codec attribute.
        /// </summary>
        public const string CodecAttr = "SpriteCodec";
        #endregion

        #region Variables.
        // The default color for a sprite.
        private static readonly GorgonColor[] _defaultColor = new GorgonColor[]
        {
            GorgonColor.White,
            GorgonColor.White,
            GorgonColor.White,
            GorgonColor.White
        };

        // The sprite being edited.
        private GorgonSprite _sprite;
        // The undo service.
        private IUndoService _undoService;
        // The texture file associated with the sprite.
        private IContentFile _textureFile;
        // The file manager used to access external content files.
        private IContentFileManager _contentFiles;
        // The sprite texture service.
        private ISpriteTextureService _textureService;
        // The codec used to read/write sprite data.
        private IGorgonSpriteCodec _spriteCodec;
        // The original texture.
        private IContentFile _originalTexture;
        // The currently active tool for editing the sprite.
        private SpriteEditTool _currentTool = SpriteEditTool.None;
        // The image data for the sprite texture.
        private IGorgonImage _imageData;
        // The factory used to create sprite content data.
        private ISpriteContentFactory _factory;
        // The currently active panel.
        private IHostedPanelViewModel _currentPanel;
		// The sampler build service.
        private ISamplerBuildService _samplerBuilder;
        #endregion

        #region Properties.
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

        /// <summary>
        /// Property to return the editor used to modify the texture wrapping state for a sprite.
        /// </summary>
        public ISpriteWrappingEditor WrappingEditor
        {
            get;
            private set;
        }

        /// <summary>Property to return whether the sprite will use nearest neighbour filtering, or bilinear filtering.</summary>
        public bool IsPixellated => (_sprite.TextureSampler != null) && (_sprite.TextureSampler.Filter == SampleFilter.MinMagMipPoint);

        /// <summary>
        /// Property to return the current sampler state for the sprite.
        /// </summary>
        public GorgonSamplerState SamplerState => _sprite.TextureSampler ?? GorgonSamplerState.Default;

        /// <summary>
        /// Property to return whether the sub panel is modal or not.
        /// </summary>
        public bool IsSubPanelModal => (CurrentPanel != null) && (CurrentPanel.IsModal);

        /// <summary>Property to return the currently active panel.</summary>
        public IHostedPanelViewModel CurrentPanel
        {
            get => _currentPanel;
            private set
            {
                if (_currentPanel == value)
                {
                    return;
                }

                if (_currentPanel != null)
                {
                    _currentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
                    _currentPanel.IsActive = false;
                }

                OnPropertyChanging();
                _currentPanel = value;
                OnPropertyChanged();

                if (_currentPanel != null)
                {
                    _currentPanel.IsActive = true;
                    _currentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
                }

                NotifyPropertyChanged(nameof(IsSubPanelModal));
            }
        }

        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => SpriteEditorCommonConstants.ContentType;

        /// <summary>
        /// Property to return the view model for the plug in settings.
        /// </summary>
        public IEditorPlugInSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the manual rectangle editor interface.
        /// </summary>
        public IManualRectangleEditor ManualRectangleEditor
        {
            get;
            private set;
        }

        /// <summary>Property to return the view model for the manual vertex editor interface.</summary>
        public IManualVertexEditor ManualVertexEditor
        {
            get;
            private set;
        }

        /// <summary>
        /// Property tor return the view model for the sprite picker mask color editor.
        /// </summary>
        public ISpritePickMaskEditor SpritePickMaskEditor
        {
            get;
            private set;
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

                CurrentPanel = null;

                OnPropertyChanging();
                _currentTool = value;
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(SupportsArrayChange));

                if (ManualRectangleEditor != null)
                {
                    ManualRectangleEditor.IsActive = false;
                }

                if (ManualVertexEditor != null)
                {
                    ManualVertexEditor.IsActive = false;
                }

                switch (value)
                {
                    case SpriteEditTool.CornerResize:
                        CommandContext = "SpriteCornerOffsets";
                        break;
                    case SpriteEditTool.SpriteClip:
                        CommandContext = "ClipSprite";
                        break;
                    case SpriteEditTool.SpritePick:
                        CommandContext = "SpritePick";
                        break;
                    default:
                        CommandContext = string.Empty;
                        break;
                }
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

                if (value == null)
                {
                    value = _defaultColor;
                }

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
        public IReadOnlyList<DX.Vector3> VertexOffsets
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
                    _sprite.CornerOffsets[i] = i >= value.Count ? DX.Vector3.Zero : value[i];
                }
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the anchor position for for the sprite.</summary>
        public DX.Vector2 Anchor
        {
            get => _sprite.Anchor;
            private set
            {
                if (_sprite.Anchor.Equals(ref value))
                {
                    return;
                }

                OnPropertyChanging();
                _sprite.Anchor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the buffer that contains the image data for the <see cref="Texture"/>.
        /// </summary>
        public IGorgonImage ImageData => _imageData;

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
        /// Property to return the command used to show the sprite sprite picker mask color editor.
        /// </summary>
        public IEditorCommand<object> ShowSpritePickMaskEditorCommand
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
        public IEditorCommand<object> NewSpriteCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when toggling the manual input.
        /// </summary>
        public IEditorCommand<object> ToggleManualClipRectCommand
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
        public IEditorCommand<IReadOnlyList<DX.Vector3>> SetVertexOffsetsCommand
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
        #endregion

        #region Methods.
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
        private bool CanUndo() => (_undoService.CanUndo) && (_currentTool == SpriteEditTool.None) && (CurrentPanel == null);

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => (_undoService.CanRedo) && (_currentTool == SpriteEditTool.None) && (CurrentPanel == null);

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
        /// Function to determine if a new sprite can be created.
        /// </summary>
        /// <returns><b>true</b> if a new sprite can be created, <b>false</b> if not.</returns>
        private bool CanCreateSprite() => (CurrentTool == SpriteEditTool.None) && (CurrentPanel == null);

        /// <summary>
        /// Function to create a new sprite.
        /// </summary>
        private async void DoCreateSprite()
        {
            MemoryStream stream = null;

            try
            {
                // If this content is currently in a modified state, ask if we want to save first.
                if (ContentState != ContentState.Unmodified)
                {
                    MessageResponse response = MessageDisplay.ShowConfirmation(string.Format(Resources.GORSPR_CONFIRM_CLOSE, File.Name), allowCancel: true);

                    switch (response)
                    {
                        case MessageResponse.Yes:
                            // This task is synchronous, so we can call Wait() here and not require an async method.
                            BusyState.SetBusy();
                            SaveSprite();
                            BusyState.SetIdle();
                            return;
                        case MessageResponse.Cancel:
                            return;
                    }
                }

                var existingItems = new HashSet<string>(_contentFiles.EnumeratePaths(_contentFiles.CurrentDirectory, "*").Select(item =>
                {
                    // Get the last path part.
                    if (item.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        item = item.Substring(0, item.Length - 1);
                    }

                    return Path.GetFileName(item);
                }), StringComparer.CurrentCultureIgnoreCase);

                (string newName, byte[] newContent) = await _factory.GetDefaultContentAsync(File.Name, existingItems);

                if ((newName == null) || (newContent == null))
                {
                    return;
                }

                BusyState.SetBusy();

                // Remove the undo stack, we don't need it now, this is a new piece of content.
                _undoService.ClearStack();
                CurrentTool = SpriteEditTool.None;
                CurrentPanel = null;

                // Deserialize the sprite, might as well use it since we have it.
                stream = new MemoryStream(newContent);
                GorgonSprite newSprite = _spriteCodec.FromStream(stream);
                newSprite.Texture = Texture;

                // Link our current texture with the new file.
                IContentFile newFile = _contentFiles.WriteFile(Path.Combine(_contentFiles.CurrentDirectory, newName), s => _spriteCodec.Save(newSprite, s));
                _textureFile.LinkContent(newFile);

                // We set to unmodified here because the new sprite was saved with the current texture.
                ContentState = ContentState.Unmodified;

                // Update the backing store for the view model so we can start using the new sprite.
                _sprite = newSprite;
                File = newFile;

				// Reset the size of the sprite to match the new texture coordinates.
                _sprite.Size = newSprite.Texture.ToPixel(newSprite.TextureRegion).ToRectangleF().Size;

				// Set all vertices as selected on the color editor.
                ColorEditor.SelectedVertices = new bool[] { true, true, true, true };				
                
                NotifyPropertyChanged(nameof(ArrayIndex));
                NotifyPropertyChanged(nameof(TextureCoordinates));
                NotifyPropertyChanged(nameof(ImageData));
                NotifyPropertyChanged(nameof(Texture));
                NotifyPropertyChanged(nameof(VertexColors));
                NotifyPropertyChanged(nameof(VertexOffsets));
                NotifyPropertyChanged(nameof(Size));
                NotifyPropertyChanged(nameof(IsPixellated));
                NotifyPropertyChanged(nameof(SamplerState));

                newFile.SaveMetadata();
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_CREATE_SPRITE);
                CloseContentCommand.Execute(new CloseContentArgs(false));
            }
            finally
            {
                stream?.Dispose();
                BusyState.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if the current color can be comitted.
        /// </summary>
        /// <returns><b>true</b> if the color can be comitted, <b>false</b> if not.</returns>
        private bool CanCommitColorChange() => (ColorEditor != null) && (!ColorEditor.SpriteColor.SequenceEqual(ColorEditor.OriginalSpriteColor));

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
                    MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
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

            var colorUndoArgs = new SpriteUndoArgs
            {
                VertexColor = _sprite.CornerColors.ToArray()
            };

            var colorRedoArgs = new SpriteUndoArgs
            {
                VertexColor = ColorEditor.SpriteColor.ToArray()
			};

            if (!SetColor(colorRedoArgs.VertexColor))
            {
                return;
            }

            _undoService.Record(Resources.GORSPR_UNDO_DESC_COLOR, UndoAction, RedoAction, colorUndoArgs, colorRedoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }

        /// <summary>
        /// Function to determine if the current vertex offset changes can be comitted.
        /// </summary>
        /// <param name="offsets">The vertex offsets to check.</param>
        /// <returns><b>true</b> if the vertex offset changes can be comitted, <b>false</b> if not.</returns>
        private bool CanCommitVertexOffsets(IReadOnlyList<DX.Vector3> offsets) => (CurrentPanel == null) && (!_sprite.CornerOffsets.SequenceEqual(offsets));

        /// <summary>
        /// Function called to change the vertex offsets of the sprite.
        /// </summary>
        /// <param name="vertexOffsets">The offsets to apply.</param>
        private void DoCommitVertexOffsets(IReadOnlyList<DX.Vector3> vertexOffsets)
        {
            bool SetVertices(IReadOnlyList<DX.Vector3> offsets)
            {
                try
                {
                    VertexOffsets = offsets;
                    ContentState = ContentState.Modified;

                    CurrentPanel = null;

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
                SetVertices(undoArgs.VertexOffset);
                return Task.CompletedTask;
            }

            Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
            {
                SetVertices(redoArgs.VertexOffset);
                return Task.CompletedTask;
            }

            var vtxUndoArgs = new SpriteUndoArgs
            {
                VertexOffset = _sprite.CornerOffsets.ToArray()
            };

            var vtxRedoArgs = new SpriteUndoArgs
            {
                VertexOffset = vertexOffsets
            };

            if (!SetVertices(vertexOffsets))
            {
                return;
            }

            _undoService.Record(Resources.GORSPR_UNDO_DESC_CORNER_OFFSET, UndoAction, RedoAction, vtxUndoArgs, vtxRedoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }

        /// <summary>
        /// Function to save the sprite data back to the content file.
        /// </summary>
        private void SaveSprite()
        {
            Stream outStream = null;

            try
            {
                outStream = File.OpenWrite();
                _spriteCodec.Save(_sprite, outStream);
                outStream.Dispose();

                // This file is no longer "new".
                File.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);
                File.Refresh();

                _originalTexture = _textureFile;
                ContentState = ContentState.Unmodified;

                File.SaveMetadata();
            }
            finally
            {
                outStream?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if the current content can be saved.
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
        private bool CanSaveSprite(SaveReason saveReason) => (ContentState != ContentState.Unmodified) && (((CurrentTool == SpriteEditTool.None) && (CurrentPanel == null)) || (saveReason != SaveReason.UserSave));

        /// <summary>
        /// Function to save the sprite back to the project file system.
        /// </summary>
        /// <param name="saveReason">The reason why the content is being saved.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private Task DoSaveSpriteTask(SaveReason saveReason)
        {
            BusyState.SetBusy();

            try
            {
                SaveSprite();
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_SAVE_SPRITE);
            }
            finally
            {
                BusyState.SetIdle();
            }

            return Task.FromResult<object>(null);
        }

		/// <summary>
        /// Function to determine whether the vertex edit(s) can be applied.
        /// </summary>
        /// <returns><b>true</b> if the edits can be applied, <b>false</b> if not.</returns>
        private bool CanApplyVertexEdit()
        {
            if ((CurrentTool != SpriteEditTool.CornerResize) || (CurrentPanel != null) || (Texture == null) || (ManualVertexEditor == null))
            {
                return false;
            }

            IEnumerable<DX.Vector3> newOffsets = ManualVertexEditor.Vertices.Select(item => new DX.Vector3(item.X, item.Y, 0));

            return !VertexOffsets.SequenceEqual(newOffsets);
        }

		/// <summary>
        /// Function to apply vertex offsets to the sprite.
        /// </summary>
        private void DoApplyVertexEdit()
        {
            bool SetVertices(IReadOnlyList<DX.Vector3> offsets)
            {
                try
                {
                    VertexOffsets = offsets;
                    ContentState = ContentState.Modified;

                    CurrentPanel = null;

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
                SetVertices(undoArgs.VertexOffset);
                return Task.CompletedTask;
            }

            Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
            {
                SetVertices(redoArgs.VertexOffset);
                return Task.CompletedTask;
            }

            var vtxUndoArgs = new SpriteUndoArgs
            {
                VertexOffset = _sprite.CornerOffsets.ToArray()
            };

            var vtxRedoArgs = new SpriteUndoArgs
            {
                VertexOffset = ManualVertexEditor.Vertices.Select(item => new DX.Vector3(item.X, item.Y, 0)).ToArray()
            };

            if (!SetVertices(vtxRedoArgs.VertexOffset))
            {
                return;
            }

            CurrentTool = SpriteEditTool.None;

            _undoService.Record(Resources.GORSPR_UNDO_DESC_CORNER_OFFSET, UndoAction, RedoAction, vtxUndoArgs, vtxRedoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }

        /// <summary>
        /// Function to cancel the vertex offset editing.
        /// </summary>
        private void DoCancelVertexEdit()
        {
            if (ManualVertexEditor == null)
            {
                return;
            }

            try
            {
                ManualVertexEditor.Vertices = VertexOffsets.Select(item => new DX.Vector2(item.X, item.Y)).ToArray();                

                // Resetting the tool will submit the texture coordinates and execute the actual command to assign the values. Need to do it this way for now, 
                // the information required to set the coordinates are passed in from the clipper.
                CurrentTool = SpriteEditTool.None;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>
        /// Function to determine if the current editor clipping coordinates can be applied to the sprite or not.
        /// </summary>
        /// <returns><b>true</b> if the coordinates can be applied, <b>false</b> if not.</returns>
        private bool CanApplyClip()
        {
            if (((CurrentTool != SpriteEditTool.SpriteClip) && (CurrentTool != SpriteEditTool.SpritePick)) 
                || ((CurrentPanel != null) && (CurrentPanel != SpritePickMaskEditor)) 
                || (Texture == null) || (ManualRectangleEditor == null))
            {
                return false;
            }

            DX.Rectangle oldCoordinates = Texture.ToPixel(TextureCoordinates);
            var newCoordinates = ManualRectangleEditor.Rectangle.ToRectangle();

            return (!newCoordinates.IsEmpty) 
				&& ((!oldCoordinates.Equals(ref newCoordinates)) || (ArrayIndex != ManualRectangleEditor.TextureArrayIndex)) 
				&& (ManualRectangleEditor.TextureArrayIndex >= 0) 
				&& (ManualRectangleEditor.TextureArrayIndex < Texture.Texture.ArrayCount);
        }

		/// <summary>
        /// Function to apply the clipping coordinates to the sprite.
        /// </summary>
        private void DoApplyClip()
        {			
            try
            {
                bool SetTextureCoordinates(DX.RectangleF coordinates, int index, IReadOnlyList<DX.Vector3> vertexOffsets)
                {
                    try
                    {
                        ManualRectangleEditor.IsActive = false;

                        TextureCoordinates = Texture.ToTexel(coordinates.ToRectangle());
                        Size = new DX.Size2F((int)coordinates.Size.Width, (int)coordinates.Size.Height);
                        ArrayIndex = index;
                        VertexOffsets = vertexOffsets;

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
                    SetTextureCoordinates(undoArgs.TextureCoordinates, undoArgs.ArrayIndex, undoArgs.VertexOffset);
                    return Task.CompletedTask;
                }

                Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
                {
                    SetTextureCoordinates(redoArgs.TextureCoordinates, redoArgs.ArrayIndex, redoArgs.VertexOffset);
                    return Task.CompletedTask;
                }                

                var texCoordUndoArgs = new SpriteUndoArgs
                {
                    TextureCoordinates = Texture.ToPixel(TextureCoordinates).ToRectangleF(),
                    ArrayIndex = ArrayIndex,
                    VertexOffset = _sprite.CornerOffsets.ToArray()
                };
                var texCoordRedoArgs = new SpriteUndoArgs
                {
                    TextureCoordinates = ManualRectangleEditor.Rectangle,
                    ArrayIndex = ManualRectangleEditor.TextureArrayIndex,
                    VertexOffset = new DX.Vector3[_sprite.CornerOffsets.Count]
                };

                if (!SetTextureCoordinates(texCoordRedoArgs.TextureCoordinates, texCoordRedoArgs.ArrayIndex, texCoordRedoArgs.VertexOffset))
                {
                    return;
                }

                CurrentTool = SpriteEditTool.None;

                _undoService.Record(Resources.GORSPR_UNDO_DESC_CLIP, UndoAction, RedoAction, texCoordUndoArgs, texCoordRedoArgs);
                NotifyPropertyChanged(nameof(UndoCommand));
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

		/// <summary>
        /// Function to cancel the clipping coordinate editing.
        /// </summary>
        private void DoCancelClip()
        {
            if (ManualRectangleEditor == null)
            {
                return;
            }

            try
            {
                ManualRectangleEditor.Rectangle = Texture.ToPixel(_sprite.TextureRegion).ToRectangleF();
                ManualRectangleEditor.TextureArrayIndex = ArrayIndex;

                // Resetting the tool will submit the texture coordinates and execute the actual command to assign the values. Need to do it this way for now, 
                // the information required to set the coordinates are passed in from the clipper.
                CurrentTool = SpriteEditTool.None;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

		/// <summary>
        /// Function to determine if the sprite dimensions can be set to the full size of the texture or not.
        /// </summary>
        /// <returns><b>true</b> if the dimensions can be set, <b>false</b> if not.</returns>
        private bool CanSetFullSize() => (CurrentTool == SpriteEditTool.SpriteClip)
            && (CurrentPanel == null)
			&& (Texture != null)
            && (ManualRectangleEditor != null)
            && (!ManualRectangleEditor.IsFixedSize)
            && (!ManualRectangleEditor.Rectangle.Equals(new DX.RectangleF(0, 0, Texture.Width, Texture.Height)));

		/// <summary>
        /// Function to set the editing texture coordinates to the full size of the texture.
        /// </summary>
        private void DoSetFullSize()
        {
            try
            {
                ManualRectangleEditor.Rectangle = new DX.RectangleF(0, 0, Texture.Width, Texture.Height);
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>
        /// Function to determine if the sprite picker can activate or not.
        /// </summary>
        /// <returns><b>true</b> if it can activate, <b>false</b> if not.</returns>
        private bool CanSpritePick() => ((CurrentTool == SpriteEditTool.None) || (CurrentTool == SpriteEditTool.SpritePick))
                                            && (ImageData != null)
                                            && (ImageData.Format == BufferFormat.R8G8B8A8_UNorm)
                                            && (CurrentPanel == null);

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
                if ((Settings.ShowImageSizeWarning) && (ImageData.Buffers[0, ArrayIndex].Data.SizeInBytes > 67108864))
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
        /// Function to determine if a sprite can be clipped.
        /// </summary>
        /// <returns><b>true</b> if the sprite can be clipped, <b>false</b> if not.</returns>
        private bool CanSpriteClip() => ((CurrentTool == SpriteEditTool.None) || (CurrentTool == SpriteEditTool.SpriteClip))
                                            && (Texture != null)
                                            && (CurrentPanel == null);

        /// <summary>
        /// Function to activate (or deactivate) the sprite clipper tool.
        /// </summary>
        private void DoSpriteClip()
        {
            try
            {
                if (CurrentTool == SpriteEditTool.SpriteClip)
                {
                    CurrentTool = SpriteEditTool.None;
                    return;
                }

                CurrentTool = SpriteEditTool.SpriteClip;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, string.Format(Resources.GORSPR_ERR_TOOL_CHANGE, SpriteEditTool.SpriteClip));
            }
        }

        /// <summary>
        /// Function to determine if the sprite vertex offsets can be adjusted.
        /// </summary>
        /// <returns><b>true</b> if the vertices can be adjusted, <b>false</b> if not.</returns>
        private bool CanSpriteVertexOffset() => (Texture != null) && ((CurrentTool == SpriteEditTool.None) || (CurrentTool == SpriteEditTool.CornerResize)) && (CurrentPanel == null);

        /// <summary>
        /// Function to activate the sprite vertex editing functionality.
        /// </summary>
        private void DoSpriteVertexOffset()
        {
            try
            {
                if (CurrentTool == SpriteEditTool.CornerResize)
                {
                    CurrentTool = SpriteEditTool.None;
                    return;
                }

                CurrentTool = SpriteEditTool.CornerResize;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, string.Format(Resources.GORSPR_ERR_TOOL_CHANGE, SpriteEditTool.CornerResize));
            }
        }

        /// <summary>
        /// Function to determine if manual clipping rectangle can be toggled.
        /// </summary>
        /// <returns><b>true</b> if it can be toggled, <b>false</b> if not.</returns>
        private bool CanToggleManualClipRect() => CurrentTool == SpriteEditTool.SpriteClip;

        /// <summary>
        /// Function to toggle the manual clipping rectangle interface.
        /// </summary>
        private void DoToggleManualClipRect()
        {
            try
            {
                ManualRectangleEditor.IsActive = !ManualRectangleEditor.IsActive;
            }
            catch (Exception ex)
            {
                Log.Print("[ERROR] Could not toggle the manual input system.", LoggingLevel.Simple);
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to determine if manual vertex editing can be toggled.
        /// </summary>
        /// <returns><b>true</b> if it can be toggled, <b>false</b> if not.</returns>
        private bool CanToggleManualVertexEdit() => (CurrentTool == SpriteEditTool.CornerResize);

        /// <summary>
        /// Function to toggle the manual vertex editing interface.
        /// </summary>
        private void DoToggleManualVertexEdit()
        {
            try
            {
                ManualVertexEditor.IsActive = !ManualVertexEditor.IsActive;
            }
            catch (Exception ex)
            {
                Log.Print("[ERROR] Could not toggle the manual input system.", LoggingLevel.Simple);
                Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to determine if the sprite picker mask color editor panel can be shown.
        /// </summary>
        /// <returns><b>true</b> if the color editor panel can be shown, <b>false</b> if not.</returns>
        private bool CanShowPickMaskEditor() => (Texture != null) && (CurrentTool == SpriteEditTool.SpritePick) && (CurrentPanel == null);

        /// <summary>
        /// Function to show or hide the sprite picker mask color editor.
        /// </summary>
        private void DoShowPickMaskEditor()
        {
            try                
            {                
                CurrentPanel = SpritePickMaskEditor;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>
        /// Function to determine if the color editor panel can be shown.
        /// </summary>
        /// <returns><b>true</b> if the color editor panel can be shown, <b>false</b> if not.</returns>
        private bool CanShowColorEditor() => (Texture != null) && (CurrentTool == SpriteEditTool.None) && (CurrentPanel == null);

        /// <summary>
        /// Function to show or hide the sprite color editor.
        /// </summary>
        private void DoShowSpriteColorEditor()
        {
            try
            {
                CurrentTool = SpriteEditTool.None;
                ColorEditor.OriginalSpriteColor = ColorEditor.SpriteColor = _sprite.CornerColors;
                CurrentPanel = ColorEditor;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>
        /// Function to determine if the anchor editor panel can be shown.
        /// </summary>
        /// <returns><b>true</b> if the anchor editor can be shown, or <b>false</b> if not.</returns>
        private bool CanShowAnchorEditor() => (Texture != null) && (CurrentTool == SpriteEditTool.None) && (CurrentPanel == null);

        /// <summary>
        /// Function to show or hide the sprite anchor editor.
        /// </summary>
        private void DoShowSpriteAnchorEditor()
        {
            try
            {
                CurrentTool = SpriteEditTool.None;
                AnchorEditor.Bounds = new DX.RectangleF(0, 0, _sprite.Size.Width, _sprite.Size.Height);
                AnchorEditor.AnchorPosition = new DX.Vector2(_sprite.Anchor.X * _sprite.Size.Width, _sprite.Anchor.Y * _sprite.Size.Height);
                CurrentPanel = AnchorEditor;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>
        /// Function to determine if the sprite texture wrapping editor panel can be shown.
        /// </summary>
        /// <returns><b>true</b> if the texture wrapping editor panel can be shown, <b>false</b> if not.</returns>
        private bool CanShowWrappingEditor() => (Texture != null) && (CurrentTool == SpriteEditTool.None) && (CurrentPanel == null);

        /// <summary>
        /// Function to show or hide the sprite texture wrapping editor.
        /// </summary>
        private void DoShowWrappingEditor()
        {
            try
            {
                CurrentPanel = WrappingEditor;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

		/// <summary>
        /// Function to determine if the changes to sprite texture wrapping can be committed back to the sprite.
        /// </summary>
        /// <returns><b>true</b> if the changes can be committed, <b>false</b> if not.</returns>
        private bool CanCommitWrappingChange()
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if ((WrappingEditor == null) || (Texture == null))
            {
                return false;
            }

            return ((SamplerState.WrapU != WrappingEditor.HorizontalWrapping) || (SamplerState.WrapV != WrappingEditor.VerticalWrapping) || (!SamplerState.BorderColor.Equals(WrappingEditor.BorderColor)));
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to commit the texture wrapping changes back to the sprite.
        /// </summary>
        private void DoCommitWrappingChange()
        {
            bool SetWrapping(TextureWrap wrapU, TextureWrap wrapV, GorgonColor borderColor)
            {
                try
                {
                    if ((SamplerState.WrapU == wrapU) && (SamplerState.WrapV == wrapV) && (SamplerState.BorderColor.Equals(in borderColor)))
                    {
                        return false;
                    }

                    GorgonSamplerState state = _samplerBuilder.GetSampler(SamplerState.Filter, wrapU, wrapV, borderColor);
                    _sprite.TextureSampler = state == GorgonSamplerState.Default ? null : state;

                    NotifyPropertyChanged(nameof(SamplerState));

                    ContentState = ContentState.Modified;
                    CurrentPanel = null;
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
                SetWrapping(undoArgs.HorizontalWrap, undoArgs.VerticalWrap, undoArgs.BorderColor);
                return Task.CompletedTask;
            }

            Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
            {
                SetWrapping(redoArgs.HorizontalWrap, redoArgs.VerticalWrap, redoArgs.BorderColor);
                return Task.CompletedTask;
            }

            var wrapUndoArgs = new SpriteUndoArgs
            {
                HorizontalWrap = SamplerState.WrapU,
				VerticalWrap = SamplerState.WrapV,
				BorderColor = SamplerState.BorderColor
            };

            var wrapRedoArgs = new SpriteUndoArgs
            {
                HorizontalWrap = WrappingEditor.HorizontalWrapping,
				VerticalWrap = WrappingEditor.VerticalWrapping,
				BorderColor = WrappingEditor.BorderColor
            };

            if (!SetWrapping(wrapRedoArgs.HorizontalWrap, wrapRedoArgs.VerticalWrap, wrapRedoArgs.BorderColor))
            {
                return;
            }

            _undoService.Record(Resources.GORSPR_UNDO_DESC_WRAP, UndoAction, RedoAction, wrapUndoArgs, wrapRedoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }

        /// <summary>
        /// Function to determine if the current anchor value can be comitted.
        /// </summary>
        /// <returns><b>true</b> if the anchor value can be comitted, <b>false</b> if not.</returns>
        private bool CanCommitAnchorChange()
        {
            if ((AnchorEditor == null) || (Texture == null))
            {
                return false;
            }

            DX.Vector2 anchorPosition = new DX.Vector2(_sprite.Anchor.X * Size.Width, _sprite.Anchor.Y * Size.Height).Truncate();
            return (!AnchorEditor.AnchorPosition.Equals(ref anchorPosition));
        }

        /// <summary>
        /// Function called to change the anchor value of the sprite.
        /// </summary>
        private void DoCommitAnchorChange()
        {
            bool SetAnchor(DX.Vector2 anchor)
            {
                try
                {
                    Anchor = new DX.Vector2(anchor.X / Size.Width, anchor.Y / Size.Height);
                    ContentState = ContentState.Modified;

                    CurrentPanel = null;

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
                SetAnchor(undoArgs.Anchor);
                return Task.CompletedTask;
            }

            Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
            {
                SetAnchor(redoArgs.Anchor);
                return Task.CompletedTask;
            }

            var anchorUndoArgs = new SpriteUndoArgs
            {
                Anchor = new DX.Vector2(_sprite.Anchor.X * Size.Width, _sprite.Anchor.Y * Size.Height)
            };

            var anchorRedoArgs = new SpriteUndoArgs
            {
                Anchor = AnchorEditor.AnchorPosition
            };

            if (!SetAnchor(AnchorEditor.AnchorPosition))
            {
                return;
            }

            _undoService.Record(Resources.GORSPR_UNDO_DESC_ANCHOR, UndoAction, RedoAction, anchorUndoArgs, anchorRedoArgs);
            NotifyPropertyChanged(nameof(UndoCommand));
        }

		/// <summary>
        /// Function to determine if a texture filter can be set on the sprite or not.
        /// </summary>
        /// <param name="_">Not used.</param>
        /// <returns><b>true</b> if the filter can be set, <b>false</b> if not.</returns>
        private bool CanSetTextureFilter(SampleFilter _) => (CurrentPanel == null) && (WrappingEditor != null);

        /// <summary>
        /// Function to set the texture sampler filter on the sprite.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        private void DoSetTextureFilter(SampleFilter filter)
        {
            bool SetAnchor(SampleFilter samplerState)
            {
                try
                {
                    if (SamplerState.Filter == filter)
                    {
                        return false;
                    }

					GorgonSamplerState state = _samplerBuilder.GetSampler(filter, SamplerState.WrapU, SamplerState.WrapV, SamplerState.BorderColor);
                    _sprite.TextureSampler = state == GorgonSamplerState.Default ? null : state;
                   
                    NotifyPropertyChanged(nameof(SamplerState));
                    NotifyPropertyChanged(nameof(IsPixellated));

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
                SetAnchor(undoArgs.Filter);
                return Task.CompletedTask;
            }

            Task RedoAction(SpriteUndoArgs redoArgs, CancellationToken cancelToken)
            {
                SetAnchor(redoArgs.Filter);
                return Task.CompletedTask;
            }

            var anchorUndoArgs = new SpriteUndoArgs
            {
                Filter = _sprite.TextureSampler == null ? SampleFilter.MinMagMipLinear : _sprite.TextureSampler.Filter
            };

            var anchorRedoArgs = new SpriteUndoArgs
            {
                Filter = filter
            };

            if (!SetAnchor(anchorRedoArgs.Filter))
            {
                return;
            }

            _undoService.Record(Resources.GORSPR_UNDO_DESC_SAMPLER, UndoAction, RedoAction, anchorUndoArgs, anchorRedoArgs);
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
                    if (CurrentPanel != null)
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

        /// <summary>Function to determine the action to take when this content is closing.</summary>
        /// <returns>
        ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
        /// <remarks>PlugIn authors should override this method to confirm whether save changed content, continue without saving, or cancel the operation entirely.</remarks>
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

            _factory = injectionParameters.Factory ?? throw new ArgumentMissingException(nameof(injectionParameters.Factory), nameof(injectionParameters));
            _contentFiles = injectionParameters.ContentFileManager ?? throw new ArgumentMissingException(nameof(injectionParameters.ContentFileManager), nameof(injectionParameters));
            _sprite = injectionParameters.Sprite ?? throw new ArgumentMissingException(nameof(injectionParameters.Sprite), nameof(injectionParameters));
            _undoService = injectionParameters.UndoService ?? throw new ArgumentMissingException(nameof(injectionParameters.UndoService), nameof(injectionParameters));
            _textureService = injectionParameters.TextureService ?? throw new ArgumentMissingException(nameof(injectionParameters.TextureService), nameof(injectionParameters));
            _spriteCodec = injectionParameters.SpriteCodec ?? throw new ArgumentMissingException(nameof(injectionParameters.SpriteCodec), nameof(injectionParameters));
            _samplerBuilder = injectionParameters.SamplerBuilder ?? throw new ArgumentMissingException(nameof(injectionParameters.SamplerBuilder), nameof(injectionParameters));
            ManualRectangleEditor = injectionParameters.ManualRectangleEditor ?? throw new ArgumentMissingException(nameof(injectionParameters.ManualRectangleEditor), nameof(injectionParameters));
            ManualVertexEditor = injectionParameters.ManualVertexEditor ?? throw new ArgumentMissingException(nameof(injectionParameters.ManualVertexEditor), nameof(injectionParameters));
            SpritePickMaskEditor = injectionParameters.SpritePickMaskEditor ?? throw new ArgumentMissingException(nameof(injectionParameters.SpritePickMaskEditor), nameof(injectionParameters));
            ColorEditor = injectionParameters.ColorEditor ?? throw new ArgumentMissingException(nameof(injectionParameters.ColorEditor), nameof(injectionParameters));
            AnchorEditor = injectionParameters.AnchorEditor ?? throw new ArgumentMissingException(nameof(injectionParameters.AnchorEditor), nameof(injectionParameters));
            WrappingEditor = injectionParameters.SpriteWrappingEditor ?? throw new ArgumentMissingException(nameof(injectionParameters.SpriteWrappingEditor), nameof(injectionParameters));
            Settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));

            _originalTexture = injectionParameters.SpriteTextureFile;

            SetupTextureFile(injectionParameters.SpriteTextureFile);

            ColorEditor.OkCommand = new EditorCommand<object>(DoCommitColorChange, CanCommitColorChange);
            AnchorEditor.OkCommand = new EditorCommand<object>(DoCommitAnchorChange, CanCommitAnchorChange);
            WrappingEditor.OkCommand = new EditorCommand<object>(DoCommitWrappingChange, CanCommitWrappingChange);

            ManualRectangleEditor.ApplyCommand = new EditorCommand<object>(DoApplyClip, CanApplyClip);
            ManualRectangleEditor.CancelCommand = new EditorCommand<object>(DoCancelClip);
            ManualRectangleEditor.SetFullSizeCommand = new EditorCommand<object>(DoSetFullSize, CanSetFullSize);

            ManualVertexEditor.ApplyCommand = new EditorCommand<object>(DoApplyVertexEdit, CanApplyVertexEdit);
            ManualVertexEditor.CancelCommand = new EditorCommand<object>(DoCancelVertexEdit);            
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
            CurrentPanel = null;

            if (ColorEditor != null)
            {
                ColorEditor.OkCommand = null;
            }

            CurrentTool = SpriteEditTool.None;

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
            if (((_textureFile != null) && (string.Equals(_textureFile.Path, dragData.File.Path, StringComparison.OrdinalIgnoreCase)))
                || (!_textureService.IsContentImage(dragData.File))
                || (_currentTool != SpriteEditTool.None)
                || (CurrentPanel != null))
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
                        Size = new DX.Size2F(texture.Width, texture.Height);
                        File.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);
                    }
                    else
                    {
                        DX.Rectangle updatedSize = texture.ToPixel(TextureCoordinates);
						// Readjust the size to change with the texture coordinates.
                        Size = new DX.Size2F(updatedSize.Width, updatedSize.Height);
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
        /// <summary>Initializes a new instance of the <see cref="SpriteContent"/> class.</summary>
        public SpriteContent()
        {
            SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveSpriteTask, CanSaveSprite);
            UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
            RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
            SpritePickCommand = new EditorCommand<object>(DoSpritePick, CanSpritePick);
            SpriteClipCommand = new EditorCommand<object>(DoSpriteClip, CanSpriteClip);
            NewSpriteCommand = new EditorCommand<object>(DoCreateSprite, CanCreateSprite);
            ToggleManualClipRectCommand = new EditorCommand<object>(DoToggleManualClipRect, CanToggleManualClipRect);
            ToggleManualVertexEditCommand = new EditorCommand<object>(DoToggleManualVertexEdit, CanToggleManualVertexEdit);
            ShowColorEditorCommand = new EditorCommand<object>(DoShowSpriteColorEditor, CanShowColorEditor);
            ShowAnchorEditorCommand = new EditorCommand<object>(DoShowSpriteAnchorEditor, CanShowAnchorEditor);
            ShowSpritePickMaskEditorCommand = new EditorCommand<object>(DoShowPickMaskEditor, CanShowPickMaskEditor);
            SpriteVertexOffsetCommand = new EditorCommand<object>(DoSpriteVertexOffset, CanSpriteVertexOffset);            
            SetVertexOffsetsCommand = new EditorCommand<IReadOnlyList<DX.Vector3>>(DoCommitVertexOffsets, CanCommitVertexOffsets);
			SetTextureFilteringCommand = new EditorCommand<SampleFilter>(DoSetTextureFilter, CanSetTextureFilter);
            ShowWrappingEditorCommand = new EditorCommand<object>(DoShowWrappingEditor, CanShowWrappingEditor);
        }
        #endregion
    }
}
