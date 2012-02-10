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
		/// A list of constant buffers.
		/// </summary>
		public class ShaderConstantBuffers
			: IList<GorgonConstantBuffer>
		{
			#region Variables.
			private IList<GorgonConstantBuffer> _buffers = null;
			private D3D.CommonShaderStage _d3dShaderStage = null;
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
						_d3dShaderStage.SetConstantBuffer(index, value.D3DBuffer);
					else
						_d3dShaderStage.SetConstantBuffer(index, null);
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
						_d3dBufferArray[i + slot] = buffer.D3DBuffer;
					else
						_d3dBufferArray[i + slot] = null;
				}

				_d3dShaderStage.SetConstantBuffers(slot, count, _d3dBufferArray);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="ShaderConstantBuffers"/> class.
			/// </summary>
			/// <param name="shaderStage">D3D common shader stage</param>
			internal ShaderConstantBuffers(D3D.CommonShaderStage shaderStage)
			{
				_buffers = new GorgonConstantBuffer[D3D.CommonShaderStage.ConstantBufferApiSlotCount];
				_d3dShaderStage = shaderStage;
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
		/// A list of textures.
		/// </summary>
		public class ShaderTextures
			: IList<GorgonTexture2D>
		{
			#region Variables.
			private D3D.CommonShaderStage _d3dShaderStage = null;		// Shader stage.
			private IList<GorgonTexture2D> _textures = null;			// List of textures.
			private D3D.ShaderResourceView[] _views = null;				// Resource views.
			#endregion

			#region Properties.

			#endregion

			#region Methods.
			/// <summary>
			/// Function to unbind a texture.
			/// </summary>
			/// <param name="texture">Texture to bind.</param>
			internal void Unbind(GorgonTexture2D texture)
			{
				for (int i = 0; i < Count; i++)
				{
					if (_textures[i] == texture)
						this[i] = null;
				}
			}

			/// <summary>
			/// Function to return the index of a texture in the list by name.
			/// </summary>
			/// <param name="name">Name of the texture to look up.</param>
			/// <returns>The index of the texture if found, -1 if not.</returns>
			public int IndexOf(string name)
			{
				GorgonDebug.AssertParamString(name, "name");

				for (int i = 0; i < Count; i++)
				{
					if ((_textures[i] != null) && (string.Compare(_textures[i].Name, name, true) == 0))
						return i;
				}

				return -1;
			}

			/// <summary>
			/// Function to return whether the list contains a texture by it's name.
			/// </summary>
			/// <param name="name">Name of the texture to look up.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(string name)
			{
				int index = IndexOf(name);

				return index != -1;
			}

			/// <summary>
			/// Function to set a range of textures at one time.
			/// </summary>
			/// <param name="slot">Texture slot to start at.</param>
			/// <param name="textures">List of textures to set.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="textures"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of texture slots.
			/// <para>-or-</para>
			/// <para>Thrown when the buffers count + the slot is greater than or equal to the number of available texture slots.</para>
			/// </exception>
			public void SetRange(int slot, IEnumerable<GorgonTexture2D> textures)
			{
				int count = 0;

				GorgonDebug.AssertNull<IEnumerable<GorgonTexture2D>>(textures, "textures");
#if DEBUG
				if ((slot < 0) || (slot >= _textures.Count) || ((slot + textures.Count()) >= _textures.Count))
					throw new ArgumentOutOfRangeException("Cannot have more than " + _textures.Count.ToString() + " slots occupied.");
#endif

				count = textures.Count();
				for (int i = 0; i < count; i++)
				{
					var buffer = textures.ElementAtOrDefault(i);

					_textures[i + slot] = buffer;
					if (buffer != null)
						_views[i + slot] = buffer.D3DResourceView;
					else
						_views[i + slot] = null;
				}

				_d3dShaderStage.SetShaderResources(slot, count, _views);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonShaderState&lt;T&gt;.ShaderTextures"/> class.
			/// </summary>
			/// <param name="shaderStage">D3D common shader stage.</param>
			/// <param name="views">Resource view pool.</param>
			internal ShaderTextures(D3D.CommonShaderStage shaderStage, D3D.ShaderResourceView[] views)
			{
				_d3dShaderStage = shaderStage;
				_textures = new GorgonTexture2D[D3D.CommonShaderStage.InputResourceSlotCount];
				_views = views;
			}
			#endregion

			#region IList<GorgonTexture2D> Members
			#region Properties.
			/// <summary>
			/// Property to set or return the bound texture.
			/// </summary>
			public GorgonTexture2D this[int index]
			{
				get
				{

					return _textures[index];
				}
				set
				{
					_textures[index] = value;
					if (value != null)
						_d3dShaderStage.SetShaderResource(index, value.D3DResourceView);
					else
						_d3dShaderStage.SetShaderResource(index, null);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to return the index of the specified texture.
			/// </summary>
			/// <param name="item">The texture to find the index of.</param>
			/// <returns>The index if found, or -1 if not.</returns>
			public int IndexOf(GorgonTexture2D item)
			{
				GorgonDebug.AssertNull<GorgonTexture2D>(item, "item");

				return _textures.IndexOf(item);
			}

			/// <summary>
			/// Inserts the specified index.
			/// </summary>
			/// <param name="index">The index.</param>
			/// <param name="item">The item.</param>
			void IList<GorgonTexture2D>.Insert(int index, GorgonTexture2D item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes at.
			/// </summary>
			/// <param name="index">The index.</param>
			void IList<GorgonTexture2D>.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region ICollection<GorgonTexture2D> Members
			#region Properties.
			/// <summary>
			/// Property to return the number of texture slots.
			/// </summary>
			public int Count
			{
				get
				{
					return _textures.Count;
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
			void ICollection<GorgonTexture2D>.Add(GorgonTexture2D item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Clears this instance.
			/// </summary>
			void ICollection<GorgonTexture2D>.Clear()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Function to return whether the list contains the specified texture.
			/// </summary>
			/// <param name="item">Texture to find.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(GorgonTexture2D item)
			{
				return _textures.Contains(item);
			}

			/// <summary>
			/// Function to copy the textures to an array.
			/// </summary>
			/// <param name="array">Array to copy into.</param>
			/// <param name="arrayIndex">Index in the array to start writing at.</param>
			public void CopyTo(GorgonTexture2D[] array, int arrayIndex)
			{
				_textures.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Removes the specified item.
			/// </summary>
			/// <param name="item">The item.</param>
			/// <returns></returns>
			bool ICollection<GorgonTexture2D>.Remove(GorgonTexture2D item)
			{
				throw new NotImplementedException();
			}
			#endregion
			#endregion

			#region IEnumerable<GorgonTexture2D> Members
			/// <summary>
			/// Function to return an enumerator for the list.
			/// </summary>
			/// <returns>The enumerator for the list.</returns>
			public IEnumerator<GorgonTexture2D> GetEnumerator()
			{
				foreach (var item in this)
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
				throw new NotImplementedException();
			}
			#endregion
		}
		#endregion

		#region Variables.
		private T _current = null;									// Current shader.
		private D3D.ShaderResourceView[] _views = null;				// Resource views.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		public T Current
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
		public ShaderConstantBuffers ConstantBuffers
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the sampler states.
		/// </summary>
		public GorgonTextureSamplerState Samplers
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of textures for the shaders.
		/// </summary>
		public ShaderTextures Textures
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
		/// Function to clean up.
		/// </summary>
		internal void Dispose()
		{
			if (Samplers != null)
				((IDisposable)Samplers).Dispose();

			Samplers = null;

			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderState&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="shaderStage">Direct 3D shader stage.</param>
		protected GorgonShaderState(GorgonGraphics graphics, D3D.CommonShaderStage shaderStage)
		{
			_views = new D3D.ShaderResourceView[D3D.CommonShaderStage.InputResourceSlotCount];
			Graphics = graphics;			
			ConstantBuffers = new ShaderConstantBuffers(shaderStage);			
			Samplers = new GorgonTextureSamplerState(Graphics, shaderStage);
			Textures = new ShaderTextures(shaderStage, _views);
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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPixelShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonPixelShaderState(GorgonGraphics graphics)
			: base(graphics, graphics.Context.PixelShader)
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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonVertexShaderState(GorgonGraphics graphics)
			: base(graphics, graphics.Context.VertexShader)
		{
		}
		#endregion
	}
}
