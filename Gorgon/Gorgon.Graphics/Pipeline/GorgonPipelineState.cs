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
using System.Diagnostics;
using System.Linq;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

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
		/// The input layout has been modified.
		/// </summary>
		InputLayout = 0x1,
		/// <summary>
		/// The vertex shader state has been modified.
		/// </summary>
		VertexShader = 0x2,
		/// <summary>
		/// The pixel shader state has been modified.
		/// </summary>
		PixelShader = 0x4,
		/// <summary>
		/// The view port was modified.
		/// </summary>
		Viewport = 0x8,
		/// <summary>
		/// The rasterizer state was modified.
		/// </summary>
		RasterState = 0x10,
		/// <summary>
		/// The scissor rectangles have been updated.
		/// </summary>
		ScissorRectangles = 0x20,
		/// <summary>
		/// The depth/stencil state has been updated.
		/// </summary>
		DepthStencilState = 0x40,
		/// <summary>
		/// The blending state has been updated.
		/// </summary>
		BlendState = 0x80,
	}

	/// <summary>
	/// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
	/// </summary>
	public class GorgonPipelineState
	{
		#region Variables.
		// The default state flags.
		private static readonly PipelineStateChangeFlags _defaultStates;
		// Information used to create the pipeline state.
		private readonly GorgonPipelineStateInfo _info;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11 input layout.
		/// </summary>
		internal D3D11.InputLayout D3DInputLayout
		{
			get;
			set;
		}

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
		/// Property to return the DirectX scissor rectangles.
		/// </summary>
		internal RawRectangle[] DXScissorRectangles
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the Direct viewports.
		/// </summary>
		internal RawViewportF[] DXViewports
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
		}

		/// <summary>
		/// Property to return the <see cref="IGorgonPipelineStateInfo"/> used to create this object.
		/// </summary>
		public IGorgonPipelineStateInfo Info => _info;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare this pipeline state with another pipeline state.
		/// </summary>
		/// <param name="state">The state to compare.</param>
		/// <returns>The states that have been changed between this state and the other <paramref name="state"/>.</returns>
		public PipelineStateChangeFlags GetChanges(GorgonPipelineState state)
		{
			if (state == null)
			{
				return _defaultStates;
			}

			var pipelineFlags = PipelineStateChangeFlags.None;

			// Get main pipeline changes.
			if (D3DInputLayout != state.D3DInputLayout)
			{
				pipelineFlags |= PipelineStateChangeFlags.InputLayout;
			}

			if (_info.Viewports != state._info.Viewports)
			{
				int leftCount = _info.Viewports?.Length ?? 0;
				int rightCount = state._info.Viewports?.Length ?? 0;

				if ((leftCount != rightCount)
				    || (_info.Viewports == null)
				    || (state._info.Viewports == null))
				{
					pipelineFlags |= PipelineStateChangeFlags.Viewport;
				}
				else
				{
					for (int i = 0; i < leftCount; ++i)
					{
						if (_info.Viewports[i].Equals(ref state._info.Viewports[i]))
						{
							continue;
						}

						pipelineFlags |= PipelineStateChangeFlags.ScissorRectangles;
						break;
					}
				}
			}

			if (_info.ScissorRectangles != state._info.ScissorRectangles)
			{
				int leftCount = _info.ScissorRectangles?.Length ?? 0;
				int rightCount = state._info.ScissorRectangles?.Length ?? 0;

				if ((leftCount != rightCount)
					|| (_info.ScissorRectangles == null)
					|| (state._info.ScissorRectangles == null))
				{
					pipelineFlags |= PipelineStateChangeFlags.ScissorRectangles;
				}
				else
				{
					for (int i = 0; i < leftCount; ++i)
					{
						if (_info.ScissorRectangles[i].Equals(ref state._info.ScissorRectangles[i]))
						{
							continue;
						}

						pipelineFlags |= PipelineStateChangeFlags.ScissorRectangles;
						break;
					}
				}
			}

			if (_info.PixelShader != state._info.PixelShader)
			{
				pipelineFlags |= PipelineStateChangeFlags.PixelShader;
			}

			if (_info.VertexShader != state._info.VertexShader)
			{
				pipelineFlags |= PipelineStateChangeFlags.VertexShader;
			}

			if (D3DRasterState != state.D3DRasterState)
			{
				pipelineFlags |= PipelineStateChangeFlags.RasterState;
			}

			if (D3DDepthStencilState != state.D3DDepthStencilState)
			{
				pipelineFlags |= PipelineStateChangeFlags.DepthStencilState;
			}

			if (D3DBlendState != state.D3DBlendState)
			{
				pipelineFlags |= PipelineStateChangeFlags.BlendState;
			}


			return pipelineFlags;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineState" /> class.
		/// </summary>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> instance that is used to create the state data.</param>
		/// <param name="stateInfo">The <see cref="IGorgonPipelineStateInfo"/> used to create this state object.</param>
		/// <param name="id">The ID of the cache entry for this pipeline state.</param>
		internal GorgonPipelineState(GorgonGraphics graphics, IGorgonPipelineStateInfo stateInfo, int id)
		{
			_info = new GorgonPipelineStateInfo(stateInfo);
			ID = id;
		}

		/// <summary>
		/// Initializes static members of the <see cref="GorgonPipelineState"/> class.
		/// </summary>
		static GorgonPipelineState()
		{
			// Initialize our default flags for a null current 
			var pipelineFlags = (PipelineStateChangeFlags[])Enum.GetValues(typeof(PipelineStateChangeFlags));

			foreach (PipelineStateChangeFlags flag in pipelineFlags.Where(item => item != PipelineStateChangeFlags.None))
			{
				_defaultStates |= flag;
			}
		}
		#endregion
	}
}
