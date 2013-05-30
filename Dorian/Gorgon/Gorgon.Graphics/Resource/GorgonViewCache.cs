#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, February 12, 2012 7:37:43 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A cache for resource views.
    /// </summary>
    sealed class GorgonViewCache
        : IDisposable
    {
        #region Value Types.
        /// <summary>
        /// A key for the SRV cache.
        /// </summary>
        struct ShaderViewKey
            : IEquatable<ShaderViewKey>
        {
            #region Variables.
            private readonly BufferFormat _format;
            private readonly int _value1;
            private readonly int _value2;
            private readonly int _value3;
            private readonly int _value4;
            #endregion

            #region Methods.
            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is ShaderViewKey)
                {
                    return Equals((ShaderViewKey)obj);
                }
                return base.Equals(obj);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(_format)
                       .GenerateHash(_value1)
                       .GenerateHash(_value2)
                       .GenerateHash(_value3)
                       .GenerateHash(_value4);
            }
            #endregion

            #region Constructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="ShaderViewKey"/> struct.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="value1">First key component.</param>
            /// <param name="value2">Second key component.</param>
            /// <param name="value3">Third key component.</param>
            /// <param name="value4">Fourth key component.</param>
            public ShaderViewKey(BufferFormat format, int value1, int value2, int value3, int value4)
            {
                _format = format;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
            }
            #endregion

            #region IEquatable<ShaderViewKey> Members
            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the other parameter; otherwise, false.
            /// </returns>
            public bool Equals(ShaderViewKey other)
            {
                return other._format == _format
                       && other._value1 == _value1
                       && other._value2 == _value2
                       && other._value3 == _value3
                       && other._value4 == _value4;
            }
            #endregion
        }
        #endregion

        #region Variables.
        // ReSharper disable StaticFieldInGenericType
        private static readonly object _syncLock = new object();								// Synchronization object for threading.
        // ReSharper restore StaticFieldInGenericType

        private bool _disposed;																	// Flag to indicate that the object is disposed.
        private readonly GorgonResource _resource;                                              // Resource bound to the SRVs.
        private readonly Dictionary<ShaderViewKey, GorgonBufferShaderView> _bufferViews;        // The cache of buffer views.
        private readonly Dictionary<ShaderViewKey, GorgonTextureShaderView> _textureViews;      // The cache of texture views.
        #endregion

        #region Methods.
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
                foreach (var item in _textureViews)
                {
                    item.Value.CleanUp();
                }

                foreach (var item in _bufferViews)
                {
                    item.Value.CleanUp();
                }

                _bufferViews.Clear();
            }

            _disposed = true;
        }

        /// <summary>
        /// Function to retrieve a texture view.
        /// </summary>
        /// <param name="format">Format of the view.</param>
        /// <param name="firstMip">First mip level for the view.</param>
        /// <param name="mipCount">Number of mip levels to view.</param>
        /// <param name="arrayIndex">First array index for the view.</param>
        /// <param name="arrayCount">Number of array indices to view.</param>
        /// <returns>The cached texture shader view.</returns>
        public GorgonTextureShaderView GetTextureView(BufferFormat format,
                                                      int firstMip,
                                                      int mipCount,
                                                      int arrayIndex,
                                                      int arrayCount)
        {
            var buffer = (GorgonTexture)_resource;
            var key = new ShaderViewKey(format,
                                        firstMip,
                                        mipCount,
                                        buffer.ResourceType != ResourceType.Texture3D ? arrayIndex : -1,
                                        buffer.ResourceType != ResourceType.Texture3D ? arrayCount : -1);

            lock (_syncLock)
            {
                GorgonTextureShaderView result;

                if (!_textureViews.TryGetValue(key, out result))
                {
                    result = new GorgonTextureShaderView(buffer,
                                                         format,
                                                         firstMip,
                                                         mipCount,
                                                         buffer.ResourceType != ResourceType.Texture3D ? arrayIndex : -1,
                                                         buffer.ResourceType != ResourceType.Texture3D ? arrayCount : -1);
                    result.Initialize();
                    _textureViews.Add(key, result);
                }

                return result;
            }
        }

        /// <summary>
        /// Function to retrieve a buffer view.
        /// </summary>
        /// <param name="format">Format of the view.</param>
        /// <param name="start">Starting element for the view.</param>
        /// <param name="count">Number of elements for the view.</param>
        /// <param name="isRaw">TRUE for raw buffers, FALSE for standard.</param>
        /// <returns>The cached buffer shader view.</returns>
        public GorgonBufferShaderView GetBufferView(BufferFormat format, int start, int count, bool isRaw)
        {
            var key = new ShaderViewKey(format, start, count, 0, 0);

            lock(_syncLock)
            {
                GorgonBufferShaderView result;

                if (!_bufferViews.TryGetValue(key, out result))
                {
	                result = new GorgonBufferShaderView(_resource, format, start, count, isRaw);

                    result.Initialize();
                    _bufferViews.Add(key, result);
                }

                return result;
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonViewCache"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public GorgonViewCache(GorgonResource resource)
        {
            _bufferViews = new Dictionary<ShaderViewKey, GorgonBufferShaderView>();
            _textureViews = new Dictionary<ShaderViewKey, GorgonTextureShaderView>();
            _resource = resource;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
