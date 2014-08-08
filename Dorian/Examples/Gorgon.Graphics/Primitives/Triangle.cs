#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 11:03:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
{
	class Triangle
		: IPrimitive, IDisposable
	{
		#region Variables.
		// Flag to indicate that the object is disposed.
		private bool _disposed;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Triangle"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		/// <param name="point1">The 1st point in the triangle.</param>
		/// <param name="point2">The 2nd point in the triangle.</param>
		/// <param name="point3">The 3rd point in the triangle.</param>
		public Triangle(GorgonGraphics graphics, Vertex3D point1, Vertex3D point2, Vertex3D point3)
		{
			VertexBuffer = graphics.Buffers.CreateVertexBuffer("TriVB",
			                                                   new[]
			                                                   {
				                                                   point1,
				                                                   point2,
				                                                   point3
			                                                   });

			IndexBuffer = graphics.Buffers.CreateIndexBuffer("TriIB",
			                                                  new[]
			                                                  {
				                                                  0,
				                                                  1,
				                                                  2
			                                                  }, BufferUsage.Immutable);
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
				if (IndexBuffer != null)
				{
					IndexBuffer.Dispose();
				}

				if (VertexBuffer != null)
				{
					VertexBuffer.Dispose();
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

		#region IPrimitive Members
		/// <summary>
		/// Property to return the number of vertices.
		/// </summary>
		public int VertexCount
		{
			get
			{
				return 3;
			}
		}

		/// <summary>
		/// Property to return the number of indices.
		/// </summary>
		public int IndexCount
		{
			get
			{
				return 3;
			}
		}

		/// <summary>
		/// Property to return the vertex buffer.
		/// </summary>
		public GorgonVertexBuffer VertexBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the index buffer.
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get;
			private set;
		}
		#endregion
	}
}
