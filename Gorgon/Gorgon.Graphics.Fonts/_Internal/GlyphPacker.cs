
// 
// Gorgon
// Copyright (C) 2012 Michael Winsor
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
// Created: Saturday, April 14, 2012 10:56:23 AM
// 

using Gorgon.Graphics.Fonts.Properties;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// Used to determine where glyphs should be packed onto a texture
/// </summary>
internal static class GlyphPacker
{
    /// <summary>
    /// Property to return the root node.
    /// </summary>
    public static GlyphNode Root
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to create the root node.
    /// </summary>
    /// <param name="textureWidth">The width of the texture.</param>
    /// <param name="textureHeight">The height of the texture.</param>
    public static void CreateRoot(int textureWidth, int textureHeight) => Root = new GlyphNode(null)
    {
        Region = new Rectangle(0, 0, textureWidth, textureHeight)
    };

    /// <summary>
    /// Function to add a node to the 
    /// </summary>
    /// <param name="dimensions">The glyph dimensions.</param>
    /// <returns>A rectangle for the area on the image that the glyph will be located at, or <b>null</b> if there's no room.</returns>
    public static Rectangle? Add(Size dimensions)
    {
        if ((dimensions.Width > Root.Region.Width) || (dimensions.Height > Root.Region.Height))
        {
            throw new ArgumentOutOfRangeException(nameof(dimensions), Resources.GORGFX_ERR_FONT_GLYPH_NODE_TOO_LARGE);
        }

        // Do nothing here.
        if ((dimensions.Width == 0) || (dimensions.Height == 0))
        {
            return Rectangle.Empty;
        }

        GlyphNode newNode = Root.AddNode(dimensions);

        return newNode?.Region;
    }
}
