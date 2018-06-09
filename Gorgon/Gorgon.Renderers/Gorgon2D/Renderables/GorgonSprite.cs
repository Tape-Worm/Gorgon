#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 7, 2018 3:13:51 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A class that defines a rectangluar region to display a 2D image.
    /// </summary>
    public class GorgonSprite
    {
        #region Variables.
        // Flag to indicate that the vertices for the sprite need updating.
        private bool _verticesChanged = true;
        // Flag to indicate that the vertices need transformation.
        private bool _transformChanged = true;
        // Flag to indicate that the texture coordinates have been changed.
        private bool _uvsChanged = true;
        // Rectangle bounds for the sprite.
        private DX.RectangleF _bounds;
        // The corners for the sprite rectangle.
        private DX.Vector4 _corners;
        // The axis point for the sprite.
        private DX.Vector2 _axis;
        // The scale of the sprite.
        private DX.Vector2 _scale = DX.Vector2.One;
        // The angle of rotation, in degrees.
        // This value is shown and input by the user.
        private float _angle;
        // This value is used internally for rotation calculations.
        private float _angleRads;
        // Cached sine for the angle.
        private float _angleCachedSin;
        // Cached cosine for the angle.
        private float _angleCachedCos;
        // The index of the array item in a texture array to use.
        private int _textureIndex;
        // The region of the texture to draw.
        private DX.RectangleF _textureRegion = new DX.RectangleF(0, 0, 1, 1);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the renderable associated with the sprite data.
        /// </summary>
        internal BatchRenderable Renderable
        {
            get;
        } = new BatchRenderable();

        /// <summary>
        /// Property to indicate that the sprite needs to be updated before rendering.
        /// </summary>
        internal bool NeedsUpdate => _transformChanged || _verticesChanged || CornerColors.HasChanged || _uvsChanged;

        /// <summary>
        /// Property to return the colors for each corner of the sprite.
        /// </summary>
        /// <remarks>
        /// The individual values for the corner colors can be set through this interface.
        /// </remarks>
        public GorgonRectangleColors CornerColors
        {
            get;
        } = new GorgonRectangleColors(GorgonColor.White);

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get => Renderable.Texture;
            set => Renderable.Texture = value;
        }

        /// <summary>
        /// Property to set or return the texture sampler to use when rendering.
        /// </summary>
        public GorgonSamplerState TextureSampler
        {
            get => Renderable.TextureSampler;
            set => Renderable.TextureSampler = value;
        }

        /// <summary>
        /// Property to set or return the boundaries of the sprite.
        /// </summary>
        public DX.RectangleF Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds.Equals(ref value))
                {
                    return;
                }

                _bounds = value;
                _transformChanged = true;
                _verticesChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the position of the sprite.
        /// </summary>
        public DX.Vector2 Position
        {
            get => Bounds.TopLeft;
            set
            {
                if (Bounds.TopLeft.Equals(ref value))
                {
                    return;
                }

                _bounds = new DX.RectangleF(value.X, value.Y, Bounds.Width, Bounds.Height);
                _transformChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the point around which the sprite will pivot when rotated.
        /// </summary>
        /// <remarks>
        /// This value is a relative value where 0, 0 means the upper left of the sprite, and 1, 1 means the lower right.
        /// </remarks>
        public DX.Vector2 Anchor
        {
            get => _axis;
            set
            {
                if (_axis.Equals(ref value))
                {
                    return;
                }

                _axis = value;
                _verticesChanged = true;
            }
        }

        /// <summary>
        /// Property to set or reeturn the size of the sprite.
        /// </summary>
        public DX.Size2F Size
        {
            get => Bounds.Size;
            set
            {
                if (_bounds.Size.Equals(value))
                {
                    return;
                }

                _bounds = new DX.RectangleF(Bounds.X, Bounds.Y, value.Width, value.Height);
                _verticesChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the region of the texture to use when drawing the sprite.
        /// </summary>
        /// <remarks>
        /// These values are in texel coordinates.
        /// </remarks>
        public DX.RectangleF TextureRegion
        {
            get => _textureRegion;
            set
            {
                if (_textureRegion.Equals(ref value))
                {
                    return;
                }

                _textureRegion = value;
                _uvsChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return which index within a texture array to use.
        /// </summary>
        public int TextureArrayIndex
        {
            get => _textureIndex;
            set
            {
                if (_textureIndex == value)
                {
                    return;
                }

                _textureIndex = value;
                _uvsChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the scale factor to apply to the sprite.
        /// </summary>
        public DX.Vector2 Scale
        {
            get => _scale;
            set
            {
                if (_scale.Equals(ref value))
                {
                    return;
                }

                _scale = value;
                _transformChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the angle of rotation, in degrees.
        /// </summary>
        public float Angle
        {
            get => _angle;
            set
            {
                if (_angle.EqualsEpsilon(value))
                {
                    return;
                }

                _angle = value;
                _angleRads = value.ToRadians();
                _angleCachedSin = _angleRads.FastSin();
                _angleCachedCos = _angleRads.FastCos();
                _transformChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the alpha testing values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Alpha testing will skip rendering pixels based on the current alpha value for the pixel if it falls into the range specified by this property.
        /// </para>
        /// <para>
        /// To disable alpha testing outright, set this property to <b>null</b>.
        /// </para>
        /// </remarks>
        public GorgonRangeF? AlphaTest
        {
            get
            {
                if (Renderable.AlphaTestData.IsEnabled == 0)
                {
                    return null;
                }

                return new GorgonRangeF(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha);
            }
            set
            {
                if (value == null)
                {
                    if (Renderable.AlphaTestData.IsEnabled != 0)
                    {
                        Renderable.AlphaTestData = new AlphaTestData(false, new GorgonRangeF(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha));
                    }
                    return;
                }

                Renderable.AlphaTestData = new AlphaTestData(true, value.Value);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the sprite vertices.
        /// </summary>
        private void BuildSprite()
        {
            DX.Vector2 vectorSize = new DX.Vector2(Size.Width, Size.Height);
            DX.Vector2 axisOffset = default;

            if (!_axis.IsZero)
            {
                DX.Vector2.Multiply(ref _axis, ref vectorSize, out axisOffset);
            }

            _corners = new DX.Vector4(-axisOffset.X, -axisOffset.Y, vectorSize.X - axisOffset.X, vectorSize.Y - axisOffset.Y);

            _verticesChanged = false;
            // If we've updated the physical dimensions for the sprite, then we need to update the transform as well.
            _transformChanged = true;
        }

        /// <summary>
        /// Function to update the colors for each corner of the sprite.
        /// </summary>
        private void UpdateVertexColors()
        {
            Renderable.Vertices[0].Color = CornerColors.UpperLeft;
            Renderable.Vertices[1].Color = CornerColors.UpperRight;
            Renderable.Vertices[2].Color = CornerColors.LowerLeft;
            Renderable.Vertices[3].Color = CornerColors.LowerRight;

            CornerColors.HasChanged = false;
        }

        /// <summary>
        /// Function to update the texture coordinates for the sprite.
        /// </summary>
        private void UpdateTextureCoordinates()
        {
            Renderable.Vertices[0].UV = new DX.Vector3(TextureRegion.Left, TextureRegion.Top, _textureIndex);
            Renderable.Vertices[1].UV = new DX.Vector3(TextureRegion.Right, TextureRegion.Top, _textureIndex);
            Renderable.Vertices[2].UV = new DX.Vector3(TextureRegion.Left, TextureRegion.Bottom, _textureIndex);
            Renderable.Vertices[3].UV = new DX.Vector3(TextureRegion.Right, TextureRegion.Bottom, _textureIndex);

            _uvsChanged = false;
        }

        /// <summary>
        /// Function to transform each vertex of the sprite to change its location, size and rotation.
        /// </summary>
        private void TransformVertices()
        {
            DX.Vector4 corners = _corners;

            if ((!_scale.X.EqualsEpsilon(1.0f)) || (!_scale.Y.EqualsEpsilon(1.0f)))
            {
                DX.Vector4 scale = new DX.Vector4(_scale.X, _scale.Y, _scale.X, _scale.Y);
                DX.Vector4.Multiply(ref corners, ref scale, out corners);
            }

            if (!_angle.EqualsEpsilon(0.0f))
            {
                Renderable.Vertices[0].Position.X = (corners.X * _angleCachedCos - corners.Y * _angleCachedSin) + _bounds.X;// + sprite.CornerOffsets.UpperLeft.X;
                Renderable.Vertices[0].Position.Y = (corners.X * _angleCachedSin + corners.Y * _angleCachedCos) + _bounds.Y;// + sprite.CornerOffsets.UpperLeft.Y;
                Renderable.Vertices[0].Angle = _angleRads;

                Renderable.Vertices[1].Position.X = (corners.Z * _angleCachedCos - corners.Y * _angleCachedSin) + _bounds.X;// + sprite.CornerOffsets.UpperRight.X;
                Renderable.Vertices[1].Position.Y = (corners.Z * _angleCachedSin + corners.Y * _angleCachedCos) + _bounds.Y;// + sprite.CornerOffsets.UpperRight.Y;
                Renderable.Vertices[1].Angle = _angleRads;

                Renderable.Vertices[2].Position.X = (corners.X * _angleCachedCos - corners.W * _angleCachedSin) + _bounds.X;// + sprite.CornerOffsets.LowerLeft.X;
                Renderable.Vertices[2].Position.Y = (corners.X * _angleCachedSin + corners.W * _angleCachedCos) + _bounds.Y;// + sprite.CornerOffsets.LowerLeft.Y;
                Renderable.Vertices[2].Angle = _angleRads;

                Renderable.Vertices[3].Position.X = (corners.Z * _angleCachedCos - corners.W * _angleCachedSin) + _bounds.X;// + sprite.CornerOffsets.LowerRight.X;
                Renderable.Vertices[3].Position.Y = (corners.Z * _angleCachedSin + corners.W * _angleCachedCos) + _bounds.Y;// + sprite.CornerOffsets.LowerRight.Y;
                Renderable.Vertices[3].Angle = _angleRads;
            }
            else
            {
                Renderable.Vertices[0].Position.X = corners.X + _bounds.X;
                Renderable.Vertices[0].Position.Y = corners.Y + _bounds.Y;
                Renderable.Vertices[0].Angle = 0.0f;
                Renderable.Vertices[1].Position.X = corners.Z + _bounds.X;
                Renderable.Vertices[1].Position.Y = corners.Y + _bounds.Y;
                Renderable.Vertices[1].Angle = 0.0f;
                Renderable.Vertices[2].Position.X = corners.X + _bounds.X;
                Renderable.Vertices[2].Position.Y = corners.W + _bounds.Y;
                Renderable.Vertices[2].Angle = 0.0f;
                Renderable.Vertices[3].Position.X = corners.Z + _bounds.X;
                Renderable.Vertices[3].Position.Y = corners.W + _bounds.Y;
                Renderable.Vertices[3].Angle = 0.0f;
            }

            _transformChanged = false;
        }

        /// <summary>
        /// Function to update the sprite vertex data.
        /// </summary>
        internal void UpdateSprite()
        {
            if (_verticesChanged)
            {
                BuildSprite();
            }

            if (_transformChanged)
            {
                TransformVertices();
            }

            if (CornerColors.HasChanged)
            {
                UpdateVertexColors();
            }

            if (!_uvsChanged)
            {
                return;
            }

            UpdateTextureCoordinates();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSprite"/> class.
        /// </summary>
        public GorgonSprite()
        {
            Renderable.Vertices = new Gorgon2DVertex[4];

            for (int i = 0; i < Renderable.Vertices.Length; ++i)
            {
                Renderable.Vertices[i].Position.W = 1.0f;
            }
        }
        #endregion
    }
}
