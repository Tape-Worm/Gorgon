
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
// Created: Monday, June 27, 2011 8:58:26 AM
// 

using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.FileSystem.Providers;

namespace Gorgon.IO.FileSystem;

/// <inheritdoc cref="IGorgonVirtualDirectory" />
internal class VirtualDirectory
    : IGorgonVirtualDirectory
{
    /// <inheritdoc/>
    public string Name
    {
        get;
        set;
    }

    /// <inheritdoc/>
    public IGorgonFileSystem FileSystem
    {
        get;
    }

    /// <inheritdoc/>
    public GorgonFileSystemMountPoint MountPoint
    {
        get;
        set;
    }

    /// <inheritdoc/>
    IReadOnlyDictionary<string, IGorgonVirtualDirectory> IGorgonVirtualDirectory.Directories => Directories;

    /// <summary>
    /// Property to return the list of any child <see cref="IGorgonVirtualDirectory"/> items under this virtual directory.
    /// </summary>
    public VirtualDirectoryCollection Directories
    {
        get;
    }

    /// <inheritdoc/>
    IReadOnlyDictionary<string, IGorgonVirtualFile> IGorgonVirtualDirectory.Files => Files;

    /// <summary>
    /// Property to return the list of <see cref="IGorgonVirtualFile"/> objects within this directory.
    /// </summary>
    public VirtualFileCollection Files
    {
        get;
    }

    /// <inheritdoc/>
    IGorgonVirtualDirectory? IGorgonVirtualDirectory.Parent => Parent;

    /// <summary>
    /// Property to return the parent directory for this directory.
    /// </summary>
    public VirtualDirectory? Parent
    {
        get;
    }

    /// <inheritdoc/>
    public string FullPath
    {
        get;
        set;
    }

    /// <inheritdoc/>
    public string PhysicalPath
    {
        get;
        set;
    }

    /// <summary>
    /// Function to retrieve an enumerator that will traverse all sub directories under this directory.
    /// </summary>
    /// <returns>An enumerable that will traverse all child directories</returns>
    private IEnumerable<VirtualDirectory> GetAllSubDirectories() => Directories.EnumerateVirtualDirectories()
                                                                               .TraverseBreadthFirst(d => d.Directories.EnumerateVirtualDirectories());

    /// <inheritdoc/>
    public IEnumerable<IGorgonVirtualDirectory> GetParents()
    {
        IGorgonVirtualDirectory? parent = Parent;

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

    /// <inheritdoc/>
    public int GetDirectoryCount() => Directories.Count == 0 ? 0 : GetAllSubDirectories().Count();

    /// <inheritdoc/>
    public int GetFileCount() => GetAllSubDirectories().Sum(item => item.Files.Count) + Files.Count;

    /// <inheritdoc/>
    public bool ContainsFile(IGorgonVirtualFile file) => file is null ? throw new ArgumentNullException(nameof(file)) : ContainsFile(file.Name);

    /// <inheritdoc/>
    public bool ContainsFile(string fileName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(fileName);

        if (Files.ContainsKey(fileName))
        {
            return true;
        }

        return GetAllSubDirectories().Any(item => item.Files.ContainsKey(fileName));
    }

    /// <summary>
    /// Function to create a new <see cref="VirtualDirectory"/> as a root for a file system.
    /// </summary>
    /// <param name="provider">The initial file system provider used for the root.</param>
    /// <param name="fileSystem">The file system that this root belongs to.</param>
    public static VirtualDirectory CreateRoot(IGorgonFileSystemProvider provider, IGorgonFileSystem fileSystem) => new(
                                    new GorgonFileSystemMountPoint(provider, "Root", false),
                                    fileSystem,
                                    null,
                                    GorgonFileSystem.DirectorySeparator.ToString(),
                                    GorgonFileSystem.DirectorySeparator.ToString(),
                                    string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualDirectory" /> class.
    /// </summary>
    /// <param name="mountPoint">The mount point that supplied this directory.</param>
    /// <param name="fileSystem">The file system that contains the directory.</param>
    /// <param name="parentDirectory">The parent of this directory.</param>
    /// <param name="name">The name of the directory.</param>
    /// <param name="virtualPath">The virtual path to the directory.</param>
    /// <param name="physicalPath">The physical path to the directory.</param>
    public VirtualDirectory(GorgonFileSystemMountPoint mountPoint, IGorgonFileSystem fileSystem, VirtualDirectory? parentDirectory, string name, string virtualPath, string physicalPath)
    {
        MountPoint = mountPoint;
        FileSystem = fileSystem;
        PhysicalPath = physicalPath;
        Parent = parentDirectory;

        Name = name[0] != GorgonFileSystem.DirectorySeparator ? name.FormatPathPart() : name;
        FullPath = virtualPath;
        Directories = new VirtualDirectoryCollection(this);
        Files = new VirtualFileCollection(this);
    }
}
