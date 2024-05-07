
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
// Created: Monday, June 27, 2011 8:54:59 AM
// 

using System.Buffers;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;
using Gorgon.Math;
using Gorgon.Memory;

namespace Gorgon.IO;

/// <summary>
/// The virtual file System interface
/// </summary>
/// <remarks>
/// <para>
/// This will allow the user to mount directories or packed files (such as Zip files) into a unified file system.  For example, if the user has mounted MyData.zip and C:\users\Bob\Data\ into the file 
/// system then all files and/or directories from both sources would be combined into a single virtual file system. This has the advantage of being able to access disparate file systems without having 
/// to run through multiple interfaces to get at their data
/// </para>
/// <para>
/// The virtual file system is a read only file system. This is done by design so that the integrity of the original physical file systems can be preserved. If your application needs to write data into 
/// the file system, then the <see cref="IGorgonFileSystemWriter{T}"/> has been provided to give access to a writable area that will integrate with this object
/// </para>
/// <para>
/// Physical file systems (such as a windows directory or a Zip file) are "mounted" into this object. When a physical file system is mounted, all of the file names (and other info) and directory names 
/// will be stored in hierarchal structure similar to a Unix directory structure. Because of this, there will be some differences from the typical Windows directory setup:
/// <list type="bullet">
/// <item>
///		<description>The root of the file system is not "C:\" or "\\Computer\". In this object, the root is '/' (e.g. <c>/MyFile.txt</c> is a file located in the root).</description> 
/// </item>
/// <item>
///		<description>The directory separators are forward slashes: / and not back slashes: \. (e.g. <c>c:\MyDirectory\MySubDirectory\</c> is now <c><![CDATA[/MyDirectory/MySubDirectory/]]></c>)</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// The order in which file systems are mounted into the virtual file system is important.  If a zip file contains SomeText.txt, and a directory contains the same file path, then if the zip file is 
/// mounted, followed by the directory, the file in the directory will override the file in the zip file. This is true for directories as well. 
/// </para>
/// </note>
/// </para>
/// <para>
/// By default, a new file system instance will only have access to the directories and files of the hard drive via the default provider. File systems that are in packed files (e.g. Zip files) can be 
/// loaded into the file system by way of a <see cref="GorgonFileSystemProvider"/>. Providers are typically plug-in objects that are loaded into the file system via the <see cref="GorgonFileSystemProviderFactory"/>.  
/// Once a provider plug-in is loaded, then the contents of that file system can be mounted like a standard directory. 
/// </para>
/// <para>
/// When a file system provider is added to the virtual file system object upon creation, the object will contain 2 providers, the default provider is always available with any additional providers
/// </para>
/// </remarks>
/// <seealso cref="IGorgonFileSystemWriter{T}"/>
/// <seealso cref="GorgonFileSystemProvider"/>
/// <example>
/// This example shows how to create a file system with the default provider, and mount a directory to the root:
/// <code language="csharp">
/// <![CDATA[
/// IGorgonFileSystem fileSystem = new GorgonFileSystem();
/// 
/// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
/// ]]>
/// </code>
/// This example shows how to load a provider from the provider factory and use it with the file system:
/// <code language="csharp">
/// <![CDATA[
/// // First we need to load the assembly with the provider plug-in
/// using (GorgonPlugInAssemblyCache assemblies = new GorgonPlugInAssemblyCache())
/// {
///		assemblies.Load(@"C:\PlugIns\GorgonFileSystem.Zip.dll"); 
///		GorgonPlugInService plugInService = new GorgonPlugInService(assemblies);
/// 
///		// We'll use the factory to get the zip plug-in provider
///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(plugInService);
///		
///		IGorgonFileSystemProvider zipProvider = factory.CreateProvider("Gorgon.IO.Zip.ZipProvider");
/// 
///		// Now create the file system with the zip provider
///		IGorgonFileSystem fileSystem = new GorgonFileSystem(zipProvider);
///		fileSystem.Mount(@"C:\ZipFiles\MyFileSystem.zip", "/");
/// }  
/// ]]>
/// </code>
/// </example>
public class GorgonFileSystem
    : IGorgonFileSystem, IGorgonFileSystemNotifier
{
    // The maximum size for the working buffers.
    private const int MaxBufferSize = 262_144;

    /// <summary>
    /// The character used to separate directory names and file names in paths.
    /// </summary>
    public const char DirectorySeparator = '/';

    /// <summary>
    /// The character used to separate directory names and file names in paths.
    /// </summary>
    internal static readonly char[] Separator = ['/'];

    /// <summary>
    /// The character used to separate directory names and file names in paths.
    /// </summary>
    internal static readonly string SeparatorString = "/";

    // Synchronization object.
    private readonly object _syncLock = new();
    // The list of providers available to the file system.
    private readonly Dictionary<string, IGorgonFileSystemProvider> _providers;
    // The log file for the application.
    private readonly IGorgonLog _log;
    // A list of mount points grouped by provider.
    private readonly HashSet<GorgonFileSystemMountPoint> _mountProviders;
    // A read/write version of the RootDirectory property.
    private readonly VirtualDirectory _rootDirectory;

    /// <summary>
    /// Property to return the <see cref="IGorgonFileSystemProvider"/> installed in this file system.
    /// </summary>
    public IEnumerable<IGorgonFileSystemProvider> Providers => _providers.Select(item => item.Value);

    /// <summary>
    /// Property to return the default file system provider for this file system.
    /// </summary>
    /// <remarks>
    /// This is the default folder file system provider.
    /// </remarks>
    public IGorgonFileSystemProvider DefaultProvider
    {
        get;
    }

    /// <summary>
    /// Property to return a list of mount points that are currently assigned to this file system.
    /// </summary>
    /// <remarks>
    /// This is a list of <see cref="GorgonFileSystemMountPoint"/> values. These values contain location of the mount point in the virtual file system, the physical location of the physical file system and 
    /// the provider that mounted the physical file system.
    /// </remarks>
    public IEnumerable<GorgonFileSystemMountPoint> MountPoints => _mountProviders;

    /// <summary>
    /// Property to return the root directory for the file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the beginning of the directory/file structure for the file system. Users can walk through the properties on this object to get the sub directories and files for a virtual file system as a 
    /// hierarchical view.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// When populating a tree view, this property is useful for helping to lay out the nodes in the tree.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public IGorgonVirtualDirectory RootDirectory => _rootDirectory;

    /// <summary>
    /// Function to mount a file as a mount point in the virtual file system.
    /// </summary>
    /// <param name="physicalPath">The physical path to the file.</param>
    /// <param name="mountPath">The path to mount under.</param>
    /// <returns>A new mount point.</returns>
    private GorgonFileSystemMountPoint MountFile(string physicalPath, string mountPath)
    {
        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, physicalPath));
        }

        IGorgonFileSystemProvider provider = _providers.FirstOrDefault(item => item.Value.CanReadFileSystem(physicalPath)).Value ?? throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_READ_FILESYSTEM, physicalPath));

        GorgonFileSystemMountPoint mountPoint = new(provider, physicalPath, mountPath);

        GetFileSystemObjects(mountPoint);

        _mountProviders.Remove(mountPoint);

        _mountProviders.Add(mountPoint);

        return mountPoint;
    }

    /// <summary>
    /// Function to mount an operating system directory as a mount point in the virtual file system.
    /// </summary>
    /// <param name="physicalPath">The physical path to the operating system directory.</param>
    /// <param name="mountPath">The path to mount under.</param>
    /// <returns>A new mount point.</returns>
    private GorgonFileSystemMountPoint MountDirectory(string physicalPath, string mountPath)
    {
        if ((string.IsNullOrWhiteSpace(physicalPath)) || (!Directory.Exists(physicalPath)))
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, physicalPath));
        }

        GorgonFileSystemMountPoint mountPoint = new(DefaultProvider, physicalPath, mountPath);

        GetFileSystemObjects(mountPoint);

        _mountProviders.Remove(mountPoint);

        // Assign the default provider to the mount point.
        _mountProviders.Add(mountPoint);

        return mountPoint;
    }

    /// <summary>
    /// Function to mount a non physical location in the file system.
    /// </summary>
    /// <param name="location">The non physical location to mount.</param>
    /// <param name="mountDirectory">The virtual directory to mount in.</param>
    /// <returns>A <see cref="GorgonFileSystemMountPoint"/>.</returns>
    private GorgonFileSystemMountPoint MountNonPhysicalLocation(string location, string mountDirectory)
    {
        IGorgonFileSystemProvider provider = _providers.FirstOrDefault(item => item.Value.CanReadFileSystem(location)).Value ?? throw new IOException(string.Format(Resources.GORFS_ERR_CANNOT_READ_FILESYSTEM, location));

        GorgonFileSystemMountPoint mountPoint = new(provider, location, mountDirectory, true);

        GetFileSystemObjects(mountPoint);

        _mountProviders.Remove(mountPoint);

        // Assign the default provider to the mount point.
        _mountProviders.Add(mountPoint);

        return mountPoint;
    }

    /// <summary>
    /// Function to retrieve the file system objects from the physical file system.
    /// </summary>
    /// <param name="mountPoint">The mount point to link the physical file system with the virtual file system.</param>
    private void GetFileSystemObjects(GorgonFileSystemMountPoint mountPoint)
    {
        ReadOnlySpan<char> physicalPath = mountPoint.IsFakeMount ? mountPoint.PhysicalPath.AsSpan() : Path.GetFullPath(mountPoint.PhysicalPath).AsSpan();
        ReadOnlySpan<char> fileName = mountPoint.IsFakeMount ? [] : Path.GetFileName(physicalPath).FormatFileName();

        if (!mountPoint.IsFakeMount)
        {
            physicalPath = Path.GetDirectoryName(physicalPath).FormatDirectory(Path.DirectorySeparatorChar);

            if (!fileName.IsEmpty)
            {
                physicalPath = string.Concat(physicalPath, fileName).AsSpan();
            }
        }

        if (physicalPath.IsEmpty)
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, physicalPath.ToString()));
        }

        // Find existing mount point.
        VirtualDirectory? mountDirectory = GetVirtualDirectory(mountPoint.MountLocation.AsSpan());

        if (mountDirectory is null)
        {
            mountDirectory = _rootDirectory.Directories.Add(mountPoint, mountPoint.MountLocation);
        }
        else
        {
            mountDirectory.MountPoint = mountPoint;
        }

        _log.Print($"Mounting physical file system path '{physicalPath}' to virtual file system path '{mountPoint.MountLocation}'.", LoggingLevel.Simple);

        GorgonPhysicalFileSystemData data = mountPoint.Provider.Enumerate(physicalPath.ToString(), mountDirectory);

        // Process the directories.
        foreach (string directory in data.Directories)
        {
            VirtualDirectory? existingDirectory = GetVirtualDirectory(directory.AsSpan());

            // If the directory path already exists for another provider, then override it with the 
            // provider we're currently loading. All directories and files will be overridden by the last 
            // provider loaded if they already exist.
            if (existingDirectory is null)
            {
                _rootDirectory.Directories.Add(mountPoint, directory);
            }
            else
            {
                _log.PrintWarning($"\"{existingDirectory.FullPath}\" already exists in provider: " +
                                  $"\"{existingDirectory.MountPoint.Provider.Description}\". Changing provider to \"{mountPoint.Provider.Description}\"",
                                  LoggingLevel.Verbose);

                existingDirectory.MountPoint = mountPoint;
            }
        }

        // Process the files.
        foreach (IGorgonPhysicalFileInfo fileInfo in data.Files)
        {
            ReadOnlySpan<char> directoryName = Path.GetDirectoryName(fileInfo.VirtualPath.AsSpan()).FormatDirectory(DirectorySeparator);

            if (directoryName.IsEmpty)
            {
                continue;
            }

            if (!directoryName.StartsWith(Separator, StringComparison.OrdinalIgnoreCase))
            {
                directoryName = string.Concat(Separator, directoryName).AsSpan();
            }

            VirtualDirectory directory = GetVirtualDirectory(directoryName) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryName.ToString()));

            // Update the file information to the most recent provider.
            if (!directory.Files.TryGetVirtualFile(fileInfo.Name.AsSpan(), out VirtualFile? virtualFile))
            {
                directory.Files.Add(mountPoint, fileInfo);
            }
            else
            {
                _log.PrintWarning($"\"{fileInfo.FullPath}\" already exists in provider: " +
                                  $"\"{virtualFile.MountPoint.Provider.Description}\". Changing provider to \"{mountPoint.Provider.Description}\"",
                                  LoggingLevel.Verbose);

                virtualFile.MountPoint = mountPoint;
                virtualFile.PhysicalFile = fileInfo;
            }
        }

        _log.Print($"{data.Directories.Count} directories parsed, and {data.Files.Count} files processed.", LoggingLevel.Simple);
    }

    /// <summary>
    /// Function to determine if the name of an object matches the pattern specified.
    /// </summary>
    /// <typeparam name="T">Type of named object to compare.</typeparam>
    /// <param name="item">Item to compare.</param>
    /// <param name="searchMask">The mask used to filter.</param>
    /// <returns><b>true</b> if the name of the item matches the pattern, <b>false</b> if not.</returns>
    private static bool IsPatternMatch<T>(T item, string searchMask)
        where T : IGorgonNamedObject
    {
        if ((searchMask.EndsWith("*", StringComparison.OrdinalIgnoreCase)) || (!searchMask.StartsWith("*", StringComparison.OrdinalIgnoreCase)) || (searchMask.Length != 1))
        {
            return Regex.IsMatch(item.Name,
                                 Regex.Escape(searchMask).Replace(@"\*", ".*").Replace(@"\?", "."),
                                 RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        return item.Name.EndsWith(searchMask[(searchMask.IndexOf('*') + 1)..], StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Function to flatten a file hierarchy.
    /// </summary>
    /// <param name="directory">The directory to start searching in.</param>
    /// <param name="searchMask">The pattern mask to search on.</param>
    /// <returns>An enumerable containing the flattened list.</returns>
    private static IEnumerable<VirtualFile> FlattenFileHierarchy(VirtualDirectory directory, string searchMask)
    {
        IEnumerable<VirtualDirectory> directories = FlattenDirectoryHierarchy(directory, "*");

        foreach (VirtualFile file in directory.Files.EnumerateVirtualFiles().Where(item => IsPatternMatch(item, searchMask)))
        {
            yield return file;
        }

        foreach (VirtualFile file in directories.SelectMany(subDirectory => subDirectory.Files.EnumerateVirtualFiles()
                                                                                              .Where(item => IsPatternMatch(item, searchMask))))
        {
            yield return file;
        }
    }

    /// <summary>
    /// Function to return the list of <see cref="VirtualDirectory"/> objects for this file system.
    /// </summary>
    /// <param name="path">Path to iterate.</param>
    /// <param name="directoryMask">The mask to use when filtering entries.</param>
    /// <param name="recursive">true to recursively evaluate directories, false to just evaluate the top directory.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing <see cref="VirtualDirectory"/> objects.</returns>
    private IEnumerable<VirtualDirectory> FindVirtualDirectories(ReadOnlySpan<char> path, string directoryMask, bool recursive)
    {
        VirtualDirectory? startDirectory = GetVirtualDirectory(path);

        return startDirectory is null
            ? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path.ToString()))
            : !recursive
                   ? (string.Equals(directoryMask, "*", StringComparison.OrdinalIgnoreCase)
                          ? startDirectory.Directories.EnumerateVirtualDirectories()
                          : startDirectory.Directories.EnumerateVirtualDirectories()
                                          .Where(item => IsPatternMatch(item, directoryMask)))
                   : FlattenDirectoryHierarchy(startDirectory, directoryMask);
    }

    /// <summary>
    /// Function to return the list of <see cref="VirtualFile"/> objects for this file system.
    /// </summary>
    /// <param name="path">Path to iterate.</param>
    /// <param name="fileMask">The mask to use when filtering entries.</param>
    /// <param name="recursive">true to recursively evaluate directories, false to just evaluate the top directory.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing <see cref="VirtualFile"/> objects.</returns>
    private IEnumerable<VirtualFile> FindVirtualFiles(ReadOnlySpan<char> path, string fileMask, bool recursive)
    {
        VirtualDirectory? start = GetVirtualDirectory(path);

        return start is null
            ? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path.ToString()))
            : !recursive
                   ? (string.Equals(fileMask, "*", StringComparison.OrdinalIgnoreCase)
                          ? start.Files.EnumerateVirtualFiles()
                          : start.Files.EnumerateVirtualFiles().Where(item => IsPatternMatch(item, fileMask)))
                   : FlattenFileHierarchy(start, fileMask);
    }

    /// <summary>
    /// Function to retrieve a writable <see cref="VirtualFile"/> entry from the file system.
    /// </summary>
    /// <param name="path">The path to the file entry.</param>
    /// <returns>The file entry if found, null if not.</returns>
    private VirtualFile? GetVirtualFile(ReadOnlySpan<char> path)
    {
        ReadOnlySpan<char> separatorSpan = Separator.AsSpan();

        if (!path.StartsWith(Separator, StringComparison.Ordinal))
        {
            path = string.Concat(separatorSpan, path).AsSpan();
        }

        // Get path parts.        
        ReadOnlySpan<char> directory = Path.GetDirectoryName(path);

        directory = directory.IsEmpty ? separatorSpan : directory;

        ReadOnlySpan<char> filename = Path.GetFileName(path);

        // Check for file name.
        if (filename.IsEmpty)
        {
            throw new ArgumentException(string.Format(Resources.GORFS_ERR_NO_FILENAME, path.ToString()), nameof(path));
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
        if (path.IsEmpty)
        {
            return null;
        }

        // Optimization to deal with the root path.
        if (path.Equals(Separator, StringComparison.Ordinal))
        {
            return _rootDirectory;
        }

        int pathPartCount = path.Count(DirectorySeparator);

        if (pathPartCount == 0)
        {
            return null;
        }

        pathPartCount++;

        Span<Range> ranges = stackalloc Range[pathPartCount.Min(4096)];
        Range[]? rangeArray = null;
        ArrayPool<Range>? rangePool = null;

        try
        {
            // If we have a LOT of path parts, then allocate on the heap anyway because who cares at this point.
            if (pathPartCount > 4096)
            {
                rangePool = GorgonArrayPools<Range>.GetBestPool(pathPartCount);
                rangeArray = rangePool.Rent(pathPartCount);
                ranges = rangeArray.AsSpan(0, pathPartCount);
                ranges.Clear();
            }

            int splitCount = path.Split(ranges, DirectorySeparator, StringSplitOptions.RemoveEmptyEntries);

            if (splitCount == 0)
            {
                return null;
            }

            VirtualDirectory directory = _rootDirectory;

            for (int i = 0; i < splitCount; i++)
            {
                ReadOnlySpan<char> directoryName = path[ranges[i]];

                if (!directory.Directories.TryGetValue(directoryName.ToString(), out VirtualDirectory? childDirectory))
                {
                    return null;
                }

                // We could have this in the TryGetValue as 'out directory', but this is more readable.
                directory = childDirectory;
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
    /// Function to flatten a directory hierarchy.
    /// </summary>
    /// <param name="directory">The directory to start searching in.</param>
    /// <param name="searchMask">The pattern mask to search on.</param>
    /// <returns>An enumerable containing the flattened list.</returns>
    internal static IEnumerable<VirtualDirectory> FlattenDirectoryHierarchy(VirtualDirectory directory, string searchMask)
    {
        IEnumerable<VirtualDirectory> directories = directory.Directories.EnumerateVirtualDirectories()
                                                                         .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories());

        if (!string.Equals(searchMask, "*", StringComparison.OrdinalIgnoreCase))
        {
            directories = directories.Where(item => IsPatternMatch(item, searchMask));
        }

        return directories;
    }

    /// <summary>Function to notify a file system that a new directory has been added.</summary>
    /// <param name="mountPoint">The mountpoint that triggered the update.</param>
    /// <param name="path">The path to the new directory.</param>
    /// <returns>The new (or existing) virtual directory corresponding to the physical path.</returns>
    IGorgonVirtualDirectory IGorgonFileSystemNotifier.NotifyDirectoryAdded(GorgonFileSystemMountPoint mountPoint, string path)
    {
        VirtualDirectory dir = _rootDirectory.Directories.Add(mountPoint, path);

        if (dir.MountPoint != mountPoint)
        {
            dir.MountPoint = mountPoint;
        }

        return dir;
    }

    /// <summary>
    /// Function to notify a file system that a directory as been deleted.
    /// </summary>        
    /// <param name="directoryPath">The path to the directory that was deleted.</param>
    void IGorgonFileSystemNotifier.NotifyDirectoryDeleted(string directoryPath)
    {
        VirtualDirectory? dir = GetVirtualDirectory(directoryPath.AsSpan());

        if (dir is null)
        {
            return;
        }

        // If this is a root directory, then just clear everything.
        if (dir.Parent is null)
        {
            _rootDirectory.Directories.Clear();
            _rootDirectory.Files.Clear();
            return;
        }

        dir.Parent.Directories.Remove(dir);
        dir.Refresh();
    }

    /// <summary>
    /// Function to notify a file system that a directory has been renamed.
    /// </summary>
    /// <param name="mountPoint">The mount point for the directory.</param>
    /// <param name="oldPath">The path to the directory that was renamed.</param>
    /// <param name="physicalPath">The physical path for the directory.</param>
    /// <param name="newName">The new name for the directory.</param>
    void IGorgonFileSystemNotifier.NotifyDirectoryRenamed(GorgonFileSystemMountPoint mountPoint, string oldPath, string physicalPath, string newName)
    {
        VirtualDirectory? dir = GetVirtualDirectory(oldPath.AsSpan());

        if (dir?.Parent is null)
        {
            return;
        }

        // Function to update the physical file information for each file in a directory.
        void UpdatePhysicalFileInfo(GorgonFileSystemMountPoint mount, VirtualDirectory directory)
        {
            ReadOnlySpan<char> path = mount.Provider.MapToPhysicalPath(directory.FullPath.AsSpan(), mountPoint);
            IReadOnlyDictionary<string, IGorgonPhysicalFileInfo> files = mountPoint.Provider.EnumerateFiles(path.ToString(), dir);

            // Update the physical file information on the files.
            foreach (KeyValuePair<string, IGorgonPhysicalFileInfo> fileInfo in files)
            {
                if (!directory.Files.TryGetVirtualFile(fileInfo.Value.Name.AsSpan(), out VirtualFile? file))
                {
                    // This shouldn't happen. 
                    _log.Print($"The file '{fileInfo.Value.Name}' was not found in the directory '{directory.FullPath}'.", LoggingLevel.All);
                    continue;
                }

                file.PhysicalFile = fileInfo.Value;

                if (file.MountPoint != mountPoint)
                {
                    file.MountPoint = mountPoint;
                }
            }
        }

        // Remove it from the list so the dictionary keys are fixed up.
        dir.Parent.Directories.Remove(dir);

        // Assign the new name.
        dir.Name = newName;
        if (dir.MountPoint != mountPoint)
        {
            dir.MountPoint = mountPoint;
        }

        UpdatePhysicalFileInfo(mountPoint, dir);

        // All files have a physical link, and since that physical link is no longer valid, we need to update the physical path for each file under
        // the directory and any subdirectories.
        foreach (VirtualDirectory subDir in dir.Directories.EnumerateVirtualDirectories().TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories()))
        {
            if (subDir.MountPoint != mountPoint)
            {
                subDir.MountPoint = mountPoint;
            }

            UpdatePhysicalFileInfo(mountPoint, subDir);
        }

        // Re-add to the list after it's been updated.
        dir.Parent.Directories.Add(dir);
        dir.Refresh();
    }

    /// <summary>
    /// Function to notify a file system that a file has been renamed.
    /// </summary>
    /// <param name="mountPoint">The mount point for the file.</param>
    /// <param name="oldPath">The path to the file that was renamed.</param>        
    /// <param name="fileInfo">Physical file information for the renamed file.</param>
    void IGorgonFileSystemNotifier.NotifyFileRenamed(GorgonFileSystemMountPoint mountPoint, string oldPath, IGorgonPhysicalFileInfo fileInfo)
    {
        VirtualFile? file = GetVirtualFile(oldPath.AsSpan());

        Debug.Assert(file is not null, $"The file with the previous name '{oldPath}' was not found. This should not happen.");

        file.Directory.Files.Remove(file);

        if (file.MountPoint != mountPoint)
        {
            file.MountPoint = mountPoint;
        }

        file.PhysicalFile = fileInfo;
        file.Directory.Files[file.Name] = file;
        file.Refresh();
    }

    /// <summary>
    /// Function to notify a file system that a file has been deleted.
    /// </summary>
    /// <param name="filePath">The path to the file that was deleted.</param>
    void IGorgonFileSystemNotifier.NotifyFileDeleted(string filePath)
    {
        VirtualFile? file = GetVirtualFile(filePath.AsSpan());

        if (file is null)
        {
            return;
        }

        file.Directory.Files.Remove(file);
    }

    /// <summary>Function to notify a file system that a previously opened write stream has been closed.</summary>
    /// <param name="mountPoint">The mountpoint that triggered the update.</param>
    /// <param name="fileInfo">The information about the physical file.</param>
    /// <returns>The file that was updated.</returns>
    IGorgonVirtualFile IGorgonFileSystemNotifier.NotifyFileWriteStreamClosed(GorgonFileSystemMountPoint mountPoint, IGorgonPhysicalFileInfo fileInfo)
    {
        ReadOnlySpan<char> path = fileInfo.VirtualPath.AsSpan();

        VirtualFile? file = GetVirtualFile(path);

        if (file is null)
        {
            ReadOnlySpan<char> directoryPath = Path.GetDirectoryName(path);
            VirtualDirectory? directory = GetVirtualDirectory(directoryPath);

            Debug.Assert(directory is not null, "Directory should not be null after a file has been closed.");

            return directory.Files.Add(mountPoint, fileInfo);
        }

        file.PhysicalFile = fileInfo;

        if (file.MountPoint != mountPoint)
        {
            file.MountPoint = mountPoint;
        }

        return file;
    }

    /// <summary>
    /// Function to find all the directories with the name specified by the directory mask.
    /// </summary>
    /// <param name="path">The path to the directory to start searching from.</param>
    /// <param name="directoryMask">The directory name or mask to search for.</param>
    /// <param name="recursive">[Optional] <b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
    /// <returns>An enumerable object containing <see cref="IGorgonVirtualDirectory"/> objects that match the <paramref name="directoryMask"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryMask"/> or the <paramref name="path"/> parameter are empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory in specified by the <paramref name="path"/> parameter was not found.</exception>
    /// <remarks>
    /// <para>
    /// This will look for all the directories specified by the <paramref name="directoryMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching 
    /// which can be used to find a series of directories. If the wild card is omitted, then only that name is sought out.
    /// </para> 
    /// <para>
    /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
    /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically.
    /// </para> 
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Calling <c>FindDirectories("/", "<![CDATA[/MyDir/ThisDir/C*w/]]>");</c> 
    /// will not return any results. Use the <paramref name="path"/> parameter to specify the origin for the search.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask, bool recursive = true)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        return FindVirtualDirectories(path.AsSpan().FormatDirectory(DirectorySeparator), directoryMask, recursive);
    }

    /// <summary>
    /// Function to find all the directories with the name specified by the directory mask.
    /// </summary>
    /// <param name="directoryMask">The directory name or mask to search for.</param>
    /// <param name="recursive">[Optional] <b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
    /// <returns>An enumerable object containing <see cref="IGorgonVirtualDirectory"/> objects that match the <paramref name="directoryMask"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryMask"/> parameter is empty.</exception>
    /// <remarks> 
    /// <para>
    /// This will look for all the directories specified by the <paramref name="directoryMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching 
    /// which can be used to find a series of directories. If the wild card is omitted, then only that name is sought out.
    /// </para> 
    /// <para>
    /// Unlike the <see cref="FindDirectories(string,string,bool)"/> overload, this method starts searching from the root of the virtual file system.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Calling <c>FindDirectories("/", "<![CDATA[/MyDir/ThisDir/C*w/]]>");</c> 
    /// will not return any results. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="FindDirectories(string,string,bool)"/>
    public IEnumerable<IGorgonVirtualDirectory> FindDirectories(string directoryMask, bool recursive = true) => FindDirectories(DirectorySeparator.ToString(), directoryMask, recursive);

    /// <summary>
    /// Function to find all the files with the name specified by the file mask.
    /// </summary>
    /// <param name="path">The path to the directory to start searching from.</param>
    /// <param name="fileMask">The file name or mask to search for.</param>
    /// <param name="recursive">[Optional] <b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
    /// <returns>An enumerable object containing <see cref="IGorgonVirtualFile"/> objects that match the <paramref name="fileMask"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> are empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory in specified by the <paramref name="path"/> parameter was not found.</exception>
    /// <remarks>
    /// <para>
    /// This will look for all the files specified by the <paramref name="fileMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching which can be 
    /// used to find a series of files. If the wild card is omitted, then only that name is sought out.
    /// </para> 
    /// <para>
    /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
    /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically.
    /// </para> 
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Calling <c>FindFiles("/", "<![CDATA[/MyDir/C*w.txt]]>");</c> will not return 
    /// any results. Use the <paramref name="path"/> parameter to specify the origin for the search.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask, bool recursive = true)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        return FindVirtualFiles(path.AsSpan().FormatDirectory(DirectorySeparator), fileMask, recursive);
    }

    /// <summary>
    /// Function to find all the files with the name specified by the file mask.
    /// </summary>
    /// <param name="fileMask">The file name or mask to search for.</param>
    /// <param name="recursive">[Optional] <b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
    /// <returns>An enumerable object containing <see cref="IGorgonVirtualFile"/> objects that match the <paramref name="fileMask"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileMask"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will look for all the files specified by the <paramref name="fileMask"/> parameter. This parameter will accept wild card characters like * and ?. This allows pattern matching which can be 
    /// used to find a series of files. If the wild card is omitted, then only that name is sought out.
    /// </para> 
    /// <para>
    /// Unlike the <see cref="FindFiles(string,string,bool)"/> overload, this method will start searching from the root of the file system.
    /// </para> 
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Calling <c>FindFiles("/", "<![CDATA[/MyDir/C*w.txt]]>");</c> will not return 
    /// any results. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public IEnumerable<IGorgonVirtualFile> FindFiles(string fileMask, bool recursive = true) => FindFiles(DirectorySeparator.ToString(), fileMask, recursive);

    /// <summary>
    /// Function to retrieve a <see cref="IGorgonVirtualFile"/> from the file system.
    /// </summary>
    /// <param name="path">Path to the file to retrieve.</param>
    /// <returns>The <see cref="IGorgonVirtualFile"/> requested or <b>null</b> if the file was not found.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.
    /// <para>-or-</para>
    /// <para>Thrown when there is no file name in the <paramref name="path"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the primary method of accessing files from the file system. It will return a <see cref="IGorgonVirtualFile"/> object that will allow users to open a stream to the file and read its 
    /// contents. 
    /// </para>
    /// <para>
    /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
    /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically. This is also true for filenames without a directory in the path.
    /// </para> 
    /// </remarks>
    /// <example>
    /// This example will show how to read a file from the file system:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\", "/");
    /// 
    /// IGorgonVirtualFile file = fileSystem.GetFile("/ASubDirectory/MyFile.txt");
    /// 
    /// using (Stream stream = file.OpenStream())
    /// {
    ///    // Read the file from the stream...
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public IGorgonVirtualFile? GetFile(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        return GetVirtualFile(path.AsSpan().FormatPath(DirectorySeparator));
    }

    /// <summary>
    /// Function to retrieve a directory from the file system.
    /// </summary>
    /// <param name="path">Path to the directory to retrieve.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
    /// <returns>A <see cref="IGorgonVirtualDirectory"/> if found, <b>null</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// This is the primary method of accessing directories from the file system. It will return a <see cref="IGorgonVirtualDirectory"/> object that will allow users retrieve the files or any sub 
    /// directories under that directory.
    /// </para>
    /// <para>
    /// The <paramref name="path"/> parameter is assumed to be starting from the root directory: <c>/</c>. If the path omits the starting root separator (e.g. <c><![CDATA[MyDir/MyFile.txt]]></c> 
    /// instead of <c><![CDATA[/MyDir/MyFile.txt]]></c>), then one will be supplied automatically. 
    /// </para> 
    /// </remarks>
    /// <example>
    /// This example will show how to retrieve a directory from the file system:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\", "/");
    /// 
    /// IGorgonVirtualDirectory directory = fileSystem.GetDirectory("/ASubDirectory");
    /// 
    /// foreach(IGorgonVirtualDirectory subDirectory in directory.Directories)
    /// {
    ///		Console.WriteLine("Sub directory: {0}", subDirectory.FullPath);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public IGorgonVirtualDirectory? GetDirectory(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);
        return GetVirtualDirectory(path.AsSpan().FormatDirectory(DirectorySeparator));
    }

    /// <summary>
    /// Function to reload all the files and directories within, and optionally, under the specified directory.
    /// </summary> 
    /// <param name="path">The path to the directory to refresh.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the <paramref name="path"/> points to a directory that does not exist.</exception>
    /// <remarks>
    /// <para>
    /// Any files or directories sharing the same path as those in the <see cref="IGorgonFileSystemWriter{T}"/> will be restored if they were deleted. This is because the physical file systems (other 
    /// than the write area) are never changed. For example:
    /// <para>
    /// <code language="csharp">
    /// <![CDATA[
    /// // Let's assume this contains a file called "MyBudget.xls"
    /// fileSystem.Mount(@"C:\MountDirectory\", "/");
    /// 
    /// // Copy an external file into out mounted directory.
    /// File.Copy(@"C:\OtherData\MyBudget.xls", "C:\MountDirectory\Files\");
    /// 
    /// // Get the file...
    /// IGorgonVirtualFile file = fileSystem.GetFile("/Files/MyBudget.xls");
    /// 
    /// // The file does not exist yet because the file system has no idea that it's been added.
    /// if (file is null)
    /// {
    ///    Console.WriteLine("File does not exist.");
    /// }
    /// 
    /// // Now refresh the file system...
    /// fileSystem.Refresh("/Files/");
    /// 
    /// // Get the file... again.
    /// IGorgonVirtualFile file = fileSystem.GetFile("/Files/MyBudget.xls");
    /// 
    /// // The file will now show up in the directory.
    /// if (file is not null)
    /// {
    ///    Console.WriteLine("File exists.");
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// </para>
    /// </remarks>
    public void Refresh(string path)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(path);

        ReadOnlySpan<char> pathSpan = path.AsSpan().FormatDirectory(DirectorySeparator);

        VirtualDirectory directory = GetVirtualDirectory(pathSpan) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));

        if (pathSpan.Equals(Separator, StringComparison.Ordinal))
        {
            Refresh();
            return;
        }

        directory.Directories.Clear();
        directory.Files.Clear();

        ReadOnlySpan<char> physicalPath = directory.MountPoint.Provider.MapToPhysicalPath(path.AsSpan(), directory.MountPoint).FormatDirectory(directory.MountPoint.Provider.PhysicalPathSeparator);

        if (physicalPath.Equals(directory.MountPoint.PhysicalPath, StringComparison.OrdinalIgnoreCase))
        {
            Refresh();
            return;
        }

        GorgonPhysicalFileSystemData fsData = directory.MountPoint.Provider.Enumerate(physicalPath.ToString(), directory);

        if ((fsData.Directories.Count == 0) && (fsData.Files.Count == 0))
        {
            return;
        }

        foreach (string dirPath in fsData.Directories)
        {
            _rootDirectory.Directories.Add(directory.MountPoint, dirPath);
        }

        foreach (IGorgonPhysicalFileInfo file in fsData.Files)
        {
            ReadOnlySpan<char> directoryName = Path.GetDirectoryName(file.VirtualPath.AsSpan()).FormatDirectory(DirectorySeparator);

            if (!directoryName.StartsWith(Separator, StringComparison.OrdinalIgnoreCase))
            {
                directoryName = string.Concat(Separator, directoryName).ToString();
            }

            VirtualDirectory subDirectory = GetVirtualDirectory(directoryName) ?? throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, directoryName.ToString()));

            subDirectory.Files.Add(directory.MountPoint, file);
        }
    }

    /// <summary>
    /// Function to reload all the files and directories in the file system.
    /// </summary> 
    /// <remarks>
    /// <para>
    /// This will unmount and re-mount all the known mount points for the file system, effectively rebuilding the file system file/directory tree.
    /// </para>
    /// <para>
    /// Any files or directories sharing the same path as those in the <see cref="IGorgonFileSystemWriter{T}"/> will be restored if they were deleted. This is because the physical file systems (other 
    /// than the write area) are never changed. For example:
    /// <para>
    /// <code language="csharp">
    /// <![CDATA[
    /// // Let's assume this contains a file called "MyBudget.xls"
    /// fileSystem.Mount("MyZip.zip", "/");
    /// 
    /// // And the write area also contains a file called "MyBudget.xls".
    /// writeArea.Mount();
    /// 
    /// // The "MyBudget.xls" in the write area overrides the file in the zip file since the write area mount 
    /// // is higher in the order list.
    /// //
    /// // This will now delete the MyBudget.xls from the virtual file system and from the physical write area.
    /// writeArea.Delete("MyBudget.xls");
    /// 
    /// // Now refresh the file system...
    /// fileSystem.Refresh();
    /// 
    /// // Now the file will be back, only this time it will belong to the zip file because 
    /// // the write area had its file deleted.
    /// var file = fileSystem.GetFile("MyBudget.xls");
    /// ]]>
    /// </code>
    /// </para>
    /// </para>
    /// </remarks>
    public void Refresh()
    {
        // Don't allow multiple threads to refresh this file system at the same time.
        lock (_syncLock)
        {
            // We need to copy the current mount point locations before refreshing.
            GorgonFileSystemMountPoint[] mountPoints = [.. _mountProviders];

            // Unload everything at once.
            _rootDirectory.Directories.Clear();
            _rootDirectory.Files.Clear();
            _mountProviders.Clear();

            // Refresh the mount points so we can capture the most up to date data.
            foreach (GorgonFileSystemMountPoint mountPoint in mountPoints)
            {
                Mount(mountPoint.PhysicalPath, mountPoint.MountLocation);
            }
        }
    }

    /// <summary>
    /// Function to unmount the mounted virtual file system directories and files specified by the mount point.
    /// </summary>
    /// <param name="mountPoint">The mount point to unmount.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="mountPoint"/> was not found in the file system.</exception>
    /// <remarks>
    /// <para>
    /// This will unmount a physical file system from the virtual file system by removing all directories and files associated with that <paramref name="mountPoint"/>. 
    /// </para>
    /// <para>
    /// When passing the <paramref name="mountPoint"/> parameter, users should pass the return value from the <see cref="IGorgonFileSystem.Mount"/> method.
    /// </para>
    /// <para>
    /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="IGorgonFileSystem.Refresh()"/> method if they 
    /// wish to restore any file/directory entries.
    /// </para>
    /// </remarks>
    public void Unmount(GorgonFileSystemMountPoint mountPoint)
    {
        lock (_syncLock)
        {
            if (!_mountProviders.Contains(mountPoint))
            {
                throw new ArgumentException(string.Format(Resources.GORFS_ERR_MOUNTPOINT_NOT_FOUND,
                                                          mountPoint.MountLocation,
                                                          mountPoint.PhysicalPath),
                                            nameof(mountPoint));
            }

            // Find the directory for the mount point.
            ReadOnlySpan<char> mountPointLocation = mountPoint.MountLocation.AsSpan();
            VirtualDirectory? mountPointDirectory = GetVirtualDirectory(mountPointLocation);

            // If we don't have the directory in the file system, then we have nothing to remove.
            if (mountPointDirectory is null)
            {
                return;
            }

            IEnumerable<VirtualFile> files = FindVirtualFiles(mountPointLocation, "*", true)
                .Where(item => item.MountPoint.Equals(mountPoint))
                .ToArray();

            foreach (VirtualFile file in files)
            {
                file.Directory.Files.Remove(file);
            }

            // Find all directories and files that are related to the provider.
            IEnumerable<VirtualDirectory> directories = [.. FindVirtualDirectories(mountPointLocation, "*", true)
                .Where(item => item.MountPoint.Equals(mountPoint))
                .OrderByDescending(item => item.FullPath)
                .ThenByDescending(item => item.FullPath.Length)];

            GorgonFileSystemMountPoint newMountPoint;

            foreach (VirtualDirectory directory in directories)
            {
                if ((directory.Files.Count == 0) && (directory.Directories.Count == 0))
                {
                    directory.Parent?.Directories.Remove(directory);
                }
                else
                {
                    newMountPoint = directory.Directories.Count > 0
                                     ? directory.Directories.EnumerateVirtualDirectories().First().MountPoint
                                     : (directory.Files.Count > 0
                                            ? directory.Files.EnumerateVirtualFiles().First().MountPoint
                                            : new GorgonFileSystemMountPoint(DefaultProvider, directory.MountPoint.PhysicalPath, directory.MountPoint.MountLocation));
                    // If there are still files or sub directories in the directory, then keep it around, and 
                    // set its provider back to the first provider in the list of directories, files or the 
                    // default.
                    directory.MountPoint = newMountPoint;
                }
            }

            _mountProviders.Remove(mountPoint);

            // If there's nothing left under this mount point, and it's not the root, then dump the directory.
            if ((mountPointDirectory.Parent is null)
                || (mountPointDirectory.Directories.Count != 0)
                || (mountPointDirectory.Files.Count != 0))
            {
                newMountPoint = mountPointDirectory.Directories.Count > 0
                                    ? mountPointDirectory.Directories.EnumerateVirtualDirectories().First().MountPoint
                                    : (mountPointDirectory.Files.Count > 0
                                           ? mountPointDirectory.Files.EnumerateVirtualFiles().First().MountPoint
                                           : new GorgonFileSystemMountPoint(DefaultProvider,
                                                                            mountPointDirectory.MountPoint.PhysicalPath,
                                                                            mountPointDirectory.MountPoint.MountLocation));

                mountPointDirectory.MountPoint = newMountPoint;
                return;
            }

            mountPointDirectory.Parent.Directories.Remove(mountPointDirectory);
        }
    }

    /// <summary>
    /// Function to unmount the mounted virtual file system directories and files pointed at the by the physical path specified and mounted into the mount location specified.
    /// </summary>
    /// <param name="physicalPath">The physical file system path.</param>
    /// <param name="mountLocation">The virtual sub directory that the physical location is mounted under.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are empty.
    /// <para>-or-</para>
    /// <para>Thrown when the mount point with the <paramref name="physicalPath"/> and <paramref name="mountLocation"/> was not found in the file system.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will unmount a physical file system from the virtual file system by removing all directories and files associated with that mount point <paramref name="physicalPath"/> and <paramref name="mountLocation"/>. 
    /// </para>
    /// <para>
    /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="IGorgonFileSystem.Refresh()"/> method if they 
    /// wish to restore any file/directory entries.
    /// </para>
    /// </remarks>
    public void Unmount(string physicalPath, string mountLocation)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(physicalPath);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(mountLocation);

        // Find all mount points with the physical and virtual paths supplied.
        GorgonFileSystemMountPoint[] mountPoints = MountPoints.Where(item =>
                                            string.Equals(Path.GetFullPath(physicalPath),
                                                          Path.GetFullPath(item.PhysicalPath),
                                                          StringComparison.OrdinalIgnoreCase)
                                            &&
                                            mountLocation.AsSpan()
                                                         .FormatDirectory(DirectorySeparator)
                                                         .Equals(item.MountLocation.AsSpan()
                                                                                   .FormatDirectory(DirectorySeparator), StringComparison.OrdinalIgnoreCase))
                                     .ToArray();

        foreach (GorgonFileSystemMountPoint mountPoint in mountPoints)
        {
            Unmount(mountPoint);
        }
    }

    /// <summary>
    /// Function to unmount the mounted virtual file system directories and files pointed at by a physical path.
    /// </summary>
    /// <param name="physicalPath">The physical path to unmount.</param>
    /// <remarks>This overload will unmount all the mounted virtual files/directories for every mount point with the specified <paramref name="physicalPath"/>.</remarks>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is empty.
    /// <para>-or-</para>
    /// <para>Thrown when the mount point with the <paramref name="physicalPath"/> was not found in the file system.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will unmount a physical file system from the virtual file system by removing all directories and files associated with that mount point <paramref name="physicalPath"/>. 
    /// </para>
    /// <para>
    /// Since the mount order overrides any existing directories or files with the same paths, those files/directories will not be restored. A user should call the <see cref="IGorgonFileSystem.Refresh()"/> method if they 
    /// wish to restore any file/directory entries.
    /// </para>
    /// </remarks>
    public void Unmount(string physicalPath)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(physicalPath);

        GorgonFileSystemMountPoint[] mountPoints = MountPoints.Where(item =>
                                            string.Equals(Path.GetFullPath(physicalPath),
                                                          Path.GetFullPath(item.PhysicalPath),
                                                          StringComparison.OrdinalIgnoreCase))
                                     .ToArray();

        foreach (GorgonFileSystemMountPoint mountPoint in mountPoints)
        {
            Unmount(mountPoint);
        }
    }

    /// <summary>
    /// Function to mount a physical file system into the virtual file system.
    /// </summary>
    /// <param name="physicalPath">Path to the physical file system directory or file that contains the files/directories to enumerate.</param>
    /// <param name="mountPath">[Optional] Virtual directory path to mount into.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.
    /// <para>-or-</para>
    /// <para>Thrown if mounting a directory and there is no directory in the path.</para>
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by <paramref name="physicalPath"/> was not found.</exception>
    /// <exception cref="FileNotFoundException">Thrown if a file was specified by <paramref name="physicalPath"/> was not found.</exception>    
    /// <exception cref="IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
    /// <returns>A mount point value for the currently mounted physical path, its mount point in the virtual file system, and the provider used to mount the physical location.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to mount the contents of a physical file system (such as a windows directory, or a file if the appropriate provider is installed) into a virtual directory in the file system. 
    /// All folders and files in the physical file system object will be made available under the virtual folder specified by the <paramref name="mountPath"/> parameter.
    /// </para>
    /// <para>
    /// The method will determine if the user is attempting to mount a physical directory or file. For directories, the <paramref name="physicalPath"/> must end with a trailing backward slash (or 
    /// forward slash depending on your native physical file system). For files, the directory should contain a file name. If mounting a file, and only the file name is supplied, then the current 
    /// directory is used to locate the file. Relative paths are supported, and will be converted into absolute paths based on the current directory.
    /// </para>
    /// <para>
    /// When files (e.g. zip files) are mounted, the appropriate provider must be loaded and installed via the constructor of this object. The providers can be loaded through a 
    /// <see cref="IGorgonFileSystemProviderFactory"/> instance.
    /// </para>
    /// <para>
    /// The <paramref name="physicalPath"/> is usually a file or a directory on the operating system file system. But in some cases, this physical location may point to somewhere completely virtual. 
    /// In order to mount data from those file systems, a provider-specific prefix must be prefixed to the parameter (see provider documentation 
    /// for the correct prefix). This prefix must always begin with <c>::\\</c>.
    /// </para>
    /// <para>
    /// The <paramref name="mountPath"/> parameter is optional, and if omitted, the contents of the physical file system object will be mounted into the root (<c>/</c>) of the virtual file system. If 
    /// the <paramref name="mountPath"/> is supplied, then a virtual directory is created (or used if it already exists) to host the contents of the physical file system.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to create a file system with the default provider, and mount a directory to the root:
    /// <code language="csharp">
    /// <![CDATA[
    /// IGorgonFileSystem fileSystem = new GorgonFileSystem();
    /// 
    /// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
    /// ]]>
    /// </code>
    /// This example shows how to load a provider from the provider factory and use it with the file system:
    /// <code language="csharp">
    /// <![CDATA[
    /// // First we need to load the assembly with the provider plug-in.
    /// using (GorgonPlugInAssemblyCache assemblies = new GorgonPlugInAssemblyCache())
    /// {
    ///		assemblies.Load(@"C:\PlugIns\GorgonFileSystem.Zip.dll"); 
    ///		GorgonPlugInService plugInService = new GorgonPlugInService(assemblies);
    /// 
    ///		// We'll use the factory to get the zip plug-in provider.
    ///		IGorgonFileSystemProviderFactory factory = new GorgonFileSystemProviderFactory(plugInService);
    ///		
    ///		IGorgonFileSystemProvider zipProvider = factory.CreateProvider("Gorgon.IO.Zip.ZipProvider");
    /// 
    ///		// Now create the file system with the zip provider.
    ///		IGorgonFileSystem fileSystem = new GorgonFileSystem(zipProvider);
    ///		fileSystem.Mount(@"C:\ZipFiles\MyFileSystem.zip", "/");
    /// }  
    /// ]]>
    /// </code>
    /// </example>
    public GorgonFileSystemMountPoint Mount(string physicalPath, string? mountPath = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(physicalPath);

        if (string.IsNullOrWhiteSpace(mountPath))
        {
            mountPath = DirectorySeparator.ToString();
        }

        lock (_syncLock)
        {
            if (physicalPath.StartsWith(@"::\\", StringComparison.OrdinalIgnoreCase))
            {
                return MountNonPhysicalLocation(physicalPath, mountPath);
            }

            physicalPath = Path.GetFullPath(physicalPath);
            string? fileName = Path.GetFileName(physicalPath)?.FormatFileName();
            string? directory = Path.GetDirectoryName(physicalPath)?.FormatDirectory(Path.DirectorySeparatorChar);

            // If we have a file, then mount the file using a provider.
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                // If we omitted a directory, then assume we're using the current directory.
                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = Directory.GetCurrentDirectory().FormatDirectory(Path.DirectorySeparatorChar);
                }

                return MountFile(directory + fileName, mountPath);
            }

            // Ensure that our directory name is not empty or null.
            // We can't mount that.
            return string.IsNullOrWhiteSpace(directory)
                ? throw new ArgumentException(string.Format(Resources.GORFS_ERR_ILLEGAL_PATH, physicalPath), nameof(physicalPath))
                : MountDirectory(physicalPath, mountPath);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
    /// </summary>
    /// <param name="provider">A single file system provider to assign to this file system.</param>
    /// <param name="log">[Optional] The application log file.</param>
    /// <remarks>
    /// <para>
    /// To retrieve a <paramref name="provider"/>, use the <see cref="GorgonFileSystemProviderFactory.CreateProvider(string, string)"/> method, or pass in an already instantiated provider.
    /// </para>
    /// </remarks>
    public GorgonFileSystem(IGorgonFileSystemProvider provider, IGorgonLog? log = null)
        : this(log) => _providers[provider.Name] = provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
    /// </summary>
    /// <param name="providers">The providers available to this file system.</param>
    /// <param name="log">[Optional] The application log file.</param>
    /// <remarks>
    /// <para>
    /// To get a list of providers to pass in, use the <see cref="GorgonFileSystemProviderFactory.CreateProviders(string)"/> object to create the providers, or pass a list of already instantiated providers.
    /// </para>
    /// </remarks>
    public GorgonFileSystem(IEnumerable<IGorgonFileSystemProvider> providers, IGorgonLog? log = null)
        : this(log)
    {
        // Get all the providers in the parameter.
        foreach (IGorgonFileSystemProvider provider in providers.Where(item => item is not null))
        {
            _providers[provider.Name] = provider;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystem"/> class.
    /// </summary>
    /// <param name="log">[Optional] The application log file.</param>
    /// <remarks>
    /// <para>
    /// This will create a file system without any providers. Because of this, the only physical file system objects that can be mounted are folders.
    /// </para>
    /// </remarks>
    public GorgonFileSystem(IGorgonLog? log = null)
    {
        _log = log ?? GorgonLog.NullLog;

        _providers = new Dictionary<string, IGorgonFileSystemProvider>(StringComparer.OrdinalIgnoreCase);
        _mountProviders = [];

        DefaultProvider = new FolderFileSystemProvider();

        _rootDirectory = new VirtualDirectory(new GorgonFileSystemMountPoint(DefaultProvider, "Root"), this, null, DirectorySeparator.ToString());
    }
}
