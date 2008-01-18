#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Wednesday, November 21, 2007 12:33:01 AM
// 
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object used to represent a glyph node.
	/// </summary>
	internal class GlyphNode
	{
		#region Variables.
		private GlyphNode _left = null;						// Left node.
		private GlyphNode _right = null;					// Right node.
		private GlyphNode _parent = null;					// Parent node.		
		private Rectangle _imageRect = Rectangle.Empty;		// Image rectangle.
		private string _imagePath = string.Empty;			// Path to the image.
		private bool _dontAdd = false;						// Flag to indicate that we should not add.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the image rectangle.
		/// </summary>
		public Rectangle ImageRect
		{
			get
			{
				return _imageRect;
			}
			set
			{
				_imageRect = value;
			}
		}

		/// <summary>
		/// Property to return the path to the image that occupies this area.
		/// </summary>
		public string ImagePath
		{
			get
			{
				return _imagePath;
			}
			set
			{
				_imagePath = value;
			}
		}

		/// <summary>
		/// Property to return the image node.
		/// </summary>
		public GlyphNode Parent
		{
			get
			{
				return _parent;
			}
		}

		/// <summary>
		/// Property to set or return the left child node.
		/// </summary>
		public GlyphNode Left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		/// <summary>
		/// Property to set or return the right child node.
		/// </summary>
		public GlyphNode Right
		{
			get
			{
				return _right;
			}
			set
			{
				_right = value;
			}
		}

		/// <summary>
		/// Property to return whether this node is a leaf node or not.
		/// </summary>
		public bool IsLeaf
		{
			get
			{
				return ((_right == null) && (_left == null));
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a node to this node.
		/// </summary>
		/// <param name="dimensions">Dimensions of the image.</param>
		/// <returns>A new image node.</returns>
		public GlyphNode Add(Size dimensions)
		{
			Size delta = Size.Empty;		// Delta width & height.
			
			// If not a leaf node, then keep going.
			if (!IsLeaf)
			{
				GlyphNode newNode = null;

				newNode = _left.Add(dimensions);

				return newNode ?? _right.Add(dimensions);
			}

			// Don't add if not empty.
			if (_dontAdd)
				return null;

			// Ensure we can fit this image.
			if ((dimensions.Width > _imageRect.Width) || (dimensions.Height  > _imageRect.Height))
				return null;

			// Exact fit.
			if ((dimensions.Width  == _imageRect.Width) && (dimensions.Height == _imageRect.Height))
			{
				_dontAdd = true;
				return this;
			}

			// Set left and right nodes.
			_left = new GlyphNode(this);
			_right = new GlyphNode(this);

			delta.Width = ImageRect.Width - dimensions.Width;
			delta.Height = ImageRect.Height - dimensions.Height;

			if (delta.Width > delta.Height)
			{
				_left.ImageRect = new Rectangle(ImageRect.Left, ImageRect.Top, dimensions.Width, ImageRect.Height);
				_right.ImageRect = new Rectangle(ImageRect.Left + dimensions.Width, ImageRect.Top, delta.Width, ImageRect.Height);
			}
			else
			{
				_left.ImageRect = new Rectangle(ImageRect.Left, ImageRect.Top, ImageRect.Width, dimensions.Height);
				_right.ImageRect = new Rectangle(ImageRect.Left, ImageRect.Top + dimensions.Height, ImageRect.Width, delta.Height);
			}
			
			return _left.Add(dimensions);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GlyphNode"/> class.
		/// </summary>
		/// <param name="parent">The parent of this node, NULL for root.</param>
		public GlyphNode(GlyphNode parent)
		{
			_parent = parent;
		}
		#endregion
	}

	/// <summary>
	/// Object used to represent a spatial tree of glyphs.
	/// </summary>
	internal class GlyphTree
	{
		#region Variables.
		private GlyphNode _root = null;				// Root image node.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the root image node.
		/// </summary>
		public GlyphNode Root
		{
			get
			{
				return _root;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an image to the tree.
		/// </summary>
		/// <param name="imageDims">Image dimensions.</param>
		/// <returns>A rectangle for the item.</returns>
		public Rectangle Add(Size imageDims)
		{
			GlyphNode newNode = null;		// New node.

			if ((imageDims.Width > _root.ImageRect.Width) || (imageDims.Height > _root.ImageRect.Height))
				throw new GorgonLibrary.Graphics.FontGlyphTooLargeException();

			newNode = _root.Add(imageDims);

			if (newNode == null)
				return Rectangle.Empty;

			return newNode.ImageRect;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GlyphTree"/> class.
		/// </summary>
		/// <param name="imageSize">Size of the image image.</param>
		public GlyphTree(Size imageSize)
		{
			_root = new GlyphNode(null);
			_root.ImagePath = string.Empty;
			_root.ImageRect = new Rectangle(new Point(0, 0), imageSize);
		}
		#endregion
	}
}
