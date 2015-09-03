#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, August 29, 2013 8:22:58 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
{
    /// <summary>
    /// The type of drawing to use when drawing the polygon.
    /// </summary>
    [Flags]
    public enum PolygonType
    {
        /// <summary>
        /// Use a triangle list to draw the polygon.
        /// </summary>
        Triangle = 1,
        /// <summary>
        /// Use a line list to draw the polygon.
        /// </summary>
        Line = 2,
        /// <summary>
        /// Use the stripped version of the drawing type.
        /// </summary>
        /// <remarks>This flag must be combined with either Triangle or Line.</remarks>
        Strip = 4
    }

    /// <summary>
    /// A polygonal renderable.  This object is used to draw irregularly shaped areas.
    /// </summary>
    /// <remarks>Use this object to draw irregularly shaped areas as a sprite.  This object takes raw vertex (and optionally, index) data to build a sprite object that can have any 
    /// shape.
    /// <para>
    /// When building the polygon it can either use a line or triangle type to define how to draw it.  If the triangle polygon type is chosen, then the polygon will be composed of 
    /// triangles which require 3 vertices per triangle, and if the line type is chosen then lines, which require 2 vertices per line, will be used to draw the polygon.  The lines/triangles 
    /// may also be drawn as a strip in order to cut down on the amount of data being sent to the GPU, this is done by specifying the Strip flag.
    /// </para>
    /// <para>The <see cref="CullingMode"/> of the polygon affects the triangle type.  If it is set to  CCW then the vertices must be arranged in clockwise order, otherwise, they must 
    /// be arranged in counter clockwise order.</para>
    /// </remarks>
    public class GorgonPolygon
        : GorgonNamedObject, IRenderable, IMoveable, IDisposable, I2DCollisionObject, IDeferredTextureLoad, IPersistedRenderable
    {
		#region Constants.
		// POLYDATA chunk.
	    private const string PolygonDataChunk = "POLYDATA";
		// RNDRDATA chunk.
		private const string RenderDataChunk = "RNDRDATA";
		// TXTRDATA chunk.
		private const string TextureDataChunk = "TXTRDATA";

		/// <summary>
		/// Header for the Gorgon polygon file.
		/// </summary>		
		public const string FileHeader = "GORPLY20";
		#endregion

		#region Value Types.
        /// <summary>
        /// Material for a polygon.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 32, Pack = 16)]
        private struct PolygonMaterial
        {
            /// <summary>
            /// Color of the material.
            /// </summary>
            public GorgonColor DiffuseColor;
            /// <summary>
            /// Transformation to apply to the bound texture.
            /// </summary>
            public Vector4 TextureTransform;

            /// <summary>
            /// Initializes a new instance of the <see cref="PolygonMaterial"/> struct.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <param name="texTransform">The tex transform.</param>
            public PolygonMaterial(GorgonColor color, Vector4 texTransform)
            {
                DiffuseColor = color;
                TextureTransform = texTransform;
            }
        }
        #endregion

        #region Variables.
		private string _textureName = string.Empty;												// Name of the texture for the polygon.
	    private GorgonTexture2D _texture;														// Texture for the polygon.
	    private bool _disposed;																	// Flag to indicate that the object was disposed.
	    private readonly Vector2[] _boundVertexPositions;										// Boundaries for the min/max vertex positions in object space.
		private readonly Gorgon2DVertex[] _boundVertices;										// Min/max boundary vertices.
		private GorgonVertexBuffer _vertexBuffer;                                               // The vertex buffer for the polygon.
        private bool _isDynamicIndexBuffer;                                                     // Flag to indicate that the polygon uses a dynamic index buffer.
        private bool _isDynamicVertexBuffer;                                                    // Flag to indicate that the polygon uses a dynamic vertex buffer.
        private GorgonRenderable.DepthStencilStates _depthStencilState;                         // Depth stencil state for the renderable.
        private GorgonRenderable.TextureSamplerState _samplerState;                             // Texture sampler state for the renderable.
        private GorgonRenderable.BlendState _blendState;                                        // Blending state for the renderable.
        private bool _needsTransformUpdate = true;                                              // Flag to indicate that the object needs to be transformed.
        private Matrix _transformMatrix = Matrix.Identity;                                      // The final transformation matrix to send to the GPU.
        private Vector2 _position = Vector2.Zero;                                               // The position of the polygon.
        private Vector2 _anchor = Vector2.Zero;                                                 // The anchor position of the polygon.
        private float _angle;                                                                   // The angle of rotation in degrees.
        private Vector2 _scale = Vector2.Zero;                                                  // The scale of the polygon.
        private float _depth;                                                                   // Depth value.
        private PolygonMaterial _material;                                                      // Material for the polygon.
		private Gorgon2DCollider _collider;														// Collider for the sprite.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the size of the renderable.
        /// </summary>
        public Vector2 Size
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the type of primitive to use for drawing the polygon.
        /// </summary>
        public PolygonType PolygonType
        {
            get;
            set;
        }

		/// <summary>
		/// Property to return the renderer that created this object.
		/// </summary>
	    public Gorgon2D Gorgon2D
	    {
		    get;
	    }

        /// <summary>
        /// Property to set or return whether to use a dynamic index buffer for the polygon.
        /// </summary>
        /// <remarks>This property rebuilds the index buffer for the polygon and allows it to be used as a dynamic resource.  This means that the shape can be updated with less of a performance 
        /// penalty.
        /// <para>
        /// If switching between a static and dynamic buffer, please note that the operation is destructive.  This means that the index buffer will lose its contents.
        /// </para>
        /// </remarks>
        public bool UseDynamicIndexBuffer
        {
            get
            {
                return _isDynamicIndexBuffer;
            }
            set
            {
                if (value == _isDynamicIndexBuffer)
                {
                    return;
                }
                
                _isDynamicIndexBuffer = value;

                // Remove the index buffer
                if (IndexBuffer == null)
                {
                    return;
                }

                IndexBuffer.Dispose();
                IndexBuffer = null;

                // Reallocate the buffer.
                EnsureIndexCapacity(IndexCount);
            }
        }

        /// <summary>
        /// Property to set or return whether to use a dynamic vertex buffer for the polygon.
        /// </summary>
        /// <remarks>This property rebuilds the vertex buffer for the polygon and allows it to be used as a dynamic resource.  This means that the shape can be updated with less of a performance 
        /// penalty.  
        /// <para>
        /// If switching between a static and dynamic buffer, please note that the operation is destructive.  This means that the vertex buffer will lose its contents.
        /// </para>
        /// </remarks>
        public bool UseDynamicVertexBuffer
        {
            get
            {
                return _isDynamicVertexBuffer;
            }
            set
            {
                if (value == _isDynamicVertexBuffer)
                {
                    return;
                }
               
                _isDynamicVertexBuffer = value;

                if (_vertexBuffer == null)
                {
                    return;
                }

                _vertexBuffer.Dispose();
                _vertexBuffer = null;

                EnsureVertexCapacity(VertexCount);
            }
        }

        /// <summary>
        /// Property to set or return the texture scale.
        /// </summary>
        [AnimatedProperty]
        public Vector2 TextureScale
        {
            get
            {
                return new Vector2(_material.TextureTransform.Z, _material.TextureTransform.W);
            }
            set
            {
                _material.TextureTransform = new Vector4(_material.TextureTransform.X, _material.TextureTransform.Y, value.X, value.Y);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the transform for the polygon.
        /// </summary>
        private void UpdateTransform()
        {
            var rotationDir = new Vector3(0, 0, 1);
            
            Matrix translateMatrix;

            Matrix.Translation(_position.X, _position.Y, Depth, out translateMatrix);

	        // ReSharper disable CompareOfFloatsByEqualityOperator
            if ((_scale.X != 1.0f)
                || (_scale.Y != 1.0f))
            {
                Matrix scaleMatrix;

                Matrix.Scaling(_scale.X, _scale.Y, 0, out scaleMatrix);

                Matrix.Multiply(ref scaleMatrix, ref translateMatrix, out translateMatrix);
            }

            if (_angle != 0.0f)
            {
                Matrix rotationMatrix;

                Matrix.RotationAxis(ref rotationDir, _angle, out rotationMatrix);

                Matrix.Multiply(ref rotationMatrix, ref translateMatrix, out translateMatrix);
            }

            if ((Anchor.X != 0.0f)
                || (Anchor.Y != 0.0f))
            {
                Matrix anchorMatrix;
                Matrix.Translation(-_anchor.X, -_anchor.Y, 0.0f, out anchorMatrix);
                Matrix.Multiply(ref anchorMatrix, ref translateMatrix, out translateMatrix);
            }
			// ReSharper restore CompareOfFloatsByEqualityOperator

			// Update boundaries.
	        if (Collider != null)
	        {
		        for (int i = 0; i < _boundVertexPositions.Length; ++i)
		        {
			        Vector2 position = _boundVertexPositions[i];
			        Vector4 bound;
			        Vector2.Transform(ref position, ref translateMatrix, out bound);
			        _boundVertices[i].Position = bound;
		        }

		        Collider.UpdateFromCollisionObject();
	        }

	        Matrix viewProj = Gorgon2D.Camera.ViewProjection;

            Matrix.Multiply(ref translateMatrix, ref viewProj, out _transformMatrix);

            _needsTransformUpdate = false;
        }

        /// <summary>
        /// Function to determine if the vertex buffer is large enough to hold the required vertices.
        /// </summary>
        /// <param name="vertexCount">Number of vertices to assign.</param>
        private void EnsureVertexCapacity(int vertexCount)
        {
            int sizeInBytes = vertexCount * Gorgon2DVertex.SizeInBytes;

            // If the vertex buffer is too small, we need to resize it.
            if (_vertexBuffer != null)
            {
                if (sizeInBytes <= _vertexBuffer.SizeInBytes)
                {
                    return;
                }

                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            // If the vertex buffer is OK (i.e. it wasn't destroyed), then leave.
            if (_vertexBuffer != null)
            {
                return;
            }

            // Recreate (or create) the buffer.
            _vertexBuffer = Gorgon2D.Graphics.ImmediateContext.Buffers.CreateVertexBuffer(Name + " Gorgon2D.GorgonPolygon.GorgonVertexBuffer",
                new GorgonBufferSettings
                {
                    SizeInBytes = sizeInBytes * 2,
                    Usage = UseDynamicVertexBuffer ? BufferUsage.Dynamic : BufferUsage.Default
                });

            VertexBufferBinding = new GorgonVertexBufferBinding(_vertexBuffer, Gorgon2DVertex.SizeInBytes);

            // Update the vertex count.
            VertexCount = vertexCount;
        }

        /// <summary>
        /// Function to determine if the index buffer is large enough to hold the required indices.
        /// </summary>
        /// <param name="indexCount">Number of indices to assign.</param>
        private void EnsureIndexCapacity(int indexCount)
        {
            int sizeInBytes = indexCount * sizeof(int);

            // If the vertex buffer is too small, we need to resize it.
            if (IndexBuffer != null)
            {
                if (sizeInBytes <= IndexBuffer.SizeInBytes)
                {
                    return;
                }

                IndexBuffer.Dispose();
                IndexBuffer = null;
            }

            // If the index buffer is OK (i.e. it wasn't destroyed), then leave.
            if (IndexBuffer != null)
            {
                return;
            }

            // Recreate (or create) the buffer.
            IndexBuffer = Gorgon2D.Graphics.ImmediateContext.Buffers.CreateIndexBuffer(Name + " Gorgon2D.GorgonPolygon.GorgonIndexBuffer",
                new GorgonIndexBufferSettings
                {
                    SizeInBytes = sizeInBytes * 2,
                    Usage = UseDynamicVertexBuffer ? BufferUsage.Dynamic : BufferUsage.Default,
                    Use32BitIndices = true
                });

            // Update the vertex count.
            IndexCount = indexCount;
        }

        /// <summary>
        /// Function to assign index data to the polygon.
        /// </summary>
        /// <param name="indices">An array of indices to send to the polygon.</param>
        /// <remarks>
        /// Use this method to upload index values to the polygon.  You may use this method to update all, or a portion of the buffer.
        /// <para>This method will expand the index buffer for the polygon as necessary and will adjust the index buffer size based on the size of the <paramref name="indices"/> parameter.</para>
        /// <para>If the data in the index buffer needs to be updated frequently, then it is best to ensure that the index buffer is dynamic and to update only the portion of the buffer 
        /// that requires updating.</para>
        /// <para>Passing NULL or an empty array to the <paramref name="indices"/> parameter will disable the index buffer for the polygon.</para>
        /// <para>All values in the index buffer are 32 bit integer values.</para>
        /// </remarks>
        public void SetIndexData(int[] indices)
        {
            SetIndexData(indices, 0, 0, indices == null ? 0 : indices.Length);
        }

        /// <summary>
        /// Function to assign index data to the polygon.
        /// </summary>
        /// <param name="indices">An array of indices to send to the polygon.</param>
        /// <param name="indexListOffset">Offset within the array to start reading from.</param>
        /// <param name="offset">Offset in the polygon index buffer to start writing at.</param>
        /// <remarks>
        /// Use this method to upload index values to the polygon.  You may use this method to update all, or a portion of the buffer.
        /// <para>This method will expand the index buffer for the polygon as necessary and will adjust the index buffer size based on the total of the <paramref name="offset"/> and the length of the 
        /// <paramref name="indices"/> parameters.  This means that a gap could be allocated in the buffer before the <paramref name="offset"/>.</para>
        /// <para>If the data in the index buffer needs to be updated frequently, then it is best to ensure that the index buffer is dynamic and to update only the portion of the buffer 
        /// that requires updating.</para>
        /// <para>Passing NULL or an empty array to the <paramref name="indices"/> parameter will disable the index buffer for the polygon.</para>
        /// <para>All values in the index buffer are 32 bit integer values.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>The <paramref name="indexListOffset"/> parameter is less than 0 or greater than or equal to the number of indices in the <paramref name="indices"/> parameter.</para>
        /// </exception>
        public void SetIndexData(int[] indices, int indexListOffset, int offset)
        {
            SetIndexData(indices, indexListOffset, offset, indices == null ? 0 : indices.Length);
        }

        /// <summary>
        /// Function to assign index data to the polygon.
        /// </summary>
        /// <param name="indices">An array of indices to send to the polygon.</param>
        /// <param name="indexListOffset">Offset within the array to start reading from.</param>
        /// <param name="offset">Offset in the polygon index buffer to start writing at.</param>
        /// <param name="count">The number of indices to write.</param>
        /// <remarks>
        /// Use this method to upload index values to the polygon.  You may use this method to update all, or a portion of the buffer.
        /// <para>This method will expand the index buffer for the polygon as necessary and will adjust the index buffer size based on the total of the <paramref name="offset"/> and <paramref name="count"/> 
        /// parameters.  This means that a gap could be allocated in the buffer before the <paramref name="offset"/>.</para>
        /// <para>If the data in the index buffer needs to be updated frequently, then it is best to ensure that the index buffer is dynamic and to update only the portion of the buffer 
        /// that requires updating.</para>
        /// <para>Passing NULL or an empty array to the <paramref name="indices"/> parameter, or 0 to the <paramref name="count"/> parameter will disable the index buffer for the polygon.</para>
        /// <para>All values in the index buffer are 32 bit integer values.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>The <paramref name="indexListOffset"/> parameter is less than 0 or greater than or equal to the number of indices in the <paramref name="indices"/> parameter.</para>
        /// <para>-or-</para>
        /// <para>The <paramref name="count"/> parameter is less than 0 or greater than the number of indices in the <paramref name="indices"/> parameter.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="indexListOffset"/> parameter and the <paramref name="count"/> parameter are greater than the 
        /// number of indices in the <paramref name="indices"/> parameter.</exception>
        public void SetIndexData(int[] indices, int indexListOffset, int offset, int count)
        {
            const int intSize = sizeof(int);

            // If we pass nothing, then set the index count to 0 and remove the index buffer.
            if ((indices == null) || (indices.Length == 0) || (count <= 0))
            {
                IndexCount = 0;

                if (IndexBuffer == null)
                {
                    return;
                }

                IndexBuffer.Dispose();
                IndexBuffer = null;

                return;
            }

#if DEBUG
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), string.Format(Resources.GOR2D_ARG_LESS_THAN_ZERO, offset));
            }

            if ((indexListOffset < 0)
                || (indexListOffset >= indices.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(indexListOffset),
                    string.Format(Resources.GOR2D_ARG_OUT_OF_RANGE, indexListOffset, 0, indices.Length - 1));
            }

            if  ((count > indices.Length)
                || (count < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (indexListOffset + count > indices.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR2D_ARGS_LARGER_THAN_ARRAY, indexListOffset, count, indices.Length));
            }
#endif
            // Ensure that the vertex buffer has enough room to hold our vertices.
            EnsureIndexCapacity(offset + count);

            unsafe
            {
                if (UseDynamicIndexBuffer)
                {
                    // If we're using a dynamic index buffer, then we need to lock and write our data.
                    BufferLockFlags lockFlags = offset == 0 ? BufferLockFlags.Discard : BufferLockFlags.NoOverwrite;
                    GorgonDataStream lockStream = IndexBuffer.Lock(lockFlags | BufferLockFlags.Write,
                        Gorgon2D.Graphics);

                    var vertexPtr = (int*)lockStream.BasePointer;

                    // Move to the offset.
                    vertexPtr += offset;

                    for (int i = 0; i < count; ++i)
                    {
                        *vertexPtr = indices[i + indexListOffset];
                        vertexPtr++;
                    }

					IndexBuffer.Unlock(Gorgon2D.Graphics);
                }
                else
                {
                    fixed (int* ptr = &indices[indexListOffset])
                    {
                        using (var data = new GorgonDataStream(ptr, count * intSize))
                        {
                            IndexBuffer.Update(data,
                                offset * intSize,
                                count * intSize,
                                Gorgon2D.Graphics);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to send vertices into the polygon.
        /// </summary>
        /// <param name="vertices">Vertices to upload to the polygon.</param>
        /// <remarks>
        /// Use this method to upload vertices to the polygon.  You may use this method to update all, or a portion of the vertex buffer.
        /// <para>If the data in the vertex buffer needs to be updated frequently, then it is best to ensure that the vertex buffer is dynamic and to update only the portion of the buffer 
        /// that requires updating.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="vertices"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        public void SetVertexData(GorgonPolygonPoint[] vertices)
        {
            vertices.ValidateObject("vertices");

            SetVertexData(vertices, 0, 0, vertices.Length);
        }

        /// <summary>
        /// Function to send vertices into the polygon.
        /// </summary>
        /// <param name="vertices">Vertices to upload to the polygon.</param>
        /// <param name="vertexListOffset">Offset within the array to start reading from.</param>
        /// <param name="offset">Offset in the polygon vertex buffer to start writing at.</param>
        /// <remarks>
        /// Use this method to upload vertices to the polygon.  You may use this method to update all, or a portion of the vertex buffer.
        /// <para>This method will expand the vertex buffer for the polygon as necessary and will adjust the vertex buffer size based on the total of the <paramref name="offset"/> and the length 
        /// of the <paramref name="vertices"/> parameters.  This means that a gap could be allocated in the buffer before the <paramref name="offset"/>.</para>
        /// <para>If the data in the vertex buffer needs to be updated frequently, then it is best to ensure that the vertex buffer is dynamic and to update only the portion of the buffer 
        /// that requires updating.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="vertices"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>The <paramref name="vertexListOffset"/> parameter is less than 0 or greater than or equal to the number of vertices in the <paramref name="vertices"/> parameter.</para>
        /// </exception>
        public void SetVertexData(GorgonPolygonPoint[] vertices, int vertexListOffset, int offset)
        {
            vertices.ValidateObject("vertices");

            SetVertexData(vertices, vertexListOffset, offset, vertices.Length);
        }

        /// <summary>
        /// Function to send vertices into the polygon.
        /// </summary>
        /// <param name="vertices">Vertices to upload to the polygon.</param>
        /// <param name="vertexListOffset">Offset within the array to start reading from.</param>
        /// <param name="offset">Offset in the polygon vertex buffer to start writing at.</param>
        /// <param name="count">The number of vertices to write.</param>
        /// <remarks>
        /// Use this method to upload vertices to the polygon.  You may use this method to update all, or a portion of the vertex buffer.
        /// <para>This method will expand the vertex buffer for the polygon as necessary and will adjust the vertex buffer size based on the total of the <paramref name="offset"/> and <paramref name="count"/> 
        /// parameters.  This means that a gap could be allocated in the buffer before the <paramref name="offset"/>.</para>
        /// <para>If the data in the vertex buffer needs to be updated frequently, then it is best to ensure that the vertex buffer is dynamic and to update only the portion of the buffer 
        /// that requires updating.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="vertices"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>The <paramref name="vertexListOffset"/> parameter is less than 0 or greater than or equal to the number of vertices in the <paramref name="vertices"/> parameter.</para>
        /// <para>-or-</para>
        /// <para>The <paramref name="count"/> parameter is less than 0 or greater than the number of vertices in the <paramref name="vertices"/> parameter.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="vertexListOffset"/> parameter and the <paramref name="count"/> parameter are greater than the 
        /// number of vertices in the <paramref name="vertices"/> parameter.</exception>
        public void SetVertexData(GorgonPolygonPoint[] vertices, int vertexListOffset, int offset, int count)
        {
            vertices.ValidateObject("vertices");

            if (vertices.Length == 0)
            {
                return;
            }

#if DEBUG
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset),
                    string.Format(Resources.GOR2D_ARG_LESS_THAN_ZERO, offset));
            }

            if ((vertexListOffset < 0)
                || (vertexListOffset >= vertices.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(vertexListOffset),
                    string.Format(Resources.GOR2D_ARG_OUT_OF_RANGE, vertexListOffset, 0, vertices.Length - 1));
            }

            if ((count > vertices.Length)
                || (count < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (vertexListOffset + count > vertices.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR2D_ARGS_LARGER_THAN_ARRAY,
                    vertexListOffset,
                    count,
                    vertices.Length));
            }
#endif
            // Ensure that the vertex buffer has enough room to hold our vertices.
            EnsureVertexCapacity(offset + count);

            // If we're using a dynamic vertex buffer, then we need to lock and write our data.
            unsafe
            {
	            float minX = _boundVertices[0].Position.X;
	            float minY = _boundVertices[0].Position.Y;
                float maxX = _boundVertices[1].Position.X;
                float maxY = _boundVertices[1].Position.Y;

                // If we're replacing the entire buffer, then update our max size entirely.
                if ((count >= VertexCount) && (offset == 0))
                {
                    maxY = maxX = float.MinValue;
	                minY = minX = float.MaxValue;
                }

                if (UseDynamicVertexBuffer)
                {
                    BufferLockFlags lockFlags = offset == 0 ? BufferLockFlags.Discard : BufferLockFlags.NoOverwrite;
                    GorgonDataStream lockStream = _vertexBuffer.Lock(lockFlags | BufferLockFlags.Write,
                        Gorgon2D.Graphics);

                    var vertexPtr = (Gorgon2DVertex*)lockStream.BasePointer;

                    // Move to the offset.
                    vertexPtr += offset;

                    for (int i = 0; i < count; ++i)
                    {
                        Gorgon2DVertex vertex = vertices[i + vertexListOffset];
                        Vector4 position = vertex.Position;

                        position.Z = Depth;
                        vertex.Position = position;
                        
                        *vertexPtr = vertex;

                        maxX = maxX.Max(position.X);
                        maxY = maxY.Max(position.Y);
	                    minX = minX.Min(position.X);
	                    minY = minY.Min(position.Y);

                        vertexPtr++;
                    }

					_vertexBuffer.Unlock(Gorgon2D.Graphics);
                }
                else
                {
                    using(var data = new GorgonDataStream(Gorgon2DVertex.SizeInBytes * count))
                    {
                        var ptr = (Gorgon2DVertex *)data.BasePointer;

                        // Find the new size if necessary.
                        for (int i = 0; i < count; ++i)
                        {
                            *ptr = vertices[i];

                            ptr->Position.Z = Depth;

                            maxX = maxX.Max(ptr->Position.X);
                            maxY = maxY.Max(ptr->Position.Y);
                            minX = minX.Min(ptr->Position.X);
                            minY = minY.Min(ptr->Position.Y);

                            ptr++;
                        }

                        _vertexBuffer.Update(data, offset * Gorgon2DVertex.SizeInBytes, count * Gorgon2DVertex.SizeInBytes);
                    }
                }

				// Assign boundaries.
	            _boundVertexPositions[0] = new Vector2(minX, minY);
				_boundVertexPositions[1] = new Vector2(maxX, minY);
				_boundVertexPositions[2] = new Vector2(minX, maxY);
	            _boundVertexPositions[3] = new Vector2(maxX, maxY);

                Size = new Vector2(maxX - minX, maxY - minY);
            }
        }

		/// <summary>
		/// Function to save the sprite into memory.
		/// </summary>
		/// <returns>A byte array containing the sprite data.</returns>
		public byte[] Save()
		{
			using (var stream = new MemoryStream())
			{
				Save(stream);

				return stream.ToArray();
			}
		}

		/// <summary>
		/// Function to save the sprite to a file.
		/// </summary>
		/// <param name="filePath">Path to the file to write the sprite information into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.</exception>
		public void Save(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, filePath);
			}

			using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				Save(stream);
			}
		}
        #endregion

        #region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPolygon"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		/// <param name="name">The name of the renderable.</param>
		internal GorgonPolygon(Gorgon2D gorgon2D, string name)
			: this(gorgon2D, name, PolygonType.Triangle)
	    {
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPolygon"/> class.
        /// </summary>
        /// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
        /// <param name="name">The name of the renderable.</param>
        /// <param name="polygonType">The type of polygon to draw.</param>
        internal GorgonPolygon(Gorgon2D gorgon2D, string name, PolygonType polygonType)
            : base(name)
        {
            Gorgon2D = gorgon2D;

			// Update bound vertex positions.
	        _boundVertexPositions = new[]
	        {
		        new Vector2(float.MaxValue, float.MaxValue),
				new Vector2(float.MinValue, float.MaxValue), 
				new Vector2(float.MaxValue, float.MinValue),
				new Vector2(float.MinValue, float.MinValue)
	        };

			_boundVertices = new Gorgon2DVertex[_boundVertexPositions.Length];

	        CullingMode = CullingMode.Back;
            Size = new Vector2(float.MinValue, float.MinValue);
            Scale = new Vector2(1);
            _material = new PolygonMaterial(GorgonColor.White, new Vector4(0, 0, 1, 1));

            AlphaTestValues = GorgonRangeF.Empty;
            DepthStencil = new GorgonRenderable.DepthStencilStates();
            Blending = new GorgonRenderable.BlendState();
            TextureSampler = new GorgonRenderable.TextureSamplerState();

            PolygonType = polygonType;
        }
        #endregion

        #region IRenderable Members
        #region Properties.
        /// <summary>
        /// Property to set or return the texture region.
        /// </summary>
        /// <remarks>This has no meaning for this renderable type.</remarks>
        /// <exception cref="NotSupportedException">Thrown if an attempt to set this property is made.</exception>
        RectangleF IRenderable.TextureRegion
        {
            get
            {
                return RectangleF.Empty;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Property to set or return the coordinates in the texture to use as a starting point for drawing.
        /// </summary>
        /// <remarks>This value is in texels.</remarks>
        [AnimatedProperty]
        public Vector2 TextureOffset
        {
            get
            {
                return new Vector2(_material.TextureTransform.X, _material.TextureTransform.Y);
            }
            set
            {
                _material.TextureTransform = new Vector4(value, _material.TextureTransform.Z, _material.TextureTransform.W);
            }
        }

        /// <summary>
        /// Property to set or return the scaling of the texture width and height.
        /// </summary>
        /// <remarks>This has no meaning for this renderable type.</remarks>
        /// <exception cref="NotSupportedException">Thrown if an attempt to set this property is made.</exception>
        Vector2 IRenderable.TextureSize
        {
            get
            {
                return Vector2.Zero;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Property to set or return the vertex buffer binding for this renderable.
        /// </summary>
        public GorgonVertexBufferBinding VertexBufferBinding
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the index buffer for this renderable.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the type of primitive for the renderable.
        /// </summary>
        PrimitiveType IRenderable.PrimitiveType
        {
            get
            {
                if ((PolygonType & PolygonType.Line) == PolygonType.Line)
                {
                    return (PolygonType & PolygonType.Strip) == PolygonType.Strip
                        ? PrimitiveType.LineStrip
                        : PrimitiveType.LineList;
                }

                return (PolygonType & PolygonType.Strip) == PolygonType.Strip
                    ? PrimitiveType.TriangleStrip
                    : PrimitiveType.TriangleList;
            }
        }

        /// <summary>
        /// Property to return a list of vertices to render.
        /// </summary>
        /// <remarks>This renderable isn't cached, so it will always return NULL.</remarks>
        Gorgon2DVertex[] IRenderable.Vertices => null;

	    /// <summary>
        /// Property to return the number of indices that make up this renderable.
        /// </summary>
        /// <remarks>
        /// This only matters when the renderable uses an index buffer.
        /// </remarks>
        public int IndexCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of vertices to add to the base starting index.
        /// </summary>
        int IRenderable.BaseVertexCount => 0;

	    /// <summary>
        /// Property to return the number of vertices for the renderable.
        /// </summary>
        public int VertexCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return depth/stencil buffer states for this renderable.
        /// </summary>
        public GorgonRenderable.DepthStencilStates DepthStencil
        {
            get
            {
                return _depthStencilState;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _depthStencilState = value;
            }
        }

        /// <summary>
        /// Property to set or return advanced blending states for this renderable.
        /// </summary>
        public GorgonRenderable.BlendState Blending
        {
            get
            {
                return _blendState;
            }
            set
            {
                if (value == null)
                    return;

                _blendState = value;
            }
        }

        /// <summary>
        /// Property to set or return advanded texture sampler states for this renderable.
        /// </summary>
        public GorgonRenderable.TextureSamplerState TextureSampler
        {
            get
            {
                return _samplerState;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _samplerState = value;
            }
        }

        /// <summary>
        /// Property to set or return pre-defined smoothing states for the renderable.
        /// </summary>
        /// <remarks>These modes are pre-defined smoothing states, to get more control over the smoothing, use the <see cref="Gorgon.Renderers.GorgonRenderable.TextureSamplerState.TextureFilter">TextureFilter</see> 
        /// property exposed by the <see cref="Gorgon.Renderers.GorgonRenderable.TextureSampler">TextureSampler</see> property.</remarks>
        public SmoothingMode SmoothingMode
        {
            get
            {
                switch (TextureSampler.TextureFilter)
                {
                    case TextureFilter.Point:
                        return SmoothingMode.None;
                    case TextureFilter.Linear:
                        return SmoothingMode.Smooth;
                    case TextureFilter.MinLinear | TextureFilter.MipLinear:
                        return SmoothingMode.SmoothMinify;
                    case TextureFilter.MagLinear | TextureFilter.MipLinear:
                        return SmoothingMode.SmoothMagnify;
                    default:
                        return SmoothingMode.Custom;
                }
            }
            set
            {
                switch (value)
                {
                    case SmoothingMode.None:
                        TextureSampler.TextureFilter = TextureFilter.Point;
                        break;
                    case SmoothingMode.Smooth:
                        TextureSampler.TextureFilter = TextureFilter.Linear;
                        break;
                    case SmoothingMode.SmoothMinify:
                        TextureSampler.TextureFilter = TextureFilter.MinLinear | TextureFilter.MipLinear;
                        break;
                    case SmoothingMode.SmoothMagnify:
                        TextureSampler.TextureFilter = TextureFilter.MagLinear | TextureFilter.MipLinear;
                        break;
                }
            }
        }

        /// <summary>
        /// Property to set or return a pre-defined blending states for the renderable.
        /// </summary>
        /// <remarks>These modes are pre-defined blending states, to get more control over the blending, use the <see cref="Gorgon.Renderers.GorgonRenderable.BlendState.SourceBlend">SourceBlend</see> 
        /// or the <see cref="P:Gorgon.Renderers.GorgonRenderable.BlendState.DestinationBlend">DestinationBlend</see> property which are exposed by the 
        /// <see cref="P:Gorgon.Renderers.GorgonRenderable.Blending">Blending</see> property.</remarks>
        public BlendingMode BlendingMode
        {
            get
            {
                if ((Blending.SourceBlend == BlendType.One)
                    && (Blending.DestinationBlend == BlendType.Zero))
                {
                    return BlendingMode.None;
                }

                if (Blending.SourceBlend == BlendType.SourceAlpha)
                {
	                switch (Blending.DestinationBlend)
	                {
		                case BlendType.InverseSourceAlpha:
			                return BlendingMode.Modulate;
		                case BlendType.One:
			                return BlendingMode.Additive;
	                }
                }

	            if ((Blending.SourceBlend == BlendType.One)
                    && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
                {
                    return BlendingMode.PreMultiplied;
                }

                if ((Blending.SourceBlend == BlendType.InverseDestinationColor)
                    && (Blending.DestinationBlend == BlendType.InverseSourceColor))
                {
                    return BlendingMode.Inverted;
                }

                return BlendingMode.Custom;
            }
            set
            {
                switch (value)
                {
                    case BlendingMode.Additive:
                        Blending.SourceBlend = BlendType.SourceAlpha;
                        Blending.DestinationBlend = BlendType.One;
                        break;
                    case BlendingMode.Inverted:
                        Blending.SourceBlend = BlendType.InverseDestinationColor;
                        Blending.DestinationBlend = BlendType.InverseSourceColor;
                        break;
                    case BlendingMode.Modulate:
                        Blending.SourceBlend = BlendType.SourceAlpha;
                        Blending.DestinationBlend = BlendType.InverseSourceAlpha;
                        break;
                    case BlendingMode.PreMultiplied:
                        Blending.SourceBlend = BlendType.One;
                        Blending.DestinationBlend = BlendType.InverseSourceAlpha;
                        break;
                    case BlendingMode.None:
                        Blending.SourceBlend = BlendType.One;
                        Blending.DestinationBlend = BlendType.Zero;
                        break;
                }
            }
        }

        /// <summary>
        /// Property to set or return the culling mode.
        /// </summary>
        /// <remarks>Use this to make a renderable two-sided.</remarks>
        public CullingMode CullingMode
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the range of alpha values to reject on this renderable.
        /// </summary>
        /// <remarks>The alpha testing tests to see if an alpha value is between or equal to the values and rejects the pixel if it is not.
        /// <para>This value will not take effect until <see cref="P:Gorgon.Renderers.Gorgon2D.IsAlphaTestEnabled">IsAlphaTestEnabled</see> is set to <b>true</b>.</para>
        /// <para>Typically, performance is improved when alpha testing is turned on with a range of 0.  This will reject any pixels with an alpha of 0.</para>
        /// <para>Be aware that the default shaders implement alpha testing.  However, a custom shader will have to make use of the GorgonAlphaTest constant buffer 
        /// in order to take advantage of alpha testing.</para>
        /// </remarks>
        public GorgonRangeF AlphaTestValues
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the opacity (Alpha channel) of the renderable object.
        /// </summary>
        /// <remarks>
        /// This will only return the alpha value for the first vertex of the renderable and consequently will set all the vertices to the same alpha value.
        /// </remarks>
        [AnimatedProperty]
        public float Opacity
        {
            get
            {
                return _material.DiffuseColor.Alpha;
            }
            set
            {
                _material.DiffuseColor = new GorgonColor(_material.DiffuseColor, value);
            }
        }

        /// <summary>
        /// Property to set or return the color for a renderable object.
        /// </summary>
        /// <remarks>
        /// This will only return the color for the first vertex of the renderable and consequently will set all the vertices to the same color.
        /// </remarks>
        [AnimatedProperty]
        public GorgonColor Color
        {
            get
            {
                return _material.DiffuseColor;
            }
            set
            {
                _material.DiffuseColor = value;
            }
        }

        /// <summary>
        /// Property to set or return a texture for the renderable.
        /// </summary>
        [AnimatedProperty]
        public GorgonTexture2D Texture
        {
	        get
	        {
		        return _texture;
	        }
	        set
	        {
		        if (_texture == value)
		        {
			        return;
		        }

		        _texture = value;
		        _textureName = _texture != null ? _texture.Name : string.Empty;
	        }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to force an update to the renderable object.
		/// </summary>
		/// <remarks>
		/// Take care when calling this method repeatedly.  It will have a significant performance impact.
		/// </remarks>
	    public void Refresh()
		{
			_needsTransformUpdate = true;
		}

        /// <summary>
        /// Function to draw the object.
        /// </summary>
        /// <remarks>
        /// Please note that this doesn't draw the object to the target right away, but queues it up to be
        /// drawn when <see cref="Gorgon.Renderers.Gorgon2D.Render">Render</see> is called.
        /// </remarks>
        public void Draw()
        {
            if (Gorgon2D.Camera.NeedsUpdate)
            {
                Gorgon2D.Camera.Update();
            }

            if (_needsTransformUpdate)
            {
                UpdateTransform();
            }

            // Do this to flush the states.
            Gorgon2D.AddRenderable(this);

            // Set the appropriate pixel shader (if we've not set a custom one).
            GorgonPixelShader previousShader = Gorgon2D.PixelShader.Current;
            GorgonConstantBuffer previousBuffer = Gorgon2D.PixelShader.ConstantBuffers[1];

            if (previousShader == null)
            {
                Gorgon2D.PixelShader.Current = Texture != null
                    ? Gorgon2D.PixelShader.DefaultPixelShaderTexturedMaterial
                    : Gorgon2D.PixelShader.DefaultPixelShaderDiffuseMaterial;
            }

            Gorgon2D.PixelShader.MaterialBuffer.Update(ref _material);
			Gorgon2D.PixelShader.ConstantBuffers[1] = Gorgon2D.PixelShader.MaterialBuffer;

            // Draw the renderable.
			Gorgon2D.VertexShader.TransformBuffer.Update(ref _transformMatrix);

            if (IndexCount == 0)
            {
                Gorgon2D.Graphics.Output.Draw(0, VertexCount);
            }
            else
            {
                Gorgon2D.Graphics.Output.DrawIndexed(0, 0, IndexCount);
            }

            // This will reset the world transform.            
            Gorgon2D.Camera.Update();
            Gorgon2D.PixelShader.ConstantBuffers[1] = previousBuffer;

            if (previousShader != null)
            {
                return;
            }

            Gorgon2D.PixelShader.Current = null;
        }
        #endregion
        #endregion

        #region IMoveable Members
        #region Properties.
        /// <summary>
        /// Property to set or return the position of the renderable.
        /// </summary>
        [AnimatedProperty]
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                _needsTransformUpdate = true;
            }
        }
        
        /// <summary>
        /// Property to set or return the angle of rotation (in degrees) for a renderable.
        /// </summary>
        [AnimatedProperty]
        public float Angle
        {
            get
            {
                return _angle;
            }
            set
            {
	            // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_angle == value)
                {
                    return;
                }

                _angle = value;
                _needsTransformUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the scale of the renderable.
        /// </summary>
        /// <remarks>
        /// This property uses scalar values to provide a relative scale.  To set an absolute scale (i.e. pixel coordinates), use the <see cref="P:Gorgon.Renderers.GorgonMoveable.Size">Size</see> property.
        /// <para>Setting this value to a 0 vector will cause undefined behaviour and is not recommended.</para>
        /// </remarks>
        [AnimatedProperty]
        public Vector2 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (_scale == value)
                {
                    return;
                }

                _scale = value;
                _needsTransformUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the anchor point of the renderable.
        /// </summary>
        [AnimatedProperty]
        public Vector2 Anchor
        {
            get
            {
                return _anchor;
            }
            set
            {
                if (_anchor == value)
                {
                    return;
                }

                _anchor = value;
                _needsTransformUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the "depth" of the renderable in a depth buffer.
        /// </summary>
        [AnimatedProperty]
        public float Depth
        {
            get
            {
                return _depth;
            }
            set
            {
	            // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_depth == value)
                {
                    return;
                }

                _depth = value;
                _needsTransformUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the size of the renderable.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if an attempt to set this property is made.</exception>
        Vector2 IMoveable.Size
        {
            get
            {
                return Size;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion
        #endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
	    private void Dispose(bool disposing)
	    {
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_vertexBuffer != null)
				{
					_vertexBuffer.Dispose();
					_vertexBuffer = null;
				}

				if (IndexBuffer != null)
				{
					IndexBuffer.Dispose();
					IndexBuffer = null;
				}
			}

			_disposed = true;
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

		#region I2DCollisionObject Members
		/// <summary>
		/// Property to set or return the collider that is assigned to the object.
		/// </summary>
		public Gorgon2DCollider Collider
		{
			get
			{
				return _collider;
			}
			set
			{
				if (value == _collider)
				{
					return;
				}

				if (value == null)
				{
					if (_collider != null)
					{
						_collider.CollisionObject = null;
					}

					return;
				}

				// Force a transform to get the latest vertices.
				value.CollisionObject = this;
				_collider = value;
				UpdateTransform();
			}
		}

		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
	    int I2DCollisionObject.VertexCount => 4;

	    /// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2DVertex[] I2DCollisionObject.Vertices => _boundVertices;

	    #endregion

		#region IDeferredTextureLoad Members
		/// <summary>
		/// Property to set or return the name of the deferred texture.
		/// </summary>
		/// <remarks>This is used to defer the texture assignment until it the texture with the specified name is loaded.</remarks>
		public string DeferredTextureName
		{
			get
			{
				if (Texture != null)
				{
					return Texture.Name;
				}

				// Check to see if the texture is loaded.
				if (!string.IsNullOrWhiteSpace(_textureName))
				{
					GetDeferredTexture();
				}

				return _textureName;

			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}

				if (string.Equals(_textureName, value, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				_textureName = value;
				GetDeferredTexture();
			}
		}

		/// <summary>
		/// Function to assign a deferred texture.
		/// </summary>
		/// <remarks>
		/// Call this method to assign a texture that's been deferred.  If a sprite is created/loaded before its texture has been loaded, then the 
		/// sprite will just appear with the color assigned to it and no image.  To counteract this we can assign the <see cref="P:Gorgon.Renderers.GorgonSprite.DeferredTextureName">DeferredTextureName</see> 
		/// property to the name of the texture.  Once the texture with the right name is loaded, call this method to get the sprite to update its texture value from the deferred name.
		/// <para>If loading a sprite from a data source, then this method will be called upon load.  If the texture is not bound successfully (i.e. Texture == null), then it will set the deferred name 
		/// to the texture name stored in the sprite data.</para>
		/// <para>If there are multiple textures with the same name, then the first texture will be chosen.</para>
		/// </remarks>
		public void GetDeferredTexture()
		{
			if (string.IsNullOrEmpty(_textureName))
			{
				Texture = null;
				return;
			}

			Texture = (from texture in Gorgon2D.Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
					   where (texture != null) && (string.Equals(texture.Name, _textureName, StringComparison.OrdinalIgnoreCase))
					   select texture).FirstOrDefault();
		}
		#endregion

		#region IPersistedRenderable Members
		/// <summary>
		/// Function to write out the vertex and index data to the file.
		/// </summary>
	    private unsafe void WriteVertexIndexData(GorgonBinaryWriter writer)
	    {
			// Get the vertices.
			using (var stagingVertex = _vertexBuffer.GetStagingBuffer())
			{
				// Write the contents of the vertex buffer.
				using (var lockStream = stagingVertex.Lock(BufferLockFlags.Read, Gorgon2D.Graphics))
				{
					var vertexPtr = (Gorgon2DVertex*)lockStream.BasePointer;

					for (int i = 0; i < VertexCount; ++i)
					{
						writer.WriteValue(*vertexPtr);
						vertexPtr++;
					}
				}

				if (IndexBuffer == null)
				{
					return;
				}

				// Write index buffer contents.
				using (var stagingIndex = IndexBuffer.GetStagingBuffer())
				{
					using (var lockStream = stagingIndex.Lock(BufferLockFlags.Read, Gorgon2D.Graphics))
					{
						var indexPtr = (int*)lockStream.BasePointer;

						for (int i = 0; i < IndexCount; ++i)
						{
							writer.Write(*indexPtr);
							indexPtr++;
						}
					}
				}
			}
	    }

		/// <summary>
		/// Function to save the renderable to a stream.
		/// </summary>
		/// <param name="stream">Stream to write into.</param>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanWrite)
			{
                throw new IOException(Resources.GOR2D_STREAM_READ_ONLY);
			}

			GorgonChunkFileWriter polyFile = new GorgonChunkFileWriter(stream, FileHeader.ChunkID());

			try
			{
				polyFile.Open();

				GorgonBinaryWriter writer = polyFile.OpenChunk(PolygonDataChunk);

				writer.WriteValue(PolygonType);
				writer.WriteValue(Anchor);
				writer.Write(Color.ToARGB());
				writer.Write(VertexCount);
				writer.Write(IndexCount);

				WriteVertexIndexData(writer);

				polyFile.CloseChunk();

				writer = polyFile.OpenChunk(RenderDataChunk);

				writer.WriteValue(CullingMode);
				writer.WriteValue(AlphaTestValues);
				writer.WriteValue(Blending.AlphaOperation);
				writer.WriteValue(Blending.BlendOperation);
				writer.Write(Blending.BlendFactor.ToARGB());
				writer.WriteValue(Blending.DestinationAlphaBlend);
				writer.WriteValue(Blending.DestinationBlend);
				writer.WriteValue(Blending.SourceAlphaBlend);
				writer.WriteValue(Blending.SourceBlend);
				writer.WriteValue(Blending.WriteMask);
				writer.WriteValue(DepthStencil.BackFace.ComparisonOperator);
				writer.WriteValue(DepthStencil.BackFace.DepthFailOperation);
				writer.WriteValue(DepthStencil.BackFace.FailOperation);
				writer.WriteValue(DepthStencil.BackFace.PassOperation);
				writer.WriteValue(DepthStencil.FrontFace.ComparisonOperator);
				writer.WriteValue(DepthStencil.FrontFace.DepthFailOperation);
				writer.WriteValue(DepthStencil.FrontFace.FailOperation);
				writer.WriteValue(DepthStencil.FrontFace.PassOperation);
				writer.Write(DepthStencil.DepthBias);
				writer.WriteValue(DepthStencil.DepthComparison);
				writer.Write(DepthStencil.StencilReference);
				writer.Write(DepthStencil.IsDepthWriteEnabled);
				writer.Write(DepthStencil.StencilReadMask);
				writer.Write(DepthStencil.StencilWriteMask);

				polyFile.CloseChunk();

				// Write texture information.
				if (string.IsNullOrWhiteSpace(DeferredTextureName))
				{
					return;
				}

				writer = polyFile.OpenChunk(TextureDataChunk);

				writer.Write(TextureSampler.BorderColor.ToARGB());
				writer.WriteValue(TextureSampler.HorizontalWrapping);
				writer.WriteValue(TextureSampler.VerticalWrapping);
				writer.WriteValue(TextureSampler.TextureFilter);
				writer.Write(DeferredTextureName);
				writer.WriteValue(TextureOffset);
				writer.WriteValue(TextureScale);

				polyFile.CloseChunk();
			}
			finally
			{
				polyFile.Close();
			}
		}

		/// <summary>
		/// Function to read the renderable data from a stream.
		/// </summary>
		/// <param name="stream">Open file stream containing the renderable data.</param>
		void IPersistedRenderable.Load(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanRead)
			{
				throw new IOException(Resources.GOR2D_STREAM_WRITE_ONLY);
			}

			GorgonChunkFileReader polyFile = new GorgonChunkFileReader(stream,
			                                                            new[]
			                                                            {
				                                                            FileHeader.ChunkID()
			                                                            });

			try
			{
				polyFile.Open();
				
				GorgonBinaryReader reader = polyFile.OpenChunk(PolygonDataChunk);

				PolygonType = reader.ReadValue<PolygonType>();
				Anchor = reader.ReadValue<Vector2>();
				Color = new GorgonColor(reader.ReadInt32());
				VertexCount = reader.ReadInt32();
				IndexCount = reader.ReadInt32();

				var vertices = new GorgonPolygonPoint[VertexCount];

				reader.ReadRange(vertices);

				SetVertexData(vertices);

				if (IndexCount > 0)
				{
					var indices = new int[IndexCount];

					reader.ReadRange(indices);

					SetIndexData(indices);
				}

				polyFile.CloseChunk();

				reader = polyFile.OpenChunk(RenderDataChunk);

				CullingMode = reader.ReadValue<CullingMode>();
				AlphaTestValues = reader.ReadValue<GorgonRangeF>();
				Blending.AlphaOperation = reader.ReadValue<BlendOperation>();
				Blending.BlendOperation = reader.ReadValue<BlendOperation>();
				Blending.BlendFactor = new GorgonColor(reader.ReadInt32());
				Blending.DestinationAlphaBlend = reader.ReadValue<BlendType>();
				Blending.DestinationBlend = reader.ReadValue<BlendType>();
				Blending.SourceAlphaBlend = reader.ReadValue<BlendType>();
				Blending.SourceBlend = reader.ReadValue<BlendType>();
				Blending.WriteMask = reader.ReadValue<ColorWriteMaskFlags>();
				DepthStencil.BackFace.ComparisonOperator = reader.ReadValue<ComparisonOperator>();
				DepthStencil.BackFace.DepthFailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.BackFace.FailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.BackFace.PassOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.FrontFace.ComparisonOperator = reader.ReadValue<ComparisonOperator>();
				DepthStencil.FrontFace.DepthFailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.FrontFace.FailOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.FrontFace.PassOperation = reader.ReadValue<StencilOperation>();
				DepthStencil.DepthBias = reader.ReadInt32();
				DepthStencil.DepthComparison = reader.ReadValue<ComparisonOperator>();
				DepthStencil.StencilReference = reader.ReadInt32();
				DepthStencil.IsDepthWriteEnabled = reader.ReadBoolean();
				DepthStencil.StencilReadMask = reader.ReadByte();
				DepthStencil.StencilReadMask = reader.ReadByte();
				
				polyFile.CloseChunk();

				if (!polyFile.Chunks.Contains(TextureDataChunk))
				{
					return;
				}

				reader = polyFile.OpenChunk(TextureDataChunk);

				TextureSampler.BorderColor = new GorgonColor(reader.ReadInt32());
				TextureSampler.HorizontalWrapping = reader.ReadValue<TextureAddressing>();
				TextureSampler.VerticalWrapping = reader.ReadValue<TextureAddressing>();
				TextureSampler.TextureFilter = reader.ReadValue<TextureFilter>();
				DeferredTextureName = reader.ReadString();
				TextureOffset = reader.ReadValue<Vector2>();
				TextureScale = reader.ReadValue<Vector2>();

				polyFile.CloseChunk();
			}
			finally
			{
				polyFile.Close();
			}
		}
		#endregion
	}
}