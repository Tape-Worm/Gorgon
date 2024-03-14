#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Monday, June 27, 2011 9:00:18 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.PlugIns;
using Microsoft.IO;

namespace Gorgon.IO.Providers;

/// <summary>
/// A file system provider that mounts Windows file system directories.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IGorgonFileSystemProvider"/> implementors must inherit from this type to create a provider plug in. 
/// </para>
/// <para>
/// File system providers provide access to a physical file system, and provides the communications necessary to read data from that physical file system. When used in conjunction with the <see cref="IGorgonFileSystem"/> 
/// object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea if the file is a standard 
/// file system file, or a file inside of a zip archive.  
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// As the documentation states, providers can read data from a file system. However, no mechanism is available to write to a file system through a provider. This is by design. The <see cref="IGorgonFileSystemWriter{T}"/> 
/// type allows writing to a file system via a predefined area in a physical file system. 
/// </para>
/// </note>
/// </para>
/// <para>
/// When this type is implemented, it can be made to read any type of file system, including those that store their contents in a packed file format (e.g. Zip). And since this type inherits from <see cref="GorgonPlugIn"/>, 
/// the file system provider can be loaded dynamically through Gorgon's plug in system.
/// </para>
/// <para>
/// This type allows the mounting of a directory so that data can be read from the native operating system file system. This is the default provider for any <see cref="IGorgonFileSystem"/>.
/// </para>
/// </remarks>
public abstract class GorgonFileSystemProvider
    : GorgonPlugIn, IGorgonFileSystemProvider
{
    #region Properties.
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
    /// Property to return whether this provider only gives read only access to the physical file system.
    /// </summary>
    public abstract bool IsReadOnly
    {
        get;
    }

    /// <summary>
    /// Property to return a list of preferred file extensions (if applicable).
    /// </summary>
    /// <remarks>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> that reads from a packed file should supply a list of well known file name extensions wrapped in <see cref="GorgonFileExtension"/> objects for 
    /// that physical file system type. This list can then be then used in an application to filter the types of files to open with a <see cref="IGorgonFileSystem"/>. If the file system reads directories on 
    /// the native file system, then this collection should remain empty.
    /// </remarks>
    public IGorgonNamedObjectReadOnlyDictionary<GorgonFileExtension> PreferredExtensions
    {
        get;
        protected set;
    }

    /// <summary>Property to return the path to the provider assembly (if applicable).</summary>
    public string ProviderPath => PlugInPath ?? string.Empty;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to return the virtual file system path from a physical file system path.
    /// </summary>
    /// <param name="physicalPath">Physical path to the file/folder.</param>
    /// <param name="physicalRoot">Location of the physical folder holding the root for the virtual file system.</param>
    /// <param name="mountPoint">Path to the mount point.</param>
    /// <returns>The virtual file system path.</returns>
    protected private static string MapToVirtualPath(string physicalPath, string physicalRoot, string mountPoint)
    {
        if ((string.IsNullOrWhiteSpace(physicalPath))
            || (string.IsNullOrWhiteSpace(physicalRoot))
            || (string.IsNullOrWhiteSpace(mountPoint)))
        {
            return string.Empty;
        }

        if (!Path.IsPathRooted(physicalPath))
        {
            physicalPath = Path.GetFullPath(physicalPath);
        }

        physicalPath = mountPoint + physicalPath.Replace(physicalRoot, string.Empty);

        if (physicalPath.EndsWith(GorgonFileSystem.PhysicalDirSeparator, StringComparison.OrdinalIgnoreCase))
        {
            physicalPath = physicalPath.FormatDirectory('/');
        }
        else
        {
            physicalPath = Path.GetDirectoryName(physicalPath).FormatDirectory('/') +
                           Path.GetFileName(physicalPath).FormatFileName();
        }

        return physicalPath;
    }

    /// <summary>
    /// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
    /// </summary>
    /// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
    /// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
    /// responsibility of the user to close the stream when finished.
    /// </para>
    /// <para>
    /// If the file does not exist in the physical file system, this method should return <b>null</b>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can overload this method to return a stream into a file within their specific native provider (e.g. a Zip file provider will 
    /// return a stream into the zip file positioned at the location of the compressed file within the zip file).
    /// </para>
    /// </remarks>
    protected abstract GorgonFileSystemStream OnOpenFileStream(IGorgonVirtualFile file);

    /// <summary>
    /// Function to determine if a physical file system can be read by this provider.
    /// </summary>
    /// <param name="physicalPath">Path to the packed file containing the file system.</param>
    /// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
    /// <remarks>
    /// <para>
    /// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method should always return 
    /// <b>false</b>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to determine if a packed file can be read by reading the header of the file specified in <paramref name="physicalPath"/>.
    /// </para>
    /// </remarks>
    protected abstract bool OnCanReadFile(string physicalPath);

    /// <summary>
    /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
    /// </summary>
    /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
    /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
    /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
    /// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
    /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
    /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
    /// The default functionality will only enumerate directories and files from the operating system file system.
    /// </para>
    /// </remarks>
    protected abstract GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint);

    /// <summary>
    /// Function to enumerate the files for a given directory.
    /// </summary>
    /// <param name="physicalLocation">The physical location containing files to enumerate.</param>
    /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the files from the physical file system will be mounted into.</param>		
    /// <returns>A list of files contained within the physical file system.</returns>
    /// <remarks>
    /// <para>
    /// This will return a list of <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file system file is mapped from its <paramref name="physicalLocation"/> on the 
    /// physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the mount point is set to <c>/MyMount/</c>, and the physical location of a file is 
    /// <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// </remarks>
    protected abstract IReadOnlyDictionary<string, IGorgonPhysicalFileInfo> OnEnumerateFiles(string physicalLocation, IGorgonVirtualDirectory mountPoint);

    /// <summary>
    /// Function to return the physical file system path from a virtual file system path.
    /// </summary>
    /// <param name="virtualPath">Virtual path to the file/folder.</param>
    /// <param name="mountPoint">The mount point used to map the physical path.</param>
    /// <returns>The physical file system path.</returns>
    protected abstract string OnGetPhysicalPath(string virtualPath, GorgonFileSystemMountPoint mountPoint);

    /// <summary>
    /// Function to return the virtual file system path from a physical file system path.
    /// </summary>
    /// <param name="physicalPath">Physical path to the file/folder.</param>
    /// <param name="mountPoint">The mount point used to map the physical path.</param>
    /// <returns>The virtual file system path.</returns>
    public string MapToVirtualPath(string physicalPath, GorgonFileSystemMountPoint mountPoint) => MapToVirtualPath(physicalPath, mountPoint.PhysicalPath, mountPoint.MountLocation);

    /// <summary>
    /// Function to return the physical file system path from a virtual file system path.
    /// </summary>
    /// <param name="virtualPath">Virtual path to the file/folder.</param>
    /// <param name="mountPoint">The mount point used to map the physical path.</param>
    /// <returns>The physical file system path.</returns>
    public string MapToPhysicalPath(string virtualPath, GorgonFileSystemMountPoint mountPoint)
    {
        if ((string.IsNullOrWhiteSpace(virtualPath))
            || (string.IsNullOrWhiteSpace(mountPoint.PhysicalPath))
            || (string.IsNullOrWhiteSpace(mountPoint.MountLocation)))
        {
            return string.Empty;
        }

        if (virtualPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            virtualPath = virtualPath[1..];
        }

        return string.IsNullOrWhiteSpace(virtualPath) ? mountPoint.PhysicalPath : OnGetPhysicalPath(virtualPath, mountPoint);
    }

    /// <summary>
    /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
    /// </summary>
    /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
    /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
    /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
    /// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
    /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
    /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// </remarks>
    public GorgonPhysicalFileSystemData Enumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint)
    {
        if (physicalLocation is null)
        {
            throw new ArgumentNullException(nameof(physicalLocation));
        }

#pragma warning disable IDE0046 // Convert to conditional expression

        if (mountPoint is null)
        {
            throw new ArgumentNullException(nameof(mountPoint));
        }

        return string.IsNullOrWhiteSpace(physicalLocation)
            ? throw new ArgumentEmptyException(nameof(physicalLocation))
            : OnEnumerate(physicalLocation, mountPoint);
#pragma warning restore IDE0046 // Convert to conditional expression

    }

    /// <summary>
    /// Function to enumerate the files for a given directory.
    /// </summary>
    /// <param name="physicalLocation">The physical location containing files to enumerate.</param>
    /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the files from the physical file system will be mounted into.</param>		
    /// <returns>A list of files contained within the physical file system.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will return a list of <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file system file is mapped from its <paramref name="physicalLocation"/> on the 
    /// physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the mount point is set to <c>/MyMount/</c>, and the physical location of a file is 
    /// <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, IGorgonPhysicalFileInfo> EnumerateFiles(string physicalLocation, IGorgonVirtualDirectory mountPoint)
    {
        if (physicalLocation is null)
        {
            throw new ArgumentNullException(nameof(physicalLocation));
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (mountPoint is null)
        {
            throw new ArgumentNullException(nameof(mountPoint));
        }

        return string.IsNullOrWhiteSpace(physicalLocation)
            ? throw new ArgumentEmptyException(nameof(physicalLocation))
            : OnEnumerateFiles(physicalLocation, mountPoint);
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
    /// </summary>
    /// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
    /// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
    /// responsibility of the user to close the stream when finished.
    /// </para>
    /// </remarks>
    public Stream OpenFileStream(IGorgonVirtualFile file) => file is null ? throw new ArgumentNullException(nameof(file)) : OnOpenFileStream(file);

    /// <summary>
    /// Function to determine if a physical file system can be read by this provider.
    /// </summary>
    /// <param name="physicalPath">Path to the packed file containing the file system.</param>
    /// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
    /// <remarks>
    /// <para>
    /// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method should always return 
    /// <b>false</b>.
    /// </para>
    /// </remarks>
    public bool CanReadFileSystem(string physicalPath)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (physicalPath is null)
        {
            throw new ArgumentNullException(nameof(physicalPath));
        }

        return string.IsNullOrWhiteSpace(physicalPath)
            ? throw new ArgumentEmptyException(nameof(physicalPath))
            : OnCanReadFile(physicalPath);
#pragma warning restore IDE0046 // Convert to conditional expression
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class.
    /// </summary>
    /// <param name="providerDescription">The human readable description for the file system provider.</param>
    protected GorgonFileSystemProvider(string providerDescription)
        : base(providerDescription) => PreferredExtensions = new GorgonFileExtensionCollection();
    #endregion
}
