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

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
	{
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
	        internal set;
	    } = int.MinValue;

        /// <summary>
        /// Property to return the pixel shader.
        /// </summary>
	    public GorgonPixelShader PixelShader
	    {
	        get;
	        internal set;
	    }

	    /// <summary>
	    /// Property to return the vertex shader.
	    /// </summary>
	    public GorgonVertexShader VertexShader
	    {
	        get;
	        internal set;
	    }

	    /// <summary>
	    /// Property to return the geometry shader.
	    /// </summary>
	    public GorgonGeometryShader GeometryShader
	    {
	        get;
	        internal set;
	    }

	    /// <summary>
	    /// Property to return the domain shader.
	    /// </summary>
	    public GorgonDomainShader DomainShader
	    {
	        get;
	        internal set;
	    }

	    /// <summary>
	    /// Property to return the hull shader.
	    /// </summary>
	    public GorgonHullShader HullShader
	    {
	        get;
	        internal set;
	    }

	    /// <summary>
	    /// Property to return the compute shader.
	    /// </summary>
	    public GorgonComputeShader ComputeShader
	    {
	        get;
	        internal set;
	    }
        
        /// <summary>
        /// Property to return the rasterizer state for the pipeline.
        /// </summary>
	    public GorgonRasterState RasterState
	    {
	        get;
            internal set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clear the pipeline state.
        /// </summary>
	    internal void Clear()
        {
            RasterState = null;
            PixelShader = null;
            VertexShader = null;
            GeometryShader = null;
            DomainShader = null;
            HullShader = null;
            ComputeShader = null;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineState"/> class.
        /// </summary>
        /// <param name="state">The state copy.</param>
        internal GorgonPipelineState(GorgonPipelineState state)
        {
            RasterState = state.RasterState;
            PixelShader = state.PixelShader;
            VertexShader = state.VertexShader;
            GeometryShader = state.GeometryShader;
            DomainShader = state.DomainShader;
            HullShader = state.HullShader;
            ComputeShader = state.ComputeShader;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineState" /> class.
		/// </summary>
		internal GorgonPipelineState()
		{
		}
		#endregion
	}
}
