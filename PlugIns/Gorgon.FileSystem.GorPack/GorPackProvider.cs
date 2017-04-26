﻿#region MIT.
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
using Gorgon.Core;
using Gorgon.IO.GorPack.Properties;
using Gorgon.IO.Providers;
using ICSharpCode.SharpZipLib.BZip2;

namespace Gorgon.IO.GorPack
{
	/// <summary>
	/// A file system provider for Gorgon BZip2 compressed packed files.
	/// </summary>
	/// <remarks>
	/// The BZip2 compressed pack files are written by an older (1.x) version of Gorgon.  This provider will enable the new file system interface to be able to read these files.
	/// </remarks>
	class GorPackProvider
		: GorgonFileSystemProvider
	{
		#region Constants.
		/// <summary>
		/// The pack file header.
		/// </summary>
		public const string GorPackHeader = "GORPACK1.SharpZip.BZ2";
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
		/// Function to enumerate the available directories stored in the packed file.
		/// </summary>
		/// <param name="index">The XML file containing the index of files and directories.</param>
		/// <param name="mountPoint">The mount point to map into.</param>
		/// <returns>A read only list of directory paths mapped to the virtual file system.</returns>
		private static IReadOnlyList<string> EnumerateDirectories(XDocument index, IGorgonVirtualDirectory mountPoint)
		{
			var result = new List<string>();
			IEnumerable<XElement> directories = index.Descendants("Path");

			foreach (XElement directoryNode in directories)
			{
				var pathAttrib = directoryNode.Attribute("FullPath");

				if (string.IsNullOrWhiteSpace(pathAttrib?.Value))
				{
					throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
				}

				// Add the directory.
				result.Add((mountPoint.FullPath + pathAttrib.Value).FormatDirectory('/'));
			}

			return result;
		}

		/// <summary>
		/// Function to enumerate the available files stored in the packed file.
		/// </summary>
		/// <param name="index">The XML file containing the index of files and directories.</param>
		/// <param name="offset">The offset into the physical file.</param>
		/// <param name="physicalLocation">Physical location of the packed file.</param>
		/// <param name="mountPoint">The mount point to map into.</param>
		/// <returns>A read only list of <see cref="IGorgonPhysicalFileInfo"/> objects mapped to the virtual file system.</returns>
		private static IReadOnlyList<IGorgonPhysicalFileInfo> EnumerateFiles(XDocument index, long offset, string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			IEnumerable<XElement> files = index.Descendants("File");
			var result = new List<IGorgonPhysicalFileInfo>();

			foreach (XElement file in files)
			{
				var fileNameNode = file.Element("Filename");
				var fileExtensionNode = file.Element("Extension");
				var fileOffsetNode = file.Element("Offset");
				var fileCompressedSizeNode = file.Element("CompressedSize");
				var fileSizeNode = file.Element("Size");
				var fileDateNode = file.Element("FileDate");
				var fileLastModNode = file.Element("LastModDate");
				var parentDirectoryPath = file.Parent?.Attribute("FullPath")?.Value;
				
				// We need these nodes.
				if ((fileNameNode == null) || (fileOffsetNode == null)
					|| (fileSizeNode == null) || (fileDateNode == null)
					|| (string.IsNullOrWhiteSpace(fileNameNode.Value))
					|| (string.IsNullOrWhiteSpace(fileDateNode.Value))
					|| (string.IsNullOrWhiteSpace(parentDirectoryPath)))
				{
					throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
				}

				parentDirectoryPath = (mountPoint.FullPath + parentDirectoryPath).FormatDirectory('/');

				string fileName = fileNameNode.Value;
				long? compressedSize = null;

				// If we don't have a creation date, then don't allow the file to be processed.
				if (!DateTime.TryParse(fileDateNode.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate))
				{
					throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
				}

				if ((fileLastModNode == null) || (!DateTime.TryParse(fileLastModNode.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastModDate)))
				{
					lastModDate = fileDate;
				}

				if (!long.TryParse(fileOffsetNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileOffset))
				{
					throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
				}

				fileOffset += offset;

				if (!long.TryParse(fileSizeNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileSize))
				{
					throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
				}

				string fileExtension = fileExtensionNode?.Value;

				if (!string.IsNullOrWhiteSpace(fileExtension) && (!string.IsNullOrWhiteSpace(fileName)))
				{
					fileName += fileExtension;
				}

				// If the file is compressed, then add it to a special list.
				if (fileCompressedSizeNode != null)
				{

					if (!long.TryParse(fileCompressedSizeNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long compressed))
					{
						throw new FileLoadException(Resources.GORFS_GORPACK_ERR_FILEINDEX_CORRUPT);
					}

					if (compressed > 0)
					{
						compressedSize = compressed;
					}
				}

				result.Add(new GorPackPhysicalFileInfo(physicalLocation + "::/" + parentDirectoryPath + fileName,
				                                       fileName,
				                                       fileDate,
				                                       lastModDate,
				                                       fileOffset,
				                                       fileSize,
				                                       parentDirectoryPath + fileName,
				                                       compressedSize));
			}

			return result;
		}

		/// <summary>
		/// Function to enumerate the files and directories from a physical location and map it to a virtual location.
		/// </summary>
		/// <param name="physicalLocation">The physical location containing files and directories to enumerate.</param>
		/// <param name="mountPoint">A <see cref="IGorgonVirtualDirectory"/> that the directories and files from the physical file system will be mounted into.</param>		
		/// <returns>A <see cref="GorgonPhysicalFileSystemData"/> object containing information about the directories and files contained within the physical file system.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalLocation"/>, or the <paramref name="mountPoint"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalLocation"/> parameter is empty.</exception>
		/// <remarks>
		/// <para>
		/// This will return a <see cref="GorgonPhysicalFileSystemData"/> representing the paths to directories and <see cref="IGorgonPhysicalFileInfo"/> objects under the virtual file system. Each file 
		/// system file and directory is mapped from its <paramref name="physicalLocation"/> on the physical file system to a <paramref name="mountPoint"/> on the virtual file system. For example, if the 
		/// mount point is set to <c>/MyMount/</c>, and the physical location of a file is <c>c:\SourceFileSystem\MyDirectory\MyTextFile.txt</c>, then the returned value should be 
		/// <c>/MyMount/MyDirectory/MyTextFile.txt</c>.
		/// </para>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can override this method to read the list of files from another type of file system, like a Zip file.
		/// </para>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to read the list of directories and files from another type of file system, like a Zip file. 
		/// The default functionality will only enumerate directories and files from the operating system file system.
		/// </para>
		/// </remarks>
		protected override GorgonPhysicalFileSystemData OnEnumerate(string physicalLocation, IGorgonVirtualDirectory mountPoint)
        {
            using (var reader = new GorgonBinaryReader(File.Open(physicalLocation, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Skip the header.
                reader.ReadString();

                int indexLength = reader.ReadInt32();

                byte[] indexData = Decompress(reader.ReadBytes(indexLength));
                string xmlData = Encoding.UTF8.GetString(indexData);
	            XDocument index = XDocument.Parse(xmlData, LoadOptions.None);

	            return new GorgonPhysicalFileSystemData(EnumerateDirectories(index, mountPoint),
	                                                    EnumerateFiles(index, reader.BaseStream.Position, physicalLocation, mountPoint));
            }
        }

		/// <summary>
		/// Function to open a stream to a file on the physical file system from the <see cref="IGorgonVirtualFile"/> passed in.
		/// </summary>
		/// <param name="file">The <see cref="IGorgonVirtualFile"/> that will be used to locate the file that will be opened on the physical file system.</param>
		/// <returns>A <see cref="Stream"/> to the file, or <b>null</b> if the file does not exist.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This will take the <see cref="IGorgonVirtualFile"/> and open its corresponding physical file location as a stream for reading. The stream that is returned will be opened, and as such, it is the 
		/// responsibility of the user to close the stream when finished.
		/// </para>
		/// <para>
		/// If the file does not exist in the physical file system, this method should return <b>null</b>.
		/// </para>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> plug in can overload this method to return a stream into a file within their specific native provider (e.g. a Zip file provider will 
		/// return a stream into the zip file positioned at the location of the compressed file within the zip file).
		/// </para>
		/// </remarks>
		protected override GorgonFileSystemStream OnOpenFileStream(IGorgonVirtualFile file)
		{
			return new GorPackFileStream(file,
			                                   File.Open(file.MountPoint.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read),
			                                   file.PhysicalFile.CompressedLength);
		}

		/// <summary>
		/// Function to determine if a physical file system can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the packed file containing the file system.</param>
		/// <returns><b>true</b> if the provider can read the packed file, <b>false</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		/// <remarks>
		/// <para>
		/// This will test a physical file system (e.g. a Zip file) to see if the provider can open it or not. If used with a directory on an operating system file system, this method should always return 
		/// <b>false</b>.
		/// </para>
		/// <para>
		/// When used with a <see cref="IGorgonFileSystemProvider"/> that supports a non operating system based physical file system, such as the <see cref="GorgonFileSystemRamDiskProvider"/>, then this 
		/// method should compare the <paramref name="physicalPath"/> with its <see cref="IGorgonFileSystemProvider.Prefix"/> to ensure that the <see cref="IGorgonFileSystem"/> requesting the provider is using the correct provider.
		/// </para>
		/// <para>
		/// Implementors of a <see cref="GorgonFileSystemProvider"/> should override this method to determine if a packed file can be read by reading the header of the file specified in <paramref name="physicalPath"/>.
		/// </para>
		/// </remarks>
		protected override bool OnCanReadFile(string physicalPath)
		{
			string header;

			using (var reader = new GorgonBinaryReader(File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				// If the length of the stream is less or equal to the header size, it's unlikely that we can read this file.
				if (reader.BaseStream.Length <= GorPackHeader.Length)
				{
					return false;
				}

			    reader.ReadByte();
				header = new string(reader.ReadChars(GorPackHeader.Length));
			}

			return (string.Equals(header, GorPackHeader));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorPackProvider"/> class.
		/// </summary>
		public GorPackProvider()
			: base(Resources.GORFS_GORPACK_DESC)
		{
			PreferredExtensions = new GorgonFileExtensionCollection
			                      {
				                      new GorgonFileExtension("gorPack", Resources.GORFS_GORPACK_FILE_DESC)
			                      };
		}
		#endregion
	}
}
