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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A buffer object used store varying kinds of data that can be bound with the graphics pipeline.
	/// </summary>
	public class GorgonBuffer
		: GorgonResource
	{
		#region Variables.
		// The log interface used for debug logging.
		private readonly IGorgonLog _log;
		// The information used to create the buffer.
		private readonly GorgonBufferInfo _info;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType => ResourceType.Buffer;

		/// <summary>
		/// Property used to return the information used to create this buffer.
		/// </summary>
		public GorgonBufferInfo Info => _info;

		/// <summary>
		/// Property to return the size, in bytes, of the resource.
		/// </summary>
		public override int SizeInBytes => _info.SizeInBytes;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize a buffer used to store shader constant data.
		/// </summary>
		/// <param name="initialData">The data used to initialize the buffer.</param>
		private void InitializeConstantBuffer(GorgonPointer initialData)
		{
			// If the buffer is not aligned to 16 bytes, then increase its size.
			_info.SizeInBytes = ((_info.SizeInBytes + 15) & ~15);

			if (_info.SizeInBytes > VideoDevice.MaxConstantBufferSize)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CONSTANT_BUFFER_TOO_LARGE, _info.SizeInBytes, VideoDevice.MaxConstantBufferSize));
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
		
			var desc = new D3D11.BufferDescription
			           {
				           SizeInBytes = _info.SizeInBytes,
				           Usage = (D3D11.ResourceUsage)_info.Usage,
				           BindFlags = D3D11.BindFlags.ConstantBuffer,
				           OptionFlags = D3D11.ResourceOptionFlags.None,
				           CpuAccessFlags = cpuFlags,
				           StructureByteStride = 0
			           };

			// Send over initial data.
			IntPtr pointer = initialData == null ? IntPtr.Zero : new IntPtr(initialData.Address);

			_log.Print($"{Name} Constant Buffer: Creating D3D11 constant buffer. Size: {_info.SizeInBytes} bytes", LoggingLevel.Simple);
			D3DResource = new D3D11.Buffer(VideoDevice.D3DDevice(), pointer, desc);
		}
		
		/// <summary>
		/// Function to initialize the buffer data.
		/// </summary>
		/// <param name="initialData">The initial data used to populate the buffer.</param>
		private void Initialize(GorgonPointer initialData)
		{
			switch (Info.BufferType)
			{
				case BufferType.Constant:
					InitializeConstantBuffer(initialData);
					break;
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonResource" /> class.
		/// </summary>
		/// <param name="videoDevice">The video device interface that owns this object.</param>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="info">Information used to create the buffer.</param>
		/// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="videoDevice"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is empty.</exception>
		/// <exception cref="GorgonException">
		/// Thrown when the size of the constant buffer exceeds the maximum constant buffer size. See <see cref="IGorgonVideoDevice.MaxConstantBufferSize"/> to determine the maximum size of a constant buffer.
		/// </exception>
		public GorgonBuffer(IGorgonVideoDevice videoDevice, string name, GorgonBufferInfo info, GorgonPointer initialData = null, IGorgonLog log = null)
			: base(videoDevice, name)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			_info = new GorgonBufferInfo
			        {
				        Usage = info.Usage,
				        BufferType = info.BufferType,
				        SizeInBytes = info.SizeInBytes
			        };

			Initialize(initialData);
		}
		#endregion
	}
}
