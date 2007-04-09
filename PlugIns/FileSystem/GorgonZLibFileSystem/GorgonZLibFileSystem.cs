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
using DotZLib;
using SharpUtilities.IO;
using SharpUtilities.Collections;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing a packed file system compressed with ZLib 1.2.3.
    /// </summary>
	[FileSystemEditorInfo("ZLib File System", true, true)]
    public class GorgonZLibFileSystem
        : FileSystem
    {
        #region Variables.        
        private MemoryStream _dataStream = null;        // Data stream.   
		private long _fileOffset = 0;					// Offset within the archive of the file data.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the root path of the file system.
		/// </summary>
		public override string RootPath
		{
			get
			{
				return base.RootPath;
			}
			set
			{
				FileStream stream = null;					// File stream.
				BinaryReaderEx reader = null;               // Binary reader.			
				string xmlData = string.Empty;				// String to contain the XML data.
				string header = string.Empty;               // Header text.
				int indexSize = 0;							// Size of the directory index.

				base.RootPath = value;

				// Append default extension.
				if (Path.GetExtension(_root) == string.Empty)
					_root += ".gorPack";

				// Check for the archive file.
				if (!File.Exists(_root))
					throw new InvalidRootException(_root, null);

				try
				{
					// Open the archive file.
					stream = File.OpenRead(_root);
					reader = new BinaryReaderEx(stream, true);

					// Read the header.
					header = reader.ReadString();
					if (header.ToLower() != "gorpack1.zlib")
						throw new Exception("Invalid pack file format.");

					// Get the index size (compressed).
					indexSize = reader.ReadInt32();

					// Get the XML.
					xmlData = Encoding.UTF8.GetString(DecompressData(reader.ReadBytes(indexSize)));

					// Load into the XML document object.
					_fileIndex.LoadXml(xmlData);

					// Get the file offset.				
					_fileOffset = stream.Position;
				}
				catch (Exception ex)
				{
					throw new InvalidRootException(_root, ex);
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
		/// Represents the method that will be called from a codec when new data
		/// are available.
		/// </summary>
		/// <paramref name="data">The byte array containing the processed data</paramref>
		/// <paramref name="startIndex">The index of the first processed byte in <c>data</c></paramref>
		/// <paramref name="count">The number of processed bytes available</paramref>
		/// <remarks>On return from this method, the data may be overwritten, so grab it while you can. 
		/// You cannot assume that startIndex will be zero.
		/// </remarks>
		private void DataCodec(byte[] data, int startIndex, int count)
		{
			// Send to the file steam.
			_dataStream.Write(data, startIndex, count);
		}

		/// <summary>
		/// Function to decompress a block of data.
		/// </summary>
		/// <param name="compressedData">Byte array containing compressed data.</param>
		/// <returns>The uncompressed data.</returns>
		private byte[] DecompressData(byte[] compressedData)
		{
			Inflater decompressor = null;			// Decompressor.
		
			try
			{
				// Get the decompressor.
				_dataStream = new MemoryStream();
				decompressor = new Inflater();
				decompressor.DataAvailable += new DataAvailableHandler(DataCodec);				
				decompressor.Add(compressedData);
				decompressor.Finish();
				_dataStream.Position = 0;

				return _dataStream.ToArray();
			}
			catch
			{
				throw;
			}
			finally
			{
				// Clean up.
				if (decompressor != null)
					decompressor.Dispose();
				decompressor = null;
				if (_dataStream != null)
					_dataStream.Dispose();
				_dataStream = null;
			}
		}

		/// <summary>
		/// Function to compress a block of data.
		/// </summary>
		/// <param name="uncompressedData">Byte array containing uncompressed data.</param>
		/// <returns>The compressed data.</returns>
		private byte[] CompressData(byte[] uncompressedData)
		{
			Deflater compressor = null;			// Compressor.

			try
			{
				compressor = new Deflater(CompressLevel.Best);
				compressor.DataAvailable += new DataAvailableHandler(DataCodec);
				_dataStream = new MemoryStream();
				compressor.Add(uncompressedData);
				compressor.Finish();
				_dataStream.Position = 0;

				return _dataStream.ToArray();
			}
			catch
			{
				throw;
			}
			finally
			{
				// Clean up.
				if (compressor != null)
					compressor.Dispose();
				compressor = null;
				if (_dataStream != null)
					_dataStream.Dispose();
				_dataStream = null;
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="entry">File system entry for the object.</param>
		/// <returns>The raw binary data for the file.</returns>
		protected override byte[] DecodeData(FileSystemEntry entry)
		{
			try
			{
				// Decompress the data.
				if (entry.IsCompressed)
					return DecompressData(entry.ObjectData);
				else
					return entry.ObjectData;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + entry.FullPath + " from the file system.", ex);
			}
		}

		/// <summary>
		/// Function to encode object data.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <param name="data">Data to encode.</param>
		/// <returns>A new file system entry.</returns>
		protected override FileSystemEntry EncodeData(string filePath, byte[] data)
		{
			byte[] compressedData = null;		// Compressed data.
			long offset = 0;					// Offset.
			int compressedSize = 0;				// Compressed size in bytes.

			try
			{
				// Calculate offset.
				foreach (FileSystemEntry entry in _entries)
				{
					if (entry.IsCompressed)
						offset += entry.CompressedFileSize;
					else
						offset += entry.FileSize;
				}

				compressedData = CompressData(data);
				// If we get MORE data than what we had originally, don't save it.
				if (compressedData.Length >= data.Length)
					compressedData = data;
				else
					compressedSize = compressedData.Length;

				// Add the entry.
				return _entries.AddEntry(filePath, compressedData, offset, data.Length, compressedSize, false);
			}
			catch (Exception ex)
			{
				throw new CannotCreateException("Cannot encode " + filePath + " to the file system.", ex);
			}
		}

		/// <summary>
		/// Function to load an object from the file system.
		/// </summary>
		/// <param name="entry">File system entry for the object.</param>
		protected override void Load(FileSystemEntry entry)
		{
			BinaryReaderEx reader = null;				// Binary data reader.
			Stream stream = null;						// File stream.
			int fileSize = 0;							// File size.

			try
			{
				if (entry.IsCompressed)
					fileSize = entry.CompressedFileSize;
				else
					fileSize = entry.FileSize;

				// Open the archive for reading.
				stream = File.Open(RootPath, FileMode.Open);
				reader = new BinaryReaderEx(stream, false);

				// Move to the data.
				reader.BaseStream.Position = _fileOffset + entry.Offset;
				entry.ObjectData = reader.ReadBytes(fileSize);

				// Close the file to get around concurrency issues.
				if (reader != null)
					reader.Close();
				reader = null;
				if (stream != null)
					stream.Dispose();
				stream = null;

				if ((entry.ObjectData == null) || (entry.ObjectData.Length != fileSize))
					throw new Exception("Failure reading file system file.");
			}
			catch (Exception ex)
			{
				throw new CannotLoadException("Cannot load " + entry.FullPath + " from the file system.", ex);
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
		/// Function to write the file system contents to a specified path.
		/// </summary>
		/// <param name="path">Path to write the file system contents into.</param>
		public override void Save(string path)
		{
			XmlElement fsElement = null;				// File system.
			XmlElement entry = null;					// File system entry.
			XmlElement entryProperty = null;			// Entry property.
			XmlAttribute entryPath = null;				// Full relative path.
			Stream fileStream = null;					// File stream.
			BinaryWriterEx writer = null;				// Binary writer.
			List<byte[]> fileData = null;				// List of file data.

			try
			{				

				if ((path.Length == 0) || (path == null))
					throw new Exception("Path is empty.");

				if (Path.GetFileName(path) == string.Empty)
					throw new Exception("Invalid file name.");

				// Append the file system extension.
				if (Path.GetExtension(path) == string.Empty)
					path += ".gorPack";

				// If we have no entries, remove the file system.
				if (EntryCount == 0)
				{
					if (File.Exists(path))
						File.Delete(path);

					return;
				}

				// Reset the XML directory.
				RebuildIndex();

				// Get the file system node.
				fsElement = (XmlElement)_fileIndex.SelectSingleNode("//FileSystem");

				if (fsElement == null)
					throw new Exception("File system is corrupt.");

				fileData = new List<byte[]>();

				// Add entries.
				foreach (FileSystemEntry fileEntry in this)
				{
					if (fileEntry.ObjectData != null)
					{
						// Create full path.
						entryPath = _fileIndex.CreateAttribute("RelativePath");
						entryPath.Value = fileEntry.Path + fileEntry.Filename + fileEntry.Extension;

						entry = _fileIndex.CreateElement("Entry");
						entry.Attributes.Append(entryPath);
						fsElement.AppendChild(entry);

						// Set the properties.
						entryProperty = _fileIndex.CreateElement("Path");
						entryProperty.InnerText = fileEntry.Path;
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("Filename");
						entryProperty.InnerText = fileEntry.Filename;
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("Extension");
						entryProperty.InnerText = fileEntry.Extension;
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("FileSize");
						entryProperty.InnerText = fileEntry.FileSize.ToString();
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("CompressedFileSize");
						entryProperty.InnerText = fileEntry.CompressedFileSize.ToString();
						entry.AppendChild(entryProperty);

						entryProperty = _fileIndex.CreateElement("FileOffset");
						entryProperty.InnerText = fileEntry.Offset.ToString();
						entry.AppendChild(entryProperty);

						// Add to the list of files.
						fileData.Add(fileEntry.ObjectData);
					}
				}

				byte[] compressed = null;		// Compressed index data.

				// Save the index file.
				compressed = CompressData(Encoding.UTF8.GetBytes(_fileIndex.OuterXml));				
				
				// Open the archive for writing.
				fileStream = File.Open(path, FileMode.Create);
				writer = new BinaryWriterEx(fileStream, false);

				// Write out the archive.
				writer.Write("GORPACK1.ZLib");
				writer.Write(compressed.Length);
				writer.Write(compressed);

				foreach (byte[] compressedData in fileData)
					writer.Write(compressedData);

				// Remove deleted entries.
				_deleted.Clear();
			}
			catch (Exception ex)
			{
				throw new CannotSaveException("Cannot save the file system state.", ex);
			}
			finally
			{
				if (writer != null)
					writer.Close();
				writer = null;
				if (fileStream != null)
					fileStream.Dispose();
				fileStream = null;
			}
		}
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this file system.</param>
        /// <param name="entry">Plug-in entry object.</param>
        internal GorgonZLibFileSystem(string name, PlugInEntryPoint entry)
            : base(name, entry)
        {
        }
        #endregion
    }
}
