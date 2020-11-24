#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 7, 2015 12:28:22 PM
// 
#endregion

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Gorgon.Collections.Specialized
{
    /// <summary>
    /// A custom concurrent dictionary that supplements the original <see cref="ConcurrentDictionary{TKey,TValue}"/> type by supplying a read-only interface.
    /// </summary>
    /// <typeparam name="TK">The key type for the dictionary.</typeparam>
    /// <typeparam name="TV">The value type for the dictionary.</typeparam>
    /// <remarks>
    /// This type is the same as the <see cref="ConcurrentDictionary{TKey,TValue}"/> type, with the only difference being that it supports the <see cref="IReadOnlyDictionary{TKey,TValue}"/> 
    /// interface. See the documentation on <see cref="ConcurrentDictionary{TKey,TValue}"/> for more information.
    /// </remarks>
    public class GorgonConcurrentDictionary<TK, TV>
        : ConcurrentDictionary<TK, TV>, IReadOnlyDictionary<TK, TV>
    {
        #region Constructors.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        public GorgonConcurrentDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        /// <param name="collection">The collection used to populate the dictionary.</param>
        public GorgonConcurrentDictionary(IEnumerable<KeyValuePair<TK, TV>> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        /// <param name="collection">The collection used to populate the dictionary.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when looking up keys in the dictionary.</param>
        public GorgonConcurrentDictionary(IEnumerable<KeyValuePair<TK, TV>> collection, IEqualityComparer<TK> comparer)
            : base(collection, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the dictionary concurrently.</param>
        /// <param name="collection">The collection used to populate the dictionary.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when looking up keys in the dictionary.</param>
        public GorgonConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TK, TV>> collection, IEqualityComparer<TK> comparer)
            : base(concurrencyLevel, collection, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the dictionary concurrently.</param>
        /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
        public GorgonConcurrentDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the dictionary concurrently.</param>
        /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when looking up keys in the dictionary.</param>
        public GorgonConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TK> comparer)
            : base(concurrencyLevel, capacity, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConcurrentDictionary{TK, TV}"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when looking up keys in the dictionary.</param>
        public GorgonConcurrentDictionary(IEqualityComparer<TK> comparer)
            : base(comparer)
        {
        }
        #endregion

        #region IReadOnlyDictionary<TK,TV> Members
        /// <summary>
        /// Gets a collection containing the keys in the <see cref="Dictionary{Tk, Tv}" />.
        /// </summary>
        public new IEnumerable<TK> Keys => base.Keys;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="Dictionary{Tk, Tv}" />.
        /// </summary>
        public new IEnumerable<TV> Values => base.Values;

        #endregion
    }
}
