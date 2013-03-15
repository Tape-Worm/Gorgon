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
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// Interface for an Gorgon file system provider.
	/// </summary>
	public abstract class GorgonFileSystemProvider
		: INamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the file system that owns this provider.
		/// </summary>
		public GorgonFileSystem FileSystem
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of preferred file extensions (if applicable).
		/// </summary>
		public IEnumerable<string> PreferredExtensions
		{
			get;
			protected set;
		}

        /// <summary>
        /// Property to return a description of the file system provider.
        /// </summary>
        public abstract string Description
        {
            get;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a file entry to the list of files.
		/// </summary>
		/// <param name="path">Path to the file entry.</param>
		/// <param name="mountPoint">The mount point that contains the file.</param>
		/// <param name="physicalLocation">Location of the file in the physical file system.</param>
		/// <param name="size">Size of the file in bytes.</param>
		/// <param name="offset">Offset of the file within a packed file.</param>
		/// <param name="createDate">Date the file was created.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the path parameter is an empty string.
		/// <para>-or-</para><para>The file already exists.</para>
		/// <para>-or-</para><para>The file name is missing from the path.</para>
		/// <para>-or-</para><para>The path was not found in the file system.</para>
		/// </exception>
		/// <returns>A new file system entry.</returns>
		protected GorgonFileSystemFileEntry AddFileEntry(string path, string mountPoint, string physicalLocation, long size, long offset, DateTime createDate)
		{
			return FileSystem.AddFileEntry(this, path, mountPoint, physicalLocation, size, offset, createDate);
		}
		
		/// <summary>
		/// Function to add a directory entry to the list of directories.
		/// </summary>
		/// <param name="path">Path to the directory entry.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the path parameter is an empty string.
		/// <para>-or-</para><para>The directory already exists.</para>
		/// <para>-or-</para><para>The driectory path is not valid.</para></exception>
		/// <returns>A new virtual directory entry.</returns>
		protected GorgonFileSystemDirectory AddDirectoryEntry(string path)
		{
			return FileSystem.AddDirectoryEntry(path);
		}

		/// <summary>
		/// Function to return the writeable path including the virtual path passed in.
		/// </summary>
		/// <param name="path">Virtual path.</param>
		/// <returns>The physical writeable path.</returns>
		protected string GetWritePath(string path)
		{
			return FileSystem.GetWritePath(path);
		}

		/// <summary>
		/// Function to write a file into the write location.
		/// </summary>
		/// <param name="file">File to write to the write location.</param>
		/// <returns>The writable stream for the file.</returns>
		/// <exception cref="System.InvalidOperationException">Thrown when the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> property is not set.</exception>
		protected GorgonFileSystemStream WriteToWriteLocation(GorgonFileSystemFileEntry file)
		{			
			if (string.IsNullOrEmpty(FileSystem.WriteLocation))
				throw new InvalidOperationException("Cannot write to the file '" + file.FullPath + "' because the there is no write path set.");

			string newPath = GetWritePath(file.FullPath);

			if (!Directory.Exists(Path.GetDirectoryName(newPath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(newPath));
			}

			// Write the file out to the write location.
			FileStream stream = File.Open(newPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			UpdateFileInfo(file, null, null, null, FileSystem.WriteLocation, newPath, FileSystem.Providers[FileSystem.DefaultProviderType.FullName]);

			return new GorgonFileSystemStream(file, stream);
		}


		/// <summary>
		/// Function to enumerate the files and directories for a mount point.
		/// </summary>
		/// <param name="physicalMountPoint">Mount point being enumerated.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>		
		protected abstract void Enumerate(string physicalMountPoint, GorgonFileSystemDirectory mountPoint);

		/// <summary>
		/// Function called when a file is read from the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The file data stored in a byte array or NULL if the file could not be read.</returns>
		/// <remarks>Implementors must implement this method to read the file from the physical file system.</remarks>
		protected abstract byte[] OnReadFile(GorgonFileSystemFileEntry file);

		/// <summary>
		/// Function called when a file is written to the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <param name="data">Data to write to the file.</param>
		/// <exception cref="System.InvalidOperationException">Thrown when the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> on the file system is empty.</exception>
		protected virtual void OnWriteFile(GorgonFileSystemFileEntry file, byte[] data)
		{
            if (data != null)
            {
                throw new ArgumentNullException("data");                
            }

			using (Stream stream = WriteToWriteLocation(file))
			{
				stream.Write(data, 0, data.Length);
			}
		}

		/// <summary>
		/// Function called when a file is opened as a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <param name="writeable">TRUE if the file can be written to, FALSE if not.</param>
		/// <returns>The open <see cref="GorgonLibrary.FileSystem.GorgonFileSystemStream"/> file stream object.</returns>
		/// <remarks>Some providers cannot write, and should throw an exception.</remarks>
		protected abstract GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file, bool writeable);

		/// <summary>
		/// Function called when a file system is unloaded.
		/// </summary>
		protected virtual internal void OnUnload()
		{
			var files = FileSystem.FindFiles("*", true).Where(file => file.Provider == this);

			// Remove any files attached to this provider.
			foreach (GorgonFileSystemFileEntry file in files)
				file.Directory.Files.Remove(file);
		}

		/// <summary>
		/// Function to read a file from the
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>An array of bytes containing the data in the file.</returns>
		internal byte[] ReadFile(GorgonFileSystemFileEntry file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			return OnReadFile(file);
		}

		/// <summary>
		/// Function to read a file from the
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <param name="data">Data to write.</param>
		internal void WriteFile(GorgonFileSystemFileEntry file, byte[] data)
		{
            FileInfo info = null;
            string newPath = string.Empty;

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (data != null)
            {
                OnWriteFile(file, data);

                info = new FileInfo(file.PhysicalFileSystemPath);
                UpdateFileInfo(file, info.Length, null, info.CreationTime, FileSystem.WriteLocation, file.PhysicalFileSystemPath, FileSystem.Providers[FileSystem.DefaultProviderType.FullName]);
            }
        }

		/// <summary>
		/// Function to open a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <param name="writeable">TRUE to write to the file, FALSE to make read-only.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="writeable"/> parameter is TRUE and the <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">WriteLocation</see> property has not been set.</exception>
		/// <returns>The open <see cref="GorgonLibrary.FileSystem.GorgonFileSystemStream"/> file stream object.</returns>
		internal GorgonFileSystemStream OpenStream(GorgonFileSystemFileEntry file, bool writeable)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			return OnOpenFileStream(file, writeable);
		}

		/// <summary>
		/// Function to mount a physical file system location into this provider.
		/// </summary>
		/// <param name="physicalPath">Path on the physical file system to load.</param>
		/// <param name="mountPoint">The mount point for the file system directories/files.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		internal void Mount(string physicalPath, string mountPoint)
		{
			GorgonFileSystemDirectory mountDirectory = null;
			string fileName = string.Empty;

			GorgonDebug.AssertParamString(physicalPath, "physicalPath");
			GorgonDebug.AssertParamString(mountPoint, "mountPoint");

			physicalPath = Path.GetFullPath(physicalPath);
			fileName = Path.GetFileName(physicalPath);
			physicalPath = Path.GetDirectoryName(physicalPath).FormatDirectory(Path.DirectorySeparatorChar);
			if (!string.IsNullOrEmpty(fileName))
				physicalPath += fileName.FormatFileName();

			mountDirectory = FileSystem.GetDirectory(mountPoint);

			if (mountDirectory == null)
				mountDirectory = AddDirectoryEntry(mountPoint);

			Gorgon.Log.Print("'{2}' is mounting physical file system path '{0}' to virtual file system path '{1}'.", Diagnostics.LoggingLevel.Verbose, physicalPath, mountPoint, this.Name);
			
			Enumerate(physicalPath, mountDirectory);

#if DEBUG
			var fileCount = FileSystem.FindFiles("*", true).Count();
			var dirCount = FileSystem.FindDirectories("*", true).Count();

			Gorgon.Log.Print("{0} directories parsed, and {1} files processed.", Diagnostics.LoggingLevel.Verbose, dirCount, fileCount);
#endif
		}

		/// <summary>
		/// Function to update the file information for a file.
		/// </summary>
		/// <param name="file">File to update.</param>
		/// <param name="fileSize">The file size.  Pass NULL (Nothing in VB.Net) to leave unchanged.</param>
		/// <param name="fileOffset">The offset of the file within a packed file.  Pass NULL (Nothing in VB.Net) to leave unchanged.</param>
		/// <param name="createDate">The date/time the file was created.  Pass NULL (Nothing in VB.Net) to leave unchanged.</param>
		/// <param name="mountPoint">The new mount point for the file.  Pass NULL (Nothing in VB.Net) to leave unchanged.</param>
		/// <param name="physicalPath">A new file system provider for the file.  Pass NULL (Nothing in VB.Net) or an empty string to leave unchanged.</param>
		/// <param name="provider">A new file system provider for the file.  Pass NULL (Nothing in VB.Net) to leave unchanged.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected void UpdateFileInfo(GorgonFileSystemFileEntry file, long? fileSize, int? fileOffset, DateTime? createDate, string mountPoint, string physicalPath, GorgonFileSystemProvider provider )
		{
			if (file == null)
				throw new ArgumentNullException("file");

			if (fileSize.HasValue)
			{
				file.Size = fileSize.Value;
			}

			if (fileOffset.HasValue)
			{
				file.Offset = fileOffset.Value;
			}

			if (createDate.HasValue)
			{
				file.CreateDate = createDate.Value;
			}

			if (!string.IsNullOrWhiteSpace(mountPoint))
			{
				file.MountPoint = mountPoint;
			}

			if (provider != null)
			{
				file.Provider = provider;
			}

			if (!string.IsNullOrWhiteSpace(physicalPath))
			{
				file.PhysicalFileSystemPath = physicalPath;
			}
		}

		/// <summary>
		/// Function to determine if a file can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the file containing the file system.</param>
		/// <returns>TRUE if the provider can read the packed file, FALSE if not.</returns>
		/// <remarks>This method is applicable to packed files only.
		/// <para>Implementors must use this method to determine if a packed file can be read by reading the header of the file.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		public abstract bool CanReadFile(string physicalPath);
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProvider"/> class.
		/// </summary>
		/// <param name="fileSystem">File system that owns this provider.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonFileSystemProvider(GorgonFileSystem fileSystem)			
		{
			FileSystem = fileSystem;
			PreferredExtensions = new List<string>();
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get 
			{
				return this.GetType().FullName;
			}
		}
		#endregion
	}
}
