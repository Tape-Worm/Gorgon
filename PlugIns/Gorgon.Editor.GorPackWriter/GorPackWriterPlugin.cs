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
// Created: October 12, 2018 1:38:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using System.Xml.Linq;
using Gorgon.Editor.GorPackWriter.Properties;
using Gorgon.Editor.Plugins;
using Gorgon.IO;
using Gorgon.Math;
using ICSharpCode.SharpZipLib.BZip2;
using Gorgon.Core;

namespace Gorgon.Editor.GorPackWriterPlugIn
{
    /// <summary>
    /// Gorgon packed file writer plug-in interface.
    /// </summary>
    internal class GorPlugWriterPlugin
        : FileWriterPlugin
	{
        #region Constants.
        // The header for the file.
        private const string FileHeader = "GORPACK1.SharpZip.BZ2";
        #endregion

        #region Variables.
        // The buffer used to write blocks of data (sized to avoid the LOH).
		private byte[] _writeBuffer = new byte[80000];
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the capabilities of the writer.
        /// </summary>
        /// <value>The capabilities.</value>
        public override WriterCapabilities Capabilities => WriterCapabilities.Compression;
		#endregion

		#region Methods.
        /// <summary>
        /// Function to create a new path node.
        /// </summary>
        /// <param name="root">The root directory for the file system.</param>
        /// <param name="directory">Directory to retrieve information from.</param>
        /// <returns>A new node element with the directory information.</returns>
        private static XElement CreatePathNode(DirectoryInfo root, DirectoryInfo directory)
        {
            string virtDirectoryName = directory.ToFileSystemPath(root, Path.DirectorySeparatorChar);
            return new XElement("Path", 
                                new XAttribute("Name", string.Equals(root.FullName, directory.FullName, StringComparison.OrdinalIgnoreCase) ? @"\" : directory.Name), 
                                new XAttribute("FullPath", virtDirectoryName));
        }

        /// <summary>
        /// Function to create a new file node.
        /// </summary>
        /// <param name="file">File to retrieve information from.</param>
        /// <param name="position">Position of the file in the packed data.</param>
        /// <param name="size">Size of the compressed file in the packed data.</param>
		/// <param name="compressedSize">Compressed size of the file.</param>
        /// <returns>A new node element with the file information.</returns>
        private static XElement CreateFileNode(FileInfo file, long position, long size, long compressedSize) => 
                                new XElement("File",
                                new XElement("Filename", Path.GetFileNameWithoutExtension(file.Name)),
                                new XElement("Extension", file.Extension),
                                new XElement("Offset", position),
                                new XElement("Size", size),
                                new XElement("CompressedSize", compressedSize),
                                new XElement("LastModDate", file.LastWriteTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat)),
                                new XElement("FileDate", file.CreationTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat)));

        /// <summary>
        /// Function to compress data.
        /// </summary>
        /// <param name="inStream">Input stream.</param>
        /// <param name="outStream">Output stream.</param>
        /// <param name="compressionRate">The value used to define how well to compress the data.</param>
        /// <param name="token">The token used to cancel the operation.</param>
        private void CompressData(Stream inStream, Stream outStream, int compressionRate, CancellationToken token)
		{
			Debug.Assert(outStream != null, "outStream != null");

			using (var bzStream = new BZip2OutputStream(outStream, compressionRate))
			{
				long streamSize = inStream.Length;
				bzStream.IsStreamOwner = false;

				while (streamSize > 0)
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					int readSize = inStream.Read(_writeBuffer, 0, _writeBuffer.Length);

					if (token.IsCancellationRequested)
					{
						return;
					}

					if (readSize > 0)
					{
						if (token.IsCancellationRequested)
						{
							return;
						}

						bzStream.Write(_writeBuffer, 0, readSize);

						if (token.IsCancellationRequested)
						{
							return;
						}
					}

					streamSize -= readSize;
				}
			}			
		}

        /// <summary>
        /// Function to perform a block copy of a file from stream to another.
        /// </summary>
        /// <param name="inStream">The input stream to copy.</param>
        /// <param name="outStream">The output stream to write into.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        private void BlockCopyStream(FileStream inStream, Stream outStream, CancellationToken cancelToken)
        {
            long inSize = inStream.Length;
            long blockSize = 1048000L.Min(inSize);
            inStream.Position = 0;

            // Copy in ~1MB chunks.
            while (inSize > 0)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                inStream.CopyToStream(outStream, (int)blockSize);
                inSize -= blockSize;
                blockSize = 1048000L.Min(inSize);
            }
        }

        /// <summary>
        /// Function to compress a file into the file system data blob.
        /// </summary>
        /// <param name="output">The output stream to the file system data blob.</param>
        /// <param name="file">The file to compress.</param>
        /// <param name="compressionRate">The value used to define how well to compress the data.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns>The size of the compressed file, in bytes. Or 0, if the file could not be compressed.</returns>
        private long CompressFile(FileStream output, FileInfo file, int compressionRate, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return 0;
            }

            long compressSize = 0;
            long compressStart = output.Position;

            string outDir = Path.GetDirectoryName(output.Name).FormatDirectory(Path.DirectorySeparatorChar);
            string filePath = Path.Combine(outDir, Guid.NewGuid().ToString("N"));
            FileStream outputFile = null;
            FileStream fileStream = null;

            try
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return 0;
                }

                outputFile = File.OpenWrite(filePath);
                fileStream = file.OpenRead();

                CompressData(fileStream, outputFile, compressionRate, cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    return 0;
                }

                fileStream?.Dispose();

                compressSize = outputFile.Length;
                outputFile?.Dispose();

                if (compressSize >= file.Length)
                {
                    fileStream = file.OpenRead();
                    compressSize = 0;
                }
                else
                {
                    fileStream = File.OpenRead(filePath);
                }

                BlockCopyStream(fileStream, output, cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    return 0;
                }
            }
            finally
            {
                outputFile?.Dispose();
                fileStream?.Dispose();

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return compressSize;            
        }

        /// <summary>
        /// Function to compress the files in a given directory.
        /// </summary>
        /// <param name="output">The output data stream.</param>
        /// <param name="workspace">The workspace directory.</param>
        /// <param name="progressCallback">The method used to report progress back to the application.</param>        
        /// <param name="fat">File layout for the file system.</param>
        /// <param name="compressionRate">The value to define how well to compress the data.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        private void CompressFiles(FileStream output, DirectoryInfo workspace, Action<int, int, bool> progressCallback, XElement fat, int compressionRate, CancellationToken cancelToken)
        {
            var subDirs = new Queue<DirectoryInfo>();
            subDirs.Enqueue(workspace);
            var parentNodes = new Queue<XElement>();
            parentNodes.Enqueue(fat);
            int totalFileCount = workspace.GetFiles("*", SearchOption.AllDirectories).Length;
            int fileCount = 0;
            long fileOffset = output.Position;

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            while (subDirs.Count > 0)
            {
                DirectoryInfo current = subDirs.Dequeue();
                XElement parentNode = parentNodes.Dequeue();
                XElement dirPathNode = CreatePathNode(workspace, current);

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                FileInfo[] files = current.GetFiles("*", SearchOption.TopDirectoryOnly);
                                
                foreach (FileInfo file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    long compressedAmount = CompressFile(output, file, compressionRate, cancelToken);

                    XElement fileNode = CreateFileNode(file, fileOffset, file.Length, compressedAmount);
                    dirPathNode.Add(fileNode);

                    fileOffset = output.Position;
                    
                    progressCallback?.Invoke(++fileCount, totalFileCount, true);
                }

                parentNode.Add(dirPathNode);

                DirectoryInfo[] childDirs = current.GetDirectories("*", SearchOption.TopDirectoryOnly)
                                                .Where(item => ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((item.Attributes & FileAttributes.System) != FileAttributes.System))
                                                .ToArray();

                foreach (DirectoryInfo child in childDirs)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    subDirs.Enqueue(child);
                    parentNodes.Enqueue(dirPathNode);
                }              
            }
        }

        /// <summary>
        /// Function to build the file system layout via an XML document.
        /// </summary>
        /// <param name="tempFile">The temporary file.</param>
        /// <param name="workspace">The workspace directory.</param>
        /// <param name="progressCallback">The method used to report progress back to the application.</param>
        /// <param name="cancelToken">The cancel token used to cancel the operation.</param>
        /// <returns>The XML documentation containing the file system layout, or <b>null</b> if cancelled.</returns>
        private XDocument BuildFileSystemLayout(FileInfo tempFile, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            // Initialize our file allocation table.
            var fat = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), 
                                 new XElement("FileSystem",
                                     new XElement("Header", "GORFS1.0")));

			if (cancelToken.IsCancellationRequested)
			{
				return null;
			}
                       
            // Create a temporary blob file to hold our compressed data.
            using (FileStream outputFile = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Get all files in the scratch file system.
                // The compression rate for BZip2 goes from 0 to 9, where 9 is the best, but slowest compression.
                CompressFiles(outputFile, workspace, progressCallback, fat.Element("FileSystem"), (int)(Compression * 9.0f).FastFloor(), cancelToken);
            }

            return fat;
        }

        /// <summary>
        /// Function to determine if the type of file specified can be written by this plug in.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns><b>true</b> if the writer can write the type of file, or <b>false</b> if it cannot.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="file" /> parameter is never <b>null</b>, and is guaranteed to exist when this method is called. If neither of these are true, then this method is not called.
        /// </para>
        /// </remarks>
        protected override bool OnEvaluateCanWriteFile(FileInfo file)
        {
            using (FileStream stream = file.OpenRead())
            using (var memStream = new MemoryStream())
            {
                int byteCount = Encoding.UTF8.GetByteCount(FileHeader) + 5; // Plus string length bytes (potentially up to 4), plus byte order mark.

                if (stream.Length <= byteCount)
                {
                    return false;
                }

                stream.CopyToStream(memStream, byteCount);
                string header = memStream.ReadString();
                return string.Equals(header, FileHeader, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Function to write the file to the specified path.
        /// </summary>
        /// <param name="stream">The stream to save the data into.</param>
        /// <param name="workspace">The directory that represents the workspace for our file system data.</param>
        /// <param name="progressCallback">The method used to report progress back to the application.</param>
        /// <param name="cancelToken">The token used for cancelling the operation.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progressCallback"/> is a method that takes 3 parameters:
        /// <list type="number">
        /// <item>
        ///     <description>The current item number being written.</description>
        /// </item>
        /// <item>
        ///     <description>The total number of items to write.</description>
        /// </item>
        /// <item>
        ///     <description><b>true</b> if the operation can be cancelled, or <b>false</b> if not.</description>
        /// </item>
        /// </list>
        /// This progress method is optional, and if <b>null</b> is passed, then no progress is reported.
        /// </para>
        /// </remarks>
        protected override void OnWrite(Stream stream, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            // We will dump our file data into a temporary work space.
            var tempFolderPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

            if (!tempFolderPath.Exists)
            {
                tempFolderPath.Create();
                tempFolderPath.Refresh();
            }

            var tempFile = new FileInfo(Path.Combine(tempFolderPath.FullName, "working_file"));
            var writer = new GorgonBinaryWriter(stream, true);
            FileStream reader = null;
            var fatStream = new MemoryStream();
            var compressedFatStream = new MemoryStream();

            try
            {
                XDocument fat = BuildFileSystemLayout(tempFile, workspace, progressCallback, cancelToken);

                if ((cancelToken.IsCancellationRequested) || (fat == null))
                {
                    return;
                }

                tempFile.Refresh();

                reader = tempFile.OpenRead();
                writer.Write(FileHeader);                

                // Copy file layout.
                byte[] fatData = Encoding.UTF8.GetBytes(fat.ToStringWithDeclaration());
                fatStream = new MemoryStream(fatData);
                compressedFatStream = new MemoryStream();

                long streamStart = stream.Position;
                CompressData(fatStream, compressedFatStream, (int)(Compression * 9).FastFloor(), cancelToken);
                writer.Write((int)compressedFatStream.Length);

                compressedFatStream.Position = 0;
                compressedFatStream.CopyToStream(stream, (int)compressedFatStream.Length);

                // Copy temp file into our final file.
                BlockCopyStream(reader, stream, cancelToken);
            }
            finally
            {
                compressedFatStream?.Dispose();
                fatStream.Dispose();
                writer.Dispose();
                reader?.Dispose();

                if (tempFile.Exists)
                {
                    tempFile.Delete();
                }

                if (tempFolderPath.Exists)
                {
                    tempFolderPath.Delete(true);
                }
            }
        }        
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorPlugWriterPlugin"/> class.
        /// </summary>
        public GorPlugWriterPlugin()
            : base(Resources.GORPKW_DESC, new[] { new GorgonFileExtension("gorPack", Resources.GORPKW_GORPACK_FILE_EXT_DESC) } )
        {
        }
        #endregion
    }
}
