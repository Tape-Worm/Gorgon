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
// Created: March 21, 2026 1:07:03 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Stores current state for all resources for a given <see cref="GorgonGraphics"/> instance.
/// </summary>
internal class GlobalBarrierState
{
    private const int InitialPageCount = 4096;
    private const int MaxPageSize = 65536;

    private readonly Lock _syncLock = new();
    private readonly GlobalBarrier[][] _barriers = new GlobalBarrier[InitialPageCount][];

    /// <summary>
    /// Function to retrieve the state for a given resource.
    /// </summary>
    /// <param name="resourceID">The ID of the resource.</param>
    /// <returns>A read only reference to its current state.</returns>
    /// <remarks>
    /// <para>
    /// If no state is found, then a default value is returned.
    /// </para>
    /// </remarks>
    public ref readonly GlobalBarrier GetState(ulong resourceID)
    {
        using (_syncLock.EnterScope())
        {
            Debug.Assert(resourceID < 268_435_456, $"The resource ID {resourceID} is really large. This may be a broken resource ID, or the application has too many resources. Please eliminate 3. P.S. I am not a crackpot");

            ulong page = resourceID / MaxPageSize;
            ulong barrierIndex = resourceID % MaxPageSize;

            GlobalBarrier[]? barriers = _barriers[page];

            if ((barriers is null) || (barriers.Length == 0))
            {
                return ref GlobalBarrier.Empty;
            }

            ref readonly GlobalBarrier barrier = ref barriers[barrierIndex];

            // This means not initialized.
            if (barrier.SubResources is null)
            {
                return ref GlobalBarrier.Empty;
            }

            return ref barrier;            
        }
    }

    /// <summary>
    /// Function to capture the current state for resource barriers from a command list.
    /// </summary>
    /// <param name="barriers">The barriers to evaluate.</param>
    public void CaptureBufferState(Dictionary<ulong, GorgonBufferBarrier> barriers)
    {
        using (_syncLock.EnterScope())
        {
            foreach (KeyValuePair<ulong, GorgonBufferBarrier> barrier in barriers)
            {
                ulong page = barrier.Key / MaxPageSize;
                ulong barrierIndex = barrier.Key % MaxPageSize;

                Debug.Assert(page < (ulong)_barriers.Length, $"The page index {page} is outside of the total number of pages {_barriers.Length}.");

                GlobalBarrier[]? gBarriers = _barriers[page];

                gBarriers ??= _barriers[page] = new GlobalBarrier[MaxPageSize];

                ref GlobalBarrier gBarrier = ref gBarriers[barrierIndex];

                if (gBarrier.SubResources is null)
                {
                    gBarrier = new GlobalBarrier(barrier.Value.Sync, barrier.Value.Access, BarrierLayout.None);
                }
                else
                {
                    gBarrier.Sync = barrier.Value.Sync;
                    gBarrier.Access = barrier.Value.Access;
                    gBarrier.SubResources.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Function to capture the current state for resource barriers from a command list.
    /// </summary>
    /// <param name="barriers">The barriers to evaluate.</param>
    /// <param name="subResources">The list of sub resources for the barrier.</param>
    public void CaptureTextureState(Dictionary<ulong, GorgonTextureBarrier> barriers, Dictionary<ulong, List<GorgonSubResourceRange>> subResources)
    {
        using (_syncLock.EnterScope())
        {
            foreach (KeyValuePair<ulong, GorgonTextureBarrier> barrier in barriers)
            {
                ulong page = barrier.Key / MaxPageSize;
                ulong barrierIndex = barrier.Key % MaxPageSize;

                Debug.Assert(page < (ulong)_barriers.Length, $"The page index {page} is outside of the total number of pages {_barriers.Length}.");

                GlobalBarrier[]? gBarriers = _barriers[page];

                gBarriers ??= _barriers[page] = new GlobalBarrier[MaxPageSize];

                ref GlobalBarrier newBarrier = ref gBarriers[barrierIndex];

                if (newBarrier.SubResources is null)
                {
                    newBarrier = new(barrier.Value.Sync, barrier.Value.Access, barrier.Value.Layout);
                }
                else
                {
                    newBarrier.Sync = barrier.Value.Sync;
                    newBarrier.Access = barrier.Value.Access;
                    newBarrier.Layout = barrier.Value.Layout;
                    newBarrier.SubResources.Clear();
                }

                List<GorgonSubResourceRange> srcRanges = subResources[barrier.Key];

                for (int i = 0; i < srcRanges.Count; ++i)
                {                    
                    // This complains of a null ref on subresources (which is true if you use 'default').
                    // So we keep the check here to make VS happy.
                    newBarrier.SubResources?.Add(srcRanges[i]);
                }               
            }
        }
    }
}
