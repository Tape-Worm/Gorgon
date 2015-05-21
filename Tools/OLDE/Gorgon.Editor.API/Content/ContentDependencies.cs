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
// Created: Sunday, October 19, 2014 1:44:08 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// A collection of dependency items for content.
	/// </summary>
	public class ContentDependencies
		: ICollection<Dependency>, ICollection
	{
		#region Variables.
		private readonly DependencyCollection _dependencies;						// A list of dependencies to retrieve.
		private readonly Dictionary<Dependency, object> _cachedDependencies;		// A list of cached dependency objects.
		private static readonly object _syncLock = new object();					// Synchronization object.
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
				return _dependencies[file, type];
			}
			set
			{
				_dependencies[file, type] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to try and retrieve a dependency with the specified file and type.
		/// </summary>
		/// <param name="file">Editor file for the dependency.</param>
		/// <param name="type">Type of dependency.</param>
		/// <param name="value">The dependency, if found.  NULL (Nothing in VB.Net) if not.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
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

			return _dependencies.TryGetValue(file, type, out value);
		}

		/// <summary>
		/// Function to remove the dependency with the specified file and type from the list.
		/// </summary>
		/// <param name="file">File associated with the dependency.</param>
		/// <param name="type">Type of dependency.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="file"/> or the <paramref name="type"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="type"/> parameter is empty.</exception>
		public void Remove(EditorFile file, string type)
		{
			DiscardDependencyObject(file, type);
			_dependencies[file, type] = null;
		}

		/// <summary>
		/// Function to determine if the collection contains a dependency with the specified name and type.
		/// </summary>
		/// <param name="file">The dependency file.</param>
		/// <param name="type">Type of the dependency.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool Contains(EditorFile file, string type = null)
		{
			return _dependencies.Contains(file, type);
		}

		/// <summary>
		/// Function to discard a cached dependency object.
		/// </summary>
		/// <param name="file">The file associated with the dependency.</param>
		/// <param name="type">The type of dependency.</param>
		public void DiscardDependencyObject(EditorFile file, string type)
		{
			Dependency dependency;

			if (!TryGetValue(file, type, out dependency))
			{
				return;
			}

			var disposer = _cachedDependencies[dependency] as IDisposable;

			if (disposer != null)
			{
				disposer.Dispose();
			}

			_cachedDependencies.Remove(dependency);
		}

		/// <summary>
		/// Function to return the cached object associated with a dependency object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="dependency">Dependency associated with the cached object.</param>
		/// <returns>The cached dependency object, or the default value for the type if not.</returns>
		public T GetCachedDependencyObject<T>(Dependency dependency)
		{
			object value;

			_cachedDependencies.TryGetValue(dependency, out value);

			if (value == null)
			{
				return default(T);
			}

			return (T)value;
		}

		/// <summary>
		/// Function to cache a dependency object for a dependency with the specified file and type.
		/// </summary>
		/// <param name="file">The file associated with the dependency.</param>
		/// <param name="type">The type of dependency.</param>
		/// <param name="dependencyObj">Dependency object to cache.</param>
		public Dependency CacheDependencyObject(EditorFile file, string type, object dependencyObj)
		{
			Dependency dependency;

			if (!TryGetValue(file, type, out dependency))
			{
				dependency = _dependencies[file, type] = new Dependency(file, type);
			}

			object prevCached;

			if (_cachedDependencies.TryGetValue(dependency, out prevCached))
			{
				var disposer = prevCached as IDisposable;

				if (disposer != null)
				{
					disposer.Dispose();
				}
			}

			if (dependencyObj != null)
			{
				_cachedDependencies[dependency] = dependencyObj;	
			}

			return dependency;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentDependencies"/> class.
		/// </summary>
		internal ContentDependencies()
		{
			_dependencies = new DependencyCollection();
			_cachedDependencies = new Dictionary<Dependency, object>();
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
		public void Add(Dependency item)
		{
			((ICollection<Dependency>)_dependencies).Add(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			var disposers = from cachedObject in _cachedDependencies
			                let disposer = cachedObject.Value as IDisposable
			                where disposer != null
			                select disposer;

			foreach (IDisposable disposer in disposers)
			{
				disposer.Dispose();
			}

			_cachedDependencies.Clear();
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
			return _dependencies.Contains(item);
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
		public bool Remove(Dependency item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			Remove(item.EditorFile, item.Type);
			return true;
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
			return _dependencies.GetEnumerator();
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
			return ((IEnumerable)_dependencies).GetEnumerator();
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
