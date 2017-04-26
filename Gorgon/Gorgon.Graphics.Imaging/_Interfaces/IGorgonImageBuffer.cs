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
// Created: June 29, 2016 9:43:51 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Native;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Imaging
{
	/// <summary>
	/// An image buffer containing data about a part of a <see cref="IGorgonImage"/>.
	/// </summary>
	public interface IGorgonImageBuffer
	{
		/// <summary>
		/// Property to return the format of the buffer.
		/// </summary>
		DXGI.Format Format
		{
			get;
		}

		/// <summary>
		/// Property to return the width for the current buffer.
		/// </summary>
		int Width
		{
			get;
		}

		/// <summary>
		/// Property to return the height for the current buffer.
		/// </summary>
		/// <remarks>This is only valid for 2D and 3D images.</remarks>
		int Height
		{
			get;
		}

		/// <summary>
		/// Property to return the depth for the current buffer.
		/// </summary>
		/// <remarks>This is only valid for 3D images.</remarks>
		int Depth
		{
			get;
		}

		/// <summary>
		/// Property to return the mip map level this buffer represents.
		/// </summary>
		int MipLevel
		{
			get;
		}

		/// <summary>
		/// Property to return the array this buffer represents.
		/// </summary>
		/// <remarks>For 3D images, this will always be 0.</remarks>
		int ArrayIndex
		{
			get;
		}

		/// <summary>
		/// Property to return the depth slice index.
		/// </summary>
		/// <remarks>For 1D or 2D images, this will always be 0.</remarks>
		int DepthSliceIndex
		{
			get;
		}

		/// <summary>
		/// Property to return the data stream for the image data.
		/// </summary>
		IGorgonPointer Data
		{
			get;
		}

		/// <summary>
		/// Property to return information about the pitch of the data for this buffer.
		/// </summary>
		GorgonPitchLayout PitchInformation
		{
			get;
		}

		/// <summary>
		/// Function to copy the image buffer data from this buffer into another.
		/// </summary>
		/// <param name="buffer">The buffer to copy into.</param>
		/// <param name="sourceRegion">[Optional] The region in the source to copy.</param>
		/// <param name="destX">[Optional] Horizontal offset in the destination buffer.</param>
		/// <param name="destY">[Optional] Vertical offset in the destination buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer" /> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="buffer" /> is not the same format as this buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the source region does not fit within the bounds of this buffer.</exception>
		/// <remarks>
		/// <para>
		/// This method will copy the contents of this buffer into another buffer and will provide clipping to handle cases where the buffer or <paramref name="sourceRegion" /> is mismatched with the 
		/// destination size. If this buffer, and the buffer passed to <paramref name="buffer"/> share the same pointer address, then this method will return immediately without making any changes.
		/// </para>
		/// <para>
		/// Users may define an area on this buffer to copy by specifying the <paramref name="sourceRegion" /> parameter. If <b>null</b> is passed to this parameter, then the entire buffer will be copied 
		/// to the destination.
		/// </para>
		/// <para>
		/// An offset into the destination buffer may also be specified to relocate the data copied from this buffer into the destination.  Clipping will be applied if the offset pushes the source data 
		/// outside of the boundaries of the destination buffer.
		/// </para>
		/// <para>
		/// The destination buffer must be the same format as the source buffer.  If it is not, then an exception will be thrown.
		/// </para>
		/// </remarks>
		void CopyTo(IGorgonImageBuffer buffer, DX.Rectangle? sourceRegion = null, int destX = 0, int destY = 0);
	}
}