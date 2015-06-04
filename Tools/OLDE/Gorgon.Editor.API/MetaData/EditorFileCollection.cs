#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Wednesday, October 15, 2014 10:15:47 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// A list of editor files present in the file system.
	/// </summary>
	public class EditorFileCollection
		: ICollection<EditorFile>, ICollection
	{
		#region Variables.
		private readonly Dictionary<string, EditorFile> _files;			// A list of files.
		private readonly static object _syncLock = new object();		// Synchronization object for multiple threads.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return a file by name.
		/// </summary>
		/// <remarks>Setting a file to NULL (<i>Nothing</i> in VB.Net) will remove it from the collection.</remarks>
		public EditorFile this[string name]
		{
			get
			{
				return _files[name];
			}
			set
			{
				EditorFile file;

				_files.TryGetValue(name, out file);

				if (value == null)
				{
					if (file != null)
					{
						_files.Remove(name);
					}

					return;
				}

				// If the file existed, but we're changing its name, then remove the previous file.
				if ((file != null) && (!string.Equals(name, value.FilePath, StringComparison.OrdinalIgnoreCase)))
				{
					_files.Remove(name);
				}
			
				_files[value.FilePath] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize the collection into an XML node.
		/// </summary>
		/// <returns>The XML nodes containing the serialized collection.</returns>
		internal IEnumerable<XElement> Serialize()
		{
			return this.Select(dependency => dependency.Serialize()).ToList();
		}

		/// <summary>
		/// Function to deserialize a file collection from an XML node.
		/// </summary>
		/// <param name="fileNodes">The list of nodes that contain the files.</param>
		/// <returns>The deserialized files.</returns>
		internal static EditorFileCollection Deserialize(IEnumerable<XElement> fileNodes)
		{
			var result = new EditorFileCollection();

			if (fileNodes == null)
			{
				return result;
			}

			// Get the files.
			// ReSharper disable PossibleMultipleEnumeration
			foreach (XElement fileNode in fileNodes)
			{
				EditorFile file = EditorFile.Deserialize(fileNode);
				result[file.FilePath] = file;
			}

			// Now that all the files are loaded, get the dependencies.
			foreach (EditorFile file in result)
			{
				XElement fileNode = fileNodes.FirstOrDefault(item =>
				                                             {
					                                             XAttribute attr = item.Attribute(EditorFile.EditorFilePathAttr);

					                                             return attr != null && string.Equals(file.FilePath, attr.Value, StringComparison.OrdinalIgnoreCase);
				                                             });

				if (fileNode == null)
				{
					continue;
				}

				XElement dependRoot = fileNode.Element(EditorFile.EditorDependenciesNodeRoot);

				if (dependRoot == null)
				{
					continue;
				}

				file.DependsOn.CopyFrom(DependencyCollection.Deserialize(result, dependRoot.Elements(Dependency.DependencyNode)));
			}

			// ReSharper restore PossibleMultipleEnumeration
			return result;
		}

		/// <summary>
		/// Function to determine if the collection contains a dependency with the specified name and type.
		/// </summary>
		/// <param name="name">Name of the dependency.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		public bool Contains(string name)
		{
			return !string.IsNullOrWhiteSpace(name) && _files.ContainsKey(name);
		}

		/// <summary>
		/// Function to try and retrieve the value with the specified name.
		/// </summary>
		/// <param name="name">Name of the item to retrieve.</param>
		/// <param name="value">The value, if found.  NULL if not.</param>
		/// <returns><b>true</b> if the value was found, <b>false</b> if not.</returns>
		public bool TryGetValue(string name, out EditorFile value)
		{
			return _files.TryGetValue(name, out value);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFileCollection"/> class.
		/// </summary>
		internal EditorFileCollection()
		{
			_files = new Dictionary<string, EditorFile>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region ICollection<EditorFile> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count
		{
			get
			{
				return _files.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<EditorFile>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="System.ArgumentNullException">item</exception>
		/// <exception cref="System.ArgumentException"></exception>
		void ICollection<EditorFile>.Add(EditorFile item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if (_files.ContainsKey(item.FilePath))
			{
				throw new ArgumentException(string.Format(APIResources.GOREDIT_ERR_FILE_ALREADY_EXISTS,
				                                          APIResources.GOREDIT_TEXT_FILE.ToLower(CultureInfo.CurrentUICulture),
				                                          item.FilePath));
			}

			_files[item.FilePath] = item;
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		void ICollection<EditorFile>.Clear()
		{
			_files.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(EditorFile item)
		{
			return _files.ContainsValue(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		void ICollection<EditorFile>.CopyTo(EditorFile[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">item</exception>
		bool ICollection<EditorFile>.Remove(EditorFile item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			return _files.ContainsKey(item.FilePath) && _files.Remove(item.FilePath);
		}
		#endregion
		#endregion

		#region IEnumerable<EditorFile> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<EditorFile> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<string, EditorFile> file in _files)
			{
				yield return file.Value;
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
			return _files.Values.GetEnumerator();
		}
		#endregion

		#region ICollection Members
		#region Properties.
		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).
		/// </summary>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
		/// </summary>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
		object ICollection.SyncRoot
		{
			get
			{
				return _syncLock;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}
		#endregion
		#endregion
	}
}
