#region MIT.
// 
// SharpUtils.
// Copyright (C) 2006 Michael Winsor
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
// Created: Sunday, October 22, 2006 1:54:40 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a stream writer that can keep its stream open after it's closed.
	/// </summary>
	public class StreamWriterEx
		: StreamWriter 
	{
		#region Variables.
		private bool _keepOpen = false;			// Flag to keep the underlying stream open.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to keep the underlying stream open or not after the writer is closed.
		/// </summary>
		public bool KeepStreamOpen
		{
			get
			{
				return _keepOpen;
			}
			set
			{
				_keepOpen = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to close the writer and its underlying stream if KeepStreamOpen is false.
		/// </summary>
		public override void Close()
		{
			try
			{
				Flush();
			}
			finally
			{
			}			
			Dispose(!_keepOpen);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="output">Output stream.</param>
		/// <param name="encoder">Encoding for the stream writer.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the writer is closed, FALSE for normal behaviour.</param>
		/// <param name="bufferSize">Sets the buffer size.</param>
		public StreamWriterEx(Stream output, Encoding encoder, bool keepStreamOpen, int bufferSize)
			: base(output, encoder, bufferSize)
		{
			_keepOpen = keepStreamOpen;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="output">Output stream.</param>
		/// <param name="encoder">Encoding for the stream writer.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the writer is closed, FALSE for normal behaviour.</param>		
		public StreamWriterEx(Stream output, Encoding encoder, bool keepStreamOpen)
			: this(output, encoder, keepStreamOpen, 1024)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="output">Output stream.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the writer is closed, FALSE for normal behaviour.</param>
		public StreamWriterEx(Stream output, bool keepStreamOpen)
			: this(output, Encoding.UTF8, keepStreamOpen, 1024)
		{
		}
		#endregion
	}
}
