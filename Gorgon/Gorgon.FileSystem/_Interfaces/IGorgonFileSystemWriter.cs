
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
// Created: Tuesday, September 22, 2015 10:43:31 PM
// 

namespace Gorgon.IO;

/// <summary>
/// Specifies a writable area on the physical file system for a virtual file system
/// </summary>
/// <typeparam name="T">The type of stream returned from <see cref="OpenStream"/>.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="IGorgonFileSystem"/> is a read only object that is only capable of returning existing files and directories from a physical file system. This is by design in order to keep the integrity 
/// and security of the original file system intact. However, in some cases, the need to write data is required by the application, and that data should be reflected in the current file system. Thus, we 
/// have the <see cref="IGorgonFileSystemWriter{T}"/> interface
/// </para>
/// <para>
/// This object will allow applications to define an area on a physical file system that can be written to by the application. This provides isolation from the main file system and gives a degree of security 
/// when persisting data to an application. For example, if you have a zip file mounted in <c>/</c> and you want to write some data in a new directory, then you could create this object and provide a 
/// path to the writable area: <c>c:\users\username\AppData\YourApplication\CustomData\</c>. When creating, or deleting a directory or file, all data will be shunted to that physical location. For example, 
/// creating a directory named <c>CustomDirectory</c> would actually put the directory under <c>c:\users\AppData\YourApplication\CustomData\CustomDirectory</c>. Likewise, a file named <c>SomeFile.txt</c> would 
/// be put under <c>>c:\users\username\AppData\YourApplication\CustomData\SomeFile.txt</c>
/// </para>
/// <para>
/// If the <see cref="IGorgonFileSystem"/> already has a file or directory mounted from a physical file system, then files from the write area will override those files, providing the most up to date copy of 
/// the data in the physical file systems. There is no actual change to the original files and they will remain in their original location, untouched. Only the files in the directory designated to be the writable 
/// area for a file system will be used for write operations
/// </para>
/// <para>
/// Because the type parameter <typeparamref name="T"/> is based on <see cref="Stream"/>, we can use this interface to build an object that can write into anything that supports a stream, including memory
/// </para>
/// <para> 
/// <note type="tip">
/// <para>
/// When attaching a <see cref="IGorgonFileSystemWriter{T}"/> to a <see cref="IGorgonFileSystem"/>, the write area location is <u>always</u> mounted to the root of the virtual file system
/// </para>
/// </note> 
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// Because the <see cref="IGorgonFileSystem.Mount"/> method always overrides existing files and directories (with the same path) with files and directories from the last loaded physical file system, the write 
/// area may have its files overridden if <see cref="IGorgonFileSystem.Mount"/> is called after linking the write area with a <see cref="IGorgonFileSystem"/>
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <example>
/// This example shows how to create a file system with the default provider, mount a directory to the root, and create a new file:
/// <code language="csharp">
/// <![CDATA[
/// IGorgonFileSystem fileSystem = new GorgonFileSystem();
/// IGorgonFileSystemWriter writeArea = new GorgonFileSystemWriter(fileSystem, @"C:\MyWritingSpot\");
/// 
/// // Mount a directory for this file system
/// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
/// 
/// // Ensure that we mount the write area to ensure that the files in the write directory 
/// // are available
/// writeArea.Mount();
/// 
/// // Create a text file
/// using (Stream stream = writeArea.OpenStream("/AFile.txt", FileMode.Create))
/// {
///		using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
///		{
///			writer.WriteLine("This is a line of text.");
///		}
/// }
/// 
/// // This should retrieve the updated file
/// IGorgonVirtualFile file = fileSystem.GetFile("/AFile.txt");
/// 
/// ]]>
/// </code>
/// </example>
public interface IGorgonFileSystemWriter<out T>
    where T : Stream
{
    /// <summary>
    /// Event triggered when a virtual directory has been added to the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryAddedEventArgs> VirtualDirectoryAdded;
    /// <summary>
    /// Event triggered when a virtual directory has been deleted from the file system.
    /// </summary>
    event EventHandler<VirtualDirectoryDeletedEventArgs> VirtualDirectoryDeleted;
    /// <summary>
    /// Event triggered when a virtual directory has been renamed.
    /// </summary>
    event EventHandler<VirtualDirectoryRenamedEventArgs> VirtualDirectoryRenamed;
    /// <summary>
    /// Event triggered when a virtual directory has been copied.
    /// </summary>
    event EventHandler<VirtualDirectoryCopiedMovedEventArgs> VirtualDirectoryCopied;
    /// <summary>
    /// Event triggered when a virtual directory has been moved.
    /// </summary>
    event EventHandler<VirtualDirectoryCopiedMovedEventArgs> VirtualDirectoryMoved;
    /// <summary>
    /// Event triggered before a phsyical file is imported into the file system.
    /// </summary>
    event EventHandler<FileImportingArgs> FileImporting;
    /// <summary>
    /// Event triggered after a physical file is imported into the file system.
    /// </summary>
    event EventHandler<FileImportedArgs> FileImported;
    /// <summary>
    /// Event triggered when directories, and files have been imported.
    /// </summary>
    event EventHandler<ImportedEventArgs> Imported;
    /// <summary>
    /// Event triggered when a virtual file has been deleted from the file system.
    /// </summary>
    event EventHandler<VirtualFileDeletedEventArgs> VirtualFileDeleted;
    /// <summary>
    /// Event triggered when a virtual file has been opened for writing on the file system.
    /// </summary>
    event EventHandler<VirtualFileOpenedEventArgs> VirtualFileOpened;
    /// <summary>
    /// Event triggered when a virtual file has had its write stream closed.
    /// </summary>
    event EventHandler<VirtualFileClosedEventArgs> VirtualFileClosed;
    /// <summary>
    /// Event triggered when a virtual file has been renamed.
    /// </summary>
    event EventHandler<VirtualFileRenamedEventArgs> VirtualFileRenamed;
    /// <summary>
    /// Event triggered when virtual files were copied.
    /// </summary>
    event EventHandler<VirtualFileCopiedMovedEventArgs> VirtualFileCopied;
    /// <summary>
    /// Event triggered when virtual files were moved.
    /// </summary>
    event EventHandler<VirtualFileCopiedMovedEventArgs> VirtualFileMoved;

    /// <summary>
    /// Property to return the location on the physical file system to use as the writable area for a <see cref="IGorgonFileSystem"/>.
    /// </summary>
    /// <remarks>
    /// This value may return <b>null</b> or an empty string if there's no actual location on a physical file system (e.g. the file system is located in memory).
    /// </remarks>
    string WriteLocation
    {
        get;
    }

    /// <summary>
    /// Property to return the file system linked to this writable area.
    /// </summary>
    IGorgonFileSystem FileSystem
    {
        get;
    }

    /// <summary>
    /// Function to create a new directory in the writable area on the physical file system.
    /// </summary>
    /// <param name="path">Path to the directory (or directories) to create.</param>
    /// <returns>A <see cref="IGorgonVirtualDirectory"/> representing the final directory in the <paramref name="path"/>.</returns>
    /// <remarks>
    /// <para>
    /// This will create a new directory within the physical file system directory specified by the <see cref="WriteLocation"/>. If the <paramref name="path"/> contains multiple directories that don't 
    /// exist (e.g. <c><![CDATA[/Exists/AlsoExists/DoesNotExist/DoesNotExistEither/]]></c>), then those directories will be created until the path is completely parsed. The file system will be updated 
    /// to ensure that those directories will exist and can be referenced.
    /// </para>
    /// <para>
    /// If the directory path contains a name that is the same as a file name within a directory (e.g. <c><![CDATA[/MyDirectory/SomeFile.txt/AnotherDirectory]]></c>, where <c>SomeFile.txt</c> already 
    /// exists as a file under <c>MyDirectory</c>), then an exception will be thrown.
    /// </para>
    /// <para>
    /// If the directory already exists (either in the <see cref="IGorgonFileSystem"/> or on the physical file system), then nothing will be done and the existing directory will be returned from the 
    /// method.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryAdded"/> event.
    /// </para>
    /// </remarks>
    IGorgonVirtualDirectory CreateDirectory(string path);

    /// <summary>
    /// Function to delete a directory from the writeable area with progress functionality.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    /// <param name="onDelete">[Optional] The callback method to execute when a directory, or file is deleted.</param>
    /// <param name="cancelToken">[Optional] The token used to determine if the operation should be canceled or not.</param>
    /// <remarks>
    /// <para>
    /// This will delete a directory, its sub directories, and any files under this directory and sub directories. It will remove all references to those files and directories from the <see cref="IGorgonFileSystemWriter{T}.FileSystem"/>, 
    /// and will delete these items from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/> if they exist in that location.
    /// </para>
    /// <para>
    /// When a directory is removed from the <see cref="FileSystem"/>, only the directory in the <see cref="WriteLocation"/> is physically removed. The items in the other file system mount points will be removed 
    /// from the mounted directory list(s), but not from the other mount points. That is, anything mounted outside of the write location will be preserved and can be re-added to the directory list(s) by calling 
    /// <see cref="IGorgonFileSystem.Refresh()"/>.
    /// </para>
    /// <para>
    /// This method provides the means to receive feedback during the deletion operation, and a means to cancel the operation. This is useful in cases where a UI is present and a delete operation can take a long time to return 
    /// to the user. The callback method takes a single string parameter which represents the full path to the directory,subdirectory, or file being deleted. 
    /// </para>
    /// <para>
    /// If the <paramref name="path"/> is set to the root of the file system (<c>/</c>), then the entire file system will be deleted, but the root directory will always remain.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted directories in the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// This method will use the delete callback action passed to the constructor (if supplied) to perform custom deletion of file system items. If no action is provided, then the files and directories 
    /// deleted by this method are erased and cannot be recovered easily.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryDeleted"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonFileSystem.Refresh()"/>        
    void DeleteDirectory(string path, Action<string> onDelete = null, CancellationToken? cancelToken = null);

    /// <summary>
    /// Function to delete a file from the file system.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    /// <remarks>
    /// <para>
    /// This will delete a single file from the file system, and physically remove it from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>. 
    /// </para>
    /// <para>
    /// When a file is removed from the <see cref="FileSystem"/>, only the file in the <see cref="WriteLocation"/> is physically removed. The items in the other file system mount points will be removed 
    /// from the mounted directory list(s), but not from the other mount point physical locations. That is, anything mounted outside of the write location will be preserved and can be re-added to the 
    /// directory list(s) by calling <see cref="IGorgonFileSystem.Refresh()"/>.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted files in the 
    /// <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// This method will use the delete callback action passed to the constructor (if supplied) to perform custom deletion of file system items. If no action is provided, then the files and directories 
    /// deleted by this method are erased and cannot be recovered easily.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileDeleted"/> event.
    /// </para>
    /// </remarks>
    void DeleteFile(string path);

    /// <summary>
    /// Function to delete multiple files from the file system.
    /// </summary>
    /// <param name="paths">The path to the files to delete.</param>
    /// <param name="onDelete">[Optional] The callback method to execute when a directory, or file is deleted.</param>
    /// <param name="cancelToken">[Optional] The token used to determine if the operation should be canceled or not.</param>
    /// <remarks>
    /// <para>
    /// This will delete multiple files from the file system, and physically remove them from the <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>. 
    /// </para>
    /// <para>
    /// This method provides the means to receive feedback during the deletion operation, and a means to cancel the operation. This is useful in cases where a UI is present and a delete operation can 
    /// take a long time to return to the user. The callback method takes a single string parameter which represents the full path to the file being deleted. 
    /// </para>
    /// <para>
    /// When a file is removed from the <see cref="FileSystem"/>, only the file in the <see cref="WriteLocation"/> is physically removed. The items in the other file system mount points will be removed 
    /// from the mounted directory list(s), but not from the other mount point physical locations. That is, anything mounted outside of the write location will be preserved and can be re-added to the 
    /// directory list(s) by calling <see cref="IGorgonFileSystem.Refresh()"/>.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted files in the 
    /// <see cref="IGorgonFileSystemWriter{T}.WriteLocation"/>.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// This method will use the delete callback action passed to the constructor (if supplied) to perform custom deletion of file system items. If no action is provided, then the files and directories 
    /// deleted by this method are erased and cannot be recovered easily.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileDeleted"/> event.
    /// </para>
    /// </remarks>
    void DeleteFiles(IEnumerable<string> paths, Action<string> onDelete = null, CancellationToken? cancelToken = null);

    /// <summary>
    /// Function to rename a directory.
    /// </summary>
    /// <param name="path">The path to the directory to rename.</param>
    /// <param name="newName">The new name of the directory.</param>
    /// <remarks>
    /// <para>
    /// This will change the name of the specified directory in the <paramref name="path"/> to the name specified by <paramref name="newName"/>. The <paramref name="newName"/> must only contain the name 
    /// of the directory, and no path information. This <paramref name="newName"/> must also not already exist as a file or directory name in the same <paramref name="path"/>.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryRenamed"/> event.
    /// </para>
    /// </remarks>
    void RenameDirectory(string path, string newName);

    /// <summary>
    /// Function to rename a file.
    /// </summary>
    /// <param name="path">The path to the file to rename.</param>
    /// <param name="newName">The new name of the file.</param>
    /// <remarks>
    /// <para>
    /// This will change the name of the specified file in the <paramref name="path"/> to the name specified by <paramref name="newName"/>. The <paramref name="newName"/> must only contain the name 
    /// of the file, and no path information. This <paramref name="newName"/> must also not already exist as a file or directory name in the same <paramref name="path"/>.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileRenamed"/> event.
    /// </para>
    /// </remarks>
    void RenameFile(string path, string newName);

    /// <summary>
    /// Function to copy files to another directory.
    /// </summary>
    /// <param name="filePaths">The path to the files to copy.</param>
    /// <param name="destDirectoryPath">The destination directory path that will receive the copied data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method copies a file, or files, to a new directory path. 
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileCopied"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void CopyFiles(IEnumerable<string> filePaths, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to move files to another directory.
    /// </summary>
    /// <param name="filePaths">The path to the files to file.</param>
    /// <param name="destDirectoryPath">The destination directory path that will receive the moved data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method moves a file, or files, to a new directory path.
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileMoved"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void MoveFiles(IEnumerable<string> filePaths, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to copy a directory to another directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to copy.</param>
    /// <param name="destDirectoryPath">The destination directory path that will receive the copied data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method copies a directory, its sub directories and all files under those directories to a new directory path. 
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryCopied"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void CopyDirectory(string directoryPath, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to move a directory to another directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to move.</param>
    /// <param name="destDirectoryPath">The destination directory path that will receive the moved data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method moves a directory, its sub directories and all files under those directories to a new directory path. 
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// <para>
    /// If the <paramref name="directoryPath"/>, and the <paramref name="destDirectoryPath"/> point to the same location, then this method will return immediately without doing anything. 
    /// </para>
    /// <para>
    /// When moving a directory the <paramref name="directoryPath"/> must not be an ancestor of the <paramref name="destDirectoryPath"/>, and the <paramref name="directoryPath"/> must not be the root 
    /// (<c>/</c>) directory. An exception will be thrown if either condition is true.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualDirectoryMoved"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void MoveDirectory(string directoryPath, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to export a directory from the virtual file system to a directory on the physical file system.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to export.</param>
    /// <param name="destDirectoryPath">The destination directory path on the physical file system that will receive the exported data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method exports the virtual directory, its sub directories and all files under those directories to a directory on the physical file system. 
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void ExportDirectory(string directoryPath, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to import files/directories from the physical file system to a directory on the virtual file system.
    /// </summary>
    /// <param name="paths">The paths to the files/directories on the physical file system to import.</param>
    /// <param name="destDirectoryPath">The destination directory path in the virtual file system that will receive the imported data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method imports files, and directories from the physical file system into a directory on the virtual file system.
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void Import(IReadOnlyList<string> paths, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to export files from the virtual file system to a directory on the physical file system.
    /// </summary>
    /// <param name="filePaths">The path to the files to export.</param>
    /// <param name="destDirectoryPath">The destination directory path on the physical file system that will receive the exported data.</param>
    /// <param name="options">[Optional] The options used to report progress, support cancelation, etc...</param>
    /// <remarks>
    /// <para>
    /// This method exports the specified files to a directory on the physical file system. 
    /// </para>
    /// <para>
    /// When the <paramref name="options"/> parameter is specified, callback methods can be used to show progress of the copy operation, handle file naming conflicts and provide cancellation support. 
    /// These callbacks are useful in scenarios where a UI component is necessary to show progress, and/or prompts to allow the user to decide on how to handle a file naming conflict when copying 
    /// files. The cancellation support is useful in an asynchronous scenario where it's desirable to allow the user to cancel the operation, or cancellation is required due to other conditions.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCopyCallbackOptions"/>
    void ExportFiles(IEnumerable<string> filePaths, string destDirectoryPath, GorgonCopyCallbackOptions options = null);

    /// <summary>
    /// Function to open a file for reading or for writing.
    /// </summary>
    /// <param name="path">The path to the file to read/write.</param>
    /// <param name="mode">The mode to determine how to read/write the file.</param>
    /// <returns>An open <see cref="FileStream"/> to the file.</returns>
    /// <remarks>
    /// <para>
    /// This will open a file for reading or writing depending on the value passed to the <paramref name="mode"/> parameter. See the <see cref="FileMode"/> enumeration for more information about how these modes 
    /// affect the returned <see cref="Stream"/>.
    /// </para>
    /// <para>
    /// When the <paramref name="mode"/> parameter is set to <see cref="FileMode.Open"/>, or <see cref="FileMode.Truncate"/>, and the file does not exist in the <see cref="FileSystem"/>, then an exception will 
    /// be thrown.
    /// </para>
    /// <para>
    /// If the <paramref name="path"/> has a directory, for example <c><![CDATA[/MyDirectory/MyFile.txt]]></c>, and the directory <c><![CDATA[/MyDirectory]]></c> does not exist, an exception will be thrown.
    /// </para>
    /// <para>
    /// Calling this method will trigger the <see cref="VirtualFileOpened"/> event, and when the file stream is closed the <see cref="VirtualFileClosed"/> event.
    /// </para>
    /// </remarks>
    /// <seealso cref="FileMode"/>
    T OpenStream(string path, FileMode mode);

    /// <summary>
    /// Function to mount the directory specified by <see cref="WriteLocation"/> into the file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will refresh the <see cref="FileSystem"/> with the most up to date contents of the <see cref="WriteLocation"/> by mounting the <see cref="WriteLocation"/> into the current <see cref="FileSystem"/>.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Users should call this method immediately after creating the object in order to get the contents of the write area into the file system. If this is not called, the file system will not know what 
    /// files or directories are available in the <see cref="WriteLocation"/>.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks> 
    void Mount();

    /// <summary>
    /// Function to unmount the directory specified by <see cref="WriteLocation"/> from the file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will unmount the <see cref="WriteLocation"/> from the file system. All directories and files for the writable area will be removed from the associated <see cref="FileSystem"/>. 
    /// </para>
    /// <para>
    /// If another physical file system has files or directories with the same path as one in the write area, then they will be removed from the file system as well since the last mounted file system 
    /// (including the write area) will override the previous entries. To refresh and retrieve those items from the currently mounted file systems after unmounting the write area, call the 
    /// <see cref="IGorgonFileSystem.Refresh()"/> method.
    /// </para>
    /// </remarks>
    void Unmount();

}
