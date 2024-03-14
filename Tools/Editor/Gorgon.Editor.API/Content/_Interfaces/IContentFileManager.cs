#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: March 2, 2019 5:29:01 PM
// 
#endregion

using Gorgon.Graphics.Core;
using Gorgon.IO;

namespace Gorgon.Editor.Content;

/// <summary>
/// Provides access to the file system for reading, writing, creating and deleting content files and directories.
/// </summary>
public interface IContentFileManager
{
    #region Events.
    /// <summary>
    /// Event triggered when the selected files change.
    /// </summary>
    event EventHandler SelectedFilesChanged;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the current directory.
    /// </summary>
    string CurrentDirectory
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to determine if a directory is excluded from a packed file.
    /// </summary>
    /// <param name="directory">Path to the directory to evaluate.</param>
    /// <returns><b>true</b> if excluded, <b>false</b> if not.</returns>
    bool IsDirectoryExcluded(string directory);

    /// <summary>
    /// Function to create a new directory
    /// </summary>
    /// <param name="directory">The path to the new directory.</param>
    /// <returns><b>true</b> if the directory was created, <b>false</b> if it already existed.</returns>
    bool CreateDirectory(string directory);

    /// <summary>
    /// Function to delete a directory.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns><b>true</b> if the directory was deleted, <b>false</b> if it wasn't found.</returns>
    bool DeleteDirectory(string path);

    /// <summary>
    /// Function to determine if a directory exists or not.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns><b>true</b> if the directory exists, <b>false</b> if not.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Function to determine if a file exists or not.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><b>true</b> if the file exists, <b>false</b> if not.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Function to retrieve a file based on the path specified.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A <see cref="IContentFile"/> if found, <b>null</b> if not.</returns>
    IContentFile GetFile(string path);

    /// <summary>
    /// Function to open a file stream for the specified virtual file.
    /// </summary>
    /// <param name="path">The path to the virtual file to open.</param>
    /// <param name="mode">The operating mode for the file stream.</param>
    /// <returns>A file stream for the virtual file.</returns>
    Stream OpenStream(string path, FileMode mode);

    /// <summary>
    /// Function to retrieve the content files for a given directory path.
    /// </summary>
    /// <param name="directoryPath">The directory path to search under.</param>
    /// <param name="searchMask">The search mask to use.</param>
    /// <param name="recursive">[Optional] <b>true</b> to retrieve all files under the path, including those in sub directories, or <b>false</b> to retrieve files in the immediate path.</param>
    /// <returns>An <c>IEnumerable</c> containing the content files found on the path.</returns>
    /// <remarks>
    /// <para>
    /// This will search on the specified <paramref name="directoryPath"/> for all content files that match the <paramref name="searchMask"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="searchMask"/> parameter can be a full file name, or can contain a wildcard character (<b>*</b>) to filter the search. If the <paramref name="searchMask"/> is set to <b>*</b>, then 
    /// all content files under the directory will be returned.
    /// </para>
    /// </remarks>
    IEnumerable<IContentFile> EnumerateContentFiles(string directoryPath, string searchMask, bool recursive = false);

    /// <summary>
    /// Function to retrieve the content sub directories for a given directory path.
    /// </summary>
    /// <param name="directoryPath">The directory path to search under.</param>
    /// <param name="searchMask">The search mask to use.</param>
    /// <param name="recursive">[Optional] <b>true</b> to retrieve all files under the path, including those in sub directories, or <b>false</b> to retrieve files in the immediate path.</param>
    /// <returns>An <c>IEnumerable</c> containing the directory paths found on the path.</returns>
    /// <remarks>
    /// <para>
    /// This will search on the specified <paramref name="directoryPath"/> for directories that match the <paramref name="searchMask"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="searchMask"/> parameter can be a full directory name, or can contain a wildcard character (<b>*</b>) to filter the search. If the <paramref name="searchMask"/> is set to <b>*</b>, then 
    /// all sub directories under the directory will be returned.
    /// </para>
    /// </remarks>
    IEnumerable<string> EnumerateDirectories(string directoryPath, string searchMask, bool recursive = false);

    /// <summary>
    /// Function to retrieve the paths under a given directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search under.</param>
    /// <param name="searchMask">The search mask to use.</param>
    /// <param name="recursive">[Optional] <b>true</b> to retrieve all paths under the directory, including those in sub directories, or <b>false</b> to retrieve paths in the immediate directory path.</param>
    /// <returns>An <c>IEnumerable</c> containing the paths found on the directory.</returns>
    /// <remarks>
    /// <para>
    /// This will search on the specified <paramref name="directoryPath"/> for all paths (i.e. both directories and files) that match the <paramref name="searchMask"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="searchMask"/> parameter can be a full path part, or can contain a wildcard character (<b>*</b>) to filter the search. If the <paramref name="searchMask"/> is set to <b>*</b>, then 
    /// all paths under the directory will be returned.
    /// </para>
    /// </remarks>
    IEnumerable<string> EnumeratePaths(string directoryPath, string searchMask, bool recursive = false);

    /// <summary>
    /// Function to delete a file.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    void DeleteFile(string path);

    /// <summary>
    /// Function to retrieve a list of the file paths that are selected on the file system.
    /// </summary>
    /// <returns>The list of selected file paths.</returns>
    IReadOnlyList<string> GetSelectedFiles();

    /// <summary>
    /// Function to notify the application that the metadata for the file system should be flushed back to the disk.
    /// </summary>
    void FlushMetadata();

    /// <summary>
    /// Function to create a content loader for loading in content information.
    /// </summary>
    /// <param name="textureCache">The cache used to hold texture data.</param>
    /// <returns>A new content loader interface.</returns>
    IGorgonContentLoader GetContentLoader(GorgonTextureCache<GorgonTexture2D> textureCache);

    /// <summary>
    /// Function to convert the content file manager to a standard read-only Gorgon virtual file system.
    /// </summary>
    /// <returns>The <see cref="IGorgonFileSystem"/> for this content manager.</returns>
    IGorgonFileSystem ToGorgonFileSystem();
    #endregion
}
