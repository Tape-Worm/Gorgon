
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
// Created: Monday, June 27, 2011 9:00:18 AM
// 

using System.Buffers;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Properties;
using Gorgon.Math;

namespace Gorgon.IO.FileSystem.Providers;

/// <inheritdoc/>
internal sealed class FolderFileSystemProvider(IGorgonLog log)
        : GorgonFileSystemProvider(Resources.GORFS_FOLDER_FS_DESC, log), IGorgonFileSystemWriteProvider
{
    // String representation of the physical path separator.
    private static readonly string _physicalPathSeparatorString = Path.DirectorySeparatorChar.ToString();

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, GorgonFileExtension> PreferredExtensions
    {
        get;
    } = new Dictionary<string, GorgonFileExtension>();

    /// <inheritdoc/>
    public override char PhysicalPathSeparator => Path.DirectorySeparatorChar;

    // Block size for copies.
    private const int MaxBufferSize = 262_144;

    /// <summary>
    /// Function to delete a file from a given path.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    private void DeletePartialFile(string path)
    {
        int counter = 0;

        try
        {
            // No point to do this if the file isn't around on the file system.
            if (!File.Exists(path))
            {
                return;
            }

            while (counter < 5)
            {
                try
                {
                    File.Delete(path);
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    // If we have an access denied, then leave because there's nothing we can do.
                    break;
                }
                catch
                {
                    // Attempt to delete the file 5 times.
                    ++counter;

                    // Pause to let the system catch up.
                    if (counter < 5)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.PrintError($"There was an error trying to delete the file '{path}'.", LoggingLevel.All);
            Log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to perform the copy of a file.
    /// </summary>
    /// <param name="srcPath">The path for the source stream.</param>
    /// <param name="destPhysicalPath">The physical path to the destination file.</param>
    /// <param name="inStream">The source stream to copy.</param>
    /// <param name="outStream">The destination stream for the file data.</param>
    /// <param name="options">Options used in the copy operation.</param>
    /// <returns><b>true</b> if the file copied successfully, <b>false</b> if the file was not copied.</returns>
    /// <remarks>
    /// <para>
    /// This performs a block copy of a file from one location to another. These locations can be anywhere on the physical file system(s) and are not checked.  
    /// </para>
    /// </remarks>    
    private bool BlockCopyStreams(string srcPath, string destPhysicalPath, Stream inStream, Stream outStream, GorgonFileSystemCopyOptions options)
    {
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);

        try
        {
            long maxBlockSize = MaxBufferSize;

            options.ProgressCallback?.Invoke(srcPath, 0);

            if (options.CancellationToken.IsCancellationRequested)
            {
                return false;
            }

            long fileSize = inStream.Length;

            // If we're under the size of the write buffer, then we can copy as-is, and we can report a 1:1 file copy.
            if (fileSize <= MaxBufferSize)
            {
                inStream.CopyToStream(outStream, (int)fileSize, writeBuffer);
                options.ProgressCallback?.Invoke(srcPath, 1.0);
                return true;
            }

            // Otherwise, we need to break up the file into chunks to get reporting of file copy progress.
            long blockSize = maxBlockSize.Min(fileSize);

            while (fileSize > 0)
            {
                if (options.CancellationToken.IsCancellationRequested)
                {
                    outStream.Close();

                    // If we cancel on this file, then we need to delete it from the physical system.
                    DeletePartialFile(destPhysicalPath);
                    return false;
                }

                inStream.CopyToStream(outStream, (int)blockSize, writeBuffer);
                fileSize -= blockSize;
                blockSize = maxBlockSize.Min(fileSize);

                options.ProgressCallback?.Invoke(srcPath, (double)inStream.Position / inStream.Length);
            }

            return true;
        }
        catch
        {
            DeletePartialFile(destPhysicalPath);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(writeBuffer, true);
        }
    }

    /// <inheritdoc/>
    protected override ReadOnlySpan<char> OnMapToVirtualPath(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootPath)
    {
        if (!Path.IsPathRooted(physicalPath))
        {
            physicalPath = Path.GetFullPath(physicalPath.ToString()).AsSpan();
        }

        if (!Path.IsPathRooted(physicalMountPoint))
        {
            physicalMountPoint = Path.GetFullPath(physicalMountPoint.ToString()).AsSpan();
        }

        physicalPath = physicalPath.FormatPath(PhysicalPathSeparator);
        physicalMountPoint = physicalMountPoint.FormatDirectory(PhysicalPathSeparator);

        // If the physical path is not in the root of the physical mount point, then we can't map it.
        if (!physicalPath.StartsWith(physicalMountPoint, StringComparison.OrdinalIgnoreCase))
        {
            Log.PrintError($"The physical path '{physicalPath}' is not under the physical mount point path '{physicalMountPoint}'. This path cannot be mapped.", LoggingLevel.Verbose);
            return [];
        }

        // Strip off the physical mount point part of the path.
        physicalPath = physicalPath[physicalMountPoint.Length..];
        char[] buffer = new char[virtualRootPath.Length + physicalPath.Length];
        Span<char> result = buffer.AsSpan();

        virtualRootPath.CopyTo(result);
        physicalPath.CopyTo(result[virtualRootPath.Length..]);

        // Ensure all separators are correct.
        result.Replace(PhysicalPathSeparator, GorgonFileSystem.DirectorySeparator);

        return result;
    }

    /// <inheritdoc/>
    protected override GorgonFileSystemStream? OnOpenReadFileStream(IGorgonPhysicalFileInfo fileInfo, ReadOnlySpan<char> physicalMountPoint, Action<string, string>? onCloseCallback)
    {
        if (!File.Exists(fileInfo.FullPath))
        {
            return null;
        }

        return new GorgonFileSystemStream(fileInfo,
                                          File.Open(fileInfo.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                                          onCloseCallback);
    }

    /// <inheritdoc/>
    protected override bool OnCanReadFileSystem(ReadOnlySpan<char> physicalPath)
    {
        try
        {
            DirectoryInfo info = new(physicalPath.FormatDirectory(PhysicalPathSeparator).ToString());

            if ((info.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
            {
                Log.PrintError($"The directory '{physicalPath}' is not a directory.", LoggingLevel.All);
                return false;
            }

            info.Refresh();

            if (!info.Exists)
            {
                Log.PrintError($"The directory '{physicalPath}' does not exist.", LoggingLevel.All);
                return false;
            }

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            Log.PrintError($"The directory '{physicalPath}' is inaccessible. Access is denied.", LoggingLevel.All);
            return false;
        }
        catch (Exception ex)
        {
            Log.PrintError($"An error occurred while attempting to read the directory '{physicalPath}'.", LoggingLevel.All);
            Log.LogException(ex);
            return false;
        }
    }

    /// <inheritdoc/>
    protected override GorgonPhysicalFileSystemData OnEnumerate(ReadOnlySpan<char> physicalLocation, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> mountDirectory)
    {
        string fullPath;

        if (!Path.IsPathRooted(physicalLocation))
        {
            fullPath = Path.GetFullPath(physicalLocation.ToString()).AsSpan().FormatDirectory(PhysicalPathSeparator).ToString();
        }
        else
        {
            fullPath = physicalLocation.FormatDirectory(PhysicalPathSeparator).ToString();
        }

        DirectoryInfo directoryInfo = new(fullPath);

        IEnumerable<DirectoryInfo> directories =
            directoryInfo.GetDirectories("*", SearchOption.AllDirectories)
                         .Where(item =>
                                (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                                (item.Attributes & FileAttributes.System) != FileAttributes.System);

        IEnumerable<FileInfo> files = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                                                   .Where(item =>
                                                          (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                                                          (item.Attributes & FileAttributes.System) != FileAttributes.System &&
                                                          (item.Attributes & FileAttributes.Compressed) != FileAttributes.Compressed &&
                                                          (item.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted &&
                                                          (item.Attributes & FileAttributes.Device) != FileAttributes.Device);

        List<(string PhysicalPath, string VirtualPath)> directoryList = [];
        List<IGorgonPhysicalFileInfo> fileList = [];

        foreach (DirectoryInfo directory in directories)
        {
            ReadOnlySpan<char> formattedDirectoryPath = directory.FullName.AsSpan().FormatDirectory(PhysicalPathSeparator);
            string virtualPath = OnMapToVirtualPath(formattedDirectoryPath,
                                                    physicalMountPoint,
                                                    mountDirectory).ToString();

            directoryList.Add((formattedDirectoryPath.ToString(), virtualPath));
        }

        foreach (FileInfo file in files)
        {
            string virtualPath = OnMapToVirtualPath(file.FullName.AsSpan().FormatPath(PhysicalPathSeparator),
                                                    physicalMountPoint,
                                                    mountDirectory).ToString();

            fileList.Add(new PhysicalFileInfo(file, virtualPath));
        }

        return new(directoryList, fileList);
    }

    /// <inheritdoc/>
    protected override ReadOnlySpan<char> OnMapToPhysicalPath(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootPath)
    {
        if (!Path.IsPathRooted(physicalMountPoint))
        {
            physicalMountPoint = Path.GetFullPath(physicalMountPoint.ToString()).AsSpan();
        }

        physicalMountPoint = physicalMountPoint.FormatDirectory(PhysicalPathSeparator);

        int stringSize = (virtualPath.Length - virtualRootPath.Length) + physicalMountPoint.Length;
        char[] buffer = new char[stringSize];
        Span<char> result = buffer.AsSpan();
        ReadOnlySpan<char> trimmedPath = virtualPath[virtualRootPath.Length..];

        physicalMountPoint.CopyTo(result);
        trimmedPath.CopyTo(result[physicalMountPoint.Length..]);
        result.Replace(GorgonFileSystem.DirectorySeparator, PhysicalPathSeparator);

        return result;
    }

    /// <inheritdoc/>
    public GorgonFileSystemStream OpenWriteFileStream(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory, Action<string, string>? onCloseCallback)
    {
        ArgumentEmptyException.ThrowIfEmpty(virtualPath);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (virtualPath[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualPath = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualPath).AsSpan();
        }

        virtualPath = virtualPath.FormatPath(GorgonFileSystem.DirectorySeparator);

        string physicalPath = OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory).ToString();
        string directory = Path.GetDirectoryName(physicalPath) ?? throw new DirectoryNotFoundException();

        if ((!string.IsNullOrWhiteSpace(directory)) && (!Directory.Exists(directory)))
        {
            Directory.CreateDirectory(directory);
        }

        FileStream stream = File.Open(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
        return new GorgonFileSystemStream(virtualPath.ToString(), physicalPath, stream, onCloseCallback);
    }

    /// <inheritdoc/>
    public bool CanWriteFileSystem(ReadOnlySpan<char> physicalPath)
    {
        try
        {
            if (!Path.IsPathRooted(physicalPath))
            {
                physicalPath = Path.GetFullPath(physicalPath.ToString()).AsSpan();
            }

            physicalPath = physicalPath.FormatDirectory(Path.DirectorySeparatorChar);

            string tempPath = Path.Combine(physicalPath.ToString(), $"WriteCheck_{Guid.NewGuid().ToString():N}");
            DirectoryInfo dir = Directory.CreateDirectory(tempPath);
            dir.Attributes |= FileAttributes.Hidden;

            // Give explorer some time to catch up. Sometimes it's a jerk and locks stuff
            Thread.Sleep(100);

            dir.Delete(true);

            return true;
        }
        catch (Exception ex)
        {
            Log.PrintError($"There was an error attempting to write into '{physicalPath}'.", LoggingLevel.Simple);
            Log.LogException(ex);

            return false;
        }
    }

    /// <inheritdoc/>
    public string CreateDirectory(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        ArgumentEmptyException.ThrowIfEmpty(virtualPath);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (virtualPath[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualPath = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualPath).AsSpan();
        }

        virtualPath = virtualPath.FormatPath(GorgonFileSystem.DirectorySeparator);

        string physicalPath = OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory).ToString();

        if (!Directory.Exists(physicalPath))
        {
            Directory.CreateDirectory(physicalPath);
        }

        return physicalPath;
    }

    /// <inheritdoc/>
    public void DeleteDirectory(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        ArgumentEmptyException.ThrowIfEmpty(virtualPath);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (virtualPath[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualPath = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualPath).AsSpan();
        }

        virtualPath = virtualPath.FormatPath(GorgonFileSystem.DirectorySeparator);

        string physicalPath = OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory).ToString();

        if (!Directory.Exists(physicalPath))
        {
            Log.PrintWarning($"The directory '{physicalPath}' does not exist on the physical file system.", LoggingLevel.Intermediate);
            return;
        }

        Directory.Delete(physicalPath, true);
    }

    /// <inheritdoc/>
    public void DeleteFile(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        ArgumentEmptyException.ThrowIfEmpty(virtualPath);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (virtualPath[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualPath = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualPath).AsSpan();
        }

        virtualPath = virtualPath.FormatPath(GorgonFileSystem.DirectorySeparator);

        string physicalPath = OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory).ToString();

        if (!File.Exists(physicalPath))
        {
            Log.PrintWarning($"The file '{physicalPath}' does not exist on the physical file system.", LoggingLevel.Intermediate);
            return;
        }

        File.Delete(physicalPath);
    }

    /// <inheritdoc/>
    public string RenameDirectory(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> newDirectoryName, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        ArgumentEmptyException.ThrowIfEmpty(virtualPath);
        ArgumentEmptyException.ThrowIfEmpty(newDirectoryName);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (virtualPath[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualPath = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualPath).AsSpan();
        }

        virtualPath = virtualPath.FormatDirectory(GorgonFileSystem.DirectorySeparator);
        string physicalSourcePath = OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory)[..^1].ToString();

        if (!Directory.Exists(physicalSourcePath))
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, virtualPath.ToString()));
        }

        ReadOnlySpan<char> physicalSourceDir = Path.GetDirectoryName(physicalSourcePath.AsSpan());
        string newPhysicalDestPath = string.Concat(physicalSourceDir, _physicalPathSeparatorString, newDirectoryName).FormatDirectory(PhysicalPathSeparator);

        Directory.Move(physicalSourcePath, newPhysicalDestPath);

        return newPhysicalDestPath;
    }

    /// <inheritdoc/>
    public IGorgonPhysicalFileInfo RenameFile(ReadOnlySpan<char> sourceVirtualFile, ReadOnlySpan<char> newFileName, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        ArgumentEmptyException.ThrowIfEmpty(sourceVirtualFile);
        ArgumentEmptyException.ThrowIfEmpty(newFileName);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (sourceVirtualFile[0] != GorgonFileSystem.DirectorySeparator)
        {
            sourceVirtualFile = string.Concat(GorgonFileSystem.DirectorySeparatorList, sourceVirtualFile).AsSpan();
        }

        sourceVirtualFile = sourceVirtualFile.FormatPath(GorgonFileSystem.DirectorySeparator);

        string physicalSourcePath = OnMapToPhysicalPath(sourceVirtualFile, physicalMountPoint, virtualRootDirectory).ToString();

        if (!File.Exists(physicalSourcePath))
        {
            throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, sourceVirtualFile.ToString()));
        }

        ReadOnlySpan<char> physicalSourceDir = Path.GetDirectoryName(physicalSourcePath.AsSpan()).FormatDirectory(PhysicalPathSeparator);
        string newPhysicalDestPath = string.Concat(physicalSourceDir, newFileName);

        File.Move(physicalSourcePath, newPhysicalDestPath);

        return new PhysicalFileInfo(new FileInfo(newPhysicalDestPath), OnMapToVirtualPath(newPhysicalDestPath, physicalMountPoint, virtualRootDirectory).ToString());
    }

    /// <inheritdoc/>
    public IGorgonPhysicalFileInfo CopyFile(IGorgonVirtualFile sourceFile, ReadOnlySpan<char> destinationPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory, GorgonFileSystemCopyOptions options)
    {
        ArgumentEmptyException.ThrowIfEmpty(destinationPath);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory[0] != GorgonFileSystem.DirectorySeparator)
        {
            virtualRootDirectory = string.Concat(GorgonFileSystem.DirectorySeparatorList, virtualRootDirectory).AsSpan();
        }

        virtualRootDirectory = virtualRootDirectory.FormatDirectory(GorgonFileSystem.DirectorySeparator);

        if (destinationPath[0] != GorgonFileSystem.DirectorySeparator)
        {
            destinationPath = string.Concat(GorgonFileSystem.DirectorySeparatorList, destinationPath).AsSpan();
        }

        destinationPath = destinationPath.FormatPath(GorgonFileSystem.DirectorySeparator);

        string physicalPath = OnMapToPhysicalPath(destinationPath, physicalMountPoint, virtualRootDirectory).ToString();
        string destinationDirPath = Path.GetDirectoryName(physicalPath) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, string.Empty));

        // If the physical path directory does not exist, then create it now.
        if (!Directory.Exists(destinationDirPath))
        {
            Directory.CreateDirectory(destinationDirPath);
        }

        using Stream sourceStream = sourceFile.MountPoint.Provider.OpenReadFileStream(sourceFile.PhysicalFile, sourceFile.MountPoint.PhysicalPath, null)
            ?? throw new IOException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, sourceFile.FullPath));
        using FileStream destStream = File.Open(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);

        if (!BlockCopyStreams(sourceFile.FullPath, physicalPath, sourceStream, destStream, options))
        {
            throw new Exception(string.Format(Resources.GORFS_ERR_COPY_FAILED, sourceFile.FullPath, destinationPath.ToString()));
        }

        return new PhysicalFileInfo(new FileInfo(physicalPath), destinationPath.ToString());
    }

    /// <inheritdoc/>
    public void PrepareWriteArea(ReadOnlySpan<char> physicalPath)
    {
        string path = physicalPath.ToString();

        // If the area specified by the write directory does not exist yet, then 
        // create it.
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
