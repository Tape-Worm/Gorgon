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
// Created: Monday, June 10, 2013 8:56:47 PM
// 
#endregion

using Gorgon.Core;
using SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Vertex shader states.
	/// </summary>
	public class GorgonVertexShaderState
		: GorgonShaderState<GorgonVertexShader>
	{
		#region Methods.
		/// <summary>
		/// Function to reset the internal shader states.
		/// </summary>
		internal override void Reset()
		{
			TextureSamplers.Reset();
			ConstantBuffers.Reset();
			Resources.Reset();
		}

		/// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		protected override void SetCurrent()
		{
			Graphics.VideoDevice.D3DDeviceContext().VertexShader.Set(Current == null ? null : Current.D3DShader);
		}

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="resources">Resources to update.</param>
		/// <exception cref="GorgonException">Thrown when the current video device is a SM2_a_b device.</exception>
		protected override void SetResources(int slot, int count, ShaderResourceView[] resources)
		{
		    if (count == 1)
		    {
		        Graphics.VideoDevice.D3DDeviceContext().VertexShader.SetShaderResource(slot, resources[0]);
		    }
		    else
		    {
		        Graphics.VideoDevice.D3DDeviceContext().VertexShader.SetShaderResources(slot, count, resources);
		    }
		}

		/// <summary>
		/// Function to set the texture samplers for a shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="samplers">Samplers to update.</param>
        /// <exception cref="GorgonException">Thrown when the current video device is a SM2_a_b device.</exception>
		protected override void SetSamplers(int slot, int count, SamplerState[] samplers)
		{
		    if (count == 1)
		    {
		        Graphics.VideoDevice.D3DDeviceContext().VertexShader.SetSampler(slot, samplers[0]);
		    }
		    else
		    {
		        Graphics.VideoDevice.D3DDeviceContext().VertexShader.SetSamplers(slot, count, samplers);
		    }
		}

		/// <summary>
		/// Function to set constant buffers for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="buffers">Constant buffers to update.</param>
		protected override void SetConstantBuffers(int slot, int count, Buffer[] buffers)
		{
		    if (count == 1)
		    {
		        Graphics.VideoDevice.D3DDeviceContext().VertexShader.SetConstantBuffer(slot, buffers[0]);
		    }
		    else
		    {
		        Graphics.VideoDevice.D3DDeviceContext().VertexShader.SetConstantBuffers(slot, count, buffers);
		    }
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