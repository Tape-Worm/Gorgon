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
// Created: Thursday, December 15, 2011 1:44:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A constant buffer for shaders.
	/// </summary>
	/// <remarks>Constant buffers are used to send groups of scalar values to a shader.  The buffer is just a block of allocated memory that is written to by one of the various Write methods.
	/// <para>Typically, the user will define a value type that matches a constant buffer layout.  Then, if the value type uses nothing but blittable types, the user can then write the entire 
	/// value type structure to the constant buffer.  If the value type contains more complex types, such as arrays, then the user can write each item in the value type to a variable in the constant 
	/// buffer.  Please note that the names for the variables in the value type and the shader do -not- have to match, although, for the sake of clarity, it is a good idea that they do.</para>
	/// <para>In order to write to a constant buffer, the user must <see cref="GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">lock</see> the buffer beforehand, and unlock it when done.  Failure to do so will result in an exception.</para>
	/// <para>Constant buffers follow very specific rules, which are explained at http://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx </para>
	/// <para>When passing a value type to the constant buffer, ensure that the type has a System.Runtime.InteropServices.StructLayout attribute assigned to it, and that the layout is explicit.  Also, the size of the 
	/// value type must be a multiple of 16, so padding variables may be required.</para>
	/// </remarks>
	public class GorgonConstantBuffer
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the buffer was disposed.
		private GorgonConstantBufferStream _data = null;		// Constant buffer data stream.
		private bool _locked = false;							// Flag to indicate that the buffer was locked for writing.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D buffer.
		/// </summary>
		internal D3D.Buffer D3DBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer is locked for read/write.
		/// </summary>
		public bool IsLocked
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of data used for the constant buffer.
		/// </summary>
		public Type DataType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to allow CPU write access.
		/// </summary>
		public bool AllowCPUWrite
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the buffer.
		/// </summary>
		/// <param name="value">Value used to initialize the buffer.</param>
		internal void Initialize(GorgonDataStream value)
		{
			D3D.ResourceUsage usage = D3D.ResourceUsage.Default;
			D3D.CpuAccessFlags cpuFlags = D3D.CpuAccessFlags.None;

			if (D3DBuffer != null)
				D3DBuffer.Dispose();

			if (AllowCPUWrite)
			{
				usage = D3D.ResourceUsage.Dynamic;
				cpuFlags = D3D.CpuAccessFlags.Write;
			}

			if (value != null)
			{
				using (DX.DataStream dxStream = new DX.DataStream(value.BasePointer, value.Length, true, true))
				{
					D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, dxStream, new D3D.BufferDescription()
					{
						BindFlags = D3D.BindFlags.ConstantBuffer,
						CpuAccessFlags = cpuFlags,
						OptionFlags = D3D.ResourceOptionFlags.None,
						SizeInBytes = Size,
						StructureByteStride = 0,
						Usage = usage
					});
				}
			}
			else
			{
				D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.ConstantBuffer,
					CpuAccessFlags = cpuFlags,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = Size,
					StructureByteStride = 0,
					Usage = usage
				});
			}

			D3DBuffer.DebugName = "Gorgon Constant Buffer '" + DataType.FullName + "'";
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		/// <returns>Returns a constant buffer stream used to write into the buffer.</returns>
		public GorgonConstantBufferStream Lock()
		{
			if (_locked)
				return _data;

			if (!AllowCPUWrite)
			{
				if (_data == null)
					_data = new GorgonConstantBufferStream(this, Size);

				_locked = true;
				_data.Position = 0;
				return _data;
			}

			if (_data != null)
			{
				_data.Dispose();
				_data = null;
			}

			DX.DataStream dxStream = null;

			Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None, out dxStream);
			_data = new GorgonConstantBufferStream(this, dxStream);
			_locked = true;

			return _data;
		}

		/// <summary>
		/// Function unlock the buffer after writing is complete.
		/// </summary>
		public void Unlock()
		{
			if (!_locked)
				return;

			if (!AllowCPUWrite)
			{
				Graphics.Context.UpdateSubresource(new DX.DataBox(_data.BasePointer, 0, 0), D3DBuffer, 0);
				_locked = false;
				return;
			}

			_data.Dispose();
			_data = null;
			Graphics.Context.UnmapSubresource(D3DBuffer, 0);
			_locked = false;
		}

		/// <summary>
		/// Function to retrieve data about the type used for the constant buffer.
		/// </summary>
		private void GetTypeData()
		{
			if (!DataType.IsExplicitLayout)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot use the type '" + DataType.FullName + "'.  The type must have a System.RuntimeInteropServices.StructLayout attribute, and must use an explicit layout");

			Size = DataType.StructLayoutAttribute.Size;
			if (Size <= 0)
				Size = Marshal.SizeOf(DataType);

			if (((Size % 16) != 0) || (Size == 0))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot use the type '" + DataType.FullName + "'.  The size of the type (" + Size.ToString() + " bytes) is not on a 16 byte boundary or is 0.");
		}			
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU write access to the buffer, FALSE to disallow.</param>
		/// <param name="type">Type of data to write to the constant buffer.</param>
		internal GorgonConstantBuffer(GorgonGraphics graphics, bool allowCPUWrite, Type type)
		{
			Graphics = graphics;
			AllowCPUWrite = allowCPUWrite;
			DataType = type;
			GetTypeData();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_locked)
						_locked = false;

					if (_data != null)
						_data.Dispose();

					if (D3DBuffer != null)
						D3DBuffer.Dispose();					
				}

				D3DBuffer = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
