
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

using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.FileSystem.Properties;
using Microsoft.IO;

namespace Gorgon.IO.FileSystem.Providers;

/// <inheritdoc cref="IGorgonFileSystemProvider"/>
public abstract class GorgonFileSystemProvider
    : IGorgonFileSystemProvider
{
    /// <summary>
    /// Property to return a memory stream manager for efficient usage of the <see cref="MemoryStream"/> type.
    /// </summary>
    /// <remarks>
    /// Developers should use this instead of creating new MemoryStream objects.
    /// </remarks>
    protected RecyclableMemoryStreamManager MemoryStreamManager
    {
        get;
    } = new RecyclableMemoryStreamManager(new RecyclableMemoryStreamManager.Options()
    {
        MaximumSmallPoolFreeBytes = int.MaxValue / 2,
        MaximumLargePoolFreeBytes = int.MaxValue
    });

    /// <summary>
    /// Property to return the log used for debug messaging.
    /// </summary>
    protected internal IGorgonLog Log
    {
        get;
        internal set;
    }

    /// <inheritdoc/>
    public abstract char PhysicalPathSeparator
    {
        get;
    }

    /// <inheritdoc/>
    public abstract IReadOnlyDictionary<string, GorgonFileExtension> PreferredExtensions
    {
        get;
    }

    /// <inheritdoc/>
    public string Description
    {
        get;
    }

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Function to prefix a virtual file path with a directory separator if it does not already have one.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns>The formatted path.</returns>
    private static ReadOnlySpan<char> PrefixVirtualPath(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
        {
            return [];
        }

        if (path[0] != GorgonFileSystem.DirectorySeparator)
        {
            path = string.Concat(GorgonFileSystem.DirectorySeparatorList, path).AsSpan();
        }

        return path;
    }

    /// <summary>
    /// Function to return the virtual file system path from a physical file system path.
    /// </summary>
    /// <param name="physicalPath">Physical path to the file/folder.</param>
    /// <param name="physicalMountPoint">The physical path that is mounted in the file system.</param>
    /// <param name="virtualRootDirectory">The virtual directory that the physical file system is mounted into.</param>
    /// <returns>The virtual file system path.</returns>
    /// <remarks>
    /// <para>
    /// This will format the <paramref name="physicalPath"/> into a virtual path format for this provider. Note that no attempt is made to verify whether the <paramref name="physicalPath"/>, 
    /// <paramref name="virtualRootDirectory"/> or the <paramref name="physicalMountPoint"/> parameters are valid.
    /// </para>
    /// <para>
    /// The <paramref name="virtualRootDirectory"/> parameter indicates where in the virtual file system the physical file system is mounted. For example, if we want to mount the physical file system under the 
    /// '/MyOtherMount/' directory in the virtual file system, then the <paramref name="virtualRootDirectory"/> should be set to '/MyOtherMount/'. Typically this will be the root directory of the virtual file 
    /// system.
    /// </para>
    /// <para>
    /// The <paramref name="physicalMountPoint"/> is the physical file system location (e.g. a directory on the operating system file system, or a zip file physical location) that is mounted into the virtual file 
    /// system. Users can use the <see cref="GorgonFileSystemMountPoint.PhysicalPath"/> property on a <see cref="GorgonFileSystemMountPoint"/> as the physical mount point (this value is returned from the 
    /// <see cref="IGorgonFileSystem.Mount(string, string?, IGorgonFileSystemProvider?)"/> method).
    /// </para>
    /// <para>
    /// Implementors of this method should note that the <paramref name="virtualRootDirectory"/> is already formatted correctly when this method is called, thus no formatting should be required for that path.
    /// </para>
    /// </remarks>
    /// <returns>The virtual file system path.</returns>
    protected abstract ReadOnlySpan<char> OnMapToVirtualPath(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to open a stream to a file on the physical file system for reading.
    /// </summary>
    /// <param name="fileInfo">The physical file information used to open the file.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point.</param>
    /// <param name="onCloseCallback">A method that will be called when the stream is closed or disposed.</param>
    /// <returns>An open <see cref="Stream"/> to the file.</returns>
    /// <remarks>
    /// <para>
    /// This method will open the file specified by the <paramref name="fileInfo"/> as a stream for reading.
    /// </para>
    /// <para>
    /// The <paramref name="onCloseCallback"/> parameter is a method that will be called when the stream is closed or disposed. This can be used to clean up any resources that are associated with the stream, 
    /// or even update file information for the file system. The callback method takes 2 parameters: The virtual path to the file, and the physical path to the file.
    /// </para>
    /// </remarks>
    protected abstract GorgonFileSystemStream? OnOpenReadFileStream(IGorgonPhysicalFileInfo fileInfo, ReadOnlySpan<char> physicalMountPoint, Action<string, string>? onCloseCallback);

    /// <summary>
    /// Function to determine if a physical file system can be read by this provider.
    /// </summary>
    /// <param name="physicalPath">Path to the packed file containing the file system.</param>
    /// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// This will test a physical file system to see if the provider can open it or not. If used with a directory on an operating system file system, this method should test access to the physical 
    /// directory, and for the existence of the directory.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to determine if a packed file can be read by reading the header of the file specified in 
    /// <paramref name="physicalPath"/>, or that the directory can be accessed with the current security permissions on the operating system.
    /// </para>
    /// </remarks>
    protected abstract bool OnCanReadFileSystem(ReadOnlySpan<char> physicalPath);

    /// <summary>
    /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
    /// </summary>
    /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
    /// <param name="physicalMountPoint">The physical path that is mounted in the file system.</param>
    /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
    /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
    /// <remarks>
    /// <para>
    /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
    /// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
    /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
    /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug-in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
    /// The default functionality will only enumerate directories and files from the operating system file system.
    /// </para>
    /// </remarks>
    protected abstract GorgonPhysicalFileSystemData OnEnumerate(ReadOnlySpan<char> physicalLocation, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> mountPoint);

    /// <summary>
    /// Function to return the physical file system path from a virtual file system path.
    /// </summary>
    /// <param name="virtualPath">Virtual path to the file/folder.</param>
    /// <param name="physicalMountPoint">The physical path that is mounted in the file system.</param>
    /// <param name="virtualRootDirectory">The virtual directory that the physical file system is mounted into.</param>
    /// <returns>The physical file system path.</returns>
    /// <remarks>
    /// <para>
    /// This will format the <paramref name="virtualPath"/> into a physical path format for this provider. Note that no attempt is made to verify whether the <paramref name="virtualPath"/> or the 
    /// <paramref name="physicalMountPoint"/> parameters are valid.
    /// </para>
    /// <para>
    /// The <paramref name="virtualPath"/> will have its leading '/' directory separator stripped off before being passed to this method. This is to facilitate joining the path with the physical path. If 
    /// the separator is required, then implementors may add it to the beginning of the path as needed.
    /// </para>
    /// <para>
    /// The <paramref name="physicalMountPoint"/> is the physical file system location (e.g. a directory on the operating system file system, or a zip file physical location) that is mounted into the virtual file 
    /// system. Users can use the <see cref="GorgonFileSystemMountPoint.PhysicalPath"/> property on a <see cref="GorgonFileSystemMountPoint"/> as the physical mount point (this value is returned from the 
    /// <see cref="IGorgonFileSystem.Mount(string, string?, IGorgonFileSystemProvider?)"/> method).
    /// </para>
    /// <para>
    /// The <paramref name="virtualRootDirectory"/> parameter indicates where in the virtual file system the physical file system is mounted. For example, if we mounted the physical file system under the 
    /// '/MyOtherMount/' directory in the virtual file system, then the <paramref name="virtualRootDirectory"/> should be set to '/MyOtherMount/'. Typically this will be the root directory of the virtual file 
    /// system.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonFileSystemMountPoint"/>
    protected abstract ReadOnlySpan<char> OnMapToPhysicalPath(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <inheritdoc/>
    public ReadOnlySpan<char> MapToVirtualPath(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        if ((physicalPath.IsEmpty)
            || (physicalMountPoint.IsEmpty))
        {
            return [];
        }

        if (virtualRootDirectory.IsEmpty)
        {
            virtualRootDirectory = GorgonFileSystem.DirectorySeparatorList;
        }
        else
        {
            virtualRootDirectory = PrefixVirtualPath(virtualRootDirectory).FormatDirectory(GorgonFileSystem.DirectorySeparator);
        }

        return OnMapToVirtualPath(physicalPath, physicalMountPoint, virtualRootDirectory);
    }

    /// <inheritdoc/>
    public ReadOnlySpan<char> MapToPhysicalPath(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        if ((virtualPath.IsEmpty)
            || (physicalMountPoint.IsEmpty))
        {
            return [];
        }

        virtualPath = PrefixVirtualPath(virtualPath);

        if (virtualRootDirectory.IsEmpty)
        {
            virtualRootDirectory = GorgonFileSystem.DirectorySeparatorList;
        }
        else
        {
            virtualRootDirectory = PrefixVirtualPath(virtualRootDirectory).FormatDirectory(GorgonFileSystem.DirectorySeparator);
        }

        return OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory);
    }

    /// <inheritdoc/>
    public GorgonPhysicalFileSystemData Enumerate(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
    {
        ArgumentEmptyException.ThrowIfEmpty(physicalPath);
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);
        ArgumentEmptyException.ThrowIfEmpty(virtualRootDirectory);

        if (virtualRootDirectory.IsEmpty)
        {
            virtualRootDirectory = GorgonFileSystem.DirectorySeparatorList;
        }
        else
        {
            virtualRootDirectory = PrefixVirtualPath(virtualRootDirectory).FormatDirectory(GorgonFileSystem.DirectorySeparator);
        }

        if (!physicalPath.StartsWith(physicalMountPoint, StringComparison.OrdinalIgnoreCase))
        {
            throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORFS_ERR_PHYSICAL_MISMATCH, physicalPath.ToString()));
        }

        return OnEnumerate(physicalPath, physicalMountPoint, virtualRootDirectory);
    }

    /// <inheritdoc/>
    public GorgonFileSystemStream? OpenReadFileStream(IGorgonPhysicalFileInfo fileInfo, ReadOnlySpan<char> physicalMountPoint, Action<string, string>? onCloseCallback)
    {
        ArgumentEmptyException.ThrowIfEmpty(physicalMountPoint);

        ReadOnlySpan<char> physicalPath = fileInfo.FullPath;

        if (!physicalPath.StartsWith(physicalMountPoint, StringComparison.OrdinalIgnoreCase))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORFS_ERR_PHYSICAL_MISMATCH, fileInfo.VirtualPath));
        }

        return OnOpenReadFileStream(fileInfo, physicalMountPoint, onCloseCallback);
    }

    /// <inheritdoc/>    
    public bool CanReadFileSystem(ReadOnlySpan<char> physicalPath)
    {
        ArgumentEmptyException.ThrowIfEmpty(physicalPath);

        return OnCanReadFileSystem(physicalPath);
    }

    /// <inheritdoc/>
    public IGorgonPhysicalFileInfo GetPhysicalFileInfo(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory)
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

        string physicalPath = OnMapToPhysicalPath(virtualPath, physicalMountPoint, virtualRootDirectory).ToString();

        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, physicalPath));
        }

        return new PhysicalFileInfo(new FileInfo(physicalPath), virtualPath.ToString());
    }

    /// <remarks>
    /// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class
    /// </remarks>
    /// <param name="providerDescription">The human readable description for the file system provider.</param>
    /// <param name="log">The log used for debug messaging.</param>
    protected GorgonFileSystemProvider(string providerDescription, IGorgonLog log)
    {
        Log = log;
        Description = providerDescription;

        string? name = GetType().FullName;

        Debug.Assert(name is not null, "The type name should not be null.");

        Name = name;
    }
}
