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
// Created: Monday, June 27, 2011 9:33:12 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.IO.Zip
{
	/// <summary>
	/// A file system provider for zip files.
	/// </summary>
	public class GorgonZipFileSystemProvider
		: GorgonFileSystemProvider
	{
		#region Methods.
		/// <summary>
		/// Function to enumerate the files and directories for a mount point.
		/// </summary>
		/// <param name="physicalMountPoint">Path on the physical file system to enumerate.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>
		protected override void Enumerate(string physicalMountPoint, GorgonFileSystemDirectory mountPoint)
		{
			if (mountPoint == null)
				mountPoint = FileSystem.Directories[0];					

			using (FileStream stream = File.Open(physicalMountPoint, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (ZipInputStream zipStream = new ZipInputStream(stream))
				{
					ZipEntry entry = null;

					while ((entry = zipStream.GetNextEntry()) != null)
					{
						if (entry.IsDirectory)
							AddDirectoryEntry(mountPoint.FullPath + entry.Name);
						else
						{
							GorgonFileSystemDirectory directory = null;
							string directoryName = GorgonPath.FormatDirectory(Path.GetDirectoryName(entry.Name), '/');
							string fileName = GorgonPath.FormatFileName(Path.GetFileName(entry.Name));

							directoryName = mountPoint.FullPath + directoryName;
							if (string.IsNullOrEmpty(directoryName))
								directoryName = "/";

							directory = FileSystem.GetDirectory(directoryName);
							if (directory == null)
								AddDirectoryEntry(directoryName);
							
							AddFileEntry(directory.FullPath + fileName, physicalMountPoint, physicalMountPoint + "::/" + entry.Name, entry.Size, entry.Offset, entry.DateTime);
						}
					}
				}
			}
		}

		/// <summary>
		/// Function called when a file is written to the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <param name="data">Data to write to the file.</param>
		/// <remarks>Implementors must implement this function to read the file from the physical file system.</remarks>
		protected override void OnWriteFile(GorgonFileSystemFileEntry file, byte[] data)
		{
			throw new NotImplementedException("The " + GetType().FullName + " provider is read-only.");
		}

		/// <summary>
		/// Function called when a file is opened as a file stream.
		/// </summary>
		/// <param name="file">File to open.</param>
		/// <param name="writeable">TRUE if the file can be written to, FALSE if not.</param>
		/// <returns>
		/// The open <see cref="GorgonLibrary.IO.GorgonFileSystemStream"/> file stream object.
		/// </returns>
		/// <remarks>Some providers cannot write, and should throw an exception.</remarks>
		protected override GorgonFileSystemStream OnOpenFileStream(GorgonFileSystemFileEntry file, bool writeable)
		{
			FileStream stream = null;

			if (writeable)
				throw new NotImplementedException("The " + GetType().FullName + " provider is read-only.");

			try
			{
				stream = File.Open(file.MountPoint, FileMode.Open, FileAccess.Read, FileShare.Read);
				return new GorgonZipFileStream(file, stream);
			}
			catch
			{
				if (stream != null)
					stream.Dispose();
				throw;
			}
		} 

		/// <summary>
		/// Function called when a file is read from the provider.
		/// </summary>
		/// <param name="file">File to read.</param>
		/// <returns>The file data stored in a byte array or NULL if the file could not be read.</returns>
		/// <remarks>Implementors must implement this function to read the file from the physical file system.</remarks>
		protected override byte[] OnReadFile(GorgonFileSystemFileEntry file)
		{
			using (FileStream stream = File.Open(file.MountPoint, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (ZipInputStream zipStream = new ZipInputStream(stream))
				{
					ZipEntry entry = null;

					while ((entry = zipStream.GetNextEntry()) != null)
					{
						if (entry.IsFile)
						{
							string newPath = entry.Name;
							string physicalPath = file.PhysicalFileSystemPath.Substring(file.PhysicalFileSystemPath.LastIndexOf(':') + 1);

							if (!newPath.StartsWith("/"))
								newPath = "/" + newPath;

							if (string.Compare(newPath, physicalPath, true) == 0)
							{
								byte[] chunk = new byte[entry.Size];
								zipStream.Read(chunk, 0, chunk.Length);

								using (MemoryStream decodedStream = new MemoryStream(chunk.Length))
								{
									decodedStream.Write(chunk, 0, chunk.Length);
									return decodedStream.ToArray();
								}
							}
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Function to determine if a file can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the file containing the file system.</param>
		/// <returns>
		/// TRUE if the provider can read the packed file, FALSE if not.
		/// </returns>
		/// <remarks>This function is applicable to packed files only.
		/// <para>Implementors must use this function to determine if a packed file can be read by reading the header of the file.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		public override bool CanReadFile(string physicalPath)
		{
			GorgonDebug.AssertParamString(physicalPath, "physicalPath");

			byte[] headerBytes = new byte[4];

			using (FileStream stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				stream.Read(headerBytes, 0, headerBytes.Length);
			}

			return (string.Compare(Encoding.UTF8.GetString(headerBytes), "PK\x03\x04", false) == 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonZipFileSystemProvider"/> class.
		/// </summary>
		/// <param name="fileSystem">File system that owns this provider.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		internal GorgonZipFileSystemProvider(GorgonFileSystem fileSystem)			
			: base(fileSystem)
		{
			PreferredExtensions = new List<string>() { "Zip files (*.zip)|*.zip" };
		}
		#endregion
	}
}
