
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: January 2, 2020 2:03:29 PM
// 

using Gorgon.IO.Providers;

namespace Gorgon.IO;

/// <summary>
/// A notification system used to notify a <see cref="IGorgonFileSystem"/> that changes have been made
/// </summary>
public interface IGorgonFileSystemNotifier
{
    /// <summary>
    /// Function to notify a file system that a new directory has been added.
    /// </summary>
    /// <param name="mountPoint">The mountpoint that triggered the update.</param>
    /// <param name="physicalPath">The physical path to the new directory.</param>
    /// <returns>The new (or existing) directory corresponding to the physical path.</returns>
    IGorgonVirtualDirectory NotifyDirectoryAdded(GorgonFileSystemMountPoint mountPoint, string physicalPath);

    /// <summary>
    /// Function to notify a file system that a directory has been deleted.
    /// </summary>
    /// <param name="directoryPath">The path to the directory that was deleted.</param>
    void NotifyDirectoryDeleted(string directoryPath);

    /// <summary>
    /// Function to notify a file system that a directory has been renamed.
    /// </summary>
    /// <param name="mountPoint">The mount point for the directory.</param>
    /// <param name="oldPath">The path to the directory that was renamed.</param>        
    /// <param name="physicalPath">The physical path for the directory.</param>
    /// <param name="newName">The new name for the directory.</param>
    void NotifyDirectoryRenamed(GorgonFileSystemMountPoint mountPoint, string oldPath, string physicalPath, string newName);

    /// <summary>
    /// Function to notify a file system that a file has been renamed.
    /// </summary>
    /// <param name="mountPoint">The mount point for the file.</param>
    /// <param name="oldPath">The path to the file that was renamed.</param>        
    /// <param name="fileInfo">Physical file information for the renamed file.</param>
    void NotifyFileRenamed(GorgonFileSystemMountPoint mountPoint, string oldPath, IGorgonPhysicalFileInfo fileInfo);

    /// <summary>
    /// Function to notify a file system that a file has been deleted.
    /// </summary>
    /// <param name="filePath">The path to the file that was deleted.</param>
    void NotifyFileDeleted(string filePath);

    /// <summary>
    /// Function to notify a file system that a previously opened write stream has been closed.
    /// </summary>
    /// <param name="mountPoint">The mountpoint that triggered the update.</param>
    /// <param name="fileInfo">The information about the physical file.</param>
    /// <returns>The file that was updated.</returns>
    IGorgonVirtualFile NotifyFileWriteStreamClosed(GorgonFileSystemMountPoint mountPoint, IGorgonPhysicalFileInfo fileInfo);
}
