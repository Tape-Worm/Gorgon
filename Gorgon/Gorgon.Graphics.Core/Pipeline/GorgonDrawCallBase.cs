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
// Created: December 19, 2016 11:26:44 AM
// 
#endregion

using System;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Flags to indicate which part of the pipeline needs updating.
	/// </summary>
	[Flags]
	internal enum PipelineStateChange
	{
		/// <summary>
		/// Nothing has changed, just draw the item.
		/// </summary>
		None = 0,
		/// <summary>
		/// The primitive topology has changed.
		/// </summary>
		PrimitiveTopology = 0x1,
		/// <summary>
		/// Viewports have changed.
		/// </summary>
		Viewports = 0x2,
		/// <summary>
		/// Scissor rectangles have changed.
		/// </summary>
		ScissorRectangles = 0x4,
		/// <summary>
		/// Vertex buffers have changed.
		/// </summary>
		VertexBuffers = 0x8,
		/// <summary>
		/// Index buffer has changed.
		/// </summary>
		IndexBuffer = 0x10,
		/// <summary>
		/// Input layout has changed.
		/// </summary>
		InputLayout = 0x20,
		/// <summary>
		/// Render targets and/or depth stencil have changed.
		/// </summary>
		RenderTargets = 0x40,
		/// <summary>
		/// Vertex shader constant buffers have changed.
		/// </summary>
		VertexShaderConstantBuffers = 0x80,
		/// <summary>
		/// Pixel shader constant buffers have changed.
		/// </summary>
		PixelShaderConstantBuffers = 0x100,
		/// <summary>
		/// Vertex shader resources have changed.
		/// </summary>
		VertexShaderResources = 0x200,
		/// <summary>
		/// Pixel shader resources have changed.
		/// </summary>
		PixelShaderResources = 0x400,
		/// <summary>
		/// <para>
		/// Vertex shader samplers have changed.
		/// </para>
		/// <para>
		/// Only supported on devices that are feature level 11.0 or better.
		/// </para>
		/// </summary>
		VertexShaderSamplers = 0x800,
		/// <summary>
		/// Pixel shader samplers have changed.
		/// </summary>
		PixelShaderSamplers = 0x1000,
		/// <summary>
		/// Pipeline state has changed.
		/// </summary>
		PipelineState = 0x40000000,
		/// <summary>
		/// All states have changed.
		/// </summary>
		All = PrimitiveTopology 
			| Viewports 
			| ScissorRectangles 
			| VertexBuffers 
			| IndexBuffer 
			| InputLayout 
			| RenderTargets 
			| VertexShaderConstantBuffers 
			| PixelShaderConstantBuffers
			| PipelineState
	}

	/// <summary>
	/// A common class for draw calls.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A draw call is used to submit vertex (and potentially index and instance) data to the GPU pipeline or an output buffer. This type will contain all the necessary information used to set the state of the 
	/// pipeline prior to rendering any data.
	/// </para>
	/// </remarks>
	public class GorgonDrawCallBase
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed viewports.
		/// </summary>
		public const int MaximumViewportCount = 16;
		/// <summary>
		/// The maximum number of allowed scissor rectangles.
		/// </summary>
		public const int MaximumScissorCount = 16;
		#endregion

		#region Variables.
		// The viewports for rendering to the output.
		private GorgonMonitoredValueTypeArray<DX.ViewportF> _viewports;
		// The scissor rectangles for clipping the output.
		private GorgonMonitoredValueTypeArray<DX.Rectangle> _scissorRectangles;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the viewports to apply during this draw call.
		/// </summary>
		public GorgonMonitoredValueTypeArray<DX.ViewportF> Viewports
		{
			get => _viewports;
			set
			{
				if (value == null)
				{
					_viewports.Clear();
					return;
				}

				_viewports = value;
			}
		}

		/// <summary>
		/// Property to return the scissor rectangles to apply during this draw call.
		/// </summary>
		public GorgonMonitoredValueTypeArray<DX.Rectangle> ScissorRectangles
		{
			get => _scissorRectangles;
			set
			{
				if (value == null)
				{
					_scissorRectangles.Clear();
					return;
				}

				_scissorRectangles = value;
			}
		}

		/// <summary>
		/// Property to set or return the vertex buffers to bind to the pipeline.
		/// </summary>
		public GorgonVertexBufferBindings VertexBuffers
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the index buffer to bind to the pipeline.
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the render targets to bind to the pipeline.
		/// </summary>
		public GorgonRenderTargetViews RenderTargets
		{
			get;
		}

		/// <summary>
		/// Property to return the constant buffers for the vertex shader.
		/// </summary>
		public GorgonConstantBuffers VertexShaderConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to return the constant buffers for the pixel shader.
		/// </summary>
		public GorgonConstantBuffers PixelShaderConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to return the vertex shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews VertexShaderResourceViews
		{
			get;
		}

		/// <summary>
		/// Property to set or return the list of pixel shader resource views.
		/// </summary>
		public GorgonShaderResourceViews PixelShaderResourceViews
		{
			get;
		}

		/// <summary>
		/// Property to return the pixel shader samplers to bind to the pipeline.
		/// </summary>
		public GorgonSamplerStates PixelShaderSamplers
		{
			get;
		}

		/// <summary>
		/// Property to return the vertex shader samplers to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This only applies to an <see cref="IGorgonVideoDevice"/> that has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerStates VertexShaderSamplers
		{
			get;
		}

		/// <summary>
		/// Property to return the type of primitives to draw.
		/// </summary>
		public D3D.PrimitiveTopology PrimitiveTopology
		{
			get;
			set;
		} = D3D.PrimitiveTopology.TriangleStrip;

		/// <summary>
		/// Property to set or return the current pipeline state.
		/// </summary>
		/// <remarks>
		/// If this value is <b>null</b>, then the previous state will remain set.
		/// </remarks>
		public GorgonPipelineState State
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the states on this draw call back to an initialized state.
		/// </summary>
		public void Reset()
		{
			PrimitiveTopology = D3D.PrimitiveTopology.TriangleStrip;
			VertexBuffers = null;
			IndexBuffer = null;
			State = null;

			Viewports.Clear();
			ScissorRectangles.Clear();

			VertexShaderResourceViews.Clear();
			VertexShaderConstantBuffers.Clear();
			VertexShaderSamplers.Clear();

			PixelShaderResourceViews.Clear();
			PixelShaderConstantBuffers.Clear();
			PixelShaderSamplers.Clear();
			
			RenderTargets.DepthStencilView = null;
			RenderTargets.Clear();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDrawCallBase"/> class.
		/// </summary>
		protected internal GorgonDrawCallBase()
		{
			_viewports = new GorgonMonitoredValueTypeArray<DX.ViewportF>(MaximumViewportCount);
			_scissorRectangles = new GorgonMonitoredValueTypeArray<DX.Rectangle>(MaximumScissorCount);
			RenderTargets = new GorgonRenderTargetViews();
			VertexShaderConstantBuffers = new GorgonConstantBuffers();
			PixelShaderConstantBuffers = new GorgonConstantBuffers();
			PixelShaderResourceViews = new GorgonShaderResourceViews();
			VertexShaderResourceViews = new GorgonShaderResourceViews();
			PixelShaderSamplers = new GorgonSamplerStates();
			VertexShaderSamplers = new GorgonSamplerStates();
		}
		#endregion
	}
}
