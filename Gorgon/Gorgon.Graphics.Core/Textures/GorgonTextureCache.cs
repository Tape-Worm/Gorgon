
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: June 21, 2020 12:18:20 AM
// 

using System.Collections;
using System.Collections.Concurrent;
using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A texture cache used to keep textures resident for use over a user defined lifetime
/// </summary>
/// <typeparam name="T">The type of texture view to store. Must be a reference type, and implement <see cref="IGorgonTextureResource"/>.</typeparam>
/// <remarks>
/// <para>
/// In some cases, textures are shared amongst several components because it is inefficient to load the same texture multiple times. However, the problem of ownership arises when one or more of the 
/// components needs to be destroyed, what to do about the texture it uses?  Simply disposing of the texture would not work since the other components need this texture to work properly. 
/// </para>
/// <para>
/// This is where the texture cache can be used to solve the problem. A texture cache will keep the textures resident in memory as long as they're being used. When a texture is requested by passing in 
/// its <see cref="GorgonTexture2D.Name"/> it will load the texture if it is not previously cached, or if the actual texture object was disposed. If the texture was previously cached, then the cached texture 
/// will be returned, incrementing an internal count, which is used to determine how many items are using the texture
/// </para>
/// <para>
/// When a texture is no longer required, the texture should <b>not</b> be disposed. Instead, use the texture cache to return the texture which will automatically dispose of it when no more objects 
/// are using it. If the texture is required again, then retrieving it from the texture cache will load the texture again
/// </para>
/// </remarks>
/// <seealso cref="IGorgonTextureResource"/>
/// <remarks>Initializes a new instance of the <see cref="GorgonTextureCache{T}"/> class.</remarks>
/// <param name="graphics">The graphics interface used to create textures.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
public class GorgonTextureCache<T>(GorgonGraphics graphics)
    : IEnumerable<T>
    where T : class, IGorgonTextureResource
{
    /// <summary>
    /// An entry in the texture cache.
    /// </summary>
    private class TextureEntry
        : IDisposable
    {
        /// <summary>
        /// The texture instance.
        /// </summary>
        public WeakReference<T> Texture;

        /// <summary>
        /// The name of the texture.
        /// </summary>
        public string TextureName;

        /// <summary>
        /// The number of references to the texture.
        /// </summary>
        public int Users;

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            WeakReference<T> textureRef = Interlocked.Exchange(ref Texture, null);
            if ((textureRef is null) || (!textureRef.TryGetTarget(out T texture)))
            {
                return;
            }

            texture?.Dispose();
        }
    }

    // The graphics interface used to create the textures.
    private readonly GorgonGraphics _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
    // The cache that holds the textures and redirected file name.
    private readonly ConcurrentDictionary<string, Lazy<TextureEntry>> _cache = new(StringComparer.OrdinalIgnoreCase);
    // The lock for updating the cache concurrently.
    private SemaphoreSlim _cacheLock = new(1, 1);
    // The list of textures to that are currently being loaded.
    private readonly List<string> _scheduledTextures = [];

    /// <summary>
    /// Property to return the number of cached textures.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Function used to locate the texture if it is not stored in the cache.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <returns>The texture to locate, if available, or <b>null</b> if not found.</returns>
    private Task<T> DefaultTextureLocator(string name)
    {
        T resource = _graphics.LocateResourcesByName<GorgonGraphicsResource>(name).OfType<T>().FirstOrDefault();
        return Task.FromResult(resource);
    }

    /// <summary>
    /// Function to retrieve the texture directly from the cache.
    /// </summary>
    /// <param name="textureName">The name of the texture to look up.</param>
    /// <param name="texture">The texture that was found.</param>
    /// <param name="entry">The cache entry for the texture.</param>
    /// <returns><b>true</b> if the texture was returned successfully, <b>false</b> if not.</returns>
    private bool GetTextureFromCache(string textureName, out T texture, out Lazy<TextureEntry> entry)
    {
        texture = null;

        if ((_cache.TryGetValue(textureName, out entry)) && (entry?.Value?.Texture is not null))
        {
            if (entry.Value.Texture.TryGetTarget(out T textureRef))
            {
                entry.Value.Users++;
                _graphics.Log.Print($"Texture '{textureName}' exists in cache with {entry.Value.Users} users.", LoggingLevel.Verbose);
            }
            else
            {
                _graphics.Log.PrintWarning($"Texture '{textureName}' exists in the cache with {entry.Value.Users} users, but was garbage collected!", LoggingLevel.Intermediate);
            }

            texture = textureRef;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Function to return the texture to cache.
    /// </summary>
    /// <param name="texture">The texture to return.</param>
    /// <returns><b>true</b> if the texture was freed, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// This method <b>should</b> be called when the <paramref name="texture"/> is no longer required. 
    /// </para>
    /// <para>
    /// This method is thread safe.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Do <b>not</b> dispose of the returned texture manually. Doing so will leave the reference count incorrect, and the cached value returned will be invalid.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public bool ReturnTexture(T texture)
    {
        if (texture is null)
        {
            return false;
        }

        if (!_cache.TryGetValue(texture.Name, out Lazy<TextureEntry> entry))
        {
            _graphics.Log.PrintWarning($"Texture '{texture.Name}' not found in cache.", LoggingLevel.Verbose);
            return false;
        }

        // If the texture was collected, then we can dump it now.
        T textureRef = null;
        if ((entry?.Value?.Texture is not null) && (!entry.Value.Texture.TryGetTarget(out textureRef)))
        {
            Interlocked.Exchange(ref entry.Value.Users, 0);
            Interlocked.Exchange(ref entry.Value.Texture, null);
            return true;
        }

        if (entry?.Value is null)
        {
            return true;
        }

        if (Interlocked.Decrement(ref entry.Value.Users) > 0)
        {
            _graphics.Log.Print($"Texture '{texture.Name}' still has {entry.Value.Users} user(s), texture will stay resident.", LoggingLevel.Verbose);
            return false;
        }

        Interlocked.Exchange(ref entry.Value.Users, 0);
        Interlocked.Exchange(ref entry.Value.Texture, null);

        try
        {
            _cacheLock.Wait();

            textureRef?.Dispose();
            _graphics.Log.Print($"Texture '{texture.Name}' has been unloaded from the cache.", LoggingLevel.Verbose);
        }
        finally
        {
            _cacheLock.Release();
        }

        return true;
    }

    /// <summary>
    /// Function to retrieve a cached texture (or load one if it's not available).
    /// </summary>
    /// <param name="textureName">The name of the texture to look up.</param>
    /// <param name="missingTextureAction">[Optional] The method to call if a texture is missing, or not loaded.</param>
    /// <returns>The cached texture.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="textureName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="textureName"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This method will load the texture associated with the <paramref name="textureName"/> passed in if it does not exist in the cache. If the texture is in the cache, then the cached version is 
    /// returned to the user.
    /// </para>
    /// <para>
    /// If a texture was reclaimed by the garbage collector, or was never added to the cache, then the <paramref name="missingTextureAction"/> method is called to retrieve the texture from an external 
    /// source (e.g. loaded from the file system) and returned to be added to the cache (or updated with a fresh copy). If the parameter is not specified, then Gorgon's internal resource list will be 
    /// examined for the specified texture (using the <see cref="GorgonResourceLocator.LocateResourcesByName"/> extension method).
    /// </para>
    /// <para>
    /// Every call to this method will increment an internal reference count for a cached texture. The texture will not be disposed of until this reference count hits zero. This can only be done with a 
    /// call to <see cref="ReturnTexture"/>. However, since the texture is stored as a weak reference internally, the garbage collector can still free the texture, so calling 
    /// <see cref="ReturnTexture"/> is not mandatory. However, it is best practice to call the method when it is no longer in use.
    /// </para>
    /// <para>
    /// This method is thread safe.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Do <b>not</b> dispose of the returned texture manually. Doing so will leave the reference count incorrect, and the cached value returned will be invalid until the garbage collector picks it up.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public async Task<T> GetTextureAsync(string textureName, Func<string, Task<T>> missingTextureAction = null)
    {
        if (textureName is null)
        {
            throw new ArgumentNullException(nameof(textureName));
        }

        if (string.IsNullOrWhiteSpace(textureName))
        {
            throw new ArgumentEmptyException(nameof(textureName));
        }

        if (missingTextureAction is null)
        {
            _graphics.Log.Print("Defaulting to internal resource search method.", LoggingLevel.Verbose);
            missingTextureAction = DefaultTextureLocator;
        }

        _graphics.Log.Print($"Retrieving texture '{textureName}'.", LoggingLevel.Verbose);

        try
        {
            T result = null;

            await _cacheLock.WaitAsync();
            bool isScheduled = _scheduledTextures.Contains(textureName);
            _cacheLock.Release();

            if (isScheduled)
            {
                _graphics.Log.Print($"Requested texture '{textureName}' is currently being loaded on another thread, waiting for it to become available.", LoggingLevel.Verbose);

                // If we're requesting a texture that's in the process of loading, then wait until the previous guy is done.
                await Task.Run(() => SpinWait.SpinUntil(() => (!_scheduledTextures.Contains(textureName)) || (GetTextureFromCache(textureName, out result, out _))));
            }

            if ((result is not null) || ((GetTextureFromCache(textureName, out result, out Lazy<TextureEntry> entry)) && (result is not null)))
            {
                return result;
            }

            await _cacheLock.WaitAsync();
            _scheduledTextures.Add(textureName);
            _cacheLock.Release();

            // If we are at this point, then the reference is dead, so clear it out before loading.
            if (entry?.Value?.Texture is not null)
            {
                entry.Value.Texture = null;
                entry.Value.Users = 0;
            }

            T texture;

            if (!string.IsNullOrWhiteSpace(entry?.Value?.TextureName))
            {
                _graphics.Log.Print($"Texture '{textureName}' not available, reloading...", LoggingLevel.Verbose);

                texture = await missingTextureAction(textureName);

                if (texture is null)
                {
                    _graphics.Log.PrintWarning($"The texture '{textureName}' was not loaded and not added to cache.", LoggingLevel.Intermediate);
                    return null;
                }

                entry.Value.Texture = new WeakReference<T>(texture);
                entry.Value.Users = 1;

                _graphics.Log.Print($"Texture '{entry.Value.TextureName}' loaded into cache with 1 user.", LoggingLevel.Verbose);

                return texture;
            }

            _graphics.Log.Print($"Texture '{textureName}' not found, adding to cache...", LoggingLevel.Verbose);
            texture = await missingTextureAction(textureName);

            if (texture is null)
            {
                _graphics.Log.PrintWarning($"The texture '{textureName}' was not loaded and not added to cache.", LoggingLevel.Intermediate);
                return null;
            }

            entry = new Lazy<TextureEntry>(() => new TextureEntry
            {
                Texture = new WeakReference<T>(texture),
                TextureName = textureName,
                Users = 1
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            _cache[textureName] = entry;
            _graphics.Log.Print($"Texture '{textureName}' added to cache with 1 user.", LoggingLevel.Verbose);

            return texture;
        }
        finally
        {
            await _cacheLock.WaitAsync();
            _scheduledTextures.Remove(textureName);
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Function to retrieve a previously cached texture.
    /// </summary>
    /// <param name="textureName">The name of the texture to look up.</param>
    /// <returns>The cached texture, or <b>null</b> if it was not cached.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="textureName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="textureName"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This method will retrieve the texture associated with the <paramref name="textureName"/> from the cache if it has been loaded into the cache. If it was not loaded, then the method will return 
    /// <b>null</b>.
    /// </para>
    /// <para>
    /// If a texture was reclaimed by the garbage collector, then this method will return <b>null</b>.
    /// </para>
    /// <para>
    /// Every call to this method will increment an internal reference count for a cached texture. The texture will not be disposed of until this reference count hits zero. This can only be done with a 
    /// call to <see cref="ReturnTexture"/>. However, since the texture is stored as a weak reference internally, the garbage collector can still free the texture, so calling 
    /// <see cref="ReturnTexture"/> is not mandatory. However, it is best practice to call the method when it is no longer in use.
    /// </para>
    /// <para>
    /// This method is thread safe.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Do <b>not</b> dispose of the returned texture manually. Doing so will leave the reference count incorrect, and the cached value returned will be invalid until the garbage collector picks it up.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public T FindTexture(string textureName)
    {
        if (textureName is null)
        {
            throw new ArgumentNullException(nameof(textureName));
        }

        if (string.IsNullOrWhiteSpace(textureName))
        {
            throw new ArgumentEmptyException(nameof(textureName));
        }

        _cacheLock.Wait();

        try
        {
            _graphics.Log.Print($"Locating texture '{textureName}'.", LoggingLevel.Verbose);

            if ((GetTextureFromCache(textureName, out T result, out Lazy<TextureEntry> _)) && (result is not null))
            {
                return result;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        return null;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is <b>NOT</b> thread safe.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        Clear();
        SemaphoreSlim cacheLock = Interlocked.Exchange(ref _cacheLock, null);
        cacheLock?.Dispose();
    }

    /// <summary>
    /// Function to add a texture to the cache.
    /// </summary>
    /// <param name="texture">The texture to add.</param>
    /// <returns>The number of users for the texture.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is <b>null</b>.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the name of the <paramref name="texture"/> cannot be correlated to a file in the file system.</exception>
    /// <remarks>
    /// <para>
    /// Use this method to assign a pre-loaded texture to the cache. If the texture is already in the cache, then its user count is incremented.
    /// </para>
    /// <para>
    /// This method is thread safe.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Once a texture is added, the cache will assume ownership of the texture. Thus, the texture must not be disposed.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public int AddTexture(T texture)
    {
        if (texture is null)
        {
            throw new ArgumentNullException(nameof(texture));
        }

        TextureEntry GenerateEntry()
        {
            _graphics.Log.Print($"Texture '{texture.Name}' adding to cache.", LoggingLevel.Verbose);
            return new TextureEntry
            {
                Texture = new WeakReference<T>(texture),
                TextureName = texture.Name,
                Users = 0
            };
        }

        Lazy<TextureEntry> entry = _cache.GetOrAdd(texture.Name, new Lazy<TextureEntry>(GenerateEntry, LazyThreadSafetyMode.ExecutionAndPublication));

        if (entry?.Value is null)
        {
            return 0;
        }

        Interlocked.Increment(ref entry.Value.Users);

        if ((entry.Value.Texture is not null) && (entry.Value.Texture.TryGetTarget(out _)))
        {
            _graphics.Log.Print($"Texture '{texture.Name}' exists in cache with {entry.Value.Users} users.", LoggingLevel.Verbose);
            return entry.Value.Users;
        }

        try
        {
            _cacheLock.Wait();
            Interlocked.Exchange(ref entry.Value.Texture, new WeakReference<T>(texture));
            _graphics.Log.Print($"Texture '{texture.Name}' exists in cache, but has been collected. Refreshing with {entry.Value.Users} users.", LoggingLevel.Verbose);
            return entry.Value.Users;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Function to retrieve the user count for a texture in the cache.
    /// </summary>
    /// <param name="texture">The texture to look up.</param>
    /// <returns>The number of users for the texture.</returns>
    public int GetUserCount(T texture)
    {
        if (texture is null)
        {
            return 0;
        }

        Lazy<TextureEntry> entry = _cache.Values.FirstOrDefault(item => (item?.Value?.Texture is not null)
                                                               && (item.Value.Texture.TryGetTarget(out T itemTexture))
                                                               && (itemTexture == texture));

        if (entry?.Value is null)
        {
            return 0;
        }

        // If the texture was collected, then we can dump it now.
        if (!entry.Value.Texture.TryGetTarget(out T textureRef))
        {
            entry.Value.Users = 0;
            entry.Value.Texture = null;
            return 0;
        }

        return entry.Value.Users;
    }

    /// <summary>
    /// Function to clear the cached textures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will dispose of all textures in the cache. Please take care to update any objects dependent upon the textures stored in the cache after calling this method.
    /// </para>
    /// </remarks>
    public void Clear()
    {
        if ((_cache is null) || (_cache.Values.Count == 0) || (_cacheLock is null))
        {
            return;
        }

        try
        {
            _cacheLock.Wait();

            _graphics.Log.Print("Clearing texture cache.", LoggingLevel.Verbose);
            foreach (Lazy<TextureEntry> entry in _cache.Values)
            {
                entry?.Value?.Dispose();
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        _cache.Clear();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<T> GetEnumerator()
    {
        foreach (KeyValuePair<string, Lazy<TextureEntry>> entry in _cache)
        {
            if ((entry.Value?.Value?.Texture is not null) && (entry.Value.Value.Texture.TryGetTarget(out T texture)))
            {
                yield return texture;
            }
        }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
