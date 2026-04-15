// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: January 3, 2026 2:07:39 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pool for resource heaps.
/// </summary>
/// <param name="graphics">The graphics interface associated with this pool.</param>
/// <param name="isDownload"><b>true</b> to use this heap pool for download only heaps, <b>false</b> to use it for upload only.</param>
internal unsafe sealed class CpuResourceHeapPool(GorgonGraphics graphics, bool isDownload)
        : IDisposable
{
    private const ulong DefaultHeapSize = 65536;

    private readonly Lock _lock = new();
    private readonly Queue<CpuBufferHeap> _inUse = new(128);
    private readonly List<CpuBufferHeap> _free = new(128);
    private readonly List<CpuBufferHeap> _active = new(128);
    private readonly bool _isDownload = isDownload;
    private CpuBufferHeap? _lastHeap;

    /// <summary>
    /// Property to return the graphics interface associated with this heap pool.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout);

            // Wait for the GPU to finish with our heaps.
            while(_inUse.Count > 0)
            {
                Release();
            }

            foreach (CpuBufferHeap heap in _free.Concat(_active))
            {
                heap.Dispose();
            }

            Graphics.Log.Print($"Freed {_free.Count + _active.Count} dynamic resource heaps from the pool.", LoggingLevel.Intermediate);

            _lastHeap = null;
        }
    }

    /// <summary>
    /// Function to release any in-use heaps if the GPU is finished with them.
    /// </summary>
    private void Release()
    {
        if (_inUse.Count == 0)
        {
            return;
        }

        ulong gfxCompleted = Graphics.Queues.GraphicsQueue.D3DFence.Get()->GetCompletedValue();
        ulong computeCompleted = Graphics.Queues.ComputeQueue.D3DFence.Get()->GetCompletedValue();
        ulong copyCompleted = Graphics.Queues.CopyQueue.D3DFence.Get()->GetCompletedValue();

        while(_inUse.TryPeek(out CpuBufferHeap? heap))
        {
            Debug.Assert(heap.GfxFence != 0 || heap.ComputeFence != 0 || heap.CopyFence != 0, "The heap does not have a valid fence value. Cannot safely remove this heap.");

            if ((gfxCompleted < heap.GfxFence) || (computeCompleted < heap.ComputeFence) || (copyCompleted < heap.CopyFence))
            {
                break;
            }

            _inUse.Dequeue();
            _free.Add(heap);

            // Reset the heap, so we can reuse it.
            // This has to be done or we'd forever be creating heaps per frame and end up with a memory leak.
            heap.FreeAll();
        }
    }

    /// <summary>
    /// Function to allocate a CPU descriptor handle.
    /// </summary>
    /// <param name="size">The size of the allocation.</param>
    /// <param name="alignment">The alignment of the data within the heap.</param>
    /// <param name="allocation">The allocation from the heap.</param>
    /// <returns>The CPU descriptor handle, and optionally, the GPU descriptor handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="size"/> is less than 1.</exception>
    public void Allocate(ulong size, int alignment, out CpuBufferAllocation allocation)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0u, nameof(size));

        using (_lock.EnterScope())
        {
            CpuBufferHeap searchHeap;
            CpuBufferHeap? heap = null;
            ulong alignedSize = size.AlignUp((ulong)alignment);

            if ((_lastHeap is not null) && (_lastHeap.Used + alignedSize <= _lastHeap.Size))
            {
                heap = _lastHeap;
            }

            if ((heap is null) && (_active.Count > 0))
            {
                // Check the active list.
                for (int i = 0; i < _active.Count; ++i)
                {
                    searchHeap = _active[i];

                    if (searchHeap.Used + alignedSize <= searchHeap.Size)
                    {
                        heap = searchHeap;
                        break;
                    }
                }
            }

            if ((heap is null) && (_free.Count > 0))
            {
                // If no active heaps are available, then check for free empty heaps.
                for (int i = _free.Count - 1; i >= 0; --i)
                {
                    searchHeap = _free[i];

                    if (alignedSize <= searchHeap.Size)
                    {
                        _free.RemoveAt(i);
                        _active.Add(searchHeap);
                        heap = searchHeap;
                        break;
                    }
                }
            }
                        
            if (heap is null)
            {
                // No available heap. So create it.
                ulong newSize = BitOperations.RoundUpToPowerOf2(alignedSize * 2).Max(DefaultHeapSize);
                heap = new CpuBufferHeap(this, newSize, _isDownload);
                _active.Add(heap);
            }            

            heap.Allocate(size, alignment, out allocation);
            _lastHeap = heap;
        }
    }

    /// <summary>
    /// Function to signal the internal fence so we can lock down the existing in-use heaps in this pool.
    /// </summary>
    public void Signal()
    {
        using (_lock.EnterScope())
        {
            Release();

            _lastHeap = null;

            if (_active.Count == 0)
            {
                return;
            }

            while (_active.Count > 0)
            {
                int i = _active.Count - 1;
                CpuBufferHeap heap = _active[i];
                _active.RemoveAt(i);

                if (heap.Used == 0)
                {
                    _free.Add(heap);
                    continue;
                }

                heap.GfxFence = Graphics.Queues.GraphicsQueue.FenceValue;
                heap.ComputeFence = Graphics.Queues.ComputeQueue.FenceValue;
                heap.CopyFence = Graphics.Queues.CopyQueue.FenceValue;

                _inUse.Enqueue(heap);
            }
        }
    }

    /// <summary>
    /// Function to collect free heaps.
    /// </summary>
    public void GarbageCollect()
    {
        using (_lock.EnterScope())
        {
            if (_free.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _free.Count; ++i)
            {
                if ((_lastHeap is not null) && (_free[i] == _lastHeap))
                {
                    // Don't hang on to a heap that is going to die.
                    _lastHeap = null;
                }

                _free[i].Dispose();
            }

            _free.Clear();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
