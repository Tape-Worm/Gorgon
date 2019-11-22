#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Tuesday, September 22, 2015 10:43:31 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;

namespace Gorgon.IO
{
    /// <summary>
    /// Specifies a writable area on the physical file system for a virtual file system.
    /// </summary>
    /// <typeparam name="T">The type of stream returned from <see cref="OpenStream"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// The <see cref="IGorgonFileSystem"/> is a read only object that is only capable of returning existing files and directories from a physical file system. This is by design in order to keep the integrity 
    /// and security of the original file system intact. However, in some cases, the need to write data is required by the application, and that data should be reflected in the current file system. Thus, we 
    /// have the <see cref="IGorgonFileSystemWriter{T}"/> interface.
    /// </para>
    /// <para>
    /// This object will allow applications to define an area on a physical file system that can be written to by the application. This provides isolation from the main file system and gives a degree of security 
    /// when persisting data to an application. For example, if you have a zip file mounted in <c>/</c> and you want to write some data in a new directory, then you could create this object and provide a 
    /// path to the writable area: <c>c:\users\username\AppData\YourApplication\CustomData\</c>. When creating, or deleting a directory or file, all data will be shunted to that physical location. For example, 
    /// creating a directory named <c>CustomDirectory</c> would actually put the directory under <c>c:\users\AppData\YourApplication\CustomData\CustomDirectory</c>. Likewise, a file named <c>SomeFile.txt</c> would 
    /// be put under <c>>c:\users\username\AppData\YourApplication\CustomData\SomeFile.txt</c>.
    /// </para>
    /// <para>
    /// If the <see cref="IGorgonFileSystem"/> already has a file or directory mounted from a physical file system, then files from the write area will override those files, providing the most up to date copy of 
    /// the data in the physical file systems. There is no actual change to the original files and they will remain in their original location, untouched. Only the files in the directory designated to be the writable 
    /// area for a file system will be used for write operations.
    /// </para>
    /// <para>
    /// Because the type parameter <typeparamref name="T"/> is based on <see cref="Stream"/>, we can use this interface to build an object that can write into anything that supports a stream, including memory.
    /// </para>
    /// <para> 
    /// <note type="tip">
    /// <para>
    /// When attaching a <see cref="IGorgonFileSystemWriter{T}"/> to a <see cref="IGorgonFileSystem"/>, the write area location is <u>always</u> mounted to the root of the virtual file system.
    /// </para>
    /// </note> 
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Because the <see cref="IGorgonFileSystem.Mount"/> method always overrides existing files and directories (with the same path) with files and directories from the last loaded physical file system, the write 
    /// area may have its files overridden if <see cref="IGorgonFileSystem.Mount"/> is called after linking the write area with a <see cref="IGorgonFileSystem"/>.
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
    /// // Mount a directory for this file system.
    /// fileSystem.Mount(@"C:\MyDirectory\", "/"); 
    /// 
    /// // Ensure that we mount the write area to ensure that the files in the write directory 
    /// // are available.
    /// writeArea.Mount();
    /// 
    /// // Create a text file.
    /// using (Stream stream = writeArea.OpenStream("/AFile.txt", FileMode.Create))
    /// {
    ///		using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
    ///		{
    ///			writer.WriteLine("This is a line of text.");
    ///		}
    /// }
    /// 
    /// // This should retrieve the updated file.
    /// IGorgonVirtualFile file = fileSystem.GetFile("/AFile.txt");
    /// 
    /// ]]>
    /// </code>
    /// </example>
    public interface IGorgonFileSystemWriter<out T>
        where T : Stream
    {
        #region Properties.
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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy the contents of a file system to the writable area.
        /// </summary> 
        /// <param name="sourceFileSystem">The <see cref="IGorgonFileSystem"/> to copy.</param>
        /// <param name="copyProgress">A method callback used to track the progress of the copy operation.</param>
        /// <param name="allowOverwrite">[Optional] <b>true</b> to allow overwriting of files that already exist in the file system with the same path, <b>false</b> to throw an exception when a file with the same path is encountered.</param>
        /// <returns>A <see cref="ValueTuple{T1, T2}"/> containing the number of directories (<c>item1</c>) and the number of files (<c>item2</c>) copied, or <b>null</b> if the operation was cancelled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceFileSystem"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the a file exists in <see cref="FileSystem"/>, and the <paramref name="allowOverwrite"/> parameter is set to <b>false</b>.</exception>
        /// <remarks>
        /// <para>
        /// This copies all the file and directory information from one file system, into the <see cref="FileSystem"/> linked to this writer. 
        /// </para>
        /// <para>
        /// When the <paramref name="allowOverwrite"/> is set to <b>false</b>, and a <see cref="IGorgonVirtualFile"/> already exists with the same path as another <see cref="IGorgonVirtualFile"/> in the 
        /// <paramref name="sourceFileSystem"/>, then an exception will be raised.
        /// </para>
        /// </remarks>
        (int DirectoryCount, int FileCount)? CopyFrom(IGorgonFileSystem sourceFileSystem, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true);

        /// <summary>
        /// Function to asynchronously copy the contents of a file system to the writable area.
        /// </summary>
        /// <param name="sourceFileSystem">The <see cref="IGorgonFileSystem"/> to copy.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/> used to cancel an in progress copy.</param>
        /// <param name="copyProgress">A method callback used to track the progress of the copy operation.</param>
        /// <param name="allowOverwrite">[Optional] <b>true</b> to allow overwriting of files that already exist in the file system with the same path, <b>false</b> to throw an exception when a file with the same path is encountered.</param>
        /// <returns>A <see cref="ValueTuple{T1,T2}"/> containing the number of directories (<c>item1</c>) and the number of files (<c>item2</c>) copied, or <b>null</b> if the operation was cancelled.</returns>
        /// <remarks>
        /// <para>
        /// This copies all the file and directory information from one file system, into the <see cref="FileSystem"/> linked to this writer. 
        /// </para>
        /// <para>
        /// When the <paramref name="allowOverwrite"/> is set to <b>false</b>, and a <see cref="IGorgonVirtualFile"/> already exists with the same path as another <see cref="IGorgonVirtualFile"/> in the 
        /// <paramref name="sourceFileSystem"/>, then an exception will be raised.
        /// </para>
        /// <para>
        /// This version of the copy method allows for an asynchronous copy of a set of a files and directories from another <see cref="IGorgonFileSystem"/>. This method should be used when there is a large 
        /// amount of data to transfer between the file systems.
        /// </para>
        /// <para>
        /// Unlike the <see cref="CopyFrom"/> method, this method will report the progress of the copy through the <paramref name="copyProgress"/> callback. This callback is a method that takes a 
        /// <see cref="GorgonWriterCopyProgress"/> value as a parameter that will report the current state, and will return a <see cref="bool"/> to indicate whether to continue the copy or not (<b>true</b> to 
        /// continue, <b>false</b> to stop). 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// The <paramref name="copyProgress"/> method does not switch back to the UI context. Ensure that you invoke any operations that update a UI on the appropriate thread (e.g <c>BeginInvoke</c> on a 
        /// WinForms UI element or <c>Dispatcher</c> on a WPF element).
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// This method also allows for cancellation of the copy operation by passing a <see cref="CancellationToken"/> to the <paramref name="cancelToken"/> parameter.
        /// </para>
        /// </remarks>
        Task<(int DirectoryCount, int FileCount)?> CopyFromAsync(IGorgonFileSystem sourceFileSystem, CancellationToken cancelToken, Func<GorgonWriterCopyProgress, bool> copyProgress = null, bool allowOverwrite = true);

        /// <summary>
        /// Function to create a new directory in the writable area on the physical file system.
        /// </summary>
        /// <param name="path">Path to the directory (or directories) to create.</param>
        /// <returns>A <see cref="IGorgonVirtualDirectory"/> representing the final directory in the <paramref name="path"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="path"/> is empty.
        /// <para>-or-</para>
        /// <para>Thrown if the <paramref name="path"/> does not contain any meaningful names.</para>
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when a part of the <paramref name="path"/> has the same name as a file name in the parent of the directory being created.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="path"/> is set to the root directory: <c>/</c>.</para>
        /// </exception>
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
        /// </remarks>
        IGorgonVirtualDirectory CreateDirectory(string path);

        /// <summary>
        /// Function to delete a directory from the writable area.
        /// </summary>
        /// <param name="path">Path to the directory to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the <paramref name="path"/> could not be found.</exception>
        /// <remarks>
        /// <para>
        /// This will delete a directory, its sub directories, and any files under this directory and sub directories. It will remove all references to those files and directories from the <see cref="FileSystem"/>, 
        /// and will delete these items from the <see cref="WriteLocation"/> if they exist in that location.
        /// </para>
        /// <para>
        /// When a directory is removed from the <see cref="FileSystem"/>, it will be removed all mounted file systems. However, the actual directory in the physical file systems will not be touched and the 
        /// deleted directory may be restored with a call to <see cref="IGorgonFileSystem.Refresh()"/>. 
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted directories in the <see cref="WriteLocation"/>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonFileSystem.Refresh()"/>
        void DeleteDirectory(string path);

        /// <summary>
        /// Function to open a file for reading or for writing.
        /// </summary>
        /// <param name="path">The path to the file to read/write.</param>
        /// <param name="mode">The mode to determine how to read/write the file.</param>
        /// <returns>An open <see cref="FileStream"/> to the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="path"/> does not contain a file name.</para>
        /// </exception>
        /// <exception cref="FileNotFoundException">Thrown when the file referenced by the <paramref name="path"/> was not found and the <paramref name="mode"/> is set to <see cref="FileMode.Open"/> or <see cref="FileMode.Truncate"/>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> was not found.</exception>
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
        /// </remarks>
        /// <seealso cref="FileMode"/>
        T OpenStream(string path, FileMode mode);

        /// <summary>
        /// Function to delete a file from the file system.
        /// </summary>
        /// <param name="path">The path to the file to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file referenced by the <paramref name="path"/> was not found.</exception>
        /// <remarks>
        /// <para>
        /// This will remove the <see cref="IGorgonVirtualFile"/> from the <see cref="FileSystem"/>, and will delete the physical file from the <see cref="WriteLocation"/> if it exists there.
        /// </para>
        /// <para>
        /// When a file is removed from the <see cref="FileSystem"/>, it will be removed all mounted file systems. However, the actual file in the physical file systems will not be touched and the 
        /// deleted file may be restored with a call to <see cref="IGorgonFileSystem.Refresh()"/>. 
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// When restoring files with <see cref="IGorgonFileSystem.Refresh()"/>, only the file system object will be updated. The method will not restore any deleted files in the <see cref="WriteLocation"/>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        void DeleteFile(string path);

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
        #endregion
    }
}
