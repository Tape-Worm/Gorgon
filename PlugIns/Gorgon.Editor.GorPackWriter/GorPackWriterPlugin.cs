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
using Gorgon.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Gorgon.Editor.GorPackWriterPlugIn
{
    /// <summary>
    /// Gorgon packed file writer plug-in interface.
    /// </summary>
    internal class GorPackWriterPlugin
        : FileWriterPlugin
	{
        #region Constants.
        // The header for the file.
        private const string FileHeader = "GORPACK1.SharpZip.BZ2";
        // The type name for v2 of the Gorgon file writer plugin.
        private const string EquivV2Plugin = "GorgonLibrary.Editor.GorPackWriterPlugIn.GorgonGorPackWriterPlugIn";
        #endregion

        #region Variables.
        // Computer information used to determine how to best compress data.
        private readonly IGorgonComputerInfo _computerInfo = new GorgonComputerInfo();
        #endregion

        #region Properties.
        /// <summary>Property to return the equivalent type name for v2 of the Gorgon file writer plugin.</summary>
        /// <remarks>This is here to facilitate importing of file metadata from v2 of the gorgon editor files. Only specify a compatible type here, otherwise things will go wrong.</remarks>
        public override string V2PluginName => EquivV2Plugin;

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

            byte[] writeBuffer = new byte[81920];

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

					int readSize = inStream.Read(writeBuffer, 0, writeBuffer.Length);

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

						bzStream.Write(writeBuffer, 0, readSize);

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
        /// Function to build up the file layout XML data.
        /// </summary>
        /// <param name="workspace">The work space directory.</param>
        /// <param name="fatRoot">The root of the layout XML data.</param>
        /// <returns>A list of all elements that represent files for compression.</returns>
        private List<(XElement fileNode, FileInfo physicalPath)> BuildFatXml(DirectoryInfo workspace, XElement fatRoot, CancellationToken cancelToken)
        {
            var subDirs = new Queue<DirectoryInfo>();
            subDirs.Enqueue(workspace);
            var parentNodes = new Queue<XElement>();
            parentNodes.Enqueue(fatRoot);
            int totalFileCount = workspace.GetFiles("*", SearchOption.AllDirectories).Length;
            var fileNodes = new List<(XElement fileNode, FileInfo physicalPath)>();

            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            while (subDirs.Count > 0)
            {
                DirectoryInfo current = subDirs.Dequeue();
                XElement parentNode = parentNodes.Dequeue();
                XElement dirPathNode = CreatePathNode(workspace, current);

                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                FileInfo[] files = current.GetFiles("*", SearchOption.TopDirectoryOnly);

                foreach (FileInfo file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    XElement fileNode = CreateFileNode(file, 0, file.Length, 0);
                    dirPathNode.Add(fileNode);
                    fileNodes.Add((fileNode, file));
                }

                parentNode.Add(dirPathNode);

                DirectoryInfo[] childDirs = current.GetDirectories("*", SearchOption.TopDirectoryOnly)
                                                .Where(item => ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((item.Attributes & FileAttributes.System) != FileAttributes.System))
                                                .ToArray();

                foreach (DirectoryInfo child in childDirs)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    subDirs.Enqueue(child);
                    parentNodes.Enqueue(dirPathNode);
                }
            }

            return fileNodes;
        }

        /// <summary>
        /// Function to perform a block copy of a file from stream to another.
        /// </summary>
        /// <param name="inStream">The input stream to copy.</param>
        /// <param name="outStream">The output stream to write into.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        private void BlockCopyStream(Stream inStream, Stream outStream, CancellationToken cancelToken)
        {
            long inSize = inStream.Length;
            long blockSize = 1048000L.Min(inSize);
            byte[] buffer = new byte[81920];
            inStream.Position = 0;

            // Copy in ~1MB chunks.
            while (inSize > 0)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                inStream.CopyToStream(outStream, (int)blockSize, buffer);
                inSize -= blockSize;
                blockSize = 1048000L.Min(inSize);
            }
        }

        /// <summary>
        /// Function to compress a file into the file system data blob.
        /// </summary>
        /// <param name="outDirectory">The directory used to temporarily store the compressed file.</param>
        /// <param name="file">The file to compress.</param>
        /// <param name="compressedSizeNode">The node containing the size of the file, in bytes, when compressed.</param>
        /// <param name="compressionRate">The value used to define how well to compress the data.</param>
        /// <param name="cancelToken">The token used to cancel the process.</param>
        /// <returns>The path to the compressed file.</returns>
        private FileInfo CompressFile(DirectoryInfo outDirectory, FileInfo file, XElement compressedSizeNode, int compressionRate, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            long compressSize = 0;

            string filePath = Path.Combine(outDirectory.FullName, Path.GetFileNameWithoutExtension(file.Name) + Guid.NewGuid().ToString("N") + file.Extension);
            var result = new FileInfo(filePath);
            Stream outputFile = null;
            Stream fileStream = null;            

            try
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                outputFile = result.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

                // Just copy if we have no compression, or if the file is less than 1K - Not much sense to compress something so small.
                if (compressionRate == 0)
                {
                    BlockCopyStream(fileStream, outputFile, cancelToken);
                }
                else
                {
                    CompressData(fileStream, outputFile, compressionRate, cancelToken);
                    compressSize = outputFile.Length;
                }

                outputFile?.Dispose();
                fileStream?.Dispose();

                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }
                
                // If our compression yields a file larger, or the same size as our original file, then just copy it as-is.
                if (compressSize >= file.Length)
                {
                    outputFile = result.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                    fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    compressSize = 0;                    
                    BlockCopyStream(fileStream, outputFile, cancelToken);
                }

                compressedSizeNode.Value = compressSize.ToString(CultureInfo.InvariantCulture);                
            }
            finally
            {
                outputFile?.Dispose();
                fileStream?.Dispose();

                result.Refresh();
            }

            return result;
        }

        /// <summary>
        /// Function to compile all the compressed files into a single data blob.
        /// </summary>
        /// <param name="tempFile">The temporary file representing the data blob.</param>
        /// <param name="files">The list of compressed files to process.</param>
        /// <param name="progressCallback">The method used to report save progress.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private Task CompileFileDataTask(FileInfo tempFile, IEnumerable<(FileInfo file, XElement node)> files, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            // This is the last step, so show a full bar.
            progressCallback?.Invoke(1, 1, true);

            return Task.Run(() =>
                {
                    long offset = 0;

                    using (Stream outputStream = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        foreach ((FileInfo file, XElement node) in files.OrderByDescending(item => item.file.Length))
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                return;
                            }

                            XElement offsetNode = node.Element("Offset");
                            offsetNode.Value = offset.ToString(CultureInfo.InvariantCulture);

                            using (Stream inputStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                BlockCopyStream(inputStream, outputStream, cancelToken);
                            }

                            offset = outputStream.Position;

                            file.Delete();
                        }
                    }
                }, cancelToken);
        }

        /// <summary>
        /// Function to build the file system layout via an XML document.
        /// </summary>
        /// <param name="tempFile">The temporary file.</param>
        /// <param name="workspace">The workspace directory.</param>
        /// <param name="progressCallback">The method used to report progress back to the application.</param>
        /// <param name="cancelToken">The cancel token used to cancel the operation.</param>
        /// <returns>The XML documentation containing the file system layout, or <b>null</b> if cancelled.</returns>
        private async Task<XDocument> BuildFileSystemLayoutAsync(FileInfo tempFile, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
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

            // Build up our file layout XML document, and return all nodes that represent files (for compression purposes).
            List<(XElement fileNode, FileInfo physicalPath)> fileNodes = BuildFatXml(workspace, fat.Element("FileSystem"), cancelToken);

            if ((fileNodes == null) || (cancelToken.IsCancellationRequested))
            {
                return null;
            }

            // The jobs to process.
            var jobs = new List<Task<(XElement, FileInfo)>>();
            int maxJobCount = (Environment.ProcessorCount * 2).Max(1).Min(24);
            Task<(XElement, FileInfo)> deadJob = null;
            var compressedFiles = new ConcurrentBag<(FileInfo, XElement)>();
            int outputIndex = 0;
            int fileCount = 0;
            int totalFileCount = fileNodes.Count;

            // Build a job list of files to compress.
            do
            {
                if (deadJob != null)
                {
                    jobs.Remove(deadJob);
                }

                while ((fileNodes.Count > 0) && (jobs.Count < maxJobCount))
                {
                    outputIndex = fileNodes.Count - 1;
                    (XElement fileNode, FileInfo physicalPath) = fileNodes[outputIndex];
                    fileNodes.RemoveAt(outputIndex);
                    
                    jobs.Add(Task.Run(() =>
                    {
                        int fileIndex = outputIndex;
                        XElement compressedSizeNode = fileNode.Element("CompressedSize");
                        FileInfo compressedFile = CompressFile(tempFile.Directory, physicalPath, compressedSizeNode, (int)(Compression * 9), cancelToken);

                        if ((compressedFile == null) || (cancelToken.IsCancellationRequested))
                        {
                            return (null, null);
                        }

                        if ((compressedFile == null) || (fileNode == null))
                        {
                            Debugger.Break();
                        }
                        
                        compressedFiles.Add((compressedFile, fileNode));

                        Interlocked.Increment(ref fileCount);
                        progressCallback?.Invoke(fileCount, totalFileCount, true);

                        return (fileNode, physicalPath);
                    }, cancelToken));
                }

                if (jobs.Count > 0)
                {
                    deadJob = await Task.WhenAny(jobs);
                    (XElement, FileInfo) jobItem = await deadJob;

                    if (jobItem.Item1 == null)
                    {
                        return null;
                    }                    
                }
            }
            while (jobs.Count > 0);

            // Validate.
            Debug.Assert(compressedFiles.All(item => item.Item1 != null), "File info not found in compression list.");
            Debug.Assert(compressedFiles.All(item => item.Item2 != null), "XElement node not found in compression list.");
            Debug.Assert(compressedFiles.All(item =>
            {
                XElement fileName = item.Item2.Element("Filename");
                return item.Item1.Name.StartsWith(fileName.Value, StringComparison.OrdinalIgnoreCase);
            }), "File node and file info are not the same.");

            await CompileFileDataTask(tempFile, compressedFiles, progressCallback, cancelToken);

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
            using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
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
        /// <param name="file">The file to write into.</param>
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
        protected override async Task OnWriteAsync(FileInfo file, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            // We will dump our file data into a temporary work space.
            var tempFolderPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

            if (!tempFolderPath.Exists)
            {
                tempFolderPath.Create();
                tempFolderPath.Refresh();
            }

            var tempFile = new FileInfo(Path.Combine(tempFolderPath.FullName, "working_file"));
            GorgonBinaryWriter writer = null;
            FileStream inStream = null;
            FileStream outStream = null;
            var fatStream = new MemoryStream();
            var compressedFatStream = new MemoryStream();

            try
            {
                XDocument fat = await BuildFileSystemLayoutAsync(tempFile, workspace, progressCallback, cancelToken);

                if ((cancelToken.IsCancellationRequested) || (fat == null))
                {
                    return;
                }

                tempFile.Refresh();

                // Copy file layout.
                byte[] fatData = Encoding.UTF8.GetBytes(fat.ToStringWithDeclaration());
                fatStream = new MemoryStream(fatData);
                compressedFatStream = new MemoryStream();

                CompressData(fatStream, compressedFatStream, (int)(Compression * 9), cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                // We are now at the point of no return, so we cannot cancel the operation here, else we'll have a corrupt file.
                progressCallback?.Invoke(1, 1, false);

                inStream = tempFile.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                outStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new GorgonBinaryWriter(outStream);
                writer.Write(FileHeader);
                writer.Write((int)compressedFatStream.Length);

                compressedFatStream.Position = 0;
                compressedFatStream.CopyToStream(outStream, (int)compressedFatStream.Length);

                // Copy temp file into our final file.
                BlockCopyStream(inStream, outStream, CancellationToken.None);
            }
            finally
            {
                compressedFatStream?.Dispose();
                fatStream.Dispose();
                outStream?.Dispose();
                writer?.Dispose();
                
                inStream?.Dispose();

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
        /// Initializes a new instance of the <see cref="GorPackWriterPlugin"/> class.
        /// </summary>
        public GorPackWriterPlugin()
            : base(Resources.GORPKW_DESC, new[] { new GorgonFileExtension("gorPack", Resources.GORPKW_GORPACK_FILE_EXT_DESC) } )
        {            
        }
        #endregion
    }
}
