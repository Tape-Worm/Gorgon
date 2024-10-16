﻿
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
using Gorgon.IO.Properties;
using Gorgon.PlugIns;

namespace Gorgon.IO.Providers;

/// <summary>
/// A file system provider that mounts Windows file system directories
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IGorgonFileSystemProvider"/> implementors must inherit from this type to create a provider plug-in. 
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
/// the file system provider can be loaded dynamically through Gorgon's plug-in system
/// </para>
/// <para>
/// This type allows the mounting of a directory so that data can be read from the native operating system file system. This is the default provider for any <see cref="IGorgonFileSystem"/>
/// </para>
/// </remarks>
internal sealed class FolderFileSystemProvider
    : GorgonFileSystemProvider
{
    // The physical file system path separator.
    private static readonly char[] _physicalSeparator = [Path.DirectorySeparatorChar];

    /// <summary>Property to return whether this provider only gives read only access to the physical file system.</summary>
    public override bool IsReadOnly => false;

    /// <summary>
    /// Function to enumerate files from the file system.
    /// </summary>
    /// <param name="physicalLocation">The physical file system location to enumerate.</param>
    /// <param name="mountPoint">The mount point to remap the file paths to.</param>
    /// <param name="recurse"><b>true</b> to recursively retrieve files, <b>false</b> to retrieve only the files in the physical location.</param>
    /// <returns>A read only list of <see cref="IGorgonPhysicalFileInfo"/> entries.</returns>
    private IGorgonPhysicalFileInfo[] OnEnumerateFiles(string physicalLocation, IGorgonVirtualDirectory mountPoint, bool recurse)
    {
        DirectoryInfo directoryInfo = new(physicalLocation);

        IEnumerable<FileInfo> files = directoryInfo.GetFiles("*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                                   .Where(item =>
                                                          (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                                                          (item.Attributes & FileAttributes.System) != FileAttributes.System &&
                                                          (item.Attributes & FileAttributes.Compressed) != FileAttributes.Compressed &&
                                                          (item.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted &&
                                                          (item.Attributes & FileAttributes.Device) != FileAttributes.Device);

        return files.Select(file =>
                            new PhysicalFileInfo(file,
                                                 physicalLocation,
                                                 MapToVirtualPath(file.FullName.AsSpan().FormatPath(PhysicalPathSeparator),
                                                                  physicalLocation.AsSpan(),
                                                                  mountPoint.FullPath.AsSpan()).ToString()))
                    .Cast<IGorgonPhysicalFileInfo>()
                    .ToArray();
    }

    /// <summary>
    /// Function to enumerate directories from the file system.
    /// </summary>
    /// <param name="physicalLocation">The physical file system location to enumerate.</param>
    /// <param name="mountPoint">The mount point to remap the directory paths to.</param>
    /// <returns>A read only list of <see cref="string"/> values representing the mapped directory entries.</returns>
    private static string[] OnEnumerateDirectories(string physicalLocation, IGorgonVirtualDirectory mountPoint)
    {
        DirectoryInfo directoryInfo = new(physicalLocation);

        IEnumerable<DirectoryInfo> directories =
            directoryInfo.GetDirectories("*", SearchOption.AllDirectories)
                         .Where(
                             item =>
                             (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                             (item.Attributes & FileAttributes.System) != FileAttributes.System);

        return directories.Select(item => MapToVirtualPath(item.FullName.AsSpan().FormatDirectory(Path.DirectorySeparatorChar),
                                                           physicalLocation.AsSpan(),
                                                           mountPoint.FullPath.AsSpan()).ToString())
                          .ToArray();
    }

    /// <summary>
    /// Function to return the virtual file system path from a physical file system path.
    /// </summary>
    /// <param name="physicalPath">Physical path to the file/folder.</param>
    /// <param name="physicalRoot">Location of the physical folder holding the root for the virtual file system.</param>
    /// <param name="mountPoint">Path to the mount point.</param>
    /// <returns>The virtual file system path.</returns>
    private static ReadOnlySpan<char> MapToVirtualPath(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalRoot, ReadOnlySpan<char> mountPoint)
    {
        if ((physicalPath.IsEmpty)
            || (physicalRoot.IsEmpty)
            || (mountPoint.IsEmpty))
        {
            return [];
        }

        if (!Path.IsPathRooted(physicalPath))
        {
            physicalPath = Path.GetFullPath(physicalPath.ToString()).AsSpan();
        }

        physicalPath = physicalPath[physicalRoot.Length..];
        char[] buffer = new char[mountPoint.Length + physicalPath.Length];
        Span<char> result = buffer.AsSpan();

        mountPoint.CopyTo(result);
        physicalPath.CopyTo(result[mountPoint.Length..]);

        // We can only format on read only spans.
        physicalPath = result;

        if (physicalPath.EndsWith(_physicalSeparator, StringComparison.Ordinal))
        {
            physicalPath = physicalPath.FormatDirectory(GorgonFileSystem.DirectorySeparator);
        }
        else
        {
            physicalPath = physicalPath.FormatPath(GorgonFileSystem.DirectorySeparator);
        }

        return physicalPath;
    }

    /// <summary>
    /// Function to return the virtual file system path from a physical file system path.
    /// </summary>
    /// <param name="physicalPath">Physical path to the file/folder.</param>
    /// <param name="mountPoint">The mount point used to map the physical path.</param>
    /// <returns>The virtual file system path.</returns>
    protected override ReadOnlySpan<char> OnMapToVirtualPath(ReadOnlySpan<char> physicalPath, GorgonFileSystemMountPoint mountPoint) => MapToVirtualPath(physicalPath, mountPoint.PhysicalPath.AsSpan(), mountPoint.MountLocation.AsSpan());

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
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug-in can overload this method to return a stream into a file within their specific native provider (e.g. a Zip file provider will 
    /// return a stream into the zip file positioned at the location of the compressed file within the zip file).
    /// </para>
    /// </remarks>
    protected override GorgonFileSystemStream? OnOpenFileStream(IGorgonVirtualFile file) => !File.Exists(file.PhysicalFile.FullPath) ? null : new GorgonFileSystemStream(file, File.Open(file.PhysicalFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read));

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
    protected override bool OnCanReadFile(string physicalPath) => false;

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
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug-in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
    /// The default functionality will only enumerate directories and files from the operating system file system.
    /// </para>
    /// </remarks>
    protected override GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint) => new(OnEnumerateDirectories(physicalLocation, mountPoint), OnEnumerateFiles(physicalLocation, mountPoint, true));

    /// <summary>
    /// Function to return the physical file system path from a virtual file system path.
    /// </summary>
    /// <param name="virtualPath">Virtual path to the file/folder.</param>
    /// <param name="mountPoint">The mount point used to map the physical path.</param>
    /// <returns>The physical file system path.</returns>
    protected override ReadOnlySpan<char> OnGetPhysicalPath(ReadOnlySpan<char> virtualPath, GorgonFileSystemMountPoint mountPoint)
    {
        ReadOnlySpan<char> mountPointPhysPath = mountPoint.PhysicalPath.AsSpan().FormatDirectory(PhysicalPathSeparator);
        int stringSize = virtualPath.Length + mountPointPhysPath.Length;

        char[] buffer = new char[stringSize];
        Span<char> result = buffer.AsSpan();

        virtualPath.CopyTo(result[mountPointPhysPath.Length..]);
        result.Replace(GorgonFileSystem.DirectorySeparator, PhysicalPathSeparator);
        mountPointPhysPath.CopyTo(result);

        return result;
    }

    /// <summary>Function to enumerate the files for a given directory.</summary>
    /// <param name="physicalLocation">The physical location containing files to enumerate.</param>
    /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory" /> that the files from the physical file system will be mounted into.</param>
    /// <returns>A list of files contained within the physical file system.</returns>
    /// <remarks>
    ///   <para>
    /// This will return a list of <see cref="IGorgonPhysicalFileInfo" /> objects under the virtual file system. Each file system file is mapped from its <paramref name="physicalLocation" /> on the
    /// physical file system to a <paramref name="mountPoint" /> on the virtual file system. For example, if the mount point is set to <c>/MyMount/</c>, and the physical location of a file is
    /// <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    ///   <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider" /> plug-in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// </remarks>
    protected override IReadOnlyDictionary<string, IGorgonPhysicalFileInfo> OnEnumerateFiles(string physicalLocation, IGorgonVirtualDirectory mountPoint)
    {
        DirectoryInfo directoryInfo = new(physicalLocation);

        return directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly)
                            .Where(item =>
                                    (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                                    (item.Attributes & FileAttributes.System) != FileAttributes.System &&
                                    (item.Attributes & FileAttributes.Compressed) != FileAttributes.Compressed &&
                                    (item.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted &&
                                    (item.Attributes & FileAttributes.Device) != FileAttributes.Device)
                            .Select(item => new PhysicalFileInfo(item, physicalLocation,
                                                                MapToVirtualPath(item.FullName.AsSpan().FormatPath(PhysicalPathSeparator),
                                                                physicalLocation.AsSpan(),
                                                                mountPoint.FullPath.AsSpan()).ToString()))
                            .ToDictionary(k => k.VirtualPath, v => (IGorgonPhysicalFileInfo)v, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderFileSystemProvider"/> class.
    /// </summary>
    /// <remarks>
    /// Every copy of the <see cref="IGorgonFileSystem"/> automatically creates an instance of this type as a default provider.
    /// </remarks>
    public FolderFileSystemProvider()
        : base(Resources.GORFS_FOLDER_FS_DESC)
    {
    }
}
