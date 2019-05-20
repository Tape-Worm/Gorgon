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
// Created: Monday, August 11, 2014 10:22:49 PM
// 
#endregion

using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// Defines a mesh that can be transformed.
    /// </summary>
    internal abstract class MoveableMesh
        : Mesh
    {
        #region Variables.
        // Position of the triangle.
        private DX.Vector3 _position = DX.Vector3.Zero;

        // Angle of the triangle.
        private DX.Vector3 _rotation = DX.Vector3.Zero;

        // Scale of the triangle.
        private DX.Vector3 _scale = new DX.Vector3(1);

        // Cached position matrix.
        private DX.Matrix _posMatrix = DX.Matrix.Identity;

        // Cached rotation matrix.
        private DX.Matrix _rotMatrix = DX.Matrix.Identity;

        // Cached scale matrix.
        private DX.Matrix _scaleMatrix = DX.Matrix.Identity;

        // Cached world matrix.
        private DX.Matrix _world = DX.Matrix.Identity;

        // Flags to indicate that the object needs to update its transform.
        private bool _needsPosTransform;

        private bool _needsSclTransform;
        private bool _needsRotTransform;
        private bool _needsWorldUpdate;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the current position of the triangle.
        /// </summary>
        public DX.Vector3 Position
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
        public DX.Vector3 Rotation
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
        public DX.Vector3 Scale
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
        public ref DX.Matrix WorldMatrix
        {
            get
            {
                if (!_needsWorldUpdate)
                {
                    return ref _world;
                }

                if (_needsPosTransform)
                {
                    DX.Matrix.Translation(ref _position, out _posMatrix);
                    _needsPosTransform = false;
                }

                if (_needsRotTransform)
                {
                    var rads = new DX.Vector3(_rotation.X.ToRadians(), _rotation.Y.ToRadians(), _rotation.Z.ToRadians());
                    DX.Matrix.RotationYawPitchRoll(rads.Y, rads.X, rads.Z, out _rotMatrix);
                    _needsRotTransform = false;
                }

                if (_needsSclTransform)
                {
                    DX.Matrix.Scaling(ref _scale, out _scaleMatrix);
                    _needsSclTransform = false;
                }


                DX.Matrix.Multiply(ref _rotMatrix, ref _scaleMatrix, out _world);
                DX.Matrix.Multiply(ref _world, ref _posMatrix, out _world);

                return ref _world;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the 2D axis aligned bounding box for the mesh.
        /// </summary>
        /// <returns>The rectangle that represents a 2D axis aligned bounding box.</returns>
        public virtual DX.RectangleF GetAABB() => DX.RectangleF.Empty;
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveableMesh" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this object.</param>
        protected MoveableMesh(GorgonGraphics graphics)
            : base(graphics)
        {
        }
        #endregion
    }
}
