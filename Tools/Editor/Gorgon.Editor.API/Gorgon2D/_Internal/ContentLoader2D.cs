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
// Created: August 2, 2020 1:12:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;

namespace Gorgon.IO
{
    /// <summary>
    /// Loads <see cref="Gorgon2D"/> specific content from an editor file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides a set of convenience methods to load <see cref="Gorgon2D"/> content such as sprites from an editor file system.  
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// These methods load the data using the layout and metadata information as provided by the default plug ins for the editor.  Custom plug ins for sprite data, etc... may not work with these methods 
    /// unless those plug ins follow the same file layout as the default plug ins.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    internal class ContentLoader2D
        : IGorgonContentLoader
    {
        #region Variables.
        // The renderer used to handle loading.
        private readonly Gorgon2D _renderer;
        // The graphics interface used to handle loading textures.
        private readonly GorgonGraphics _graphics;
        // The file system containing the content data.
        private readonly IGorgonFileSystem _fileSystem;
        // The metadata from the editor.
        private readonly IProjectMetadata _metadata;
        #endregion

        #region Properties.
        /// <summary>Property to return a list of codecs that can be used to load animation content data.</summary>
        /// <remarks>Codecs added here are for external codecs. All built-in codecs for Gorgon will not appear in this list and are always used when loading files.</remarks>
        public IList<IGorgonAnimationCodec> ExternalAnimationCodecs
        {
            get;
        } = new List<IGorgonAnimationCodec>();

        /// <summary>Property to return a list of codecs that can be used to load image content data.</summary>
        /// <remarks>Codecs added here are for external codecs. All built-in codecs for Gorgon will not appear in this list and are always used when loading files.</remarks>
        public IList<IGorgonImageCodec> ExternalImageCodecs
        {
            get;
        } = new List<IGorgonImageCodec>();

        /// <summary>Property to return a list of codecs that can be used to load sprite content data.</summary>
        /// <remarks>Codecs added here are for external codecs. All built-in codecs for Gorgon will not appear in this list and are always used when loading files.</remarks>
        public IList<IGorgonSpriteCodec> ExternalSpriteCodecs
        {
            get;
        } = new List<IGorgonSpriteCodec>();

        /// <summary>Property to return the texture cache for the loader.</summary>
        public GorgonTextureCache<GorgonTexture2D> TextureCache
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the list of available codecs for loading animation data.
        /// </summary>
        /// <returns>A list of animation codecs by type name.</returns>
        private IReadOnlyDictionary<string, IGorgonAnimationCodec> GetAnimationCodecs()
        {
            var result = new Dictionary<string, IGorgonAnimationCodec>(StringComparer.OrdinalIgnoreCase)
            {
                { typeof(GorgonV31AnimationBinaryCodec).FullName, new GorgonV31AnimationBinaryCodec(_renderer) },
                { typeof(GorgonV31AnimationJsonCodec).FullName, new GorgonV31AnimationJsonCodec(_renderer) }
            };

            foreach (IGorgonAnimationCodec codec in ExternalAnimationCodecs)
            {
                string typeName = codec.GetType().FullName;
                result[typeName] = codec;
            }

            return result;
        }

        /// <summary>
        /// Function to retrieve the list of available codecs for loading sprite data.
        /// </summary>
        /// <returns>A list of sprite codecs by type name.</returns>
        private IReadOnlyDictionary<string, IGorgonSpriteCodec> GetSpriteCodecs()
        {
            var result = new Dictionary<string, IGorgonSpriteCodec>(StringComparer.OrdinalIgnoreCase)
            {
                { typeof(GorgonV3SpriteBinaryCodec).FullName, new GorgonV3SpriteBinaryCodec(_renderer) },
                { typeof(GorgonV3SpriteJsonCodec).FullName, new GorgonV3SpriteJsonCodec(_renderer) },
                { typeof(GorgonV2SpriteCodec).FullName, new GorgonV2SpriteCodec(_renderer) },
                { typeof(GorgonV1SpriteBinaryCodec).FullName, new GorgonV1SpriteBinaryCodec(_renderer) }
            };
            
            foreach (IGorgonSpriteCodec codec in ExternalSpriteCodecs)
            {
                string typeName = codec.GetType().FullName;
                result[typeName] = codec;
            }

            return result;
        }

        /// <summary>
        /// Function to retrieve the list of available codecs for loading image data.
        /// </summary>
        /// <returns>A list of image codecs by type name.</returns>
        private IReadOnlyDictionary<string, IGorgonImageCodec> GetImageCodecs()
        {
            var result = new Dictionary<string, IGorgonImageCodec>(StringComparer.OrdinalIgnoreCase)
            {
                { typeof(GorgonCodecDds).FullName, new GorgonCodecDds() },
                { typeof(GorgonCodecPng).FullName, new GorgonCodecPng() },
                { typeof(GorgonCodecTga).FullName, new GorgonCodecTga() },
                { typeof(GorgonCodecGif).FullName, new GorgonCodecGif() },
                { typeof(GorgonCodecJpeg).FullName, new GorgonCodecJpeg() },
                { typeof(GorgonCodecBmp).FullName, new GorgonCodecBmp() }
            };

            foreach (IGorgonImageCodec codec in ExternalImageCodecs)
            {
                string typeName = codec.GetType().FullName;
                result[typeName] = codec;
            }

            return result;
        }

        /// <summary>
        /// Function to load a texture from the file system.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>A new texture object.</returns>
        private async Task<GorgonTexture2D> ReadTextureAsync(string path)
        {
            IGorgonVirtualFile file = _fileSystem.GetFile(path);

            if ((file == null) || (!_metadata.ProjectItems.TryGetValue(path, out ProjectItemMetadata metadata)))
            {
                _graphics.Log.Print($"WARNING: The texture '{path}' was not found in the editor file system.", LoggingLevel.Intermediate);
                return null;
            }


            if (!metadata.Attributes.TryGetValue("ImageCodec", out string codecName))
            {
                _graphics.Log.Print($"WARNING: The codec for the texture '{path}' is empty or not found.", LoggingLevel.Intermediate);
                return null;
            }

            IReadOnlyDictionary<string, IGorgonImageCodec> imageCodecs = GetImageCodecs();

            if (!imageCodecs.TryGetValue(codecName, out IGorgonImageCodec codec))
            {
                _graphics.Log.Print($"WARNING: The image codec '{codecName}' is unknown.", LoggingLevel.Intermediate);
                return null;
            }

            IGorgonImage image = await Task.Run(() =>
            {
                using (Stream stream = file.OpenStream())
                {
                    return codec.LoadFromStream(stream);
                }
            });

            using (image)
            {
                return image.ToTexture2D(_graphics, new GorgonTexture2DLoadOptions
                {
                    Name = path,
                    Binding = TextureBinding.ShaderResource,
                    IsTextureCube = image.ImageType == ImageType.ImageCube,
                    Usage = ResourceUsage.Immutable
                });
            }
        }

        /// <summary>
        /// Function to load an animation from the editor file system.
        /// </summary>
        /// <param name="path">The path to the animation content.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/> containing the animation data from the file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="IGorgonAnimation"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// If the animation is not in a format known by Gorgon, then users should add the <see cref="IGorgonAnimationCodec"/> for reading the sprite data to the <see cref="ExternalAnimationCodecs"/> list. 
        /// Doing this will allow a user to create a custom image codec plug in and use that to read animation data.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The animation file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.AnimationType"/>, and an attribute called <c>AnimationCodec</c> 
        /// with the fully qualified type name of the animation codec as its value or the animation will not load.
        /// </para>
        /// </para>
        /// </remarks>
        public async Task<IGorgonAnimation> LoadAnimationAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IReadOnlyDictionary<string, IGorgonAnimationCodec> animCodecs = GetAnimationCodecs();

            IGorgonVirtualFile animationFile = _fileSystem.GetFile(path);

            if ((!_metadata.ProjectItems.TryGetValue(path, out ProjectItemMetadata metadata))
                || (animationFile == null))
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            if ((!metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.AnimationType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NOT_ANIMATION, path));
            }

            if (!metadata.Attributes.TryGetValue("AnimationCodec", out string codecTypeName))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, string.Empty));
            }

            if (!animCodecs.TryGetValue(codecTypeName, out IGorgonAnimationCodec animationCodec))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, codecTypeName));
            }

            if ((metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> paths)) 
                && (paths != null)
                && (paths.Count > 0))
            {
                IEnumerable<Task> dependencyTasks = paths.Select(item => TextureCache.GetTextureAsync(item, ReadTextureAsync));
                await Task.WhenAll(dependencyTasks);
            }

            using (Stream stream = animationFile.OpenStream())
            {
                return animationCodec.FromStream(stream);
            }
        }

        /// <summary>
        /// Function to load an image from the editor file system.
        /// </summary>
        /// <param name="path">The path to the image content.</param>
        /// <returns>A new <see cref="IGorgonImage"/> containing the image data from the file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="IGorgonImage"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// If the image is not in a format known by Gorgon, then users should add the <see cref="IGorgonImageCodec"/> for reading the sprite data to the <see cref="ExternalImageCodecs"/> list. 
        /// Doing this will allow a user to create a custom image codec plug in and use that to read image data.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The texture file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.ImageType"/>, and an attribute called <c>ImageCodec</c> 
        /// with the fully qualified type name of the image codec as its value or the texure will not load.
        /// </para>
        /// <para>
        /// If image file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is read from the 
        /// file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// </remarks>        
        public IGorgonImage LoadImage(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IProjectMetadata metaData = _fileSystem.GetMetadata();
            IGorgonVirtualFile file = _fileSystem.GetFile(path);

            if ((file == null) || (!metaData.ProjectItems.TryGetValue(path, out ProjectItemMetadata fileMetadata)))
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            IReadOnlyDictionary<string, IGorgonImageCodec> supportedImageCodecs = GetImageCodecs();

            if ((!fileMetadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NOT_IMAGE, path));
            }

            if (!fileMetadata.Attributes.TryGetValue("ImageCodec", out string codecTypeName))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, string.Empty));
            }

            if (!supportedImageCodecs.TryGetValue(codecTypeName, out IGorgonImageCodec imageCodec))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, codecTypeName));
            }

            using (Stream stream = file.OpenStream())
            {
                return imageCodec.LoadFromStream(stream, (int)file.Size);
            }
        }

        /// <summary>
        /// Function to load an image as a texture from the editor file system.
        /// </summary>
        /// <param name="path">The path to the image context.</param>
        /// <param name="cache">[Optional] <b>true</b> to use the texture cache, <b>false</b> to load a new instance.</param>
        /// <returns>A new <see cref="GorgonTexture2D"/> containing the image data from the file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="GorgonTexture2D"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// If the texture image is not in a format known by Gorgon, then users should add the <see cref="IGorgonImageCodec"/> for reading the sprite data to the <see cref="ExternalImageCodecs"/> list. 
        /// Doing this will allow a user to create a custom image codec plug in and use that to read image data.
        /// </para>
        /// <para>
        /// If the <paramref name="cache"/> parameter is set to <b>true</b>, then this method will load the data from the <see cref="TextureCache"/>. If the texture data is not in the cache, then it will 
        /// be added to the cache and returned. If the parameter is set to <b>false</b>, then the image data will be loaded as a new texture outside of the cache and it will be the responsibility of the 
        /// developer to manage its lifetime.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The texture file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.ImageType"/>, and an attribute called <c>ImageCodec</c> 
        /// with the fully qualified type name of the image codec as its value or the texure will not load.
        /// </para>
        /// <para>
        /// If image file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is read from the 
        /// file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// </remarks>        
        public async Task<GorgonTexture2D> LoadTextureAsync(string path, bool cache = true)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IProjectMetadata metaData = _fileSystem.GetMetadata();
            IGorgonVirtualFile file = _fileSystem.GetFile(path);

            if ((file == null) || (!metaData.ProjectItems.TryGetValue(path, out ProjectItemMetadata fileMetadata)))
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            IReadOnlyDictionary<string, IGorgonImageCodec> supportedImageCodecs = GetImageCodecs();

            if ((!fileMetadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NOT_IMAGE, path));
            }

            if (!fileMetadata.Attributes.TryGetValue("ImageCodec", out string codecTypeName))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, string.Empty));
            }

            if (!supportedImageCodecs.TryGetValue(codecTypeName, out IGorgonImageCodec imageCodec))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, codecTypeName));
            }

            if (cache)
            {
                return await TextureCache.GetTextureAsync(path, ReadTextureAsync);
            }

            IGorgonImage image = await Task.Run(() => LoadImage(path));

            using (image)
            {
                return image.ToTexture2D(_graphics, new GorgonTexture2DLoadOptions
                {
                    Name = path,
                    Binding = TextureBinding.ShaderResource,
                    IsTextureCube = image.ImageType == ImageType.ImageCube,
                    Usage = ResourceUsage.Immutable
                });
            }
        }

        /// <summary>
        /// Function to load sprite content from the editor file system.
        /// </summary>
        /// <param name="path">The path to the sprite to load.</param>
        /// <param name="overrideTexture">[Optional] When supplied, this will override the associated texture for the sprite.</param>
        /// <returns>The <see cref="GorgonSprite"/> content for the file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the <paramref name="path"/> does not point to an existing file.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="GorgonSprite"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// Providing the <paramref name="overrideTexture"/> will skip the texture loading and use the texture passed in.  In this case, the texture return value will be <b>null</b> as it is assumed the 
        /// user already knows about the texture resource and is managing the lifetime of the texture elsewhere.
        /// </para>
        /// <para>
        /// If the sprite is not in a format known by Gorgon, then users should add the <see cref="IGorgonSpriteCodec"/> for reading the sprite data to the <see cref="ExternalSpriteCodecs"/> list. 
        /// Doing this will allow a user to create a custom sprite codec plug in and use that to read sprite data.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The sprite file metadata must have the following attributes: <c>Type</c> with the value set to the <see cref="CommonEditorContentTypes.SpriteType"/> constant, and an attribute called <c>SpriteCodec</c>
        /// with the fully qualified type name of the sprite codec as its value or the sprite will not load. 
        /// </para>
        /// <para>
        /// The associated texture file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.ImageType"/>, and an attribute called <c>ImageCodec</c> 
        /// with the fully qualified type name of the image codec as its value or the texure will not load.
        /// </para>
        /// <para>
        /// If the associated texture file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is 
        /// read from the file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// <b>Regarding sprite textures:</b> Gorgon will use the <see cref="GorgonTextureCache{GorgonTexture2D}"/> object to cache any textures loaded from the file system. This cache is exposed to the user 
        /// via the <see cref="TextureCache"/> property. It is up to the developer to ensure the cache is managed correctly for their applications. 
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public async Task<GorgonSprite> LoadSpriteAsync(string path, GorgonTexture2DView overrideTexture = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IReadOnlyDictionary<string, IGorgonSpriteCodec> spriteCodecs = GetSpriteCodecs();

            IGorgonVirtualFile spriteFile = _fileSystem.GetFile(path);

            if ((!_metadata.ProjectItems.TryGetValue(path, out ProjectItemMetadata metadata))
                || (spriteFile == null))
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            if ((!metadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NOT_SPRITE, path));
            }

            if (!metadata.Attributes.TryGetValue("SpriteCodec", out string codecTypeName))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, string.Empty));
            }

            if (!spriteCodecs.TryGetValue(codecTypeName, out IGorgonSpriteCodec spriteCodec))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, codecTypeName));
            }

            // If we've not provided an override, then load the texture dependencies.
            if ((overrideTexture == null)
                && (metadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> paths))
                && (paths != null)
                && (paths.Count > 0))
            {
                IEnumerable<Task<GorgonTexture2D>> dependencies = paths.Select(item => TextureCache.GetTextureAsync(item, ReadTextureAsync));
                await Task.WhenAll(dependencies);
                overrideTexture = dependencies.FirstOrDefault(item => string.Equals(item.Result?.Name, paths[0], StringComparison.OrdinalIgnoreCase))?.Result?.GetShaderResourceView();
            }

            using (Stream stream = spriteFile.OpenStream())
            {
                return spriteCodec.FromStream(stream, overrideTexture);
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ContentLoader2D"/> class.</summary>
        /// <param name="fileSystem">The file system containing the content.</param>
        /// <param name="metadata">The metadata from the editor, containing dependencies.</param>
        /// <param name="renderer">The renderer.</param>
        /// <param name="textureCache">The texture cache.</param>
        public ContentLoader2D(IGorgonFileSystem fileSystem, IProjectMetadata metadata, Gorgon2D renderer, GorgonTextureCache<GorgonTexture2D> textureCache)
        {
            _fileSystem = fileSystem;
            _metadata = metadata;            
            _renderer = renderer;
            _graphics = renderer.Graphics;
            TextureCache = textureCache;
        }        
        #endregion
    }
}
