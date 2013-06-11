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

using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Pixel shader states.
	/// </summary>
	public class GorgonPixelShaderState
		: GorgonShaderState<GorgonPixelShader>
    {
        #region Variables.
        private GorgonUnorderedAccessView[] _unorderedViews;
        #endregion

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
		protected override void SetResources(int slot, int count, SharpDX.Direct3D11.ShaderResourceView[] resources)
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
		protected override void SetSamplers(int slot, int count, SharpDX.Direct3D11.SamplerState[] samplers)
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
		protected override void SetConstantBuffers(int slot, int count, SharpDX.Direct3D11.Buffer[] buffers)
		{
			if (count == 1)
				Graphics.Context.PixelShader.SetConstantBuffer(slot, buffers[0]);
			else
				Graphics.Context.PixelShader.SetConstantBuffers(slot, count, buffers);
		}

        /// <summary>
        /// Function to set a single unordered access view for a pixel shader.
        /// </summary>
        /// <param name="slot">The slot to place the view into.</param>
        /// <param name="view">Unordered access view to </param>
        /// <param name="initialCount">[Optional] The initial count for the UAV if it's set up as a AppendConsume or Counter view.</param>
        /// <remarks>This will bind an <see cref="GorgonLibrary.Graphics.GorgonUnorderedAccessView">unordered access view</see> (UAV) to the pixel shader.
        /// <para>UAVs for pixel shaders use the same slots as render targets when being written to.  Use the <paramref name="slot"/> parameter to indicate the slot to be used -after- the bound render targets.  
        /// Note that all render targets after the slot will be unbound.</para>
        /// <para>If the resource bound to the view is currently bound to the pipeline with another view type (e.g. <see cref="GorgonLibrary.Graphics.GorgonShaderView">shader view</see>, compute shader UAV, stream output), 
        /// then the resource will be unbound from those other views since cannot be bound to another part of the pipeline while binding the UAV to the pixel shader.</para>
        /// <para>Pass -1 to the <paramref name="initialCount"/> value to keep the current count for a AppendConsume/Counter view.</para>
        /// <para>UAVs require a video device with a feature level of SM5 or better.</para>
        /// </remarks> 
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0, or greater than 7.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the current video device does not have a feature level of SM5 or better.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="view"/> could not be bound to the pixel shader.</para>
        /// </exception>
        public void SetUnorderedAccessView(int slot, GorgonUnorderedAccessView view, int initialCount = -1)
        {
            GorgonDebug.AssertParamRange(slot, 0, 8, "slot");

#if DEBUG
            if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                throw new GorgonException(GorgonResult.CannotBind, string.Format(Properties.Resources.GORGFX_REQUIRES_SM, "SM5"));
            }
#endif
            // Unbind from the shader resource views.
	        if (view != null)
	        {
		        Resources.Unbind(view.Resource);
		        // TODO: Provide unbinding from Compute Shader UAVs, and stream outputs.
		        // Graphics.Shaders.ComputeShader.Unbind(view.Resource);
		        // Graphics.StreamOutput.Unbind(view.Resource);
	        }

	        // Unbind any render targets.
            Graphics.Output.UnbindSlots(slot);

            _unorderedViews[slot] = view;

	        Graphics.Context.OutputMerger.SetUnorderedAccessView(slot, view != null ? view.D3DView : null);
        }

		/// <summary>
		/// Function to set a single unordered access view for a pixel shader.
		/// </summary>
		/// <param name="slot">The slot to place the view into.</param>
		/// <param name="view">Unordered access view to </param>
		/// <param name="initialCount">[Optional] The initial count for the UAV if it's set up as a AppendConsume or Counter view.</param>
		/// <remarks>This will bind an <see cref="GorgonLibrary.Graphics.GorgonUnorderedAccessView">unordered access view</see> (UAV) to the pixel shader.
		/// <para>UAVs for pixel shaders use the same slots as render targets when being written to.  Use the <paramref name="slot"/> parameter to indicate the slot to be used -after- the bound render targets.  
		/// Note that all render targets after the slot will be unbound.</para>
		/// <para>If the resource bound to the view is currently bound to the pipeline with another view type (e.g. <see cref="GorgonLibrary.Graphics.GorgonShaderView">shader view</see>, compute shader UAV, stream output), 
		/// then the resource will be unbound from those other views since cannot be bound to another part of the pipeline while binding the UAV to the pixel shader.</para>
		/// <para>Pass -1 to the <paramref name="initialCount"/> value to keep the current count for a AppendConsume/Counter view.</para>
		/// <para>UAVs require a video device with a feature level of SM5 or better.</para>
		/// </remarks> 
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0, or greater than 7.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the current video device does not have a feature level of SM5 or better.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="view"/> could not be bound to the pixel shader.</para>
		/// </exception>
		public void SetUnorderedAccessViewTest(GorgonRenderTargetView rt, int slot, GorgonUnorderedAccessView view, int initialCount = -1)
		{
			GorgonDebug.AssertParamRange(slot, 0, 8, "slot");

#if DEBUG
			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Properties.Resources.GORGFX_REQUIRES_SM, "SM5"));
			}
#endif
			// Unbind from the shader resource views.
			if (view != null)
			{
				Resources.Unbind(view.Resource);
				// TODO: Provide unbinding from Compute Shader UAVs, and stream outputs.
				// Graphics.Shaders.ComputeShader.Unbind(view.Resource);
				// Graphics.StreamOutput.Unbind(view.Resource);
			}

			// Unbind any render targets.
			Graphics.Output.UnbindSlots(slot);

			_unorderedViews[slot] = view;

			Graphics.Context.OutputMerger.SetTargets((D3D.DepthStencilView)null, rt.D3DView, slot, new D3D.UnorderedAccessView[] {view != null ? view.D3DView : null});
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
		    _unorderedViews = new GorgonUnorderedAccessView[8];
		}
		#endregion
	}
}