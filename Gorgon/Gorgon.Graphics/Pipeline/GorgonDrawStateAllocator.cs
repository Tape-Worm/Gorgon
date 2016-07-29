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
// Created: July 19, 2016 7:33:16 PM
// 
#endregion

using System;
using Gorgon.Core.Memory;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An allocator for creating <see cref="GorgonDrawIndexedCall"/> objects.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Because garbage collection can cause minor (or major) stutters during rendering, we use this type to allocate a <see cref="GorgonDrawIndexedCall"/> from a pool of pre-allocated objects. When a new 
	/// <see cref="GorgonDrawIndexedCall"/> is allocated, an existing instance is used and returned from the pool.  
	/// </para>
	/// <para>
	/// If no more instances are available in the pool, the application should make use of the object(s) it's allocated, and then reset the pool.
	/// </para>
	/// <para>
	/// This allocator is thread safe.
	/// </para>
	/// </remarks>
	public class GorgonDrawCallAllocator
		: GorgonLinearAllocatorPool<GorgonDrawIndexedCall>
	{
		#region Methods.
		/// <summary>
		/// Function to allocate a <see cref="GorgonDrawIndexedCall"/> from the pool.
		/// </summary>
		/// <returns>The <see cref="GorgonDrawIndexedCall"/> from the pool, or <b>null</b> if the pool has been exhausted.</returns>
		public GorgonDrawIndexedCall AllocateDrawIndexedCall()
		{
			int index = Allocate();

			if (index == -1)
			{
				return null;
			}

			GorgonDrawIndexedCall result = Items[index];
			result.Reset();
			return result;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDrawCallAllocator"/> class.
		/// </summary>
		/// <param name="drawStateCount">The total number of draw states in this pool.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="drawStateCount"/> parameter is less than 1.</exception>
		public GorgonDrawCallAllocator(int drawStateCount)
			: base(drawStateCount)
		{
			for (int i = 0; i < Items.Length; ++i)
			{
				Items[i] = new GorgonDrawIndexedCall();
			}
		}
		#endregion
	}
}
