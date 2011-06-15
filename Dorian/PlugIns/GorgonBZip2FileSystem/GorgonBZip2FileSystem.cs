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
// Created: Thursday, November 16, 2006 11:21:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Serialization;
using ICSharpCode.SharpZipLib.BZip2;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
	/// Object representing a packed file system compressed with SharpZip.854's BZip2 compression.
    /// </summary>
	[FileSystemInfo("SharpZip.BZip2 File System", true, true, false, "GORPACK1.SharpZip.BZ2", "Gorgon packed file systems (*.gorPack)|*.gorPack")]
    public class GorgonBZip2FileSystem
        : FileSystem
    {
        #region Variables.
        private MemoryStream _dataStream = null;        // Data stream.   
		private long _fileOffset = 0;					// Offset within the archive of the file data.
		private Stream _fileStream = null;				// File stream for packed file.
		private bool _streamIsRoot = false;				// Flag to indicate that the root of the file system is from a stream.
		private long _fileSystemOffset = 0;				// Offset within the file system.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the header for the file system.
		/// </summary>
		protected override string FileSystemHeader
		{
			get 
			{
				return "GORPACK1.SharpZip.BZ2";
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
			BinaryReaderEx reader = null;		// Binary reader.
			string xmlData = string.Empty;		// String to contain the XML data.
			string header = string.Empty;       // Header text.
			int indexSize = 0;					// Size of the directory index.

			try
			{
				reader = new BinaryReaderEx(fileSystemStream, true);

				// Read the header.
				header = reader.ReadString();
				if (string.Compare(header, FileSystemHeader, true) != 0)
					throw new Exception("Invalid pack file format.");

				// Get the index size (compressed).
				indexSize = reader.ReadInt32();

				// Get the XML.
				xmlData = Encoding.UTF8.GetString(DecompressData(reader.ReadBytes(indexSize)));

				// Load into the XML document object.
				FileIndexXML.LoadXml(xmlData);

				// Get the file offset.				
				_fileOffset = fileSystemStream.Position - _fileSystemOffset;
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		/// <summary>
		/// Function to compress a block of data.
		/// </summary>
		/// <param name="data">Data block to compress.</param>
		/// <returns>A block of compressed data.</returns>
		private byte[] CompressData(byte[] data)
		{
			MemoryStream compressedStream = null;	// Compressed stream.
			
			try
			{
				// Prepare compression streams.
				_dataStream = new MemoryStream(data);
				_dataStream.Position = 0;
				compressedStream = new MemoryStream();

				// Compress using best compression.
				BZip2.Compress(_dataStream, compressedStream, BZip2Constants.baseBlockSize * 9);
				return compressedStream.ToArray();
			}
			finally
			{
				if (_dataStream != null)
					_dataStream.Dispose();
				if (compressedStream != null)
					compressedStream.Dispose();

				_dataStream = null;
				compressedStream = null;
			}
		}

		/// <summary>
		/// Function to decompress a block of data.
		/// </summary>
		/// <param name="data">Data block to compress.</param>
		/// <returns>Block of data that was compressed.</returns>
		private byte[] DecompressData(byte[] data)
		{
			MemoryStream uncompressedStream = null;	// Uncompressed stream.

			try
			{
				// Get the compressed data.
				_dataStream = new MemoryStream(data);
				_dataStream.Position = 0;
				uncompressedStream = new MemoryStream();

				// Decompress.				
				BZip2.Decompress(_dataStream, uncompressedStream);				

				// Get the data.
				return uncompressedStream.ToArray();
			}
			finally
			{
				if (uncompressedStream != null)
					uncompressedStream.Dispose();
				if (_dataStream != null)
					_dataStream.Dispose();

				uncompressedStream = null;
				_dataStream = null;
			}
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

			// If not compressed, then leave.
			if (!file.IsCompressed)
				return file.Data;
			
			return DecompressData(file.Data);
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
			byte[] compressedData = null;			// Compressed data.
			int compressedSize = 0;					// Compressed size in bytes.
			FileSystemFile file = null;				// File.
			double ratio = 0.0f;					// Compression ratio.

			// Get compressed data.
			compressedData = CompressData(data);

			// Get compression ratio.
			ratio = ((double)(data.Length - compressedData.Length) / (double)data.Length) * 100.0;

			// If less than 64 bytes of compression, then don't bother.
			if ((data.Length - compressedData.Length) <= 64)
				compressedData = data;
			else
				compressedSize = compressedData.Length;

			file = path.Files.Add(filePath, compressedData, data.Length, compressedSize, DateTime.Now, false);

			// Add the entry.
			return file;
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File system entry for the object.</param>
		protected override void Load(FileSystemFile file)
		{
			BinaryReaderEx reader = null;				// Binary data reader.
			int fileSize = 0;							// File size.

			if (file == null)
				throw new ArgumentNullException("file");

			try
			{
				if (file.IsCompressed)
					fileSize = file.CompressedSize;
				else
					fileSize = file.Size;

				// Open the archive for reading.
				if (IsRootInStream)
					reader = new BinaryReaderEx(_fileStream, true);
				else
					reader = new BinaryReaderEx(File.Open(Root, FileMode.Open, FileAccess.Read, FileShare.Read), false);

				// Move to the data.
				if (!IsRootInStream)
					reader.BaseStream.Position = _fileOffset + file.Offset;
				else
					reader.BaseStream.Position = _fileSystemOffset + _fileOffset + file.Offset;
				file.Data = reader.ReadBytes(fileSize);

				// Close the file to get around concurrency issues.
				if (reader != null)
					reader.Close();
				reader = null;

				if ((file.Data == null) || (file.Data.Length != fileSize))
					throw new GorgonException(GorgonErrors.CannotReadData, "Cannot read file system file '" + file.FullPath + "'");

				// Fire the event.
				OnDataLoad(this, new FileSystemDataIOEventArgs(file));
			}
			finally
			{
				if (reader != null)
					reader.Close();
				reader = null;
			}
		}

		/// <summary>
		/// Function called when a save operation begins.
		/// </summary>
		/// <param name="filePath">Path to the file system location.</param>
		protected override void SaveInitialize(string filePath)
		{
			// Append the file system extension.
			if ((Path.GetExtension(filePath) == string.Empty) && (!IsRootInStream))
				filePath += ".gorPack";

			// Open the stream for writing.
			if (!IsRootInStream)
				_fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			else
				_fileStream.Position = _fileSystemOffset;
		}

		/// <summary>
		/// Function called when the save function is complete.
		/// </summary>
		/// <remarks>This function is called at the end of the save function, regardless of whether the save was successful or not.</remarks>
		protected override void SaveFinalize()
		{
			if (IsRootInStream)
				return;

			if (_fileStream != null)
				_fileStream.Dispose();
			_fileStream = null;
		}

		/// <summary>
		/// Function to save the file index.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		protected override void SaveIndex(string filePath)
		{
			byte[] compressed = null;		// Compressed index data.
			BinaryWriterEx writer = null;	// Binary writer.
			
			try
			{
				// Save the index file.
				compressed = CompressData(Encoding.UTF8.GetBytes(FileIndexXML.OuterXml));				

				writer = new BinaryWriterEx(_fileStream, true);
				writer.Write(FileSystemHeader);
				writer.Write(compressed.Length);
				writer.Write(compressed);
			}
			finally			
			{
				if (writer != null)
					writer.Close();
				writer = null;
			}
		}

		/// <summary>
		/// Function to save the file data.
		/// </summary>
		/// <param name="filePath">Root of the file system on the disk.</param>
		/// <param name="file">File to save.</param>
		protected override void SaveFileData(string filePath, FileSystemFile file)
		{
			_fileStream.Write(file.Data, 0, file.Data.Length);
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
		/// ending with extension .gorPack) for a packed file system.</remarks>		
		public override void AssignRoot(string path)
		{
			FileStream stream = null;					// File stream.

			if (path == null)
				path = string.Empty;

			// Append default extension.
			if (Path.GetExtension(path) == string.Empty)
				path = Path.ChangeExtension(path, ".gorPack");

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
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this file system.</param>
        /// <param name="provider">File system provider.</param>
		internal GorgonBZip2FileSystem(string name, FileSystemProvider provider)
            : base(name, provider)
        {
        }
        #endregion
    }
}
