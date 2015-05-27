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
using System.IO;
using System.Linq;
using Gorgon.IO.Zip.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.Zip
{
	/// <summary>
	/// A file system provider for zip files.
	/// </summary>
	public class GorgonZipProvider
		: GorgonFileSystemProvider
    {
        #region Variables.
        private readonly string _description = string.Empty;             // The description of the provider.
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
		/// Function to enumerate the files and directories for a mount point.
		/// </summary>
		/// <param name="physicalMountPoint">Path on the physical file system to enumerate.</param>
		/// <param name="mountPoint">Directory to hold the sub directories and files.</param>
		/// <param name="physicalDirectories">The directories in the physical file system.</param>
		/// <param name="physicalFiles">The files in the physical file system.</param>
		protected override void Enumerate(string physicalMountPoint, GorgonFileSystemDirectory mountPoint, out string[] physicalDirectories, out PhysicalFileInfo[] physicalFiles)
		{
            var directories = new List<string>();
            var files = new List<PhysicalFileInfo>();

			using (var zipStream = new ZipInputStream(File.Open(physicalMountPoint, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				ZipEntry entry;
					
				while ((entry = zipStream.GetNextEntry()) != null)
				{
					if (!entry.IsDirectory)
					{
					    string directoryName = Path.GetDirectoryName(entry.Name).FormatDirectory('/');
					    string fileName = Path.GetFileName(entry.Name).FormatFileName();

					    directoryName = mountPoint.FullPath + directoryName;

					    if (string.IsNullOrWhiteSpace(directoryName))
					    {
					        directoryName = "/";
					    }

					    directories.Add(directoryName);

					    files.Add(new PhysicalFileInfo(physicalMountPoint + "::/" + entry.Name, fileName, entry.DateTime,
					                                    entry.Offset, entry.Size, directoryName + fileName));
					}
					else
					{
					    directories.Add(mountPoint.FullPath + entry.Name);
					}
				}
			}

		    physicalDirectories = directories.ToArray();
		    physicalFiles = files.ToArray();

            directories.Clear();
            files.Clear();
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
			return new GorgonZipFileStream(file, File.Open(file.MountPoint, FileMode.Open, FileAccess.Read, FileShare.Read));
		} 

		/// <summary>
		/// Function to determine if a file can be read by this provider.
		/// </summary>
		/// <param name="physicalPath">Path to the file containing the file system.</param>
		/// <returns>
		/// <c>true</c> if the provider can read the packed file, <c>false</c> if not.
		/// </returns>
		/// <remarks>This method is applicable to packed files only.
		/// <para>Implementors must use this method to determine if a packed file can be read by reading the header of the file.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="physicalPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="physicalPath"/> parameter is an empty string.</exception>
		public override bool CanReadFile(string physicalPath)
		{
		    if (physicalPath == null)
		    {
		        throw new ArgumentNullException("physicalPath");
		    }

		    if (string.IsNullOrWhiteSpace(physicalPath))
		    {
		        throw new ArgumentException(Resources.GORFS_PARAMETER_MUST_NOT_BE_EMPTY, "physicalPath");
		    }

			var headerBytes = new byte[4];

			using (FileStream stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				stream.Read(headerBytes, 0, headerBytes.Length);
			}

		    return headerBytes.SequenceEqual(GorgonZipPlugIn.ZipHeader);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonZipProvider"/> class.
		/// </summary>
        /// <param name="description">The description of the provider.</param>
		internal GorgonZipProvider(string description)
		{
            _description = description;
            PreferredExtensions.Add(new GorgonFileExtension("Zip", Resources.GORFS_FILE_DESC));
		}
		#endregion
	}
}
