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

using System.Runtime.CompilerServices;
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
        // The corners for the sprite rectangle.
        private DX.Vector4 _corners;
        // The angle of rotation, in degrees.
        private float _angle;
        // The renderable.
        // We have it outside of the property as a performance improvement (and it's quite noticable).
        private readonly BatchRenderable _renderable = new BatchRenderable();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the renderable associated with the sprite data.
        /// </summary>
        internal BatchRenderable Renderable => _renderable;

        /// <summary>
        /// Property to return whether or not the sprite has had its position, size, texture information, or object space vertices updated since it was last drawn.
        /// </summary>
        public bool IsUpdated => _renderable.HasTextureChanges || _renderable.HasTransformChanges || _renderable.HasVertexChanges ||
                                 _renderable.RectangleColors.HasChanged;

        /// <summary>
        /// Property to return the interface that allows colors to be assigned to each corner of the sprite.
        /// </summary>
        public GorgonRectangleColors CornerColors => _renderable.RectangleColors;

        /// <summary>
        /// Property to set or return the color of the sprite.
        /// </summary>
        /// <remarks>
        /// This sets the color for the entire sprite.  To assign colors to each corner of the sprite, use the <see cref="CornerColors"/> property.
        /// </remarks>
        public GorgonColor Color
        {
            get => _renderable.RectangleColors.UpperLeft;
            set => _renderable.RectangleColors.SetAll(in value);
        }

        /// <summary>
        /// Property to return the interface that allows an offset to be applied to each corner of the sprite.
        /// </summary>
        public GorgonRectangleOffsets CornerOffsets => _renderable.RectangleOffsets;

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get => _renderable.Texture;
            set
            {
                if (_renderable.Texture == value)
                {
                    return;
                }

                _renderable.Texture = value;
                _renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the texture sampler to use when rendering.
        /// </summary>
        public GorgonSamplerState TextureSampler
        {
            get => _renderable.TextureSampler;
            set
            {
                if (_renderable.TextureSampler == value)
                {
                    return;
                }

                _renderable.TextureSampler = value;
                _renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the boundaries of the sprite.
        /// </summary>
        public DX.RectangleF Bounds
        {
            get => _renderable.Bounds;
            set
            {
                ref DX.RectangleF bounds = ref _renderable.Bounds;

                if ((bounds.Left == value.Left) 
                    && (bounds.Right == value.Right)
                    && (bounds.Top == value.Top)
                    && (bounds.Bottom == value.Bottom))
                {
                    return;
                }

                bounds = value;
                _renderable.HasVertexChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the position of the sprite.
        /// </summary>
        public DX.Vector2 Position
        {
            get => _renderable.Bounds.TopLeft;
            set
            {
                ref DX.RectangleF bounds = ref _renderable.Bounds;
                if ((bounds.X == value.X)
                    && (bounds.Y == value.Y))
                {
                    return;
                }

                bounds.X = value.X;
                bounds.Y = value.Y;
                _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the depth value for this sprite.
        /// </summary>
        public float Depth
        {
            get => _renderable.Depth;
            set
            {
                if (_renderable.Depth.EqualsEpsilon(value))
                {
                    return;
                }

                _renderable.Depth = value;
                _renderable.HasTransformChanges = true;
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
            get => _renderable.Anchor;
            set
            {
                ref DX.Vector2 anchor = ref _renderable.Anchor;
                if ((anchor.X == value.X)
                    && (anchor.Y == value.Y))
                {
                    return;
                }

                anchor = value;
                _renderable.HasVertexChanges = true;
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
                ref DX.RectangleF bounds = ref _renderable.Bounds;
                if ((bounds.Size.Width == value.Width)
                    && (bounds.Size.Height == value.Height))
                {
                    return;
                }

                bounds = new DX.RectangleF(Bounds.X, Bounds.Y, value.Width, value.Height);
                _renderable.HasVertexChanges = true;
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
            get => _renderable.TextureRegion;
            set
            {
                ref DX.RectangleF region = ref _renderable.TextureRegion;
                if ((region.Left == value.Left)
                    && (region.Top == value.Top)
                    && (region.Right == value.Right)
                    && (region.Bottom == value.Bottom))
                {
                    return;
                }

                region = value;
                _renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return which index within a texture array to use.
        /// </summary>
        public int TextureArrayIndex
        {
            get => _renderable.TextureArrayIndex;
            set
            {
                if (_renderable.TextureArrayIndex == value)
                {
                    return;
                }

                _renderable.TextureArrayIndex = value;
                _renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the size of the renderable after scaling has been applied.
        /// </summary>
        /// <remarks>
        /// This property will set or return the actual size of the renderable.  This means that if a <see cref="Scale"/> has been set, then this property will return the size of the renderable with
        /// multiplied by the scale.  When assigning a value, the scale be set on value derived from the current size of the renderable.
        /// </remarks>
        public DX.Vector2 ScaledSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref DX.RectangleF bounds = ref _renderable.Bounds;
                ref DX.Vector2 scale = ref _renderable.Scale;
                return new DX.Vector2(scale.X * bounds.Width, scale.Y * bounds.Height);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ref DX.RectangleF bounds = ref _renderable.Bounds;
                ref DX.Vector2 scale = ref _renderable.Scale;
                scale = new DX.Vector2(value.X / bounds.Width, value.Y / bounds.Height);
                _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the scale factor to apply to the sprite.
        /// </summary>
        public DX.Vector2 Scale
        {
            get => _renderable.Scale;
            set
            {
                ref DX.Vector2 scale = ref _renderable.Scale;
                if ((scale.X == value.X)
                    && (scale.Y == value.Y))
                {
                    return;
                }

                scale = value;
                _renderable.HasTransformChanges = true;
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
                if (_angle == value)
                {
                    return;
                }

                _angle = value;
                float rads = value.ToRadians();
                _renderable.AngleRads = rads;
                _renderable.AngleSin = rads.FastSin();
                _renderable.AngleCos = rads.FastCos();
                _renderable.HasTransformChanges = true;
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
                if (_renderable.AlphaTestData.IsEnabled == 0)
                {
                    return null;
                }

                return new GorgonRangeF(_renderable.AlphaTestData.LowerAlpha, _renderable.AlphaTestData.UpperAlpha);
            }
            set
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (value == null)
                {
                    if (_renderable.AlphaTestData.IsEnabled == 0)
                    {
                        return;
                    }

                    _renderable.AlphaTestData = new AlphaTestData(false, new GorgonRangeF(_renderable.AlphaTestData.LowerAlpha, _renderable.AlphaTestData.UpperAlpha));
                    _renderable.StateChanged = true;
                    return;
                }

                _renderable.AlphaTestData = new AlphaTestData(true, value.Value);
                _renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return whether the sprite texture is flipped horizontally.
        /// </summary>
        /// <remarks>
        /// This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.
        /// </remarks>
        public bool HorizontalFlip
        {
            get => _renderable.HorizontalFlip;
            set
            {
                if (value == _renderable.HorizontalFlip)
                {
                    return;
                }

                HorizontalFlip = value;
                _renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return whether the sprite texture is flipped vertically.
        /// </summary>
        /// <remarks>
        /// This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.
        /// </remarks>
        public bool VerticalFlip
        {
            get => _renderable.VerticalFlip;
            set
            {
                if (value == VerticalFlip)
                {
                    return;
                }

                VerticalFlip = value;
                _renderable.HasTextureChanges = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the sprite vertices.
        /// </summary>
        private void BuildSprite()
        {
            ref DX.RectangleF bounds = ref _renderable.Bounds;
            DX.Vector2 vectorSize = new DX.Vector2(bounds.Size.Width, bounds.Size.Height);
            DX.Vector2 axisOffset = default;

            ref DX.Vector2 anchor = ref _renderable.Anchor;
            if (!anchor.IsZero)
            {
                DX.Vector2.Multiply(ref anchor, ref vectorSize, out axisOffset);
            }

            _corners = new DX.Vector4(-axisOffset.X, -axisOffset.Y, vectorSize.X - axisOffset.X, vectorSize.Y - axisOffset.Y);

            _renderable.HasVertexChanges = false;
            // If we've updated the physical dimensions for the sprite, then we need to update the transform as well.
            _renderable.HasTransformChanges = true;
        }

        /// <summary>
        /// Function to update the colors for each corner of the sprite.
        /// </summary>
        private void UpdateVertexColors()
        {
            _renderable.Vertices[0].Color = CornerColors.UpperLeft;
            _renderable.Vertices[1].Color = CornerColors.UpperRight;
            _renderable.Vertices[2].Color = CornerColors.LowerLeft;
            _renderable.Vertices[3].Color = CornerColors.LowerRight;

            _renderable.RectangleColors.HasChanged = false;
        }

        /// <summary>
        /// Function to update the texture coordinates for the sprite.
        /// </summary>
        private void UpdateTextureCoordinates()
        {
            // Calculate texture coordinates.
            ref DX.RectangleF textureRegion = ref _renderable.TextureRegion;

            var rightBottom = new DX.Vector3(textureRegion.BottomRight, _renderable.TextureArrayIndex);
            var leftTop = new DX.Vector3(textureRegion.TopLeft, _renderable.TextureArrayIndex);

            if (_renderable.HorizontalFlip)
            {
                leftTop.X = TextureRegion.Right;
                rightBottom.X = TextureRegion.Left;
            }

            if (_renderable.VerticalFlip)
            {
                leftTop.Y = TextureRegion.Bottom;
                rightBottom.Y = TextureRegion.Top;
            }

            _renderable.Vertices[0].UV = leftTop;
            _renderable.Vertices[1].UV = new DX.Vector3(rightBottom.X, leftTop.Y, _renderable.TextureArrayIndex);
            _renderable.Vertices[2].UV = new DX.Vector3(leftTop.X, rightBottom.Y, _renderable.TextureArrayIndex);
            _renderable.Vertices[3].UV = rightBottom;

            _renderable.HasTextureChanges = false;
        }

        /// <summary>
        /// Function to transform each vertex of the sprite to change its location, size and rotation.
        /// </summary>
        private void TransformVertices()
        {
            ref DX.RectangleF bounds = ref _renderable.Bounds;
            ref DX.Vector2 renderableScale = ref _renderable.Scale;

            if ((renderableScale.X != 1.0f) || (renderableScale.Y != 1.0f))
            {
                var scale = new DX.Vector4(renderableScale.X, renderableScale.Y, renderableScale.X, renderableScale.Y);
                DX.Vector4.Multiply(ref _corners, ref scale, out _corners);
            }

            Gorgon2DVertex[] vertices = _renderable.Vertices;
            ref Gorgon2DVertex v1 = ref vertices[0];
            ref Gorgon2DVertex v2 = ref vertices[1];
            ref Gorgon2DVertex v3 = ref vertices[2];
            ref Gorgon2DVertex v4 = ref vertices[3];
            float depth = _renderable.Depth;
            GorgonRectangleOffsets cornerOffsets = _renderable.RectangleOffsets;
            DX.Vector3 cornerUpperLeft = cornerOffsets.UpperLeft;
            DX.Vector3 cornerUpperRight = cornerOffsets.UpperRight;
            DX.Vector3 cornerLowerLeft = cornerOffsets.LowerLeft;
            DX.Vector3 cornerLowerRight = cornerOffsets.LowerRight;

            if (_angle != 0.0f)
            {
                float angleRads = _renderable.AngleRads;
                float angleSin = _renderable.AngleSin;
                float angleCos = _renderable.AngleCos;

                v1.Position.X = (_corners.X * angleCos - _corners.Y * angleSin) + bounds.X + cornerUpperLeft.X;
                v1.Position.Y = (_corners.X * angleSin + _corners.Y * angleCos) + bounds.Y + cornerUpperLeft.Y;
                v1.Position.Z = depth + cornerUpperLeft.Z;
                v1.Angle = angleRads;

                v2.Position.X = (_corners.Z * angleCos - _corners.Y * angleSin) + bounds.X + cornerUpperRight.X;
                v2.Position.Y = (_corners.Z * angleSin + _corners.Y * angleCos) + bounds.Y + cornerUpperRight.Y;
                v2.Position.Z = depth + cornerUpperRight.Z;
                v2.Angle = angleRads;

                v3.Position.X = (_corners.X * angleCos - _corners.W * angleSin) + bounds.X + cornerLowerLeft.X;
                v3.Position.Y = (_corners.X * angleSin + _corners.W * angleCos) + bounds.Y + cornerLowerLeft.Y;
                v3.Position.Z = depth + cornerLowerLeft.Z;
                v3.Angle = angleRads;

                v4.Position.X = (_corners.Z * angleCos - _corners.W * angleSin) + bounds.X + cornerLowerRight.X;
                v4.Position.Y = (_corners.Z * angleSin + _corners.W * angleCos) + bounds.Y + cornerLowerRight.Y;
                v4.Position.Z = depth + cornerLowerRight.Z;
                v4.Angle = angleRads;
            }
            else
            {
                v1.Position.X = _corners.X + bounds.X + cornerUpperLeft.X;
                v1.Position.Y = _corners.Y + bounds.Y + cornerUpperLeft.Y;
                v1.Position.Z = depth + cornerUpperLeft.Z;
                v1.Angle = 0.0f;
                v2.Position.X = _corners.Z + bounds.X + cornerUpperRight.X;
                v2.Position.Y = _corners.Y + bounds.Y + cornerUpperRight.Y;
                v2.Position.Z = depth + cornerUpperRight.Z;
                v2.Angle = 0.0f;
                v3.Position.X = _corners.X + bounds.X + cornerLowerLeft.X;
                v3.Position.Y = _corners.W + bounds.Y + cornerLowerLeft.Y;
                v3.Position.Z = depth + cornerLowerLeft.Z;
                v3.Angle = 0.0f;
                v4.Position.X = _corners.Z + bounds.X + cornerLowerRight.X;
                v4.Position.Y = _corners.W + bounds.Y + cornerLowerRight.Y;
                v4.Position.Z = depth + cornerLowerRight.Z;
                v4.Angle = 0.0f;
            }

            _renderable.HasTransformChanges = false;
            cornerOffsets.HasChanged = false;
        }

        /// <summary>
        /// Function to update the sprite vertex data.
        /// </summary>
        internal void UpdateSprite()
        {
            if (_renderable.HasVertexChanges)
            {
                BuildSprite();
            }

            if (_renderable.HasTransformChanges)
            {
                TransformVertices();
            }

            if (_renderable.RectangleColors.HasChanged)
            {
                UpdateVertexColors();
            }

            if (!_renderable.HasTextureChanges)
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
            _renderable.Vertices = new Gorgon2DVertex[4];

            for (int i = 0; i < _renderable.Vertices.Length; ++i)
            {
                _renderable.Vertices[i].Position.W = 1.0f;
            }
        }
        #endregion
    }
}
