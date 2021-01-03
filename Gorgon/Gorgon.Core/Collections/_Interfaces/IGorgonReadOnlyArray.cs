#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Collections
{
    /// <summary>
    /// Defines an array that is read only.
    /// </summary>
    /// <typeparam name="T">The type of data in the array.</typeparam>
    public interface IGorgonReadOnlyArray<T>
        : IReadOnlyList<T>, IEquatable<IReadOnlyList<T>>
        where T : IEquatable<T>
    {
        #region Properties.
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
        #endregion

        #region Methods.
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
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="offset">[Optional] The offset in this array to start comparing from.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        bool DirtyEquals(IGorgonReadOnlyArray<T> other, int offset = 0);

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
        ref readonly (int Start, int Count) GetDirtyItems(bool peek = false);
        #endregion
    }
}
