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
// Created: June 7, 2020 9:42:02 PM
// 
#endregion

using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor.Services;

/// <summary>
/// A service used to manage IO for animations.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="AnimationIOService"/> class.</remarks>
/// <param name="fileManager">The content file manager.</param>
/// <param name="textureCache">The texture caching system.</param>
/// <param name="animCodec">The codec used to read animation data.</param>        
/// <param name="spriteCodec">The codec used to read sprite data.</param>
/// <param name="log">The logging interface for debug logging.</param>
internal class AnimationIOService(IContentFileManager fileManager, ITextureCache textureCache, IGorgonAnimationCodec animCodec, IGorgonSpriteCodec spriteCodec, IGorgonLog log)
{
    #region Constants.
    /// <summary>
    /// The name of the attributes that define the start position.
    /// </summary>
    public const string StartPositionAttrX = "SpriteStartPositionX";
    public const string StartPositionAttrY = "SpriteStartPositionY";
    #endregion

    #region Classes.
    /// <summary>
    /// The primary sprite dependency data.
    /// </summary>
    /// <remarks>Initializes a new instance of the <see cref="TextureDependencies"/> class.</remarks>
    /// <param name="sprite">The primary sprite.</param>
    /// <param name="spriteFile">The sprite file.</param>
    /// <param name="textureFile">The sprite texture file.</param>
    internal class PrimarySpriteDependency(GorgonSprite sprite, IContentFile spriteFile, IContentFile textureFile)
    {
        /// <summary>
        /// Property to return the primary sprite for the animation.
        /// </summary>
        public GorgonSprite PrimarySprite
        {
            get;
        } = sprite;

        /// <summary>
        /// Property to return the texture initially associated with the sprite.
        /// </summary>
        public GorgonTexture2DView SpriteTexture
        {
            get;
        } = sprite.Texture;

        /// <summary>
        /// Property to return the file containing the primary sprite.
        /// </summary>
        public IContentFile File
        {
            get;
        } = spriteFile;

        /// <summary>
        /// Property to return the file containing the texture for the primary sprite.
        /// </summary>
        public IContentFile TextureFile
        {
            get;
        } = textureFile;
    }

    /// <summary>
    /// The texture dependency data.
    /// </summary>
    /// <remarks>Initializes a new instance of the <see cref="TextureDependencies"/> class.</remarks>
    /// <param name="textures">The textures.</param>
    /// <param name="textureFiles">The texture files.</param>
    internal class TextureDependencies(IReadOnlyList<GorgonTexture2DView> textures, IReadOnlyList<IContentFile> textureFiles)
    {
        /// <summary>
        /// Property to return the textures associated with the animation.
        /// </summary>
        public IReadOnlyList<GorgonTexture2DView> Textures
        {
            get;
        } = textures;

        /// <summary>
        /// Property to return the files containing the textures.
        /// </summary>
        public IReadOnlyList<IContentFile> Files
        {
            get;
        } = textureFiles;
    }
    #endregion

    #region Variables.
    // The content file manager.
    private readonly IContentFileManager _fileManager = fileManager;
    // The image codec used to read image file data.
    private readonly ITextureCache _textureCache = textureCache;
    // The codec used to read sprite data.
    private readonly IGorgonSpriteCodec _spriteCodec = spriteCodec;
    // The codec used to read animation data.
    private readonly IGorgonAnimationCodec _animationCodec = animCodec;
    // The logging interface for debug logging.
    private readonly IGorgonLog _log = log;
    #endregion

    #region Methods.
    /// <summary>Function to load an associated sprite texture for sprite content.</summary>
    /// <param name="spriteContent">The sprite content file to use.</param>
    /// <returns>The texture associated with the sprite and the file containing the texture.</returns>
    private async Task<(GorgonTexture2DView texture, IContentFile textureFile)> LoadSpriteTextureAsync(IContentFile spriteContent)
    {
        _log.Print($"Loading sprite texture for '{spriteContent.Path}'...", LoggingLevel.Verbose);

        if ((!spriteContent.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> dependency))
                || (dependency.Count == 0))
        {
            _log.Print("WARNING: No sprite texture dependency found, interrogating sprite data...", LoggingLevel.Verbose);
            // If there's no linkage, then see if the sprite has the path information embedded within its data.
            using Stream spriteStream = _fileManager.OpenStream(spriteContent.Path, FileMode.Open);
            string textureName = _spriteCodec.GetAssociatedTextureName(spriteStream);

            if ((string.IsNullOrWhiteSpace(textureName))
                || (!_fileManager.FileExists(textureName)))
            {
                return (null, null);
            }

            dependency = [textureName];
        }

        IContentFile imageFile = _fileManager.GetFile(dependency[0]);

        if (imageFile is null)
        {
            return (null, null);
        }

        _log.Print($"Found sprite texture '{dependency[0]}'...", LoggingLevel.Verbose);

        if (!IsContentImage(imageFile))
        {
            _log.Print($"ERROR: '{dependency[0]}' not found in project or is not an image content file.", LoggingLevel.Simple);
            return (null, null);
        }

        // Retrieve the texture from the cache.
        GorgonTexture2DView texture = await _textureCache.GetTextureAsync(imageFile);
        imageFile.IsOpen = true;

        return texture is null ? (null, null) : (texture, imageFile);
    }

    /// <summary>
    /// Function to load any dependent primary sprite data for the animation.
    /// </summary>
    /// <param name="animationFile">The file containing the animation data.</param>
    /// <returns>The primary sprite and file associated with the animation, or <b>null</b> if the data ccould not be retrieved.</returns>
    private async Task<PrimarySpriteDependency> LoadPrimarySpriteDependency(IContentFile animationFile)
    {
        _log.Print($"Loading sprites for '{animationFile.Path}'...", LoggingLevel.Verbose);

        if ((!animationFile.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.SpriteType, out List<string> primaryPath))
            || (primaryPath is null)
            || (primaryPath.Count == 0)
            || (!_fileManager.FileExists(primaryPath[0])))
        {
            _log.Print($"WARNING: Primary sprite is missing for the animation. It will need to be reassigned.", LoggingLevel.Intermediate);
            return null;
        }

        IContentFile primarySpriteFile = _fileManager.GetFile(primaryPath[0]);

        Stream spriteStream = null;

        _log.Print($"Reading sprite data for '{primarySpriteFile.Path}'...", LoggingLevel.Verbose);
        try
        {
            if (!IsContentSprite(primarySpriteFile))
            {
                _log.Print($"ERROR: '{primarySpriteFile.Path}' not found in project or is not a sprite content file.", LoggingLevel.Simple);
                return null;
            }

            spriteStream = _fileManager.OpenStream(primarySpriteFile.Path, FileMode.Open);
            if (!_spriteCodec.IsReadable(spriteStream))
            {
                _log.Print($"ERROR: '{primarySpriteFile.Path}' is not a {_spriteCodec.Name} file.", LoggingLevel.Simple);
                return null;
            }

            GorgonTexture2DView texture = null;
            IContentFile spriteTextureFile = null;

            if ((primarySpriteFile.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> spriteTextures))
                && (spriteTextures.Count > 0))
            {
                string textureName = spriteTextures[0];

                if (!string.IsNullOrWhiteSpace(textureName))
                {
                    spriteTextureFile = _fileManager.GetFile(textureName);

                    if (spriteTextureFile is not null)
                    {
                        texture = await _textureCache.GetTextureAsync(spriteTextureFile);
                        spriteTextureFile.IsOpen = true;
                    }
                }
            }

            GorgonSprite primarySprite = _spriteCodec.FromStream(spriteStream, texture);
            primarySpriteFile.IsOpen = true;

            _log.Print($"Sprite '{primarySpriteFile.Path}' found and loaded.", LoggingLevel.Verbose);

            return new PrimarySpriteDependency(primarySprite, primarySpriteFile, spriteTextureFile);
        }
        catch (Exception ex)
        {
            _log.Print($"ERROR: There was an error loading the sprite '{primarySpriteFile.Path}'.", LoggingLevel.Simple);
            _log.LogException(ex);

            return null;
        }
        finally
        {
            spriteStream?.Dispose();
        }
    }

    /// <summary>
    /// Function to load any dependent texture data for the animation.
    /// </summary>
    /// <param name="animationFile">The file containing the animation data.</param>
    /// <returns>The textures and files associated with the animation, or <b>null</b> if the data ccould not be retrieved.</returns>
    private async Task<TextureDependencies> LoadTexturesAsync(IContentFile animationFile)
    {
        _log.Print($"Loading textures for '{animationFile.Path}'...", LoggingLevel.Verbose);

        if ((!animationFile.Metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> dependencies))
            || (dependencies.Count == 0))
        {
            _log.Print("WARNING: No sprite texture dependencies found, interrogating animation data...", LoggingLevel.Intermediate);
            using Stream stream = _fileManager.OpenStream(animationFile.Path, FileMode.Open);
            dependencies = new List<string>(_animationCodec.GetAssociatedTextureNames(stream));

            if (dependencies.Count == 0)
            {
                _log.Print($"WARNING: No textures for the animation for were found on the file system.", LoggingLevel.Intermediate);
                return new TextureDependencies([], []);
            }
        }

        // Check for duplicate texture paths (because of my original stupidity).
        if (dependencies.GroupBy(g => g, StringComparer.OrdinalIgnoreCase).Any(g => g.Skip(1).Any()))
        {
            dependencies = dependencies.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        var files = new List<IContentFile>(dependencies.Where(item => (!string.IsNullOrWhiteSpace(item)) && (_fileManager.FileExists(item)))
                                                       .Select(item => _fileManager.GetFile(item)));

        if (files.Count == 0)
        {
            _log.Print($"WARNING: No textures for the animation for were found on the file system.", LoggingLevel.Intermediate);
            return new TextureDependencies([], []);
        }

        var textures = new List<(GorgonTexture2DView texture, IContentFile file)>();

        foreach (IContentFile file in files)
        {
            GorgonTexture2DView texture = await _textureCache.GetTextureAsync(file);
            file.IsOpen = true;

            if (texture is null)
            {
                if (file is not null)
                {
                    _log.Print($"WARNING: The texture '{file.Path}' was not loaded and will not be available when rendering the animation.", LoggingLevel.Intermediate);
                }
                continue;
            }

            textures.Add((texture, file));
        }

        _log.Print($"{textures.Count} textures out of {files.Count} loaded.", LoggingLevel.Verbose);

        return new TextureDependencies(textures.Select(item => item.texture).ToArray(), textures.Select(item => item.file).ToArray());
    }

    /// <summary>
    /// Function to determine if the content file passed in is image content as supported by this editor.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <returns><b>true</b> if the file is an image, supported by this editor, or <b>false</b> if not.</returns>
    public bool IsContentImage(IContentFile file) => ((file is not null)
            && (file.Metadata is not null)
            && (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
            && (string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Function to determine if the content file passed in is sprite content as supported by this editor.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <returns><b>true</b> if the file is a sprite, supported by this editor, or <b>false</b> if not.</returns>
    public bool IsContentSprite(IContentFile file) => ((file is not null)
            && (file.Metadata is not null)
            && (file.Metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
            && (string.Equals(contentType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Function to load an associated textures and sprites for animation content.</summary>
    /// <param name="animationFile">The animation content file to use.</param>
    /// <returns>The textures and sprites associated with the animation.</returns>
    public async Task<(TextureDependencies, PrimarySpriteDependency)> LoadDependenciesAsync(IContentFile animationFile)
    {
        Debug.Assert(animationFile.Metadata is not null, "No meta data for animation content!");

        TextureDependencies textures = await LoadTexturesAsync(animationFile);
        PrimarySpriteDependency sprite = await LoadPrimarySpriteDependency(animationFile);

        return (textures, sprite);
    }

    /// <summary>
    /// Function to load a sprite for the animation.
    /// </summary>
    /// <param name="file">The file containing the sprite data.</param>
    /// <returns>The primary sprite and its texture file.</returns>
    public async Task<(GorgonSprite sprite, IContentFile textureFile)> LoadSpriteAsync(IContentFile file)
    {
        if (file is null)
        {
            return (null, null);
        }

        _log.Print($"Loading sprite '{file.Path}'...", LoggingLevel.Verbose);
        (GorgonTexture2DView texture, IContentFile textureFile) = await LoadSpriteTextureAsync(file);

        (GorgonSprite, IContentFile) result = await Task.Run(() =>
        {
            using Stream stream = _fileManager.OpenStream(file.Path, FileMode.Open);
            return (_spriteCodec.FromStream(stream, texture), textureFile);
        });

        return result;
    }

    /// <summary>
    /// Function to unload a sprite and its texture.
    /// </summary>
    /// <param name="spriteInfo">A tuple containing the sprite, the file containing the sprite data, and the file containing the sprite texture.</param>
    public void UnloadSprite((GorgonSprite sprite, IContentFile spriteFile, IContentFile textureFile) spriteInfo)
    {
        bool unloaded = _textureCache.ReturnTexture(spriteInfo.sprite?.Texture);

        if (spriteInfo.spriteFile is not null)
        {
            spriteInfo.spriteFile.IsOpen = false;
        }

        if (!unloaded)
        {
            return;
        }

        spriteInfo.textureFile.IsOpen = false;
    }

    /// <summary>
    /// Function to save an animation to the file system.
    /// </summary>
    /// <param name="path">The path to the file to write to.</param>
    /// <param name="animation">The animation to save.</param>
    /// <param name="backgroundImage">The file containing the background image.</param>
    /// <param name="startPos">The starting position for the source sprite.</param>
    /// <param name="primarySpriteFile">The file containing the primary sprite.</param>
    /// <param name="textureFiles">The texture files used by the keyframes in the animation.</param>
    /// <param name="excluded">The list of tracks that are excluded from the animation.</param>
    /// <returns>The file containing the animation.</returns>
    public async Task<IContentFile> SaveAnimation(string path, IGorgonAnimation animation, IContentFile backgroundImage, Vector2 startPos, IContentFile primarySpriteFile, IReadOnlyList<IContentFile> textureFiles, IReadOnlyList<ITrack> excluded)
    {
        Stream stream = null;
        IContentFile animFile = null;

        void SaveTask()
        {
            IEnumerable<IGorgonAnimationTrack<IGorgonKeyFrame>> excludedTracks = animation.SingleTracks
                                                                              .Select(item => item.Value)
                                                                              .Cast<IGorgonAnimationTrack<IGorgonKeyFrame>>()
                                                                              .Concat(animation.Vector2Tracks.Select(item => item.Value))
                                                                              .Concat(animation.Vector3Tracks.Select(item => item.Value))
                                                                              .Concat(animation.Vector4Tracks.Select(item => item.Value))
                                                                              .Concat(animation.ColorTracks.Select(item => item.Value))
                                                                              .Concat(animation.RectangleTracks.Select(item => item.Value))
                                                                              .Concat(animation.Texture2DTracks.Select(item => item.Value))
                                                                              .Where(item => excluded.Any(track => string.Equals(track.Name, item.Name, StringComparison.OrdinalIgnoreCase)));

            // Re-enable tracks that may have been disabled due to lack of support.
            foreach (IGorgonAnimationTrack<IGorgonKeyFrame> track in excludedTracks)
            {
                track.IsEnabled = true;
            }

            _animationCodec.Save(animation, stream);

            // Turn them off again.
            foreach (IGorgonAnimationTrack<IGorgonKeyFrame> track in excludedTracks)
            {
                track.IsEnabled = false;
            }
        }

        try
        {
            animFile = _fileManager.GetFile(path);

            if (animFile is not null)
            {
                animFile.IsOpen = false;
                animFile.ClearLinks();
            }

            stream = _fileManager.OpenStream(path, FileMode.Create);
            await Task.Run(SaveTask);
            stream?.Dispose();

            animFile = _fileManager.GetFile(path);

            if (animFile is null)
            {
                throw new FileNotFoundException();
            }

            // Setup metadata.
            animFile.IsOpen = true;
            if (backgroundImage is not null)
            {
                animFile.Metadata.DependsOn[AnimationEditorPlugIn.BgImageDependencyName] = [backgroundImage.Path];
            }

            animFile.LinkContent(primarySpriteFile);

            if ((textureFiles is not null) && (textureFiles.Count > 0))
            {

                var files = new List<string>();

                for (int i = 0; i < textureFiles.Count; ++i)
                {
                    IContentFile file = textureFiles[i];
                    files.Add(file?.Path);
                }

                animFile.Metadata.DependsOn[CommonEditorContentTypes.ImageType] = files;
            }

            animFile.Metadata.Attributes[StartPositionAttrX] = startPos.X.ToString("0.0#####", CultureInfo.InvariantCulture);
            animFile.Metadata.Attributes[StartPositionAttrY] = startPos.Y.ToString("0.0#####", CultureInfo.InvariantCulture);
            animFile.Metadata.Attributes.Remove(CommonEditorConstants.IsNewAttr);

            // Persist the metadata.
            _fileManager.FlushMetadata();
            animFile.Refresh();
        }
        finally
        {
            stream?.Dispose();
            if (animFile is not null)
            {
                animFile.IsOpen = true;
            }
        }

        return animFile;
    }

    /// <summary>
    /// Function to load the background texture for the animation.
    /// </summary>
    /// <param name="file">The file containing the background texture.</param>
    /// <returns>A tuple containing the texture, and its file.</returns>
    public async Task<(GorgonTexture2DView texture, IContentFile file)> LoadBackgroundTextureAsync(IContentFile file)
    {
        if (file is null)
        {
            return (null, null);
        }

        GorgonTexture2DView result = await _textureCache.GetTextureAsync(file);
        file.IsOpen = true;
        return (result, file);
    }

    /// <summary>
    /// Function to unload the background texture.
    /// </summary>
    /// <param name="backgroundTexture">The background texture to unload.</param>
    public void UnloadBackgroundTexture((GorgonTexture2DView texture, IContentFile file) backgroundTexture)
    {
        bool unloaded = false;

        if (backgroundTexture.texture is not null)
        {
            unloaded = _textureCache.ReturnTexture(backgroundTexture.texture);
        }

        if ((unloaded) && (backgroundTexture.file is not null))
        {
            backgroundTexture.file.IsOpen = false;
        }
    }

    #endregion
}
