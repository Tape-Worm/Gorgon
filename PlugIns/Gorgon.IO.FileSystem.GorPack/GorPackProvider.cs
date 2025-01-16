
// 
// Gorgon
// Copyright (C) 2011 Michael Winsor
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
// Created: Sunday, July 03, 2011 9:16:14 AM
// 

using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Providers.GorPack;
using Gorgon.IO.FileSystem.Providers.Properties;
using ICSharpCode.SharpZipLib.BZip2;
using Microsoft.IO;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A file system provider for Gorgon BZip2 compressed packed files
/// </summary>
/// <remarks>
/// The BZip2 compressed pack files are written by an older (1.x) version of Gorgon.  This provider will enable the new file system interface to be able to read these files
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorPackProvider"/> class.
/// </remarks>
/// <inheritdoc/>
internal class GorPackProvider(IGorgonLog log)
        : GorgonFileSystemProvider(Resources.GORFS_GORPACK_DESC, log)
{
    /// <summary>
    /// The pack file header.
    /// </summary>
    public const string GorPackHeader = "GORPACK1.SharpZip.BZ2";

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, GorgonFileExtension> PreferredExtensions
    {
        get;
    } = new GorgonFileExtensionCollection
    {
         new GorgonFileExtension("gorPack", Resources.GORFS_GORPACK_FILE_DESC)
    };

    /// <inheritdoc/>
    public override char PhysicalPathSeparator => '/';

    /// <summary>
    /// Function to decompress a data block.
    /// </summary>
    /// <param name="data">Data to decompress.</param>
    /// <returns>The uncompressed data memory stream.</returns>
    private RecyclableMemoryStream Decompress(byte[] data)
    {
        using MemoryStream sourceStream = MemoryStreamManager.GetStream(data);
        RecyclableMemoryStream decompressedStream = MemoryStreamManager.GetStream();
        BZip2.Decompress(sourceStream, decompressedStream, false);
        return decompressedStream;
    }

    /// <summary>
    /// Function to enumerate the available directories stored in the packed file.
    /// </summary>
    /// <param name="index">The XML file containing the index of files and directories.</param>
    /// <param name="physicalFile">The physical path to the packed file.</param>
    /// <param name="physicalMountPoint">The physical mount point path.</param>
    /// <param name="virtualRootDirectory">The mount point to map into.</param>
    /// <returns>A read only list of directory paths mapped to the virtual file system.</returns>
    private List<(string, string)> EnumerateDirectories(XDocument index, ReadOnlySpan<char> physicalFile, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        List<(string, string)> result = [];
        IEnumerable<XElement> directories = index.Descendants("Path");

        foreach (XElement directoryNode in directories)
        {
            XAttribute pathAttrib = directoryNode.Attribute("FullPath") ?? throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            ReadOnlySpan<char> path = pathAttrib.Value ?? string.Empty;

            if (path.IsEmpty)
            {
                throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            }

            // Add the directory.
            ReadOnlySpan<char> virtualPath = path.FormatDirectory(PhysicalPathSeparator);
            string fullPhysicalPath = string.Concat(physicalFile, "::", virtualPath);
            string fullVirtualPath = OnMapToVirtualPath(fullPhysicalPath, physicalMountPoint, virtualRootDirectory).FormatDirectory(GorgonFileSystem.DirectorySeparator).ToString();

            result.Add((fullPhysicalPath, fullVirtualPath));
        }

        return result;
    }

    /// <summary>
    /// Function to enumerate the available files stored in the packed file.
    /// </summary>
    /// <param name="index">The XML file containing the index of files and directories.</param>
    /// <param name="offset">The offset into the physical file.</param>
    /// <param name="physicalLocation">Physical location of the packed file.</param>
    /// <param name="physicalMountPoint">The physical mount point.</param>
    /// <param name="virtualRootDirectory">The mount point to map into.</param>
    /// <returns>A read only list of <see cref="IGorgonPhysicalFileInfo"/> objects mapped to the virtual file system.</returns>
    private List<IGorgonPhysicalFileInfo> EnumerateFiles(XDocument index, long offset, ReadOnlySpan<char> physicalLocation, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        IEnumerable<XElement> files = index.Descendants("File");
        List<IGorgonPhysicalFileInfo> result = [];

        foreach (XElement file in files)
        {
            XElement fileNameNode = file.Element("Filename") ?? throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            XElement fileOffsetNode = file.Element("Offset") ?? throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            XElement fileSizeNode = file.Element("Size") ?? throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            XElement fileDateNode = file.Element("FileDate") ?? throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            XElement? fileExtensionNode = file.Element("Extension");
            XElement? fileCompressedSizeNode = file.Element("CompressedSize");
            XElement? fileLastModNode = file.Element("LastModDate");
            string parentDirectoryPath = file.Parent?.Attribute("FullPath")?.Value ?? string.Empty;

            // We need these nodes.
            if (((string.IsNullOrWhiteSpace(fileNameNode.Value)) && (string.IsNullOrWhiteSpace(fileExtensionNode?.Value)))
                || (string.IsNullOrWhiteSpace(fileDateNode.Value))
                || (string.IsNullOrWhiteSpace(parentDirectoryPath)))
            {
                throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            }

            ReadOnlySpan<char> fileNameString = fileNameNode.Value;
            ReadOnlySpan<char> fileExtensionString = fileExtensionNode?.Value ?? string.Empty;
            ReadOnlySpan<char> fileDateString = fileDateNode.Value;
            ReadOnlySpan<char> fileLastModString = fileLastModNode?.Value ?? string.Empty;
            ReadOnlySpan<char> fileOffsetString = fileOffsetNode.Value;
            ReadOnlySpan<char> fileSizeString = fileSizeNode.Value;
            ReadOnlySpan<char> fileCompressedSizeString = fileCompressedSizeNode?.Value ?? string.Empty;

            // If we don't have a creation date, then don't allow the file to be processed.
            if (!DateTime.TryParse(fileDateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
            {
                throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            }

            if ((fileLastModString.IsEmpty) || (!DateTime.TryParse(fileLastModString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastModDate)))
            {
                lastModDate = fileDate;
            }

            if (!long.TryParse(fileOffsetString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileOffset))
            {
                throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            }

            fileOffset += offset;

            if (!long.TryParse(fileSizeString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileSize))
            {
                throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            }

            string fileName = fileNameString.IsEmpty ? fileExtensionString.ToString()
                                                     : string.Concat(fileNameString, fileExtensionString);

            if (fileName.Length == 0)
            {
                throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
            }

            long? compressedSize = null;

            // If the file is compressed, then add it to a special list.
            if (!fileCompressedSizeString.IsEmpty)
            {
                if (!long.TryParse(fileCompressedSizeString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long compressed))
                {
                    throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
                }

                if (compressed > 0)
                {
                    compressedSize = compressed;
                }
            }

            string physical = string.Concat(physicalLocation, "::",
                                            parentDirectoryPath.AsSpan().FormatDirectory(GorgonFileSystem.DirectorySeparator),
                                            fileName.AsSpan());

            result.Add(new GorPackPhysicalFileInfo(physical,
                                                   fileName,
                                                   fileDate,
                                                   lastModDate,
                                                   fileOffset,
                                                   fileSize,
                                                   OnMapToVirtualPath(physical, physicalMountPoint, virtualRootDirectory).ToString(),
                                                   compressedSize));
        }

        return result;
    }

    /// <inheritdoc>
    protected override GorgonPhysicalFileSystemData OnEnumerate(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        physicalPath = physicalPath.FormatPath(Path.DirectorySeparatorChar);

        using BinaryReader reader = new(File.Open(physicalPath.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read));
        // Skip the header.
        string header = reader.ReadString();

        if (!string.Equals(header, GorPackHeader, StringComparison.Ordinal))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORFS_GORPACK_ERR_NOT_GORPACK_FILE, physicalPath.ToString()));
        }

        int indexLength = reader.ReadInt32();

        using RecyclableMemoryStream indexData = Decompress(reader.ReadBytes(indexLength));
        string xmlData = Encoding.UTF8.GetString(indexData.GetReadOnlySequence());
        XDocument index = XDocument.Parse(xmlData, LoadOptions.None);

        return new GorgonPhysicalFileSystemData(EnumerateDirectories(index, physicalPath, physicalMountPoint, virtualRootDirectory),
                                                EnumerateFiles(index, reader.BaseStream.Position, physicalPath, physicalMountPoint, virtualRootDirectory));
    }

    /// <inheritdoc/>
    protected override ReadOnlySpan<char> OnMapToVirtualPath(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        physicalMountPoint = string.Concat(physicalMountPoint.FormatPath(Path.DirectorySeparatorChar), "::/");

        // If the physical path is not in the root of the physical mount point, then we can't map it.
        if (!physicalPath.StartsWith(physicalMountPoint, StringComparison.OrdinalIgnoreCase))
        {
            Log.PrintError($"The physical path '{physicalPath}' is not under the physical mount point path '{physicalMountPoint}'. This path cannot be mapped.", LoggingLevel.Verbose);
            return [];
        }

        // Strip off the physical mount point part of the path.
        physicalPath = physicalPath[physicalMountPoint.Length..];
        char[] buffer = new char[virtualRootDirectory.Length + physicalPath.Length];
        Span<char> result = buffer.AsSpan();

        virtualRootDirectory.CopyTo(result);
        physicalPath.CopyTo(result[virtualRootDirectory.Length..]);

        return result;
    }

    /// <inheritdoc/>
    protected override ReadOnlySpan<char> OnMapToPhysicalPath(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        if (virtualPath[0] == GorgonFileSystem.DirectorySeparator)
        {
            return physicalMountPoint.FormatPath(Path.DirectorySeparatorChar);
        }

        if (!physicalMountPoint.EndsWith("::", StringComparison.OrdinalIgnoreCase))
        {
            physicalMountPoint = string.Concat(physicalMountPoint, "::");
        }

        physicalMountPoint = physicalMountPoint.FormatPath(Path.DirectorySeparatorChar);

        int stringSize = (virtualPath.Length - virtualRootDirectory.Length) + physicalMountPoint.Length;
        char[] buffer = new char[stringSize];
        Span<char> result = buffer.AsSpan();
        ReadOnlySpan<char> trimmedPath = virtualPath[virtualRootDirectory.Length..];

        physicalMountPoint.CopyTo(result);
        trimmedPath.CopyTo(result[physicalMountPoint.Length..]);

        return result;
    }

    /// <inheritdoc/>
    protected override GorgonFileSystemStream? OnOpenReadFileStream(IGorgonPhysicalFileInfo fileInfo, ReadOnlySpan<char> physicalMountPoint, Action<string, string>? onCloseCallback)
    {
        string path = physicalMountPoint.FormatPath(Path.DirectorySeparatorChar).ToString();

        if (!File.Exists(path))
        {
            return null;
        }

        return new GorPackFileStream(fileInfo, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), onCloseCallback);
    }

    /// <inheritdoc/>
    protected override bool OnCanReadFileSystem(ReadOnlySpan<char> physicalPath)
    {
        physicalPath = physicalPath.FormatPath(Path.DirectorySeparatorChar);

        using BinaryReader reader = new(File.Open(physicalPath.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read));

        // If the length of the stream is less or equal to the header size, it's unlikely that we can read this file.
        if (reader.BaseStream.Length <= GorPackHeader.Length)
        {
            return false;
        }

        return string.Equals(reader.ReadString(), GorPackHeader, StringComparison.Ordinal);
    }
}
