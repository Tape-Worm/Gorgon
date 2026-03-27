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
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pool for CPU descriptor heaps.
/// </summary>
internal unsafe sealed class CpuDescriptorHeapPool
    : IDisposable
{
    private const uint DefaultHeapSize = 128;

    private readonly Lock _lock = new();
    private readonly List<CpuDescriptorHeap> _free = new(4);
    private readonly List<CpuDescriptorHeap> _active = new(4);
    private CpuDescriptorHeap? _lastHeap;

    /// <summary>
    /// Property to return the graphics interface associated with this descriptor heal pool;.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the size of a descriptor, in bytes.
    /// </summary>
    public uint DescriptorSize
    {
        get;
    }

    /// <summary>
    /// Property to return the type of descriptors for this pool.
    /// </summary>
    public D3D12_DESCRIPTOR_HEAP_TYPE HeapType
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (CpuDescriptorHeap heap in _free.Concat(_active))
            {
                heap.Dispose();
            }

            Graphics.Log.Print($"Freed {_free.Count + _active.Count} descriptor heaps from the pool.", LoggingLevel.Intermediate);

            _lastHeap = null;
        }
    }

    /// <summary>
    /// Function to create a descriptor heap.
    /// </summary>
    /// <param name="count">The number of entries in the heap.</param>
    /// <returns>The newly created heap.</returns>
    private CpuDescriptorHeap CreateHeap(uint count)
    {
        // No available heap. So create it.
        uint maxDescriptors = BitOperations.RoundUpToPowerOf2(count * 2).Max(DefaultHeapSize).Min(1_000_000);
        CpuDescriptorHeap dynamicHeap = new(this, maxDescriptors);
        _active.Add(dynamicHeap);

        return dynamicHeap;
    }

    /// <summary>
    /// Function to allocate a CPU descriptor handle.
    /// </summary>
    /// <param name="count">The number of handles to allocate.</param>
    /// <param name="allocation">The allocation from the heap.</param>
    /// <returns>The CPU descriptor handle, and optionally, the GPU descriptor handle.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="count"/> is less than 1.</exception>
    public void Allocate(uint count, out CpuDescriptorAllocation allocation)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0u, nameof(count));

        using (_lock.EnterScope())
        {
            CpuDescriptorHeap searchHeap;
            CpuDescriptorHeap? heap = null;

            if ((_lastHeap is not null) && (_lastHeap.Used + count <= _lastHeap.Length))
            {
                heap = _lastHeap;
            }

            if ((heap is null) && (_active.Count > 0))
            {
                // Check the active list.
                for (int i = 0; i < _active.Count; ++i)
                {
                    searchHeap = _active[i];

                    if (searchHeap.Used + count <= searchHeap.Length)
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

                    if (searchHeap.Used + count <= searchHeap.Length)
                    {
                        _free.RemoveAt(i);
                        _active.Add(searchHeap);
                        heap = searchHeap;
                        break;
                    }
                }
            }

            heap ??= CreateHeap(count);
            heap.Allocate(count, out allocation);
            _lastHeap = heap;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CpuDescriptorHeapPool"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this pool.</param>
    /// <param name="heapType">The type of descriptor heap that this pool generates.</param>
    public CpuDescriptorHeapPool(GorgonGraphics graphics, D3D12_DESCRIPTOR_HEAP_TYPE heapType)
    {
        Graphics = graphics;
        HeapType = heapType;        
        DescriptorSize = graphics.D3DDevice.Get()->GetDescriptorHandleIncrementSize(HeapType);
    }
}
