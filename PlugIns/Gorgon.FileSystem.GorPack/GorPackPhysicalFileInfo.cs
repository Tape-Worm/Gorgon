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
// Created: Saturday, September 19, 2015 11:45:31 PM
// 
#endregion

using System;
using Gorgon.IO.Providers;

namespace Gorgon.IO.GorPack
{
	/// <summary>
	/// Gorgon packed file physical file information.
	/// </summary>
	class GorPackPhysicalFileInfo
		: IGorgonPhysicalFileInfo
	{
		#region Properties.
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
		public DateTime LastModifiedDate
		{
			get;
		}

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

		public long? CompressedLength
		{
			get;
		}

		public bool IsEncrypted => false;
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorPackPhysicalFileInfo" /> class.
		/// </summary>
		/// <param name="physicalPath">The physical path.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="createDate">The create date.</param>
		/// <param name="lastModDate">The last mod date.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="size">The size.</param>
		/// <param name="virtualPath">The virtual path.</param>
		/// <param name="compressedSize">The size of the compressed file.</param>
		public GorPackPhysicalFileInfo(string physicalPath,
		                               string fileName,
		                               DateTime createDate,
		                               DateTime lastModDate,
		                               long offset,
		                               long size,
		                               string virtualPath,
		                               long? compressedSize)
		{
			FullPath = physicalPath;
			Name = fileName;
			CreateDate = createDate;
			LastModifiedDate = lastModDate;
			Offset = offset;
			Length = size;
			VirtualPath = virtualPath;
			CompressedLength = compressedSize;
		}
		#endregion
	}
}
