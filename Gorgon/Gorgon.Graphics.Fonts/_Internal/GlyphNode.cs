#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 23, 2017 9:49:20 PM
// 
#endregion

using System.Drawing;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// A node for the glyph packing.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GlyphNode"/> class.
/// </remarks>
/// <param name="parentNode">The parent node.</param>
internal class GlyphNode(GlyphNode parentNode)
{
    #region Variables.
    // Flag to indicate that we have no more space.
    private bool _noMoreRoom;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the region that this node occupies on the image.
    /// </summary>
    public Rectangle Region
    {
        get;
        set;
    } = Rectangle.Empty;

    /// <summary>
    /// Property to return the parent node for this node.
    /// </summary>
    public GlyphNode Parent
    {
        get;
    } = parentNode;

    /// <summary>
    /// Property to set or return the node to the left of this one.
    /// </summary>
    public GlyphNode Left
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the node to the right of this one.
    /// </summary>
    public GlyphNode Right
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether this node is a leaf node.
    /// </summary>
    public bool IsLeaf => ((Left is null) && (Right is null));
    #endregion

    #region Methods.
    /// <summary>
    /// Function to add a node as a child to this node.
    /// </summary>
    /// <param name="dimensions">Dimensions that the node will occupy.</param>
    public GlyphNode AddNode(Size dimensions)
    {
        if (!IsLeaf)
        {
            GlyphNode node = Left.AddNode(dimensions);
            // If we can't add the node to the left, then try the right.
            return node ?? Right.AddNode(dimensions);
        }

        // Do nothing if there's no more room.
        if (_noMoreRoom)
        {
            return null;
        }

        // Ensure we can fit this node.
        if ((dimensions.Width > Region.Width) || (dimensions.Height > Region.Height))
        {
            return null;
        }

        // We have an exact fit, so there will be no more room for other nodes.
        if ((dimensions.Width == Region.Width) && (dimensions.Height == Region.Height))
        {
            _noMoreRoom = true;
            return this;
        }

        Left = new GlyphNode(this);
        Right = new GlyphNode(this);

        // Subdivide.
        var delta = new Size(Region.Width - dimensions.Width, Region.Height - dimensions.Height);

        if (delta.Width > delta.Height)
        {
            Left.Region = new Rectangle(Region.Left, Region.Top, dimensions.Width, Region.Height);
            Right.Region = new Rectangle(Region.Left + dimensions.Width, Region.Top, delta.Width, Region.Height);
        }
        else
        {
            Left.Region = new Rectangle(Region.Left, Region.Top, Region.Width, dimensions.Height);
            Right.Region = new Rectangle(Region.Left, Region.Top + dimensions.Height, Region.Width, delta.Height);
        }

        return Left.AddNode(dimensions);
    }

    #endregion
    #region Constructor/Destructor.
    #endregion
}
