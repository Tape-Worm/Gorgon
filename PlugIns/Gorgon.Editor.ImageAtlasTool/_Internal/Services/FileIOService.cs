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
// Created: May 8, 2019 11:46:39 AM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.ImageAtlasTool.Properties;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.Renderers.Services;

namespace Gorgon.Editor.ImageAtlasTool;

/// <summary>
/// The service used for file operations.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FileIOService"/> class.</remarks>
/// <param name="fileSystem">The file system.</param>
/// <param name="imageCodec">The default image codec to use.</param>
/// <param name="spriteCodec">The default sprite codec to use.</param>
internal class FileIOService(IContentFileManager fileSystem, IGorgonImageCodec imageCodec, IGorgonSpriteCodec spriteCodec)
{
    #region Variables.
    // The file system for content files.
    private readonly IContentFileManager _fileSystem = fileSystem;
    // The default image codec.
    private readonly IGorgonImageCodec _defaultImageCodec = imageCodec;
    // The default sprite codec.
    private readonly IGorgonSpriteCodec _defaultSpriteCodec = spriteCodec;
    #endregion

    #region Methods.
    /// <summary>Function to load the images used to generate the atlas texture(s).</summary>
    /// <param name="files">The image files to load.</param>
    /// <param name="progress">The progress of the operation.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    /// <returns>A list of images and their associated files.</returns>
    public Task<IReadOnlyDictionary<IContentFile, IGorgonImage>> LoadImagesAsync(IEnumerable<IContentFile> files, Action<string> progress, CancellationToken cancelToken) =>
        Task.Run(() =>
        {
            var result = new Dictionary<IContentFile, IGorgonImage>();
            Stream stream = null;

            try
            {
                foreach (IContentFile file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        foreach (IGorgonImage image in result.Values)
                        {
                            image?.Dispose();
                        }
                        result.Clear();
                        return result;
                    }

                    progress?.Invoke(file.Name);

                    stream = _fileSystem.OpenStream(file.Path, FileMode.Open);
                    result[file] = _defaultImageCodec.FromStream(stream);
                    stream.Dispose();
                }
            }
            finally
            {
                stream?.Dispose();
            }

            return (IReadOnlyDictionary<IContentFile, IGorgonImage>)result;
        });

    /// <summary>Function to determine if a directory exists or not.</summary>
    /// <param name="path">The path to the directory/</param>
    /// <returns>
    ///   <b>true</b> if the directory exists, <b>false</b> if not.</returns>
    public bool DirectoryExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        path = path.FormatDirectory('/');

        return _fileSystem.DirectoryExists(path);
    }

    /// <summary>Function to determine if there are any existing files that match the texture names or image names.</summary>
    /// <param name="atlas">The texture atlas to evaluate.</param>
    /// <returns>
    ///   <b>true</b> if there are any images or textures with the same name in the output area, or <b>false</b> if not.</returns>
    public bool HasExistingFiles(GorgonTextureAtlas atlas)
    {
        string directory = Path.GetDirectoryName(atlas.Textures[0].Texture.Name).FormatDirectory('/');

        if (!_fileSystem.DirectoryExists(directory))
        {
            return false;
        }

        foreach (GorgonTexture2DView texture in atlas.Textures)
        {
            if (_fileSystem.GetFile(texture.Texture.Name) is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Function to save the atlas data.</summary>
    /// <param name="sprites">The sprites associated with the image file.</param>
    /// <param name="atlas">The atlas data to save.</param>
    /// <param name="saveSprites"><b>true</b> to save the atlas sprites, <b>false</b> to only save the image.</param>
    public void SaveAtlas(IReadOnlyDictionary<IContentFile, GorgonSprite> sprites, GorgonTextureAtlas atlas, bool saveSprites)
    {
        IGorgonImage image = null;
        string directory = Path.GetDirectoryName(atlas.Textures[0].Texture.Name).FormatDirectory('/');
        Stream outStream = null;

        // Function to retrieve the new path for the sprite.
        string GetSpritePath(IContentFile imageFile)
        {
            string texturePath = imageFile.Path;
            string filename = Path.GetFileNameWithoutExtension(texturePath);
            string spritePath = directory + filename;

            if (_defaultSpriteCodec.FileExtensions.Count == 0)
            {
                return spritePath;
            }

            spritePath += "." + _defaultSpriteCodec.FileExtensions[0].Extension;

            return spritePath;
        }

        try
        {
            if (!_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            // Check the files to ensure they're not open for editing.
            foreach (GorgonTexture2DView texture in atlas.Textures)
            {
                IContentFile textureFile = _fileSystem.GetFile(texture.Texture.Name);

                if ((textureFile is not null) && (textureFile.IsOpen))
                {
                    throw new IOException(string.Format(Resources.GORIAG_ERR_IMAGE_OPEN, textureFile.Path));
                }
            }

            if (saveSprites)
            {
                foreach ((GorgonSprite original, GorgonSprite sprite) in atlas.Sprites)
                {
                    string spritePath = GetSpritePath(sprites.First(item => item.Value == original).Key);
                    IContentFile fileExists = _fileSystem.GetFile(spritePath);

                    if ((fileExists is not null) && (fileExists.IsOpen))
                    {
                        throw new IOException(string.Format(Resources.GORIAG_ERR_SPRITE_OPEN, fileExists.Path));
                    }
                }
            }

            // Write textures.
            foreach (GorgonTexture2DView texture in atlas.Textures)
            {
                image = texture.Texture.ToImage();
                outStream = _fileSystem.OpenStream(texture.Texture.Name, FileMode.Create);
                _defaultImageCodec.Save(image, outStream);
                outStream.Dispose();
                image.Dispose();
            }

            if (!saveSprites)
            {
                return;
            }

            // Write out the updated sprites.
            foreach ((GorgonSprite original, GorgonSprite sprite) in atlas.Sprites)
            {
                IContentFile textureFile = _fileSystem.GetFile(sprite.Texture.Texture.Name);
                string spritePath = GetSpritePath(sprites.First(item => item.Value == original).Key);
                IContentFile spriteFile = _fileSystem.GetFile(spritePath);

                if ((spriteFile is not null)
                    && (spriteFile.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> oldTexturePaths))
                    && (oldTexturePaths.Count > 0)
                    && (_fileSystem.FileExists(oldTexturePaths[0])))
                {
                    IContentFile oldTextureFile = _fileSystem.GetFile(oldTexturePaths[0]);
                    spriteFile.UnlinkContent(oldTextureFile);
                }

                outStream = _fileSystem.OpenStream(spritePath, FileMode.Create);
                _defaultSpriteCodec.Save(sprite, outStream);
                outStream.Dispose();

                spriteFile = _fileSystem.GetFile(spritePath);
                spriteFile.LinkContent(textureFile);

                textureFile.Refresh();
                spriteFile.Refresh();
            }
        }
        finally
        {
            outStream?.Dispose();
            image?.Dispose();
        }
    }

    #endregion
}
