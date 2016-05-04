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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core.Properties;
using Gorgon.Math;
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
	/// The object supports a multitude of reading/writing options for primitive data, array data, and even generic types to the stream. For generic types being read or written, the type must 
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
	/// Existing pointers to native memory can be used when constructing this stream type via the <see cref="GorgonDataStream(IntPtr, long)"/> (or using the constructor with the raw <c>void *</c> pointer). 
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
	/// is a possibility and native memory access is required, then the <see cref="GorgonPointerBase"/> object provides better functionality for working with native memory directly while providing functions that 
	/// are better suited to thread safety. 
	/// </para>
	/// <para>
	/// A better interface to dealing with unmanaged memory is through the <see cref="GorgonPointerBase"/> class.
	/// </para>
	/// </note>
	/// <note type="caution">
	/// <para>
	/// Gorgon will be phasing out it's internal usage of this type in favour of more direct access to memory via the <see cref="GorgonPointerBase"/>
	/// </para>
	/// </note>
	/// </remarks>
	/// <seealso cref="GorgonPointerBase"/>
	public unsafe class GorgonDataStream
		: Stream
	{
		#region Variables.
		// The pointer to unmanaged memory.
		private IGorgonPointer _pointer;
		// Position in the buffer.
		private long _position;
		// Flag to indicate that we own this pointer.
		private readonly bool _ownsPointer;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the access level for the stream.
		/// </summary>
		public StreamAccess StreamAccess
		{
			get;
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <returns><b>true</b> if the stream supports reading; otherwise, <b>false</b>.</returns>
		public override bool CanRead => ((StreamAccess == StreamAccess.ReadOnly) || (StreamAccess == StreamAccess.ReadWrite));

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <returns>This property will always return <b>true</b> for this type.</returns>
		public override bool CanSeek => true;

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <returns><b>true</b> if the stream supports writing; otherwise, <b>false</b>.</returns>
		public override bool CanWrite => ((StreamAccess == StreamAccess.WriteOnly) || (StreamAccess == StreamAccess.ReadWrite));

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <returns>A long value representing the length of the stream in bytes.</returns>
		public override long Length => _pointer?.Size ?? 0;

		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		/// <returns>The current position within the stream.</returns>
		/// <remarks>
		/// <note type="important">
		/// To improve performance, exceptions will only be thrown for this property if the library is compiled for <b>DEBUG</b> mode.
		/// </note>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the value is less than zero or greater than <see cref="int.MaxValue"/>.</exception>
		public override long Position
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
#if DEBUG
				if (_pointer == null)
				{
					throw new ObjectDisposedException("GorgonDataStream");
				}
#endif
				return _position;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
#if DEBUG
				if (_pointer == null)
				{
					throw new ObjectDisposedException("GorgonDataStream");
				}

				// Limit to the bounds of an integer.
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, value, long.MaxValue));
				}
#endif

				_position = (int)value;
			}
		}

		/// <summary>
		/// Property to return the <see cref="IntPtr"/> which wraps the native pointer to the unmanaged memory wrapped by the stream.
		/// </summary>
		/// <remarks>
		/// This value returns an <see cref="IntPtr"/> that wraps the pointer that points at the starting address for the unmanaged memory wrapped by the stream. To get the pointer to the memory address 
		/// offset by the current <see cref="Position"/>, use the <see cref="PositionIntPtr"/> property.
		/// </remarks>
		[Obsolete("TODO: This property is going away. People who use this are going to experience crashes.")]
		public IntPtr BaseIntPtr => new IntPtr(_pointer.Address);

		/// <summary>
		/// Property to return the native pointer to the unmanaged memory wrapped by the stream, and offset by the current <see cref="Position"/> in the stream.
		/// </summary>
		/// <remarks>
		/// This value returns the same value as the <see cref="BasePointer"/>, but offset by the number of bytes indicated by <see cref="Position"/>.
		/// </remarks>
		[Obsolete("TODO: This property is going away. People who use this are going to experience crashes.")]
		public void* PositionPointer => (void *)(_pointer.Address + _position);

		/// <summary>
		/// Property to return the <see cref="IntPtr"/> which wraps the native pointer to the unmanaged memory wrapped by the stream, and offset by the current <see cref="Position"/> in the stream.
		/// </summary>
		/// <remarks>
		/// This value returns the same value as the <see cref="BaseIntPtr"/>, but offset by the number of bytes indicated by <see cref="Position"/>.
		/// </remarks>
		[Obsolete("TODO: This property is going away. People who use this are going to experience crashes.")]
		public IntPtr PositionIntPtr => new IntPtr(_pointer.Address + _position);

		/// <summary>
		/// Property to return the native pointer to the unmanaged memory wrapped by the stream.
		/// </summary>
		/// <remarks>
		/// This value returns a pointer that points at the starting address for the unmanaged memory wrapped by the stream. To get the pointer to the memory address offset by the current 
		/// <see cref="Position"/>, use the <see cref="PositionPointer"/> property.
		/// </remarks>
		[Obsolete("TODO: This property is going away. People who use this are going to experience crashes.")]
		public void* BasePointer => (void *)(_pointer.Address);

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

			value = value.Max(0);

			if (_pointer != null)
			{
				_pointer.Dispose();
				_pointer = null;
			}

			_position = 0;

			if (value == 0)
			{
				return;
			}

			_pointer = new GorgonPointer(value);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		[SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose", Justification = "Base Stream does nothing in Dispose(bool). Why call it?")]
		protected override void Dispose(bool disposing)
		{
			// We suppress the base call to Dispose because Stream's Dispose(bool) does nothing.
			if ((!_ownsPointer) || (_pointer == null) || (_pointer.IsDisposed))
			{
				return;
			}

			// Destroy the buffer.
			_pointer.Dispose();
			_pointer = null;
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
		/// Function to retrieve a <see cref="GorgonPointerAlias"/> set to the position within the stream.
		/// </summary>
		/// <returns>A <see cref="GorgonPointerAlias"/> pointing at the data within the stream, at the offset defined by <see cref="Position"/>.</returns>
		/// <remarks>
		/// <para>
		/// The <see cref="GorgonPointerAlias"/> returned by this method will point at the block of memory held by this stream offset by the <see cref="Position"/> within the data 
		/// stream. The size of the memory being pointed at will be the <see cref="Length"/> - <see cref="Position"/> to ensure that pointer bounds are not exceeded.
		/// </para>
		/// <para>
		/// If a pointer to the beginning of the memory block is desired, then set the <see cref="Position"/> value to 0, and reset it to its original value after calling this method.
		/// </para>
		/// </remarks>
		public GorgonPointerAlias GetPointer()
		{
			return new GorgonPointerAlias((byte *)(_pointer.Address + _position), Length - _position);
		}

		/// <summary>
		/// Function to marshal an array of value types into a <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to marshal. Must be a value or primitive type.</typeparam>
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
		/// <exception cref="ArgumentException">
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

			if (value == null)
			{
				return null;
			}

			int count = value.Length;

			if (((!type.IsExplicitLayout) && (!type.IsLayoutSequential)) || (type.StructLayoutAttribute == null))
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_STRUCT_NOT_EXPLICIT_LAYOUT, type.FullName), nameof(value));
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
					result.WriteMarshal(item);
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

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
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

			if ((actualCount + _position) > _pointer.Size)
			{
				actualCount = (int)(_pointer.Size - _position);
			}

			if (actualCount <= 0)
			{
				return 0;
			}
			
			_pointer.ReadRange(_position, buffer, offset, actualCount);
			_position += actualCount;

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

			if (_pointer == null)
			{
				return 0;
			}

			switch (origin)
			{
				case SeekOrigin.Begin:
#if DEBUG
					if (offset < 0)
					{
						throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_STREAM_BOS);
					}
					if (offset > _pointer.Size)
					{
						throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_STREAM_EOS);
					}
#endif
					newPosition = offset;
					break;
				case SeekOrigin.End:
					newPosition = _pointer.Size + offset;
#if DEBUG
					if (newPosition < 0)
					{
						throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_STREAM_BOS);
					}
					if (newPosition > _pointer.Size)
					{
						throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_STREAM_EOS);
					}
#endif
					break;
				default:
					newPosition = _position + offset;

#if DEBUG
					if (newPosition < 0)
					{
						throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_STREAM_BOS);
					}
					if (newPosition > _pointer.Size)
					{
						throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_STREAM_EOS);
					}
#endif
					break;
			}

			_position = newPosition;
			return _position;
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
		public override void Write(byte[] buffer, int offset, int count)
		{
			int actualCount = count;

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
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

			if ((actualCount + _position) > _pointer.Size)
			{
				actualCount = (int)(_pointer.Size - _position);
			}

			_pointer.WriteRange(_position, buffer, offset, actualCount);
			_position += actualCount;
		}

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		/// <exception cref="EndOfStreamException">Thrown when an attempt to write beyond the end of the stream is made.</exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
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
		public override void WriteByte(byte value)
		{
			const int size = sizeof(byte);

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (_position + size > _pointer.Size)
			{
				throw new EndOfStreamException();
			}
#endif
			_pointer.Write(_position, ref value);
			_position += size;
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>
		/// The unsigned byte cast to an int, or -1 if at the end of the stream.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>   
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
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
		public override int ReadByte()
		{
			const int size = sizeof(byte);

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}


			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}
#endif

			if (_position >= _pointer.Size)
			{
				return -1;
			}
			
			byte value;

			_pointer.Read(_position, out value);
			_position += size;

			return value;
		}

		/// <summary>
		/// Function to write a list of values to the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to write. Must be a value or primitive type.</typeparam>
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

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
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

			if ((actualCount + _position) > _pointer.Size)
			{
				actualCount = (int)(_pointer.Size - _position);
			}

			_pointer.WriteRange(_position, buffer, offset, count);
			_position += actualCount;
		}

		/// <summary>
		/// Function to write a list of values to the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to write. Must be a value or primitive type.</typeparam>
		/// <param name="buffer">Array of data to write to the stream.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is null.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support writing. </exception>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <remarks>
		/// <para>
		/// This method, unlike the <see cref="WriteRange{T}(T[],int,int)"/> method will write the entire contents of the <paramref name="buffer"/> to the stream.
		/// </para>
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
		public void WriteRange<T>(T[] buffer)
			where T : struct
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			WriteRange(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Function to write a value to the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to write. Must be a value or primitive type.</typeparam>
		/// <param name="item">Value to write.</param>
		/// <exception cref="EndOfStreamException">Thrown when trying to write beyond the end of the stream.</exception>
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
		public void Write<T>(T item)
			where T : struct
		{
			int typeSize = DirectAccess.SizeOf<T>();

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}
#endif
			if (typeSize + Position > _pointer.Size)
			{
				return;
			}

			_pointer.Write(_position, ref item);
			_position += typeSize;
		}

		/// <summary>
		/// Function to read an array of values from the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">The type of the values to read. Must be a value or primitive type.</typeparam>
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
		public virtual int ReadRange<T>(T[] buffer, int offset, int count)
			where T : struct
		{
#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
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
			int countSize = DirectAccess.SizeOf<T>() * count;

			if ((countSize + _position) > _pointer.Size)
			{
				countSize = (int)(_pointer.Size - _position);
			}

			if (countSize <= 0)
			{
				return 0;
			}

			_pointer.ReadRange(_position, buffer, offset, count);
			_position += countSize;

			return countSize;
		}

		/// <summary>
		/// Function to read an array of values from the <see cref="GorgonDataStream"/>.
		/// </summary>
		/// <typeparam name="T">Type of data to read. Must be a value or primitive type.</typeparam>
		/// <param name="count">Number of items to read from the stream.</param>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
		/// <returns>An array of values of type <typeparamref name="T"/> that were deserialized from the stream.</returns>
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
		/// <typeparam name="T">The type of the value to read. Must be a value or primitive type.</typeparam>
		/// <returns>The deserialized value from the stream.</returns>
		/// <exception cref="ObjectDisposedException">Thrown when methods were called after the stream was closed.</exception>
		/// <exception cref="EndOfStreamException">Thrown when trying to read beyond the end of the stream.</exception>
		/// <exception cref="NotSupportedException">Thrown when the stream does not support reading.</exception>
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
		public T Read<T>()
			where T : struct
		{
			int typeSize = DirectAccess.SizeOf<T>();

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (typeSize + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif

			T result;
			_pointer.Read(_position, out result);
			_position += typeSize;

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

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif

			DirectAccess.MemoryCopy(pointer, new IntPtr(_pointer.Address + _position), size);
			_position += size;
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

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif
			DirectAccess.MemoryCopy(pointer, (void *)(_pointer.Address + _position), size);
			_position += size;
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

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif
			_pointer.CopyMemory(pointer, size, _position);
			_position += size;
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
		/// <para>
		/// <note type="caution">
		/// This method is unsafe, there is no bounds checking on the <paramref name="pointer"/> parameter and therefore a buffer overrun is possible. Use this method with care.
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyFromPointer(void* pointer, int size)
		{
#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (size + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif
			_pointer.CopyMemory(pointer, size, _position);
			_position += size;
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
		public void WriteMarshal<T>(T data, bool deleteContents = false)
		{
			int dataSize = Marshal.SizeOf(typeof(T));

#if DEBUG
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (dataSize + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_OFFSET_COUNT_TOO_LARGE);
			}
#endif

			Marshal.StructureToPtr(data, new IntPtr(_pointer.Address + _position), deleteContents);

			_position += dataSize;
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
			Type typeT = typeof(T);
			int dataSize = Marshal.SizeOf(typeT);

#if DEBUG
			if (!CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			if (_pointer == null)
			{
				throw new ObjectDisposedException("GorgonDataStream");
			}

			if (dataSize + Position > _pointer.Size)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}
#endif

			T value = (T)Marshal.PtrToStructure(new IntPtr(_pointer.Address + _position), typeT);
			_position += dataSize;

			return value;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="capacity">The size of the buffer used by the stream, in bytes.</param>
		/// <remarks>
		/// This constructor allocates a buffer of unmanaged native memory and uses that to read/write. Callers of the stream should call <see cref="Dispose"/> when finished with the stream. Otherwise 
		/// the memory will not be freed until the object is finalized much later.
		/// </remarks>
		public GorgonDataStream(int capacity)
		{
			_ownsPointer = true;
			AllocateBuffer(capacity.Max(0));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="source">The <see cref="IntPtr"/> wrapping the pointer to the unmanaged memory.</param>
		/// <param name="size">The size of the data, in bytes.</param>
		/// <remarks>
		/// This constructor will wrap an <see cref="IntPtr"/> pointing at unmanaged memory and uses that to read/write. The stream does not own the pointer, and as such a call to <see cref="Dispose"/> will 
		/// do nothing and therefore optional (although it is recommended for best practise).
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter has a value of <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		public GorgonDataStream(IntPtr source, long size)
		{
			if (source == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(source), Resources.GOR_ERR_DATASTREAM_POINTER_IS_NULL);
			}

			if (size < 0)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATASTREAM_POINTER_SIZE_TOO_SMALL);
			}

			_ownsPointer = false;
			_pointer = new GorgonPointerAlias(source, size);
			_position = 0;
			StreamAccess = StreamAccess.ReadWrite;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="source">The native pointer to the unmanaged memory.</param>
		/// <param name="size">The size of the data, in bytes.</param>
		/// <remarks>
		/// This constructor will wrap a native pointer that is pointing at unmanaged memory and uses that to read/write. The stream does not own the pointer, and as such a call to <see cref="Dispose"/> will 
		/// do nothing and therefore optional (although it is recommended for best practise).
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter has a value of <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		public GorgonDataStream(void* source, long size)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source), Resources.GOR_ERR_DATASTREAM_POINTER_IS_NULL);
			}

			if (size < 0)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATASTREAM_POINTER_SIZE_TOO_SMALL);
			}

			_ownsPointer = false;
			_pointer = new GorgonPointerAlias(source, size);
			_position = 0;
			StreamAccess = StreamAccess.ReadWrite;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDataStream"/> class.
		/// </summary>
		/// <param name="pointer">The <see cref="IGorgonPointer"/> to wrap within this stream.</param>
		/// <remarks>
		/// This constructor will take an <see cref="IGorgonPointer"/>, and wrap it within this stream object. The stream will <b>not</b> own the <see cref="IGorgonPointer"/>, and consequently, will 
		/// not be responsible for freeing the memory it may have allocated. To free the memory, the code that created the <see cref="IGorgonPointer"/> must call its dispose method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonDataStream(IGorgonPointer pointer)
		{
			if (pointer == null)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			_ownsPointer = false;
			_pointer = pointer;
			StreamAccess = StreamAccess.ReadWrite;
		}
		#endregion
	}
}
