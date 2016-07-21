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
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics
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
		/// The render target views have been modified.
		/// </summary>
		RenderTargetViews = 0x1,
		/// <summary>
		/// The input layout has been modified.
		/// </summary>
		InputLayout = 0x2,
		/// <summary>
		/// The vertex shader state has been modified.
		/// </summary>
		VertexShader = 0x4,
		/// <summary>
		/// The pixel shader state has been modified.
		/// </summary>
		PixelShader = 0x8,
		/// <summary>
		/// The view port was modified.
		/// </summary>
		Viewport = 0x10
	}

	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
		: IEquatable<GorgonPipelineState>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the current pixel shader state.
		/// </summary>
		public GorgonPixelShaderState PixelShader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current vertex shader state.
		/// </summary>
		public GorgonVertexShaderState VertexShader
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
		/// Property to set or return the current render target views and depth/stencil view for this state.
		/// </summary>
		public GorgonRenderTargetViews RenderTargetViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current viewport(s) for this state.
		/// </summary>
		public GorgonViewports Viewports
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(GorgonPipelineState other)
		{
			return ((other != null)
			        && (other.InputLayout == InputLayout)
			        && (GorgonRenderTargetViews.Equals(other.RenderTargetViews, RenderTargetViews))
					&& (GorgonViewports.Equals(other.Viewports, Viewports))
			        && (PixelShader == other.PixelShader)
			        && (VertexShader == other.VertexShader));
		}

		/// <summary>
		/// Function to reset back to the default states.
		/// </summary>
		public void Reset()
		{
			VertexShader.Shader = null;
			PixelShader.Shader = null;
			InputLayout = null;

			for (int i = 0; i < RenderTargetViews.Count; ++i)
			{
				RenderTargetViews[i] = null;
			}

			for (int i = 0; i < PixelShader.ConstantBuffers.Count; ++i)
			{
				PixelShader.ConstantBuffers[i] = null;
				VertexShader.ConstantBuffers[i] = null;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineState"/> class.
		/// </summary>
		public GorgonPipelineState()
		{
			RenderTargetViews = new GorgonRenderTargetViews();
			PixelShader = new GorgonPixelShaderState();
			VertexShader = new GorgonVertexShaderState();
			Viewports = new GorgonViewports();

			Reset();
		}
		#endregion
	}
}
