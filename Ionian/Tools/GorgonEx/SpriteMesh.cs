#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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

namespace GorgonLibrary.Extras
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
		private VertexTypeList.PositionDiffuse2DTexture1[] _grid;		// List of grid vertices.
		private BoundingCircle _boundCircle = BoundingCircle.Empty;		// Bounding circle.
		private float _cosVal = 0.0f;									// Cached cosine.
		private float _sinVal = 0.0f;									// Cached sine.
		#endregion

		#region Properties.
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
		public override GorgonLibrary.Internal.PrimitiveStyle PrimitiveStyle
		{
			get 
			{
				return GorgonLibrary.Internal.PrimitiveStyle.TriangleList;
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
					_grid[i].Color = _color.ToArgb();
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
					_grid[i].Color = _color.ToArgb();
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

			result = vertex - Axis;
			result *= Scale;

			if (FinalRotation != 0.0f)
			{
				result.X = (result.X * cos - result.Y * sin);
				result.Y = (result.X * sin + result.Y * cos);
			}

			result += Position;

			return (Vector3D)result;
		}
		
		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			_colWidth = Width / (_cols - 1);
			_rowHeight = Height / (_rows - 1);

			for (int y = 0; y < _rows; y++)
			{
				for (int x = 0; x < _cols; x++)
				{
					_grid[y * _cols + x].TextureCoordinates = Vector2D.Divide(new Vector2D(ImageOffset.X + x * _colWidth, ImageOffset.Y + y * _rowHeight), new Vector2D(Width, Height));
					_grid[y * _cols + x].Position = new Vector2D(x * _colWidth, y * _rowHeight);
					_grid[y * _cols + x].Color = _color.ToArgb();
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
			if (sprite == null)
				throw new ArgumentNullException("sprite");
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			return new SpriteMesh(name, sprite.Image, rows, columns, sprite.ImageOffset.X, sprite.ImageOffset.Y, sprite.Size.X, sprite.Size.Y);;
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
			int index = 0;			// Index.
			_cosVal = 0.0f;	// Cached cosine.
			_sinVal = 0.0f;	// Cached sine.


			BeginRendering(flush);

			if ((IsSizeUpdated) || (IsImageUpdated))
				UpdateDimensions();

			if (FinalRotation != 0.0f)
			{
				float angle;		// Angle in radians.

				angle = MathUtility.Radians(FinalRotation);
				_cosVal = (float)Math.Cos(angle);
				_sinVal = (float)Math.Sin(angle);
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
					Vertices[0].Color = _grid[index].Color;

					Vertices[1].TextureCoordinates = _grid[index + 1].TextureCoordinates;
					Vertices[1].Position = TransformVertex(_grid[index + 1].Position, _cosVal, _sinVal);
					Vertices[1].Color = _grid[index+1].Color;

					Vertices[2].TextureCoordinates = _grid[index + _cols + 1].TextureCoordinates;
					Vertices[2].Position = TransformVertex(_grid[index + _cols + 1].Position, _cosVal, _sinVal);
					Vertices[2].Color = _grid[index + _cols + 1].Color;

					Vertices[3].TextureCoordinates = _grid[index + _cols].TextureCoordinates;
					Vertices[3].Position = TransformVertex(_grid[index + _cols].Position, _cosVal, _sinVal);
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
			_grid[row * _cols + column].Color = color.ToArgb();
		}

		/// <summary>
		/// Function to return a vertex color.
		/// </summary>
		/// <param name="column">Column index of the vertex.</param>
		/// <param name="row">Row index of the vertex.</param>
		/// <returns>Color to retrieve from the vertex.</returns>
		public Drawing.Color GetVertexColor(int column, int row)
		{
			return Drawing.Color.FromArgb(_grid[row * _cols + column].Color);
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
			Scale = new Vector2D(1, 1);
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
