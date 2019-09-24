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
// Created: June 11, 2016 7:26:28 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Memory
{
    /// <summary>
    /// A memory allocation strategy that uses a standar pool/free list allocator to perform memory allocations.
    /// </summary>
    /// <typeparam name="T">The type of objects to allocate.  These must be a reference type.</typeparam>
    /// <remarks>
    /// <para>
    /// While the .NET memory manager is quite fast (e.g. <c>new</c>), and is useful for most cases, it does have the problem of creating garbage. When these items are created and discarded, 
    /// the garbage collector may kick in at any given time, causing performance issues during time critical code (e.g. a rendering loop). By allocating a large pool of objects and then drawing 
    /// directly from this pool, we can reuse existing, but expired objects to ensure that the garbage collector does not collect these items until we are truly done with them.
    /// </para>
    /// <para>
    /// This type will allocate a new object on demand (and optionally initialize that new object).  When the user is done with the object, they can deallocate it and it will be put back into a free list, 
    /// which allows us to reuse an object as needed.  This puts the responsibility on the user to ensure they deallocate objects they are using, otherwise the benefits provided by this allocator are 
    /// nullified.  
    /// </para>
    /// <para>
    /// This allocator will never grow beyond its initial size. So care must be taken ahead of time to ensure the pool is large enough.
    /// </para>
    /// </remarks>
	public class GorgonPool<T>
        : IGorgonAllocator<T>
        where T : class
    {
        #region Variables.
        // The list of items that are free for use.
        private readonly ConcurrentStack<T> _freeList;
        // The number of available slots.
        private int _availableSlots;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the allocator to use when creating new instances of an object.
        /// </summary>
        protected Func<T> ItemAllocator
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of items available to the allocator.
        /// </summary>
        public int TotalSize
        {
            get;
        }

        /// <summary>
        /// Property to return the number of available slots in the pool.
        /// </summary>
        /// <remarks>
        /// When this value is 0, then the pool is full and should be reset using the <see cref="Reset"/> method.
        /// </remarks>
        /// <seealso cref="Reset"/>
        public int AvailableSlots => _availableSlots;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to allocate a new object from the pool.
        /// </summary>
        /// <param name="initializer">[Optional] A function used to initialize the object returned by the allocator.</param>
        /// <exception cref="GorgonException">Thrown when the pool is completely full.</exception>
        /// <returns>The newly allocated object.</returns>
        /// <remarks>
        /// <para>
        /// Applications should check to ensure that there is enough free space in the pool to allocate another object by checking the <see cref="AvailableSlots"/> property prior to calling this method. 
        /// If there is no more room (i.e. <see cref="AvailableSlots"/> is 0), then applications should call the <see cref="Reset"/> method to reset the pool.
        /// </para>
        /// <para>
        /// If the <paramref name="initializer"/> parameter is supplied, then this callback method can be used to initialize the new object before returning it from the allocator. If the object returned 
        /// is <b>null</b> (because an allocator was not supplied to the constructor), then this parameter will be ignored.
        /// </para>
        /// </remarks>
        /// <seealso cref="AvailableSlots"/>
        /// <seealso cref="Reset"/>
        public T Allocate(Action<T> initializer = null)
        {
            if (Interlocked.Decrement(ref _availableSlots) <= 0)
            {
                throw new GorgonException(GorgonResult.OutOfMemory, Resources.GOR_ERR_ALLOCATOR_FULL);
            }

            if ((_freeList.Count == 0)
                || (!_freeList.TryPop(out T item)))
            {
                item = ItemAllocator();
            }

            initializer?.Invoke(item);

            return item;
        }

        /// <summary>
        /// Function to deallocate an item allocated by this pool.
        /// </summary>
        /// <param name="item">The item to deallocate.</param>
        /// <param name="finalizer">[Optional] A finalizer callback method used to clean up the object before putting it back in the pool.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="item"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown when there is no more room to add an item to the free list.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="item"/> parameter will be passed by reference to the method so that it will be set to <b>null</b> upon deallocation. This keeps the end user from using the object after it's 
        /// been deallocated.
        /// </para>
        /// <para>
        /// If the <paramref name="finalizer"/> callback method is supplied, then the object will have a chance to be cleaned up prior to putting it back into the pool. 
        /// </para>
        /// </remarks>
	    public void Deallocate(ref T item, Action<T> finalizer = null)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Interlocked.Increment(ref _availableSlots) > TotalSize)
            {
                throw new GorgonException(GorgonResult.OutOfMemory, Resources.GOR_ERR_ALLOCATOR_FULL);
            }

            T localItem = Interlocked.Exchange(ref item, null);

            if (localItem == null)
            {
                return;
            }

            finalizer?.Invoke(localItem);
            _freeList.Push(localItem);
        }

        /// <summary>
        /// Function to reset the allocator heap and "free" all previous instances.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method does not actually free any memory in the traditional sense, but merely resets the allocation pointer back to the beginning of the heap 
        /// to allow re-use of objects.
        /// </para>
        /// </remarks>
        public void Reset()
        {
            _freeList.Clear();
            Interlocked.Exchange(ref _availableSlots, TotalSize);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonLinearPool{T}"/> class.
        /// </summary>
        /// <param name="objectCount">The number of total objects available to the allocator.</param>
        /// <param name="allocator">The allocator used to create an object in the pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="objectCount"/> parameter is less than 1.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="allocator"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="allocator"/> callback method passed will be executed for each object that has not been initialized in the pool (i.e. the object would be returned as <b>null</b> when 
        /// allocated). This ensures that the allocator can correctly initialize the object prior to allocation in the pool. 
        /// </para>
        /// </remarks>
        public GorgonPool(int objectCount, Func<T> allocator)
        {
            if (objectCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(objectCount), Resources.GOR_ERR_ALLOCATOR_SIZE_TOO_SMALL);
            }

            ItemAllocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
            _availableSlots = TotalSize = objectCount;
            _freeList = new ConcurrentStack<T>();
        }
        #endregion
    }
}
