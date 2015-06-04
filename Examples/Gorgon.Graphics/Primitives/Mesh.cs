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
// Created: Thursday, September 18, 2014 2:11:22 AM
// 
#endregion

using System;
using Gorgon.Math;
using SlimMath;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// Base class for a mesh object.
    /// </summary>
    abstract class Mesh
        : IDisposable
    {
        #region Variables.
        // Flag to indicate that the object was disposed.
        private bool _disposed;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of primitive used to draw the object.
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get;
            protected set;
        }

		/// <summary>
		/// Property to return the number of triangles.
		/// </summary>
	    public int TriangleCount
	    {
		    get;
		    protected set;
	    }

        /// <summary>
        /// Property to return the number of vertices.
        /// </summary>
        public int VertexCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the number of indices.
        /// </summary>
        public int IndexCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the vertex buffer.
        /// </summary>
        public GorgonVertexBuffer VertexBuffer
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the index buffer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            protected set;
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

        #region Methods.
        /// <summary>
        /// Function to calculate tangent information for bump mapping.
        /// </summary>
        /// <param name="vertexData">Buffer holding the vertices.</param>
        /// <param name="indexData">Buffer holding the indices.</param>
        protected unsafe void CalculateTangents(Vertex3D* vertexData, int* indexData)
        {
			var biTanData = new Vector3[VertexCount];
			var tanData = new Vector3[VertexCount];

            for (int i = 0; i < TriangleCount; ++i)
            {
                int index1 = *(indexData++);

				// If we hit a strip-restart index, then skip to the next index.
	            if ((PrimitiveType == PrimitiveType.TriangleStrip) 
					&& (index1 < 0))
	            {
		            index1 = *(indexData++);
	            }

                int index2 = *(indexData++);
                int index3 = *(indexData++);

                Vertex3D vertex1 = vertexData[index1];
                Vertex3D vertex2 = vertexData[index2];
                Vertex3D vertex3 = vertexData[index3];

                Vector4 deltaPos1;
                Vector4.Subtract(ref vertex2.Position, ref vertex1.Position, out deltaPos1);

                Vector4 deltaPos2;
                Vector4.Subtract(ref vertex3.Position, ref vertex1.Position, out deltaPos2);

                Vector2 deltaUV1;
                Vector2.Subtract(ref vertex2.UV, ref vertex1.UV, out deltaUV1);
                Vector2 deltaUV2;
                Vector2.Subtract(ref vertex3.UV, ref vertex1.UV, out deltaUV2);

	            float denom = (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
	            float r = 0.0f;

	            if (!denom.EqualsEpsilon(0))
	            {
		            r = 1.0f / denom;
	            }
				
	            var tangent = new Vector3((deltaUV2.Y * deltaPos1.X - deltaUV1.Y * deltaPos2.X) * r,
	                                      (deltaUV2.Y * deltaPos1.Y - deltaUV1.Y * deltaPos2.Y) * r,
	                                      (deltaUV2.Y * deltaPos1.Z - deltaUV1.Y * deltaPos2.Z) * r);

				var biTangent = new Vector3((deltaUV1.X * deltaPos2.X - deltaUV2.X * deltaPos1.X) * r,
	                                        (deltaUV1.X * deltaPos2.Y - deltaUV2.X * deltaPos1.Y) * r,
	                                        (deltaUV1.X * deltaPos2.Z - deltaUV2.X * deltaPos1.Z) * r);

                Vector3.Add(ref tanData[index1], ref tangent, out tanData[index1]);
                Vector3.Add(ref tanData[index2], ref tangent, out tanData[index2]);
                Vector3.Add(ref tanData[index3], ref tangent, out tanData[index3]);

                Vector3.Add(ref biTanData[index1], ref biTangent, out biTanData[index1]);
                Vector3.Add(ref biTanData[index2], ref biTangent, out biTanData[index2]);
                Vector3.Add(ref biTanData[index3], ref biTangent, out biTanData[index3]);
            }

            for (int i = 0; i < VertexCount; ++i)
            {
                Vertex3D vertex = vertexData[i];

                Vector3 tangent;
                Vector3 cross;
                float dot;

				Vector3.Dot(ref vertex.Normal, ref tanData[i], out dot);
				Vector3.Multiply(ref vertex.Normal, dot, out tangent);
				Vector3.Subtract(ref tanData[i], ref tangent, out tangent);
                Vector3.Normalize(ref tangent, out tangent);

                Vector3.Cross(ref vertex.Normal, ref tanData[i], out cross);
                Vector3.Dot(ref cross, ref biTanData[i], out dot);

                vertexData[i] = new Vertex3D
                {
                    Position = vertex.Position,
                    Normal = vertex.Normal,
                    UV = vertex.UV,
                    Tangent = new Vector4(tangent, dot < 0.0f ? -1.0f : 1.0f)
                };
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (VertexBuffer != null)
                {
                    VertexBuffer.Dispose();
                }

                if (IndexBuffer != null)
                {
                    IndexBuffer.Dispose();
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
