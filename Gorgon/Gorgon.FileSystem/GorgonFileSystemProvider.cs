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
using System.IO;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// The base Gorgon file system provider.
	/// </summary>
	/// <returns>File system providers are used to read in file information from various sources.  For example, to read files from a WinZip file all one needs to do is derive 
	/// a provider from this one.
	/// <remarks>File system providers are read-only systems, so writing via the provider is not allowed or available.</remarks>
	/// </returns>
	public class GorgonFileSystemProvider
		: IGorgonNamedObject
	{
		#region Value Types.
		/// <summary>
		/// Information about a physical file.
		/// </summary>
		public struct PhysicalFileInfo
		{
			/// <summary>
			/// Full path to the physical file.
			/// </summary>
			public readonly string FullPath;
			/// <summary>
			/// Name of the file.
			/// </summary>
			public readonly string Name;
			/// <summary>
			/// Date of creation for the file.
			/// </summary>
			public readonly DateTime CreateDate;
			/// <summary>
			/// Offset, in bytes, of the file within a packed file.
			/// </summary>
			public readonly long Offset;
			/// <summary>
			/// Length of the file, in bytes.
			/// </summary>
			public readonly long Length;
			/// <summary>
			/// Virtual path for the file.
			/// </summary>
			public readonly string VirtualPath;

			/// <summary>
			/// Initializes a new instance of the <see cref="PhysicalFileInfo"/> struct.
			/// </summary>
			/// <param name="physicalPath">The full physical path.</param>
			/// <param name="fileName">The name of the file (without directory).</param>
			/// <param name="createDate">The create date.</param>
			/// <param name="offset">The offset.</param>
			/// <param name="size">The size.</param>
			/// <param name="virtualPath">The virtual path.</param>
			public PhysicalFileInfo(string physicalPath, string fileName, DateTime createDate, long offset, long size, string virtualPath)
			{
				FullPath = physicalPath;
				Name = fileName;
				CreateDate = createDate;
				Offset = offset;
				Length = size;
				VirtualPath = virtualPath;
			}
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of preferred file extensions (if applicable).
		/// </summary>
		public IGorgonNamedObjectDictionary<GorgonFileExtension> PreferredExtensions
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return a description of the file system provider.
        /// </summary>
        public virtual string Description
        {
            get
            {
	            return Resources.GORFS_FOLDER_FS_DESC;
            }
        }
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
		/// Function to enumerate the files and directories for a mount point.
		/// </summary>
		/// <param name="physicalMountPoint">Mount point being enumerated.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>		
		/// <param name="physicalDirectories">A list of directories in the physical file system (formatted to the virtual file system).</param>
		/// <param name="physicalFiles">A list of files in the physical file system (formatted to the virtual file system).</param>
		protected internal virtual void Enumerate(string physicalMountPoint, GorgonFileSystemDirectory mountPoint, out string[] physicalDirectories,
		                       out PhysicalFileInfo[] physicalFiles)
		{
			var directoryInfo = new DirectoryInfo(physicalMountPoint);

			DirectoryInfo[] directories =
				directoryInfo.GetDirectories("*", SearchOption.AllDirectories)
				             .Where(
					             item =>
					             (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
					             (item.Attributes & FileAttributes.System) != FileAttributes.System)
				             .ToArray();

			FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
							.Where(
								item =>
								(item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
								(item.Attributes & FileAttributes.System) != FileAttributes.System)
			                .ToArray();

			physicalDirectories = new string[directories.Length];
			physicalFiles = new PhysicalFileInfo[files.Length];

			for (int i = 0; i < directories.Length; i++)
			{
				DirectoryInfo directory = directories[i];

				physicalDirectories[i] = MapToVirtualPath(directory.FullName.FormatDirectory(Path.DirectorySeparatorChar), physicalMountPoint, mountPoint.FullPath);
			}

			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = files[i];
				string newPath = MapToVirtualPath(file.DirectoryName.FormatDirectory(Path.DirectorySeparatorChar) + file.Name, physicalMountPoint, mountPoint.FullPath);

				physicalFiles[i] = new PhysicalFileInfo(file.FullName, file.Name, file.CreationTime, 0, file.Length, newPath);
			}
		}

		/// <summary>
		/// Function called when a file is read from the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The file data stored in a byte array or NULL if the file could not be read.</returns>
		/// <remarks>Implementors must implement this method to read the file from the physical file system.</remarks>
		protected internal virtual byte[] OnReadFile(GorgonFileSystemFileEntry file)
		{
			byte[] data;

			// Return an empty file if the file does not exist on the physical file system.
			if (!File.Exists(file.PhysicalFileSystemPath))
			{
				return new byte[0];
			}

			using (FileStream stream = File.Open(file.PhysicalFileSystemPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				data = new byte[stream.Length];

				if (stream.Length > 0)
				{
					stream.Read(data, 0, (int)stream.Length);
				}
			}

			return data;
		}

		/// <summary>
		/// Function called when a file is opened as a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <returns>The open <see cref="GorgonFileSystemStream"/> file stream object.</returns>
		protected internal virtual GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file)
		{
			return new GorgonFileSystemStream(file, File.Open(file.PhysicalFileSystemPath, FileMode.Open, FileAccess.Read, FileShare.Read));			
		}

		/// <summary>
		/// Function called when a file system is unloaded.
		/// </summary>
		protected virtual internal void OnUnload()
		{
		}

		/// <summary>
		/// Function to determine if a file can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the file containing the file system.</param>
		/// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
		/// <remarks>This method is applicable to packed files only.
		/// <para>Implementors must use this method to determine if a packed file can be read by reading the header of the file.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		public virtual bool CanReadFile(string physicalPath)
		{
			return false;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class.
		/// </summary>
		protected internal GorgonFileSystemProvider()			
		{
			PreferredExtensions = new GorgonFileExtensionCollection();
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get 
			{
				return GetType().FullName;
			}
		}
		#endregion
	}
}
