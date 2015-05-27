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
// Created: Monday, June 27, 2011 8:59:49 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file entries available from the file system.
	/// </summary>
	class GorgonFileSystemFileEntryCollection
		: IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry>, IGorgonNamedObjectReadOnlyDictionary<GorgonFileSystemFileEntry>
	{
		#region Variables.
		// Parent directory for this file system entry.
		private readonly GorgonFileSystemDirectory _parent;
		// The list of file entries.
		private readonly Dictionary<string, GorgonFileSystemFileEntry> _files = new Dictionary<string, GorgonFileSystemFileEntry>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemFileEntryCollection"/> class.
		/// </summary>
		/// <param name="parent">The parent directory that owns this collection.</param>
		internal GorgonFileSystemFileEntryCollection(GorgonFileSystemDirectory parent)			
		{
			_parent = parent;
		}
		#endregion

		#region IGorgonNamedObjectDictionary<GorgonFileSystemFileEntry> Members
		#region Properties.
		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Property to return whether the file entry names are case sensitive.
		/// </summary>
		public bool KeysAreCaseSensitive
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Property to set or return a file system file entry by name.
		/// </summary>
		public GorgonFileSystemFileEntry this[string fileName]
		{
			get
			{
				fileName = fileName.RemoveIllegalFilenameChars();

				if (!_files.ContainsKey(fileName))
				{
					throw new FileNotFoundException(string.Format(Resources.GORFS_FILE_NOT_FOUND, fileName));
				}

				return _files[fileName];
			}
			set
			{
				string formattedFileName = fileName.RemoveIllegalFilenameChars();

				if (string.IsNullOrWhiteSpace(formattedFileName))
				{
					return;
				}

				GorgonFileSystemFileEntry file;
				_files.TryGetValue(formattedFileName, out file);

				// If we pass in NULL, remove the file.
				if (value == null)
				{
					if (file == null)
					{
						return;
					}

					file.Directory = null;
					_files.Remove(formattedFileName);
					return;
				}

				if (file == null)
				{
					Add(value);
					return;
				}

				if ((value.Directory != null) && (value.Directory != _parent))
				{
					throw new GorgonException(GorgonResult.CannotBind,
					                          string.Format(Resources.GORFS_ERR_FILE_BELONGS_TO_ANOTHER_DIRECTORY, value.Name, value.Directory.FullPath));
				}

				file.Directory = null;

				if (!formattedFileName.Equals(value.Name, StringComparison.OrdinalIgnoreCase))
				{
					Remove(file);
				}

				value.Directory = _parent;
				_files[value.Name] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public void CopyTo(GorgonFileSystemFileEntry[] array, int arrayIndex)
		{
			_files.Values.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Function to remove a file entry from the collection by name.
		/// </summary>
		/// <param name="name">Name of the file entry to remove.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <exception cref="FileNotFoundException">Thrown when no file entry with the <paramref name="name"/> could be found in the collection.</exception>
		public void Remove(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY);
			}

			name = name.RemoveIllegalFilenameChars();
			GorgonFileSystemFileEntry file;

			if (!_files.TryGetValue(name, out file))
			{
				throw new FileNotFoundException(string.Format(Resources.GORFS_FILE_NOT_FOUND, name));
			}

			file.Directory = null;
			_files.Remove(name);
		}

		/// <summary>
		/// Function to return whether a file entry with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the file entry to find.</param>
		/// <returns>
		///   <c>true</c>if found, <c>false</c> if not.
		/// </returns>
		public bool Contains(string name)
		{
			name = name.RemoveIllegalPathChars();

			return !string.IsNullOrWhiteSpace(name) && _files.ContainsKey(name);
		}

		/// <summary>
		/// Function to return a file entry from the collection.
		/// </summary>
		/// <param name="name">The name of the file entry to look up.</param>
		/// <param name="value">The file entry, if found, or <c>null</c> if not.</param>
		/// <returns>
		///   <c>true</c> if the file was found, <c>false</c> if not.
		/// </returns>
		public bool TryGetValue(string name, out GorgonFileSystemFileEntry value)
		{
			value = null;

			name = name.RemoveIllegalFilenameChars();

			return !string.IsNullOrWhiteSpace(name) && _files.TryGetValue(name, out value);
		}
		#endregion
		#endregion

		#region ICollection<GorgonFileSystemFileEntry> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get
			{
				return _files.Count;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when a file with the same name as <paramref name="item"/> already exists.
		/// <para>-or-</para>
		/// <para>Thrown when the file entry in <paramref name="item"/> is already assigned to another directory.</para>
		/// </exception>
		public void Add(GorgonFileSystemFileEntry item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if ((item.Directory != null) && (item.Directory != _parent))
			{
				throw new ArgumentException(string.Format(Resources.GORFS_ERR_FILE_BELONGS_TO_ANOTHER_DIRECTORY, item.Name, item.Directory.FullPath));
			}

			if (_files.ContainsKey(item.Name))
			{
				throw new ArgumentException(string.Format(Resources.GORFS_FILE_EXISTS, item.Name));
			}

			item.Directory = _parent;
			_files.Add(item.Name, item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			foreach (GorgonFileSystemFileEntry file in this)
			{
				file.Directory = null;
			}

			_files.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(GorgonFileSystemFileEntry item)
		{
			return item != null && _files.ContainsValue(item);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove(GorgonFileSystemFileEntry item)
		{
			if (item == null)
			{
				return false;
			}

			GorgonFileSystemFileEntry file;

			if (!_files.TryGetValue(item.Name, out file))
			{
				return false;
			}

			file.Directory = null;
			_files.Remove(item.Name);
			return true;
		}
		#endregion
		#endregion

		#region IEnumerable<GorgonFileSystemFileEntry> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonFileSystemFileEntry> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<string, GorgonFileSystemFileEntry> fileEntry in _files)
			{
				yield return fileEntry.Value;
			}
		}

		#endregion

		#region IEnumerable Members
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
	}
}
