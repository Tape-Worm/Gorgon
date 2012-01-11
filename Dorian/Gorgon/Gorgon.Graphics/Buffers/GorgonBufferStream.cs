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
// Created: Tuesday, January 10, 2012 10:40:41 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DX = SharpDX;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A stream created by a buffer when it is locked for reading/writing.
	/// </summary>
	/// <typeparam name="T">Type of buffer that is locked.</typeparam>	
	public class GorgonBufferStream<T>
		: GorgonDataStream
		where T : GorgonBaseBuffer
	{
		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the buffer was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the base DirectX stream.
		/// </summary>
		internal DX.DataStream BaseStream
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer is persistent or not.
		/// </summary>
		/// <remarks>When this property is TRUE, the backing buffer will not be destroyed when <see cref="M:Dispose"/> is called.  That will be handled by the 
		/// object that created this stream.  If this property is FALSE, then the backing store for the buffer will be destroyed.</remarks>
		public bool IsPersistent
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the constant buffer that owns this stream.
		/// </summary>
		public T Buffer
		{
			get;
			private set;
		}
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
					if (Buffer.IsLocked)
						Buffer.UnlockBuffer();

					if ((BaseStream != null) && (!IsPersistent))
					{
						BaseStream.Dispose();
						BaseStream = null;
					}

					// Destroy the backing store buffer if it is not persistent.
					if (!IsPersistent)
					{
						base.Dispose(true);
						_disposed = true;
					}
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferStream&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="buffer">Buffer that owns this stream.</param>
		/// <param name="baseStream">Base DirectX stream.</param>
		internal GorgonBufferStream(T buffer, DX.DataStream baseStream)
			: base(baseStream.DataPointer, (int)baseStream.Length)
		{
			StreamStatus = StreamStatus.WriteOnly;
			BaseStream = baseStream;
			Buffer = buffer;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferStream&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="buffer">Buffer that owns this stream.</param>
		/// <param name="size">Size of the buffer, in bytes.</param>
		internal GorgonBufferStream(T buffer, int size)
			: base(size)
		{
			StreamStatus = StreamStatus.WriteOnly;
			BaseStream = null;
			Buffer = buffer;
		}
		#endregion
	}
}
