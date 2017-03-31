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

using System.Collections.Generic;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A common class for draw calls.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A draw call is used to submit vertex (and potentially index and instance) data to the GPU pipeline or an output buffer. This type will contain all the necessary information used to set the state of the 
	/// pipeline prior to rendering any data.
	/// </para>
	/// </remarks>
	public abstract class GorgonDrawCallBase
	{
		/// <summary>
		/// Property to return the type of primitives to draw.
		/// </summary>
		public D3D.PrimitiveTopology PrimitiveTopology
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scissor rectangles to apply during this draw call.
		/// </summary>
		public IReadOnlyList<DX.Rectangle> ScissorRectangles
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the viewports to apply during this draw call.
		/// </summary>
		public IReadOnlyList<DX.ViewportF> Viewports
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return resources to use in the draw call.
		/// </summary>
		public GorgonPipelineResources Resources
		{
			get;
		} = new GorgonPipelineResources();

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

		/// <summary>
		/// Property to set or return the factor used to modulate the pixel shader, render target or both.
		/// </summary>
		/// <remarks>
		/// To use this value, ensure that the blend state was creating using <c>Factor</c> operation.
		/// </remarks>
		public GorgonColor BlendFactor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the mask used to define which samples get updated in the active render targets.
		/// </summary>
		public int BlendSampleMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth/stencil reference value used when performing a depth/stencil test.
		/// </summary>
		public int DepthStencilReference
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDrawCallBase"/> class.
		/// </summary>
		protected GorgonDrawCallBase()
		{
			unchecked
			{
				BlendSampleMask = (int)(0xffffffff);
				BlendFactor = new GorgonColor(1, 1, 1, 1);
			}
		}
	}
}
