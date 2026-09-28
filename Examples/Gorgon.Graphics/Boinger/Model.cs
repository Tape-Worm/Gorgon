// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: July 30, 2026 9:37:04 PM
//

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// A vertex in our geometry.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Vertex
{
    /// <summary>
    /// The size of the vertex, in bytes.
    /// </summary>
    public static readonly int Size = Unsafe.SizeOf<Vertex>();

    /// <summary>
    /// The object space position of the vertex.
    /// </summary>
    public Vector4 Position;
    /// <summary>
    /// The texture UV coordinates.
    /// </summary>
    public Vector2 UV;
}

/// <summary>
/// The base model object
/// </summary>
internal abstract class Model
    : IDisposable
{
    // Our world matrix.
    private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
    // Our position matrix.
    private Matrix4x4 _positionMatrix = Matrix4x4.Identity;
    // Our scale matrix.
    private Matrix4x4 _scaleMatrix = Matrix4x4.Identity;
    // Our rotation matrix.
    private Matrix4x4 _rotationMatrix = Matrix4x4.Identity;
    // Our position.
    private Vector3 _position = Vector3.Zero;
    // Our scale.
    private Vector3 _scale = Vector3.One;
    // Our rotation.
    private Vector3 _rotation = Vector3.Zero;
    // Flag to indicate that our position has been updated.	
    private bool _isPositionChanged;
    // Flag to indicate that our scale has been updated.
    private bool _isScaleChanged;
    // Flag to inidcate that our rotation has been updated.
    private bool _isRotationChanged;

    /// <summary>
    /// Property to set or return the vertices for our object.
    /// </summary>
    protected Vertex[] Vertices
    {
        get;
        set;
    } = [];

    /// <summary>
    /// Property to set or return the indices for our object.
    /// </summary>
    protected ushort[] Indices
    {
        get;
        set;
    } = [];

    /// <summary>
    /// Property to return the number of vertices in the model.
    /// </summary>
    public int VertexCount => Vertices.Length;

    /// <summary>
    /// Property to return the number of indices in the model.
    /// </summary>
    public int IndexCount => Indices.Length;

    /// <summary>
    /// Property to return the index buffer for this object.
    /// </summary>
    public GorgonIndexBuffer? IndexBuffer
    {
        get;
        protected set;
    }

    /// <summary>
    /// Property to return the vertex buffer views for this model.
    /// </summary>
    public GorgonStructuredBufferView? VertexBufferView
    {
        get;
        protected set;
    }

    /// <summary>
    /// Property to return the world matrix for this model.
    /// </summary>
    public ref readonly Matrix4x4 WorldMatrix
    {
        get
        {
            UpdateTransform();
            return ref _worldMatrix;
        }
    }

    /// <summary>
    /// Property to set or return the material used on the model.
    /// </summary>
    public Material? Material
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the world position of the object.
    /// </summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            if (value.Equals(_position))
            {
                return;
            }

            _position = value;
            _isPositionChanged = true;
        }
    }

    /// <summary>
    /// Property to set or return the scale of the object.
    /// </summary>
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            if (value.Equals(_scale))
            {
                return;
            }

            _scale = value;
            _isScaleChanged = true;
        }
    }

    /// <summary>
    /// Property to set or return the rotation of the object, in degrees.
    /// </summary>
    public Vector3 Rotation
    {
        get => _rotation;
        set
        {
            if (value.Equals(_rotation))
            {
                return;
            }

            _rotation = value;
            _isRotationChanged = true;
        }
    }

    /// <summary>
    /// Function to update the transformation matrix of the plane.
    /// </summary>
    public void UpdateTransform()
    {
        // Don't recalculate unless we have changes.
        if ((!_isPositionChanged) && (!_isScaleChanged) && (!_isRotationChanged))
        {
            return;
        }

        if (_isPositionChanged)
        {
            _positionMatrix.SetTranslation(Position);
        }

        if (_isScaleChanged)
        {
            _scaleMatrix.M11 = Scale.X;
            _scaleMatrix.M22 = Scale.Y;
            _scaleMatrix.M33 = Scale.Z;
        }

        if (_isRotationChanged)
        {
            // Convert degrees to radians.
            Vector3 rotRads = new(_rotation.X.ToRadians(), _rotation.Y.ToRadians(), _rotation.Z.ToRadians());

            // Quaternion for rotation.
            Quaternion quatRotation = Quaternion.CreateFromYawPitchRoll(rotRads.Y, rotRads.X, rotRads.Z);
            _rotationMatrix = Matrix4x4.CreateFromQuaternion(quatRotation);
        }

        // Build our world matrix.
        Matrix4x4 temp = Matrix4x4.Multiply(_scaleMatrix, _rotationMatrix);
        _worldMatrix = Matrix4x4.Multiply(temp, _positionMatrix);
    }

    /// <summary>
    /// Function to upload the vertex/index data into the GPU
    /// </summary>
    /// <param name="list">The command list to use.</param>
    public void Upload(GorgonCommandList list)
    {
        Debug.Assert(VertexBufferView is not null, "No vertex buffer!");
        Debug.Assert(IndexBuffer is not null, "No index buffer!");

        list.CopyRange(Vertices, VertexBufferView.Buffer)
            .CopyRange(Indices, IndexBuffer);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        VertexBufferView?.Dispose();
        IndexBuffer?.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Model" /> class.
    /// </summary>
    protected Model()
    {
    }
}
