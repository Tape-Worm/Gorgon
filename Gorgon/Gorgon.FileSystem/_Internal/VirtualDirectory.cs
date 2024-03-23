
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
// Created: Monday, June 27, 2011 8:58:26 AM
// 

using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Providers;

namespace Gorgon.IO;

/// <summary>
/// A representation of a virtual directory within a <see cref="IGorgonFileSystem"/>
/// </summary>
/// <remarks>
/// <para>
/// A virtual directory is a container for sub directories and files. 
/// </para>
/// <para>
/// Directories can be created by creating a <see cref="IGorgonFileSystemWriter{T}"/> instance and calling its <see cref="IGorgonFileSystemWriter{T}.CreateDirectory"/>. Likewise, if you wish to delete 
/// a directory, call the <see cref="IGorgonFileSystemWriter{T}.DeleteDirectory(string, Action{string}, System.Threading.CancellationToken?)"/> method on the <see cref="IGorgonFileSystemWriter{T}"/> 
/// object
/// </para>
/// </remarks>
internal class VirtualDirectory
    : IGorgonVirtualDirectory
{
    /// <summary>
    /// Property to return the name of the directory.
    /// </summary>
    public string Name
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the <see cref="IGorgonFileSystem"/> that contains this directory.
    /// </summary>
    IGorgonFileSystem IGorgonVirtualDirectory.FileSystem => FileSystem;

    /// <summary>
    /// Property to return the <see cref="IGorgonFileSystem"/> that contains this directory.
    /// </summary>
    public GorgonFileSystem FileSystem
    {
        get;
    }

    /// <summary>
    /// Property to return the mount point for this directory.
    /// </summary>
    /// <remarks>
    /// This will show where the directory is mounted within the <see cref="IGorgonFileSystem"/>, the physical path to the directory, and the <see cref="IGorgonFileSystemProvider"/> used to import the directory.
    /// </remarks>
    public GorgonFileSystemMountPoint MountPoint
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of any child <see cref="IGorgonVirtualDirectory"/> items under this virtual directory.
    /// </summary>
    IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualDirectory> IGorgonVirtualDirectory.Directories => Directories;

    /// <summary>
    /// Property to return the list of any child <see cref="IGorgonVirtualDirectory"/> items under this virtual directory.
    /// </summary>
    public VirtualDirectoryCollection Directories
    {
        get;
    }

    /// <summary>
    /// Property to return the list of <see cref="IGorgonVirtualFile"/> objects within this directory.
    /// </summary>
    IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualFile> IGorgonVirtualDirectory.Files => Files;

    /// <summary>
    /// Property to return the list of <see cref="IGorgonVirtualFile"/> objects within this directory.
    /// </summary>
    public VirtualFileCollection Files
    {
        get;
    }

    /// <summary>
    /// Property to return the parent of this directory.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then this will be the root directory for the file system.
    /// </remarks>
    IGorgonVirtualDirectory IGorgonVirtualDirectory.Parent => Parent;

    /// <summary>
    /// Property to return the parent of this directory.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then this will be the root directory for the file system.
    /// </remarks>
    public VirtualDirectory Parent
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the directory.
    /// </summary>
    public string FullPath => Parent is null ? "/" : Parent.FullPath + Name + "/";

    /// <summary>
    /// Function to return all the parents up to the root directory.
    /// </summary>
    /// <returns>A list of all the parents, up to and including the root.</returns>
    /// <remarks>
    /// If this value is empty, then there is no parent for this directory. This indicates that the current directory is the root directory for the file system.
    /// </remarks>
    public IEnumerable<IGorgonVirtualDirectory> GetParents()
    {
        IGorgonVirtualDirectory parent = Parent;

        if (parent is null)
        {
            yield break;
        }

        while (parent is not null)
        {
            yield return parent;

            parent = parent.Parent;
        }
    }

    /// <summary>
    /// Function to retrieve the total number of directories in this directory including any directories under this one.
    /// </summary>
    /// <returns>The total number of directories.</returns>
    /// <remarks>
    /// Use this to retrieve the total number of <see cref="IGorgonVirtualDirectory"/> entries under this directory. This search includes all sub directories for this and child directories. To get 
    /// the count of the immediate subdirectories, use the <see cref="IReadOnlyCollection{T}.Count"/> property on the <see cref="IGorgonVirtualDirectory.Directories"/> property.
    /// </remarks>
    public int GetDirectoryCount() => Directories.Count == 0 ? 0 : GorgonFileSystem.FlattenDirectoryHierarchy(this, "*").Count();

    /// <summary>
    /// Function to retrieve the total number of files in this directory and any directories under this one.
    /// </summary>
    /// <returns>The total number of files.</returns>
    /// <remarks>
    /// Use this to retrieve the total number of <see cref="IGorgonVirtualFile"/> entries under this directory. This search includes all sub directories for this and child directories. To get 
    /// the count of the immediate files, use the <see cref="IReadOnlyCollection{T}.Count"/> property on the <see cref="IGorgonVirtualDirectory.Files"/> property.
    /// </remarks>
    public int GetFileCount() => GorgonFileSystem.FlattenDirectoryHierarchy(this, "*").Sum(item => item.Files.Count) + Files.Count;

    /// <summary>
    /// Function to determine if this directory, or optionally, any of the sub directories contains the specified file.
    /// </summary>
    /// <param name="file">The <see cref="IGorgonVirtualFile"/> to search for.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// Use this to determine if a <see cref="IGorgonVirtualFile"/> exists under this directory or any of its sub directories. This search includes all sub directories for this and child directories. 
    /// To determine if a file exists in the immediate directory, use the <see cref="IGorgonNamedObjectReadOnlyDictionary{T}.Contains"/> method.
    /// </remarks>
    public bool ContainsFile(IGorgonVirtualFile file) => file is null ? throw new ArgumentNullException(nameof(file)) : ContainsFile(file.Name);

    /// <summary>
    /// Function to determine if this directory, or optionally, any of the sub directories contains a <see cref="IGorgonVirtualFile"/> with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the file to search for.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="fileName"/> parameter is empty.</exception>
    /// <remarks>
    /// Use this to determine if a <see cref="IGorgonVirtualFile"/> exists under this directory or any of its sub directories. This search includes all sub directories for this and child directories. 
    /// To determine if a file exists in the immediate directory, use the <see cref="IGorgonNamedObjectReadOnlyDictionary{T}.Contains"/> method.
    /// </remarks>
    public bool ContainsFile(string fileName)
    {
        if (fileName is null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentEmptyException(nameof(fileName));
        }

        if (Files.Contains(fileName))
        {
            return true;
        }

        IEnumerable<VirtualDirectory> directories = GorgonFileSystem.FlattenDirectoryHierarchy(this, "*");

        return directories.Any(item => item.Files.Contains(fileName));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualDirectory" /> class.
    /// </summary>
    /// <param name="mountPoint">The mount point that supplied this directory.</param>
    /// <param name="fileSystem">The file system that contains the directory.</param>
    /// <param name="parentDirectory">The parent of this directory.</param>
    /// <param name="name">The name of the directory.</param>
    public VirtualDirectory(GorgonFileSystemMountPoint mountPoint, GorgonFileSystem fileSystem, VirtualDirectory parentDirectory, string name)
    {
        MountPoint = mountPoint;
        FileSystem = fileSystem;
        Name = name != "/" ? name.FormatPathPart() : name;
        Parent = parentDirectory;
        Directories = new VirtualDirectoryCollection(this);
        Files = new VirtualFileCollection(this);
    }
}
