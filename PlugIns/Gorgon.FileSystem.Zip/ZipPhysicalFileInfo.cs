﻿
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
// Created: Sunday, September 20, 2015 9:32:27 AM
// 

using Gorgon.IO.Providers;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.Zip;

/// <summary>
/// Provides information about a file stored in a physical file system
/// </summary>
internal class ZipPhysicalFileInfo
    : IGorgonPhysicalFileInfo
{
    /// <summary>
    /// Property to return the compressed size of the file, in bytes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some providers will compress files, and the actual size will not match the <see cref="IGorgonPhysicalFileInfo.Length"/> property. This property will return the true size of the file in the physical file system. 
    /// </para>
    /// <para>
    /// For file system providers that do not support compression, this value will be <b>null</b>.
    /// </para>
    /// </remarks>
    public long? CompressedLength
    {
        get;
    }

    /// <summary>
    /// Property to return the date of creation for the file.
    /// </summary>
    public DateTime CreateDate
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the physical file.
    /// </summary>
    public string FullPath
    {
        get;
    }

    /// <summary>
    /// Property to return whether the file is encrypted or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some providers will encrypt file contents. When a file is known to be encrypted, this value will return <b>true</b>; otherwise, <b>false</b>. 
    /// </para>
    /// <para>
    /// For file system providers that do not support encryption, this value return <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsEncrypted => false;

    /// <summary>
    /// Property to return the date of when the file was last modified.
    /// </summary>
    /// <remarks>
    /// Some providers will not support a last modified date on files, and in those cases, the provider should return the <see cref="IGorgonPhysicalFileInfo.CreateDate"/> here.
    /// </remarks>
    public DateTime LastModifiedDate => CreateDate;

    /// <summary>
    /// Property to return the length of the file, in bytes.
    /// </summary>
    public long Length
    {
        get;
    }

    /// <summary>
    /// Property to return the name of the file.
    /// </summary>
    /// <remarks>
    /// This is the file name and extension for the file without the directory.
    /// </remarks>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the offset, in bytes, of the file within a packed file.
    /// </summary>
    /// <remarks>
    /// This value will always be 0 for a file located on the physical file system of the operating system.
    /// </remarks>
    public long Offset
    {
        get;
    }

    /// <summary>
    /// Property to return the virtual path for the file.
    /// </summary>
    /// <remarks>
    /// This is the path to the file within a <see cref="IGorgonFileSystem"/>. For example, the file <c>c:\Mount\MyFile.txt</c> would be mapped to <c>/Mount/MyFile.txt</c>.
    /// </remarks>
    public string VirtualPath
    {
        get;
    }

    /// <summary>Function to refresh the file information.</summary>
    public void Refresh()
    {
        // We don't need to refresh a packed file.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipPhysicalFileInfo" /> class.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="physicalLocation">The physical location of the zip file.</param>
    /// <param name="mountPoint">Mount point path.</param>
    public ZipPhysicalFileInfo(ZipEntry entry, string physicalLocation, IGorgonVirtualDirectory mountPoint)
    {
        string directory = Path.GetDirectoryName(entry.Name);

        directory = mountPoint.FullPath + directory;

        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = "/";
        }

        directory = directory.FormatDirectory('/');

        CompressedLength = entry.CompressedSize;
        CreateDate = entry.DateTime;
        FullPath = physicalLocation + "::/" + entry.Name;
        Length = entry.Size;
        Offset = entry.Offset;
        Name = Path.GetFileName(entry.Name).FormatFileName();
        VirtualPath = directory + Name;
    }
}
