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
// Created: October 20, 2025 5:51:04 PM
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
/// A pool for managing D3D 12 command allocators.
/// </summary>
/// <param name="queue">The command queue that owns this allocator pool.</param>
/// <exception cref="GorgonException">Thrown if the fence could not be created.</exception>
internal unsafe sealed class CommandAllocatorPool(CommandQueue queue)
        : IDisposable
{
    private readonly GorgonGraphics _graphics = queue.Graphics;
    private readonly Lock _lock = new();
    private readonly List<CommandAllocator> _free = new(16);
    private readonly Queue<CommandAllocator> _inUse = new(16);
    private readonly List<CommandAllocator> _active = new(16);
    private readonly CommandQueue _queue = queue;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            using (_lock.EnterScope())
            {
                // Ensure the GPU isn't using any allocators.
                while (_inUse.Count > 0)
                {
                    Release();
                }                

                foreach (CommandAllocator allocator in _free.Concat(_active))
                {                    
                    allocator.Dispose();
                }

                _graphics.Log.Print($"Freed {_free.Count + _active.Count} command allocators from the pool.", LoggingLevel.Intermediate);

                _free.Clear();
                _active.Clear();
                _inUse.Clear();
            }
        }
    }

    /// <summary>
    /// Function to retire allocators back into the unused pool.
    /// </summary>
    private void Release()
    {
        if (_inUse.Count == 0)
        {
            return;
        }

        ulong completed = _queue.D3DFence.Get()->GetCompletedValue();

        while(_inUse.TryPeek(out CommandAllocator? allocator))
        {
            if (completed < allocator.Fence)
            {
                break;
            }

            _inUse.Dequeue();            
            _free.Add(allocator);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to return a command allocator from the pool.
    /// </summary>
    /// <param name="name">The name of the command allocator.</param>
    /// <returns>The command allocator.</returns>
    public CommandAllocator Get(string name)
    {   
        using (_lock.EnterScope())
        {
            CommandAllocator? allocator = null;

            if (_free.Count > 0)
            {
                allocator = _free[^1];

                if (allocator.HasCommandList)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_COMMAND_LIST_STILL_OPEN);
                }

                allocator.Name = name;

                _free.RemoveAt(_free.Count - 1);
                _active.Add(allocator);
            }
            else
            {
                allocator = new(_graphics, name, _queue.Type);
                _active.Add(allocator);
            }

            allocator.D3DAllocator.Get()->Reset()
                .ThrowIfFailed(GorgonResult.CannotInitialize, () => Resources.GORGFX_ERR_CANNOT_RESET_CMD_ALLOCATOR);

            return allocator;
        }
    }

    /// <summary>
    /// Function to signal the fences on the allocators to notify that the GPU is finished with them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This prepares the heaps for reuse by marking the heaps with a fence value to indicate that the GPU is done with them.
    /// </para>
    /// </remarks>
    public void Signal()
    {
        using (_lock.EnterScope())
        {
            // Retire any pending allocators that are no longer in use.
            Release();

            if (_active.Count == 0)
            {
                return;
            }

            while (_active.Count > 0)
            {
                int i = _active.Count - 1;
                CommandAllocator allocator = _active[i];
                _active.RemoveAt(i);

                allocator.Fence = _queue.FenceValue;
                _inUse.Enqueue(allocator);
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
                _graphics.Log.Print($"Destroying D3D 12 command allocator {_queue.Type} #{i}...", LoggingLevel.Verbose);
                _free[i].Dispose();
            }

            _free.Clear();
        }
    }
}
