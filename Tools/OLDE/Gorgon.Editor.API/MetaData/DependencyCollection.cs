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
// Created: Thursday, February 27, 2014 11:13:48 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// A collection of dependencies.
	/// </summary>
	public class DependencyCollection
		: ICollection<Dependency>, ICollection, ICloneable<DependencyCollection>
	{
		#region Value Types.
		/// <summary>
		/// A key used for dependency look up.
		/// </summary>
		private struct DependencyKey
			: IEquatable<DependencyKey>
		{
			#region Variables.
			/// <summary>
			/// The file for the dependency
			/// </summary>
			public readonly EditorFile File;

			/// <summary>
			/// The type of dependency.
			/// </summary>
			public readonly string Type;
			#endregion

			#region Methods.
			/// <summary>
			/// Function to determine if two instances are equal.
			/// </summary>
			/// <param name="left">Left instance to compare.</param>
			/// <param name="right">Right instance to compare.</param>
			/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
			public static bool Equals(ref DependencyKey left, ref DependencyKey right)
			{
				return ((left.File == right.File)
				        && (string.Equals(left.Type, right.Type, StringComparison.OrdinalIgnoreCase)));
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode()
			{
				return 281.GenerateHash(File).GenerateHash(Type);
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
			/// <returns>
			///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			public override bool Equals(object obj)
			{
				if (obj is DependencyKey)
				{
					return ((DependencyKey)obj).Equals(this);
				}

				return base.Equals(obj);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="DependencyKey"/> struct.
			/// </summary>
			/// <param name="dependency">The dependency to derive a key from.</param>
			public DependencyKey(Dependency dependency)
				: this(dependency.EditorFile, dependency.Type)
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="DependencyKey"/> struct.
			/// </summary>
			/// <param name="file">The editor file.</param>
			/// <param name="type">The type.</param>
			public DependencyKey(EditorFile file, string type)
			{
				if (file == null)
				{
					throw new ArgumentNullException("file");
				}

				if (type == null)
				{
					throw new ArgumentNullException("type");
				}

				if (string.IsNullOrWhiteSpace(type))
				{
					throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "type");
				}

				File = file;
				Type = type;
			}
			#endregion

			#region IEquatable<Dependency> Members
			/// <summary>
			/// Indicates whether the current object is equal to another object of the same type.
			/// </summary>
			/// <param name="other">An object to compare with this object.</param>
			/// <returns>
			/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
			/// </returns>
			public bool Equals(DependencyKey other)
			{
				return Equals(ref this, ref other);
			}
			#endregion
		}
		#endregion

		#region Classes.
		/// <summary>
		/// Key comparer for the dependency list.
		/// </summary>
		private class DependencyComparer
			: IEqualityComparer<DependencyKey>
		{

			#region IEqualityComparer<DependencyKey> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type DependencyKey to compare.</param>
			/// <param name="y">The second object of type DependencyKey to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			public bool Equals(DependencyKey x, DependencyKey y)
			{
				return DependencyKey.Equals(ref x, ref y);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The object.</param>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public int GetHashCode(DependencyKey obj)
			{
				return obj.GetHashCode();
			}
			#endregion
		}
		#endregion

		#region Variables.
		private readonly Dictionary<DependencyKey, Dependency> _dependencies;			// A list of dependencies to retrieve.
		private readonly static object _syncLock = new object();						// Synchronization object for multiple threads.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return a dependency by name.
		/// </summary>
		/// <remarks>Setting a dependency to NULL (Nothing in VB.Net) will remove it from the collection.</remarks>
		public Dependency this[EditorFile file, string type]
		{
			get
			{
				return _dependencies[new DependencyKey(file, type)];
			}
			internal set
			{
				var key = new DependencyKey(file, type);
				bool hasItem = _dependencies.ContainsKey(key);

				if (value == null)
				{
					if (hasItem)
					{
						_dependencies.Remove(key);
					}

					return;
				}

				if ((hasItem) && ((file != value.EditorFile)
				                  || (!string.Equals(type, value.Type, StringComparison.OrdinalIgnoreCase))))
				{
					_dependencies.Remove(key);
				}
				
				_dependencies[key] = value;
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
		/// Function to deserialize a dependency collection from an XML node.
		/// </summary>
		/// <param name="files">Available files to evaluate.</param>
		/// <param name="dependenciesNode">The list of nodes that contain the dependencies.</param>
		/// <returns>The deserialized dependencies.</returns>
		internal static DependencyCollection Deserialize(EditorFileCollection files, IEnumerable<XElement> dependenciesNode)
		{
			var result = new DependencyCollection();

			if (dependenciesNode == null)
			{
				return result;
			}

			foreach (XElement dependencyNode in dependenciesNode)
			{
				Dependency dependency = Dependency.Deserialize(files, dependencyNode);

				// We couldn't find the dependency, skip it.
				if (dependency == null)
				{
					continue;
				}

				result[dependency.EditorFile, dependency.Type] = dependency;
			}

			return result;
		}

		/// <summary>
		/// Function to copy a list of dependencies.
		/// </summary>
		/// <param name="dependencies">Dependencies to copy.</param>
		internal void CopyFrom(DependencyCollection dependencies)
		{
			_dependencies.Clear();

			foreach (Dependency dependency in dependencies)
			{
				var copy = dependency.Clone();
				_dependencies.Add(new DependencyKey(copy), copy);
			}
		}

		/// <summary>
		/// Function to clear the dependency list.
		/// </summary>
		internal void Clear()
		{
			_dependencies.Clear();
		}

		/// <summary>
		/// Function to try and retrieve a dependency with the specified file and type.
		/// </summary>
		/// <param name="file">Editor file for the dependency.</param>
		/// <param name="type">Type of dependency.</param>
		/// <param name="value">The dependency, if found.  NULL (Nothing in VB.Net) if not.</param>
		/// <returns><c>true</c> if found, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> or the <paramref name="type"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="type"/> parameter is empty.</exception>
		public bool TryGetValue(EditorFile file, string type, out Dependency value)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (string.IsNullOrWhiteSpace(type))
			{
				throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "type");
			}

			return _dependencies.TryGetValue(new DependencyKey(file, type), out value);
		}

		/// <summary>
		/// Function to determine if the collection contains a dependency with the specified name and type.
		/// </summary>
		/// <param name="file">The dependency file.</param>
		/// <param name="type">Type of the dependency.</param>
		/// <returns><c>true</c> if found, <c>false</c> if not.</returns>
		public bool Contains(EditorFile file, string type = null)
		{
			return !string.IsNullOrWhiteSpace(type)
				       ? _dependencies.ContainsKey(new DependencyKey(file, type))
				       : this.Any(item => item.EditorFile == file);
		}

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyCollection"/> class.
		/// </summary>
		internal DependencyCollection()
		{
			_dependencies = new Dictionary<DependencyKey, Dependency>(new DependencyComparer());
		}
		#endregion

		#region ICollection<Dependency> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count
		{
			get
			{
				return _dependencies.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<Dependency>.IsReadOnly
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
		void ICollection<Dependency>.Add(Dependency item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			var key = new DependencyKey(item);

			if (Contains(item.EditorFile, key.Type))
			{
				throw new ArgumentException(string.Format(APIResources.GOREDIT_ERR_DEPENDENCY_EXISTS, key.Type, key.File.FilePath));
			}

			_dependencies[key] = item;
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		void ICollection<Dependency>.Clear()
		{
			_dependencies.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(Dependency item)
		{
			return _dependencies.ContainsValue(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		void ICollection<Dependency>.CopyTo(Dependency[] array, int arrayIndex)
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
		bool ICollection<Dependency>.Remove(Dependency item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			return Contains(item.EditorFile, item.Type) && _dependencies.Remove(new DependencyKey(item.EditorFile, item.Type));
		}
		#endregion
		#endregion

		#region IEnumerable<Dependency> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<Dependency> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<DependencyKey, Dependency> dependencyItem in _dependencies)
			{
				yield return dependencyItem.Value;
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
			return _dependencies.Values.GetEnumerator();
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

		#region ICloneable<DependencyCollection> Members
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>
		/// The cloned object.
		/// </returns>
		public DependencyCollection Clone()
		{
			var result = new DependencyCollection();

			result.CopyFrom(this);

			return result;
		}
		#endregion
	}
}
