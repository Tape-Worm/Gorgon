#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, September 15, 2011 9:53:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using GorgonLibrary.Data;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A D3D9 wrapper for the data stream.
	/// </summary>
	class D3D9DataStream
		: GorgonDataStream
	{
		#region Variables.
		private DataStream _d3dstream = null;		// D3D 9 data stream.
		private bool _disposed = false;				// Flag to indicate that the stream is disposed.
		#endregion

		#region Methods.
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{					
					if (_d3dstream != null)
						_d3dstream.Dispose();
				}

				_d3dstream = null;
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9DataStream"/> class.
		/// </summary>
		/// <param name="stream">The stream to wrap.</param>
		/// <param name="status">Status for the stream.</param>
		public D3D9DataStream(DataStream stream)
			: base(stream.DataPointer, (int)stream.Length)
		{
			if ((stream.CanWrite) && (!stream.CanRead))
			{
				StreamStatus = Data.StreamStatus.WriteOnly;
			}
			else if ((stream.CanRead) && (!stream.CanWrite))
			{
				StreamStatus = Data.StreamStatus.ReadOnly;
			}			

			_d3dstream = stream;
		}
		#endregion
	}
}
