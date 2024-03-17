
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
// Created: July 23, 2020 12:34:48 PM
// 


using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Content;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The service used to query for new animation information
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="NewAnimationService"/> class.</remarks>
/// <param name="fileManager">The project file manager.</param>
/// <param name="spriteCodec">The sprite codec for loading sprite data.</param>
/// <param name="imageCodec">The image codec for loading texture data.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileManager"/>, <paramref name="spriteCodec"/> or the <paramref name="imageCodec"/> parameter is <b>null</b>.</exception>
internal class NewAnimationService(IContentFileManager fileManager, IGorgonSpriteCodec spriteCodec, IGorgonImageCodec imageCodec)
{

    // The file manager for the project file system.
    private readonly IContentFileManager _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    // The image codec for loading texture data.
    private readonly IGorgonImageCodec _imageCodec = imageCodec ?? throw new ArgumentNullException(nameof(imageCodec));
    // THe sprite codec for loading sprite data.
    private readonly IGorgonSpriteCodec _spriteCodec = spriteCodec ?? throw new ArgumentNullException(nameof(spriteCodec));



    /// <summary>
    /// Function to determine if an image content file is a sprite or not.
    /// </summary>
    /// <param name="file">The content file for the sprite.</param>
    /// <returns><b>true</b> if the file is a sprite, or <b>false</b> if not.</returns>
    private bool IsSprite(IContentFile file)
    {
        using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
        return _spriteCodec.IsReadable(stream);
    }

    /// <summary>
    /// Function to determine if an image content file is a 2D image or not.
    /// </summary>
    /// <param name="file">The content file for the image.</param>
    /// <returns><b>true</b> if the image is 2D, or <b>false</b> if not.</returns>
    private bool Is2DImage(IContentFile file)
    {
        using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
        IGorgonImageInfo metadata = _imageCodec.GetMetaData(stream);
        return metadata.ImageType is not ImageDataType.Image3D and not ImageDataType.Image1D;
    }

    /// <summary>
    /// Function to retrieve the new animation name.
    /// </summary>
    /// <param name="animDirectory">The current directory where the animation will be saved.</param>
    /// <param name="baseAnimationName">The base name for the animation file.</param>
    /// <param name="primarySprite">The sprite to load as the primary sprite.</param>
    /// <param name="bgTextureFile">The texture used as the background image.</param>
    /// <param name="length">The length of the animation, in seconds.</param>
    /// <param name="fps">The frames per second for the animation.</param>
    /// <returns>A tuple containing the new animation name, the length of the animation, the frames per second, the primary sprite file, and the primary sprite.</returns>
    public (string animationName, float length, float fps, IContentFile primarySpriteFile, IContentFile bgTextureFile) GetNewAnimationName(string animDirectory, string baseAnimationName, IContentFile primarySprite, IContentFile bgTextureFile, float length = 1.0f, float fps = 60.0f)
    {
        string animationPath = animDirectory.FormatDirectory('/') + baseAnimationName.FormatFileName();

        int counter = 0;
        while (_fileManager.GetFile(animationPath) is not null)
        {
            string filename = Path.GetFileNameWithoutExtension(baseAnimationName);
            string extension = Path.GetExtension(baseAnimationName);

            animationPath = $"{animDirectory}{filename} ({++counter}){extension}";
        }

        // Find all available textures and sprites in our file system.
        IReadOnlyList<IContentFile> textures = _fileManager.EnumerateContentFiles("/", "*", true)
                                            .Where(item => (item.Metadata.Attributes.ContainsKey(CommonEditorConstants.ContentTypeAttr))
                                                    && (string.Equals(item.Metadata.Attributes[CommonEditorConstants.ContentTypeAttr], CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase))
                                                    && (Is2DImage(item)))
                                            .ToArray();

        IReadOnlyList<IContentFile> sprites = _fileManager.EnumerateContentFiles("/", "*", true)
                                            .Where(item => (item.Metadata.Attributes.ContainsKey(CommonEditorConstants.ContentTypeAttr))
                                                    && (string.Equals(item.Metadata.Attributes[CommonEditorConstants.ContentTypeAttr], CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase))
                                                    && (IsSprite(item)))
                                            .ToArray();

        using FormNewAnimation newAnimationForm = new()
        {
            ImageCodec = _imageCodec,
            SpriteCodec = _spriteCodec,
            FileManager = _fileManager,
            Text = Resources.GORANM_CAPTION_ANIMATION_NAME,
            ObjectName = Path.GetFileName(animationPath),
            CueText = Resources.GORANM_TEXT_CUE_ANIMATION_NAME
        };
        newAnimationForm.SetDefaults(length, fps);
        newAnimationForm.FillSprites(sprites, primarySprite);
        newAnimationForm.FillTextures(textures, bgTextureFile);

        return newAnimationForm.ShowDialog(GorgonApplication.MainForm) == DialogResult.OK
            ? (newAnimationForm.ObjectName, newAnimationForm.Length, newAnimationForm.Fps, newAnimationForm.PrimarySpriteFile, newAnimationForm.BackgroundTextureFile)
            : (null, 0, 0, null, null);
    }




}
