
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
// Created: Thursday, September 24, 2015 12:18:08 AM
// 

using Gorgon.Core;
using Gorgon.IO.FileSystem.Providers;

namespace Gorgon.IO.FileSystem;

/// <summary>
/// A representation of a file from a physical file system
/// </summary>
/// <remarks>
/// <para>
/// This represents the information for a file on a physical file system. This object only contains data about the file, and <u>not</u> the file itself. These files are read only and cannot be written 
/// into from the standard <see cref="IGorgonFileSystem"/>
/// </para>
/// </remarks>
/// <example>
/// This is an example of retrieving a file, and opening it for reading:
/// <code language="csharp">
/// <![CDATA[
/// IGorgonFileSystem fileSystem = new GorgonFileSystem();
/// 
/// // Mount a directory for this file system
/// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
/// 
/// // Get the file
/// IGorgonVirtualFile? file = fileSystem.GetFile("/AFile.txt");
/// 
/// if (file is null)
/// {
///    throw new FileNotFoundException();
/// }
/// 
/// using (Stream stream = fileSystem.OpenStream(file.FullPath))
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
public interface IGorgonVirtualFile
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the mount point for this file.
    /// </summary>
    /// <remarks>
    /// This will show where the file is mounted within the <see cref="IGorgonFileSystem"/>, the physical path to the file, and the <see cref="IGorgonFileSystemProvider"/> used to import the file information.
    /// </remarks>
    GorgonFileSystemMountPoint MountPoint
    {
        get;
    }

    /// <summary>
    /// Property to return the file system that owns this file.
    /// </summary>
    IGorgonFileSystem FileSystem
    {
        get;
    }

    /// <summary>
    /// Property to return the physical file information for this virtual file.
    /// </summary>
    /// <remarks>
    /// This will return information about the file queried from the physical file system.
    /// </remarks>
    IGorgonPhysicalFileInfo PhysicalFile
    {
        get;
    }

    /// <summary>
    /// Property to return the full path to the file in the <see cref="IGorgonFileSystem"/>.
    /// </summary>
    string FullPath
    {
        get;
    }

    /// <summary>
    /// Property to return the <see cref="IGorgonVirtualDirectory"/> that contains this file.
    /// </summary>
    IGorgonVirtualDirectory Directory
    {
        get;
    }

    /// <summary>
    /// Property to return the file name extension.
    /// </summary>
    string Extension
    {
        get;
    }

    /// <summary>
    /// Property to return the file name without the extension.
    /// </summary>
    string BaseFileName
    {
        get;
    }

    /// <summary>
    /// Property to return the uncompressed size of the file in bytes.
    /// </summary>
    long Size
    {
        get;
    }

    /// <summary>
    /// Property to return the file creation date.
    /// </summary>
    DateTime CreateDate
    {
        get;
    }

    /// <summary>
    /// Property to return the last modified date.
    /// </summary>
    DateTime LastModifiedDate
    {
        get;
    }
}
