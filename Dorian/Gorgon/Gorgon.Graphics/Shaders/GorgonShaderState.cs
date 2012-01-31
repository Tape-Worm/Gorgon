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
		{
			#region Variables.
			private GorgonConstantBuffer[] _buffers = null;
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
					return _buffers.Length;
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
				for (int i = 0; i < _buffers.Length - 1; i++)
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
				if ((slot < 0) || (slot >= _buffers.Length) || ((slot + buffers.Count()) >= _buffers.Length))
					throw new ArgumentOutOfRangeException("Cannot have more than " + _buffers.Length.ToString() + " slots occupied.");
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
				_d3dBufferArray = new D3D.Buffer[_buffers.Length];
			}
			#endregion
		}
		#endregion

		#region Variables.
		private T _current = null;			// Current shader.
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
			Graphics = graphics;			
			ConstantBuffers = new ShaderConstantBuffers(shaderStage);
			Samplers = new GorgonTextureSamplerState(Graphics, shaderStage);
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
