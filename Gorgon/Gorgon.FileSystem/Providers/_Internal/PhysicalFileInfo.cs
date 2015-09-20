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
// Created: Saturday, September 19, 2015 8:34:24 PM
// 
#endregion

using System;
using System.IO;

namespace Gorgon.IO.Providers
{
	/// <inheritdoc cref="IGorgonPhysicalFileInfo"/>
	class PhysicalFileInfo 
		: IGorgonPhysicalFileInfo
	{
		#region Properties.
		/// <inheritdoc/>
		public string FullPath
		{
			get;
		}

		/// <inheritdoc/>
		public string Name
		{
			get;
		}

		/// <inheritdoc/>
		public DateTime CreateDate
		{
			get;
		}

		/// <inheritdoc/>
		public DateTime LastModifiedDate
		{
			get;
		}

		/// <inheritdoc/>
		public long Offset
		{
			get;
		}

		/// <inheritdoc/>
		public long Length
		{
			get;
		}

		/// <inheritdoc/>
		public string VirtualPath
		{
			get;
		}

		/// <inheritdoc/>
		public long? CompressedLength => null;

		/// <inheritdoc/>
		public bool IsEncrypted => false;
		#endregion

		#region Methods.
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicalFileInfo" /> class.
		/// </summary>
		/// <param name="fullPhysicalPath">The full path.</param>
		/// <param name="createDate">The create date.</param>
		/// <param name="length">The length.</param>
		/// <param name="virtualPath">The virtual path.</param>
		public PhysicalFileInfo(string fullPhysicalPath, DateTime createDate, long length, string virtualPath)
		{
			FullPath = fullPhysicalPath;
			Name = Path.GetFileName(fullPhysicalPath).FormatFileName();
			CreateDate = LastModifiedDate = createDate;
			Length = length;
			VirtualPath = virtualPath;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicalFileInfo"/> type.
		/// </summary>
		/// <param name="file">The file information to evaluate.</param>
		/// <param name="virtualPath">The virtual path.</param>
		public PhysicalFileInfo(FileInfo file, string virtualPath)
		{
			FullPath = file.FullName;
			Name = file.Name;
			CreateDate = file.CreationTime;
			LastModifiedDate = file.LastWriteTime;
			Offset = 0;
			Length = file.Length;
			VirtualPath = virtualPath;
		}
		#endregion
	}
}