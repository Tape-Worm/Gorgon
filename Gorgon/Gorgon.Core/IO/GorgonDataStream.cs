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
// Created: Thursday, September 15, 2011 2:31:53 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.Native;

namespace Gorgon.IO
{
	/// <summary>
	/// Access values for the <see cref="GorgonDataStream"/> object.
	/// </summary>
	public enum StreamAccess
	{
		/// <summary>
		/// The buffer can be read from and written to.
		/// </summary>
		ReadWrite = 0,
		/// <summary>
		/// The buffer can be read from, but not written to.
		/// </summary>
		ReadOnly = 1,
		/// <summary>
		/// The buffer can be written to, but not read from.
		/// </summary>
		WriteOnly = 2
	}

	/// <summary>
	/// A stream type for reading and writing binary data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This stream type is similar to the <see cref="System.IO.MemoryStream"/> object, except that MemoryStream uses an array of bytes as a backing store. This object uses native unmanaged memory to hold 
	/// its data, and accesses it through the use of either unsafe pointers or <see cref="IntPtr"/> for those .NET languages that don't support raw pointers (e.g. Visual Basic.NET).
	/// </para>
	/// <para>
	/// The object supports a multitude of reading/writing options for primitive data, string data, array data, and even generic types to the stream. For generic types being read or written, the type must 
	/// be decorated with a <see cref="StructLayoutAttribute"/> and a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> value. Otherwise, .NET may rearrange the members of the type 
	/// and the data being read/written will not serialize as expected. Also, for the <see cref="O:Gorgon.IO.GorgonDataStream.Read">Read</see>/<see cref="O:Gorgon.IO.GorgonDataStream.Write">Write</see> methods, 
	/// the generic type must not use any marshalling (<see cref="MarshalAsAttribute"/>) or reference types. Only primitive types and value types (structs) are allowed. 
	/// </para>
	/// <para>
	/// If data needs to be marshalled, then the object provides three members: <see cref="ReadMarshal{T}"/>, <see cref="WriteMarshal{T}"/> and <see cref="MarshalArrayToStream{T}"/> for marshalling reference types to 
	/// and from the stream. The rules about having the <see cref="StructLayoutAttribute"/> still apply, and additionally, a size, in bytes of the value should be supplied to the attribute along with the 
	/// <see cref="LayoutKind"/>. Because these methods may cause performance to suffer, it is recommended that data be written directly on a per-member basis instead of using these functions if performance is 
	/// absolutely critical.
	/// </para>
	/// <para>
	/// This object will also provide native access to an <see cref="Array"/> by pinning the array and returning a pointer to the handle on the first element via the <see cref="GCHandle"/> type. Because of 
	/// how .NET memory management reorders and compacts memory, it is recommended that any stream access to an <see cref="Array"/> be limited to very short durations or else memory fragmentation could 
	/// become an issue.
	/// </para>
	/// <para>
	/// Existing pointers to native memory can be used when constructing this stream type via the <see cref="GorgonDataStream(IntPtr, int)"/> (or using the constructor with the raw <c>void *</c> pointer). 
	/// The data stream will read and write from this pointer, but it will not take ownership of it, and thus will not free the memory associated with it when it is done. That responsibility is up to the 
	/// caller of the data stream.
	/// </para>
	/// <note type="important">
	/// <para>
	/// When using this stream to allocate a new block of memory, it is very important that <see cref="Dispose"/> method is called when finished using this object. Failure to do so can cause memory leaks 
	/// due to the inability of the garbage collector to handle the native memory allocated by this stream. The exception to this rule is when this stream is applied to an existing pointer. In those 
	/// cases, calling <c>Dispose</c> is not necessary as no memory has been allocated by the stream, and the pointer is not owned by the stream.
	/// </para>
	/// </note>
	/// <note type="caution">
	/// <para>
	/// Like most stream based types, it is <i>not</i> safe to use the same instance of this type from multiple threads. Doing so may cause performance degradation, or worse, memory corruption. If multi threading 
	/// is a possibility and native memory access is required, then the <see cref="DirectAccess"/> class provides less restricted (and less comprehensive) functionality for working with native memory 
	/// directly. Also, using a raw pointer (<c>void *</c> in C# or <see cref="IntPtr"/> and the <see cref="GorgonIntPtrExtensions"/> in languages that don't support raw pointers), is the best route to manipulating 
	/// native memory without the worry of concurrency problems that can plague a stream type.
	/// </para>
	/// </note>
	/// <note type="caution">
	/// <para>
	/// Gorgon will be phasing out it's internal usage of this type in favour of more direct access to memory. There may be new native types available for use in the future to supplement this type. This space 
	/// will be updated with alternatives as well as <c>See Also</c> references.
	/// </para>
	/// </note>
	/// </remarks>
	/// <seealso cref="DirectAccess"/>
	public unsafe class GorgonDataStream
		: Stream
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Pointer to the data held by the stream.
		private IntPtr _data = IntPtr.Zero;
		// Position in the buffer.
		private int _position;
		// Number of bytes in the buffer.
		private int _length;
		// Pointer offset.
		private IntPtr _pointerOffset = IntPtr.Zero;
		// Handle to a pinned array.
		private GCHandle _handle = default(GCHandle);
		// Flag to indicate that we own this pointer.
		private readonly bool _ownsPointer = true;
		// Pointer to the data in memory.
		//private void* _dataPointer = null; 
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the access level for the stream.
		/// </summary>
		public StreamAccess StreamAccess
		{
			get;
			private set;
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <returns><b>true</b> if the stream supports reading; otherwise, <b>false</b>.</returns>
		public override bool CanRead
		{
			get
			{
				return ((StreamAccess == StreamAccess.ReadOnly) || (StreamAccess == StreamAccess.ReadWrite));
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <returns>This property will always return <b>true</b> for this type.</returns>
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <returns><b>true</b> if the stream supports writing; otherwise, <b>false</b>.</returns>
		public override bool CanWrite
		{
			get
			{
				return ((StreamAccess == StreamAccess.WriteOnly) || (StreamAccess == StreamAccess.ReadWrite));
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <returns>A long value representing the length of the stream in bytes.</returns>
		public override long Length
		{
			get
			{
				return _length;
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		/// <returns>The current position within the stream.</returns>
		/// <remarks>
		/// <note type="important">
		/// To improve performance, exceptions will only be thrown for this property if the library is compiled for <b>DEBUG</b> mode.
		/// </note>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the value is less than zero or greater than <see cref="Int32.MaxValue"/>.</exception>
		public override long Position
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _position;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
#if DEBUG
				// Limit to the bounds of an integer.
				if ((value > Int32.MaxValue) || (value < 0))
				{
					throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, value, Int32.MaxValue));
				}
#endif

				_position = (int)value;
				_pointerOffset = _data + _position;
			}
		}

		/// <summary>
		/// Property to return the <see cref="IntPtr"/> which wraps the native pointer to the unmanaged memory wrapped by the stream.
		/// </summary>
		/// <remarks>
		/// This value returns an <see cref="IntPtr"/> that wraps the pointer that points at the starting address for the unmanaged memory wrapped by the stream. To get the pointer to the memory address 
		/// offset by the current <see cref="Position"/>, use the <see cref="PositionIntPtr"/> property.
		/// </remarks>
		public IntPtr BaseIntPtr
		{
			get
			{
				return _data;
			}
		}

		/// <summary>
		/// Property to return the native pointer to the unmanaged memory wrapped by the stream, and offset by the current <see cref="Position"/> in the stream.
		/// </summary>
		/// <remarks>
		/// This value returns the same value as the <see cref="BasePointer"/>, but offset by the number of bytes indicated by <see cref="Position"/>.
		/// </remarks>
		public void* PositionPointer
		{
			get
			{
				return _pointerOffset.ToPointer();
			}
		}

		/// <summary>
		/// Property to return the <see cref="IntPtr"/> which wraps the native pointer to the unmanaged memory wrapped by the stream, and offset by the current <see cref="Position"/> in the stream.
		/// </summary>
		/// <remarks>
		/// This value returns the same value as the <see cref="BaseIntPtr"/>, but offset by the number of bytes indicated by <see cref="Position"/>.
		/// </remarks>
		public IntPtr PositionIntPtr
		{
			get
			{
				return _pointerOffset;
			}
		}

		/// <summary>
		/// Property to return the native pointer to the unmanaged memory wrapped by the stream.
		/// </summary>
		/// <remarks>
		/// This value returns a pointer that points at the starting address for the unmanaged memory wrapped by the stream. To get the pointer to the memory address offset by the current 
		/// <see cref="Position"/>, use the <see cref="PositionPointer"/> property.
		/// </remarks>
		public void* BasePointer
		{
			get
			{
				return _data.ToPointer();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to allocate the buffer for the stream.
		/// </summary>
		/// <param name="value">Number of bytes in the buffer.</param>
		private void AllocateBuffer(long value)
		{
			if (!_ownsPointer)
			{
				throw new NotSupportedException(Resources.GOR_ERR_DATASTREAM_NOT_OWNER_NO_RESIZE);
			}

			if (_handle.IsAllocated)
			{
				throw new NotSupportedException(Resources.GOR_ERR_DATASTREAM_PINNED_NOT_RESIZABLE);
			}

			if (value < 0)
			{
				value = 0;
			}
			if (value > Int32.MaxValue)
			{
				value = Int32.MaxValue;
			}

			if (_data != IntPtr.Zero)
			{
				if (!_handle.IsAllocated)
				{
					Marshal.FreeHGlobal(_data);

					// Ensure that the garbage collector knows that we've dumped this memory.
					if (_length > 0)
					{
						GC.RemoveMemoryPressure(_length);
					}
				}
				else
				{
					_handle.Free();
				}
			}

			_data = IntPtr.Zero;
			_pointerOffset = IntPtr.Zero;
			_position = 0;
			_length = 0;

			if (value == 0)
			{
				return;
			}

			_data = Marshal.AllocHGlobal((int)value);

			// Ensure the garbage collector is aware of how big this object really is.
			// Failure to do this will make the GC think this object is only 10's of bytes in size, thus making it 
			// fail to take into account the memory bloat from the buffer when performing analysis for collection.
			// This ensures that the GC always knows the exact size of the stream at any given time.
			GC.AddMemoryPressure(value);
			_pointerOffset = _data;
			_length = (int)value;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose")]
		protected override void Dispose(bool disposing)
		{
			// We suppress the base call to Dispose because Stream's Dispose(bool) does nothing.

			if ((_disposed) || (!_ownsPointer) || ((_data == IntPtr.Zero) && (!_handle.IsAllocated)))
			{
				return;
			}

			// Destroy the buffer.
			if (!_handle.IsAllocated)
			{
				Marshal.FreeHGlobal(_data);

				if (_length > 0)
				{
					GC.RemoveMemoryPressure(_length);
					_length = 0;
				}
			}
			else
			{
				// Unpin any array that we've captured.
				_handle.Free();
			}

			_data = IntPtr.Zero;
			_pointerOffset = IntPtr.Zero;
			_disposed = true;
		}

		/// <summary>
		/// Function to determine if marshalling is required.
		/// </summary>
		/// <param name="type">Type to examine.</param>
		/// <returns><b>true</b> if the type requires marshalling, <b>false</b> if not.</returns>
		private static bool UseMarshalling(Type type)
		{
			return type.GetMembers()
			           .Any(item =>
			                {
				                if ((item.MemberType != MemberTypes.Property) && (item.MemberType != MemberTypes.Field))
				                {
					                return false;
				                }

				                Type memberType;
				                var fieldInfo = item as FieldInfo;

				                if (fieldInfo != null)
				                {
					                if ((fieldInfo.IsInitOnly) || (fieldInfo.IsStatic))
					                {
						                return false;
					                }

					                memberType = fieldInfo.FieldType;
				                }
				                else
				                {
					                var propInfo = (PropertyInfo)item;

					                if ((!propInfo.CanRead) || (!propInfo.CanWrite))
					                {
						                return false;
					                }

					                memberType = propInfo.PropertyType;
				                }

				                return Attribute.IsDefined(item, typeof(MarshalAsAttribute), true) || UseMarshalling(memberType);
			                });
		}

		/// <summary>
		/// Function to marshal an array of value types into a <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to marshal. Must be a value type.</typeparam>
		/// <param name="value">An array of values to copy into the stream.</param>
		/// <returns>A new <see cref="GorgonDataStream"/> containing the marshalled data.</returns>
		/// <remarks>
		/// <para>
		/// This method is used to serialize an array of values of the type <typeparamref name="T"/> as binary data into a <see cref="GorgonDataStream"/> object.
		/// </para>
		/// <para>
		/// Unlike the <see cref="O:Gorgon.IO.GorgonDataStream.WriteRange">WriteRange</see> methods, this method will accept types that use marshalled fields (i.e. fields that are decorated with the <see cref="MarshalAsAttribute"/>). 
		/// This allows for a greater flexibility when copying data into a binary data stream. If the type does not use marshalling, then the method will still copy the data as-is.
		/// </para>
		/// <para>
		/// Like the <see cref="O:Gorgon.IO.GorgonDataStream.WriteRange">WriteRange</see> methods, this method requires that the type <typeparamref name="T"/> be a value or primitive type, and be decorated with the <see cref="StructLayoutAttribute"/> 
		/// with a <see cref="StructLayoutAttribute.Value"/> of <see cref="LayoutKind.Sequential"/> (preferred), or <see cref="LayoutKind.Explicit"/>.
		/// </para>
		/// <note type="warning">
		/// <para>
		/// This method has poor performance. It is not recommended for use except in cases where marshalling <i>must</i> be used to transfer data (e.g. a <see cref="string"/> or <see cref="Array"/> needs copying to a C/C++ buffer). 
		/// </para>
		/// <para>
		/// For best performance, use only primitive types or value types (that, in turn, use only primitive/value types) within the value type and use the <see cref="O:Gorgon.IO.GorgonDataStream.WriteRange">WriteRange</see> methods.
		/// </para>
		/// </note>
		/// </remarks>
		/// <exception cref="System.ArgumentException">
		/// Thrown when the type of <paramref name="value"/> is not sequentially/explicitly laid out with the <see cref="StructLayoutAttribute"/>.
		/// </exception>
		/// <example>
		/// An example where an array of values will be copied into a new stream object:
		/// <code language="csharp">
		/// <![CDATA[
		/// [StructLayout(LayoutKind.Sequential)]
		/// public struct MyStruct
		/// {
		///		public int MyInt;
		///		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		///		public string MyStr;
		/// }
		/// 
		/// MyStruct[] items = new MyStruct[50];
		/// 
		/// for (int i = 0; i < items.Length; ++i)
		/// {
		///		items[i] = new MyStruct
		///				{
		///					MyInt = i,
		///					MyStr = "String #" + i
		///				};
		/// }
		/// 
		/// using (var newStream = GorgonDataStream.MarshalArrayToStream(items))
		/// {
		///		// Do your work with you stream data.
		/// }
		/// 
		/// ]]>
		/// </code>
		/// </example>
		public static GorgonDataStream MarshalArrayToStream<T>(T[] value)
			where T : struct
		{
			Type type = typeof(T);
			int count = value.Count();

			if (value == null)
			{
				return null;
			}

			if (((!type.IsExplicitLayout) && (!type.IsLayoutSequential)) || (type.StructLayoutAttribute == null))
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_STRUCT_NOT_EXPLICIT_LAYOUT, type.FullName), "value");
			}

			int size = type.StructLayoutAttribute.Size <= 0 ? Marshal.SizeOf(type) * count : type.StructLayoutAttribute.Size * count;

			var result = new GorgonDataStream(size);

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < value.Length; ++i)
			{
				T item = value[i];

				if (!UseMarshalling(type))
				{
					result.Write(item);
				}
				else
				{
					// We never need to deallocate the structure data because we're only ever using this pointer once.
					result.WriteMarshal(item, false);
				}
			}

			result.Position = 0;

			return result;
		}

		/// <summary>
		/// This method does nothing for a <see cref="GorgonDataStream"/>
		/// </summary>
		public override void Flush()
		{
			// Do nothing.
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>   
		/// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="System.ObjectDisposedException">Thrown when the stream is disposed.</exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			int actualCount = count;

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if ((offset < 0) || (offset >= buffer.Length))
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_OFFSET_OUT_OF_RANGE, offset, buffer.Length));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_COUNT_OUT_OF_RANGE, count));
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif

			if ((actualCount + _position) > _length)
			{
				actualCount = (_length - _position);
			}

			if (actualCount <= 0)
			{
				return 0;
			}

			DirectAccess.ReadArray(PositionPointer, buffer, offset, actualCount);

			Position += actualCount;

			return actualCount;
		}

		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>
		/// The new position within the current stream.
		/// </returns>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the offset moves the position outside of the boundaries of the stream.</exception>
		/// <remarks>
		/// <note type="important">
		/// <para>
		/// To improve performance, exceptions will only be thrown for this method if the library is compiled for <b>DEBUG</b> mode.
		/// </para>
		/// <para>
		/// Because of this, undefined behaviour may occur when the <b>RELEASE</b> version of the library is used and the boundaries of the buffer are exceeded.
		/// </para>
		/// </note>
		/// </remarks>
		public override long Seek(long offset, SeekOrigin origin)
		{
			long newPosition;

			if (_data == IntPtr.Zero)
			{
				return 0;
			}

			switch (origin)
			{
				case SeekOrigin.Begin:
#if DEBUG
					if (offset < 0)
					{
						throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_STREAM_BOS);
					}
					if (offset > _length)
					{
						throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_STREAM_EOS);
					}
#endif
					newPosition = offset;
					break;
				case SeekOrigin.End:
					newPosition = _length + offset;
#if DEBUG
					if (newPosition < 0)
					{
						throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_STREAM_BOS);
					}
					if (newPosition > _length)
					{
						throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_STREAM_EOS);
					}
#endif
					break;
				default:
					newPosition = _position + offset;

#if DEBUG
					if (newPosition < 0)
					{
						throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_STREAM_BOS);
					}
					if (newPosition > _length)
					{
						throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_STREAM_EOS);
					}
#endif
					break;
			}

			Position = newPosition;
			return Position;
		}

		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <remarks>
		/// <para>
		/// Calling this method will deallocate any data being held by the buffer, and reset the <see cref="Position"/> to the beginning of the stream.
		/// </para>
		/// <para>
		/// If the stream does not own the pointer to the data that it is wrapping, then an exception will be thrown. The same will happen if the stream is used to wrap and pin an <see cref="Array"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="NotSupportedException">Thrown when the stream does not own the pointer it is wrapping, or if the stream is wrapping an array.</exception>
		public override void SetLength(long value)
		{
			AllocateBuffer(value);
		}

		/// <summary>
		/// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>   
		/// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception>   
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public override void Write(byte[] buffer, int offset, int count)
		{
			int actualCount = count;

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if ((offset < 0) || (offset >= buffer.Length))
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_OFFSET_OUT_OF_RANGE, offset, buffer.Length));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_COUNT_OUT_OF_RANGE, count));
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif

			if ((actualCount + _position) > _length)
			{
				actualCount = (_length - _position);
			}

			DirectAccess.WriteArray(PositionPointer, buffer, offset, actualCount);

			Position += actualCount;
		}

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to write beyond the end of the stream is made.</exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public override void WriteByte(byte value)
		{
			const int size = sizeof(byte);

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (_position + size > _length)
			{
				throw new EndOfStreamException();
			}
#endif

			*((byte*)PositionPointer) = value;

			Position += size;
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>
		/// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public override int ReadByte()
		{
			const int size = sizeof(byte);

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}


			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}
#endif

			if (_position >= Length)
			{
				return -1;
			}

			byte value = *((byte*)PositionPointer);

			Position += size;

			return value;
		}

		/// <summary>
		/// Function to write a list of values to the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to write. Must be a value type.</typeparam>
		/// <param name="buffer">Array of data to write to the stream.</param>
		/// <param name="offset">Offset into the array to start reading the values from.</param>
		/// <param name="count">Number of elements in the array to read.</param>
		/// <exception cref="ArgumentException">Thrown when the sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>   
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is null. </exception> 
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> or <paramref name="count"/> parameters are negative. </exception>   
		/// <exception cref="NotSupportedException">Thrown when the stream does not support writing. </exception>   
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <remarks>
		/// <para>
		/// <note type="important">
		/// <para>
		/// To improve performance, exceptions will only be thrown for this method if the library is compiled for <b>DEBUG</b> mode.
		/// </para>
		/// <para>
		/// Because of this, undefined behaviour may occur when the <b>RELEASE</b> version of the library is used and the boundaries of the buffer are exceeded.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void WriteRange<T>(T[] buffer, int offset, int count)
			where T : struct
		{
			int actualCount = count * DirectAccess.SizeOf<T>();

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if ((offset < 0) || (offset >= buffer.Length))
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_OFFSET_OUT_OF_RANGE, offset, buffer.Length));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_COUNT_OUT_OF_RANGE, count));
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif

			if ((actualCount + _position) > _length)
			{
				actualCount = (_length - _position);
			}

			DirectAccess.WriteArray(PositionPointer, buffer, offset, actualCount);

			Position += actualCount;
		}

		/// <summary>
		/// Function to write a list of values to the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to write. Must be a value type.</typeparam>
		/// <param name="buffer">Array of data to write to the stream.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is null.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support writing. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <remarks>
		/// <para>
		/// This method, unlike the <see cref="WriteRange{T}(T[],int,int)"/> method will write the entire contents of the <paramref name="buffer"/> to the stream.
		/// </para>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public void WriteRange<T>(T[] buffer)
			where T : struct
		{
			buffer.ValidateObject("buffer");

			WriteRange(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Function to write a value to the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to write. Must be a value type.</typeparam>
		/// <param name="item">Value to write.</param>
		/// <exception cref="EndOfStreamException">Thrown when trying to write beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support writing. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public void Write<T>(T item)
			where T : struct
		{
			int typeSize = DirectAccess.SizeOf<T>();

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}
#endif
			if (typeSize + Position > _length)
			{
				return;
			}

			DirectAccess.WriteValue(PositionPointer, ref item);
			Position += typeSize;
		}

		/// <summary>
		/// Function to read an array of values from the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">The type of the values to read. Must be a value type.</typeparam>
		/// <param name="buffer">Array to deserialize the binary data into.</param>
		/// <param name="offset">Offset within the array to start copying into.</param>
		/// <param name="count">Number of elements to copy.</param>
		/// <returns>
		/// The total number of bytes read from the stream. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown when the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>   
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is null.</exception>   
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> or <paramref name="count"/> parameters are negative.</exception>   
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>   
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public virtual int ReadRange<T>(T[] buffer, int offset, int count)
			where T : struct
		{
			int actualCount = count * DirectAccess.SizeOf<T>();

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if ((offset < 0) || (offset >= buffer.Length))
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_OFFSET_OUT_OF_RANGE, offset, buffer.Length));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_STREAM_COUNT_OUT_OF_RANGE, count));
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif

			if ((actualCount + _position) > _length)
			{
				actualCount = (_length - _position);
			}

			if (actualCount <= 0)
			{
				return actualCount;
			}

			DirectAccess.ReadArray(PositionPointer, buffer, offset, actualCount);
			Position += actualCount;

			return actualCount;
		}

		/// <summary>
		/// Function to read an array of values from the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to read. Must be a value type.</typeparam>
		/// <param name="count">Number of items to read from the stream.</param>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
		/// <returns>An array of values of type <typeparamref name="T"/> that were deserialized from the stream.</returns>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public T[] ReadRange<T>(int count)
			where T : struct
		{
			var result = new T[count];

			ReadRange(result, 0, count);

			return result;
		}

		/// <summary>
		/// Function to read a value from the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">The type of the value to read. Must be a value type.</typeparam>
		/// <returns>The deserialized value from the stream.</returns>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <exception cref="EndOfStreamException">Thrown when trying to read beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// </remarks>
		public T Read<T>()
			where T : struct
		{
			int typeSize = DirectAccess.SizeOf<T>();

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (typeSize + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif

			T result;
			DirectAccess.ReadValue(PositionPointer, out result);
			Position += typeSize;

			return result;
		}

		/// <summary>
		/// Function to copy the data in the <see cref="GorgonDataStream"/> to an <see cref="IntPtr"/> representing a pointer to an unmanaged memory address.
		/// </summary>
		/// <param name="pointer">Pointer to the unmanaged memory to copy into.</param>
		/// <param name="size">The number of bytes to copy from the stream.</param>
		/// <exception cref="EndOfStreamException">Thrown when trying to read beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// <para>
		/// <note type="caution">
		/// This method is unsafe, there is no bounds checking on the <paramref name="pointer"/> parameter and therefore a buffer overrun is possible. Use this method with care.
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyToPointer(IntPtr pointer, int size)
		{
#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif
			_pointerOffset.CopyTo(pointer, size);
			Position += size;
		}

		/// <summary>
		/// Function to copy the data in the <see cref="GorgonDataStream"/> to the unmanaged memory specified by the pointer.
		/// </summary>
		/// <param name="pointer">Pointer to the unmanaged memory to copy into.</param>
		/// <param name="size">The number of bytes to copy from the stream.</param>
		/// <exception cref="EndOfStreamException">Thrown when trying to read beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// <para>
		/// <note type="caution">
		/// This method is unsafe, there is no bounds checking on the <paramref name="pointer"/> parameter and therefore a buffer overrun is possible. Use this method with care.
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyToPointer(void* pointer, int size)
		{
#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif
			DirectAccess.MemoryCopy(PositionPointer, pointer, size);
			Position += size;
		}

		/// <summary>
		/// Function to copy the native memory specified by the pointer wrapped by a <see cref="IntPtr"/> into a <see cref="GorgonDataStream"/>.		
		/// </summary>
		/// <param name="pointer">Pointer to the data to copy.</param>
		/// <param name="size">The number of bytes to copy into the stream.</param>
		/// <exception cref="EndOfStreamException">Thrown when trying to write beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support writing. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="WriteRange{T}(T[],int,int)"/>
		/// <para>
		/// <note type="caution">
		/// This method is unsafe, there is no bounds checking on the <paramref name="pointer"/> parameter and therefore a buffer overrun is possible. Use this method with care.
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyFromPointer(IntPtr pointer, int size)
		{
#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif

			_pointerOffset.CopyFrom(pointer, size);
			Position += size;
		}

		/// <summary>
		/// Function to copy the native memory specified by the pointer into a <see cref="GorgonDataStream"/>.		
		/// </summary>
		/// <param name="pointer">Pointer to the data to copy.</param>
		/// <param name="size">The number of bytes to copy into the stream.</param>
		/// <exception cref="EndOfStreamException">Thrown when trying to write beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support writing. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed. </exception>
		/// <remarks>
		/// <inheritdoc cref="CopyFromPointer(System.IntPtr,int)"/>
		/// </remarks>
		public void CopyFromPointer(void* pointer, int size)
		{
#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif
			DirectAccess.MemoryCopy(PositionPointer, pointer, size);
			Position += size;
		}

		/// <summary>
		/// Function to marshal a value into the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to marshal.</typeparam>
		/// <param name="data">Data to marshal.</param>
		/// <param name="deleteContents">[Optional] <b>true</b> to free memory allocated for the object/value types fields when reusing the destination pointer, <b>false</b> to leave alone.</param>
		/// <remarks>
		/// <para>
		/// This method will marshal a value of the type <typeparamref name="T"/> (can be either an object or value type) into the unmanaged memory wrapped by this stream.
		/// </para>
		/// <para>
		/// This method supports marshalling of field/property values (e.g. members with the <see cref="MarshalAsAttribute"/>).
		/// </para>
		/// <para>
		/// Passing <b>false</b> to <paramref name="deleteContents"/> may result in a memory leak if the stream data had this type written to it previous at the current <see cref="Position"/>, and is being reused and the 
		/// type specified by <typeparamref name="T"/> contains reference types (e.g. <see cref="string"/>).
		/// </para>
		/// <para>
		/// For more information, see the <see cref="Marshal.StructureToPtr"/> method.
		/// </para>
		/// <note type="warning">
		/// <para>
		/// There is a performance penalty for using this method, and it is not recommended to performance critical code. 
		/// </para>
		/// <para>
		/// For best performance, use the <see cref="Write{T}"/> method where the type specified by <typeparamref name="T"/> only contains other value types (the same restriction for child value types applies), or primitive 
		/// types (i.e. blittable fields).
		/// </para>
		/// </note>
		/// </remarks>
		/// <exception cref="NotSupportedException">The stream does not support writing. </exception>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the length of the stream is made.</exception>
		public void WriteMarshal<T>(T data, bool deleteContents)
		{
			int dataSize = Marshal.SizeOf(typeof(T));

			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (dataSize + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}

			_pointerOffset.MarshalFrom(data, deleteContents);

			Position += dataSize;
		}

		/// <summary>
		/// Function to marshal a value from the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to marshal.</typeparam>
		/// <returns>The data deserialized into the value of type <typeparamref name="T"/>.</returns>
		/// <remarks>
		/// <para>
		/// This method will marshal binary data from the unmanaged memory wrapped by this stream into a value of the type <typeparamref name="T"/> (can be either an object or value type).
		/// </para>
		/// <para>
		/// This method supports marshalling of field/property values (e.g. members with the <see cref="MarshalAsAttribute"/>).
		/// </para>
		/// <para>
		/// For more information, see the <see cref="Marshal.PtrToStructure(IntPtr, Type)"/> method.
		/// </para>
		/// <note type="warning">
		/// <para>
		/// There is a performance penalty for using this method, and it is not recommended to performance critical code. 
		/// </para>
		/// <para>
		/// For best performance, use the <see cref="Read{T}"/> method where the type specified by <typeparamref name="T"/> only contains other value types (the same restriction for child value types applies), or primitive 
		/// types (i.e. blittable fields).
		/// </para>
		/// </note>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="NotSupportedException">The stream does not support reading. </exception>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream was made.</exception>
		public T ReadMarshal<T>()
		{
			int dataSize = Marshal.SizeOf(typeof(T));

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_data == IntPtr.Zero)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (dataSize + Position > _length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif

			var value = _pointerOffset.MarshalTo<T>();
			Position += dataSize;

			return value;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream" /> class.
		/// </summary>
		/// <param name="data">The data used to initialize the stream.</param>
		/// <param name="index">Index inside of the source array to start reading from.</param>
		/// <param name="count">Number of elements to read.</param>
		/// <param name="status">A flag indicating if the buffer is read only, write only or both.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the data parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> or <paramref name="count"/> parameters are less than 0.
		/// <para>-or-</para>
		/// Thrown when the index parameter is larger than the source array.
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the index plus the count parameters total to larger than the size of the array.</exception>
		/// <remarks>
		/// The array elements should all be of the same type, and value types.
		/// <para>A pointer to the array will be held and released upon disposal of the stream, this may impact garbage collection performance.
		/// Also, since the stream is holding a pointer, any changes to the <paramref name="data" /> parameter array elements will be reflected
		/// in the stream.
		/// </para>
		/// </remarks>
		public GorgonDataStream(Array data, int index, int count, StreamAccess status = StreamAccess.ReadWrite)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			if ((index < 0) || (index >= data.Length))
			{
				throw new ArgumentOutOfRangeException("index", string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, index, data.Length));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", string.Format(Resources.GOR_ERR_STREAM_COUNT_OUT_OF_RANGE, count));
			}

			if (index + count > data.Length)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}

			// Pin the array.
			_handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			_data = Marshal.UnsafeAddrOfPinnedArrayElement(data, index);
			_length = count * Marshal.SizeOf(data.GetType().GetElementType());
			_pointerOffset = _data;
			StreamAccess = status;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="data">The data used to initialize the stream.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the data parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>The array elements should all be of the same type, and value types.
		/// <para>A pointer to the array will be held and released upon disposal of the stream, this may impact garbage collection performance.  
		/// Also, since the stream is holding a pointer, any changes to the <paramref name="data"/> parameter array elements will be reflected 
		/// in the stream.
		/// </para>
		/// </remarks>
		public GorgonDataStream(Array data)
			: this(data, 0, data.Length)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="capacity">The capacity of the underlying buffer.</param>
		public GorgonDataStream(int capacity)
		{
			AllocateBuffer(capacity);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="source">The source pointer.</param>
		/// <param name="size">The size of the buffer (in bytes).</param>
		public GorgonDataStream(IntPtr source, int size)
		{
			_ownsPointer = false;
			_data = source;
			_pointerOffset = source;
			_position = 0;
			_length = size;
			StreamAccess = StreamAccess.ReadWrite;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="source">The source pointer.</param>
		/// <param name="size">The size of the buffer (in bytes).</param>
		public GorgonDataStream(void* source, int size)
		{
			_ownsPointer = false;
			_data = new IntPtr(source);
			_pointerOffset = _data;
			_position = 0;
			_length = size;
			StreamAccess = StreamAccess.ReadWrite;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="GorgonDataStream"/> is reclaimed by garbage collection.
		/// </summary>
		~GorgonDataStream()
		{
			Dispose(false);
		}
		#endregion
	}
}
