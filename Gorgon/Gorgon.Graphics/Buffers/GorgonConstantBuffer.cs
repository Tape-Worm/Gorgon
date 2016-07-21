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
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using SharpDX;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A constant buffer used to send constant data to a shader.
	/// </summary>
	public class GorgonConstantBuffer
		: GorgonBuffer
	{
		#region Variables.
		// The log interface used for debug logging.
		private readonly IGorgonLog _log;
		// The information used to create the buffer.
		private readonly GorgonConstantBufferInfo _info;
		// The graphic interface used to create and manipulate this buffer.
		private readonly GorgonGraphics _graphics;
		#endregion

		#region Properties.
		/// <summary>
		/// Property used to return the information used to create this buffer.
		/// </summary>
		public GorgonConstantBufferInfo Info => _info;

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

			if (SizeInBytes > VideoDevice.MaxConstantBufferSize)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CONSTANT_BUFFER_TOO_LARGE, SizeInBytes, VideoDevice.MaxConstantBufferSize));
			}

			D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

			switch (_info.Usage)
			{
				case BufferUsage.Staging:
					cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
					break;
				case BufferUsage.Dynamic:
					cpuFlags = D3D11.CpuAccessFlags.Write;
					break;
			}

			_log.Print($"{Name} Constant Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

			var desc  = new D3D11.BufferDescription
			{
				SizeInBytes = SizeInBytes,
				Usage = (D3D11.ResourceUsage)_info.Usage,
				BindFlags = D3D11.BindFlags.ConstantBuffer,
				OptionFlags = D3D11.ResourceOptionFlags.None,
				CpuAccessFlags = cpuFlags,
				StructureByteStride = 0
			};

			if ((initialData != null) && (initialData.Size > 0))
			{
				D3DResource = D3DBuffer = new D3D11.Buffer(VideoDevice.D3DDevice(), new IntPtr(initialData.Address), desc);
			}
			else
			{
				D3DResource = D3DBuffer = new D3D11.Buffer(VideoDevice.D3DDevice(), desc);
			}
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <param name="data">The data used to populate the buffer.</param>
		public void Update<T>(ref T data)
			where T : struct
		{
			int size = DirectAccess.SizeOf<T>();

#if DEBUG
			if (size > SizeInBytes)
			{
				throw new ArgumentOutOfRangeException(nameof(data));
			}
#endif

			_graphics.D3DDeviceContext.UpdateSubresource(ref data, D3DResource, 0, size);
		}

		/// <summary>
		/// Function to update the constant buffer data with data from native memory.
		/// </summary>
		/// <param name="data">The <see cref="GorgonPointer"/> to the native memory holding the data to copy into the buffer.</param>
		public void Update(GorgonPointer data)
		{
			data.ValidateObject(nameof(data));
			Update(data, 0, (int)data.Size);
		}

		/// <summary>
		/// Function to update the constant buffer data with data from native memory.
		/// </summary>
		/// <param name="data">The <see cref="GorgonPointer"/> to the native memory holding the data to copy into the buffer.</param>
		/// <param name="offset">The offset, in bytes, to start copying from.</param>
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
		/// <param name="size">The size, in bytes to copy.</param>
		public void Update(GorgonPointer data, int offset, int size)
		{
			data.ValidateObject(nameof(data));

#if DEBUG
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

			_graphics.D3DDeviceContext.UpdateSubresource(new DataBox
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
		/// <param name="data">The data to populate the buffer with.</param>
		public void Update<T>(T[] data)
			where T : struct
		{
			data.ValidateObject(nameof(data));

			Update(data, 0, data.Length);			
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <param name="data">The data to populate the buffer with.</param>
		/// <param name="offset">The offset within the array to start copying from.</param>
		/// <param name="count">The number of elements to copy.</param>
		public void Update<T>(T[] data, int offset, int count)
			where T : struct
		{
			data.ValidateObject(nameof(data));

			int size = DirectAccess.SizeOf<T>();
			
#if DEBUG
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if ((offset + count) >= data.Length)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, count));
			}

			if ((size * count) > SizeInBytes)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
#endif
			_graphics.D3DDeviceContext.UpdateSubresource(data, D3DResource, 0, size);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer" /> class.
		/// </summary>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="info">Information used to create the buffer.</param>
		/// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is empty.</exception>
		/// <exception cref="GorgonException">
		/// Thrown when the size of the constant buffer exceeds the maximum constant buffer size. See <see cref="IGorgonVideoDevice.MaxConstantBufferSize"/> to determine the maximum size of a constant buffer.
		/// </exception>
		public GorgonConstantBuffer(GorgonGraphics graphics, string name, GorgonConstantBufferInfo info, GorgonPointerBase initialData = null, IGorgonLog log = null)
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

			_graphics = graphics;
			_log = log ?? GorgonLogDummy.DefaultInstance;
			_info = new GorgonConstantBufferInfo
			        {
				        Usage = info.Usage,
						SizeInBytes = info.SizeInBytes
			        };

			Initialize(initialData);
		}
		#endregion
	}
}
