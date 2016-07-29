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
using System.Linq;
using DX = SharpDX;

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
		Viewport = 0x10,
		/// <summary>
		/// The vertex buffers were modified.
		/// </summary>
		VertexBuffers = 0x20,
		/// <summary>
		/// The index buffer was modified.
		/// </summary>
		IndexBuffer = 0x40,
		/// <summary>
		/// The rasterizer state was modified.
		/// </summary>
		RasterState = 0x80,
		/// <summary>
		/// The scissor rectangles have been updated.
		/// </summary>
		ScissorRectangles = 0x100
	}

	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
		: IEquatable<GorgonPipelineState>
	{
		#region Variables.
		// The default state flags.
		private static readonly StateChanges _defaultStates;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current pixel shader 
		/// </summary>
		public GorgonPixelShaderState PixelShader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current vertex shader 
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
		/// Property to set or return the current render target views and depth/stencil view for this 
		/// </summary>
		public GorgonRenderTargetViews RenderTargetViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current viewport(s) for this 
		/// </summary>
		public GorgonViewports Viewports
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current vertex buffers for this 
		/// </summary>
		public GorgonVertexBufferBindings VertexBuffers
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current index buffer for this 
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current rasterizer state.
		/// </summary>
		public GorgonRasterState RasterState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current scissor rectangles.
		/// </summary>
		public GorgonScissorRectangles ScissorRectangles
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare this pipeline state with another pipeline state.
		/// </summary>
		/// <param name="state">The state to compare.</param>
		/// <param name="newState">The changes between the two states.</param>
		internal void GetChanges(GorgonPipelineState state, out StateChanges newState)
		{
			if (state == null)
			{
				newState = _defaultStates;
				return;
			}

			var pipelineFlags = PipelineStateChangeFlags.None;

			// Get main pipeline changes.
			if (InputLayout != state.InputLayout)
			{
				pipelineFlags |= PipelineStateChangeFlags.InputLayout;
			}

			if (!GorgonRenderTargetViews.Equals(RenderTargetViews, state.RenderTargetViews))
			{
				pipelineFlags |= PipelineStateChangeFlags.RenderTargetViews;
			}

			if (!GorgonViewports.Equals(Viewports, state.Viewports))
			{
				pipelineFlags |= PipelineStateChangeFlags.Viewport;
			}

			if (!GorgonScissorRectangles.Equals(ScissorRectangles, state.ScissorRectangles))
			{
				pipelineFlags |= PipelineStateChangeFlags.ScissorRectangles;
			}

			if (!GorgonVertexBufferBindings.Equals(VertexBuffers, state.VertexBuffers))
			{
				pipelineFlags |= PipelineStateChangeFlags.VertexBuffers;
			}

			if (IndexBuffer != state.IndexBuffer)
			{
				pipelineFlags |= PipelineStateChangeFlags.IndexBuffer;
			}

			if (RasterState != state.RasterState)
			{
				pipelineFlags |= PipelineStateChangeFlags.RasterState;
			}

			// Gather shader sub state changes.
			ShaderStateChangeFlags pixelShaderFlags;
			ShaderStateChangeFlags vertexShaderFlags;

			if (PixelShader != null)
			{
				pixelShaderFlags = PixelShader.GetChanges(state.PixelShader);
			}
			else
			{
				pixelShaderFlags = ShaderStateChangeFlags.Shader | ShaderStateChangeFlags.Constants | ShaderStateChangeFlags.ShaderResourceViews | ShaderStateChangeFlags.SamplerStates;
			}

			if (VertexShader != null)
			{
				vertexShaderFlags = VertexShader.GetChanges(state.VertexShader);
			}
			else
			{
				vertexShaderFlags = ShaderStateChangeFlags.Shader | ShaderStateChangeFlags.Constants | ShaderStateChangeFlags.ShaderResourceViews;
			}

			// If the shaders have sub state changes, then record that the shader info has changed on the pipeline.
			if (pixelShaderFlags != ShaderStateChangeFlags.None)
			{
				pipelineFlags |= PipelineStateChangeFlags.PixelShader;
			}

			if (vertexShaderFlags != ShaderStateChangeFlags.None)
			{
				pipelineFlags |= PipelineStateChangeFlags.VertexShader;
			}

			newState = new StateChanges
			           {
				           PipelineFlags = pipelineFlags,
				           PixelShaderStateFlags = pixelShaderFlags,
				           VertexShaderStateFlags = vertexShaderFlags
			           };
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(GorgonPipelineState other)
		{
			return ((other != null)
			        && (other.InputLayout == InputLayout)
			        && (GorgonRenderTargetViews.Equals(other.RenderTargetViews, RenderTargetViews))
					&& (GorgonViewports.Equals(other.Viewports, Viewports))
					&& (GorgonVertexBufferBindings.Equals(other.VertexBuffers, VertexBuffers))
					&& (GorgonScissorRectangles.Equals(other.ScissorRectangles, ScissorRectangles))
					&& (IndexBuffer == other.IndexBuffer)
			        && (PixelShader == other.PixelShader)
			        && (VertexShader == other.VertexShader)
					&& (RasterState == other.RasterState));
		}

		/// <summary>
		/// Function to reset back to the default states.
		/// </summary>
		public void Reset()
		{
			VertexShader.Shader = null;
			PixelShader.Shader = null;
			InputLayout = null;
			IndexBuffer = null;
			RasterState = null;

			for (int i = 0; i < VertexBuffers.Count; ++i)
			{
				VertexBuffers[i] = GorgonVertexBufferBinding.Empty;
			}

			for (int i = 0; i < Viewports.Count; ++i)
			{
				Viewports[i] = default(DX.ViewportF);
			}

			for (int i = 0; i < RenderTargetViews.Count; ++i)
			{
				RenderTargetViews[i] = null;
			}

			for (int i = 0; i < ScissorRectangles.Count; ++i)
			{
				ScissorRectangles[i] = DX.Rectangle.Empty;
			}

			for (int i = 0; i < PixelShader.SamplerStates.Count; ++i)
			{
				PixelShader.SamplerStates[i] = null;
			}

			for (int i = 0; i < PixelShader.ConstantBuffers.Count; ++i)
			{
				PixelShader.ConstantBuffers[i] = null;
				VertexShader.ConstantBuffers[i] = null;
			}

			for (int i = 0; i < PixelShader.ResourceViews.Count; ++i)
			{
				PixelShader.ResourceViews[i] = null;
				VertexShader.ResourceViews[i] = null;
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
			VertexBuffers = new GorgonVertexBufferBindings();
			ScissorRectangles = new GorgonScissorRectangles();

			Reset();
		}

		/// <summary>
		/// Initializes static members of the <see cref="GorgonPipelineState"/> class.
		/// </summary>
		static GorgonPipelineState()
		{
			// Initialize our default flags for a null current 
			var pipelineFlags = (PipelineStateChangeFlags[])Enum.GetValues(typeof(PipelineStateChangeFlags));
			var shaderFlags = (ShaderStateChangeFlags[])Enum.GetValues(typeof(ShaderStateChangeFlags));
			PipelineStateChangeFlags currentPipelineFlags = PipelineStateChangeFlags.None;
			ShaderStateChangeFlags currentShaderFlags = ShaderStateChangeFlags.None;

			foreach (PipelineStateChangeFlags flag in pipelineFlags.Where(item => item != PipelineStateChangeFlags.None))
			{
				currentPipelineFlags |= flag;
			}

			foreach (ShaderStateChangeFlags flag in shaderFlags.Where(item => item != ShaderStateChangeFlags.None))
			{
				currentShaderFlags |= flag;
			}

			_defaultStates = new StateChanges
			                 {
				                 VertexShaderStateFlags = currentShaderFlags,
				                 PixelShaderStateFlags = currentShaderFlags,
				                 PipelineFlags = currentPipelineFlags
			                 };
		}
		#endregion
	}
}
