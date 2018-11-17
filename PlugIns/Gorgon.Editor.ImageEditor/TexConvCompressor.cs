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
// Created: November 12, 2018 10:23:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The texture conversion functionality for the external texconv process.
    /// </summary>
    internal class TexConvCompressor
    {
        #region Variables.
        // The path to the texture converter process.
        private FileInfo _texConv;
        // The file system writer to use.
        private IGorgonFileSystemWriter<Stream> _writer;
        // The temporary directory that will contain the files.
        private IGorgonVirtualDirectory _tempDir;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        public IGorgonImage Decompress(IContentFile contentFile, IGorgonImageCodec codec)
        {
            // If we pass a codec that doesn't support block compression, then just get out and return the image as-is.
            if (!codec.SupportsBlockCompression)
            {
                using (Stream inStream = contentFile.OpenRead())
                {
                    return codec.LoadFromStream(inStream);
                }
            }

            // Copy the file to a working location.
            string filePath = _tempDir.FullPath + Path.ChangeExtension(contentFile.Name, "tmp");

            using (Stream inStream = contentFile.OpenRead())
            using (Stream outStream = _writer.OpenStream(filePath, FileMode.Create))
            {
                inStream.CopyTo(outStream);
            }

            IGorgonVirtualFile file = _writer.FileSystem.GetFile(filePath);
            var physicalFile = new FileInfo(file.PhysicalFile.FullPath);

            using (Stream inStream = file.OpenStream())
            {
                IGorgonImageInfo imageInfo = codec.GetMetaData(inStream);
            }
            /*
                        var info = new ProcessStartInfo
                        {
                            Arguments =
                                                           "-f " + _d3dFormats[compressionFormat] + " -m " + mipLevels + " -fl " +
                                                           (Graphics.VideoDevice.SupportedFeatureLevel > DeviceFeatureLevel.SM4_1 ? "11.0" : "10.0") + " -ft DDS -o \"" +
                                                           Path.GetDirectoryName(tempFilePath) + "\" -nologo -px decomp_ \"" + tempFilePath + "\"",
                            ErrorDialog = true,                
                            FileName = _texConv.FullName,
                            WorkingDirectory = 
                            UseShellExecute = false,
            #if DEBUG
                            CreateNoWindow = false,
            #else
                                           CreateNoWindow = true,
            #endif
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        };

                        using (var texConvProcess = Process.Start(info))
                        {
                            if (texConvProcess == null)
                            {
                                return null;
                            }

                            texConvProcess.WaitForExit();
                        }

                        // Refresh the file system since the texture converter process is an outside application.
                        _tempDir.FileSystem.Refresh();*/

            return null;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the TexConvCompressor class.</summary>
        /// <param name="texConvFile">The tex conv file.</param>
        /// <param name="scratchWriter">The scratch writer.</param>
        /// <param name="tempDir">The temporary dir.</param>
        public TexConvCompressor(FileInfo texConvFile, IGorgonFileSystemWriter<Stream> scratchWriter, IGorgonVirtualDirectory tempDir)
        {

        }
        #endregion
    }
}
