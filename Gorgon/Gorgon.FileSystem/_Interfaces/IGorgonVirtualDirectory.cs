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
// Created: Tuesday, September 22, 2015 8:54:34 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Collections;
using Gorgon.Core;

namespace Gorgon.IO
{
	/// <summary>
	/// A representation of a virtual directory within a <see cref="IGorgonFileSystem"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A virtual directory is a container for sub directories and files. 
	/// </para>
	/// <para>
	/// Directories can be created by creating a <see cref="IGorgonFileSystemWriteArea"/> instance and calling its <see cref="IGorgonFileSystemWriteArea.CreateDirectory"/>. Conversely, if you wish to delete 
	/// a directory, call the <see cref="IGorgonFileSystemWriteArea.DeleteDirectory"/> method on the <see cref="IGorgonFileSystemWriteArea"/> object.
	/// </para>
	/// </remarks>
	public interface IGorgonVirtualDirectory
		: IGorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the mount point that supplied this directory.
		/// </summary>
		GorgonFileSystemMountPoint MountPoint
		{
			get;
		}

		/// <summary>
		/// Property to return the <see cref="IGorgonFileSystem"/> that contains this directory.
		/// </summary>
		IGorgonFileSystem FileSystem
		{
			get;
		}

		/// <summary>
		/// Property to return the full path to the directory.
		/// </summary>
		string FullPath
		{
			get;
		}

		/// <summary>
		/// Property to return the list of any child <see cref="IGorgonVirtualDirectory"/> items under this virtual directory.
		/// </summary>
		IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualDirectory> Directories
		{
			get;
		}

		/// <summary>
		/// Property to return the list of <see cref="IGorgonVirtualFile"/> objects within this directory.
		/// </summary>
		IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualFile> Files
		{
			get;
		}

		/// <summary>
		/// Property to return the parent of this directory.
		/// </summary>
		/// <remarks>
		/// If this value is <b>null</b> (<i>Nothing</i> in VB.Net), then this will be the root directory for the file system.
		/// </remarks>
		IGorgonVirtualDirectory Parent
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return all the parents up to the root directory.
		/// </summary>
		/// <returns>A list of all the parents, up to and including the root. Or NULL (<i>Nothing</i> in VB.Net) if there is no parent directory.</returns>
		/// <remarks>A NULL (<i>Nothing</i> in VB.Net) return value indicates that this directory is the root directory.</remarks>
		IEnumerable<IGorgonVirtualDirectory> GetParents();

		/// <summary>
		/// Function to retrieve the total number of directories in this directory including any directories under this one.
		/// </summary>
		/// <returns>The total number of directories.</returns>
		/// <remarks>
		/// Use this to retrieve the total number of <see cref="Directories"/> under this directory. This recursively includes all sub directories for this and child directories. 
		/// To get the count of the immediate subdirectories, use the <see cref="IReadOnlyCollection{T}.Count"/> property on the <see cref="Directories"/> property.
		/// </remarks>
		int GetDirectoryCount();

		/// <summary>
		/// Function to retrieve the total number of files in this directory and optionally, any directories under this one.
		/// </summary>
		/// <param name="includeChildren">[Optional] <b>true</b> to include child directories, <b>false</b> to only use this directory.</param>
		/// <returns>The total number of files.</returns>
		int GetFileCount(bool includeChildren = true);

		/// <summary>
		/// Function to determine if this directory, or optionally, any of the sub directories contains the specified file.
		/// </summary>
		/// <param name="file">The <see cref="IGorgonVirtualFile"/> to search for.</param>
		/// <param name="searchChildren">[Optional] <b>true</b> to search through child directories, <b>false</b> to only search this directory.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		bool ContainsFile(IGorgonVirtualFile file, bool searchChildren = true);
		#endregion
	}
}