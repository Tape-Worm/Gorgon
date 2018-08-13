#region MIT
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
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO.Properties;
using Gorgon.Renderers;

namespace Gorgon.IO.FileSystemExtensions
{
    /// <summary>
    /// Extension methods for loading sprite data from file systems.
    /// </summary>
    public static class GorgonFileSystemExtensions
    {
        /// <summary>
        /// Function to determine the sprite codec to use when loading the sprite.
        /// </summary>
        /// <param name="renderer">The renderer to use with the sprite codec.</param>
        /// <param name="stream">The stream containing the sprite data.</param>
        /// <param name="codecs">The list of codecs to try.</param>
        /// <returns>The sprite codec if found, or <b>null</b> if not.</returns>
        private static IGorgonSpriteCodec GetSpriteCodec(Gorgon2D renderer, Stream stream, IEnumerable<IGorgonSpriteCodec> codecs)
        {
            if (codecs == null)
            {
                // Use all built-in codecs if we haven't asked for any.
                codecs = new IGorgonSpriteCodec[]
                         {
                             new GorgonV1SpriteBinaryCodec(renderer),
                             new GorgonV2SpriteCodec(renderer),
                             new GorgonV3SpriteJsonCodec(renderer),
                         };
            }

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
        /// Function to load an associated texture file from the file system.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create the texture.</param>
        /// <param name="file">The file </param>
        /// <param name="codecs"></param>
        /// <returns></returns>
        private static IGorgonImageCodec FindTextureCodec(GorgonGraphics graphics, IGorgonVirtualFile file, IEnumerable<IGorgonImageCodec> codecs)
        {
            using (Stream textureStream = file.OpenStream())
            {
                foreach (IGorgonImageCodec codec in codecs)
                {
                    if ((codec.CanDecode) && (codec.IsReadable(textureStream)))
                    {
                        return codec;
                    }
                }
            }

            return null;
        }

        private static (IGorgonImageCodec codec, IGorgonVirtualFile file, bool alreadyLoaded) LocateTextureCodecAndFile(
            GorgonFileSystem fileSystem,
            IGorgonVirtualDirectory localDir,
            Gorgon2D renderer,
            string textureName,
            IEnumerable<IGorgonImageCodec> codecs)
        {
            // First, attempt to locate the resource by its name.  If it's already loaded, we should not load it again.
            GorgonTexture2D texture = renderer.Graphics.LocateResourcesByName<GorgonTexture2D>(textureName).FirstOrDefault();

            if (texture != null)
            {
                return (null, null, true);
            }

            IGorgonImageCodec codec;

            if (codecs == null)
            {
                // If we don't specify any codecs, then use the built in ones.
                codecs = new IGorgonImageCodec[]
                         {
                             new GorgonCodecPng(),
                             new GorgonCodecBmp(),
                             new GorgonCodecDds(),
                             new GorgonCodecGif(),
                             new GorgonCodecJpeg(),
                             new GorgonCodecTga(),
                         };
            }

            // We couldn't find the texture, so try to locate it on the file system.

            // First, check the local directory.
            IEnumerable<IGorgonVirtualFile> files = fileSystem.FindFiles(localDir.FullPath, $"{textureName}.*", false);

            foreach (IGorgonVirtualFile file in files)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                codec = FindTextureCodec(renderer.Graphics, file, codecs);

                if (codec != null)
                {
                    return (codec, file, false);
                }
            }

            // Check to see if the name has path information for the texture in the name.
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

            codec = FindTextureCodec(renderer.Graphics, textureFile, codecs);

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
        /// <seealso cref="GorgonFileSystem"/>
        public static GorgonSprite LoadSprite(this GorgonFileSystem fileSystem,
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

            // We need to copy the sprite data into a memory stream since the underlying stream may not be seekable (BZip2 lies and says it is seekable, but it really isn't).
            using (var spriteStream = new MemoryStream())
            {
                using (Stream stream = file.OpenStream())
                {
                    stream.CopyTo(spriteStream);
                    spriteStream.Position = 0;
                }

                IGorgonSpriteCodec spriteCodec = GetSpriteCodec(renderer, spriteStream, spriteCodecs);

                if (spriteCodec == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOR2DIO_ERR_NO_SUITABLE_CODEC_FOUND, path));
                }

                // Try to locate the texture.
                string textureName = spriteCodec.GetAssociatedTextureName(spriteStream);

                GorgonTexture2DView textureForSprite = null;

                // Let's try and load the into memory.
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
                                                                              new GorgonTextureLoadOptions
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
    }
}
