#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, September 20, 2015 9:32:27 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.IO.Providers;
using ICSharpCode.SharpZipLib.Zip;

namespace Gorgon.IO.Zip
{
	/// <inheritdoc cref="IGorgonPhysicalFileInfo"/>
	class ZipPhysicalFileInfo
		: IGorgonPhysicalFileInfo
	{
		#region Properties.
		/// <inheritdoc/>
		public long? CompressedLength
		{
			get;
		}

		/// <inheritdoc/>
		public DateTime CreateDate
		{
			get;
		}

		/// <inheritdoc/>
		public string FullPath
		{
			get;
		}

		/// <inheritdoc/>
		public bool IsEncrypted => false;

		/// <inheritdoc/>
		public DateTime LastModifiedDate => CreateDate;

		/// <inheritdoc/>
		public long Length
		{
			get;
		}

		/// <inheritdoc/>
		public string Name
		{
			get;
		}

		/// <inheritdoc/>
		public long Offset
		{
			get;
		}

		/// <inheritdoc/>
		public string VirtualPath
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ZipPhysicalFileInfo" /> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="physicalLocation">The physical location of the zip file.</param>
		/// <param name="mountPoint">Mount point path.</param>
		public ZipPhysicalFileInfo(ZipEntry entry, string physicalLocation, IGorgonVirtualDirectory mountPoint)
		{
			string directory = Path.GetDirectoryName(entry.Name);

			directory = mountPoint.FullPath + directory;

			if (string.IsNullOrWhiteSpace(directory))
			{
				directory = "/";
			}

			directory = directory.FormatDirectory('/');

			CompressedLength = entry.CompressedSize;
			CreateDate = entry.DateTime;
			FullPath = physicalLocation + "::/" + entry.Name;
			Length = entry.Size;
			Offset = entry.Offset;
			Name = Path.GetFileName(entry.Name).FormatFileName();
			VirtualPath = directory + Name;
		}
		#endregion
	}
}
