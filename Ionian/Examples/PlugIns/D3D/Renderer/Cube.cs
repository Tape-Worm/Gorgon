#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Drawing = System.Drawing;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// Object representing a 3D dimensional cube.
    /// </summary>
    public class Cube
        : IDisposable
    {
        #region Variables.
        private D3DObjects _d3d = null;                                     // Our device object.
        private SlimDX.Direct3D9.VertexDeclaration _vertexType = null;      // Vertex type.
        private SlimDX.Direct3D9.VertexBuffer _vertexBuffer = null;         // Vertex buffer.
        private SlimDX.Direct3D9.IndexBuffer _indexBuffer = null;           // Index buffer.
        private SlimDX.Direct3D9.Material _material;                        // Cube material.
        private Image _cubeTexture = null;                                  // Texture to apply to the cube.        
        private D3DRenderer.Vertex[] _vertices = null;                      // Vertices for the cube.
        private short[] _indices = null;                                    // Indices for the cube.
        private bool _disposed = false;                                     // Flag to indicate that an object is disposed.
        private int _vertexByteSize = 0;                                    // Vertex size in bytes.
        private int _indexByteSize = 0;                                     // Index size in bytes.
        private SlimDX.Matrix _rotation = SlimDX.Matrix.Identity;           // Rotation matrix.
        private SlimDX.Matrix _translation = SlimDX.Matrix.Identity;        // Translation matrix.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the texture image for the cube.
        /// </summary>
        public Image Texture
        {
            get
            {
                return _cubeTexture;
            }
            set
            {
                _cubeTexture = value;
            }
        }

        /// <summary>
        /// Property to set or return the diffuse color of the cube.
        /// </summary>
        public Drawing.Color Diffuse
        {
            get
            {
                return _material.Diffuse.ToColor();
            }
            set
            {
                _material.Diffuse = new SlimDX.Color4(value);
            }
        }

        /// <summary>
        /// Property to set or return the specular color of the cube.
        /// </summary>
        public Drawing.Color Specular
        {
            get
            {
                return _material.Specular.ToColor();
            }
            set
            {
                _material.Specular = new SlimDX.Color4(value);
            }
        }

        /// <summary>
        /// Property to set or return the ambient color of the cube.
        /// </summary>
        public Drawing.Color Ambient
        {
            get
            {
                return _material.Ambient.ToColor();
            }
            set
            {
                _material.Ambient = new SlimDX.Color4(value);
            }
        }

        /// <summary>
        /// Property to set or return the specular power for the cube material.
        /// </summary>
        public float SpecularPower
        {
            get
            {
                return _material.Power;
            }
            set
            {
                _material.Power = value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build the model.
        /// </summary>
        private void BuildModel()
        {
            SlimDX.DataStream lockData = null;                  // Locked data.            

            #region Cube Vertices and Faces.
            // Front.
            _vertices[0].Position = new SlimDX.Vector3(-0.5f, 0.5f, -0.5f);
            _vertices[0].Normal = new SlimDX.Vector3(0, 0, -1.0f);
            _vertices[0].TexUV = new SlimDX.Vector2(0, 0);

            _vertices[1].Position = new SlimDX.Vector3(0.5f, -0.5f, -0.5f);
            _vertices[1].Normal = new SlimDX.Vector3(0, 0, -1.0f);
            _vertices[1].TexUV = new SlimDX.Vector2(1.0f, 1.0f);

            _vertices[2].Position = new SlimDX.Vector3(-0.5f, -0.5f, -0.5f);
            _vertices[2].Normal = new SlimDX.Vector3(0, 0, -1.0f);
            _vertices[2].TexUV = new SlimDX.Vector2(0.0f, 1.0f);

            _vertices[3].Position = new SlimDX.Vector3(0.5f, 0.5f, -0.5f);
            _vertices[3].Normal = new SlimDX.Vector3(0, 0, -1.0f);
            _vertices[3].TexUV = new SlimDX.Vector2(1.0f, 0.0f);

            // Right.
            _vertices[4].Position = new SlimDX.Vector3(0.5f, 0.5f, -0.5f);
            _vertices[4].Normal = new SlimDX.Vector3(1.0f, 0, 0.0f);
            _vertices[4].TexUV = new SlimDX.Vector2(0, 0);

            _vertices[5].Position = new SlimDX.Vector3(0.5f, -0.5f, 0.5f);
            _vertices[5].Normal = new SlimDX.Vector3(1.0f, 0, 0.0f);
            _vertices[5].TexUV = new SlimDX.Vector2(1.0f, 1.0f);

            _vertices[6].Position = new SlimDX.Vector3(0.5f, -0.5f, -0.5f);
            _vertices[6].Normal = new SlimDX.Vector3(1.0f, 0, 0.0f);
            _vertices[6].TexUV = new SlimDX.Vector2(0.0f, 1.0f);

            _vertices[7].Position = new SlimDX.Vector3(0.5f, 0.5f, 0.5f);
            _vertices[7].Normal = new SlimDX.Vector3(1.0f, 0, 0.0f);
            _vertices[7].TexUV = new SlimDX.Vector2(1.0f, 0.0f);

            // Back.
            _vertices[8].Position = new SlimDX.Vector3(0.5f, 0.5f, 0.5f);
            _vertices[8].Normal = new SlimDX.Vector3(0.0f, 0, 1.0f);
            _vertices[8].TexUV = new SlimDX.Vector2(0, 0);

            _vertices[9].Position = new SlimDX.Vector3(-0.5f, -0.5f, 0.5f);
            _vertices[9].Normal = new SlimDX.Vector3(0.0f, 0, 1.0f);
            _vertices[9].TexUV = new SlimDX.Vector2(1.0f, 1.0f);

            _vertices[10].Position = new SlimDX.Vector3(0.5f, -0.5f, 0.5f);
            _vertices[10].Normal = new SlimDX.Vector3(0.0f, 0, 1.0f);
            _vertices[10].TexUV = new SlimDX.Vector2(0.0f, 1.0f);

            _vertices[11].Position = new SlimDX.Vector3(-0.5f, 0.5f, 0.5f);
            _vertices[11].Normal = new SlimDX.Vector3(0.0f, 0, 1.0f);
            _vertices[11].TexUV = new SlimDX.Vector2(1.0f, 0.0f);


            // Left.
            _vertices[12].Position = new SlimDX.Vector3(-0.5f, 0.5f, 0.5f);
            _vertices[12].Normal = new SlimDX.Vector3(-1.0f, 0, 0.0f);
            _vertices[12].TexUV = new SlimDX.Vector2(0, 0);

            _vertices[13].Position = new SlimDX.Vector3(-0.5f, -0.5f, -0.5f);
            _vertices[13].Normal = new SlimDX.Vector3(-1.0f, 0, 0.0f);
            _vertices[13].TexUV = new SlimDX.Vector2(1.0f, 1.0f);

            _vertices[14].Position = new SlimDX.Vector3(-0.5f, -0.5f, 0.5f);
            _vertices[14].Normal = new SlimDX.Vector3(-1.0f, 0, 0.0f);
            _vertices[14].TexUV = new SlimDX.Vector2(0.0f, 1.0f);

            _vertices[15].Position = new SlimDX.Vector3(-0.5f, 0.5f, -0.5f);
            _vertices[15].Normal = new SlimDX.Vector3(-1.0f, 0, 0.0f);
            _vertices[15].TexUV = new SlimDX.Vector2(1.0f, 0.0f);

            // Top.
            _vertices[16].Position = new SlimDX.Vector3(-0.5f, 0.5f, 0.5f);
            _vertices[16].Normal = new SlimDX.Vector3(0.0f, 1.0f, 0.0f);
            _vertices[16].TexUV = new SlimDX.Vector2(0, 0);

            _vertices[17].Position = new SlimDX.Vector3(0.5f, 0.5f, -0.5f);
            _vertices[17].Normal = new SlimDX.Vector3(0.0f, 1.0f, 0.0f);
            _vertices[17].TexUV = new SlimDX.Vector2(1.0f, 1.0f);

            _vertices[18].Position = new SlimDX.Vector3(-0.5f, 0.5f, -0.5f);
            _vertices[18].Normal = new SlimDX.Vector3(0.0f, 1.0f, 0.0f);
            _vertices[18].TexUV = new SlimDX.Vector2(0.0f, 1.0f);

            _vertices[19].Position = new SlimDX.Vector3(0.5f, 0.5f, 0.5f);
            _vertices[19].Normal = new SlimDX.Vector3(0.0f, 1.0f, 0.0f);
            _vertices[19].TexUV = new SlimDX.Vector2(1.0f, 0.0f);

            // Top.
            _vertices[20].Position = new SlimDX.Vector3(-0.5f, -0.5f, -0.5f);
            _vertices[20].Normal = new SlimDX.Vector3(0.0f, -1.0f, 0.0f);
            _vertices[20].TexUV = new SlimDX.Vector2(0, 0);

            _vertices[21].Position = new SlimDX.Vector3(0.5f, -0.5f, 0.5f);
            _vertices[21].Normal = new SlimDX.Vector3(0.0f, -1.0f, 0.0f);
            _vertices[21].TexUV = new SlimDX.Vector2(1.0f, 1.0f);

            _vertices[22].Position = new SlimDX.Vector3(-0.5f, -0.5f, 0.5f);
            _vertices[22].Normal = new SlimDX.Vector3(0.0f, -1.0f, 0.0f);
            _vertices[22].TexUV = new SlimDX.Vector2(0.0f, 1.0f);

            _vertices[23].Position = new SlimDX.Vector3(0.5f, -0.5f, -0.5f);
            _vertices[23].Normal = new SlimDX.Vector3(0.0f, -1.0f, 0.0f);
            _vertices[23].TexUV = new SlimDX.Vector2(1.0f, 0.0f);

            _indices[0] = 8;
            _indices[1] = 9;
            _indices[2] = 10;
            _indices[3] = 8;
            _indices[4] = 11;
            _indices[5] = 9;

            _indices[6] = 12;
            _indices[7] = 13;
            _indices[8] = 14;
            _indices[9] = 12;
            _indices[10] = 15;
            _indices[11] = 13;

            _indices[12] = 4;
            _indices[13] = 5;
            _indices[14] = 6;
            _indices[15] = 4;
            _indices[16] = 7;
            _indices[17] = 5;

            _indices[18] = 16;
            _indices[19] = 17;
            _indices[20] = 18;
            _indices[21] = 16;
            _indices[22] = 19;
            _indices[23] = 17;

            _indices[24] = 20;
            _indices[25] = 21;
            _indices[26] = 22;
            _indices[27] = 20;
            _indices[28] = 23;
            _indices[29] = 21;

            _indices[30] = 0;
            _indices[31] = 1;
            _indices[32] = 2;
            _indices[33] = 0;
            _indices[34] = 3;
            _indices[35] = 1;
            #endregion

            // Copy our data to the vertex and index buffers.
            try
            {
                lockData = _vertexBuffer.Lock(0, _vertexByteSize * _vertices.Length, SlimDX.Direct3D9.LockFlags.None);
                lockData.WriteRange<D3DRenderer.Vertex>(_vertices);
            }
            finally
            {
                if (lockData != null)
                {
                    _vertexBuffer.Unlock();
                    lockData.Dispose();
                    lockData = null;
                }
            }

            try
            {
                lockData = _indexBuffer.Lock(0, _indexByteSize * _indices.Length, SlimDX.Direct3D9.LockFlags.None);
                lockData.WriteRange<short>(_indices);
            }
            finally
            {
                if (lockData != null)
                {
                    _indexBuffer.Unlock();
                    lockData.Dispose();
                    lockData = null;
                }
            }
        }

        /// <summary>
        /// Function to rotate the cube.
        /// </summary>
        /// <param name="xAngle">X axis angle in degrees.</param>
        /// <param name="yAngle">Y axis angle in degrees.</param>
        /// <param name="zAngle">Z axis angle in degrees.</param>
        public void RotateXYZ(float xAngle, float yAngle, float zAngle)
        {
            SlimDX.Matrix rx = SlimDX.Matrix.RotationX(MathUtility.Radians(xAngle));         // X axis rotation.
            SlimDX.Matrix ry = SlimDX.Matrix.RotationX(MathUtility.Radians(yAngle));         // Y axis rotation.
            SlimDX.Matrix rz = SlimDX.Matrix.RotationZ(MathUtility.Radians(zAngle));         // Z axis rotation.

            _rotation = SlimDX.Matrix.Multiply(ry, rx);
            _rotation = SlimDX.Matrix.Multiply(rz, _rotation);
        }

        /// <summary>
        /// Function to translate the cube.
        /// </summary>
        /// <param name="x">X axis translation.</param>
        /// <param name="y">Y axis translation.</param>
        /// <param name="z">Z axis translation.</param>
        public void Translate(float x, float y, float z)
        {
            _translation = SlimDX.Matrix.Translation(x, y, z);
        }

        /// <summary>
        /// Function to draw the cube.
        /// </summary>
        public void Draw()
        {
            SlimDX.Matrix transform = SlimDX.Matrix.Multiply(_rotation, _translation);      // Transformation matrix..

            _d3d.Device.VertexDeclaration = _vertexType;
            _d3d.Device.Indices = _indexBuffer;
            _d3d.Device.SetStreamSource(0, _vertexBuffer, 0, _vertexByteSize);

            _d3d.Device.Material = _material;
            _d3d.SetImage(0, _cubeTexture);

            _d3d.Device.SetTransform(SlimDX.Direct3D9.TransformState.World, transform);

            _d3d.Device.DrawIndexedPrimitives(SlimDX.Direct3D9.PrimitiveType.TriangleList, 0, 0, _vertices.Length, 0, _indices.Length / 3);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Cube"/> class.
        /// </summary>
        /// <param name="d3d">Direct 3D object interface.</param>
        public Cube(D3DObjects d3d)
        {
            SlimDX.Direct3D9.VertexElement[] vertexElements;    // The pieces of our vertex type.

            _d3d = d3d;
            _vertices = new D3DRenderer.Vertex[24];
            _indices = new short[36];

            // Build our vertex type.
            vertexElements = new SlimDX.Direct3D9.VertexElement[4];
            vertexElements[0] = new SlimDX.Direct3D9.VertexElement(0, 0, SlimDX.Direct3D9.DeclarationType.Float3,
                    SlimDX.Direct3D9.DeclarationMethod.Default, SlimDX.Direct3D9.DeclarationUsage.Position, 0);

            vertexElements[1] = new SlimDX.Direct3D9.VertexElement(0, 12, SlimDX.Direct3D9.DeclarationType.Float3,
                    SlimDX.Direct3D9.DeclarationMethod.Default, SlimDX.Direct3D9.DeclarationUsage.Normal, 0);

            vertexElements[2] = new SlimDX.Direct3D9.VertexElement(0, 24, SlimDX.Direct3D9.DeclarationType.Float2,
                    SlimDX.Direct3D9.DeclarationMethod.Default, SlimDX.Direct3D9.DeclarationUsage.TextureCoordinate, 0);
            // Don't forget to add this.
            vertexElements[3] = SlimDX.Direct3D9.VertexElement.VertexDeclarationEnd;

            _vertexType = new SlimDX.Direct3D9.VertexDeclaration(d3d.Device, vertexElements);

            // Get data size.
            _vertexByteSize = Marshal.SizeOf(typeof(D3DRenderer.Vertex));
            _indexByteSize = Marshal.SizeOf(typeof(short));

            // Create the buffers that we need.
            _vertexBuffer = new SlimDX.Direct3D9.VertexBuffer(d3d.Device, _vertexByteSize * _vertices.Length, SlimDX.Direct3D9.Usage.WriteOnly,
                                SlimDX.Direct3D9.VertexFormat.None, SlimDX.Direct3D9.Pool.Managed);

            _indexBuffer = new SlimDX.Direct3D9.IndexBuffer(d3d.Device, _indexByteSize * _indices.Length, SlimDX.Direct3D9.Usage.WriteOnly,
                        SlimDX.Direct3D9.Pool.Managed, true);

            // Default the material.
            _material = new SlimDX.Direct3D9.Material();
            _material.Diffuse = new SlimDX.Color4(1.0f, 1.0f, 1.0f);
            _material.Ambient = new SlimDX.Color4(0.25f, 0.25f, 0.25f);
            _material.Specular = new SlimDX.Color4(1.0f, 1.0f, 1.0f);
            _material.Power = 16.0f;

            // Fill our buffers.
            BuildModel();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_vertexType != null)
                        _vertexType.Dispose();
                    if (_vertexBuffer != null)
                        _vertexBuffer.Dispose();
                    if (_indexBuffer != null)
                        _indexBuffer.Dispose();

                    _vertexType = null;
                    _vertexBuffer = null;
                    _indexBuffer = null;
                }

                _disposed = true;
            }
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
