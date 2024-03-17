
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 7, 2018 3:13:51 PM
// 


using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;
using Newtonsoft.Json;
using DX = SharpDX;

namespace Gorgon.Renderers;

/// <summary>
/// A class that defines a rectangluar region to display a 2D image
/// </summary>
public class GorgonSprite
{
    // The absolute anchor position.
    private Vector2 _absoluteAnchor;

    // The renderable data for this sprite.
    // It is exposed as an internal variable (which goes against C# best practices) for performance reasons (property accesses add up over time).
    internal readonly BatchRenderable Renderable = new()
    {
        Vertices = new Gorgon2DVertex[4],
        ActualVertexCount = 4,
        IndexCount = 6
    };

    /// <summary>
    /// Property to return whether or not the sprite has had its position, size, texture information, or object space vertices updated since it was last drawn.
    /// </summary>
    [JsonIgnore]
    public bool IsUpdated => Renderable.HasTextureChanges
                                 || Renderable.HasTransformChanges
                                 || Renderable.HasVertexChanges;

    /// <summary>
    /// Property to return the interface that allows colors to be assigned to each corner of the sprite.
    /// </summary>
    public GorgonSpriteColors CornerColors
    {
        get;
    }

    /// <summary>
    /// Property to set or return the color of the sprite.
    /// </summary>
    /// <remarks>
    /// This sets the color for the entire sprite.  To assign colors to each corner of the sprite, use the <see cref="CornerColors"/> property.
    /// </remarks>
    [JsonIgnore]
    public GorgonColor Color
    {
        get => Renderable.Vertices[0].Color;
        set => CornerColors.SetAll(in value);
    }

    /// <summary>
    /// Property to return the interface that allows an offset to be applied to each corner of the sprite.
    /// </summary>
    public GorgonRectangleOffsets CornerOffsets
    {
        get;
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
    /// Property to set or return the boundaries of the sprite.
    /// </summary>
    [JsonIgnore]
    public DX.RectangleF Bounds
    {
        get => Renderable.Bounds;
        set
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
            Renderable.HasVertexChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the position of the sprite.
    /// </summary>
    [JsonIgnore]
    public Vector2 Position
    {
        get => new(Renderable.Bounds.Left, Renderable.Bounds.Top);
        set
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;
            if ((bounds.Left == value.X)
                && (bounds.Top == value.Y))
            {
                return;
            }

            bounds = new DX.RectangleF(value.X, value.Y, bounds.Width, bounds.Height);
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
    public Vector2 Anchor
    {
        get => Renderable.Anchor;
        set
        {
            ref Vector2 anchor = ref Renderable.Anchor;
            if ((anchor.X == value.X)
                && (anchor.Y == value.Y))
            {
                return;
            }

            ref Vector2 absAnchor = ref _absoluteAnchor;
            ref DX.RectangleF bounds = ref Renderable.Bounds;

            anchor = value;

            absAnchor.X = value.X * bounds.Width;
            absAnchor.Y = value.Y * bounds.Height;
            Renderable.HasVertexChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the absolute anchor position.
    /// </summary>
    /// <remarks>
    /// Unlike the <see cref="Anchor"/> property, this value is absolute from the upper left corner of the sprite to the lower right corner.
    /// </remarks>
    [JsonIgnore]
    public Vector2 AbsoluteAnchor
    {
        get => _absoluteAnchor;
        set
        {
            ref Vector2 absAnchor = ref _absoluteAnchor;
            if ((absAnchor.X == value.X)
                && (absAnchor.Y == value.Y))
            {
                return;
            }

            ref Vector2 anchor = ref Renderable.Anchor;
            ref DX.RectangleF bounds = ref Renderable.Bounds;

            absAnchor = value;

            anchor.X = value.X / bounds.Width;
            anchor.Y = value.Y / bounds.Height;
            Renderable.HasVertexChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the size of the sprite.
    /// </summary>
    public DX.Size2F Size
    {
        get => Bounds.Size;
        set
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;
            if ((bounds.Width == value.Width)
                && (bounds.Height == value.Height))
            {
                return;
            }

            bounds = new DX.RectangleF(bounds.Left, bounds.Top, value.Width, value.Height);
            Renderable.HasVertexChanges = true;
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
        get => Renderable.TextureRegion;
        set
        {
            ref DX.RectangleF region = ref Renderable.TextureRegion;
            if ((region.Left == value.Left)
                && (region.Top == value.Top)
                && (region.Right == value.Right)
                && (region.Bottom == value.Bottom))
            {
                return;
            }

            region = value;
            Renderable.HasTextureChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return which index within a texture array to use.
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
    /// Property to set or return the size of the renderable after scaling has been applied.
    /// </summary>
    /// <remarks>
    /// This property will set or return the actual size of the renderable.  This means that if a <see cref="Scale"/> has been set, then this property will return the size of the renderable with
    /// multiplied by the scale.  When assigning a value, the scale be set on value derived from the current size of the renderable.
    /// </remarks>
    [JsonIgnore]
    public DX.Size2F ScaledSize
    {
        get
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;
            ref Vector2 scale = ref Renderable.Scale;
            return new DX.Size2F(scale.X * bounds.Width, scale.Y * bounds.Height);
        }
        set
        {
            ref DX.RectangleF bounds = ref Renderable.Bounds;
            ref Vector2 scale = ref Renderable.Scale;
            scale = new Vector2(value.Width / bounds.Width, value.Height / bounds.Height);
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the scale factor to apply to the sprite.
    /// </summary>
    [JsonIgnore]
    public Vector2 Scale
    {
        get => Renderable.Scale;
        set
        {
            ref Vector2 scale = ref Renderable.Scale;
            if ((scale.X == value.X)
                && (scale.Y == value.Y))
            {
                return;
            }

            scale = value;
            Renderable.HasTransformChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the angle of rotation, in degrees.
    /// </summary>
    [JsonIgnore]
    public float Angle
    {
        get => Renderable.AngleDegs;
        set
        {
            if (Renderable.AngleDegs == value)
            {
                return;
            }

            Renderable.AngleDegs = value;
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
    public GorgonRange<float>? AlphaTest
    {
        get => Renderable.AlphaTestData.IsEnabled == 0
                ? null
                : new GorgonRange<float>(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha);
        set
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (value is null)
            {
                if (Renderable.AlphaTestData.IsEnabled == 0)
                {
                    return;
                }

                Renderable.AlphaTestData = new AlphaTestData(false, new GorgonRange<float>(Renderable.AlphaTestData.LowerAlpha, Renderable.AlphaTestData.UpperAlpha));
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

    /// <summary>
    /// Function to copy the sprite data into the specified sprite.
    /// </summary>
    /// <param name="sprite">The sprite that will receive the data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprite"/> parameter is <b>null</b>.</exception>
    public void CopyTo(GorgonSprite sprite)
    {
        if (sprite is null)
        {
            throw new ArgumentNullException(nameof(sprite));
        }

        sprite.Bounds = Bounds;
        sprite.Anchor = Anchor;
        sprite._absoluteAnchor = _absoluteAnchor;
        sprite.AlphaTest = AlphaTest;
        sprite.Color = Color;
        sprite.Depth = Depth;
        sprite.HorizontalFlip = HorizontalFlip;
        sprite.Scale = Scale;
        sprite.Texture = Texture;
        sprite.TextureArrayIndex = TextureArrayIndex;
        sprite.TextureRegion = TextureRegion;
        sprite.TextureSampler = TextureSampler;
        sprite.VerticalFlip = VerticalFlip;

        sprite.CornerOffsets.UpperLeft = CornerOffsets.UpperLeft;
        sprite.CornerOffsets.UpperRight = CornerOffsets.UpperRight;
        sprite.CornerOffsets.LowerRight = CornerOffsets.LowerRight;
        sprite.CornerOffsets.LowerLeft = CornerOffsets.LowerLeft;

        sprite.CornerColors.UpperLeft = CornerColors.UpperLeft;
        sprite.CornerColors.UpperRight = CornerColors.UpperRight;
        sprite.CornerColors.LowerRight = CornerColors.LowerRight;
        sprite.CornerColors.LowerLeft = CornerColors.LowerLeft;

        // Mark the sprite as having changes on all parts.
        sprite.Renderable.AngleDegs = Renderable.AngleDegs;

        sprite.Renderable.HasTextureChanges = true;
        sprite.Renderable.HasTransformChanges = true;
        sprite.Renderable.HasVertexChanges = true;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonSprite"/> class.</summary>
    /// <param name="clone">The clone.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="clone"/> parameter is <b>null</b>.</exception>
    public GorgonSprite(GorgonSprite clone)
        : this()
    {
        if (clone is null)
        {
            throw new ArgumentNullException(nameof(clone));
        }

        clone.CopyTo(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSprite"/> class.
    /// </summary>
    public GorgonSprite()
    {
        CornerColors = new GorgonSpriteColors(GorgonColor.White, Renderable);
        CornerOffsets = new GorgonRectangleOffsets(Renderable);

        for (int i = 0; i < Renderable.Vertices.Length; ++i)
        {
            Renderable.Vertices[i].Position.W = 1.0f;
        }
    }
}
