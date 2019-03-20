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
// Created: December 18, 2018 12:30:56 AM
// 
#endregion

using System.IO;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor.Services
{
    /// <summary>
    /// A sprite importer that reads in a sprite file, and converts it into a Gorgon sprite (v3) format image prior to import into the application.
    /// </summary>
    internal class GorgonSpriteImporter
        : IEditorContentImporter
    {
        #region Variables.
        // The log used for debug message logging.
        private readonly IGorgonLog _log;
        // The codec for the input file.
        private readonly IGorgonSpriteCodec _codec;
        // The DDS codec used to import files.
        private readonly IGorgonImageCodec _ddsCodec = new GorgonCodecDds();
        // The renderer used to locate the image for the sprite.
        private readonly Gorgon2D _renderer;
        // The file system containing the sprite being imported, used for texture look up.
        private readonly IGorgonFileSystem _fileSystem;
        #endregion

        #region Properties.
        /// <summary>Property to return the file being imported.</summary>
        /// <value>The source file.</value>
        public FileInfo SourceFile
        {
            get;
        }

        /// <summary>Property to return whether or not the imported file needs to be cleaned up after processing.</summary>
        public bool NeedsCleanup => true;
        #endregion

        #region Methods.

        /// <summary>
        /// Function to locate the associated texture file for a sprite.
        /// </summary>
        /// <param name="textureName">The name of the texture.</param>
        /// <returns>The texture file.</returns>
        private IGorgonVirtualFile LocateTextureFile(string textureName)
        {
            // Check to see if the name has path information for the texture in the name.
            // The GorgonEditor from v2 does this.
            if (textureName.Contains("/"))
            {
                IGorgonVirtualFile textureFile = _fileSystem.GetFile(textureName);

                if (textureFile == null)
                {
                    return null;
                }

                using (Stream imgFileStream = textureFile.OpenStream())
                {
                    if (_ddsCodec.IsReadable(imgFileStream))
                    {
                        return textureFile;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Function to locate the texture file associated with the sprite being imported.
        /// </summary>
        /// <returns>The texture being imported.</returns>
        private GorgonTexture2DView GetTexture()
        {
            // We need to copy the sprite data into a memory stream since the underlying stream may not be seekable (BZip2 lies and says it is seekable, but it really isn't).
            using (Stream fileStream = SourceFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string textureName = _codec.GetAssociatedTextureName(fileStream);
                GorgonTexture2DView textureForSprite = null;

                // Let's try and load the texture into memory.
                // ReSharper disable once InvertIf
                if (!string.IsNullOrWhiteSpace(textureName))
                {
                    IGorgonVirtualFile textureFile = LocateTextureFile(textureName);

                    // We couldn't load the file, so, let's try again without a file extension since we strip those.
                    int extensionDot = textureName.LastIndexOf('.');
                    if ((textureFile == null) && (extensionDot > 1))
                    {
                        textureName = textureName.Substring(0, extensionDot);
                        textureFile = LocateTextureFile(textureName);
                    }

                    // We have not loaded the texture yet.  Do so now.
                    // ReSharper disable once InvertIf
                    if (textureFile != null)
                    {
                        using (Stream textureStream = textureFile.OpenStream())
                        {
                            textureForSprite = GorgonTexture2DView.FromStream(_renderer.Graphics,
                                                                              textureStream,
                                                                              _ddsCodec,
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

                return textureForSprite;
            }
        }        

        /// <summary>Imports the data.</summary>
        /// <param name="temporaryDirectory">The temporary directory for writing any transitory data.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="temporaryDirectory"/> should be used to write any working/temporary data used by the import.  Note that all data written into this directory will be deleted when the 
        /// project is unloaded from memory.
        /// </para>
        /// </remarks>
        public FileInfo ImportData(DirectoryInfo temporaryDirectory, CancellationToken cancelToken)
        {
            var spriteCodec = new GorgonV3SpriteBinaryCodec(_renderer);

            _log.Print("Importing associated texture for sprite...", LoggingLevel.Simple);
            GorgonTexture2DView texture = GetTexture();

            try
            {                
                _log.Print($"Importing file '{SourceFile.FullName}' (Codec: {_codec.Name})...", LoggingLevel.Verbose);
                GorgonSprite sprite = _codec.FromFile(SourceFile.FullName);

                if (sprite.Texture == null)
                {
                    sprite.Texture = texture;
                }

                var tempFile = new FileInfo(Path.Combine(temporaryDirectory.FullName, Path.GetFileNameWithoutExtension(SourceFile.Name)));
                
                _log.Print($"Converting '{SourceFile.FullName}' to Gorgon v3 Sprite file format.", LoggingLevel.Verbose);
                spriteCodec.Save(sprite, tempFile.FullName);

                return tempFile;
            }
            finally
            {
                texture?.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.Services.GorgonSpriteImporter"/> class.</summary>
        /// <param name="sourceFile">The file being imported.</param>
        /// <param name="codec">The original codec for the file.</param>
        /// <param name="renderer">The renderer used to locate the image linked to the sprite.</param>
        /// <param name="fileSystem">The file system containing the file being imported.</param>
        /// <param name="log">The log used for logging debug messages.</param>
        public GorgonSpriteImporter(FileInfo sourceFile, IGorgonSpriteCodec codec, Gorgon2D renderer, IGorgonFileSystem fileSystem, IGorgonLog log)
        {
            _log = log ?? GorgonLog.NullLog;
            SourceFile = sourceFile;
            _codec = codec;
            _renderer = renderer;
            _fileSystem = fileSystem;
        }
        #endregion
    }
}
