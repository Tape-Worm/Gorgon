﻿#region MIT
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
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example;
using Gorgon.Math;
using Gorgon.Native;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// Object representing a 3D dimensional cube.
    /// </summary>
    public class Cube
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
        public GorgonTextureShaderView Texture
        {
            get;
            set;
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
        public void Translate(float x, float y, float z)
        {
			DX.Matrix.Translation(x, y, z, out _translation);
        }

		/// <summary>
		/// Function to retrieve the world matrix for transforming the cube.
		/// </summary>
		/// <param name="worldMatrix">The world matrix for this cube.</param>
	    public void GetWorldMatrix(out DX.Matrix worldMatrix)
		{
			DX.Matrix.Multiply(ref _rotation, ref _translation, out _world);
		    worldMatrix = _world;
	    }

        /// <summary>
        /// Function to draw the cube.
        /// </summary>
        public void Draw()
        {
            /*SlimDX.Matrix transform = Matrix.Multiply(_rotation, _translation);      // Transformation matrix..

            _d3d.Device.VertexDeclaration = _vertexType;
            _d3d.Device.Indices = _indexBuffer;
            _d3d.Device.SetStreamSource(0, _vertexBuffer, 0, _vertexByteSize);

            _d3d.Device.Material = _material;
            _d3d.SetImage(0, _cubeTexture);

            _d3d.Device.SetTransform(Direct3D9.TransformState.World, transform);

            _d3d.Device.DrawIndexedPrimitives(Direct3D9.PrimitiveType.TriangleList, 0, 0, _vertices.Length, 0, _indices.Length / 3);*/
        }

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
			GlassCubeVertex[] vertices =
			{
			    // Front face.
			    new GlassCubeVertex(new DX.Vector3(-0.5f, 0.5f, -0.5f), new DX.Vector2(0, 0)),
				new GlassCubeVertex(new DX.Vector3(0.5f, -0.5f, -0.5f), new DX.Vector2(1.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, -0.5f, -0.5f), new DX.Vector2(0.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(0.5f, 0.5f, -0.5f), new DX.Vector2(1.0f, 0.0f)),

			    // Right face.
			    new GlassCubeVertex(new DX.Vector3(0.5f, 0.5f, -0.5f), new DX.Vector2(0, 0)),
				new GlassCubeVertex(new DX.Vector3(0.5f, -0.5f, 0.5f), new DX.Vector2(1.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(0.5f, -0.5f, -0.5f), new DX.Vector2(0.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(0.5f, 0.5f, 0.5f), new DX.Vector2(1.0f, 0.0f)),

			    // Back face.
			    new GlassCubeVertex(new DX.Vector3(0.5f, 0.5f, 0.5f), new DX.Vector2(0, 0)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, -0.5f, 0.5f), new DX.Vector2(1.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(0.5f, -0.5f, 0.5f), new DX.Vector2(0.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, 0.5f, 0.5f), new DX.Vector2(1.0f, 0.0f)),

			    // Left face.
			    new GlassCubeVertex(new DX.Vector3(-0.5f, 0.5f, 0.5f), new DX.Vector2(0, 0)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, -0.5f, -0.5f), new DX.Vector2(1.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, -0.5f, 0.5f), new DX.Vector2(0.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, 0.5f, -0.5f), new DX.Vector2(1.0f, 0.0f)),

			    // Top face.
			    new GlassCubeVertex(new DX.Vector3(-0.5f, 0.5f, 0.5f), new DX.Vector2(0, 0)),
				new GlassCubeVertex(new DX.Vector3(0.5f, 0.5f, -0.5f), new DX.Vector2(1.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, 0.5f, -0.5f), new DX.Vector2(0.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(0.5f, 0.5f, 0.5f), new DX.Vector2(1.0f, 0.0f)),

			    // Bottom face.
			    new GlassCubeVertex(new DX.Vector3(-0.5f, -0.5f, -0.5f), new DX.Vector2(0, 0)),
				new GlassCubeVertex(new DX.Vector3(0.5f, -0.5f, 0.5f), new DX.Vector2(1.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(-0.5f, -0.5f, 0.5f), new DX.Vector2(0.0f, 1.0f)),
				new GlassCubeVertex(new DX.Vector3(0.5f, -0.5f, -0.5f), new DX.Vector2(1.0f, 0.0f))
			};

			ushort[] indices =
			{
				8, 9, 10, 8, 11, 9,
				12, 13, 14, 12, 15, 13,
				4, 5, 6, 4, 7, 5,
				16, 17, 18, 16, 19, 17,
				20, 21, 22, 20, 23, 21,
				0, 1, 2, 0, 3, 1
			};

			// Create our index buffer and vertex buffer and populate with our cube data.
			using (var indexPtr = new GorgonPointerPinned<ushort>(indices))
			using (var vertexPtr = new GorgonPointerPinned<GlassCubeVertex>(vertices))
			{
				IndexBuffer = new GorgonIndexBuffer("GlassCube Index Buffer",
													graphics,
													new GorgonIndexBufferInfo
													{
														Usage = D3D11.ResourceUsage.Immutable,
														IndexCount = indices.Length,
														Use16BitIndices = true
													},
													indexPtr);

				VertexBuffer = new GorgonVertexBufferBindings(inputLayout, 1)
				{
					[0] = new GorgonVertexBufferBinding(new GorgonVertexBuffer("GlassCube Vertex Buffer",
																								graphics,
																								new GorgonVertexBufferInfo
																								{
																									Usage = D3D11.ResourceUsage.Immutable,
																									SizeInBytes = GlassCubeVertex.SizeInBytes * vertices.Length
																								},
																								vertexPtr),
																		GlassCubeVertex.SizeInBytes)
				};
			}
		}
		#endregion
	}
}
