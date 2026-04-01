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
/// A pool for managing D3D 12 command lists.
/// </summary>
/// <param name="graphics">The graphics interface that owns this allocator.</param>
/// <param name="queue">The command queue that owns this pool.</param>
internal unsafe sealed class CommandListPool(GorgonGraphics graphics, CommandQueue queue)
    : IDisposable
{
    private readonly GorgonGraphics _graphics = graphics;
    private readonly CommandQueue _queue = queue;
    private readonly Lock _lock = new();
    private readonly List<GorgonCommandList> _free = new(32);
    private readonly List<GorgonCommandList> _active = new(32);

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            using (_lock.EnterScope())
            {
                foreach (IDisposable list in _free.Concat(_active))
                {
                    list.Dispose();
                }

                _graphics.Log.Print($"Freed {_free.Count + _active.Count} command list(s) from the pool.", LoggingLevel.Intermediate);

                _free.Clear();
                _active.Clear();
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
    /// Function to return a command allocator from the pool.
    /// </summary>
    /// <param name="name">The name of the command list.</param>
    /// <param name="allocator">The command allocator used to reset the list.</param>
    /// <returns>The command allocator.</returns>
    /// <exception cref="GorgonException">Thrown if the command list fails its reset operation.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonCommandList Get(string name, CommandAllocator allocator)
    {   
        using (_lock.EnterScope())
        {
            Debug.Assert(allocator.Type == _queue.Type, $"The command allocator and command list types are mismatched. {allocator.Type} != {_queue.Type}");

            GorgonCommandList list;

            if (_free.Count > 0)
            {
                list = _free[^1];
                list.ResetState(name, allocator);

                _free.RemoveAt(_free.Count - 1);
                _active.Add(list);
            }
            else 
            {
                list = new GorgonCommandList(_graphics, name, allocator, _queue);
                _active.Add(list);
            }

            list.D3DGraphicsCommandList.Get()->Reset(allocator.D3DAllocator.Get(), null)
                .ThrowIfFailed(GorgonResult.CannotInitialize, () => Resources.GORGFX_ERR_CANNOT_RESET_COMMAND_LIST);            

            return list;
        }
    }

    /// <summary>
    /// Function to return the command list to the pool when we are done with it.
    /// </summary>
    /// <param name="list">The command list to return.</param>
    public void Return(GorgonCommandList list)
    {
        using (_lock.EnterScope())
        {
            if (_active.Remove(list))
            {
                list.ResetState(list.Name, null);
                _free.Add(list);
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

            int index = 0;
            foreach (IDisposable list in _free)
            {
                _graphics.Log.Print($"Destroying D3D 12 command list {_queue.Type} #{index++}...", LoggingLevel.Verbose);
                list.Dispose();
            }

            _free.Clear();
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~CommandListPool() => Dispose(false);
}
