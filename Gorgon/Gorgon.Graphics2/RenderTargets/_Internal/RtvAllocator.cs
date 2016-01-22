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
// Created: Thursday, January 14, 2016 9:28:04 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using D3D12 = SharpDX.Direct3D12;

namespace Gorgon.Graphics.RenderTargets
{
	/// <summary>
	/// An allocator used to create render target views.
	/// </summary>
	class RtvAllocator
		: IDisposable
	{
		#region Constant.
		// The maximum number of handles/heap.
		private const int MaxHandles = 256;
		#endregion

		#region Variables.
		// The graphics interface that owns this allocator.
		private readonly GorgonGraphics _graphics;
		// A pool of previously created heaps.
		private ConcurrentBag<D3D12.DescriptorHeap> _heapPool = new ConcurrentBag<D3D12.DescriptorHeap>();
		// The descriptor heap.
		private D3D12.DescriptorHeap _heap;
		// Number of free handles to place on the heap.
		private int _freeHandles = MaxHandles;
		// The current handle.
		private D3D12.CpuDescriptorHandle _current;
		// Current descriptor size, in bytes.
		private int _currentSize;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a new heap.
		/// </summary>
		/// <returns>A new descriptor heap.</returns>
		private D3D12.DescriptorHeap CreateHeap()
		{
			var desc = new D3D12.DescriptorHeapDescription
				        {
					        Flags = D3D12.DescriptorHeapFlags.None,
					        NodeMask = 1,
					        DescriptorCount = MaxHandles,
					        Type = D3D12.DescriptorHeapType.RenderTargetView
				        };

			D3D12.DescriptorHeap result = _graphics.D3DDevice.CreateDescriptorHeap(desc);
			_heapPool.Add(result);
			result.Name = string.Format($"Gorgon RTV Heap #{_heapPool.Count}");
			return result;
		}

		/// <summary>
		/// Function to allocate a new descriptor handle for a render target view.
		/// </summary>
		/// <param name="count">The number of handles to place on the heap.</param>
		/// <returns>The new descriptor handle.</returns>
		public D3D12.CpuDescriptorHandle Allocate(int count)
		{
			if ((_heap == null) || (_freeHandles < count))
			{
				_heap = CreateHeap();
				_current = _heap.CPUDescriptorHandleForHeapStart;
				_freeHandles = MaxHandles;
				_currentSize = _graphics.D3DDevice.GetDescriptorHandleIncrementSize(D3D12.DescriptorHeapType.RenderTargetView);
			}

			D3D12.CpuDescriptorHandle result = _current;
			_current.Ptr += count * _currentSize;
			_freeHandles -= count;

			return result;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			ConcurrentBag<D3D12.DescriptorHeap> pool = Interlocked.Exchange(ref _heapPool, null);

			if (pool != null)
			{
				foreach (D3D12.DescriptorHeap heap in pool)
				{
					heap.Dispose();
				}
			}

			// This heap is stored in the pool which is deallocated prior to this.	
			_heap = null;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="RtvAllocator"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this allocator.</param>
		public RtvAllocator(GorgonGraphics graphics)
		{
			_graphics = graphics;
		}
		#endregion
	}
}
