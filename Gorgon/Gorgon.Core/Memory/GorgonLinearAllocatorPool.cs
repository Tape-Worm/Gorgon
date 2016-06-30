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
using System.Threading;
using Gorgon.Core.Properties;

namespace Gorgon.Core.Memory
{
	/// <summary>
	/// A function used to allocate objects from a predefined heap of objects using a linear allocator strategy.
	/// </summary>
	/// <typeparam name="T">The type of objects to allocate.  These must be a reference type.</typeparam>
	/// <remarks>
	/// <para>
	/// While the .NET memory manager is quite fast (e.g. <c>new</c>), and is useful for most cases, it does have the problem of creating garbage. When these items are created and discarded, 
	/// the garbage collector may kick in at any given time, causing performance issues during time critical code (e.g. a rendering loop). By allocating a large pool of objects and then drawing 
	/// directly from this pool, we can reuse existing, but expired objects to ensure that the garbage collector does not collect these items until we are truly done with them.
	/// </para>
	/// <para>
	/// This allocator will never grow beyond its initial size.
	/// </para>
	/// </remarks>
	public class GorgonLinearAllocatorPool<T>
		where T : class
	{
		#region Variables.
		// The most current item in the heap.
		private int _currentItem = -1;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of items in the pool.
		/// </summary>
		protected T[] Items
		{
			get;
		}

		/// <summary>
		/// Property to return the number of items available to the allocator.
		/// </summary>
		public int TotalSize
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to allocate a new object off of the heap.
		/// </summary>
		/// <returns>The index of the next available slot, or -1 if no slot is available..</returns>
		protected int Allocate()
		{
			int nextIndex = Interlocked.Add(ref _currentItem, 1);

			if (nextIndex >= Items.Length)
			{
				return -1;
			}

			return nextIndex;
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
			Interlocked.Exchange(ref _currentItem, 0);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLinearAllocatorPool{T}"/> class.
		/// </summary>
		/// <param name="objectCount">The number of total objects available to the allocator.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="objectCount"/> parameter is less than 1.</exception>
		public GorgonLinearAllocatorPool(int objectCount)
		{
			if (objectCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(objectCount), Resources.GOR_ERR_ALLOCATOR_SIZE_TOO_SMALL);
			}

			TotalSize = objectCount;
			Items = new T[objectCount];
		}
		#endregion
	}
}
