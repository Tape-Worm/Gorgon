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
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// A sphere object.
	/// </summary>
	class Sphere
		: MoveableMesh
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Initial orientation.
		private Matrix _orientation = Matrix.Identity;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the radius of the sphere.
		/// </summary>
		public float Radius
		{
			get;
			private set;
		}

		public GorgonVertexBuffer Normals
		{
			get;
			private set;
		}
		#endregion

		#region Methods.

		/// <summary>
		/// Function to create the vertex data for the sphere.
		/// </summary>
		/// <param name="vertexData">Pointer to the buffer that will hold the vertex data.</param>
		/// <param name="indexData">Pointer to the buffer that will hold the index data.</param>
		/// <param name="radius">Radius of the sphere.</param>
		/// <param name="textureCoordinates">Texture coordinates for the sphere.</param>
		/// <param name="ringCount">Number of rings in the sphere.</param>
		/// <param name="segmentCount">Number of segments in the sphere.</param>
		private unsafe void GetVertices(Vertex3D* vertexData,
		                                int* indexData,
			void *normalData,
		                                float radius,
		                                RectangleF textureCoordinates,
		                                int ringCount,
		                                int segmentCount)
		{
			int index = 0;						// Current index.
			float deltaRingAngle = ((float)System.Math.PI) / ringCount;
			float deltaSegAngle = (((float)System.Math.PI) * 2.0f) / segmentCount;
			var linePtStart = (Vector4*)normalData;

			Radius = radius;

			// Build our sphere.
			for (int ring = 0; ring <= ringCount; ring++)
			{
				float ringAngle = ring * deltaRingAngle;
				radius = ringAngle.Sin() * 0.5f * Radius;
				float radiusY = ringAngle.Cos() * Radius * 0.5f;

				for (int segment = 0; segment <= segmentCount; segment++)
				{
					var textureDelta = new Vector2(1.0f - segment / (float)segmentCount, ring / (float)ringCount);
					float segmentAngle = deltaSegAngle * segment;

					var position = new Vector3(radius * segmentAngle.Sin(), radiusY, radius * segmentAngle.Cos());

					Vector3 normal;

					Vector3.Multiply(ref position, 2.0f, out normal);
					Vector3.TransformCoordinate(ref position, ref _orientation, out position);
					Vector3.TransformCoordinate(ref normal, ref _orientation, out normal);
					normal.Normalize();

					*(linePtStart++) = new Vector4(position, 1.0f);
					*(linePtStart++) = new Vector4(position + (normal * 0.05f), 1.0f);				

					// Create the vertex.
					textureDelta.X *= textureCoordinates.Width;
					textureDelta.Y *= textureCoordinates.Height;
					textureDelta.X += textureCoordinates.X;
					textureDelta.Y += textureCoordinates.Y;

					*(vertexData++) = new Vertex3D
					              {
						              Position = new Vector4(position, 1.0f),
						              UV = textureDelta,
									  Normal = normal
					              };

					// Add the indices and skip the last ring.
					if (ring == ringCount)
					{
						continue;
					}

					*(indexData++) = (index + segmentCount + 1);
					*(indexData++) = index;
					*(indexData++) = (index + segmentCount);
					
					*(indexData++) = (index + segmentCount + 1);
					*(indexData++) = (index + 1);
					*(indexData++) = index;
					index++;
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
	    /// <summary>
	    /// Initializes a new instance of the <see cref="Sphere" /> class.
	    /// </summary>
	    /// <param name="graphics">Graphics interface to use.</param>
	    /// <param name="radius">Radius of the sphere</param>
	    /// <param name="textureCoordinates">The texture coordinates to apply to the sphere.</param>
	    /// <param name="angle">The angle of rotation, in degrees.</param>
	    /// <param name="ringCount">Number of rings in the sphere.</param> 
	    /// <param name="segmentCount">Number of segments in the sphere.</param>
	    public Sphere(GorgonGraphics graphics, float radius, RectangleF textureCoordinates, Vector3 angle, int ringCount = 8, int segmentCount = 16)
	    {
		    Quaternion orientation;

			// Calculate number of vertices and indices required for our sphere.
		    PrimitiveType = PrimitiveType.TriangleList;
			VertexCount = (ringCount + 1) * (segmentCount + 1);
			IndexCount = 6 * ringCount * (segmentCount + 1);
		    TriangleCount = IndexCount / 3;

			Quaternion.RotationYawPitchRoll(angle.Y.Radians(), angle.X.Radians(), angle.Z.Radians(), out orientation);
		    Matrix.RotationQuaternion(ref orientation, out _orientation);

		    unsafe
		    {
			    using (var vertexData = new GorgonDataStream(VertexCount * Vertex3D.Size))
				using (var normalData = new GorgonDataStream(VertexCount * 2 * Vector4.SizeInBytes))
				using (var indexData = new GorgonDataStream(IndexCount * DirectAccess.SizeOf<int>()))
				{
					GetVertices((Vertex3D *)vertexData.UnsafePointer, (int *)indexData.UnsafePointer, normalData.UnsafePointer, radius, textureCoordinates, ringCount, segmentCount);

					VertexBuffer = graphics.Buffers.CreateVertexBuffer("SphereVertexBuffer",
					                                                   new GorgonBufferSettings
					                                                   {
						                                                   Usage = BufferUsage.Immutable,
						                                                   SizeInBytes = (int)vertexData.Length
					                                                   },
					                                                   vertexData);

					IndexBuffer = graphics.Buffers.CreateIndexBuffer("SphereIndexBuffer",
					                                                 new GorgonIndexBufferSettings
					                                                 {
						                                                 Usage = BufferUsage.Immutable,
						                                                 Use32BitIndices = true,
						                                                 SizeInBytes = (int)indexData.Length
					                                                 },
					                                                 indexData);

					Normals = graphics.Buffers.CreateVertexBuffer("NormalsBuffer",
					                                              new GorgonBufferSettings
					                                              {
						                                              Usage = BufferUsage.Immutable,
						                                              SizeInBytes = (int)normalData.Length
					                                              },
					                                              normalData);
				}
		    }
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Normals != null)
				{
					Normals.Dispose();
				}
			}

			_disposed = true;
		}
		#endregion
	}
}
