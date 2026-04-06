// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: October 18, 2025 11:36:04 PM
//

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A heap containing GPU visible views to sub allocate.
/// </summary>
internal unsafe class GpuDescriptorHeap
        : IDisposable
{
    private ComPtr<ID3D12DescriptorHeap> _heap;
    private ComPtr<D3D12MA_VirtualBlock> _memoryBlock;
    
    private readonly Lock _syncLock = new();
    private readonly GorgonGraphics _graphics;
    private readonly D3D12_DESCRIPTOR_HEAP_TYPE _type;
    private uint _used;
    private readonly Queue<(ulong GfxFenceValue, ulong ComputeFenceValue, GpuDescriptorAllocation Allocation)> _freedAllocations = new(65536);

    /// <summary>
    /// Property to return the size of a descriptor, in bytes.
    /// </summary>
    public uint DescriptorSize
    {
        get;
    }

    /// <summary>
    /// Property to return the CPU handle for the view heap.
    /// </summary>
    public D3D12_CPU_DESCRIPTOR_HANDLE D3DCpuHandle
    {
        get;
    } 

    /// <summary>
    /// Property to return the GPU handle of the view heap.
    /// </summary>
    public D3D12_GPU_DESCRIPTOR_HANDLE D3DGpuHandle
    {
        get;
    }

    /// <summary>
    /// Property to return the number of views in the heap.
    /// </summary>
    public uint Count
    {
        get;
    }

    /// <summary>
    /// Property to return the amount of the heap that's been been consumed.
    /// </summary>
    public uint Used => _used;

    /// <summary>
    /// Property to return the view heap object for this heap.
    /// </summary>
    public ref readonly ComPtr<ID3D12DescriptorHeap> D3DHeap => ref _heap;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout);
            _graphics.Log.Print($"Cleaning up GPU {_type} view heap.", LoggingLevel.Simple);
        }

        if (!_memoryBlock.IsNull)
        {
            _memoryBlock.Get()->Clear();
            _memoryBlock.Dispose();
        }

        _heap.Dispose();
    }

    /// <summary>
    /// Function to create the heap for the descriptors.
    /// </summary>
    /// <returns>The CPU and GPU handle for the heap start.</returns>
    private (D3D12_CPU_DESCRIPTOR_HANDLE CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE GpuHandle) CreateNative()
    {
        D3D12MA_VIRTUAL_BLOCK_DESC maDesc = new(Count);

        D3D12MemAlloc.D3D12MA_CreateVirtualBlock(&maDesc, _memoryBlock.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_DESCRIPTOR_HEAP, _type));

        string heapName = $"Gorgon D3D12 {_type} GPU visible Descriptor Heap ({Count} descriptors)";

        _graphics.Log.Print($"Creating {nameof(ID3D12DescriptorHeap)} {heapName}..", LoggingLevel.Intermediate);

        D3D12_DESCRIPTOR_HEAP_DESC desc = new()
        {
            Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE,
            Type = _type,
            NumDescriptors = Count
        };

        _graphics.D3DDevice.Get()->CreateDescriptorHeap(&desc, Win32.__uuidof<ID3D12DescriptorHeap>(), (void**)_heap.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_DESCRIPTOR_HEAP, _type));

        _heap.SetD3DDebugName(heapName);

        return (_heap.Get()->GetCPUDescriptorHandleForHeapStart(), _heap.Get()->GetGPUDescriptorHandleForHeapStart());
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to sub allocate a portion of this view heap for use by an application.
    /// </summary>
    /// <param name="count">The number of views to allocate from the heap.</param>
    /// <param name="allocation">The allocation returned from the allocator.</param>
    /// <returns>The handle at which the views start on the CPU.</returns>    
    public void Allocate(uint count, out GpuDescriptorAllocation allocation)
    {
        using (_syncLock.EnterScope())
        {
            Debug.Assert(count > 0, "At least 1 view should be allocated.");

            D3D12MA_VIRTUAL_ALLOCATION_DESC desc = new(count, 0);
            D3D12MA_VirtualAllocation vmAllocation = default;
            ulong location = 0;

            _memoryBlock.Get()->Allocate(&desc, &vmAllocation, &location)
                .ThrowIfFailed(GorgonResult.OutOfMemory, () => string.Format(Resources.GORGFX_ERR_DESCRIPTORHEAP_OUT_OF_MEMORY, _type));

            allocation = new(vmAllocation.AllocHandle, (int)location, count);
            
            _used += count;
        }
    }

    /// <summary>
    /// Function to free a descriptor back to the heap.
    /// </summary>
    /// <param name="allocation">The allocation to return.</param>
    public void Free(ref GpuDescriptorAllocation allocation)
    {
        using (_syncLock.EnterScope())
        {
            if (allocation.Equals(in GpuDescriptorAllocation.Null))
            {
                return;
            }

            _freedAllocations.Enqueue((_graphics.GraphicsQueue.FenceValue, _graphics.ComputeQueue.FenceValue, allocation));

            allocation = GpuDescriptorAllocation.Null;
        }
    }

    /// <summary>
    /// Function to signal to release any outstanding descriptors.
    /// </summary>
    public void Signal()
    {
        using (_syncLock.EnterScope())
        {
            if (_freedAllocations.Count == 0)
            {
                return;
            }

            ulong gfxCompleted = _graphics.GraphicsQueue.D3DFence.Get()->GetCompletedValue();
            ulong computeCompleted = _graphics.ComputeQueue.D3DFence.Get()->GetCompletedValue();

            while (_freedAllocations.TryPeek(out (ulong GfxFence, ulong ComputeFence, GpuDescriptorAllocation Allocation) item))
            {
                // If we hit a fence that's greater than what's completed, then leave it be until the next pass.
                if ((gfxCompleted < item.GfxFence) || (computeCompleted < item.ComputeFence))
                {
                    break;
                }

                _memoryBlock.Get()->FreeAllocation(new D3D12MA_VirtualAllocation
                {
                    AllocHandle = item.Allocation.Handle
                });

                _used -= item.Allocation.Count;

                _freedAllocations.Dequeue();
            }
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GpuDescriptorHeap() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GpuDescriptorHeap"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this object.</param>
    /// <param name="type">The type of views in the heap.</param>
    /// <param name="count">The number of views available in the heap.</param>
    public GpuDescriptorHeap(GorgonGraphics graphics, D3D12_DESCRIPTOR_HEAP_TYPE type, uint count)
    {
        _graphics = graphics;
        _type = type;
        DescriptorSize = graphics.D3DDevice.Get()->GetDescriptorHandleIncrementSize(_type);
        Count = count;

        (D3DCpuHandle, D3DGpuHandle) = CreateNative();
    }
}
