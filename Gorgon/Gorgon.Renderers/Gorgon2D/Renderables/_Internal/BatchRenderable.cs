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
// Created: June 9, 2018 12:42:34 AM
// 
#endregion

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Geometry;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A base class for a 2D renderable object.
    /// </summary>
    internal class BatchRenderable
    {
        // Many of these "Properties" are variables.  While this is horrid coding style, it's also SUPER efficient for access, so we'll trade pretty code for raw performance here.

        /// <summary>
        /// Property to set or return whether the vertices need to be transformed or not.
        /// </summary>
        public bool HasTransformChanges;

        /// <summary>
        /// Flag to indicate whether the vertex count changed or not.
        /// </summary>
        public bool VertexCountChanged;

        /// <summary>
        /// The color of the upper left corner of the renderable.
        /// </summary>
        public GorgonColor UpperLeftColor = GorgonColor.White;

        /// <summary>
        /// The color of the upper right corner of the renderable.
        /// </summary>
        public GorgonColor UpperRightColor = GorgonColor.White;

        /// <summary>
        /// The color of the lower left corner of the renderable.
        /// </summary>
        public GorgonColor LowerLeftColor = GorgonColor.White;

        /// <summary>
        /// The color of the lower right corner of the renderable.
        /// </summary>
        public GorgonColor LowerRightColor = GorgonColor.White;

        /// <summary>
        /// The offset of the upper left corner of the renderable.
        /// </summary>
        public DX.Vector3 UpperLeftOffset;

        /// <summary>
        /// The offset of the upper right corner of the renderable.
        /// </summary>
        public DX.Vector3 UpperRightOffset;

        /// <summary>
        /// The offset of the lower left corner of the renderable.
        /// </summary>
        public DX.Vector3 LowerLeftOffset;

        /// <summary>
        /// The offset of the lower right corner of the renderable.
        /// </summary>
        public DX.Vector3 LowerRightOffset;

        /// <summary>
        /// Property to set or return whether the object space information the vertices need updating or not.
        /// </summary>
        public bool HasVertexChanges = true;

        /// <summary>
        /// A flag to indicate whether the colors of the individual corners of the renderable have changed.
        /// </summary>
        public bool HasVertexColorChanges = true;

        /// <summary>
        /// Property to set or return whether the texture coordinates for the vertices need updating.
        /// </summary>
        public bool HasTextureChanges = true;

        /// <summary>
        /// Property to set or return the rectangle bounds for this renderable item.
        /// </summary>
        public DX.RectangleF Bounds;

        /// <summary>
        /// Property to set or return the depth value for this sprite.
        /// </summary>
        public float Depth;

        /// <summary>
        /// Property to set or return the point around which the sprite will pivot when rotated.
        /// </summary>
        /// <remarks>
        /// This value is a relative value where 0, 0 means the upper left of the sprite, and 1, 1 means the lower right.
        /// </remarks>
        public DX.Vector2 Anchor;

        /// <summary>
        /// Property to set or return the region of the texture to use when drawing the sprite.
        /// </summary>
        /// <remarks>
        /// These values are in texel coordinates.
        /// </remarks>
        public DX.RectangleF TextureRegion = new(0, 0, 1, 1);

        /// <summary>
        /// Property to set or return the scale factor to apply to the sprite.
        /// </summary>
        public DX.Vector2 Scale = DX.Vector2.One;

        /// <summary>
        /// Property to set or return the angle of rotation in degrees.
        /// </summary>
        public float AngleDegs;

        /// <summary>
        /// Property to set or return which index within a texture array to use.
        /// </summary>
        public int TextureArrayIndex;

        /// <summary>
        /// Property to set or return whether the sprite texture is flipped horizontally.
        /// </summary>
        /// <remarks>
        /// This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.
        /// </remarks>
        public bool HorizontalFlip;

        /// <summary>
        /// Property to set or return whether the sprite texture is flipped vertically.
        /// </summary>
        /// <remarks>
        /// This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.
        /// </remarks>
        public bool VerticalFlip;

        /// <summary>
        /// Property to set or return whether or not the state properties have been changed.
        /// </summary>
        public bool StateChanged = true;

        /// <summary>
        /// Property to set or return the alpha test data.
        /// </summary>
        public AlphaTestData AlphaTestData = new(true, GorgonRangeF.Empty);

        /// <summary>
        /// Property to set or return the vertices for this renderable.
        /// </summary>
        public Gorgon2DVertex[] Vertices;

        /// <summary>
        /// The actual number of vertices used (the vertices property may be larger than the actual number of vertices).
        /// </summary>
        public int ActualVertexCount;

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture;

        /// <summary>
        /// Property to set or return the texture sampler to use when rendering.
        /// </summary>
        public GorgonSamplerState TextureSampler;

        /// <summary>
        /// The bounding corners for a renderable.
        /// </summary>
        public DX.Vector4 Corners;

        /// <summary>
        /// The number of indices used by the renderable.
        /// </summary>
        public int IndexCount;

        /// <summary>
        /// The type of primitive.
        /// </summary>
        public PrimitiveType PrimitiveType = PrimitiveType.TriangleList;

        /// <summary>
        /// Function to determine if the states of the two renderables match for rendering.
        /// </summary>
        /// <param name="left">The left state to compare.</param>
        /// <param name="right">The right state to compare.</param>
        /// <returns><b>true</b> if the states are the same, or <b>false</b> if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreStatesSame(BatchRenderable left, BatchRenderable right)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if ((left == right) && (left.StateChanged))
            {
                return false;
            }

            return (left.PrimitiveType == right.PrimitiveType)
                   && (left.Texture == right.Texture)
                   && (left.TextureSampler == right.TextureSampler)
                   && (AlphaTestData.Equals(in left.AlphaTestData, in right.AlphaTestData));
#pragma warning restore IDE0046 // Convert to conditional expression
        }
    }
}
