#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 4, 2017 10:22:08 AM
// 
#endregion

using System;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Object representing a 3D dimensional cube.
    /// </summary>
    internal class Cube
        : IDisposable
    {
        #region Variables.
        // The matrix that defines our rotation.
        private DX.Matrix _rotation = DX.Matrix.Identity;
        // The matrix that defines our translation.
        private DX.Matrix _translation = DX.Matrix.Identity;
        // The world matrix to send to the vertex shader for transformation.
        // This is the combination of the rotation and translation matrix.
        private DX.Matrix _world = DX.Matrix.Identity;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the vertex buffer that holds the cube vertex data.
        /// </summary>
        public GorgonVertexBufferBindings VertexBuffer
        {
            get;
        }

        /// <summary>
        /// Property to return the index buffer that holds the cube index data.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
        }

        /// <summary>
        /// Property to set or return the texture image for the cube.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the world matrix for this object.
        /// </summary>
        public ref DX.Matrix WorldMatrix
        {
            get
            {
                DX.Matrix.Multiply(ref _rotation, ref _translation, out _world);
                return ref _world;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to rotate the cube.
        /// </summary>
        /// <param name="xAngle">X axis angle in degrees.</param>
        /// <param name="yAngle">Y axis angle in degrees.</param>
        /// <param name="zAngle">Z axis angle in degrees.</param>
        public void RotateXYZ(float xAngle, float yAngle, float zAngle)
        {
            // Quaternion for rotation.

            // Convert degrees to radians.
            var rotRads = new DX.Vector3(xAngle.ToRadians(), yAngle.ToRadians(), zAngle.ToRadians());

            // Rotate and build a new rotation matrix.
            DX.Quaternion.RotationYawPitchRoll(rotRads.Y, rotRads.X, rotRads.Z, out DX.Quaternion quatRotation);
            DX.Matrix.RotationQuaternion(ref quatRotation, out _rotation);
        }

        /// <summary>
        /// Function to translate the cube.
        /// </summary>
        /// <param name="x">X axis translation.</param>
        /// <param name="y">Y axis translation.</param>
        /// <param name="z">Z axis translation.</param>
        public void Translate(float x, float y, float z) => DX.Matrix.Translation(x, y, z, out _translation);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            VertexBuffer[0].VertexBuffer.Dispose();
            IndexBuffer?.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Cube"/> class.
        /// </summary>
        /// <param name="graphics">The graphics object used to create the buffers needed by this object.</param>
        /// <param name="inputLayout">The input layout describing how a vertex is laid out.</param>
        public Cube(GorgonGraphics graphics, GorgonInputLayout inputLayout)
        {
            CubeVertex[] vertices =
            {
                new CubeVertex(new DX.Vector3(-0.5f, 0.5f, -0.5f), new DX.Vector3(0, 0, 0)),
                new CubeVertex(new DX.Vector3(0.5f, 0.5f, -0.5f), new DX.Vector3(1.0f, 1.0f, 0)),
                new CubeVertex(new DX.Vector3(0.5f, -0.5f, -0.5f), new DX.Vector3(0.0f, 1.0f, 0)),
                new CubeVertex(new DX.Vector3(-0.5f, -0.5f, -0.5f), new DX.Vector3(1.0f, 0.0f, 0)),

                new CubeVertex(new DX.Vector3(-0.5f, 0.5f, 0.5f), new DX.Vector3(0, 0, 0)),
                new CubeVertex(new DX.Vector3(0.5f, 0.5f, 0.5f), new DX.Vector3(1.0f, 1.0f, 0)),
                new CubeVertex(new DX.Vector3(0.5f, -0.5f, 0.5f), new DX.Vector3(0.0f, 1.0f, 0)),
                new CubeVertex(new DX.Vector3(-0.5f, -0.5f, 0.5f), new DX.Vector3(1.0f, 0.0f, 0)),
            };

            ushort[] indices =
            {
                // Front face.
                0, 1, 2,
                2, 3, 0,
                // Back face.
                5, 4, 6,
                4, 7, 6,
                // Left face.
                4, 0, 3,
                3, 7, 4,
                // Right face.
                1, 5, 6,
                6, 2, 1,
                // Top face
                4, 5, 1,
                1, 0, 4,
                // Bottom face
                2, 6, 7,
                7, 3, 2
            };

            // Create our index buffer and vertex buffer and populate with our cube data.
            using (var indexPtr = GorgonNativeBuffer<ushort>.Pin(indices))
            using (var vertexPtr = GorgonNativeBuffer<CubeVertex>.Pin(vertices))
            {
                IndexBuffer = new GorgonIndexBuffer(graphics,
                                                    new GorgonIndexBufferInfo("Volume Index Buffer")
                                                    {
                                                        Usage = ResourceUsage.Immutable,
                                                        IndexCount = indices.Length,
                                                        Use16BitIndices = true
                                                    },
                                                    indexPtr);

                VertexBuffer = new GorgonVertexBufferBindings(inputLayout)
                {
                    [0] = GorgonVertexBufferBinding.CreateVertexBuffer(graphics,
                                                                                      vertices.Length,
                                                                                      ResourceUsage.Immutable,
                                                                                      initialData: vertexPtr,
                                                                                      bufferName: "Volume Vertex Buffer")
                };
            }
        }
        #endregion
    }
}
