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
using System.Collections;
using System.Collections.Generic;
using Gorgon.Core;
using SharpDX.Direct3D;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using Gorgon.Diagnostics;
using Gorgon.Math;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
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
			private D3D.VertexBufferBinding[] _D3DBindings;						// List of D3D bindings.
			private readonly GorgonVertexBufferBinding[] _bindings;				// List of bindings.
			private readonly GorgonGraphics _graphics;					// Graphics interface.
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
					return _bindings.Length;
				}
			}

			/// <summary>
			/// Property to set or return the vertex buffer binding for a given slot.
			/// </summary>
			/// <exception cref="GorgonException">Thrown when an the vertex buffer is already bound to another slot.</exception>
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

#if DEBUG
					if (!value.Equals(GorgonVertexBufferBinding.Empty))
					{
						var oldIndex = IndexOf(value);
						if (oldIndex != -1)
						{
							throw new GorgonException(GorgonResult.CannotBind,
							                          string.Format(Resources.GORGFX_VERTEXBUFFER_ALREADY_BOUND,
							                                        value.VertexBuffer != null ? value.VertexBuffer.Name : "NULL", oldIndex));
						}
					}
#endif

					var D3DBinding = value.Convert();
					_bindings[index] = value;
					_graphics.Context.InputAssembler.SetVertexBuffers(index, D3DBinding);
				}
			}
			#endregion

			#region Methods.
            /// <summary>
            /// Function to reset the vertex buffer states.
            /// </summary>
            internal void Reset()
            {
                for (int i = 0; i < _bindings.Length; i++)
                {
                    _bindings[i] = GorgonVertexBufferBinding.Empty;
                    _graphics.Context.InputAssembler.SetVertexBuffers(i, new D3D.VertexBufferBinding(null, 0, 0));
                }
            }

            /// <summary>
            /// Function to unbind any bindings with the specified vertex buffer.
            /// </summary>
            /// <param name="buffer">Buffer to unbind.</param>
		    internal void Unbind(GorgonVertexBuffer buffer)
		    {
                for (int i = 0; i < _bindings.Length; ++i)
                {
                    if (_bindings[i].VertexBuffer == buffer)
                    {
                        _bindings[i] = GorgonVertexBufferBinding.Empty;
                    }
                }
		    }

			/// <summary>
			/// Function to return the index of the vertex buffer with the specified name.
			/// </summary>
			/// <param name="name">Name of the vertex buffer.</param>
			/// <returns>The index of the vertex buffer if found, -1 if not.</returns>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
			public int IndexOf(string name)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}

				if (string.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
				}

				for (int i = 0; i < _bindings.Length; i++)
				{
					if ((_bindings[i].VertexBuffer != null) && (string.Equals(name, _bindings[i].VertexBuffer.Name, StringComparison.OrdinalIgnoreCase)))
					{
						return i;
					}
				}

				return -1;
			}

			/// <summary>
			/// Function to determine if the list contains a buffer with the specified name.
			/// </summary>
			/// <param name="name">Name of the buffer.</param>
			/// <returns><c>true</c> if found, <c>false</c> if not.</returns>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
			public bool Contains(string name)
			{
				return IndexOf(name) > -1;
			}

			/// <summary>
			/// Function to find the index of a vertex buffer binding that contains the specified vertex buffer.
			/// </summary>
			/// <param name="buffer">Vertex buffer to find.</param>
			/// <returns>The index of the buffer binding in the list, -1 if not found.</returns>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is NULL (Nothing in VB.Net).</exception>
			public int IndexOf(GorgonVertexBuffer buffer)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException("buffer");
				}

				for (int i = 0; i < _bindings.Length; i++)
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
			/// <returns><c>true</c> if found, <c>false</c> if not.</returns>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is NULL (Nothing in VB.Net).</exception>
			public bool Contains(GorgonVertexBuffer buffer)
			{
				return IndexOf(buffer) > -1;
			}

			/// <summary>
			/// Function to set a series of bindings at once.
			/// </summary>
			/// <param name="binding">Bindings to set.</param>
			/// <param name="slot">Index to start writing at.</param>
			/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="binding"/> parameter will set the bindings to empty (starting at <paramref name="slot"/>).</remarks>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the startIndex parameter is less than 0 or greater than the number of available bindings - 1.</exception>
			/// <exception cref="GorgonException">Thrown when an the vertex buffer is already bound to another slot.</exception>
			public void SetRange(int slot, GorgonVertexBufferBinding[] binding)
			{
				int count = _bindings.Length - slot;

				GorgonDebug.AssertParamRange(slot, 0, _bindings.Length, true, false,"startIndex");
                
                if (binding != null)
				{
					count = binding.Length.Min(_bindings.Length);
				}

				if ((_D3DBindings == null) || (_D3DBindings.Length != count))
				{
					_D3DBindings = new D3D.VertexBufferBinding[count];
				}

				for (int i = 0; i < count; i++)
				{
				    int slotIndex = i + slot;
					GorgonVertexBufferBinding currentBinding = GorgonVertexBufferBinding.Empty;

					if (binding != null)
					{
						currentBinding = binding[i];
					}

					// If we've already set the binding, then don't bother.
					if (_bindings[slotIndex].Equals(currentBinding))
					{
						continue;
					}

#if DEBUG
					if (!currentBinding.Equals(GorgonVertexBufferBinding.Empty))
					{
						var oldIndex = IndexOf(currentBinding);
						if (oldIndex != -1)
						{
							throw new GorgonException(GorgonResult.CannotBind,
							                          string.Format(Resources.GORGFX_VERTEXBUFFER_ALREADY_BOUND,
							                                        currentBinding.VertexBuffer != null
								                                        ? currentBinding.VertexBuffer.Name
								                                        : "NULL", oldIndex));
						}
					}
#endif

					_bindings[slotIndex] = currentBinding;
					_D3DBindings[i] = currentBinding.Convert();
				}

				_graphics.Context.InputAssembler.SetVertexBuffers(slot, _D3DBindings);
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
                _bindings = new GorgonVertexBufferBinding[graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4_1 ? 16 : 32];
				for (int i = 0; i < _bindings.Length; i++)
				{
					_bindings[i] = GorgonVertexBufferBinding.Empty;
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
				// ReSharper disable LoopCanBeConvertedToQuery
				// ReSharper disable ForCanBeConvertedToForeach
                for (int i = 0; i < _bindings.Length; i++ )
                {
                    yield return _bindings[i];
                }
				// ReSharper restore ForCanBeConvertedToForeach
				// ReSharper restore LoopCanBeConvertedToQuery
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return _bindings.GetEnumerator();
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
				return Array.IndexOf(_bindings, item);
			}

			/// <summary>
			/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
			/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
			/// <exception cref="System.NotSupportedException">This method is not supported.</exception>
			void IList<GorgonVertexBufferBinding>.Insert(int index, GorgonVertexBufferBinding item)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the item to remove.</param>
			/// <exception cref="System.NotSupportedException">This method is not supported.</exception>
			void IList<GorgonVertexBufferBinding>.RemoveAt(int index)
			{
				throw new NotSupportedException();
			}
			#endregion

			#region ICollection<GorgonVertexBufferBinding> Members
			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
			/// <exception cref="System.NotSupportedException">This method is not supported.</exception>
			void ICollection<GorgonVertexBufferBinding>.Add(GorgonVertexBufferBinding item)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
			/// </summary>
			/// <exception cref="System.NotSupportedException">This method is not supported.</exception>
			void ICollection<GorgonVertexBufferBinding>.Clear()
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Function to return whether an item exists within this collection.
			/// </summary>
			/// <param name="item">Item to scan for.</param>
			/// <returns><c>true</c> if found, <c>false</c> if not.</returns>
			public bool Contains(GorgonVertexBufferBinding item)
			{
				return IndexOf(item) > -1;
			}

			/// <summary>
			/// Function to copy the bindings to an external array.
			/// </summary>
			/// <param name="array">The array to copy into.</param>
			/// <param name="arrayIndex">Index of the array to start writing at.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="array"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> parameter is less than 0 or not less than the length of the array.</exception>
			public void CopyTo(GorgonVertexBufferBinding[] array, int arrayIndex)
			{
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}

				if ((arrayIndex < 0) || (arrayIndex >= array.Length))
				{
					throw new ArgumentOutOfRangeException("arrayIndex");
				}

				int count = (array.Length - arrayIndex).Min(_bindings.Length);

				for (int i = 0; i < count; i++)
				{
					array[i + arrayIndex] = _bindings[i];
				}
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
			/// </summary>
			/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
			///   </returns>
			public bool IsReadOnly
			{
				get 
				{
					return false;
				}
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
			/// </summary>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
			/// <returns>
			/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
			/// </returns>
			/// <exception cref="System.NotSupportedException">This method is not supported.</exception>
			bool ICollection<GorgonVertexBufferBinding>.Remove(GorgonVertexBufferBinding item)
			{
				throw new NotSupportedException();
			}
			#endregion
		}
		#endregion

		#region Variables.
		private PrimitiveType _primitiveType = PrimitiveType.Unknown;		// Primitive type to use.
		private GorgonInputLayout _inputLayout;								// The current input layout.
		private readonly GorgonGraphics _graphics;					// Current graphics interface.
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
				_graphics.Context.InputAssembler.PrimitiveTopology = (PrimitiveTopology)value;
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
        /// Function to reset the states for the input assembler.
        /// </summary>
        internal void Reset()
        {
            _graphics.Context.InputAssembler.SetIndexBuffer(null, GI.Format.Unknown, 0);
            VertexBuffers.Reset();
            _graphics.Context.InputAssembler.PrimitiveTopology = (PrimitiveTopology.TriangleList);
            _graphics.Context.InputAssembler.InputLayout = null;

            _indexBuffer = null;
            _inputLayout = null;
            _primitiveType = PrimitiveType.TriangleList;
        }

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
				                                                buffer.Settings.Use32BitIndices ? GI.Format.R32_UInt : GI.Format.R16_UInt, offset);
			}
			else
			{
				_graphics.Context.InputAssembler.SetIndexBuffer(null, GI.Format.Unknown, 0);
			}

			_indexBuffer = buffer;
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
        /// <exception cref="GorgonException">Thrown when the graphics context is deferred.</exception>
		/// <remarks>The shader parameter is used to compare input layout on the shader side with the input layout.  If the layout is mismatched, a warning will appear in the debug output.
		/// <para>Note that any shader can be used with the input layout as long as the shader contains the same layout for the input, i.e. there is no need to create a new layout for each shader if the element layouts are identical.</para>
        /// <para>This function should not be called from a deferred context.</para>
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
		/// <exception cref="System.ArgumentException">
		/// Thrown when then <paramref name="name"/> parameter is an empty string.
		/// <para>-or-</para>
        /// <para>Thrown when the <paramref name="elements"/> parameter is empty.</para>
		/// </exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shader"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="elements"/> parameter is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the graphics context is deferred.</exception>
		/// <remarks>The shader parameter is used to compare input layout on the shader side with the input layout.  If the layout is mismatched, a warning will appear in the debug output.
		/// <para>Note that any shader can be used with the input layout as long as the shader contains the same layout for the input, i.e. there is no need to create a new layout for each shader if the element layouts are identical.</para>
		/// <para>This function should not be called from a deferred context.</para>
		/// </remarks>
		public GorgonInputLayout CreateInputLayout(string name, IList<GorgonInputElement> elements, GorgonShader shader)
		{
            if (_graphics.IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }
            
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
		/// <param name="graphics">The graphics instance that owns this instance.</param>
		internal GorgonInputGeometry(GorgonGraphics graphics)
		{
			_graphics = graphics;
			PrimitiveType = PrimitiveType.TriangleList;
			VertexBuffers = new VertexBufferBindingList(_graphics);
		}
		#endregion
	}
}
