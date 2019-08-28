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

using System.Drawing;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Examples
{
    internal class Cube
        : MoveableMesh
    {
        #region Variables.
        // Initial orientation.
        private DX.Matrix _orientation;
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
        private void GetVertices(GorgonNativeBuffer<Vertex3D> buffer, int vertexOffset, DX.Vector3 up, DX.Vector3 normal, DX.Vector3 size, RectangleF textureCoordinates, int columns, int rows)
        {
            float columnWidth = 1.0f / columns;
            float columnHeight = 1.0f / rows;
            DX.Matrix rotation = DX.Matrix.Identity;

            DX.Vector3.Cross(ref normal, ref up, out DX.Vector3 orientVector);
            DX.Vector3.Multiply(ref normal, 0.5f, out DX.Vector3 translate);

            rotation.Row1 = (DX.Vector4)orientVector;
            rotation.Row2 = (DX.Vector4)up;
            rotation.Row3 = (DX.Vector4)normal;
            rotation.Row4 = new DX.Vector4(translate, 1);

            DX.Vector3.TransformCoordinate(ref normal, ref _orientation, out DX.Vector3 transformNormal);
            DX.Vector3.Normalize(ref transformNormal, out transformNormal);

            for (int y = 0; y <= rows; ++y)
            {
                for (int x = 0; x <= columns; ++x)
                {
                    var vertexPos = new DX.Vector3(((x * columnWidth) - 0.5f) * size.X,
                                                ((y * columnHeight) - 0.5f) * size.Y,
                                                0);

                    DX.Vector3.TransformCoordinate(ref vertexPos, ref rotation, out vertexPos);
                    DX.Vector3.TransformCoordinate(ref vertexPos, ref _orientation, out vertexPos);

                    buffer[vertexOffset++] = new Vertex3D
                    {
                        Position =
                                                     new DX.Vector4(vertexPos,
                                                                    1.0f),
                        Normal = transformNormal,
                        UV = new DX.Vector2((x * (textureCoordinates.Width / columns)) + textureCoordinates.X,
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
        private static void GetIndices(GorgonNativeBuffer<int> buffer, int offset, int vertexStart, int columns, int rows)
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
        public Cube(GorgonGraphics graphics, DX.Vector3 size, RectangleF textureCoordinates, DX.Vector3 angle, int columnsPerFace = 1, int rowsPerFace = 1)
            : base(graphics)
        {
            PrimitiveType = PrimitiveType.TriangleList;
            int faceVertexCount = (columnsPerFace + 1) * (rowsPerFace + 1);
            int faceIndexCount = (columnsPerFace * rowsPerFace) * 6;
            VertexCount = faceVertexCount * 6;
            IndexCount = faceIndexCount * 6;
            TriangleCount = IndexCount / 3;

            DX.Quaternion.RotationYawPitchRoll(angle.Y.ToRadians(), angle.X.ToRadians(), angle.Z.ToRadians(), out DX.Quaternion orientation);
            DX.Matrix.RotationQuaternion(ref orientation, out _orientation);

            using (var vertexData = new GorgonNativeBuffer<Vertex3D>(VertexCount))
            using (var indexData = new GorgonNativeBuffer<int>(IndexCount))
            {
                // Front.
                GetVertices(vertexData, 0, DX.Vector3.UnitY, -DX.Vector3.UnitZ, size, textureCoordinates, columnsPerFace, rowsPerFace);
                // Bottom.
                GetVertices(vertexData, faceVertexCount, -DX.Vector3.UnitZ, -DX.Vector3.UnitY, size, textureCoordinates, columnsPerFace, rowsPerFace);
                // Back.
                GetVertices(vertexData, faceVertexCount * 2, DX.Vector3.UnitY, DX.Vector3.UnitZ, size, textureCoordinates, columnsPerFace, rowsPerFace);
                // Top.
                GetVertices(vertexData, faceVertexCount * 3, DX.Vector3.UnitZ, DX.Vector3.UnitY, size, textureCoordinates, columnsPerFace, rowsPerFace);
                // Left.
                GetVertices(vertexData, faceVertexCount * 4, DX.Vector3.UnitY, -DX.Vector3.UnitX, size, textureCoordinates, columnsPerFace, rowsPerFace);
                // Right
                GetVertices(vertexData, faceVertexCount * 5, DX.Vector3.UnitY, DX.Vector3.UnitX, size, textureCoordinates, columnsPerFace, rowsPerFace);

                GetIndices(indexData, 0, 0, columnsPerFace, rowsPerFace);
                GetIndices(indexData, faceIndexCount, faceVertexCount, columnsPerFace, rowsPerFace);
                GetIndices(indexData, faceIndexCount * 2, faceVertexCount * 2, columnsPerFace, rowsPerFace);
                GetIndices(indexData, faceIndexCount * 3, faceVertexCount * 3, columnsPerFace, rowsPerFace);
                GetIndices(indexData, faceIndexCount * 4, faceVertexCount * 4, columnsPerFace, rowsPerFace);
                GetIndices(indexData, faceIndexCount * 5, faceVertexCount * 5, columnsPerFace, rowsPerFace);

                CalculateTangents(vertexData, indexData);

                VertexBuffer = new GorgonVertexBuffer(graphics,
                                                      new GorgonVertexBufferInfo("CubeVB")
                                                      {
                                                          Usage = ResourceUsage.Immutable,
                                                          SizeInBytes = vertexData.SizeInBytes
                                                      },
                                                      vertexData.Cast<byte>());


                IndexBuffer = new GorgonIndexBuffer(graphics,
                                                    new GorgonIndexBufferInfo("CubeIB")
                                                    {
                                                        Usage = ResourceUsage.Immutable,
                                                        Use16BitIndices = false,
                                                        IndexCount = IndexCount
                                                    },
                                                    indexData);
            }
        }
        #endregion
    }
}
