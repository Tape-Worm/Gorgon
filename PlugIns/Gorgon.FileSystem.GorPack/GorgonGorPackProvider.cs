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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Gorgon.IO.GorPack.Properties;
using ICSharpCode.SharpZipLib.BZip2;

namespace Gorgon.IO.GorPack
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

		private IDictionary<string, CompressedFileEntry> _compressedFiles;			// List of compressed files.
		#endregion

        #region Properties.
        /// <summary>
        /// Property to return a description of the file system provider.
        /// </summary>        
        public override string Description
        {
	        get;
        }

		#endregion

        #region Methods.
        /// <summary>
		/// Function to decompress a data block.
		/// </summary>
		/// <param name="data">Data to decompress.</param>
		/// <returns>The uncompressed data.</returns>
		private static byte[] Decompress(byte[] data)
		{
			using (var sourceStream = new MemoryStream(data))
			{
				using (var decompressedStream = new MemoryStream())
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
		/// <param name="physicalDirectories">The list of directories in the physical file system.</param>
		/// <param name="physicalFiles">The list of files in the physical file system.</param>
		private void ParseIndexXML(string physicalPath, GorgonFileSystemDirectory mountPoint, XContainer index, long physicalOffset, out string[] physicalDirectories, out PhysicalFileInfo[] physicalFiles)
		{
            var dirNodeNames = new List<string>();
            var fileNodeInfo = new List<PhysicalFileInfo>();

			var directories = index.Descendants("Path");

		    _compressedFiles = new Dictionary<string, CompressedFileEntry>();

			foreach (var directoryNode in directories)
			{
			    var pathAttrib = directoryNode.Attribute("FullPath");

                if (string.IsNullOrWhiteSpace(pathAttrib?.Value))
                {
                    throw new FileLoadException(Resources.GORFS_FILEINDEX_CORRUPT);
                }
                
				string path = (mountPoint.FullPath + pathAttrib.Value).FormatDirectory('/') ;
				var files = directoryNode.Elements("File");

                // Add the directory.
			    dirNodeNames.Add(path);

				foreach (var file in files)
				{
				    var fileNameNode = file.Element("Filename");
				    var fileExtensionNode = file.Element("Extension");
				    var fileOffsetNode = file.Element("Offset");
				    var fileCompressedSizeNode = file.Element("CompressedSize");
                    var fileSizeNode = file.Element("Size");
				    var fileDateNode = file.Element("FileDate");

                    // We need these nodes.
                    if ((fileNameNode == null) || (fileOffsetNode == null)
                        || (fileSizeNode == null) || (fileDateNode == null)
                        || (string.IsNullOrWhiteSpace(fileNameNode.Value))
                        || (string.IsNullOrWhiteSpace(fileDateNode.Value)))
                    {
                        throw new FileLoadException(Resources.GORFS_FILEINDEX_CORRUPT);
                    }

				    string fileName = fileNameNode.Value;
				    long fileOffset;
				    long fileSize;
                    DateTime fileDate;

                    // If we don't have a creation date, then don't allow the file to be processed.
                    if (!DateTime.TryParse(fileDateNode.Value, CultureInfo.InvariantCulture, DateTimeStyles.None,  out fileDate))
                    {
                        throw new FileLoadException(Resources.GORFS_FILEINDEX_CORRUPT);
                    }

				    if (!Int64.TryParse(fileOffsetNode.Value, out fileOffset))
                    {
                        throw new FileLoadException(Resources.GORFS_FILEINDEX_CORRUPT);
                    }

                    fileOffset += physicalOffset;

                    if (!Int64.TryParse(fileSizeNode.Value, out fileSize))
                    {
                        throw new FileLoadException(Resources.GORFS_FILEINDEX_CORRUPT);
                    }

					string fileExtension = fileExtensionNode?.Value;

					if (!string.IsNullOrWhiteSpace(fileExtension) && (!string.IsNullOrWhiteSpace(fileName)))
					{
						fileName += fileExtension;
					}

					// If the file is compressed, then add it to a special list.
                    if (fileCompressedSizeNode != null)
                    {
                        long compressedSize;

                        if (!Int64.TryParse(fileCompressedSizeNode.Value, out compressedSize))
                        {
                            throw new FileLoadException(Resources.GORFS_FILEINDEX_CORRUPT);
                        }

                        if (compressedSize > 0)
                        {
                            // Add to our list of compressed files for processing later.
                            _compressedFiles.Add(path + fileName,
                                                 new CompressedFileEntry(fileSize, compressedSize));
                        }
                    }

				    fileNodeInfo.Add(new PhysicalFileInfo(physicalPath + "::/" + path + fileName, fileName, fileDate, fileOffset, fileSize, path + fileName));
				}
			}

		    physicalDirectories = dirNodeNames.ToArray();
		    physicalFiles = fileNodeInfo.ToArray();

		    dirNodeNames.Clear();
            fileNodeInfo.Clear();
		}

        /// <summary>
        /// Function to enumerate the files and directories for a mount point.
        /// </summary>
        /// <param name="physicalMountPoint">Mount point being enumerated.</param>
        /// <param name="mountPoint">Directory to hold the sub directories and files.</param>
        /// <param name="physicalDirectories">A list of directories in the physical file system (formatted to the virtual file system).</param>
        /// <param name="physicalFiles">A list of files in the physical file system (formatted to the virtual file system).</param>
        protected override void Enumerate(string physicalMountPoint, GorgonFileSystemDirectory mountPoint, 
            out string[] physicalDirectories, out PhysicalFileInfo[] physicalFiles)
        {
            using (var reader = new GorgonBinaryReader(File.Open(physicalMountPoint, FileMode.Open, FileAccess.Read, FileShare.Read), false))
            {
                // Skip the header.
                reader.ReadString();

                int indexLength = reader.ReadInt32();

                byte[] indexData = Decompress(reader.ReadBytes(indexLength));
                string xmlData = Encoding.UTF8.GetString(indexData);

                ParseIndexXML(physicalMountPoint, mountPoint, XDocument.Parse(xmlData, LoadOptions.None),
                                reader.BaseStream.Position, out physicalDirectories, out physicalFiles);
            }
        }

		/// <summary>
		/// Function called when a file is opened as a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <returns>
		/// The open <see cref="GorgonFileSystemStream" /> file stream object.
		/// </returns>
		protected override GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file)
		{
		    return new GorgonGorPackFileStream(file,
		                                       File.Open(file.MountPoint, FileMode.Open, FileAccess.Read, FileShare.Read),
		                                       (_compressedFiles.ContainsKey(file.FullPath)
		                                            ? new CompressedFileEntry?(_compressedFiles[file.FullPath])
		                                            : null));
		} 

		/// <summary>
		/// Function to determine if a file can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the file containing the file system.</param>
		/// <returns>
		/// <b>true</b> if the provider can read the packed file, <b>false</b> if not.
		/// </returns>
		/// <remarks>This method is applicable to packed files only.
		/// <para>Implementors must use this method to determine if a packed file can be read by reading the header of the file.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		public override bool CanReadFile(string physicalPath)
		{
			string header;

		    if (physicalPath == null)
		    {
		        throw new ArgumentNullException(nameof(physicalPath));
		    }

		    if (string.IsNullOrWhiteSpace(physicalPath))
		    {
		        throw new ArgumentException(Resources.GORFS_PARAMETER_MUST_NOT_BE_EMPTY, nameof(physicalPath));
		    }

			using (var reader = new GorgonBinaryReader(File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read), false))
			{
			    reader.ReadByte();
				header = new string(reader.ReadChars(GorgonGorPackPlugIn.GorPackHeader.Length));
			}

			return (string.Equals(header, GorgonGorPackPlugIn.GorPackHeader));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGorPackProvider"/> class.
		/// </summary>
        /// <param name="description">The description of the provider.</param>
		internal GorgonGorPackProvider(string description)
		{
            Description = description;
		    PreferredExtensions.Add(new GorgonFileExtension("gorPack", Resources.GORFS_FILE_DESC));
		}
		#endregion
	}
}
