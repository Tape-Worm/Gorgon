﻿#region MIT
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
// Created: June 9, 2018 10:57:03 AM
// 
#endregion

using System;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Provides functionality for transforming renderable vertices.
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
		private static void GetTextAlignmentExtents(ref DX.Vector2 leftTop, Alignment alignment, DX.Size2F layoutSize, float lineLength, float textHeight)
		{
			int calc;

			switch (alignment)
			{
				case Alignment.UpperCenter:
					calc = (int)((layoutSize.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					break;
				case Alignment.UpperRight:
					calc = (int)(layoutSize.Width - lineLength);
					leftTop.X += calc;
					break;
				case Alignment.CenterLeft:
					calc = (int)(layoutSize.Height / 2.0f - textHeight / 2.0f);
					leftTop.Y += calc;
					break;
				case Alignment.Center:
					calc = (int)((layoutSize.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					calc = (int)(layoutSize.Height / 2.0f - textHeight / 2.0f);
					leftTop.Y += calc;
					break;
				case Alignment.CenterRight:
					calc = (int)(layoutSize.Width - lineLength);
					leftTop.X += calc;
					calc = (int)(layoutSize.Height / 2.0f - textHeight / 2.0f);
					leftTop.Y += calc;
					break;
				case Alignment.LowerLeft:
					calc = (int)(layoutSize.Height - textHeight);
					leftTop.Y += calc;
					break;
				case Alignment.LowerCenter:
					calc = (int)((layoutSize.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					calc = (int)(layoutSize.Height - textHeight);
					leftTop.Y += calc;
					break;
				case Alignment.LowerRight:
					calc = (int)(layoutSize.Width - lineLength);
					leftTop.X += calc;
					calc = (int)(layoutSize.Height - textHeight);
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
        private static void BuildRenderable(ref DX.RectangleF bounds, ref DX.Vector2 anchor, ref DX.Vector4 corners)
        {
            if (anchor.IsZero)
            {
                return;
            }

            var anchorProjected = new DX.Vector2(anchor.X * bounds.Width, anchor.Y * bounds.Height);
            corners = new DX.Vector4(-anchorProjected.X, -anchorProjected.Y, bounds.Width, bounds.Height);
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
        private static void UpdateVertexColors(Gorgon2DVertex[] vertices,
                                               in GorgonColor upperLeft,
                                               in GorgonColor upperRight,
                                               in GorgonColor lowerLeft,
                                               in GorgonColor lowerRight,
                                               int vertexOffset,
                                               bool hasOutline,
                                               GorgonColor outlineTint)
        {
            ref Gorgon2DVertex v0 = ref vertices[vertexOffset];
            ref Gorgon2DVertex v1 = ref vertices[vertexOffset + 1];
            ref Gorgon2DVertex v2 = ref vertices[vertexOffset + 2];
            ref Gorgon2DVertex v3 = ref vertices[vertexOffset + 3];

            if (hasOutline)
            {
                v0.Color = v1.Color = v2.Color = v3.Color = outlineTint;
                return;
            }

            v0.Color = upperLeft;
            v1.Color = upperRight;
            v2.Color = lowerLeft;
            v3.Color = lowerRight;
        }

        /// <summary>
        /// Function to update the texture coordinates for the renderable.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="textureRegion">The texture coordinates.</param>
        /// <param name="textureArrayIndex">The index into a texture array.</param>
        /// <param name="vertexOffset">The offset into the vertex array.</param>
        private static void UpdateTextureCoordinates(Gorgon2DVertex[] vertices,
                                                     ref DX.RectangleF textureRegion,
                                                     int textureArrayIndex,
                                                     int vertexOffset)
        {
            ref Gorgon2DVertex v0 = ref vertices[vertexOffset];
            ref Gorgon2DVertex v1 = ref vertices[vertexOffset + 1];
            ref Gorgon2DVertex v2 = ref vertices[vertexOffset + 2];
            ref Gorgon2DVertex v3 = ref vertices[vertexOffset + 3];

            v0.UV = new DX.Vector3(textureRegion.TopLeft, textureArrayIndex);
            v1.UV = new DX.Vector3(textureRegion.TopRight, textureArrayIndex);
            v2.UV = new DX.Vector3(textureRegion.BottomLeft, textureArrayIndex);
            v3.UV = new DX.Vector3(textureRegion.BottomRight, textureArrayIndex);
        }

        /// <summary>
        /// Function to transform each vertex of the renderable to change its location, size and rotation.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="glyphBounds">The bounds of the character to render.</param>
        /// <param name="spriteBounds">The bounds of the entire sprite.</param>
        /// <param name="scale">The scale of the renderable.</param>
        /// <param name="angleRads">The cached angle, in radians.</param>
        /// <param name="angleSin">The cached sine of the angle.</param>
        /// <param name="angleCos">The cached cosine of the angle.</param>
        /// <param name="depth">The depth of the text sprite.</param>
        /// <param name="vertexOffset">The offset into the vertex array.</param>
        private static void TransformVertices(Gorgon2DVertex[] vertices,
                                              ref DX.RectangleF glyphBounds,
                                              ref DX.RectangleF spriteBounds,
                                              ref DX.Vector2 scale,
                                              float angleRads,
                                              float angleSin,
                                              float angleCos,
                                              float depth,
                                              int vertexOffset)
        {
            if ((scale.X != 1.0f) || (scale.Y != 1.0f))
            {
                glyphBounds = new DX.RectangleF(glyphBounds.Left * scale.X, glyphBounds.Top * scale.Y, glyphBounds.Width * scale.X, glyphBounds.Height * scale.Y);
            }

            ref Gorgon2DVertex v0 = ref vertices[vertexOffset];
            ref Gorgon2DVertex v1 = ref vertices[vertexOffset + 1];
            ref Gorgon2DVertex v2 = ref vertices[vertexOffset + 2];
            ref Gorgon2DVertex v3 = ref vertices[vertexOffset + 3];

            if (angleRads != 0.0f)
            {
                v0.Position = new DX.Vector4((glyphBounds.Left * angleCos - glyphBounds.Top * angleSin) + spriteBounds.X,
                                             (glyphBounds.Left * angleSin + glyphBounds.Top * angleCos) + spriteBounds.Y,
                                             depth,
                                             1.0f);
                v0.Angle = angleRads;

                v1.Position = new DX.Vector4((glyphBounds.Right * angleCos - glyphBounds.Top * angleSin) + spriteBounds.X,
                                             (glyphBounds.Right * angleSin + glyphBounds.Top * angleCos) + spriteBounds.Y,
                                             depth,
                                             1.0f);
                v1.Angle = angleRads;

                v2.Position = new DX.Vector4((glyphBounds.Left * angleCos - glyphBounds.Bottom * angleSin) + spriteBounds.X,
                                             (glyphBounds.Left * angleSin + glyphBounds.Bottom * angleCos) + spriteBounds.Y,
                                             depth,
                                             1.0f);
                v2.Angle = angleRads;

                v3.Position = new DX.Vector4((glyphBounds.Right * angleCos - glyphBounds.Bottom * angleSin) + spriteBounds.X,
                                             (glyphBounds.Right * angleSin + glyphBounds.Bottom * angleCos) + spriteBounds.Y,
                                             depth,
                                             1.0f);
                v3.Angle = angleRads;
            }
            else
            {
                v0.Position = new DX.Vector4(glyphBounds.Left + spriteBounds.X, glyphBounds.Top + spriteBounds.Y, depth, 1.0f);
                v0.Angle = 0;

                v1.Position = new DX.Vector4(glyphBounds.Right + spriteBounds.X, glyphBounds.Top + spriteBounds.Y, depth, 1.0f);
                v1.Angle = 0;

                v2.Position = new DX.Vector4(glyphBounds.Left + spriteBounds.X, glyphBounds.Bottom + spriteBounds.Y, depth, 1.0f);
                v2.Angle = 0;

                v3.Position = new DX.Vector4(glyphBounds.Right + spriteBounds.X, glyphBounds.Bottom + spriteBounds.Y, depth, 1.0f);
                v3.Angle = 0;
            }
        }

        /// <summary>
        /// Function to adjust and initialize the number of vertices for a text sprite.
        /// </summary>
        /// <param name="vertices">The vertices used by a text sprite.</param>
        /// <param name="characterCount">The number of characters in the text to render.</param>
        private static void AdjustTextSpriteVertices(ref Gorgon2DVertex[] vertices, int characterCount)
        {
            int vertexCount = characterCount * 4;

            // The vertices do not need to be adjusted at this time.
            if ((vertices != null) && (vertexCount <= vertices.Length))
            {
                return;
            }


            int oldSize = 0;
            int newSize = (((characterCount + (characterCount / 2)) + 63) & ~63) * 4;
            if (vertices != null)
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

                vertex.Position = new DX.Vector4(DX.Vector3.Zero, 1.0f);
                vertex.Color = GorgonColor.White;
                vertex.UV = DX.Vector3.Zero;
                vertex.Angle = 0;
            }
        }

        /// <summary>
        /// Function to transform the vertices for a renderable.
        /// </summary>
        /// <param name="renderable">The renderable to transform.</param>
        /// <param name="glyph">The current glyph to render.</param>
        /// <param name="spriteBounds">The boundaries of the sprite.</param>
        /// <param name="glyphPosition">The glyph position relative to the upper left corner of the text sprite.</param>
        /// <param name="textLength">The total number of characters to render.</param>
        /// <param name="vertexOffset">The position in the vertex array to update.</param>
        /// <param name="hasOutlines"><b>true</b> if outlines need to be drawn, or <b>false</b> if not.</param>
        /// <param name="outlineTint">A color used to tint the </param>
        /// <param name="alignment">The alignment of the text.</param>
        /// <param name="layoutArea">The area to layout the text inside of for an alignment.</param>
        /// <param name="lineMeasure">The width of the line.</param>
        public void Transform(BatchRenderable renderable, GorgonGlyph glyph, ref DX.RectangleF spriteBounds, ref DX.Vector2 glyphPosition, int textLength, int vertexOffset, bool hasOutlines, in GorgonColor outlineTint, Alignment alignment, DX.Size2F? layoutArea, float lineMeasure)
        {
            ref Gorgon2DVertex[] vertices = ref renderable.Vertices;
            
            // Ensure there's enough vertices allocated.
            if (renderable.VertexCountChanged)
            {
                AdjustTextSpriteVertices(ref vertices, textLength);
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

            if (renderable.HasVertexColorChanges)
            {
                UpdateVertexColors(vertices, in renderable.UpperLeftColor, in renderable.UpperRightColor, in renderable.LowerLeftColor, in renderable.LowerRightColor, vertexOffset, hasOutlines, outlineTint);
            }

            if (renderable.HasTransformChanges)
            {
                DX.Vector2 offset = hasOutlines ? glyph.OutlineOffset : glyph.Offset;
                var size = hasOutlines
                                      ? new DX.Vector2(glyph.OutlineCoordinates.Width, glyph.OutlineCoordinates.Height)
                                      : new DX.Vector2(glyph.GlyphCoordinates.Width, glyph.GlyphCoordinates.Height);
                var upperLeft = new DX.Vector2(glyphPosition.X + offset.X + renderable.Corners.X,
                                               glyphPosition.Y + offset.Y + renderable.Corners.Y);

                if (alignment != Alignment.UpperLeft)
                {
                    GetTextAlignmentExtents(ref upperLeft,
                                            alignment,
                                            layoutArea ?? new DX.Size2F(spriteBounds.Width, spriteBounds.Height),
                                            lineMeasure,
                                            spriteBounds.Height);
                }
                
                var glyphBounds = new DX.RectangleF(upperLeft.X, upperLeft.Y, size.X, size.Y);

                TransformVertices(vertices,
                                  ref glyphBounds,
                                  ref spriteBounds,
                                  ref renderable.Scale,
                                  renderable.AngleRads,
                                  renderable.AngleSin,
                                  renderable.AngleCos,
                                  renderable.Depth,
                                  vertexOffset);
            }

            if (!renderable.HasTextureChanges)
            {
                return;
            }

            DX.RectangleF textureCoordinates = hasOutlines ? glyph.OutlineTextureCoordinates : glyph.TextureCoordinates;
            UpdateTextureCoordinates(vertices, ref textureCoordinates, glyph.TextureIndex, vertexOffset);
        }
    }
}
