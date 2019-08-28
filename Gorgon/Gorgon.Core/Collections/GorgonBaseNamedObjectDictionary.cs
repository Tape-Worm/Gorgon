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
// Created: Tuesday, June 14, 2011 10:12:12 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Collections
{
    /// <summary>
    /// Base dictionary for Gorgon library named objects.
    /// </summary>
    /// <typeparam name="T">Type of object, must implement <see cref="IGorgonNamedObject">IGorgonNamedObject</see>.</typeparam>
    /// <remarks>
    /// This is a base class used to help in the creation of custom dictionaries that store objects that implement the <see cref="IGorgonNamedObject"/> interface.
    /// </remarks>
    public abstract class GorgonBaseNamedObjectDictionary<T>
        : IGorgonNamedObjectDictionary<T>, IGorgonNamedObjectReadOnlyDictionary<T>
        where T : IGorgonNamedObject
    {
        #region Variables.
        // Internal collection to hold our objects.
        private readonly Dictionary<string, T> _list;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of items in the underlying collection.
        /// </summary>
        protected IDictionary<string, T> Items => _list;

        /// <summary>
        /// Property to return whether the keys are case sensitive.
        /// </summary>
        public bool KeysAreCaseSensitive
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add several items to the list.
        /// </summary>
        /// <param name="items">IEnumerable containing the items to copy.</param>
        protected void AddItems(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                _list[item.Name] = item;
            }
        }

        /// <summary>
        /// Function to update an item in the list by its name, and optionally, rename the key for that item if necessary.
        /// </summary>
        /// <param name="name">Name of the object to set.</param>
        /// <param name="value">Value to set to the item.</param>
        /// <remarks>
        /// If the item in <paramref name="value"/> has a different name than the name provided, then the object with the name specified will be 
        /// removed, and the new item will be added. This allows the collection to rename the key for an item should its name change.
        /// </remarks>
        protected void UpdateItem(string name, T value)
        {
            if (!string.Equals(name, value.Name, !KeysAreCaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            {
                _list.Remove(name);
            }

            _list[value.Name] = value;
        }

        /// <summary>
        /// Function to remove an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        protected void RemoveItem(T item) => _list.Remove(item.Name);

        /// <summary>
        /// Function to return whether an item with the specified name exists in this collection.
        /// </summary>
        /// <param name="name">Name of the item to find.</param>
        /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        public bool Contains(string name) => _list.ContainsKey(name);

        /// <summary>
        /// Function to return whether the specified object exists in the collection.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        public bool Contains(T value) => _list.ContainsValue(value);

        /// <summary>
        /// Function to return an item from the collection.
        /// </summary>
        /// <param name="name">The name of the item to look up.</param>
        /// <param name="value">The item, if found, or the default value for the type if not.</param>
        /// <returns><b>true</b> if the item was found, <b>false</b> if not.</returns>
        public bool TryGetValue(string name, out T value) => _list.TryGetValue(name, out value);
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBaseNamedObjectDictionary{T}"/> class.
        /// </summary>
        /// <param name="caseSensitive"><b>true</b> if the key names are case sensitive, <b>false</b> if not.</param>
        protected GorgonBaseNamedObjectDictionary(bool caseSensitive)
        {
            _list = new Dictionary<string, T>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            KeysAreCaseSensitive = caseSensitive;
        }
        #endregion

        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (KeyValuePair<string, T> item in _list)
            {
                yield return item.Value;
            }
        }
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (KeyValuePair<string, T> item in _list)
            {
                yield return item.Value;
            }
        }
        #endregion

        #region ICollection<T> Members
        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the dictionary.
        /// </returns>
        public int Count => _list.Count;

        /// <summary>
        /// Property to return whether the collection is read-only or not.
        /// </summary>
        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The object to add to the dictionary.</param>
        void ICollection<T>.Add(T item) => _list.Add(item?.Name ?? string.Empty, item);

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        void ICollection<T>.Clear() => _list.Clear();

        /// <summary>
        /// Copies the elements of the dictionary to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from dictionary. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.
        /// -or-
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// -or-
        /// The number of elements in the source dictionary is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// -or-
        /// Type <paramref name="array"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex) => _list.Values.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the dictionary.
        /// </summary>
        /// <param name="item">The object to remove from the dictionary.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the dictionary; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original dictionary.
        /// </returns>
        bool ICollection<T>.Remove(T item)
        {
            if (!Contains(item))
            {
                return false;
            }

            _list.Remove(item?.Name ?? string.Empty);
            return true;
        }
        #endregion

        #region IGorgonNamedObjectDictionary<T> Members
        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <b>true</b> if this instance is read only; otherwise, <b>false</b>.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsReadOnly => false;

        /// <summary>
        /// Property to set or return an item in the dictionary by its name.
        /// </summary>
        T IGorgonNamedObjectDictionary<T>.this[string name]
        {
            get => _list[name];
            set
            {
                if (!_list.ContainsKey(name))
                {
                    if (value != null)
                    {
                        _list[value.Name] = value;
                    }

                    return;
                }

                if (value == null)
                {
                    _list.Remove(name);
                    return;
                }

                UpdateItem(name, value);
            }
        }

        /// <summary>
        /// Function to remove an item by its name.
        /// </summary>
        /// <param name="name">The name of the object to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no item with the name specified could be found in the dictionary.</exception>
        void IGorgonNamedObjectDictionary<T>.Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (!Contains(name))
            {
                throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_KEY_NOT_FOUND, name));
            }

            _list.Remove(name);
        }
        #endregion

        #region IGorgonNamedObjectReadOnlyDictionary<T> Members
        /// <summary>
        /// Property to set or return an item in the dictionary by its name.
        /// </summary>
        T IGorgonNamedObjectReadOnlyDictionary<T>.this[string name] => _list[name];

        #endregion
    }
}
