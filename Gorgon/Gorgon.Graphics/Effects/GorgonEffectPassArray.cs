#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, August 19, 2013 11:51:21 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Math;

namespace Gorgon.Graphics
{
    /// <summary>
    /// An array of effect passes.
    /// </summary>
    public class GorgonEffectPassArray
        : IList<GorgonEffectPass>
    {
        #region Variables.
        private readonly GorgonEffectPass[] _passes;
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonEffectPassArray"/> class.
        /// </summary>
        /// <param name="effect">Effect that owns the passes in this list.</param>
        /// <param name="passCount">The pass count.</param>
        internal GorgonEffectPassArray(GorgonEffect effect, int passCount)
        {
            _passes = new GorgonEffectPass[passCount.Max(1)];

            for (int i = 0; i < _passes.Length; i++)
            {
                _passes[i] = new GorgonEffectPass(effect, i);
            }
        }
        #endregion

        #region IList<GorgonEffectPass> Members
        #region Properties.
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public GorgonEffectPass this[int index]
        {
            get
            {
                return _passes[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(GorgonEffectPass item)
        {
            return Array.IndexOf(_passes, item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="System.NotSupportedException">This method is not supported for this type.</exception>
        void IList<GorgonEffectPass>.Insert(int index, GorgonEffectPass item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException">This method is not supported for this type.</exception>
        void IList<GorgonEffectPass>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        #endregion

        #endregion

        #region ICollection<GorgonEffectPass> Members
        #region Properties.
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count => _passes.Length;

	    /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly => true;

	    #endregion

        #region Methods.
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.NotSupportedException">This method is not supported by this type.</exception>
        void ICollection<GorgonEffectPass>.Add(GorgonEffectPass item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.NotSupportedException">This method is not supported by this type.</exception>
        void ICollection<GorgonEffectPass>.Clear()
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
        public bool Contains(GorgonEffectPass item)
        {
            return Array.IndexOf(_passes, item) > -1;
        }

        /// <summary>
        /// Copies the automatic.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(GorgonEffectPass[] array, int arrayIndex)
        {
            _passes.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException">This method is not supported by this type.</exception>
        bool ICollection<GorgonEffectPass>.Remove(GorgonEffectPass item)
        {
            throw new NotSupportedException();
        }
        #endregion
        #endregion

        #region IEnumerable<GorgonEffectPass> Members
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<GorgonEffectPass> GetEnumerator()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _passes.Length; i++)
            {
                yield return _passes[i];
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
            return _passes.GetEnumerator();
        }
        #endregion
    }
}
