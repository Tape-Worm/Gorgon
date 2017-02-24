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
// Created: July 25, 2016 12:40:16 AM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Provides the necessary information required to set up a index buffer.
	/// </summary>
	public class GorgonIndexBufferInfo
		: IGorgonIndexBufferInfo
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the intended usage for binding to the GPU.
		/// </summary>
		public D3D11.ResourceUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of indices to store.
		/// </summary>
		/// <remarks>
		/// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
		/// </remarks>
		public int IndexCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use 16 bit values for indices.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Specifying 16 bit indices can improve performance.
		/// </para>
		/// <para>
		/// The default value is <b>true</b>.
		/// </para>
		/// </remarks>
		public bool Use16BitIndices
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonIndexBufferInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonIndexBufferInfo"/> to copy settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonIndexBufferInfo(IGorgonIndexBufferInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			Usage = info.Usage;
			Use16BitIndices = info.Use16BitIndices;
			IndexCount = info.IndexCount;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonIndexBufferInfo"/> class.
		/// </summary>
		public GorgonIndexBufferInfo()
		{
			Usage = D3D11.ResourceUsage.Default;
			Use16BitIndices = true;
		}
		#endregion
	}
}
