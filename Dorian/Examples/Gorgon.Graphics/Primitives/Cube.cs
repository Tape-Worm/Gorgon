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
// Created: Sunday, August 10, 2014 10:49:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
{
	class Cube
		: MoveableMesh, IPrimitive, IDisposable
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Initial orientation.
		private Matrix _orientation = Matrix.Identity;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the plane vertices.
		/// </summary>
		/// <param name="buffer">Buffer to populate.</param>
		/// <param name="up">Up vector for orientation.</param>
		/// <param name="normal">The face normal.</param>
		/// <param name="size">The width and height of the plane.</param>
		/// <param name="textureCoordinates">The texture coordinates to apply to the plane.</param>
		/// <param name="columns">The number of columns to subdivide by.</param>
		/// <param name="rows">The number of rows to subdivide by.</param>
		private unsafe void GetVertices(Vertex3D* buffer, Vector3 up, Vector3 normal, Vector3 size, RectangleF textureCoordinates, int columns, int rows)
		{
			float columnWidth = 1.0f / columns;
			float columnHeight = 1.0f / rows;
			Matrix rotation = Matrix.Identity;
			Vector3 translate;

			Vector3 orientVector;
			Vector3.Cross(ref normal, ref up, out orientVector);
			Vector3.Multiply(ref normal, 0.5f, out translate);

			rotation.Row1 = orientVector;
			rotation.Row2 = up;
			rotation.Row3 = normal;
			rotation.Row4 = new Vector4(translate, 1);

			for (int y = 0; y <= rows; ++y)
			{
				for (int x = 0; x <= columns; ++x)
				{
					var vertexPos = new Vector3(((x * columnWidth) - 0.5f) * size.X,
					                            ((y * columnHeight) - 0.5f) * size.Y,
					                            0);

					Vector3.TransformCoordinate(ref vertexPos, ref rotation, out vertexPos);
					Vector3.TransformCoordinate(ref vertexPos, ref _orientation, out vertexPos);

					*(buffer++) = new Vertex3D
					{
						Position =
							new Vector4(vertexPos,
										1.0f),
						Normal = normal,
						UV = new Vector2((x * (textureCoordinates.Width / columns)) + textureCoordinates.X,
										 (1.0f - (y * (textureCoordinates.Height / rows))) + textureCoordinates.Y)
					};
				}
			}
		}

		/// <summary>
		/// Function to build the plane indices.
		/// </summary>
		/// <param name="buffer">Buffer to populate.</param>
		/// <param name="vertexStart">The starting vertex.</param>
		/// <param name="columns">Number of columns for the plane.</param>
		/// <param name="rows">Number of rows for the plane.</param>
		private unsafe void GetIndices(int* buffer, int vertexStart, int columns, int rows)
		{
			int columnWrap = columns + 1;

			for (int row = 0; row < rows; ++row)
			{
				for (int column = 0; column < columns; ++column)
				{
					*(buffer++) = (column + (row * columnWrap)) + vertexStart;
					*(buffer++) = (column + ((row + 1) * columnWrap)) + vertexStart;
					*(buffer++) = ((column + 1) + (row * columnWrap)) + vertexStart;

					*(buffer++) = (column + ((row + 1) * columnWrap)) + vertexStart;
					*(buffer++) = ((column + 1) + ((row + 1) * columnWrap)) + vertexStart;
					*(buffer++) = ((column + 1) + (row * columnWrap)) + vertexStart;
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Cube"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		/// <param name="size">The width, height and depth of the cube.</param>
		/// <param name="textureCoordinates">The texture coordinates for the faces of the cube.</param>
		/// <param name="angle">The initial orientation of the cube, in degrees.</param>
		/// <param name="columnsPerFace">The number of columns per face.</param>
		/// <param name="rowsPerFace">The number of rows per face.</param>
		public Cube(GorgonGraphics graphics, Vector3 size, RectangleF textureCoordinates, Vector3 angle, int columnsPerFace = 1, int rowsPerFace = 1)
		{
			Quaternion orientation;
			int faceVertexCount = (columnsPerFace + 1) * (rowsPerFace + 1);
			int faceIndexCount = (columnsPerFace * rowsPerFace) * 6;
			VertexCount = faceVertexCount * 6;
			IndexCount = faceIndexCount * 6;

			Quaternion.RotationYawPitchRoll(angle.Y.Radians(), angle.X.Radians(), angle.Z.Radians(), out orientation);
			Matrix.RotationQuaternion(ref orientation, out _orientation);

			unsafe
			{
				using (var data = new GorgonDataStream(VertexCount * Vertex3D.Size))
				{
					var vertices = (Vertex3D *)data.UnsafePointer;
					GetVertices(vertices, Vector3.UnitY, -Vector3.UnitZ, size, textureCoordinates, columnsPerFace, rowsPerFace);
					vertices += faceVertexCount;

					GetVertices(vertices, -Vector3.UnitZ, -Vector3.UnitY, size, textureCoordinates, columnsPerFace, rowsPerFace);
					vertices += faceVertexCount;

					GetVertices(vertices, Vector3.UnitY, Vector3.UnitZ, size, textureCoordinates, columnsPerFace, rowsPerFace);
					vertices += faceVertexCount;

					GetVertices(vertices, Vector3.UnitY, -Vector3.UnitX, size, textureCoordinates, columnsPerFace, rowsPerFace);
					vertices += faceVertexCount;

					GetVertices(vertices, Vector3.UnitY, Vector3.UnitX, size, textureCoordinates, columnsPerFace, rowsPerFace);
					vertices += faceVertexCount;

					GetVertices(vertices, Vector3.UnitZ, Vector3.UnitY, size, textureCoordinates, columnsPerFace, rowsPerFace);

					VertexBuffer = graphics.Buffers.CreateVertexBuffer("CubeVB", new GorgonBufferSettings
					{
						Usage = BufferUsage.Immutable,
						SizeInBytes = (int)data.Length
					}, data);
				}

				using (var data = new GorgonDataStream(IndexCount * sizeof(int)))
				{
					var indices = (int*)data.UnsafePointer;
					GetIndices(indices, 0, columnsPerFace, rowsPerFace);
					indices += faceIndexCount;

					GetIndices(indices, faceVertexCount, columnsPerFace, rowsPerFace);
					indices += faceIndexCount;

					GetIndices(indices, faceVertexCount * 2, columnsPerFace, rowsPerFace);
					indices += faceIndexCount;

					GetIndices(indices, faceVertexCount * 3, columnsPerFace, rowsPerFace);
					indices += faceIndexCount;

					GetIndices(indices, faceVertexCount * 4, columnsPerFace, rowsPerFace);
					indices += faceIndexCount;

					GetIndices(indices, faceVertexCount * 5, columnsPerFace, rowsPerFace);
					
					IndexBuffer = graphics.Buffers.CreateIndexBuffer("CubeIB", new GorgonIndexBufferSettings
					{
						Usage = BufferUsage.Immutable,
						Use32BitIndices = true,
						SizeInBytes = (int)data.Length
					}, data);
				}
			}
		}
		#endregion

		#region IPrimitive Members
		/// <summary>
		/// Property to return the type of primitive used to draw the object.
		/// </summary>
		public PrimitiveType PrimitiveType
		{
			get
			{
				return PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to return the number of vertices.
		/// </summary>
		public int VertexCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of indices.
		/// </summary>
		public int IndexCount
		{
			get;
			private set;
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

		/// <summary>
		/// Property to set or return the texture to use.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			set;
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
	}
}
