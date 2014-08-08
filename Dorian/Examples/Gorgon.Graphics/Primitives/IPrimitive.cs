#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 11:07:25 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Example
{
	/// <summary>
	/// A primitive.
	/// </summary>
	interface IPrimitive
	{
		/// <summary>
		/// Property to return the number of vertices.
		/// </summary>
		int VertexCount
		{
			get;
		}

		/// <summary>
		/// Property to return the number of indices.
		/// </summary>
		int IndexCount
		{
			get;
		}

		/// <summary>
		/// Property to return the vertex buffer.
		/// </summary>
		GorgonVertexBuffer VertexBuffer
		{
			get;
		}

		/// <summary>
		/// Property to return the index buffer.
		/// </summary>
		GorgonIndexBuffer IndexBuffer
		{
			get;
		}
	}
}
