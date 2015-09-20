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
using Gorgon.IO.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO.Providers
{
	/// <inheritdoc cref="IGorgonFileSystemProvider"/>
	[ExcludeAsPlugin]
	public class GorgonFileSystemProvider
		: GorgonPlugin, IGorgonFileSystemProvider
	{
		#region Properties.
		/// <inheritdoc/>
		public IGorgonNamedObjectDictionary<GorgonFileExtension> PreferredExtensions
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate files from the file system.
		/// </summary>
		/// <param name="physicalLocation">The physical file system location to enumerate.</param>
		/// <param name="mountPoint">The mount point to remap the file paths to.</param>
		/// <returns>A read only list of <see cref="IGorgonPhysicalFileInfo"/> entries.</returns>
		private IReadOnlyList<IGorgonPhysicalFileInfo> EnumerateFiles(string physicalLocation, GorgonFileSystemDirectory mountPoint)
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
		private IReadOnlyList<string> EnumerateDirectories(string physicalLocation, GorgonFileSystemDirectory mountPoint)
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
		/// <inheritdoc cref="OpenFileStream"/>
		/// </summary>
		/// <param name="file"><inheritdoc cref="OpenFileStream"/></param>
		/// <returns><inheritdoc cref="OpenFileStream"/></returns>
		/// <remarks>
		/// <inheritdoc cref="OpenFileStream"/>
		/// <para>
		/// If the file does not exist in the physical file system, this method should return <b>null</b> (<i>Nothing</i> in VB.Net).
		/// </para>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can overload this method to return a stream into a file within their specific native provider (e.g. a Zip file provider will 
		/// return a stream into the zip file positioned at the location of the compressed file within the zip file).
		/// </para>
		/// </remarks>
		protected virtual GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file)
		{
			return !File.Exists(file.PhysicalFile.FullPath) ? null : new GorgonFileSystemStream(file, File.Open(file.PhysicalFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		/// <summary>
		/// <inheritdoc cref="CanReadFile"/>
		/// </summary>
		/// <param name="physicalPath"><inheritdoc cref="CanReadFile"/></param>
		/// <returns><inheritdoc cref="CanReadFile"/></returns>
		/// <remarks>
		/// <inheritdoc cref="CanReadFile"/>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to determine if a packed file can be read by reading the header of the file specified in <paramref name="physicalPath"/>.
		/// </para>
		/// </remarks>
		protected virtual bool OnCanReadFile(string physicalPath)
		{
			return false;
		}

		/// <summary>
		/// Function to return the virtual file system path from a physical file system path.
		/// </summary>
		/// <param name="physicalPath">Physical path to the file/folder.</param>
		/// <param name="physicalRoot">Location of the physical folder holding the root for the virtual file system.</param>
		/// <param name="mountPoint">Path to the mount point.</param>
		/// <returns>The virtual file system path.</returns>
		public string MapToVirtualPath(string physicalPath, string physicalRoot, string mountPoint)
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
		/// <inheritdoc cref="Enumerate"/>
		/// </summary> 
		/// <param name="physicalLocation"><inheritdoc cref="Enumerate"/></param>
		/// <param name="mountPoint"><inheritdoc cref="Enumerate"/></param>
		/// <returns><inheritdoc cref="Enumerate"/></returns>
		/// <remarks>
		/// <inheritdoc cref="Enumerate"/>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
		/// The default functionality will only enumerate directories and files from the operating system file system.
		/// </para>
		/// </remarks>
		protected virtual IGorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, GorgonFileSystemDirectory mountPoint)
		{
			return new GorgonPhysicalFileSystemData(EnumerateDirectories(physicalLocation, mountPoint), EnumerateFiles(physicalLocation, mountPoint));
		}

		/// <inheritdoc/>
		public IGorgonPhysicalFileSystemData Enumerate(string physicalLocation, GorgonFileSystemDirectory mountPoint)
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
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(physicalLocation));
			}

			return OnEnumerate(physicalLocation, mountPoint);
		}

		/// <inheritdoc/>
		public Stream OpenFileStream(GorgonFileSystemFileEntry file)
		{
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			return OnOpenFileStream(file);
		}

		/// <inheritdoc/>
		public bool CanReadFile(string physicalPath)
		{
			if (physicalPath == null)
			{
				throw new ArgumentNullException(nameof(physicalPath));
			}

			if (string.IsNullOrWhiteSpace(physicalPath))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(physicalPath));
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
			: base(providerDescription)
		{
			PreferredExtensions = new GorgonFileExtensionCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class.
		/// </summary>
		/// <remarks>
		/// Every copy of the <see cref="GorgonFileSystem"/> automatically creates an instance of this type as a default provider.
		/// </remarks>
		internal GorgonFileSystemProvider()
			: this(Resources.GORFS_FOLDER_FS_DESC)
		{
		}
		#endregion
	}
}
