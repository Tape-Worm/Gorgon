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
// Created: Thursday, October 1, 2015 11:39:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// File information for ram disk files.
	/// </summary>
	internal struct RamDiskFileInfo
		: IEquatable<RamDiskFileInfo>
	{        
        /// <summary>
        /// Full path to the file.
        /// </summary>
        public readonly string FullPath;
		/// <summary>
		/// Size of the file.
		/// </summary>
		public readonly long Size;
		/// <summary>
		/// Creation date.
		/// </summary>
		public readonly DateTime CreateDate;
		/// <summary>
		/// Last modified date.
		/// </summary>
		public readonly DateTime LastModified;

		/// <summary>
		/// Initializes a new instance of the <see cref="RamDiskFileInfo"/> struct.
		/// </summary>
		/// <param name="fullPath">The full path.</param>
		/// <param name="size">The size.</param>
		/// <param name="created">The created.</param>
		/// <param name="lastMod">The last mod.</param>
		public RamDiskFileInfo(string fullPath, long size, DateTime created, DateTime lastMod)
		{
			FullPath = fullPath;
			Size = size;
			CreateDate = created;
			LastModified = lastMod;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RamDiskFileInfo"/> struct.
		/// </summary>
		/// <param name="file">The file.</param>
		public RamDiskFileInfo(IGorgonVirtualFile file)
			: this(file.FullPath, file.Size, file.CreateDate, file.LastModifiedDate)
		{
		}

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(RamDiskFileInfo other) => string.Equals(other.FullPath, FullPath, StringComparison.OrdinalIgnoreCase);
    }

	/// <summary>
	/// A file system object for ram disks.
	/// </summary>
	internal class RamDiskFileSystem
	{
		#region Variables.
		// The list of files and their data for the file system.
		private readonly Dictionary<RamDiskFileInfo, MemoryStream> _fileData;

		// The list of file information values.
		private readonly Dictionary<string, RamDiskFileInfo> _fileInfos;

		// The list of directory paths for the file system.
		private readonly HashSet<string> _directories;
        #endregion

        #region Properties.
        /// <summary>
        /// Function to retrieve the list of directories for the file system.
        /// </summary>
        /// <returns>A read only list of directories.</returns>
        public IReadOnlyList<string> GetDirectories() => _directories.OrderBy(item => item).ThenBy(item => item.Length).ToArray();

        /// <summary>
        /// Function to retrieve a list of file information for the ram disk.
        /// </summary>
        /// <returns>A read only list of file information.</returns>
        public IEnumerable<RamDiskFileInfo> GetFileInfos() => _fileInfos
                .OrderBy(item => item.Value.FullPath)
                .ThenBy(item => item.Value.FullPath.Length)
                .Select(item => item.Value)
                .ToArray();

        /// <summary>
        /// Function to retrieve file information for a specific file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>The file information.</returns>
        public RamDiskFileInfo GetFileInfo(string path)
		{

			if (!_fileInfos.TryGetValue(path, out RamDiskFileInfo fileInfo))
			{
				throw new FileNotFoundException(Resources.GORFS_ERR_FILE_NOT_FOUND, path);
			}

			return fileInfo;
		}

		/// <summary>
		/// Function to open a read only stream to a file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The stream to the file.</returns>
		public Stream OpenReadStream(string path)
		{
			RamDiskFileInfo fileInfo = GetFileInfo(path);


			if (!_fileData.TryGetValue(fileInfo, out MemoryStream stream))
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
			}

			return new GorgonStreamWrapper(stream, 0, stream.Length, false);
		}

		/// <summary>
		/// Function to add a directory to the file system.
		/// </summary>
		/// <param name="path">Path to the directory.</param>
		public void AddDirectoryPath(string path)
		{
			if (_directories.Contains(path))
			{
				return;
			}

			_directories.Add(path);
		}

		/// <summary>
		/// Function to remove a directory from the file system.
		/// </summary>
		/// <param name="path">Path to the directory.</param>
		public void RemoveDirectoryPath(string path)
		{
			if (!_directories.Any(item => item.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, path));
			}

			string[] directories = _directories.Where(item => item.StartsWith(path, StringComparison.OrdinalIgnoreCase)).ToArray();
			RamDiskFileInfo[] files = _fileInfos
				.Where(item => item.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase))
				.Select(item => item.Value)
				.ToArray();

			foreach (RamDiskFileInfo fileInfo in files)
			{
				_fileInfos.Remove(fileInfo.FullPath);


				if (!_fileData.TryGetValue(fileInfo, out MemoryStream stream))
				{
					continue;
				}

				stream.Dispose();
				_fileData.Remove(fileInfo);
			}

			foreach (string directory in directories)
			{
				_directories.Remove(directory);
			}
		}

		/// <summary>
		/// Function to clear the file system of all directories and files.
		/// </summary>
		public void Clear()
		{
			foreach (KeyValuePair<RamDiskFileInfo, MemoryStream> file in _fileData)
			{
				file.Value?.Dispose();
			}

			_fileData.Clear();
			_fileInfos.Clear();
			_directories.Clear();
		}

		/// <summary>
		/// Function to open a writable stream to the file.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>A stream to the file.</returns>
		public MemoryStream OpenForWrite(string path)
		{
			MemoryStream stream;

			if (_fileInfos.ContainsKey(path))
			{
				RamDiskFileInfo fileInfo = GetFileInfo(path);

				if (!_fileData.TryGetValue(fileInfo, out stream))
				{
					throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
				}

				return stream;
			}

			// This is a new file, so open it as such.
			var info = new RamDiskFileInfo(path, 0, DateTime.Now, DateTime.Now);
			stream = new MemoryStream();
			_fileInfos.Add(path, info);
			_fileData.Add(info, stream);

			return stream;
		}

		/// <summary>
		/// Function to delete a file from the file system.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		public void DeleteFile(string path)
		{
			if (!_fileInfos.ContainsKey(path))
			{
				return;
			}


			if (!_fileInfos.TryGetValue(path, out RamDiskFileInfo fileInfo))
			{
				return;
			}

			_fileInfos.Remove(path);


			if (!_fileData.TryGetValue(fileInfo, out MemoryStream _))
			{
				return;
			}

			_fileData[fileInfo]?.Dispose();
			_fileData.Remove(fileInfo);
		}

		/// <summary>
		/// Function to update the information for a file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <param name="fileInfo">File information to replace.</param>
		public void UpdateFile(string path, ref RamDiskFileInfo fileInfo)
		{

			if (_fileInfos.TryGetValue(path, out RamDiskFileInfo old))
			{
				_fileInfos.Remove(old.FullPath);
				_fileInfos[path] = fileInfo;
			}
			else
			{
				old = fileInfo;
			}

			if (!_fileData.TryGetValue(old, out MemoryStream oldStream))
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
			}

			_fileData.Remove(old);
			_fileData[fileInfo] = oldStream;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RamDiskFileSystem"/> class.
		/// </summary>
		public RamDiskFileSystem()
		{
			_fileData = new Dictionary<RamDiskFileInfo, MemoryStream>();		
			_fileInfos = new Dictionary<string, RamDiskFileInfo>();	
			_directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion
	}
}
