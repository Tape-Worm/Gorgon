#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, November 04, 2006 11:36:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing a standard disk based filesystem.
    /// </summary>
	[FileSystemInfo("Folder File System", false, false, false, "GORPACK1.GorgonFolderSystem", "Gorgon folder file system.|header.folderSystem")]
    public class FolderFileSystem
        : FileSystem
	{
		#region Variables.
		private List<FileSystemPath> _deletedPaths = null;				// List of deleted paths.
		private List<FileSystemFile> _deletedFiles = null;				// List of deleted files.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the header for the file system.
		/// </summary>
		/// <value></value>
		protected override string FileSystemHeader
		{
			get 
			{
				// We don't need one for this.
				return "GORPACK1.GorgonFolderSystem";
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used by custom file systems to perform custom actions when a path is deleted.
		/// </summary>
		/// <param name="filePath">Path to be deleted.</param>
		protected override void OnPathDelete(FileSystemPath filePath)
		{			
			base.OnPathDelete(filePath);

			if (!_deletedPaths.Contains(filePath))
			{
				foreach (FileSystemPath path in filePath.ChildPaths)
					OnPathDelete(path);

				foreach (FileSystemFile file in filePath.Files)
					OnFileDelete(file);
								
				if (filePath.FullPath != @"\")
					_deletedPaths.Add(filePath);
			}
		}

		/// <summary>
		/// Funciton used by custom file systems to perform custom actions when a file is deleted.
		/// </summary>
		/// <param name="file">File to delete.</param>
		protected override void OnFileDelete(FileSystemFile file)
		{
			base.OnFileDelete(file);

			if (!_deletedFiles.Contains(file))
				_deletedFiles.Add(file);
		}

		/// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="path">Path to add the file into.</param>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system file.</returns>
		protected override FileSystemFile EncodeData(FileSystemPath path, string filePath, byte[] data)
		{
			FileSystemFile file = null;			// New file system file.

			// Add the file.
			file = path.Files.Add(filePath, data, data.Length, 0, DateTime.Now, false);

			// This is very straight forward, the basic folder file system doesn't
			// need to do any special encoding.
			return file;
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected override byte[] DecodeData(FileSystemFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			// This file system requires no conversion of data, so pass through.
			return file.Data;
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to load.</param>
		protected override void Load(FileSystemFile file)
		{
			Stream stream = null;					// File stream.

			if (file == null)
				throw new ArgumentNullException("file");

			if (!File.Exists(Root + file.FullPath))
				throw new GorgonException(GorgonErrors.CannotLoad, "File '" + Root + file.FullPath + "' was not found in the file system.");

            try
			{
				// Open the file.
				stream = File.Open(Root + file.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				file.Data = new byte[stream.Length];
				stream.Read(file.Data, 0, file.Data.Length);
			}
			finally
			{
				// Close the file.
				if (stream != null)
					stream.Dispose();
				stream = null;
			}

			// Fire the event.
			OnDataLoad(this, new FileSystemDataIOEventArgs(file));
		}

		/// <summary>
		/// Function to save the file index.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		protected override void SaveIndex(string filePath)
		{
			BinaryWriter writer = null;		// Writer for header.
			Stream outStream = null;		// File stream for header.

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");

			// Append the path separator if necessary.
			if (filePath.EndsWith(@"\"))
				filePath = Path.GetDirectoryName(filePath) ?? string.Empty;

			// Write out the header ID.
			try
			{
				outStream = File.Open(filePath + @"\Header.folderSystem", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				writer = new BinaryWriter(outStream, Encoding.UTF8);
				writer.Write(FileSystemHeader);
			}
			finally
			{
				if (outStream != null)
					outStream.Dispose();
				if (writer != null)
					writer.Close();

				outStream = null;
				writer = null;
			}

			FileIndexXML.Save(filePath + @"\FileSystem.Index.xml");
		}

		/// <summary>
		/// Function called when the save function is complete.
		/// </summary>
		/// <remarks>This function is called at the end of the save function, regardless of whether the save was successful or not.</remarks>
		protected override void SaveFinalize()
		{
			base.SaveFinalize();
			string rootPath;

			if (!PurgeDeletedFiles)
			{
				_deletedPaths.Clear();
				_deletedFiles.Clear();
				return;
			}

			rootPath = Path.GetDirectoryName(Root) ?? string.Empty;

			if (string.IsNullOrEmpty(rootPath))
				throw new DirectoryNotFoundException("The root path is empty.");

			if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				rootPath += Path.DirectorySeparatorChar.ToString();

			if (!Directory.Exists(rootPath))
				throw new DirectoryNotFoundException("The root path '" + rootPath + "' was not found.");

			try
			{
				// Destroy all deleted files.
				foreach (FileSystemFile file in _deletedFiles)
				{
					string fileName = Path.GetFullPath(rootPath + file.FullPath);

					if ((File.Exists(fileName)) && (!FileExists(file.FullPath)))
						File.Delete(fileName);
				}

				foreach (FileSystemPath path in _deletedPaths)
				{
					string pathName = Path.GetFullPath(rootPath + path.FullPath);

					if ((Directory.Exists(pathName)) && (!PathExists(path.FullPath)))
						Directory.Delete(pathName, true);
				}
			}
			finally
			{
				_deletedFiles.Clear();
				_deletedPaths.Clear();
			}
		}

		/// <summary>
		/// Function to save the file data.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		/// <param name="file">File to save.</param>
		protected override void SaveFileData(string filePath, FileSystemFile file)
		{
			Stream fileStream = null;		// File stream to the file.
			string path = string.Empty;		// File path.

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");
			if (file == null)
				throw new ArgumentNullException("file");

			try
			{
				if (filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					filePath = Path.GetDirectoryName(filePath) ?? string.Empty;

				// Get the path to the file.
				path = filePath + Path.GetDirectoryName(file.FullPath);

				// Create the directory if needed.
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				fileStream = File.Open(filePath + file.FullPath, FileMode.Create, FileAccess.Write, FileShare.None);

				// Write.
				fileStream.Write(file.Data, 0, file.Data.Length);
			}
			finally
			{
				// Clean up.
				if (fileStream != null)
					fileStream.Dispose();
				fileStream = null;
			}
		}

		/// <summary>
		/// Function return whether a file system is valid for a given file system provider.
		/// </summary>
		/// <param name="provider">Provider to test.</param>
		/// <param name="fileSystemStream">Stream containing the file system root.</param>
		/// <returns>
		/// TRUE if the provider can support this filesystem, FALSE if not.
		/// </returns>
		public override bool IsValidForProvider(FileSystemProvider provider, Stream fileSystemStream)
		{
			BinaryReaderEx reader = null;

			if (provider == null)
				throw new ArgumentNullException("provider");
			if (fileSystemStream == null)
				throw new ArgumentNullException("fileSystemStream");

			long streamPosition = fileSystemStream.Position;		// Remember where we were.

			if (!fileSystemStream.CanSeek)
				throw new ArgumentException("Stream is incapable of seeking.");

			try
			{
				reader = new BinaryReaderEx(fileSystemStream, true);
				string fileID = reader.ReadString();

				return fileID == provider.ID;
			}
			finally
			{
				if ((fileSystemStream != null) && (fileSystemStream.CanSeek))
					fileSystemStream.Position = streamPosition;

				if (reader != null)
					reader.Close();
			}
		}

		/// <summary>
		/// Function to assign the root of this file system.
		/// </summary>
		/// <param name="path">Path to the root of the file system.</param>
		/// <remarks>Path can be a folder that contains the file system XML index for a folder file system or a file (typically
		/// ending with extension .gorPack) for a packed file system.</remarks>
		/// <exception cref="System.IO.DirectoryNotFoundException">Path to the file system was not found.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">The file system could not load its index file.</exception>
		public override void AssignRoot(string path)
		{
			if (path == null)
				path = string.Empty;

			// Append the directory seperator character.
			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				path += Path.DirectorySeparatorChar;

			path = Path.GetFullPath(path);

			if (path.EndsWith(Path.VolumeSeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()))
				throw new ArgumentException("Cannot mount the root of the file system.");

			if (!Directory.Exists(path))
				throw new DirectoryNotFoundException("The folder file system path '" + path + "' was not found.");

			_deletedPaths.Clear();
			_deletedFiles.Clear();
			InitializeIndex(path);

			if (!File.Exists(Root + "Header.folderSystem"))
				throw new GorgonException(GorgonErrors.CannotLoad, "The file system header data was not found at the root location: '" + Root + "'.");

			// Load the index file.
			if (!File.Exists(Root + "FileSystem.Index.xml"))
				throw new GorgonException(GorgonErrors.CannotLoad, "The file system index was not found at the root location: '" + Root + "'.");

			FileIndexXML.Load(Root + "FileSystem.Index.xml");

			// Validate the index XML.
			ValidateIndexXML();

			Mount();
		}
		#endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this file system.</param>
		/// <param name="provider">File system provider.</param>
        public FolderFileSystem(string name, FileSystemProvider provider)
            : base(name, provider)
        {
			_deletedFiles = new List<FileSystemFile>();
			_deletedPaths = new List<FileSystemPath>();
        }
        #endregion
    }
}
