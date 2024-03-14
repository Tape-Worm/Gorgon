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

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor.Services;

/// <summary>
/// An image importer that reads in an image file, and converts it into a DDS format image prior to import into the application.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="DdsImageImporter"/> class.</remarks>
/// <param name="tempFileSystemWriter">The file system writer used to write to the temporary area of the project.</param>
/// <param name="codecs">The available codecs for image import.</param>
/// <param name="log">The log used for logging debug messages.</param>
internal class DdsImageImporter(IGorgonFileSystemWriter<Stream> tempFileSystemWriter, ICodecRegistry codecs, IGorgonLog log)
        : IEditorContentImporter
{
    #region Variables.
    // The log used for debug message logging.
    private readonly IGorgonLog _log = log ?? GorgonLog.NullLog;
    // The file system writer used to write to the temporary area.
    private readonly IGorgonFileSystemWriter<Stream> _tempWriter = tempFileSystemWriter;
    // The available image codecs.
    private readonly ICodecRegistry _codecs = codecs;
    // The path to the temporary directory.
    private string _tempDirPath;
    #endregion

    #region Methods.
    /// <summary>Function to clean up any temporary working data.</summary>
    public void CleanUp()
    {
        IGorgonVirtualDirectory directory = _tempWriter.FileSystem.GetDirectory(_tempDirPath);

        if (directory is null)
        {
            return;
        }

        try
        {
            _tempWriter.DeleteDirectory(directory.FullPath);
            _tempDirPath = null;
        }
        catch(Exception ex)
        {
            // We'll eat and log this exception, the worst case is we end up with a little more disk usage than we'd like.
            _log.Print("Error cleaning up temporary directory.", LoggingLevel.Simple);
            _log.LogException(ex);
        }
    }

    /// <summary>Function to import content.</summary>
    /// <param name="physicalFilePath">The path to the physical file to import into the virtual file system.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    /// <returns>A new virtual file object pointing to the imported file data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalFilePath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalFilePath"/> parameter is empty.</exception>
    public IGorgonVirtualFile ImportData(string physicalFilePath, CancellationToken cancelToken)
    {
        if (physicalFilePath is null)
        {
            throw new ArgumentNullException(nameof(physicalFilePath));
        }

        if (string.IsNullOrWhiteSpace(physicalFilePath))
        {
            throw new ArgumentEmptyException(nameof(physicalFilePath));
        }

        IGorgonImageCodec sourceCodec = ImageImporterPlugIn.GetCodec(physicalFilePath, _codecs);

        // This source is the same as the destination codec. So there's nothing to do.
        if (sourceCodec is null)
        {
            return null;
        }

        var ddsCodec = new GorgonCodecDds();

        if (string.IsNullOrWhiteSpace(_tempDirPath))
        {
            _tempDirPath = $"/Importer_{Guid.NewGuid():N}/";
        }

        IGorgonVirtualDirectory directory = _tempWriter.CreateDirectory(_tempDirPath);
        _log.Print($"Importing file '{physicalFilePath}' (Codec: {sourceCodec.Name})...", LoggingLevel.Verbose);

        string outputFilePath = directory.FullPath + Path.GetFileName(physicalFilePath);

        int lastExt = outputFilePath.LastIndexOf('.');

        if (lastExt == -1)
        {
            outputFilePath += ddsCodec.CodecCommonExtensions[0];
        }
        else
        {
            outputFilePath = outputFilePath[..lastExt] + "." + ddsCodec.CodecCommonExtensions[0];
        }

        using (Stream fileStream = File.Open(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (IGorgonImage image = sourceCodec.FromStream(fileStream))
        using (Stream outStream = _tempWriter.OpenStream(outputFilePath, FileMode.Create))
        {
            _log.Print($"Converting '{physicalFilePath}' to DDS file format. Image format [{image.Format}].", LoggingLevel.Verbose);
            ddsCodec.Save(image, outStream);
        }

        return _tempWriter.FileSystem.GetFile(outputFilePath);
    }

    #endregion
}
