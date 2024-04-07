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

using System.Collections.Concurrent;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Memory;

/// <summary>
/// A memory allocation strategy that uses a standard pool/free list allocator to perform memory allocations
/// </summary>
/// <typeparam name="T">The type of objects to allocate.  These must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// While the .NET memory manager is quite fast (e.g. <c>new</c>), and is useful for most cases, it does have the problem of creating garbage. When these items are created and discarded, 
/// the garbage collector may kick in at any given time, causing performance issues during time critical code (e.g. a rendering loop). By allocating a large pool of objects and then drawing 
/// directly from this pool, we can reuse existing, but expired objects to ensure that the garbage collector does not collect these items until we are truly done with them
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
    // The list of items that are free for use.
    private readonly ConcurrentStack<T> _freeList = new();
    // The number of slots that are filled.
    private int _slotCount = -1;

    /// <summary>
    /// Property to return the allocator to use when creating new instances of an object.
    /// </summary>
    protected Func<T> ItemAllocator
    {
        get;
    }

    /// <summary>
    /// Property to return the number of items available to the pool.
    /// </summary>
    public int TotalSize
    {
        get;
    }

    /// <summary>
    /// Property to return the number of available slots in the pool.
    /// </summary>
    /// <remarks>
    /// When this value is 0, then the pool is full, and items need to be returned to the pool using <see cref="Deallocate(ref T?, Action{T}?)"/>.
    /// </remarks>
    /// <seealso cref="Deallocate(ref T?, Action{T}?)"/>
    public int AvailableSlots => TotalSize - (_slotCount + 1);

    /// <summary>
    /// Function to allocate a new object from the pool.
    /// </summary>
    /// <param name="initializer">[Optional] A function used to initialize the object returned by the allocator.</param>
    /// <returns>The newly allocated object.</returns>
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
    /// If there is no more room (i.e. <see cref="AvailableSlots"/> is 0), then applications should call the <see cref="Deallocate(ref T?, Action{T}?)"/> method to return an object to the pool.
    /// </para>
    /// <para>
    /// If the <paramref name="initializer"/> parameter is supplied, then this callback method can be used to initialize the new object before returning it from the allocator.
    /// </para>
    /// </remarks>
    /// <seealso cref="Deallocate(ref T?, Action{T}?)"/>    
    public T Allocate(Action<T>? initializer = null)
    {
        int filledSlotCount = Interlocked.Increment(ref _slotCount);

        if (filledSlotCount >= TotalSize)
        {
            throw new GorgonException(GorgonResult.OutOfMemory, Resources.GOR_ERR_ALLOCATOR_FULL);
        }        

        if ((_freeList.IsEmpty) || (!_freeList.TryPop(out T? item)))
        {
            item = ItemAllocator() ?? throw new GorgonException(GorgonResult.CannotCreate, Resources.GOR_ERR_OBJECT_CREATION_RETURNED_NULL);
        }

        initializer?.Invoke(item);

        return item;
    }

    /// <summary>
    /// Function to deallocate an item allocated by this pool.
    /// </summary>
    /// <param name="item">The item to deallocate.</param>
    /// <param name="finalizer">[Optional] A callback method used to perform any required clean up for the object being reset in the pool.</param>
    /// <exception cref="GorgonException">Thrown when there is no more room to add an item to the free list.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="item"/> parameter will be passed by reference to the method so that it will be set to <b>null</b> upon deallocation. This keeps the end user from using the object after it's been 
    /// deallocated.
    /// </para>
    /// <para>
    /// This method can take a <paramref name="finalizer"/> parameter that can be used to perform any required clean up, or reset of the objects in the pool. This can be useful if the object requires 
    /// disposal via a <see cref="IDisposable.Dispose"/> method, of if the object requires some other form of clean up like being reset to a default state. If this parameter is omitted, then no clean up 
    /// will be executed and the objects will be left in their current state.
    /// </para>
    /// </remarks>
    public void Deallocate(ref T? item, Action<T>? finalizer = null)
    {
        if (item is null)
        {
            return;
        }

        if (Interlocked.Decrement(ref _slotCount) < -1)
        {
            throw new GorgonException(GorgonResult.OutOfMemory, Resources.GOR_ERR_ALLOCATOR_FULL);
        }

        T? localItem = Interlocked.Exchange(ref item, null);

        if (localItem is null)
        {
            return;
        }

        finalizer?.Invoke(localItem);
        _freeList.Push(localItem);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonLinearPool{T}"/> class.
    /// </summary>
    /// <param name="maxObjectCount">The number of total objects available to the pool.</param>
    /// <param name="allocator">The allocator used to create an object in the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="maxObjectCount"/> parameter is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="allocator"/> parameter is a method that is used to create a new object in the pool. This method is called when the pool object has never been used before so that it will 
    /// return an instance. If the object does not need to be created (i.e. the resulting object has been used before), then the <paramref name="allocator"/> will not be called.
    /// </para>
    /// </remarks>
    /// <seealso cref="Allocate(Action{T}?)"/>
    public GorgonPool(int maxObjectCount, Func<T> allocator)
    {
        if (maxObjectCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxObjectCount), Resources.GOR_ERR_ALLOCATOR_SIZE_TOO_SMALL);
        }

        ItemAllocator = allocator;
        TotalSize = maxObjectCount;
        _freeList = new ConcurrentStack<T>();
    }
}