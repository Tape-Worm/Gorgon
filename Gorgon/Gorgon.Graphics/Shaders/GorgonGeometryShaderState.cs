#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, June 10, 2013 8:56:42 PM
// 
#endregion

using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Geometry shader states.
	/// </summary>
	public class GorgonGeometryShaderState
        : GorgonShaderState<GorgonGeometryShader>
	{
		#region Variables.
		private GorgonOutputBufferBinding[] _bindings;				// Output buffer bindings
		private D3D.StreamOutputBufferBinding[] _D3Dbindings;		// Direct 3D buffer bindings.
		#endregion

		#region Methods.
		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		protected override void SetCurrent()
        {
	        Graphics.Context.GeometryShader.Set(Current == null ? null : Current.D3DShader);
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
		    {
		        Graphics.Context.GeometryShader.SetShaderResource(slot, resources[0]);
		    }
		    else
		    {
		        Graphics.Context.GeometryShader.SetShaderResources(slot, count, resources);
		    }
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
		    {
		        Graphics.Context.GeometryShader.SetSampler(slot, samplers[0]);
		    }
		    else
		    {
		        Graphics.Context.GeometryShader.SetSamplers(slot, count, samplers);
		    }
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
		    {
		        Graphics.Context.GeometryShader.SetConstantBuffer(slot, buffers[0]);
		    }
		    else
		    {
		        Graphics.Context.GeometryShader.SetConstantBuffers(slot, count, buffers);
		    }
		}

        /// <summary>
        /// Function to reset the geometry shader state.
        /// </summary>
        internal override void Reset()
        {
            base.Reset();

            _bindings = null;
            _D3Dbindings = null;

            Graphics.Context.StreamOutput.SetTargets(_D3Dbindings);
        }

		/// <summary>
		/// Function to retrieve the list of output buffers bound to the pipeline.
		/// </summary>
		/// <returns>An array of output buffers.</returns>
		public GorgonOutputBufferBinding[] GetOutputBuffers()
		{
			var result = new GorgonOutputBufferBinding[_bindings == null ? 0 : _bindings.Length];

			if ((_bindings != null) && (_bindings.Length != 0))
			{
				_bindings.CopyTo(result, 0);
			}

			return result;
		}

		/// <summary>
		/// Function to return a stream output buffer bind to the pipeline.
		/// </summary>
		/// <param name="index">Index of the buffer.</param>
		/// <returns>The output buffer if bound, NULL (<i>Nothing</i> in VB.Net) if not.</returns>
		public GorgonOutputBufferBinding? GetOutputBuffer(int index)
		{
			if ((_bindings == null) || (index < 0) || (index >= _bindings.Length))
			{
				return null;
			}

			return _bindings[index];
		}

		/// <summary>
		/// Function to set a single stream output buffer.
		/// </summary>
		/// <param name="buffer">Buffer to set.</param>
		/// <remarks>Stream output buffers are only used by the <see cref="Gorgon.Graphics.GorgonOutputGeometryShader">GorgonOutputGeometryShader</see> object.  If the currently bound shader 
		/// is not an output geometry shader, then any values set here will be ignored.</remarks>
		public void SetStreamOutputBuffer(GorgonOutputBufferBinding buffer)
		{
			if ((_bindings != null) && (GorgonOutputBufferBinding.Equals(ref buffer, ref _bindings[0])))
			{
				return;
			}

			if ((_bindings == null) || (_bindings.Length != 1))
			{
				_bindings = null;
				_D3Dbindings = null;
					 
				if (!buffer.Equals(GorgonOutputBufferBinding.Empty))
				{
					_bindings = new GorgonOutputBufferBinding[1];
					_D3Dbindings = new D3D.StreamOutputBufferBinding[1];
				}
			}

			if ((_bindings != null) && (_D3Dbindings != null))
			{
				_bindings[0] = buffer;
				_D3Dbindings[0] = buffer.Convert();

				Graphics.Context.StreamOutput.SetTargets(_D3Dbindings);
			}
			else
			{
				Graphics.Context.StreamOutput.SetTargets(null);				
			}
		}

		/// <summary>
		/// Function to set a list of stream output buffers at the same time.
		/// </summary>
		/// <param name="buffers">Buffers to bind.</param>
		/// <remarks>Stream output buffers are only used by the <see cref="Gorgon.Graphics.GorgonOutputGeometryShader">GorgonOutputGeometryShader</see> object.  If the currently bound shader 
		/// is not an output geometry shader, then any values set here will be ignored.</remarks>
		public void SetStreamOutputBuffers(GorgonOutputBufferBinding[] buffers)
		{
			bool hasChanged = false;

			// If we didn't pass any buffers, then unbind everything.
			if ((buffers == null) || (buffers.Length == 0))
			{
			    if (_bindings == null)
			    {
			        return;
			    }

			    Graphics.Context.StreamOutput.SetTargets(null);
			    _bindings = null;
			    _D3Dbindings = null;

			    return;
			}

			if ((_bindings == null) || (_bindings.Length != buffers.Length))
			{
				_bindings = new GorgonOutputBufferBinding[buffers.Length];
				_D3Dbindings = new D3D.StreamOutputBufferBinding[buffers.Length];
				hasChanged = true;
			}

			// Copy.
			for (int i = 0; i < _bindings.Length; i++)
			{
				var buffer = buffers[i];

				if (GorgonOutputBufferBinding.Equals(ref _bindings[i], ref buffer))
				{
					continue;
				}

				hasChanged = true;
				_bindings[i] = buffer;
				_D3Dbindings[i] = buffer.Convert();
			}

			if (hasChanged)
			{
				Graphics.Context.StreamOutput.SetTargets(_D3Dbindings);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonGeometryShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
        protected internal GorgonGeometryShaderState(GorgonGraphics graphics)
			: base(graphics)
		{
		}
		#endregion
	}
}