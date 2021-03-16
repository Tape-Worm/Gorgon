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

using DX = SharpDX;

namespace Gorgon.Renderers.Services
{
    /// <summary>
    /// A node for the sprite packing.
    /// </summary>
    internal class SpriteNode
    {
        #region Variables.
        // Flag to indicate that we have no more space.
        private bool _noMoreRoom;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the region that this node occupies on the image.
        /// </summary>
        public DX.Rectangle Region
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the parent node for this node.
        /// </summary>
        public SpriteNode Parent
        {
            get;
        }

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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add a node as a child to this node.
        /// </summary>
        /// <param name="dimensions">Dimensions that the node will occupy.</param>
        public SpriteNode AddNode(DX.Size2 dimensions)
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

            // The region occupied by the requested dimensions.
            Left = new SpriteNode(this);
            Right = new SpriteNode(this);

            // Subdivide.
            var delta = new DX.Size2(Region.Width - dimensions.Width, Region.Height - dimensions.Height);

            if (delta.Height <= 0) //(delta.Width > delta.Height)
            {
                Left.Region = new DX.Rectangle(Region.Left, Region.Top, dimensions.Width, Region.Height);
                Right.Region = new DX.Rectangle(Region.Left + dimensions.Width, Region.Top, delta.Width, Region.Height);
            }
            else
            {
                Left.Region = new DX.Rectangle(Region.Left, Region.Top, Region.Width, dimensions.Height);
                Right.Region = new DX.Rectangle(Region.Left, Region.Top + dimensions.Height, Region.Width, delta.Height);
            }

            return Left.AddNode(dimensions) ?? Right.AddNode(dimensions);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteNode"/> class.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        public SpriteNode(SpriteNode parentNode)
        {
            Region = DX.Rectangle.Empty;
            Parent = parentNode;
        }
        #endregion
    }
}