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
// Created: July 19, 2016 7:17:17 PM
// 
#endregion

using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A draw call to submit to the pipeline.
	/// </summary>
	public class GorgonDrawIndexedCall
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of primitives to draw.
		/// </summary>
		public D3D.PrimitiveTopology PrimitiveTopology => D3D.PrimitiveTopology.TriangleList;

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
		/// Property to set or return the starting index to of the buffer to draw.
		/// </summary>
		public int IndexStart

		{
			get;
			set;
		}
		/// <summary>
		/// Property to set or return the number of indices used to draw.
		/// </summary>
		public int IndexCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the vertex in the vertex buffer to start drawing at.
		/// </summary>
		public int BaseVertexIndex
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the draw call parameters.
		/// </summary>
		public void Reset()
		{
			State = null;
			BaseVertexIndex = 0;
			IndexCount = 0;
			IndexStart = 0;
		}
		#endregion
	}
}
