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

using GorgonLibrary.Math;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
{
	/// <summary>
	/// A sphere object.
	/// </summary>
	class Sphere
		: Model
	{
		#region Properties.
		/// <summary>
		/// Property to return the radius of the sphere.
		/// </summary>
		public float Radius
		{
			get;
			private set;
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw our sphere.
		/// </summary>
		public override void Draw()
		{
			// If our transforms have updated, then calculate the new world matrix.
			UpdateTransform();
			
			Program.Graphics.Input.IndexBuffer = IndexBuffer;
			Program.Graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(VertexBuffer, BoingerVertex.Size);

			Program.Graphics.Output.DrawIndexed(0, 0, Indices.Length);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Sphere" /> class.
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="textureOffset">Offset of the texture.</param>
		/// <param name="textureScale">Scale of the texture.</param>
		public Sphere(float radius, Vector2 textureOffset, Vector2 textureScale)
			: base()
		{
			int ringCount = 8;						// Number of rings.
			int segmentCount = 16;					// Number of segments.
			float deltaRingAngle = 0;				// Delta angle between rings.
			float deltaSegAngle = 0;				// Delta angle between segments.
			int index = 0;							// Current index.
			int vertexIndex = 0;					// Current vertex index.
			int indexIndex = 0;						// Current index array index.
			int vertexCount = 0;					// Number of vertices.
			int indexCount = 0;						// Number of indices.

			deltaRingAngle = ((float)System.Math.PI) / (float)ringCount;
			deltaSegAngle = (((float)System.Math.PI) * 2.0f) / (float)segmentCount;

			// Calculate number of vertices and indices required for our sphere.
			vertexCount = (ringCount + 1) * (segmentCount + 1);
			indexCount = 6 * ringCount * (segmentCount + 1);

			Vertices = new BoingerVertex[vertexCount];
			Indices = new int[indexCount];

			Radius = radius;

			// Build our sphere.
			for (int ring = 0; ring <= ringCount; ring++)
			{
				float angle = deltaRingAngle * (float)ring;
				float ringSin = angle.Sin();
				Vector3 position = new Vector3(0, angle.Cos() * radius, 0);

				for (int segment = 0; segment <= segmentCount; segment++)
				{
					Vector2 textureDelta = new Vector2(1.0f - (float)segment / (float)segmentCount, 1.0f - (float)ring / (float)ringCount);
					float segmentAngle = deltaSegAngle * (float)segment;

					position.X = ringSin * segmentAngle.Sin() * radius;
					position.Z = ringSin * segmentAngle.Cos() * radius;

					// Create the vertex.
					textureDelta.X *= textureScale.X;
					textureDelta.Y *= textureScale.Y;
					textureDelta.X += textureOffset.X;
					textureDelta.Y += textureOffset.Y;

					Vertices[vertexIndex++] = new BoingerVertex(
								position,
								textureDelta									
							);

					// Add the indices and skip the last ring.
					if (ring != ringCount)
					{
						Indices[indexIndex++] = (index + segmentCount + 1);
						Indices[indexIndex++] = index;
						Indices[indexIndex++] = (index + segmentCount);
						Indices[indexIndex++] = (index + segmentCount + 1);
						Indices[indexIndex++] = (index + 1);
						Indices[indexIndex++] = index;
						index++;
					}
				}
			}
			
			// Create our buffers.
			VertexBuffer = Program.Graphics.Input.CreateVertexBuffer<BoingerVertex>(BufferUsage.Immutable, Vertices);
			IndexBuffer = Program.Graphics.Input.CreateIndexBuffer<int>(BufferUsage.Immutable, true, Indices);
		}
		#endregion
	}
}
