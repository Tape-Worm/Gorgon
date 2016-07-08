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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		/// The primitive topology has been modified.
		/// </summary>
		PrimitiveTopology = 0x10
	}

	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
	{
		#region Variables.
		// The render target views to set for the pipeline.
		private GorgonRenderTargetViews _renderTargetViews;
		// The input layout to use for defining a vertex structure.
		private GorgonInputLayout _inputLayout;
		// The state used for the vertex shaders.
		private GorgonVertexShaderState _vertexShaderState;
		// The state used for the pixel shaders.
		private GorgonPixelShaderState _pixelShaderState;
		// The primitive topology state.
		private D3D.PrimitiveTopology _primitiveTopology;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current pipeline state as a <see cref="PipelineStateChangeFlags"/> value to determine state change.
		/// </summary>
		/// <remarks>
		/// This is used to determine what states have changed since the last pipeline state was set. This is used to reduce overhead when changing states during a frame.
		/// </remarks>
		public PipelineStateChangeFlags PipelineStateChangeFlags
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current pixel shader state.
		/// </summary>
		public GorgonPixelShaderState PixelShader
		{
			get
			{
				return _pixelShaderState;
			}
			set
			{
				if (_pixelShaderState == value)
				{
					return;
				}

				_pixelShaderState = value;
				PipelineStateChangeFlags |= PipelineStateChangeFlags.PixelShader;
			}
		}

		/// <summary>
		/// Property to set or return the current vertex shader state.
		/// </summary>
		public GorgonVertexShaderState VertexShader
		{
			get
			{
				return _vertexShaderState;
			}
			set
			{
				if (_vertexShaderState == value)
				{
					return;
				}

				_vertexShaderState = value;
				PipelineStateChangeFlags |= PipelineStateChangeFlags.VertexShader;
			}
		}

		/// <summary>
		/// Property to set or return the current input layout used to define how vertices are interpreted in a vertex shader and/or vertex buffer.
		/// </summary>
		public GorgonInputLayout InputLayout
		{
			get
			{
				return _inputLayout;
			}
			set
			{
				if (_inputLayout == value)
				{
					return;
				}

				_inputLayout = value;
				PipelineStateChangeFlags |= PipelineStateChangeFlags.InputLayout;
			}
		}

		/// <summary>
		/// Property to set or return the current render target views and depth/stencil view for this state.
		/// </summary>
		public GorgonRenderTargetViews RenderTargetViews
		{
			get
			{
				return _renderTargetViews;
			}
			set
			{
				if (_renderTargetViews == value)
				{
					return;
				}

				_renderTargetViews = value;
				PipelineStateChangeFlags |= PipelineStateChangeFlags.RenderTargetViews;
			}
		}

		/// <summary>
		/// Property to set or return the primitive topology to use when rendering.
		/// </summary>
		public D3D.PrimitiveTopology PrimitiveTopology
		{
			get
			{
				return _primitiveTopology;
			}
			set
			{
				if (_primitiveTopology == value)
				{
					return;
				}

				_primitiveTopology = value;
				PipelineStateChangeFlags |= PipelineStateChangeFlags.PrimitiveTopology;
			}
		}
		#endregion
	}
}
