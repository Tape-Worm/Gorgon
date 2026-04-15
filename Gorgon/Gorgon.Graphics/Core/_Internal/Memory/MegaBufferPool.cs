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
// Created: April 11, 2026 2:19:04 PM
//

using System.Diagnostics;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Manages a pool of mega buffers.
/// </summary>
/// <param name="graphics"><inheritdoc cref="MegaBuffer(GorgonGraphics, byte, int)" path="/param[@name='graphics']"/></param>
/// <param name="sizePercent"><inheritdoc cref="MegaBuffer(GorgonGraphics, byte, int)" path="/param[@name='sizePercent']"/></param>
internal class MegaBufferPool(GorgonGraphics graphics, int sizePercent)
    : IDisposable
{
    private readonly GorgonGraphics _graphics = graphics;
    private readonly int _sizePercent = sizePercent;
    private readonly Lock _lock = new();    
    private readonly MegaBuffer?[] _pool = new MegaBuffer[256];     // This gives us a maximum of 256 * 2GB = 512GB of space. If we hit this limit there's no hope.
    private MegaBuffer? _current;
    private int _poolCount;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _graphics.WaitForGpu(GorgonGraphics.WaitFenceTimeout);

            foreach (MegaBuffer? buffer in _pool)
            {
                buffer?.Dispose();
            }

            Array.Clear(_pool);
        }

        _current = null;
        _poolCount = 0;
    }

    /// <summary>
    /// Property to return the resource for the allocation.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="allocation"/> is <see cref="GpuBufferAllocation.Null"/>.</exception>
    public ref readonly ComPtr<ID3D12Resource2> this[ref readonly GpuBufferAllocation allocation]
    {
        get
        {
            if (allocation.Equals(GpuBufferAllocation.Null))
            {
                throw new ArgumentNullException(nameof(allocation));
            }

            MegaBuffer? buffer = _pool[allocation.PoolIndex];

            // If this happens, something's really fucked up and we need figure out why this happened.
            Debug.Assert(buffer is not null, "Null buffer in pool with live allocation.");

            return ref buffer.D3DBuffer;
        }
    }

    /// <inheritdoc cref="MegaBuffer.TryAllocate(ulong, out GpuBufferAllocation, uint)"/>
    public void Allocate(ulong size, out GpuBufferAllocation allocation, uint alignment)
    {
        using (_lock.EnterScope())
        {
            if (alignment == 0)
            {
                alignment = 1;
            }

            size += (alignment - 1);

            // Try to allocate using the current buffer.
            if ((_current is not null) && (_current.FreeSpace >= size) && (_current.TryAllocate(size, out allocation, alignment) == AllocationState.Success))
            {
                return;
            }

            MegaBuffer? current = null;

            // Walk through the buffers to see which has free space.
            for (int i = 0; i < _pool.Length; ++i)
            {
                MegaBuffer? pool = _pool[i];

                // We've found a useful buffer.
                if ((pool is not null) && (pool.FreeSpace >= size))
                {
                    current = pool;
                    break;
                }
            }

            while ((current is null) || (current.TryAllocate(size, out allocation, alignment) == AllocationState.FragmentationError))
            {
                if (_poolCount == _pool.Length)
                {
                    throw new OutOfMemoryException();
                }

                current = _pool[_poolCount] = new MegaBuffer(_graphics, (byte)_poolCount, _sizePercent);
                _poolCount++;
            }

            _current = current;
        }
    }

    /// <inheritdoc cref="MegaBuffer.Free(ref GpuBufferAllocation)"/>
    public void Free(ref GpuBufferAllocation allocation)
    {
        using (_lock.EnterScope())
        {
            if (allocation.Equals(in GpuBufferAllocation.Null))
            {
                return;
            }

            MegaBuffer? megaBuffer = _pool[allocation.PoolIndex];

            if (megaBuffer is null)
            {
                allocation = GpuBufferAllocation.Null;
                return;
            }

            megaBuffer.Free(ref allocation);
        }
    }

    /// <inheritdoc cref="MegaBuffer.Signal()"/>
    public void Signal()
    {
        using (_lock.EnterScope())
        {
            for (int i = 0; i < _poolCount; ++i)
            {
                _pool[i]?.Signal();
            }
        }
    }

    /// <inheritdoc cref="MegaBuffer.Commit(CommandQueue)"/>
    public void Commit(CommandQueue commandQueue)
    {
        using (_lock.EnterScope())
        {
            for (int i = 0; i < _poolCount; ++i)
            {
                _pool[i]?.Commit(commandQueue);
            }
        }
    }

    /// <inheritdoc cref="MegaBuffer.GarbageCollect"/>
    public void GarbageCollect()
    {
        using (_lock.EnterScope())
        {
            if (_poolCount == 0)
            {
                return;
            }

            for (int i = 0; i < _poolCount; ++i)
            {
                MegaBuffer? pool = _pool[i];

                if (pool is null)
                {
                    continue;
                }

                pool.GarbageCollect();

                // Only dump the later pools. Keep the first, we'll still need it and there's no point to reallocating it.
                if ((pool.FreeSpace == pool.SizeInBytes) && (i >= 1))
                {
                    pool.Dispose();
                    _pool[i] = null;
                }
            }

            int index = -1;

            for (int i = _poolCount - 1; i >= 0; --i)
            {
                if (_pool[i] is null)
                {
                    continue;
                }

                index = i;
                break;
            }

            if (index == -1)
            {
                _poolCount = 0;
                _current = null;
                return;
            }

            _poolCount = index + 1;
            _current = _pool[index];
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}