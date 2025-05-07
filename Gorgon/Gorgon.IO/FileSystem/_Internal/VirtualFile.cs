
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
// Created: Monday, June 27, 2011 8:59:25 AM
// 

using System.Diagnostics.CodeAnalysis;
using Gorgon.IO.FileSystem.Providers;

namespace Gorgon.IO.FileSystem;

/// <inheritdoc cref="IGorgonVirtualFile" />
internal class VirtualFile
    : IGorgonVirtualFile
{
    private IGorgonPhysicalFileInfo _physicalFile;

    /// <summary>
    /// Property to return the file system that owns this file.
    /// </summary>
    public IGorgonFileSystem FileSystem => Directory.FileSystem;

    /// <summary>
    /// Property to return the physical file information for this virtual file.
    /// </summary>
    /// <remarks>
    /// This will return information about the file queried from the physical file system.
    /// </remarks>    
    public IGorgonPhysicalFileInfo PhysicalFile
    {
        get => _physicalFile;
        [MemberNotNull(nameof(_physicalFile))]
        set
        {
            _physicalFile = value;

            BaseFileName = Path.GetFileNameWithoutExtension(value.Name);
            Extension = Path.GetExtension(value.Name);
        }
    }

    /// <summary>
    /// Property to return the full path to the file in the <see cref="IGorgonFileSystem"/>.
    /// </summary>
    public string FullPath => PhysicalFile.VirtualPath;

    /// <summary>
    /// Property to return the mount point for this file.
    /// </summary>
    /// <remarks>
    /// This will show where the file is mounted within the <see cref="IGorgonFileSystem"/>, the physical path to the file, and the <see cref="IGorgonFileSystemProvider"/> used to import the file information.
    /// </remarks>
    public GorgonFileSystemMountPoint MountPoint
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the <see cref="IGorgonVirtualDirectory"/> that contains this file.
    /// </summary>
    IGorgonVirtualDirectory IGorgonVirtualFile.Directory => Directory;

    /// <summary>
    /// Property to return the <see cref="IGorgonVirtualDirectory"/> that contains this file.
    /// </summary>
    public VirtualDirectory Directory
    {
        get;
    }

    /// <summary>
    /// Property to return the file name extension.
    /// </summary>
    public string Extension
    {
        get;
        private set;
    } = string.Empty;

    /// <summary>
    /// Property to return the file name without the extension.
    /// </summary>
    public string BaseFileName
    {
        get;
        private set;
    } = string.Empty;

    /// <summary>
    /// Property to return the uncompressed size of the file in bytes.
    /// </summary>
    public long Size => PhysicalFile.Length;

    /// <summary>
    /// Property to return the file creation date.
    /// </summary>
    public DateTime CreateDate => PhysicalFile.CreateDate;

    /// <summary>
    /// Property to return the last modified date.
    /// </summary>
    public DateTime LastModifiedDate => PhysicalFile.LastModifiedDate;

    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    public string Name => PhysicalFile.Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualFile"/> class.
    /// </summary>
    /// <param name="mountPoint">The mount point that supplies this file.</param>
    /// <param name="fileInfo">Information about the physical file.</param>
    /// <param name="parent">The parent directory for this file..</param>
    public VirtualFile(GorgonFileSystemMountPoint mountPoint, IGorgonPhysicalFileInfo fileInfo, VirtualDirectory parent)
    {
        MountPoint = mountPoint;
        PhysicalFile = fileInfo;
        Directory = parent;
        MountPoint = mountPoint;
    }
}
