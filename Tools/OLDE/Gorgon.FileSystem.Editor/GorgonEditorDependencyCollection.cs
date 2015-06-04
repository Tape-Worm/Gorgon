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
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of dependencies.
	/// </summary>
	public sealed class GorgonEditorDependencyCollection
		: ICollection<GorgonEditorDependency>, ICollection
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
			private readonly string _name;

			/// <summary>
			/// The type of dependency.
			/// </summary>
			private readonly string _type;
			#endregion

			#region Methods.
			/// <summary>
			/// Function to determine if two instances are equal.
			/// </summary>
			/// <param name="left">Left instance to compare.</param>
			/// <param name="right">Right instance to compare.</param>
			/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
			public static bool Equals(ref DependencyKey left, ref DependencyKey right)
			{
				return ((string.Equals(left._name, right._name, StringComparison.OrdinalIgnoreCase))
				        && (string.Equals(left._type, right._type, StringComparison.OrdinalIgnoreCase)));
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode()
			{
				return 281.GenerateHash(_name).GenerateHash(_type);
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
			/// <returns>
			///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
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
					throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "name");
				}

				if (string.IsNullOrWhiteSpace(type))
				{
					throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "type");
				}

				_name = name;
				_type = type;
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
			/// <param name="x">The first object of type <see cref="DependencyKey"/> to compare.</param>
			/// <param name="y">The second object of type <see cref="DependencyKey"/> to compare.</param>
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
		private readonly Dictionary<DependencyKey, GorgonEditorDependency> _dependencies;			// A list of dependencies to retrieve.
		private readonly static object _syncLock = new object();						// Synchronization object for multiple threads.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return a dependency by name.
		/// </summary>
		/// <remarks>Setting a dependency to NULL (<i>Nothing</i> in VB.Net) will remove it from the collection.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="type"/> parameters are NULL.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or the <paramref name="type"/> parameters are empty.</exception>
        public GorgonEditorDependency this[string name, string type]
		{
			get
			{
				return _dependencies[new DependencyKey(name, type)];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to deserialize a dependency collection from an XML node.
		/// </summary>
		/// <param name="dependenciesNode">The list of nodes that contain the dependencies.</param>
		/// <returns>The deserialized dependencies.</returns>
		internal static GorgonEditorDependencyCollection Deserialize(IEnumerable<XElement> dependenciesNode)
		{
			var result = new GorgonEditorDependencyCollection();

			foreach (GorgonEditorDependency dependency in dependenciesNode.Select(GorgonEditorDependency.Deserialize))
			{
			    result._dependencies[new DependencyKey(dependency.Path, dependency.Type)] = dependency;
			}

			return result;
		}

		/// <summary>
		/// Function to determine if the collection contains a dependency with the specified name and type.
		/// </summary>
		/// <param name="name">Name of the dependency.</param>
		/// <param name="type">Type of the dependency.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="type"/> parameters are NULL.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or the <paramref name="type"/> parameters are empty.</exception>
        public bool Contains(string name, string type)
		{
		    return _dependencies.ContainsKey(new DependencyKey(name, type));
		}

        /// <summary>
        /// Function to retrieve a dependency by its name and type.
        /// </summary>
        /// <param name="name">Name of the dependency.</param>
        /// <param name="type">Type of dependency.</param>
        /// <param name="dependency">The dependency if it exists, NULL (<i>Nothing</i> in VB.Net) if not.</param>
        /// <returns><b>true</b> if the dependency name and type were found, <b>false</b> if not.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="type"/> parameters are NULL.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or the <paramref name="type"/> parameters are empty.</exception>
	    public bool TryGetValue(string name, string type, out GorgonEditorDependency dependency)
        {
            return _dependencies.TryGetValue(new DependencyKey(name, type), out dependency);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEditorDependencyCollection"/> class.
		/// </summary>
		internal GorgonEditorDependencyCollection()
		{
			_dependencies = new Dictionary<DependencyKey, GorgonEditorDependency>(new DependencyComparer());
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
		bool ICollection<GorgonEditorDependency>.IsReadOnly
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
        /// <remarks>This operation is not supported on this collection type and will throw an exception.</remarks>
        /// <exception cref="System.NotSupportedException">This operation is not supported on this collection type.</exception>
        void ICollection<GorgonEditorDependency>.Add(GorgonEditorDependency item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <remarks>This operation is not supported on this collection type and will throw an exception.</remarks>
        /// <exception cref="System.NotSupportedException">This operation is not supported on this collection type.</exception>
        void ICollection<GorgonEditorDependency>.Clear()
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
        /// <remarks>This operation is not supported on this collection type and will throw an exception.</remarks>
        /// <exception cref="System.NotSupportedException">This operation is not supported on this collection type.</exception>
        bool ICollection<GorgonEditorDependency>.Remove(GorgonEditorDependency item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(GorgonEditorDependency item)
		{
			return _dependencies.ContainsValue(item);
		}

		/// <summary>
		/// Function to copy the dependencies to an array.
		/// </summary>
		/// <param name="array">The array to copy into.</param>
		/// <param name="arrayIndex">Index of the destination array to copy into.</param>
		public void CopyTo(GorgonEditorDependency[] array, int arrayIndex)
		{
            _dependencies.Values.CopyTo(array, arrayIndex);
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
		public IEnumerator<GorgonEditorDependency> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<DependencyKey, GorgonEditorDependency> dependencyItem in _dependencies)
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
		void ICollection.CopyTo(Array array, int index)
		{
			CopyTo((GorgonEditorDependency[])array, index);
		}
		#endregion
		#endregion
    }
}
