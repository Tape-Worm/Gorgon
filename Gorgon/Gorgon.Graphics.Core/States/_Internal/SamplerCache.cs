
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 4, 2021 12:38:32 PM
// 


using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A cache for holding sampler state objects
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SamplerCache" /> class.</remarks>
/// <param name="device">The D3D11 device object for creating samplers.</param>
internal class SamplerCache(D3D11.Device5 device)
        : IDisposable
{

    // The size of the initial cache.
    private const int InitialCacheSize = 8;



    // The D3D11 device object for creating samplers.
    private readonly D3D11.Device5 _device = device;
    // A list of cached pipeline states.
    private GorgonSamplerState[] _cachedSamplers = new GorgonSamplerState[InitialCacheSize];
    // A syncrhonization lock for multiple thread when dealing with the sampler cache.
    private readonly object _samplerLock = new();



    /// <summary>
    /// Function to invalidate the cache data.
    /// </summary>
    private void InvalidateCache()
    {
        foreach (GorgonSamplerState sampler in _cachedSamplers)
        {
            if (sampler is null)
            {
                break;
            }

            sampler.ID = int.MinValue;
            sampler.Native?.Dispose();
        }

        Array.Clear(_cachedSamplers, 0, _cachedSamplers.Length);

        if (_cachedSamplers.Length != InitialCacheSize)
        {
            Array.Resize(ref _cachedSamplers, InitialCacheSize);
        }
    }

    /// <summary>
    /// Function to cache a sampler state, or return a previously cached sampler state.
    /// </summary>
    /// <param name="newState">The state to cache.</param>
    /// <returns>The cached state.</returns>
    public GorgonSamplerState Cache(GorgonSamplerState newState)
    {
        lock (_samplerLock)
        {
            if ((newState.ID >= 0) && (newState.ID < _cachedSamplers.Length) && (_cachedSamplers[newState.ID] is not null))
            {
                GorgonSamplerState state = _cachedSamplers[newState.ID];

                if ((newState.Native is not null) && (newState.Native == state.Native))
                {
                    return state;
                }

                newState.ID = int.MinValue;
            }

            if (_cachedSamplers[^1] is not null)
            {
                Array.Resize(ref _cachedSamplers, _cachedSamplers.Length + InitialCacheSize);
            }

            int index = 0;
            while (index < _cachedSamplers.Length)
            {
                GorgonSamplerState cached = _cachedSamplers[index];

                if (cached is null)
                {
                    break;
                }

                if (cached?.Equals(newState) ?? false)
                {
                    return cached;
                }

                ++index;
            }

            // We didn't find what we wanted, so create a new one.
            var resultState = new GorgonSamplerState(newState)
            {
                ID = index
            };
            resultState.BuildD3D11SamplerState(_device);
            _cachedSamplers[index] = resultState;

            return resultState;
        }
    }

    /// <summary>
    /// Function to clear the sampler cache.
    /// </summary>
    public void Clear()
    {
        lock (_samplerLock)
        {
            InvalidateCache();
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => InvalidateCache();

    /// <summary>
    /// Function to warm up the cache with predefined states.
    /// </summary>
    public void WarmCache()
    {
        lock (_samplerLock)
        {
            GorgonSamplerState.Default.BuildD3D11SamplerState(_device);
            GorgonSamplerState.AnisotropicFiltering.BuildD3D11SamplerState(_device);
            GorgonSamplerState.PointFiltering.BuildD3D11SamplerState(_device);
            GorgonSamplerState.Wrapping.BuildD3D11SamplerState(_device);
            GorgonSamplerState.PointFilteringWrapping.BuildD3D11SamplerState(_device);

            _cachedSamplers[0] = GorgonSamplerState.Default;
            _cachedSamplers[1] = GorgonSamplerState.Wrapping;
            _cachedSamplers[2] = GorgonSamplerState.AnisotropicFiltering;
            _cachedSamplers[3] = GorgonSamplerState.PointFiltering;
            _cachedSamplers[4] = GorgonSamplerState.PointFilteringWrapping;
        }
    }


}
