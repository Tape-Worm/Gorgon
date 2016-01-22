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
// Created: Thursday, January 14, 2016 2:13:32 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Math;
using D3D12 = SharpDX.Direct3D12;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Used to manage the lifetime of command queues, lists, etc...
	/// </summary>
	class GraphicsCommander
		: IDisposable
	{
		#region Variables.
		// The command queue for this object.
		private D3D12.CommandQueue _commandQueue;
		// A queue of available command allocators.
		private Queue<KeyValuePair<long, D3D12.CommandAllocator>> _readyAllocators = new Queue<KeyValuePair<long, D3D12.CommandAllocator>>();
		// A pool of allocators.
		private List<D3D12.CommandAllocator> _allocators = new List<D3D12.CommandAllocator>();
		// Event to signal when a frame is complete.
		private AutoResetEvent _fenceEvent = new AutoResetEvent(false);
		// The fence used to signal an event to indicate that a frame is complete.
		private D3D12.Fence _fence;
		// Next fence value.
		private long _nextFenceValue = 1;
		// Last fence value.
		private long _lastFenceValue;
		// Synchronization object for multiple threads.
		private readonly object _syncSignalLock = new object();
		// Synchronization object for multiple threads.
		private readonly object _syncAllocatorLock = new object();
		// The graphics object to use with this command manager.
		private readonly GorgonGraphics _graphics;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 12 command queue.
		/// </summary>
		public D3D12.CommandQueue D3DCommandQueue => _commandQueue;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create or retrieve a command allocator from the allocator pool.
		/// </summary>
		/// <returns>A new command allocator.</returns>
		public D3D12.CommandAllocator GetAllocator()
		{
			D3D12.CommandAllocator result = null;

			lock (_syncAllocatorLock)
			{
				if ((_readyAllocators.Count != 0)
					&& (IsFenceComplete(_readyAllocators.Peek().Key)))
				{
					KeyValuePair<long, D3D12.CommandAllocator> allocator = _readyAllocators.Dequeue();
					result = allocator.Value;
					result.Reset();
				}

				if (result != null)
				{
					return result;
				}

				result = _graphics.D3DDevice.CreateCommandAllocator(D3D12.CommandListType.Direct);
				result.Name = string.Format($"Gorgon Command Allocator #{_readyAllocators.Count}");
				_allocators.Add(result);

				return result;
			}
		}

		/// <summary>
		/// Function to retire an allocator back to the available allocator queue.
		/// </summary>
		/// <param name="fenceValue">The fence value for the allocator.</param>
		/// <param name="allocator">The allocator to retire.</param>
		public void Retire(long fenceValue, D3D12.CommandAllocator allocator)
		{
			lock (_syncAllocatorLock)
			{
				_readyAllocators.Enqueue(new KeyValuePair<long, D3D12.CommandAllocator>(fenceValue, allocator));
			}
		}

		/// <summary>
		/// Function to create a command list and associated allocator for a command list.
		/// </summary>
		/// <returns>A Tuple containing the command allocator and command list, </returns>
		public Tuple<D3D12.CommandAllocator, D3D12.GraphicsCommandList> CreateCommandList()
		{
			D3D12.CommandAllocator allocator = GetAllocator();
			D3D12.GraphicsCommandList list = _graphics.D3DDevice.CreateCommandList(D3D12.CommandListType.Direct, allocator, null);
			list.Name = "Gorgon Command List";
			list.Close();

			return new Tuple<D3D12.CommandAllocator, D3D12.GraphicsCommandList>(allocator, list);
		}

		/// <summary>
		/// Function to execute a command list.
		/// </summary>
		/// <param name="commandList">The command list to execute.</param>
		/// <returns>A fence value for the command list.</returns>
		public long ExecuteCommandList(D3D12.GraphicsCommandList commandList)
		{
			lock (_syncSignalLock)
			{
				_commandQueue.ExecuteCommandLists(1, new D3D12.CommandList[] { commandList });
				_commandQueue.Signal(_fence, _nextFenceValue);
				return _nextFenceValue++;
			}
		}

		/// <summary>
		/// Function to force an increment on the fence value.
		/// </summary>
		/// <returns>The new fence value.</returns>
		public long Increment()
		{
			lock (_syncSignalLock)
			{
				_commandQueue.Signal(_fence, _nextFenceValue);
				return _nextFenceValue++;
			}
		}

		/// <summary>
		/// Function to determine if a fence is finished.
		/// </summary>
		/// <param name="fenceValue">The fence value to check.</param>
		/// <returns><b>true</b> if finished, <b>false</b> if not.</returns>
		public bool IsFenceComplete(long fenceValue)
		{
			if (fenceValue > _lastFenceValue)
			{
				_lastFenceValue = _lastFenceValue.Max(_fence.CompletedValue);
			}

			return fenceValue <= _lastFenceValue;
		}

		/// <summary>
		/// Function to force the application to wait until the specified fence value is finished.
		/// </summary>
		/// <param name="fenceValue">The fence value to wait for.</param>
		public void WaitForFence(long fenceValue)
		{
			if (IsFenceComplete(fenceValue))
			{
				return;
			}

			lock (_syncSignalLock)
			{
				_fence.SetEventOnCompletion(fenceValue, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
				_fenceEvent.WaitOne();
				_lastFenceValue = fenceValue;
			}
		}

		/// <summary>
		/// Function to wait for the GPU to become idle.
		/// </summary>
		public void WaitForGPUIdle()
		{
			WaitForFence(Increment());
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			D3D12.CommandQueue queue = Interlocked.Exchange(ref _commandQueue, null);
			D3D12.Fence fence = Interlocked.Exchange(ref _fence, null);
			AutoResetEvent fenceEvent = Interlocked.Exchange(ref _fenceEvent, null);
			List<D3D12.CommandAllocator> allocators = Interlocked.Exchange(ref _allocators, null);

			_readyAllocators = null;

			foreach (D3D12.CommandAllocator allocator in allocators)
			{
				allocator.Dispose();
			}
			
			fence?.Dispose();
			fenceEvent?.Dispose();
			queue?.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GraphicsCommander"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use for this object.</param>
		public GraphicsCommander(GorgonGraphics graphics)
		{
			_graphics = graphics;
			_commandQueue = graphics.D3DDevice.CreateCommandQueue(new D3D12.CommandQueueDescription(D3D12.CommandListType.Direct));
			_commandQueue.Name = "Gorgon Graphics Command Queue";

			// Initialize our fence.
			_fence = graphics.D3DDevice.CreateFence(0, D3D12.FenceFlags.None);
			_fence.Name = "Gorgon Graphics Command Queue Fence";
			_fence.Signal(0);
		}
		#endregion
	}
}
