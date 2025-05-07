
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
// Created: Saturday, September 19, 2015 11:40:07 PM
// 

using Gorgon.Core;

namespace Gorgon.IO.FileSystem.Providers;

/// <summary>
/// A file system provider that provides read and write access to physical file systems for a <see cref="IGorgonFileSystem"/>.
/// </summary>
/// <remarks>
/// <para>
/// File system providers provide access to a physical file system, and provides the communications necessary to read and write data in that physical file system. When used in conjunction with the <see cref="IGorgonFileSystem"/> 
/// object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea if the file is a standard 
/// file system file, or a file inside of a zip archive.  
/// </para>
/// <para>
/// This specific interface allows for the implementation of a provider that can write to a physical file system.
/// </para>
/// </remarks>
public interface IGorgonFileSystemWriteProvider
    : IGorgonFileSystemProvider
{
    /// <summary>
    /// Function used to prepare the write area for writing.
    /// </summary>
    /// <param name="physicalPath">The physical path to the write area.</param>
    void PrepareWriteArea(ReadOnlySpan<char> physicalPath);

    /// <summary>
    /// Function to retrieve the physical file system information for a given virtual file path.
    /// </summary>
    /// <param name="virtualPath">The virtual path to the file.</param>
    /// <param name="physicalMountPoint">The physical path that is mounted in the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>		
    /// <returns>A <see cref="IGorgonPhysicalFileInfo"/> containing information about the file.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="virtualPath"/>, or the <paramref name="virtualRootDirectory"/> parameter is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the physical file does not exist.</exception>
    /// <remarks>
    /// <para>
    /// This method retrieves physical file system information for a given file with the specified virtual path. If the file exists, a new <see cref="IGorgonPhysicalFileInfo"/> object is returned with the 
    /// relevant information for the file.
    /// </para>
    /// <para>
    /// If the file does not exist on the physical file system, then an exception will be thrown.
    /// </para>
    /// </remarks>
    IGorgonPhysicalFileInfo GetPhysicalFileInfo(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to open a stream to a file for writing on the physical file system.
    /// </summary>
    /// <param name="virtualPath">The virtual file system path to the file.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>		
    /// <param name="onCloseCallback">A method that will be called when the stream is closed or disposed.</param>
    /// <returns>An open <see cref="GorgonFileSystemStream"/> to the file.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="virtualPath"/>, <paramref name="physicalMountPoint"/>, or the <paramref name="virtualRootDirectory"/> parameters are empty.</exception>
    /// <exception cref="NotSupportedException">Thrown if the provider does not support writing.</exception>
    /// <remarks>
    /// <para>
    /// This method will open the file specified by the <paramref name="virtualPath"/> at its physical file location as a stream for writing. 
    /// </para>
    /// <para>
    /// The <paramref name="onCloseCallback"/> parameter is a method that will be called when the stream is closed or disposed. This can be used to clean up any resources that are associated with the stream, 
    /// or even update file information for the file system. The callback method takes 2 parameters: The virtual path to the file, and the physical path to the file.
    /// </para>
    /// <para>
    /// The stream that is returned will be returned in an opened state, and as such, it is the responsibility of the user to <see cref="Stream.Close"/> or <see cref="IDisposable.Dispose"/> the stream when 
    /// finished with it.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Not all providers support write operations for their physical file systems, if the provider does not support writing, then this method will throw an exception.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    GorgonFileSystemStream OpenWriteFileStream(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory, Action<string, string>? onCloseCallback);

    /// <summary>
    /// Function to determine if a physical file system can be written to by this provider.
    /// </summary>
    /// <param name="physicalPath">Path to the packed file containing the file system.</param>
    /// <returns><b>true</b> if the provider can write to the file system, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will test a physical file system to see if the provider can write into it or not. If used with a directory on an operating system file system, then the provider should check that the folder is 
    /// accessible by the current user.
    /// </para>
    /// </remarks>
    bool CanWriteFileSystem(ReadOnlySpan<char> physicalPath);

    /// <summary>
    /// Function to create a directory on the physical file system.
    /// </summary>
    /// <param name="virtualPath">The virtual path to the directory.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>	
    /// <returns>The physical path to the directory.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="virtualPath"/>, <paramref name="physicalMountPoint"/>, or the <paramref name="virtualRootDirectory"/> parameters are empty.</exception>
    /// <remarks>
    /// <para>
    /// This will create a directory on the physical file system. If the directory already exists, then this method will do nothing. 
    /// </para>
    /// <para>
    /// If the <paramref name="virtualPath"/> contains multiple directories, and those directories do not exist, then this method will create those directories as well.    
    /// </para>
    /// </remarks>
    string CreateDirectory(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to delete a directory on the physical file system.
    /// </summary>
    /// <param name="virtualPath">The virtual path to the directory.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>	
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="virtualPath"/>, <paramref name="physicalMountPoint"/>, or the <paramref name="virtualRootDirectory"/> parameters are empty.</exception>
    /// <remarks>
    /// <para>
    /// This will delete a physical directory on the physical file system, along with its files and subdirectories. 
    /// </para>
    /// <para>
    /// If the physical directory was not found, then this method should do nothing.
    /// </para>
    /// </remarks>
    void DeleteDirectory(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to rename a directory on the physical file system.
    /// </summary>
    /// <param name="virtualPath">The virtual path to the directory to rename.</param>
    /// <param name="newDirectoryName">The new name for the directory.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>	
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="virtualPath"/>, <paramref name="physicalMountPoint"/>, or the <paramref name="virtualRootDirectory"/> parameters are empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the physical path does not exist.</exception>
    /// <remarks>
    /// <para>
    /// This will rename a physical directory on the physical file system, along with its files and subdirectories. 
    /// </para>
    /// </remarks>
    string RenameDirectory(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> newDirectoryName, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to delete a file on the physical file system.
    /// </summary>
    /// <param name="virtualPath">The virtual path to the file.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="virtualPath"/>, <paramref name="physicalMountPoint"/>, or the <paramref name="virtualRootDirectory"/> parameters are empty.</exception>
    /// <remarks>
    /// <para>
    /// This will delete a physical file on the physical file system. 
    /// </para>
    /// <para>
    /// If the physical file was not found, then this method should do nothing.
    /// </para>
    /// </remarks>
    void DeleteFile(ReadOnlySpan<char> virtualPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);

    /// <summary>
    /// Function to copy a file from a virtual file system to the physical file system.
    /// </summary>
    /// <param name="sourceFile">The source file to copy.</param>
    /// <param name="destinationPath">The virtual destination path for the file to copy.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>	
    /// <param name="options">Options to pass to the copy operation.</param>
    /// <returns>A <see cref="IGorgonPhysicalFileInfo"/> object containing data about the physical file.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="destinationPath"/>, <paramref name="physicalMountPoint"/>, or the <paramref name="virtualRootDirectory"/> parameters are empty.</exception>
    /// <remarks>
    /// <para>
    /// This will copy the contents of a file on the file system into a new file on the physical file system. If the file already exists, it will be overwritten.
    /// </para>
    /// </remarks>
    IGorgonPhysicalFileInfo CopyFile(IGorgonVirtualFile sourceFile, ReadOnlySpan<char> destinationPath, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory, GorgonFileSystemCopyOptions options);

    /// <summary>
    /// Function to rename a file on the physical file system.
    /// </summary>
    /// <param name="sourceVirtualFile">The virtual path the source file to rename.</param>
    /// <param name="newFileName">The new name for the file.</param>
    /// <param name="physicalMountPoint">The path to the physical mount point for the file system.</param>
    /// <param name="virtualRootDirectory">A virtual directory path that the directories and files from the physical file system will be mounted into.</param>	
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="sourceVirtualFile"/>, <paramref name="newFileName"/>, <paramref name="physicalMountPoint"/> or the <paramref name="virtualRootDirectory"/> parameter is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file at <paramref name="sourceVirtualFile"/> is not found on the physical file system.</exception>
    /// <returns>A <see cref="IGorgonPhysicalFileInfo"/> object containing data about the physical file.</returns>
    /// <remarks>
    /// <para>
    /// This will rename a file on the physical file system. If the file is not found, then this method will throw an exception.
    /// </para>
    /// </remarks>
    IGorgonPhysicalFileInfo RenameFile(ReadOnlySpan<char> sourceVirtualFile, ReadOnlySpan<char> newFileName, ReadOnlySpan<char> physicalMountPoint, ReadOnlySpan<char> virtualRootDirectory);
}
