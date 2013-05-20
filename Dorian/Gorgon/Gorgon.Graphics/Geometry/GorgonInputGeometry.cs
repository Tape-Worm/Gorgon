#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, December 15, 2011 10:43:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Native;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
    #region Enums
    /// <summary>
	/// Primitive type.
	/// </summary>
	public enum PrimitiveType
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// A list of points.
		/// </summary>
		PointList = 1,
		/// <summary>
		/// A list of lines.
		/// </summary>
		LineList = 2,
		/// <summary>
		/// A strip of lines.
		/// </summary>
		LineStrip = 3,
		/// <summary>
		/// A list of triangles.
		/// </summary>
		TriangleList = 4,
		/// <summary>
		/// A strip of triangles.
		/// </summary>
		TriangleStrip = 5,
		/// <summary>
		/// A list of lines including adjacency information.
		/// </summary>
		LineListWithAdjacency = 10,
		/// <summary>
		/// A strip of lines including adjacency information.
		/// </summary>
		LineStripWithAdjacency = 11,
		/// <summary>
		/// A list of triangles including adjacency information.
		/// </summary>
		TriangleListWithAdjacency = 12,
		/// <summary>
		/// A strip of triangles including adjacency information.
		/// </summary>
		TriangleStripWithAdjacency = 13,
		/// <summary>
		/// A patch list with 1 control point.
		/// </summary>
		PatchListWith1ControlPoints = 33,
		/// <summary>
		/// A patch list with 2 control points.
		/// </summary>
		PatchListWith2ControlPoints = 34,
		/// <summary>
		/// A patch list with 3 control points.
		/// </summary>
		PatchListWith3ControlPoints = 35,
		/// <summary>
		/// A patch list with 4 control points.
		/// </summary>
		PatchListWith4ControlPoints = 36,
		/// <summary>
		/// A patch list with 5 control points.
		/// </summary>
		PatchListWith5ControlPoints = 37,
		/// <summary>
		/// A patch list with 6 control points.
		/// </summary>
		PatchListWith6ControlPoints = 38,
		/// <summary>
		/// A patch list with 7 control points.
		/// </summary>
		PatchListWith7ControlPoints = 39,
		/// <summary>
		/// A patch list with 8 control points.
		/// </summary>
		PatchListWith8ControlPoints = 40,
		/// <summary>
		/// A patch list with 9 control points.
		/// </summary>
		PatchListWith9ControlPoints = 41,
		/// <summary>
		/// A patch list with 10 control points.
		/// </summary>
		PatchListWith10ControlPoints = 42,
		/// <summary>
		/// A patch list with 11 control points.
		/// </summary>
		PatchListWith11ControlPoints = 43,
		/// <summary>
		/// A patch list with 12 control points.
		/// </summary>
		PatchListWith12ControlPoints = 44,
		/// <summary>
		/// A patch list with 13 control points.
		/// </summary>
		PatchListWith13ControlPoints = 45,
		/// <summary>
		/// A patch list with 14 control points.
		/// </summary>
		PatchListWith14ControlPoints = 46,
		/// <summary>
		/// A patch list with 15 control points.
		/// </summary>
		PatchListWith15ControlPoints = 47,
		/// <summary>
		/// A patch list with 16 control points.
		/// </summary>
		PatchListWith16ControlPoints = 48,
		/// <summary>
		/// A patch list with 17 control points.
		/// </summary>
		PatchListWith17ControlPoints = 49,
		/// <summary>
		/// A patch list with 18 control points.
		/// </summary>
		PatchListWith18ControlPoints = 50,
		/// <summary>
		/// A patch list with 19 control points.
		/// </summary>
		PatchListWith19ControlPoints = 51,
		/// <summary>
		/// A patch list with 20 control points.
		/// </summary>
		PatchListWith20ControlPoints = 52,
		/// <summary>
		/// A patch list with 21 control points.
		/// </summary>
		PatchListWith21ControlPoints = 53,
		/// <summary>
		/// A patch list with 22 control points.
		/// </summary>
		PatchListWith22ControlPoints = 54,
		/// <summary>
		/// A patch list with 23 control points.
		/// </summary>
		PatchListWith23ControlPoints = 55,
		/// <summary>
		/// A patch list with 24 control points.
		/// </summary>
		PatchListWith24ControlPoints = 56,
		/// <summary>
		/// A patch list with 25 control points.
		/// </summary>
		PatchListWith25ControlPoints = 57,
		/// <summary>
		/// A patch list with 26 control points.
		/// </summary>
		PatchListWith26ControlPoints = 58,
		/// <summary>
		/// A patch list with 27 control points.
		/// </summary>
		PatchListWith27ControlPoints = 59,
		/// <summary>
		/// A patch list with 28 control points.
		/// </summary>
		PatchListWith28ControlPoints = 60,
		/// <summary>
		/// A patch list with 29 control points.
		/// </summary>
		PatchListWith29ControlPoints = 61,
		/// <summary>
		/// A patch list with 30 control points.
		/// </summary>
		PatchListWith30ControlPoints = 62,
		/// <summary>
		/// A patch list with 31 control points.
		/// </summary>
		PatchListWith31ControlPoints = 63,
		/// <summary>
		/// A patch list with 32 control points.
		/// </summary>
		PatchListWith32ControlPoints = 64,
	}
    #endregion

    /// <summary>
	/// Manages the input bindings such as the vertex/index buffer, input layout and primitive types.
	/// </summary>
	public sealed class GorgonInputGeometry
	{
		#region Classes.
		/// <summary>
		/// A list of vertex buffer bindings.
		/// </summary>
		public sealed class VertexBufferBindingList
			: IList<GorgonVertexBufferBinding>
		{
			#region Variables.
			private readonly IList<GorgonVertexBufferBinding> _bindings;		// List of bindings.
			private readonly D3D.VertexBufferBinding[] _D3DBindings;			// List of D3D bindings.
			private readonly GorgonGraphics _graphics;							// Graphics interface.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the number of available bindings.
			/// </summary>
			/// <remarks>On Shader Model 4 with a 4.1 profile and better, the number of bindings available is 32, otherwise it is 16.</remarks>
			public int Count
			{
				get
				{
					return _bindings.Count;
				}
			}

			/// <summary>
			/// Property to set or return the vertex buffer binding for a given slot.
			/// </summary>
			public GorgonVertexBufferBinding this[int index]
			{
				get
				{
					return _bindings[index];
				}
				set
				{
					if (value.Equals(_bindings[index]))
					{
						return;
					}

					_bindings[index] = value;
					_D3DBindings[index] = value.Convert();
					_graphics.Context.InputAssembler.SetVertexBuffers(index, _D3DBindings[index]);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to find the index of a vertex buffer binding that contains the specified vertex buffer.
			/// </summary>
			/// <param name="buffer">Vertex buffer to find.</param>
			/// <returns>The index of the buffer binding in the list, -1 if not found.</returns>
			public int IndexOf(GorgonVertexBuffer buffer)
			{
				GorgonDebug.AssertNull(buffer, "buffer");

				for (int i = 0; i < _bindings.Count; i++)
				{
					if (_bindings[i].VertexBuffer == buffer)
					{
						return i;
					}
				}

				return -1;
			}

			/// <summary>
			/// Function to return whether the vertex buffer specified has a binding in the list.
			/// </summary>
			/// <param name="buffer">The buffer to find.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonVertexBuffer buffer)
			{
				GorgonDebug.AssertNull(buffer, "buffer");

				return IndexOf(buffer) != -1;
			}

			/// <summary>
			/// Function to set a series of bindings at once.
			/// </summary>
			/// <param name="binding">Bindings to set.</param>
			/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="binding"/> parameter will set the bindings to empty.</remarks>
			public void SetVertexBindingRange(GorgonVertexBufferBinding[] binding)
			{
				SetVertexBindingRange(binding, 0);
			}

			/// <summary>
			/// Function to set a series of bindings at once.
			/// </summary>
			/// <param name="binding">Bindings to set.</param>
			/// <param name="startIndex">Index to start writing at.</param>
			/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="binding"/> parameter will set the bindings to empty (starting at <paramref name="startIndex"/>).</remarks>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the startIndex parameter is less than 0 or greater than the number of available bindings - 1.</exception>
			public void SetVertexBindingRange(GorgonVertexBufferBinding[] binding, int startIndex)
			{
				int count = _bindings.Count - startIndex;

				GorgonDebug.AssertParamRange(startIndex, 0, _bindings.Count, true, false,"startIndex");

				if (binding != null)
				{
					count = binding.Length.Min(_bindings.Count);
				}

				for (int i = 0; i < count; i++)
				{
					GorgonVertexBufferBinding currentBinding = GorgonVertexBufferBinding.Empty;

					if (binding != null)
					{
						currentBinding = binding[i];
					}

					// If we've already set the binding, then don't bother.
					if (_bindings[startIndex + i].Equals(currentBinding))
					{
						continue;
					}

					_bindings[startIndex + i] = currentBinding;
					_D3DBindings[i] = currentBinding.Convert();
				}

				_graphics.Context.InputAssembler.SetVertexBuffers(startIndex, _D3DBindings);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="VertexBufferBindingList"/> class.
			/// </summary>
			/// <param name="graphics">The graphics.</param>
			internal VertexBufferBindingList(GorgonGraphics graphics)
			{
				_graphics = graphics;
				_bindings = new GorgonVertexBufferBinding[_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4_1 ? 16 : 32];
				_D3DBindings = new D3D.VertexBufferBinding[_bindings.Count];
				for (int i = 0; i < _bindings.Count; i++)
				{
					_bindings[i] = GorgonVertexBufferBinding.Empty;
					_D3DBindings[i] = new D3D.VertexBufferBinding();
				}				
			}
			#endregion

			#region IEnumerable<GorgonVertexBufferBinding> Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="System.Collections.Generic.IEnumerator{GorgonVertexBufferBinding}"/> object that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<GorgonVertexBufferBinding> GetEnumerator()
			{
				return _bindings.GetEnumerator();
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
				return ((System.Collections.IEnumerable)_bindings).GetEnumerator();
			}
			#endregion

			#region IList<GorgonVertexBufferBinding> Members
			/// <summary>
			/// Function to return the index of a binding.
			/// </summary>
			/// <param name="item">The binding to find.</param>
			/// <returns>The index of the binding, if found.  -1 if the binding was not found.</returns>
			public int IndexOf(GorgonVertexBufferBinding item)
			{
				return _bindings.IndexOf(item);
			}

			/// <summary>
			/// Inserts the specified index.
			/// </summary>
			/// <param name="index">The index.</param>
			/// <param name="item">The item.</param>
			void IList<GorgonVertexBufferBinding>.Insert(int index, GorgonVertexBufferBinding item)
			{
			}

			/// <summary>
			/// Removes at.
			/// </summary>
			/// <param name="index">The index.</param>
			void IList<GorgonVertexBufferBinding>.RemoveAt(int index)
			{
			}
			#endregion

			#region ICollection<GorgonVertexBufferBinding> Members
			/// <summary>
			/// Adds the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			void ICollection<GorgonVertexBufferBinding>.Add(GorgonVertexBufferBinding item)
			{
			}

			/// <summary>
			/// Clears this instance.
			/// </summary>
			void ICollection<GorgonVertexBufferBinding>.Clear()
			{
			}

			/// <summary>
			/// Function to return whether an item exists within this collection.
			/// </summary>
			/// <param name="item">Item to scan for.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonVertexBufferBinding item)
			{
				return _bindings.Contains(item);
			}

			/// <summary>
			/// Function to copy the bindings to an external array.
			/// </summary>
			/// <param name="array">The array to copy into.</param>
			/// <param name="arrayIndex">Index of the array to start writing at.</param>
			public void CopyTo(GorgonVertexBufferBinding[] array, int arrayIndex)
			{
				_bindings.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Gets a value indicating whether this instance is read only.
			/// </summary>
			/// <value>
			/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
			/// </value>
			public bool IsReadOnly
			{
				get 
				{
					return false;
				}
			}

			/// <summary>
			/// Removes the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			bool ICollection<GorgonVertexBufferBinding>.Remove(GorgonVertexBufferBinding item)
			{
				return false;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private PrimitiveType _primitiveType = PrimitiveType.Unknown;		// Primitive type to use.
		private GorgonInputLayout _inputLayout;								// The current input layout.
		private readonly GorgonGraphics _graphics;							// Current graphics interface.
		private GorgonIndexBuffer _indexBuffer;								// The current index buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the index buffer.
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get
			{
				return _indexBuffer;
			}
			set
			{
				SetIndexBuffer(value, 0);
			}
		}

		/// <summary>
		/// Property to return the vertex buffer binding interface.
		/// </summary>
		public VertexBufferBindingList VertexBuffers
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the primtive type.
		/// </summary>
		public PrimitiveType PrimitiveType
		{
			get
			{
				return _primitiveType;
			}
			set
			{
				if (_primitiveType == value)
				{
					return;
				}

				_primitiveType = value;
				_graphics.Context.InputAssembler.PrimitiveTopology = (SharpDX.Direct3D.PrimitiveTopology)value;
			}
		}

		/// <summary>
		/// Property to set or return the input layout.
		/// </summary>
		public GorgonInputLayout Layout
		{
			get
			{
				return _inputLayout;
			}
			set
			{
				if (_inputLayout == value)
				{
					return;
				}

				_inputLayout = value;

				_graphics.Context.InputAssembler.InputLayout = _inputLayout != null
					                                               ? _inputLayout.Convert(_graphics.D3DDevice)
					                                               : null;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the current index buffer, with an offset inside the buffer.
		/// </summary>
		/// <param name="buffer">Buffer to set.</param>
		/// <param name="offset">Offset into the buffer to use, in bytes.</param>
		public void SetIndexBuffer(GorgonIndexBuffer buffer, int offset)
		{
			if (_indexBuffer == buffer)
			{
				return;
			}
			
#if DEBUG
			if ((buffer != null) && ((offset >= buffer.SizeInBytes) || (offset < 0)))
			{
				throw new ArgumentOutOfRangeException("offset", string.Format(Resources.GORGFX_VALUE_OUT_OF_RANGE, offset, buffer.SizeInBytes));
			}
#endif

			if (buffer != null)
			{
				_graphics.Context.InputAssembler.SetIndexBuffer((D3D.Buffer)buffer.D3DResource,
				                                                buffer.Is32Bit ? GI.Format.R32_UInt : GI.Format.R16_UInt, offset);
			}
			else
			{
				_graphics.Context.InputAssembler.SetIndexBuffer(null, GI.Format.Unknown, 0);
			}

			_indexBuffer = buffer;
		}

		/// <summary>
		/// Function to create a index buffer.
		/// </summary>
        /// <param name="name">The name of the buffer.</param>
		/// <param name="data">Data used to initialize the buffer.</param>
        /// <param name="usage">[Optional] Usage of the buffer.</param>
        /// <param name="is32Bit">[Optional] TRUE to indicate that we're using 32 bit indices, FALSE to use 16 bit indices </param>
		/// <typeparam name="T">Type of data used to populate the buffer.</typeparam>
		/// <returns>A new index buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
		/// </exception> 
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="data"/> is NULL (Nothing in VB.Net) or empty.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is empty.</para>
		/// </exception>
		/// <remarks>If creating an immutable index buffer, be sure to pre-populate it via the initialData parameter.</remarks>
        public GorgonIndexBuffer CreateIndexBuffer<T>(string name, T[] data, BufferUsage usage, bool is32Bit = true)
			where T : struct
		{
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "data");
            }

			int size = data.Length * DirectAccess.SizeOf<T>();

			using (var dataStream = new GorgonDataStream(data))
			{
				return CreateIndexBuffer(name, size, usage, is32Bit, false, dataStream);
			}
		}

		/// <summary>
		/// Function to create a index buffer.
		/// </summary>
        /// <param name="name">The name of the buffer.</param>
        /// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="usage">[Optional] Usage of the buffer.</param>
		/// <param name="is32Bit">[Optional] TRUE to indicate that we're using 32 bit indices, FALSE to use 16 bit indices </param>
		/// <param name="isOutput">[Optional] TRUE to allow this buffer to be used in stream output, FALSE to only allow stream input.</param>
		/// <param name="initialData">[Optional] Initial data to populate the index buffer with.</param>
		/// <returns>A new index buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the usage parameter is set to Immutable and the <paramref name="initialData"/> is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is empty.</para>
		/// </exception>
		/// <remarks>If creating an immutable index buffer, be sure to pre-populate it via the initialData parameter.</remarks>
		public GorgonIndexBuffer CreateIndexBuffer(string name, int size, BufferUsage usage = BufferUsage.Default, bool is32Bit = true, bool isOutput = false, GorgonDataStream initialData = null)
		{
            if (size < 1)
			{
				throw new ArgumentOutOfRangeException("size");
			}

			if ((usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Length == 0)))
			{
				throw new ArgumentException(Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA, "usage");
			}

            if (string.IsNullOrWhiteSpace(name))
            {
                name = string.Format("IndexBuffer #{0}", GorgonRenderStatistics.IndexBufferCount);
            }

			var buffer = new GorgonIndexBuffer(_graphics, name, usage, size, is32Bit, isOutput);
			buffer.Initialize(initialData);

			_graphics.AddTrackedObject(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create a vertex buffer.
		/// </summary>
        /// <param name="name">Name of the vertex buffer.</param>
		/// <param name="data">Data used to initialize the buffer.</param>
        /// <param name="usage">[Optional] Usage of the buffer.</param>
        /// <typeparam name="T">Type of data used to populate the buffer.</typeparam>
		/// <returns>A new vertex buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
        /// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="data"/> is NULL (Nothing in VB.Net) or empty.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="name"/> or the <paramref name="data"/> parameter is empty.</para>
        /// </exception>
		/// <remarks>If creating an immutable vertex buffer, be sure to pre-populate it via the initialData parameter.
		/// </remarks>
        public GorgonVertexBuffer CreateVertexBuffer<T>(string name, T[] data, BufferUsage usage = BufferUsage.Default)
			where T : struct
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

            if (data.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "data");
            }

			int size = data.Length * DirectAccess.SizeOf<T>();

			using (var dataStream = new GorgonDataStream(data))
			{
				return CreateVertexBuffer(name, size, usage, false, dataStream);
			}
		}

		/// <summary>
		/// Function to create a vertex buffer.
		/// </summary>
        /// <param name="name">Name of the vertex buffer.</param>
        /// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="usage">[Optional] Usage of the buffer.</param>
        /// <param name="isOutput">[Optional] TRUE to allow this buffer to be used in stream output, FALSE to only allow stream input.</param>
		/// <param name="initialData">[Optional] Initial data to populate the vertex buffer with.</param>
		/// <returns>A new vertex buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="initialData"/> is NULL (Nothing in VB.Net).</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is empty.</para>
		/// </exception>
		/// <remarks>If creating an immutable vertex buffer, be sure to pre-populate it via the initialData parameter.
		/// </remarks>
		public GorgonVertexBuffer CreateVertexBuffer(string name, int size, BufferUsage usage = BufferUsage.Default, bool isOutput = false, GorgonDataStream initialData = null)
		{
			if (size < 1)
			{
				throw new ArgumentOutOfRangeException("size");
			}

			if (usage == BufferUsage.Staging)
			{
				throw new ArgumentException(Resources.GORGFX_BUFFER_NO_STAGING, "usage");
			}

			if ((usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Length == 0)))
			{
				throw new ArgumentException(Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA, "usage");
			}

            if (string.IsNullOrWhiteSpace(name))
            {
                name = string.Format("VertexBuffer #{0}", GorgonRenderStatistics.VertexBufferCount);
            }

			var buffer = new GorgonVertexBuffer(_graphics, name, usage, size, isOutput);
			buffer.Initialize(initialData);

			_graphics.AddTrackedObject(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create an input layout object from a predefined type.
		/// </summary>
        /// <param name="name">The name of the input layout.</param>
        /// <param name="type">Type to evaluate.</param>
		/// <param name="shader">The shader that holds the input layout signature.</param>
		/// <returns>The input layout object to create.</returns>
		/// <exception cref="System.ArgumentException">Thrown when then <paramref name="name"/> parameter is an empty string.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shader"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="type"/> parameter is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the name parameter is NULL.</para>
		/// </exception>
		/// <remarks>The shader parameter is used to compare input layout on the shader side with the input layout.  If the layout is mismatched, a warning will appear in the debug output.
		/// <para>Note that any shader can be used with the input layout as long as the shader contains the same layout for the input, i.e. there is no need to create a new layout for each shader if the element layouts are identical.</para>
		/// </remarks>
		public GorgonInputLayout CreateInputLayout(string name, Type type, GorgonShader shader)
		{
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (shader == null)
            {
                throw new ArgumentNullException("shader");
            }

            var layout = new GorgonInputLayout(_graphics, name, shader);
			layout.InitializeFromType(type);

			_graphics.AddTrackedObject(layout);

			return layout;
		}

		/// <summary>
		/// Function to create an input layout object.
		/// </summary>
        /// <param name="name">The name of the input layout.</param>
        /// <param name="elements">The input elements to assign to the layout.</param>
		/// <param name="shader">The shader that holds the input layout signature.</param>
		/// <returns>The input layout object to create.</returns>
		/// <exception cref="System.ArgumentException">Thrown when then name parameter is an empty string.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shader"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="elements"/> parameter is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="elements"/> parameter is empty.</exception>
		/// <remarks>The shader parameter is used to compare input layout on the shader side with the input layout.  If the layout is mismatched, a warning will appear in the debug output.
		/// <para>Note that any shader can be used with the input layout as long as the shader contains the same layout for the input, i.e. there is no need to create a new layout for each shader if the element layouts are identical.</para>
		/// </remarks>
		public GorgonInputLayout CreateInputLayout(string name, IList<GorgonInputElement> elements, GorgonShader shader)
		{
		    if (shader == null)
            {
                throw new ArgumentNullException("shader");
            }

			if (elements == null)
			{
				throw new ArgumentNullException("elements");
			}

			if (elements.Count == 0)
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "elements");	
			}

			var layout = new GorgonInputLayout(_graphics, name, shader);
			layout.InitializeFromList(elements);

			_graphics.AddTrackedObject(layout);
			return layout;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputGeometry"/> class.
		/// </summary>
		internal GorgonInputGeometry(GorgonGraphics graphics)
		{
			_graphics = graphics;
			PrimitiveType = PrimitiveType.TriangleList;
			VertexBuffers = new VertexBufferBindingList(_graphics);
		}
		#endregion
	}
}
