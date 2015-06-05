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
// Created: Monday, June 27, 2011 8:57:11 AM
// 
#endregion

using System;
using System.IO;
using System.Text;
using Gorgon.Core.Properties;
using Gorgon.Native;

namespace Gorgon.IO
{
	/// <summary>
	/// An extended binary writer class.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This object extends the functionality of the <see cref="BinaryWriter"/> type by adding extra functions to write to a pointer (or <see cref="IntPtr"/>), and to generic value types.
	/// </para>
	/// </remarks>
	public class GorgonBinaryWriter
		: BinaryWriter
	{
		#region Variables.
		private byte[] _tempBuffer;		// Temporary buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to keep the underlying stream open or not after the writer is closed.
		/// </summary>
		public bool KeepStreamOpen
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to write the bytes pointed at by the pointer into the stream.
		/// </summary>
		/// <param name="pointer">Pointer to the buffer containing the data.</param>
		/// <param name="size">Number of bytes to write.</param>
		/// <remarks>This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.</remarks>
		public unsafe void Write(IntPtr pointer, int size)
		{
			Write(pointer.ToPointer(), size);
		}

		/// <summary>
		/// Function to write the bytes pointed at by the pointer into the stream.
		/// </summary>
		/// <param name="pointer">Pointer to the buffer containing the data.</param>
		/// <param name="size">Number of bytes to write.</param>
		/// <remarks>This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.</remarks>
		public unsafe void Write(void* pointer, int size)
		{
			if ((pointer == null) || (size < 1))
			{
				return;
			}

			var data = (byte*)pointer;
			while (size > 0)
			{
				if (size >= 8)
				{
					Write(*((long*)data));
					size -= 8;
					data += 8;
				}
				else if (size >= 4)
				{
					Write(*((int*)data));
					size -= 4;
					data += 4;
				}
				else if (size >= 2)
				{
					Write(*((short*)data));
					size -= 2;
					data += 2;
				}
				else
				{
					Write(*data);
					size--;
					data++;
				}
			}
		}

		/// <summary>
		/// Function to write a generic value to the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public unsafe void WriteValue<T>(T value)
			where T : struct
		{
			int size = DirectAccess.SizeOf<T>();
			byte* pointer = stackalloc byte[size];

			DirectAccess.WriteValue(pointer, ref value);

			while (size > 0)
			{
				if (size >= 8)
				{					
					Write(*((long*)pointer));
					pointer += 8;
					size -= 8;
				}
				else if (size >= 4)
				{
					Write(*((int*)pointer));
					pointer += 4;
					size -= 4;
				}
				else if (size >= 2)
				{
					Write(*((short*)pointer));
					pointer += 2;
					size -= 2;
				}
				else
				{
					Write(*pointer);
					pointer++;
					size--;
				}
			}
		}

		/// <summary>
		/// Function to write a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to write.</param>
		/// <param name="startIndex">Starting index in the array.</param>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
		/// <para>-or-</para>
		/// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public unsafe void WriteRange<T>(T[] value, int startIndex, int count)
			where T : struct
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			if ((value.Length == 0) || (count <= 0))
			{
				return;
			}

			if (startIndex < 0)
			{
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_VALUE_IS_LESS_THAN, startIndex, 0));
			}

			if (startIndex >= value.Length)
			{
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_VALUE_IS_GREATER_THAN, startIndex, value.Length));
			}

			if (startIndex + count > value.Length)
			{
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_VALUE_IS_LESS_THAN, startIndex + count, value.Length));
			}

			int typeSize = DirectAccess.SizeOf<T>();
			int offset = typeSize * startIndex;
			int size = typeSize * count;

			// Allocate our temporary buffer if we haven't already.
			if (_tempBuffer == null)
			{
				_tempBuffer = new byte[GorgonChunkedFormat.TempBufferSize];
			}

			fixed (byte* tempBufferPointer = &_tempBuffer[0])
			{
				while (size > 0)
				{
					int blockSize = size > GorgonChunkedFormat.TempBufferSize ? GorgonChunkedFormat.TempBufferSize : size;

					// Read our array into our temporary byte buffer.
					DirectAccess.ReadArray(tempBufferPointer, value, offset, blockSize);

					offset += blockSize;
					size -= size;

					// Write the temporary byte buffer to the stream.
					Write(_tempBuffer, 0, blockSize);
				}
			}
		}

		/// <summary>
		/// Function to write a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to write.</param>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="count"/> parameter is greater than the number of elements in the value parameter.
		/// </exception>
		public void WriteRange<T>(T[] value, int count)
			where T : struct
		{
			WriteRange(value, 0, count);
		}

		/// <summary>
		/// Functio to write a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to write.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public void WriteRange<T>(T[] value)
			where T : struct
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			WriteRange(value, 0, value.Length);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBinaryWriter"/> class.
		/// </summary>
		/// <param name="output">Output stream.</param>
		/// <param name="encoder">Encoding for the binary writer.</param>
		/// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
		public GorgonBinaryWriter(Stream output, Encoding encoder, bool keepStreamOpen = false)
			: base(output, encoder, keepStreamOpen)
		{
			KeepStreamOpen = keepStreamOpen;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBinaryWriter"/> class.
		/// </summary>
		/// <param name="output">Output stream.</param>
		/// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
		public GorgonBinaryWriter(Stream output, bool keepStreamOpen = false)
			: this(output, Encoding.UTF8, keepStreamOpen)
		{
		}
		#endregion
	}
}
