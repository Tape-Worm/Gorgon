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
// Created: June 21, 2020 12:18:57 AM
// 
#endregion

using System;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.Rendering
{
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
    public interface ITextureCache
        : IDisposable
    {
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
        Task<GorgonTexture2DView> GetTextureAsync(IContentFile file);

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
        bool ReturnTexture(GorgonTexture2DView texture);

        /// <summary>
        /// Function to add a texture to the cache.
        /// </summary>
        /// <param name="texture">The texture to add.</param>
        /// <returns>The number of users for the texture.</returns>
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
        Task<int> AddTextureAsync(GorgonTexture2DView texture);

        /// <summary>
        /// Function to retrieve the user count for a texture in the cache.
        /// </summary>
        /// <param name="texture">The texture to look up.</param>
        /// <returns>The number of users for the texture.</returns>
        int GetUserCount(GorgonTexture2DView texture);
    }
}