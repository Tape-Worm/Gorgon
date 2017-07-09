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
// Created: August 22, 2016 12:29:40 AM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Pipeline state information used to create a <see cref="GorgonPipelineState"/> to bind state information to the GPU.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A pipeline state is used to set all the necessary states for rendering on the GPU prior to making a draw call. 
	/// </para>
	/// <para>
	/// This interface is used to provide an immutable view of the pipeline state information object that is used to create a <see cref="GorgonPipelineState"/> so that it cannot be modified.
	/// </para>
	/// </remarks>
	public interface IGorgonPipelineStateInfo
	{
		/// <summary>
		/// Property to return the current pixel shader 
		/// </summary>
		GorgonPixelShader PixelShader
		{
			get;
		}

		/// <summary>
		/// Property to return the current vertex shader 
		/// </summary>
		GorgonVertexShader VertexShader
		{
			get;
		}

		/// <summary>
		/// Property to return the current <see cref="IGorgonRasterStateInfo"/>.
		/// </summary>
		GorgonRasterState RasterState
		{
			get;
		}

		/// <summary>
		/// Property to return the current <see cref="IGorgonDepthStencilStateInfo"/>.
		/// </summary>
		IGorgonDepthStencilStateInfo DepthStencilState
		{
			get;
		}

		/// <summary>
		/// Property to return whether alpha to coverage is enabled or not for blending.
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
		bool IsAlphaToCoverageEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return whether independent render target blending is enabled or not.
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
		bool IsIndependentBlendingEnabled
		{
			get;
		}

		/// <summary>
		/// Property to return the current blending state for an individual render target.
		/// </summary>
		IReadOnlyList<IGorgonRenderTargetBlendStateInfo> RenderTargetBlendState
		{
			get;
		}
	}
}