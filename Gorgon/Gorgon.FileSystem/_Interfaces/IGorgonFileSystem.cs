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
// Created: Thursday, September 24, 2015 10:30:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <summary>
	/// The virtual file System interface.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will allow the user to mount directories or packed files (such as Zip files) into a unified file system.  For example, if the user has mounted MyData.zip and C:\users\Bob\Data\ into the file system 
	/// then all files and/or directories from both sources would be combined into a single virtual file system. This has the advantage of being able to access disparate file systems without having to run 
	/// through multiple interfaces to get at their data.
	/// </para>
	/// <para>
	/// The virtual file system is a read only file system. This is done by design so that the integrity of the original physical file systems can be preserved. If your application needs to write data into the 
	/// file system, then the <see cref="IGorgonFileSystemWriteArea{T}"/> has been provided to give safe access to a writable area that will integrate with this object.
	/// </para>
	/// <para>
	/// Physical file systems (such as a windows directory or a Zip file) are "mounted" into this object. When a physical file system is mounted, all of the file names (and other info) and directory names will be 
	/// stored in hierarchal structure similar to a unix directory structure. Because of this, there will be some differences from the typical Windows directory setup:
	/// <list type="bullet">
	/// <item>
	///		<description>The root of the file system is not "C:\" or "\\Computer\". In this object, the root is '/' (e.g. <c>/MyFile.txt</c> is a file located in the root).</description> 
	/// </item>
	/// <item>
	///		<description>The directory separators are forward slashes: / and not back slashes: \. (e.g. <c>c:\MyDirectory\MySubDirectory\</c> is now <c><![CDATA[/MyDirectory/MySubDirectory/]]></c>)</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// The order in which file systems are mounted into the virtual file system is important.  If a zip file contains SomeText.txt, and a directory contains the same file path, then if the zip file is mounted, 
	/// followed by the directory, the file in the directory will override the file in the zip file. This is true for directories as well. 
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// By default, a new file system instance will only have access to the folders and files of the hard drive via the default folder file provider. File systems that are in packed files (e.g. Zip files) can 
	/// be loaded into the file system by way of a <see cref="GorgonFileSystemProvider"/>. Providers are typically plug in objects that are loaded into the file system via the 
	/// <see cref="GorgonFileSystemProviderFactory"/>.  Once a provider plug-in is loaded, then the contents of that file system can be mounted like a standard directory. For example, if the zip file provider 
	/// plug in is loaded, then the file system may be mounted into the root by: <c>fileSystem.Mount("d:\zipFiles\myZipFile.zip", "/");</c>. 
	/// </para>
	/// </remarks>
	/// <seealso cref="IGorgonFileSystemWriteArea{T}"/>
	/// <seealso cref="GorgonFileSystemProvider"/>
	public interface IGorgonFileSystem
	{
		/// <summary>
		/// Property to return the default file system provider for this file system.
		/// </summary>
		IGorgonFileSystemProvider DefaultProvider
		{
			get;
		}

		/// <summary>
		/// Property to return a list of mount points that are currently assigned to this file system.
		/// </summary>
		/// <remarks>
		/// This is a list of <see cref="GorgonFileSystemMountPoint"/> values. These values contain location of the mount point in the virtual file system, the physical location of the physical file system and 
		/// the provider that mounted the physical file system.
		/// </remarks>
		IEnumerable<GorgonFileSystemMountPoint> MountPoints
		{
			get;
		}

		/// <summary>
		/// Property to return the root directory for the file system.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is the beginning of the directory/file structure for the file system. Users can walk through the properties on this object to get the sub directories and files for a virtual file system as a 
		/// hierarchical view.
		/// </para>
		/// <para>
		/// <note type="tip">
		/// <para>
		/// When populating a tree view, this property is useful for helping to lay out the nodes in the tree.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		IGorgonVirtualDirectory RootDirectory
		{
			get;
		}

		/// <summary>
		/// Function to find all the directories with the name specified by the directory mask.
		/// </summary>
		/// <param name="path">Path to the directory to start searching in.</param>
		/// <param name="directoryMask">The directory name or mask to search for.</param>
		/// <param name="recursive">[Optional] <b>true</b> to search all child directories, <b>false</b> to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="IGorgonVirtualDirectory"/> objects that match the <paramref name="directoryMask"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryMask"/> or the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="directoryMask"/> or the path parameter is a zero length string.</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> was not found.</exception>
		/// <remarks>
		/// <para>
		/// This will look for all the directories specified by the <paramref name="directoryMask"/> parameter. This parameter will accept wild card characters like directory*, directory??1 and directory*a* when 
		/// searching so that pattern matching can be used to find a series of directories. When a wild card is omitted, then only that name is sought out.
		/// </para> 
		/// <para>
		/// <note type="warning">
		/// <para>
		/// The <paramref name="directoryMask"/> is not a path.  It is the name (or mask of the name) of the directory we wish to find.  Calling <c>FindDirectories("/", "<![CDATA[/MyDir/ThisDir/C*w/]]>");</c> 
		/// will not return any results.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		IEnumerable<IGorgonVirtualDirectory> FindDirectories(string path, string directoryMask, bool recursive = true);

		/// <summary>
		/// Function to find all the files specified in the file mask.
		/// </summary>
		/// <param name="path">Path to the directory to start searching in.</param>
		/// <param name="fileMask">The file name or mask to search for.</param>
		/// <param name="recursive"><b>true</b> to search all directories, <b>false</b> to search only the immediate directory.</param>
		/// <returns>An enumerable object containing <see cref="IGorgonVirtualFile"/>objects.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="fileMask"/> or the <paramref name="path"/> is a zero length string.</exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory in the <paramref name="path"/> parameter was not found.</exception>
		/// <remarks>This method will accept file name masks like file*, file??1 and file*a* when searching.
		/// <para>Please note that the <paramref name="fileMask"/> is not a path.  It is the name (or mask of the name) of the file we wish to find.  Specifying something like /MyDir/ThisDir/C*w.ext will fail.</para>
		/// </remarks>
		IEnumerable<IGorgonVirtualFile> FindFiles(string path, string fileMask, bool recursive = true);

		/// <summary>
		/// Function to retrieve a file from the file system.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The file requested or NULL (<i>Nothing</i> in VB.Net) if the file was not found.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when there is no file name in the path.</para>
		/// </exception>
		IGorgonVirtualFile GetFile(string path);

		/// <summary>
		/// Function to retrieve a directory from the provider.
		/// </summary>
		/// <param name="path">Path to the directory to retrieve.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is an empty string.</exception>
		/// <returns>The file system directory if found, NULL (<i>Nothing</i> in VB.Net) if not.</returns>
		IGorgonVirtualDirectory GetDirectory(string path);

		/// <summary>
		/// Function to refresh all the files and directories in the file system.
		/// </summary> 
		/// <remarks>
		/// <para>
		/// This will unmount and re-mount all the known mount points for the file system, effectively rebuilding the file system file/directory tree.
		/// </para>
		/// <para>
		/// Because of the read only nature of the mounted file systems, any files or directories deleted from the file system via the <see cref="IGorgonFileSystemWriteArea{T}"/> will re-appear since they were 
		/// never deleted from the source file system and will need to be deleted again to remove them from the file system interface.
		/// </para>
		/// </remarks>
		void Refresh();

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at by a mount point.
		/// </summary>
		/// <param name="mountPoint">The mount point to unmount.</param>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		void Unmount(GorgonFileSystemMountPoint mountPoint);

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at the by the physical path specified and mounted into the mount location specified.
		/// </summary>
		/// <param name="physicalPath">Physical file system path.</param>
		/// <param name="mountLocation">Virtual sub directory that the physical location is mounted under.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> or the <paramref name="mountLocation"/> parameters are empty.</exception>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		void Unmount(string physicalPath, string mountLocation);

		/// <summary>
		/// Function to unmount the mounted virtual file system directories and files pointed at by a physical path.
		/// </summary>
		/// <param name="physicalPath">The physical path to unmount.</param>
		/// <remarks>This overload will unmount all the mounted virtual files/directories for every mount point with the specified <paramref name="physicalPath"/>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is empty.</exception>
		/// <exception cref="System.IO.IOException">The path was not found.</exception>
		void Unmount(string physicalPath);
		
		/// <summary>
		/// Function to mount a physical file system into the virtual file system.
		/// </summary>
		/// <param name="physicalPath">Path to the directory or file that contains the files/directories to enumerate.</param>
		/// <param name="mountPath">[Optional] Folder path to mount into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory specified by <paramref name="physicalPath"/> was not found.</exception>
		/// <exception cref="System.IO.FileNotFoundException">Thrown if a file was specified by <paramref name="physicalPath"/> and was not found.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the file pointed to by the physicalPath parameter could not be read by any of the file system providers.</exception>
		/// <returns>A mount point value for the currently mounted physical path and its mount point in the virtual file system.</returns>
		/// <remarks>
		/// <para>
		/// This method is used to mount the contents of a physical file system object (such as a folder, or a zip file if the appropriate provider is installed) into a virtual folder in the 
		/// file system. All folders and files in the physical file system object will be made available under the virtual folder specified by the <paramref name="mountPath"/> parameter.
		/// </para>
		/// <para>
		/// The <paramref name="mountPath"/> parameter is optional, and if omitted, the contents of the physical file system object will be mounted into the root of the virtual file system. If the 
		/// <paramref name="mountPath"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net) or empty, then the mount point will be at the root.
		/// </para>
		/// </remarks>
		GorgonFileSystemMountPoint Mount(string physicalPath, string mountPath = null);
	}
}