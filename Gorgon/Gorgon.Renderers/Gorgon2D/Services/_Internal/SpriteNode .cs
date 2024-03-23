// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 23, 2017 9:49:20 PM
// 

using Gorgon.Graphics;

namespace Gorgon.Renderers.Services;

/// <summary>
/// A node for the sprite packing
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SpriteNode"/> class
/// </remarks>
/// <param name="parentNode">The parent node.</param>
internal class SpriteNode(SpriteNode parentNode)
{
    // Flag to indicate that we have no more space.
    private bool _noMoreRoom;

    /// <summary>
    /// Property to set or return the region that this node occupies on the image.
    /// </summary>
    public GorgonRectangle Region
    {
        get;
        set;
    } = GorgonRectangle.Empty;

    /// <summary>
    /// Property to return the parent node for this node.
    /// </summary>
    public SpriteNode Parent
    {
        get;
    } = parentNode;

    /// <summary>
    /// Property to set or return the node to the left of this one.
    /// </summary>
    public SpriteNode Left
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the node to the right of this one.
    /// </summary>
    public SpriteNode Right
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether this node is a leaf node.
    /// </summary>
    public bool IsLeaf => ((Left is null) && (Right is null));

    /// <summary>
    /// Function to add a node as a child to this node.
    /// </summary>
    /// <param name="dimensions">Dimensions that the node will occupy.</param>
    public SpriteNode AddNode(GorgonPoint dimensions)
    {
        if (!IsLeaf)
        {
            return Left.IsLeaf ? Left.AddNode(dimensions) ?? Right.AddNode(dimensions)
                               : Right.AddNode(dimensions) ?? Left.AddNode(dimensions);
        }

        // Do nothing if there's no more room.
        if (_noMoreRoom)
        {
            return null;
        }

        // Ensure we can fit this node.
        if ((dimensions.X > Region.X) || (dimensions.Y > Region.Y))
        {
            return null;
        }

        // We have an exact fit, so there will be no more room for other nodes.
        if ((dimensions.X == Region.X) && (dimensions.Y == Region.Y))
        {
            _noMoreRoom = true;
            return this;
        }

        // The region occupied by the requested dimensions.
        Left = new SpriteNode(this);
        Right = new SpriteNode(this);

        // Subdivide.
        GorgonPoint delta = new(Region.X - dimensions.X, Region.Y - dimensions.Y);

        if (delta.Y <= 0)
        {
            Left.Region = new GorgonRectangle(Region.Left, Region.Top, dimensions.X, Region.Y);
            Right.Region = new GorgonRectangle(Region.Left + dimensions.X, Region.Top, delta.X, Region.Y);
        }
        else
        {
            Left.Region = new GorgonRectangle(Region.Left, Region.Top, Region.X, dimensions.Y);
            Right.Region = new GorgonRectangle(Region.Left, Region.Top + dimensions.Y, Region.X, delta.Y);
        }

        return Left.AddNode(dimensions) ?? Right.AddNode(dimensions);
    }
}
