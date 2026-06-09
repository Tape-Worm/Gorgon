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
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
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
using Windows.System.Preview;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// TODO:
/// </summary>
/// <param name="graphics">The graphics interface that is associated with this pool.</param>
internal unsafe sealed class VirtualTextureTilePool(GorgonGraphics graphics)
        : IDisposable
{
    /// <summary>
    /// An allocation record used to track heap allocations for the subresource and region on a virtual texture.
    /// </summary>
    /// <param name="heapTiles">The heap and tiles for the allocation.</param>
    /// <param name="subresourceIndex">The sub resource index that was allocated from.</param>
    /// <param name="resource">The resource that contains the allocation.</param>
    /// <param name="tileBox">The region in the sub resource that was allocated from.</param>
    private class HeapAllocation(List<(int HeapIndex, ulong TileMask)> heapTiles, ref readonly ComPtr<ID3D12Resource2> resource, int subresourceIndex, ref readonly GorgonBox tileBox)
    {
        /// <summary>
        /// The COM pointer for the resource.
        /// </summary>
        public ComPtr<ID3D12Resource2> Resource = resource;

        /// <summary>
        /// The heap and tiles.
        /// </summary>
        public readonly List<(int HeapIndex, ulong TileMask)> HeapTiles = heapTiles;

        /// <summary>
        /// The sub resource index.
        /// </summary>
        public readonly int SubresourceIndex = subresourceIndex;

        /// <summary>
        /// The tile region.
        /// </summary>
        public readonly GorgonBox TileBox = tileBox;
    }

    // Default heap size: 4 MB. This is enough to hold a single 1024x1024 32 bit texture with 1 array level and 1 mip level. 
    // We will use more heaps for larger textures.
    private const ulong HeapSize = 4 * 1024 * 1024;
    // 256MB/heap div 65536 bytes per tile = 4096 tiles per heap.
    private const uint MaxTilesPerHeap = (uint)(HeapSize / 65536);

    private readonly List<ComPtr<ID3D12Heap>> _d3dHeaps = new(64);    

    private readonly List<ulong> _tiles = new(64);
    private readonly GorgonGraphics _graphics = graphics;    
    private readonly Queue<(ulong GfxFence, ulong ComputeFence, ulong CopyFence, ulong Handle)> _pendingDeletions = new(65536);
    private readonly Queue<int> _emptyHeaps = [];
    private readonly Dictionary<ulong, HeapAllocation> _allocations = [];
    private ulong _allocationID = ulong.MaxValue;
    private readonly Lock _signalLock = new();
    private readonly Queue<HeapAllocation> _mapQueue = [];
    private readonly Queue<HeapAllocation> _unmapQueue = [];

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _graphics.Log.Print($"Destroying virtual texture tile pool.", LoggingLevel.Verbose);

            while (_mapQueue.TryDequeue(out HeapAllocation? alloc))
            {
                alloc.Resource.Dispose();
            }

            while (_unmapQueue.TryDequeue(out HeapAllocation? alloc))
            {
                alloc.Resource.Dispose();
            }

            _mapQueue.Clear();
            _unmapQueue.Clear();
            _tiles.Clear();
            _emptyHeaps.Clear();
            _pendingDeletions.Clear();

            for (int i = 0; i < _d3dHeaps.Count; i++)
            {
                _d3dHeaps[i].Dispose();
            }
        }
    }

    /// <summary>
    /// Function to create a D3D heap for the pool.
    /// </summary>
    /// <returns>The index of the heap in the heap list.</returns>
    private int CreateHeap()
    {
        ComPtr<ID3D12Heap> result = default;
        int heapIndex = _d3dHeaps.Count;

        if (_emptyHeaps.Count > 0)
        {
            int currentHeapIndex = _emptyHeaps.Dequeue();

            if (currentHeapIndex < _d3dHeaps.Count)
            {
                result = _d3dHeaps[currentHeapIndex];
                heapIndex = currentHeapIndex;
            }
        }

        if (result.IsNull)
        {
            string name = $"Gorgon Virtual Texture Tile Pool Heap #{heapIndex + 1} ({HeapSize.FormatMemory()})";

            D3D12_HEAP_DESC desc = new(HeapSize, D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT, 0, D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_ALLOW_ALL_BUFFERS_AND_TEXTURES);

            _graphics.Log.Print($"Creating '{name}'...", LoggingLevel.Verbose);

            _graphics.D3DDevice.Get()->CreateHeap(&desc, Win32.__uuidof<ID3D12Heap>(), (void**)result.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_HEAP, name));

            result.SetD3DDebugName(name);

            if ((heapIndex < _d3dHeaps.Count) && (_d3dHeaps[heapIndex].IsNull))
            {
                _d3dHeaps[heapIndex] = result;
            }
            else
            {
                _d3dHeaps.Add(result);
            }
        }

        if (heapIndex >= _tiles.Count)
        {
            _tiles.Add(0);
        }
        else
        {
            _tiles[heapIndex] = 0;
        }

        return heapIndex;
    }

    /// <summary>
    /// Function to processed any pending mapped tiles.
    /// </summary>
    /// <param name="queue">The command queue used to map the tiles and heap to the resource.</param>
    private void ProcessMapped(ref readonly ComPtr<ID3D12CommandQueue> queue)
    {
        if (_mapQueue.Count == 0)
        {
            return;
        }

        D3D12_TILED_RESOURCE_COORDINATE* coords = stackalloc D3D12_TILED_RESOURCE_COORDINATE[64];
        D3D12_TILE_REGION_SIZE* ranges = stackalloc D3D12_TILE_REGION_SIZE[64];

        while (_mapQueue.TryDequeue(out HeapAllocation? alloc))
        {
            Debug.Assert(alloc is not null, "Recorded allocation is null!");

            try
            {
                // If, by some weird reason, we don't have tiles to map, then move on.
                if (alloc.HeapTiles.Count == 0)
                {
                    continue;
                }

                ref readonly GorgonBox tileBox = ref alloc.TileBox;
                uint tileSlice = (uint)(tileBox.Width * tileBox.Height);
                uint destinationOffset = 0;

                for (int ht = 0; ht < alloc.HeapTiles.Count; ht++)
                {
                    (int heapIndex, ulong tileMask) = alloc.HeapTiles[ht];
                    uint heapOffset = (uint)BitOperations.TrailingZeroCount(tileMask);
                    uint count = (uint)BitOperations.PopCount(tileMask);
                    uint sourceOffset = 0;
                    uint destOffset = 0;

                    while (sourceOffset < count)
                    {
                        uint z = destinationOffset / tileSlice;
                        uint y = (destinationOffset % tileSlice) / (uint)tileBox.Width;
                        uint x = destinationOffset % (uint)tileBox.Width;
                        uint rowTileCount = ((uint)tileBox.Width - x).Min(count - sourceOffset);

                        // Do copy here.
                        coords[destOffset] = new D3D12_TILED_RESOURCE_COORDINATE((uint)(tileBox.X + x), (uint)(tileBox.Y + y), (uint)(tileBox.Z + z), (uint)alloc.SubresourceIndex);
                        ranges[destOffset++] = new D3D12_TILE_REGION_SIZE(rowTileCount, true, rowTileCount, 1, 1);

                        sourceOffset += rowTileCount;
                        destinationOffset += rowTileCount;
                    }

                    // Do update here.
                    queue.Get()->UpdateTileMappings((PID3D12Resource2)alloc.Resource.Get(), destOffset, coords, ranges,
                        _d3dHeaps[heapIndex].Get(), 
                        1, null, &heapOffset, &count, 
                        D3D12_TILE_MAPPING_FLAGS.D3D12_TILE_MAPPING_FLAG_NONE);                    
                }
            }
            finally
            {
                if (!alloc.Resource.IsNull)
                {
                    alloc.Resource.Get()->Release();
                }
            }
        }
    }

    /// <summary>
    /// Function to processed any pending unmapped tiles.
    /// </summary>
    /// <param name="queue">The command queue used to unmap the tiles and heap from the resource.</param>
    private void ProcessUnmapped(ref readonly ComPtr<ID3D12CommandQueue> queue)
    {
        if (_unmapQueue.Count == 0)
        {
            return;
        }

        while (_unmapQueue.TryDequeue(out HeapAllocation? alloc))
        {
            Debug.Assert(alloc is not null, "Recorded allocation is null!");

            try
            {
                ref readonly GorgonBox tileBox = ref alloc.TileBox;                
                uint tileSize = (uint)(tileBox.Width * tileBox.Height * tileBox.Depth);

                D3D12_TILED_RESOURCE_COORDINATE coord = new((uint)tileBox.X, (uint)tileBox.Y, (uint)tileBox.Z, (uint)alloc.SubresourceIndex);
                D3D12_TILE_REGION_SIZE range = new(tileSize, true, (uint)tileBox.Width, (ushort)tileBox.Height, (ushort)tileBox.Depth);
                D3D12_TILE_RANGE_FLAGS flags = D3D12_TILE_RANGE_FLAGS.D3D12_TILE_RANGE_FLAG_NULL;

                // Do update here.
                queue.Get()->UpdateTileMappings((PID3D12Resource2)alloc.Resource.Get(), 1, &coord, &range,
                    null,
                    1, &flags, null, null,
                    D3D12_TILE_MAPPING_FLAGS.D3D12_TILE_MAPPING_FLAG_NONE);
            }
            finally
            {
                if (!alloc.Resource.IsNull)
                {
                    alloc.Resource.Get()->Release();
                }
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to sub allocate tiles from the virtual texture tile pool.
    /// </summary>
    /// <param name="resource">The resource to allocate for.</param>
    /// <param name="tileCount">The number of tiles to allocate</param>
    /// <param name="subResourceIndex">The texture sub resource index for the allocation.</param>
    /// <param name="tileBox">The region of tiles to allocate.</param>
    /// <returns>The handle for the allocation.</returns>
    public ulong Allocate(ref readonly ComPtr<ID3D12Resource2> resource, uint tileCount, int subResourceIndex, ref readonly GorgonBox tileBox)
    {
        int startHeap = 0;

        List<(int HeapIndex, ulong TileMask)> heapAllocations = [];        

        while (tileCount > 0)
        {
            uint tiles = tileCount < 64 ? tileCount : 64;
            int heapIndex = -1;
            ulong tileCountBits = tiles == 64 ? ulong.MaxValue : (1UL << (int)tiles) - 1;

            Span<ulong> tileList = CollectionsMarshal.AsSpan(_tiles);

            // Locate a heap with enough free space at the end of its bitmask.
            for (int i = startHeap; i < tileList.Length; ++i)
            {
                ref ulong value = ref tileList[i];
                ulong mask = ~value;

                int bitIndex = BitOperations.TrailingZeroCount(mask);
                int bitCount = BitOperations.PopCount(mask);
                ulong shifted = tileCountBits << bitIndex;

                if ((bitCount < tiles) || ((shifted & value) != 0))
                {
                    continue;
                }

                value |= shifted;
                heapIndex = i;
                heapAllocations.Add((heapIndex, shifted));
                break;
            }

            if (heapIndex == -1)
            {
                heapIndex = CreateHeap();
                _tiles[heapIndex] = tileCountBits;
                heapAllocations.Add((heapIndex, tileCountBits));
            }

            // If we've not filled the heap, keep this one and reuse on the next pass (if we can).
            if (tiles < 64)
            {
                startHeap = heapIndex;
            }

            tileCount -= tiles;
        }

        unchecked
        {   
            HeapAllocation alloc = new(heapAllocations, in resource, subResourceIndex, in tileBox);
            _allocations[++_allocationID] = alloc;

            alloc.Resource.Get()->AddRef();
            _mapQueue.Enqueue(alloc);

            return _allocationID;
        }
    }

    /// <summary>
    /// Function to free an allocation for a set of tiles on a virtual texture.
    /// </summary>
    /// <param name="handle">The allocation handle to free.</param>
    public void Free(ulong handle)
    {
        if (!_allocations.ContainsKey(handle))
        {
            return;
        }

        _pendingDeletions.Enqueue((_graphics.Queues.GraphicsQueue.FenceValue,
                                    _graphics.Queues.ComputeQueue.FenceValue,
                                    _graphics.Queues.CopyQueue.FenceValue, handle));
    }

    /// <summary>
    /// Function to signal the mega buffer that it is time to collect any pending deallocations.
    /// </summary>
    public void Signal()
    {
        ulong gfxCompleted = _graphics.Queues.GraphicsQueue.D3DFence.Get()->GetCompletedValue();
        ulong computeCompleted = _graphics.Queues.ComputeQueue.D3DFence.Get()->GetCompletedValue();
        ulong copyCompleted = _graphics.Queues.CopyQueue.D3DFence.Get()->GetCompletedValue();

        using (_signalLock.EnterScope())
        {
            while (_pendingDeletions.TryPeek(out (ulong GfxFence, ulong ComputeFence, ulong CopyFence, ulong Handle) item))
            {
                // If we hit a fence that's greater than what's completed, then leave it be until the next pass.
                if ((gfxCompleted < item.GfxFence) || (computeCompleted < item.ComputeFence) || (copyCompleted < item.CopyFence))
                {
                    break;
                }

                _allocations.TryGetValue(item.Handle, out HeapAllocation? heaps);

                Debug.Assert(heaps is not null, $"The allocation handle 0x{item.Handle.FormatHex()} is not valid.");

                Span<ulong> tiles = CollectionsMarshal.AsSpan(_tiles);

                for (int i = 0; i < heaps.HeapTiles.Count; i++)
                {
                    (int heapIndex, ulong tileMask) = heaps.HeapTiles[i];

                    ref ulong tileValue = ref tiles[heapIndex];

                    tileValue &= ~tileMask;

                    if (tileValue == 0)
                    {
                        _emptyHeaps.Enqueue(heapIndex);
                    }
                }
                                
                heaps.HeapTiles.Clear();
                // Ensure the resource lives until we can unmap it.
                heaps.Resource.Get()->AddRef();

                _unmapQueue.Enqueue(heaps);
                _pendingDeletions.Dequeue();
            }
        }
    }

    /// <summary>
    /// Function to reclaim memory that is no longer needed.
    /// </summary>
    public void GarbageCollect()
    {
        using (_signalLock.EnterScope())
        {
            ulong[] allocationHandles = ArrayPool<ulong>.Shared.Rent(_allocations.Count);

            try
            {
                int deallocCount = 0;

                foreach (KeyValuePair<ulong, HeapAllocation> allocation in _allocations)
                {
                    if (allocation.Value.HeapTiles.Count == 0)
                    {
                        allocationHandles[deallocCount++] = allocation.Key;
                    }
                }

                for (int i = 0; i < deallocCount; ++i)
                {
                    if (_allocations.Remove(allocationHandles[i], out HeapAllocation? value))
                    {
                        value.Resource = default;
                    }
                }
            }
            finally
            {
                ArrayPool<ulong>.Shared.Return(allocationHandles);
            }

            while (_emptyHeaps.TryDequeue(out int heapIndex))
            {
                _d3dHeaps[heapIndex].Dispose();
                _d3dHeaps[heapIndex] = default;
            }

            if (_allocations.Count != 0)
            {
                return;
            }

            _allocationID = ulong.MaxValue;
        }
    }

    /// <summary>
    /// Function to commit reserved virtual memory to physically allocated memory, and handle any pending unmap operations.
    /// </summary>
    /// <param name="queue">The command queue calling this method.</param>
    public void Commit(CommandQueue queue)
    {
        using (_signalLock.EnterScope())
        {
            ProcessUnmapped(in queue.D3DQueue);
            ProcessMapped(in queue.D3DQueue);
        }
    }
}
