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
// Created: December 30, 2020 8:59:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Functionality for evaluating state changes.
    /// </summary>
    internal class StateEvaluator
    {
		#region Variables.
		// The previous depth/stencil reference value.
		private int _depthStencilReference;
		// The previous blend sample mask.
		private int _blendSampleMask = int.MinValue;
		// The previous blend factor.
		private GorgonColor _blendFactor = GorgonColor.White;

		// The previously assigned pipeline state.
		private GorgonPipelineState _prevPipelineState = new GorgonPipelineState
        {
            PrimitiveType = PrimitiveType.None
        };

		// The previously assigned resource state.
		private readonly D3DState _lastState = new D3DState
		{
			CsReadWriteViews = new GorgonReadWriteViewBindings(),
			PsSamplers = new GorgonSamplerStates(),
			VsSrvs = new GorgonShaderResourceViews(),
			CsConstantBuffers = new GorgonConstantBuffers(),
			CsSamplers = new GorgonSamplerStates(),
			CsSrvs = new GorgonShaderResourceViews(),
			DsConstantBuffers = new GorgonConstantBuffers(),
			DsSamplers = new GorgonSamplerStates(),
			DsSrvs = new GorgonShaderResourceViews(),
			GsConstantBuffers = new GorgonConstantBuffers(),
			GsSamplers = new GorgonSamplerStates(),
			GsSrvs = new GorgonShaderResourceViews(),
			HsConstantBuffers = new GorgonConstantBuffers(),
			HsSamplers = new GorgonSamplerStates(),
			HsSrvs = new GorgonShaderResourceViews(),
			PsConstantBuffers = new GorgonConstantBuffers(),
			PsSrvs = new GorgonShaderResourceViews(),
			ReadWriteViews = new GorgonReadWriteViewBindings(),
			StreamOutBindings = new GorgonStreamOutBindings(),
			VertexBuffers = new GorgonVertexBufferBindings(),
			VsConstantBuffers = new GorgonConstantBuffers(),
			VsSamplers = new GorgonSamplerStates()
		};
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the changed pipeline state from the previous pipeline state applied by a draw call.
		/// </summary>
		/// <param name="newState">The new pipeline state to apply</param>
		/// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
		/// <param name="blendSampleMask">The mask used to define which samples get updated in the active render targets.</param>
		/// <param name="depthStencilReference">The depth/stencil reference value used when performing a depth/stencil test.</param>
		/// <returns>The changed individual states as a combined set of flags.</returns>
		public DrawCallChanges GetPipelineStateChanges(GorgonPipelineState newState, GorgonColor blendFactor, int blendSampleMask, int depthStencilReference)
        {
			DrawCallChanges changes = DrawCallChanges.None;

			changes |= _prevPipelineState.PrimitiveType == newState.PrimitiveType ? DrawCallChanges.None : DrawCallChanges.Topology;
			changes |= _prevPipelineState.D3DRasterState == newState.D3DRasterState ? DrawCallChanges.None : DrawCallChanges.RasterState;
			changes |= _prevPipelineState.D3DBlendState == newState.D3DBlendState ? DrawCallChanges.None : DrawCallChanges.BlendState;
			changes |= _prevPipelineState.D3DDepthStencilState == newState.D3DDepthStencilState ? DrawCallChanges.None : DrawCallChanges.DepthStencilState;
			changes |= _prevPipelineState.VertexShader == newState.VertexShader ? DrawCallChanges.None : DrawCallChanges.VertexShader;
			changes |= _prevPipelineState.PixelShader == newState.PixelShader ? DrawCallChanges.None : DrawCallChanges.PixelShader;
			changes |= _prevPipelineState.GeometryShader == newState.GeometryShader ? DrawCallChanges.None : DrawCallChanges.GeometryShader;
			changes |= _prevPipelineState.HullShader == newState.HullShader ? DrawCallChanges.None : DrawCallChanges.HullShader;
			changes |= _prevPipelineState.DomainShader == newState.DomainShader ? DrawCallChanges.None : DrawCallChanges.DomainShader;

			if (changes != DrawCallChanges.None)
			{
				newState.CopyTo(_prevPipelineState);
			}

			if (!blendFactor.Equals(in _blendFactor))
			{
				_blendFactor = blendFactor;
				changes |= DrawCallChanges.BlendFactor;
			}

			if (blendSampleMask != _blendSampleMask)
			{
				_blendSampleMask = blendSampleMask;
				changes |= DrawCallChanges.BlendSampleMask;
			}

			if (depthStencilReference != _depthStencilReference)
			{
				_depthStencilReference = depthStencilReference;
				changes |= DrawCallChanges.DepthStencilReference;
			}
			
			return changes;
		}
		#endregion

		#region Constructor.
		/// <summary>Initializes a new instance of the <see cref="StateEvaluator" /> class.</summary>
		public StateEvaluator()
		{
		}
		#endregion
	}
}
