#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 4, 2017 11:24:02 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Diagnostics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A base class for common functionality for generic, structures and raw buffers.
    /// </summary>
    public abstract class GorgonBufferCommon
        : GorgonBufferBase
    {
        #region Variables.
        // A cache of shader views for the buffer.
        private readonly Dictionary<BufferShaderViewKey, GorgonShaderResourceView> _shaderViews = new Dictionary<BufferShaderViewKey, GorgonShaderResourceView>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return a cached shader resource view.
        /// </summary>
        /// <param name="key">The key associated with the view.</param>
        /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
        internal GorgonShaderResourceView GetView(BufferShaderViewKey key)
        {
            return _shaderViews.TryGetValue(key, out GorgonShaderResourceView view) ? view : null;
        }

        /// <summary>
        /// Function to register the shader resource view in the cache.
        /// </summary>
        /// <param name="key">The unique key for the shader view.</param>
        /// <param name="view">The view to register.</param>
        internal void RegisterView(BufferShaderViewKey key, GorgonShaderResourceView view)
        {
            _shaderViews[key] = view;
        }

        /// <summary>
        /// Function to retrieve the total number of elements that can be placed in the buffer.
        /// </summary>
        /// <param name="info">Information about the element format.</param>
        /// <returns>The number of elements in the buffer.</returns>
        protected int GetTotalElementCount(GorgonFormatInfo info)
        {
            if (info.IsTypeless)
            {
                return 0;
            }

            return SizeInBytes / info.SizeInBytes;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the buffer is locked when this method is called, it will automatically be unlocked and any lock pointer will be invalidated.
        /// </para>
        /// <para>
        /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
        /// </para>
        /// </remarks>
        public override void Dispose()
        {
            // Remove any cached views for this buffer.
            foreach (KeyValuePair<BufferShaderViewKey, GorgonShaderResourceView> view in _shaderViews)
            {
                view.Value.Dispose();
            }

            _shaderViews.Clear();

            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferCommon"/> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics" /> object used to create and manipulate the buffer.</param>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="log">The log interface used for debug logging.</param>
        protected GorgonBufferCommon(GorgonGraphics graphics, string name, IGorgonLog log)
            : base(graphics, name, log)
        {
            
        }
        #endregion
    }
}
