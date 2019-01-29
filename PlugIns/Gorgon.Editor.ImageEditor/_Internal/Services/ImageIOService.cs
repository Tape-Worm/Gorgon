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
// Created: January 4, 2019 9:42:51 PM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides I/O functionality for reading/writing image data.
    /// </summary>
    internal class ImageIOService : IImageIOService
    {
        #region Variables.
        // The block compressor.
        private readonly TexConvCompressor _compressor;
        // The export image dialog service.
        private readonly IExportImageDialogService _exportDialog;
        // The logging interface to use.
        private readonly IGorgonLog _log;
        // The busy state service.
        private readonly IBusyStateService _busyState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the default plugin codec.
        /// </summary>
        public IGorgonImageCodec DefaultCodec
        {
            get;
        }

        /// <summary>
        /// Property to return the list of installed image codecs.
        /// </summary>
        public IReadOnlyList<IGorgonImageCodec> InstalledCodecs
        {
            get;
        }

        /// <summary>
        /// Property to return the file system writer used to write to the temporary area.
        /// </summary>
        public IGorgonFileSystemWriter<Stream> ScratchArea
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not block compression is supported.
        /// </summary>
        public bool CanHandleBlockCompression => _compressor != null;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform an export of an image.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="image">The image data to export.</param>
        /// <param name="codec">The codec to use when encoding the image.</param>
        /// <returns>The path to the exported file.</returns>
        public FileInfo ExportImage(IContentFile file, IGorgonImage image, IGorgonImageCodec codec)
        {
            _exportDialog.ContentFile = file;
            _exportDialog.SelectedCodec = codec;
            string exportFilePath = _exportDialog.GetFilename();

            if (string.IsNullOrWhiteSpace(exportFilePath))
            {
                return null;
            }

            _busyState.SetBusy();

            var result = new FileInfo(exportFilePath);

            _log.Print($"Exporting '{file.Name}' to '{exportFilePath}' as {codec.CodecDescription}", LoggingLevel.Verbose);
            codec.SaveToFile(image, result.FullName);

            return result;
        }

        /// <summary>
        /// Function to save the specified image file.
        /// </summary>
        /// <param name="name">The name of the file to write into.</param>
        /// <param name="image">The image to save.</param>
        /// <param name="pixelFormat">The pixel format for the image.</param>
        /// <returns>The updated working file.</returns>
        public IGorgonVirtualFile SaveImageFile(string name, IGorgonImage image, BufferFormat pixelFormat)
        {
            IGorgonVirtualFile result = null;

            // We absolutely need to have an extension, or else the texconv tool will not work.
            if ((DefaultCodec.CodecCommonExtensions.Count > 0) 
                && (!string.Equals(Path.GetExtension(name), DefaultCodec.CodecCommonExtensions[0], System.StringComparison.OrdinalIgnoreCase)))
            {
                _log.Print("Adding DDS extension to working file or else external tools may not be able to read it.", LoggingLevel.Verbose);
                name = Path.ChangeExtension(name, DefaultCodec.CodecCommonExtensions[0]);
            }

            IGorgonVirtualFile workFile = ScratchArea.FileSystem.GetFile(name);
            var formatInfo = new GorgonFormatInfo(pixelFormat);

            // The file doesn't exist, so we need to create a dummy file.
            if (workFile == null)
            {
                using (Stream tempStream = ScratchArea.OpenStream(name, FileMode.Create))
                {
                    tempStream.WriteString("TEMP_WORKING_FILE");
                }

                workFile = ScratchArea.FileSystem.GetFile(name);
            }

            _log.Print($"Working image file: '{workFile.FullPath}'.", LoggingLevel.Verbose);

            // For compressed images, we need to rely on an external tool to do the job.             
            if (formatInfo.IsCompressed)
            {
                _log.Print($"Pixel format [{pixelFormat}] is a block compression format, compressing using external tool...", LoggingLevel.Intermediate);
                if (_compressor == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Saving to working file '{workFile.FullPath}'...", LoggingLevel.Simple);
                result = _compressor.Compress(workFile, pixelFormat, image.MipCount);

                if (result == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }
            }
            else
            {
                // We've changed the pixel format, so convert prior to saving.
                if (pixelFormat != image.Format)
                {
                    _log.Print($"Image pixel format [{image.Format}] is different than requested format of [{pixelFormat}], converting...", LoggingLevel.Intermediate);
                    image.ConvertToFormat(pixelFormat);
                    _log.Print($"Converted image '{workFile.Name}' to pixel format: [{pixelFormat}].", LoggingLevel.Simple);
                }

                _log.Print($"Saving to working file '{workFile.FullPath}'...", LoggingLevel.Simple);
                using (Stream outStream = ScratchArea.OpenStream(workFile.FullPath, FileMode.Create))
                {
                    DefaultCodec.SaveToStream(image, outStream);
                }

                ScratchArea.FileSystem.Refresh();
                result = ScratchArea.FileSystem.GetFile(workFile.FullPath);
            }

            return result;
        }


        /// <summary>Function to load the image file into memory.</summary>
        /// <param name="file">The stream for the file to load.</param>
        /// <param name="name">The name of the file.</param>
        /// <returns>The image data, the virtual file entry for the working file and the original pixel format of the file.</returns>
        public (IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) LoadImageFile(Stream file, string name)
        {
            IGorgonImage result = null;
            IGorgonVirtualFile workFile;
            BufferFormat originalFormat;

            IGorgonImageInfo imageInfo = DefaultCodec.GetMetaData(file);
            originalFormat = imageInfo.Format;
            var formatInfo = new GorgonFormatInfo(imageInfo.Format);

            // We absolutely need to have an extension, or else the texconv tool will not work.
            if ((DefaultCodec.CodecCommonExtensions.Count > 0)
                && (!string.Equals(Path.GetExtension(name), DefaultCodec.CodecCommonExtensions[0], System.StringComparison.OrdinalIgnoreCase)))
            {
                _log.Print("Adding DDS extension to working file or else external tools may not be able to read it.", LoggingLevel.Verbose);
                name = Path.ChangeExtension(name, DefaultCodec.CodecCommonExtensions[0]);
            }

            _log.Print($"Copying content file {name} to {ScratchArea.FileSystem.MountPoints.First().PhysicalPath} as working file...", LoggingLevel.Intermediate);

            // Copy to a working file.
            using (Stream outStream = ScratchArea.OpenStream(name, FileMode.Create))
            {
                file.CopyTo(outStream);
                workFile = ScratchArea.FileSystem.GetFile(name);
            }

            _log.Print($"{workFile.FullPath} is now the working file for the image editor.", LoggingLevel.Intermediate);

            if (formatInfo.IsCompressed)
            {
                _log.Print($"Image is compressed using [{formatInfo.Format}] as its pixel format.", LoggingLevel.Intermediate);

                if (_compressor == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Loading image '{workFile.FullPath}'...", LoggingLevel.Simple);
                result = _compressor.Decompress(ref workFile, imageInfo);

                if (result == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Loaded compressed ([{formatInfo.Format}]) image data as [{result.Format}]", LoggingLevel.Intermediate);
            }
            else
            {
                _log.Print($"Loading image '{workFile.FullPath}'...", LoggingLevel.Simple);
                using (Stream workingFileStream = workFile.OpenStream())
                {
                    result = DefaultCodec.LoadFromStream(workingFileStream);
                }
            }

            return (result, workFile, originalFormat);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ImageIO"/> class.</summary>
        /// <param name="defaultCodec">The default codec used by the plug in.</param>
        /// <param name="installedCodecs">The list of installed codecs.</param>
        /// <param name="exportDialog">The dialog service used to export an image.</param>
        /// <param name="busyService">The busy state service.</param>
        /// <param name="scratchArea">The file system writer used to write to the temporary area.</param>
        /// <param name="bcCompressor">The block compressor used to block (de)compress image data.</param>
        /// <param name="log">The logging interface to use.</param>
        public ImageIOService(IGorgonImageCodec defaultCodec, 
            IReadOnlyList<IGorgonImageCodec> installedCodecs,
            IExportImageDialogService exportDialog, 
            IBusyStateService busyService,
            IGorgonFileSystemWriter<Stream> scratchArea, 
            TexConvCompressor bcCompressor, 
            IGorgonLog log)
        {
            _exportDialog = exportDialog;
            DefaultCodec = defaultCodec;
            InstalledCodecs = installedCodecs;
            ScratchArea = scratchArea;
            _compressor = bcCompressor;
            _busyState = busyService;
            _log = log ?? GorgonLog.NullLog;
        }
        #endregion
    }
}
