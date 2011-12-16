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
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A constant buffer for shaders.
	/// </summary>
	public class GorgonConstantBuffer
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the buffer was disposed.
		private SharpDX.DataStream _data = null;				// Stream for writing.
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
		/// Property to return the size of buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of an item in the buffer, in bytes.
		/// </summary>
		public int ItemSize
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of items in the buffer.
		/// </summary>
		public int Count
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
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		internal void Initialize<T>(T value)
			where T : struct
		{
			Type dataType = typeof(T);
			D3D.ResourceUsage usage = D3D.ResourceUsage.Default;
			D3D.CpuAccessFlags cpuFlags = D3D.CpuAccessFlags.None;

			if (D3DBuffer != null)
				D3DBuffer.Dispose();

			ItemSize = DirectAccess.SizeOf<T>();

			if ((ItemSize % 16) != 0)
				throw new GorgonException(GorgonResult.CannotCreate, "The size of '" + dataType.FullName + "' must be divisible by 16.");

			if (AllowCPUWrite)
			{
				usage = D3D.ResourceUsage.Dynamic;
				cpuFlags = D3D.CpuAccessFlags.Write;
			}
						
			_data = new SharpDX.DataStream(ItemSize, true, true);
			_data.Write<T>(value);
			_data.Position = 0;

			D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, _data, new D3D.BufferDescription()
			{
				BindFlags = D3D.BindFlags.ConstantBuffer,
				CpuAccessFlags = cpuFlags,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SizeInBytes = ItemSize,
				StructureByteStride = 0,
				Usage = usage
			});

			D3DBuffer.DebugName = "Gorgon Constant Buffer '" + dataType.FullName + "'";
			Size = D3DBuffer.Description.SizeInBytes;
			Count = Size / ItemSize;
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		private void Lock()
		{
			if (!AllowCPUWrite)
				return;

			if (_data != null)
			{
				_data.Dispose();
				_data = null;
			}
			
			Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.DoNotWait, out _data);
		}

		/// <summary>
		/// Function unlock the buffer after writing is complete.
		/// </summary>
		private void Unlock()
		{
			if (!AllowCPUWrite)
			{
				Graphics.Context.UpdateSubresource(new SharpDX.DataBox(_data.DataPointer, 0, 0), D3DBuffer, 0);
				return;
			}

			_data.Dispose();
			_data = null;
			Graphics.Context.UnmapSubresource(D3DBuffer, 0);
		}
		
		/// <summary>
		/// Function to write an item to the buffer.
		/// </summary>
		/// <param name="item">Item to write.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		public void Write<T>(T item)
			where T : struct
		{
			Lock();
			_data.Position = 0;
			_data.Write<T>(item);
			Unlock();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU write access to the buffer, FALSE to disallow.</param>
		internal GorgonConstantBuffer(GorgonGraphics graphics, bool allowCPUWrite)
		{
			Graphics = graphics;
			AllowCPUWrite = allowCPUWrite;
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
