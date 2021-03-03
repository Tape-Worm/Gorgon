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

using System;
using System.Diagnostics;
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
        // The DDS codec used to import files.
        private readonly IGorgonImageCodec _ddsCodec = new GorgonCodecDds();
        // The renderer used to locate the image for the sprite.
        private readonly Gorgon2D _renderer;
        // The sprite codecs available to the system.
        private readonly CodecRegistry _codecs;
        // The read only project file system.
        private readonly IGorgonFileSystem _projectFileSystem;
        // The temporary file system for writing working data.
        private readonly IGorgonFileSystemWriter<Stream> _tempFileSystem;
        // The path to the temporary directory.
        private string _tempDirPath;
        #endregion

        #region Properties.
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
                IGorgonVirtualFile textureFile = _projectFileSystem.GetFile(textureName);

                if (textureFile is null)
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
        /// <param name="sourceFilePath">The path to the sprite file to import.</param>
        /// <param name="codec">The codec for the source file.</param>
        /// <returns>The texture being imported.</returns>
        private GorgonTexture2DView GetTexture(string sourceFilePath, IGorgonSpriteCodec codec)
        {
            using (Stream fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string textureName = codec.GetAssociatedTextureName(fileStream);
                GorgonTexture2DView textureForSprite = null;

                // Let's try and load the texture into memory.
                if (string.IsNullOrWhiteSpace(textureName))
                {
                    return null;
                }
                IGorgonVirtualFile textureFile = LocateTextureFile(textureName);

                // We couldn't load the file, so, let's try again without a file extension since we strip those.
                int extensionDot = textureName.LastIndexOf('.');
                if ((textureFile is null) && (extensionDot > 1))
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

                return textureForSprite;
            }
        }

        /// <summary>Function to import content.</summary>
        /// <param name="physicalFilePath">The path to the physical file to import into the virtual file system.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>A new virtual file object pointing to the imported file data.</returns>
        public IGorgonVirtualFile ImportData(string physicalFilePath, CancellationToken cancelToken)
        {
            GorgonTexture2DView texture = null;
            Stream fileStream = null;
            Stream outStream = null;

            try
            {
                var spriteCodec = new GorgonV3SpriteBinaryCodec(_renderer);

                _log.Print("Importing associated texture for sprite...", LoggingLevel.Simple);

                IGorgonSpriteCodec sourceCodec = SpriteImporterPlugIn.GetCodec(physicalFilePath, _codecs);
                Debug.Assert(sourceCodec != null, "We shouldn't be able to get this far without a codec.");
                                
                texture = GetTexture(physicalFilePath, sourceCodec);

                if (string.IsNullOrWhiteSpace(_tempDirPath))
                {
                    _tempDirPath = $"/SpriteImporter_{Guid.NewGuid():N}/";
                }

                IGorgonVirtualDirectory directory = _tempFileSystem.CreateDirectory(_tempDirPath);
                _log.Print($"Importing file '{physicalFilePath}' (Codec: {sourceCodec.Name})...", LoggingLevel.Verbose);

                string outputFilePath = directory.FullPath + Path.GetFileName(physicalFilePath);
                int lastExt = outputFilePath.LastIndexOf('.');

                if (lastExt == -1)
                {
                    outputFilePath += spriteCodec.FileExtensions[0].Extension;
                }
                else
                {
                    outputFilePath = outputFilePath.Substring(0, lastExt) + "." + spriteCodec.FileExtensions[0].Extension;
                }

                fileStream = File.Open(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                outStream = _tempFileSystem.OpenStream(outputFilePath, FileMode.Create);
                GorgonSprite sprite = sourceCodec.FromStream(fileStream, texture);
                _log.Print($"Converting '{physicalFilePath}' to Gorgon Sprite v3 file format.", LoggingLevel.Verbose);
                spriteCodec.Save(sprite, outStream);
                fileStream.Dispose();
                outStream.Dispose();
                
                return _tempFileSystem.FileSystem.GetFile(outputFilePath);
            }
            catch
            {
                texture?.Dispose();
                throw;
            }
            finally
            {
                fileStream?.Dispose();
                outStream?.Dispose();
            }
        }

        /// <summary>Function to clean up any temporary working data.</summary>
        public void CleanUp()
        {
            IGorgonVirtualDirectory directory = _tempFileSystem.FileSystem.GetDirectory(_tempDirPath);

            if (directory is null)
            {
                return;
            }

            try
            {
                _tempFileSystem.DeleteDirectory(directory.FullPath);
                _tempDirPath = null;
            }
            catch (Exception ex)
            {
                // We'll eat and log this exception, the worst case is we end up with a little more disk usage than we'd like.
                _log.Print("Error cleaning up temporary directory.", LoggingLevel.Simple);
                _log.LogException(ex);
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonSpriteImporter"/> class.</summary>
        /// <param name="projectFileSystem">The read only file system used by the project.</param>
        /// <param name="tempFileSystem">The temporary file system to use for writing working data.</param>
        /// <param name="codecs">The sprite codecs available to the system.</param>
        /// <param name="renderer">The renderer used to locate the image linked to the sprite.</param>
        /// <param name="log">The log used for logging debug messages.</param>
        public GorgonSpriteImporter(IGorgonFileSystem projectFileSystem, IGorgonFileSystemWriter<Stream> tempFileSystem, CodecRegistry codecs, Gorgon2D renderer, IGorgonLog log)
        {
            _projectFileSystem = projectFileSystem;
            _tempFileSystem = tempFileSystem;
            _codecs = codecs;
            _log = log ?? GorgonLog.NullLog;
            _renderer = renderer;
        }
        #endregion
    }
}
