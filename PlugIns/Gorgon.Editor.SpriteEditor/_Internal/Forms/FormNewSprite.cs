
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 31, 2020 10:49:39 PM
// 


using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.Content;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A form used to set up a new sprite
/// </summary>
internal partial class FormNewSprite
    : Form
{
    // The list of textures.
    private IReadOnlyList<IContentFile> _textures = [];
    // The preview image for the selected texture.
    private Image _previewImage;
    // The cancellation token source for the preview thread.
    private CancellationTokenSource _cancelSource;
    // The task used to load the preview image.
    private Task<IGorgonImage> _previewTask;
    // The original size for the sprite.
    private Vector2? _originalSize;
    // The path to the preview directory.
    private static readonly string _previewDirPath = $"/Thumbnails/";

    /// <summary>
    /// Property to set or return the image codec for sprites.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IGorgonImageCodec ImageCodec
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
    [Browsable(false)]
    public string CueText
    {
        get => TextName.CueText;
        set => TextName.CueText = value;
    }

    /// <summary>
    /// Property to return the width and height for the sprite.
    /// </summary>
    public Vector2 SpriteSize => new((float)NumericWidth.Value, (float)NumericHeight.Value);

    /// <summary>
    /// Property to return the selected texture file.
    /// </summary>
    public IContentFile TextureFile
    {
        get;
        private set;
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
            if (_previewTask is not null)
            {
                if (_previewTask.Status == TaskStatus.Faulted)
                {
                    _previewImage?.Dispose();
                    PicturePreview.Image = null;
                    _previewTask = null;
                    return;
                }

                _cancelSource.Cancel();
                (await _previewTask)?.Dispose();
                _previewTask = null;
            }

            if (string.IsNullOrWhiteSpace(e.FileEntry.File.Metadata.Thumbnail))
            {
                e.FileEntry.File.Metadata.Thumbnail = Guid.NewGuid().ToString("N");
            }
            string filePath = _previewDirPath + e.FileEntry.File.Metadata.Thumbnail;

            _cancelSource = new CancellationTokenSource();

            imageStream = FileManager.OpenStream(e.FileEntry.FullPath, FileMode.Open);
            IGorgonImageInfo metaData = ImageCodec.GetMetaData(imageStream);
            imageStream.Dispose();

            _previewTask = e.FileEntry.File.Metadata.ContentMetadata.GetThumbnailAsync(e.FileEntry.File, filePath, _cancelSource.Token);
            image = await _previewTask;

            if (_cancelSource.IsCancellationRequested)
            {
                _previewImage?.Dispose();
                PicturePreview.Image = null;
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
                    PicturePreview.Image = _previewImage = Resources.no_thumb_sprite_64x64;
                    return;
                }
            }

            PicturePreview.Image = _previewImage = image.Buffers[0].ToBitmap();

            // Default width/height to the image size.
            if (_originalSize is null)
            {
                NumericWidth.Value = metaData.Width.Max(1).Min((int)NumericWidth.Maximum);
                NumericHeight.Value = metaData.Height.Max(1).Min((int)NumericHeight.Maximum);
            }

            TextureFile = e.FileEntry.File;
        }
        catch
        {
            // Do nothing.                 
            _previewImage?.Dispose();
            PicturePreview.Image = null;
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
        PicturePreview.Image = null;
        TextureFile = null;
    }

    /// <summary>Handles the Search event of the FileTextures control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="Gorgon.UI.GorgonSearchEventArgs"/> instance containing the event data.</param>
    private void FileTextures_Search(object sender, Gorgon.UI.GorgonSearchEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.SearchText))
        {
            FillTextureList(_textures);
            return;
        }

        FillTextureList(_textures.Where(item => item.Name.IndexOf(e.SearchText, StringComparison.CurrentCultureIgnoreCase) > -1).ToArray());
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

    /// <summary>
    /// Function to populate the texture browser.
    /// </summary>
    private void FillTextureList(IReadOnlyList<IContentFile> textures)
    {
        ContentFileExplorerFileEntry selectedTexture = null;
        Dictionary<string, ContentFileExplorerDirectoryEntry> dirs = new(StringComparer.OrdinalIgnoreCase);

        foreach (IContentFile texture in textures.OrderBy(item => item.Path))
        {
            List<ContentFileExplorerFileEntry> fileEntries = null;
            string dirName = Path.GetDirectoryName(texture.Path).FormatDirectory('/');

            if (!dirs.TryGetValue(dirName, out ContentFileExplorerDirectoryEntry dirEntry))
            {
                fileEntries = [];
                dirEntry = new ContentFileExplorerDirectoryEntry(dirName, fileEntries);
                dirs[dirName] = dirEntry;
            }
            else
            {
                fileEntries = (List<ContentFileExplorerFileEntry>)dirEntry.Files;
            }

            ContentFileExplorerFileEntry file = new(texture, dirEntry);

            if (TextureFile == texture)
            {
                selectedTexture = file;
                file.IsSelected = true;
            }

            fileEntries.Add(file);
        }

        FileTextures.Entries = [.. dirs.Values];

        if (selectedTexture is null)
        {
            return;
        }

        FileTextures_FileEntrySelected(FileTextures, new ContentFileEntrySelectedEventArgs(selectedTexture));
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
        TextureFile = currentTexture;
        FillTextureList(textures);
    }

    /// <summary>
    /// Function to assign the original size for the sprite.
    /// </summary>
    /// <param name="size">The size to assign, or <b>null</b> to automatically size.</param>
    public void SetOriginalSize(Vector2? size)
    {
        _originalSize = size;

        if (_originalSize is not null)
        {
            NumericWidth.Value = (int)_originalSize.Value.X.Max(1).Min((int)NumericWidth.Maximum);
            NumericHeight.Value = (int)_originalSize.Value.Y.Max(1).Min((int)NumericHeight.Maximum);
        }
    }

    /// <summary>Initializes a new instance of the <see cref="FormNewSprite"/> class.</summary>
    public FormNewSprite() => InitializeComponent();
}
