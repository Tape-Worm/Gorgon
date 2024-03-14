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
// Created: June 11, 2018 4:07:20 PM
// 
#endregion

using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Renderers;

/// <summary>
/// A renderable containing extra information for text items.
/// </summary>
internal class TextRenderable
    : BatchRenderable
{
    /// <summary>
    /// The font used to render the text.
    /// </summary>
    public GorgonFont Font;

    /// <summary>
    /// The individual lines of text to render.
    /// </summary>
    public string[] Lines;

    /// <summary>
    /// The area in which text can be laid out.
    /// </summary>
    public DX.Size2F LayoutArea;

    /// <summary>
    /// A multiplier used to define the amount of space between each line of text.
    /// </summary>
    public float LineSpaceMultiplier = 1.0f;

    /// <summary>
    /// The current drawing mode for the sprite.
    /// </summary>
    public TextDrawMode DrawMode;

    /// <summary>
    /// The alignment for the text within the layout area.
    /// </summary>
    public Alignment Alignment;

    /// <summary>
    /// The color used to tint the outline (if the font supports it and outlining is enabled).
    /// </summary>
    public GorgonColor OutlineTint = GorgonColor.White;

    /// <summary>
    /// The number of spaces to use when rendering a tab control character.
    /// </summary>
    public int TabSpaceCount = 4;

    /// <summary>
    /// The length of the complete text.
    /// </summary>
    public int TextLength;

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
    /// A flag to indicate whether the colors of the individual corners of the renderable have changed.
    /// </summary>
    public bool HasVertexColorChanges = true;

    /// <summary>
    /// A list of blocks of text to colorize using embedded codes.
    /// </summary>
    public List<ColorBlock> ColorBlocks = [];
}
