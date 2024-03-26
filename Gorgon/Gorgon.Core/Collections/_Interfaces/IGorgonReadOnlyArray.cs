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
// Created: May 23, 2018 12:55:38 PM
// 

namespace Gorgon.Collections;

/// <summary>
/// A read only interface for a <see cref="GorgonArray{T}"/>.
/// </summary>
/// <typeparam name="T">The type of data in the array.</typeparam>
public interface IGorgonReadOnlyArray<T>
    : IReadOnlyList<T>
    where T : IEquatable<T>?
{
    /// <summary>
    /// Property to return the length of the array.
    /// </summary>
    int Length
    {
        get;
    }

    /// <summary>
    /// Property to return whether the array is in a dirty state.
    /// </summary>
    bool IsDirty
    {
        get;
    }

    /// <summary>
    /// Property to return a range of values from the array as a span.
    /// </summary>
    ReadOnlySpan<T> this[Range range]
    {
        get;
    }

    /// <summary>
    /// Function to return a read only span for a slice of the array.
    /// </summary>
    /// <param name="start">The starting index for the array.</param>
    /// <param name="count">The number of items to slice.</param>
    /// <returns>The read only span for the array slice.</returns>
    ReadOnlySpan<T> AsSpan(int start, int count);

    /// <summary>
    /// Function to return a read only span for the array.
    /// </summary>
    /// <returns>The read only span for the array.</returns>
    ReadOnlySpan<T> AsSpan();

    /// <summary>
    /// Function to return read only memory for a slice of the array.
    /// </summary>
    /// <param name="start">The starting index for the array.</param>
    /// <param name="count">The number of items to slice.</param>
    /// <returns>The read only memory for the array slice.</returns>
    ReadOnlyMemory<T> AsMemory(int start, int count);

    /// <summary>
    /// Function to return read only memory for a slice of the array.
    /// </summary>
    /// <returns>The read only memory for the array slice.</returns>
    ReadOnlyMemory<T> AsMemory();

    /// <summary>
    /// Function called to determine if an index within the array is dirty.
    /// </summary>
    /// <param name="index">The index to evaluate.</param>
    /// <returns><b>true</b> if the index contains dirty data, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to determine if a specific index within the array has been changed (i.e. "dirty"). This will return <b>true</b> for an index with an updated value until the 
    /// <see cref="GetDirtySpan(bool)"/>, or the <see cref="GetDirtyStartIndexAndCount(bool)"/> methods are called (with <b>false</b> as its parameter).
    /// </para>
    /// </remarks>
    /// <seealso cref="GetDirtySpan(bool)"/>
    /// <seealso cref="GetDirtyStartIndexAndCount(bool)"/>
    bool IsIndexDirty(int index);

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
    ReadOnlySpan<T> GetDirtySpan(bool keepDirtyState = false);

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
    (int StartingIndex, int Count) GetDirtyStartIndexAndCount(bool keepDirtyState = false);

    /// <summary>
    /// Funciton to retrieve an enumerable that will iterate through dirty items only.
    /// </summary>
    /// <returns>The enumerable.</returns>
    IEnumerable<T> SelectDirty();

    /// <summary>
    /// Funciton to retrieve an enumerable that will iterate through clean items only.
    /// </summary>
    /// <returns>The enumerable.</returns>
    IEnumerable<T> SelectClean();
}
