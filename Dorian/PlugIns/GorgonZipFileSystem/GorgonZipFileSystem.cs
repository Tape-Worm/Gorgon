#region MIT.
// 
// Gorgon.
// Copyright (C) 2009 Michael Winsor
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
// Created: Wednesday, April 22, 2009 4:39:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Serialization;
using ICSharpCode.SharpZipLib.Zip;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
	/// Object representing a packed file system compressed using Zip (WinZip) compression.
    /// </summary>
	[FileSystemInfo("SharpZip.Zip File System", true, true, false, "PK\x03\x04", "Zip compressed archive files (*.zip)|*.zip")]
    public class GorgonZipFileSystem
        : FileSystem
    {
        #region Variables.
		private Stream _fileStream = null;				// File stream for packed file.
		private bool _streamIsRoot = false;				// Flag to indicate that the root of the file system is from a stream.
		private long _fileSystemOffset = 0;				// Offset within the file system.
		private ZipOutputStream _zipOut = null;			// Zip output stream.
		private bool _disposed = false;					// Flag to indicate that the object is disposed.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the header for the file system.
		/// </summary>
		protected override string FileSystemHeader
		{
			get 
			{
				return "PK\x03\x04";
			}
		}

		/// <summary>
		/// Property to return whether the root of the file system is a stream or not.
		/// </summary>
		/// <value></value>
		public override bool IsRootInStream
		{
			get 
			{ 
				return _streamIsRoot; 
			}
		}

		/// <summary>
		/// Property to return the offset of the file system within the stream.
		/// </summary>
		public override long FileSystemStreamOffset
		{
			get 
			{
				if (!IsRootInStream)
					return -1;

				return _fileSystemOffset;
			}
			set
			{
				if (!IsRootInStream)
					return;

				_fileSystemOffset = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read the file system index.
		/// </summary>
		/// <param name="fileSystemStream">Stream that contains the index.</param>
		private void ReadIndex(Stream fileSystemStream)
		{
			ZipInputStream zipStream = null;	// Zip file stream.
			string header = string.Empty;       // Header text.
			string dirPath = string.Empty;		// Directory path.
			long lastPosition = 0;				// Last stream position.
			byte[] headerBytes = new byte[4];	// Header bytes.

			// Read the header.
			if (!fileSystemStream.CanSeek)
				throw new GorgonException(GorgonErrors.CannotReadData, "Cannot read file system stream.  Cannot use non-seeking stream.");

			lastPosition = fileSystemStream.Position;
			try
			{
				fileSystemStream.Read(headerBytes, 0, 4);
				header = Encoding.UTF8.GetString(headerBytes);
				if (header != FileSystemHeader)
					throw new Exception("Invalid pack file format.");
				fileSystemStream.Position = lastPosition;

				using (zipStream = new ZipInputStream(fileSystemStream))
				{
					ZipEntry entry = null;
					zipStream.IsStreamOwner = false;

					// Get zip directory entries.
					while ((entry = zipStream.GetNextEntry()) != null)
					{
						if (entry.IsDirectory)
							CreatePath(entry.Name);
						else
						{
							dirPath = Path.GetDirectoryName(entry.Name);
							
							if (string.IsNullOrEmpty(dirPath))
								dirPath = "/";

							if (!PathExists(dirPath))
								CreatePath(dirPath);
							FileSystemPath path = GetPath(dirPath);

							path.Files.Add(entry.Name, null, (int)entry.Size, (int)entry.CompressedSize, entry.DateTime, false);
						}
					}
				}

				// Clear the current index.
				RebuildIndex(true);
			}
			finally
			{
				fileSystemStream.Position = lastPosition;

				if (zipStream != null)
					zipStream.Dispose();
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected override byte[] DecodeData(FileSystemFile file)
		{
			ZipEntry entry = null;
			ZipInputStream stream = null;
			Stream baseStream = null;
			MemoryStream outputStream = null;
			byte[] result = null;

			if (file == null)
				throw new ArgumentNullException("file");

			try
			{
				if (IsRootInStream)
				{
					_fileStream.Position = _fileSystemOffset;
					stream = new ZipInputStream(_fileStream);
					stream.IsStreamOwner = false;
				}
				else
				{
					baseStream = File.Open(Root, FileMode.Open, FileAccess.Read, FileShare.Read);
					stream = new ZipInputStream(baseStream);					
				}

				outputStream = new MemoryStream();

				// Find our file.
				while ((entry = stream.GetNextEntry()) != null)
				{
					if (entry.IsFile)
					{
						string entryName = entry.Name;

						entryName = entryName.Replace('/', '\\');
						if (!entryName.StartsWith(@"\"))
							entryName = @"\" + entryName;

						if (string.Compare(entryName, file.FullPath, true) == 0)
						{
							byte[] chunk = new byte[entry.Size];
							stream.Read(chunk, 0, (int)entry.Size);
							outputStream.Write(chunk, 0, (int)entry.Size);
							result = outputStream.ToArray();
							break;
						}
					}
				}				
			}
			finally
			{
				if (IsRootInStream)
					_fileStream.Position = _fileSystemOffset;
				if (stream != null)
					stream.Dispose();
				if (baseStream != null)
					baseStream.Dispose();
				if (outputStream != null)
					outputStream.Dispose();
			}

			return result;
		}

		/// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="path">Path to place the file into.</param>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system entry.</returns>
		protected override FileSystemFile EncodeData(FileSystemPath path, string filePath, byte[] data)
		{
			return path.Files.Add(filePath, data, data.Length, data.Length, DateTime.Now, false);
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File system entry for the object.</param>
		protected override void Load(FileSystemFile file)
		{
			// This is unnecessary for the zip file.
		}

		/// <summary>
		/// Function called when a save operation begins.
		/// </summary>
		/// <param name="filePath">Path to the file system location.</param>
		protected override void SaveInitialize(string filePath)
		{
			// Append the file system extension.
			if ((Path.GetExtension(filePath) == string.Empty) && (!IsRootInStream))
				filePath += ".zip";

			var files = Paths.GetFiles().Where((path) => path.Data == null);
			// If we don't have the file data loaded, then load it in for saving.
			foreach (var file in files)
				file.Data = DecodeData(file);

			// Open the stream for writing.
			if (!IsRootInStream)
				_fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			else
				_fileStream.Position = _fileSystemOffset;

			_zipOut = new ZipOutputStream(_fileStream);
			_zipOut.IsStreamOwner = !IsRootInStream;
			_zipOut.SetLevel(9);
		}

		/// <summary>
		/// Function called when the save function is complete.
		/// </summary>
		/// <remarks>This function is called at the end of the save function, regardless of whether the save was successful or not.</remarks>
		protected override void SaveFinalize()
		{
			if (_zipOut != null)
				_zipOut.Dispose();
			_zipOut = null;

			// We will have to remount the file system in order to see our changes.
			if (IsRootInStream)
			{
				_fileStream.Position = _fileSystemOffset;
				AssignRoot(_fileStream);
				return;
			}

			if (_fileStream != null)
				_fileStream.Dispose();
			_fileStream = null;

			AssignRoot(Root);
		}

		/// <summary>
		/// Function to save the empty directory entries into the zip file.
		/// </summary>
		/// <param name="path">The path to the empty directory.</param>
		private void SaveEmptyDirectory(FileSystemPath path)
		{
			if (path.ChildPaths.Count > 0)
			{
				foreach(FileSystemPath childPath in path.ChildPaths)
					SaveEmptyDirectory(childPath);
			}

			if (path.Files.Count == 0)
				_zipOut.PutNextEntry(new ZipEntry(path.FullPath));
		}

		/// <summary>
		/// Function to save the file index.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		protected override void SaveIndex(string filePath)
		{
			// Store only paths that have no files
			SaveEmptyDirectory(Paths);
		}

		/// <summary>
		/// Function to save the file data.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		/// <param name="file">File to save.</param>
		protected override void SaveFileData(string filePath, FileSystemFile file)
		{
			ZipEntry entry = new ZipEntry(file.FullPath);

			entry.DateTime = DateTime.Now;
			_zipOut.PutNextEntry(entry);
			_zipOut.Write(file.Data, 0, file.Data.Length);
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!_disposed)
			{
				if (disposing)
				{
					if (_zipOut != null)
						_zipOut.Dispose();
					_zipOut = null;
				}

				_disposed = true;
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
			byte[] zipID = new byte[4];

			if (provider == null)
				throw new ArgumentNullException("provider");
			if (fileSystemStream == null)
				throw new ArgumentNullException("fileSystemStream");

			long streamPosition = fileSystemStream.Position;		// Remember where we were.

			if (!fileSystemStream.CanSeek)
				throw new ArgumentException("Stream is incapable of seeking.");

			try
			{
				fileSystemStream.Read(zipID, 0, 4);

				string zipMagic = Encoding.UTF8.GetString(zipID);

				return zipMagic == provider.ID;
			}
			finally
			{
				if ((fileSystemStream != null) && (fileSystemStream.CanSeek))
					fileSystemStream.Position = streamPosition;
			}
		}
	
		/// <summary>
		/// Function to save the file system to a stream.
		/// </summary>
		/// <param name="fileSystemStream">Stream to save into.</param>
		public override void Save(Stream fileSystemStream)
		{
			if (fileSystemStream == null)
				throw new ArgumentNullException("fileSystemStream");

			_streamIsRoot = true;
			_fileSystemOffset = fileSystemStream.Position;
			_fileStream = fileSystemStream;

			base.Save(string.Empty);
		}

		/// <summary>
		/// Function to save the file system.
		/// </summary>
		/// <param name="filePath">Path to save the file system into.</param>
		public override void Save(string filePath)
		{
			_streamIsRoot = false;
			_fileStream = null;

			base.Save(filePath);
		}

		/// <summary>
		/// Function to assign the root of this file system.
		/// </summary>
		/// <param name="fileSystemStream">The file stream that will contain the file system.</param>
		/// <remarks>Due to the nature of a file stream, the file system within the stream must be a packed file system.</remarks>
		public override void AssignRoot(Stream fileSystemStream)
		{
			if (fileSystemStream == null)
				throw new ArgumentNullException("fileSystemStream");

			base.AssignRoot(fileSystemStream);
			InitializeIndex("[Stream]->" + Provider.Name + "." + Name);

			// Set up stream binding information.
			_fileStream = fileSystemStream;
			_fileSystemOffset = fileSystemStream.Position;
			_streamIsRoot = true;

			try
			{
				ReadIndex(fileSystemStream);
			}
			catch
			{
				_fileStream = null;
				_fileSystemOffset = 0;
				_streamIsRoot = false;
				throw;
			}

			// Validate the index XML.
			ValidateIndexXML();

			Mount();
		}

		/// <summary>
		/// Function to assign the root of this file system.
		/// </summary>
		/// <param name="path">Path to the root of the file system.</param>
		/// <remarks>Path can be a folder that contains the file system XML index for a folder file system or a file (typically
		/// ending with extension .zip) for a packed file system.</remarks>		
		public override void AssignRoot(string path)
		{
			FileStream stream = null;					// File stream.

			if (path == null)
				path = string.Empty;

			// Append default extension.
			if (Path.GetExtension(path) == string.Empty)
				path = Path.ChangeExtension(path, ".zip");

			InitializeIndex(path);

			// Check for the archive file.
			if (!File.Exists(Root))
				throw new FileNotFoundException("The file system root '" + Root + "' does not exist.");

			try
			{
				// Unbind from any stream.
				_fileSystemOffset = 0;
				_fileStream = null;
				_streamIsRoot = false;

				// Open the archive file.
				stream = File.OpenRead(Root);
				ReadIndex(stream);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}

			// Validate the index XML.
			ValidateIndexXML();

			Mount();
		}
        #endregion

        #region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonZipFileSystem"/> class.
		/// </summary>
		/// <param name="name">Name of this file system.</param>
		/// <param name="provider">File system provider.</param>
		internal GorgonZipFileSystem(string name, FileSystemProvider provider)
            : base(name, provider)
        {
        }
        #endregion
    }
}
