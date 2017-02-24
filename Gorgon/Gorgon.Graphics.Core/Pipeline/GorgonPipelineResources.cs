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
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core
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
		PixelShaderSampler = 0x80,
	}

	/// <summary>
	/// Used to bind resource types (e.g. buffers, textures, etc...) to the GPU pipeline.
	/// </summary>
	public class GorgonPipelineResources
	{
		#region Variables.
		// The list of vertex buffer resources bound.
		private GorgonVertexBufferBindings _vertexBuffers;
		// The list of render target resources bound.
		private GorgonRenderTargetViews _renderTargets;
		// The list of constant buffers for a pixel shader.
		private GorgonConstantBuffers _pixelShaderConstantBuffers;
		// The list of constant buffers for a vertex shader.
		private GorgonConstantBuffers _vertexShaderConstantBuffers;
		// The index buffer.
		private GorgonIndexBuffer _indexBuffer;
		// The list of texture samplers for a pixel shader.
		private GorgonSamplerStates _pixelShaderSamplers;
		// The list of pixel shader resources.
		private GorgonShaderResourceViews _pixelShaderResources;
		// The list of vertex shader resources.
		private GorgonShaderResourceViews _vertexShaderResources;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the available changes on this resource list.
		/// </summary>
		internal PipelineResourceChangeFlags Changes
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the vertex buffers to bind to the pipeline.
		/// </summary>
		public GorgonVertexBufferBindings VertexBuffers
		{
			get
			{
				return _vertexBuffers;
			}
			set
			{
				if (_vertexBuffers == value)
				{
					return;
				}

				_vertexBuffers = value;
				Changes |= PipelineResourceChangeFlags.VertexBuffer;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer to bind to the pipeline.
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get
			{
				return _indexBuffer;
			}
			set
			{
				if (_indexBuffer == value)
				{
					return;
				}

				_indexBuffer = value;
				Changes |= PipelineResourceChangeFlags.IndexBuffer;
			}
		}

		/// <summary>
		/// Property to return the render targets to bind to the pipeline.
		/// </summary>
		public GorgonRenderTargetViews RenderTargets
		{
			get
			{
				return _renderTargets;
			}
			set
			{
				if (_renderTargets == value)
				{
					return;
				}

				_renderTargets = value;
				Changes |= PipelineResourceChangeFlags.RenderTargets;
			}
		}

		/// <summary>
		/// Property to return the pixel shader constant buffers to bind to the pipeline.
		/// </summary>
		public GorgonConstantBuffers PixelShaderConstantBuffers
		{
			get
			{
				return _pixelShaderConstantBuffers;
			}
			set
			{
				if (_pixelShaderConstantBuffers == value)
				{
					return;
				}

				_pixelShaderConstantBuffers = value;
				Changes |= PipelineResourceChangeFlags.PixelShaderConstantBuffer;
			}
		}

		/// <summary>
		/// Property to return the vertex shader constant buffers to bind to the pipeline.
		/// </summary>
		public GorgonConstantBuffers VertexShaderConstantBuffers
		{
			get
			{
				return _vertexShaderConstantBuffers;
			}
			set
			{
				if (_vertexShaderConstantBuffers == value)
				{
					return;
				}

				_vertexShaderConstantBuffers = value;
				Changes |= PipelineResourceChangeFlags.VertexShaderConstantBuffer;
			}
		}

		/// <summary>
		/// Property to return the pixel shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews PixelShaderResources
		{
			get
			{
				return _pixelShaderResources;
			}
			set
			{
				if (_pixelShaderResources == value)
				{
					return;
				}

				_pixelShaderResources = value;
				Changes |= PipelineResourceChangeFlags.PixelShaderResource;
			}
		}

		/// <summary>
		/// Property to return the vertex shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews VertexShaderResources
		{
			get
			{
				return _vertexShaderResources;
			}
			set
			{
				if (_vertexShaderResources == value)
				{
					return;
				}

				_vertexShaderResources = value;
				Changes |= PipelineResourceChangeFlags.VertexShaderResource;
			}
		}

		/// <summary>
		/// Property to return the pixel shader samplers to bind to the pipeline.
		/// </summary>
		public GorgonSamplerStates PixelShaderSamplers
		{
			get
			{
				return _pixelShaderSamplers;
			}
			set
			{
				if (_pixelShaderSamplers == value)
				{
					return;
				}

				_pixelShaderSamplers = value;
				Changes |= PipelineResourceChangeFlags.PixelShaderSampler;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the pipeline resources from another instance.
		/// </summary>
		/// <param name="resources">The resources to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="resources"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This will copy the items from another <see cref="GorgonPipelineResources"/> state into this state. This is a shallow copy, and the items hold the same references as the original 
		/// <paramref name="resources"/>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Exceptions are only thrown from this method when Gorgon is compiled as <b>DEBUG</b>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyResourceState(GorgonPipelineResources resources)
		{
			resources.ValidateObject(nameof(resources));

			VertexBuffers = resources.VertexBuffers;
			IndexBuffer = resources.IndexBuffer;
			PixelShaderResources = resources.PixelShaderResources;
			VertexShaderResources = resources.VertexShaderResources;
			PixelShaderConstantBuffers = resources.PixelShaderConstantBuffers;
			VertexShaderConstantBuffers = resources.VertexShaderConstantBuffers;
			PixelShaderSamplers = resources.PixelShaderSamplers;
			RenderTargets = resources.RenderTargets;
		}
		#endregion
	}
}
