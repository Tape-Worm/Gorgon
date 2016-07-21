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
// Created: July 19, 2016 7:10:17 PM
// 
#endregion

using System;
using Gorgon.Core.Memory;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An allocator used to create a pool of pipeline states.
	/// </summary>
	public class GorgonPipelineStateAllocator
		: GorgonLinearAllocatorPool<GorgonPipelineState>
	{
		#region Methods.
		/// <summary>
		/// Function to allocate a single pipeline state.
		/// </summary>
		/// <returns>A <see cref="GorgonPipelineState"/> object or <b>null</b> if the allocator is out of space.</returns>
		public GorgonPipelineState AllocateState()
		{
			int stateIndex = Allocate();
			
			if (stateIndex == -1)
			{
				return null;
			}

			GorgonPipelineState result = Items[stateIndex];
			result.Reset();

			return result;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPipelineStateAllocator"/> class.
		/// </summary>
		/// <param name="stateCount">The total number of states available in this allocator.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="stateCount"/> parameter is less than 1.</exception>
		public GorgonPipelineStateAllocator(int stateCount)
			: base(stateCount)
		{
			for (int i = 0; i < stateCount; ++i)
			{
				Items[i] = new GorgonPipelineState();
			}
		}
		#endregion
	}
}
