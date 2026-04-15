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
// Created: January 16, 2026 9:02:10 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A heap for storing resource data.
/// </summary>
internal sealed unsafe class CpuBufferHeap
    : IDisposable, IGorgonNamedObject
{
    private ComPtr<D3D12MA_VirtualBlock> _memoryBlock;
    private ComPtr<ID3D12Resource2> _resourceHeap;
    private ComPtr<D3D12MA_Allocation> _heapAllocation;

    private readonly Guid _id = Guid.NewGuid();

    /// <summary>
    /// Property to return a reference to the D3D 12 resource acting as a heap.
    /// </summary>
    public ref readonly ComPtr<ID3D12Resource2> D3DResource => ref _resourceHeap;

    /// <summary>
    /// Property to return the address of the resource on the GPU.
    /// </summary>
    public ulong GpuAddress
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the pointer to the resource in memory.
    /// </summary>
    public byte* CpuPointer
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the fence value for the heap while it is in use by the graphics queue on the GPU.
    /// </summary>
    public ulong GfxFence
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the fence value for the heap while it is in use by the compute queue on the GPU.
    /// </summary>
    public ulong ComputeFence
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the fence value for the heap while it is in use by the copy queue on the GPU.
    /// </summary>
    public ulong CopyFence
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the total size of this heap, in bytes.
    /// </summary>
    public ulong Size
    {
        get;
    }

    /// <summary>
    /// Property to return the amount of memory used on this heap.
    /// </summary>
    public ulong Used
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the pool that owns this heap.
    /// </summary>
    public CpuResourceHeapPool Pool
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_resourceHeap.IsNull)
            {
                _resourceHeap.Get()->Unmap(0, null);
            }

            CopyFence = ComputeFence = GfxFence = 0;
            Used = 0;
            GpuAddress = 0;
            CpuPointer = null;

            Pool.Graphics.Log.Print($"Destroying {nameof(ID3D12Resource2)} '{Name}'...", LoggingLevel.Verbose);
        }

        if (!_memoryBlock.IsNull)
        {
            _memoryBlock.Get()->Clear();
            _memoryBlock.Dispose();
        }

        _resourceHeap.Dispose();
        _heapAllocation.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to allocate a block of memory from the heap.
    /// </summary>
    /// <param name="size">The size of the block, in bytes.</param>
    /// <param name="alignment">The alignment of the memory within the heap.</param>
    /// <param name="allocation">The resulting allocation from the heap.</param>
    public void Allocate(ulong size, int alignment, out CpuBufferAllocation allocation)
    {
        D3D12MA_VIRTUAL_ALLOCATION_DESC desc = new()
        {
            Alignment = (ulong)alignment,
            Flags = D3D12MA_VIRTUAL_ALLOCATION_FLAGS.D3D12MA_VIRTUAL_ALLOCATION_FLAG_NONE,
            Size = size
        };

        D3D12MA_VirtualAllocation vmAlloc = default;
        ulong offset = 0;

        _memoryBlock.Get()->Allocate(&desc, &vmAlloc, &offset)
            .ThrowIfFailed(GorgonResult.OutOfMemory, () => string.Format(Resources.GORGFX_ERR_HEAP_OUT_OF_MEMORY, Name, size.FormatMemory()));

        Used += size.AlignUp((ulong)alignment);

        allocation = new(this, vmAlloc.AllocHandle, offset);
    }

    /// <summary>
    /// Function to free all allocations from this heap.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method will invalidate all outstanding allocations, please use with care.
    /// </para>
    /// </remarks>
    public void FreeAll()
    {
        CopyFence = ComputeFence = GfxFence = 0;
        _memoryBlock.Get()->Clear();
        Used = 0;        
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~CpuBufferHeap() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of a <see cref="CpuBufferHeap"/> class.
    /// </summary>
    /// <param name="pool">The pool that owns this heap.</param>
    /// <param name="size">The total size of the heap, in bytes.</param>
    /// <param name="isDownload"><b>true</b> if the heap is used for downloading data, or <b>false</b> for uploading.</param>
    /// <exception cref="GorgonException">Thrown if the heap could not be created or mapped.</exception>
    public CpuBufferHeap(CpuResourceHeapPool pool, ulong size, bool isDownload)
    {        
        Pool = pool;
        Size = size;
        Name = $"Gorgon Dynamic Memory Heap {_id} ({size.FormatMemory()})";

        D3D12_HEAP_TYPE heapType;
        D3D12_RESOURCE_FLAGS flags = Pool.Graphics.Adapter.HasTightAlignmentSupport ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;

        if (isDownload)
        {
            heapType = D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK;            
        }
        else
        {
            heapType = Pool.Graphics.Adapter.HasGpuUploadSupport ? D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_GPU_UPLOAD : D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD;
        }

        D3D12MA_ALLOCATION_DESC heapDesc = new(heapType);
        ComPtr<D3D12MA_Allocation> heapAllocation = default;
        ComPtr<D3D12MA_VirtualBlock> memoryBlock = default;
        D3D12_RESOURCE_DESC1 desc = D3D12_RESOURCE_DESC1.Buffer(size, flags);
        ComPtr<ID3D12Resource2> resource = default;

        Pool.Graphics.Log.Print($"Creating {nameof(ID3D12Resource2)} '{Name}'...", LoggingLevel.Verbose);

        Pool.Graphics.Memory.Allocator.Get()->CreateResource3(&heapDesc, &desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED, 
            null, 0, null, 
            heapAllocation.GetAddressOf(), Win32.__uuidof<ID3D12Resource2>(), (void**)resource.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_HEAP, Name));

        resource.SetD3DDebugName($"Gorgon D3D12 Resource {Name}");        

        GpuAddress = resource.Get()->GetGPUVirtualAddress();
        
        byte* resourceCpu;
        resource.Get()->Map(0, null, (void**)&resourceCpu)
            .ThrowIfFailed(GorgonResult.CannotWrite, () => string.Format(Resources.GORGFX_ERR_CANNOT_WRITE_DATA, Name));

        CpuPointer = resourceCpu;

        D3D12MA_VIRTUAL_BLOCK_DESC vBlockDesc = new(Size);

        D3D12MemAlloc.D3D12MA_CreateVirtualBlock(&vBlockDesc, memoryBlock.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_HEAP, Name));

        _heapAllocation = heapAllocation;
        _resourceHeap = resource;
        _memoryBlock = memoryBlock;
    }
}
