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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Editor.Content;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.Renderers.Services;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The service used for file operations.
/// </summary>
internal class FileIOService
{
    #region Variables.
    // The file system for content files.
    private readonly IContentFileManager _fileSystem;
    // The default image codec.
    private readonly IGorgonImageCodec _defaultImageCodec;
    // The default sprite codec.
    private readonly IGorgonSpriteCodec _defaultSpriteCodec;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to load the texture for the sprite.
    /// </summary>
    /// <param name="textureFile">The file for the texture.</param>
    /// <returns>The sprite texture.</returns>
    private GorgonTexture2DView LoadSpriteTexture(IContentFile textureFile)
    {
        if (textureFile is null)
        {
            return null;
        }

        using Stream stream = _fileSystem.OpenStream(textureFile.Path, FileMode.Open);
        return GorgonTexture2DView.FromStream(_defaultSpriteCodec.Renderer.Graphics, stream, _defaultImageCodec, options: new GorgonTexture2DLoadOptions
        {
            Binding = TextureBinding.ShaderResource,
            Usage = ResourceUsage.Immutable,
            Name = textureFile.Path,
            IsTextureCube = false
        });
    }

    /// <summary>Function to load the sprites used to generate the atlas texture(s).</summary>
    /// <param name="files">The sprite files to load.</param>
    /// <returns>A list of sprites and their associated files.</returns>
    public IReadOnlyDictionary<IContentFile, GorgonSprite> LoadSprites(IEnumerable<IContentFile> files)
    {
        var result = new Dictionary<IContentFile, GorgonSprite>();
        Stream stream = null;
        var textures = new Dictionary<string, GorgonTexture2DView>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (IContentFile file in files)
            {
                GorgonTexture2DView texture = null;

                if (file.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> texturePaths))
                {
                    if (texturePaths.Count == 0)
                    {
                        continue;
                    }

                    if (!textures.TryGetValue(texturePaths[0], out texture))
                    {
                        texture = LoadSpriteTexture(_fileSystem.GetFile(texturePaths[0]));
                        textures[texturePaths[0]] = texture;
                    }
                }

                stream = _fileSystem.OpenStream(file.Path, FileMode.Open);
                result[file] = _defaultSpriteCodec.FromStream(stream, texture);
                stream.Dispose();
            }
        }
        catch
        {
            // If we loaded in a bunch of textures already, then dump them since we can't do it outside of here.
            foreach (KeyValuePair<IContentFile, GorgonSprite> sprite in result)
            {
                sprite.Value.Texture?.Dispose();
            }

            throw;
        }
        finally
        {
            stream?.Dispose();
        }

        return result;
    }

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

    /// <summary>Function to determine if there are any existing files that match the texture names or sprite names.</summary>
    /// <param name="atlas">The texture atlas to evaluate.</param>
    /// <returns>
    ///   <b>true</b> if there are any sprites or textures with the same name in the output area, or <b>false</b> if not.</returns>
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
    /// <param name="atlas">The atlas data to save.</param>
    public void SaveAtlas(IReadOnlyDictionary<IContentFile, GorgonSprite> spriteFiles, GorgonTextureAtlas atlas)
    {
        IGorgonImage image = null;
        string directory = Path.GetDirectoryName(atlas.Textures[0].Texture.Name).FormatDirectory('/');
        Stream outStream = null;

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
                    throw new IOException(string.Format(Resources.GORTAG_ERR_IMAGE_OPEN, textureFile.Path));
                }
            }

            foreach ((GorgonSprite original, GorgonSprite sprite) in atlas.Sprites)
            {
                IContentFile oldFile = spriteFiles.FirstOrDefault(item => item.Value == original).Key;

                if ((oldFile is not null) && (oldFile.IsOpen))
                {
                    throw new IOException(string.Format(Resources.GORTAG_ERR_IMAGE_OPEN, oldFile.Path));
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

            // Write out the updated sprites.
            foreach ((GorgonSprite original, GorgonSprite sprite) in atlas.Sprites)
            {
                IContentFile textureFile = _fileSystem.GetFile(sprite.Texture.Texture.Name);
                IContentFile spriteFile = spriteFiles.FirstOrDefault(item => item.Value == original).Key;

                if ((spriteFile.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> oldTexturePaths))
                    && (oldTexturePaths.Count > 0) 
                    && (_fileSystem.FileExists(oldTexturePaths[0])))
                {
                    IContentFile oldTextureFile = _fileSystem.GetFile(oldTexturePaths[0]);
                    spriteFile.UnlinkContent(oldTextureFile);
                }                                       
                
                outStream = _fileSystem.OpenStream(spriteFile.Path, FileMode.Create);
                _defaultSpriteCodec.Save(sprite, outStream);
                outStream.Dispose();

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

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FileIOService"/> class.</summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="imageCodec">The default image codec to use.</param>
    /// <param name="spriteCodec">The default sprite codec to use.</param>
    public FileIOService(IContentFileManager fileSystem, IGorgonImageCodec imageCodec, IGorgonSpriteCodec spriteCodec)
    {
        _fileSystem = fileSystem;
        _defaultImageCodec = imageCodec;
        _defaultSpriteCodec = spriteCodec;
    }
    #endregion
}
