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

using System.Numerics;
using System.Drawing;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Examples
{
    /// <summary>
    /// A sphere object.
    /// </summary>
    internal class Sphere
        : MoveableMesh
    {
        #region Variables.
        // Initial orientation.
        private Matrix4x4 _orientation;
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
        private void GetVertices(GorgonVertexPosNormUvTangent[] vertexData, int[] indexData, float radius, RectangleF textureCoordinates, int ringCount, int segmentCount)
        {
            int index = 0; // Current index.
            int vertexOffset = 0;
            int indexOffset = 0;
            float deltaRingAngle = ((float)System.Math.PI) / ringCount;
            float deltaSegAngle = (((float)System.Math.PI) * 2.0f) / segmentCount;

            Radius = radius;

            // Build our sphere.
            for (int ring = 0; ring <= ringCount; ring++)
            {
                float ringAngle = ring * deltaRingAngle;
                radius = ringAngle.Sin() * 0.5f * Radius;
                float radiusY = ringAngle.Cos() * Radius * 0.5f;

                for (int segment = 0; segment <= segmentCount; segment++)
                {
                    var textureDelta = new Vector2(1.0f - (segment / (float)segmentCount), ring / (float)ringCount);
                    float segmentAngle = deltaSegAngle * segment;

                    var position = new Vector3(radius * segmentAngle.Sin(), radiusY, radius * segmentAngle.Cos());

                    var normal = Vector3.Multiply(position, 2.0f);
                    position = Vector3.Transform(position, _orientation);
                    normal = Vector3.Normalize(Vector3.Transform(normal, _orientation));                    

                    // Create the vertex.
                    textureDelta.X *= textureCoordinates.Width;
                    textureDelta.Y *= textureCoordinates.Height;
                    textureDelta.X += textureCoordinates.X;
                    textureDelta.Y += textureCoordinates.Y;

                    vertexData[vertexOffset++] = new GorgonVertexPosNormUvTangent
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

                    indexData[indexOffset++] = (index + segmentCount + 1);
                    indexData[indexOffset++] = index;
                    indexData[indexOffset++] = (index + segmentCount);

                    indexData[indexOffset++] = (index + segmentCount + 1);
                    indexData[indexOffset++] = (index + 1);
                    indexData[indexOffset++] = index;
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
            : base(graphics)
        {

            // Calculate number of vertices and indices required for our sphere.
            PrimitiveType = PrimitiveType.TriangleList;
            VertexCount = (ringCount + 1) * (segmentCount + 1);
            IndexCount = 6 * ringCount * (segmentCount + 1);
            TriangleCount = IndexCount / 3;

            var orientation = Quaternion.CreateFromYawPitchRoll(angle.Y.ToRadians(), angle.X.ToRadians(), angle.Z.ToRadians());
            _orientation = Matrix4x4.CreateFromQuaternion(orientation);

            var vertexData = new GorgonVertexPosNormUvTangent[VertexCount];
            int[] indexData = new int[IndexCount];

            GetVertices(vertexData,
                        indexData,
                        radius,
                        textureCoordinates,
                        ringCount,
                        segmentCount);

            VertexBuffer = GorgonVertexBuffer.Create<GorgonVertexPosNormUvTangent>(graphics,
                                                  new GorgonVertexBufferInfo(GorgonVertexPosNormUvTangent.SizeInBytes * vertexData.Length)
                                                  {
                                                      Name = "SphereVertexBuffer",
                                                      Usage = ResourceUsage.Immutable
                                                  },
                                                  vertexData);

            IndexBuffer = new GorgonIndexBuffer(graphics,
                                                new GorgonIndexBufferInfo(IndexCount)
                                                {
                                                    Name = "SphereIndexBuffer",
                                                    Usage = ResourceUsage.Immutable
                                                },
                                                indexData);

            UpdateAabb(vertexData);
        }
        #endregion
    }
}
