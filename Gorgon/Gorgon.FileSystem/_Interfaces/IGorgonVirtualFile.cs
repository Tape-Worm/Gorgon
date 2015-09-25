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
// Created: Thursday, September 24, 2015 12:18:08 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <summary>
	/// A representation of a file from a physical file system.
	/// </summary>
	public interface IGorgonVirtualFile 
		: IGorgonNamedObject
	{
		/// <summary>
		/// Property to return the mount point that is supplying this file.
		/// </summary>
		GorgonFileSystemMountPoint MountPoint
		{
			get;
		}

		/// <summary>
		/// Property to return the file system that owns this file.
		/// </summary>
		IGorgonFileSystem FileSystem
		{
			get;
		}

		/// <summary>
		/// Property to return the physical file information for this virtual file.
		/// </summary>
		IGorgonPhysicalFileInfo PhysicalFile
		{
			get;
		}

		/// <summary>
		/// Property to return the full path to the file in the virtual file system.
		/// </summary>
		string FullPath
		{
			get;
		}

		/// <summary>
		/// Property to return the directory that owns this file.
		/// </summary>
		IGorgonVirtualDirectory Directory
		{
			get;
		}

		/// <summary>
		/// Property to return the extension
		/// </summary>
		string Extension
		{
			get;
		}

		/// <summary>
		/// Property to return the file name without the extension.
		/// </summary>
		string BaseFileName
		{
			get;
		}

		/// <summary>
		/// Property to return the uncompressed size of the file in bytes.
		/// </summary>
		long Size
		{
			get;
		}

		/// <summary>
		/// Property to return the file creation date.
		/// </summary>
		DateTime CreateDate
		{
			get;
		}

		/// <summary>
		/// Property to return the last modified date.
		/// </summary>
		DateTime LastModifiedDate
		{
			get;
		}

		/// <summary>
		/// Function to open a stream to the file on the physical file system.
		/// </summary>
		/// <returns>The open <see cref="Stream"/> object.</returns>
		Stream OpenStream();
	}
}