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
// Created: January 31, 2026 12:46:11 AM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to track resources when in flight.
/// </summary>
/// <param name="queue">The command queue that owns this tracker.</param>
internal unsafe class ResourceTracker(CommandQueue queue)
        : IDisposable
{
    private readonly CommandQueue _queue = queue;
    private readonly List<(ulong Fence, ComPtr<IUnknown> Resource)> _trackedResources = [];
    private readonly Lock _fenceLock = new();

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _queue.WaitForGpu(GorgonGraphics.WaitFenceTimeout);

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
    /// Function to track a resource used by the command list on the GPU.
    /// </summary>
    /// <typeparam name="T">The type of COM object to track.</typeparam>
    /// <param name="resource">The resource to track.</param>
    /// <remarks>
    /// <para>
    /// Objects in use by the GPU must be tracked so they won't be destroyed while in use.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrackResource<T>(ComPtr<T> resource)
        where T : unmanaged, IUnknown.Interface
    {
        using (_fenceLock.EnterScope())
        {
            if (resource.IsNull)
            {
                return;
            }

            ComPtr<IUnknown> unk = new((IUnknown*)resource.Get());
            _trackedResources.Add(new(_queue.FenceValue, unk));
        }
    }

    /// <inheritdoc cref="TrackResource{T}(ComPtr{T})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrackResource(GorgonGpuResource resource) => TrackResource(resource.D3DResource);

    /// <summary>
    /// Function to tell the parent queue to signal that the items are no longer in flight.
    /// </summary>
    public void Signal()
    {
        using (_fenceLock.EnterScope())
        {
            for(int i = _trackedResources.Count - 1; i >= 0; --i)
            {
                ulong currentFence = _queue.D3DFence.Get()->GetCompletedValue();
                (ulong fence, ComPtr<IUnknown> resource) = _trackedResources[i];

                if (currentFence < fence)
                {
                    // We're not done with this one yet.
                    continue;
                }
                                
                resource.Dispose();
                _trackedResources.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~ResourceTracker() => Dispose(false);
}

