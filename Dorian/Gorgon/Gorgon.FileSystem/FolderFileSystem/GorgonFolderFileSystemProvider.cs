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
// Created: Monday, June 27, 2011 8:54:33 AM
// 
#endregion

using System;
using System.IO;
using GorgonLibrary.IO;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// A file system provider interface to access files on the native OS file system.
	/// </summary>
	/// <remarks>This object is a pass through to the underlying file system for the operating system.</remarks>
	internal class GorgonFolderFileSystemProvider
		: GorgonFileSystemProvider
    {
        #region Properties.
        /// <summary>
        /// Property to return a description of the file system provider.
        /// </summary>        
        public override string Description
        {
            get 
            {
                return "A provider to mount a directory as a file system.";
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
		private string MapToVirtualPath(string physicalPath, string physicalRoot, string mountPoint)
		{
			if (!Path.IsPathRooted(physicalPath))
				physicalPath = Path.GetFullPath(physicalPath);

			if (physicalPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				physicalPath = mountPoint + physicalPath.Replace(physicalRoot, string.Empty).FormatDirectory('/');
			else
			{
				physicalPath = physicalPath.Replace(physicalRoot, string.Empty);
				physicalPath = mountPoint + Path.GetDirectoryName(physicalPath).FormatDirectory('/') + Path.GetFileName(physicalPath).FormatFileName();
			}

			return physicalPath;
		}

		/// <summary>
		/// Function to enumerate the files and directories for a mount point.
		/// </summary>
		/// <param name="physicalMountPoint">Mount point being enumerated.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>
		protected override void Enumerate(string physicalMountPoint, GorgonFileSystemDirectory mountPoint)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(physicalMountPoint);			
			
			var directories = directoryInfo.GetDirectories("*", SearchOption.AllDirectories);
			var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

			foreach (DirectoryInfo directory in directories)
			{
				if (((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((directory.Attributes & FileAttributes.System) != FileAttributes.System))
				{
					string newPath = MapToVirtualPath(directory.FullName.FormatDirectory(Path.DirectorySeparatorChar), physicalMountPoint, mountPoint.FullPath);

					if (FileSystem.GetDirectory(newPath) == null)
						AddDirectoryEntry(newPath);
				}
			}

			foreach (FileInfo file in files)
			{
				if (((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((file.Attributes & FileAttributes.System) != FileAttributes.System))
				{
					string newPath = MapToVirtualPath(file.DirectoryName.FormatDirectory(Path.DirectorySeparatorChar) + file.Name, physicalMountPoint, mountPoint.FullPath);

					AddFileEntry(newPath, physicalMountPoint, file.FullName, file.Length, 0, file.CreationTime);
				}
			}
		}

		/// <summary>
		/// Function called when a file is read from the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The file data stored in a byte array or NULL if the file could not be read.</returns>
		/// <remarks>Implementors must implement this method to read the file from the physical file system.</remarks>
		protected override byte[] OnReadFile(GorgonFileSystemFileEntry file)
		{
			byte[] data = null;
            
            // Return an empty file if the file does not exist on the physical file system.
            if (!File.Exists(file.PhysicalFileSystemPath))
            {
                return new byte[0];
            }

			using (FileStream stream = File.Open(file.PhysicalFileSystemPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				data = new byte[stream.Length];

				if (stream.Length > 0)
					stream.Read(data, 0, (int)stream.Length);
			}

			return data;
		}

		/// <summary>
		/// Function called when a file is opened as a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <returns>
		/// The open <see cref="GorgonLibrary.FileSystem.GorgonFileSystemStream"/> file stream object.
		/// </returns>
		protected override GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file)
		{
			return new GorgonFileSystemStream(file, File.Open(file.PhysicalFileSystemPath, FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		/// <summary>
		/// Function to determine if a file can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the file containing the file system.</param>
		/// <returns>
		/// TRUE if the provider can read the packed file, FALSE if not.
		/// </returns>
		/// <remarks>This method is applicable to packed files only.
		/// <para>Implementors must use this method to determine if a packed file can be read by reading the header of the file.</para>
		/// </remarks>		
		public override bool CanReadFile(string physicalPath)
		{
			// Folder file systems don't use packed files, always return false.
			return false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFolderFileSystemProvider"/> class.
		/// </summary>
		/// <param name="fileSystem">File system that owns this provider.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		internal GorgonFolderFileSystemProvider(GorgonFileSystem fileSystem)
			: base(fileSystem)
		{
		}
		#endregion
	}
}
