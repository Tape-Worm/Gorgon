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
// Created: Monday, June 27, 2011 8:56:28 AM
// 
#endregion

using System.Text;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// An extended binary writer class.
	/// </summary>
	/// <remarks>The <see cref="System.IO.BinaryReader">BinaryReader</see> object included with .NET automatically closes the underlying stream when the reader
	/// is closed.  This object was created to allow the user to decide when to close the underlying stream.</remarks>
	public class GorgonFileSystemBinaryReader
		: System.IO.BinaryReader
	{
		#region Variables.
		private bool _keepOpen = false;			// Flag to keep the underlying stream open.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to keep the underlying stream open or not after the reader is closed.
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
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.BinaryReader"/> class and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_keepOpen)
					this.BaseStream.Dispose();
			}

			// Force the dispose to -not- destroy the underlying stream.
			base.Dispose(false);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBinaryReaderEx"/> class.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="encoder">Encoding for the binary reader.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the writer is closed, FALSE to close when done.</param>
		public GorgonFileSystemBinaryReader(System.IO.Stream input, Encoding encoder, bool keepStreamOpen)
			: base(input, encoder)
		{
			_keepOpen = keepStreamOpen;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBinaryReaderEx"/> class.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the writer is closed, FALSE to close when done.</param>
		public GorgonFileSystemBinaryReader(System.IO.Stream input, bool keepStreamOpen)
			: this(input, Encoding.UTF8, keepStreamOpen)
		{
		}
		#endregion
	}
}
