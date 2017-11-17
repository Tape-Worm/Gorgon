﻿#region MIT.
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
// Created: Saturday, August 9, 2014 10:47:56 AM
// 
#endregion

using System.Drawing;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics.Example
{
    internal class Plane
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
        /// <param name="size">The width and height of the plane.</param>
        /// <param name="textureCoordinates">The texture coordinates to apply to the plane.</param>
        /// <param name="columns">The number of columns to subdivide by.</param>
        /// <param name="rows">The number of rows to subdivide by.</param>
        private unsafe void GetVertices(Vertex3D*buffer, DX.Vector2 size, RectangleF textureCoordinates, int columns, int rows)
        {
            float columnWidth = 1.0f / columns;
            float columnHeight = 1.0f / rows;

            DX.Vector3 vertexNormal = -DX.Vector3.UnitZ;
            DX.Vector3.TransformNormal(ref vertexNormal, ref _orientation, out vertexNormal);

            for (int y = 0; y <= rows; ++y)
            {
                for (int x = 0; x <= columns; ++x)
                {
                    DX.Vector3 vertexPos = new DX.Vector3(((x * columnWidth) - 0.5f) * size.X,
                                                   ((y * columnHeight) - 0.5f) * size.Y,
                                                   0);

                    DX.Vector3.TransformCoordinate(ref vertexPos, ref _orientation, out vertexPos);

                    *(buffer++) = new Vertex3D
                                  {
                                      Position =
                                          new DX.Vector4(vertexPos,
                                                         1.0f),
                                      Normal = vertexNormal,
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
        /// <param name="columns">Number of columns for the plane.</param>
        /// <param name="rows">Number of rows for the plane.</param>
        private static unsafe void GetIndices(int*buffer, int columns, int rows)
        {
            int columnWrap = columns + 1;

            for (int row = 0; row < rows; ++row)
            {
                for (int column = 0; column < columns; ++column)
                {
                    *(buffer++) = column + (row * columnWrap);
                    *(buffer++) = column + ((row + 1) * columnWrap);
                    *(buffer++) = (column + 1) + (row * columnWrap);

                    *(buffer++) = column + ((row + 1) * columnWrap);
                    *(buffer++) = (column + 1) + (row * columnWrap);
                    *(buffer++) = (column + 1) + ((row + 1) * columnWrap);
                }

                if (row < rows - 1)
                {
                    *(buffer++) = unchecked((int)0xffffffff);
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Plane" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use.</param>
        /// <param name="size">The width and height of the plane.</param>
        /// <param name="textureCoordinates">The texture coordinates to apply to the plane.</param>
        /// <param name="angle">The initial orientation, in degrees.</param>
        /// <param name="columns">The number of columns to subdivide by.</param>
        /// <param name="rows">The number of rows to subdivide by.</param>
        public Plane(GorgonGraphics graphics, DX.Vector2 size, RectangleF textureCoordinates, DX.Vector3 angle, int columns = 1, int rows = 1)
            : base(graphics)
        {
            PrimitiveType = PrimitiveType.TriangleStrip;
            VertexCount = (columns + 1) * (rows + 1);
            IndexCount = ((columns * rows) * 6) + (rows - 1);
            TriangleCount = (IndexCount - (rows - 1)) / 3;

            DX.Quaternion.RotationYawPitchRoll(angle.Y.ToRadians(), angle.X.ToRadians(), angle.Z.ToRadians(), out DX.Quaternion orientation);
            DX.Matrix.RotationQuaternion(ref orientation, out _orientation);

            unsafe
            {
                using (IGorgonPointer vertexData = new GorgonPointerTyped<Vertex3D>(VertexCount))
                    using (IGorgonPointer indexData = new GorgonPointerTyped<int>(IndexCount))
                    {
                        GetVertices((Vertex3D*)vertexData.Address, size, textureCoordinates, columns, rows);
                        GetIndices((int*)indexData.Address, columns, rows);

                        CalculateTangents((Vertex3D*)vertexData.Address, (int*)indexData.Address);

                        VertexBuffer = new GorgonVertexBuffer("PlaneVB",
                                                              graphics,
                                                              new GorgonVertexBufferInfo
                                                              {
                                                                  Usage = ResourceUsage.Immutable,
                                                                  SizeInBytes = (int)vertexData.Size
                                                              },
                                                              vertexData);

                        IndexBuffer = new GorgonIndexBuffer("PlaneIB",
                                                            graphics,
                                                            new GorgonIndexBufferInfo
                                                            {
                                                                Usage = ResourceUsage.Immutable,
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
