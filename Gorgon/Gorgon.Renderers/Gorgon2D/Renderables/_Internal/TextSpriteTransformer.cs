
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
// Created: June 9, 2018 10:57:03 AM
// 

using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;
using Gorgon.UI;

namespace Gorgon.Renderers;

/// <summary>
/// Provides functionality for transforming renderable vertices
/// </summary>
internal class TextSpriteTransformer
{
    /// <summary>
    /// Function to update the transform coordinates by the alignment settings.
    /// </summary>
    /// <param name="leftTop">Left and top coordinates.</param>
    /// <param name="alignment">The alignment to use.</param>
    /// <param name="layoutSize">The layout region size.</param>
    /// <param name="lineLength">Length of the line, in pixels.</param>
    /// <param name="textHeight">The height of the text block being rendered.</param>
    private static void GetTextAlignmentExtents(ref Vector2 leftTop, Alignment alignment, ref Vector2 layoutSize, float lineLength, float textHeight)
    {
        int calc;

        switch (alignment)
        {
            case Alignment.UpperCenter:
                calc = (int)((layoutSize.X / 2.0f) - (lineLength / 2.0f));
                leftTop.X += calc;
                break;
            case Alignment.UpperRight:
                calc = (int)(layoutSize.X - lineLength);
                leftTop.X += calc;
                break;
            case Alignment.CenterLeft:
                calc = (int)((layoutSize.Y / 2.0f) - (textHeight / 2.0f));
                leftTop.Y += calc;
                break;
            case Alignment.Center:
                calc = (int)((layoutSize.X / 2.0f) - (lineLength / 2.0f));
                leftTop.X += calc;
                calc = (int)((layoutSize.Y / 2.0f) - (textHeight / 2.0f));
                leftTop.Y += calc;
                break;
            case Alignment.CenterRight:
                calc = (int)(layoutSize.X - lineLength);
                leftTop.X += calc;
                calc = (int)((layoutSize.Y / 2.0f) - (textHeight / 2.0f));
                leftTop.Y += calc;
                break;
            case Alignment.LowerLeft:
                calc = (int)(layoutSize.Y - textHeight);
                leftTop.Y += calc;
                break;
            case Alignment.LowerCenter:
                calc = (int)((layoutSize.X / 2.0f) - (lineLength / 2.0f));
                leftTop.X += calc;
                calc = (int)(layoutSize.Y - textHeight);
                leftTop.Y += calc;
                break;
            case Alignment.LowerRight:
                calc = (int)(layoutSize.X - lineLength);
                leftTop.X += calc;
                calc = (int)(layoutSize.Y - textHeight);
                leftTop.Y += calc;
                break;
        }
    }

    /// <summary>
    /// Function to build up the renderable vertices.
    /// </summary>
    /// <param name="bounds">The bounds of the renderable.</param>
    /// <param name="anchor">The anchor point for the renderable.</param>
    /// <param name="corners">The corners of the sprite.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildRenderable(ref GorgonRectangleF bounds, ref Vector2 anchor, ref Vector4 corners)
    {
        if (anchor.Equals(Vector2.Zero))
        {
            return;
        }

        Vector2 anchorProjected = new(anchor.X * bounds.Width, anchor.Y * bounds.Height);
        corners = new Vector4(-anchorProjected.X, -anchorProjected.Y, bounds.Width, bounds.Height);
    }

    /// <summary>
    /// Function to update the colors for each corner of the renderable.
    /// </summary>
    /// <param name="vertices">The vertices for the renderable.</param>
    /// <param name="upperLeft">The color of the upper left corner.</param>
    /// <param name="upperRight">The color of the upper right corner.</param>
    /// <param name="lowerLeft">The color of the lower left corner.</param>
    /// <param name="lowerRight">The color of the lower right corner.</param>
    /// <param name="vertexOffset">The offset into the vertex array.</param>
    /// <param name="hasOutline"><b>true</b> if using the outlines in the font, <b>false</b> if not.</param>
    /// <param name="outlineTint">A color used to tint the </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateVertexColors(Gorgon2DVertex[] vertices,
                                           ref GorgonColor upperLeft,
                                           ref GorgonColor upperRight,
                                           ref GorgonColor lowerLeft,
                                           ref GorgonColor lowerRight,
                                           ref GorgonColor outlineTint,
                                           int vertexOffset,
                                           bool hasOutline)
    {
        ref GorgonColor v0 = ref vertices[vertexOffset++].Color;
        ref GorgonColor v1 = ref vertices[vertexOffset++].Color;
        ref GorgonColor v2 = ref vertices[vertexOffset++].Color;
        ref GorgonColor v3 = ref vertices[vertexOffset].Color;

        if (hasOutline)
        {
            v0 = v1 = v2 = v3 = outlineTint;
            return;
        }

        v0 = upperLeft;
        v1 = upperRight;
        v2 = lowerLeft;
        v3 = lowerRight;
    }

    /// <summary>
    /// Function to update the texture coordinates for the renderable.
    /// </summary>
    /// <param name="vertices">The vertices for the renderable.</param>
    /// <param name="textureRegion">The texture coordinates.</param>
    /// <param name="textureArrayIndex">The index into a texture array.</param>
    /// <param name="vertexOffset">The offset into the vertex array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateTextureCoordinates(Gorgon2DVertex[] vertices,
                                                 ref GorgonRectangleF textureRegion,
                                                 int textureArrayIndex,
                                                 int vertexOffset)
    {
        ref Vector4 v0 = ref vertices[vertexOffset++].UV;
        ref Vector4 v1 = ref vertices[vertexOffset++].UV;
        ref Vector4 v2 = ref vertices[vertexOffset++].UV;
        ref Vector4 v3 = ref vertices[vertexOffset].UV;

        v0 = new Vector4(textureRegion.Left, textureRegion.Top, textureArrayIndex, 1);
        v1 = new Vector4(textureRegion.Right, textureRegion.Top, textureArrayIndex, 1);
        v2 = new Vector4(textureRegion.Left, textureRegion.Bottom, textureArrayIndex, 1);
        v3 = new Vector4(textureRegion.Right, textureRegion.Bottom, textureArrayIndex, 1);
    }

    /// <summary>
    /// Function to transform each vertex of the renderable to change its location, size and rotation.
    /// </summary>
    /// <param name="renderable">The text renderable.</param>
    /// <param name="glyphBounds">The bounds of the character to render.</param>
    /// <param name="sin">The sine value for the rotation angle.</param>
    /// <param name="cos">The cosine value for the rotation angle.</param>
    /// <param name="vertexOffset">The offset into the vertex array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformVertices(BatchRenderable renderable,
                                          ref GorgonRectangleF glyphBounds,
                                          float sin,
                                          float cos,
                                          int vertexOffset)
    {
        glyphBounds = new GorgonRectangleF(glyphBounds.Left * renderable.Scale.X,
                                        glyphBounds.Top * renderable.Scale.Y,
                                        glyphBounds.Width * renderable.Scale.X,
                                        glyphBounds.Height * renderable.Scale.Y);

        ref Gorgon2DVertex v0 = ref renderable.Vertices[vertexOffset++];
        ref Gorgon2DVertex v1 = ref renderable.Vertices[vertexOffset++];
        ref Gorgon2DVertex v2 = ref renderable.Vertices[vertexOffset++];
        ref Gorgon2DVertex v3 = ref renderable.Vertices[vertexOffset];

        v0.Position = new Vector4(((glyphBounds.Left * cos) - (glyphBounds.Top * sin)) + renderable.Bounds.Left,
                                  ((glyphBounds.Left * sin) + (glyphBounds.Top * cos)) + renderable.Bounds.Top,
                                  renderable.Depth,
                                  1.0f);

        v1.Position = new Vector4(((glyphBounds.Right * cos) - (glyphBounds.Top * sin)) + renderable.Bounds.Left,
                                  ((glyphBounds.Right * sin) + (glyphBounds.Top * cos)) + renderable.Bounds.Top,
                                  renderable.Depth,
                                  1.0f);

        v2.Position = new Vector4(((glyphBounds.Left * cos) - (glyphBounds.Bottom * sin)) + renderable.Bounds.Left,
                                  ((glyphBounds.Left * sin) + (glyphBounds.Bottom * cos)) + renderable.Bounds.Top,
                                  renderable.Depth,
                                  1.0f);

        v3.Position = new Vector4(((glyphBounds.Right * cos) - (glyphBounds.Bottom * sin)) + renderable.Bounds.Left,
                                  ((glyphBounds.Right * sin) + (glyphBounds.Bottom * cos)) + renderable.Bounds.Top,
                                  renderable.Depth,
                                  1.0f);

        Vector2 cosSin = new(cos, sin);
        v0.Angle =
        v1.Angle =
        v2.Angle =
        v3.Angle = cosSin;
    }

    /// <summary>
    /// Function to adjust and initialize the number of vertices for a text sprite.
    /// </summary>
    /// <param name="vertices">The vertices used by a text sprite.</param>
    /// <param name="characterCount">The number of characters in the text to render.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AdjustTextSpriteVertices(ref Gorgon2DVertex[] vertices, int characterCount)
    {
        int vertexCount = characterCount * 4;

        // The vertices do not need to be adjusted at this time.
        if ((vertices is not null) && (vertexCount <= vertices.Length))
        {
            return;
        }

        int oldSize = 0;
        int newSize = (((characterCount + (characterCount / 2)) + 63) & ~63) * 4;
        if (vertices is not null)
        {
            oldSize = vertices.Length;
            Array.Resize(ref vertices, newSize);
        }
        else
        {
            vertices = new Gorgon2DVertex[newSize];
        }

        for (int i = oldSize; i < newSize; ++i)
        {
            ref Gorgon2DVertex vertex = ref vertices[i];

            vertex.Position = new Vector4(Vector3.Zero, 1.0f);
            vertex.Color = GorgonColors.White;
            vertex.UV = Vector4.UnitW;
            vertex.Angle = Vector2.Zero;
        }
    }

    /// <summary>
    /// Function to return the color for a specific character index based on formatting blocks in the text.
    /// </summary>
    /// <param name="charIndex">The index of the character.</param>
    /// <param name="colorBlocks">The blocks to evaulate.</param>
    /// <returns>The color if found, or <b>null</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonColor? GetColorForCharacter(int charIndex, List<ColorBlock> colorBlocks)
    {
        for (int i = 0; i < colorBlocks.Count; ++i)
        {
            ColorBlock block = colorBlocks[i];

            if ((charIndex >= block.Start) && (charIndex <= block.End))
            {
                return block.Color;
            }
        }

        return null;
    }

    /// <summary>
    /// Function to transform the vertices for a renderable.
    /// </summary>
    /// <param name="renderable">The renderable to transform.</param>
    /// <param name="glyph">The current glyph to render.</param>
    /// <param name="blockColor">The current block color.</param>
    /// <param name="glyphPosition">The glyph position relative to the upper left corner of the text sprite.</param>
    /// <param name="vertexOffset">The position in the vertex array to update.</param>
    /// <param name="isOutlinePass"><b>true</b> if outlines need to be drawn, or <b>false</b> if not.</param>
    /// <param name="lineMeasure">The width of the line.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(TextRenderable renderable, GorgonGlyph glyph, GorgonColor? blockColor, ref readonly Vector2 glyphPosition, int vertexOffset, bool isOutlinePass, float lineMeasure)
    {
        ref GorgonRectangleF spriteBounds = ref renderable.Bounds;
        Alignment alignment = renderable.Alignment;
        ref GorgonColor outlineTint = ref renderable.OutlineTint;

        // Ensure there's enough vertices allocated.
        if (renderable.VertexCountChanged)
        {
            int textLength = renderable.TextLength * (renderable.DrawMode == TextDrawMode.OutlinedGlyphs ? 2 : 1);
            AdjustTextSpriteVertices(ref renderable.Vertices, textLength);

            renderable.VertexCountChanged = false;
            renderable.HasVertexChanges = true;
            renderable.HasTransformChanges = true;
            renderable.HasTextureChanges = true;
            renderable.HasVertexColorChanges = true;
        }

        if (renderable.HasVertexChanges)
        {
            BuildRenderable(ref renderable.Bounds, ref renderable.Anchor, ref renderable.Corners);

            // If we've updated the physical dimensions for the renderable, then we need to update the transform as well.
            renderable.HasVertexChanges = false;
            renderable.HasTransformChanges = true;
            renderable.HasTextureChanges = true;
            renderable.HasVertexColorChanges = true;
        }

        if ((renderable.HasVertexColorChanges)
            || (isOutlinePass))
        {
            // Override the colors if we have a block color.
            GorgonColor upperLeftColor = blockColor ?? renderable.UpperLeftColor;
            GorgonColor upperRightColor = blockColor ?? renderable.UpperRightColor;
            GorgonColor lowerLeftColor = blockColor ?? renderable.LowerLeftColor;
            GorgonColor lowerRightColor = blockColor ?? renderable.LowerRightColor;

            UpdateVertexColors(renderable.Vertices,
                               ref upperLeftColor,
                               ref upperRightColor,
                               ref lowerLeftColor,
                               ref lowerRightColor,
                               ref outlineTint,
                               vertexOffset,
                               isOutlinePass);
        }

        if (renderable.HasTextureChanges)
        {
            GorgonRectangleF textureCoordinates = isOutlinePass ? glyph.OutlineTextureCoordinates : glyph.TextureCoordinates;
            UpdateTextureCoordinates(renderable.Vertices, ref textureCoordinates, glyph.TextureIndex, vertexOffset);
        }

        if (!renderable.HasTransformChanges)
        {
            return;
        }

        Vector2 offset = isOutlinePass ? new Vector2(glyph.OutlineOffset.X, glyph.OutlineOffset.Y) : new Vector2(glyph.Offset.X, glyph.Offset.Y);
        Vector2 size = isOutlinePass
                       ? new Vector2(glyph.OutlineCoordinates.Width, glyph.OutlineCoordinates.Height)
                       : new Vector2(glyph.GlyphCoordinates.Width, glyph.GlyphCoordinates.Height);

        Vector2 upperLeft = new(glyphPosition.X + offset.X + renderable.Corners.X,
                                           glyphPosition.Y + offset.Y + renderable.Corners.Y);
        float rads = renderable.AngleDegs.ToRadians();

        if (alignment != Alignment.UpperLeft)
        {
            GetTextAlignmentExtents(ref upperLeft, alignment, ref renderable.LayoutArea, lineMeasure, spriteBounds.Height);
        }

        GorgonRectangleF glyphBounds = new(upperLeft.X, upperLeft.Y, size.X, size.Y);

        TransformVertices(renderable, ref glyphBounds, rads.FastSin(), rads.FastCos(), vertexOffset);
    }
}
