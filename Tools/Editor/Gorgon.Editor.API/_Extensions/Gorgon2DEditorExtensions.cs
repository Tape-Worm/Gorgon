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
// Created: March 27, 2019 10:41:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
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
    /// Extension methods to load editor content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A set of convenience methods to load data from an editor file system.  
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
    public static class Gorgon2DEditorExtensions
    {
        #region Obsolete.
        /// <summary>
        /// Function to retrieve the list of available codecs for loading sprite data.
        /// </summary>
        /// <param name="renderer">The 2D renderer being used to handle the sprite data.</param>
        /// <param name="spriteCodecs">The list of extra codecs supplied by the user.</param>
        /// <returns>A list of sprite codecs by type name.</returns>
        private static IReadOnlyDictionary<string, IGorgonSpriteCodec> GetSpriteCodecs(Gorgon2D renderer, IReadOnlyList<IGorgonSpriteCodec> spriteCodecs)
        {
            var result = new Dictionary<string, IGorgonSpriteCodec>(StringComparer.OrdinalIgnoreCase)
            {
                { typeof(GorgonV3SpriteBinaryCodec).FullName, new GorgonV3SpriteBinaryCodec(renderer) },
                { typeof(GorgonV3SpriteJsonCodec).FullName, new GorgonV3SpriteJsonCodec(renderer) },
                { typeof(GorgonV2SpriteCodec).FullName, new GorgonV2SpriteCodec(renderer) },
                { typeof(GorgonV1SpriteBinaryCodec).FullName, new GorgonV1SpriteBinaryCodec(renderer) }
            };

            if (spriteCodecs is not null)
            {
                foreach (IGorgonSpriteCodec codec in spriteCodecs)
                {
                    string typeName = codec.GetType().FullName;

                    if (result.ContainsKey(typeName))
                    {
                        continue;
                    }

                    result.Add(typeName, codec);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to retrieve the list of available codecs for loading image data.
        /// </summary>
        /// <param name="imageCodecs">The list of extra codecs supplied by the user.</param>
        /// <returns>A list of image codecs by type name.</returns>
        private static IReadOnlyDictionary<string, IGorgonImageCodec> GetImageCodecs(IReadOnlyList<IGorgonImageCodec> imageCodecs)
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

            if (imageCodecs is not null)
            {
                foreach (IGorgonImageCodec codec in imageCodecs)
                {
                    string typeName = codec.GetType().FullName;

                    if (result.ContainsKey(typeName))
                    {
                        continue;
                    }

                    result.Add(typeName, codec);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to load the associated sprite texture based on the project metadata.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when looking up or creating the texture.</param>
        /// <param name="fileSystem">The file system containing the file to load.</param>
        /// <param name="metadata">The metadata for the project.</param>
        /// <param name="imagePath">The path to the image.</param>
        /// <param name="usage">The intended usage for the texture.</param>
        /// <param name="codecs">The list of available codecs.</param>
        /// <returns>A new texture containing the sprite texture data.</returns>
        private static GorgonTexture2D GetTexture(GorgonGraphics graphics, IGorgonFileSystem fileSystem, IProjectMetadata metadata, string imagePath, ResourceUsage usage, IReadOnlyDictionary<string, IGorgonImageCodec> codecs)
        {
            bool shouldConvertToPremultiplied = false;

            // First, check to see if this texture isn't already loaded into memory.
            GorgonTexture2D texture = graphics.LocateResourcesByName<GorgonTexture2D>(imagePath).FirstOrDefault();

            if (texture is not null)
            {
                return texture;
            }

            if (!metadata.ProjectItems.TryGetValue(imagePath, out ProjectItemMetadata textureMetadata))
            {
                return null;
            }

            if (textureMetadata.Attributes.TryGetValue("PremultipliedAlpha", out string isPremultiplied))
            {
                bool.TryParse(isPremultiplied, out shouldConvertToPremultiplied);
            }

            if ((!textureMetadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase))
                || (!textureMetadata.Attributes.TryGetValue("ImageCodec", out string imageCodecTypeName))
                || (!codecs.TryGetValue(imageCodecTypeName, out IGorgonImageCodec codec)))
            {
                return null;
            }

            IGorgonVirtualFile file = fileSystem.GetFile(imagePath);

            if (file is null)
            {
                return null;
            }

            using (Stream fileStream = file.OpenStream())
            {
                texture = GorgonTexture2D.FromStream(graphics, fileStream, codec, file.Size, new GorgonTexture2DLoadOptions
                {
                    ConvertToPremultipliedAlpha = shouldConvertToPremultiplied,
                    IsTextureCube = false,
                    Name = file.FullPath,
                    Binding = TextureBinding.ShaderResource,
                    Usage = (((usage & ResourceUsage.None) == ResourceUsage.None) || ((usage & ResourceUsage.Staging) == ResourceUsage.Staging)) ? ResourceUsage.Default : usage
                });
            }

            return texture;
        }

        /// <summary>
        /// Function to load an image from the editor file system.
        /// </summary>
        /// <param name="fileSystem">The file system containing the editor data.</param>
        /// <param name="path">The path to the sprite.</param>
        /// <param name="imageCodecs">[Optional] A list of additonal codecs used to read image data.</param>
        /// <returns>A new <see cref="IGorgonImage"/> containing the image data from the file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, or the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This method is obsolete and may not work correctly with the latest version of the editor file system. Please use the <see cref="CreateContentLoader"/> method to create an instance of the 
        /// <see cref="IGorgonContentLoader"/> to read image data instead.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method will load an image from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// The <paramref name="imageCodecs"/> parameter is used to allow custom image codecs to be used when loading data (assuming the image data was saved using one of the codecs supplied). This 
        /// allows a user to create a custom image codec plug in and use that to read image data.  
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The image file metadata must have the following attributes: <c>Type</c> with a value of "Image", and <c>ImageCodec</c> or else the image will not load.
        /// </para>
        /// <para>
        /// If image file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is read from the 
        /// file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// </remarks>
        [Obsolete("This method is no longer used. Please use the IGorgonFileSystem.CreateEditorLoader method and use the resulting object to load images.")]
        public static IGorgonImage LoadImage(this IGorgonFileSystem fileSystem, string path, IReadOnlyList<IGorgonImageCodec> imageCodecs = null)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IProjectMetadata metaData = fileSystem.GetMetadata();

            if (!metaData.ProjectItems.TryGetValue(path, out ProjectItemMetadata fileMetadata))
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            IReadOnlyDictionary<string, IGorgonImageCodec> supportedImageCodecs = GetImageCodecs(imageCodecs);

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

            IGorgonVirtualFile file = fileSystem.GetFile(path);

            if (file is null)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            bool shouldConvertToPremultiplied = false;

            if (fileMetadata.Attributes.TryGetValue("PremultipliedAlpha", out string isPremultiplied))
            {
                bool.TryParse(isPremultiplied, out shouldConvertToPremultiplied);
            }

            using Stream stream = file.OpenStream();
            return shouldConvertToPremultiplied ? imageCodec.FromStream(stream, (int)file.Size).BeginUpdate().ConvertToPremultipliedAlpha().EndUpdate() : imageCodec.FromStream(stream, (int)file.Size);
        }

        /// <summary>
        /// Function to load a sprite from the editor file system.
        /// </summary>
        /// <param name="fileSystem">The file system containing the editor data.</param>
        /// <param name="renderer">The current renderer.</param>
        /// <param name="path">The path to the sprite.</param>
        /// <param name="textureUsage">[Optional] The intended usage for the texture.</param>
        /// <param name="spriteCodecs">[Optional] A list of additonal codecs used to read sprite data.</param>
        /// <param name="imageCodecs">[Optional] A list of additonal codecs used to read image data.</param>
        /// <param name="overrideTexture">[Optional] A texture view to use instead of loading the texture from the file system.</param>
        /// <returns>A new <see cref="GorgonSprite"/>, along with its associated texture.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, <paramref name="renderer"/>, or the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This method is obsolete and may not work correctly with the latest version of the editor file system. Please use the <see cref="CreateContentLoader"/> method to create an instance of the 
        /// <see cref="IGorgonContentLoader"/> to read sprite data instead.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method will load a sprite from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// The <paramref name="spriteCodecs"/> parameter is used to allow custom sprite codecs to be used when loading data (assuming the sprite data was generated using one of the codecs supplied). This 
        /// allows a user to create a custom sprite codec plug in and use that to read sprite data.  The <paramref name="imageCodecs"/> is used in exactly the same way, but only for image data.
        /// </para>
        /// <para>
        /// Providing the <paramref name="overrideTexture"/> will skip the texture loading and use the texture passed in.  In this case, the texture return value will be <b>null</b> as it is assumed the 
        /// user already knows about the texture resource and is managing the lifetime of the texture elsewhere.
        /// </para>
        /// <para>
        /// When the method returns, it returns a tuple containing the sprite that was loaded, and the associated texture resource for the sprite. If the texture could not be loaded for any reason, 
        /// and the <paramref name="overrideTexture"/> parameter is <b>null</b>, then the texture return value will be <b>null</b>, and no texture will be assigned to the sprite.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The sprite file metadata must have the following attributes: <c>Type</c> with a value of "Sprite", and <c>SpriteCodec</c>, and its associated texture must have a dependency type of <c>Image</c> 
        /// or else the sprite will not load.
        /// </para>
        /// <para>
        /// The associated texture file metadata must have the following attributes: <c>Type</c> with a value of "Image", and <c>ImageCodec</c> or the texure will not load.
        /// </para>
        /// <para>
        /// If the associated texture file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is 
        /// read from the file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// <b>Regarding textures:</b> This method will load the associated texture for the sprite into memory, and will do its best to only load that texture one time. When the texture is loaded, it will 
        /// remain resident until Gorgon is shut down (typically when the application shuts down). In many cases, this is not ideal, so users must dispose of the <see cref="GorgonTexture2D"/> returned by 
        /// this method if unloading the texture data is desired (e.g. a level changes and new graphics need to be loaded).
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        [Obsolete("This method is no longer used. Please use the IGorgonFileSystem.CreateEditorLoader method and use the resulting object to load sprites.")]
        public static (GorgonSprite sprite, GorgonTexture2D texture) LoadSprite(this IGorgonFileSystem fileSystem, Gorgon2D renderer, string path, ResourceUsage textureUsage = ResourceUsage.Default, IReadOnlyList<IGorgonSpriteCodec> spriteCodecs = null, IReadOnlyList<IGorgonImageCodec> imageCodecs = null, GorgonTexture2DView overrideTexture = null)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IProjectMetadata metaData = fileSystem.GetMetadata();

            if (!metaData.ProjectItems.TryGetValue(path, out ProjectItemMetadata fileMetadata))
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            IReadOnlyDictionary<string, IGorgonSpriteCodec> supportedSpriteCodecs = GetSpriteCodecs(renderer, spriteCodecs);
            IReadOnlyDictionary<string, IGorgonImageCodec> supportedImageCodecs = GetImageCodecs(imageCodecs);

            if ((!fileMetadata.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string contentType))
                || (!string.Equals(contentType, CommonEditorContentTypes.SpriteType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NOT_SPRITE, path));
            }

            if (!fileMetadata.Attributes.TryGetValue("SpriteCodec", out string codecTypeName))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, string.Empty));
            }

            if (!supportedSpriteCodecs.TryGetValue(codecTypeName, out IGorgonSpriteCodec spriteCodec))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_UNSUPPORTED_CODEC, codecTypeName));
            }

            IGorgonVirtualFile file = fileSystem.GetFile(path);

            if (file is null)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));
            }

            GorgonTexture2D texture = null;
            if (overrideTexture is null)
            {
                if (fileMetadata.DependsOn.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> imagePaths))
                {
                    if ((imagePaths is not null) && (imagePaths.Count > 0))
                    {
                        texture = GetTexture(renderer.Graphics, fileSystem, metaData, imagePaths[0], textureUsage, supportedImageCodecs);
                        overrideTexture = texture.GetShaderResourceView();
                    }
                }
            }

            using Stream stream = file.OpenStream();
            return (spriteCodec.FromStream(stream, overrideTexture, (int)file.Size), texture);
        }
        #endregion

        /// <summary>
        /// Function to create a loader object for reading content from an editor file system.
        /// </summary>
        /// <param name="fileSystem">The file system that holds the content to load.</param>
        /// <param name="renderer">The 2D renderer used to handle loading of additional resources.</param>
        /// <param name="textureCache">The cache used to host any texture dependencies for the content.</param>
        /// <returns>A new <see cref="IGorgonContentLoader"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="fileSystem"/> is not a Gorgon Editor file system, or is an unsupported version.</exception>
        public static IGorgonContentLoader CreateContentLoader(this IGorgonFileSystem fileSystem, Gorgon2D renderer, GorgonTextureCache<GorgonTexture2D> textureCache)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));

            }

            if (textureCache is null)
            {
                throw new ArgumentNullException(nameof(textureCache));
            }

            IProjectMetadata metadata = fileSystem.GetMetadata();

            return new ContentLoader2D(fileSystem, metadata, renderer, textureCache);
        }
    }
}
