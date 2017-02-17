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
// Created: June 15, 2016 9:33:57 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using Gorgon.Reflection;
using DX = SharpDX;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A buffer for constant shader data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use a constant buffer to send information to a shader every frame (or more). 
	/// </para>
	/// <para>
	/// To send data to a shader using a constant buffer, an application can upload a value type (or primitive) value to the buffer using one of the 
	/// <see cref="O:Gorgon.Graphics.GorgonConstantBuffer.Update{T}(ref T)">Update&lt;T&gt;</see> overloads. This allows an application to update the state of a shader to reflect changes in the application. 
	/// Things like animation or setup information can easily be sent to modify the state of a shader (hence somewhat making the term <i>constant</i> a misnomer).
	/// </para>
	/// <para>
	/// Constant buffers are bound to a finite number of slots in the shader. Typically these are declared as follows:
	/// <pre>
	/// cbuffer ViewMatrix : register(b0)
	/// {
	///	   Matrix4x4 viewMatrix;
	/// }
	/// </pre>
	/// This binds a matrix used for the view to constant buffer slot 0. Note that the register slot name starts with a <b>b</b>.
	/// </para>
	/// <para> 
	/// <example language="csharp">
	/// For example, to update a view matrix to shift to the right every frame:
	/// <code language="csharp">
	/// <![CDATA[
	/// Vector3 _lastPosition;
	/// GorgonConstantBuffer _viewMatrixBuffer;		// This is created elsewhere with a size of 64 bytes to hold a Matrix.
	/// 
	/// void IdleMethod()
	/// {
	///		// Move 2 units to the right every second.
	///		_lastPosition = new Vector3(_lastPosition.X + 2 * GorgonTiming.Delta, 0, -2.0f);
	///		Matrix viewMatrix = Matrix.Identity;
	/// 
	///		// Adjust the matrix to perform the translation.
	///		// We use ref/out here for better performance.
	///		Matrix.Translation(ref _lastPosition, out viewMatrix);
	///  
	///		// Send to the shader (typically, this would be the vertex shader).
	///		_viewMatrixBuffer.Update<Matrix>(ref viewMatrix);
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// </para>
	/// </remarks>
	public class GorgonConstantBuffer
		: GorgonBuffer, IEquatable<GorgonConstantBuffer>
	{
		#region Variables.
		// The log interface used for debug logging.
		private readonly IGorgonLog _log;
		// The information used to create the buffer.
		private readonly GorgonConstantBufferInfo _info;
		#endregion

		#region Properties.
		/// <summary>
		/// Property used to return the information used to create this buffer.
		/// </summary>
		public IGorgonConstantBufferInfo Info => _info;

		/// <summary>
		/// Property to return the type of buffer.
		/// </summary>
		public override BufferType BufferType => BufferType.Constant;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the buffer data.
		/// </summary>
		/// <param name="initialData">The initial data used to populate the buffer.</param>
		private void Initialize(GorgonPointerBase initialData)
		{
			// If the buffer is not aligned to 16 bytes, then increase its size.
			SizeInBytes = (Info.SizeInBytes + 15) & ~15;

			if (SizeInBytes > Graphics.VideoDevice.MaxConstantBufferSize)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CONSTANT_BUFFER_TOO_LARGE, SizeInBytes, Graphics.VideoDevice.MaxConstantBufferSize));
			}
			
			D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

			switch (_info.Usage)
			{
				case D3D11.ResourceUsage.Staging:
					cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
					break;
				case D3D11.ResourceUsage.Dynamic:
					cpuFlags = D3D11.CpuAccessFlags.Write;
					break;
			}

			_log.Print($"{Name} Constant Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

			var desc  = new D3D11.BufferDescription
			{
				SizeInBytes = SizeInBytes,
				Usage = _info.Usage,
				BindFlags = D3D11.BindFlags.ConstantBuffer,
				OptionFlags = D3D11.ResourceOptionFlags.None,
				CpuAccessFlags = cpuFlags,
				StructureByteStride = 0
			};

			if ((initialData != null) && (initialData.Size > 0))
			{
				D3DResource = D3DBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), new IntPtr(initialData.Address), desc);
			}
			else
			{
				D3DResource = D3DBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), desc);
			}
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <typeparam name="T">The type of data to send to the buffer. This must be a value type or primitive.</typeparam>
		/// <param name="data">The data used to populate the buffer.</param>
		/// <exception cref="ArgumentException">Thrown if the type specified by <typeparamref name="T"/> is not safe to use in a native memory operation.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.</exception>
		/// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to send a value type (<c>struct</c>) or primitive value to the buffer. 
		/// </para>
		/// <para>
		/// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
		/// </para>
		/// <para>
		/// When sending a value type to the buffer the data must be a primitive type with no complex members (i.e. members must be value types or primitive types). Furthermore, the value type and any 
		/// members that are value types must use the <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind"/> of <see cref="LayoutKind.Explicit"/> or <see cref="LayoutKind.Sequential"/>. This is 
		/// mandatory in order to ensure that the data gets sent to the card as-is without the .NET memory manager rearranging the members of the type. 
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Update<T>(ref T data)
			where T : struct
		{
			int size = DirectAccess.SizeOf<T>();

#if DEBUG
			if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_IMMUTABLE_OR_DYNAMIC);
			}

			if (size > SizeInBytes)
			{
				throw new ArgumentOutOfRangeException(nameof(data));
			}

			Type type = typeof(T);

			if (!type.IsSafeForNative())
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TYPE_NOT_VALID_FOR_NATIVE, type.FullName));
			}
#endif

			Graphics.D3DDeviceContext.UpdateSubresource(ref data, D3DResource, 0, size);
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <param name="data">A <see cref="GorgonPointerBase"/> that points at a region of memory that holds the data to copy to the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.</exception>
		/// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to send a blob of byte data to the buffer. This allows for fine grained control over what gets sent to the buffer. 
		/// </para>
		/// <para>
		/// Because this is using native, unmanaged, memory, special care must be taken to ensure that the application does not attempt to read/write out of bounds of that memory region.
		/// </para>
		/// <para>
		/// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Update(GorgonPointerBase data)
		{
			data.ValidateObject(nameof(data));
			Update(data, 0, (int)data.Size);
		}

		/// <summary>
		/// Function to update the constant buffer data with data from native memory.
		/// </summary>
		/// <param name="data">The <see cref="GorgonPointerBase"/> to the native memory holding the data to copy into the buffer.</param>
		/// <param name="offset">The offset, in bytes, to start copying from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/> plus the size of the data in <paramref name="data"/> exceed the size of this buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.
		/// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
		/// <para>-or-</para>
		/// <para>The <paramref name="offset"/> parameter is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Use this method to send a blob of byte data to the buffer. This allows for fine grained control over what gets sent to the buffer. 
		/// </para>
		/// <para>
		/// Because this is using native, unmanaged, memory, special care must be taken to ensure that the application does not attempt to read/write out of bounds of that memory region. Particular care must be 
		/// taken to ensure that <paramref name="offset"/> does not exceed the bounds of the memory region.
		/// </para>
		/// <para>
		/// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Update(GorgonPointer data, int offset)
		{
			data.ValidateObject(nameof(data));

			int size = (int)data.Size - offset;

			Update(data, offset, size);
		}

		/// <summary>
		/// Function to update the constant buffer data with data from native memory.
		/// </summary>
		/// <param name="data">The <see cref="GorgonPointer"/> to the native memory holding the data to copy into the buffer.</param>
		/// <param name="offset">The offset, in bytes, to start copying from.</param>
		/// <param name="size">The size, in bytes, to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/> plus the size of the data in <paramref name="data"/> exceed the size of this buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.
		/// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
		/// <para>-or-</para>
		/// <para>The <paramref name="size"/> parameter is less than 1, or larger than the buffer size.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="offset"/> parameter is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Use this method to send a blob of byte data to the buffer. This allows for fine grained control over what gets sent to the buffer. 
		/// </para>
		/// <para>
		/// Because this is using native, unmanaged, memory, special care must be taken to ensure that the application does not attempt to read/write out of bounds of that memory region. Particular care must be 
		/// taken to ensure that <paramref name="offset"/> and <paramref name="size"/> do not exceed the bounds of the memory region.
		/// </para>
		/// <para>
		/// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Update(GorgonPointerBase data, int offset, int size)
		{
			data.ValidateObject(nameof(data));

#if DEBUG
			if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_IMMUTABLE_OR_DYNAMIC);
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}

			if (offset + size > data.Size)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, size));
			}

			if (size > SizeInBytes)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}
#endif

			Graphics.D3DDeviceContext.UpdateSubresource(new DX.DataBox
			                                             {
				                                             DataPointer = new IntPtr(data.Address + offset),
				                                             SlicePitch = 0,
				                                             RowPitch = size
			                                             },
			                                             D3DResource);
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <typeparam name="T">The type of data to send to the buffer. This must be a value type or primitive.</typeparam>
		/// <param name="data">The array of data to populate the buffer with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the type specified by <typeparamref name="T"/> is not safe to use in a native memory operation.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter, multiplied by the number of items to copy is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.
		/// <para>-or-</para>
		/// <para>Thrown when the size of the type <typeparamref name="T"/> multiplied by the number of elements in <paramref name="data"/> is larger than the buffer size.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to send an array of value types (<c>struct</c>) or primitive values to the buffer. 
		/// </para>
		/// <para>
		/// When sending a value type to the buffer the data must be a primitive type with no complex members (i.e. members must be value types or primitive types). Furthermore, the value type and any 
		/// members that are value types must use the <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind"/> of <see cref="LayoutKind.Explicit"/> or <see cref="LayoutKind.Sequential"/>. This is 
		/// mandatory in order to ensure that the data gets sent to the card as-is without the .NET memory manager rearranging the members of the type. 
		/// </para>
		/// <para>
		/// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Update<T>(T[] data)
			where T : struct
		{
			data.ValidateObject(nameof(data));

			Update(data, 0, data.Length);			
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <typeparam name="T">The type of data to send to the buffer. This must be a value type or primitive.</typeparam>
		/// <param name="data">The array of data to populate the buffer with.</param>
		/// <param name="offset">The offset within the array to start copying from.</param>
		/// <param name="count">The number of elements to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the type specified by <typeparamref name="T"/> is not safe to use in a native memory operation.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> plus the <paramref name="count"/> exceeds the number of elements in the <paramref name="data"/> parameter.</para>
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter, multiplied by the number of items to copy is larger than the total <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> of the buffer.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0, or the <paramref name="count"/> is less than 1.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the size of the type <typeparamref name="T"/> multiplied by the count (minus the offset) is larger than the buffer size.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonConstantBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to send an array of value types (<c>struct</c>) or primitive values to the buffer. 
		/// </para>
		/// <para>
		/// When sending a value type to the buffer the data must be a primitive type with no complex members (i.e. members must be value types or primitive types). Furthermore, the value type and any 
		/// members that are value types must use the <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind"/> of <see cref="LayoutKind.Explicit"/> or <see cref="LayoutKind.Sequential"/>. This is 
		/// mandatory in order to ensure that the data gets sent to the card as-is without the .NET memory manager rearranging the members of the type. 
		/// </para>
		/// <para>
		/// This method will throw an exception when the buffer is created with a <see cref="IGorgonConstantBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Update<T>(T[] data, int offset, int count)
			where T : struct
		{
			data.ValidateObject(nameof(data));

			int size = DirectAccess.SizeOf<T>();
			
#if DEBUG
			if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_IMMUTABLE_OR_DYNAMIC);	
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if ((offset + count) > data.Length)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, count));
			}

			if ((size * count) > SizeInBytes)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
#endif
			Graphics.D3DDeviceContext.UpdateSubresource(data, D3DResource, 0, size);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(GorgonConstantBuffer other)
		{
			return other == this;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer" /> class.
		/// </summary>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
		/// <param name="info">Information used to create the buffer.</param>
		/// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is empty.</exception>
		/// <exception cref="GorgonException">
		/// Thrown when the size of the constant buffer exceeds the maximum constant buffer size. See <see cref="IGorgonVideoDevice.MaxConstantBufferSize"/> to determine the maximum size of a constant buffer.
		/// </exception>
		public GorgonConstantBuffer(string name, GorgonGraphics graphics, IGorgonConstantBufferInfo info, GorgonPointerBase initialData = null, IGorgonLog log = null)
			: base(graphics, name, log)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (info.SizeInBytes < 16)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 16));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			_info = new GorgonConstantBufferInfo(info);

			Initialize(initialData);
		}
		#endregion
	}
}
