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
using System.Text;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

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
		/// <remarks>This is used to control how textures are displayed.</remarks>
		public class TextureSamplerState
			: GorgonStateCache<GorgonTextureSamplerStates>, IList<GorgonTextureSamplerStates>
		{
			#region Variables.
			private GorgonShaderState<T> _shader = null;							// Shader that owns the state.
			private IList<GorgonTextureSamplerStates> _textureStates = null;		// Sampler states.
			private D3D.SamplerState[] _states = null;								// D3D Sampler states.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to retrieve the cached sampler state or return a new one.
			/// </summary>
			/// <param name="state">State to look up or create.</param>
			/// <returns>The cached/new state.</returns>
			private D3D.SamplerState GetState(GorgonTextureSamplerStates state)
			{
				D3D.SamplerState result = null;

				result = GetItem(state) as D3D.SamplerState;
				if (result == null)
				{
					result = Convert(state);
					SetItem(state, result);
				}

				return result;
			}

			/// <summary>
			/// Function to convert this blend state into a Direct3D blend state.
			/// </summary>
			/// <param name="states">States being converted.</param>
			/// <returns>The Direct3D blend state.</returns>
			private D3D.SamplerState Convert(GorgonTextureSamplerStates states)
			{
				D3D.SamplerStateDescription desc = new D3D.SamplerStateDescription();

				desc.AddressU = (D3D.TextureAddressMode)states.HorizontalAddressing;
				desc.AddressV = (D3D.TextureAddressMode)states.VerticalAddressing;
				desc.AddressW = (D3D.TextureAddressMode)states.DepthAddressing;
				desc.BorderColor = new SharpDX.Color4(states.BorderColor.Red, states.BorderColor.Green, states.BorderColor.Blue, states.BorderColor.Alpha);
				desc.ComparisonFunction = (D3D.Comparison)states.ComparisonFunction;
				desc.MaximumAnisotropy = states.MaxAnisotropy;
				desc.MaximumLod = states.MaxLOD;
				desc.MinimumLod = states.MinLOD;
				desc.MipLodBias = states.MipLODBias;


				if (states.TextureFilter == TextureFilter.Anisotropic)
					desc.Filter = D3D.Filter.Anisotropic;
				if (states.TextureFilter == TextureFilter.CompareAnisotropic)
					desc.Filter = D3D.Filter.ComparisonAnisotropic;

				// Sort out filter states.
				// Check comparison states.
				if ((states.TextureFilter & TextureFilter.Comparison) == TextureFilter.Comparison)
				{
					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinMagMipLinear;
					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinMagMipPoint;

					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinMagLinearMipPoint;
					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinLinearMagMipPoint;
					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinLinearMagPointMipLinear;

					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinPointMagMipLinear;
					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.ComparisonMinMagPointMipLinear;
					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.ComparisonMinPointMagLinearMipPoint;
				}
				else
				{
					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinMagMipLinear;
					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinMagMipPoint;

					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinMagLinearMipPoint;
					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinLinearMagMipPoint;
					if (((states.TextureFilter & TextureFilter.MinLinear) == TextureFilter.MinLinear) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinLinearMagPointMipLinear;

					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinPointMagMipLinear;
					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagPoint) == TextureFilter.MagPoint) && ((states.TextureFilter & TextureFilter.MipLinear) == TextureFilter.MipLinear))
						desc.Filter = D3D.Filter.MinMagPointMipLinear;
					if (((states.TextureFilter & TextureFilter.MinPoint) == TextureFilter.MinPoint) && ((states.TextureFilter & TextureFilter.MagLinear) == TextureFilter.MagLinear) && ((states.TextureFilter & TextureFilter.MipPoint) == TextureFilter.MipPoint))
						desc.Filter = D3D.Filter.MinPointMagLinearMipPoint;
				}

				D3D.SamplerState state = new D3D.SamplerState(Graphics.D3DDevice, desc);
				state.DebugName = "Gorgon Sampler State #" + StateCacheCount.ToString();

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
					var state = states.ElementAtOrDefault(i);
					_textureStates[i + slot] = state;
					_states[i] = GetState(state);
				}

				_shader.SetSamplers(slot, count, _states);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonBlendRenderState"/> class.
			/// </summary>
			/// <param name="shaderState">Shader that owns the state.</param>
			internal TextureSamplerState(GorgonShaderState<T> shaderState)
				: base(shaderState.Graphics, 4096, Int32.MaxValue)
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

				_textureStates = new GorgonTextureSamplerStates[count];
				_states = new D3D.SamplerState[_textureStates.Count];
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
					return _textureStates[index];
				}
				set
				{
					if (_textureStates[index] != value)
					{
						_textureStates[index] = value;
						_states[0] = GetState(value);
						_shader.SetSamplers(index, 1, _states);
					}
					else
						Touch(value);

					// Drop expired items if we are at or over our limit.
					if (StateCacheCount >= CacheLimit)
						EvictCache();
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
				return _textureStates.IndexOf(item);
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
					return _textureStates.Count;
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
			public new bool Contains(GorgonTextureSamplerStates item)
			{
				return _textureStates.Contains(item);
			}

			/// <summary>
			/// Function to copy the list of bound sampler states to an array.
			/// </summary>
			/// <param name="array">The array to copy into.</param>
			/// <param name="arrayIndex">Index of the array to start writing at.</param>
			public void CopyTo(GorgonTextureSamplerStates[] array, int arrayIndex)
			{
				_textureStates.CopyTo(array, arrayIndex);
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
				foreach (var item in _textureStates)
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
				return GetEnumerator();
			}
			#endregion
		}

		/// <summary>
		/// A list of constant buffers.
		/// </summary>
		public class ShaderConstantBuffers
			: IList<GorgonConstantBuffer>
		{
			#region Variables.
			private IList<GorgonConstantBuffer> _buffers = null;
			private GorgonShaderState<T> _shader = null;
			private D3D.Buffer[] _d3dBufferArray = null;
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
					_buffers[index] = value;
					if (value != null)
						_d3dBufferArray[0] = value.D3DBuffer;
					else
						_d3dBufferArray[0] = null;

					_shader.SetConstantBuffers(index, 1, _d3dBufferArray);
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
						_d3dBufferArray[i] = buffer.D3DBuffer;
					else
						_d3dBufferArray[i] = null;
				}

				_shader.SetConstantBuffers(slot, count, _d3dBufferArray);
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
				_d3dBufferArray = new D3D.Buffer[_buffers.Count];
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
		/// A list of shader resources.
		/// </summary>
		/// <remarks>A shader resource can be a data structure like a buffer, or a texture.</remarks>
		public class ShaderResources
			: IList<GorgonResource>
		{
			#region Variables.
			private GorgonShaderState<T> _shader = null;				// Shader that owns this interface.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to unbind a shader resource.
			/// </summary>
			/// <param name="resource">Resource to bind.</param>
			internal void Unbind(GorgonResource resource)
			{
				for (int i = 0; i < Count; i++)
				{
					if (_shader._resources[i] == resource)
						this[i] = null;
				}
			}

			/// <summary>
			/// Function to re-seat a resource after it's been altered.
			/// </summary>
			/// <param name="resource">Resource to re-seat.</param>
			internal void ReSeat(GorgonResource resource)
			{
				int index = IndexOf(resource);

				if (index > -1)
				{
					this[index] = null;
					this[index] = resource;
				}
			}

			/// <summary>
			/// Function to set a range of resources at one time.
			/// </summary>
			/// <param name="slot">Resource slot to start at.</param>
			/// <param name="resources">A list of resources to set.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resources"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of resource slots.
			/// <para>-or-</para>
			/// <para>Thrown when the buffers count + the slot is greater than or equal to the number of available resource slots.</para>
			/// </exception>
			public void SetRange(int slot, IEnumerable<GorgonResource> resources)
			{
				int count = 0;

				GorgonDebug.AssertNull<IEnumerable<GorgonResource>>(resources, "resources");
#if DEBUG
				if ((slot < 0) || (slot >= _shader._resources.Count) || ((slot + resources.Count()) >= _shader._resources.Count))
					throw new ArgumentOutOfRangeException("Cannot have more than " + _shader._resources.Count.ToString() + " slots occupied.");
#endif

				count = resources.Count();
				for (int i = 0; i < count; i++)
				{
					var buffer = resources.ElementAtOrDefault(i);

					_shader._resources[i + slot] = buffer;
					if ((buffer != null) && (buffer.View != null))
						_shader._views[i] = _shader._resources[i + slot].View.D3DResourceView;
					else
						_shader._views[i] = null;
				}

				_shader.SetResources(slot, count);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonShaderState&lt;T&gt;.ShaderResources"/> class.
			/// </summary>
			/// <param name="shader">Shader state that owns this interface.</param>
			internal ShaderResources(GorgonShaderState<T> shader)
			{
				_shader = shader;
			}
			#endregion

			#region IList<GorgonResource> Members
			#region Properties.
			/// <summary>
			/// Property to set or return the bound shader resource.
			/// </summary>
			public GorgonResource this[int index]
			{
				get
				{
					return _shader._resources[index] as GorgonResource;
				}
				set
				{
					_shader._resources[index] = value;
					if ((value != null) && (value.View != null))
						_shader._views[0] = _shader._resources[index].View.D3DResourceView;
					else
						_shader._views[0] = null;

					_shader.SetResources(index, 1);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to return the index of the specified shader resource.
			/// </summary>
			/// <param name="item">The shader resource to find the index of.</param>
			/// <returns>The index if found, or -1 if not.</returns>
			public int IndexOf(GorgonResource item)
			{
				GorgonDebug.AssertNull<GorgonResource>(item, "item");

				return _shader._resources.IndexOf(item);
			}

			/// <summary>
			/// Inserts the specified index.
			/// </summary>
			/// <param name="index">The index.</param>
			/// <param name="item">The item.</param>
			void IList<GorgonResource>.Insert(int index, GorgonResource item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes at.
			/// </summary>
			/// <param name="index">The index.</param>
			void IList<GorgonResource>.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region ICollection<GorgonResource> Members
			#region Properties.
			/// <summary>
			/// Property to return the number of texture slots.
			/// </summary>
			public int Count
			{
				get
				{
					return _shader._resources.Count;
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
			void ICollection<GorgonResource>.Add(GorgonResource item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Clears this instance.
			/// </summary>
			void ICollection<GorgonResource>.Clear()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Function to return whether the list contains the specified texture.
			/// </summary>
			/// <param name="item">Texture to find.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonResource item)
			{
				return _shader._resources.Contains(item);
			}

			/// <summary>
			/// Function to copy the textures to an array.
			/// </summary>
			/// <param name="array">Array to copy into.</param>
			/// <param name="arrayIndex">Index in the array to start writing at.</param>
			public void CopyTo(GorgonResource[] array, int arrayIndex)
			{
				var resources = from shaderView in _shader._resources							   
							   where shaderView != null
							   select shaderView;

				foreach (var item in resources)
				{
					array[arrayIndex] = item;
					arrayIndex++;
				}
			}

			/// <summary>
			/// Removes the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			bool ICollection<GorgonResource>.Remove(GorgonResource item)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region IEnumerable<GorgonResource> Members
			/// <summary>
			/// Function to return an enumerator for the list.
			/// </summary>
			/// <returns>The enumerator for the list.</returns>
			public IEnumerator<GorgonResource> GetEnumerator()
			{
				foreach (var resource in _shader._resources)
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
		private T _current = null;									// Current shader.
		private D3D.ShaderResourceView[] _views = null;				// Resource views.
		private IList<GorgonResource> _resources = null;			// List of resources.
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
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of resources for the shaders.
		/// </summary>
		/// <remarks>
		/// A resource may be a raw buffer (with shader binding enabled), <see cref="GorgonLibrary.Graphics.GorgonStructuredBuffer">structured buffer</see>, append/consume buffer, or a <see cref="GorgonLibrary.Graphics.GorgonTexture">texture</see>.
		/// <para>On a SM2_a_b device, and while using a Vertex Shader, setting a texture will raise an exception.</para></remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the current video device is a SM2_a_b device.</exception>
		public virtual ShaderResources Resources
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
		internal abstract void SetSamplers(int slot, int count, D3D.SamplerState[] samplers);		

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count">Number of resources to update.</param>
		internal void SetResources(int slot, int count)
		{			
			SetResources(slot, count, _views);
		}

		/// <summary>
		/// Function to set constant buffers for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count">Number of constant buffers to update.</param>
		/// <param name="buffers">Constant buffers to update.</param>
		internal abstract void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers);

		/// <summary>
		/// Function to clean up.
		/// </summary>
		internal void Dispose()
		{
			if (TextureSamplers != null)
				((IDisposable)TextureSamplers).Dispose();

			TextureSamplers = null;

			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderState&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		protected GorgonShaderState(GorgonGraphics graphics)
		{
			int count = D3D.CommonShaderStage.InputResourceSlotCount;

			if (graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
			{
				if (this is GorgonVertexShaderState)
					count = 0;
				else
					count = 8;
			}

			_views = new D3D.ShaderResourceView[count];
			_resources = new GorgonResource[_views.Length];

			Graphics = graphics;			
			ConstantBuffers = new ShaderConstantBuffers(this);
			TextureSamplers = new TextureSamplerState(this);
			Resources = new ShaderResources(this);
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
		internal override void SetSamplers(int slot, int count, D3D.SamplerState[] samplers)
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
		internal override void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers)
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
			for (int i = 0; i < TextureSamplers.Count; i++)
				TextureSamplers[i] = GorgonTextureSamplerStates.DefaultStates;
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
		internal override void SetSamplers(int slot, int count, D3D.SamplerState[] samplers)
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
		internal override void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers)
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
			for (int i = 0; i < TextureSamplers.Count; i++)
				TextureSamplers[i] = GorgonTextureSamplerStates.DefaultStates;
		}
		#endregion
	}
}
