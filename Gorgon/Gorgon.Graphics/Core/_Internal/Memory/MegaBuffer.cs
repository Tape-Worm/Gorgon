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
// Created: March 23, 2026 12:41:43 PM
//

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Windows.Storage.Streams;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A giant monolithic buffer to store all GPU data (except textures).
/// </summary>
/// <remarks>
/// <para>
/// Gorgon uses a single huge buffer (up to 4GB of address space, just address space, not committed memory) to store all buffers required for rendering. This has the advantage of keeping all our buffer 
/// data in the same place for easier management, and as fast default heap memory. By using a single large buffer, this minimizes descriptor switching, and, more importantly enables the use of bindless 
/// rendering for more than just textures. By using this, we can do vertex pulling to read our vertex data without needing to bind vertex buffers to the pipeline.
/// </para>
/// <para>
/// The buffer is a reserved resource that uses default heaps. Each time a buffer is allocated, the tiles (a tile = 65,536 bytes) used by that buffer are marked for allocation. When the memory is ready to be 
/// used, it is committed to VRAM so the buffer will actually have a place to store its data. This allows us to use a large address space, without actually consuming VRAM until it's absolutely needed.
/// </para>
/// <para>
/// When memory is freed back to this buffer, it does so based on fence values from the queues. This means that deletion of objects is deferred for n+2 (or n+1 depending on frames in flight) frames.
/// </para>
/// </remarks>
internal unsafe sealed class MegaBuffer
    : IDisposable
{
    // Default heap size: 256 MB.
    private const ulong HeapSize = 256 * 1024 * 1024;
    // 256MB/heap div 65536 bytes per tile = 4096 tiles per heap.
    private const uint MaxTilesPerHeap = (int)(HeapSize / 65536);

    private ComPtr<ID3D12Resource2> _d3dBuffer;
    private ComPtr<D3D12MA_VirtualBlock> _memoryBlock;
    private readonly ComPtr<ID3D12Heap>[] _d3dHeaps = [];

    private static readonly uint[] _tileCounts = new uint[MaxTilesPerHeap];
    private readonly ulong _maxAddressSpace = 4UL * 1024 * 1024 * 1024;    
    private readonly Lock _lock = new();
    private readonly GorgonGraphics _graphics;
    private readonly ulong _size;
    private readonly Stack<ushort>[] _tiles;
    private readonly List<GorgonRange<ushort>> _pendingCommits = [];
    private readonly ushort[] _tileRefCounts;
    private readonly (ushort HeapIndex, ushort Index)[] _allocatedTiles;
    private ushort _heapIndex;
    private readonly Queue<(ulong GfxFence, ulong ComputeFence, ulong CopyFence, GpuBufferAllocation Allocation)> _pendingDeletions = new(65536);
    private readonly List<ushort> _unmappedTiles = [];
    private readonly List<int> _emptyHeaps = [];

    /// <summary>
    /// Property to return the pointer to the backing buffer store.
    /// </summary>
    public ref readonly ComPtr<ID3D12Resource2> D3DBuffer => ref _d3dBuffer;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout);

            _graphics.Log.Print($"Destroying mega buffer ({_size.FormatMemory()}).", LoggingLevel.Verbose);

            _pendingDeletions.Clear();
            _pendingCommits.Clear();
            Array.Clear(_allocatedTiles);

            using (_lock.EnterScope())
            {
                for (int i = 0; i < _d3dHeaps.Length; i++)
                {
                    _d3dHeaps[i].Dispose();
                }
            }
        }

        if (!_memoryBlock.IsNull)
        {
            _memoryBlock.Get()->Clear();
            _memoryBlock.Dispose();
        }

        _d3dBuffer.Dispose();        
    }

    /// <summary>
    /// Function to create a D3D heap for the resource.
    /// </summary>
    /// <returns>The pointer to the heap.</returns>
    private ComPtr<ID3D12Heap> CreateHeap()
    {
        string name = $"Gorgon Mega Buffer Heap #{_heapIndex + 1} ({HeapSize.FormatMemory()})";
        D3D12_HEAP_DESC desc = new(HeapSize, D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT, 0, D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_ALLOW_ONLY_BUFFERS);

        ComPtr<ID3D12Heap> result = default;

        _graphics.Log.Print($"Creating {name}...", LoggingLevel.Verbose);

        _graphics.D3DDevice.Get()->CreateHeap(&desc, Win32.__uuidof<ID3D12Heap>(), (void**)result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_HEAP, name));

        result.SetD3DDebugName(name);
        
        for (int i = 0; i < MaxTilesPerHeap; ++i)
        {
            _tiles[_heapIndex].Push((ushort)i);
        }

        return result;
    }

    /// <summary>
    /// Function to create the native backing store for the mega buffer.
    /// </summary>
    /// <returns>The pointer to the mega buffer resource and virtual memory block.</returns>
    private (ComPtr<ID3D12Resource2> resource,ComPtr<D3D12MA_VirtualBlock> memoryBlock) CreateNative()
    {
        string name = $"Gorgon Mega Buffer ({_size.FormatMemory()})";
        D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;

        if (_graphics.Adapter.HasTightAlignmentSupport)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT;
        }

        _graphics.Log.Print($"Creating reserved {nameof(ID3D12Resource2)} '{name}'...", LoggingLevel.Verbose);

        ComPtr<D3D12MA_VirtualBlock> memoryBlock = default;
        ComPtr<ID3D12Resource2> resource = default;

        D3D12_RESOURCE_DESC desc = D3D12_RESOURCE_DESC.Buffer(_maxAddressSpace, flags);

        _graphics.D3DDevice.Get()->CreateReservedResource2(&desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED, 
            null, null, 0, null, 
            Win32.__uuidof<ID3D12Resource2>(), (void**)resource.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE);

        resource.SetD3DDebugName(name);

        _d3dHeaps[0] = CreateHeap();

        D3D12MA_VIRTUAL_BLOCK_DESC maDesc = new(_size);

        D3D12MemAlloc.D3D12MA_CreateVirtualBlock(&maDesc, memoryBlock.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_HEAP, name));

        return (resource, memoryBlock);
    }

    /// <summary>
    /// Function to get an existing heap, or a new heap if the tile space has run out.
    /// </summary>
    /// <exception cref="GorgonException">Thrown if a new heap allocation was not possible due to running out of space.</exception>
    /// <remarks>
    /// <para>
    /// Running out of heap space is fatal here. The application must stop whatever it is doing and the developer must try to adjust the available memory to avoid any possible OOM scenarios.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetHeap()
    {
        if (_tiles[_heapIndex].Count != 0)
        {
            return;
        }

        for (int i = 0; i < _tiles.Length; ++i)
        {
            if (_tiles[i].Count != 0)
            {
                _heapIndex = (ushort)i;
                return;
            }
        }

        _heapIndex++;

        if (_heapIndex >= _d3dHeaps.Length)
        {
            throw new GorgonException(GorgonResult.OutOfMemory, string.Format(Resources.GORGFX_ERR_HEAP_OUT_OF_MEMORY, "Mega Buffer", _size.FormatMemory()));
        }               

        _d3dHeaps[_heapIndex] = CreateHeap();
    }

    /// <summary>
    /// Function to map the allocation to tiles and heaps.
    /// </summary>
    /// <param name="location">The location of the allocation in the mega buffer.</param>
    /// <param name="size">The size, in bytes, of the allocation.</param>
    /// <returns>The starting tile for the allocation, and the ending tile for the allocation.</returns>
    private (ushort start, ushort end) MapTilesAndHeaps(ulong location, ulong size)
    {
        ushort tileStart = (ushort)(location >> 16);
        ushort tileEnd = (ushort)((location + size - 1) >> 16);

        ushort? currentRange = null;

        for (int i = tileStart; i <= tileEnd; ++i)
        {
            if (_tileRefCounts[i] == 0)
            {
                ushort currentHeapIndex = _heapIndex;
                GetHeap();

                // We split the heap, so add a pending commit for the last heap.
                if ((currentRange is not null) && (_heapIndex != currentHeapIndex))
                {
                    _pendingCommits.Add(new GorgonRange<ushort>(currentRange.Value, (ushort)(i - 1)));
                    currentRange = (ushort)i;
                }
                else
                {
                    currentRange ??= (ushort)i;
                }

                _allocatedTiles[i] = (_heapIndex, _tiles[_heapIndex].Pop());
            }
            else
            {
                if (currentRange is not null)
                {
                    _pendingCommits.Add(new GorgonRange<ushort>(currentRange.Value, (ushort)(i - 1)));
                    currentRange = null;
                }
            }

            ++_tileRefCounts[i];
        }

        if (currentRange is not null)
        {
            _pendingCommits.Add(new GorgonRange<ushort>(currentRange.Value, tileEnd));
        }

        return (tileStart, tileEnd);
    }


    /// <summary>
    /// Function to process all unmapped tiles.
    /// </summary>
    /// <param name="queue"><inheritdoc cref="Commit" path="/param[@name='queue']"/></param>
    private void ProcessUnmaps(ref readonly ComPtr<ID3D12CommandQueue> queue)
    {
        if (_unmappedTiles.Count == 0)
        {
            return;
        }

        D3D12_TILED_RESOURCE_COORDINATE* coords = stackalloc D3D12_TILED_RESOURCE_COORDINATE[_unmappedTiles.Count];
        D3D12_TILE_RANGE_FLAGS* flags = stackalloc D3D12_TILE_RANGE_FLAGS[_unmappedTiles.Count];

        for (int i = 0; i < _unmappedTiles.Count; i++)
        {
            coords[i] = new D3D12_TILED_RESOURCE_COORDINATE(_unmappedTiles[i], 0, 0, 0);
            flags[i] = D3D12_TILE_RANGE_FLAGS.D3D12_TILE_RANGE_FLAG_NULL;
        }

        fixed (uint* countPtr = &_tileCounts[0])
        {
            queue.Get()->UpdateTileMappings((PID3D12Resource2)_d3dBuffer.Get(),
            (uint)_unmappedTiles.Count,
            coords,
            null,
            null,
            (uint)_unmappedTiles.Count,
            flags,
            null,
            countPtr,
            D3D12_TILE_MAPPING_FLAGS.D3D12_TILE_MAPPING_FLAG_NONE);
        }

        _unmappedTiles.Clear();

        // Purge any heaps scheduled for destruction.
        for (int i = 0; i < _emptyHeaps.Count; ++i)
        {
            _d3dHeaps[_emptyHeaps[i]].Dispose();            
        }

        _emptyHeaps.Clear();
    }

    /// <summary>
    /// Function to commit the pending allocations to the reserved resource.
    /// </summary>
    /// <param name="queue">The command queue that will commit the allocations to have backing VRAM.</param>
    private void ProcessPending(in ComPtr<ID3D12CommandQueue> queue)
    {
        if (_pendingCommits.Count == 0)
        {
            return;
        }

        ReadOnlySpan<GorgonRange<ushort>> pendingSpan = CollectionsMarshal.AsSpan(_pendingCommits);
        D3D12_TILED_RESOURCE_COORDINATE[] coordinates = ArrayPool<D3D12_TILED_RESOURCE_COORDINATE>.Shared.Rent((int)MaxTilesPerHeap);
        D3D12_TILE_RANGE_FLAGS[] flags = ArrayPool<D3D12_TILE_RANGE_FLAGS>.Shared.Rent((int)MaxTilesPerHeap);
        uint[] offsets = ArrayPool<uint>.Shared.Rent((int)MaxTilesPerHeap);

        try
        {
            for (int i = 0; i < pendingSpan.Length; ++i)
            {
                ref readonly GorgonRange<ushort> range = ref pendingSpan[i];
                ushort rangeCount = range.Length;

                // No range? No inclusion.
                if (rangeCount == 0)
                {
                    continue;
                }

                ComPtr<ID3D12Heap> heap = default;

                for (int j = range.Minimum, a = 0; j <= range.Maximum; ++j, ++a)
                {
                    // The heap index is always going to be the same for the range. We ensured that we have multiple ranges
                    // for tiles that cross heaps up in MapTilesAndHeaps.
                    ref readonly (ushort HeapIndex, ushort TileIndex) allocated = ref _allocatedTiles[j];

                    if (heap.IsNull)
                    {
                        heap = _d3dHeaps[allocated.HeapIndex];
                    }
                    coordinates[a] = new D3D12_TILED_RESOURCE_COORDINATE((uint)j, 0, 0, 0);
                    flags[a] = D3D12_TILE_RANGE_FLAGS.D3D12_TILE_RANGE_FLAG_NONE;
                    offsets[a] = allocated.TileIndex;
                }

                Debug.Assert(!heap.IsNull, $"The heap for this range at index {i} is not assigned!");

                fixed (D3D12_TILED_RESOURCE_COORDINATE* coordPtr = &coordinates[0])
                fixed (D3D12_TILE_RANGE_FLAGS* flagPtr = &flags[0])
                fixed (uint* offsetPtr = &offsets[0])
                fixed (uint* countPtr = &_tileCounts[0])
                {
                    queue.Get()->UpdateTileMappings((PID3D12Resource2)_d3dBuffer.Get(),
                        rangeCount,
                        coordPtr,
                        null,
                        heap.Get(),
                        rangeCount,
                        flagPtr,
                        offsetPtr,
                        countPtr,
                        D3D12_TILE_MAPPING_FLAGS.D3D12_TILE_MAPPING_FLAG_NONE);
                }
            }
        }
        finally
        {
            ArrayPool<D3D12_TILED_RESOURCE_COORDINATE>.Shared.Return(coordinates, true);
            ArrayPool<D3D12_TILE_RANGE_FLAGS>.Shared.Return(flags, true);
            ArrayPool<uint>.Shared.Return(offsets, true);
        }

        _pendingCommits.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to sub allocate a buffer from the Mega Buffer.
    /// </summary>
    /// <param name="size">The size, in bytes, for the buffer.</param>
    /// <param name="allocation">The resulting allocation from the mega buffer.</param>
    /// <param name="alignment">The alignment, in bytes, of the buffer within the mega buffer.</param>
    public void Allocate(ulong size, out GpuBufferAllocation allocation, uint alignment)
    {
        using (_lock.EnterScope())
        {
            if (alignment == 0)
            {
                alignment = 1;
            }

            D3D12MA_VIRTUAL_ALLOCATION_DESC desc = new(size + (alignment - 1), 0);
            D3D12MA_VirtualAllocation vmAllocation = default;
            ulong location = 0;
                        
            _memoryBlock.Get()->Allocate(&desc, &vmAllocation, &location)
                .ThrowIfFailed(GorgonResult.OutOfMemory, () => string.Format(Resources.GORGFX_ERR_HEAP_OUT_OF_MEMORY, "Mega Buffer", _size.FormatMemory()));

            location = (location + (alignment - 1)) / alignment * alignment;

            (ushort start, ushort end) = MapTilesAndHeaps(location, size);

            allocation = new GpuBufferAllocation(vmAllocation.AllocHandle, desc.Size, (uint)location, start, end);
        }
    }

    /// <summary>
    /// Function to free an allocation for a buffer.
    /// </summary>
    /// <param name="allocation">The allocation to free.</param>
    public void Free(ref GpuBufferAllocation allocation)
    {
        using (_lock.EnterScope())
        {
            if (allocation.IsNull)
            {
                return;
            }

            _pendingDeletions.Enqueue((_graphics.GraphicsQueue.FenceValue, _graphics.ComputeQueue.FenceValue, _graphics.CopyQueue.FenceValue, allocation));

            allocation = GpuBufferAllocation.Null;
        }
    }

    /// <summary>
    /// Function to signal the mega buffer that it is time to collect any pending deallocations.
    /// </summary>
    public void Signal()
    {
        using (_lock.EnterScope())
        {
            ulong gfxCompleted = _graphics.GraphicsQueue.D3DFence.Get()->GetCompletedValue();
            ulong computeCompleted = _graphics.ComputeQueue.D3DFence.Get()->GetCompletedValue();
            ulong copyCompleted = _graphics.CopyQueue.D3DFence.Get()->GetCompletedValue();

            while (_pendingDeletions.TryPeek(out (ulong GfxFence, ulong ComputeFence, ulong CopyFence, GpuBufferAllocation Allocation) item))
            {
                // If we hit a fence that's greater than what's completed, then leave it be until the next pass.
                if ((gfxCompleted < item.GfxFence) || (computeCompleted < item.ComputeFence) || (copyCompleted < item.CopyFence))
                {
                    break;
                }

                ushort tileStart = item.Allocation.TileStart;
                ushort tileEnd = item.Allocation.TileEnd;

                for (int i = tileStart; i <= tileEnd; ++i)
                {
                    ref (ushort HeapIndex, ushort TileIndex) allocated = ref _allocatedTiles[i];                    

                    ref ushort tileRef = ref _tileRefCounts[i];

                    --tileRef;

                    if (tileRef <= 0)
                    {
                        tileRef = 0;
                        _tiles[allocated.HeapIndex].Push(allocated.TileIndex);
                        allocated = default;
                        _unmappedTiles.Add((ushort)i);
                    }                    
                }

                _memoryBlock.Get()->FreeAllocation(new D3D12MA_VirtualAllocation
                {
                    AllocHandle = item.Allocation.Handle
                });

                _pendingDeletions.Dequeue();
            }
        }
    }

    /// <summary>
    /// Function to commit the allocated data to actual VRAM.
    /// </summary>
    /// <param name="queue">The command queue that will commit the allocations to VRAM.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Commit(CommandQueue queue)
    {
        using (_lock.EnterScope())
        {
            // Dump any tiles that need to be unmapped from the resource.
            ProcessUnmaps(in queue.D3DQueue);

            ProcessPending(in queue.D3DQueue);
        }
    }

    /// <summary>
    /// Function to reclaim memory that is no longer needed.
    /// </summary>
    public void GarbageCollect()
    {
        // Always leave Heap #0 alive.
        for (int i = 1; i < _tiles.Length; ++i)
        {
            Stack<ushort> tileStack = _tiles[i];

            if (tileStack.Count == MaxTilesPerHeap)
            {
                _emptyHeaps.Add(i);
            }
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~MegaBuffer() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="MegaBuffer"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that is associated with this buffer.</param>
    /// <param name="sizePercent">The percentage of VRAM to dedicate, between 10 to 50%. Which can be from 256MB to 4GB depending on available GPU memory.</param>
    public MegaBuffer(GorgonGraphics graphics, int sizePercent)
    {
        D3D12MA_Budget local = default;

        _graphics = graphics;

        int maxAddressingBits = (_graphics.Adapter.MaxAddressBitsPerResource).Min(32);

        if (maxAddressingBits < 32)
        {
            _maxAddressSpace = 1UL << maxAddressingBits;
        }

        uint pageTableCounts = (uint)(_maxAddressSpace >> 16);
        _tileRefCounts = new ushort[pageTableCounts];
        _allocatedTiles = new (ushort HeapIndex, ushort Index)[pageTableCounts];

        _graphics.Allocator.Get()->GetBudget(&local, null);

        decimal percent = sizePercent / 100.0M;
        _size = (ulong)(local.BudgetBytes * percent).Max(HeapSize).Min(_maxAddressSpace);
        _d3dHeaps = new ComPtr<ID3D12Heap>[(int)System.Math.Ceiling((decimal)_size / HeapSize).Max(1)];
        _tiles = new Stack<ushort>[_d3dHeaps.Length];

        for (int i = 0; i < _tiles.Length; ++i)
        {
            _tiles[i] = new Stack<ushort>((int)MaxTilesPerHeap);
        }

        (_d3dBuffer, _memoryBlock) = CreateNative();
    }

    /// <summary>
    /// Initializes the static values for the <see cref="MegaBuffer"/> class.
    /// </summary>
    static MegaBuffer() => Array.Fill<uint>(_tileCounts, 1);
}
