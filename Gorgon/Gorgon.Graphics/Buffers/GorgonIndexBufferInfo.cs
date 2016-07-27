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

using Gorgon.Core;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides the necessary information required to set up a index buffer.
	/// </summary>
	public class GorgonIndexBufferInfo
		: IGorgonCloneable<GorgonIndexBufferInfo>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the intend
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

		#region Methods.
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>The cloned object.</returns>
		public GorgonIndexBufferInfo Clone()
		{
			return new GorgonIndexBufferInfo
			       {
				       Usage = Usage,
					   IndexCount = IndexCount,
					   Use16BitIndices = Use16BitIndices
			       };
		}
		#endregion

		#region Constructor/Finalizer.
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
