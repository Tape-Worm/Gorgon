
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: December 18, 2018 12:30:56 AM
// 

using System.Diagnostics;
using Gorgon.Animation;
using Gorgon.Diagnostics;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor.Services;

/// <summary>
/// A animation importer that reads in a animation file, and converts it into a Gorgon animation (v3) format image prior to import into the application
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="GorgonAnimationImporter"/> class.</remarks>
/// <param name="projectFileSystem">The read only file system used by the project.</param>
/// <param name="tempFileSystem">The temporary file system to use for writing working data.</param>
/// <param name="codecs">The animation codecs available to the system.</param>
/// <param name="renderer">The renderer used to locate the image linked to the animation.</param>
/// <param name="log">The log used for logging debug messages.</param>
internal class GorgonAnimationImporter(IGorgonFileSystem projectFileSystem, IGorgonFileSystemWriter<Stream> tempFileSystem, CodecRegistry codecs, Gorgon2D renderer, IGorgonLog log)
        : IEditorContentImporter
{

    // The log used for debug message logging.
    private readonly IGorgonLog _log = log ?? GorgonLog.NullLog;
    // The DDS codec used to import files.
    private readonly IGorgonImageCodec _ddsCodec = new GorgonCodecDds();
    // The renderer used to locate the image for the animation.
    private readonly Gorgon2D _renderer = renderer;
    // The animation codecs available to the system.
    private readonly CodecRegistry _codecs = codecs;
    // The read only project file system.
    private readonly IGorgonFileSystem _projectFileSystem = projectFileSystem;
    // The temporary file system for writing working data.
    private readonly IGorgonFileSystemWriter<Stream> _tempFileSystem = tempFileSystem;
    // The path to the temporary directory.
    private string _tempDirPath;

    /// <summary>Property to return whether or not the imported file needs to be cleaned up after processing.</summary>
    public bool NeedsCleanup => true;

    /// <summary>
    /// Function to locate the associated texture file for a animation.
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

            using Stream imgFileStream = textureFile.OpenStream();
            if (_ddsCodec.IsReadable(imgFileStream))
            {
                return textureFile;
            }
        }

        return null;
    }

    /// <summary>
    /// Function to locate the texture files associated with the animation being imported.
    /// </summary>
    /// <param name="sourceFilePath">The path to the animation file to import.</param>
    /// <param name="codec">The codec for the source file.</param>
    /// <returns>The texture being imported.</returns>
    private IReadOnlyList<GorgonTexture2DView> GetTextures(string sourceFilePath, IGorgonAnimationCodec codec)
    {
        using Stream fileStream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IReadOnlyList<string> textureNames = codec.GetAssociatedTextureNames(fileStream);

        // Let's try and load the texture into memory.
        if (textureNames.Count == 0)
        {
            return [];
        }

        List<GorgonTexture2DView> textureForAnimation = [];

        foreach (string textureName in textureNames)
        {
            IGorgonVirtualFile textureFile = LocateTextureFile(textureName);

            // We couldn't load the file, so, let's try again without a file extension since we strip those.
            int extensionDot = textureName.LastIndexOf('.');
            if ((textureFile is null) && (extensionDot > 1))
            {
                textureFile = LocateTextureFile(textureName[..extensionDot]);
            }

            // We have not loaded the texture yet.  Do so now.
            // ReSharper disable once InvertIf
            if (textureFile is not null)
            {
                using Stream textureStream = textureFile.OpenStream();
                textureForAnimation.Add(GorgonTexture2DView.FromStream(_renderer.Graphics,
                                                                    textureStream,
                                                                    _ddsCodec,
                                                                    textureFile.Size,
                                                                    new GorgonTexture2DLoadOptions
                                                                    {
                                                                        Name = textureFile.FullPath,
                                                                        Usage = ResourceUsage.Default,
                                                                        Binding = TextureBinding.ShaderResource
                                                                    }));
            }
        }

        return textureForAnimation;
    }

    /// <summary>Function to import content.</summary>
    /// <param name="physicalFilePath">The path to the physical file to import into the virtual file system.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    /// <returns>A new virtual file object pointing to the imported file data.</returns>
    public IGorgonVirtualFile ImportData(string physicalFilePath, CancellationToken cancelToken)
    {
        IReadOnlyList<GorgonTexture2DView> textures = null;
        Stream fileStream = null;
        Stream outStream = null;

        try
        {
            GorgonV31AnimationBinaryCodec animationCodec = new(_renderer);

            _log.Print("Importing associated texture for animation...", LoggingLevel.Simple);

            IGorgonAnimationCodec sourceCodec = AnimationImporterPlugIn.GetCodec(physicalFilePath, _codecs);
            Debug.Assert(sourceCodec is not null, "We shouldn't be able to get this far without a codec.");

            textures = GetTextures(physicalFilePath, sourceCodec);

            if (string.IsNullOrWhiteSpace(_tempDirPath))
            {
                _tempDirPath = $"/AnimationImporter_{Guid.NewGuid():N}/";
            }

            IGorgonVirtualDirectory directory = _tempFileSystem.CreateDirectory(_tempDirPath);
            _log.Print($"Importing file '{physicalFilePath}' (Codec: {sourceCodec.Name})...", LoggingLevel.Verbose);

            string outputFilePath = directory.FullPath + Path.GetFileName(physicalFilePath);
            int lastExt = outputFilePath.LastIndexOf('.');

            if (lastExt == -1)
            {
                outputFilePath += animationCodec.FileExtensions[0].Extension;
            }
            else
            {
                outputFilePath = outputFilePath[..lastExt] + "." + animationCodec.FileExtensions[0].Extension;
            }

            fileStream = File.Open(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            outStream = _tempFileSystem.OpenStream(outputFilePath, FileMode.Create);
            IGorgonAnimation animation = sourceCodec.FromStream(fileStream, name: outputFilePath);
            _log.Print($"Converting '{physicalFilePath}' to Gorgon Animation v3.1 file format.", LoggingLevel.Verbose);
            animationCodec.Save(animation, outStream);
            fileStream.Dispose();
            outStream.Dispose();

            return _tempFileSystem.FileSystem.GetFile(outputFilePath);
        }
        catch
        {
            if (textures is not null)
            {
                foreach (GorgonTexture2DView texture in textures)
                {
                    texture.Dispose();
                }
            }
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
}
