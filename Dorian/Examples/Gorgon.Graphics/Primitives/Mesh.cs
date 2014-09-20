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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
{
    /// <summary>
    /// Base class for a mesh object.
    /// </summary>
    abstract class Mesh
        : IDisposable
    {
        #region Variables.
        // Flag to indicate that the object was disposed.
        private bool _disposed = false;
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
            int triangleCount = IndexCount / 3;
            var biTanData = new Vector3[VertexCount];
            var tanData = new Vector3[VertexCount];

            for (int i = 0; i < triangleCount; ++i)
            {
                int index1 = *(indexData++);
                int index2 = *(indexData++);
                int index3 = *(indexData++);

                Vertex3D vertex1 = vertexData[index1];
                Vertex3D vertex2 = vertexData[index2];
                Vertex3D vertex3 = vertexData[index3];

                Vector4 vertexEdge1;
                Vector4.Subtract(ref vertex2.Position, ref vertex1.Position, out vertexEdge1);

                Vector4 vertexEdge2;
                Vector4.Subtract(ref vertex3.Position, ref vertex1.Position, out vertexEdge2);

                Vector2 st1;
                Vector2.Subtract(ref vertex2.UV, ref vertex1.UV, out st1);
                Vector2 st2;
                Vector2.Subtract(ref vertex3.UV, ref vertex1.UV, out st2);

                float r = 1.0f / (st1.X * st2.Y - st2.X * st1.Y);

                var s = new Vector3((st2.Y * vertexEdge1.X - st1.Y * vertexEdge2.X) * r,
                                    (st2.Y * vertexEdge1.Y - st1.Y * vertexEdge2.Y) * r,
                                    (st2.Y * vertexEdge1.Z - st2.Y * vertexEdge2.Z) * r);

                var t = new Vector3((st2.X * vertexEdge2.X - st1.X * vertexEdge1.X) * r,
                                    (st2.X * vertexEdge2.Y - st1.X * vertexEdge1.Y) * r,
                                    (st2.X * vertexEdge2.Z - st1.X * vertexEdge1.Z) * r);

                Vector3.Add(ref tanData[index1], ref s, out tanData[index1]);
                Vector3.Add(ref tanData[index2], ref s, out tanData[index2]);
                Vector3.Add(ref tanData[index3], ref s, out tanData[index3]);

                Vector3.Add(ref biTanData[index1], ref t, out biTanData[index1]);
                Vector3.Add(ref biTanData[index2], ref t, out biTanData[index2]);
                Vector3.Add(ref biTanData[index3], ref t, out biTanData[index3]);
            }

            for (int i = 0; i < VertexCount; ++i)
            {
                Vertex3D vertex = vertexData[i];

                Vector3 tangent;
                Vector3 cross;
                float dot;

                Vector3.Subtract(ref tanData[i], ref vertex.Normal, out tangent);
                Vector3.Dot(ref vertex.Normal, ref tanData[i], out dot);
                Vector3.Multiply(ref tangent, dot, out tangent);
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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
