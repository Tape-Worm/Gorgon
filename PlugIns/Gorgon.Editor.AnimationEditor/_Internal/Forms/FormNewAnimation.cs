#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: March 31, 2020 10:49:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.Content;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A form used to set up a new sprite.
    /// </summary>
    internal partial class FormNewAnimation
        : Form
    {
        #region Variables.
        // The list of textures.
        private IReadOnlyList<IContentFile> _textures = Array.Empty<IContentFile>();
        // The list of textures.
        private IReadOnlyList<IContentFile> _sprites = Array.Empty<IContentFile>();
        // The preview images for the selected files.
        private Image _previewImage;
        private Image _previewSprite;
        // The cancellation token source for the preview threads.
        private CancellationTokenSource _cancelTextureSource;
        private CancellationTokenSource _cancelSpriteSource;
        // The task used to load the preview images.
        private Task<IGorgonImage> _previewTextureTask;
        private Task<IGorgonImage> _previewSpriteTask;
        // The original length and FPS for the animation.
        private float _originalLength;
        private float _originalFps;
        // The path to the preview directory.
        private static readonly string _previewDirPath = $"/Thumbnails/";
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the image codec for textures.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGorgonImageCodec ImageCodec
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the sprite codec for sprites.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGorgonSpriteCodec SpriteCodec
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the file manager for the project.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IContentFileManager FileManager
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the name of the object.
        /// </summary>
        [Category("Appearance"), Description("The name of the object to be named."), DefaultValue("")]
        public string ObjectName
        {
            get => TextName.Text;
            set => TextName.Text = value?.FormatFileName().Trim() ?? string.Empty;
        }

        /// <summary>
        /// Property to set or return the cue text for the name box.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CueText
        {
            get => TextName.CueText;
            set => TextName.CueText = value;
        }

        /// <summary>
        /// Property to return the selected texture file for a background image.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IContentFile BackgroundTextureFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the selected primary file.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IContentFile PrimarySpriteFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the length of the animation, in seconds.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Length => (float)NumericLength.Value;

        /// <summary>
        /// Property to return the frames per second for the animation.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Fps => (float)NumericFps.Value;
        #endregion

        #region Methods.
        /// <summary>Handles the FileEntrySelected event of the FilePrimarySprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContentFileEntrySelectedEventArgs"/> instance containing the event data.</param>
        private async void FilePrimarySprite_FileEntrySelected(object sender, ContentFileEntrySelectedEventArgs e)
        {
            IGorgonImage image = null;
            Stream imageStream = null;

            try
            {
                if (_previewSpriteTask != null)
                {
                    if (_previewSpriteTask.Status == TaskStatus.Faulted)
                    {
                        _previewSprite?.Dispose();
                        PictureTexturePreview.Image = null;
                        _previewSpriteTask = null;
                        return;
                    }

                    _cancelSpriteSource.Cancel();
                    (await _previewSpriteTask)?.Dispose();
                    _previewSpriteTask = null;
                }

                if (string.IsNullOrWhiteSpace(e.FileEntry.File.Metadata.Thumbnail))
                {
                    e.FileEntry.File.Metadata.Thumbnail = Guid.NewGuid().ToString("N");
                }
                string filePath = _previewDirPath + e.FileEntry.File.Metadata.Thumbnail;

                _cancelSpriteSource = new CancellationTokenSource();

                _previewSpriteTask = e.FileEntry.File.Metadata.ContentMetadata.GetThumbnailAsync(e.FileEntry.File, filePath, _cancelSpriteSource.Token);
                image = await _previewSpriteTask;

                _previewSprite?.Dispose();
                if (_cancelSpriteSource.IsCancellationRequested)
                {
                    PictureSpritePreview.Image = null;
                    image?.Dispose();
                    return;
                }                

                if (image.Format != BufferFormat.R8G8B8A8_UNorm)
                {
                    if (image.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm))
                    {
                        image.BeginUpdate().ConvertToFormat(BufferFormat.R8G8B8A8_UNorm).EndUpdate();
                    }
                    else
                    {
                        // If we cannot convert, then use a placeholder.
                        image.Dispose();
                        PictureSpritePreview.Image = _previewImage = Resources.no_thumb_sprite_64x64;
                        return;
                    }
                }

                PictureSpritePreview.Image = _previewSprite = image.Buffers[0].ToBitmap();

                PrimarySpriteFile = e.FileEntry.File;
            }
            catch
            {
                // Do nothing.                 
                _previewSprite?.Dispose();
                PictureSpritePreview.Image = null;
            }
            finally
            {
                imageStream?.Dispose();
                image?.Dispose();
            }
        }

        /// <summary>Handles the FileEntryUnselected event of the FilePrimarySprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContentFileEntrySelectedEventArgs"/> instance containing the event data.</param>
        private void FilePrimarySprite_FileEntryUnselected(object sender, ContentFileEntrySelectedEventArgs e)
        {
            _previewSprite?.Dispose();
            PictureSpritePreview.Image = null;
            PrimarySpriteFile = null;
        }

        /// <summary>Handles the Search event of the FilePrimarySprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Gorgon.UI.GorgonSearchEventArgs"/> instance containing the event data.</param>
        private void FilePrimarySprite_Search(object sender, Gorgon.UI.GorgonSearchEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.SearchText))
            {
                FillFileList(_sprites, PrimarySpriteFile, FilePrimarySprite, FilePrimarySprite_FileEntrySelected);
                ;
                return;
            }

            FillFileList(_sprites.Where(item => item.Name.IndexOf(e.SearchText, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray(),
                         PrimarySpriteFile,
                         FilePrimarySprite,
                         FilePrimarySprite_FileEntrySelected);
        }

        /// <summary>Handles the FileEntrySelected event of the FileTextures control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContentFileEntrySelectedEventArgs"/> instance containing the event data.</param>
        private async void FileTextures_FileEntrySelected(object sender, ContentFileEntrySelectedEventArgs e)
        {
            IGorgonImage image = null;
            Stream imageStream = null;

            try
            {
                if (_previewTextureTask != null)
                {
                    if (_previewTextureTask.Status == TaskStatus.Faulted)
                    {
                        _previewImage?.Dispose();
                        PictureTexturePreview.Image = null;
                        _previewTextureTask = null;
                        return;
                    }

                    _cancelTextureSource.Cancel();
                    (await _previewTextureTask)?.Dispose();
                    _previewTextureTask = null;
                }

                if (string.IsNullOrWhiteSpace(e.FileEntry.File.Metadata.Thumbnail))
                {
                    e.FileEntry.File.Metadata.Thumbnail = Guid.NewGuid().ToString("N");
                }
                string filePath = _previewDirPath + e.FileEntry.File.Metadata.Thumbnail;

                _cancelTextureSource = new CancellationTokenSource();

                _previewTextureTask = e.FileEntry.File.Metadata.ContentMetadata.GetThumbnailAsync(e.FileEntry.File, filePath, _cancelTextureSource.Token);
                image = await _previewTextureTask;

                if (_cancelTextureSource.IsCancellationRequested)
                {
                    _previewImage?.Dispose();
                    PictureTexturePreview.Image = null;
                    image?.Dispose();
                    return;
                }

                _previewImage?.Dispose();

                if (image.Format != BufferFormat.R8G8B8A8_UNorm)
                {
                    if (image.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm))
                    {
                        image.BeginUpdate().ConvertToFormat(BufferFormat.R8G8B8A8_UNorm).EndUpdate();
                    }
                    else
                    {
                        // If we cannot convert, then use a placeholder.
                        image.Dispose();
                        PictureTexturePreview.Image = _previewImage = Resources.no_thumb_sprite_64x64;
                        return;
                    }
                }

                PictureTexturePreview.Image = _previewImage = image.Buffers[0].ToBitmap();

                BackgroundTextureFile = e.FileEntry.File;
            }
            catch
            {
                // Do nothing.                 
                _previewImage?.Dispose();
                PictureTexturePreview.Image = null;
            }
            finally
            {
                imageStream?.Dispose();
                image?.Dispose();
            }
        }

        /// <summary>Handles the FileEntryUnselected event of the FileTextures control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContentFileEntrySelectedEventArgs"/> instance containing the event data.</param>
        private void FileTextures_FileEntryUnselected(object sender, ContentFileEntrySelectedEventArgs e)
        {
            _previewImage?.Dispose();
            PictureTexturePreview.Image = null;
            BackgroundTextureFile = null;
        }

        /// <summary>Handles the Search event of the FileTextures control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Gorgon.UI.GorgonSearchEventArgs"/> instance containing the event data.</param>
        private void FileTextures_Search(object sender, Gorgon.UI.GorgonSearchEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.SearchText))
            {
                FillFileList(_textures, BackgroundTextureFile, FileBgTextures, FileTextures_FileEntrySelected);
                return;
            }

            FillFileList(_textures.Where(item => item.Name.IndexOf(e.SearchText, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray(), 
                         BackgroundTextureFile, 
                         FileBgTextures, 
                         FileTextures_FileEntrySelected);
        }

        /// <summary>Handles the Leave event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_Leave(object sender, EventArgs e)
        {
            PanelUnderline.BackColor = DarkFormsRenderer.WindowBackground;
            TextName.BackColor = DarkFormsRenderer.WindowBackground;
            TextName.ForeColor = ForeColor;

            if (!string.IsNullOrWhiteSpace(TextName.Text))
            {
                TextName.Text = TextName.Text.Trim().FormatFileName();
            }
        }

        /// <summary>Handles the MouseEnter event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_MouseEnter(object sender, EventArgs e)
        {
            if (TextName.Focused)
            {
                return;
            }

            TextName.BackColor = Color.FromKnownColor(KnownColor.SteelBlue);
        }

        /// <summary>Handles the MouseLeave event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_MouseLeave(object sender, EventArgs e)
        {
            if (TextName.Focused)
            {
                return;
            }

            TextName.BackColor = DarkFormsRenderer.WindowBackground;
            TextName.ForeColor = ForeColor;
        }

        /// <summary>Handles the Enter event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_Enter(object sender, EventArgs e)
        {
            PanelUnderline.BackColor = Color.Black;
            TextName.Parent.BackColor = TextName.BackColor = Color.White;
            TextName.ForeColor = BackColor;
            TextName.SelectAll();
        }

        /// <summary>Handles the TextChanged event of the TextName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextName_TextChanged(object sender, EventArgs e) => ButtonOK.Enabled = !string.IsNullOrWhiteSpace(ObjectName);

        /// <summary>Handles the ValueChanged event of the NumericLength control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericLength_ValueChanged(object sender, EventArgs e) => UpdateFrameCountLabel();

        /// <summary>
        /// Function to update the label used to display the total number of frames for the animation.
        /// </summary>
        private void UpdateFrameCountLabel() => LabelFrameCount.Text = string.Format(Resources.GORANM_TEXT_FRAME_COUNT, (NumericFps.Value * NumericLength.Value).Round());

        /// <summary>
        /// Function to populate the file browsers.
        /// </summary>
        /// <param name="files">The files to enumerate.</param>
        /// <param name="currentFile">The currently selected file.</param>
        /// <param name="target">The target control.</param>
        /// <param name="selectedEvent">The event method to call for a selected file.</param>
        private void FillFileList(IReadOnlyList<IContentFile> files, IContentFile currentFile, ContentFileExplorer target, Action<object, ContentFileEntrySelectedEventArgs> selectedEvent)
        {
            ContentFileExplorerFileEntry selectedTexture = null;
            var dirs = new Dictionary<string, ContentFileExplorerDirectoryEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (IContentFile file in files.OrderBy(item => item.Path))
            {
                List<ContentFileExplorerFileEntry> fileEntries = null;
                string dirName = Path.GetDirectoryName(file.Path).FormatDirectory('/');

                if (!dirs.TryGetValue(dirName, out ContentFileExplorerDirectoryEntry dirEntry))
                {
                    fileEntries = new List<ContentFileExplorerFileEntry>();
                    dirEntry = new ContentFileExplorerDirectoryEntry(dirName, fileEntries);
                    dirs[dirName] = dirEntry;
                }
                else
                {
                    fileEntries = (List<ContentFileExplorerFileEntry>)dirEntry.Files;
                }

                var contentFile = new ContentFileExplorerFileEntry(file, dirEntry);

                if (currentFile == file)
                {
                    selectedTexture = contentFile;
                    contentFile.IsSelected = true;
                }

                fileEntries.Add(contentFile);
            }

            target.Entries = dirs.Values.ToArray();

            if (selectedTexture == null)
            {
                return;
            }

            selectedEvent?.Invoke(target, new ContentFileEntrySelectedEventArgs(selectedTexture));
        }

        /// <summary>Handles the <see cref="UserControl.Load"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            TextName.Select();
            TextName.SelectAll();
        }

        /// <summary>
        /// Function to populate the file list with available textures from the project file system.
        /// </summary>
        /// <param name="textures">The textures to display.</param>
        /// <param name="currentTexture">[Optional] The currently active texture file.</param>
        public void FillTextures(IReadOnlyList<IContentFile> textures, IContentFile currentTexture = null)
        {
            _textures = textures;
            BackgroundTextureFile = currentTexture;
            FillFileList(textures, currentTexture, FileBgTextures, FileTextures_FileEntrySelected);
        }

        /// <summary>
        /// Function to populate the file list with available sprites from the project file system.
        /// </summary>
        /// <param name="textures">The textures to display.</param>
        /// <param name="currentTexture">[Optional] The currently active texture file.</param>
        public void FillSprites(IReadOnlyList<IContentFile> sprites, IContentFile currentSprite = null)
        {
            _sprites = sprites;
            PrimarySpriteFile = currentSprite;
            FillFileList(sprites, currentSprite, FilePrimarySprite, FilePrimarySprite_FileEntrySelected);
        }

        /// <summary>
        /// Function to assign the original size for the sprite.
        /// </summary>
        /// <param name="length">The length of the animation, in seconds.</param>
        /// <param name="fps">The number of frames per second.</param>
        public void SetDefaults(float length, float fps)
        {
            _originalLength = length;
            _originalFps = fps;

            NumericLength.Value = ((decimal)_originalLength).Max(NumericLength.Minimum).Min(NumericLength.Maximum);
            NumericFps.Value = ((decimal)_originalFps).Max(NumericFps.Minimum).Min(NumericFps.Maximum);
            UpdateFrameCountLabel();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormNewAnimation"/> class.</summary>
        public FormNewAnimation() => InitializeComponent();
        #endregion
    }
}
