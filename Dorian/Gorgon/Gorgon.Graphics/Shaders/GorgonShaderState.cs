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
// Created: Thursday, December 15, 2011 1:24:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Shader state interface.
	/// </summary>
	public abstract class GorgonShaderState<T>
		where T : GorgonShader
	{
		#region Classes.
		/// <summary>
		/// Sampler states.
		/// </summary>
		/// <remarks>This is used to control how textures are used by the shader.</remarks>
		public sealed class TextureSamplerState
			: GorgonState<GorgonTextureSamplerStates>, IList<GorgonTextureSamplerStates>
		{
			#region Variables.
			private readonly GorgonTextureSamplerStates[] _states;								// List of sampler states.
			private readonly D3D.SamplerState[] _D3DStates;										// List of sampler state objects.
			private readonly GorgonShaderState<T> _shader;										// Shader that owns the state.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the current state.
			/// </summary>
			/// <remarks>This property is not used for texture samplers and will throw an exception if used.</remarks>
			/// <exception cref="System.NotImplementedException">Thrown when this property is accessed because it is not implemented for texture samplers.</exception>
			[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
			public override GorgonTextureSamplerStates States
			{
				get
				{
					return _states[0];
				}
				set
				{
					throw new NotImplementedException();
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Applies the state.
			/// </summary>
			/// <param name="stateObject">The state object.</param>
			internal override void ApplyState(D3D.DeviceChild stateObject)
			{				
				// Not used.
			}

			/// <summary>
			/// Function to retrieve the D3D state object.
			/// </summary>
			/// <param name="stateType">The state type information.</param>
			/// <returns>The D3D state object.</returns>
			internal override D3D.DeviceChild GetStateObject(ref GorgonTextureSamplerStates stateType)
			{
#if DEBUG
				// Perform validation of the state type.
				if (stateType.ComparisonFunction == ComparisonOperators.Unknown)
				{
					throw new GorgonException(GorgonResult.CannotBind,
					                          string.Format(Properties.Resources.GORGFX_INVALID_ENUM_VALUE,
					                                        stateType.ComparisonFunction, "ComparisonFunction"));
				}

				if (stateType.DepthAddressing == TextureAddressing.Unknown)
				{
					throw new GorgonException(GorgonResult.CannotBind,
											  string.Format(Properties.Resources.GORGFX_INVALID_ENUM_VALUE,
															stateType.DepthAddressing, "DepthAddressing"));
				}

				if (stateType.HorizontalAddressing == TextureAddressing.Unknown)
				{
					throw new GorgonException(GorgonResult.CannotBind,
											  string.Format(Properties.Resources.GORGFX_INVALID_ENUM_VALUE,
															stateType.HorizontalAddressing, "HorizontalAddressing"));
				}

				if (stateType.VerticalAddressing == TextureAddressing.Unknown)
				{
					throw new GorgonException(GorgonResult.CannotBind,
											  string.Format(Properties.Resources.GORGFX_INVALID_ENUM_VALUE,
															stateType.VerticalAddressing, "VerticalAddressing"));
				}

				if (stateType.TextureFilter == TextureFilter.Unknown)
				{
					throw new GorgonException(GorgonResult.CannotBind,
											  string.Format(Properties.Resources.GORGFX_INVALID_ENUM_VALUE,
															stateType.TextureFilter, "TextureFilter"));
				}
#endif

				var desc = new D3D.SamplerStateDescription
					{
						AddressU = (D3D.TextureAddressMode)stateType.HorizontalAddressing,
						AddressV = (D3D.TextureAddressMode)stateType.VerticalAddressing,
						AddressW = (D3D.TextureAddressMode)stateType.DepthAddressing,
						BorderColor =
							new SharpDX.Color4(stateType.BorderColor.Red, stateType.BorderColor.Green, stateType.BorderColor.Blue,
							                   stateType.BorderColor.Alpha),
						ComparisonFunction = (D3D.Comparison)stateType.ComparisonFunction,
						MaximumAnisotropy = stateType.MaxAnisotropy,
						MaximumLod = stateType.MaxLOD,
						MinimumLod = stateType.MinLOD,
						MipLodBias = stateType.MipLODBias
					};


				if (stateType.TextureFilter == TextureFilter.Anisotropic)
				{
					desc.Filter = D3D.Filter.Anisotropic;
				}

				if (stateType.TextureFilter == TextureFilter.CompareAnisotropic)
				{
					desc.Filter = D3D.Filter.ComparisonAnisotropic;
				}

				// Sort out filter stateType.
				// Check comparison stateType.
				if ((stateType.TextureFilter & TextureFilter.Comparison) == TextureFilter.Comparison)
				{
					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinMagMipLinear;
					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinMagMipPoint;

					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinMagLinearMipPoint;
					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinLinearMagMipPoint;
					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinLinearMagPointMipLinear;

					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinPointMagMipLinear;
					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinMagPointMipLinear;
					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinPointMagLinearMipPoint;
				}
				else
				{
					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinMagMipLinear;
					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinMagMipPoint;

					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinMagLinearMipPoint;
					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinLinearMagMipPoint;
					if (((stateType.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinLinearMagPointMipLinear;

					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinPointMagMipLinear;
					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((stateType.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinMagPointMipLinear;
					if (((stateType.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((stateType.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((stateType.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinPointMagLinearMipPoint;
				}

				var state = new D3D.SamplerState(Graphics.D3DDevice, desc)
					{
						DebugName = "Gorgon Sampler State #" + StateCacheCount
					};

				return state;
			}

			/// <summary>
			/// Function to set a range of states at once.
			/// </summary>
			/// <param name="slot">Starting slot for the states.</param>
			/// <param name="states">States to set.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of state slots.
			/// <para>-or-</para>
			/// <para>Thrown when the <paramref name="states"/> count + the slot is greater than or equal to the number of available state slots.</para>
			/// </exception>
			public void SetRange(int slot, IEnumerable<GorgonTextureSamplerStates> states)
			{
				int count = 0;

				GorgonDebug.AssertNull<IEnumerable<GorgonTextureSamplerStates>>(states, "states");
#if DEBUG
				if ((slot < 0) || (slot >= Count) || ((slot + states.Count()) >= Count))
					throw new ArgumentOutOfRangeException("Cannot have more than " + Count.ToString() + " slots occupied.");
#endif

				count = states.Count();
				for (int i = 0; i < count; i++)
				{
					int stateIndex = i + slot;
					var state = states.ElementAtOrDefault(i);

					if (!_states[stateIndex].Equals(ref state))
					{
						D3D.DeviceChild d3dState = GetFromCache(ref state);

						if (d3dState == null)
						{
							d3dState = GetStateObject(ref state);
							StoreInCache(ref state, d3dState);
						}

						_states[stateIndex] = state;
						_D3DStates[i] = (D3D.SamplerState)d3dState;
					}
				}

				_shader.SetSamplers(slot, count, _D3DStates);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonBlendRenderState"/> class.
			/// </summary>
			/// <param name="shaderState">Shader that owns the state.</param>
			internal TextureSamplerState(GorgonShaderState<T> shaderState)
				: base(shaderState.Graphics)
			{
				int count = D3D.CommonShaderStage.SamplerSlotCount;

				_shader = shaderState;
				if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				{
					if (shaderState is GorgonVertexShaderState)
						count = 0;
					else
						count = 16;
				}

				_states = new GorgonTextureSamplerStates[count];
				_D3DStates = new D3D.SamplerState[_states.Length];
			}
			#endregion

			#region IList<GorgonTextureSamplerStates> Members
			#region Properties.
			/// <summary>
			/// Gets or sets the element at the specified index.
			/// </summary>
			/// <returns>The element at the specified index.</returns>
			///   
			/// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
			///   
			/// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
			public GorgonTextureSamplerStates this[int index]
			{
				get
				{
					return _states[index];
				}
				set
				{					
					if (!_states[index].Equals(ref value))
					{
						D3D.DeviceChild state = GetFromCache(ref value);

						if (state == null)
						{
							state = GetStateObject(ref value);
							StoreInCache(ref value, state);
						}

						_states[index] = value;
						_D3DStates[0] = (D3D.SamplerState)state;
						_shader.SetSamplers(index, 1, _D3DStates);
					}
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
			/// <returns>
			/// The index of item if found in the list; otherwise, -1.
			/// </returns>
			public int IndexOf(GorgonTextureSamplerStates item)
			{
				for (int i = 0; i < _states.Length; i++)
				{
					if (_states[i].Equals(ref item))
					{
						return i;
					}
				}

				return -1;
			}

			/// <summary>
			/// Inserts the specified index.
			/// </summary>
			/// <param name="index">The index.</param>
			/// <param name="item">The item.</param>
			void IList<GorgonTextureSamplerStates>.Insert(int index, GorgonTextureSamplerStates item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes at.
			/// </summary>
			/// <param name="index">The index.</param>
			void IList<GorgonTextureSamplerStates>.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region ICollection<GorgonTextureSamplerStates> Members
			#region Properties.
			/// <summary>
			/// Property to return the number of sampler slots.
			/// </summary>
			public int Count
			{
				get
				{
					return _states.Length;
				}
			}

			/// <summary>
			/// Property to return whether this list is read-only or not.
			/// </summary>
			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Adds the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			void ICollection<GorgonTextureSamplerStates>.Add(GorgonTextureSamplerStates item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Clears this instance.
			/// </summary>
			void ICollection<GorgonTextureSamplerStates>.Clear()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Function to return whether the specified sampler state is bound.
			/// </summary>
			/// <param name="item">Sampler state to look up.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonTextureSamplerStates item)
			{
				return IndexOf(item) != -1;
			}

			/// <summary>
			/// Function to copy the list of bound sampler states to an array.
			/// </summary>
			/// <param name="array">The array to copy into.</param>
			/// <param name="arrayIndex">Index of the array to start writing at.</param>
			public void CopyTo(GorgonTextureSamplerStates[] array, int arrayIndex)
			{
				_states.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Removes the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			bool ICollection<GorgonTextureSamplerStates>.Remove(GorgonTextureSamplerStates item)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region IEnumerable<GorgonTextureSamplerStates> Members
			/// <summary>
			/// Function to return an enumerator for the sampler states.
			/// </summary>
			/// <returns>The enumerator for the sampler states.</returns>
			public IEnumerator<GorgonTextureSamplerStates> GetEnumerator()
			{
				foreach (var item in _states)
					yield return item;
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			/// <returns></returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return ((System.Collections.IEnumerable)_states).GetEnumerator();
			}
			#endregion
		}

		/// <summary>
		/// A list of constant buffers.
		/// </summary>
		public sealed class ShaderConstantBuffers
			: IList<GorgonConstantBuffer>
		{
			#region Variables.
			private readonly IList<GorgonConstantBuffer> _buffers;
			private readonly GorgonShaderState<T> _shader;
			private readonly D3D.Buffer[] _D3DBufferArray;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the number of buffers.
			/// </summary>
			public int Count
			{
				get
				{
					return _buffers.Count;
				}
			}

			/// <summary>
			/// Property to set or return a constant buffer at the specified index.
			/// </summary>
			public GorgonConstantBuffer this[int index]
			{
				get
				{
					return _buffers[index];
				}
				set
				{
					if (_buffers[index] == value)
					{
						return;
					}

					_buffers[index] = value;
					if (value != null)
					{
						_D3DBufferArray[0] = value.D3DBuffer;
					}
					else
					{
						_D3DBufferArray[0] = null;
					}

					_shader.SetConstantBuffers(index, 1, _D3DBufferArray);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to unbind a constant buffer.
			/// </summary>
			/// <param name="buffer">Buffer to unbind.</param>
			internal void Unbind(GorgonConstantBuffer buffer)
			{
				for (int i = 0; i < _buffers.Count - 1; i++)
				{
					if (_buffers[i] == buffer)
						_buffers[i] = null;
				}
			}

			/// <summary>
			/// Function to set a range of constant buffers at once.
			/// </summary>
			/// <param name="slot">Starting slot for the buffer.</param>
			/// <param name="buffers">Buffers to set.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of constant buffer slots.
			/// <para>-or-</para>
			/// <para>Thrown when the <paramref name="buffers"/> count + the slot is greater than or equal to the number of available constant buffer slots.</para>
			/// </exception>
			public void SetRange(int slot, IEnumerable<GorgonConstantBuffer> buffers)
			{
				int count = 0;

				GorgonDebug.AssertNull<IEnumerable<GorgonConstantBuffer>>(buffers, "buffers");
#if DEBUG
				if ((slot < 0) || (slot >= _buffers.Count) || ((slot + buffers.Count()) >= _buffers.Count))
					throw new ArgumentOutOfRangeException("Cannot have more than " + _buffers.Count.ToString() + " slots occupied.");
#endif

				count = buffers.Count();
				for (int i = 0; i < count; i++)
				{
					var buffer = buffers.ElementAtOrDefault(i);

					_buffers[i + slot] = buffer;
					if (buffer != null)
						_D3DBufferArray[i] = buffer.D3DBuffer;
					else
						_D3DBufferArray[i] = null;
				}

				_shader.SetConstantBuffers(slot, count, _D3DBufferArray);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="ShaderConstantBuffers"/> class.
			/// </summary>
			/// <param name="shader">Shader stage state.</param>
			internal ShaderConstantBuffers(GorgonShaderState<T> shader)
			{
				_buffers = new GorgonConstantBuffer[D3D.CommonShaderStage.ConstantBufferApiSlotCount];
				_shader = shader;
				_D3DBufferArray = new D3D.Buffer[_buffers.Count];
			}
			#endregion

			#region IList<GorgonConstantBuffer> Members
			/// <summary>
			/// Indexes the of.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			public int IndexOf(GorgonConstantBuffer item)
			{
				GorgonDebug.AssertNull<GorgonConstantBuffer>(item, "item");

				return _buffers.IndexOf(item);
			}

			/// <summary>
			/// Inserts the specified index.
			/// </summary>
			/// <param name="index">The index.</param>
			/// <param name="item">The item.</param>
			void IList<GorgonConstantBuffer>.Insert(int index, GorgonConstantBuffer item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes at.
			/// </summary>
			/// <param name="index">The index.</param>
			void IList<GorgonConstantBuffer>.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			#endregion

			#region ICollection<GorgonConstantBuffer> Members
			#region Properties.
			/// <summary>
			/// Property to return whether this list is read only or not.
			/// </summary>
			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Adds the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			void ICollection<GorgonConstantBuffer>.Add(GorgonConstantBuffer item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Clears this instance.
			/// </summary>
			void ICollection<GorgonConstantBuffer>.Clear()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Function to return whether the specified constant buffer is bound or not.
			/// </summary>
			/// <param name="item">Constant buffer to check.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonConstantBuffer item)
			{
				return _buffers.Contains(item);
			}

			/// <summary>
			/// Function to copy the contents of this list to an array.
			/// </summary>
			/// <param name="array">Array to copy into.</param>
			/// <param name="arrayIndex">Index in the array to start writing at.</param>
			public void CopyTo(GorgonConstantBuffer[] array, int arrayIndex)
			{
				_buffers.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Removes the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			bool ICollection<GorgonConstantBuffer>.Remove(GorgonConstantBuffer item)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region IEnumerable<GorgonConstantBuffer> Members
			/// <summary>
			/// Function to return an enumerator for the list.
			/// </summary>
			/// <returns>The enumerator for the list.</returns>
			public IEnumerator<GorgonConstantBuffer> GetEnumerator()
			{
				foreach (var item in _buffers)
					yield return item;
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
		}			

		/// <summary>
		/// A list of shader resource views.
		/// </summary>
		/// <remarks>A view is a way for a shader to read (or potentially write) a resource.  Views can also be used to cast the data 
        /// in a resource to another type.</remarks>
		public sealed class ShaderResourceViews
			: IList<GorgonResourceView>
		{
			#region Variables.
			private readonly D3D.ShaderResourceView[] _views;			// Shader resource views.
			private readonly IList<GorgonResourceView> _resources;		// Shader resources.
			private readonly GorgonShaderState<T> _shader;				// Shader that owns this interface.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to unbind a shader resource view.
			/// </summary>
			/// <param name="resourceView">Resource view to unbind.</param>
			internal void Unbind(GorgonResourceView resourceView)
			{
				for (int i = 0; i < Count; i++)
				{
					if (_resources[i] == resourceView)
					{
						this[i] = null;
					}
				}
			}

            /// <summary>
            /// Function to unbind a shader resource view.
            /// </summary>
            /// <param name="resource">Resource containing the view to unbind.</param>
            internal void Unbind(GorgonResource resource)
            {
				if (resource == null)
				{
					return;
				}

                Unbind(resource.DefaultView);
            }

			/// <summary>
			/// Function to re-seat a resource view after it's been altered.
			/// </summary>
			/// <param name="resourceView">Resource view to re-seat.</param>
			internal void ReSeat(GorgonResourceView resourceView)
			{
				int index = IndexOf(resourceView);

				if (index > -1)
				{
					this[index] = null;
					this[index] = resourceView;
				}
			}

			/// <summary>
			/// Function to re-seat a resource view after it's been altered.
			/// </summary>
			/// <param name="resource">Resource containing the view to re-seat.</param>
            internal void ReSeat(GorgonResource resource)
            {
                ReSeat(resource.DefaultView);
            }

			/// <summary>
			/// Function to set a range of resource views at one time.
			/// </summary>
			/// <param name="slot">Resource view slot to start at.</param>
			/// <param name="resourceViews">A list of resource views to set.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceViews"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of resource view slots.
			/// <para>-or-</para>
			/// <para>Thrown when the buffers count + the slot is greater than or equal to the number of available resource slots.</para>
			/// </exception>
			public void SetRange(int slot, IEnumerable<GorgonResourceView> resourceViews)
			{
				int count = 0;

				GorgonDebug.AssertNull<IEnumerable<GorgonResourceView>>(resourceViews, "resourceViews");
#if DEBUG
				if ((slot < 0) || (slot >= _resources.Count) || ((slot + resourceViews.Count()) >= _resources.Count))
					throw new ArgumentOutOfRangeException("Cannot have more than " + _resources.Count.ToString() + " slots occupied.");
#endif

				count = resourceViews.Count();
				for (int i = 0; i < count; i++)
				{
					var buffer = resourceViews.ElementAtOrDefault(i);

#if DEBUG
                    if (buffer != null)
                    {
                        int bufferIndex = _resources.IndexOf(buffer);

                        if ((bufferIndex != i + slot) && (bufferIndex != -1) && (_views[bufferIndex] == buffer.D3DResourceView))
                        {
                            throw new ArgumentException("The resource view at index [" + i.ToString() + "] is already bound to a shader with the same resource view.");
                        }
                    }
#endif

					_resources[i + slot] = buffer;
					if (buffer != null)
						_views[i] = _resources[i + slot].D3DResourceView;
					else
						_views[i] = null;
				}

				_shader.SetResources(slot, count, _views);
			}

            /// <summary>
            /// Function to return the texture resource assigned to the view at the specified index.
            /// </summary>
            /// <typeparam name="Tx">Type of texture.</typeparam>
            /// <param name="index">Index of the texture to look up.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is outside of the available resource view slots.</exception>
            /// <exception cref="System.InvalidCastException">Thrown when the type of resource at the specified index is not a texture.</exception>
            /// <returns>The texture assigned to the view at the specified index, or NULL if nothing is assigned to the specified index.</returns>
            public Tx GetTexture<Tx>(int index)
                where Tx : GorgonTexture
            {
                GorgonDebug.AssertParamRange(index, 0, _resources.Count, "index");

                var resourceView = _resources[index];
                
#if DEBUG
                if ((resourceView != null) && (resourceView.Resource != null) && (!(resourceView.Resource is Tx)))
                {
                    throw new InvalidCastException("The resource at index [" + index.ToString() + "] is not a texture.");
                }
#endif

                if (resourceView == null)
                {
                    return null;    
                }

                return (Tx)resourceView.Resource;                
            }

            /// <summary>
            /// Function to set a texture resource's default view at the specified index.
            /// </summary>
            /// <typeparam name="Tx">Type of texture.</typeparam>
            /// <param name="index">Index of the resource view to use.</param>
            /// <param name="texture">Texture to assign.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is outside of the available resource view slots.</exception>
            public void SetTexture<Tx>(int index, Tx texture)
                where Tx : GorgonTexture
            {
                GorgonDebug.AssertParamRange(index, 0, _resources.Count, "index");

                if (texture != null)
                {
                    this[index] = texture.DefaultView;
                }
                else
                {
                    this[index] = null;
                }
            }

            /// <summary>
            /// Function to return the shader buffer resource assigned to the view at the specified index.
            /// </summary>
            /// <typeparam name="B">Type of shader buffer.</typeparam>
            /// <param name="index">Index of the texture to look up.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is outside of the available resource view slots.</exception>
            /// <exception cref="System.InvalidCastException">Thrown when the type of resource at the specified index is not a shader buffer.</exception>
            /// <returns>The shader buffer assigned to the view at the specified index, or NULL if nothing is assigned to the specified index.</returns>
            public B GetShaderBuffer<B>(int index)
                where B : GorgonShaderBuffer
            {
                GorgonDebug.AssertParamRange(index, 0, _resources.Count, "index");

                var resourceView = _resources[index];
#if DEBUG
                if ((resourceView != null) && (resourceView.Resource != null) && (!(resourceView.Resource is B)))
                {
                    throw new InvalidCastException("The resource at index [" + index.ToString() + "] is not a shader buffer.");
                }
#endif

                if (resourceView == null)
                {
                    return null;    
                }

                return (B)resourceView.Resource;                
            }

            /// <summary>
            /// Function to set a shader buffer resource's default view at the specified index.
            /// </summary>
            /// <typeparam name="B">Type of shader buffer.</typeparam>
            /// <param name="index">Index of the resource view to use.</param>
            /// <param name="buffer">Shader buffer to assign.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is outside of the available resource view slots.</exception>
            public void SetShaderBuffer<B>(int index, B buffer)
                where B : GorgonShaderBuffer
            {
                GorgonDebug.AssertParamRange(index, 0, _resources.Count, "index");

                if (buffer != null)
                {
                    this[index] = buffer.DefaultView;
                }
                else
                {
                    this[index] = null;
                }
            }
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonShaderState&lt;T&gt;.ShaderResourceViews"/> class.
			/// </summary>
			/// <param name="shader">Shader state that owns this interface.</param>
			internal ShaderResourceViews(GorgonShaderState<T> shader)
			{
				_shader = shader;

				if ((_shader is GorgonVertexShaderState) && (_shader.Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b))
				{
					_views = new D3D.ShaderResourceView[] { };
					_resources = new GorgonResourceView[] { };
				}
				else
				{
					_views = new D3D.ShaderResourceView[D3D.CommonShaderStage.InputResourceSlotCount];
					_resources = new GorgonResourceView[_views.Length];							
				}
			}
			#endregion

			#region IList<GorgonResourceView> Members
			#region Properties.            
			/// <summary>
			/// Property to set or return the bound shader resource view.
			/// </summary>
			public GorgonResourceView this[int index]
			{
				get
				{
					return _resources[index];
				}
				set
				{
					if (_resources[index] != value)
					{
#if DEBUG
						if (value != null)
						{
							int currentIndex = IndexOf(value);

							if ((currentIndex != -1) && (currentIndex != index) && (_views[currentIndex] == value.D3DResourceView))
							{
								throw new ArgumentException("The resource view at index [" + index.ToString() + "] is already bound to a shader with the same resource view.");
							}
						}
#endif
						_resources[index] = value;
						if (value != null)
							_views[0] = _resources[index].D3DResourceView;
						else
							_views[0] = null;

						_shader.SetResources(index, 1, _views);
					}
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to return the index of the specified shader resource.
			/// </summary>
			/// <param name="item">The shader resource to find the index of.</param>
			/// <returns>The index if found, or -1 if not.</returns>
			public int IndexOf(GorgonResourceView item)
			{
				GorgonDebug.AssertNull<GorgonResourceView>(item, "item");

				return _resources.IndexOf(item);
			}

			/// <summary>
			/// Inserts the specified index.
			/// </summary>
			/// <param name="index">The index.</param>
			/// <param name="item">The item.</param>
			void IList<GorgonResourceView>.Insert(int index, GorgonResourceView item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes at.
			/// </summary>
			/// <param name="index">The index.</param>
			void IList<GorgonResourceView>.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region ICollection<GorgonResourceView> Members
			#region Properties.
			/// <summary>
			/// Property to return the number of resource view slots.
			/// </summary>
			public int Count
			{
				get
				{
					return _resources.Count;
				}
			}

			/// <summary>
			/// Property to return whether the list is read-only or not.
			/// </summary>
			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Adds the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			void ICollection<GorgonResourceView>.Add(GorgonResourceView item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Clears this instance.
			/// </summary>
			void ICollection<GorgonResourceView>.Clear()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Function to return whether the list contains the specified texture.
			/// </summary>
			/// <param name="item">Texture to find.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonResourceView item)
			{
				return _resources.Contains(item);
			}

			/// <summary>
			/// Function to copy the resource views to an array.
			/// </summary>
			/// <param name="array">Array to copy into.</param>
			/// <param name="arrayIndex">Index in the array to start writing at.</param>
			public void CopyTo(GorgonResourceView[] array, int arrayIndex)
			{
#if DEBUG
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if ((arrayIndex < 0) || (arrayIndex >= array.Length))
                {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
#endif

			    int count = array.Length.Min(_resources.Count);

			    for (int i = 0; i < count; i++)
			    {
			        array[i] = _resources[i];
			    }
			}

			/// <summary>
			/// Removes the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			bool ICollection<GorgonResourceView>.Remove(GorgonResourceView item)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region IEnumerable<GorgonResourceView> Members
			/// <summary>
			/// Function to return an enumerator for the list.
			/// </summary>
			/// <returns>The enumerator for the list.</returns>
			public IEnumerator<GorgonResourceView> GetEnumerator()
			{
				foreach (var resource in _resources)
					yield return resource;
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
		}
		#endregion

		#region Variables.
		private T _current;									// Current shader.
		private TextureSamplerState _samplers;				// Sampler states.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		public virtual T Current
		{
			get
			{
				return _current;
			}
			set
			{
				if (_current != value)
				{
					_current = value;
					SetCurrent();
				}
			}
		}

		/// <summary>
		/// Property to return the list of constant buffers for the shaders.
		/// </summary>
		public virtual ShaderConstantBuffers ConstantBuffers
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the sampler states.
		/// </summary>
		/// <remarks>On a SM2_a_b device, and while using a Vertex Shader, setting a sampler will raise an exception.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		public virtual TextureSamplerState TextureSamplers
		{
			get
			{
				return _samplers;
			}
		}

		/// <summary>
		/// Property to return the list of resources for the shaders.
		/// </summary>
		/// <remarks>
		/// A resource may be a raw buffer (with shader binding enabled), <see cref="GorgonLibrary.Graphics.GorgonStructuredBuffer">structured buffer</see>, append/consume buffer, or a <see cref="GorgonLibrary.Graphics.GorgonTexture">texture</see>.
		/// <para>On a SM2_a_b device, and while using a Vertex Shader, setting a texture will raise an exception.</para></remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		public virtual ShaderResourceViews Resources
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the current shader.
		/// </summary>
		protected abstract void SetCurrent();

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count">Number of resources to update.</param>
		/// <param name="resources">Resources to update.</param>
		protected abstract void SetResources(int slot, int count, D3D.ShaderResourceView[] resources);

		/// <summary>
		/// Function to set the texture samplers for a shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count">Number of samplers to update.</param>
		/// <param name="samplers">Samplers to update.</param>
		protected abstract void SetSamplers(int slot, int count, D3D.SamplerState[] samplers);		

		/// <summary>
		/// Function to set constant buffers for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count">Number of constant buffers to update.</param>
		/// <param name="buffers">Constant buffers to update.</param>
		protected abstract void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers);

		/// <summary>
		/// Function to clean up.
		/// </summary>
		internal virtual void CleanUp()
		{
			if (_samplers != null)
			{
				_samplers.CleanUp();
			}

			_samplers = null;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderState&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		protected GorgonShaderState(GorgonGraphics graphics)
		{
			Graphics = graphics;			
			ConstantBuffers = new ShaderConstantBuffers(this);
			_samplers = new TextureSamplerState(this);
			Resources = new ShaderResourceViews(this);
		}
		#endregion
	}

	/// <summary>
	/// Pixel shader states.
	/// </summary>
	public class GorgonPixelShaderState
		: GorgonShaderState<GorgonPixelShader>
	{
		#region Methods.
		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		protected override void SetCurrent()
		{
			if (Current == null)
				Graphics.Context.PixelShader.Set(null);
			else
				Graphics.Context.PixelShader.Set(Current.D3DShader);
		}

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="resources">Resources to update.</param>
		protected override void SetResources(int slot, int count, D3D.ShaderResourceView[] resources)
		{
			if (count == 1)
				Graphics.Context.PixelShader.SetShaderResource(slot, resources[0]);
			else
				Graphics.Context.PixelShader.SetShaderResources(slot, count, resources);
		}

		/// <summary>
		/// Function to set the texture samplers for a shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="samplers">Samplers to update.</param>
		protected override void SetSamplers(int slot, int count, D3D.SamplerState[] samplers)
		{
			if (count == 1)
				Graphics.Context.PixelShader.SetSampler(slot, samplers[0]);
			else
				Graphics.Context.PixelShader.SetSamplers(slot, count, samplers);
		}

		/// <summary>
		/// Function to set constant buffers for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="buffers">Constant buffers to update.</param>
		protected override void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers)
		{
			if (count == 1)
				Graphics.Context.PixelShader.SetConstantBuffer(slot, buffers[0]);
			else
				Graphics.Context.PixelShader.SetConstantBuffers(slot, count, buffers);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPixelShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		protected internal GorgonPixelShaderState(GorgonGraphics graphics)
			: base(graphics)
		{
		}
		#endregion
	}

	/// <summary>
	/// Vertex shader states.
	/// </summary>
	public class GorgonVertexShaderState
		: GorgonShaderState<GorgonVertexShader>
	{
		#region Methods.
		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		protected override void SetCurrent()
		{
			if (Current == null)
				Graphics.Context.VertexShader.Set(null);
			else
				Graphics.Context.VertexShader.Set(Current.D3DShader);
		}

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="resources">Resources to update.</param>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		protected override void SetResources(int slot, int count, D3D.ShaderResourceView[] resources)
		{
#if DEBUG
			if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				throw new InvalidOperationException("Cannot set resources on a SM2_a_b device.");
#endif
			if (count == 1)
				Graphics.Context.VertexShader.SetShaderResource(slot, resources[0]);
			else
				Graphics.Context.VertexShader.SetShaderResources(slot, count, resources);
		}

		/// <summary>
		/// Function to set the texture samplers for a shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="samplers">Samplers to update.</param>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		protected override void SetSamplers(int slot, int count, D3D.SamplerState[] samplers)
		{
#if DEBUG
			if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				throw new InvalidOperationException("Cannot set resources on a SM2_a_b device.");
#endif
			if (count == 1)
				Graphics.Context.VertexShader.SetSampler(slot, samplers[0]);
			else
				Graphics.Context.VertexShader.SetSamplers(slot, count, samplers);
		}

		/// <summary>
		/// Function to set constant buffers for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="buffers">Constant buffers to update.</param>
		protected override void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers)
		{
			if (count == 1)
				Graphics.Context.VertexShader.SetConstantBuffer(slot, buffers[0]);
			else
				Graphics.Context.VertexShader.SetConstantBuffers(slot, count, buffers);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		protected internal GorgonVertexShaderState(GorgonGraphics graphics)
			: base(graphics)
		{
		}
		#endregion
	}
}
