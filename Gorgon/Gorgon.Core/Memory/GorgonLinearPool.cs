// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 11, 2016 7:26:28 PM
// 

using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Memory;

/// <summary>
/// A memory allocation strategy that uses a linear allocator to perform memory allocations
/// </summary>
/// <typeparam name="T">The type of objects to allocate. These must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// While the .NET memory manager is quite fast (e.g. <c>new</c>), and is useful for most cases, it does have the problem of creating garbage. When these items are created and discarded, 
/// the garbage collector may kick in at any given time, causing performance issues during time critical code (e.g. a rendering loop). By allocating a large pool of objects and then drawing 
/// directly from this pool, we can reuse existing, but expired objects to ensure that the garbage collector does not collect these items until we are truly done with them
/// </para>
/// <para>
/// This allocator will never grow beyond its initial size. So care must be taken ahead of time to ensure the pool is large enough
/// </para>
/// </remarks>
public class GorgonLinearPool<T>
    : IGorgonAllocator<T>
    where T : class
{
    // The most current item in the heap.
    private int _currentItem = -1;
    // The items in the pool.
    private readonly T[] _items;

    /// <summary>
    /// Property to return the allocator function to use when creating new instances of an object.
    /// </summary>
    protected Func<T> ItemAllocator
    {
        get;
    }

    /// <summary>
    /// Property to return the number of items available in the pool.
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
    public int AvailableSlots => TotalSize - (_currentItem + 1);

    /// <summary>
    /// Function to allocate a new object from the pool.
    /// </summary>
    /// <param name="initializer">[Optional] A function used to initialize the object returned by the allocator.</param>
    /// <returns>A reference to the object in the pool.</returns>
    /// <exception cref="GorgonException">Thrown when the pool is completely full.
    /// <para>-or-</para>
    /// <para>The object could not be created with the <see cref="ItemAllocator"/> and returned <b>null</b>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method returns either a new, or reused object from the pool. The <see cref="ItemAllocator"/> is used to create a new object in the pool if one has never been used before. Otherwise, if the 
    /// object is reused, then the existing object will be returned from the pool.
    /// </para>
    /// <para>
    /// Applications should check to ensure that there is enough free space in the pool to allocate another object by checking the <see cref="AvailableSlots"/> property prior to calling this method. 
    /// If there is no more room (i.e. <see cref="AvailableSlots"/> is 0), then applications should call the <see cref="Reset"/> method to reset the pool.
    /// </para>
    /// <para>
    /// If the <paramref name="initializer"/> parameter is supplied, then this callback method can be used to initialize the new object before returning it from the allocator.
    /// </para>
    /// </remarks>
    /// <seealso cref="Reset"/>
    public T Allocate(Action<T>? initializer = null)
    {
        int nextIndex = Interlocked.Increment(ref _currentItem);

        if (nextIndex >= _items.Length)
        {
            throw new GorgonException(GorgonResult.OutOfMemory, Resources.GOR_ERR_ALLOCATOR_FULL);
        }

        T? item = _items[nextIndex];

        if (item is null)
        {
            _items[nextIndex] = item ??= ItemAllocator() ?? throw new GorgonException(GorgonResult.CannotCreate, Resources.GOR_ERR_OBJECT_CREATION_RETURNED_NULL);
        }        

        initializer?.Invoke(item);

        return _items[nextIndex];
    }

    /// <summary>
    /// Function to reset the allocator heap and "free" all previous instances.
    /// </summary>
    /// <param name="finalizer">[Optional] A callback method used to perform any required clean up for the object being reset in the pool.</param>
    /// <remarks>
    /// <para>
    /// This method does not actually free any memory in the traditional sense, but merely resets the allocation pointer back to the beginning of the heap to allow re-use of objects.
    /// </para>
    /// <para>
    /// This method can take a <paramref name="finalizer"/> parameter that can be used to perform any required clean up, or reset of the objects in the pool. This can be useful if the object requires 
    /// disposal via a <see cref="IDisposable.Dispose"/> method, of if the object requires some other form of clean up like being reset to a default state. If this parameter is omitted, then no clean up 
    /// will be executed and the objects will be left in their current state.
    /// </para>
    /// </remarks>
    public void Reset(Action<T>? finalizer = null)
    {
        Interlocked.Exchange(ref _currentItem, -1);

        if (finalizer is null)
        {
            return;
        }

        foreach (T item in _items)
        {
            finalizer(item);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonLinearPool{T}"/> class.
    /// </summary>
    /// <param name="maxObjectCount">The number of total objects available in the pool.</param>
    /// <param name="allocator">The allocator used to create an object in the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="maxObjectCount"/> parameter is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="allocator"/> parameter is a method that is used to create a new object in the pool. This method is called when the pool object has never been used before so that it will 
    /// return an instance. If the object does not need to be created (i.e. the resulting object has been used before), then the <paramref name="allocator"/> will not be called.
    /// </para>
    /// </remarks>
    /// <seealso cref="Allocate(Action{T}?)"/>
    public GorgonLinearPool(int maxObjectCount, Func<T> allocator)
    {
        if (maxObjectCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxObjectCount), Resources.GOR_ERR_ALLOCATOR_SIZE_TOO_SMALL);
        }

        TotalSize = maxObjectCount;
        _items = new T[maxObjectCount];
        ItemAllocator = allocator;
    }
}
