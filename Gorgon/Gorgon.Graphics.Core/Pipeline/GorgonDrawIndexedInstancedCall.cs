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

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A draw call that submits data from a vertex buffer and uses indexing from an index buffer to reference the vertices.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A draw call is used to submit vertex (and potentially index and instance) data to the GPU pipeline or an output buffer. This type will contain all the necessary information used to set the state of the 
	/// pipeline prior to rendering any data.
	/// </para>
	/// <para>
	/// This type supports instancing which allows the GPU to draw the same item multiple times using an instance buffer.
	/// </para>
	/// </remarks>
	public class GorgonDrawIndexedInstancedCall
		: GorgonDrawCallBase
	{
		/// <summary>
		/// Property to set or return the starting index to of the buffer to draw.
		/// </summary>
		public int IndexStart
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of indices to draw per instance.
		/// </summary>
		public int IndexCountPerInstance
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertex in the vertex buffer to start drawing at.
		/// </summary>
		public int BaseVertexIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the instance number to start at within the instance buffer.
		/// </summary>
		public int StartInstanceIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of instances to draw.
		/// </summary>
		public int InstanceCount
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDrawIndexedInstancedCall"/> class.
		/// </summary>
		public GorgonDrawIndexedInstancedCall()
		{
			unchecked
			{
				BlendSampleMask = (int)(0xffffffff);
				BlendFactor = new GorgonColor(1, 1, 1, 1);
			}
		}
	}
}
