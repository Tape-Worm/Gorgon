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
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.ImageEditor.Services
{
    /// <summary>
    /// An image importer that reads in an image file, and converts it into a DDS format image prior to import into the application.
    /// </summary>
    internal class DdsImageImporter
        : IEditorContentImporter
    {
        #region Variables.
        // The log used for debug message logging.
        private readonly IGorgonLog _log;
        // The codec for the input file.
        private readonly IGorgonImageCodec _codec;
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
            var ddsCodec = new GorgonCodecDds();

            _log.Print($"Importing file '{SourceFile.FullName}' (Codec: {_codec.Name})...", LoggingLevel.Verbose);
            using (IGorgonImage image = _codec.LoadFromFile(SourceFile.FullName))
            {
                var tempFile = new FileInfo(Path.Combine(temporaryDirectory.FullName, Path.GetFileNameWithoutExtension(SourceFile.Name)));

                _log.Print($"Converting '{SourceFile.FullName}' to DDS file format. Image format [{image.Format}].", LoggingLevel.Verbose);
                ddsCodec.SaveToFile(image, tempFile.FullName);

                return tempFile;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.Services.DdsImageImporter"/> class.</summary>
        /// <param name="log">The log used for logging debug messages.</param>
        public DdsImageImporter(FileInfo sourceFile, IGorgonImageCodec codec, IGorgonLog log)
        {
            _log = log ?? GorgonLog.NullLog;
            SourceFile = sourceFile;
            _codec = codec;
        }
        #endregion
    }
}
