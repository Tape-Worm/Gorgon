#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Thursday, November 01, 2007 11:57:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing a cache of vertex/index data for the sprites.
	/// </summary>
	internal static class Geometry
	{
		#region Variables.
		private static VertexCache<VertexTypeList.PositionDiffuse2DTexture1> _vertices;	// Cache vertex buffer.
		private static IndexCache<short> _indices;												// Cache index buffer.			
		private static PrimitiveStyle _currentPrimitiveStyle;									// Current used primitive style.
		private static bool _currentUseIndices;													// Current use indices flag.			
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the offset for the vertex buffer.
		/// </summary>
		public static int VertexOffset
		{
			get
			{
				if (_vertices != null)
					return _vertices.DataOffset;

				return 0;
			}
			set
			{
				if (_vertices != null)
					_vertices.DataOffset = value;
			}
		}

		/// <summary>
		/// Property to return the offset for the index buffer.
		/// </summary>
		public static int IndexOffset
		{
			get
			{
				if (_indices != null)
					return _indices.DataOffset;
				return 0;
			}
		}

		/// <summary>
		/// Property to return the number of indices written to the cache.
		/// </summary>
		public static int IndicesWritten
		{
			get
			{
				if (_vertices != null)
					return (_vertices.DataWritten * 6) / 4;
				return 0;
			}
		}

		/// <summary>
		/// Property to return the number of vertices written to the cache.
		/// </summary>
		public static int VerticesWritten
		{
			get
			{
				if (_vertices != null)
					return _vertices.DataWritten;
				return 0;
			}
			set
			{
				if (_vertices != null)
					_vertices.DataWritten = value;
			}
		}

		/// <summary>
		/// Property to return whether we're using indices or not.
		/// </summary>
		public static bool UseIndices
		{
			get
			{
				return _currentUseIndices;
			}
			set
			{
				_currentUseIndices = value;
			}
		}

		/// <summary>
		/// Property to return the vertex buffer.
		/// </summary>
		/// <value></value>
		public static VertexBuffer VertexBuffer
		{
			get
			{
				if (_vertices != null)
					return _vertices.VertexBuffer;
				return null;
			}
		}

		/// <summary>
		/// Property to return the index buffer.
		/// </summary>
		/// <value></value>
		public static IndexBuffer IndexBuffer
		{
			get
			{
				if (_indices != null)
					return _indices.IndexBuffer;
				return null;
			}
		}

		/// <summary>
		/// Property to return vertex type.
		/// </summary>
		/// <value></value>
		public static VertexType VertexType
		{
			get
			{
				return _vertices.VertexType;
			}
		}

		/// <summary>
		/// Property to set or return the style of primitive to use.
		/// </summary>
		/// <value></value>
		public static PrimitiveStyle PrimitiveStyle
		{
			get
			{
				return _currentPrimitiveStyle;
			}
			set
			{
				_currentPrimitiveStyle = value;
			}
		}

		/// <summary>
		/// Property to return the allocated number of vertices in the buffer.
		/// </summary>
		public static int VertexCount
		{
			get
			{
				if (_vertices != null)
					return _vertices.Count;
				return 0;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform clean up.
		/// </summary>		
		public static void DestroyBuffers()
		{
			if (_indices != null)
				_indices.Dispose();

			if (_vertices != null)
				_vertices.Dispose();

			// Do unmanaged clean up.
			_vertices = null;
			_indices = null;
		}

		/// <summary>
		/// Function to update the vertex data.
		/// </summary>
		/// <param name="vertexCount">Number of vertices in the buffer.</param>
		public static void UpdateVertexData(int vertexCount)
		{
			// Create buffers.
			if (_vertices != null)
				_vertices.Dispose();
			if (_indices != null)
				_indices.Dispose();

			_vertices = new VertexCache<VertexTypeList.PositionDiffuse2DTexture1>();
			_indices = new IndexCache<short>();

			// Initialize vertex type.
			_vertices.VertexType = Gorgon.Renderer.VertexTypes["PositionDiffuse2DTexture1"];

			// Set up access types.
			_vertices.BufferUsage = BufferUsages.Dynamic | BufferUsages.WriteOnly;
			_indices.BufferUsage = BufferUsages.Static | BufferUsages.WriteOnly;

			_vertices.Count = vertexCount;
			_indices.Count = (vertexCount / 4) * 6;

			// Build the index buffer.
			short index = 0;
			int pos = 0;
			for (int i = 0; i < (_vertices.Count / 4); i++)
			{
				_indices.CachedData[pos] = (short)(index + 2);
				_indices.CachedData[pos + 1] = (short)(index + 1);
				_indices.CachedData[pos + 2] = index;
				_indices.CachedData[pos + 3] = index;
				_indices.CachedData[pos + 4] = (short)(index + 3);
				_indices.CachedData[pos + 5] = (short)(index + 2);
				pos += 6;
				index += 4;
			}
			_indices.DataWritten = _indices.Count;
		}

		/// <summary>
		/// Property to return the vertex cache for the list.
		/// </summary>
		public static VertexCache<VertexTypeList.PositionDiffuse2DTexture1> VertexCache
		{
			get
			{
				return _vertices;
			}
		}
		#endregion
	}
}
