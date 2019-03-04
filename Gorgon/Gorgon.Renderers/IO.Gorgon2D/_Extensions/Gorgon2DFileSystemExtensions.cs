﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 12, 2018 7:31:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO.Properties;
using Gorgon.Renderers;
using SharpDX;

namespace Gorgon.IO.Extensions
{
    /// <summary>
    /// Extension methods for loading sprite data from file systems.
    /// </summary>
    public static class Gorgon2DFileSystemExtensions
    {
        // ReSharper disable PossibleMultipleEnumeration

        /// <summary>
        /// Function to determine the polygonal sprite codec to use when loading the sprite.
        /// </summary>
        /// <param name="stream">The stream containing the sprite data.</param>
        /// <param name="codecs">The list of codecs to try.</param>
        /// <returns>The sprite codec if found, or <b>null</b> if not.</returns>
        private static IGorgonPolySpriteCodec GetPolySpriteCodec(Stream stream, IEnumerable<IGorgonPolySpriteCodec> codecs)
        {
            foreach (IGorgonPolySpriteCodec codec in codecs)
            {
                if (codec.IsReadable(stream))
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to determine the sprite codec to use when loading the sprite.
        /// </summary>
        /// <param name="stream">The stream containing the sprite data.</param>
        /// <param name="codecs">The list of codecs to try.</param>
        /// <returns>The sprite codec if found, or <b>null</b> if not.</returns>
        private static IGorgonSpriteCodec GetSpriteCodec(Stream stream, IEnumerable<IGorgonSpriteCodec> codecs)
        {
            foreach (IGorgonSpriteCodec codec in codecs)
            {
                if (codec.IsReadable(stream))
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to determine the animation codec to use when loading the animation.
        /// </summary>
        /// <param name="stream">The stream containing the animation data.</param>
        /// <param name="codecs">The list of codecs to try.</param>
        /// <returns>The animation codec if found, or <b>null</b> if not.</returns>
        private static IGorgonAnimationCodec GetAnimationCodec(Stream stream, IEnumerable<IGorgonAnimationCodec> codecs)
        {
            foreach (IGorgonAnimationCodec codec in codecs)
            {
                if (codec.IsReadable(stream))
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to load an associated texture file from the file system.
        /// </summary>
        /// <param name="file">The file that may contain the texture data.</param>
        /// <param name="codecs">The list of codecs to use when determining file type.</param>
        /// <returns>The image codec to use, or <b>null</b> if no appropriate codec was found.</returns>
        private static IGorgonImageCodec FindTextureCodec(IGorgonVirtualFile file, IEnumerable<IGorgonImageCodec> codecs)
        {
            using (Stream textureStream = file.OpenStream())
            {
                // First try to find the codec by file extension.
                IEnumerable<IGorgonImageCodec> matchedExtensions =
                    codecs.Where(item => item.CodecCommonExtensions.Any(ext => string.Equals(file.Extension, ext, StringComparison.OrdinalIgnoreCase)));

                foreach (IGorgonImageCodec codec in matchedExtensions)
                {
                    if (codec.IsReadable(textureStream))
                    {
                        return codec;
                    }
                }

                // If that failed, then look it up by testing all passed in codecs.
                foreach (IGorgonImageCodec codec in codecs)
                {
                    if (codec.IsReadable(textureStream))
                    {
                        return codec;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Function to locate the associated texture codec and file for a sprite.
        /// </summary>
        /// <param name="fileSystem">The file system to evaluate.</param>
        /// <param name="localDir">The local directory for the sprite file.</param>
        /// <param name="renderer">The renderer used for resource look up.</param>
        /// <param name="textureName">The name of the texture.</param>
        /// <param name="codecs">The list of available image codecs to use when determining texture type.</param>
        /// <returns>A tuple containing the codec, the texture file, and a flag to indicate that the texture was previously loaded into memory.</returns>
        private static (IGorgonImageCodec codec, IGorgonVirtualFile file, bool alreadyLoaded) LocateTextureCodecAndFile(
            IGorgonFileSystem fileSystem,
            IGorgonVirtualDirectory localDir,
            Gorgon2D renderer,
            string textureName,
            IEnumerable<IGorgonImageCodec> codecs)
        {
            // First, attempt to locate the resource by its name.  If it's already loaded, we should not load it again.
            GorgonTexture2D texture = renderer.Graphics
                                              .LocateResourcesByName<GorgonTexture2D>(textureName)
                                              .FirstOrDefault();

            if (texture != null)
            {
                return (null, null, true);
            }

            IGorgonImageCodec codec;

            // We couldn't find the texture in our loaded resources, so try to locate it on the file system.

            // First, check the local directory.
            IEnumerable<IGorgonVirtualFile> files = fileSystem.FindFiles(localDir.FullPath, $"{textureName}.*", false);

            foreach (IGorgonVirtualFile file in files)
            {
                codec = FindTextureCodec(file, codecs);

                if (codec != null)
                {
                    return (codec, file, false);
                }
            }

            // Check to see if the name has path information for the texture in the name.
            // The GorgonEditor from v2 does this.
            if (!textureName.Contains("/"))
            {
                // It is not.  We cannot load the texture.
                return (null, null, false);
            }

            IGorgonVirtualFile textureFile = fileSystem.GetFile(textureName);

            if (textureFile == null)
            {
                return (null, null, false);
            }
            
            // Try to find a codec for the image file.
            codec = FindTextureCodec(textureFile, codecs);

            return codec == null ? (null, null, false) : (codec, textureFile, false);
        }

        /// <summary>
        /// Function to load a <see cref="GorgonSprite"/> from a <see cref="GorgonFileSystem"/>.
        /// </summary>
        /// <param name="fileSystem">The file system to load the sprite from.</param>
        /// <param name="renderer">The renderer for the sprite.</param>
        /// <param name="path">The path to the sprite file in the file system.</param>
        /// <param name="spriteCodecs">The list of sprite codecs to try and load the sprite with.</param>
        /// <param name="imageCodecs">The list of image codecs to try and load the sprite texture with.</param>
        /// <returns>The sprite data in the file as a <see cref="GorgonSprite"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, <paramref name="renderer"/>, or <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file in the <paramref name="path"/> was not found.</exception>
        /// <exception cref="GorgonException">Thrown if the sprite data in the file system could not be loaded because a suitable codec was not found.</exception>
        /// <remarks>
        /// <para>
        /// This method extends a <see cref="GorgonFileSystem"/> so that sprites can be loaded by calling a method on the file system object itself. This negates the need for users to create complex code
        /// for loading a sprite.  
        /// </para>
        /// <para>
        /// When loading a sprite, the method will attempt to locate the <see cref="GorgonTexture2DView"/> associated with the sprite (if it exists). When loading, it will check:
        /// <list type="number">
        ///     <item>
        ///         <description>For a texture resource with the same name that is already loaded into memory.</description> 
        ///     </item>
        ///     <item>
        ///         <description>Use the local <see cref="IGorgonVirtualDirectory"/> for the sprite file and search for the texture in that directory.</description> 
        ///     </item>
        ///     <item>
        ///         <description>Check the entire <paramref name="fileSystem"/> for a file if the texture name contains path information (this is done by the GorgonEditor from v2).</description>
        ///     </item>
        /// </list>
        /// If the file is found, and can be loaded by one of the <paramref name="imageCodecs"/>, then it is loaded and assigned to the sprite.
        /// </para>
        /// <para>
        /// The <paramref name="spriteCodecs"/> is a list of codecs for loading sprite data. If the user specifies this parameter, the only the codecs provided will be used for determining if a sprite can
        /// be read. If it is not supplied, then all built-in (i.e. not plug-in based) sprite codecs will be used.
        /// </para>
        /// <para>
        /// The <paramref name="imageCodecs"/> is a list of codecs for loading image data. If the user specifies this parameter, the only the codecs provided will be used for determining if an image can be 
        /// read. If it is not supplied, then all built-in (i.e. not plug-in based) image codecs will be used.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonFileSystem"/>
        /// <seealso cref="GorgonTexture2DView"/>
        /// <seealso cref="GorgonSprite"/>
        public static GorgonSprite LoadSprite(this IGorgonFileSystem fileSystem,
                                              Gorgon2D renderer,
                                              string path,
                                              IEnumerable<IGorgonSpriteCodec> spriteCodecs = null,
                                              IEnumerable<IGorgonImageCodec> imageCodecs = null)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            IGorgonVirtualFile file = fileSystem.GetFile(path);

            if (file == null)
            {
                throw new FileNotFoundException(string.Format(Resources.GOR2DIO_ERR_FILE_NOT_FOUND, path));
            }

            if ((imageCodecs == null) || (!imageCodecs.Any()))
            {
                // If we don't specify any codecs, then use the built in ones.
                imageCodecs = new IGorgonImageCodec[]
                         {
                             new GorgonCodecPng(),
                             new GorgonCodecBmp(),
                             new GorgonCodecDds(),
                             new GorgonCodecGif(),
                             new GorgonCodecJpeg(),
                             new GorgonCodecTga(),
                         };
            }
            else
            {
                // Only use codecs that can decode image data.
                imageCodecs = imageCodecs.Where(item => item.CanDecode);
            }

            if ((spriteCodecs == null) || (!spriteCodecs.Any()))
            {
                // Use all built-in codecs if we haven't asked for any.
                spriteCodecs = new IGorgonSpriteCodec[]
                               {
                                   new GorgonV3SpriteBinaryCodec(renderer),
                                   new GorgonV3SpriteJsonCodec(renderer),
                                   new GorgonV2SpriteCodec(renderer),
                                   new GorgonV1SpriteBinaryCodec(renderer),
                               };
            }
            else
            {
                // Only use codecs that can decode sprite data.
                spriteCodecs = spriteCodecs.Where(item => item.CanDecode);
            }

            // We need to copy the sprite data into a memory stream since the underlying stream may not be seekable (BZip2 lies and says it is seekable, but it really isn't).
            using (var spriteStream = new MemoryStream())
            {
                using (Stream stream = file.OpenStream())
                {
                    stream.CopyTo(spriteStream);
                    spriteStream.Position = 0;
                }

                IGorgonSpriteCodec spriteCodec = GetSpriteCodec(spriteStream, spriteCodecs);

                if (spriteCodec == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_NO_SUITABLE_SPRITE_CODEC_FOUND, path));
                }

                // Try to locate the texture.
                string textureName = spriteCodec.GetAssociatedTextureName(spriteStream);

                GorgonTexture2DView textureForSprite = null;

                // Let's try and load the texture into memory.
                // This does this by:
                // 1. Checking to see if a texture resource with the name specified is already available in memory.
                // 2. Checking the local directory of the file to see if the texture is there.
                // 3. A file system wide search.

                // ReSharper disable once InvertIf
                if (!string.IsNullOrWhiteSpace(textureName))
                {
                    (IGorgonImageCodec codec, IGorgonVirtualFile textureFile, bool loaded) =
                        LocateTextureCodecAndFile(fileSystem, file.Directory, renderer, textureName, imageCodecs);

                    // We have not loaded the texture yet.  Do so now.
                    // ReSharper disable once InvertIf
                    if ((!loaded) && (textureFile != null) && (codec != null))
                    {
                        using (Stream textureStream = textureFile.OpenStream())
                        {
                            textureForSprite = GorgonTexture2DView.FromStream(renderer.Graphics,
                                                                              textureStream,
                                                                              codec,
                                                                              textureFile.Size,
                                                                              new GorgonTexture2DLoadOptions
                                                                              {
                                                                                  Name = textureFile.FullPath,
                                                                                  Usage = ResourceUsage.Default,
                                                                                  Binding = TextureBinding.ShaderResource
                                                                              });
                        }
                    }

                }

                return spriteCodec.FromStream(spriteStream, textureForSprite, (int)file.Size);
            }
        }

        /// <summary>
        /// Function to load a <see cref="GorgonPolySprite"/> from a <see cref="GorgonFileSystem"/>.
        /// </summary>
        /// <param name="fileSystem">The file system to load the sprite from.</param>
        /// <param name="renderer">The renderer for the sprite.</param>
        /// <param name="path">The path to the sprite file in the file system.</param>
        /// <param name="spriteCodecs">The list of polygonal sprite codecs to try and load the sprite with.</param>
        /// <param name="imageCodecs">The list of image codecs to try and load the sprite texture with.</param>
        /// <returns>The sprite data in the file as a <see cref="GorgonSprite"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, <paramref name="renderer"/>, or <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file in the <paramref name="path"/> was not found.</exception>
        /// <exception cref="GorgonException">Thrown if the sprite data in the file system could not be loaded because a suitable codec was not found.</exception>
        /// <remarks>
        /// <para>
        /// This method extends a <see cref="GorgonFileSystem"/> so that sprites can be loaded by calling a method on the file system object itself. This negates the need for users to create complex code
        /// for loading a sprite.  
        /// </para>
        /// <para>
        /// When loading a sprite, the method will attempt to locate the <see cref="GorgonTexture2DView"/> associated with the sprite (if it exists). When loading, it will check:
        /// <list type="number">
        ///     <item>
        ///         <description>For a texture resource with the same name that is already loaded into memory.</description> 
        ///     </item>
        ///     <item>
        ///         <description>Use the local <see cref="IGorgonVirtualDirectory"/> for the sprite file and search for the texture in that directory.</description> 
        ///     </item>
        ///     <item>
        ///         <description>Check the entire <paramref name="fileSystem"/> for a file if the texture name contains path information (this is done by the GorgonEditor from v2).</description>
        ///     </item>
        /// </list>
        /// If the file is found, and can be loaded by one of the <paramref name="imageCodecs"/>, then it is loaded and assigned to the sprite.
        /// </para>
        /// <para>
        /// The <paramref name="spriteCodecs"/> is a list of codecs for loading polygonal sprite data. If the user specifies this parameter, the only the codecs provided will be used for determining if a
        /// sprite can be read. If it is not supplied, then all built-in (i.e. not plug-in based) sprite codecs will be used.
        /// </para>
        /// <para>
        /// The <paramref name="imageCodecs"/> is a list of codecs for loading image data. If the user specifies this parameter, the only the codecs provided will be used for determining if an image can be 
        /// read. If it is not supplied, then all built-in (i.e. not plug-in based) image codecs will be used.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonFileSystem"/>
        /// <seealso cref="GorgonTexture2DView"/>
        /// <seealso cref="GorgonPolySprite"/>
        public static GorgonPolySprite LoadPolySprite(this GorgonFileSystem fileSystem,
                                              Gorgon2D renderer,
                                              string path,
                                              IEnumerable<IGorgonPolySpriteCodec> spriteCodecs = null,
                                              IEnumerable<IGorgonImageCodec> imageCodecs = null)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            IGorgonVirtualFile file = fileSystem.GetFile(path);

            if (file == null)
            {
                throw new FileNotFoundException(string.Format(Resources.GOR2DIO_ERR_FILE_NOT_FOUND, path));
            }

            if ((imageCodecs == null) || (!imageCodecs.Any()))
            {
                // If we don't specify any codecs, then use the built in ones.
                imageCodecs = new IGorgonImageCodec[]
                         {
                             new GorgonCodecPng(),
                             new GorgonCodecBmp(),
                             new GorgonCodecDds(),
                             new GorgonCodecGif(),
                             new GorgonCodecJpeg(),
                             new GorgonCodecTga(),
                         };
            }
            else
            {
                // Only use codecs that can decode image data.
                imageCodecs = imageCodecs.Where(item => item.CanDecode);
            }

            if ((spriteCodecs == null) || (!spriteCodecs.Any()))
            {
                // Use all built-in codecs if we haven't asked for any.
                spriteCodecs = new IGorgonPolySpriteCodec[]
                               {
                                   new GorgonV3PolySpriteBinaryCodec(renderer), 
                                   new GorgonV3PolySpriteJsonCodec(renderer)
                               };
            }
            else
            {
                // Only use codecs that can decode sprite data.
                spriteCodecs = spriteCodecs.Where(item => item.CanDecode);
            }

            // We need to copy the sprite data into a memory stream since the underlying stream may not be seekable (BZip2 lies and says it is seekable, but it really isn't).
            using (var spriteStream = new MemoryStream())
            {
                using (Stream stream = file.OpenStream())
                {
                    stream.CopyTo(spriteStream);
                    spriteStream.Position = 0;
                }

                IGorgonPolySpriteCodec spriteCodec = GetPolySpriteCodec(spriteStream, spriteCodecs);

                if (spriteCodec == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_NO_SUITABLE_SPRITE_CODEC_FOUND, path));
                }

                // Try to locate the texture.
                string textureName = spriteCodec.GetAssociatedTextureName(spriteStream);

                GorgonTexture2DView textureForSprite = null;

                // Let's try and load the texture into memory.
                // This does this by:
                // 1. Checking to see if a texture resource with the name specified is already available in memory.
                // 2. Checking the local directory of the file to see if the texture is there.
                // 3. A file system wide search.

                // ReSharper disable once InvertIf
                if (!string.IsNullOrWhiteSpace(textureName))
                {
                    (IGorgonImageCodec codec, IGorgonVirtualFile textureFile, bool loaded) =
                        LocateTextureCodecAndFile(fileSystem, file.Directory, renderer, textureName, imageCodecs);

                    // We have not loaded the texture yet.  Do so now.
                    // ReSharper disable once InvertIf
                    if ((!loaded) && (textureFile != null) && (codec != null))
                    {
                        using (Stream textureStream = textureFile.OpenStream())
                        {
                            textureForSprite = GorgonTexture2DView.FromStream(renderer.Graphics,
                                                                              textureStream,
                                                                              codec,
                                                                              textureFile.Size,
                                                                              new GorgonTexture2DLoadOptions
                                                                              {
                                                                                  Name = textureFile.FullPath,
                                                                                  Usage = ResourceUsage.Default,
                                                                                  Binding = TextureBinding.ShaderResource
                                                                              });
                        }
                    }

                }

                return spriteCodec.FromStream(spriteStream, textureForSprite, (int)file.Size);
            }
        }

        /// <summary>
        /// Function to load a <see cref="IGorgonAnimation"/> from a <see cref="GorgonFileSystem"/>.
        /// </summary>
        /// <param name="fileSystem">The file system to load the animation from.</param>
        /// <param name="renderer">The renderer for the animation.</param>
        /// <param name="path">The path to the animation file in the file system.</param>
        /// <param name="animationCodecs">The list of animation codecs to try and load the animation with.</param>
        /// <param name="imageCodecs">The list of image codecs to try and load the animation texture(s) with.</param>
        /// <returns>The animation data in the file as a <see cref="IGorgonAnimation"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystem"/>, <paramref name="renderer"/>, or <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file in the <paramref name="path"/> was not found.</exception>
        /// <exception cref="GorgonException">Thrown if the animation data in the file system could not be loaded because a suitable codec was not found.</exception>
        /// <remarks>
        /// <para>
        /// This method extends a <see cref="GorgonFileSystem"/> so that animations can be loaded by calling a method on the file system object itself. This negates the need for users to create complex code
        /// for loading an animation.  
        /// </para>
        /// <para>
        /// When loading an animation, the method will attempt to locate any <see cref="GorgonTexture2DView"/> objects associated with the animation (if they exist). When loading, it will check:
        /// <list type="number">
        ///     <item>
        ///         <description>For a texture resource with the same name that is already loaded into memory.</description> 
        ///     </item>
        ///     <item>
        ///         <description>Use the local <see cref="IGorgonVirtualDirectory"/> for the sprite file and search for the texture in that directory.</description> 
        ///     </item>
        ///     <item>
        ///         <description>Check the entire <paramref name="fileSystem"/> for a file if the texture name contains path information (this is done by the GorgonEditor from v2).</description>
        ///     </item>
        /// </list>
        /// If the file is found, and can be loaded by one of the <paramref name="imageCodecs"/>, then it is loaded and assigned to the sprite.
        /// </para>
        /// <para>
        /// The <paramref name="animationCodecs"/> is a list of codecs for loading sprite data. If the user specifies this parameter, the only the codecs provided will be used for determining if an 
        /// animation can be read. If it is not supplied, then all built-in (i.e. not plug-in based) sprite codecs will be used.
        /// </para>
        /// <para>
        /// The <paramref name="imageCodecs"/> is a list of codecs for loading image data. If the user specifies this parameter, the only the codecs provided will be used for determining if an image can be 
        /// read. If it is not supplied, then all built-in (i.e. not plug-in based) image codecs will be used.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonFileSystem"/>
        /// <seealso cref="GorgonTexture2DView"/>
        /// <seealso cref="IGorgonAnimation"/>
        public static IGorgonAnimation LoadAnimation(this GorgonFileSystem fileSystem,
                                              Gorgon2D renderer,
                                              string path,
                                              IEnumerable<IGorgonAnimationCodec> animationCodecs = null,
                                              IEnumerable<IGorgonImageCodec> imageCodecs = null)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            IGorgonVirtualFile file = fileSystem.GetFile(path);

            if (file == null)
            {
                throw new FileNotFoundException(string.Format(Resources.GOR2DIO_ERR_FILE_NOT_FOUND, path));
            }

            if ((imageCodecs == null) || (!imageCodecs.Any()))
            {
                // If we don't specify any codecs, then use the built in ones.
                imageCodecs = new IGorgonImageCodec[]
                         {
                             new GorgonCodecPng(),
                             new GorgonCodecBmp(),
                             new GorgonCodecDds(),
                             new GorgonCodecGif(),
                             new GorgonCodecJpeg(),
                             new GorgonCodecTga(),
                         };
            }
            else
            {
                // Only use codecs that can decode image data.
                imageCodecs = imageCodecs.Where(item => item.CanDecode);
            }

            if ((animationCodecs == null) || (!animationCodecs.Any()))
            {
                // Use all built-in codecs if we haven't asked for any.
                animationCodecs = new IGorgonAnimationCodec[]
                               {
                                   new GorgonV3AnimationBinaryCodec(renderer), 
                                   new GorgonV3AnimationJsonCodec(renderer),
                                   new GorgonV1AnimationCodec(renderer)
                               };
            }
            else
            {
                // Only use codecs that can decode sprite data.
                animationCodecs = animationCodecs.Where(item => item.CanDecode);
            }

            // We need to copy the sprite data into a memory stream since the underlying stream may not be seekable (BZip2 lies and says it is seekable, but it really isn't).
            using (var animStream = new MemoryStream())
            {
                using (Stream stream = file.OpenStream())
                {
                    stream.CopyTo(animStream);
                    animStream.Position = 0;
                }

                IGorgonAnimationCodec animationCodec = GetAnimationCodec(animStream, animationCodecs);

                if (animationCodec == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_NO_SUITABLE_ANIM_CODEC_FOUND, path));
                }

                // Load the animation.
                IGorgonAnimation animation = animationCodec.FromStream(animStream, (int)file.Size);

                // We have no textures to update, leave.
                if (animation.Texture2DTrack.KeyFrames.Count == 0)
                {
                    return animation;
                }

                // Try to locate the textures.

                // V1 sprite animations need texture coordinate correction.
                bool needsCoordinateFix = animationCodec is GorgonV1AnimationCodec;

                foreach (GorgonKeyTexture2D textureKey in animation.Texture2DTrack.KeyFrames)
                {
                    // Let's try and load the texture into memory.
                    // This does this by:
                    // 1. Checking to see if a texture resource with the name specified is already available in memory.
                    // 2. Checking the local directory of the file to see if the texture is there.
                    // 3. A file system wide search.

                    // ReSharper disable once InvertIf
                    (IGorgonImageCodec codec, IGorgonVirtualFile textureFile, bool loaded) =
                        LocateTextureCodecAndFile(fileSystem, file.Directory, renderer, textureKey.TextureName, imageCodecs);

                    // We have not loaded the texture yet.  Do so now.
                    // ReSharper disable once InvertIf
                    if ((!loaded) && (textureFile != null) && (codec != null))
                    {
                        using (Stream textureStream = textureFile.OpenStream())
                        {
                            textureKey.Value  = GorgonTexture2DView.FromStream(renderer.Graphics,
                                                                              textureStream,
                                                                              codec,
                                                                              textureFile.Size,
                                                                              new GorgonTexture2DLoadOptions
                                                                              {
                                                                                  Name = textureFile.FullPath,
                                                                                  Usage = ResourceUsage.Default,
                                                                                  Binding = TextureBinding.ShaderResource
                                                                              });
                        }
                    }

                    if ((needsCoordinateFix) && (textureKey.Value != null))
                    {
                        textureKey.TextureCoordinates = new RectangleF(textureKey.TextureCoordinates.X / textureKey.Value.Width,
                                                                       textureKey.TextureCoordinates.Y / textureKey.Value.Height,
                                                                       textureKey.TextureCoordinates.Width / textureKey.Value.Width,
                                                                       textureKey.TextureCoordinates.Height / textureKey.Value.Height);
                    }
                }

                return animation;
            }
        }
        // ReSharper restore PossibleMultipleEnumeration
    }
    
}
