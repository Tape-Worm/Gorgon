#region MIT.
// 
// Gorgon.
// Copyright (C) 2009 Michael Winsor
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
// Created: Tuesday, April 28, 2009 11:18:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GorgonLibrary;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A vertex used by the batch.
	/// </summary>
	public struct BatchVertex
	{
		/// <summary>
		/// A processed vertex.
		/// </summary>
		public VertexTypeList.PositionDiffuse2DTexture1 Vertex;

		/// <summary>
		/// Name of an image associated with the sprite being drawn.
		/// </summary>
		public string ImageName;
	}

	/// <summary>
	/// A batch of sprites to be statically renderered.
	/// </summary>
	/// <remarks>The purpose of this object is to quickly render multiple sprites in one pass.  In the immediate type model used by the <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> object 
	/// the buffer where the sprites are stored is flushed when a change (image change, state change, etc...) occurs.  For example, if you draw multiple sprites and one of those sprites has an image that is different
	/// from the previous sprites, the buffer will flush all the previous sprites to the render target.  Then the process starts over with an empty buffer.  This has advantages, but 
	/// sometimes this will slow down the rendering process.<para>Batched sprites however will take multiple sprite objects and store them into a buffer and will flush that buffer
	/// only when the render target is updated.  This has the advantage of being able to draw all the sprites in one shot and those sprites will be "cached" each time the batch
	/// is drawn so that the buffers that store the sprites never need to be updated (unless a change is made).</para>
	/// <para>There is a caveat with this approach:  The sprites cannot move or be animated in any way.  This means that the sprites stored are "snapshots" of the sprite at a given 
	/// time and will not be affected by any transformations made after the fact.  The sprites will update when the batch is invalidated and rebuilt however, but this process is 
	/// slow and is not recommended for real time use.  The sprites in the batch will be affected by the batch transforms however, but all the sprites will be effected as a single 
	/// entity and not individually.</para>
	/// <para>The best usage of this object is in a "map" scenario, where the map is a tile map and the sprites that compose the map as tiles are blitted in one shot.</para>
	/// </remarks>
	public class Batch
		: NamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private List<Renderable> _sprites = null;				// List of sprites.
		private VertexBuffer _vertices = null;					// List of vertices.
		private IndexBuffer _indices = null;					// List of indices.
		private int _renderableCount = 0;						// Number of renderables to allow.
		private bool _needsRefresh = false;						// Flag to indicate that we need to refresh the batch.
		private IEnumerable<Renderable> _sorted = null;			// List of sorted sprites.
		#endregion

		#region Properties.
		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the buffers to use for the batch.
		/// </summary>
		private void BuildBuffers(int vertexCount)
		{
			int indexCount = 0;
			IndexBufferType indexType = IndexBufferType.Index16;

			if (_vertices != null)
			{
				_vertices.Dispose();
				_vertices = null;
			}
			if (_indices != null)
			{
				_indices.Dispose();
				_indices = null;
			}

			if ((_sprites.Count < 1) || (vertexCount < 1))
			{
				_needsRefresh = false;
				return;
			}

			if ((vertexCount > 65535) && (!Gorgon.CurrentDriver.SupportIndex32))
				vertexCount = 65535;
			else
				indexType = IndexBufferType.Index32;

			indexCount = vertexCount * 6 / 4;

			_vertices = new VertexBuffer(Gorgon.Renderer.VertexTypes["PositionDiffuse2DTexture1"].VertexSize(0), vertexCount, BufferUsages.Static | BufferUsages.WriteOnly);
			_indices = new IndexBuffer(indexType, indexCount, BufferUsages.WriteOnly | BufferUsages.Static);

			// Fill in the index buffer.
			uint index = 0;
			try
			{
				_indices.Lock(BufferLockFlags.Discard);
				for (uint i = 0; i < (vertexCount / 4); i++)
				{
					if (indexType == IndexBufferType.Index32)
					{
						_indices.Write<uint>(index + 2);
						_indices.Write<uint>(index + 1);
						_indices.Write<uint>(index);
						_indices.Write<uint>(index);
						_indices.Write<uint>(index + 3);
						_indices.Write<uint>(index + 2);
					}
					else
					{
						_indices.Write<ushort>((ushort)(index + 2));
						_indices.Write<ushort>((ushort)(index + 1));
						_indices.Write<ushort>((ushort)(index));
						_indices.Write<ushort>((ushort)(index));
						_indices.Write<ushort>((ushort)(index + 3));
						_indices.Write<ushort>((ushort)(index + 2));
					}
					index += 4;
				}
			}
			finally
			{
				_indices.Unlock();
			}
		}

		/// <summary>
		/// Function to re-fill the buffers.
		/// </summary>
		private void RefreshBuffers()
		{
			List<BatchVertex> vertices = null;			// List of vertices.

			if ((!_needsRefresh) || (_sprites.Count < 1))
				return;
			
			// Build the list of vertices to add.
			vertices = new List<BatchVertex>();
			foreach (Renderable renderable in _sprites)
			{
				BatchVertex[] renderableVertices = renderable.GetVertices();

				if (renderableVertices != null)
				{
					foreach (BatchVertex vertex in renderableVertices)
						vertices.Add(vertex);
				}
			}

			if ((_vertices == null) || (_indices == null) || (vertices.Count > _vertices.VertexCount))
				BuildBuffers();


		}

		/// <summary>
		/// Function to perform a re-build of the buffers used for the batch.
		/// </summary>
		public void Refresh()
		{
			_needsRefresh = true;
		}

		/// <summary>
		/// Function to add a renderable to the batch.
		/// </summary>
		/// <param name="renderable">Renderable to add.</param>
		public void AddRenderable(Renderable renderable)
		{
			if (renderable == null)
				throw new ArgumentNullException("renderable");

			_sprites.Add(renderable);
			_needsRefresh = true;
		}

		/// <summary>
		/// Function to remove a renderable from the batch.
		/// </summary>
		/// <param name="renderable">Renderable to remove.</param>
		public void RemoveRenderable(Renderable renderable)
		{
			if (renderable == null)
				throw new ArgumentNullException("renderable");

			_sprites.Remove(renderable);
			_needsRefresh = true;
		}

		/// <summary>
		/// Function to return whether the specified renderable exists in this batch.
		/// </summary>
		/// <param name="renderable">Renderable to find.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool ContainsRenderable(Renderable renderable)
		{
			if (renderable == null)
				throw new ArgumentNullException("renderable");

			return _sprites.Contains(renderable);
		}

		/// <summary>
		/// Function to clear all the renderable objects from the batch.
		/// </summary>
		public void ClearRenderables()
		{
			_sprites.Clear();
			BuildBuffers();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Batch"/> class.
		/// </summary>
		/// <param name="batchName">Name of the batch.</param>
		/// <param name="renderableCount">Initial number of renderables to store in the batch.</param>
		public Batch(string batchName, int renderableCount)
			: base(batchName)
		{
			if (renderableCount < 1)
				throw new ArgumentException("There must be at least 1 renderable.", "renderableCount");

			_sprites = new List<Renderable>();
			_renderableCount = renderableCount;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_sprites != null)
						_sprites.Clear();
					if (_indices != null)
						_indices.Dispose();
					if (_vertices != null)
						_vertices.Dispose();
				}

				_vertices = null;
				_indices = null;
				_disposed = true;
			}
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
