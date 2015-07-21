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
// Created: Monday, September 2, 2013 9:17:27 PM
// 
#endregion

using System;
using Gorgon.Graphics;
using Gorgon.IO;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A cache used to hold vertices for rendering.
    /// </summary>
    class Gorgon2DVertexCache
    {
        #region Variables.
		private readonly Gorgon2D _renderer;                // The renderer bound to this cache.
		private readonly Gorgon2DVertex[] _vertices;        // The list of vertices in the cache.
        private int _verticesWritten;						// The number of vertices written to the cache since the last flush.
        private int _indexCount;							// The number of indices that use the vertices in the cache.
        private int _firstIndex;							// The first index to use when looking up vertices.
        private int _vertexOffset;							// An offset in the vertex list.  Used when data in the vertex buffer does not match up to its index buffer.
        private int _currentVertex;							// The most current vertex in the cache.
        private int _nextVertex;							// Next vertex slots in the cache.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to use this cache or not.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the size of the cache, in vertices.
        /// </summary>
        public int CacheSize
        {
            get;
        }

        /// <summary>
        /// Property to return whether the cache needs to be flushed.
        /// </summary>
        public bool NeedsFlush
        {
            get;
            private set;
        }
        #endregion

        #region Methods.

        /// <summary>
        /// Function to reset the cache.
        /// </summary>
        public void Reset()
        {
            NeedsFlush = false;
            _vertexOffset = 0;
            _verticesWritten = 0;
            _indexCount = 0;
            _firstIndex = 0;
            _currentVertex = 0;
            _nextVertex = 0;
        }

        /// <summary>
        /// Function to add vertices to the cache.
        /// </summary>
        /// <param name="vertices">An array of vertices to add to the cache.</param>
        /// <param name="baseVertex">Base vertex offset to apply to this set of vertices.</param>
        /// <param name="indexCount">Number of indices for this set of vertices.</param>
        /// <param name="firstVertex">First vertex in the array to start copying from.</param>
        /// <param name="vertexCount">Number of vertices in the array to copy.</param>
        public void AddVertices(
            Gorgon2DVertex[] vertices,
            int baseVertex,
            int indexCount,
            int firstVertex,
            int vertexCount)
        {
            // Do nothing.
            if ((vertices == null)
                || (vertices.Length == 0)
                || (vertexCount == 0))
            {
                return;
            }

            // If this set of vertices will overflow the cache, then we need to flush the vertices that we have and reset.
            if (vertexCount + _nextVertex > CacheSize)
            {
                if (_verticesWritten > 0)
                {
	                Flush();
                }

                Reset();
            }

            // Copy into our cache.
            Array.Copy(vertices, firstVertex, _vertices, _nextVertex, vertexCount);
  
            _indexCount += indexCount;
            _nextVertex += vertexCount;
            _verticesWritten = _nextVertex - _currentVertex;

            // Update the offset.
            _vertexOffset += baseVertex;

            NeedsFlush = true;
        }

        /// <summary>
        /// Function to flush the cache by rendering its contents.
        /// </summary>
        public void Flush()
        {
            GorgonVertexBufferBinding binding = _renderer.Graphics.Input.VertexBuffers[0];

			// Apply the current projection/view matrix.
			if (_renderer.Camera.NeedsUpdate)
			{
				_renderer.Camera.Update();
			}

            // Only advance the cache when we've got something to copy into the buffer.
            switch (binding.VertexBuffer.Settings.Usage)
            {
                case BufferUsage.Dynamic:
                    // If we're not at the beginning of the cache, then 
                    // do a no overwrite lock.  This will help performance.
                    var flags = BufferLockFlags.Write
                                | (_currentVertex > 0 ? BufferLockFlags.NoOverwrite : BufferLockFlags.Discard);

                    using(GorgonDataStream stream = binding.VertexBuffer.Lock(flags, _renderer.Graphics))
                    {
                        stream.Position = _currentVertex * Gorgon2DVertex.SizeInBytes;
                        stream.WriteRange(_vertices, _currentVertex, _verticesWritten);
                        binding.VertexBuffer.Unlock();
                    }
                    break;
                default:
                    binding.VertexBuffer.Update(_vertices,
                        _currentVertex * Gorgon2DVertex.SizeInBytes,
                        _renderer.Graphics);
                    break;
            }

            // Draw the buffer data.
            switch (_renderer.Graphics.Input.PrimitiveType)
            {
                case PrimitiveType.PointList:
                case PrimitiveType.LineList:
                    _renderer.Graphics.Output.Draw(_currentVertex, _verticesWritten);
                    break;
                case PrimitiveType.TriangleList:
                    if (_renderer.Graphics.Input.IndexBuffer == null)
                    {
                        _renderer.Graphics.Output.Draw(_currentVertex, _verticesWritten);
                    }
                    else
                    {
                        _renderer.Graphics.Output.DrawIndexed(_firstIndex, _vertexOffset, _indexCount);
                    }
                    break;
            }

            _currentVertex = _nextVertex;
            _firstIndex += _indexCount;
            _verticesWritten = 0;
            _indexCount = 0;
            NeedsFlush = false;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DVertexCache"/> class.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="cacheSize">Size of the cache.</param>
        public Gorgon2DVertexCache(Gorgon2D renderer, int cacheSize)
        {
            _renderer = renderer;
            _vertices = new Gorgon2DVertex[cacheSize];
            Enabled = true;
            CacheSize = cacheSize;
        }
        #endregion
    }
}
