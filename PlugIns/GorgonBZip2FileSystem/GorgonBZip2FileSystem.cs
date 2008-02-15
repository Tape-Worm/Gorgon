#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Thursday, November 16, 2006 11:21:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using SharpUtilities.IO;
using SharpUtilities.Collections;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Serialization;
using ICSharpCode.SharpZipLib.BZip2;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
	/// Object representing a packed file system compressed with SharpZip.854's BZip2 compression.
    /// </summary>
	[FileSystemInfo("SharpZip.BZip2 File System", true, true, "GORPACK1.SharpZip.BZ2")]
    public class GorgonBZip2FileSystem
        : FileSystem
    {
        #region Variables.
        private MemoryStream _dataStream = null;        // Data stream.   
		private long _fileOffset = 0;					// Offset within the archive of the file data.
		private Stream _fileStream = null;				// File stream for packed file.
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
		/// Property to set or return the root path of the file system.
		/// </summary>
		public override string Root
		{
			get
			{
				return base.Root;
			}
			set
			{
				FileStream stream = null;					// File stream.
				BinaryReaderEx reader = null;               // Binary reader.			
				string xmlData = string.Empty;				// String to contain the XML data.
				string header = string.Empty;               // Header text.
				int indexSize = 0;							// Size of the directory index.

				if (value == null)
					value = string.Empty;

				// Append default extension.
				if (Path.GetExtension(value) == string.Empty)
					value += ".gorPack";

				base.Root = value;
				
				// Check for the archive file.
				if (!File.Exists(Root))
					throw new FileSystemRootIsInvalidException(Root);

				try
				{
					// Open the archive file.
					stream = File.OpenRead(Root);
					reader = new BinaryReaderEx(stream, true);

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
					_fileOffset = stream.Position;
				}
				catch (Exception ex)
				{
					throw new FileSystemRootIsInvalidException(value, ex);
				}
				finally
				{
					// Clean up.
					if (reader != null)
						reader.Close();
					reader = null;
					if (stream != null)
						stream.Dispose();
					stream = null;
				}

				// Validate the index XML.
				ValidateIndexXML();
			}
		}
		#endregion

		#region Methods.
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
			catch
			{
				throw;
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
			catch
			{
				throw;
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
			try
			{
				// If not compressed, then leave.
				if (!file.IsCompressed)
					return file.Data;
				
				return DecompressData(file.Data);
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + file.FullPath + " from the file system.", ex);
			}
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

			try
			{
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
			catch (Exception ex)
			{
				throw new CannotCreateException("Cannot encode " + filePath + " to the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="file">File system entry for the object.</param>
		protected override void Load(FileSystemFile file)
		{
			BinaryReaderEx reader = null;				// Binary data reader.
			Stream stream = null;						// File stream.
			int fileSize = 0;							// File size.

			try
			{
				if (file.IsCompressed)
					fileSize = file.CompressedSize;
				else
					fileSize = file.Size;

				// Open the archive for reading.
				stream = File.Open(Root, FileMode.Open, FileAccess.Read, FileShare.Read);
				reader = new BinaryReaderEx(stream, false);

				// Move to the data.
				reader.BaseStream.Position = _fileOffset + file.Offset;
				file.Data = reader.ReadBytes(fileSize);

				// Close the file to get around concurrency issues.
				if (reader != null)
					reader.Close();
				reader = null;
				if (stream != null)
					stream.Dispose();
				stream = null;

				if ((file.Data == null) || (file.Data.Length != fileSize))
					throw new Exception("Failure reading file system file.");

				// Fire the event.
				OnDataLoad(this, new FileSystemDataIOEventArgs(file));
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + file.FullPath + " from the file system.", ex);
			}
			finally
			{
				if (reader != null)
					reader.Close();
				reader = null;
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function called when a save operation begins.
		/// </summary>
		/// <param name="filePath">Path to the file system location.</param>
		protected override void SaveInitialize(string filePath)
		{
			if ((filePath.Length == 0) || (filePath == null))
				throw new Exception("Path is empty.");

			if (Path.GetFileName(filePath) == string.Empty)
				throw new Exception("Invalid file name.");

			// Append the file system extension.
			if (Path.GetExtension(filePath) == string.Empty)
				filePath += ".gorPack";

			// Open the stream for writing.
			_fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
		}

		/// <summary>
		/// Function called when the save function is complete.
		/// </summary>
		/// <remarks>This function is called at the end of the save function, regardless of whether the save was successful or not.</remarks>
		protected override void SaveFinalize()
		{
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
			catch
			{
				throw;
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
