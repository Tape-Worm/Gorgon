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
// Created: Monday, June 27, 2011 8:59:02 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Collections;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file system virtual directories.
	/// </summary>
	class VirtualDirectoryCollection
		: IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualDirectory>
	{
		#region Variables.
		// The backing store for the directories.
		private readonly Dictionary<string, VirtualDirectory> _directories = new Dictionary<string, VirtualDirectory>(StringComparer.OrdinalIgnoreCase);
		// The parent directory that owns this collection.
		private readonly VirtualDirectory _parent;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a directory by its name.
		/// </summary>
		public VirtualDirectory this[string name]
		{
			get
			{
				name = name.FormatPathPart();

				VirtualDirectory directory;

				if (!_directories.TryGetValue(name, out directory))
				{
					throw new DirectoryNotFoundException(string.Format(Resources.GORFS_ERR_DIRECTORY_NOT_FOUND, name));
				}

				return directory;
			}
		}

		/// <summary>
		/// Property to return an item in the dictionary by its name.
		/// </summary>
		IGorgonVirtualDirectory IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualDirectory>.this[string name] => this[name];

		/// <summary>
		/// Property to return whether the keys are case sensitive.
		/// </summary>
		public bool KeysAreCaseSensitive => false;

		/// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements in the collection. 
		/// </returns>
		public int Count => _directories.Count;
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="item"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when this collection already contains a directory with the same name as the <paramref name="item"/> parameter.</exception>
		private void Add(VirtualDirectory item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (_directories.ContainsKey(item.Name))
			{
				throw new ArgumentException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, item.Name));
			}

			_directories.Add(item.Name, item);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove(VirtualDirectory item)
		{
			if (item == null)
			{
				return false;
			}

			if (!_directories.ContainsValue(item))
			{
				return false;
			}

			_directories.Remove(item.Name);
			return true;
		}

		/// <summary>
		/// Function to return a directory by its name.
		/// </summary>
		/// <param name="name">The name of the item to look up.</param>
		/// <param name="value">The directory, if found, or <b>null</b> if not.</param>
		/// <returns>
		///   <b>true</b> if the directory was found, <b>false</b> if not.
		/// </returns>
		public bool TryGetValue(string name, out VirtualDirectory value)
		{
			value = null;

			name = name.FormatPathPart();

			return !string.IsNullOrWhiteSpace(name) && _directories.TryGetValue(name, out value);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IGorgonVirtualDirectory> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<string, VirtualDirectory> directory in _directories)
			{
				yield return directory.Value;
			}
		}

		/// <summary>
		/// Function to return an internal enumerator for manipulation of the directory structure.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		internal IEnumerator<VirtualDirectory> InternalGetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<string, VirtualDirectory> directory in _directories)
			{
				yield return directory.Value;
			}
		}

		/// <summary>
		/// Function to return the internal enumerable for this collection.
		/// </summary>
		/// <returns>
		/// The <see cref="IEnumerable{T}"/> for this collection.
		/// </returns>
		internal IEnumerable<VirtualDirectory> InternalEnumerable()
		{
			return _directories.Select(item => item.Value);
		}

			/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_directories.Values).GetEnumerator();
		}

		/// <summary>
		/// Function to add a new directory to the collection.
		/// </summary>
		/// <param name="mountPoint">The mount point that is creating this directory.</param>
		/// <param name="path">The path to the directory.</param>
		public VirtualDirectory Add(GorgonFileSystemMountPoint mountPoint, string path)
		{
			path = path.FormatDirectory('/');

			if (!path.StartsWith("/"))
			{
				path = "/" + path;
			}

			if (path == "/")
			{
				throw new IOException(string.Format(Resources.GORFS_ERR_DIRECTORY_EXISTS, "/"));
			}

			string[] directories = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (directories.Length == 0)
			{
				throw new ArgumentException(string.Format(Resources.GORFS_ERR_PATH_INVALID, path), nameof(path));
			}

			VirtualDirectory directory = _parent;

			foreach (string item in directories)
			{
				// If there's a file with the same name as the directory, then we can't continue.
				// This should never happen, but if an archive is corrupt, there's always a weird possibility that this could become an issue.
				if (directory.Files.Contains(item))
				{
					throw new IOException(string.Format(Resources.GORFS_ERR_FILE_EXISTS, directory.Files[item].FullPath));
				}

				if (directory.Directories.Contains(item))
				{
					directory = directory.Directories[item];
				}
				else
				{
					var newDirectoryInfo = new VirtualDirectory(mountPoint, _parent.FileSystem, directory, item);
					directory.Directories.Add(newDirectoryInfo);
					directory = newDirectoryInfo;
				}
			}

			return directory;
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			_directories.Clear();
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns><b>true</b>if found, <b>false</b> if not.</returns>
		public bool Contains(string name)
		{
			name = name.FormatPathPart();

			return !string.IsNullOrWhiteSpace(name) && _directories.ContainsKey(name);
		}

		/// <summary>
		/// Function to return an item from the collection.
		/// </summary>
		/// <param name="name">The name of the item to look up.</param>
		/// <param name="value">The item, if found, or the default value for the type if not.</param>
		/// <returns><b>true</b> if the item was found, <b>false</b> if not.</returns>
		bool IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualDirectory>.TryGetValue(string name, out IGorgonVirtualDirectory value)
		{
			VirtualDirectory directory;

			if (!TryGetValue(name, out directory))
			{
				value = null;
				return false;
			}

			value = directory;
			return true;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualDirectoryCollection" /> class.
		/// </summary>
		/// <param name="parent">The parent directory that owns this collection.</param>
		public VirtualDirectoryCollection(VirtualDirectory parent)
		{
			_parent = parent;
		}
		#endregion
	}
}
