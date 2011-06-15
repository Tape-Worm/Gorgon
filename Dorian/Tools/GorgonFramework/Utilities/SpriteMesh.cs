#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Monday, May 19, 2008 2:00:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Graphics.Utilities
{
	/// <summary>
	/// Object representing a deformable mesh for the sprites.
	/// </summary>
	public class SpriteMesh
		: Renderable
	{
		#region Variables.
		private string _deferredImage = string.Empty;					// Name of the deferred load image.
		private Drawing.Color _color = Drawing.Color.White;				// Color of the sprite.
		private float _colWidth = 0;									// Column width.
		private float _rowHeight = 0;									// Row height.
		private int _rows = 0;											// Number of rows in the mesh.
		private int _cols = 0;											// Number of columns in the mesh.
		private VertexTypeList.PositionDiffuse2DTexture1[] _grid;	// List of grid vertices.
		private BoundingCircle _boundCircle = BoundingCircle.Empty;		// Bounding circle.
		private float _cosVal = 0.0f;									// Cached cosine.
		private float _sinVal = 0.0f;									// Cached sine.
		private bool _flipHorizontal = false;							// Flag to flip horizontally.
		private bool _flipVertical = false;								// Flag to flip vertically.
		private Vector2D _imageOffset = Vector2D.Zero;					// Image offset.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the offset within the source image to start drawing from.
		/// </summary>
		public override Vector2D ImageOffset
		{
			get
			{
				return _imageOffset;
			}
			set
			{
				// If we haven't changed, then do nothing.
				if (value == _imageOffset)
					return;

				_imageOffset = value;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the region of the image that the sprite will use.
		/// </summary>
		public Drawing.RectangleF ImageRegion
		{
			get
			{
				return new Drawing.RectangleF(_imageOffset.X, _imageOffset.Y, Size.X, Size.Y);
			}
			set
			{
				_imageOffset.X = value.X;
				_imageOffset.Y = value.Y;
				SetSize(value.Width, value.Height);

				IsImageUpdated = true;
				IsAABBUpdated = true;
				IsSizeUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite is flipped horizontally or not.
		/// </summary>
		public bool HorizontalFlip
		{
			get
			{
				return _flipHorizontal;
			}
			set
			{
				_flipHorizontal = value;
				IsSizeUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite is flipped vertically or not.
		/// </summary>
		public bool VerticalFlip
		{
			get
			{
				return _flipVertical;
			}
			set
			{
				_flipVertical = value;
				IsSizeUpdated = true;
			}
		}

		/// <summary>
		/// Property to return the bounding circle of the sprite.
		/// </summary>
		public BoundingCircle BoundingCircle
		{
			get
			{
				if (this.IsAABBUpdated)
					UpdateAABB();
				return _boundCircle;
			}
		}

		/// <summary>
		/// Property to set or return the number of rows in the mesh.
		/// </summary>
		public int Rows
		{
			get
			{
				return _rows;
			}
			set
			{
				if (_rows < 1)
					throw new ArgumentOutOfRangeException("Rows cannot be less than 1.");

				_rows = value;

				_grid = new VertexTypeList.PositionDiffuse2DTexture1[_cols * _rows];
				IsSizeUpdated = true;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to return the width of the columns (in pixels) in the mesh.
		/// </summary>
		public float ColumnWidth
		{
			get
			{
				if (IsSizeUpdated)
					UpdateDimensions();
				return _colWidth;
			}
		}

		/// <summary>
		/// Property to return the height of the rows (in pixels) in the mesh.
		/// </summary>
		public float RowHeight
		{
			get
			{
				if (IsSizeUpdated)
					UpdateDimensions();
				return _rowHeight;
			}
		}

		/// <summary>
		/// Property to set or return the number of columns in the mesh.
		/// </summary>
		public int Columns
		{
			get
			{
				return _cols;
			}
			set
			{
				if (_cols < 1)
					throw new ArgumentOutOfRangeException("Columns cannot be less than 1.");

				_cols = value;

				_grid = new VertexTypeList.PositionDiffuse2DTexture1[_cols * _rows];
				IsSizeUpdated = true;
				IsImageUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		/// <value></value>
		public override Image Image
		{
			get
			{
				// Ensure that we have the image.
				if (_deferredImage != string.Empty)
				{
					if (ImageCache.Images.Contains(_deferredImage))
					{
						_deferredImage = string.Empty;
						base.Image = ImageCache.Images[_deferredImage];
					}
					else
						base.Image = null;
				}

				return base.Image;
			}
			set
			{
				base.Image = value;
				_deferredImage = string.Empty;
			}
		}

		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		/// <value></value>
		public override float UniformScale
		{
			get
			{
				return Scale.X;
			}
			set
			{
				Scale = new Vector2D(value, value);				
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		/// <value></value>
		public override PrimitiveStyle PrimitiveStyle
		{
			get 
			{
				return PrimitiveStyle.TriangleList;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		/// <value></value>
		public override bool UseIndices
		{
			get 
			{
				return true;
			}
		}

		/// <summary>
		/// Property to set or return the color.
		/// </summary>
		public override System.Drawing.Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
				for (int i = 0; i < _grid.Length; i++)
					_grid[i].Color = _color;
			}
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		/// <value></value>
		public override int Opacity
		{
			get
			{
				return (int)_color.A;
			}
			set
			{
				_color = Drawing.Color.FromArgb(value, _color);

				for (int i = 0; i < _grid.Length; i++)
					_grid[i].Color = _color;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Funciton to transform a vertex.
		/// </summary>
		/// <param name="vertex">Vertex to transform.</param>
		/// <param name="cos">Cosine for rotation.</param>
		/// <param name="sin">Sine for rotation.</param>
		/// <returns>The transformed vertex.</returns>
		private Vector3D TransformVertex(Vector2D vertex, float cos, float sin)
		{
			Vector2D result = Vector2D.Zero;			// Result vertex.

			result = Vector2D.Subtract(vertex,Axis);
			result = Vector2D.Multiply(FinalScale, result);

			if (FinalRotation != 0.0f)
				result = new Vector2D(result.X * cos - result.Y * sin, result.X * sin + result.Y * cos);

			result = Vector2D.Add(FinalPosition, result);			

			return (Vector3D)result;
		}

		/// <summary>
		/// Function to return the number of vertices for this object.
		/// </summary>
		/// <returns>
		/// An array of vertices used for this renderable.
		/// </returns>
		/// <remarks>This function is not implemented for a Sprite Mesh and will throw an exception when called.  
		/// Do not use this with a <see cref="GorgonLibrary.Graphics.Batch">Batch</see> object or an exception will be thrown.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when using a SpriteMesh with a Batch object.</exception>
		protected override BatchVertex[] GetVertices()
		{
			throw new NotSupportedException("This functionality is not supported by the SpriteMesh object.");
		}
		
		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			float imageWidth = Width;				// Image width to use in calculation
			float imageHeight = Height;				// Image height to use in calculation.
			float imageCellWidth = 0.0f;			// Image cell width.
			float imageCellHeight = 0.0f;			// Image cell height.
			Vector2D texPosition = Vector2D.Zero;	// Texture coordinate position.


			if (Image != null)
			{
				imageWidth = Image.Width;
				imageHeight = Image.Height;
			}

			_colWidth = Width / (_cols - 1);
			_rowHeight = Height / (_rows - 1);

			for (int y = 0; y < _rows; y++)
			{
				for (int x = 0; x < _cols; x++)
				{
					if (IsImageUpdated)
					{
						// Save us the extra calculations if we don't have a texture.
						if (Image != null)
						{
							imageCellHeight = (y * _rowHeight);
							imageCellWidth = (x * _colWidth);

							if (_flipHorizontal)
								imageCellWidth = imageWidth - imageCellWidth - ImageOffset.X;
							else
								imageCellWidth += ImageOffset.X;

							if (_flipVertical)
								imageCellHeight = imageHeight - imageCellHeight - ImageOffset.Y;
							else
								imageCellHeight += ImageOffset.Y;

							_grid[y * _cols + x].TextureCoordinates = Vector2D.Divide(
								new Vector2D(imageCellWidth, imageCellHeight),
								new Vector2D(imageWidth, imageHeight));
						}
						else
							_grid[y * _cols + x].TextureCoordinates = Vector2D.Zero;
					}

					_grid[y * _cols + x].Position = new Vector2D(x * _colWidth, y * _rowHeight);
					_grid[y * _cols + x].Color = _color;
					_grid[y * _cols + x].Position.Z = Depth;
				}
			}

			IsAABBUpdated = true;
			IsImageUpdated = false;
			IsSizeUpdated = false;
		}

		/// <summary>
		/// Function to create a new sprite mesh from an existing sprite.
		/// </summary>
		/// <param name="name">Name of the new sprite mesh.</param>
		/// <param name="sprite">Sprite to duplicate.</param>
		/// <param name="rows">Number of rows.</param>
		/// <param name="columns">Number of columns.</param>
		/// <returns>A new sprite mesh.</returns>
		public static SpriteMesh FromSprite(string name, Sprite sprite, int rows, int columns)
		{
			SpriteMesh result = null;			// New sprite mesh.

			if (sprite == null)
				throw new ArgumentNullException("sprite");
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			
			result = new SpriteMesh(name, sprite.Image, rows, columns, sprite.ImageOffset.X, sprite.ImageOffset.Y, sprite.Size.X, sprite.Size.Y);

			result.AlphaMaskFunction = sprite.AlphaMaskFunction;
			result.AlphaMaskValue = sprite.AlphaMaskValue;
			result.Axis = sprite.Axis;
			result.BlendingMode = sprite.BlendingMode;
			result.Color = sprite.Color;
			result.Depth = sprite.Depth;
			result.DepthBufferBias = sprite.DepthBufferBias;
			result.DepthTestFunction = sprite.DepthTestFunction;
			result.DepthWriteEnabled = sprite.DepthWriteEnabled;
			result.DestinationBlend = sprite.DestinationBlend;
			result.HorizontalWrapMode = sprite.HorizontalWrapMode;
			result.Opacity = sprite.Opacity;
			result.Position = sprite.Position;
			result.Rotation = sprite.Rotation;
			result.Scale = sprite.Scale;
			result.Smoothing = sprite.Smoothing;
			result.SourceBlend = sprite.SourceBlend;
			result.StencilCompare = sprite.StencilCompare;
			result.StencilEnabled = sprite.StencilEnabled;
			result.StencilFailOperation = sprite.StencilFailOperation;
			result.StencilMask = sprite.StencilMask;
			result.StencilPassOperation = sprite.StencilPassOperation;
			result.StencilReference = sprite.StencilReference;
			result.StencilZFailOperation = sprite.StencilZFailOperation;
			result.VerticalWrapMode = sprite.VerticalWrapMode;
			result.HorizontalFlip = sprite.HorizontalFlip;
			result.VerticalFlip = sprite.VerticalFlip;
			result.BorderColor = sprite.BorderColor;

			result.InheritAlphaMaskFunction = sprite.InheritAlphaMaskFunction;
			result.InheritAlphaMaskValue = sprite.InheritAlphaMaskValue;
			result.InheritBlending = sprite.InheritBlending;
			result.InheritDepthBias  = sprite.InheritDepthBias;
			result.InheritDepthTestFunction = sprite.InheritDepthTestFunction;
			result.InheritDepthWriteEnabled = sprite.InheritDepthWriteEnabled;
			result.InheritHorizontalWrapping = sprite.InheritHorizontalWrapping;
			result.InheritSmoothing = sprite.InheritSmoothing;
			result.InheritStencilCompare = sprite.InheritStencilCompare;
			result.InheritStencilEnabled = sprite.InheritStencilEnabled;
			result.InheritStencilFailOperation = sprite.InheritStencilFailOperation;
			result.InheritStencilMask = sprite.InheritStencilMask;
			result.InheritStencilPassOperation = sprite.InheritStencilPassOperation;
			result.InheritStencilReference = sprite.InheritStencilReference;
			result.InheritStencilZFailOperation = sprite.InheritStencilZFailOperation;
			result.InheritVerticalWrapping = sprite.InheritVerticalWrapping;

			return result;
		}

		/// <summary>
		/// Function to create a new sprite mesh from an existing sprite.
		/// </summary>
		/// <param name="sprite">Sprite to duplicate.</param>
		/// <param name="rows">Number of rows.</param>
		/// <param name="columns">Number of columns.</param>
		/// <returns>A new sprite mesh.</returns>
		public static SpriteMesh FromSprite(Sprite sprite, int rows, int columns)
		{
			return FromSprite(sprite.Name, sprite, rows, columns);
		}
		
		/// <summary>
		/// Function to reset the mesh transformation.
		/// </summary>
		public void Reset()
		{
			UpdateDimensions();
		}

		/// <summary>
		/// Function to update AABB.
		/// </summary>
		public override void UpdateAABB()
		{
			Vector2D max = new Vector2D(float.MinValue, float.MinValue);	// Max boundary for the AABB.
			Vector2D min = new Vector2D(float.MaxValue, float.MaxValue);	// Min boundary for the AABB.
			Vector3D transformed = Vector3D.Zero;							// Transformed vertex.
			float xradius = 0.0f;											// Bounding circle radius. 
			float yradius = 0.0f;											// Bounding circle radius.

			base.UpdateAABB();

			for (int i = 0; i < _grid.Length; i++)
			{
				// Create bounding.			
				transformed = TransformVertex(_grid[i].Position, _cosVal, _sinVal);
				min.X = MathUtility.Min(min.X, transformed.X);
				min.Y = MathUtility.Min(min.Y, transformed.Y);
				max.X = MathUtility.Max(max.X, transformed.X);
				max.Y = MathUtility.Max(max.Y, transformed.Y);
			}

			SetAABB(min, max);
			_boundCircle.Center = new Vector2D(min.X + (max.X - min.X) / 2.0f, min.Y + (max.Y - min.Y) / 2.0f);
			xradius = MathUtility.Abs(max.X - min.X) / 2.0f;
			yradius = MathUtility.Abs(max.Y - min.Y) / 2.0f;
			if (xradius > yradius)
				_boundCircle.Radius = xradius;
			else
				_boundCircle.Radius = yradius;
			IsAABBUpdated = false;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public override void Draw(bool flush)
		{
			int index = 0;	// Index.
			_cosVal = 0.0f;	// Cached cosine.
			_sinVal = 0.0f;	// Cached sine.


			BeginRendering(flush);

			if ((IsSizeUpdated) || (IsImageUpdated))
				UpdateDimensions();

			if (FinalRotation != 0.0f)
			{
				float angle;		// Angle in radians.

				angle = MathUtility.Radians(FinalRotation);
				_cosVal = MathUtility.Cos(angle);
				_sinVal = MathUtility.Sin(angle);
			}

			if (IsAABBUpdated)
				UpdateAABB();

			for (int y = 0; y < _rows - 1; y++)
			{
				for (int x = 0; x < _cols - 1; x++)
				{
					index = y * _cols + x;

					Vertices[0].TextureCoordinates = _grid[index].TextureCoordinates;
					Vertices[0].Position = TransformVertex(_grid[index].Position, _cosVal, _sinVal);
					Vertices[0].Position.Z = -Depth;
					Vertices[0].Color = _grid[index].Color;

					Vertices[1].TextureCoordinates = _grid[index + 1].TextureCoordinates;
					Vertices[1].Position = TransformVertex(_grid[index + 1].Position, _cosVal, _sinVal);
					Vertices[1].Position.Z = -Depth;
					Vertices[1].Color = _grid[index+1].Color;

					Vertices[2].TextureCoordinates = _grid[index + _cols + 1].TextureCoordinates;
					Vertices[2].Position = TransformVertex(_grid[index + _cols + 1].Position, _cosVal, _sinVal);
					Vertices[2].Position.Z = -Depth;
					Vertices[2].Color = _grid[index + _cols + 1].Color;

					Vertices[3].TextureCoordinates = _grid[index + _cols].TextureCoordinates;
					Vertices[3].Position = TransformVertex(_grid[index + _cols].Position, _cosVal, _sinVal);
					Vertices[3].Position.Z = -Depth;
					Vertices[3].Color = _grid[index + _cols].Color;

					if (WriteVertexData(0, 4) + 4 >= BufferSize)
						FlushToRenderer();
				}
			}

			EndRendering(flush);
		}

		/// <summary>
		/// Function to set a vertex color.
		/// </summary>
		/// <param name="column">Column index of the vertex.</param>
		/// <param name="row">Row index of the vertex.</param>
		/// <param name="color">Color to set at the vertex.</param>
		public void SetVertexColor(int column, int row, Drawing.Color color)
		{
			_grid[row * _cols + column].Color = color;
		}

		/// <summary>
		/// Function to return a vertex color.
		/// </summary>
		/// <param name="column">Column index of the vertex.</param>
		/// <param name="row">Row index of the vertex.</param>
		/// <returns>Color to retrieve from the vertex.</returns>
		public Drawing.Color GetVertexColor(int column, int row)
		{
			return _grid[row * _cols + column].Color;
		}

		/// <summary>
		/// Function to set a vertex position.
		/// </summary>
		/// <param name="column">Column index of the vertex.</param>
		/// <param name="row">Row index of the vertex.</param>
		/// <param name="deltaX">Horizontal delta offset of the vertex.</param>
		/// <param name="deltaY">Vertical Delta offset of the vertex.</param>
		public void SetVertexPosition(int column, int row, float deltaX, float deltaY)
		{
			_grid[row * _cols + column].Position = new Vector2D(deltaX, deltaY);
			IsAABBUpdated = true;
		}

		/// <summary>
		/// Function to return the position for a vertex at the specified column and row.
		/// </summary>
		/// <param name="column">Column for the vertex.</param>
		/// <param name="row">Row for the vertex.</param>
		/// <returns>The value for the vertex.</returns>
		public Vector2D GetVertexPosition(int column, int row)
		{
			return _grid[row * _cols + column].Position;
		}

		/// <summary>
		/// Function to return the transformed position for a vertex at the specified column and row.
		/// </summary>
		/// <param name="column">Column for the vertex.</param>
		/// <param name="row">Row for the vertex.</param>
		/// <returns>The value for the vertex.</returns>
		public Vector2D GetTransformedVertexPosition(int column, int row)
		{
			float sin = 0.0f;		// Sine for rotation.
			float cos = 0.0f;		// Cosine for rotation.

			if (FinalRotation != 0.0f)
			{
				float angle;		// Angle in radians.
				
				angle = MathUtility.Radians(FinalRotation);
				cos = (float)Math.Cos(angle);
				sin = (float)Math.Sin(angle);
			}

			return TransformVertex(_grid[row * _cols + column].Position, cos, sin);
		}

		/// <summary>
		/// Function to set the depth of a specific vertex.
		/// </summary>
		/// <param name="column">Column index of the vertex.</param>
		/// <param name="row">Row index of the vertex.</param>
		/// <param name="depth">Depth of the vertex.</param>
		public void SetVertexDepth(int column, int row, float depth)
		{
			if (depth < 0.0f)
				depth = 0.0f;
			if (depth > 1.0f)
				depth = 1.0f;

			_grid[row * _cols + column].Position.Z = depth;
		}

		/// <summary>
		/// Function to return the depth value for the vertex at the specified column and row.
		/// </summary>
		/// <param name="column">Column index of the vertex.</param>
		/// <param name="row">Row index of the vertex.</param>
		/// <returns>The depth value for the vertex.</returns>
		public float GetVertexDepth(int column, int row)
		{
			return _grid[row * _cols + column].Position.Z;
		}
		
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override Renderable Clone()
		{
			SpriteMesh clone = new SpriteMesh(Name + ".Clone", Image, _rows, _cols, ImageOffset.X, ImageOffset.Y, Width, Height);

			clone.ImageOffset = ImageOffset;
			clone.Size = Size;
			clone.Position = Position;
			clone.Rotation = Rotation;
			clone.Scale = Scale;
			clone.Axis = Axis;
			clone.SetAABB(AABB);
			clone.ParentPosition = ParentPosition;
			clone.ParentRotation = ParentRotation;
			clone.ParentScale = ParentScale;
			clone.HorizontalFlip = HorizontalFlip;
			clone.VerticalFlip = VerticalFlip;
			clone.BorderColor = BorderColor;

			if (!InheritSmoothing)
				clone.Smoothing = Smoothing;
			if (!InheritBlending)
			{
				clone.BlendingMode = BlendingMode;
				clone.SourceBlend = SourceBlend;
				clone.DestinationBlend = DestinationBlend;
			}
			clone.Depth = Depth;
			if (!InheritDepthBias)
				clone.DepthBufferBias = DepthBufferBias;
			if (!InheritDepthTestFunction)
				clone.DepthTestFunction = DepthTestFunction;
			if (!InheritDepthWriteEnabled)
				clone.DepthWriteEnabled = DepthWriteEnabled;
			if (!InheritAlphaMaskFunction)
				clone.AlphaMaskFunction = AlphaMaskFunction;
			if (!InheritAlphaMaskValue)
				clone.AlphaMaskValue = AlphaMaskValue;
			if (!InheritHorizontalWrapping)
				clone.HorizontalWrapMode = HorizontalWrapMode;
			if (!InheritVerticalWrapping)
				clone.VerticalWrapMode = VerticalWrapMode;
			if (!InheritStencilPassOperation)
				clone.StencilPassOperation = StencilPassOperation;
			if (!InheritStencilFailOperation)
				clone.StencilFailOperation = StencilFailOperation;
			if (!InheritStencilZFailOperation)
				clone.StencilZFailOperation = StencilZFailOperation;
			if (!InheritStencilCompare)
				clone.StencilCompare = StencilCompare;
			if (!InheritStencilEnabled)
				clone.StencilEnabled = StencilEnabled;
			if (!InheritStencilReference)
				clone.StencilReference = StencilReference;
			if (!InheritStencilMask)
				clone.StencilMask = StencilMask;
			for (int i = 0; i < _grid.Length; i++)
				clone._grid[i] = _grid[i];

			return clone;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteMesh"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite mesh.</param>
		/// <param name="spriteImage">The image used for the mesh.</param>
		/// <param name="rows">The number of rows in the mesh.</param>
		/// <param name="cols">The number of columns in the mesh.</param>
		/// <param name="imageOffsetX">Horizontal offset of the starting point in the bound image.</param>
		/// <param name="imageOffsetY">Vertical offset of the starting point in the bound image.</param>
		/// <param name="width">The width of the mesh.</param>
		/// <param name="height">The height of the mesh.</param>
		public SpriteMesh(string name, Image spriteImage, int rows, int cols, float imageOffsetX, float imageOffsetY, float width, float height)
			: base(name)
		{
			if (rows < 1)
				throw new ArgumentOutOfRangeException("Rows cannot be less than 1.");
			if (cols < 1)
				throw new ArgumentOutOfRangeException("Columns cannot be less than 1.");
						
			_rows = rows;
			_cols = cols;
			ImageOffset = new Vector2D(imageOffsetX, imageOffsetY);
			Width = width;
			Height = height;
			
			InitializeVertices(4);
			// Create a single cell.
			_grid = new VertexTypeList.PositionDiffuse2DTexture1[rows * cols];
			Image = spriteImage;
			Scale = Vector2D.Unit;
			IsAABBUpdated = true;
			IsSizeUpdated = true;
			IsImageUpdated = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteMesh"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite mesh.</param>
		/// <param name="spriteImage">The image used for the mesh.</param>
		/// <param name="rows">The number of rows in the mesh.</param>
		/// <param name="cols">The number of columns in the mesh.</param>
		/// <param name="imageOffsetX">Horizontal offset of the starting point in the bound image.</param>
		/// <param name="imageOffsetY">Vertical offset of the starting point in the bound image.</param>
		/// <param name="width">The width of the mesh.</param>
		/// <param name="height">The height of the mesh.</param>
		public SpriteMesh(string name, RenderImage spriteImage, int rows, int cols, float imageOffsetX, float imageOffsetY, float width, float height)
			: this(name, spriteImage.Image, rows, cols, imageOffsetX, imageOffsetY, width, height)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteMesh"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite mesh.</param>
		/// <param name="spriteImage">The image used for the mesh.</param>
		/// <param name="rows">The number of rows in the mesh.</param>
		/// <param name="cols">The number of columns in the mesh.</param>
		/// <param name="imageOffsetX">Horizontal offset of the starting point in the bound image.</param>
		/// <param name="imageOffsetY">Vertical offset of the starting point in the bound image.</param>
		public SpriteMesh(string name, Image spriteImage, int rows, int cols, float imageOffsetX, float imageOffsetY)
			: this(name, spriteImage, rows, cols, imageOffsetX, imageOffsetY, spriteImage.Width, spriteImage.Height)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteMesh"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite mesh.</param>
		/// <param name="spriteImage">The image used for the mesh.</param>
		/// <param name="rows">The number of rows in the mesh.</param>
		/// <param name="cols">The number of columns in the mesh.</param>
		/// <param name="imageOffsetX">Horizontal offset of the starting point in the bound image.</param>
		/// <param name="imageOffsetY">Vertical offset of the starting point in the bound image.</param>
		public SpriteMesh(string name, RenderImage spriteImage, int rows, int cols, float imageOffsetX, float imageOffsetY)
			: this(name, spriteImage, rows, cols, imageOffsetX, imageOffsetY, spriteImage.Width, spriteImage.Height)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteMesh"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite mesh.</param>
		/// <param name="spriteImage">The image used for the mesh.</param>
		/// <param name="rows">The number of rows in the mesh.</param>
		/// <param name="cols">The number of columns in the mesh.</param>
		public SpriteMesh(string name, Image spriteImage, int rows, int cols)
			: this(name, spriteImage, rows, cols, 0, 0, spriteImage.Width, spriteImage.Height)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteMesh"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite mesh.</param>
		/// <param name="spriteImage">The image used for the mesh.</param>
		/// <param name="rows">The number of rows in the mesh.</param>
		/// <param name="cols">The number of columns in the mesh.</param>
		public SpriteMesh(string name, RenderImage spriteImage, int rows, int cols)
			: this(name, spriteImage, rows, cols, 0, 0, spriteImage.Width, spriteImage.Height)
		{
		}
		#endregion
	}
}
