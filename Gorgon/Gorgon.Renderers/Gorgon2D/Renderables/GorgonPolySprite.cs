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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A class that defines a polygonal region to display a 2D image.
    /// </summary>
    public class GorgonPolySprite
        : IDisposable
    {
        #region Variables.
        // The angle of rotation, in degrees.
        private float _angle;

        // The renderable data for this sprite.
        // It is exposed as an internal variable (which goes against C# best practices) for performance reasons (property accesses add up over time).
        internal PolySpriteRenderable Renderable = new PolySpriteRenderable
                                                   {
                                                       WorldMatrix = DX.Matrix.Identity,
                                                       TextureTransform = new DX.Vector4(0, 0, 1, 1)
                                                   };
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the read/write list of vertices for the poly sprite.
        /// </summary>
        [JsonProperty("verts")]
        internal List<GorgonPolySpriteVertex> RwVertices
        {
            get;
        } = new List<GorgonPolySpriteVertex>(256);

        /// <summary>
        /// Property to set or return the list of vertices used by the poly sprite.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<GorgonPolySpriteVertex> Vertices => RwVertices;

        /// <summary>
        /// Property to return whether or not the sprite has had its position, size, texture information, or object space vertices updated since it was last drawn.
        /// </summary>
        [JsonIgnore]
        public bool IsUpdated => Renderable.HasTextureChanges 
                                 || Renderable.HasTransformChanges 
                                 || Renderable.HasVertexChanges 
                                 || Renderable.HasVertexColorChanges;

        /// <summary>
        /// Property to set or return the color of the sprite.
        /// </summary>
        public GorgonColor Color
        {
            get => Renderable.UpperLeftColor;
            set => Renderable.UpperLeftColor = Renderable.LowerLeftColor = Renderable.UpperRightColor = Renderable.LowerRightColor = value;
        }

        /// <summary>
        /// Property to set or return the texture array index for the sprite.
        /// </summary>
        public int TextureArrayIndex
        {
            get => Renderable.TextureArrayIndex;
            set
            {
                if (Renderable.TextureArrayIndex == value)
                {
                    return;
                }

                Renderable.TextureArrayIndex = value;
                Renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get => Renderable.Texture;
            set
            {
                if (Renderable.Texture == value)
                {
                    return;
                }

                Renderable.Texture = value;
                Renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset to apply to a texture.
        /// </summary>
        public DX.Vector2 TextureOffset
        {
            get => (DX.Vector2)Renderable.TextureTransform;
            set
            {
                if ((Renderable.TextureTransform.X == value.X)
                    && (Renderable.TextureTransform.Y == value.Y))
                {
                    return;
                }

                Renderable.TextureTransform = new DX.Vector4(value.X, value.Y, Renderable.TextureTransform.Z, Renderable.TextureTransform.W);
                Renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the scale to apply to a texture.
        /// </summary>
        public DX.Vector2 TextureScale
        {
            get => new DX.Vector2(Renderable.TextureTransform.Z, Renderable.TextureTransform.W);
            set
            {
                if ((Renderable.TextureTransform.Z == value.X)
                    && (Renderable.TextureTransform.W == value.Y))
                {
                    return;
                }

                Renderable.TextureTransform = new DX.Vector4(Renderable.TextureTransform.X, Renderable.TextureTransform.Y, value.X, value.Y);
                Renderable.HasTextureChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the texture sampler to use when rendering.
        /// </summary>
        public GorgonSamplerState TextureSampler
        {
            get => Renderable.TextureSampler;
            set
            {
                if (Renderable.TextureSampler == value)
                {
                    return;
                }

                Renderable.TextureSampler = value;
                Renderable.StateChanged = true;
            }
        }

        /// <summary>
        /// Property to return the boundaries of the sprite.
        /// </summary>
        [JsonIgnore]
        public DX.RectangleF Bounds
        {
            get => Renderable.Bounds;
            internal set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;

                if ((bounds.Left == value.Left) 
                    && (bounds.Right == value.Right)
                    && (bounds.Top == value.Top)
                    && (bounds.Bottom == value.Bottom))
                {
                    return;
                }

                bounds = value;
            }
        }

        /// <summary>
        /// Property to set or return the position of the sprite.
        /// </summary>
        [JsonIgnore]
        public DX.Vector2 Position
        {
            get => Renderable.Bounds.TopLeft;
            set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                if ((bounds.X == value.X)
                    && (bounds.Y == value.Y))
                {
                    return;
                }

                bounds.X = value.X;
                bounds.Y = value.Y;

                Renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the depth value for this sprite.
        /// </summary>
        [JsonIgnore]
        public float Depth
        {
            get => Renderable.Depth;
            set
            {
                if (Renderable.Depth.EqualsEpsilon(value))
                {
                    return;
                }

                Renderable.Depth = value;
                Renderable.HasTransformChanges = true;
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
            get => Renderable.Anchor;
            set
            {
                ref DX.Vector2 anchor = ref Renderable.Anchor;
                if ((anchor.X == value.X)
                    && (anchor.Y == value.Y))
                {
                    return;
                }

                anchor = value;
                Renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to return the size of the sprite.
        /// </summary>
        [JsonIgnore]
        public DX.Size2F Size => Bounds.Size;

        /// <summary>
        /// Property to set or return the size of the renderable after scaling has been applied.
        /// </summary>
        /// <remarks>
        /// This property will set or return the actual size of the renderable.  This means that if a <see cref="Scale"/> has been set, then this property will return the size of the renderable with
        /// multiplied by the scale.  When assigning a value, the scale be set on value derived from the current size of the renderable.
        /// </remarks>
        [JsonIgnore]
        public DX.Size2F ScaledSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                ref DX.Vector2 scale = ref Renderable.Scale;
                return new DX.Size2F(scale.X * bounds.Width, scale.Y * bounds.Height);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ref DX.RectangleF bounds = ref Renderable.Bounds;
                ref DX.Vector2 scale = ref Renderable.Scale;
                scale = new DX.Vector2(value.Width / bounds.Width, value.Height / bounds.Height);
                Renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the scale factor to apply to the sprite.
        /// </summary>
        [JsonIgnore]
        public DX.Vector2 Scale
        {
            get => Renderable.Scale;
            set
            {
                ref DX.Vector2 scale = ref Renderable.Scale;
                if ((scale.X == value.X)
                    && (scale.Y == value.Y))
                {
                    return;
                }

                scale = value;

                ref DX.Matrix matrix = ref Renderable.WorldMatrix;
                matrix.M11 = scale.X;
                matrix.M22 = scale.Y;
                Renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the angle of rotation, in degrees.
        /// </summary>
        [JsonIgnore]
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
                Renderable.AngleRads = rads;
                Renderable.AngleSin = rads.FastSin();
                Renderable.AngleCos = rads.FastCos();



                Renderable.HasTransformChanges = true;
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
            get => Renderable.AlphaTestData.IsEnabled == 0
                    ? null
                    : (GorgonRangeF?)new GorgonRangeF(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha);
            set
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (value == null)
                {
                    if (Renderable.AlphaTestData.IsEnabled == 0)
                    {
                        return;
                    }

                    Renderable.AlphaTestData = new AlphaTestData(false, new GorgonRangeF(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha));
                    Renderable.StateChanged = true;
                    return;
                }

                Renderable.AlphaTestData = new AlphaTestData(true, value.Value);
                Renderable.StateChanged = true;
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
            get => Renderable.HorizontalFlip;
            set
            {
                if (value == Renderable.HorizontalFlip)
                {
                    return;
                }

                Renderable.HorizontalFlip = value;
                Renderable.HasTextureChanges = true;
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
            get => Renderable.VerticalFlip;
            set
            {
                if (value == VerticalFlip)
                {
                    return;
                }

                Renderable.VerticalFlip = value;
                Renderable.HasTextureChanges = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            PolySpriteRenderable renderable = Interlocked.Exchange(ref Renderable, null);
            renderable?.VertexBuffer.VertexBuffer?.Dispose();
            renderable?.IndexBuffer?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPolySprite"/> class.
        /// </summary>
        internal GorgonPolySprite()
        {
        }
        #endregion
    }
}
