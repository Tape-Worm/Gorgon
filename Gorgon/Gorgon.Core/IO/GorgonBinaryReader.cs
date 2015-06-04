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

using System;
using System.IO;
using System.Text;
using Gorgon.Core.Properties;
using Gorgon.Native;

namespace Gorgon.IO
{
	/// <summary>
	/// An extended binary reader class.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This object extends the functionality of the <see cref="BinaryReader"/> type by adding extra functions to read from a pointer (or <see cref="IntPtr"/>), and from generic value types.
	/// </para>
	/// </remarks>
	public class GorgonBinaryReader
		: BinaryReader
	{
		#region Variables.
		private byte[] _tempBuffer;			// Temporary buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to keep the underlying stream open or not after the reader is closed.
		/// </summary>
		public bool KeepStreamOpen
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read bytes from a stream into a buffer pointed at by the pointer.
		/// </summary>
		/// <param name="pointer">Pointer to the buffer to fill with data.</param>
		/// <param name="size">Number of bytes to read.</param>
		/// <remarks>This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.</remarks>
		public unsafe void Read(IntPtr pointer, int size)
		{
			Read(pointer.ToPointer(), size);
		}

		/// <summary>
		/// Function to read bytes from a stream into a buffer pointed at by the pointer.
		/// </summary>
		/// <param name="pointer">Pointer to the buffer to fill with data.</param>
		/// <param name="size">Number of bytes to read.</param>
		/// <remarks>This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.</remarks>
		public unsafe void Read(void* pointer, int size)
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
					*((long*)data) = ReadInt64();
					size -= 8;
					data += 8;
				}
				else if (size >= 4)
				{
					*((int*)data) = ReadInt32();
					size -= 4;
					data += 4;
				}
				else if (size >= 2)
				{
					*((short*)data) = ReadInt16();
					size -= 2;
					data += 2;
				}
				else
				{
					*data = ReadByte();
					size--;
					data++;
				}
			}
		}

		/// <summary>
		/// Function to read a generic value from the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <returns>The value in the stream.</returns>
		public unsafe T ReadValue<T>()
			where T : struct
		{
			T returnVal;
			int size = DirectAccess.SizeOf<T>();
			byte* pointer = stackalloc byte[size];
			byte* bytes = pointer;

			while (size > 0)
			{
				if (size >= 8)
				{
					*((long*)bytes) = ReadInt64();
					bytes += 8;
					size -= 8;
				}
				else if (size >= 4)
				{
					*((int*)bytes) = ReadInt32();
					bytes += 4;
					size -= 4;
				}
				else if (size >= 2)
				{
					*((short*)bytes) = ReadInt16();
					bytes += 2;
					size -= 2;
				}
				else
				{
					*bytes = ReadByte();
					bytes++;
					size--;
				}
			}

			DirectAccess.ReadValue(pointer, out returnVal);

			return returnVal;
		}

		/// <summary>
		/// Function to read a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to read.</param>
		/// <param name="startIndex">Starting index in the array.</param>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
		/// <para>-or-</para>
		/// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public unsafe void ReadRange<T>(T[] value, int startIndex, int count)
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
			int size = typeSize * count;
			int offset = startIndex * typeSize;

			if (_tempBuffer == null)
			{
				_tempBuffer = new byte[GorgonChunkedFormat.TempBufferSize];
			}

			fixed (byte* tempBufferPointer = &_tempBuffer[0])
			{
				while (size > 0)
				{
					int blockSize = size > GorgonChunkedFormat.TempBufferSize ? GorgonChunkedFormat.TempBufferSize : size;

					// Read the data from the stream as byte values.
					Read(_tempBuffer, 0, blockSize);

					// Copy into our array.
					DirectAccess.ReadArray(tempBufferPointer, value, offset, blockSize);

					offset += blockSize;
					size -= blockSize;
				}
			}
		}

		/// <summary>
		/// Function to read a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to read.</param>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is greater than the number of elements in the value parameter.
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public void ReadRange<T>(T[] value, int count)
			where T : struct
		{
			ReadRange(value, 0, count);
		}

		/// <summary>
		/// Function to read a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to read.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public void ReadRange<T>(T[] value)
			where T : struct
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			ReadRange(value, 0, value.Length);
		}

		/// <summary>
		/// Function to read a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public T[] ReadRange<T>(int count)
			where T : struct
		{
			var array = new T[count];

			ReadRange(array, 0, count);

			return array;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBinaryReader"/> class.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="encoder">Encoding for the binary reader.</param>
		/// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
		public GorgonBinaryReader(Stream input, Encoding encoder, bool keepStreamOpen = false)
			: base(input, encoder, keepStreamOpen)
		{
			KeepStreamOpen = keepStreamOpen;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBinaryReader"/> class.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
		public GorgonBinaryReader(Stream input, bool keepStreamOpen = false)
			: this(input, Encoding.UTF8, keepStreamOpen)
		{
		}
		#endregion
	}
}
