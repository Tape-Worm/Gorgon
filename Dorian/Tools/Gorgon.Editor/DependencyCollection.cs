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
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A collection of dependencies.
	/// </summary>
	public class DependencyCollection
		: ICollection<Dependency>, ICollection
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
			/// The name for the dependency.
			/// </summary>
			public readonly string Name;

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
			/// <returns>TRUE if equal, FALSE if not.</returns>
			public static bool Equals(ref DependencyKey left, ref DependencyKey right)
			{
				return ((string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase))
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
				return 281.GenerateHash(Name).GenerateHash(Type);
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
				: this(dependency.Path, dependency.Type)
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="DependencyKey"/> struct.
			/// </summary>
			/// <param name="name">The name.</param>
			/// <param name="type">The type.</param>
			public DependencyKey(string name, string type)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}

				if (type == null)
				{
					throw new ArgumentNullException("type");
				}

				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "name");
				}

				if (string.IsNullOrWhiteSpace(type))
				{
					throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "type");
				}

				Name = name;
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
		public Dependency this[string name, string type]
		{
			get
			{
				return _dependencies[new DependencyKey(name, type)];
			}
			set
			{
				var key = new DependencyKey(name, type);
				bool hasItem = _dependencies.ContainsKey(key);

				if (value == null)
				{
					if (hasItem)
					{
						_dependencies.Remove(key);
					}

					return;
				}

				if ((hasItem) && ((!string.Equals(name, value.Path, StringComparison.OrdinalIgnoreCase))
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
		/// <param name="dependenciesNode">The list of nodes that contain the dependencies.</param>
		/// <returns>The deserialized dependencies.</returns>
		internal static DependencyCollection Deserialize(IEnumerable<XElement> dependenciesNode)
		{
			var result = new DependencyCollection();

			foreach (XElement dependencyNode in dependenciesNode)
			{
				Dependency dependency = Dependency.Deserialize(dependencyNode);
				result[dependency.Path, dependency.Type] = dependency;
			}

			return result;
		}

		/// <summary>
		/// Function to determine if the collection contains a dependency with the specified name and type.
		/// </summary>
		/// <param name="name">Name of the dependency.</param>
		/// <param name="type">Type of the dependency.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool Contains(string name, string type = null)
		{
			return !string.IsNullOrWhiteSpace(type)
				       ? _dependencies.ContainsKey(new DependencyKey(name, type))
				       : this.Any(item => string.Equals(name, item.Path, StringComparison.OrdinalIgnoreCase));
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

			if (Contains(key.Name, key.Type))
			{
				throw new ArgumentException(string.Format(Resources.GOREDIT_DEPENDENCY_EXISTS, key.Type, key.Name));
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

			return Contains(item.Path, item.Type) && _dependencies.Remove(new DependencyKey(item.Path, item.Type));
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
	}
}
