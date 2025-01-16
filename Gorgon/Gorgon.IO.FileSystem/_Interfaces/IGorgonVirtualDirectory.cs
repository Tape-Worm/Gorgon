
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
// Created: Tuesday, September 22, 2015 8:54:34 PM
// 

using Gorgon.Core;
using Gorgon.IO.FileSystem.Providers;

namespace Gorgon.IO.FileSystem;

/// <summary>
/// A representation of a virtual directory within a <see cref="IGorgonFileSystem"/>
/// </summary>
/// <remarks>
/// <para>
/// A virtual directory is an organizational unit that acts as a container for files, and sub-directories.  
/// </para>
/// </remarks>
public interface IGorgonVirtualDirectory
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the mount point for this directory.
    /// </summary>
    /// <remarks>
    /// This will show where the directory is mounted within the <see cref="IGorgonFileSystem"/>, the physical path to the directory, and the <see cref="IGorgonFileSystemProvider"/> used to import the directory.
    /// </remarks>
    GorgonFileSystemMountPoint MountPoint
    {
        get;
    }

    /// <summary>
    /// Property to return the <see cref="IGorgonFileSystem"/> that contains this directory.
    /// </summary>
    IGorgonFileSystem FileSystem
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the directory on the virtual file system.
    /// </summary>
    /// <remarks>
    /// If this property is empty, then this directory does not exist on the physical file system. This can happen when a mount directory is specified when calling the 
    /// <see cref="IGorgonFileSystem.Mount(string, string?, IGorgonFileSystemProvider?)"/> method and that virtual directory does not already exist in the file system.
    /// </remarks>
    /// <seealso cref="IGorgonFileSystem.Mount(string, string?, IGorgonFileSystemProvider?)"/>
    string FullPath
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the directory on the physical file system.
    /// </summary>
    string PhysicalPath
    {
        get;
    }

    /// <summary>
    /// Property to return the list of any child <see cref="IGorgonVirtualDirectory"/> items under this virtual directory.
    /// </summary>
    IReadOnlyDictionary<string, IGorgonVirtualDirectory> Directories
    {
        get;
    }

    /// <summary>
    /// Property to return the list of <see cref="IGorgonVirtualFile"/> objects within this directory.
    /// </summary>
    IReadOnlyDictionary<string, IGorgonVirtualFile> Files
    {
        get;
    }

    /// <summary>
    /// Property to return the parent of this directory.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then this will be the root directory for the file system.
    /// </remarks>
    IGorgonVirtualDirectory? Parent
    {
        get;
    }

    /// <summary>
    /// Function to return all the parents up to the root directory.
    /// </summary>
    /// <returns>A list of all the parents, up to and including the root.</returns>
    /// <remarks>
    /// <para>
    /// If this value is empty, then there is no parent for this directory. This indicates that the current directory is the root directory for the file system.
    /// </para>
    /// </remarks>
    IEnumerable<IGorgonVirtualDirectory> GetParents();

    /// <summary>
    /// Function to retrieve the total number of directories in this directory including any directories under this one.
    /// </summary>
    /// <returns>The total number of directories.</returns>
    /// <remarks>
    /// <para>
    /// Use this to retrieve the total number of <see cref="IGorgonVirtualDirectory"/> entries under this directory. This search includes all sub directories for this and child directories. To get 
    /// the count of the immediate subdirectories, use the <see cref="IReadOnlyCollection{T}.Count"/> property on the <see cref="Directories"/> property.
    /// </para>
    /// </remarks>
    int GetDirectoryCount();

    /// <summary>
    /// Function to retrieve the total number of files in this directory and any directories under this one.
    /// </summary>
    /// <returns>The total number of files.</returns>
    /// <remarks>
    /// <para>
    /// Use this to retrieve the total number of <see cref="IGorgonVirtualFile"/> entries under this directory. This search includes all sub directories for this and child directories. To get 
    /// the count of the immediate files, use the <see cref="IReadOnlyCollection{T}.Count"/> property on the <see cref="Files"/> property.
    /// </para>
    /// </remarks>
    int GetFileCount();

    /// <summary>
    /// Function to determine if this directory, or any of the sub directories contains the specified file.
    /// </summary>
    /// <param name="file">The <see cref="IGorgonVirtualFile"/> to search for.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Use this to determine if a <see cref="IGorgonVirtualFile"/> exists under this directory or any of its sub directories. This search includes all sub directories for this and child directories. 
    /// To determine if a file exists in the immediate directory, use the <see cref="IReadOnlyDictionary{TKey, TValue}.ContainsKey"/> method.
    /// </para>
    /// </remarks>
    bool ContainsFile(IGorgonVirtualFile file);

    /// <summary>
    /// Function to determine if this directory, or any of the sub directories contains a <see cref="IGorgonVirtualFile"/> with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the file to search for.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileName"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// Use this to determine if a <see cref="IGorgonVirtualFile"/> exists under this directory or any of its sub directories. This search includes all sub directories for this and child directories. 
    /// To determine if a file exists in the immediate directory, use the <see cref="IReadOnlyDictionary{TKey, TValue}.ContainsKey"/> method.
    /// </para>
    /// </remarks>
    bool ContainsFile(string fileName);
}
