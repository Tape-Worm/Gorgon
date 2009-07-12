#region MIT.
// 
// Gorgon.
// Copyright (C) 2009 Michael Winsor
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
// Created: Friday, May 29, 2009 10:48:12 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An arbitrary n-sided polygon sprite.
	/// </summary>
	/// <remarks>This allows the user to generate an arbitrarily shaped sprite.
	/// <para>This is an advanced object, be sure to review how triangle primitives work in the Direct3D documentation located here: http://msdn.microsoft.com/en-us/library/bb219837(VS.85).aspx.</para></remarks>
	public class PolygonSprite
		: NamedObject, IDisposable, IDeviceStateObject, IRenderableStates
	{
		#region Classes.
		/// <summary>
		/// A list of polygon indices.
		/// </summary>
		public class IndexList
			: BaseDynamicArray<int>, IList<int>
		{
			#region Variables.
			private PolygonSprite _polygon = null;			// Polygon that owns the indices.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to add a range of indices.
			/// </summary>
			/// <param name="items">Indices to add to the collection.</param>
			public void AddRange(int[] items)
			{
				if ((items == null) || (items.Length == 0))
					return;
				for (int i = 0; i < items.Length - 1; i++)
					Add(items[i]);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Function to remove a vertex from the list by its index (note, this is the collection index, not the vertex index).
			/// </summary>
			/// <param name="index">Collection index of the vertex index to remove.</param>
			public void Remove(int index)
			{
				RemoveItem(index);
				_polygon.NeedsUpdate = true;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="IndexList"/> class.
			/// </summary>
			/// <param name="polygon">Polygon that owns the indices.</param>
			internal IndexList(PolygonSprite polygon)
				: base(4)
			{
				if (polygon == null)
					throw new ArgumentNullException("polygon");

				_polygon = polygon;
			}
			#endregion

			#region IList<int> Members
			/// <summary>
			/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			/// <returns>
			/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
			/// </returns>
			public int IndexOf(int item)
			{
				return Items.IndexOf(item);
			}

			/// <summary>
			/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
			/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </exception>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			/// </exception>
			public void Insert(int index, int item)
			{
				Items.Insert(index, item);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the item to remove.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </exception>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			/// </exception>
			void IList<int>.RemoveAt(int index)
			{
				Remove(index);
			}

			/// <summary>
			/// Gets or sets the <see cref="System.Int32"/> at the specified index.
			/// </summary>
			/// <value></value>
			public int this[int index]
			{
				get
				{
					return GetItem(index);
				}
				set
				{
					SetItem(index, value);
					_polygon.NeedsUpdate = true;
				}
			}
			#endregion

			#region ICollection<int> Members
			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </exception>
			public void Add(int item)
			{
				Items.Add(item);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </exception>
			public void Clear()
			{
				ClearItems();
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
			/// </summary>
			/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
			/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
			/// <exception cref="T:System.ArgumentNullException">
			/// 	<paramref name="array"/> is null.
			/// </exception>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="arrayIndex"/> is less than 0.
			/// </exception>
			/// <exception cref="T:System.ArgumentException">
			/// 	<paramref name="array"/> is multidimensional.
			/// -or-
			/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
			/// -or-
			/// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
			/// -or-
			/// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
			/// </exception>
			public void CopyTo(int[] array, int arrayIndex)
			{
				Items.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </summary>
			/// <value></value>
			/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
			/// </returns>
			public bool IsReadOnly
			{
				get 
				{ 
					return false; 
				}
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </returns>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </exception>
			bool ICollection<int>.Remove(int item)
			{
				Remove(item);
				return true;
			}
			#endregion
		}

		/// <summary>
		/// A list of polygon vertices.
		/// </summary>
		public class PolygonVertexList
			: BaseDynamicArray<VertexTypeList.PositionDiffuse2DTexture1>, IList<VertexTypeList.PositionDiffuse2DTexture1>
		{
			#region Variables.
			private PolygonSprite _polygon = null;			// Polygon that owns the vertices.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to set the vertex position at the vertex specified by the index.
			/// </summary>
			/// <param name="vertexIndex">The index of the vertex to update.</param>
			/// <param name="position">Position of the vertex.</param>
			public void SetVertexPosition(int vertexIndex, Vector3D position)
			{
				Items[vertexIndex] = new VertexTypeList.PositionDiffuse2DTexture1(position, Items[vertexIndex].Color, Items[vertexIndex].TextureCoordinates);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Function to set the vertex texture coordinates at the vertex specified by the index.
			/// </summary>
			/// <param name="vertexIndex">The index of the vertex to update.</param>
			/// <param name="coordinates">Texture coordinates of the vertex.</param>
			public void SetVertexTextureCoordinates(int vertexIndex, Vector2D coordinates)
			{
				Items[vertexIndex] = new VertexTypeList.PositionDiffuse2DTexture1(Items[vertexIndex].Position, Items[vertexIndex].Color, coordinates);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Function to set the color of the vertex specified by the index.
			/// </summary>
			/// <param name="vertexIndex">The index of the vertex to update.</param>
			/// <param name="color">Color of the vertex.</param>
			public void SetVertexColor(int vertexIndex, Drawing.Color color)
			{
				Items[vertexIndex] = new VertexTypeList.PositionDiffuse2DTexture1(Items[vertexIndex].Position, color, Items[vertexIndex].TextureCoordinates);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Function to add a range of vertices.
			/// </summary>
			/// <param name="items">Vertices to add to the collection.</param>
			public void AddRange(VertexTypeList.PositionDiffuse2DTexture1[] items)
			{
				if ((items == null) || (items.Length == 0))
					return;
				for (int i = 0; i < items.Length - 1; i++)
					Add(items[i]);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Function to add a vertex to the polygon.
			/// </summary>
			/// <param name="position">3D position of the vertex.  The Z value is used to interact with the depth buffer.</param>
			/// <param name="imageCoordinate">The relative coordinate of the image being mapped to the polygon.</param>
			/// <param name="diffuse">The diffuse color of the vertex.</param>
			/// <returns>The vertex that was added to the polygon.</returns>
			/// <remarks>The <paramref name="imageCoordinate"/> parameter is different from that of other objects that use image mapping (e.g. a <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see>).
			///   The other objects express their image mapping in absolute image coordinates, that is if a 32x32 image is used then the image is mapped from 0 to 32 for width and/or height.
			///   However in this case the image mapping is relative to the image.  This means that to map the aforementioned image you would pass 0.0 to 1.0.  Where 1.0 is the width or height 
			/// of our image.</remarks>
			public VertexTypeList.PositionDiffuse2DTexture1 AddVertex(Vector3D position, Vector2D imageCoordinate, Drawing.Color diffuse)
			{				
				VertexTypeList.PositionDiffuse2DTexture1 vertex = new VertexTypeList.PositionDiffuse2DTexture1(position, diffuse, imageCoordinate);
				Add(vertex);
				_polygon.NeedsUpdate = true;
				return vertex;
			}

			/// <summary>
			/// Function to remove a vertex from the list by its index.
			/// </summary>
			/// <param name="index">Index of the vertex to remove.</param>
			public void Remove(int index)
			{
				RemoveItem(index);
				_polygon.NeedsUpdate = true;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="PolygonVertexList"/> class.
			/// </summary>
			/// <param name="polygon">Polygon that owns the indices.</param>
			internal PolygonVertexList(PolygonSprite polygon)
				: base(4)
			{
				if (polygon == null)
					throw new ArgumentNullException("polygon");

				_polygon = polygon;
			}
			#endregion
			
			#region IList<PositionDiffuse2DTexture1> Members
			/// <summary>
			/// Gets or sets the <see cref="GorgonLibrary.Graphics.VertexTypeList.PositionDiffuse2DTexture1"/> at the specified index.
			/// </summary>
			/// <value></value>
			public VertexTypeList.PositionDiffuse2DTexture1 this[int index]
			{
				get
				{
					return GetItem(index);
				}
				set
				{
					SetItem(index, value);
					_polygon.NeedsUpdate = true;
				}
			}

			/// <summary>
			/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			/// <returns>
			/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
			/// </returns>
			public int IndexOf(VertexTypeList.PositionDiffuse2DTexture1 item)
			{
				return Items.IndexOf(item);
			}

			/// <summary>
			/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
			/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </exception>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			/// </exception>
			public void Insert(int index, VertexTypeList.PositionDiffuse2DTexture1 item)
			{
				Items.Insert(index, item);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the item to remove.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </exception>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			/// </exception>
			void IList<VertexTypeList.PositionDiffuse2DTexture1>.RemoveAt(int index)
			{
				Remove(index);
			}
			#endregion

			#region ICollection<PositionDiffuse2DTexture1> Members
			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </exception>
			public void Add(VertexTypeList.PositionDiffuse2DTexture1 item)
			{
				Items.Add(item);
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
			/// </summary>
			/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
			/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
			/// <exception cref="T:System.ArgumentNullException">
			/// 	<paramref name="array"/> is null.
			/// </exception>
			/// <exception cref="T:System.ArgumentOutOfRangeException">
			/// 	<paramref name="arrayIndex"/> is less than 0.
			/// </exception>
			/// <exception cref="T:System.ArgumentException">
			/// 	<paramref name="array"/> is multidimensional.
			/// -or-
			/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
			/// -or-
			/// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
			/// -or-
			/// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
			/// </exception>
			public void CopyTo(VertexTypeList.PositionDiffuse2DTexture1[] array, int arrayIndex)
			{
				Items.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </exception>
			public void Clear()
			{
				ClearItems();
				_polygon.NeedsUpdate = true;
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </summary>
			/// <value></value>
			/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
			/// </returns>
			public bool IsReadOnly
			{
				get 
				{
					return false;
				}
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </returns>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </exception>
			public bool Remove(VertexTypeList.PositionDiffuse2DTexture1 item)
			{
				return Items.Remove(item);
			}
			#endregion
		}

		#endregion

		#region Variables.
		private bool _disposed = false;									// Flag to indicate that we've disposed this object.
		private PolygonVertexList _vertices = null;						// Vertex list.
		private IndexList _indices = null;								// Index list.
		private VertexBuffer _vertexBuffer = null;						// Vertex buffer.
		private IndexBuffer _indexBuffer = null;						// Index buffer.
		private Smoothing _smoothing;									// Smoothing mode.
		private AlphaBlendOperation _sourceBlendOp;						// Alphablend source operation.
		private AlphaBlendOperation _destBlendOp;						// Alphablend destination operation.
		private BlendingModes _blendMode;								// Alpha blending preset mode.
		private ImageAddressing _hwrapMode;								// Horizontal wrapping mode.
		private ImageAddressing _vwrapMode;								// Vertical wrapping mode.
		private CompareFunctions _alphaCompareFunction;					// Alpha test compare function.
		private int _alphaTestCompareValue;								// Alpha test compare value.
		private StencilOperations _stencilPassOperation;				// Stencil pass operation.
		private StencilOperations _stencilFailOperation;				// Stencil fail operation.
		private StencilOperations _stencilZFailOperation;				// Stencil Z fail operation.
		private CompareFunctions _stencilCompare;						// Stencil compare operation.
		private int _stencilReference;									// Stencil reference value.
		private int _stencilMask;										// Stencil mask value.
		private bool _useStencil;										// Flag to indicate whether to use the stencil or not.
		private float _depthBias;										// Depth bias.
		private bool _depthWriteEnabled;								// Depth writing enabled flag.
		private CompareFunctions _depthCompare;							// Depth test comparison function.
		private Matrix _worldMatrix = Matrix.Identity;					// World matrix.
		private Drawing.RectangleF _aabb = Drawing.RectangleF.Empty;	// Axis aligned bounding box for the sprite.
		private Vector2D _size = Vector2D.Zero;							// Size of the polygon sprite.
		private bool _needsUpdate = false;								// Flag to indicate that we need to update.
		private bool _needsAABB = false;								// Flag to indicate that we need an AABB update.
		private float _rotation = 0.0f;									// Rotation angle in degrees.
		private Vector2D _scale = Vector2D.Unit;						// Scale of the sprite.
		private Vector3D _position = Vector3D.Zero;						// Position of the sprite.
		private Vector2D _axis = Vector2D.Zero;							// Pivot axis  for the sprite.
		private BoundingCircle _boundCircle = BoundingCircle.Empty;		// Bounding circle.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the buffers need updating.
		/// </summary>
		internal bool NeedsUpdate
		{
			get
			{
				return _needsUpdate;
			}
			set
			{
				_needsUpdate = value;
				_needsAABB = value;
			}
		}

		/// <summary>
		/// Property to return the width of the polygon sprite.
		/// </summary>
		public float Width
		{
			get
			{
				if (NeedsUpdate)
					CalculateWidthHeight();
				return _size.X;
			}
		}

		/// <summary>
		/// Property to return the height of the polygon sprite.
		/// </summary>
		public float Height
		{
			get
			{
				if (NeedsUpdate)
					CalculateWidthHeight();
				return _size.Y;
			}
		}

		/// <summary>
		/// Property to return the AABB rectangle for the sprite.
		/// </summary>
		public Drawing.RectangleF AABB
		{
			get
			{
				if (_needsAABB)
					CalculateAABB();
				return _aabb;
			}
		}

		/// <summary>
		/// Property to return the bounding circle for the sprite.
		/// </summary>
		public BoundingCircle BoundingCircle
		{
			get
			{
				if (_needsAABB)
					CalculateAABB();
				return _boundCircle;
			}
		}

		/// <summary>
		/// Property to set or return the rotation (in degrees) of the polygon.
		/// </summary>
		public float Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = value;
				_needsAABB = true;
			}
		}

		/// <summary>
		/// Property to set or return the position of the polygon.
		/// </summary>
		public Vector3D Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				_needsAABB = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the polygon.
		/// </summary>
		public Vector2D Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
				_needsAABB = true;
			}
		}

		/// <summary>
		/// Property to set or return the pivot axis of the polygon.
		/// </summary>
		public Vector2D Axis
		{
			get
			{
				return _axis;
			}
			set
			{
				_axis = value;
				_needsAABB = true;
			}
		}

		/// <summary>
		/// Property to set or return the uniform scale of the polygon.
		/// </summary>
		public float UniformScale
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
		/// Property to set or return the image to map to the polygon.
		/// </summary>
		public Image Image
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the depth bias.
		/// </summary>
		public bool InheritDepthBias
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the depth writing enabled flag.
		/// </summary>
		public virtual bool InheritDepthWriteEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the depth testing function.
		/// </summary>
		public virtual bool InheritDepthTestFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil enabled flag from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil reference from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilReference
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil mask from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil pass operation from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilPassOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil failed operation from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil z-failed operation from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilZFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil compare from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilCompare
		{
			get;
			set;
		}


		/// <summary>
		/// Property to set or return whether we inherit the alpha mask function from the layer.
		/// </summary>
		public bool InheritAlphaMaskFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask value from the layer.
		/// </summary>
		public virtual bool InheritAlphaMaskValue
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the horizontal wrapping from the layer.
		/// </summary>
		public bool InheritHorizontalWrapping
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether we inherit the vertical wrapping from the layer.
		/// </summary>
		public bool InheritVerticalWrapping
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to inherit the smoothing mode from the global states.
		/// </summary>
		public bool InheritSmoothing
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to inherit the blending mode from the global states.
		/// </summary>
		public bool InheritBlending
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the vertex list for this polygon.
		/// </summary>
		public PolygonVertexList PolygonVertices
		{
			get
			{
				return _vertices;
			}
		}

		/// <summary>
		/// Property to return the vertex index list for this polygon.
		/// </summary>
		public IndexList PolygonIndices
		{
			get
			{
				return _indices;
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		/// <value></value>
		public PrimitiveStyle PrimitiveStyle
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to refresh the vertex/index buffers.
		/// </summary>
		private void RefreshBuffers()
		{
			IndexBufferType indexType = (Gorgon.CurrentDriver.SupportIndex32) ? IndexBufferType.Index32 : IndexBufferType.Index16;

			if (_vertexBuffer != null)
			{
				_vertexBuffer.Dispose();
				_vertexBuffer = null;
			}

			if (_indexBuffer != null)
			{
				_indexBuffer.Dispose();
				_indexBuffer = null;
			}

			if (_vertices.Count < 1)
				return;

			if (_indices.Count < 1)
				return;

			_vertexBuffer = new VertexBuffer(Gorgon.Renderer.VertexTypes["PositionDiffuse2DTexture1"].VertexSize(0), _vertices.Count, BufferUsages.WriteOnly | BufferUsages.Dynamic);
			_indexBuffer = new IndexBuffer(indexType, _indices.Count, BufferUsages.WriteOnly | BufferUsages.Dynamic);
			NeedsUpdate = true;
		}

		/// <summary>
		/// Function to calculate the actual (untransformed) width and height of the polygon sprite.
		/// </summary>
		protected void CalculateWidthHeight()
		{
			if (NeedsUpdate)
			{
				Vector2D sizemin = Vector2D.Zero;
				Vector2D sizemax = Vector2D.Zero;

				sizemin = new Vector3D(float.MaxValue, float.MaxValue, 0.0f);
				sizemax = new Vector3D(float.MinValue, float.MinValue, 0.0f);

				for (int i = 0; i < _vertices.Count; i++)
				{
					Vector3D position = _vertices[i].Position;
					if (position.X < sizemin.X)
						sizemin.X = position.X;
					if (position.Y < sizemin.Y)
						sizemin.Y = position.Y;
					if (position.X > sizemax.X)
						sizemax.X = position.X;
					if (position.Y > sizemax.Y)
						sizemax.Y = position.Y;
				}

				_size = Vector2D.Subtract(sizemax, sizemin);
			}
		}

		/// <summary>
		/// Function to calculate the Axis Aligned bounding box for the polygon sprite.
		/// </summary>
		protected void CalculateAABB()
		{
			if (_needsAABB)
			{
				Vector3D min = Vector3D.Zero;
				Vector3D max = Vector3D.Zero;
				Matrix position = Matrix.Identity;
				Matrix rotation = Matrix.Identity;
				Matrix scale = Matrix.Identity;

				_worldMatrix = Matrix.Identity;
				position.Translate(Position);
				_worldMatrix.Translate(-Axis.X, -Axis.Y, 0.0f);

				if (!MathUtility.EqualFloat(Rotation, 0.0f))
				{
					rotation.RotateZ(Rotation);
					_worldMatrix = Matrix.Multiply(rotation, _worldMatrix);
				}

				if (!(MathUtility.EqualFloat(Scale.X, 1.0f)) || (!MathUtility.EqualFloat(Scale.Y, 1.0f)))
				{
					scale.Scale(Scale.X, Scale.Y, 1.0f);
					_worldMatrix = Matrix.Multiply(scale, _worldMatrix);
				}

				_worldMatrix = Matrix.Multiply(position, _worldMatrix);

				min = new Vector3D(float.MaxValue, float.MaxValue, 0.0f);
				max = new Vector3D(float.MinValue, float.MinValue, 0.0f);

				for (int i = 0; i < _vertices.Count; i++)
				{
					Vector3D transformed = Matrix.Multiply(_worldMatrix, Vector3D.Subtract(_vertices[i].Position, Axis));

					if (transformed.X < min.X)
						min.X = transformed.X;
					if (transformed.Y < min.Y)
						min.Y = transformed.Y;
					if (transformed.X > max.X)
						max.X = transformed.X;
					if (transformed.Y > max.Y)
						max.Y = transformed.Y;
				}

				min = Vector3D.Add(min, Position);
				max = Vector3D.Add(max, Position);
				_aabb = new Drawing.RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);

				_boundCircle.Center = new Vector2D(min.X + (max.X - min.X) / 2.0f, min.Y + (max.Y - min.Y) / 2.0f);
				float xradius = MathUtility.Abs(max.X - min.X) / 2.0f;
				float yradius = MathUtility.Abs(max.Y - min.Y) / 2.0f;
				if (xradius > yradius)
					_boundCircle.Radius = xradius;
				else
					_boundCircle.Radius = yradius;
				
				_needsAABB = false;
			}			
		}

		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected void UpdateDimensions()
		{
			if (NeedsUpdate)
			{
				CalculateWidthHeight();

				// Copy the values into the vertex/index buffers.
				_vertexBuffer.Lock(BufferLockFlags.Discard);
				_vertexBuffer.Write<VertexTypeList.PositionDiffuse2DTexture1>(_vertices.ToArray());
				_vertexBuffer.Unlock();

				_indexBuffer.Lock(BufferLockFlags.Discard);
				if (Gorgon.CurrentDriver.SupportIndex32)
					_indexBuffer.Write<int>(_indices.ToArray());
				else
				{
					ushort[] shortIndices = new ushort[_indices.Count];
					for (int i = 0; i < _indices.Count; i++)
						shortIndices[i] = (ushort)_indices[i];

					_indexBuffer.Write<ushort>(shortIndices);
				}
				_indexBuffer.Unlock();				
			}

			CalculateAABB();

			NeedsUpdate = false;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public void Draw()
		{
			// Update the buffers.
			if ((_indexBuffer == null) || (_vertexBuffer == null) || (_indexBuffer.IndexCount < _indices.Count) || (_vertexBuffer.VertexCount < _vertices.Count))
				RefreshBuffers();

			if ((_indexBuffer == null) || (_vertexBuffer == null))
				return;

			UpdateDimensions();

			// Flush the renderer before drawing.
			Gorgon.Renderer.Render();

			if (Gorgon.GlobalStateSettings.StateChanged(this, Gorgon.Renderer.GetImage(0)))
				Gorgon.GlobalStateSettings.SetStates(this);

			// Reset the world transform.
			Gorgon.Renderer.SetWorldTransform(0, _worldMatrix);

			Gorgon.Renderer.SetImage(0, Image);
			Gorgon.Renderer.Render(Gorgon.Renderer.VertexTypes["PositionDiffuse2DTexture1"], _vertexBuffer, _indexBuffer, PrimitiveStyle, 0, _vertices.Count, 0, _indices.Count);

			// Reset the world transform.
			Gorgon.Renderer.SetWorldTransform(0, Matrix.Identity);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public PolygonSprite Clone()
		{
			PolygonSprite clone = new PolygonSprite(Name + ".Clone", Image);

			clone.PolygonVertices.AddRange(_vertices.ToArray());
			clone.PolygonIndices.AddRange(_indices.ToArray());
			clone.AlphaMaskFunction = AlphaMaskFunction;
			clone.AlphaMaskValue = AlphaMaskValue;
			clone.BlendingMode = BlendingMode;
			clone.BorderColor = BorderColor;
			clone.DepthBufferBias = DepthBufferBias;
			clone.DepthTestFunction = DepthTestFunction;
			clone.DepthWriteEnabled = DepthWriteEnabled;
			clone.DestinationBlend = DestinationBlend;
			clone.PrimitiveStyle = PrimitiveStyle;
			clone.Smoothing = Smoothing;
			clone.SourceBlend = SourceBlend;
			clone.StencilCompare = StencilCompare;
			clone.StencilEnabled = StencilEnabled;
			clone.StencilFailOperation = StencilFailOperation;
			clone.StencilMask = StencilMask;
			clone.StencilPassOperation = StencilPassOperation;
			clone.StencilReference = StencilReference;
			clone.StencilZFailOperation = StencilZFailOperation;
			clone.VerticalWrapMode = VerticalWrapMode;
			clone.HorizontalWrapMode = HorizontalWrapMode;
			clone.InheritAlphaMaskFunction = clone.InheritAlphaMaskFunction;
			clone.InheritAlphaMaskValue = clone.InheritAlphaMaskValue;
			clone.InheritBlending = clone.InheritBlending;
			clone.InheritDepthBias = clone.InheritDepthBias;
			clone.InheritDepthTestFunction = clone.InheritDepthTestFunction;
			clone.InheritDepthWriteEnabled = clone.InheritDepthWriteEnabled;
			clone.InheritHorizontalWrapping = clone.InheritHorizontalWrapping;
			clone.InheritSmoothing = clone.InheritSmoothing;
			clone.InheritStencilCompare = clone.InheritStencilCompare;
			clone.InheritStencilEnabled = clone.InheritStencilEnabled;
			clone.InheritStencilFailOperation = clone.InheritStencilFailOperation;
			clone.InheritStencilMask = clone.InheritStencilMask;
			clone.InheritStencilPassOperation = clone.InheritStencilPassOperation;
			clone.InheritStencilReference = clone.InheritStencilReference;
			clone.InheritStencilZFailOperation = clone.InheritStencilZFailOperation;
			clone.InheritVerticalWrapping = clone.InheritVerticalWrapping;

			return clone;
		}

		/// <summary>
		/// Function to set the position of the polygon sprite.
		/// </summary>
		/// <param name="x">Horizontal position of the sprite.</param>
		/// <param name="y">Vertical position of the sprite.</param>
		/// <param name="depth">Depth of the sprite when using depth buffer.</param>
		public void SetPosition(float x, float y, float depth)
		{
			Position = new Vector3D(x, y, depth);
		}

		/// <summary>
		/// Function to set the scale of the polygon sprite.
		/// </summary>
		/// <param name="x">Horizontal scale.</param>
		/// <param name="y">Vertical scale.</param>
		public void SetScale(float x, float y)
		{
			Scale = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the pivot axis of the polygon sprite.
		/// </summary>
		/// <param name="x">Horizontal position of the pivot axis.</param>
		/// <param name="y">Vertical position of the pivot axis.</param>
		public void SetAxis(float x, float y)
		{
			Axis = new Vector2D(x, y);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PolygonSprite"/> class.
		/// </summary>
		/// <param name="name">The name of the sprite object.</param>
		/// <param name="image">The image to map to the polygon.</param>
		public PolygonSprite(string name, Image image)
			: base(name)
		{
			Image = image;
			PrimitiveStyle = PrimitiveStyle.TriangleStrip;
			_vertices = new PolygonVertexList(this);
			_indices = new IndexList(this);
			DeviceStateList.Add(this);

			UniformScale = 1.0f;
			BorderColor = Drawing.Color.Black;

			// Inherit all global states.
			InheritAlphaMaskFunction = true;
			InheritAlphaMaskValue = true;
			InheritBlending = true;
			InheritDepthBias = true;
			InheritDepthTestFunction = true;
			InheritDepthWriteEnabled = true;
			InheritHorizontalWrapping = true;
			InheritSmoothing = true;
			InheritStencilCompare = true;
			InheritStencilEnabled = true;
			InheritStencilFailOperation = true;
			InheritStencilMask = true;
			InheritStencilPassOperation = true;
			InheritStencilReference = true;
			InheritStencilZFailOperation = true;
			InheritVerticalWrapping = true;
			CullingMode = CullingMode.None;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_vertexBuffer != null)
						_vertexBuffer.Dispose();
					if (_indexBuffer != null)
						_indexBuffer.Dispose();

					_vertexBuffer = null;
					_indexBuffer = null;

					if (DeviceStateList.Contains(this))
						DeviceStateList.Remove(this);
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			if (_vertexBuffer != null)
				_vertexBuffer.Dispose();
			if (_indexBuffer != null)
				_indexBuffer.Dispose();

			_vertexBuffer = null;
			_indexBuffer = null;
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public void DeviceReset()
		{
			NeedsUpdate = true;
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			DeviceLost();
		}
		#endregion

		#region IRenderableStates Members
		/// <summary>
		/// Property to set or return the horizontal wrapping mode to use.
		/// </summary>
		/// <value></value>
		public ImageAddressing HorizontalWrapMode
		{
			get
			{
				if (InheritHorizontalWrapping)
					return Gorgon.GlobalStateSettings.GlobalHorizontalWrapMode;
				return _hwrapMode;
			}
			set
			{
				_hwrapMode = value;
				InheritHorizontalWrapping = false;
			}
		}

		/// <summary>
		/// Property to set or return the vertical wrapping mode to use.
		/// </summary>
		/// <value></value>
		public ImageAddressing VerticalWrapMode
		{
			get
			{
				if (InheritVerticalWrapping)
					return Gorgon.GlobalStateSettings.GlobalVerticalWrapMode;
				return _vwrapMode;
			}
			set
			{
				_vwrapMode = value;
				InheritVerticalWrapping = false;
			}
		}

		/// <summary>
		/// Property to set or return the color of the border when the wrapping mode is set to Border.
		/// </summary>
		/// <value></value>
		public System.Drawing.Color BorderColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of smoothing for the sprites.
		/// </summary>
		/// <value></value>
		public Smoothing Smoothing
		{
			get
			{
				if (InheritSmoothing)
					return Gorgon.GlobalStateSettings.GlobalSmoothing;

				return _smoothing;
			}
			set
			{
				_smoothing = value;
				InheritSmoothing = false;
			}
		}

		/// <summary>
		/// Property to set or return the function used for alpha masking.
		/// </summary>
		/// <value></value>
		public CompareFunctions AlphaMaskFunction
		{
			get
			{
				if (InheritAlphaMaskFunction)
					return Gorgon.GlobalStateSettings.GlobalAlphaMaskFunction;

				return _alphaCompareFunction;
			}
			set
			{
				_alphaCompareFunction = value;
				InheritAlphaMaskFunction = false;
			}
		}

		/// <summary>
		/// Property to set or return the alpha value used for alpha masking.
		/// </summary>
		/// <value></value>
		public int AlphaMaskValue
		{
			get
			{
				if (InheritAlphaMaskValue)
					return Gorgon.GlobalStateSettings.GlobalAlphaMaskValue;

				return _alphaTestCompareValue;
			}
			set
			{
				_alphaTestCompareValue = value;
				InheritAlphaMaskValue = false;
			}
		}

		/// <summary>
		/// Property to set or return the blending mode.
		/// </summary>
		/// <value></value>
		public BlendingModes BlendingMode
		{
			get
			{
				if (InheritBlending)
					return Gorgon.GlobalStateSettings.GlobalBlending;

				return _blendMode;
			}
			set
			{
				_blendMode = value;
				InheritBlending = false;
			}
		}

		/// <summary>
		/// Property to set or return the source blending operation.
		/// </summary>
		/// <value></value>
		public AlphaBlendOperation SourceBlend
		{
			get
			{
				if (InheritBlending)
					return Gorgon.GlobalStateSettings.GlobalSourceBlend;

				return _sourceBlendOp;
			}
			set
			{
				_sourceBlendOp = value;
				InheritBlending = false;
			}
		}

		/// <summary>
		/// Property to set or return the destination blending operation.
		/// </summary>
		/// <value></value>
		public AlphaBlendOperation DestinationBlend
		{
			get
			{
				if (InheritBlending)
					return Gorgon.GlobalStateSettings.GlobalDestinationBlend;

				return _destBlendOp;
			}
			set
			{
				_destBlendOp = value;
				InheritBlending = false;
			}
		}

		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		public bool StencilEnabled
		{
			get
			{
				if (InheritStencilEnabled)
					return Gorgon.GlobalStateSettings.GlobalStencilEnabled;
				return _useStencil;
			}
			set
			{
				_useStencil = value;
				InheritStencilEnabled = false;
			}
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		public int StencilReference
		{
			get
			{
				if (InheritStencilReference)
					return Gorgon.GlobalStateSettings.GlobalStencilReference;
				return _stencilReference;
			}
			set
			{
				_stencilReference = value;
				InheritStencilReference = false;
			}
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		public int StencilMask
		{
			get
			{
				if (InheritStencilMask)
					return Gorgon.GlobalStateSettings.GlobalStencilMask;

				return _stencilMask;
			}
			set
			{
				_stencilMask = value;
				InheritStencilMask = false;
			}
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		public StencilOperations StencilPassOperation
		{
			get
			{
				if (InheritStencilPassOperation)
					return Gorgon.GlobalStateSettings.GlobalStencilPassOperation;

				return _stencilPassOperation;
			}
			set
			{
				_stencilPassOperation = value;
				InheritStencilPassOperation = false;
			}
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		public StencilOperations StencilFailOperation
		{
			get
			{
				if (InheritStencilFailOperation)
					return Gorgon.GlobalStateSettings.GlobalStencilFailOperation;

				return _stencilFailOperation;
			}
			set
			{
				_stencilFailOperation = value;
				InheritStencilFailOperation = false;
			}
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		public StencilOperations StencilZFailOperation
		{
			get
			{
				if (InheritStencilZFailOperation)
					return Gorgon.GlobalStateSettings.GlobalStencilZFailOperation;

				return _stencilZFailOperation;
			}
			set
			{
				_stencilZFailOperation = value;
				InheritStencilZFailOperation = false;
			}
		}

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		public CompareFunctions StencilCompare
		{
			get
			{
				if (InheritStencilCompare)
					return Gorgon.GlobalStateSettings.GlobalStencilCompare;

				return _stencilCompare;
			}
			set
			{
				_stencilCompare = value;
				InheritStencilCompare = false;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		/// <value></value>
		bool IRenderableStates.UseIndices
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		/// <value></value>
		PrimitiveStyle IRenderableStates.PrimitiveStyle
		{
			get
			{
				return PrimitiveStyle.TriangleList;
			}
		}

		/// <summary>
		/// Property to set or return the wrapping mode to use.
		/// </summary>
		/// <value></value>
		public ImageAddressing WrapMode
		{
			get
			{
				if (InheritHorizontalWrapping)
					return Gorgon.GlobalStateSettings.GlobalHorizontalWrapMode;
				else
					return _hwrapMode;
			}
			set
			{
				HorizontalWrapMode = VerticalWrapMode = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to enable the depth buffer (if applicable) writing or not.
		/// </summary>
		public virtual bool DepthWriteEnabled
		{
			get
			{
				if (InheritDepthWriteEnabled)
					return Gorgon.GlobalStateSettings.GlobalDepthWriteEnabled;
				return _depthWriteEnabled;
			}
			set
			{
				_depthWriteEnabled = value;
				InheritDepthWriteEnabled = false;
			}
		}

		/// <summary>
		/// Property to set or return (if applicable) the depth buffer bias.
		/// </summary>
		public virtual float DepthBufferBias
		{
			get
			{
				if (InheritDepthBias)
					return Gorgon.GlobalStateSettings.GlobalDepthBufferBias;
				return _depthBias;
			}
			set
			{
				_depthBias = value;
				InheritDepthBias = false;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer (if applicable) testing comparison function.
		/// </summary>
		public virtual CompareFunctions DepthTestFunction
		{
			get
			{
				if (InheritDepthTestFunction)
					return Gorgon.GlobalStateSettings.GlobalDepthBufferTestFunction;
				return _depthCompare;
			}
			set
			{
				_depthCompare = value;
				InheritDepthTestFunction = false;
			}
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		/// <remarks>The default value is <see cref="GorgonLibrary.Graphics.CullingMode">CullingMode.None</see>.</remarks>
		public CullingMode CullingMode
		{
			get;
			set;
		}
		#endregion
	}
}
