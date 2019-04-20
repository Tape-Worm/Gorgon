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
// Created: Monday, June 27, 2011 9:00:18 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO.Providers
{
	/// <summary>
	/// A file system provider that mounts Windows file system directories.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="IGorgonFileSystemProvider"/> implementors must inherit from this type to create a provider plug in. 
	/// </para>
	/// <para>
	/// File system providers provide access to a physical file system, and provides the communications necessary to read data from that physical file system. When used in conjunction with the <see cref="IGorgonFileSystem"/> 
	/// object, a provider enables access to multiple types of physical file systems so they seamlessly appear to be from a single file system. The underlying system has no idea if the file is a standard 
	/// file system file, or a file inside of a zip archive.  
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// As the documentation states, providers can read data from a file system. However, no mechanism is available to write to a file system through a provider. This is by design. The <see cref="IGorgonFileSystemWriter{T}"/> 
	/// type allows writing to a file system via a predefined area in a physical file system. 
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// When this type is implemented, it can be made to read any type of file system, including those that store their contents in a packed file format (e.g. Zip). And since this type inherits from <see cref="GorgonPlugin"/>, 
	/// the file system provider can be loaded dynamically through Gorgon's plug in system.
	/// </para>
	/// <para>
	/// Some providers may not use a physical location on the operating system file system. In such cases, implementors of a <see cref="IGorgonFileSystemProvider"/> must provide a prefix for a physical location 
	/// (e.g. <c>Mount("::\\Prefix\DirectoryName", "/");</c>, <c>Mount("::\\Prefix", "/")</c>, or whatever else the provider chooses). This prefix is specific to the provider and should be made available via 
	/// the <see cref="Prefix"/> property. The prefix must <u>always</u> begin with the characters <c>::\\</c>. Otherwise, the <see cref="IGorgonFileSystem"/> will not know how to parse the physical location.
	/// </para>
	/// <para>
	/// This type allows the mounting of a directory so that data can be read from the native operating system file system. This is the default provider for any <see cref="IGorgonFileSystem"/>.
	/// </para>
	/// </remarks>
	public class GorgonFileSystemProvider
		: GorgonPlugin, IGorgonFileSystemProvider
	{
		#region Properties.
		/// <summary>
		/// Property to return the provider specific prefix for a physical location.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some providers may not use a physical location on the operating system file system. In such cases, implementors of a <see cref="IGorgonFileSystemProvider"/> must provide a prefix for a physical 
		/// location (e.g. <c>Mount("::\\Prefix\DirectoryName", "/");</c>, <c>Mount("::\\Prefix", "/")</c>, or whatever else the provider chooses). 
		/// </para>
		/// <para>
		/// This value must <u>always</u> begin with the characters <c>::\\</c>. Otherwise, the <see cref="IGorgonFileSystem"/> will not know how to parse the physical location.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// If the provider accesses a physical file system directory or file for its information, then this value should always return <see cref="string.Empty"/> or <b>null</b>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public string Prefix => null;

		/// <summary>
		/// Property to return a list of preferred file extensions (if applicable).
		/// </summary>
		/// <remarks>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> that reads from a packed file should supply a list of well known file name extensions wrapped in <see cref="GorgonFileExtension"/> objects for 
		/// that physical file system type. This list can then be then used in an application to filter the types of files to open with a <see cref="IGorgonFileSystem"/>. If the file system reads directories on 
		/// the native file system, then this collection should remain empty.
		/// </remarks>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonFileExtension> PreferredExtensions
		{
			get;
			protected set;
		}

        /// <summary>Property to return the path to the provider assembly (if applicable).</summary>
        public string ProviderPath => PlugInPath ?? string.Empty;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the virtual file system path from a physical file system path.
        /// </summary>
        /// <param name="physicalPath">Physical path to the file/folder.</param>
        /// <param name="physicalRoot">Location of the physical folder holding the root for the virtual file system.</param>
        /// <param name="mountPoint">Path to the mount point.</param>
        /// <returns>The virtual file system path.</returns>
        private static string MapToVirtualPath(string physicalPath, string physicalRoot, string mountPoint)
		{
			if ((string.IsNullOrWhiteSpace(physicalPath))
				|| (string.IsNullOrWhiteSpace(physicalRoot))
				|| (string.IsNullOrWhiteSpace(mountPoint)))
			{
				return string.Empty;
			}

			if (!Path.IsPathRooted(physicalPath))
			{
				physicalPath = Path.GetFullPath(physicalPath);
			}

			physicalPath = mountPoint + physicalPath.Replace(physicalRoot, string.Empty);

			if (physicalPath.EndsWith(GorgonFileSystem.PhysicalDirSeparator))
			{
				physicalPath = physicalPath.FormatDirectory('/');
			}
			else
			{
				physicalPath = Path.GetDirectoryName(physicalPath).FormatDirectory('/') +
							   Path.GetFileName(physicalPath).FormatFileName();
			}

			return physicalPath;
		}

		/// <summary>
		/// Function to enumerate files from the file system.
		/// </summary>
		/// <param name="physicalLocation">The physical file system location to enumerate.</param>
		/// <param name="mountPoint">The mount point to remap the file paths to.</param>
		/// <returns>A read only list of <see cref="IGorgonPhysicalFileInfo"/> entries.</returns>
		private static IReadOnlyList<IGorgonPhysicalFileInfo> EnumerateFiles(string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			var directoryInfo = new DirectoryInfo(physicalLocation);

			IEnumerable<FileInfo> files = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
			                                           .Where(item =>
			                                                  (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
			                                                  (item.Attributes & FileAttributes.System) != FileAttributes.System &&
			                                                  (item.Attributes & FileAttributes.Compressed) != FileAttributes.Compressed &&
			                                                  (item.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted &&
			                                                  (item.Attributes & FileAttributes.Device) != FileAttributes.Device);

			return files.Select(file =>
			                    new PhysicalFileInfo(file,
			                                         MapToVirtualPath(file.DirectoryName.FormatDirectory(Path.DirectorySeparatorChar) + file.Name,
			                                                          physicalLocation,
			                                                          mountPoint.FullPath)))
			            .Cast<IGorgonPhysicalFileInfo>()
			            .ToArray();
		}

		/// <summary>
		/// Function to enumerate directories from the file system.
		/// </summary>
		/// <param name="physicalLocation">The physical file system location to enumerate.</param>
		/// <param name="mountPoint">The mount point to remap the directory paths to.</param>
		/// <returns>A read only list of <see cref="string"/> values representing the mapped directory entries.</returns>
		private static IReadOnlyList<string> EnumerateDirectories(string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			var directoryInfo = new DirectoryInfo(physicalLocation);

			IEnumerable<DirectoryInfo> directories =
				directoryInfo.GetDirectories("*", SearchOption.AllDirectories)
							 .Where(
								 item =>
								 (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
								 (item.Attributes & FileAttributes.System) != FileAttributes.System);

			return directories.Select(item => MapToVirtualPath(item.FullName.FormatDirectory(Path.DirectorySeparatorChar),
															   physicalLocation,
															   mountPoint.FullPath))
							  .ToArray();
		}

        /// <summary>
        /// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
        /// </summary>
        /// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
        /// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
        /// responsibility of the user to close the stream when finished.
        /// </para>
        /// <para>
        /// If the file does not exist in the physical file system, this method should return <b>null</b>.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can overload this method to return a stream into a file within their specific native provider (e.g. a Zip file provider will 
        /// return a stream into the zip file positioned at the location of the compressed file within the zip file).
        /// </para>
        /// </remarks>
        protected virtual GorgonFileSystemStream OnOpenFileStream(IGorgonVirtualFile file) => !File.Exists(file.PhysicalFile.FullPath) ? null : new GorgonFileSystemStream(file, File.Open(file.PhysicalFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read));

        /// <summary>
        /// Function to determine if a physical file system can be read by this provider.
        /// </summary>
        /// <param name="physicalPath">Path to the packed file containing the file system.</param>
        /// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
        /// <remarks>
        /// <para>
        /// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method should always return 
        /// <b>false</b>.
        /// </para>
        /// <para>
        /// When used with a <see cref="IGorgonFileSystemProvider"/> that supports a non operating system based physical file system, such as the <see cref="GorgonFileSystemRamDiskProvider"/>, then this 
        /// method should compare the <paramref name="physicalPath"/> with its <see cref="IGorgonFileSystemProvider.Prefix"/> to ensure that the <see cref="IGorgonFileSystem"/> requesting the provider is using the correct provider.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to determine if a packed file can be read by reading the header of the file specified in <paramref name="physicalPath"/>.
        /// </para>
        /// </remarks>
        protected virtual bool OnCanReadFile(string physicalPath) => false;

        /// <summary>
        /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
        /// </summary>
        /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
        /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
        /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
        /// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
        /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
        /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
        /// The default functionality will only enumerate directories and files from the operating system file system.
        /// </para>
        /// </remarks>
        protected virtual GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint) => new GorgonPhysicalFileSystemData(EnumerateDirectories(physicalLocation, mountPoint), EnumerateFiles(physicalLocation, mountPoint));

        /// <summary>
        /// Function to enumerate the files and directories from a physical location and map it to a virtual location.
        /// </summary>
        /// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
        /// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
        /// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
        /// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
        /// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
        /// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
        /// </para>
        /// <para>
        /// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
        /// </para>
        /// </remarks>
        public GorgonPhysicalFileSystemData Enumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			if (physicalLocation == null)
			{
				throw new ArgumentNullException(nameof(physicalLocation));
			}

			if (mountPoint == null)
			{
				throw new ArgumentNullException(nameof(mountPoint));
			}

			if (string.IsNullOrWhiteSpace(physicalLocation))
			{
				throw new ArgumentEmptyException(nameof(physicalLocation));
			}

			return OnEnumerate(physicalLocation, mountPoint);
		}

		/// <summary>
		/// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
		/// </summary>
		/// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
		/// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
		/// responsibility of the user to close the stream when finished.
		/// </para>
		/// </remarks>
		public Stream OpenFileStream(IGorgonVirtualFile file)
		{
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			return OnOpenFileStream(file);
		}

		/// <summary>
		/// Function to determine if a physical file system can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the packed file containing the file system.</param>
		/// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		/// <remarks>
		/// <para>
		/// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method should always return 
		/// <b>false</b>.
		/// </para>
		/// <para>
		/// When used with a <see cref="IGorgonFileSystemProvider"/> that supports a non operating system based physical file system, such as the <see cref="GorgonFileSystemRamDiskProvider"/>, then this 
		/// method should compare the <paramref name="physicalPath"/> with its <see cref="IGorgonFileSystemProvider.Prefix"/> to ensure that the <see cref="IGorgonFileSystem"/> requesting the provider is using the correct provider.
		/// </para>
		/// </remarks>
		public bool CanReadFileSystem(string physicalPath)
		{
			if (physicalPath == null)
			{
				throw new ArgumentNullException(nameof(physicalPath));
			}

			if (string.IsNullOrWhiteSpace(physicalPath))
			{
				throw new ArgumentEmptyException(nameof(physicalPath));
			}

			return OnCanReadFile(physicalPath);
		}
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class.
        /// </summary>
        /// <param name="providerDescription">The human readable description for the file system provider.</param>
        protected GorgonFileSystemProvider(string providerDescription)
            : base(providerDescription) => PreferredExtensions = new GorgonFileExtensionCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class.
        /// </summary>
        /// <remarks>
        /// Every copy of the <see cref="IGorgonFileSystem"/> automatically creates an instance of this type as a default provider.
        /// </remarks>
        internal GorgonFileSystemProvider()
			: this(Resources.GORFS_FOLDER_FS_DESC)
		{
		}
		#endregion
	}
}
