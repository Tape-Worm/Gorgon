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
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers.Properties;
using Newtonsoft.Json;
using DX = SharpDX;

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

        /// <summary>
        /// Function to build a <see cref="GorgonPolySprite"/> using predefined vertices and indices to define the polygonal sprite.
        /// </summary>
        /// <param name="renderer">The renderer interface used to build up the vertex and index buffer for the polygonal sprite.</param>
        /// <param name="vertices">The vertices for the polygonal sprite.</param>
        /// <param name="indices">The indices for the polygonal sprite.</param>
        /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, <paramref name="vertices"/>, or the <paramref name="indices"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="vertices"/>, or the <paramref name="indices"/> parameter does not contain any values.</exception>
        /// <remarks>
        /// <para>
        /// This method is used to build a <see cref="GorgonPolySprite"/> using already defined set of <see cref="GorgonPolySpriteVertex"/> vertices, and a set of indices. 
        /// </para>
        /// </remarks>
        public static GorgonPolySprite Create(Gorgon2D renderer, IReadOnlyList<GorgonPolySpriteVertex> vertices, IReadOnlyList<int> indices) =>
            Create(renderer?.Graphics ?? throw new ArgumentNullException(nameof(renderer)), vertices, indices);        

        /// <summary>
        /// Function to build a <see cref="GorgonPolySprite"/> using predefined vertices and indices to define the polygonal sprite.
        /// </summary>
        /// <param name="graphics">The graphics interface used to build up the vertex and index buffer for the polygonal sprite.</param>
        /// <param name="vertices">The vertices for the polygonal sprite.</param>
        /// <param name="indices">The indices for the polygonal sprite.</param>
        /// <returns>A new <see cref="GorgonPolySprite"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="vertices"/>, or the <paramref name="indices"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="vertices"/>, or the <paramref name="indices"/> parameter does not contain any values.</exception>
        /// <remarks>
        /// <para>
        /// This method is used to build a <see cref="GorgonPolySprite"/> using already defined set of <see cref="GorgonPolySpriteVertex"/> vertices, and a set of indices. 
        /// </para>
        /// </remarks>
        public static GorgonPolySprite Create(GorgonGraphics graphics, IReadOnlyList<GorgonPolySpriteVertex> vertices, IReadOnlyList<int> indices)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }

            if (indices == null)
            {
                throw new ArgumentNullException(nameof(indices));
            }


            if ((vertices.Count == 0) || (indices.Count == 0))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GOR2D_ERR_POLY_SPRITE_NOT_ENOUGH_VERTS);
            }

            var newSprite = new GorgonPolySprite();

            // Send the vertices into the sprite.
            newSprite.RwVertices.AddRange(vertices);
            newSprite.Renderable.ActualVertexCount = newSprite.RwVertices.Count;
            newSprite.Renderable.Vertices = new Gorgon2DVertex[newSprite.RwVertices.Count];

            for (int i = 0; i < newSprite.RwVertices.Count; ++i)
            {
                Gorgon2DVertex vertex = newSprite.RwVertices[i].Vertex;
                newSprite.Renderable.Vertices[i] = vertex;

                minX = minX.Min(vertex.Position.X);
                minY = minY.Min(vertex.Position.Y);
                maxX = maxX.Max(vertex.Position.X);
                maxY = maxY.Max(vertex.Position.Y);
            }

            // Split the polygon hull into triangles.            
            GorgonNativeBuffer<Gorgon2DVertex> vertexData = newSprite.Renderable.Vertices.ToNativeBuffer();
            var indexData = new GorgonNativeBuffer<int>(indices.Count);

            try
            {
                // Send the index data to the local memory buffer.
                for (int i = 0; i < indices.Count; ++i)
                {
                    ref int indexValue = ref indexData[i];
                    indexValue = indices[i];
                }

                newSprite.Renderable.IndexBuffer = new GorgonIndexBuffer(graphics, new GorgonIndexBufferInfo
                {
                    Binding = VertexIndexBufferBinding.None,
                    Use16BitIndices = false,
                    IndexCount = indexData.Length,
                    Usage = ResourceUsage.Immutable
                }, indexData);

                newSprite.Renderable.VertexBuffer = GorgonVertexBufferBinding.CreateVertexBuffer(graphics, new GorgonVertexBufferInfo
                {
                    Usage = ResourceUsage.Immutable,
                    Binding = VertexIndexBufferBinding.None,
                    SizeInBytes = vertexData.SizeInBytes
                }, vertexData);

                newSprite.Renderable.ActualVertexCount = newSprite.RwVertices.Count;
                newSprite.Renderable.IndexCount = indexData.Length;
                newSprite.Bounds = new DX.RectangleF
                {
                    Left = minX,
                    Top = minY,
                    Right = maxX,
                    Bottom = maxY
                };
            }
            finally
            {
                vertexData?.Dispose();
                indexData?.Dispose();
            }

            return newSprite;
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
