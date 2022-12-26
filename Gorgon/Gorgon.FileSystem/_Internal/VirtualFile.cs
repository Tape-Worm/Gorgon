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
// Created: Monday, June 27, 2011 8:59:25 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.IO.Providers;

namespace Gorgon.IO;

/// <summary>
/// A representation of a file from a physical file system.
/// </summary>
/// <remarks>
/// <para>
/// This represents the information for a file on a physical file system. This object only contains data about the file, and <u>not</u> the file itself. These files are read only and cannot be written 
/// into from the standard <see cref="IGorgonFileSystem"/>.
/// </para>
/// <para>
/// Users may retrieve a file from a <see cref="IGorgonFileSystem"/> via the <see cref="IGorgonFileSystem.GetFile"/> method. When the file has been returned to the user, they may call the <see cref="OpenStream"/> 
/// method to read the contents of the file.
/// </para>
/// <para>
/// To create, update or delete a file from the <see cref="IGorgonFileSystem"/>, use an instance of the <see cref="IGorgonFileSystemWriter{T}"/> object and call the <see cref="IGorgonFileSystemWriter{T}.OpenStream"/> 
/// method to create or update a file. Use the <see cref="IGorgonFileSystemWriter{T}.DeleteFile"/> method to delete a file.
/// </para>
/// </remarks>
/// <example>
/// This is an example of retrieving a file, and opening it for reading:
/// <code language="csharp">
/// <![CDATA[
/// IGorgonFileSystem fileSystem = new GorgonFileSystem();
/// 
/// // Mount a directory for this file system.
/// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
/// 
/// // Get the file.
/// IGorgonVirtualFile file = fileSystem.GetFile("/AFile.txt");
/// 
/// using (Stream stream = file.OpenStream())
/// {
///		using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
///		{
///			while (!reader.EndOfStream)
///			{
///				Console.WriteLine(reader.ReadLine());
///			}
///		}
/// } 
/// ]]>
/// </code>
/// </example>
internal class VirtualFile
    : IGorgonVirtualFile
{
    #region Properties.
    /// <summary>
    /// Property to return the file system that owns this file.
    /// </summary>
    public IGorgonFileSystem FileSystem => Directory?.FileSystem;

    /// <summary>
    /// Property to return the physical file information for this virtual file.
    /// </summary>
    /// <remarks>
    /// This will return information about the file queried from the physical file system.
    /// </remarks>
    public IGorgonPhysicalFileInfo PhysicalFile
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the full path to the file in the <see cref="IGorgonFileSystem"/>.
    /// </summary>
    public string FullPath => Directory is null ? Name : Directory.FullPath + Name;

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
        set;
    }

    /// <summary>
    /// Property to return the file name without the extension.
    /// </summary>
    public string BaseFileName
    {
        get;
        set;
    }

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
    /// <remarks>
    /// For best practises, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
    /// property.
    /// </remarks>
    public string Name => PhysicalFile.Name;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to open a stream to the file on the physical file system.
    /// </summary>
    /// <returns>The open <see cref="Stream"/> object.</returns>
    /// <remarks>
    /// This will open a <see cref="Stream"/> to the physical file for reading. Applications that open a stream to a file are responsible for closing the <see cref="Stream"/> when they are done.
    /// </remarks>
    public Stream OpenStream() => MountPoint.Provider.OpenFileStream(this);
    #endregion

    #region Constructor/Destructor.
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
        Extension = Path.GetExtension(fileInfo.Name);
        BaseFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
        MountPoint = mountPoint;
    }
    #endregion
}
