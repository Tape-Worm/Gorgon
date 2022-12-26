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
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// A sphere object.
/// </summary>
internal class Sphere
    : Model
{
    #region Properties.
    /// <summary>
    /// Property to return the radius of the sphere.
    /// </summary>
    public float Radius
    {
        get;
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="Sphere" /> class.
    /// </summary>
    /// <param name="graphics">The graphics interface used to create the buffers for this object.</param>
    /// <param name="inputLayout">The input layout for the vertices in this mesh.</param>
    /// <param name="radius">Radius of the sphere</param>
    /// <param name="textureOffset">Offset of the texture.</param>
    /// <param name="textureScale">Scale of the texture.</param>
    /// <param name="ringCount">Number of rings in the sphere.</param>
    /// <param name="segmentCount">Number of segments in the sphere.</param>
    public Sphere(GorgonGraphics graphics,
                  GorgonInputLayout inputLayout,
                  float radius,
                  Vector2 textureOffset,
                  DX.Size2F textureScale,
                  int ringCount = 8,
                  int segmentCount = 16)
        : base(inputLayout)
    {
        ushort index = 0; // Current index.
        int vertexIndex = 0; // Current vertex index.
        int indexIndex = 0; // Current index array index.

        float deltaRingAngle = ((float)System.Math.PI) / ringCount;
        float deltaSegAngle = (((float)System.Math.PI) * 2.0f) / segmentCount;

        // Calculate number of vertices and indices required for our sphere.
        int vertexCount = (ringCount + 1) * (segmentCount + 1);
        int indexCount = 6 * ringCount * (segmentCount + 1);

        Vertices = new GorgonVertexPosUv[vertexCount];
        Indices = new ushort[indexCount];

        Radius = radius;

        // Build our sphere.
        for (int ring = 0; ring <= ringCount; ring++)
        {
            float angle = deltaRingAngle * ring;
            float ringSin = angle.Sin();
            var position = new Vector3(0, angle.Cos() * radius, 0);

            for (int segment = 0; segment <= segmentCount; segment++)
            {
                var textureDelta = new Vector2(1.0f - (segment / (float)segmentCount), 1.0f - (ring / (float)ringCount));
                float segmentAngle = deltaSegAngle * segment;

                position.X = ringSin * segmentAngle.Sin() * radius;
                position.Z = ringSin * segmentAngle.Cos() * radius;

                // Create the vertex.
                textureDelta.X *= textureScale.Width;
                textureDelta.Y *= textureScale.Height;
                textureDelta.X += textureOffset.X;
                textureDelta.Y += textureOffset.Y;

                Vertices[vertexIndex++] = new GorgonVertexPosUv(
                                                            position,
                                                            textureDelta
                                                           );

                // Add the indices and skip the last ring.
                if (ring == ringCount)
                {
                    continue;
                }

                Indices[indexIndex++] = (ushort)(index + segmentCount + 1);
                Indices[indexIndex++] = index;
                Indices[indexIndex++] = (ushort)(index + segmentCount);
                Indices[indexIndex++] = (ushort)(index + segmentCount + 1);
                Indices[indexIndex++] = (ushort)(index + 1);
                Indices[indexIndex++] = index;
                index++;
            }
        }

        // Copy the above vertex/index data into a vertex and index buffer so we can render our sphere.
        VertexBufferBindings[0] = GorgonVertexBufferBinding.CreateVertexBuffer<GorgonVertexPosUv>(graphics,
                                                                                new GorgonVertexBufferInfo(Vertices.Length * GorgonVertexPosUv.SizeInBytes)
                                                                                {
                                                                                    Name = "Sphere Vertex Buffer",                                                                                        
                                                                                    Usage = ResourceUsage.Immutable
                                                                                },
                                                                                Vertices);
        IndexBuffer = new GorgonIndexBuffer(graphics,
                                            new GorgonIndexBufferInfo(Indices.Length)
                                            {
                                                Name = "Sphere Index Buffer",
                                                Usage = ResourceUsage.Immutable,
                                                Use16BitIndices = true
                                            },
                                            Indices);
    }
    #endregion
}
