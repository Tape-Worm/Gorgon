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
// Created: July 20, 2026 4:58:49 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for factories that create, cache, and reuse pipeline state objects.
/// </summary>
/// <typeparam name="TPso">The type of pipeline state object stored in the factory.</typeparam>
/// <param name="graphics">The graphics interface that is associated with this factory.</param>
/// <remarks>
/// <para>
/// This is the base class for the <see cref="GorgonGraphicsPsoFactory"/> and the <see cref="GorgonComputePsoFactory"/>. It stores the pipeline state objects by name, along with the shaders they use, and 
/// provides the methods to look up the pipeline state objects, and to flush the factory. 
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsPsoFactory"/>
/// <seealso cref="GorgonComputePsoFactory"/>
public abstract unsafe class GorgonPsoFactoryCommon<TPso>(GorgonGraphics graphics)
    : IDisposable
    where TPso : class, IGorgonNamedObject
{
    private readonly Lock _cacheLock = new();
    private readonly Dictionary<string, TPso> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<GorgonShader> _shaderCache = [];

    /// <summary>
    /// Property to return the pipeline state objects that are stored in the factory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pipeline state objects are keyed by name, and the names are not case sensitive.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The dictionary returned is not thread safe. Do not create pipeline state objects, or flush the factory, in other threads while iterating through the dictionary.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, TPso> PipelineStateObjects => _cache;

    /// <summary>
    /// Property to return the graphics interface associated with this factory.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);
            FlushCache();
        }
    }

    /// <summary>
    /// Function to compile the pipeline state object.
    /// </summary>
    /// <param name="pso">The PSO to build from.</param>
    /// <remarks>
    /// <para>
    /// This will rebuild the pipeline state object on the GPU and prepare it for use.
    /// </para>
    /// </remarks>
    private protected abstract void OnCompile(TPso pso);

    /// <summary>
    /// Function called to unload a previous loaded PSO.
    /// </summary>
    /// <param name="pso">The PSO to unload.</param>
    private protected abstract void OnUnload(TPso pso);

    /// <summary>
    /// Function called when the cache is flushed.
    /// </summary>
    private protected virtual void OnFlush()
    {
    }

    /// <summary>
    /// Function called when a shader or shaders from a PSO need to be cached.
    /// </summary>
    /// <param name="pso">The PSO which contains the shaders to add to the cache.</param>
    /// <param name="shaderList">The list of cached shaders to update.</param>
    private protected abstract void OnAddShaderToCache(TPso pso, HashSet<GorgonShader> shaderList);

    /// <summary>
    /// Function to add a PSO and its shaders to the cache.
    /// </summary>
    /// <param name="pso">The PSO with the shaders to add.</param>
    /// <returns>The cache object.</returns>
    private protected TPso Cache(TPso pso)
    {
        OnCompile(pso);

        using (_cacheLock.EnterScope())
        {
            // Check to ensure that a thread hasn't already created our PSO.
            if (_cache.TryGetValue(pso.Name, out TPso? existing))
            {
                OnUnload(pso);
                return existing;
            }

            _cache[pso.Name] = pso;

            OnAddShaderToCache(pso, _shaderCache);

            return pso;
        }
    }

    /// <summary>
    /// Function to locate a cached shader by using its hash code and binary blob.
    /// </summary>
    /// <param name="hashCode">The hashcode of the shader to look up.</param>
    /// <param name="blob">The binary blob to compare.</param>
    /// <returns>A <see cref="GorgonShader"/> matching the hashcode and blob data, or <b>null</b> if no shader was found in the cache.</returns>
    internal GorgonShader? FindShaderWithSignature(int hashCode, ReadOnlySpan<byte> blob)
    {
        using (_cacheLock.EnterScope())
        {
            if (_shaderCache.Count == 0)
            {
                return null;
            }

            foreach (GorgonShader cachedShader in _shaderCache)
            {
                if (cachedShader.CheckShaderBlob(hashCode, blob))
                {
                    return cachedShader;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Function to retrieve a cached PSO by its name.
    /// </summary>
    /// <param name="name">The name of the PSO to return.</param>
    /// <param name="pso">The PSO if found, or <b>null</b> if not.</param>
    /// <returns><b>true</b> if the PSO was found in the cache, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected bool TryGetCachedPso(string name, [NotNullWhen(true)] out TPso? pso)
    {
        using (_cacheLock.EnterScope())
        {
            return _cache.TryGetValue(name, out pso);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to flush the factory, and remove all of the stored pipeline state objects, and <see cref="GorgonShader"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will wait for the GPU to finish its work on all queues, and then destroy the underlying state for every pipeline state object in the factory. If the application is still holding on to a 
    /// pipeline state object after this method is called, it can no longer be used, and its <c>IsCompiled</c> property will return <b>false</b>.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Because this method waits for the GPU to finish all of its work, it should not be called during rendering.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShader"/>
    public void FlushCache()
    {
        Graphics.Log.Print("Flushing the Pipeline State Object cache and Shader cache.", LoggingLevel.Simple);

        using (_cacheLock.EnterScope())
        {
            if (_cache.Count == 0)
            {
                return;
            }

            Graphics.WaitForGpu();

            foreach (TPso pso in _cache.Values)
            {
                OnUnload(pso);
            }

            _cache.Clear();
            _shaderCache.Clear();

            OnFlush();
        }
    }

    /// <summary>
    /// Function to determine if the requested pipeline state object exists in the factory.
    /// </summary>
    /// <param name="psoName">The name of the pipeline state object to look up.</param>
    /// <returns><b>true</b> if the pipeline state object is in the factory, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="psoName"/> is not case sensitive. If the <paramref name="psoName"/> is empty, then this method will return <b>false</b>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasPso(string psoName)
    {
        if (string.IsNullOrWhiteSpace(psoName))
        {
            return false;
        }

        using (_cacheLock.EnterScope())
        {
            return _cache.ContainsKey(psoName);
        }
    }

    /// <summary>
    /// Function to attempt to return a pipeline state object from the factory.
    /// </summary>
    /// <param name="psoName">The name of the pipeline state object to return.</param>
    /// <param name="pso">The pipeline state object if found, or <b>null</b> if not.</param>
    /// <returns><b>true</b> if the pipeline state object was found, or <b>false</b> if not.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="psoName"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="psoName"/> is not case sensitive.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetPso(string psoName, [NotNullWhen(true)] out TPso? pso)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(psoName);

        using (_cacheLock.EnterScope())
        {
            return _cache.TryGetValue(psoName, out pso);
        }
    }

    /// <summary>
    /// Function to return a pipeline state object from the factory.
    /// </summary>
    /// <param name="psoName">The name of the pipeline state object to return.</param>
    /// <returns>The pipeline state object with the <paramref name="psoName"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="psoName"/> is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no pipeline state object with the <paramref name="psoName"/> was found.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="psoName"/> is not case sensitive.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TPso GetPso(string psoName)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(psoName);

        using (_cacheLock.EnterScope())
        {
            return !_cache.TryGetValue(psoName, out TPso? pso) ? throw new KeyNotFoundException() : pso;
        }
    }

    /// <summary>
    /// Function to iterate through the pipeline state objects in the factory, and find one based on the predicate filter passed.
    /// </summary>
    /// <param name="predicate">The predicate filter used to locate the pipeline state object.</param>
    /// <returns>The first pipeline state object that matches the <paramref name="predicate"/>, or <b>null</b> if no match was found.</returns>
    public TPso? FindPso(Func<TPso, bool> predicate)
    {
        using (_cacheLock.EnterScope())
        {
            if (_cache.Count == 0)
            {
                return null;
            }

            foreach (TPso pso in _cache.Values)
            {
                if (predicate(pso))
                {
                    return pso;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Function to retrieve a copy of the unique shaders used by all of the pipeline state objects in the factory.
    /// </summary>
    /// <returns>A dictionary containing each shader, and an index ID for that shader.</returns>
    /// <remarks>
    /// <para>
    /// The index IDs are sequential, start at 0, and are only valid for the returned dictionary.
    /// </para>
    /// </remarks>
    public Dictionary<GorgonShader, int> GetShaders()
    {
        using (_cacheLock.EnterScope())
        {
            Dictionary<GorgonShader, int> result = [];

            int i = 0;
            foreach (GorgonShader shader in _shaderCache)
            {
                result[shader] = i++;
            }

            return result;
        }
    }
}
