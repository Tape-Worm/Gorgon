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
// Created: Sunday, December 30, 2012 10:25:22 AM
// 
#endregion

using System.Drawing;
using SlimMath;

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
		public Vector2 Size
		{
			get;
			private set;
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw our plane.
		/// </summary>
		public override void Draw()
		{
			// If our transforms have updated, then calculate the new world matrix.
			UpdateTransform();
			
			Program.Graphics.Input.IndexBuffer = IndexBuffer;
			Program.Graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(VertexBuffer, BoingerVertex.Size);

			Program.Graphics.Output.DrawIndexed(0, 0, 6);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Plane" /> class.
		/// </summary>
		/// <param name="size">The width and height of the plane.</param>
		/// <param name="textureCoordinates">Texture coordinates.</param>
		public Plane(Vector2 size, RectangleF textureCoordinates)
		{
			Size = size;
			
			// Create our vertices.
			Vertices = new [] {
				new BoingerVertex(new Vector3(-size.X, size.Y, 0.0f), textureCoordinates.Location),
				new BoingerVertex(new Vector3(size.X, size.Y, 0.0f), new Vector2(textureCoordinates.Right, textureCoordinates.Top)),
				new BoingerVertex(new Vector3(-size.X, -size.Y, 0.0f), new Vector2(textureCoordinates.Left, textureCoordinates.Bottom)),
				new BoingerVertex(new Vector3(size.X, -size.Y, 0.0f), new Vector2(textureCoordinates.Right, textureCoordinates.Bottom))
			};

			VertexBuffer = Program.Graphics.Buffers.CreateVertexBuffer("Plane Vertex Buffer", Vertices, BufferUsage.Immutable);

			// Create our indices.
			Indices = new[] {
				0,
				1,
				2,
				2,
				1,
				3
			};

			IndexBuffer = Program.Graphics.Buffers.CreateIndexBuffer("Plane Index Buffer", Indices, BufferUsage.Immutable);
		}
		#endregion
	}
}
