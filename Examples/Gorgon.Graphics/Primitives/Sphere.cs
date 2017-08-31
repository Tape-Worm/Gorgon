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
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// A sphere object.
    /// </summary>
    internal class Sphere
        : MoveableMesh
    {
        #region Variables.
        // Initial orientation.
        private DX.Matrix _orientation;
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
        private unsafe void GetVertices(Vertex3D* vertexData,
                                        int* indexData,
                                        float radius,
                                        RectangleF textureCoordinates,
                                        int ringCount,
                                        int segmentCount)
        {
            int index = 0; // Current index.
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
                    DX.Vector2 textureDelta = new DX.Vector2(1.0f - segment / (float)segmentCount, ring / (float)ringCount);
                    float segmentAngle = deltaSegAngle * segment;

                    DX.Vector3 position = new DX.Vector3(radius * segmentAngle.Sin(), radiusY, radius * segmentAngle.Cos());


                    DX.Vector3.Multiply(ref position, 2.0f, out DX.Vector3 normal);
                    DX.Vector3.TransformCoordinate(ref position, ref _orientation, out position);
                    DX.Vector3.TransformCoordinate(ref normal, ref _orientation, out normal);
                    normal.Normalize();

                    // Create the vertex.
                    textureDelta.X *= textureCoordinates.Width;
                    textureDelta.Y *= textureCoordinates.Height;
                    textureDelta.X += textureCoordinates.X;
                    textureDelta.Y += textureCoordinates.Y;

                    *(vertexData++) = new Vertex3D
                                      {
                                          Position = new DX.Vector4(position, 1.0f),
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
        public Sphere(GorgonGraphics graphics, float radius, RectangleF textureCoordinates, DX.Vector3 angle, int ringCount = 8, int segmentCount = 16)
            : base(graphics)
        {

            // Calculate number of vertices and indices required for our sphere.
            PrimitiveType = D3D.PrimitiveTopology.TriangleList;
            VertexCount = (ringCount + 1) * (segmentCount + 1);
            IndexCount = 6 * ringCount * (segmentCount + 1);
            TriangleCount = IndexCount / 3;

            DX.Quaternion.RotationYawPitchRoll(angle.Y.ToRadians(), angle.X.ToRadians(), angle.Z.ToRadians(), out DX.Quaternion orientation);
            DX.Matrix.RotationQuaternion(ref orientation, out _orientation);

            unsafe
            {
                using (IGorgonPointer vertexData = new GorgonPointerTyped<Vertex3D>(VertexCount),
                                      indexData = new GorgonPointerTyped<int>(IndexCount))
                {
                    GetVertices((Vertex3D*)vertexData.Address,
                                (int*)indexData.Address,
                                radius,
                                textureCoordinates,
                                ringCount,
                                segmentCount);

                    VertexBuffer = new GorgonVertexBuffer("SphereVertexBuffer",
                                                          graphics,
                                                          new GorgonVertexBufferInfo
                                                          {
                                                              Usage = D3D11.ResourceUsage.Immutable,
                                                              SizeInBytes = (int)vertexData.Size
                                                          },
                                                          vertexData);

                    IndexBuffer = new GorgonIndexBuffer("SphereIndexBuffer",
                                                        graphics,
                                                        new GorgonIndexBufferInfo
                                                        {
                                                            Usage = D3D11.ResourceUsage.Immutable,
                                                            Use16BitIndices = false,
                                                            IndexCount = IndexCount
                                                        },
                                                        indexData);
                }
            }
        }
        #endregion
    }
}
