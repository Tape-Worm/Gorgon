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
// Created: June 8, 2016 2:04:35 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A pipeline state used to determine the state of a scene when rendering.
	/// </summary>
	class PipelineState 
		: IGorgonPipelineState
	{
		#region Variables.
		/// <summary>
		/// The default pipeline state.
		/// </summary>
		public static readonly IGorgonPipelineState Default = new PipelineState(int.MinValue, new GorgonPipelineStateInfo());
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the pipeline state info for this pipeline state.
		/// </summary>
		public GorgonPipelineStateInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the ID of this state object.
		/// </summary>
		public int ID
		{
			get;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="PipelineState"/> class.
		/// </summary>
		/// <param name="id">The ID of the pipeline state.</param>
		/// <param name="stateInfo">The state information.</param>
		public PipelineState(int id, GorgonPipelineStateInfo stateInfo)
		{
			ID = id;
			Info = stateInfo;
		}
		#endregion
		
	}
}
