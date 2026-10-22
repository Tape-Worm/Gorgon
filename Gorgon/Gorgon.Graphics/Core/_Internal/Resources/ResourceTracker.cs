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
// Created: January 31, 2026 12:46:11 AM
//

using System;
using System.Collections.Generic;
using System.Threading;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to track resources when in flight.
/// </summary>
/// <param name="queue">The command queue that owns this tracker.</param>
internal unsafe class ResourceTracker(CommandQueue queue)
        : IDisposable
{
    private readonly CommandQueue _queue = queue;
    private readonly List<(ulong Fence, ComPtr<IUnknown> Resource)> _trackedResources = new(4096);
    private readonly Lock _fenceLock = new();

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            using (_fenceLock.EnterScope())
            {
                for (int i = 0; i < _trackedResources.Count; ++i)
                {
                    _trackedResources[i].Resource.Dispose();
                }
                _trackedResources.Clear();
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
    /// Function to track a batch of resources recorded by a command list.
    /// </summary>
    /// <param name="resources">The native pointers for the resources to track.</param>
    public void TrackResources(HashSet<nint> resources)
    {
        if (resources.Count == 0)
        {
            return;
        }

        using (_fenceLock.EnterScope())
        {
            ulong fence = _queue.FenceValue + 1;

            foreach (nint resource in resources)
            {
                ComPtr<IUnknown> unk = new((IUnknown*)resource);
                _trackedResources.Add(new(fence, unk));
            }
        }
    }

    /// <summary>
    /// Function to tell the parent queue to signal that the items are no longer in flight.
    /// </summary>
    public void Signal()
    {
        using (_fenceLock.EnterScope())
        {
            ulong currentFence = _queue.D3DFence.Get()->GetCompletedValue();
            int i;

            for (i = 0; i < _trackedResources.Count; ++i)
            {
                (ulong fence, ComPtr<IUnknown> resource) = _trackedResources[i];

                if (currentFence < fence)
                {
                    // We're not done with this one yet.
                    break;
                }

                resource.Dispose();
            }

            _trackedResources.RemoveRange(0, i);
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~ResourceTracker() => Dispose(false);
}

