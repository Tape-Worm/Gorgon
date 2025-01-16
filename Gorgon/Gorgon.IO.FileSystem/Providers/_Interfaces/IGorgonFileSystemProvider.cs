
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Saturday, September 19, 2015 11:40:07 PM
// 

using Gorgon.Core;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A file system provider that provides access to physical file systems for a <see cref="IGorgonFileSystem"/>.
/// </summary>
/// <remarks>
/// <para>
/// File system providers provide access to a physical file system, and provides the communications necessary to read data from that physical file system. When used in conjunction with the <see cref="IGorgonFileSystem"/> 
/// object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea if the file is a standard 
/// file system file, or a file inside of a zip archive.  
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// This provider is read only, meaning it will only read data from the physical file system.
/// </para>
/// </note>
/// </para>
/// <para>
/// When this type is implemented, it can be made to read any type of file system, including those that store their contents in a packed file format (e.g. Zip). 
/// </para>
/// </remarks>
public interface IGorgonFileSystemProvider
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the character used to separate path parts on the physical file system.
    /// </summary>
    char PhysicalPathSeparator
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
    IReadOnlyDictionary<string, GorgonFileExtension> PreferredExtensions
    {
        get;
    }

    /// <summary>
    /// Property to return the human readable description of the provider.
    /// </summary>
    string Description
    {
        get;
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
    /// </remarks>
    /// <seealso cref="GorgonFileSystemMountPoint"/>
    ReadOnlySpan<char> MapToVirtualPath(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

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
    ReadOnlySpan<char> MapToPhysicalPath(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
    /// </summary>
    /// <param name="physicalPath">The physical location containing files and directories to enumerate.</param>
    /// <param name="physicalMountPoint">The physical path that is mounted in the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>		
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/>, or the <paramref name="virtualRootDirectory"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the physical file system could not be read.
    /// <para>-or-</para>
    /// <para>The <paramref name="physicalPath"/> is not under the <paramref name="physicalMountPoint"/> path.</para>
    /// </exception>
    /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
    /// <remarks>
    /// <para>
    /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
    /// system file and directory is mapped from its <paramref name="physicalPath"/> on the physical file system to a <paramref name="virtualRootDirectory"/> on the virtual file system. For example, if the 
    /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
    /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
    /// </para>
    /// <para>
    /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug-in can override this method to read the list of files from another type of file system, like a Zip file.
    /// </para>
    /// </remarks>
    GorgonPhysicalFileSystemData Enumerate(ReadOnlySpan<char> physicalPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to open a stream to a file for reading on the physical file system.
    /// </summary>
    /// <param name="fileInfo">The physical file information for the file to open.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="onCloseCallback">A method that will be called when the stream is closed or disposed.</param>
    /// <returns>An open <see cref="GorgonFileSystemStream"/> to the file, or <b>null</b> if the file was not found on the physical file system.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalMountPoint"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">The <paramref name="fileInfo"/> is not under the <paramref name="physicalMountPoint"/> path.</exception>
    /// <remarks>
    /// <para>
    /// This method will open the file specified by the <paramref name="fileInfo"/> at its physical file location as a stream for reading. 
    /// </para>
    /// <para>
    /// The <paramref name="onCloseCallback"/> parameter is a method that will be called when the stream is closed or disposed. This can be used to clean up any resources that are associated with the stream, 
    /// or even update file information for the file system. The callback method takes 2 parameters: The virtual path to the file, and the physical path to the file.
    /// </para>
    /// <para>
    /// The stream that is returned will be returned in an opened state, and as such, it is the responsibility of the user to <see cref="Stream.Close"/> or <see cref="IDisposable.Dispose"/> the stream when 
    /// finished with it.
    /// </para>
    /// </remarks>
    GorgonFileSystemStream? OpenReadFileStream(IGorgonPhysicalFileInfo fileInfo, ReadOnlySpan<char> physicalMountPoint, Action<string, string>? onCloseCallback);

    /// <summary>
    /// Function to determine if a physical file system can be read by this provider.
    /// </summary>
    /// <param name="physicalPath">Path to the packed file containing the file system.</param>
    /// <returns><b>true</b> if the provider can read the file system, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, then the provider should check to 
    /// ensure that the directory exists, and is accessible by the current user.
    /// </para>
    /// </remarks>
    bool CanReadFileSystem(ReadOnlySpan<char> physicalPath);
}
