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
// Created: June 8, 2016 12:24:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A packet of state data to pass to a <see cref="GorgonGraphics"/> instance to create a new pipeline state object used to set up the pipeline for a scene.
	/// </summary>
	public class GorgonPipelineStateInfo
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the active render targets for the scene.
		/// </summary>
		public D3D11.RenderTargetView[] RenderTargets
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current depth/stencil view.
		/// </summary>
		public D3D11.DepthStencilView DepthStencilView
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.

		#endregion
	}
}
