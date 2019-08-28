#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 7, 2018 4:32:07 PM
// 
#endregion

using System;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines an interface for rendering objects.
    /// </summary>
    internal abstract class ObjectRenderer
        : IGorgonGraphicsObject, IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to return the graphics interface used to render objects.
        /// </summary>
        public abstract GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the vertex buffer used by the object renderer.
        /// </summary>
        public GorgonVertexBufferBinding VertexBuffer
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the index buffer used by the object renderer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the equality comparer for renderable object state.
        /// </summary>
        public BatchRenderableStateEqualityComparer RenderableStateComparer
        {
            get;
        } = new BatchRenderableStateEqualityComparer();
        #endregion

        #region Methods.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            VertexBuffer.VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
        #endregion
    }
}
