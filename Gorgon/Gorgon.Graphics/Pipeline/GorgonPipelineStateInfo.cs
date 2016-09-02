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
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using System.Collections.Generic;
using Gorgon.Math;

namespace Gorgon.Graphics
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
		/// Property to set or return the current input layout used to define how vertices are interpreted in a vertex shader and/or vertex buffer.
		/// </summary>
		public GorgonInputLayout InputLayout
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current viewport(s) for this pipeline state information.
		/// </summary>
		/// <remarks>
		/// This will only support up to 16 viewport items. If the array is larger than 16 items, the only the first 16 items will be used.
		/// </remarks>
		public DX.ViewportF[] Viewports
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current <see cref="IGorgonRasterStateInfo"/>.
		/// </summary>
		public IGorgonRasterStateInfo RasterState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current <see cref="IGorgonDepthStencilStateInfo"/>.
		/// </summary>
		public IGorgonDepthStencilStateInfo DepthStencilState
		{
			get;
			set;
		}

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
		public IGorgonRenderTargetBlendStateInfo[] RenderTargetBlendState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current scissor rectangles used to clip the pixels being rendered.
		/// </summary>
		/// <remarks>
		/// This will only support up to 16 scissor rectangles. If the array is larger than 16 items, the only the first 16 items will be used.
		/// </remarks>
		public DX.Rectangle[] ScissorRectangles
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the current viewport(s) for this pipeline state information.
		/// </summary>
		IReadOnlyList<DX.ViewportF> IGorgonPipelineStateInfo.Viewports => Viewports;

		/// <summary>
		/// Property to return the current blending state for an individual render target.
		/// </summary>
		IReadOnlyList<IGorgonRenderTargetBlendStateInfo> IGorgonPipelineStateInfo.RenderTargetBlendState => RenderTargetBlendState;

		/// <summary>
		/// Property to return the current scissor rectangles used to clip the pixels being rendered.
		/// </summary>
		IReadOnlyList<DX.Rectangle> IGorgonPipelineStateInfo.ScissorRectangles => ScissorRectangles;

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

			IsIndependentBlendingEnabled = info.IsIndependentBlendingEnabled;
			IsAlphaToCoverageEnabled = info.IsAlphaToCoverageEnabled;
			PixelShader = info.PixelShader;
			DepthStencilState = info.DepthStencilState;
			InputLayout = info.InputLayout;
			RasterState = info.RasterState;
			VertexShader = info.VertexShader;

			if (info.RenderTargetBlendState != null)
			{
				RenderTargetBlendState = new IGorgonRenderTargetBlendStateInfo[info.RenderTargetBlendState.Count.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount)];

				for (int i = 0; i < RenderTargetBlendState.Length; ++i)
				{
					RenderTargetBlendState[i] = new GorgonRenderTargetBlendStateInfo(info.RenderTargetBlendState[i]);
				}
			}

			if (info.ScissorRectangles != null)
			{
				ScissorRectangles = new DX.Rectangle[info.ScissorRectangles.Count.Min(16)];

				for (int i = 0; i < ScissorRectangles.Length.Min(16); ++i)
				{
					ScissorRectangles[i] = info.ScissorRectangles[i];
				}
			}

			if (info.Viewports == null)
			{
				return;
			}

			Viewports = new DX.ViewportF[info.Viewports.Count.Min(16)];

			for (int i = 0; i < Viewports.Length.Min(16); ++i)
			{
				Viewports[i] = info.Viewports[i];
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineStateInfo"/> class.
		/// </summary>
		public GorgonPipelineStateInfo()
		{
		}
	}
}
