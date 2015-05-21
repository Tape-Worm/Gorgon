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
// Created: Monday, June 17, 2013 8:26:51 PM
// 
#endregion

using SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Domain shader states.
	/// </summary>
	public class GorgonDomainShaderState
        : GorgonShaderState<GorgonDomainShader>
    {
        #region Methods.
        /// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		protected override void SetCurrent()
        {
	        Graphics.Context.DomainShader.Set(Current == null ? null : Current.D3DShader);
        }

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="resources">Resources to update.</param>
		protected override void SetResources(int slot, int count, ShaderResourceView[] resources)
		{
            if (count == 1)
		    {
		        Graphics.Context.DomainShader.SetShaderResource(slot, resources[0]);
		    }
		    else
		    {
		        Graphics.Context.DomainShader.SetShaderResources(slot, count, resources);
		    }
		}

		/// <summary>
		/// Function to set the texture samplers for a shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="samplers">Samplers to update.</param>
		protected override void SetSamplers(int slot, int count, SamplerState[] samplers)
		{
            if (count == 1)
		    {
		        Graphics.Context.DomainShader.SetSampler(slot, samplers[0]);
		    }
		    else
		    {
		        Graphics.Context.DomainShader.SetSamplers(slot, count, samplers);
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
		        Graphics.Context.DomainShader.SetConstantBuffer(slot, buffers[0]);
		    }
		    else
		    {
		        Graphics.Context.DomainShader.SetConstantBuffers(slot, count, buffers);
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonDomainShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
        protected internal GorgonDomainShaderState(GorgonGraphics graphics)
			: base(graphics)
		{
		}
		#endregion
	}
}