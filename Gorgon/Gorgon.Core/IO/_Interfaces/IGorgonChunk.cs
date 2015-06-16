#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 11:18:42 AM
// 
#endregion

using System;

namespace Gorgon.IO
{
	/// <summary>
	/// A type describing a chunk within a chunked file format.
	/// </summary>
	public interface IGorgonChunk
	{
		/// <summary>
		/// Property to return the ID for this chunk.
		/// </summary>
		ulong ID
		{
			get;
		}

		/// <summary>
		/// Property to return the size of the chunk, in bytes.
		/// </summary>
		int Size
		{
			get;
		}

		/// <summary>
		/// Property to return the offset, in bytes, of the chunk within the chunked file.
		/// </summary>
		/// <remarks>
		/// This is relative to the header of the file.
		/// </remarks>
		Int64 FileOffset
		{
			get;
		}
	}
}
