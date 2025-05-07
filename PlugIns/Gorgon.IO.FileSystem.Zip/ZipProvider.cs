
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: Monday, June 27, 2011 9:33:12 AM
// 

using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Providers.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A file system provider for zip files
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ZipProvider"/> class.
/// </remarks>
internal class ZipProvider(IGorgonLog log)
        : GorgonFileSystemProvider(Resources.GORFS_ZIP_DESC, log)
{
    /// <summary>
    /// Header bytes for a zip file.
    /// </summary>
    public static IEnumerable<byte> ZipHeader = [0x50, 0x4B, 0x3, 0x4];

    /// <inheritdoc/>
    public override char PhysicalPathSeparator => '/';

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, GorgonFileExtension> PreferredExtensions
    {
        get;
    } = new GorgonFileExtensionCollection
    {
        new GorgonFileExtension("Zip", Resources.GORFS_ZIP_FILE_DESC)
    };

    /// <inheritdoc/>
    protected override GorgonPhysicalFileSystemData OnEnumerate(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        List<(string physicalPath, string virtualPath)> directories = [];
        List<IGorgonPhysicalFileInfo> files = [];
        string pathSep = PhysicalPathSeparator.ToString();

        using ZipInputStream zipStream = new(File.Open(physicalMountPoint.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read));

        ZipEntry entry;

        while ((entry = zipStream.GetNextEntry()) is not null)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                Log.PrintError("An entry in the zip file has no name. This entry will be skipped.", LoggingLevel.Verbose);
                continue;
            }

            string directoryPath;
            ReadOnlySpan<char> entryName = entry.Name.AsSpan().FormatPath(PhysicalPathSeparator);
            ReadOnlySpan<char> directoryName = Path.GetDirectoryName(entryName).FormatDirectory(PhysicalPathSeparator);

            if (directoryName.IsEmpty)
            {
                directoryPath = virtualRootDirectory.ToString();
            }
            else
            {
                directoryPath = string.Concat(virtualRootDirectory, directoryName);
            }

            string physicalDirectoryPath = string.Concat(physicalMountPoint, "::", pathSep, directoryName);

            if (!directories.Contains((physicalDirectoryPath, directoryPath)))
            {
                directories.Add((physicalDirectoryPath, directoryPath));
            }

            if (!entry.IsDirectory)
            {
                files.Add(new ZipPhysicalFileInfo(string.Concat(physicalMountPoint, "::", pathSep, entryName),
                                                       Path.GetFileName(entryName).ToString(),
                                                       entry.DateTime,
                                                       entry.Size,
                                                       string.Concat(virtualRootDirectory, entryName),
                                                       entry.CompressedSize));
            }
        }

        return new GorgonPhysicalFileSystemData(directories, files);
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

        return new ZipFileStream(fileInfo, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), onCloseCallback);
    }

    /// <inheritdoc/>
    protected override bool OnCanReadFileSystem(ReadOnlySpan<char> physicalPath)
    {
        physicalPath = physicalPath.FormatPath(Path.DirectorySeparatorChar);

        byte[] headerBytes = new byte[4];

        using FileStream stream = File.Open(physicalPath.ToString(), FileMode.Open, FileAccess.Read, FileShare.Read);

        if (stream.Length <= headerBytes.Length)
        {
            return false;
        }

        stream.Read(headerBytes, 0, headerBytes.Length);

        return headerBytes.SequenceEqual(ZipHeader);
    }
}
