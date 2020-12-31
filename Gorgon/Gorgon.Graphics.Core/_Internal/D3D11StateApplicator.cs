#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 30, 2020 10:45:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Functionality to apply Direct3D 11 states and resources.
    /// </summary>
    internal class D3D11StateApplicator
    {
        #region Variables.
        // The device context used for applying state and resource information.
        private readonly D3D11.DeviceContext4 _deviceContext;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply a pipeline state to the Direct3D immediate context.
		/// </summary>
		/// <param name="state">The state to apply.</param>
		/// <param name="changes">The state changes to apply.</param>
		/// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
		/// <param name="blendSampleMask">The mask used to define which samples get updated in the active render targets.</param>
		/// <param name="depthStencilRef">The depth/stencil reference value used when performing a depth/stencil test.</param>
		public void ApplyPipelineState(GorgonPipelineState state, DrawCallChanges changes, GorgonColor blendFactor, int blendSampleMask, int depthStencilRef)
        {
            if (changes == DrawCallChanges.None)
            {
                return;
            }

			if ((changes & DrawCallChanges.BlendFactor) == DrawCallChanges.BlendFactor)
			{
				_deviceContext.OutputMerger.BlendFactor = blendFactor.ToRawColor4();
			}

			if ((changes & DrawCallChanges.BlendSampleMask) == DrawCallChanges.BlendSampleMask)
			{
				_deviceContext.OutputMerger.BlendSampleMask = blendSampleMask;
			}

			if ((changes & DrawCallChanges.DepthStencilReference) == DrawCallChanges.DepthStencilReference)
			{
				_deviceContext.OutputMerger.DepthStencilReference = depthStencilRef;
			}

			if ((changes & DrawCallChanges.Topology) == DrawCallChanges.Topology)
			{
				_deviceContext.InputAssembler.PrimitiveTopology = (D3D.PrimitiveTopology)state.PrimitiveType;
			}

			if ((changes & DrawCallChanges.RasterState) == DrawCallChanges.RasterState)
			{
				_deviceContext.Rasterizer.State = state.D3DRasterState;
			}

			if ((changes & DrawCallChanges.DepthStencilState) == DrawCallChanges.DepthStencilState)
			{
				_deviceContext.OutputMerger.DepthStencilState = state.D3DDepthStencilState;
			}

			if ((changes & DrawCallChanges.BlendState) == DrawCallChanges.BlendState)
			{
				_deviceContext.OutputMerger.BlendState = state.D3DBlendState;
			}

			if ((changes & DrawCallChanges.PixelShader) == DrawCallChanges.PixelShader)
			{
				_deviceContext.PixelShader.Set(state.PixelShader?.NativeShader);
			}

			if ((changes & DrawCallChanges.VertexShader) == DrawCallChanges.VertexShader)
			{
				_deviceContext.VertexShader.Set(state.VertexShader?.NativeShader);
			}

			if ((changes & DrawCallChanges.GeometryShader) == DrawCallChanges.GeometryShader)
			{
				_deviceContext.GeometryShader.Set(state.GeometryShader?.NativeShader);
			}

			if ((changes & DrawCallChanges.DomainShader) == DrawCallChanges.DomainShader)
			{
				_deviceContext.DomainShader.Set(state.DomainShader?.NativeShader);
			}

			if ((changes & DrawCallChanges.HullShader) == DrawCallChanges.HullShader)
			{
				_deviceContext.HullShader.Set(state.HullShader?.NativeShader);
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>Initializes a new instance of the <see cref="D3D11StateApplicator" /> class.</summary>
		/// <param name="deviceContext">The device context.</param>
		public D3D11StateApplicator(D3D11.DeviceContext4 deviceContext) => _deviceContext = deviceContext;
        #endregion
    }
}
