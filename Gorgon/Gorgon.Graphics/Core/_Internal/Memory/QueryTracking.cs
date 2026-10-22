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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 16, 2026 10:22:34 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides functionality to perform tracking of in flight queries.
/// </summary>
/// <param name="queue">The queue for the query.</param>
internal unsafe sealed class QueryTracking(CommandQueue queue)
    : IDisposable
{
    private readonly List<(GorgonQuery Query, int Index)> _available = [];
    private readonly Queue<(GorgonQuery Query, int Index, ulong Fence)> _inUse = [];
    private readonly CommandQueue _queue = queue;
    private readonly GorgonGraphics _graphics = queue.Graphics;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            while (_inUse.Count > 0)
            {
                Release();
            }
        }
    }

    /// <summary>
    /// Function to release any query slots that are no longer in use by the GPU.
    /// </summary>
    private void Release()
    {
        if (_inUse.Count == 0)
        {
            return;
        }

        ulong completed = _queue.D3DFence.Get()->GetCompletedValue();

        while (_inUse.TryPeek(out (GorgonQuery Query, int Index, ulong Fence) query))
        {
            Debug.Assert(query.Fence != 0, "The query does not have a valid fence value. Cannot safely remove this query.");

            if (completed < query.Fence)
            {
                break;
            }

            query.Query.Free(query.Index);
            _inUse.Dequeue();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to add a resolved query index to the tracking list.
    /// </summary>
    /// <param name="query">The query containing the index.</param>
    /// <param name="index">The index to capture.</param>
    public void AddResolvedQueryIndex(GorgonQuery query, int index) => _available.Add((query, index));

    /// <summary>
    /// Function to signal which queries are in flight.
    /// </summary>
    public void Signal()
    {
        Release();

        if (_available.Count == 0)
        {
            return;
        }

        while (_available.Count > 0)
        {
            int i = _available.Count - 1;
            (GorgonQuery query, int index) = _available[i];
            _available.RemoveAt(i);

            _inUse.Enqueue((query, index, _queue.FenceValue));
        }
    }
}
