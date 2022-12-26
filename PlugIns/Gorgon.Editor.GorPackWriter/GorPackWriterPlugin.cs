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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Editor.GorPackWriter.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.IO;
using Gorgon.Math;
using Microsoft.IO;

namespace Gorgon.Editor.GorPackWriterPlugIn;

/// <summary>
/// Gorgon packed file writer plug in interface.
/// </summary>
internal class GorPackWriterPlugIn
    : FileWriterPlugIn
{
    #region Constants.
    // The header for the file.
    private const string FileHeader = "GORPACK1.SharpZip.BZ2";
    // The type name for v2 of the Gorgon file writer plugin.
    private const string EquivV2PlugIn = "GorgonLibrary.Editor.GorPackWriterPlugIn.GorgonGorPackWriterPlugIn";
    /// <summary>
    /// The maximum size of a write transfer buffer, in bytes.
    /// </summary>
    public const int MaxBufferSize = 1048575;
    #endregion

    #region Variables.
    // The memory stream manager for efficient memory usage.
    private static readonly RecyclableMemoryStreamManager _memStreamManager = new(1_048_576, 16_777_216);
    // The global buffer used to write out data to a stream.
    private byte[] _globalWriteBuffer;
    #endregion

    #region Properties.
    /// <summary>Property to return the equivalent type name for v2 of the Gorgon file writer plugin.</summary>
    /// <remarks>This is here to facilitate importing of file metadata from v2 of the gorgon editor files. Only specify a compatible type here, otherwise things will go wrong.</remarks>
    public override string V2PlugInName => EquivV2PlugIn;

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
                            new("File",
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
    /// <param name="writeBuffer">The buffer used to transfer data from one stream to another.</param>
    /// <param name="compressionRate">The value used to define how well to compress the data.</param>
    /// <param name="token">The token used to cancel the operation.</param>
    private void CompressData(Stream inStream, Stream outStream, byte[] writeBuffer, int compressionRate, CancellationToken token)
    {
        Debug.Assert(outStream is not null, "outStream is not null");

        using var bzStream = new Ionic.BZip2.ParallelBZip2OutputStream(outStream, compressionRate, true);
        long streamSize = inStream.Length;

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
    /// Function to perform a block copy of a large (> 2GB) file from stream to another.
    /// </summary>
    /// <param name="inStream">The input stream to copy.</param>
    /// <param name="outStream">The output stream to write into.</param>
    /// <param name="writeBuffer">The buffer used to facilitate transfer of data in blocks.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    private void BlockCopyStream(Stream inStream, Stream outStream, byte[] writeBuffer, CancellationToken cancelToken)
    {
        const long maxBlockSize = int.MaxValue - 1;
        long inSize = inStream.Length;

        // If we're under 2GB, then we can copy as-is.
        if (inSize <= maxBlockSize)
        {
            inStream.CopyToStream(outStream, (int)inSize, writeBuffer);
            return;
        }

        // Otherwise, we need to break up the file into 2GB chunks to get around the int32 limitation.
        int blockSize = (int)(maxBlockSize.Min(inSize));
        inStream.Position = 0;

        while (inSize > 0)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            inStream.CopyToStream(outStream, blockSize, writeBuffer);
            inSize -= blockSize;
            blockSize = (int)(maxBlockSize.Min(inSize));
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
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);

        try
        {
            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            outputFile = result.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

            // Just copy if we have no compression, or if the file is less than 4K - Not much sense to compress something so small.
            if ((compressionRate == 0) || (file.Length <= 4096))
            {

                BlockCopyStream(fileStream, outputFile, writeBuffer, cancelToken);
            }
            else
            {
                Debug.Print($"Compressing '{file.FullName}' {file.Length.FormatMemory()}");
                CompressData(fileStream, outputFile, writeBuffer, compressionRate, cancelToken);
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
                BlockCopyStream(fileStream, outputFile, writeBuffer, cancelToken);
            }

            compressedSizeNode.Value = compressSize.ToString(CultureInfo.InvariantCulture);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(writeBuffer, true);

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
    /// <param name="writeBuffer">The write buffer used to transfer data from one stream to the next.</param>
    /// <param name="progressCallback">The method used to report save progress.</param>
    /// <param name="cancelToken">The cancellation token.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private Task CompileFileDataTask(FileInfo tempFile, IEnumerable<(XElement node, FileInfo file)> files, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
    {
        // This is the last step, so show a full bar.
        progressCallback?.Invoke(1, 1, true);

        return Task.Run(() =>
            {
                long offset = 0;
                Stream outputStream = null;

                try
                {
                    outputStream = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                    foreach ((XElement node, FileInfo file) in files.OrderByDescending(item => item.file.Length))
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        XElement offsetNode = node.Element("Offset");
                        offsetNode.Value = offset.ToString(CultureInfo.InvariantCulture);

                        using (Stream inputStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            BlockCopyStream(inputStream, outputStream, _globalWriteBuffer, cancelToken);
                        }

                        offset = outputStream.Position;

                        file.Delete();
                    }
                }
                finally
                {
                    outputStream?.Dispose();
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

        if ((fileNodes is null) || (cancelToken.IsCancellationRequested))
        {
            return null;
        }

        // The jobs to process.            
        int maxJobCount = (Environment.ProcessorCount).Min(16).Max(1);
        int fileCount = 0;
        int totalFileCount = fileNodes.Count;
        int filesPerJob = (int)((float)totalFileCount / maxJobCount).FastCeiling();
        var jobs = new List<Task<CompressJob>>();

        if ((totalFileCount <= 100) || (maxJobCount < 2))
        {
            filesPerJob = totalFileCount;
        }

        // Function to perform the compression on the background thread.
        CompressJob Compressor(CompressJob jobData)
        {
            while (jobData.Files.Count > 0)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                (XElement fileNode, FileInfo physicalPath) = jobData.Files[0];
                jobData.Files.RemoveAt(0);

                XElement compressedSizeNode = fileNode.Element("CompressedSize");
                FileInfo compressedFile = CompressFile(tempFile.Directory,
                                                    physicalPath,
                                                    compressedSizeNode,
                                                    (int)(Compression * 9), // Bzip compression block size is from 0 - 9.
                                                    cancelToken);

                if ((compressedFile is null) || (cancelToken.IsCancellationRequested))
                {
                    return null;
                }

                jobData.CompressedFiles.Add((fileNode, compressedFile));

                progressCallback?.Invoke(Interlocked.Increment(ref fileCount), totalFileCount, true);
            }

            return jobData;
        }

        // Build up the tasks for our jobs.
        while (fileNodes.Count > 0)
        {
            var jobData = new CompressJob();

            // Copy the file information to the compression job data.
            int length = filesPerJob.Min(fileNodes.Count);
            for (int i = 0; i < length; ++i)
            {
                jobData.Files.Add(fileNodes[i]);
            }
            fileNodes.RemoveRange(0, length);

            jobs.Add(Task.Run(() => Compressor(jobData), cancelToken));
        }

        CompressJob[] finishedTasks = await Task.WhenAll(jobs);

        if ((finishedTasks.Length == 0) || (finishedTasks.Any(item => item is null)) || (cancelToken.IsCancellationRequested))
        {
            return null;
        }

        IEnumerable<(XElement node, FileInfo file)> compressedFiles = finishedTasks.SelectMany(item => item.CompressedFiles);
        // Validate.
        Debug.Assert(compressedFiles.All(item => item.node is not null), "File info not found in compression list.");
        Debug.Assert(compressedFiles.All(item => item.file is not null), "XElement node not found in compression list.");
        Debug.Assert(compressedFiles.All(item =>
        {
            XElement fileName = item.node.Element("Filename");
            return item.file.Name.StartsWith(fileName.Value, StringComparison.OrdinalIgnoreCase);
        }), "File node and file info are not the same.");

        await CompileFileDataTask(tempFile, finishedTasks.SelectMany(item => item.CompressedFiles), progressCallback, cancelToken);

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
    protected override bool OnEvaluateCanWriteFile(string file)
    {
        using FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using MemoryStream memStream = _memStreamManager.GetStream();
        int byteCount = Encoding.UTF8.GetByteCount(FileHeader) + 5; // Plus string length bytes (potentially up to 4), plus byte order mark.

        if (stream.Length <= byteCount)
        {
            return false;
        }

        stream.CopyToStream(memStream, byteCount);
        string header = memStream.ReadString();
        return string.Equals(header, FileHeader, StringComparison.Ordinal);
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
    protected override async Task OnWriteAsync(string file, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
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
        MemoryStream fatStream = null;
        MemoryStream compressedFatStream = null;

        _globalWriteBuffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);

        try
        {
            // Allow cancelation immediately.
            progressCallback?.Invoke(0, 1, true);

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            XDocument fat = await BuildFileSystemLayoutAsync(tempFile, workspace, progressCallback, cancelToken);

            if ((cancelToken.IsCancellationRequested) || (fat is null))
            {
                return;
            }

            tempFile.Refresh();

            // Copy file layout to a compressed data block.
            byte[] fatData = Encoding.UTF8.GetBytes(fat.ToStringWithDeclaration());
            fatStream = _memStreamManager.GetStream(fatData);
            compressedFatStream =_memStreamManager.GetStream();

            CompressData(fatStream, compressedFatStream, _globalWriteBuffer, (int)(Compression * 9), cancelToken);

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            // We are now at the point of no return, so we cannot cancel the operation here, else we'll have a corrupt file.
            progressCallback?.Invoke(1, 1, false);

            inStream = tempFile.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            outStream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None);
            writer = new GorgonBinaryWriter(outStream);
            writer.Write(FileHeader);
            writer.Write((int)compressedFatStream.Length);

            compressedFatStream.Position = 0;
            compressedFatStream.CopyToStream(outStream, (int)compressedFatStream.Length, _globalWriteBuffer);

            // Copy temp file into our final file.
            BlockCopyStream(inStream, outStream, _globalWriteBuffer, CancellationToken.None);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(_globalWriteBuffer, true);

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
    /// Initializes a new instance of the <see cref="GorPackWriterPlugIn"/> class.
    /// </summary>
    public GorPackWriterPlugIn()
        : base(Resources.GORPKW_DESC, new[] { new GorgonFileExtension("gorPack", Resources.GORPKW_GORPACK_FILE_EXT_DESC) })
    {
    }
    #endregion
}
