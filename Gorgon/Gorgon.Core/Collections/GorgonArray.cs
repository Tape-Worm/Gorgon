#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: April 2, 2017 2:04:59 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Gorgon.Math;

namespace Gorgon.Collections
{
    /// <summary>
    /// A special array type that is used to monitor and track changes to itself.
    /// </summary>
    /// <typeparam name="T">The type of data in the array.</typeparam>
    /// <returns>
    /// <para>
    /// Due to how the array determines dirty indices, the maximum size of the the array is 64 items.
    /// </para>
    /// </returns>
    public class GorgonArray<T>
        : IList<T>, IGorgonReadOnlyArray<T>
        where T : IEquatable<T>
    {
        #region Variables.
        // The indices that are dirty.
        private int _dirtyIndices;

        // The last set of dirty items.
        private (int Start, int Count) _dirtyItems;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the backing store to objects that need it.
        /// </summary>
        protected T[] BackingArray
        {
            get;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        int IReadOnlyCollection<T>.Count => Length;

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        int ICollection<T>.Count => Length;

        /// <summary>
        /// Property to return the length of the array.
        /// </summary>
	    public int Length => BackingArray.Length;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Property to return whether or not the list is dirty.
        /// </summary>
        public bool IsDirty => _dirtyIndices != 0;

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get => BackingArray[index];
            set
            {
                if (((value == null) && (BackingArray[index] == null))
                    || ((BackingArray[index] != null) && (value != null) && (value.Equals(BackingArray[index]))))
                {
                    return;
                }

                BackingArray[index] = value;
                _dirtyIndices |= 1 << index;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a dirty item is assigned.
        /// </summary>
        /// <param name="dirtyIndex">The index that is considered dirty.</param>
        /// <param name="value">The dirty value.</param>
	    protected virtual void OnAssignDirtyItem(int dirtyIndex, T value)
        {
        }

        /// <summary>
        /// Function called when an item is reset at an index.
        /// </summary>
        /// <param name="index">The index of the item that was assigned.</param>
        /// <param name="oldItem">The previous item in the slot.</param>
        protected virtual void OnItemReset(int index, T oldItem)
        {
        }

        /// <summary>
        /// Function called when the array is cleared.
        /// </summary>
        protected virtual void OnClear()
        {
        }

        /// <summary>
        /// Function to validate an item being assigned to a slot.
        /// </summary>
        protected virtual void OnValidate()
        {
        }

        /// <summary>
        /// Function to perform validation on this list prior to applying it.
        /// </summary>
        [Conditional("DEBUG")]
        internal void Validate()
#if DEBUG
         => OnValidate();
#else
        {
        }
#endif


        /// <summary>
        /// Function to retrieve the dirty items in this list.
        /// </summary>
        /// <param name="peek">[Optional] <b>true</b> if the dirty state should not be modified by calling this method, or <b>false</b> if it should be.</param>
        /// <returns>A tuple containing the starting index and the number of items.</returns>
        /// <remarks>
        /// <para>
        /// This will return a tuple that contains the start index, and count of the items that have been changed in this collection.  
        /// </para>
        /// <para>
        /// If the <paramref name="peek"/> parameter is set to <b>true</b>, then the state of this collection is not changed when retrieving the modified objects. Otherwise, the state will be reset and a 
        /// subsequent call to this method will result in a tuple that does not contain any changed values (i.e. the start and count will be 0) until the collection is modified again.
        /// </para>
        /// </remarks>
        public ref readonly (int Start, int Count) GetDirtyItems(bool peek = false)
        {
            int startSlot = -1;
            int count = 0;

            if (_dirtyIndices == 0)
            {
                return ref _dirtyItems;
            }

            int dirtyState = _dirtyIndices;
            int dirtyIndex = 0;

            for (int i = 0; dirtyState != 0 && i < BackingArray.Length; ++i)
            {
                int dirtyMask = 1 << i;

                if (((dirtyState & dirtyMask) == dirtyMask) && (startSlot == -1))
                {
                    startSlot = i;
                }

                if (startSlot > -1)
                {
                    // We need to re-assign dirty items regardless of whether the mask states that it's dirty or 
                    // not. The reason being that in a situation where the list has multiple items that are the same 
                    // but some items in between are not, the native slots for the in between items will not be updated 
                    // correctly. This can lead to the native array being out of sync.
                    //
                    // For example, if the list has slots 1, 2, and 3 assigned, but slot 2 is not actually dirty (its 
                    // value was the same on assignment), it can lead to slots 1 and 3 being updated in the native list, 
                    // but slot 2 would not be updated, and this can lead to the wrong value being used in slot 2.
                    OnAssignDirtyItem(dirtyIndex++, BackingArray[i]);

                    // Our slots must not be contigious. So, increment the count. If we don't we'll end up 
                    // with an incorrect count for our range. For example, if slots 1,2 and 6 were changed, 
                    // the count would be 3.  But that is incorrect because slots 1, 2 and 3 would be set, 
                    // and 6 would be ignored.  So the best way to handle this is to set slots 1, 2, 3, 4, 
                    // 5, and 6, making for a total count of 6.
                    //
                    ++count;
                }

                // Remove this bit.
                dirtyState &= ~dirtyMask;
            }

            if (!peek)
            {
                _dirtyIndices = dirtyState;
            }

            _dirtyItems = (startSlot == -1 ? 0 : startSlot, count);
            return ref _dirtyItems;
        }

        /// <summary>
        /// Function to reset the value at the specified index, and remove it from the dirty range.
        /// </summary>
        /// <param name="index">The index of the item to reset.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
	    public void ResetAt(int index)
        {
            if ((index < 0) || (index >= BackingArray.Length))
            {
                throw new ArgumentOutOfRangeException();
            }

            T oldValue = BackingArray[index];
            BackingArray[index] = default;
            _dirtyIndices &= ~(1 << index);

            OnItemReset(index, oldValue);
        }

        /// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        void IList<T>.RemoveAt(int index) => ResetAt(index);

        /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        /// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        /// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public int IndexOf(T item) => Array.IndexOf(BackingArray, item);

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(T item) => IndexOf(item) != -1;

        /// <summary>
        /// Function to copy the dirty entries for this array into the specified array.
        /// </summary>
        /// <param name="array">The array that will receive the dirty entries.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b>.</exception>
	    public void CopyDirty(GorgonArray<T> array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // Find all the dirty entries (if we haven't already).
            if (_dirtyItems.Count == 0)
            {
                GetDirtyItems(true);
            }

            int end = (_dirtyItems.Count + _dirtyItems.Start).Min(array.Length);

            for (int i = _dirtyItems.Start; i < end; ++i)
            {
                array[i] = BackingArray[i];
            }
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex) => BackingArray.CopyTo(array, arrayIndex);

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="destIndex">[Optional] The destination index in this array to start writing into.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        public void CopyTo(GorgonArray<T> array, int destIndex = 0)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array == this)
            {
                return;
            }

            if (destIndex < 0)
            {
                destIndex = 0;
            }

            if (destIndex >= array.Length)
            {
                destIndex = array.Length - 1;
            }

            for (int j = destIndex, i = 0; j < array.Length && i < Length; ++i, ++j)
            {
                array[j] = this[i];
            }
        }

        /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            OnClear();

            Array.Clear(BackingArray, 0, BackingArray.Length);

            _dirtyIndices = 0;
            _dirtyItems = (0, 0);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < BackingArray.Length; ++i)
            {
                yield return BackingArray[i];
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() => BackingArray.GetEnumerator();

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(IReadOnlyList<T> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Count != Length)
            {
                return false;
            }

            if (other == this)
            {
                return true;
            }

            for (int i = 0; i < Length; ++i)
            {
                T left = this[i];
                T right = other[i];

                if ((left == null) && (right == null))
                {
                    continue;
                }

                if ((left == null) || (right == null) || (!left.Equals(right)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="offset">[Optional] The offset in this array to start comparing from.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool DirtyEquals(IGorgonReadOnlyArray<T> other, int offset = 0)
        {
            if (other == null)
            {
                return false;
            }

            if (IsDirty)
            {
                // Update the dirty item state on this instance so we can cut down on some checking.
                GetDirtyItems();
            }

            ref readonly (int otherStart, int otherCount) otherRange = ref other.GetDirtyItems();

            // If the dirty state has already been updated for both arrays, then just check that.
            if (((_dirtyItems.Start != otherRange.otherStart) || (_dirtyItems.Count != otherRange.otherCount)) && (offset == 0))
            {
                return false;
            }

            for (int i = _dirtyItems.Start; i < _dirtyItems.Start + _dirtyItems.Count && i + offset < BackingArray.Length; ++i)
            {
                T thisItem = BackingArray[i + offset];
                T otherItem = other[i];

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if ((thisItem == null) && (otherItem == null))
                {
                    continue;
                }

                if (thisItem == null)
                {
                    return false;
                }

                if (!thisItem.Equals(otherItem))
                {
                    return false;
                }
            }

            return true;

            // We have different dirty states, so this array is different than the other one.
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="offset">[Optional] The offset in this array to start comparing from.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool DirtyEquals(GorgonArray<T> other, int offset = 0)
        {
            if (other == null)
            {
                return false;
            }

            if (IsDirty)
            {
                // Update the dirty item state on this instance so we can cut down on some checking.
                GetDirtyItems();
            }

            // If the dirty state has already been updated for both arrays, then just check that.
            if (((_dirtyIndices != 0) || (other._dirtyIndices != 0) || (_dirtyItems.Start != other._dirtyItems.Start) || (_dirtyItems.Count != other._dirtyItems.Count)) && (offset == 0))
            {
                return false;
            }

            for (int i = _dirtyItems.Start; i < _dirtyItems.Start + _dirtyItems.Count && i + offset < BackingArray.Length; ++i)
            {
                T thisItem = BackingArray[i + offset];
                T otherItem = other[i];

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if ((thisItem == null) && (otherItem == null))
                {
                    continue;
                }

                if (thisItem == null)
                {
                    return false;
                }

                if (!thisItem.Equals(otherItem))
                {
                    return false;
                }
            }

            return true;

            // We have different dirty states, so this array is different than the other one.
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonArray{T}"/> class.
        /// </summary>
        /// <param name="maxSize">The maximum size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxSize"/> is less than 1.</exception>
        public GorgonArray(int maxSize)
        {
            if ((maxSize < 1) || (maxSize > 64))
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize));
            }

            BackingArray = new T[maxSize];
            _dirtyItems = (0, 0);
        }
        #endregion
    }
}
