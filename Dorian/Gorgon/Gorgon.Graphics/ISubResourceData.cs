#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, March 13, 2012 1:30:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Data to pass in to sub resources.
	/// </summary>
	public interface ISubResourceData
	{
		/// <summary>
		/// Property to return size, in bytes, of the data being passed to the sub resource.
		/// </summary>
		int Size
		{
			get;
		}

		/// <summary>
		/// Property to return the number of bytes between each line in a texture.
		/// </summary>
		/// <remarks>This is only used for 2D and 3D textures and has no meaning elsewhere.</remarks>
		int RowPitch
		{
			get;
		}

		/// <summary>
		/// Property to return the number of bytes between each depth slice in a 3D texture.
		/// </summary>
		/// <remarks>This is only used for 3D textures and has no meaning elsewhere.</remarks>
		int SlicePitch
		{
			get;
		}

		/// <summary>
		/// Property to return the data to pass to a resource (like a texture).
		/// </summary>
		GorgonDataStream Data
		{
			get;
		}


	}
}
