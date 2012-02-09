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
// Created: Wednesday, February 08, 2012 3:17:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// This is used to initialize a 2 dimensional texture.
	/// </summary>
	public struct GorgonTexture2DData
	{
		#region Variables.
		/// <summary>
		/// An array of data streams used to populate the texture.
		/// </summary>
		public GorgonDataStream[] Data;
		/// <summary>
		/// Number of bytes between each row in the texture.
		/// </summary>
		public int[] Pitch;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DData"/> struct.
		/// </summary>
		/// <param name="mipCount">The number of mip levels in the texture.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="mipCount"/> parameter is less than 0.</exception>
		public GorgonTexture2DData(int mipCount)
		{
			if (mipCount == 0)
				mipCount = 1;

			if (mipCount < 0)
				throw new ArgumentOutOfRangeException("mipCount", "Number must be greater than 0.");

			Data = new GorgonDataStream[mipCount];
			Pitch = new int[mipCount];
		}
		#endregion
	}

	/// <summary>
	/// This is used to initialize a 3 dimensional texture.
	/// </summary>
	public struct GorgonTexture3DData
	{
		#region Variables.
		/// <summary>
		/// An array of data streams used to populate the texture.
		/// </summary>
		public GorgonDataStream[] Data;
		/// <summary>
		/// Number of bytes between each row in the texture.
		/// </summary>
		public int[] Pitch;
		/// <summary>
		/// Number of bytes between each layer of depth in the texture.
		/// </summary>
		public int[] Slice;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3DData"/> struct.
		/// </summary>
		/// <param name="mipCount">The number of mip levels in the texture.</param>
		/// <param name="sliceCount">The number of depth slices in the texture.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="mipCount"/> or the <paramref name="sliceCount"/> parameters are less than 0.</exception>
		public GorgonTexture3DData(int mipCount, int sliceCount)
		{
			if (mipCount == 0)
				mipCount = 1;

			if (sliceCount == 0)
				sliceCount = 1;

			if (sliceCount < 0)
				throw new ArgumentOutOfRangeException("sliceCount", "Number must be greater than 0.");

			if (mipCount < 0)
				throw new ArgumentOutOfRangeException("mipCount", "Number must be greater than 0.");

			Data = new GorgonDataStream[mipCount];
			Pitch = new int[mipCount];
			Slice = new int[sliceCount];
		}
		#endregion
	}
}
