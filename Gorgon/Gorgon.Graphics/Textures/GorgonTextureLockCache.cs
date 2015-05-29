#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, August 01, 2013 9:16:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A cache for locks on a resource or sub-resource.
    /// </summary>
    class GorgonTextureLockCache
        : IDisposable
    {
        #region Value Types.
        /// <summary>
        /// A key used to identify a lock.
        /// </summary>
        struct LockCacheKey
            : IEquatable<LockCacheKey>
        {
            #region Variables.
            private readonly GorgonGraphics _context;       // The graphics context.
            private readonly int _keyValue1;     // The first key value.
            private readonly int _keyValue2;     // The second key value.
            #endregion

            #region Methods.
            /// <summary>
            /// Function to determine equality between 2 keys.
            /// </summary>
            /// <param name="left">Left key to compare.</param>
            /// <param name="right">Right key to compare.</param>
            /// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
            private static bool Equals(ref LockCacheKey left, ref LockCacheKey right)
            {
                return (left._keyValue1 == right._keyValue1) && (left._keyValue2 == right._keyValue2) && (left._context == right._context);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(_keyValue1).GenerateHash(_keyValue2).GenerateHash(_context);
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is LockCacheKey)
                {
                    return ((LockCacheKey)obj).Equals(this);
                }

                return base.Equals(obj);
            }
            #endregion

            #region Constructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="LockCacheKey"/> struct.
            /// </summary>
            /// <param name="context">Graphics context.</param>
            /// <param name="keyValue1">The key value1.</param>
            /// <param name="keyValue2">The key value2.</param>
            public LockCacheKey(GorgonGraphics context, int keyValue1, int keyValue2)
            {
                _context = context;
                _keyValue1 = keyValue1;
                _keyValue2 = keyValue2;
            }
            #endregion

            #region IEquatable<LockCacheKey> Members
            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the other parameter; otherwise, false.
            /// </returns>
            public bool Equals(LockCacheKey other)
            {
                return Equals(ref this, ref other);
            }
            #endregion
        }
        #endregion

        #region Variables.
        private static readonly object _syncLock = new object();        // Lock used for threading.
        private bool _disposed;                                         // Flag to indicate that the object was disposed.
        private Dictionary<LockCacheKey, GorgonTextureLockData> _locks; // List of locks that are currently open on the resource.
        private GorgonTexture _texture;                                 // Texture that has the locks.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the mapping mode for locking a buffer.
        /// </summary>
        /// <param name="flags">Flags to use when locking.</param>
        /// <returns>The D3D mapping mode.</returns>
        private static D3D.MapMode GetMapMode(BufferLockFlags flags)
        {
            if ((flags & BufferLockFlags.Read) == BufferLockFlags.Read)
            {
                return (flags & BufferLockFlags.Write) == BufferLockFlags.Write ? D3D.MapMode.ReadWrite : D3D.MapMode.Read;
            }

            if ((flags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
            {
                return D3D.MapMode.WriteDiscard;
            }

            return (flags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite
                ? D3D.MapMode.WriteNoOverwrite
                : D3D.MapMode.Write;
        }

        /// <summary>
        /// Function to perform an unlock.
        /// </summary>
        /// <param name="lockData">Lock data to remove.</param>
        public void Unlock(GorgonTextureLockData lockData)
        {
            lock (_syncLock)
            {
                var key = new LockCacheKey(lockData.Graphics, lockData.MipLevel, lockData.ArrayIndex);
                GorgonTextureLockData result;

                if (!_locks.TryGetValue(key, out result))
                {
                    return;
                }

                lockData.Graphics.Context.UnmapSubresource(_texture.D3DResource,
                    D3D.Resource.CalculateSubResourceIndex(lockData.MipLevel,
                        lockData.ArrayIndex,
                        _texture.Settings.MipCount));

                _locks.Remove(key);
            }
        }

        /// <summary>
        /// Function to lock a sub resource.
        /// </summary>
        /// <param name="lockFlags">Flags used to lock the data.</param>
        /// <param name="mipLevel">Mip map level to lock.</param>
        /// <param name="arrayIndex">Array index to lock (1D/2D textures only).</param>
        /// <param name="context">Graphics context to use for the lock.</param>
        /// <returns>The locking data.</returns>
        public GorgonTextureLockData Lock(BufferLockFlags lockFlags, int mipLevel, int arrayIndex, GorgonGraphics context)
        {
            lock(_syncLock)
            {
                var key = new LockCacheKey(context, mipLevel, arrayIndex);
                GorgonTextureLockData result;

                if (_locks.TryGetValue(key, out result))
                {
                    return result;
                }

                switch (_texture.ResourceType)
                {
                    case ResourceType.Texture1D:
                    case ResourceType.Texture2D:
                    case ResourceType.Texture3D:
                        DX.DataStream lockStream;

                        DX.DataBox box = context.Context.MapSubresource(_texture.D3DResource,
                            D3D.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, _texture.Settings.MipCount),
                            GetMapMode(lockFlags),
                            D3D.MapFlags.None,
                            out lockStream);

                        result = new GorgonTextureLockData(context, _texture, this, box, mipLevel, arrayIndex);

                        _locks.Add(key, result);
                        break;
                    default:
                        throw new GorgonException(GorgonResult.CannotCreate,
                            string.Format(Resources.GORGFX_RESOURCE_INVALID, _texture.ResourceType));
                }

                return result;
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTextureLockCache"/> class.
        /// </summary>
        /// <param name="texture">Texture that will be locked.</param>
        public GorgonTextureLockCache(GorgonTexture texture)
        {
            _texture = texture;
            _locks = new Dictionary<LockCacheKey, GorgonTextureLockData>();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Destroy all the open locks.
                while (_locks.Count > 0)
                {
                    _locks.First().Value.Dispose();
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
