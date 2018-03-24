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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core.Properties;

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
		// The size of the temporary buffer used to stream data in.
		private int _bufferSize = 65536;
		// Temporary buffer.
		private byte[] _tempBuffer;			
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the size of the buffer, in bytes, used to stream the data in.
		/// </summary>
		/// <remarks>
		/// This value is meant to help in buffering data from the data source if the data is large. It will only accept a value between 128 and 81920.  The upper bound is 
		/// to ensure that the temporary buffer is not pushed into the Large Object Heap.
		/// </remarks>
		public int BufferSize
		{
			get => _bufferSize;
			set
			{
				if (value < 128)
				{
					value = 128;
				}

				if (value > 81920)
				{
					value = 81920;
				}

				_bufferSize = value;

				// If we've previously allocated the buffer, then resize it.
				if (_tempBuffer != null)
				{
					_tempBuffer = new byte[_bufferSize];
				}
			}
		}

		/// <summary>
		/// Property to return whether to keep the underlying stream open or not after the reader is closed.
		/// </summary>
		public bool KeepStreamOpen
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read bytes from a stream into a buffer pointed at by the pointer.
		/// </summary>
		/// <param name="pointer">Pointer to the buffer to fill with data.</param>
		/// <param name="size">Number of bytes to read.</param>
		/// <remarks>
		/// <para>
		/// This method will read the number of bytes specified by the <paramref name="size"/> parameter into the <see cref="IntPtr"/> representing a memory pointer.
		/// </para>
		/// <note type="caution">
		/// This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.
		/// </note>
		/// </remarks>
		public unsafe void Read(IntPtr pointer, int size)
		{
			Read(pointer.ToPointer(), size);
		}

		/// <summary>
		/// Function to read bytes from a stream into a buffer pointed at by the pointer.
		/// </summary>
		/// <param name="pointer">Pointer to the buffer to fill with data.</param>
		/// <param name="size">Number of bytes to read.</param>
		/// <remarks>
		/// <para>
		/// This method will read the number of bytes specified by the <paramref name="size"/> parameter into memory pointed at by the raw <paramref name="pointer"/>.
		/// </para>
		/// <note type="caution">
		/// This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.
		/// </note>
		/// </remarks>
		public unsafe void Read(void* pointer, int size)
		{
			if ((pointer == null) || (size < 1))
			{
				return;
			}

		    var bytePtr = (byte*)pointer;
            
			while (size > 0)
			{
				if (size >= sizeof(long))
				{
				    *((long*)bytePtr) = ReadInt64();
				    bytePtr += sizeof(long);
					size -= sizeof(long);
				}
				
			    if (size >= sizeof(int))
				{
				    *((int*)bytePtr) = ReadInt32();
				    bytePtr += sizeof(int);
				    size -= sizeof(int);
				}

				if (size >= sizeof(short))
				{
				    *((short*)bytePtr) = ReadInt16();
				    bytePtr += sizeof(short);
				    size -= sizeof(short);
				}

			    if (size <= 0)
			    {
			        return;
			    }

			    *bytePtr = ReadByte();
			    ++bytePtr;
			    --size;
			}
		}

		/// <summary>
		/// Function to read a generic value from the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value or primitive type.</typeparam>
		/// <returns>The value in the stream.</returns>
		/// <remarks>
		/// <para>
		/// This method will read the data from the binary stream into a value of type <typeparamref name="T"/>, and return that value.
		/// </para>
		/// <note type="important">
		/// <para>
		/// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
		/// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
		/// </para>
		/// <para>
		/// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
		/// </para>
		/// </note>
		/// </remarks>
		public T ReadValue<T>()
			where T : struct
		{
		    ReadValue(out T result);

		    return result;
		}

	    /// <summary>
	    /// Function to read a generic value from the stream.
	    /// </summary>
	    /// <typeparam name="T">Type of value to read.  Must be a value or primitive type.</typeparam>
	    /// <param name="result">The value from the stream.</param>
	    /// <returns>The value in the stream.</returns>
	    /// <remarks>
	    /// <para>
	    /// This method will read the data from the binary stream into a value of type <typeparamref name="T"/>, and return that value.
	    /// </para>
	    /// <note type="important">
	    /// <para>
	    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
	    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
	    /// </para>
	    /// <para>
	    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
	    /// </para>
	    /// </note>
	    /// </remarks>
	    public void ReadValue<T>(out T result)
	        where T : struct
	    {
	        result = default;
	        ref byte resultRef = ref Unsafe.As<T, byte>(ref result);
	        Read(ref resultRef, Unsafe.SizeOf<T>());
	    }

	    /// <summary>
	    /// Function to read the bytes stored in the stream into the provided reference location.
	    /// </summary>
	    /// <param name="destination">The reference location to copy data into.</param>
	    /// <param name="size">The number of bytes to copy.</param>
	    /// <remarks>
	    /// <para>
	    /// This method will read the number of bytes specified by the <paramref name="size"/> parameter from the stream and copy it into the referenced location specified in <paramref name="destination"/>.
	    /// </para>
	    /// </remarks>
	    public void Read(ref byte destination, int size)
	    {
	        if (size < 1)
	        {
	            return;
	        }

            int offset = 0;

	        while (size > 0)
	        {
	            if (size >= sizeof(long))
	            {
	                ref long longRef = ref Unsafe.As<byte, long>(ref Unsafe.Add(ref destination, offset));
	                longRef = ReadInt64();
	                offset += sizeof(long);
	                size -= sizeof(long);
	            }

	            if (size >= sizeof(int))
	            {
	                ref int intRef = ref Unsafe.As<byte, int>(ref Unsafe.Add(ref destination, offset));
	                intRef = ReadInt32();
	                offset += sizeof(int);
	                size -= sizeof(int);
	            }

	            if (size >= sizeof(short))
	            {
	                ref short shortRef = ref Unsafe.As<byte, short>(ref Unsafe.Add(ref destination, offset));
	                shortRef = ReadInt16();
	                offset += sizeof(short);
	                size -= sizeof(short);
	            }

	            if (size <= 0)
	            {
	                return;
	            }

	            ref byte byteRef = ref Unsafe.Add(ref destination, offset);
	            byteRef = ReadByte();
	            offset += sizeof(byte);
	            size -= sizeof(byte);
	        }
	    }

        /// <summary>
        /// Function to read a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to read.  Must be a value or primitive type.</typeparam>
        /// <param name="value">Array of values to read.</param>
        /// <param name="startIndex">[Optional] Starting index in the array.</param>
        /// <param name="count">[Optional] Number of array elements to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
        /// </exception>
        /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
        /// <remarks>
        /// <para>
        /// This will read data from the binary stream into the specified array of values of type <typeparamref name="T"/>. The values will be populated starting at the <paramref name="startIndex"/> up to 
        /// the <paramref name="count"/> specified. If the <paramref name="count"/> is not specified (i.e. it is <b>null</b>), then the entire array minus the <paramref name="startIndex"/> will be used.
        /// </para>
        /// <note type="important">
        /// <para>
        /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
        /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
        /// </para>
        /// <para>
        /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
        /// </para>
        /// </note>
        /// </remarks>
        public void ReadRange<T>(T[] value, int startIndex = 0, int? count = null)
			where T : struct
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

		    if (count == null)
		    {
		        count = value.Length - startIndex;
		    }
           
			if (startIndex < 0)
			{
			    throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex, 0));
			}

			if (startIndex >= value.Length)
			{
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, startIndex, value.Length));
			}

		    if ((value.Length == 0) || (count <= 0))
		    {
		        return;
		    }

			if (startIndex + count > value.Length)
			{
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex + count, value.Length));
			}

			if (_tempBuffer == null)
			{
				_tempBuffer = new byte[_bufferSize];
			}

		    int typeSize = Unsafe.SizeOf<T>();
		    int blockSize = _tempBuffer.Length;
		    int size = typeSize * count.Value;
		    int offset = 0;
		    ref byte valueRef = ref Unsafe.As<T, byte>(ref value[startIndex]);
		    ref byte bufferRef = ref _tempBuffer[0];

		    while (size > 0)
		    {
		        if (blockSize > size)
		        {
		            blockSize = size;
		        }

		        ref byte destRef = ref Unsafe.Add(ref valueRef, offset);
                    
		        // Read the data from the stream as byte values.
		        Read(_tempBuffer, 0, blockSize);
                Unsafe.CopyBlock(ref destRef, ref bufferRef, (uint)blockSize);

		        size -= blockSize;
		        offset += blockSize;
		    }
		}

        /// <summary>
        /// Function to read a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to read.  Must be a value or primitive type.</typeparam>
        /// <param name="count">Number of array elements to copy.</param>
        /// <returns>An array filled with values of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method will read the specified <paramref name="count"/> of values of type <typeparamref name="T"/> into an array from a binary data stream and return that array.
        /// </para>
        /// <note type="important">
        /// <para>
        /// The return value for this type will always create a new array of type <typeparamref name="T"/>. This may be inefficient depending on the use case.
        /// </para>
        /// </note>
        /// <note type="important">
        /// <para> 
        /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
        /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
        /// </para>
        /// <para>
        /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
        /// </para>
        /// </note>
        /// </remarks>
        /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
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
