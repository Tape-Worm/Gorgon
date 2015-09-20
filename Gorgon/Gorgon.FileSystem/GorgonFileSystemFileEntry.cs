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
// Created: Monday, June 27, 2011 8:59:25 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <summary>
	/// A file entry corresponding to a file on the physical file system.
	/// </summary>
	public sealed class GorgonFileSystemFileEntry
		: IGorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the file system that owns this file.
		/// </summary>
		public GorgonFileSystem FileSystem => Directory?.FileSystem;

		/// <summary>
		/// Property to return the physical file information for this virtual file.
		/// </summary>
		public IGorgonPhysicalFileInfo PhysicalFile
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the file system that can access this file.
		/// </summary>
		public GorgonFileSystemProvider Provider
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the full path to the file in the virtual file system.
		/// </summary>
		public string FullPath
		{
			get
			{
			    if (Directory == null)
			    {
			        return Name;
			    }

			    return Directory.FullPath + Name;
			}
		}

		/// <summary>
		/// Property to return the mount point that holds the file.
		/// </summary>
		public string MountPoint
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the directory that owns this file.
		/// </summary>
		public GorgonFileSystemDirectory Directory
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the extension
		/// </summary>
		public string Extension
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the file name without the extension.
		/// </summary>
		public string BaseFileName
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the uncompressed size of the file in bytes.
		/// </summary>
		public long Size => PhysicalFile.Length;

		/// <summary>
		/// Property to return the file creation date.
		/// </summary>
		public DateTime CreateDate => PhysicalFile.CreateDate;

		/// <summary>
		/// Property to return the last modified date.
		/// </summary>
		public DateTime LastModifiedDate => PhysicalFile.LastModifiedDate;

		/// <summary>
		/// Property to return the filename and extension for this virtual file.
		/// </summary>
		public string Name => PhysicalFile.Name;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the information about this file.
		/// </summary>
		/// <param name="fileInfo">The physical file information.</param>
		/// <param name="mountPoint">The mount point for the file.</param>
		/// <param name="provider">Provider that can access the file.</param>
		internal void Update(IGorgonPhysicalFileInfo fileInfo, string mountPoint, GorgonFileSystemProvider provider)
		{
			PhysicalFile = fileInfo;
			Extension = Path.GetExtension(fileInfo.Name);
			BaseFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

			if (!string.IsNullOrWhiteSpace(mountPoint))
            {
                MountPoint = mountPoint;
            }

            if (provider != null)
            {
                Provider = provider;
            }
        }

        /// <summary>
        /// Function to read the file.
        /// </summary>
        /// <returns>An array of bytes containing the file data.</returns>
        public byte[] Read()
        {
            return FileSystem.ReadFile(this);
        }

        /// <summary>
        /// Function to write to the file.
        /// </summary>
        /// <param name="data">The data to write to the file.</param>
        public void Write(byte[] data)
        {
            FileSystem.WriteFile(this, data);
        }

        /// <summary>
        /// Function to delete this file.
        /// </summary>
        public void Delete()
        {
            FileSystem.DeleteFile(this);
        }

        /// <summary>
        /// Function to open a stream to the file on the physical file system.
        /// </summary>
        /// <param name="writeable"><b>true</b> to write to the file, <b>false</b> to make read-only.</param>
        /// <returns>The open <see cref="GorgonFileSystemStream"/> file stream object.</returns>
        public Stream OpenStream(bool writeable)
        {
            return FileSystem.OpenStream(this, writeable);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemFileEntry"/> class.
		/// </summary>
		/// <param name="provider">The file system provider that owns this file.</param>
		/// <param name="directory">The directory that holds this file.</param>
		/// <param name="fileInfo">Information about the physical file.</param>
		/// <param name="mountPoint">The mount point that holds the file.</param>
		internal GorgonFileSystemFileEntry(GorgonFileSystemProvider provider, GorgonFileSystemDirectory directory, IGorgonPhysicalFileInfo fileInfo, string mountPoint)
		{
			PhysicalFile = fileInfo;
			Provider = provider;
			Directory = directory;
			Extension = Path.GetExtension(fileInfo.Name);
			BaseFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
			MountPoint = mountPoint;
		}
		#endregion
	}
}
