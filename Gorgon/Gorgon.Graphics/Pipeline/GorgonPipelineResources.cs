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
// Created: August 17, 2016 9:40:02 PM
// 
#endregion

using System;
using System.Linq;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Flags to indicate what part of the pipeline was changed.
	/// </summary>
	[Flags]
	public enum PipelineResourceChangeFlags
		: ulong
	{
		/// <summary>
		/// No changes.
		/// </summary>
		None = 0,
		/// <summary>
		/// Vertex buffer bindings changed.
		/// </summary>
		VertexBuffer = 0x1,
		/// <summary>
		/// Index buffer changes.
		/// </summary>
		IndexBuffer = 0x2,
		/// <summary>
		/// Render target views changed.
		/// </summary>
		RenderTargets = 0x4,
		/// <summary>
		/// Constant buffers for the pixel shader changed.
		/// </summary>
		PixelShaderConstantBuffer = 0x8,
		/// <summary>
		/// Constant buffers for the vertex shader changed.
		/// </summary>
		VertexShaderConstantBuffer = 0x10,
		/// <summary>
		/// Shader resources for the pixel shader changed.
		/// </summary>
		PixelShaderResource = 0x20,
		/// <summary>
		/// Shader resources for the vertex shader changed.
		/// </summary>
		VertexShaderResource = 0x40,
		/// <summary>
		/// Samplers for the pixel shader changed.
		/// </summary>
		PixelShaderSampler = 0x80
	}

	/// <summary>
	/// Used to bind resource types (e.g. buffers, textures, etc...) to the GPU pipeline.
	/// </summary>
	public class GorgonPipelineResources
	{
		#region Variables.
		// The default resource state for the pipeline.
		private static readonly PipelineResourceChangeFlags _defaultState;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the vertex buffers to bind to the pipeline.
		/// </summary>
		public GorgonVertexBufferBindings VertexBuffers
		{
			get;
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
		/// Property to return the pixel shader constant buffers to bind to the pipeline.
		/// </summary>
		public GorgonConstantBuffers PixelShaderConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to return the vertex shader constant buffers to bind to the pipeline.
		/// </summary>
		public GorgonConstantBuffers VertexShaderConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to return the pixel shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews PixelShaderResources
		{
			get;
		}

		/// <summary>
		/// Property to return the vertex shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews VertexShaderResources
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the differences in state changes between two <see cref="GorgonPipelineResources"/> objects.
		/// </summary>
		/// <param name="resources">The <see cref="GorgonPipelineResources"/> to evaluate.</param>
		/// <returns>A <see cref="PipelineResourceChangeFlags"/> type to indicate which parts of the resource bindings have changed.</returns>
		public PipelineResourceChangeFlags GetChanges(GorgonPipelineResources resources)
		{
			if (resources == null)
			{
				return _defaultState;
			}

			var result = PipelineResourceChangeFlags.None;

			if (IndexBuffer != resources.IndexBuffer)
			{
				result |= PipelineResourceChangeFlags.IndexBuffer;
			}

			if (!GorgonVertexBufferBindings.Equals(VertexBuffers, resources.VertexBuffers))
			{
				result |= PipelineResourceChangeFlags.VertexBuffer;
			}

			if (!GorgonRenderTargetViews.Equals(RenderTargets, resources.RenderTargets))
			{
				result |= PipelineResourceChangeFlags.RenderTargets;
			}

			if (!GorgonConstantBuffers.Equals(PixelShaderConstantBuffers, resources.PixelShaderConstantBuffers))
			{
				result |= PipelineResourceChangeFlags.PixelShaderConstantBuffer;
			}

			if (!GorgonConstantBuffers.Equals(VertexShaderConstantBuffers, resources.VertexShaderConstantBuffers))
			{
				result |= PipelineResourceChangeFlags.VertexShaderConstantBuffer;
			}

			if (!GorgonShaderResourceViews.Equals(PixelShaderResources, resources.PixelShaderResources))
			{
				result |= PipelineResourceChangeFlags.PixelShaderResource;
			}

			if (!GorgonShaderResourceViews.Equals(VertexShaderResources, resources.VertexShaderResources))
			{
				result |= PipelineResourceChangeFlags.VertexShaderResource;
			}

			if (!GorgonSamplerStates.Equals(PixelShaderSamplers, resources.PixelShaderSamplers))
			{
				result |= PipelineResourceChangeFlags.PixelShaderSampler;
			}

			return result;
		}

		/// <summary>
		/// Function to unbind the resources.
		/// </summary>
		public void Reset()
		{
			IndexBuffer = null;
			RenderTargets.Clear();
			VertexBuffers.Clear();
			PixelShaderResources.Clear();
			PixelShaderConstantBuffers.Clear();
			PixelShaderSamplers.Clear();

			VertexShaderConstantBuffers.Clear();
			VertexShaderResources.Clear();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineResources"/> class.
		/// </summary>
		public GorgonPipelineResources()
		{
			VertexBuffers = new GorgonVertexBufferBindings();
			RenderTargets = new GorgonRenderTargetViews();
			PixelShaderConstantBuffers = new GorgonConstantBuffers();
			VertexShaderConstantBuffers = new GorgonConstantBuffers();
			VertexShaderResources = new GorgonShaderResourceViews();
			PixelShaderResources = new GorgonShaderResourceViews();
			PixelShaderSamplers = new GorgonSamplerStates();
		}

		/// <summary>
		/// Initializes static members of the <see cref="GorgonPipelineResources"/> class.
		/// </summary>
		static GorgonPipelineResources()
		{
			var states = (PipelineResourceChangeFlags[])Enum.GetValues(typeof(PipelineResourceChangeFlags));

			foreach (PipelineResourceChangeFlags state in states.Where(item => item != PipelineResourceChangeFlags.None))
			{
				_defaultState |= state;
			}
		}
		#endregion
	}
}
