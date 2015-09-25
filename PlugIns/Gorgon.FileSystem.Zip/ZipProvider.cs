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
using Gorgon.IO.Providers;
using Gorgon.IO.Zip.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.Zip
{
	/// <summary>
	/// A file system provider for zip files.
	/// </summary>
	class ZipProvider
		: GorgonFileSystemProvider
    {
		#region Variables.
		/// <summary>
		/// Header bytes for a zip file.
		/// </summary>
		public static IEnumerable<byte> ZipHeader = new byte[] { 0x50, 0x4B, 0x3, 0x4 };
		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override GorgonPhysicalFileSystemData OnEnumerate(string physicalMountPoint, IGorgonVirtualDirectory mountPoint)
		{
            var directories = new List<string>();
            var files = new List<IGorgonPhysicalFileInfo>();

			using (var zipStream = new ZipInputStream(File.Open(physicalMountPoint, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				ZipEntry entry;

				while ((entry = zipStream.GetNextEntry()) != null)
				{
					if (!entry.IsDirectory)
					{
						string directoryName = Path.GetDirectoryName(entry.Name).FormatDirectory('/');

						directoryName = mountPoint.FullPath + directoryName;

						if (String.IsNullOrWhiteSpace(directoryName))
						{
							directoryName = "/";
						}


						if (!directories.Contains(directoryName))
						{
							directories.Add(directoryName);
						}

						files.Add(new ZipPhysicalFileInfo(entry, physicalMountPoint, mountPoint));
					}
					else
					{
						directories.Add((mountPoint.FullPath + entry.Name).FormatDirectory('/'));
					}
				}
			}

			return new GorgonPhysicalFileSystemData(directories, files);
		}

		/// <inheritdoc/>
		protected override GorgonFileSystemStream OnOpenFileStream(IGorgonVirtualFile file)
		{
			return new ZipFileStream(file, File.Open(file.MountPoint.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read));
		} 

		/// <inheritdoc/>
		protected override bool OnCanReadFile(string physicalPath)
		{
		    var headerBytes = new byte[4];

			using (FileStream stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				if (stream.Length <= headerBytes.Length)
				{
					return false;
				}

				stream.Read(headerBytes, 0, headerBytes.Length);
			}

		    return headerBytes.SequenceEqual(ZipHeader);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ZipProvider"/> class.
		/// </summary>
		public ZipProvider()
			: base(Resources.GORFS_ZIP_DESC)
		{
            PreferredExtensions.Add(new GorgonFileExtension("Zip", Resources.GORFS_ZIP_FILE_DESC));
		}
		#endregion
    }
}
