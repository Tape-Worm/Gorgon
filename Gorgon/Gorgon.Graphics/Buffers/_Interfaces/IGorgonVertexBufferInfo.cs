﻿#region MIT
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
// Created: July 28, 2016 10:43:47 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides the necessary information required to set up a vertex buffer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides an immutable view of the vertex buffer information so that it cannot be modified after the buffer is created.
	/// </para>
	/// </remarks>
	public interface IGorgonVertexBufferInfo 
	{
		/// <summary>
		/// Property to return the intended usage for binding to the GPU.
		/// </summary>
		D3D11.ResourceUsage Usage
		{
			get;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		/// <remarks>
		/// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
		/// </remarks>
		int SizeInBytes
		{
			get;
		}
	}
}