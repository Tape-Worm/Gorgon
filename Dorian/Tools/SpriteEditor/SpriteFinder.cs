#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, June 23, 2007 12:40:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Drawing = System.Drawing;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Enumeration containing masking operations.
	/// </summary>
	[Flags]
	public enum MaskOptions
	{
		/// <summary>Use alpha to determine masking region.</summary>
		Alpha = 1,
		/// <summary>Use color to determine masking region.</summary>
		Color = 2
	}

	/// <summary>
	/// Utility object to extract sprite rectangles from an image.
	/// </summary>
	public class SpriteFinder
		: IDisposable
	{
		#region Variables.
		private Drawing.Bitmap _image = null;				// Source image.
		private Drawing.Color _maskColor;					// Mask color.
		private MaskOptions _options;						// Masking options.
		private Drawing.Rectangle _gridConstraint;			// Grid constraint.
		private Vector2D _gridCellSpacing = Vector2D.Zero;	// Grid cell spacing.
		private Vector2D _gridCell = Vector2D.Zero;			// Grid cell size.
		private bool _topToBottom = false;					// Flag to indicate that we should move from top to bottom.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to indicate that we should read from top to bottom instead of left to right.
		/// </summary>
		public bool TopToBottom
		{
			get
			{
				return _topToBottom;
			}
			set
			{
				_topToBottom = value;
			}
		}

		/// <summary>
		/// Property to define the grid constraint.
		/// </summary>
		public Drawing.Rectangle GridConstraint
		{
			get
			{
				return _gridConstraint;
			}
			set
			{
				_gridConstraint = value;
			}
		}

		/// <summary>
		/// Property to define the grid cell spacing.
		/// </summary>
		public Vector2D GridCellSpacing
		{
			get
			{
				return _gridCellSpacing;
			}
			set
			{
				_gridCellSpacing = value;
			}
		}

		/// <summary>
		/// Property to define the grid cell size.
		/// </summary>
		public Vector2D GridCellSize
		{
			get
			{
				return _gridCell;
			}
			set
			{
				_gridCell = value;
			}
		}

		/// <summary>
		/// Property to set or return the mask alpha + color.
		/// </summary>
		public Drawing.Color MaskColor
		{
			get
			{
				return _maskColor;
			}
			set
			{
				_maskColor = value;
			}
		}

		/// <summary>
		/// Property to set or return mask options.
		/// </summary>
		public MaskOptions MaskOptions
		{
			get
			{
				return _options;
			}
			set
			{
				_options = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to extract a pixel from the image.
		/// </summary>
		/// <param name="ptr">Pointer to the image.</param>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="bounds">Bounds of the image.</param>
		/// <returns>The color at the position.</returns>
		private unsafe Drawing.Color GetPixel(int* ptr, int x, int y, Drawing.Size bounds)
		{
			int* scanline = ptr;		// Scanline pointer.

			// If out of bounds, return the mask color.
			if ((x < 0) || (x >= bounds.Width) || (y < 0) || (y >= bounds.Height))
				return _maskColor;

			scanline += (y * bounds.Width) + x;

			// Return color.
			return Drawing.Color.FromArgb(*scanline);
		}

		/// <summary>
		/// Function to determine if a color is a mask color.
		/// </summary>
		/// <param name="scanPtr">Scanline pointer.</param>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="imageDims">Image bounds.</param>
		/// <returns>TRUE if it's a mask pixel, FALSE if not.</returns>
		private unsafe bool ColorIsMask(int *scanPtr, int x, int y, Drawing.Size imageDims)
		{
			Drawing.Color sourceColor;				// Source color.
			Drawing.Color maskColor = _maskColor;	// Mask color.

			// Get the source color.
			sourceColor = GetPixel(scanPtr, x, y, imageDims);

			// Determine what to mask off.
			if ((_options & MaskOptions.Color) == 0)
				maskColor = Drawing.Color.FromArgb(_maskColor.A, sourceColor);
			if ((_options & MaskOptions.Alpha) == 0)
				maskColor = Drawing.Color.FromArgb(sourceColor.A, maskColor);

			return sourceColor == maskColor;
		}

		/// <summary>
		/// Function to retrieve the GDI image from a Gorgon image.
		/// </summary>
		/// <param name="image">Image to extract from.</param>
		public void GetGDIImage(Image image)
		{
			try
			{
				if (_image != null)
					_image.Dispose();
				_image = null;

				// Send to the image.
				_image = image.SaveBitmap() as Drawing.Bitmap;
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotCreate, "Cannot create the GDI+ image for extraction.", ex);
			}
		}

		/// <summary>
		/// Function to extract a single sprite rectangle.
		/// </summary>
		/// <param name="scanPtr">Scanline pointer from the image.</param>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="imageDims">Image width & height.</param>
		/// <returns>A rectangle.</returns>
		private unsafe Drawing.RectangleF GetSpriteRectangle(int *scanPtr, int x, int y, Drawing.Size imageDims)
		{
			Drawing.SizeF size = Drawing.SizeF.Empty;				// Width & height of the rectangle.
			Drawing.PointF origin = Drawing.PointF.Empty;			// Upper left corner of the rectangle.
			int *scanLine;											// Scanline pointer.

			// Get the scan line.
			scanLine = scanPtr + (y * (imageDims.Width)) + x;
			origin = new Drawing.PointF(x, y);

			// Get the width.
			for (int width = 0; width < (imageDims.Width - x); width++)
			{
				if (ColorIsMask(scanLine, width, 0, imageDims))
					break;

				size.Width = width + 1;
			}

			// Get the height.
			for (int height = 0; height < (imageDims.Height - y); height++)
			{
				if (ColorIsMask(scanLine, 0, height, imageDims))
					break;

				size.Height = height + 1;
			}

			// Return the rectangle.
			return new Drawing.RectangleF(origin, size);
		}

		/// <summary>
		/// Function to update a rectangle for a sprite.
		/// </summary>
		/// <param name="sprite">Sprite to update.</param>
		public void UpdateRectangle(Sprite sprite)
		{
			Drawing.RectangleF sourceRect = sprite.ImageRegion;		// Source rectangle.			
			Drawing.Imaging.BitmapData bmpData = null;				// Bitmap data.
			int x = 0;												// Horizontal position.
			int y = 0;												// Vertical position.
			Drawing.Size imageDims;									// Image dimensions.

			try
			{
				// Get the image from the sprite.
				GetGDIImage(sprite.Image);

				// Get the image width & height.
				imageDims = _image.Size;

				// Put a lock on the image.
				bmpData = _image.LockBits(new Drawing.Rectangle(0, 0, imageDims.Width, imageDims.Height), Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				unsafe
				{
					int* scanPtr = (int*)bmpData.Scan0.ToPointer();		// Scanline pointer.

					// Get each corner.
					x = (int)sourceRect.X;
					y = (int)sourceRect.Y;

					// Go through the image.
					while (y < sourceRect.Bottom)
					{
						while (x < sourceRect.Right)
						{
							if (!ColorIsMask(scanPtr, x, y, imageDims))
							{
								sprite.ImageRegion = GetSpriteRectangle(scanPtr, x, y, imageDims);

								// Force a break out of the loops.
								x = imageDims.Width + 1;
								y = imageDims.Height + 1;
							}							

							x++;
						}

						x = (int)sourceRect.X;
						y++;
					}
				}				
			}
			finally
			{
				if ((bmpData != null) && (_image != null))
					_image.UnlockBits(bmpData);

				bmpData = null;
			}
		}

		/// <summary>
		/// Function to extract the rectangles based on the grid parameters passed in.
		/// </summary>
		/// <returns>A list of rectangles.</returns>
		public Drawing.RectangleF[] GetGridRectangles()
		{
			List<Drawing.RectangleF> rectangles = null;			// The list of rectangles.
			Drawing.Rectangle constraint;						// Constraint.

			// Ensure the constraints fit within the image.
			constraint = _gridConstraint;
			if (constraint.X >= _image.Width)
				constraint.X = _image.Width - 1;
			if (constraint.Y >= _image.Height)
				constraint.Y = _image.Height - 1;

			if ((constraint.Width == 0) || (constraint.Right > _image.Width))
				constraint.Width = _image.Width - constraint.Left;

			if ((constraint.Height == 0) || (constraint.Bottom > _image.Height))
				constraint.Height = _image.Height - constraint.Top;

			rectangles = new List<System.Drawing.RectangleF>();

			// Create the rectangle list.
			if (!_topToBottom)
			{
				for (int x = constraint.Left; x < constraint.Right; x += ((int)_gridCellSpacing.X) + ((int)_gridCell.X))
				{
					for (int y = constraint.Top; y < constraint.Bottom; y += ((int)_gridCellSpacing.Y) + ((int)_gridCell.Y))
						rectangles.Add(new Drawing.RectangleF(x, y, _gridCell.X, _gridCell.Y));
				}
			}
			else
			{
				for (int y = constraint.Top; y < constraint.Bottom; y += ((int)_gridCellSpacing.Y) + ((int)_gridCell.Y))
				{
					for (int x = constraint.Left; x < constraint.Right; x += ((int)_gridCellSpacing.X) + ((int)_gridCell.X))						
						rectangles.Add(new Drawing.RectangleF(x, y, _gridCell.X, _gridCell.Y));
				}
			}

			return rectangles.ToArray();
		}

		/// <summary>
		/// Function to extract the rectangles.
		/// </summary>
		/// <returns>A list of sprite rectangles.</returns>
		public Drawing.RectangleF[] GetRectangles()
		{
			List<Drawing.RectangleF> rectangles = null;		// List of rectangles.
			Drawing.RectangleF rectangle;					// Rectangle.
			Drawing.Imaging.BitmapData bmpData = null;		// Bitmap data.
			int x = 0;										// Horizontal position.
			int y = 0;										// Vertical position.
			Drawing.Size imageDims;							// Image dimensions.
			bool add = true;								// Flag to add a rectangle.

			try
			{
				// Begin rectangle list.
				rectangles = new List<System.Drawing.RectangleF>();
				rectangle = Drawing.RectangleF.Empty;

				// Get the image width & height.
				imageDims = _image.Size;

				// Put a lock on the image.
				bmpData = _image.LockBits(new Drawing.Rectangle(0, 0, imageDims.Width, imageDims.Height), Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				unsafe
				{
					int* scanPtr = (int*)bmpData.Scan0.ToPointer();		// Scanline pointer.

					// Get each corner.
					x = 0;
					y = 0;

					// Go through the image.
					while (y < imageDims.Height)
					{
						while (x < imageDims.Width)
						{
							add = true;

							// This is SOOO inefficient, but it'll guarantee we don't get overlap.
							// Don't do anything if this point already exists within a rectangle.
							add = (rectangles.Where((baseRectangle) => baseRectangle.Contains(new Drawing.PointF(x, y)))).Count() > 0;
							if ((add) && (!ColorIsMask(scanPtr, x, y, imageDims)))
							{
								rectangle = GetSpriteRectangle(scanPtr, x, y, imageDims);								
								rectangles.Add(rectangle);
							}
							
							x++;
						}

						x = 0;						
						y++;
					}
				}

				return rectangles.ToArray();
			}
			finally
			{
				if ((bmpData != null) && (_image != null))
					_image.UnlockBits(bmpData);

				bmpData = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public SpriteFinder()
		{
			_maskColor = Drawing.Color.FromArgb(255, 255, 0, 255);
			_options = MaskOptions.Alpha | MaskOptions.Color;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_image != null)
					_image.Dispose();
			}

			// Do unmanaged clean up.
			_image = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
