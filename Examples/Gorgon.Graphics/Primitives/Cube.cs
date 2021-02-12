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
// Created: Sunday, August 10, 2014 10:49:01 PM
// 
#endregion

using System;
using System.Numerics;
using System.Drawing;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Examples
{
    /// <summary>
    /// A mesh representing a solid cube.
    /// </summary>
    internal class Cube
        : MoveableMesh
    {
        #region Variables.
        // Initial orientation.
        private Matrix4x4 _orientation;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build the plane vertices.
        /// </summary>
        /// <param name="buffer">Buffer to populate.</param>
        /// <param name="vertexOffset">The offset into the buffer.</param>
        /// <param name="up">Up vector for orientation.</param>
        /// <param name="normal">The face normal.</param>
        /// <param name="size">The width and height of the plane.</param>
        /// <param name="textureCoordinates">The texture coordinates to apply to the plane.</param>
        /// <param name="columns">The number of columns to subdivide by.</param>
        /// <param name="rows">The number of rows to subdivide by.</param>
        private void GetVertices(Span<GorgonVertexPosNormUvTangent> buffer, int vertexOffset, Vector3 up, Vector3 normal, Vector3 size, RectangleF textureCoordinates, int columns, int rows)
        {
            float columnWidth = 1.0f / columns;
            float columnHeight = 1.0f / rows;
            Matrix4x4 rotation = Matrix4x4.Identity;

            var orientVector = Vector3.Cross(normal, up);
            var translate = Vector3.Multiply(normal, 0.5f);

            rotation.SetRow(0, new Vector4(orientVector, 0));
            rotation.SetRow(1, new Vector4(up, 0));
            rotation.SetRow(2, new Vector4(normal, 0));
            rotation.SetRow(3, new Vector4(translate, 1));

            var transformNormal = Vector3.Transform(normal, _orientation);
            transformNormal = Vector3.Normalize(transformNormal);

            for (int y = 0; y <= rows; ++y)
            {
                for (int x = 0; x <= columns; ++x)
                {
                    var vertexPos = new Vector3(((x * columnWidth) - 0.5f) * size.X,
                                                ((y * columnHeight) - 0.5f) * size.Y,
                                                0);

                    vertexPos = Vector3.Transform(vertexPos, rotation);
                    vertexPos = Vector3.Transform(vertexPos, _orientation);

                    buffer[vertexOffset++] = new GorgonVertexPosNormUvTangent
                    {
                        Position = new Vector4(vertexPos, 1.0f),
                        Normal = transformNormal,
                        UV = new Vector2((x * (textureCoordinates.Width / columns)) + textureCoordinates.X,
                                                                     (1.0f - (y * (textureCoordinates.Height / rows))) + textureCoordinates.Y)
                    };
                }
            }
        }

        /// <summary>
        /// Function to build the plane indices.
        /// </summary>
        /// <param name="buffer">Buffer to populate.</param>
        /// <param name="offset">The offset into the buffer.</param>
        /// <param name="vertexStart">The starting vertex.</param>
        /// <param name="columns">Number of columns for the plane.</param>
        /// <param name="rows">Number of rows for the plane.</param>
        private static void GetIndices(Span<int> buffer, int offset, int vertexStart, int columns, int rows)
        {
            int columnWrap = columns + 1;

            for (int row = 0; row < rows; ++row)
            {
                for (int column = 0; column < columns; ++column)
                {
                    buffer[offset++] = (column + (row * columnWrap)) + vertexStart;
                    buffer[offset++] = (column + ((row + 1) * columnWrap)) + vertexStart;
                    buffer[offset++] = ((column + 1) + (row * columnWrap)) + vertexStart;

                    buffer[offset++] = (column + ((row + 1) * columnWrap)) + vertexStart;
                    buffer[offset++] = ((column + 1) + ((row + 1) * columnWrap)) + vertexStart;
                    buffer[offset++] = ((column + 1) + (row * columnWrap)) + vertexStart;
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Cube" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface.</param>
        /// <param name="size">The width, height and depth of the cube.</param>
        /// <param name="textureCoordinates">The texture coordinates for the faces of the cube.</param>
        /// <param name="angle">The initial orientation of the cube, in degrees.</param>
        /// <param name="columnsPerFace">The number of columns per face.</param>
        /// <param name="rowsPerFace">The number of rows per face.</param>
        public Cube(GorgonGraphics graphics, Vector3 size, RectangleF textureCoordinates, Vector3 angle, int columnsPerFace = 1, int rowsPerFace = 1)
            : base(graphics)
        {
            PrimitiveType = PrimitiveType.TriangleList;
            int faceVertexCount = (columnsPerFace + 1) * (rowsPerFace + 1);
            int faceIndexCount = (columnsPerFace * rowsPerFace) * 6;
            VertexCount = faceVertexCount * 6;
            IndexCount = faceIndexCount * 6;
            TriangleCount = IndexCount / 3;

            var orientation = Quaternion.CreateFromYawPitchRoll(angle.Y.ToRadians(), angle.X.ToRadians(), angle.Z.ToRadians());
            _orientation = Matrix4x4.CreateFromQuaternion(orientation);

            var vertexData = new GorgonVertexPosNormUvTangent[VertexCount];
            int[] indexData = new int[IndexCount];

            // Front.
            GetVertices(vertexData, 0, Vector3.UnitY, -Vector3.UnitZ, size, textureCoordinates, columnsPerFace, rowsPerFace);
            // Bottom.
            GetVertices(vertexData, faceVertexCount, -Vector3.UnitZ, -Vector3.UnitY, size, textureCoordinates, columnsPerFace, rowsPerFace);
            // Back.
            GetVertices(vertexData, faceVertexCount * 2, Vector3.UnitY, Vector3.UnitZ, size, textureCoordinates, columnsPerFace, rowsPerFace);
            // Top.
            GetVertices(vertexData, faceVertexCount * 3, Vector3.UnitZ, Vector3.UnitY, size, textureCoordinates, columnsPerFace, rowsPerFace);
            // Left.
            GetVertices(vertexData, faceVertexCount * 4, Vector3.UnitY, -Vector3.UnitX, size, textureCoordinates, columnsPerFace, rowsPerFace);
            // Right
            GetVertices(vertexData, faceVertexCount * 5, Vector3.UnitY, Vector3.UnitX, size, textureCoordinates, columnsPerFace, rowsPerFace);

            GetIndices(indexData, 0, 0, columnsPerFace, rowsPerFace);
            GetIndices(indexData, faceIndexCount, faceVertexCount, columnsPerFace, rowsPerFace);
            GetIndices(indexData, faceIndexCount * 2, faceVertexCount * 2, columnsPerFace, rowsPerFace);
            GetIndices(indexData, faceIndexCount * 3, faceVertexCount * 3, columnsPerFace, rowsPerFace);
            GetIndices(indexData, faceIndexCount * 4, faceVertexCount * 4, columnsPerFace, rowsPerFace);
            GetIndices(indexData, faceIndexCount * 5, faceVertexCount * 5, columnsPerFace, rowsPerFace);

            CalculateTangents(vertexData, indexData);
                        
            VertexBuffer = GorgonVertexBuffer.Create<GorgonVertexPosNormUvTangent>(graphics,
                                                     new GorgonVertexBufferInfo("CubeVB")
                                                     {
                                                        Usage = ResourceUsage.Immutable
                                                     },
                                                     vertexData);


            IndexBuffer = new GorgonIndexBuffer(graphics,
                                                new GorgonIndexBufferInfo("CubeIB")
                                                {
                                                    Usage = ResourceUsage.Immutable,
                                                    Use16BitIndices = false,
                                                    IndexCount = IndexCount
                                                },
                                                indexData);

            UpdateAabb(vertexData);
        }
        #endregion
    }
}
