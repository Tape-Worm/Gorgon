
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
// Created: Monday, August 11, 2014 10:22:49 PM
// 


using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// Defines a mesh that can be transformed
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MoveableMesh" /> class
/// </remarks>
/// <param name="graphics">The graphics interface that owns this object.</param>
internal abstract class MoveableMesh(GorgonGraphics graphics)
        : Mesh(graphics)
{
    // Position of the triangle.
    private Vector3 _position = Vector3.Zero;

    // Angle of the triangle.
    private Vector3 _rotation = Vector3.Zero;

    // Scale of the triangle.
    private Vector3 _scale = new(1);

    // Cached position matrix.
    private Matrix4x4 _posMatrix = Matrix4x4.Identity;

    // Cached rotation matrix.
    private Matrix4x4 _rotMatrix = Matrix4x4.Identity;

    // Cached scale matrix.
    private Matrix4x4 _scaleMatrix = Matrix4x4.Identity;

    // Cached world matrix.
    private Matrix4x4 _world = Matrix4x4.Identity;

    // Flags to indicate that the object needs to update its transform.
    private bool _needsPosTransform;

    private bool _needsSclTransform;
    private bool _needsRotTransform;
    private bool _needsWorldUpdate;

    /// <summary>
    /// Property to set or return the current position of the triangle.
    /// </summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _needsPosTransform = true;
            _needsWorldUpdate = true;
        }
    }

    /// <summary>
    /// Property to set or return the angle of rotation for the triangle (in degrees).
    /// </summary>
    public Vector3 Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _needsRotTransform = true;
            _needsWorldUpdate = true;
        }
    }

    /// <summary>
    /// Property to set or return the scale of the transform.
    /// </summary>
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _needsSclTransform = true;
            _needsWorldUpdate = true;
        }
    }

    /// <summary>
    /// Property to return the world matrix for this triangle.
    /// </summary>
    public ref readonly Matrix4x4 WorldMatrix
    {
        get
        {
            if (!_needsWorldUpdate)
            {
                return ref _world;
            }

            if (_needsPosTransform)
            {
                _posMatrix = Matrix4x4.CreateTranslation(_position);
                _needsPosTransform = false;
            }

            if (_needsRotTransform)
            {
                _rotMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Y.ToRadians(), _rotation.X.ToRadians(), _rotation.Z.ToRadians());
                _needsRotTransform = false;
            }

            if (_needsSclTransform)
            {
                _scaleMatrix = Matrix4x4.CreateScale(_scale);
                _needsSclTransform = false;
            }

            _world = Matrix4x4.Multiply(_rotMatrix, _scaleMatrix);
            _world = Matrix4x4.Multiply(_world, _posMatrix);

            return ref _world;
        }
    }

    /// <summary>
    /// Function to retrieve the 2D axis aligned bounding box for the mesh.
    /// </summary>
    /// <returns>The rectangle that represents a 2D axis aligned bounding box.</returns>
    public virtual GorgonRectangleF GetAABB() => GorgonRectangleF.Empty;
}
