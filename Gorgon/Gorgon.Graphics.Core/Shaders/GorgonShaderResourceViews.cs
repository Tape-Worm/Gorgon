#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 4, 2016 1:05:13 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A list of shader resource views to apply to the pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The shader resource view list is used to bind resources like textures and structured buffers to the GPU pipeline so that shaders can make use of them.
    /// </para>
    /// <para>
    /// If a resource being bound is bound to the <see cref="GorgonGraphics.RenderTargets"/> list, then the render target view will be unbound from the pipeline and rebound as a shader resource. This is
    /// because the render target cannot be used as a shader resource and a render target at the same time.
    /// </para>
    /// </remarks>
    public sealed class GorgonShaderResourceViews
        : IList<GorgonShaderResourceView>, IGorgonReadOnlyArray<GorgonShaderResourceView>
    {
        #region Constants.
        /// <summary>
        /// The maximum number of allowed shader resources that can be bound at the same time.
        /// </summary>
        public const int MaximumShaderResourceViewCount = D3D11.CommonShaderStage.InputResourceSlotCount;
        #endregion

        #region Variables.
        // The last set of dirty items.
        private (int Start, int Count) _dirtyItems;

        // The backing array for this array.
        private GorgonShaderResourceView[] _backingArray = new GorgonShaderResourceView[16];

        // The list of changed indices.
        private readonly List<int> _changedIndices = new List<int>(16);

        // The first index mask (bits 0 - 63).
        private ulong _indexMask1;

        // The second index mask (bits 0 - 63).
        private ulong _indexMask2;

        // Native views.
        private D3D11.ShaderResourceView[] _native = new D3D11.ShaderResourceView[16];
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the native shader resource views.
        /// </summary>
        internal D3D11.ShaderResourceView[] Native => _native;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        int IReadOnlyCollection<GorgonShaderResourceView>.Count => Length;

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        int ICollection<GorgonShaderResourceView>.Count => Length;

        /// <summary>
        /// Property to return the length of the array.
        /// </summary>
        public int Length => MaximumShaderResourceViewCount;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        bool ICollection<GorgonShaderResourceView>.IsReadOnly => false;

        /// <summary>
        /// Property to return whether or not the list is dirty.
        /// </summary>
        public bool IsDirty => _changedIndices.Count > 0;

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        public GorgonShaderResourceView this[int index]
        {
            get
            {
                if (index < _backingArray.Length)
                {
                    return _backingArray[index];
                }

                if (index >= MaximumShaderResourceViewCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return null;
            }
            set
            {
                // Adjust the size of the underlying array if we go out of bounds.
                if (index >= _backingArray.Length)
                {
                    if (index >= MaximumShaderResourceViewCount)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    int newSize = index * 2;
                    newSize = ((newSize + 3) & ~3).Min(MaximumShaderResourceViewCount);
                    Array.Resize(ref _backingArray, newSize);
                    Array.Resize(ref _native, newSize);
                }

                if (value == _backingArray[index])
                {
                    return;
                }

                _backingArray[index] = value;

                ulong mask;
                ref ulong indexMask = ref _indexMask1;

                if (index > 63)
                {
                    mask = (ulong)(1 << (index - 63));
                    indexMask = ref _indexMask2;
                }
                else
                {
                    mask = (ulong)(1 << index);
                }

                if ((indexMask & mask) == mask)
                {
                    return;
                }

                indexMask |= mask;
                _changedIndices.Add(index);
            }
        }

        /// <summary>
        /// Function to reset the value at the specified index, and remove it from the dirty range.
        /// </summary>
        /// <param name="index">The index of the item to reset.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        public void ResetAt(int index)
        {
            if ((index < 0) || (index >= MaximumShaderResourceViewCount))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (index >= _backingArray.Length)
            {
                return;
            }

            _changedIndices.Remove(index);
            _backingArray[index] = null;

            if (index > 63)
            {
                _indexMask2 &= ~((ulong)(1 << (index - 64)));
            }
            else
            {
                _indexMask1 &= ~((ulong)(1 << index));
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform validation on this list prior to applying it.
        /// </summary>
        [Conditional("DEBUG")]
        internal void Validate()
        {
#if DEBUG
            for (int i = 0; i < _backingArray.Length; ++i)
            {
                GorgonShaderResourceView view = _backingArray[i];

                if (view == null)
                {
                    continue;
                }

                GorgonGraphicsResource resource = view.Resource;
                int bindCount = _backingArray.Count(item => item == view);

                if (bindCount > 1)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_VIEW_ALREADY_BOUND, i));
                }

                bindCount = _backingArray.Count(item => ((resource != item.Resource) && (item != view)));

                if (bindCount <= 1)
                {
                    continue;
                }

                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_VIEW_RESOURCE_ALREADY_BOUND, resource.Name, i));
            }
#endif
        }

        /// <summary>
        /// Function to retrieve the dirty items in this list.
        /// </summary>
        /// <param name="peek">[Optional] <b>true</b> if the dirty state should not be modified by calling this method, or <b>false</b> if it should be.</param>
        /// <returns>The starting index and count of the dirty items in the array.</returns>
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
            if (_changedIndices.Count == 0)
            {
                return ref _dirtyItems;
            }

            int minSlot = int.MaxValue;
            int maxSlot = int.MinValue;

            // Find the lowest start value.
            for (int i = 0; i < _changedIndices.Count; ++i)
            {
                minSlot = minSlot.Min(_changedIndices[i]);
                maxSlot = maxSlot.Max(_changedIndices[i]);
            }

            // If we couldn't find a min/max slot, then treat as empty.
            if ((minSlot == int.MaxValue) || (maxSlot == int.MinValue))
            {
                _indexMask1 = _indexMask2 = 0;
                _changedIndices.Clear();
                _dirtyItems = (0, 0);
                return ref _dirtyItems;
            }

            int count = 0;
            // Add values to native array.
            for (int i = minSlot; i <= maxSlot; ++i)
            {
                Native[i - minSlot] = _backingArray[i]?.Native;
                ++count;
            }

            _dirtyItems = (minSlot, count);

            if (!peek)
            {
                _indexMask1 = _indexMask2 = 0;
                _changedIndices.Clear();
            }

            return ref _dirtyItems;
        }

        /// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        void IList<GorgonShaderResourceView>.RemoveAt(int index) => throw new NotSupportedException();

        /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        void ICollection<GorgonShaderResourceView>.Add(GorgonShaderResourceView item) => throw new NotSupportedException();

        /// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        void IList<GorgonShaderResourceView>.Insert(int index, GorgonShaderResourceView item) => throw new NotSupportedException();

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        bool ICollection<GorgonShaderResourceView>.Remove(GorgonShaderResourceView item) => throw new NotSupportedException();

        /// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public int IndexOf(GorgonShaderResourceView item) => Array.IndexOf(_backingArray, item);

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(GorgonShaderResourceView item) => IndexOf(item) != -1;

        /// <summary>
        /// Function to find the index of the resource bound to a shader resource view.
        /// </summary>
        /// <param name="resource">The resource to look up.</param>
        /// <returns>The index of the shader resource view, or -1 if not found.</returns>
        internal int IndexOf(GorgonGraphicsResource resource)
        {
            if (_changedIndices.Count == 0)
            {
                return -1;
            }

            for (int i = 0; i < _changedIndices.Count; ++i)
            {
                int index = _changedIndices[i];
                GorgonGraphicsResource viewResource = _backingArray[index]?.Resource;

                if (viewResource == resource)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(GorgonShaderResourceView[] array, int arrayIndex) => _backingArray.CopyTo(array, arrayIndex);

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="destIndex">[Optional] The destination index in this array to start writing into.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        public void CopyTo(GorgonShaderResourceViews array, int destIndex = 0)
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

            if (destIndex >= MaximumShaderResourceViewCount)
            {
                destIndex = MaximumShaderResourceViewCount - 1;
            }

            int count = array._backingArray.Length.Min(_backingArray.Length);

            for (int j = destIndex, i = 0; j < destIndex + count && i < count; ++i, ++j)
            {
                array[j] = this[i];
            }
        }

        /// <summary>
        /// Function to copy the dirty entries for this array into the specified array.
        /// </summary>
        /// <param name="array">The array that will receive the dirty entries.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b>.</exception>
        public void CopyDirty(GorgonShaderResourceViews array)
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
                array[i] = _backingArray[i];
            }
        }

        /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            Array.Clear(Native, 0, Native.Length);
            Array.Clear(_backingArray, 0, _backingArray.Length);
            _changedIndices.Clear();
            _dirtyItems = (0, 0);
            _indexMask1 = _indexMask2 = 0;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<GorgonShaderResourceView> GetEnumerator()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _backingArray.Length; ++i)
            {
                yield return _backingArray[i];
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() => _backingArray.GetEnumerator();

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(IReadOnlyList<GorgonShaderResourceView> other)
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
                if (_backingArray[i] != other)
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
        public bool DirtyEquals(IGorgonReadOnlyArray<GorgonShaderResourceView> other, int offset = 0)
        {
            if (other == null)
            {
                return false;
            }

            if (other == this)
            {
                return true;
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

            for (int i = _dirtyItems.Start; i < _dirtyItems.Start + _dirtyItems.Count && i + offset < _backingArray.Length; ++i)
            {
                if (_backingArray[i + offset] != other[i])
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
        public bool DirtyEquals(GorgonShaderResourceViews other, int offset = 0)
        {
            if (other == null)
            {
                return false;
            }

            if (other == this)
            {
                return true;
            }

            if (IsDirty)
            {
                // Update the dirty item state on this instance so we can cut down on some checking.
                GetDirtyItems();
            }

            // If the dirty state has already been updated for both arrays, then just check that.
            if ((((_changedIndices.Count != 0) || (other._changedIndices.Count != 0)) ||
                ((_dirtyItems.Start != other._dirtyItems.Start) || (_dirtyItems.Count != other._dirtyItems.Count)))
                && (offset == 0))
            {
                return false;
            }

            for (int i = _dirtyItems.Start; i < _dirtyItems.Start + _dirtyItems.Count && i + offset < _backingArray.Length; ++i)
            {
                if (_backingArray[i + offset] != other[i])
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
        /// Initializes a new instance of the <see cref="GorgonShaderResourceViews"/> class.
        /// </summary>
        internal GorgonShaderResourceViews()
        {
        }
        #endregion
    }
}
