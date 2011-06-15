#region MIT.
// 
// Gorgon.
// Copyright (C) 2009 Michael Winsor
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
// Created: Tuesday, April 28, 2009 9:10:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A state fence is used mark where objects change state inside of a vertex buffer.
	/// </summary>
	/// <remarks>This object is used by the <see cref="GorgonLibrary.Graphics.Batch">Batch</see> object to determine how to switch state when drawing with the static vertex buffer.</remarks>
	public class StateFence
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the image for this fence.
		/// </summary>
		public Image Image
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the starting vertex for the fence.
		/// </summary>
		public int StartVertex
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertex count encompassed by the fence.
		/// </summary>
		public int VertexCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the starting index for the fence.
		/// </summary>
		public int StartIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the index count encompassed by the fence.
		/// </summary>
		public int IndexCount
		{
			get;
			set;
		}
		#endregion
	}
}
