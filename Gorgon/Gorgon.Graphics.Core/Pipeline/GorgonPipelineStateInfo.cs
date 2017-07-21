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
// Created: August 17, 2016 9:05:35 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Math;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Pipeline state information used to create a pipeline state for the underlying renderer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A pipeline state is used to set all the necessary states for rendering on the GPU prior to making a draw call. 
	/// </para>
	/// </remarks>
	public class GorgonPipelineStateInfo 
		: IGorgonPipelineStateInfo
	{
	    #region Properties.
        /// <summary>
        /// Property to set or return the current pixel shader 
        /// </summary>
        public GorgonPixelShader PixelShader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current vertex shader 
		/// </summary>
		public GorgonVertexShader VertexShader
		{
			get;
			set;
		}

	    /// <summary>
	    /// Property to set or return the current <see cref="GorgonRasterState"/>.
	    /// </summary>
	    public GorgonRasterState RasterState
	    {
	        get;
	        set;
	    }

	    /// <summary>
	    /// Property to return the current <see cref="GorgonRasterState"/>.
	    /// </summary>
	    GorgonRasterState IGorgonPipelineStateInfo.RasterState => RasterState;

		/// <summary>
		/// Property to set or return the current <see cref="GorgonDepthStencilState"/>.
		/// </summary>
		public GorgonDepthStencilState DepthStencilState
		{
			get;
			set;
		}

        /// <summary>
        /// Property to return the current <see cref="GorgonDepthStencilState"/>.
        /// </summary>
        GorgonDepthStencilState IGorgonPipelineStateInfo.DepthStencilState => DepthStencilState;

		/// <summary>
		/// Property to set or return whether alpha to coverage is enabled or not for blending.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will use alpha to coverage as a multisampling technique when writing a pixel to a render target. Alpha to coverage is useful in situations where there are multiple overlapping polygons 
		/// that use transparency to define edges.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		public bool IsAlphaToCoverageEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether independent render target blending is enabled or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will specify whether to use different blending states for each render target. When this value is set to <b>true</b>, each render target blend state will be independent of other render 
		/// target blend states. When this value is set to <b>false</b>, then only the blend state of the first render target is used.
		/// </para>
		/// <para>
		/// The default value is <b>false</b>.
		/// </para>
		/// </remarks>
		public bool IsIndependentBlendingEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current blending states for individual render targets.
		/// </summary>
		/// <remarks>
		/// This will only support up to 8 state items. If the array is larger than 8 items, then only the first 8 items will be used.
		/// </remarks>
		public GorgonBlendState[] BlendStates
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the current blending state for an individual render target.
		/// </summary>
		IReadOnlyList<GorgonBlendState> IGorgonPipelineStateInfo.BlendStates => BlendStates;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy a <see cref="IGorgonPipelineStateInfo"/> into this one.
        /// </summary>
        /// <param name="other">The other pipeline state to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="other"/> parameter is <b>null</b>.</exception>
	    public void CopyFrom(IGorgonPipelineStateInfo other)
	    {
	        if (other == null)
	        {
	            throw new ArgumentNullException(nameof(other));
	        }

	        IsIndependentBlendingEnabled = other.IsIndependentBlendingEnabled;
	        IsAlphaToCoverageEnabled = other.IsAlphaToCoverageEnabled;
	        PixelShader = other.PixelShader;
	        VertexShader = other.VertexShader;

	        if (other.DepthStencilState != null)
	        {
	            if (DepthStencilState == null)
	            {
	                DepthStencilState = new GorgonDepthStencilState(other.DepthStencilState);
	            }
	            else
	            {
	                other.DepthStencilState.CopyTo(DepthStencilState);
	                DepthStencilState.IsLocked = false;
	            }
	        }
	        else
	        {
	            DepthStencilState = null;
	        }

	        if (other.RasterState != null)
	        {
	            if (RasterState == null)
	            {
	                RasterState = new GorgonRasterState(other.RasterState);
	            }
	            else
	            {
	                other.RasterState.CopyTo(RasterState);
	                other.RasterState.IsLocked = false;
	            }
	        }
	        else
	        {
	            RasterState = null;
	        }

	        if (other.BlendStates == null)
	        {
	            BlendStates = null;
	            return;
	        }

	        if ((BlendStates == null) || (other.BlendStates.Count != BlendStates.Length))
	        {
	            BlendStates = new GorgonBlendState[other.BlendStates.Count.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount)];
	        }

	        for (int i = 0; i < BlendStates.Length; ++i)
	        {
	            if (other.BlendStates[i] == null)
	            {
	                continue;
	            }

	            if (BlendStates[i] == null)
	            {
	                BlendStates[i] = new GorgonBlendState(other.BlendStates[i]);
	            }
	            else
	            {
	                other.BlendStates[i].CopyTo(BlendStates[i]);
	                BlendStates[i].IsLocked = false;
	            }
	        }
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(IGorgonPipelineStateInfo other)
	    {
	        if (other == null)
	        {
	            return false;
	        }

	        if (other == this)
	        {
	            return true;
	        }

            if ((VertexShader != other.VertexShader)
                || (PixelShader != other.PixelShader)
                || (IsAlphaToCoverageEnabled != other.IsAlphaToCoverageEnabled)
	            || (IsIndependentBlendingEnabled != other.IsIndependentBlendingEnabled))
	        {
	            return false;
	        }

	        if ((DepthStencilState != other.DepthStencilState) 
                && ((DepthStencilState == null) || (!DepthStencilState.Equals(other.DepthStencilState))))
	        {
	            return false;
	        }

	        if ((RasterState != other.RasterState) 
                && ((RasterState == null) || (!RasterState.Equals(other.RasterState))))
	        {
	            return false;
	        }

            // Check the blend states.
	        return BlendStates.ListEquals(other.BlendStates);
	    }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineStateInfo"/> class.
        /// </summary>
        /// <param name="info">The <see cref="IGorgonPipelineStateInfo"/> to copy from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonPipelineStateInfo(IGorgonPipelineStateInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

            CopyFrom(info);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineStateInfo"/> class.
		/// </summary>
		public GorgonPipelineStateInfo()
		{
            RasterState = GorgonRasterState.Default;
            DepthStencilState = GorgonDepthStencilState.Default;
		    BlendStates = new[]
		                  {
		                      GorgonBlendState.Default,
		                  };
		}
        #endregion
    }
}
