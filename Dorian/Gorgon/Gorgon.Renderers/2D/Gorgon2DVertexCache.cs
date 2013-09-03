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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A cache used to hold vertices for rendering.
	/// </summary>
	class Gorgon2DVertexCache
	{
		#region Variables.
		private readonly Gorgon2DVertex[] _vertices;					// The list of vertices in the cache.
		private readonly Gorgon2D _renderer;							// The renderer that owns this cache.
		private bool _verticesUpdated;									// Flag to indicate that the vertices were updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the size of the cache, in vertices.
		/// </summary>
		public int CacheSize
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the starting vertex for rendering.
		/// </summary>
		public int StartingVertex
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the ending vertex for rendering.
		/// </summary>
		public int EndingVertex
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the first index to start rendering.
		/// </summary>
		public int StartingIndex
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of indices to render.
		/// </summary>
		public int IndexCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current position in the cache.
		/// </summary>
		public int CachePosition
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current base vertex to use as an offset.
		/// </summary>
		public int BaseVertexOffset
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
			BaseVertexOffset = 0;
			CachePosition = 0;
			IndexCount = 0;
			StartingIndex = 0;
			EndingVertex = 0;
			StartingVertex = 0;
		}

		/// <summary>
		/// Function to add vertices to the cache.
		/// </summary>
		/// <param name="vertices">An array of vertices to add to the cache.</param>
		/// <param name="baseVertex">Base vertex offset to apply to this set of vertices.</param>
		/// <param name="indexCount">Number of indices for this set of vertices.</param>
		/// <param name="firstVertex">First vertex in the array to start copying from.</param>
		/// <param name="vertexCount">Number of vertices in the array to copy.</param>
		public void AddVertices(Gorgon2DVertex[] vertices, int baseVertex, int indexCount, int firstVertex, int vertexCount)
		{
			// Do nothing.
			if ((vertices == null)
			    || (vertices.Length == 0))
			{
				return;
			}

			// If this set of vertices will overflow the cache, then we need to flush the vertices that we have and reset.
			if (vertexCount + EndingVertex > CacheSize)
			{
				if (CachePosition > 0)
				{
					_renderer.Flush();
				}

				Reset();
			}

			// Copy into our cache.
			for (int i = 0; i < vertexCount; i++)
			{
				_vertices[i + EndingVertex] = vertices[i + firstVertex];
			}

			IndexCount += indexCount;
			EndingVertex += vertexCount;
			CachePosition += EndingVertex - StartingVertex;

			// Update the offset.
			BaseVertexOffset += baseVertex;

			_verticesUpdated = true;
		}

		/// <summary>
		/// Function to flush the cache by rendering its contents.
		/// </summary>
		public void Flush()
		{
			GorgonVertexBufferBinding binding = _renderer.Graphics.Input.VertexBuffers[0];

			// Only advance the cache when we've got something to copy into the buffer.
			if (_verticesUpdated)
			{
				switch (binding.VertexBuffer.Settings.Usage)
				{
					case BufferUsage.Dynamic:
						var flags = BufferLockFlags.Discard | BufferLockFlags.Write;

						if (StartingVertex > 0)
						{
							flags = BufferLockFlags.NoOverwrite | BufferLockFlags.Write;
						}

						using(GorgonDataStream stream = binding.VertexBuffer.Lock(flags, _renderer.Graphics))
						{
							stream.Position = StartingVertex * Gorgon2DVertex.SizeInBytes;
							stream.WriteRange(_vertices, StartingVertex, CachePosition);
							binding.VertexBuffer.Unlock();
						}
						break;
					default:
						binding.VertexBuffer.Update(_vertices, StartingVertex * Gorgon2DVertex.SizeInBytes, _renderer.Graphics);
						break;
				}
			}

			// Draw the buffer data.
			switch (_renderer.Graphics.Input.PrimitiveType)
			{
				case PrimitiveType.PointList:
				case PrimitiveType.LineList:
					_renderer.Graphics.Output.Draw(StartingVertex, CachePosition);
					break;
				case PrimitiveType.TriangleList:
					if (_renderer.Graphics.Input.IndexBuffer == null)
					{
						_renderer.Graphics.Output.Draw(StartingVertex, CachePosition);
					}
					else
					{
						_renderer.Graphics.Output.DrawIndexed(StartingIndex, BaseVertexOffset, IndexCount);
					}
					break;
			}

			if (!_verticesUpdated)
			{
				return;
			}

			StartingVertex = EndingVertex;
			StartingIndex += IndexCount;
			CachePosition = 0;
			IndexCount = 0;
			_verticesUpdated = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DVertexCache"/> class.
		/// </summary>
		/// <param name="gorgon2D">The renderer that is using this interface.</param>
		/// <param name="cacheSize">Size of the cache, in vertices.</param>
		public Gorgon2DVertexCache(Gorgon2D gorgon2D, int cacheSize)
		{
			_vertices = new Gorgon2DVertex[cacheSize];
			_renderer = gorgon2D;
			CacheSize = cacheSize;
		}
		#endregion
	}
}
