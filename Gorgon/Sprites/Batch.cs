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
// Created: Tuesday, April 28, 2009 11:18:50 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using System.Runtime.InteropServices;
using GorgonLibrary;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A vertex used by the batch.
	/// </summary>
	public struct BatchVertex
	{
		/// <summary>
		/// A processed vertex.
		/// </summary>
		public VertexTypeList.PositionDiffuse2DTexture1 Vertex
		{
			get;
			set;
		}

		/// <summary>
		/// Image associated with the vertex.
		/// </summary>
		public Image Image
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Fence to group a series of vertices by state change.
	/// </summary>
	public struct Fence
	{
		/// <summary>
		/// Image used for the fence.
		/// </summary>
		public Image Image;
		
		/// <summary>
		/// The start of the vertex fence.
		/// </summary>
		public int VertexStart;

		/// <summary>
		/// The start of the index fence.
		/// </summary>
		public int IndexStart;

		/// <summary>
		/// Number of vertices encompassed by the fence.
		/// </summary>
		public int VertexCount;

		/// <summary>
		/// Number of indices encompassed by the fence.
		/// </summary>
		public int IndexCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="Fence"/> struct.
		/// </summary>
		/// <param name="image">The image used by the fence.</param>
		/// <param name="vertexStart">The starting vertex for the fence.</param>
		/// <param name="vertexCount">The number of vertices encompassed by the fence.</param>
		/// <param name="indexStart">The starting index for the fence.</param>
		/// <param name="indexCount">The number of indices encompassed by the fence.</param>
		public Fence(Image image, int vertexStart, int vertexCount, int indexStart, int indexCount)
		{
			Image = image;
			VertexStart = vertexStart;
			VertexCount = vertexCount;
			IndexStart = indexStart;
			IndexCount = indexCount;
		}
	}

	/// <summary>
	/// A batch of sprites to be statically renderered.
	/// </summary>
	/// <remarks>The purpose of this object is to quickly render multiple sprites in one pass.  In the immediate type model used by the <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> object 
	/// the buffer where the sprites are stored is flushed when a change (image change, state change, etc...) occurs.  For example, if you draw multiple sprites and one of those sprites has an image that is different
	/// from the previous sprites, the buffer will flush all the previous sprites to the render target.  Then the process starts over with an empty buffer.  This has advantages, but 
	/// sometimes this will slow down the rendering process.<para>Batched sprites however will take multiple sprite objects and store them into a buffer and will flush that buffer
	/// only when the render target is updated.  This has the advantage of being able to draw all the sprites in one shot and those sprites will be "cached" each time the batch
	/// is drawn so that the buffers that store the sprites never need to be updated (unless a change is made).</para>
	/// <para>There is a caveat with this approach:  The sprites cannot move or be animated in any way.  This means that the sprites stored are "snapshots" of the sprite at a given 
	/// time and will not be affected by any transformations made after the fact.  The sprites will update when the batch is invalidated and rebuilt however, but this process is 
	/// slow and is not recommended for real time use.  The sprites in the batch will be affected by the batch transforms however, but all the sprites will be effected as a single 
	/// entity and not individually.</para>
	/// <para>The best usage of this object is in a "map" scenario, where the map is a tile map and the sprites that compose the map as tiles are blitted in one shot.</para>
	/// </remarks>
	public class Batch
		: NamedObject, IRenderableStates, IDisposable, IList<Renderable>
	{
		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the object was disposed.
		private List<Renderable> _sprites = null;					// List of sprites.
		private VertexBuffer _vertices = null;						// List of vertices.
		private IndexBuffer _indices = null;						// List of indices.
		private int _renderableCount = 0;							// Number of renderables to allow.
		private bool _needsRefresh = false;							// Flag to indicate that we need to refresh the batch.
		private List<Fence> _fences = null;							// List of vertex fences.
		private Matrix _worldTransform = Matrix.Identity;			// World transform for sprite data.
		private Smoothing _smoothing;								// Smoothing mode.
		private AlphaBlendOperation _sourceBlendOp;					// Alphablend source operation.
		private AlphaBlendOperation _destBlendOp;					// Alphablend destination operation.
        private AlphaBlendOperation _sourceBlendAlphaOp;					// Alphablend source operation.
        private AlphaBlendOperation _destBlendAlphaOp;					// Alphablend destination operation.
		private BlendingModes _blendMode;							// Alpha blending preset mode.
		private ImageAddressing _hwrapMode;							// Horizontal wrapping mode.
		private ImageAddressing _vwrapMode;							// Vertical wrapping mode.
		private CompareFunctions _alphaCompareFunction;				// Alpha test compare function.
		private int _alphaTestCompareValue;							// Alpha test compare value.
		private StencilOperations _stencilPassOperation;			// Stencil pass operation.
		private StencilOperations _stencilFailOperation;			// Stencil fail operation.
		private StencilOperations _stencilZFailOperation;			// Stencil Z fail operation.
		private CompareFunctions _stencilCompare;					// Stencil compare operation.
		private int _stencilReference;								// Stencil reference value.
		private int _stencilMask;									// Stencil mask value.
		private bool _useStencil;									// Flag to indicate whether to use the stencil or not.
		private float _depthBias;									// Depth bias.
		private bool _depthWriteEnabled;							// Depth writing enabled flag.
		private CompareFunctions _depthCompare;						// Depth test comparison function.
		private float _angle;										// Angle of rotation in degrees.
		private Vector2D _position;									// Position
		private Vector2D _axis;										// Axis for rotation.
		private Vector2D _scale;									// Scaling values for the batch.
		private bool _needsTransform = false;						// Flag to indicate that we need to build a transform.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the angle of rotation for the batch in degrees.
		/// </summary>
		public float Rotation
		{
			get
			{
				return _angle;
			}
			set
			{
				_angle = value;
				_needsTransform = true;
			}
		}

		/// <summary>
		/// Property to set or return the position of the batch.
		/// </summary>
		public Vector2D Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				_needsTransform = true;
			}
		}

		/// <summary>
		/// Property to set or return the axis of the batch.
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
				_needsTransform = true;
			}
		}

		/// <summary>
		/// Property to set or return the scaling values of the batch.
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
				_needsTransform = true;
			}
		}

		/// <summary>
		/// Property to set or return the uniform scaling value for the batch.
		/// </summary>
		public float UniformScale
		{
			get
			{
				return _scale.X;
			}
			set
			{
				Scale = new Vector2D(value, value);
			}
		}

		/// <summary>
		/// Property to set or return the "depth" value for this batch.
		/// </summary>
		public float Depth
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the buffers to use for the batch.
		/// </summary>
		private void BuildBuffers(int vertexCount)
		{
			int indexCount = 0;
			IndexBufferType indexType = IndexBufferType.Index16;

			if (_vertices != null)
			{
				_vertices.Dispose();
				_vertices = null;
			}
			if (_indices != null)
			{
				_indices.Dispose();
				_indices = null;
			}

			if ((_sprites.Count < 1) || (vertexCount < 1))
				return;

			if (vertexCount > 65535)
			{
				if (!Gorgon.CurrentDriver.SupportIndex32)
					vertexCount = 65535;
				else
					indexType = IndexBufferType.Index32;
			}

			indexCount = (vertexCount * 6) / 4;

			_vertices = new VertexBuffer(Gorgon.Renderer.VertexTypes["PositionDiffuse2DTexture1"].VertexSize(0), vertexCount, BufferUsages.Static | BufferUsages.WriteOnly);
			_indices = new IndexBuffer(indexType, indexCount, BufferUsages.WriteOnly | BufferUsages.Static);

			// Fill in the index buffer.
			uint index = 0;
			try
			{
				_indices.Lock(BufferLockFlags.Discard);
				for (uint i = 0; i < (vertexCount / 4); i++)
				{
					if (indexType == IndexBufferType.Index32)
					{
						_indices.Write<uint>(index + 2);
						_indices.Write<uint>(index + 1);
						_indices.Write<uint>(index);
						_indices.Write<uint>(index);
						_indices.Write<uint>(index + 3);
						_indices.Write<uint>(index + 2);
					}
					else
					{
						_indices.Write<ushort>((ushort)(index + 2));
						_indices.Write<ushort>((ushort)(index + 1));
						_indices.Write<ushort>((ushort)(index));
						_indices.Write<ushort>((ushort)(index));
						_indices.Write<ushort>((ushort)(index + 3));
						_indices.Write<ushort>((ushort)(index + 2));
					}
					index += 4;
				}
			}
			finally
			{
				if (_indices.IsLocked)
					_indices.Unlock();
			}
		}

		/// <summary>
		/// Function to re-fill the buffers.
		/// </summary>
		private void RefreshBuffers()
		{
			List<BatchVertex> vertices = null;			// List of vertices.

			if ((!_needsRefresh) || (_sprites.Count < 1))
				return;
			
			// Build the list of vertices to add.
			vertices = new List<BatchVertex>();
			_fences = new List<Fence>();

			foreach (Renderable renderable in _sprites)
			{
				BatchVertex[] renderableVertices = renderable.GetVertices();

				if (renderableVertices != null)
					vertices.AddRange(renderableVertices);
				else
					throw new GorgonException(GorgonErrors.CannotWriteData, "Could not fill the vertex buffer.  The renderable '" + renderable.Name + "' does not support batching.");
			}

			if ((_vertices == null) || (_indices == null) || (vertices.Count > _vertices.VertexCount))
				BuildBuffers(vertices.Count);

			try
			{
				int vertexStart = 0;				// Starting vertex.
				int vertexCount = 0;				// Number of vertices.
				Image lastImage = null;				// Last image.

				// Sort by texture.
				var sortedVertices = from vertex in vertices
									 group vertex by vertex.Image;

				_vertices.Lock(BufferLockFlags.Discard);
				// Group into fences.
				foreach (var vertex in sortedVertices)
				{					
					for (int i = 0; i < vertex.Count(); i++)
					{
						lastImage = vertex.ElementAt(i).Image;
						_vertices.Write<VertexTypeList.PositionDiffuse2DTexture1>(vertex.ElementAt(i).Vertex);
						vertexCount++;
					}

					_fences.Add(new Fence(lastImage, vertexStart, vertexCount, 0, _indices.IndexCount));
					vertexStart += vertexCount;
					vertexCount = 0;
				}
			}
			finally
			{
				if (_vertices.IsLocked)
					_vertices.Unlock();
			}

			_needsRefresh = false;
		}

		/// <summary>
		/// Function to update the transformation.
		/// </summary>
		private void UpdateTransform()
		{
			float cos = MathUtility.Cos(MathUtility.Radians(_angle));
			float sin = MathUtility.Sin(MathUtility.Radians(_angle));

			Matrix axisOffset = Matrix.Identity;
			Matrix scale = Matrix.Identity;
			Matrix translate = Matrix.Identity;
			Matrix rotate = Matrix.Identity;

			_worldTransform = Matrix.Identity;

			axisOffset.m14 = -_axis.X;
			axisOffset.m24 = -_axis.Y;

			rotate.m11 = cos;
			rotate.m12 = -sin;
			rotate.m21 = sin;
			rotate.m22 = cos;

			scale.m11 = _scale.X;
			scale.m22 = _scale.Y;

			translate.m14 = _position.X;
			translate.m24 = _position.Y;
			translate.m34 = Depth;

			_worldTransform = Matrix.Multiply(rotate, axisOffset);
			_worldTransform = Matrix.Multiply(scale, _worldTransform);
			_worldTransform = Matrix.Multiply(translate, _worldTransform);

			_needsTransform = false;
		}

		/// <summary>
		/// Function to set the position of the batch.
		/// </summary>
		/// <param name="x">Horizontal position of the batch.</param>
		/// <param name="y">Vertical position of the batch.</param>
		public void SetPosition(float x, float y)
		{
			Position = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the axis for the batch.
		/// </summary>
		/// <param name="x">Horizontal position of the axis.</param>
		/// <param name="y">Vertical position of the axis.</param>
		public void SetAxis(float x, float y)
		{
			Axis = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the scaling values for the batch.
		/// </summary>
		/// <param name="x">Horizontal scale of the batch.</param>
		/// <param name="y">Vertical scale of the batch.</param>
		public void SetScale(float x, float y)
		{
			Scale = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to perform a re-build of the buffers used for the batch.
		/// </summary>
		public void Refresh()
		{
			_needsRefresh = true;
		}

		/// <summary>
		/// Function to add and clone a <see cref="GorgonLibrary.Graphics.Renderable">Renderable</see> item to the list.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="GorgonLibrary.Graphics.Renderable">Renderable</see> to add to the batch.</typeparam>
		/// <param name="item">Renderable item to add to the list.</param>
		/// <returns>The clone of that renderable item.</returns>
		public T AddClone<T>(T item)
			where T : Renderable
		{
			if (item == null)
				throw new ArgumentNullException("item");

			T clone = item.Clone() as T;

			if (clone != null)
				Add(clone);
			else
				throw new ArgumentException("The object is not descended from Renderable.", "item");

			clone.Name = item.Name + ".Clone";

			return clone;
		}

		/// <summary>
		/// Function to remove a renderable from the list by its index.
		/// </summary>
		/// <param name="index">Index of the renderable.</param>
		public void Remove(int index)
		{
			_sprites.RemoveAt(index);
			_needsRefresh = true;
		}

		/// <summary>
		/// Function to draw the contents of the static buffers.
		/// </summary>
		public void Draw()
		{
			if (_needsRefresh)
				RefreshBuffers();

			// Flush the renderer before drawing.
			Gorgon.Renderer.Render();

			if (Gorgon.GlobalStateSettings.StateChanged(this, Gorgon.Renderer.GetImage(0)))
				Gorgon.GlobalStateSettings.SetStates(this);

			if (_needsTransform)
				UpdateTransform();

			Gorgon.Renderer.SetWorldTransform(0, _worldTransform);

			foreach(Fence fence in _fences)
			{
				Gorgon.Renderer.SetImage(0, fence.Image);
				Gorgon.Renderer.Render(Gorgon.Renderer.VertexTypes["PositionDiffuse2DTexture1"], _vertices, _indices, PrimitiveStyle.TriangleList, fence.VertexStart, fence.VertexCount, fence.IndexStart, fence.IndexCount);
			}

			// Reset the world transform.
			Gorgon.Renderer.SetWorldTransform(0, Matrix.Identity);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Batch"/> class.
		/// </summary>
		/// <param name="batchName">Name of the batch.</param>
		/// <param name="renderableCount">Initial number of renderables to store in the batch.</param>
		public Batch(string batchName, int renderableCount)
			: base(batchName)
		{
			if (renderableCount < 1)
				throw new ArgumentException("There must be at least 1 renderable.", "renderableCount");

			_sprites = new List<Renderable>();
			_renderableCount = renderableCount;
			BorderColor = Drawing.Color.Black;
			UniformScale = 1.0f;

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
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_sprites != null)
						_sprites.Clear();
					if (_indices != null)
						_indices.Dispose();
					if (_vertices != null)
						_vertices.Dispose();
				}

				_vertices = null;
				_indices = null;
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

		#region IList<Renderable> Members
		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(Renderable item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return _sprites.IndexOf(item);
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
		public void Insert(int index, Renderable item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			_sprites.Insert(index, item);
			_needsRefresh = true;
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
		void IList<Renderable>.RemoveAt(int index)
		{
			Remove(index);
		}

		/// <summary>
		/// Gets or sets the <see cref="GorgonLibrary.Graphics.Renderable"/> at the specified index.
		/// </summary>
		/// <value></value>
		public Renderable this[int index]
		{
			get
			{
				return _sprites[index];
			}
			set
			{
				if (value == null)
				{
					Remove(index);
					return;
				}

				_sprites[index] = value;
				_needsRefresh = true;
			}
		}
		#endregion

		#region ICollection<Renderable> Members
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		public void Add(Renderable item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			_sprites.Add(item);
			_needsRefresh = true;
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		public void Clear()
		{
			_sprites.Clear();
			BuildBuffers(0);
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(Renderable item)
		{
			return _sprites.Contains(item);
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
		/// The type cannot be cast automatically to the type of the destination <paramref name="array"/>.
		/// </exception>
		public void CopyTo(Renderable[] array, int arrayIndex)
		{
			_sprites.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		public int Count
		{
			get 
			{
				return _sprites.Count;
			}
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
		public bool Remove(Renderable item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			_sprites.Remove(item);
			_needsRefresh = true;
			return true;
		}
		#endregion

		#region IEnumerable<Renderable> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<Renderable> GetEnumerator()
		{
			foreach (Renderable renderable in _sprites)
				yield return renderable;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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
        /// Property to set or return the source blending operation.
        /// </summary>
        /// <value></value>
        public AlphaBlendOperation SourceBlendAlpha
        {
            get
            {
                return _sourceBlendAlphaOp;
            }
            set
            {
                _sourceBlendAlphaOp = value;
            }
        }

        /// <summary>
        /// Property to set or return the destination blending operation.
        /// </summary>
        /// <value></value>
        public AlphaBlendOperation DestinationBlendAlpha
        {
            get
            {

                return _destBlendAlphaOp;
            }
            set
            {
                _destBlendAlphaOp = value;
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
		/// Property to set or return the culling mode.
		/// </summary>
		CullingMode IRenderableStates.CullingMode
		{
			get
			{
				return CullingMode.Clockwise;
			}
			set
			{
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
		#endregion
	}
}
