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
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using SharpDX.DXGI;

namespace Gorgon.Graphics
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
        struct ViewKey
            : IEquatable<ViewKey>, IEqualityComparer<ViewKey>
        {
            #region Variables.
            private readonly BufferFormat _format;
            private readonly int _value1;
            private readonly int _value2;
            private readonly int _value3;
            private readonly int _value4;
            private readonly bool _value5;
            #endregion

            #region Methods.
            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is ViewKey)
                {
                    return Equals((ViewKey)obj);
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
                       .GenerateHash(_value4)
                       .GenerateHash(_value5);
            }
            #endregion

            #region Constructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="ViewKey"/> struct.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="value1">First key component.</param>
            /// <param name="value2">Second key component.</param>
            /// <param name="value3">Third key component.</param>
            /// <param name="value4">Fourth key component.</param>
            /// <param name="value5">Fifth key component.</param>
            public ViewKey(BufferFormat format, int value1, int value2, int value3, int value4, bool value5 = false)
            {
                _format = format;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
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
            public bool Equals(ViewKey other)
            {
                return other._format == _format
                       && other._value1 == _value1
                       && other._value2 == _value2
                       && other._value3 == _value3
                       && other._value4 == _value4
                       && other._value5 == _value5;
            }
            #endregion

            #region IEqualityComparer<ViewKey> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <see cref="ViewKey"/> to compare.</param>
			/// <param name="y">The second object of type <see cref="ViewKey"/> to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
            bool IEqualityComparer<ViewKey>.Equals(ViewKey x, ViewKey y)
            {
                return x.Equals(y);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            int IEqualityComparer<ViewKey>.GetHashCode(ViewKey obj)
            {
                return obj.GetHashCode();
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
        private readonly Dictionary<ViewKey, GorgonBufferShaderView> _bufferViews;				// The cache of buffer views.
        private readonly Dictionary<ViewKey, GorgonTextureShaderView> _textureViews;			// The cache of texture views.
	    private readonly Dictionary<ViewKey, GorgonRenderTargetView> _targetViews;				// The cache of render target views.
        private readonly Dictionary<ViewKey, GorgonUnorderedAccessView> _unorderedViews;        // The cache of unordered access views.
        private readonly Dictionary<ViewKey, GorgonDepthStencilView> _depthViews;               // The cache of depth/stencil views.
        #endregion

        #region Methods.
		/// <summary>
		/// Function to release the Direct3D resources for the views in the cache.
		/// </summary>
	    public void ReleaseResources()
	    {
			foreach (var item in _textureViews)
			{
				item.Value.Dispose();
			}

			foreach (var item in _bufferViews)
			{
				item.Value.CleanUp();
			}

			foreach (var item in _unorderedViews)
			{
				item.Value.CleanUp();
			}

			foreach (var item in _targetViews)
			{
				item.Value.Dispose();
			}

			foreach (var item in _depthViews)
			{
				item.Value.CleanUp();
			}
		}

		/// <summary>
		/// Function to initialize Direct3D resources for the views in the cache.
		/// </summary>
	    public void InitializeResources()
	    {
			foreach (var item in _bufferViews)
			{
				item.Value.InitializeOLDEN();
			}

			foreach (var item in _unorderedViews)
			{
				item.Value.InitializeOLDEN();
			}

			foreach (var item in _depthViews)
			{
				item.Value.InitializeOLDEN();
			}
		}

		/// <summary>
		/// Function to clear the cache.
		/// </summary>
		public void Clear()
		{
			ReleaseResources();

			_bufferViews.Clear();
			_textureViews.Clear();
			_targetViews.Clear();
            _unorderedViews.Clear();
            _depthViews.Clear();
		}

        /// <summary>
        /// Function to create/retrieve a depth/stencil view in the cache.
        /// </summary>
        /// <param name="format">Format of the depth/stencil view.</param>
        /// <param name="mipSlice">Mip slice.</param>
        /// <param name="arrayIndex">Array index.</param>
        /// <param name="arrayCount">Array count.</param>
        /// <param name="flags">Flags for the depth/stencil view.</param>
        /// <returns>The cached depth/stencil view.</returns>
        public GorgonDepthStencilView GetDepthStencilView(BufferFormat format,
                                                                        int mipSlice,
                                                                        int arrayIndex,
                                                                        int arrayCount,
                                                                        DepthStencilViewFlags flags)
        {
            var key = new ViewKey(format, mipSlice, arrayIndex, arrayCount, (int)flags);

            lock (_syncLock)
            {
                GorgonDepthStencilView result;

                if (_depthViews.TryGetValue(key, out result))
                {
                    return result;
                }

                switch (_resource.ResourceType)
                {
                    case ResourceType.Texture1D:
                    case ResourceType.Texture2D:
                        result = new GorgonDepthStencilView(_resource, format, mipSlice, arrayIndex, arrayCount, flags);
                        break;
                }

                // This should never happen.
                if (result == null)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                        string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, _resource.ResourceType));
                }

                result.InitializeOLDEN();
                _depthViews.Add(key, result);

                return result;
            }
        }

        /// <summary>
        /// Function to create/retrieve an unordered access view in the cache.
        /// </summary>
        /// <param name="format">Format of the unordered access view.</param>
        /// <param name="mipSliceElementStart">Mip slice for a texture, element start for a buffer.</param>
        /// <param name="arrayIndexElementCount">Array index for a texture, element count for a buffer.</param>
        /// <param name="arrayCount">Array count for a texture.</param>
        /// <param name="viewType">View type for structured buffers.</param>
        /// <param name="isRaw"><b>true</b> for raw views, <b>false</b> for normal.</param>
        /// <returns>The cached unordered access view.</returns>
        public GorgonUnorderedAccessView GetUnorderedAccessView(BufferFormat format,
                                                                        int mipSliceElementStart,
                                                                        int arrayIndexElementCount,
                                                                        int arrayCount,
                                                                        UnorderedAccessViewType viewType,
                                                                        bool isRaw)
        {
            var key = new ViewKey(format, mipSliceElementStart, arrayIndexElementCount, arrayCount, ((int)viewType) + (isRaw ? 10 : 0));

            lock (_syncLock)
            {
                GorgonUnorderedAccessView result;

                if (_unorderedViews.TryGetValue(key, out result))
                {
                    return result;
                }

                switch (_resource.ResourceType)
                {
                    case ResourceType.Buffer:
                        if (_resource is GorgonStructuredBuffer)
                        {
                            result = new GorgonStructuredBufferUnorderedAccessView(_resource,
                                mipSliceElementStart,
                                arrayIndexElementCount,
                                viewType);
                        }
                        else
                        {
                            result = new GorgonBufferUnorderedAccessView(_resource, format, mipSliceElementStart, arrayIndexElementCount, isRaw);
                        }
                        break;
                    case ResourceType.Texture1D:
                    case ResourceType.Texture2D:
                    case ResourceType.Texture3D:
                        result = new GorgonTextureUnorderedAccessView(_resource, format, mipSliceElementStart, arrayIndexElementCount, arrayCount);
                        break;
                }

                // This should never happen.
                if (result == null)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                        string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, _resource.ResourceType));
                }

                result.InitializeOLDEN();
                _unorderedViews.Add(key, result);

                return result;
            }
        }

		/// <summary>
		/// Function to retrieve a render target view.
		/// </summary>
		/// <param name="format">Format of the view.</param>
		/// <param name="mipSlice">Mip level to use in the view.</param>
		/// <param name="arrayDepthIndex">First array index to use in the view.</param>
		/// <param name="arrayDepthCount">Number of array indices to use in the view.</param>
		/// <returns>The cached render target view.</returns>
		public GorgonRenderTargetView GetRenderTargetView(BufferFormat format,
		                                                      int mipSlice,
		                                                      int arrayDepthIndex,
															  int arrayDepthCount)
		{
			var key = new ViewKey(format, mipSlice, arrayDepthIndex, arrayDepthCount, 0);

			lock(_syncLock)
			{
				GorgonRenderTargetView result;

			    if (_targetViews.TryGetValue(key, out result))
			    {
			        return result;
			    }

			    switch (_resource.ResourceType)
			    {
			        case ResourceType.Texture1D:
			        case ResourceType.Texture2D:
			        case ResourceType.Texture3D:
			            result = new GorgonRenderTargetView(_resource as GorgonTexture, (Format)format, mipSlice, arrayDepthIndex, arrayDepthCount);
			            break;
			    }

			    // This should never happen.
			    if (result == null)
			    {
			        throw new GorgonException(GorgonResult.CannotCreate,
			            string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, _resource.ResourceType));
			    }

			    _targetViews.Add(key, result);

			    return result;
			}
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
            var key = new ViewKey(format,
                                        firstMip,
                                        mipCount,
                                        buffer.ResourceType != ResourceType.Texture3D ? arrayIndex : -1,
                                        buffer.ResourceType != ResourceType.Texture3D ? arrayCount : -1,
                                        buffer.Info.IsCubeMap);

            lock (_syncLock)
            {
                GorgonTextureShaderView result;

                if (_textureViews.TryGetValue(key, out result))
                {
                    return result;
                }

                result = new GorgonTextureShaderView(buffer,
                    (Format)format,
                    firstMip,
                    mipCount,
                    buffer.ResourceType != ResourceType.Texture3D ? arrayIndex : -1,
                    buffer.ResourceType != ResourceType.Texture3D ? arrayCount : -1);
                
                _textureViews.Add(key, result);

                return result;
            }
        }

        /// <summary>
        /// Function to retrieve a buffer view.
        /// </summary>
        /// <param name="format">Format of the view.</param>
        /// <param name="start">Starting element for the view.</param>
        /// <param name="count">Number of elements for the view.</param>
        /// <param name="isRaw"><b>true</b> for raw buffers, <b>false</b> for standard.</param>
        /// <returns>The cached buffer shader view.</returns>
        public GorgonBufferShaderView GetBufferView(BufferFormat format, int start, int count, bool isRaw)
        {
            var key = new ViewKey(format, start, count, Convert.ToInt32(isRaw), 0);

            lock(_syncLock)
            {
                GorgonBufferShaderView result;

                if (_bufferViews.TryGetValue(key, out result))
                {
                    return result;
                }

                result = new GorgonBufferShaderView(_resource, format, start, count, isRaw);

                result.InitializeOLDEN();
                _bufferViews.Add(key, result);

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
            _bufferViews = new Dictionary<ViewKey, GorgonBufferShaderView>();
            _textureViews = new Dictionary<ViewKey, GorgonTextureShaderView>();
			_targetViews = new Dictionary<ViewKey, GorgonRenderTargetView>();
            _unorderedViews = new Dictionary<ViewKey, GorgonUnorderedAccessView>();
            _depthViews = new Dictionary<ViewKey, GorgonDepthStencilView>();
            _resource = resource;
        }
        #endregion

        #region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				Clear();
			}

			_disposed = true;
		}

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
