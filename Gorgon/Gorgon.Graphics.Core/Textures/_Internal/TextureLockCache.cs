#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 21, 2016 12:24:28 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Gorgon.Core;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A cache for locks on a resource or sub-resource.
    /// </summary>
    internal class TextureLockCache
        : IDisposable
    {
        #region Value Types.
        /// <summary>
        /// A key used to identify a lock.
        /// </summary>
        private readonly struct LockCacheKey
            : IEquatable<LockCacheKey>
        {
			#region Variables.
			// The first key value.
			private readonly int _keyValue1;
			// The second key value.
			private readonly int _keyValue2;     
            #endregion

            #region Methods.
            /// <summary>
            /// Function to determine equality between 2 keys.
            /// </summary>
            /// <param name="left">Left key to compare.</param>
            /// <param name="right">Right key to compare.</param>
            /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
            private static bool Equals(LockCacheKey left, LockCacheKey right)
            {
                return (left._keyValue1 == right._keyValue1) && (left._keyValue2 == right._keyValue2);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(_keyValue1).GenerateHash(_keyValue2);
            }

            /// <summary>
            /// Determines whether the specified <see cref="object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
            /// <returns>
            ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is LockCacheKey key)
                {
                    return key.Equals(this);
                }

                return base.Equals(obj);
            }

			/// <summary>
			/// Indicates whether the current object is equal to another object of the same type.
			/// </summary>
			/// <param name="other">An object to compare with this object.</param>
			/// <returns>
			/// true if the current object is equal to the other parameter; otherwise, false.
			/// </returns>
			public bool Equals(LockCacheKey other)
			{
				return Equals(this, other);
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="LockCacheKey"/> struct.
			/// </summary>
			/// <param name="keyValue1">The key value1.</param>
			/// <param name="keyValue2">The key value2.</param>
			public LockCacheKey(int keyValue1, int keyValue2)
            {
                _keyValue1 = keyValue1;
                _keyValue2 = keyValue2;
            }
            #endregion
        }
        #endregion

        #region Variables.
		// List of locks that are currently open on the resource.
		private ConcurrentDictionary<LockCacheKey, GorgonTextureLockData> _locks;
        // The texture that owns this lock cache.
        private readonly GorgonGraphicsResource _texture;
        // The number of mip levels for the texture.
        private readonly int _mipLevelCount;
        // The width of the texture.
        private readonly int _textureWidth;
        // The height of the texture.
        private readonly int _textureHeight;
        // The depth of the texture.
        private readonly int _textureDepth;
        // The format of the texture.
        private readonly BufferFormat _format;
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return whether this texture has a lock against it or not.
        /// </summary>
        public bool HasLocks => _locks.Count > 0;
		#endregion

		#region Methods.
        /// <summary>
        /// Function to perform an unlock.
        /// </summary>
        /// <param name="lockData">Lock data to remove.</param>
        public void Unlock(GorgonTextureLockData lockData)
        {
            var key = new LockCacheKey(lockData.MipLevel, lockData.ArrayIndex);

			if (!_locks.TryRemove(key, out GorgonTextureLockData _))
			{
				return;
			}

			_texture.Graphics.D3DDeviceContext
	               .UnmapSubresource(_texture.D3DResource,
	                                 D3D11.Resource.CalculateSubResourceIndex(lockData.MipLevel, lockData.ArrayIndex, _mipLevelCount));
        }

	    /// <summary>
	    /// Function to lock a sub resource.
	    /// </summary>
	    /// <param name="lockFlags">Flags used to determine how the data will be accessed during the lock.</param>
	    /// <param name="mipLevel">Mip map level to lock.</param>
	    /// <param name="arrayIndex">Array index to lock (1D/2D textures only).</param>
	    /// <returns>A new <see cref="GorgonTextureLockData"/> containing the data from the lock.</returns>
	    public GorgonTextureLockData Lock(D3D11.MapMode lockFlags, int mipLevel, int arrayIndex)
	    {
		    var key = new LockCacheKey(mipLevel, arrayIndex);

			if (_locks.TryGetValue(key, out GorgonTextureLockData result))
			{
				return result;
			}

	        DX.DataBox box = _texture.Graphics.D3DDeviceContext.MapSubresource(_texture.D3DResource,
	                                                                           D3D11.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, _mipLevelCount),
	                                                                           lockFlags,
	                                                                           D3D11.MapFlags.None,
	                                                                           out _);

	        result = new GorgonTextureLockData(this, box, _textureWidth, _textureHeight, _textureDepth, mipLevel, arrayIndex, _format);

	        return _locks.GetOrAdd(key, result);
	    }

	    /// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			ConcurrentDictionary<LockCacheKey, GorgonTextureLockData> locks = Interlocked.Exchange(ref _locks, null);

			if (locks == null)
			{
				return;
			}

			foreach(KeyValuePair<LockCacheKey, GorgonTextureLockData> lockData in locks)
			{
				lockData.Value.Dispose();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TextureLockCache"/> class.
		/// </summary>
		/// <param name="texture">Texture that will be locked.</param>
		/// <param name="info">The information used to build the texture.</param>
		public TextureLockCache(GorgonGraphicsResource texture, IGorgonTexture2DInfo info)
        {
            _mipLevelCount = info.MipLevels;
            _textureWidth = info.Width;
            _textureHeight = info.Height;
            _textureDepth = 1;
            _format = info.Format;
            _texture = texture;
            _locks = new ConcurrentDictionary<LockCacheKey, GorgonTextureLockData>();
        }
        #endregion
    }
}
