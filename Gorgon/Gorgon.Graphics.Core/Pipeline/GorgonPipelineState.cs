#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 4, 2016 9:16:56 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Flags to indicate what part of the pipeline has been modified within a <see cref="GorgonPipelineState"/>.
	/// </summary>
	[Flags]
	public enum PipelineStateChangeFlags
		: ulong
	{
		/// <summary>
		/// No state has been modified.
		/// </summary>
		None = 0,
		/// <summary>
		/// The vertex shader state has been modified.
		/// </summary>
		VertexShader = 0x1,
		/// <summary>
		/// The pixel shader state has been modified.
		/// </summary>
		PixelShader = 0x2,
		/// <summary>
		/// The rasterizer state was modified.
		/// </summary>
		RasterState = 0x4,
		/// <summary>
		/// The depth/stencil state has been updated.
		/// </summary>
		DepthStencilState = 0x8,
		/// <summary>
		/// The blending state has been updated.
		/// </summary>
		BlendState = 0x10,
		/// <summary>
		/// The blending factor has been updated.
		/// </summary>
		BlendFactor = 0x20,
		/// <summary>
		/// The blending sample mask has been updated.
		/// </summary>
		BlendSampleMask = 0x40,
		/// <summary>
		/// The depth/stencil reference value has been updated.
		/// </summary>
		DepthStencilReference = 0x80,
		/// <summary>
		/// All states have changed.
		/// </summary>
		All = VertexShader | PixelShader | RasterState | DepthStencilState | BlendState | BlendFactor | BlendSampleMask | DepthStencilReference
	}

	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
	{
		#region Variables.
		// Information used to create the pipeline state.
		private readonly GorgonPipelineStateInfo _info;
		// The current blending factor.
		private GorgonColor _blendFactor;
		// The blending sample mask.
		private int _blendSampleMask;
		// The depth/stencil reference value.
		private int _depthStencilReference;
		// Flags for state changes that are mutable on the state object.
		private PipelineStateChangeFlags _mutableStateChanges;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11 raster state.
		/// </summary>
		internal D3D11.RasterizerState1 D3DRasterState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the Direct 3D 11 depth/stencil state.
		/// </summary>
		internal D3D11.DepthStencilState D3DDepthStencilState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the Direct 3D 11 blend state.
		/// </summary>
		internal D3D11.BlendState1 D3DBlendState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the ID of the pipeline state.
		/// </summary>
		/// <remarks>
		/// This is used to store a globally cached version of the pipeline state.
		/// </remarks>
		public int ID
		{
			get;
		}

		/// <summary>
		/// Property to set or return the factor used to modulate the pixel shader, render target or both.
		/// </summary>
		/// <remarks>
		/// To use this value, ensure that the blend state was creating using <c>Factor</c> operation.
		/// </remarks>
		public GorgonColor BlendFactor
		{
			get => _blendFactor;
			set
			{
				if (GorgonColor.Equals(ref _blendFactor, ref value))
				{
					return;
				}

				_blendFactor = value;
				_mutableStateChanges |=  PipelineStateChangeFlags.BlendFactor;
			}
		}

		/// <summary>
		/// Property to set or return the mask used to define which samples get updated in the active render targets.
		/// </summary>
		public int BlendSampleMask
		{
			get => _blendSampleMask;
			set
			{
				if (_blendSampleMask == value)
				{
					return;
				}

				_blendSampleMask = value;
				_mutableStateChanges |= PipelineStateChangeFlags.BlendSampleMask;
			}
		}

		/// <summary>
		/// Property to set or return the depth/stencil reference value used when performing a depth/stencil test.
		/// </summary>
		public int DepthStencilReference
		{
			get => _depthStencilReference;
			set
			{
				if (_depthStencilReference == value)
				{
					return;
				}

				_depthStencilReference = value;
				_mutableStateChanges |= PipelineStateChangeFlags.DepthStencilReference;
			}
		}

		/// <summary>
		/// Property to return the <see cref="IGorgonPipelineStateInfo"/> used to create this object.
		/// </summary>
		public IGorgonPipelineStateInfo Info => _info;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare this pipeline state with another pipeline state.
		/// </summary>
		/// <param name="state">The state to compare.</param>
		/// <returns>The states that have been changed between this state and the other <paramref name="state"/>.</returns>
		public PipelineStateChangeFlags GetChanges(GorgonPipelineState state)
		{
			if (state == null)
			{
				return PipelineStateChangeFlags.All;
			}

			PipelineStateChangeFlags mutableFlags = _mutableStateChanges;
			var pipelineFlags = PipelineStateChangeFlags.None;
			_mutableStateChanges = PipelineStateChangeFlags.None;

			if (_info.PixelShader != state._info.PixelShader)
			{
				pipelineFlags |= PipelineStateChangeFlags.PixelShader;
			}

			if (_info.VertexShader != state._info.VertexShader)
			{
				pipelineFlags |= PipelineStateChangeFlags.VertexShader;
			}

			if (D3DRasterState != state.D3DRasterState)
			{
				pipelineFlags |= PipelineStateChangeFlags.RasterState;
			}

			if (D3DDepthStencilState != state.D3DDepthStencilState)
			{
				pipelineFlags |= PipelineStateChangeFlags.DepthStencilState;
			}

			if (D3DBlendState != state.D3DBlendState)
			{
				pipelineFlags |= PipelineStateChangeFlags.BlendState;
			}

			if (!GorgonColor.Equals(ref state._blendFactor, ref _blendFactor))
			{
				pipelineFlags |= PipelineStateChangeFlags.BlendFactor;
			}

			if (state.BlendSampleMask != _blendSampleMask)
			{
				pipelineFlags |= PipelineStateChangeFlags.BlendSampleMask;
			}

			if (state.DepthStencilReference != _depthStencilReference)
			{
				pipelineFlags |= PipelineStateChangeFlags.DepthStencilReference;
			}

			return pipelineFlags | mutableFlags;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineState" /> class.
		/// </summary>
		/// <param name="stateInfo">The <see cref="IGorgonPipelineStateInfo"/> used to create this state object.</param>
		/// <param name="id">The ID of the cache entry for this pipeline state.</param>
		internal GorgonPipelineState(IGorgonPipelineStateInfo stateInfo, int id)
		{
			unchecked
			{
				BlendSampleMask = (int)(0xffffffff);
				BlendFactor = new GorgonColor(1, 1, 1, 1);
				DepthStencilReference = 0;
			}

			_info = new GorgonPipelineStateInfo(stateInfo);
			ID = id;
		}
		#endregion
	}
}
