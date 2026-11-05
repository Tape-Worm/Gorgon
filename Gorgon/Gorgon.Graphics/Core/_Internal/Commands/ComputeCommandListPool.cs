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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 20, 2025 5:51:04 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pool for managing compute specific command lists.
/// </summary>
/// <param name="computeEngine">The compute engine for this pool.</param>
internal sealed class ComputeCommandListPool(GorgonComputeEngine computeEngine)
{
    private readonly GorgonComputeEngine _compute = computeEngine;
    private readonly CommandQueue _queue = computeEngine.Queues.ComputeQueue;
    private readonly Lock _lock = new();
    private readonly List<GorgonComputeCommandList> _free = new(32);
    private readonly List<GorgonComputeCommandList> _active = new(32);

    /// <summary>
    /// Function to return a command allocator from the pool.
    /// </summary>
    /// <param name="name">The name of the command list.</param>
    /// <returns>The command allocator.</returns>
    /// <exception cref="GorgonException">Thrown if the command list fails its reset operation.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonComputeCommandList Get(string name)
    {
        // These items are locked on their own, no need to double lock.
        CommandAllocator allocator = _queue.AllocatorPool.Get(name);
        GorgonCommandList parentList = _queue.ListPool.Get(name, allocator);

        using (_lock.EnterScope())
        {
            GorgonComputeCommandList list;

            if (_free.Count > 0)
            {
                list = _free[^1];
                list.ResetState(name, parentList);

                _free.RemoveAt(_free.Count - 1);
                _active.Add(list);
            }
            else
            {
                list = new GorgonComputeCommandList(_compute, name, parentList);
                _active.Add(list);
            }

            return list;
        }
    }

    /// <summary>
    /// Function to return the command list to the pool when we are done with it.
    /// </summary>
    /// <param name="list">The command list to return.</param>
    public void Return(GorgonComputeCommandList list)
    {
        Debug.Assert(!list.IsRecording, $"The command list '{list.Name}' is open and cannot be recycled.");

        using (_lock.EnterScope())
        {
            if (_active.Remove(list))
            {
                list.ResetState(list.Name, null);
                _free.Add(list);
            }
        }
    }
}
