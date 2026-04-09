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
// Created: January 3, 2026 2:09:37 PM
//

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A heap that holds descriptors for samplers, shader resource views, constant buffer views and read/write views.
/// </summary>
internal unsafe sealed class CpuDescriptorHeap
    : IDisposable
{
    private ComPtr<ID3D12DescriptorHeap> _d3dHeap;
    private ComPtr<D3D12MA_VirtualBlock> _memoryBlock;

    private readonly Lock _lock = new();
    private uint _used;    

    /// <summary>
    /// Property to return the D3D COM pointer to the descriptor heap.
    /// </summary>
    public ref readonly ComPtr<ID3D12DescriptorHeap> D3DHeap => ref _d3dHeap;

    /// <summary>
    /// Property to return the pool that this descriptor heap belongs to.
    /// </summary>
    public CpuDescriptorHeapPool Pool
    {
        get;
    }

    /// <summary>
    /// Property to return the starting CPU handle for the descriptor heap.
    /// </summary>
    public D3D12_CPU_DESCRIPTOR_HANDLE Start
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
    /// Property to return the total size, in descriptors, of this heap.
    /// </summary>
    public uint Length
    {
        get;
    }

    /// <summary>
    /// Property to return the number of descriptors allocated on this heap.
    /// </summary>
    public uint Used => _used;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            FreeAll();
            Pool.Graphics.Log.Print($"Destroying {nameof(ID3D12DescriptorHeap)} {Pool.HeapType} descriptor heap...", LoggingLevel.Verbose);
        }

        _d3dHeap.Dispose();
        _memoryBlock.Dispose();
    }

    /// <summary>
    /// Function to create the D3D 12 descriptor heap.
    /// </summary>
    /// <param name="device">The direct 3D device used to create the initial heap.</param>
    /// <param name="numDescriptors">The number of descriptors in the heap.</param>
    /// <returns>The COM pointer to the descriptor heap.</returns>
    private ComPtr<ID3D12DescriptorHeap> CreateNativeHeap(ref readonly ComPtr<ID3D12Device14> device, uint numDescriptors)
    {
        D3D12MA_VIRTUAL_BLOCK_DESC maDesc = new(numDescriptors);

        D3D12MemAlloc.D3D12MA_CreateVirtualBlock(&maDesc, _memoryBlock.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_DESCRIPTOR_HEAP, Pool.HeapType));

        string heapName = $"Gorgon D3D12 {Pool.HeapType} CPU visible Descriptor Heap ({numDescriptors} descriptors)";
        ComPtr<ID3D12DescriptorHeap> heap = default;

        Pool.Graphics.Log.Print($"Creating {nameof(ID3D12DescriptorHeap)} {heapName}..", LoggingLevel.Intermediate);

        D3D12_DESCRIPTOR_HEAP_DESC desc = new()
        {
            Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_NONE,
            Type = Pool.HeapType,
            NumDescriptors = numDescriptors
        };

        device.Get()->CreateDescriptorHeap(&desc, Win32.__uuidof<ID3D12DescriptorHeap>(), (void**)heap.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_DESCRIPTOR_HEAP, Pool.HeapType));

        heap.SetD3DDebugName(heapName);

        return heap;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }   

    /// <summary>
    /// Function to allocate a handle on the heap.
    /// </summary>
    /// <param name="count">The number of descriptors to allocate from the heap.</param>
    /// <param name="allocation">The allocation returned from the heap.</param>
    /// <exception cref="GorgonException">Thrown if the descriptor heap is out of room to store any new descriptors.</exception>
    public void Allocate(uint count, out CpuDescriptorAllocation allocation)
    {
        D3D12MA_VIRTUAL_ALLOCATION_DESC desc = new(count, 0);
        D3D12MA_VirtualAllocation vmAllocation = default;
        ulong location = 0;

        _memoryBlock.Get()->Allocate(&desc, &vmAllocation, &location)
            .ThrowIfFailed(GorgonResult.OutOfMemory, () => string.Format(Resources.GORGFX_ERR_DESCRIPTORHEAP_OUT_OF_MEMORY, Pool.HeapType));

        D3D12_CPU_DESCRIPTOR_HANDLE handle = Start;
        handle.Offset((int)location, DescriptorSize);

        allocation = new(this, vmAllocation.AllocHandle, count, handle);

        Interlocked.Add(ref _used, count);
    }

    /// <summary>
    /// Function to deallocate an allocated descriptor handle.
    /// </summary>
    /// <param name="allocation">The memory allocation to remove.</param>
    /// <remarks>
    /// <para>
    /// This method will set the <paramref name="allocation"/> parameter to <see cref="CpuDescriptorAllocation.Null"/> after freeing.
    /// </para>
    /// </remarks>
    public void Free(ref CpuDescriptorAllocation allocation)
    {
        if (allocation.IsNull)
        {
            return;
        }

        _memoryBlock.Get()->FreeAllocation(new D3D12MA_VirtualAllocation()
        {
            AllocHandle = allocation.Handle
        });

        // For some fucked up reason, the .NET team didn't see fit to give us a "Subtract" interlocked function, even though it exists
        // in Win32 (InterlockedExchangeSubtract). This ugly hack will give us something approximating that.
        Interlocked.Add(ref Unsafe.As<uint, long>(ref _used), -allocation.Count);
        allocation = CpuDescriptorAllocation.Null;
    }

    /// <summary>
    /// Function to free all allocated descriptors for this heap.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This frees all descriptor allocations. Any existing allocations from this heap will be considered invalid. Using freed descriptor allocations will result in undefined behaviour.
    /// </para>
    /// </remarks>
    public void FreeAll()
    {
        _memoryBlock.Get()->Clear();
        Interlocked.Exchange(ref _used, 0);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~CpuDescriptorHeap() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="CpuDescriptorHeap"/> class.
    /// </summary>
    /// <param name="pool">The pool that owns this descriptor heap.</param>
    /// <param name="initialSize">The initial size of the heap.</param>
    public CpuDescriptorHeap(CpuDescriptorHeapPool pool, uint initialSize)
    {
        Pool = pool;
        Length = initialSize;
        DescriptorSize = Pool.Graphics.D3DDevice.Get()->GetDescriptorHandleIncrementSize(Pool.HeapType);

        _d3dHeap = CreateNativeHeap(in Pool.Graphics.D3DDevice, initialSize);

        Start = _d3dHeap.Get()->GetCPUDescriptorHandleForHeapStart();
    }
}
