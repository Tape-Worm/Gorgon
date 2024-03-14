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
// Created: May 7, 2020 12:57:05 PM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The service used to query for new sprite information.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="NewSpriteService"/> class.</remarks>
/// <param name="fileManager">The project file manager.</param>
/// <param name="imageCodec">The image codec for loading texture data.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileManager"/> parameter is <b>null</b>.</exception>
internal class NewSpriteService(IContentFileManager fileManager, IGorgonImageCodec imageCodec)
{
    #region Variables.
    // The file manager for the project file system.
    private readonly IContentFileManager _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    // The image codec for loading texture data.
    private readonly IGorgonImageCodec _imageCodec = imageCodec ?? throw new ArgumentNullException(nameof(imageCodec));
    #endregion

    #region Methods.
    /// <summary>
    /// Function to determine if an image content file is a 2D image or not.
    /// </summary>
    /// <param name="file">The content file for the image.</param>
    /// <returns><b>true</b> if the image is 2D, or <b>false</b> if not.</returns>
    private bool Is2DImage(IContentFile file)
    {
        using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
        IGorgonImageInfo metadata = _imageCodec.GetMetaData(stream);
        return metadata.ImageType is not ImageType.Image3D and not ImageType.Image1D;
    }

    /// <summary>
    /// Function to retrieve the new sprite name.
    /// </summary>
    /// <param name="currentSprite">The current sprite.</param>
    /// <param name="currentTexture">The current sprite texture.</param>
    /// <param name="currentSize">The size of the current sprite.</param>
    /// <returns>A tuple containing the new sprite name, the associated sprite texture file, and the initial size of the sprite.</returns>
    public (string spriteName, IContentFile textureFile, DX.Size2F spriteSize) GetNewSpriteName(IContentFile currentSprite, IContentFile currentTexture, DX.Size2F currentSize)
    {
        if (currentSprite is null)
        {
            throw new ArgumentNullException(nameof(currentSprite));
        }


        string spritePath = currentSprite.Path;

        int counter = 0;
        while (_fileManager.GetFile(spritePath) is not null)
        {
            string directory = Path.GetDirectoryName(currentSprite.Path).FormatDirectory('/');
            string filename = Path.GetFileNameWithoutExtension(currentSprite.Name);
            string extension = Path.GetExtension(currentSprite.Name);

            spritePath = $"{directory}{filename} ({++counter}){extension}";
        }

        // Find all available textures in our file system.
        IReadOnlyList<IContentFile> textures = _fileManager.EnumerateContentFiles("/", "*", true)
                                            .Where(item => (item.Metadata.Attributes.ContainsKey(CommonEditorConstants.ContentTypeAttr))
                                                    && (string.Equals(item.Metadata.Attributes[CommonEditorConstants.ContentTypeAttr], CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase))
                                                    && (Is2DImage(item)))
                                            .ToArray();

        using var newSpriteForm = new FormNewSprite()
        {
            ImageCodec = _imageCodec,
            FileManager = _fileManager,
            Text = Resources.GORSPR_CAPTION_SPRITE_NAME,
            ObjectName = Path.GetFileName(spritePath),
            CueText = Resources.GORSPR_TEXT_CUE_SPRITE_NAME
        };
        newSpriteForm.FillTextures(textures, currentTexture);
        newSpriteForm.SetOriginalSize(currentSize);

        return newSpriteForm.ShowDialog(GorgonApplication.MainForm) == DialogResult.OK
            ? (newSpriteForm.ObjectName, newSpriteForm.TextureFile, newSpriteForm.SpriteSize)
            : (null, null, DX.Size2F.Zero);
    }

    #endregion
    #region Constructor.
    #endregion
}
