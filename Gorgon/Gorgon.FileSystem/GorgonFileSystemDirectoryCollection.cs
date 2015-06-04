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
// Created: Monday, June 27, 2011 8:59:02 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gorgon.Collections;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file system virtual directories.
	/// </summary>
	class GorgonFileSystemDirectoryCollection
		: IGorgonNamedObjectDictionary<GorgonFileSystemDirectory>, IGorgonNamedObjectReadOnlyDictionary<GorgonFileSystemDirectory>
	{
		#region Variables.
		// The backing store for the directories.
		private readonly Dictionary<string, GorgonFileSystemDirectory> _directories = new Dictionary<string, GorgonFileSystemDirectory>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region IGorgonNamedObjectDictionary<GorgonFileSystemDirectory> Members
		#region Properties.
		/// <summary>
		/// Property to return whether the directory names are case sensitive.
		/// </summary>
		public bool KeysAreCaseSensitive
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Property to set or return a directory by its name.
		/// </summary>
		public GorgonFileSystemDirectory this[string name]
		{
			get
			{
				name = name.RemoveIllegalPathChars();

				GorgonFileSystemDirectory directory;

				if (!_directories.TryGetValue(name, out directory))
				{
					throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, name));
				}

				return directory;
			}
			set
			{
				name = name.RemoveIllegalPathChars();

				if ((value == null)
					|| (string.IsNullOrWhiteSpace(name)))
				{
					return;
				}

				_directories[name] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove a directory by its name.
		/// </summary>
		/// <param name="name">The name of the directory to remove.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown when no directory with the <paramref name="name"/> could be found in the collection.</exception>
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

			name = name.RemoveIllegalPathChars();

			if (!_directories.ContainsKey(name))
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GORFS_DIRECTORY_NOT_FOUND, name));
			}

			_directories.Remove(name);
		}

		/// <summary>
		/// Function to return a directory by its name.
		/// </summary>
		/// <param name="name">The name of the item to look up.</param>
		/// <param name="value">The directory, if found, or <b>null</b> if not.</param>
		/// <returns>
		///   <b>true</b> if the directory was found, <b>false</b> if not.
		/// </returns>
		public bool TryGetValue(string name, out GorgonFileSystemDirectory value)
		{
			value = null;

			name = name.RemoveIllegalPathChars();

			return !string.IsNullOrWhiteSpace(name) && _directories.TryGetValue(name, out value);
		}

		/// <summary>
		/// Function to return whether a directory with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the directory to find.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		public bool Contains(string name)
		{
			name = name.RemoveIllegalPathChars();

			return !string.IsNullOrWhiteSpace(name) && _directories.ContainsKey(name);
		}
		#endregion
		#endregion

		#region ICollection<GorgonFileSystemDirectory> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get
			{
				return _directories.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove(GorgonFileSystemDirectory item)
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
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when this collection already contains a directory with the same name as the <paramref name="item"/> parameter.</exception>
		public void Add(GorgonFileSystemDirectory item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if (_directories.ContainsKey(item.Name))
			{
				throw new ArgumentException(string.Format(Resources.GORFS_DIRECTORY_EXISTS, item.Name));
			}

			_directories.Add(item.Name, item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			_directories.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(GorgonFileSystemDirectory item)
		{
			return item != null && _directories.ContainsValue(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo(GorgonFileSystemDirectory[] array, int arrayIndex)
		{
			_directories.Values.CopyTo(array, arrayIndex);
		}
		#endregion

		#endregion

		#region IEnumerable<GorgonFileSystemDirectory> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonFileSystemDirectory> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<string, GorgonFileSystemDirectory> directory in _directories)
			{
				yield return directory.Value;
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
			return ((IEnumerable)_directories.Values).GetEnumerator();
		}
		#endregion
	}
}
