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
// Created: Monday, June 27, 2011 8:59:49 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Collections;
using Gorgon.IO.Properties;
using Gorgon.IO.Providers;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file entries available from the file system.
	/// </summary>
	class VirtualFileCollection
		: IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualFile>
	{
		#region Variables.
		// Parent directory for this file system entry.
		private readonly VirtualDirectory _parent;
		// The list of file entries.
		private readonly Dictionary<string, VirtualFile> _files = new Dictionary<string, VirtualFile>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public bool KeysAreCaseSensitive => false;

		/// <inheritdoc/>
		IGorgonVirtualFile IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualFile>.this[string fileName] => this[fileName];

		/// <summary>
		/// Property to return a file system file entry by name.
		/// </summary>
		public VirtualFile this[string fileName]
		{
			get
			{
				fileName = fileName.FormatFileName();

				if (!_files.ContainsKey(fileName))
				{
					throw new FileNotFoundException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, fileName));
				}

				return _files[fileName];
			}
		}

		/// <inheritdoc/>
		public int Count => _files.Count;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return whether a file entry with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the file entry to find.</param>
		/// <returns>
		///   <b>true</b>if found, <b>false</b> if not.
		/// </returns>
		public bool Contains(string name)
		{
			name = name.FormatFileName();

			return !string.IsNullOrWhiteSpace(name) && _files.ContainsKey(name);
		}

		/// <inheritdoc/>
		bool IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualFile>.TryGetValue(string name, out IGorgonVirtualFile value)
		{
			VirtualFile file;

			if (!TryGetValue(name, out file))
			{
				value = null;
				return false;
			}

			value = file;
			return true;
		}

		/// <summary>
		/// Function to return a file entry from the collection.
		/// </summary>
		/// <param name="name">The name of the file entry to look up.</param>
		/// <param name="value">The file entry, if found, or <b>null</b> if not.</param>
		/// <returns>
		///   <b>true</b> if the file was found, <b>false</b> if not.
		/// </returns>
		public bool TryGetValue(string name, out VirtualFile value)
		{
			value = null;

			name = name.FormatFileName();

			return !string.IsNullOrWhiteSpace(name) && _files.TryGetValue(name, out value);
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="mountPoint">The mount point for the file.</param>
		/// <param name="fileInfo">The physical file information.</param>
		public void Add(GorgonFileSystemMountPoint mountPoint, IGorgonPhysicalFileInfo fileInfo)
		{
			if (_files.ContainsKey(fileInfo.Name))
			{
				throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, fileInfo.FullPath));
			}

			// Create the entry.
			_files.Add(fileInfo.Name, new VirtualFile(mountPoint, fileInfo, _parent));
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			_files.Clear();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove(VirtualFile item)
		{
			if (item == null)
			{
				return false;
			}

			VirtualFile file;

			if (!_files.TryGetValue(item.Name, out file))
			{
				return false;
			}

			_files.Remove(item.Name);
			return true;
		}

		/// <summary>
		/// Function to return the internal enumerable for this collection.
		/// </summary>
		/// <returns>
		/// The <see cref="IEnumerable{T}"/> for this collection.
		/// </returns>
		internal IEnumerable<VirtualFile> InternalEnumerable()
		{
			return _files.Select(item => item.Value);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IGorgonVirtualFile> GetEnumerator()
		{
			return _files.Select(fileEntry => fileEntry.Value).GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_files.Values).GetEnumerator();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualFileCollection"/> class.
		/// </summary>
		/// <param name="parent">The parent directory that owns this collection.</param>
		internal VirtualFileCollection(VirtualDirectory parent)			
		{
			_parent = parent;
		}
		#endregion
	}
}
