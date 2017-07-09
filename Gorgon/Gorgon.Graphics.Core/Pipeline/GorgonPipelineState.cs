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
	internal enum PipelineStateChange
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
		/// The geometry shader state has been modified.
		/// </summary>
		GeometryShader = 0x4,
		/// <summary>
		/// The hull shader state has been modified.
		/// </summary>
		HullShader = 0x8,
		/// <summary>
		/// The domain shader state has been modified.
		/// </summary>
		DomainShader = 0x10,
		/// <summary>
		/// The compute shader state has been modified.
		/// </summary>
		ComputeShader = 0x20,
		/// <summary>
		/// The rasterizer state was modified.
		/// </summary>
		RasterState = 0x40,
		/// <summary>
		/// The depth/stencil state has been updated.
		/// </summary>
		DepthStencilState = 0x80,
		/// <summary>
		/// The blending state has been updated.
		/// </summary>
		BlendState = 0x100,
		/// <summary>
		/// All states have changed.
		/// </summary>
		All = VertexShader | PixelShader | GeometryShader | HullShader | DomainShader | ComputeShader | RasterState | BlendState | DepthStencilState
	}

	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
	{
		#region Variables.
		// Information used to create the pipeline state.
		private readonly GorgonPipelineStateInfo _info;
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
		/// Property to return the <see cref="IGorgonPipelineStateInfo"/> used to create this object.
		/// </summary>
		public IGorgonPipelineStateInfo Info => _info;
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineState" /> class.
		/// </summary>
		/// <param name="stateInfo">The <see cref="IGorgonPipelineStateInfo"/> used to create this state object.</param>
		/// <param name="id">The ID of the cache entry for this pipeline state.</param>
		internal GorgonPipelineState(IGorgonPipelineStateInfo stateInfo, int id)
		{
			_info = new GorgonPipelineStateInfo(stateInfo);
			ID = id;

            // Lock all states that need locking at this point.
		    if (_info.RasterState != null)
		    {
		        _info.RasterState.IsLocked = true;
		    }
		}
		#endregion
	}
}
