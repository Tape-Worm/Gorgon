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
// Created: Sunday, July 03, 2011 9:16:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.BZip2;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.FileSystem.GorPack
{
	/// <summary>
	/// A file system provider for Gorgon BZip2 compressed packed files.
	/// </summary>
	/// <remarks>The BZip2 compressed pack files are written by an older (1.x) version of Gorgon.  This provider will enable the new file system interface to be able to read these files.</remarks>
	public class GorgonGorPackProvider
		: GorgonFileSystemProvider
	{
		#region Value Types.
		/// <summary>
		/// Value for a compressed file system entry.
		/// </summary>
		internal struct CompressedFileEntry
		{
			#region Variables.
			/// <summary>
			/// Uncompressed size of the file.
			/// </summary>
			public long Size;
			/// <summary>
			/// Compressed size of the file.
			/// </summary>
			public long CompressedSize;
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="CompressedFileEntry"/> struct.
			/// </summary>
			/// <param name="size">The size of the file.</param>
			/// <param name="compressedSize">Compressed size of the file.</param>
			public CompressedFileEntry(long size, long compressedSize)
			{
				Size = size;
				CompressedSize = compressedSize;
			}
			#endregion
		}
		#endregion

		#region Variables.
        private string _description = string.Empty;                                         // Description of the provider.
		private IDictionary<string, CompressedFileEntry> _compressedFiles = null;			// List of compressed files.
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return a description of the file system provider.
        /// </summary>        
        public override string Description
        {
            get
            {
                return _description;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
		/// Function to decompress a data block.
		/// </summary>
		/// <param name="data">Data to decompress.</param>
		/// <returns>The uncompressed data.</returns>
		private byte[] Decompress(byte[] data)
		{
			using (MemoryStream sourceStream = new MemoryStream(data))
			{
				using (MemoryStream decompressedStream = new MemoryStream())
				{			
					BZip2.Decompress(sourceStream, decompressedStream, true);
					return decompressedStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Function to parse the XML directory index.
		/// </summary>
		/// <param name="physicalPath">Path to the packed file.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>
		/// <param name="index">Index to parse.</param>
		/// <param name="physicalOffset">Offset in the file to add to the file offsets.</param>
		private void ParseIndexXML(string physicalPath, GorgonFileSystemDirectory mountPoint, XDocument index, long physicalOffset)
		{
			var directories = index.Descendants("Path");

			if (mountPoint == null)
				mountPoint = FileSystem.RootDirectory;

			_compressedFiles = new Dictionary<string, CompressedFileEntry>();

			foreach (var directoryNode in directories)
			{
				GorgonFileSystemDirectory directory = null;
				string path = directoryNode.Attribute("FullPath").Value.FormatDirectory('/') ;
				var files = directoryNode.Elements("File");
				
				directory = this.FileSystem.GetDirectory(mountPoint.FullPath + path);				
				if (directory == null)
					directory = AddDirectoryEntry(mountPoint.FullPath + path);

				foreach (var file in files)
				{
					string fileName = string.Empty;
					string extension = string.Empty;
					long fileOffset = 0;
					long fileSize = 0;
					DateTime fileDate = DateTime.Now;

										
					fileName = file.Element("Filename").Value;
					if (file.Element("Extension") != null)
					{
						if (!string.IsNullOrEmpty(fileName))
							fileName = Path.ChangeExtension(fileName, file.Element("Extension").Value);
					}

					fileOffset = Convert.ToInt64(file.Element("Offset").Value) + physicalOffset;
					fileSize = Convert.ToInt64(file.Element("CompressedSize").Value);
					if (fileSize < 1)
						fileSize = Convert.ToInt64(file.Element("Size").Value);
					else
						_compressedFiles.Add(directory.FullPath + fileName, new CompressedFileEntry(Convert.ToInt64(file.Element("Size").Value), fileSize));

					DateTime.TryParse(file.Element("FileDate").Value, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out fileDate);

					AddFileEntry(directory.FullPath + fileName, physicalPath, physicalPath + "::/" + path, fileSize, fileOffset, fileDate);
				}
			}
		}

		/// <summary>
		/// Function to enumerate the files and directories for a mount point.
		/// </summary>
		/// <param name="physicalPath">Path on the physical file system to enumerate.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>
		protected override void Enumerate(string physicalPath, GorgonFileSystemDirectory mountPoint)
		{
			using (FileStream stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
				{
					string header = reader.ReadString();
					int indexLength = reader.ReadInt32();
					byte[] indexData = new byte[indexLength];
					
					indexData = Decompress(reader.ReadBytes(indexLength));
					ParseIndexXML(physicalPath, mountPoint, XDocument.Parse(Encoding.UTF8.GetString(indexData), LoadOptions.None), stream.Position);
				}
			}
		}

		/// <summary>
		/// Function called when a file is opened as a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <param name="writeable">TRUE if the file can be written to, FALSE if not.</param>
		/// <returns>
		/// The open <see cref="GorgonLibrary.FileSystem.GorgonFileSystemStream"/> file stream object.
		/// </returns>
		protected override GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file, bool writeable)
		{
			if (writeable)
			{
				return WriteToWriteLocation(file);
			}
			else
			{
				return new GorgonGorPackFileStream(file, 
													File.Open(file.MountPoint, FileMode.Open, FileAccess.Read, FileShare.Read), 
													(_compressedFiles.ContainsKey(file.FullPath) ? new Nullable<CompressedFileEntry>(_compressedFiles[file.FullPath]) : null));
			}
		} 

		/// <summary>
		/// Function called when a file is read from the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns></returns>
		/// <remarks>Implementors must implement this method to read the file from the physical file system.</remarks>
		protected override byte[] OnReadFile(GorgonFileSystemFileEntry file)
		{
			byte[] data = new byte[0];

			using (FileStream stream = File.Open(file.MountPoint, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
				{

					stream.Position = file.Offset;
					if (_compressedFiles.ContainsKey(file.FullPath))
						data = Decompress(reader.ReadBytes((int)file.Size));
					else
						data = reader.ReadBytes((int)file.Size);

					return data;
				}
			}
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		public override bool CanReadFile(string physicalPath)
		{
			string header = string.Empty;

			GorgonDebug.AssertParamString(physicalPath, "physicalPath");

			byte[] headerBytes = new byte[4];

			using (FileStream stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (GorgonBinaryReader reader = new GorgonBinaryReader(stream, true))
				{
					header = reader.ReadString();
				}
			}

			return (string.Compare(header, "GORPACK1.SharpZip.BZ2", false) == 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGorPackProvider"/> class.
		/// </summary>
		/// <param name="fileSystem">File system that owns this provider.</param>
        /// <param name="description">The description of the provider.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		internal GorgonGorPackProvider(GorgonFileSystem fileSystem, string description)
			: base(fileSystem)
		{
            _description = description;
            PreferredExtensions = new List<string>() { "gorPack files (*.gorPack)|*.gorPack" };
		}
		#endregion
	}
}
