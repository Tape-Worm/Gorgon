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
// Created: December 19, 2016 11:26:40 AM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// A basic draw call that submits data from a vertex buffer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A draw call is used to submit vertex (and potentially index and instance) data to the GPU pipeline or an output buffer. This type will contain all the necessary information used to set the state of the 
	/// pipeline prior to rendering any data.
	/// </para>
	/// <para>
	/// This type does not support instancing or index buffers.
	/// </para>
	/// </remarks>
	public class GorgonDrawCall
		: GorgonDrawCallBase
	{
		/// <summary>
		/// Property to set or return the vertex number to start at within the vertex buffer.
		/// </summary>
		public int VertexStartIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of vertices to draw.
		/// </summary>
		public int VertexCount
		{
			get;
			set;
		}
	}
}
