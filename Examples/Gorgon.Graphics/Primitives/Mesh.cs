
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, September 18, 2014 2:11:22 AM
// 


using System.Numerics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Data;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Examples;

/// <summary>
/// Base class for a mesh object
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Mesh"/> class
/// </remarks>
/// <param name="graphics">The graphics interface that owns this object.</param>
internal abstract class Mesh(GorgonGraphics graphics)
        : IDisposable
{

    // The axis aligned bounding box for the mesh.
    private GorgonBoundingBox _aabb;



    /// <summary>
    /// Property to return the material for this mesh.
    /// </summary>
    public MeshMaterial Material
    {
        get;
    } = new MeshMaterial();

    /// <summary>
    /// Property to return the graphics interface that owns this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics;

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
    /// Property to return the axis aligned bounding box for the mesh.
    /// </summary>
    public ref readonly GorgonBoundingBox Aabb => ref _aabb;

    /// <summary>
    /// Property to set or return whether writing to the depth buffer is enabled or not.
    /// </summary>
    public bool IsDepthWriteEnabled
    {
        get;
        set;
    } = true;



    /// <summary>
    /// Function to update the axis aligned bounding box for the mesh.
    /// </summary>
    /// <param name="vertexData">The vertices in the mesh.</param>
    protected void UpdateAabb(GorgonVertexPosNormUvTangent[] vertexData)
    {
        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        for (int i = 0; i < vertexData.Length; ++i)
        {
            ref readonly Vector4 pos = ref vertexData[i].Position;

            minX = minX.Min(pos.X);
            minY = minY.Min(pos.Y);
            minZ = minZ.Min(pos.Z);

            maxX = maxX.Max(pos.X);
            maxY = maxY.Max(pos.Y);
            maxZ = maxZ.Max(pos.Z);
        }

        _aabb = new GorgonBoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Function to calculate tangent information for bump mapping.
    /// </summary>
    /// <param name="vertexData">Buffer holding the vertices.</param>
    /// <param name="indexData">Buffer holding the indices.</param>
    protected void CalculateTangents(GorgonVertexPosNormUvTangent[] vertexData, int[] indexData)
    {
        var biTanData = new Vector3[VertexCount];
        var tanData = new Vector3[VertexCount];
        int indexOffset = 0;

        for (int i = 0; i < TriangleCount; ++i)
        {
            int index1 = indexData[indexOffset++];

            // If we hit a strip-restart index, then skip to the next index.
            if ((PrimitiveType == PrimitiveType.TriangleStrip)
                && (index1 < 0))
            {
                index1 = indexData[indexOffset++];
            }

            int index2 = indexData[indexOffset++];
            int index3 = indexData[indexOffset++];

            GorgonVertexPosNormUvTangent vertex1 = vertexData[index1];
            GorgonVertexPosNormUvTangent vertex2 = vertexData[index2];
            GorgonVertexPosNormUvTangent vertex3 = vertexData[index3];

            var deltaPos1 = Vector4.Subtract(vertex2.Position, vertex1.Position);
            var deltaPos2 = Vector4.Subtract(vertex3.Position, vertex1.Position);

            var deltaUV1 = Vector2.Subtract(vertex2.UV, vertex1.UV);
            var deltaUV2 = Vector2.Subtract(vertex3.UV, vertex1.UV);

            float denom = ((deltaUV1.X * deltaUV2.Y) - (deltaUV1.Y * deltaUV2.X));
            float r = 0.0f;

            if (!denom.EqualsEpsilon(0))
            {
                r = 1.0f / denom;
            }

            var tangent = new Vector3(((deltaUV2.Y * deltaPos1.X) - (deltaUV1.Y * deltaPos2.X)) * r,
                                      ((deltaUV2.Y * deltaPos1.Y) - (deltaUV1.Y * deltaPos2.Y)) * r,
                                      ((deltaUV2.Y * deltaPos1.Z) - (deltaUV1.Y * deltaPos2.Z)) * r);

            var biTangent = new Vector3(((deltaUV1.X * deltaPos2.X) - (deltaUV2.X * deltaPos1.X)) * r,
                                        ((deltaUV1.X * deltaPos2.Y) - (deltaUV2.X * deltaPos1.Y)) * r,
                                        ((deltaUV1.X * deltaPos2.Z) - (deltaUV2.X * deltaPos1.Z)) * r);

            tanData[index1] = Vector3.Add(tanData[index1], tangent);
            tanData[index2] = Vector3.Add(tanData[index2], tangent);
            tanData[index3] = Vector3.Add(tanData[index3], tangent);

            biTanData[index1] = Vector3.Add(biTanData[index1], biTangent);
            biTanData[index2] = Vector3.Add(biTanData[index2], biTangent);
            biTanData[index3] = Vector3.Add(biTanData[index3], biTangent);
        }

        for (int i = 0; i < VertexCount; ++i)
        {
            GorgonVertexPosNormUvTangent vertex = vertexData[i];

            float dot = Vector3.Dot(vertex.Normal, tanData[i]);
            var tangent = Vector3.Multiply(vertex.Normal, dot);
            tangent = Vector3.Subtract(tanData[i], tangent);
            tangent = Vector3.Normalize(tangent);

            var cross = Vector3.Cross(vertex.Normal, tanData[i]);
            dot = Vector3.Dot(cross, biTanData[i]);

            vertexData[i] = new GorgonVertexPosNormUvTangent
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                UV = vertex.UV,
                Tangent = new Vector4(tangent, dot < 0.0f ? -1.0f : 1.0f)
            };
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
    }
}
