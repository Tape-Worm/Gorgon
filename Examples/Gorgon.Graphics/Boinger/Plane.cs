﻿#region MIT.
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
// Created: Sunday, December 30, 2012 10:25:22 AM
// 
#endregion

using Gorgon.Graphics.Core;
using Gorgon.Native;
using D3D11 = SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D;
using DX = SharpDX;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// A plane object.
	/// </summary>
	class Plane
		: Model
	{
		#region Properties.
		/// <summary>
		/// Property to return the size of the plane.
		/// </summary>
		public DX.Vector2 Size
		{
			get;
			private set;
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Plane" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create the buffers for this object.</param>
		/// <param name="inputLayout">The input layout for the vertices in this mesh.</param>
		/// <param name="size">The width and height of the plane.</param>
		/// <param name="textureCoordinates">Texture coordinates.</param>
		public Plane(GorgonGraphics graphics, GorgonInputLayout inputLayout, DX.Vector2 size, DX.RectangleF textureCoordinates)
			: base(inputLayout)
		{
			Size = size;

			// Create our vertices.
			Vertices = new[]
			           {
				           new BoingerVertex(new DX.Vector3(-size.X, size.Y, 0.0f), textureCoordinates.Location),
				           new BoingerVertex(new DX.Vector3(size.X, size.Y, 0.0f), new DX.Vector2(textureCoordinates.Right, textureCoordinates.Top)),
				           new BoingerVertex(new DX.Vector3(-size.X, -size.Y, 0.0f), new DX.Vector2(textureCoordinates.Left, textureCoordinates.Bottom)),
				           new BoingerVertex(new DX.Vector3(size.X, -size.Y, 0.0f), new DX.Vector2(textureCoordinates.Right, textureCoordinates.Bottom))
			           };

			// Create our indices.
			Indices = new[]
			          {
				          0,
				          1,
				          2,
				          2,
				          1,
				          3
			          };

			// Copy the above vertex/index data into a vertex and index buffer so we can render our plane.
			using (var vertexPtr = new GorgonPointerPinned<BoingerVertex>(Vertices))
			using (var indexPtr = new GorgonPointerPinned<int>(Indices))
			{
				VertexBufferBindings[0] = new GorgonVertexBufferBinding(new GorgonVertexBuffer("Plane Vertex Buffer",
					                                                                            graphics,
					                                                                            new GorgonVertexBufferInfo
					                                                                            {
						                                                                            Usage = D3D11.ResourceUsage.Immutable,
						                                                                            SizeInBytes = Vertices.Length * BoingerVertex.Size
					                                                                            },
					                                                                            vertexPtr),
					                                                    BoingerVertex.Size);
				IndexBuffer = new GorgonIndexBuffer("Plane Index Buffer",
					                                graphics,
					                                new GorgonIndexBufferInfo
					                                {
						                                Usage = D3D11.ResourceUsage.Immutable,
						                                IndexCount = Indices.Length,
						                                Use16BitIndices = false
					                                },
					                                indexPtr);
			}
		}
		#endregion
	}
}
