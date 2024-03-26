// 
// Gorgon.
// Copyright (C) 2024 Michael Winsor
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

using System.Collections;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Collections;

/// <summary>
/// A special array type that is used to track changes to the indices.
/// </summary>
/// <typeparam name="T">The type of data in the array.</typeparam>
/// <returns>
/// <para>
/// This is a special type of array that can determine when an index has been changed. It does this by comparing the value at the index being accessed, and then marking the index as changed (i.e. "dirty") if 
/// the values are not the same. Users can then retrieve a range of indices which contain the indices that are considered dirty.
/// </para>
/// <para>
/// <note type="Important">
/// <para>
/// Due to how the array determines dirty indices and for performance reasons, the maximum size of the the array is 64 indices.
/// </para>
/// </note>
/// </para>
/// </returns>
public class GorgonArray<T>
    : IList<T>, IGorgonReadOnlyArray<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// The maximum number of indices in an array.
    /// </summary>
    public const int MaxLength = 64;

    // The indices that are dirty.
    private long _dirtyIndices;

    // The starting array index for dirty items in the array.
    private int _dirtyStartIndex = -1;
    // The number of dirty items in the array.
    private int _dirtyIndexCount = -1;

    // The backing store for the array.
    private readonly T[] _backingArray;

    /// <summary>
    /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
    /// </summary>
    int IReadOnlyCollection<T>.Count => Length;

    /// <summary>Gets the number of elements contained in the <see cref="ICollection{T}" />.</summary>
    /// <returns>The number of elements contained in the <see cref="ICollection{T}" />.</returns>
    int ICollection<T>.Count => Length;

    /// <summary>
    /// Property to return the length of the array.
    /// </summary>
    public int Length => _backingArray.Length;

    /// <summary>Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.</summary>
    /// <returns>true if the <see cref="ICollection{T}" /> is read-only; otherwise, false.</returns>
    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// Property to return whether or not the list is dirty.
    /// </summary>
    public bool IsDirty => _dirtyIndices != 0;

    /// <summary>Gets or sets the element at the specified index.</summary>
    /// <returns>The element at the specified index.</returns>       
    public T this[int index]
    {
        get => _backingArray[index];
        set
        {
            if (AreValuesEqual(_backingArray[index], value))
            {
                return;
            }

            _backingArray[index] = value;
            _dirtyIndices |= 1L << index;
        }
    }

    /// <summary>
    /// Property to return a range of values from the array as a span.
    /// </summary>
    public ReadOnlySpan<T> this[Range range] => _backingArray.AsSpan(range);

    /// <summary>
    /// Function to compare two items for equality.
    /// </summary>
    /// <param name="left">The left item to compare.</param>
    /// <param name="right">The right item to compare.</param>
    /// <returns><b>true</b> if the two items are the same, or <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreValuesEqual(T left, T right)
    {
        if ((left is null) && (right is null))
        {
            return true;
        }

        return (right is not null) && (left is not null) && (left.Equals(right));
    }

    /// <summary>
    /// Function to update the starting dirty index, and the count of dirty items.
    /// </summary>
    /// <param name="keepDirtyState"><b>true</b> to keep the dirty state, or <b>false</b> to clear the dirty state.</param>
    /// <returns><b>true</b> if dirty items were found, or <b>false</b> if not.</returns>
    private bool UpdateDirtyIndexAndCount(bool keepDirtyState)
    {
        int startSlot = -1;
        int count = 0;

        _dirtyStartIndex = _dirtyIndexCount = -1;
        long dirtyState = _dirtyIndices;

        for (int i = 0; dirtyState != 0 && i < _backingArray.Length; ++i)
        {
            long dirtyMask = 1L << i;
            bool isDirty = false;

            if (((dirtyState & dirtyMask) == dirtyMask) && (startSlot == -1))
            {
                startSlot = i;
                isDirty = true;
            }

            if (startSlot > -1)
            {
                // We need to notifty on dirty items regardless of whether the mask states that it's dirty or 
                // not. The reason being that in a situation where the list has multiple items that are the same 
                // but some items in between are not, external slots for the in between items will not be updated 
                // correctly. This can lead to an external array being out of sync.
                //
                // For example, if the list has slots 1, 2, and 3 assigned, but slot 2 is not actually dirty (its 
                // value was the same on assignment), it can lead to slots 1 and 3 being updated in the native list, 
                // but slot 2 would not be updated, and this can lead to the wrong value being used in slot 2.

                // Our slots must not be contigious. So, increment the count. If we don't we'll end up 
                // with an incorrect count for our range. For example, if slots 1,2 and 6 were changed, 
                // the count would be 3.  But that is incorrect because slots 1, 2 and 3 would be set, 
                // and 6 would be ignored.  So the best way to handle this is to set slots 1, 2, 3, 4, 
                // 5, and 6, making for a total count of 6.
                //                
                OnMapDirtyItem(i, count++, isDirty);
            }

            // Remove this bit.
            dirtyState &= ~dirtyMask;
        }

        if (!keepDirtyState)
        {
            _dirtyIndices = dirtyState;
        }

        // If we didn't find any starting point (which should be impossible), then return an empty span.
        if (startSlot == -1)
        {
            return false;
        }

        _dirtyStartIndex = startSlot;
        _dirtyIndexCount = count;
        return true;
    }

    /// <summary>
    /// Function called to allow mapping of a dirty item to an external list.
    /// </summary>
    /// <param name="index">The index that is considered dirty in this array.</param>
    /// <param name="rangeIndex">The current index within the dirty range, starting from 0.</param>
    /// <param name="isDirty"><b>true</b> if the item at the index is actually dirty, or <b>false</b> if the item just falls within a dirty range.</param>
    /// <remarks>
    /// <para>
    /// This method is called from the <see cref="GetDirtySpan(bool)"/>, or the <see cref="GetDirtyStartIndexAndCount(bool)"/> methods and is helpful for developers that are using this array to 
    /// synchronize an external list of items, they can override this 
    /// method to update the external list with the item stored at the <paramref name="index"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="rangeIndex"/> parameter is the current contiguous index within the dirty range returned by <see cref="GetDirtySpan(bool)"/>, or <see cref="GetDirtyStartIndexAndCount(bool)"/>. 
    /// This value always starts at 0 and is incremented  on each call to this method. Applications may use this if their external list does not have a 1:1 correlation with the array indices. This value 
    /// can also act as the number of items within the dirty range.
    /// </para>
    /// <para>
    /// Because items within the dirty range are not always dirty, the <paramref name="isDirty"/> flag can be used to filter out only items that have indeed been changed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GetDirtySpan(bool)"/>
    /// <seealso cref="GetDirtyStartIndexAndCount(bool)"/>
    protected virtual void OnMapDirtyItem(int index, int rangeIndex, bool isDirty)
    {
    }

    /// <summary>
    /// Function called when the array is cleared.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called from the <see cref="Clear()"/> method and is helpful for developers that are using this array to synchronize an external list of items they can override this method to 
    /// clear the external list. 
    /// </para>
    /// </remarks>
    /// <seealso cref="Clear"/>
    protected virtual void OnClear()
    {
    }

    /// <summary>
    /// Function called when an item is to be removed from the array.
    /// </summary>
    /// <param name="index">The index of the item being removed.</param>
    /// <remarks>
    /// <para>
    /// This method is called from the <see cref="RemoveAt(int)"/> method and is helpful for developers that are using this array to synchronize an external list of items they can override this method to 
    /// remove an item from the external list. 
    /// </para>
    /// </remarks>
    /// <seealso cref="RemoveAt(int)"/>
    protected virtual void OnRemoveAt(int index)
    {
    }

    /// <summary>
    /// Function called to determine if an index within the array is dirty.
    /// </summary>
    /// <param name="index">The index to evaluate.</param>
    /// <returns><b>true</b> if the index contains dirty data, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to determine if a specific index within the array has been changed (i.e. "dirty"). This will return <b>true</b> for an index with an updated value until the 
    /// <see cref="GetDirtySpan(bool)"/>, or the <see cref="GetDirtyStartIndexAndCount(bool)"/> methods are called (with <b>false</b> as its parameter).
    /// </para>
    /// </remarks>
    /// <seealso cref="GetDirtySpan(bool)"/>
    /// <seealso cref="GetDirtyStartIndexAndCount(bool)"/>
    public bool IsIndexDirty(int index)
    {
        if ((index < 0) || (index >= _backingArray.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_dirtyIndices == 0)
        {
            return false;
        }

        long bitIndex = 1L << index;

        return (_dirtyIndices & bitIndex) == bitIndex;
    }

    /// <summary>
    /// Function to return a read only span for a slice of the array.
    /// </summary>
    /// <param name="start">The starting index for the array.</param>
    /// <param name="count">The number of items to slice.</param>
    /// <returns>The read only span for the array slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan(int start, int count) => _backingArray.AsSpan(start, count);

    /// <summary>
    /// Function to return a read only span for the array.
    /// </summary>
    /// <returns>The read only span for the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan() => _backingArray.AsSpan();

    /// <summary>
    /// Function to return read only memory for a slice of the array.
    /// </summary>
    /// <param name="start">The starting index for the array.</param>
    /// <param name="count">The number of items to slice.</param>
    /// <returns>The read only memory for the array slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<T> AsMemory(int start, int count) => _backingArray.AsMemory(start, count);

    /// <summary>
    /// Function to return read only memory for a slice of the array.
    /// </summary>
    /// <returns>The read only memory for the array slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<T> AsMemory() => _backingArray.AsMemory();

    /// <summary>
    /// Function to retrieve the slice of the array that is considered dirty.
    /// </summary>
    /// <param name="keepDirtyState">[Optional] <b>true</b> if the dirty state should not be modified by calling this method, or <b>false</b> if it should be.</param>
    /// <returns>A read only span slice that contains the first object in the dirty indices, to the very last object in the dirty indices.</returns>
    /// <remarks>
    /// <para>
    /// This will return a <see cref="ReadOnlySpan{T}"/> that contains items that have been changed (i.e. dirty) in this array.
    /// </para>
    /// <para>
    /// When this method is called and the <paramref name="keepDirtyState"/> parameter is <b>false</b>, the internal dirty state is reset and a cached dirty range is returned if the array has not changed since the 
    /// last time it was called. This is done to keep from scanning the array on each call and improve performance in tight loops. If this is not desired, then passing <b>true</b> to the 
    /// <paramref name="keepDirtyState"/> parameter will not reset the internal dirty state and the array will be iterated again.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Since this method will only return a single slice range, some items within the range may not be dirty. For example, if the array has indices 0, 1, 2, 3, 4 and 5 populated, and slots 0 and 3 are changed, the 
    /// slice returned will be contain the objects in indices 0, 1, 2, and 3. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public ReadOnlySpan<T> GetDirtySpan(bool keepDirtyState = false)
    {
        if ((!keepDirtyState) && (_dirtyIndices == 0))
        {
            if ((_dirtyStartIndex == -1) || (_dirtyIndexCount == -1))
            {
                return [];
            }

            return _backingArray.AsSpan(_dirtyStartIndex, _dirtyIndexCount);
        }

        if (!UpdateDirtyIndexAndCount(keepDirtyState))
        {
            return [];
        }

        return _backingArray.AsSpan(_dirtyStartIndex, _dirtyIndexCount);
    }

    /// <summary>
    /// Function to retrieve the starting index of the first dirty entry, and the number of dirty entries in the array.
    /// </summary>
    /// <param name="keepDirtyState">[Optional] <b>true</b> if the dirty state should not be modified by calling this method, or <b>false</b> if it should be.</param>
    /// <returns>A tuple containing the starting array index, and the number of items that are dirty..</returns>
    /// <remarks>
    /// <para>
    /// When this method is called and the <paramref name="keepDirtyState"/> parameter is <b>false</b>, the internal dirty state is reset and a cached dirty range is returned if the array has not changed since the 
    /// last time it was called. This is done to keep from scanning the array on each call and improve performance in tight loops. If this is not desired, then passing <b>true</b> to the 
    /// <paramref name="keepDirtyState"/> parameter will not reset the internal dirty state and the array will be iterated again.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Since this method will only return a single range, some items within the range may not be dirty. For example, if the array has indices 0, 1, 2, 3, 4 and 5 populated, and slots 0 and 3 are changed, the 
    /// range returned will be (start: 0, count: 4) which indicates that items 0, 1, 2 and 3 are considered dirty. 
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public (int StartingIndex, int Count) GetDirtyStartIndexAndCount(bool keepDirtyState = false)
    {
        if ((!keepDirtyState) && (_dirtyIndices == 0))
        {
            if ((_dirtyStartIndex == -1) || (_dirtyIndexCount == -1))
            {
                return (0, 0);
            }

            return (_dirtyStartIndex, _dirtyIndexCount);
        }

        if (!UpdateDirtyIndexAndCount(keepDirtyState))
        {        
            return (0, 0);
        }

        return (_dirtyStartIndex, _dirtyIndexCount);
    }

    /// <summary>
    /// Function to mark the specified array index as dirty.
    /// </summary>
    /// <param name="index">The index to mark as dirty.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    public void MarkDirty(int index)
    {
        if ((index < 0) || (index >= _backingArray.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _dirtyIndices |= 1L << index;
    }

    /// <summary>
    /// Function to mark the specified range of indices as dirty.
    /// </summary>
    /// <param name="range">The range to mark dirty.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="range"/> start/end is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    public void MarkDirty(Range range)
    {
        (int offset, int length) = range.GetOffsetAndLength(_backingArray.Length);

        for (int i = offset; i < offset + length; ++i)
        {
            MarkDirty(i);
        }
    }

    /// <summary>
    /// Function to reset the dirty state for a specific array index.
    /// </summary>
    /// <param name="index">The index of the item to reset.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    public void MarkClean(int index)
    {
        if ((index < 0) || (index >= _backingArray.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _dirtyIndices &= ~(1L << index);

        if (_dirtyIndices == 0)
        {
            _dirtyStartIndex = _dirtyIndexCount = -1;
        }
    }

    /// <summary>
    /// Function to mark the specified range of indices as dirty.
    /// </summary>
    /// <param name="range">The range to mark dirty.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="range"/> start/end is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    public void MarkClean(Range range)
    {
        (int offset, int length) = range.GetOffsetAndLength(_backingArray.Length);

        for (int i = offset; i < offset + length; ++i)
        {
            MarkClean(i);
        }
    }

    /// <summary>
    /// Function to remove an item in the array.
    /// </summary>
    /// <param name="index">The index of the item being removed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// This function will clear the value at the specified <paramref name="index"/> to its default value, and apply a dirty state if the index contained a non default value.
    /// </para>
    /// <para>
    /// This method will call the <see cref="OnRemoveAt(int)"/> method to allow developers an opportunity to synchronize an external list.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// The <see cref="OnRemoveAt(int)"/> method is called prior to actually removing the item from the array, so developers may access the contents of the index safely.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public void RemoveAt(int index)
    {
        if ((index < 0) || (index >= _backingArray.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        OnRemoveAt(index);
        this[index] = default!;
    }

    /// <summary>Adds an item to the <see cref="ICollection{T}" />.</summary>
    /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
    /// <exception cref="NotSupportedException">The <see cref="ICollection{T}" /> is read-only.</exception>
    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    /// <summary>Inserts an item to the <see cref="IList{T}" /> at the specified index.</summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="IList{T}" />.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index" /> is not a valid index in the <see cref="IList{T}" />.</exception>
    /// <exception cref="NotSupportedException">The <see cref="IList{T}" /> is read-only.</exception>
    void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

    /// <summary>Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.</summary>
    /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.</returns>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
    /// <exception cref="NotSupportedException">The <see cref="ICollection{T}" /> is read-only.</exception>
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    /// <summary>Determines the index of a specific item in the <see cref="IList{T}" />.</summary>
    /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
    /// <param name="item">The object to locate in the <see cref="IList{T}" />.</param>
    public int IndexOf(T item) => Array.IndexOf(_backingArray, item);

    /// <summary>Determines whether the <see cref="ICollection{T}" /> contains a specific value.</summary>
    /// <returns>true if <paramref name="item" /> is found in the <see cref="ICollection{T}" />; otherwise, false.</returns>
    /// <param name="item">The object to locate in the <see cref="ICollection{T}" />.</param>
    public bool Contains(T item) => IndexOf(item) != -1;

    /// <summary>
    /// Function to copy the dirty entries for this array into the specified array.
    /// </summary>
    /// <param name="array">The array that will receive the dirty entries.</param>
    public void CopyDirty(GorgonArray<T> array)
    {
        if (_dirtyIndices == 0)
        {
            return;
        }

        int end = array.Length.Min(_backingArray.Length);

        for (int i = 0; i < end; ++i)
        {
            if (!IsIndexDirty(i))
            {
                continue;
            }

            array[i] = _backingArray[i];
        }
    }

    /// <summary>Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="Array" />, starting at a particular <see cref="Array" /> index.</summary>
    /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.</exception>
    /// <exception cref="ArgumentException">The number of elements in the source <see cref="ICollection{T}" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
    public void CopyTo(T[] array, int arrayIndex) => _backingArray.CopyTo(array, arrayIndex);

    /// <summary>Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="Array" />, starting at a particular <see cref="Array" /> index.</summary>
    /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="Array" /> must have zero-based indexing.</param>
    /// <param name="destIndex">[Optional] The destination index in this array to start writing into.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destIndex"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    public void CopyTo(GorgonArray<T> array, int destIndex = 0)
    {
        if (array == this)
        {
            return;
        }

        if ((destIndex < 0) || (destIndex >= array.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(destIndex));
        }

        for (int j = destIndex, i = 0; j < array.Length && i < Length; ++i, ++j)
        {
            array[j] = this[i];
        }
    }

    /// <summary>
    /// Function to remove all items from the array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will reset the array items back to their default values, and clear the internal dirty state. Note that this method does not affect the size of the array.
    /// </para>
    /// <para>
    /// This method calls the <see cref="OnClear"/> method so that developers may override that method and provide their own synchronization with an external list.
    /// </para>
    /// <para>
    /// This method also resets the dirty state for the entire list since there is nothing in the list mark as dirty.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// The <see cref="OnClear"/> method is called prior to actually clearing the array, so developers may access the contents of the array safely.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="OnClear"/>
    public void Clear()
    {
        OnClear();

        Array.Clear(_backingArray, 0, _backingArray.Length);

        _dirtyIndices = 0;
        _dirtyStartIndex = _dirtyIndexCount = -1;
    }

    /// <summary>
    /// Funciton to retrieve an enumerable that will iterate through dirty items only.
    /// </summary>
    /// <returns>The enumerable.</returns>
    public IEnumerable<T> SelectDirty()
    {
        for (int i = 0; i < _backingArray.Length; ++i)
        {
            if (!IsIndexDirty(i))
            {
                continue;
            }

            yield return _backingArray[i];
        }
    }

    /// <summary>
    /// Funciton to retrieve an enumerable that will iterate through clean items only.
    /// </summary>
    /// <returns>The enumerable.</returns>
    public IEnumerable<T> SelectClean()
    {
        for (int i = 0; i < _backingArray.Length; ++i)
        {
            if (IsIndexDirty(i))
            {
                continue;
            }

            yield return _backingArray[i];
        }
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _backingArray.Length; ++i)
        {
            yield return _backingArray[i];
        }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
    /// <filterpriority>2</filterpriority>
    IEnumerator IEnumerable.GetEnumerator() => _backingArray.GetEnumerator();

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonArray{T}"/> class.
    /// </summary>
    /// <param name="maxSize">The maximum size.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxSize"/> is less than 0, or larger than 64.</exception>
    public GorgonArray(int maxSize)
    {
        if (maxSize is < 0 or > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize));
        }

        _backingArray = new T[maxSize];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonArray{T}"/> class.
    /// </summary>
    /// <param name="source">The list of items to copy.</param>
    /// <param name="markDirty">[Optional] <b>true</b> to mark the array items dirty, <b>false</b> to import the items without marking as dirty.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="source"/> size is less than 0, or larger than 64.</exception>
    public GorgonArray(IEnumerable<T> source, bool markDirty = false)
    {
        if (!source.TryGetNonEnumeratedCount(out int count))
        {
            // Fallback just in case the source is not a collection.
            count = source switch
            {
                List<T> list => list.Count,
                T[] array => array.Length,
                IReadOnlyList<T> roList => roList.Count,
                IList<T> rwList => rwList.Count,
                _ => source.Count()
            };
        }

        if (count is < 0 or > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(source));
        }

        _backingArray = new T[count];

        int index = 0;

        foreach (T item in source)
        {
            if (markDirty)
            {
                this[index++] = item;
                continue;
            }

            _backingArray[index++] = item;
        }
    }
}
