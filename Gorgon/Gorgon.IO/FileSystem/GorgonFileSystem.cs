
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
// Created: Monday, June 27, 2011 8:54:59 AM
// 

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Properties;
using Gorgon.IO.FileSystem.Providers;
using Gorgon.Memory;

namespace Gorgon.IO.FileSystem;

/// <inheritdoc cref="IGorgonFileSystem"/>
public class GorgonFileSystem
    : IGorgonFileSystem
{
    // The maximum size for the working buffers.
    private const int MaxBufferSize = 262_144;

    // The character used to separate directory names and file names in paths.
    private const string DirectorySeparatorString = "/";

    /// <summary>
    /// The character used to separate directory names and file names in paths.
    /// </summary>
    public const char DirectorySeparator = '/';

    /// <summary>
    /// The character used to separate directory names and file names in paths.
    /// </summary>
    /// <remarks>
    /// This is used in places where ReadOnlySpan&lt;char&gt; is required.
    /// </remarks>
    internal static readonly char[] DirectorySeparatorList = [DirectorySeparator];

    // The log file for the application.
    private readonly IGorgonLog _log;
    // A list of mount points grouped by provider.
    private readonly List<GorgonFileSystemMountPoint> _mountPoints;
    // A read/write version of the RootDirectory property.
    private readonly VirtualDirectory _rootDirectory;
    // The default file system provider.
    private readonly FolderFileSystemProvider _defaultProvider;
    // The default writable file system provider.
    private readonly IGorgonFileSystemWriteProvider _writeProvider;
    // The mount point for the writable area.
    private GorgonFileSystemMountPoint _writeMountPoint = GorgonFileSystemMountPoint.Empty;

    // The event handler for the directory created event.
    private EventHandler<VirtualDirectoryCreatedEventArgs>? _directoryCreated;
    // The event handler for the directory deleted event.
    private EventHandler<VirtualDirectoryDeletedEventArgs>? _directoryDeleted;
    // The event handler for the directory renamed event.
    private EventHandler<VirtualDirectoryRenamedEventArgs>? _directoryRenamed;
    // The event handler for the directory copied event.
    private EventHandler<VirtualDirectoryCopiedMovedEventArgs>? _directoryCopied;
    // The event handler for the directory moved event.
    private EventHandler<VirtualDirectoryCopiedMovedEventArgs>? _directoryMoved;
    // The event handler for the file deleted event.
    private EventHandler<VirtualFileDeletedEventArgs>? _fileDeleted;
    // The event handler for the file copied event.
    private EventHandler<VirtualFileCopiedMovedEventArgs>? _fileCopied;
    // The event handler for the file moved event.
    private EventHandler<VirtualFileCopiedMovedEventArgs>? _fileMoved;
    // The event handler for the file renamed event.
    private EventHandler<VirtualFileRenamedEventArgs>? _fileRenamed;
    // The event thandler for the file opened event.    
    private EventHandler<VirtualFileOpenedEventArgs>? _fileOpened;

    /// <inheritdoc/>
    public event EventHandler<VirtualDirectoryCreatedEventArgs>? VirtualDirectoryCreated
    {
        add
        {
            if (value is null)
            {
                _directoryCreated = null;
                return;
            }

            _directoryCreated += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            _directoryCreated -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualDirectoryDeletedEventArgs>? VirtualDirectoryDeleted
    {
        add
        {
            if (value is null)
            {
                _directoryDeleted = null;
                return;
            }

            _directoryDeleted += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            _directoryDeleted -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualDirectoryRenamedEventArgs>? VirtualDirectoryRenamed
    {
        add
        {
            if (value is null)
            {
                _directoryRenamed = null;
                return;
            }

            _directoryRenamed += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            _directoryRenamed -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualDirectoryCopiedMovedEventArgs>? VirtualDirectoryCopied
    {
        add
        {
            if (value is null)
            {
                _directoryCopied = null;
                return;
            }
            _directoryCopied += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _directoryCopied -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualDirectoryCopiedMovedEventArgs>? VirtualDirectoryMoved
    {
        add
        {
            if (value is null)
            {
                _directoryMoved = null;
                return;
            }
            _directoryMoved += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _directoryMoved -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualFileDeletedEventArgs>? VirtualFileDeleted
    {
        add
        {
            if (value is null)
            {
                _fileDeleted = null;
                return;
            }
            _fileDeleted += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _fileDeleted -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualFileCopiedMovedEventArgs>? VirtualFileCopied
    {
        add
        {
            if (value is null)
            {
                _fileCopied = null;
                return;
            }
            _fileCopied += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _fileCopied -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualFileCopiedMovedEventArgs>? VirtualFileMoved
    {
        add
        {
            if (value is null)
            {
                _fileMoved = null;
                return;
            }
            _fileMoved += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _fileMoved -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualFileRenamedEventArgs>? VirtualFileRenamed
    {
        add
        {
            if (value is null)
            {
                _fileRenamed = null;
                return;
            }
            _fileRenamed += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _fileRenamed -= value;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<VirtualFileOpenedEventArgs>? VirtualFileOpened
    {
        add
        {
            if (value is null)
            {
                _fileOpened = null;
                return;
            }
            _fileOpened += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            _fileOpened -= value;
        }
    }

    /// <inheritdoc/>
    public bool IsReadOnly => _writeMountPoint == GorgonFileSystemMountPoint.Empty;

    /// <inheritdoc/>
    public IGorgonFileSystemProvider DefaultProvider => _defaultProvider;

    /// <inheritdoc/>
    public IReadOnlyList<GorgonFileSystemMountPoint> MountPoints => _mountPoints;

    /// <inheritdoc/>
    public IGorgonVirtualDirectory RootDirectory => _rootDirectory;

    /// <summary>
    /// Function to delete a file from the file system.
    /// </summary>
    /// <param name="file">The file to delete.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnDeleteFile(VirtualFile file)
    {
        _writeProvider.DeleteFile(file.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);
        file.Directory.Files.Remove(file);
    }

    /// <summary>
    /// Function to perform a file copy operation using validated paths.
    /// </summary>
    /// <param name="sourceFile">The file to copy.</param>
    /// <param name="destination">The path to copy into.</param>
    /// <param name="options">The options to pass to the copy operation.</param>
    /// <returns>The destination file, or <b>null</b> if the file was not copied.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown if the directory in the <paramref name="destination"/> was not found.</exception>
    private VirtualFile? OnCopyFile(VirtualFile sourceFile, ReadOnlySpan<char> destination, GorgonFileSystemCopyOptions options)
    {
        VirtualFile? destFile = GetVirtualFile(destination);
        VirtualDirectory destDir = destFile?.Directory ??
            (GetVirtualDirectory(Path.GetDirectoryName(destination)) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destination.ToString())));

        if ((destFile is null) && (GetVirtualDirectory(destination) is not null))
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, destination.ToString()));
        }

        if ((destDir != _rootDirectory) && (destDir.MountPoint != _writeMountPoint))
        {
            // If we specify a directory that isn't in the write area, then create it, and remap it.
            string physicalPath = _writeProvider.CreateDirectory(destDir.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);
            destDir.MountPoint = _writeMountPoint;
            destDir.PhysicalPath = physicalPath;

            // Remap all the directories in the path.
            VirtualDirectory? parent = destDir.Parent;

            while ((parent is not null) && (parent != _rootDirectory))
            {
                parent.MountPoint = _writeMountPoint;
                parent.PhysicalPath = _writeProvider.MapToPhysicalPath(parent.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation).ToString();
                parent = parent.Parent;
            }
        }

        // If we're hitting a file that already exists, then determine how we should handle the situation.
        if (destFile is not null)
        {
            FileConflictResolution resolution = options.ConflictResolutionCallback(sourceFile.FullPath, destination.ToString());

            switch (resolution)
            {
                case FileConflictResolution.Cancel:
                case FileConflictResolution.Skip:
                case FileConflictResolution.SkipAll:
                    return null;
                case FileConflictResolution.Exception:
                    throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destination.ToString()));
                case FileConflictResolution.Rename:
                case FileConflictResolution.RenameAll:
                    destination = GenerateName(Path.GetFileName(destination), Path.GetDirectoryName(destination).FormatDirectory(DirectorySeparator));
                    destFile = null;
                    break;
            }
        }

        IGorgonPhysicalFileInfo fileInfo = _writeProvider.CopyFile(sourceFile, destination, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation, options);

        // If we overwrote a file, then update its file information.
        if (destFile is not null)
        {
            destDir.Files.Remove(destFile);
        }

        destFile = destDir.Files.Add(_writeMountPoint, fileInfo);

        return destFile;
    }

    /// <summary>
    /// Function to generate a new file name for a file that exists in a location.
    /// </summary>
    /// <param name="fileName">The name of the file being copied.</param>
    /// <param name="destDirectory">The destination directory.</param>
    /// <returns>The updated, unique name.</returns>
    private string GenerateName(ReadOnlySpan<char> fileName, ReadOnlySpan<char> destDirectory)
    {
        StringBuilder nameBuilder = new();
        int index = 1;
        ReadOnlySpan<char> ext = Path.GetExtension(fileName);
        fileName = Path.GetFileNameWithoutExtension(fileName);

        nameBuilder.Length = 0;
        nameBuilder.Append(destDirectory);
        nameBuilder.Append(fileName);
        nameBuilder.Append(" (");
        nameBuilder.Append(index);
        nameBuilder.Append(')');
        nameBuilder.Append(ext);

        string newName = nameBuilder.ToString();

        VirtualFile? file = GetVirtualFile(newName);

        while (file is not null)
        {
            nameBuilder.Length = 0;
            nameBuilder.Append(destDirectory);
            nameBuilder.Append(fileName);
            nameBuilder.Append(" (");
            nameBuilder.Append(++index);
            nameBuilder.Append(')');
            nameBuilder.Append(ext);

            newName = nameBuilder.ToString();

            file = GetVirtualFile(newName);
        }

        return newName;
    }

    /// <summary>
    /// Function to retrieve the file system objects from the physical file system.
    /// </summary>
    /// <param name="mountPoint">The mount point to link the physical file system with the virtual file system.</param>
    private void BuildFileSystemObjects(GorgonFileSystemMountPoint mountPoint)
    {
        ReadOnlySpan<char> physicalPath = mountPoint.PhysicalPath.AsSpan();

        // Find existing mount point.
        VirtualDirectory? mountDirectory = GetVirtualDirectory(mountPoint.MountLocation.AsSpan());

        if (mountDirectory is null)
        {
            mountDirectory = _rootDirectory.Directories.Add(mountPoint, mountPoint.MountLocation, string.Empty);
        }
        else
        {
            // Do not update the root directory. It should always use the default provider.
            if (mountDirectory != _rootDirectory)
            {
                mountDirectory.MountPoint = mountPoint;
            }
        }

        _log.Print($"Mounting physical file system path '{physicalPath}' to virtual file system path '{mountPoint.MountLocation}'.", LoggingLevel.Simple);

        GorgonPhysicalFileSystemData data = mountPoint.Provider.Enumerate(physicalPath, physicalPath, mountDirectory.FullPath.AsSpan());

        // Process the directories.
        foreach ((string physicalDirectoryPath, string virtualDirectoryPath) in data.Directories)
        {
            VirtualDirectory? existingDirectory = GetVirtualDirectory(virtualDirectoryPath.AsSpan());

            // If the directory path already exists for another provider, then override it with the 
            // provider we're currently loading. All directories and files will be overridden by the last 
            // provider loaded if they already exist.
            if (existingDirectory is null)
            {
                _rootDirectory.Directories.Add(mountPoint, virtualDirectoryPath, physicalDirectoryPath);
                continue;
            }

            // Do not change the mount point on our root directory, it should always be the default provider.
            if (existingDirectory == _rootDirectory)
            {
                continue;
            }

            if (existingDirectory.MountPoint == _writeMountPoint)
            {
                _log.Print($"The directory '{existingDirectory.FullPath}' is mounted into the write area mount point.", LoggingLevel.Verbose);
                continue;
            }

            _log.Print($"\"{existingDirectory.FullPath}\" already exists in provider: " +
                                $"\"{existingDirectory.MountPoint.Provider.Description}\". Changing provider to \"{mountPoint.Provider.Description}\"",
                                LoggingLevel.Verbose);

            existingDirectory.MountPoint = mountPoint;
        }

        // Process the files.
        foreach (IGorgonPhysicalFileInfo fileInfo in data.Files)
        {
            ReadOnlySpan<char> directoryName = Path.GetDirectoryName(fileInfo.VirtualPath.AsSpan()).FormatDirectory(DirectorySeparator);

            if (directoryName.IsEmpty)
            {
                continue;
            }

            // If the path is not rooted, then do so now.
            if (!directoryName.StartsWith(DirectorySeparatorList, StringComparison.OrdinalIgnoreCase))
            {
                directoryName = string.Concat(DirectorySeparatorList, directoryName).AsSpan();
            }

            // At this point, all of our directories should be set up. If we don't find the parent for the file, that means the file system may be corrupted.
            VirtualDirectory directory = GetVirtualDirectory(directoryName) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_CORRUPT_FS_NO_PARENT, fileInfo.VirtualPath));

            // Update the file information to the most recent provider.
            if (!directory.Files.TryGetVirtualFile(fileInfo.Name.AsSpan(), out VirtualFile? virtualFile))
            {
                directory.Files.Add(mountPoint, fileInfo);
                continue;
            }

            if (virtualFile.MountPoint == _writeMountPoint)
            {
                _log.Print($"The file '{virtualFile.FullPath}' is mounted into the write area mount point.", LoggingLevel.Verbose);
                continue;
            }

            _log.Print($"\"{virtualFile.FullPath}\" already exists in provider: " +
                        $"\"{virtualFile.MountPoint.Provider.Description}\". Changing provider to \"{mountPoint.Provider.Description}\"",
                        LoggingLevel.Verbose);

            virtualFile.MountPoint = mountPoint;
            virtualFile.PhysicalFile = fileInfo;
        }

        _log.Print($"{data.Directories.Count} directories parsed, and {data.Files.Count} files processed.", LoggingLevel.Simple);
    }

    /// <summary>
    /// Function to determine if the name of an object matches the pattern specified.
    /// </summary>
    /// <typeparam name="T">Type of named object to compare.</typeparam>
    /// <param name="item">Item to compare.</param>
    /// <param name="mask">The mask used to filter.</param>
    /// <returns><b>true</b> if the name of the item matches the pattern, <b>false</b> if not.</returns>
    private static bool IsPatternMatch<T>(T item, string mask)
        where T : IGorgonNamedObject
    {
        ReadOnlySpan<char> name = item.Name.AsSpan();
        ReadOnlySpan<char> searchMask = mask.AsSpan();

        int wildcardIndex = searchMask.IndexOf("*", StringComparison.OrdinalIgnoreCase);

        // If we didn't supply a wildcard, match exact names only.
        if (wildcardIndex == -1)
        {
            return name.Equals(searchMask, StringComparison.OrdinalIgnoreCase);
        }

        // If we have the search mask wrapped in wildcards, then we're looking for an occurrence within the name.
        if ((searchMask[0] == '*') && (searchMask[^1] == '*'))
        {
            if (searchMask.Length == 2)
            {
                return false;
            }

            return name.IndexOf(searchMask[1..^1], StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // If we have a wild card at the beginning, then match on names that contain the value after the wildcard.
        if (wildcardIndex == 0)
        {
            return name.EndsWith(searchMask[1..], StringComparison.OrdinalIgnoreCase);
        }

        // If we have a wild card at the end, then match on names that contain the value before the wildcard.
        if (wildcardIndex == searchMask.Length - 1)
        {
            return name.StartsWith(searchMask[..^1], StringComparison.OrdinalIgnoreCase);
        }

        // Otherwise, match on the beginning and end of the mask string surrounding the wildcard value.
        ReadOnlySpan<char> left = searchMask[..(wildcardIndex - 1)];
        ReadOnlySpan<char> right = searchMask[(wildcardIndex + 1)..];

        return name.StartsWith(left, StringComparison.OrdinalIgnoreCase) && name.EndsWith(right, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Function to retrieve a writable <see cref="VirtualFile"/> entry from the file system.
    /// </summary>
    /// <param name="path">The path to the file entry.</param>
    /// <returns>The file entry if found, null if not.</returns>
    private VirtualFile? GetVirtualFile(ReadOnlySpan<char> path)
    {
        // Get path parts.        
        ReadOnlySpan<char> directory = Path.GetDirectoryName(path);

        if (directory.IsEmpty)
        {
            directory = DirectorySeparatorList;
        }

        ReadOnlySpan<char> filename = Path.GetFileName(path);

        // Check for file name.
        if (filename.IsEmpty)
        {
            throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_NO_FILENAME, path.ToString()), nameof(path));
        }

        // Start search.
        VirtualDirectory? virtualDir = GetVirtualDirectory(directory);

        if (virtualDir is null)
        {
            return null;
        }

        virtualDir.Files.TryGetVirtualFile(filename, out VirtualFile? result);

        return result;
    }

    /// <summary>
    /// Function to retrieve a writable <see cref="VirtualDirectory"/> entry from the file system.
    /// </summary>
    /// <param name="path">The path to the directory entry.</param>
    /// <returns>The directory entry if found, null if not.</returns>
    private VirtualDirectory? GetVirtualDirectory(ReadOnlySpan<char> path)
    {
        // Optimization to deal with the root path.
        if (path.Equals(DirectorySeparatorList, StringComparison.Ordinal))
        {
            return _rootDirectory;
        }

        // Strip off any leading/trailing directory separators, so we can get an accurate sub directory count.
        if (path.StartsWith(DirectorySeparatorList, StringComparison.Ordinal))
        {
            path = path[1..];
        }

        if (path.EndsWith(DirectorySeparatorList, StringComparison.Ordinal))
        {
            path = path[..^1];
        }

        int pathPartCount = path.Count(DirectorySeparator) + 1;

        scoped Span<Range> ranges;
        Range[]? rangeArray = null;
        ArrayPool<Range>? rangePool = null;

        try
        {
            // If we have a LOT of path parts, then allocate on the heap as we want to limit the size of our stack.
            if (pathPartCount > 64)
            {
                rangePool = GorgonArrayPools<Range>.GetBestPool(pathPartCount);
                rangeArray = rangePool.Rent(pathPartCount);
                ranges = rangeArray.AsSpan(0, pathPartCount);
                ranges.Clear();
            }
            else
            {
                ranges = stackalloc Range[pathPartCount];
            }

            int splitCount = path.Split(ranges, DirectorySeparator, StringSplitOptions.RemoveEmptyEntries);

            if (splitCount == 0)
            {
                return null;
            }

            VirtualDirectory? directory = _rootDirectory;

            for (int i = 0; i < splitCount; i++)
            {
                ReadOnlySpan<char> directoryName = path[ranges[i]];

                if (!directory.Directories.TryGetValue(directoryName.ToString(), out directory))
                {
                    return null;
                }
            }

            return directory;
        }
        finally
        {
            if ((rangePool is not null) && (rangeArray is not null))
            {
                rangePool.Return(rangeArray);
            }
        }
    }

    /// <summary>
    /// Function to prepend a path with the directory separator.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns>The path prepended with the directory separator, or the original path if the <paramref name="path"/> already has a directory separator.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> PrependPath(ReadOnlySpan<char> path)
    {
        if (path[0] != DirectorySeparator)
        {
            return string.Concat(DirectorySeparatorList, path).AsSpan();
        }

        return path;
    }

    /// <summary>
    /// Function to check if the source and destination paths are the same.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="destination">The destination path.</param>
    /// <exception cref="IOException">Thrown if the paths are the same.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckForSameSourceAndDest(ReadOnlySpan<char> source, ReadOnlySpan<char> destination)
    {
        if (source.Equals(destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_SOURCE_DEST_SAME, source.ToString(), destination.ToString()));
        }
    }

    /// <summary>
    /// Function to check if the destination path is a child of the source path.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="destination">The destination path.</param>
    /// <exception cref="IOException">Thrown if the destination is a child of the source path.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckForChildRoot(ReadOnlySpan<char> source, ReadOnlySpan<char> destination)
    {
        if (destination.StartsWith(source, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_COPY_TO_CHILD, destination.ToString(), source.ToString()));
        }
    }

    /// <summary>
    /// Function to check if the file system is read only.
    /// </summary>
    /// <exception cref="GorgonException">Thrown when the file system is read only.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckReadOnlyFileSystem()
    {
        if (IsReadOnly)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORFS_ERR_FILESYSTEM_IS_READ_ONLY);
        }
    }

    /// <summary>
    /// Function to check if a mount point is from a read only provider.
    /// </summary>
    /// <param name="mountPoint">The mount point to evaluate.</param>
    /// <param name="path">The file system path being evaluated by the caller.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckForReadOnlyProvider(GorgonFileSystemMountPoint mountPoint, ReadOnlySpan<char> path)
    {
        if (mountPoint != _writeMountPoint)
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORFS_ERR_PROVIDER_READ_ONLY, path.ToString()));
        }
    }

    /// <summary>
    /// Function to copy or move a directory.
    /// </summary>
    /// <param name="sourcePath">The source path to the directory to move or copy.</param>
    /// <param name="destinationPath">The path for the destination directory that will receive the moved/copied directory.</param>
    /// <param name="move"><b>true</b> to move the directory, <b>false</b> to copy only.</param>
    /// <param name="options">Options for the operation.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="sourcePath"/>, or <paramref name="destinationPath"/> could not be found.</exception>
    /// <exception cref="GorgonException">Thrown when no physical directory has been mounted as a writable file system.</exception>
    /// <exception cref="IOException">Thrown if the source and destination paths are the same.</exception>
    /// <remarks>
    /// <para>
    /// Since copy and move are very similar operations, this method is used to handle both. This avoids having to duplicate all of this code for both operations.
    /// </para>
    /// </remarks>
    private void CopyOrMoveDirectory(string sourcePath, string destinationPath, bool move, GorgonFileSystemCopyOptions options)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(destinationPath);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> source = PrependPath(sourcePath.AsSpan()).FormatDirectory(DirectorySeparator);
        ReadOnlySpan<char> destination = PrependPath(destinationPath.AsSpan()).FormatDirectory(DirectorySeparator);

        CheckForChildRoot(source, destination);

        // Get the source directory, we're copying this guy and everything underneath.
        VirtualDirectory sourceDirectory = GetVirtualDirectory(source) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, sourcePath));
        VirtualDirectory destinationDirectory = GetVirtualDirectory(destination) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, destinationPath));

        CheckForSameSourceAndDest(source, string.Concat(destinationDirectory.FullPath, sourceDirectory.Name).FormatDirectory(DirectorySeparator));

        if (move)
        {
            CheckForReadOnlyProvider(sourceDirectory.MountPoint, sourcePath);
        }

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        if (options.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        if ((destinationDirectory != _rootDirectory) && (destinationDirectory.MountPoint != _writeMountPoint))
        {
            string dirPhysicalPath = _writeProvider.CreateDirectory(destinationDirectory.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);
            destinationDirectory.MountPoint = _writeMountPoint;
            destinationDirectory.PhysicalPath = dirPhysicalPath;
        }

        // We order by path length because the lowest level directories should be removed first.
        VirtualDirectory[] directories = [.. sourceDirectory.Directories
                                                                           .EnumerateVirtualDirectories()
                                                                           .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories())
                                                                           .OrderByDescending(item => item.FullPath.Length),
                                                            sourceDirectory];

        List<(IGorgonVirtualDirectory, IGorgonVirtualDirectory)> copiedDirectories = [];
        List<(IGorgonVirtualFile, IGorgonVirtualFile)> copiedFiles = [];

        Debug.Assert(sourceDirectory.Parent is not null, "Cannot copy root, this should have been caught in the check above.");

        int sourceCutOff = sourceDirectory.Parent.FullPath.Length;

        for (int i = 0; i < directories.Length; ++i)
        {
            if (options.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            VirtualDirectory sourceChildDirectory = directories[i];
            ReadOnlySpan<char> childPath = sourceChildDirectory.FullPath.AsSpan(sourceCutOff..);
            childPath = string.Concat(destination, childPath);

            VirtualDirectory? destinationChildDirectory = GetVirtualDirectory(childPath);

            if (destinationChildDirectory is null)
            {
                string physicalPath = _writeProvider.CreateDirectory(childPath, _writeMountPoint.PhysicalPath.AsSpan(), _writeMountPoint.MountLocation.AsSpan());
                destinationChildDirectory = _rootDirectory.Directories.Add(_writeMountPoint, childPath.ToString(), physicalPath);
            }

            foreach (VirtualFile sourceFile in sourceChildDirectory.Files.EnumerateVirtualFiles())
            {
                if (options.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ReadOnlySpan<char> destFilePath = string.Concat(childPath, sourceFile.Name);
                VirtualFile? destFile = OnCopyFile(sourceFile, destFilePath, options);

                if (destFile is null)
                {
                    _log.PrintWarning($"File '{sourceFile.FullPath}' was skipped.", LoggingLevel.Verbose);
                    continue;
                }

                if (move)
                {
                    OnDeleteFile(sourceFile);
                }

                copiedFiles.Add((sourceFile, destFile));
            }

            if (options.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (move)
            {
                _writeProvider.DeleteDirectory(sourceDirectory.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);
                sourceChildDirectory.Parent?.Directories.Remove(sourceChildDirectory);
            }

            copiedDirectories.Add((sourceChildDirectory, destinationChildDirectory));
        }

        if (!move)
        {
            _log.Print($"Directory {sourceDirectory.FullPath} copied. {copiedDirectories.Count} subdirectories copied, {copiedFiles.Count} files copied.", LoggingLevel.Verbose);
            _directoryCopied?.Invoke(this, new VirtualDirectoryCopiedMovedEventArgs(destinationDirectory, copiedDirectories, copiedFiles));
        }
        else
        {
            _log.Print($"Directory {sourceDirectory.FullPath} moved. {copiedDirectories.Count} subdirectories moved, {copiedFiles.Count} files moved.", LoggingLevel.Verbose);
            _directoryMoved?.Invoke(this, new VirtualDirectoryCopiedMovedEventArgs(destinationDirectory, copiedDirectories, copiedFiles));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask = "*", bool recursive = true)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        ReadOnlySpan<char> directoryPath = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);

        VirtualDirectory? start = GetVirtualDirectory(directoryPath) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path.ToString()));

        if (string.IsNullOrWhiteSpace(directoryMask))
        {
            directoryMask = "*";
        }

        IEnumerable<VirtualDirectory> directories = start.Directories.EnumerateVirtualDirectories();

        if (recursive)
        {
            directories = directories.TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories());
        }

        // Check to see if we're returning all files.
        bool allFiles = string.Equals(directoryMask, "*", StringComparison.OrdinalIgnoreCase);

        foreach (VirtualDirectory directory in directories)
        {
            if ((allFiles) || (IsPatternMatch(directory, directoryMask)))
            {
                yield return directory;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask = "*", bool recursive = true)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        ReadOnlySpan<char> directoryPath = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);

        VirtualDirectory? start = GetVirtualDirectory(directoryPath) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path.ToString()));

        if (string.IsNullOrWhiteSpace(fileMask))
        {
            fileMask = "*";
        }

        IEnumerable<VirtualFile> files = start.Files.EnumerateVirtualFiles();

        if (recursive)
        {
            IEnumerable<VirtualDirectory> directories = start.Directories.EnumerateVirtualDirectories()
                                                                         .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories());

            files = files.Concat(directories.SelectMany(item => item.Files.EnumerateVirtualFiles()));
        }

        // Check to see if we're returning all files.
        bool allFiles = string.Equals(fileMask, "*", StringComparison.OrdinalIgnoreCase);

        foreach (VirtualFile file in files)
        {
            if ((allFiles) || (IsPatternMatch(file, fileMask)))
            {
                yield return file;
            }
        }
    }

    /// <inheritdoc/>
    public IGorgonVirtualFile? GetFile(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        ReadOnlySpan<char> pathSpan = PrependPath(path.AsSpan());

        return GetVirtualFile(pathSpan.FormatPath(DirectorySeparator));
    }

    /// <inheritdoc/>
    public IGorgonVirtualDirectory? GetDirectory(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        ReadOnlySpan<char> pathSpan = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);

        return GetVirtualDirectory(pathSpan);
    }

    /// <inheritdoc/>
    public void Refresh(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        ReadOnlySpan<char> pathSpan = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);

        VirtualDirectory directory = GetVirtualDirectory(pathSpan) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));

        if (pathSpan.Equals(DirectorySeparatorList, StringComparison.Ordinal))
        {
            Refresh();
            return;
        }

        directory.Directories.Clear();
        directory.Files.Clear();

        ReadOnlySpan<char> physicalPath = directory.MountPoint.Provider.MapToPhysicalPath(pathSpan, directory.MountPoint.PhysicalPath, directory.MountPoint.MountLocation);

        if (physicalPath.Equals(directory.MountPoint.PhysicalPath, StringComparison.OrdinalIgnoreCase))
        {
            Refresh();
            return;
        }

        GorgonPhysicalFileSystemData fsData = directory.MountPoint.Provider.Enumerate(physicalPath, directory.MountPoint.PhysicalPath, directory.MountPoint.MountLocation);

        if ((fsData.Directories.Count == 0) && (fsData.Files.Count == 0))
        {
            return;
        }

        foreach ((string physicalDirectoryPath, string virtualDirectoryPath) in fsData.Directories)
        {
            directory.Directories.Add(directory.MountPoint, virtualDirectoryPath, physicalDirectoryPath);
        }

        foreach (IGorgonPhysicalFileInfo file in fsData.Files)
        {
            ReadOnlySpan<char> directoryName = PrependPath(Path.GetDirectoryName(file.VirtualPath.AsSpan())).FormatDirectory(DirectorySeparator);

            VirtualDirectory subDirectory = GetVirtualDirectory(directoryName) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_CORRUPT_FS_NO_PARENT, file.FullPath));

            subDirectory.Files.Add(directory.MountPoint, file);
        }
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        GorgonFileSystemMountPoint writeMount = Interlocked.Exchange(ref _writeMountPoint, GorgonFileSystemMountPoint.Empty);

        GorgonFileSystemMountPoint[] mountPoints = [.. _mountPoints];

        Unmount();

        // Refresh the mount points so we can capture the most up to date data.
        foreach (GorgonFileSystemMountPoint mountPoint in mountPoints)
        {
            Mount(mountPoint.PhysicalPath, mountPoint.MountLocation, mountPoint.Provider);
        }

        if (writeMount == GorgonFileSystemMountPoint.Empty)
        {
            return;
        }

        _writeProvider.PrepareWriteArea(writeMount.PhysicalPath);
    }

    /// <inheritdoc/>
    public void Unmount()
    {
        _writeMountPoint = GorgonFileSystemMountPoint.Empty;
        _rootDirectory.Directories.Clear();
        _rootDirectory.Files.Clear();
        _mountPoints.Clear();
    }

    /// <inheritdoc/>
    public void Unmount(GorgonFileSystemMountPoint mountPoint)
    {
        if (mountPoint == GorgonFileSystemMountPoint.Empty)
        {
            return;
        }

        GorgonFileSystemMountPoint writeMount = Interlocked.Exchange(ref _writeMountPoint, GorgonFileSystemMountPoint.Empty);

        if (!_mountPoints.Contains(mountPoint))
        {
            throw new ArgumentException(string.Format(Resources.GORFS_ERR_MOUNTPOINT_NOT_FOUND,
                                                        mountPoint.MountLocation,
                                                        mountPoint.PhysicalPath),
                                        nameof(mountPoint));
        }

        _mountPoints.Remove(mountPoint);

        // Re-mount the remaining mount points.
        // This should refresh each file, and directory in the file system without necessarily creating new objects.
        // The write location will no longer take precedence here because we've disabled it temporarily.
        for (int i = 0; i < _mountPoints.Count; ++i)
        {
            GorgonFileSystemMountPoint mp = _mountPoints[i];
            Mount(mp.PhysicalPath, mp.MountLocation, mp.Provider);
        }

        // Once the files/directories are refreshed, remove any remaining files/directories associated with the mount point.        
        VirtualDirectory[] directories = [.. _rootDirectory.Directories.EnumerateVirtualDirectories()
                                                                                      .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories())
                                                                                      .Where(item => item.MountPoint == mountPoint)
                                                                                      .OrderByDescending(item => item.FullPath.Length)];

        VirtualFile[] files = [.. _rootDirectory.Files.EnumerateVirtualFiles()
                                                                 .Where(item => item.MountPoint == mountPoint)
                                                                 .Concat(directories.SelectMany(item => item.Files.EnumerateVirtualFiles()
                                                                                                                               .Where(item => item.MountPoint == mountPoint)))];

        // Remove the files associated with the mount point.        
        foreach (VirtualFile file in files)
        {
            file.Directory.Files.Remove(file);
        }

        // Remove the directories associated with the mount point.
        foreach (VirtualDirectory directory in directories)
        {
            if ((directory.Parent is null) || (directory.Files.Count != 0))
            {
                // We should not have directories that have files in them by this point, but we'll check anyway 
                // and preserve the directory if we do.
                if (directory.Files.Count != 0)
                {
                    // Inherit the mount point from its first file because, for some reason, this directory didn't get its 
                    // mount point updated when we did the refresh earlier.
                    directory.MountPoint = directory.Files.First().Value.MountPoint;
                }

                continue;
            }

            directory.Parent.Directories.Remove(directory);
        }

        if ((writeMount == GorgonFileSystemMountPoint.Empty) || (mountPoint == writeMount))
        {
            return;
        }

        MountWriteArea(_writeMountPoint.PhysicalPath);
    }

    /// <inheritdoc/>
    public GorgonFileSystemMountPoint Mount(string physicalPath, string? mountPath = null, IGorgonFileSystemProvider? provider = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(physicalPath);

        mountPath ??= DirectorySeparator.ToString();

        if (mountPath.Length == 0)
        {
            mountPath = DirectorySeparator.ToString();
        }

        provider ??= _defaultProvider;

        mountPath = PrependPath(mountPath.AsSpan()).ToString();
        physicalPath = provider.MapToPhysicalPath(DirectorySeparatorList, physicalPath, DirectorySeparatorList).ToString();

        if (!provider.CanReadFileSystem(physicalPath))
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_READ_FILESYSTEM, physicalPath));
        }

        GorgonFileSystemMountPoint mountPoint = new(provider, physicalPath, false, mountPath);

        BuildFileSystemObjects(mountPoint);

        _mountPoints.Remove(mountPoint);
        _mountPoints.Add(mountPoint);

        return mountPoint;
    }

    /// <inheritdoc/>
    public GorgonFileSystemMountPoint MountWriteArea(string physicalPath)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(physicalPath);

        if (!IsReadOnly)
        {
            // Already mounted, get out.
            if (string.Equals(physicalPath, _writeMountPoint.PhysicalPath, StringComparison.OrdinalIgnoreCase))
            {
                return _writeMountPoint;
            }

            Unmount(_writeMountPoint);
        }

        if (!_writeProvider.CanWriteFileSystem(physicalPath.AsSpan()))
        {
            throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GORFS_ERR_CANNOT_WRITE_FILESYSTEM, physicalPath));
        }

        _writeMountPoint = Mount(physicalPath, DirectorySeparatorString, _writeProvider);

        return _writeMountPoint;
    }

    /// <inheritdoc/>
    public GorgonFileSystemStream OpenStream(string path, bool write, Action<IGorgonVirtualFile, FileStreamStatus>? closeCallback = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        if (write)
        {
            CheckReadOnlyFileSystem();
        }

        ReadOnlySpan<char> pathSpan = path.AsSpan().FormatPath(DirectorySeparator);
        ReadOnlySpan<char> directoryPath = Path.GetDirectoryName(pathSpan);
        ReadOnlySpan<char> fileName = Path.GetFileName(pathSpan);

        // Check to ensure we're not trying to open a directory.
        if (GetVirtualDirectory(pathSpan.FormatDirectory(DirectorySeparator)) is not null)
        {
            if (write)
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, path));
            }
            else
            {
                throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
            }
        }

        if (fileName.IsEmpty)
        {
            throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_NO_FILENAME, path));
        }

        VirtualDirectory? directory = GetVirtualDirectory(directoryPath) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryPath.ToString()));
        VirtualFile? file = GetVirtualFile(pathSpan);

        // If we're only reading the file, then do a file exist check and get its provider to open a stream to it.
        if (!write)
        {
            // We're opening an existing file, so check it if we require the file to be present.
            if (file is null)
            {
                throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
            }

            void OnClose(string virtualPath, string physicalPath) => closeCallback?.Invoke(file, FileStreamStatus.ReadOnly);

            GorgonFileSystemStream readStream = file.MountPoint.Provider.OpenReadFileStream(file.PhysicalFile, file.MountPoint.PhysicalPath.AsSpan(), OnClose) ?? throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));

            _fileOpened?.Invoke(this, new VirtualFileOpenedEventArgs(file.FullPath, true, false));

            return readStream;
        }

        // Function called when the stream is closed.
        void OnStreamClose(string virtualPath, string physicalPath)
        {
            // Get the new file information and pass it to the file system.            
            IGorgonPhysicalFileInfo fileInfo = _writeProvider.GetPhysicalFileInfo(virtualPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);
            VirtualFile? file = GetVirtualFile(virtualPath.AsSpan());
            FileStreamStatus status = file is null ? FileStreamStatus.NewFile : FileStreamStatus.Updated;

            if (file is null)
            {
                file = directory.Files.Add(_writeMountPoint, fileInfo);
            }
            else
            {
                file.MountPoint = _writeMountPoint;
                file.PhysicalFile = fileInfo;
            }

            closeCallback?.Invoke(file, status);
        }

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        GorgonFileSystemStream writeStream = _writeProvider.OpenWriteFileStream(pathSpan, _writeMountPoint.PhysicalPath.AsSpan(), _writeMountPoint.MountLocation.AsSpan(), OnStreamClose);

        _fileOpened?.Invoke(this, new VirtualFileOpenedEventArgs(pathSpan.ToString(), false, file is null));

        return writeStream;
    }

    /// <inheritdoc/>
    public IGorgonVirtualDirectory CreateDirectory(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> directoryPath = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);

        if (directoryPath.Equals(DirectorySeparatorList, StringComparison.Ordinal))
        {
            return _rootDirectory;
        }

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        VirtualDirectory? directory = GetVirtualDirectory(directoryPath);

        if (directory is not null)
        {
            _log.Print($"{directory.FullPath} already exists.", LoggingLevel.Verbose);
            return directory;
        }

        string physicalPath = _writeProvider.CreateDirectory(directoryPath, _writeMountPoint.PhysicalPath.AsSpan(), _writeMountPoint.MountLocation.AsSpan());

        directory = _rootDirectory.Directories.Add(_writeMountPoint, directoryPath.ToString(), physicalPath);

        VirtualDirectory? parent = directory.Parent;

        // Ensure all parent directories are set to the write mount point.
        while ((parent is not null) && (parent != _rootDirectory))
        {
            parent.MountPoint = _writeMountPoint;
            parent = parent.Parent;
        }

        _directoryCreated?.Invoke(this, new VirtualDirectoryCreatedEventArgs(directory));

        _log.Print($"{directory.FullPath} created.", LoggingLevel.Verbose);

        return directory;
    }

    /// <inheritdoc/>
    public void DeleteDirectory(string path, GorgonFileSystemDeleteOptions? options = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> pathSpan = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);

        options ??= GorgonFileSystemDeleteOptions.Default;

        VirtualDirectory directory = GetVirtualDirectory(pathSpan) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));

        CheckForReadOnlyProvider(directory.MountPoint, pathSpan);

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        if (options.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Capture which files and directories were actually deleted.
        List<VirtualFile> deletedFiles = [];
        List<VirtualDirectory> deletedDirectories = [];

        // Get empty directories under this and its child directories.
        // We order by path length because the lowest level directories should be removed first.
        VirtualDirectory[] directories = [.. directory.Directories.EnumerateVirtualDirectories()
                                                                                    .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories())
                                                                                    .OrderByDescending(item => item.FullPath.Length),
                                                            directory];

        VirtualFile[] files = ArrayPool<VirtualFile>.Shared.Rent(1);

        try
        {
            for (int i = 0; i < directories.Length; i++)
            {
                if (options.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                VirtualDirectory dir = directories[i];

                if (dir.Files.Count > 0)
                {
                    // This alters the collection of files under the directory.
                    // So, we need to copy the files to an array to avoid updating enumerables.
                    if (dir.Files.Count > files.Length)
                    {
                        ArrayPool<VirtualFile>.Shared.Return(files, true);
                        files = ArrayPool<VirtualFile>.Shared.Rent(dir.Files.Count);
                    }

                    dir.Files.CopyTo(files);

                    for (int j = 0; j < dir.Files.Count; ++j)
                    {
                        if (options.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        VirtualFile file = files[j];

                        options.ProgressCallback?.Invoke(file.FullPath);

                        OnDeleteFile(file);
                        deletedFiles.Add(file);
                    }
                }

                options.ProgressCallback?.Invoke(dir.FullPath);

                // If we're at the root level, then we're done, we can't remove this directory.
                if (dir.Parent is null)
                {
                    break;
                }

                _writeProvider.DeleteDirectory(dir.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);
                dir.Parent.Directories.Remove(dir);
                deletedDirectories.Add(dir);
            }

            _log.Print($"Directory '{directory}' deleted. {deletedDirectories.Count} directories deleted, {deletedFiles.Count} files deleted.", LoggingLevel.Verbose);
        }
        finally
        {
            ArrayPool<VirtualFile>.Shared.Return(files, true);
        }

        _directoryDeleted?.Invoke(this, new VirtualDirectoryDeletedEventArgs(deletedDirectories, deletedFiles));
    }

    /// <inheritdoc/>
    public void RenameDirectory(string path, string newName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(newName);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> directoryPath = PrependPath(path.AsSpan()).FormatDirectory(DirectorySeparator);
        ReadOnlySpan<char> destName = newName.AsSpan().FormatPathPart();

        VirtualDirectory dir = GetVirtualDirectory(directoryPath) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));

        CheckForReadOnlyProvider(dir.MountPoint, directoryPath);

        VirtualDirectory parentDir = dir.Parent ?? throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_MOVE_OR_RENAME_ROOT));

        ReadOnlySpan<char> newDirectory = string.Concat(parentDir.FullPath, destName).FormatDirectory(DirectorySeparator);

        CheckForSameSourceAndDest(directoryPath, newDirectory);

        if (GetVirtualFile(newDirectory[..^1]) is not null)
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, newDirectory.ToString()));
        }

        VirtualDirectory? destinationDir = GetVirtualDirectory(newDirectory);

        if (destinationDir is not null)
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, destinationDir.FullPath));
        }

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        string originalName = dir.Name;
        string physicalDest = _writeProvider.RenameDirectory(dir.FullPath, destName, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation).ToString();

        parentDir.Directories.Remove(dir);
        dir.Name = destName.ToString();
        dir.FullPath = newDirectory.ToString();
        dir.PhysicalPath = physicalDest;

        foreach (VirtualFile file in dir.Files.EnumerateVirtualFiles())
        {
            file.MountPoint = dir.MountPoint;
            file.PhysicalFile = _writeProvider.GetPhysicalFileInfo(string.Concat(newDirectory, file.Name), dir.MountPoint.PhysicalPath, dir.MountPoint.MountLocation);
        }

        foreach (VirtualDirectory subDir in dir.Directories.EnumerateVirtualDirectories()
                                                           .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories()))
        {
            Debug.Assert(subDir.Parent is not null, "Sub directory should not be the root directory.");

            subDir.MountPoint = _writeMountPoint;
            subDir.FullPath = string.Concat(newDirectory, subDir.Name).FormatDirectory(DirectorySeparator);
            subDir.PhysicalPath = _writeProvider.MapToPhysicalPath(subDir.FullPath, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation).ToString();

            foreach (VirtualFile file in subDir.Files.EnumerateVirtualFiles())
            {
                file.MountPoint = dir.MountPoint;
                file.PhysicalFile = _writeProvider.GetPhysicalFileInfo(string.Concat(subDir.FullPath, file.Name), dir.MountPoint.PhysicalPath, dir.MountPoint.MountLocation);
            }
        }

        parentDir.Directories.Add(dir);

        _log.Print($"Directory '{directoryPath}' renamed to '{dir.FullPath}'.", LoggingLevel.Verbose);

        _directoryRenamed?.Invoke(this, new VirtualDirectoryRenamedEventArgs(dir, originalName));
    }

    /// <inheritdoc/>
    public void CopyDirectory(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null) => CopyOrMoveDirectory(sourcePath, destinationPath, false, options ?? GorgonFileSystemCopyOptions.Default);

    /// <inheritdoc/>
    public void MoveDirectory(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null) => CopyOrMoveDirectory(sourcePath, destinationPath, true, options ?? GorgonFileSystemCopyOptions.Default);

    /// <inheritdoc/>
    public void DeleteFile(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> filePath = PrependPath(path.AsSpan()).FormatPath(DirectorySeparator);

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        if (GetVirtualDirectory(Path.GetDirectoryName(filePath).FormatDirectory(DirectorySeparator)) is null)
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, Path.GetDirectoryName(filePath).FormatDirectory(DirectorySeparator).ToString()));
        }

        VirtualFile file = GetVirtualFile(filePath) ?? throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));

        CheckForReadOnlyProvider(file.MountPoint, file.FullPath);

        OnDeleteFile(file);

        _log.Print($"File '{file.FullPath}' deleted.", LoggingLevel.Verbose);
        _fileDeleted?.Invoke(this, new VirtualFileDeletedEventArgs([file]));
    }

    /// <inheritdoc/>
    public void CopyFile(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(destinationPath);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> source = PrependPath(sourcePath.AsSpan()).FormatPath(DirectorySeparator);
        ReadOnlySpan<char> destination = destinationPath.AsSpan();

        if (destination.Contains(DirectorySeparatorList, StringComparison.Ordinal))
        {
            destination = PrependPath(destination).FormatPath(DirectorySeparator);
        }
        else
        {
            destination = string.Concat(Path.GetDirectoryName(source), DirectorySeparatorList, destination).FormatPath(DirectorySeparator);
        }

        if (GetVirtualDirectory(Path.GetDirectoryName(destination).FormatDirectory(DirectorySeparator)) is null)
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, Path.GetDirectoryName(destination).FormatDirectory(DirectorySeparator).ToString()));
        }

        VirtualFile sourceFile = GetVirtualFile(source) ?? throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, sourcePath));

        // If we did not supply a file name, then provide the same name as the source.
        if (Path.GetFileName(destination).IsEmpty)
        {
            destination = string.Concat(destination, sourceFile.Name);
        }

        CheckForSameSourceAndDest(source, destination);

        VirtualFile? destFile = OnCopyFile(sourceFile, destination, options ?? GorgonFileSystemCopyOptions.Default);

        if (destFile is null)
        {
            _log.PrintWarning($"File '{source}' was not copied.", LoggingLevel.Verbose);
            return;
        }

        _log.Print($"File '{source}' copied to '{destination}'.", LoggingLevel.Verbose);
        _fileCopied?.Invoke(this, new VirtualFileCopiedMovedEventArgs(sourceFile, destFile));
    }

    /// <inheritdoc/>
    public void RenameFile(string path, string newName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(newName);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> filePath = PrependPath(path.AsSpan()).FormatPath(DirectorySeparator);
        ReadOnlySpan<char> destName = Path.GetFileName(newName.AsSpan()).FormatFileName();

        if (destName.IsEmpty)
        {
            throw new ArgumentEmptyException(nameof(newName));
        }

        if (GetVirtualDirectory(Path.GetDirectoryName(filePath).FormatDirectory(DirectorySeparator)) is null)
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, Path.GetDirectoryName(filePath).FormatDirectory(DirectorySeparator).ToString()));
        }

        VirtualFile sourceFile = GetVirtualFile(filePath) ?? throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
        ReadOnlySpan<char> oldFilename = sourceFile.Name;

        CheckForReadOnlyProvider(sourceFile.MountPoint, filePath);

        VirtualDirectory sourceDir = sourceFile.Directory;
        ReadOnlySpan<char> destFilePath = string.Concat(sourceDir.FullPath, destName);

        if (GetVirtualDirectory(destFilePath.FormatDirectory(DirectorySeparator)) is not null)
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, destFilePath.ToString()));
        }

        VirtualFile? destFile = GetVirtualFile(destFilePath);

        if (destFile is not null)
        {
            throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, destFile.FullPath));
        }

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        IGorgonPhysicalFileInfo fileInfo = _writeProvider.RenameFile(filePath, destName, _writeMountPoint.PhysicalPath, _writeMountPoint.MountLocation);

        sourceDir.Files.Remove(sourceFile);
        destFile = sourceDir.Files.Add(_writeMountPoint, fileInfo);

        _log.Print($"File '{filePath}' renamed to '{destFile.FullPath}'.", LoggingLevel.Verbose);
        _fileRenamed?.Invoke(this, new VirtualFileRenamedEventArgs(destFile, oldFilename.ToString()));
    }

    /// <inheritdoc/>
    public void MoveFile(string sourcePath, string destinationPath, GorgonFileSystemCopyOptions? options = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(destinationPath);

        CheckReadOnlyFileSystem();

        ReadOnlySpan<char> source = PrependPath(sourcePath.AsSpan()).FormatPath(DirectorySeparator);
        ReadOnlySpan<char> destination = destinationPath.AsSpan();

        if (destination.Contains(DirectorySeparatorList, StringComparison.Ordinal))
        {
            destination = PrependPath(destination).FormatPath(DirectorySeparator);
        }
        else
        {
            destination = string.Concat(Path.GetDirectoryName(source), DirectorySeparatorList, destination).FormatPath(DirectorySeparator);
        }

        if (GetVirtualDirectory(Path.GetDirectoryName(destination).FormatDirectory(DirectorySeparator)) is null)
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, Path.GetDirectoryName(destination).FormatDirectory(DirectorySeparator).ToString()));
        }

        VirtualFile sourceFile = GetVirtualFile(source) ?? throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, sourcePath));

        CheckForReadOnlyProvider(sourceFile.MountPoint, source);

        // If we did not supply a file name, then provide the same name as the source.
        if (Path.GetFileName(destination).IsEmpty)
        {
            destination = string.Concat(destination, sourceFile.Name);
        }

        CheckForSameSourceAndDest(source, destination);

        _writeProvider.PrepareWriteArea(_writeMountPoint.PhysicalPath);

        VirtualFile? destFile = OnCopyFile(sourceFile, destination, options ?? GorgonFileSystemCopyOptions.Default);

        if (destFile is null)
        {
            _log.PrintWarning($"File '{source}' was not moved.", LoggingLevel.Verbose);
            return;
        }

        OnDeleteFile(sourceFile);

        _log.Print($"File '{source}' moved to '{destination}'.", LoggingLevel.Verbose);
        _fileMoved?.Invoke(this, new VirtualFileCopiedMovedEventArgs(sourceFile, destFile));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
    /// </summary>
    /// <param name="log">[Optional] The application log file.</param>
    /// <param name="writeProvider">[Optional] The file system provider that allows write access for this file system.</param>
    public GorgonFileSystem(IGorgonLog? log = null, IGorgonFileSystemWriteProvider? writeProvider = null)
    {
        _log = log ?? GorgonLog.NullLog;
        _mountPoints = [];
        _defaultProvider = new FolderFileSystemProvider(_log);
        _writeProvider = writeProvider ?? _defaultProvider;
        _rootDirectory = VirtualDirectory.CreateRoot(_defaultProvider, this);
    }
}
