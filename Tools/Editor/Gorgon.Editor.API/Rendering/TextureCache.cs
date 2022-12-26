#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 21, 2020 12:18:20 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Properties;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.Rendering;


/// <summary>
/// A texture cache used to keep textures resident for use over a user defined lifetime.
/// </summary>
/// <remarks>
/// <para>
/// In some cases, textures are shared amongst several components because it is inefficient to load the same texture multiple times. However, the problem of ownership arises when one or more of the 
/// components needs to be destroyed, what to do about the texture it uses?  Simply disposing of the texture would not work since the other components need this texture to work properly. 
/// </para>
/// <para>
/// This is where the texture cache can be used to solve the problem. A texture cache will keep the textures resident in memory as long as they're being used. When a texture is requested by passing in 
/// its <see cref="IContentFile"/> it will load the texture if it is not previously cached, or if the actual texture object was disposed. If the texture was previously cached, then the cached texture 
/// will be returned, incrementing an internal count, which is used to determine how many items are using the texture.
/// </para>
/// <para>
/// When a texture is no longer required, the texture should <b>not</b> be disposed. Instead, use the texture cache to return the texture which will automatically dispose of it when no more objects 
/// are using it. If the texture is required again, then retrieving it from the texture cache will load the texture again.
/// </para>
/// </remarks>
/// <seealso cref="IContentFile"/>
public class TextureCache
    : ITextureCache
{
    #region Classes.
    /// <summary>
    /// An entry in the texture cache.
    /// </summary>
    private class TextureEntry
        : IDisposable
    {
        /// <summary>
        /// The actual file for the texture.
        /// </summary>
        public IContentFile TextureFile;
        /// <summary>
        /// The cache file.
        /// </summary>
        public IGorgonVirtualFile CachedFile;
        /// <summary>
        /// The texture instance.
        /// </summary>
        public WeakReference<GorgonTexture2DView> Texture;
        /// <summary>
        /// The number of references to the texture.
        /// </summary>
        public int Users;

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            WeakReference<GorgonTexture2DView> textureRef = Interlocked.Exchange(ref Texture, null);
            if ((textureRef is null) || (!textureRef.TryGetTarget(out GorgonTexture2DView texture)))
            {
                return;
            }

            texture?.Texture?.Dispose();
            texture?.Dispose();
        }
    }
    #endregion

    #region Constants.
    // The path to the texture cache directory.
    private const string CacheDirectory = "/Gorgon.Editor/TextureCache/";
    #endregion

    #region Variables.
    // The graphics interface used to create the textures.
    private readonly GorgonGraphics _graphics;
    // The project file system manager.
    private readonly IContentFileManager _fileManager;
    // The writer used to write files to the temporary area.
    private readonly IGorgonFileSystemWriter<Stream> _tempWriter;
    // The cache that holds the textures and redirected file name.
    private readonly Dictionary<IContentFile, TextureEntry> _cache = new();
    // The codec used to load the image.
    private readonly IGorgonImageCodec _codec;
    // The directory for the cached files.
    private IGorgonVirtualDirectory _cacheDirectory;
    // The log interface for capturing debug messages.
    private readonly IGorgonLog _log;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to load a texture for the texture cache.
    /// </summary>
    /// <param name="entry">The entry containing the texture data.</param>
    /// <returns>The texture.</returns>
    private async Task<GorgonTexture2DView> LoadTextureAsync(TextureEntry entry)
    {
        // If, for some reason, the file does not exist, then we need to remove the cache entry and rebuild it.
        if (_tempWriter.FileSystem.GetFile(entry.CachedFile.FullPath) is null)
        {
            _log.Print($"Texture '{entry.TextureFile.Path}' exists in cache, but the cache file was not found. Rebuilding cache entry.", LoggingLevel.Verbose);
            _cache.Remove(entry.TextureFile);
            entry.CachedFile = null;
            return await BuildCacheEntryAsync(entry.TextureFile);
        }

        GorgonTexture2DView result;

        result = _graphics.LocateResourcesByName<GorgonTexture2D>(entry.TextureFile.Path, comparisonType: StringComparison.OrdinalIgnoreCase)
                          .FirstOrDefault()?.GetShaderResourceView();

        if (result is not null)
        {
            entry.Texture = new WeakReference<GorgonTexture2DView>(result);
            Interlocked.Increment(ref entry.Users);
            _log.Print($"Texture '{entry.TextureFile.Path}' was already loaded, but not assigned to the cache entry. Assigned to cache with {entry.Users} users.", LoggingLevel.Verbose);
            return result;
        }

        IGorgonImage image = await Task.Run(() =>
        {
            using Stream textureStream = _tempWriter.OpenStream(entry.CachedFile.FullPath, FileMode.Open);
            if (!_codec.IsReadable(textureStream))
            {
                _log.Print($"ERROR: Texture '{entry.TextureFile.Path}' is not a {_codec.Name} file.", LoggingLevel.Verbose);
            }

            return _codec.FromStream(textureStream);
        });

        using (image)
        {
            result = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
            {
                Name = entry.TextureFile.Path,
                MipLevels = image.MipCount,
                ArrayCount = image.ArrayCount,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                IsCubeMap = false
            }, image);
        }

        Interlocked.Increment(ref entry.Users);
        _log.Print($"Texture '{entry.TextureFile.Path}' loaded into cache with {entry.Users} users.", LoggingLevel.Verbose);
        entry.Texture = new WeakReference<GorgonTexture2DView>(result);

        return result;
    }

    /// <summary>
    /// Function to build a texture cache entry.
    /// </summary>
    /// <param name="file">The file containing the texture data.</param>
    /// <returns>The texture.</returns>
    private async Task<GorgonTexture2DView> BuildCacheEntryAsync(IContentFile file)
    {
        if (!_fileManager.FileExists(file.Path))
        {
            _log.Print($"ERROR: File '{file.Path}' does not exist. No texture will be available for this file.", LoggingLevel.Verbose);
            return null;
        }

        GorgonTexture2DView result = null;
        string cacheFilePath = CacheDirectory + Guid.NewGuid().ToString("N");

        result = _graphics.LocateResourcesByName<GorgonTexture2D>(file.Path, comparisonType: StringComparison.OrdinalIgnoreCase)
                          .FirstOrDefault()?.GetShaderResourceView();

        if (result is not null)
        {
            _log.Print($"Texture '{file.Path}' was already loaded, but not in the cache. Added to cache with 1 user.", LoggingLevel.Verbose);
            _cache[file] = new TextureEntry
            {
                CachedFile = _tempWriter.FileSystem.GetFile(cacheFilePath),
                Texture = new WeakReference<GorgonTexture2DView>(result),
                TextureFile = file,
                Users = 1
            };
            return result;
        }

        IGorgonImage image = await Task.Run(() =>
        {
            _log.Print($"Copying texture '{file.Path}' to temporary cache area '{cacheFilePath}'.", LoggingLevel.Verbose);
            using (Stream inStream = _fileManager.OpenStream(file.Path, FileMode.Open))
            using (Stream outStream = _tempWriter.OpenStream(cacheFilePath, FileMode.Create))
            {
                if (!_codec.IsReadable(inStream))
                {
                    _log.Print($"ERROR: Texture '{file.Path}' is not a {_codec.Name} file.", LoggingLevel.Verbose);
                    return null;
                }

                inStream.CopyTo(outStream);
            }

            using Stream textureStream = _tempWriter.OpenStream(cacheFilePath, FileMode.Open);
            return _codec.FromStream(textureStream);
        });

        if (image is null)
        {
            return null;
        }

        using (image)
        {
            result = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
            {
                Name = file.Path,
                MipLevels = image.MipCount,
                ArrayCount = image.ArrayCount,
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable,
                IsCubeMap = false
            }, image);
        }

        _log.Print($"Texture '{file.Path}' added to cache with 1 user.", LoggingLevel.Verbose);
        _cache[file] = new TextureEntry
        {
            CachedFile = _tempWriter.FileSystem.GetFile(cacheFilePath),
            Texture = new WeakReference<GorgonTexture2DView>(result),
            TextureFile = file,
            Users = 1
        };

        return result;
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
    /// <note type="important">
    /// <para>
    /// Do <b>not</b> dispose of the returned texture manually. Doing so will leave the reference count incorrect, and the cached value returned will be invalid.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public bool ReturnTexture(GorgonTexture2DView texture)
    {
        if (texture is null)
        {
            return false;
        }

        TextureEntry entry = _cache.Values.FirstOrDefault(item => (item.Texture is not null) 
                                                               && (item.Texture.TryGetTarget(out GorgonTexture2DView itemTexture)) 
                                                               && (itemTexture == texture) 
                                                               && (itemTexture.Texture is not null));

        if (entry is null)
        {
            _log.Print($"WARNING: Texture '{texture?.Texture?.Name}' not found in cache.", LoggingLevel.Verbose);
            return false;
        }

        // If the texture was collected, then we can dump it now.
        if (!entry.Texture.TryGetTarget(out GorgonTexture2DView textureRef))
        {
            entry.Users = 0;
            entry.Texture = null;
            return true;
        }

        if (Interlocked.Decrement(ref entry.Users) != 0)
        {
            _log.Print($"Texture '{texture?.Texture?.Name}' still has {entry.Users} user(s), texture will stay resident.", LoggingLevel.Verbose);
            return false;
        }

        _log.Print($"Texture '{texture?.Texture?.Name}' has been unloaded from the cache.", LoggingLevel.Verbose);
        entry.Users = 0;
        textureRef?.Texture?.Dispose();
        textureRef?.Dispose();

        entry.Texture = null;

        return true;
    }

    /// <summary>
    /// Function to retrieve a cached texture (or load one if it's not available).
    /// </summary>
    /// <param name="file">The file containing the texture data.</param>
    /// <returns>The cached texture.</returns>
    /// <remarks>
    /// <para>
    /// This method will load the texture associated with the <paramref name="file"/> passed in if it does not exist in the cache. If the texture is in the cache, then the cached version is returned to 
    /// the user.
    /// </para>
    /// <para>
    /// Every call to this method will increment an internal reference count for a cached texture. The texture will not be disposed of until this reference count hits zero. This can only be done with a 
    /// call to <see cref="ReturnTexture"/>. However, since the texture is stored as a weak reference internally, the garbage collector can still free the texture, so calling 
    /// <see cref="ReturnTexture(GorgonTexture2DView)"/> is not mandatory. However, it is still recommended you call the method.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Do <b>not</b> dispose of the returned texture manually. Doing so will leave the reference count incorrect, and the cached value returned will be invalid.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public async Task<GorgonTexture2DView> GetTextureAsync(IContentFile file)
    {
        if (file is null)
        {
            return null;
        }

        _log.Print($"Retrieving texture '{file.Path}'.", LoggingLevel.Verbose);

        if ((_cache.TryGetValue(file, out TextureEntry entry)) && (entry.Texture is not null) && (entry.Texture.TryGetTarget(out GorgonTexture2DView textureRef)))
        {
            Interlocked.Increment(ref entry.Users);
            _log.Print($"Texture '{file.Path}' exists in cache with {entry.Users} users.", LoggingLevel.Verbose);
            return textureRef;
        }

        // If we are at this point, then the reference is dead, so clear it out before loading.
        if (entry?.Texture is not null)
        {
            entry.Users = 0;
            entry.Texture = null;
        }

        // Ensure that the cache directory is available.
        _cacheDirectory = _tempWriter.FileSystem.GetDirectory(CacheDirectory);

        if (_cacheDirectory is null)
        {
            _cacheDirectory = _tempWriter.CreateDirectory(CacheDirectory);
        }

        if (entry?.CachedFile is not null)
        {
            _log.Print($"Texture '{file.Path}' not available, reloading...", LoggingLevel.Verbose);
            return await LoadTextureAsync(entry);
        }

        _log.Print($"Texture '{file.Path}' not found, adding to cache...", LoggingLevel.Verbose);
        return await BuildCacheEntryAsync(file);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _log.Print("Clearing texture cache.", LoggingLevel.Verbose);

        foreach (TextureEntry texture in _cache.Values)
        {
            texture?.Dispose();
        }

        _cache.Clear();

        IGorgonVirtualDirectory dir = Interlocked.Exchange(ref _cacheDirectory, null);

        if (dir is null)
        {
            return;
        }

        // Clear out the temporary directory.
        int counter = 0;

        while (counter < 5)
        {
            try
            {
                if (_tempWriter.FileSystem.GetDirectory(dir.FullPath) is not null)
                {
                    _tempWriter.DeleteDirectory(dir.FullPath);
                }

                break;
            }
            catch (Exception ex)
            {
                if (counter == 4)
                {
                    _log.Print("WARNING: There was an error deleting the temporary cache directory.", LoggingLevel.Verbose);
                    _log.LogException(ex);
                }
                else
                {
                    // Sleep for a bit and see if the directory can be deleted.
                    Thread.Sleep(250);
                }
                ++counter;
            }
        }
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
    /// <note type="important">
    /// <para>
    /// Once a texture is added, the cache will assume ownership of the texture. Thus, the texture must not be disposed.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public async Task<int> AddTextureAsync(GorgonTexture2DView texture)
    {
        if (texture is null)
        {
            throw new ArgumentNullException(nameof(texture));
        }

        IContentFile file = _fileManager.GetFile(texture.Texture.Name);

        if (file is null)
        {
            _log.Print($"ERROR: File '{texture.Texture.Name}' does not exist.", LoggingLevel.Verbose);
            throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, texture.Texture.Name));
        }

        if (_cache.TryGetValue(file, out TextureEntry entry))
        {
            Interlocked.Increment(ref entry.Users);

            if ((entry.Texture is not null) && (entry.Texture.TryGetTarget(out _)))
            {
                _log.Print($"Texture '{file.Path}' exists in cache with {entry.Users} users.", LoggingLevel.Verbose);
                return entry.Users;
            }
            
            Interlocked.Exchange(ref entry.Texture, new WeakReference<GorgonTexture2DView>(texture));
            _log.Print($"Texture '{file.Path}' exists in cache, but has been collected. Refreshing with {entry.Users} users.", LoggingLevel.Verbose);
            return entry.Users;
        }

        string cacheFilePath = CacheDirectory + Guid.NewGuid().ToString("N");

        bool copyResult = await Task.Run(() =>
        {
            _log.Print($"Copying texture '{file.Path}' to temporary cache area '{cacheFilePath}'.", LoggingLevel.Verbose);
            using (Stream inStream = _fileManager.OpenStream(file.Path, FileMode.Open))
            using (Stream outStream = _tempWriter.OpenStream(cacheFilePath, FileMode.Create))
            {
                if (!_codec.IsReadable(inStream))
                {
                    _log.Print($"ERROR: Texture '{file.Path}' is not a {_codec.Name} file.", LoggingLevel.Verbose);
                    return false;
                }

                inStream.CopyTo(outStream);
            }

            return true;
        });

        // If we failed to copy to the temp location, leave.
        if (!copyResult)
        {
            return -1;
        }

        _log.Print($"Texture '{file.Path}' adding to cache with 1 user.", LoggingLevel.Verbose);
        entry = new TextureEntry
        {
            CachedFile = _tempWriter.FileSystem.GetFile(cacheFilePath),
            Texture = new WeakReference<GorgonTexture2DView>(texture),
            TextureFile = file,
            Users = 1
        };

        _cache[file] = entry;
        return entry.Users;
    }

    /// <summary>
    /// Function to retrieve the user count for a texture in the cache.
    /// </summary>
    /// <param name="texture">The texture to look up.</param>
    /// <returns>The number of users for the texture.</returns>
    public int GetUserCount(GorgonTexture2DView texture)
    {
        if (texture is null)
        {
            return 0;
        }

        TextureEntry entry = _cache.Values.FirstOrDefault(item => (item.Texture is not null)
                                                               && (item.Texture.TryGetTarget(out GorgonTexture2DView itemTexture))
                                                               && (itemTexture == texture)
                                                               && (itemTexture.Texture is not null));

        if (entry is null)
        {
            return 0;
        }

        // If the texture was collected, then we can dump it now.
        if (!entry.Texture.TryGetTarget(out GorgonTexture2DView textureRef))
        {
            entry.Users = 0;
            entry.Texture = null;
            return 0;
        }

        return entry.Users;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="TextureCache"/> class.</summary>
    /// <param name="graphics">The graphics interface used to create textures.</param>
    /// <param name="fileManager">The file manager for the project file system.</param>
    /// <param name="tempWriter">The temporary writer used to write temporary data.</param>
    /// <param name="codec">The image codec.</param>
    /// <param name="log">The logging interface for capturing debug messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
    public TextureCache(GorgonGraphics graphics, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> tempWriter, IGorgonImageCodec codec, IGorgonLog log)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _tempWriter = tempWriter ?? throw new ArgumentNullException(nameof(tempWriter));
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }
    #endregion
}
