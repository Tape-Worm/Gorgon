#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Thursday, January 05, 2012 8:31:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	public class GorgonConstantBuffer
		: GorgonNamedObject, IDisposable, INotifier
	{
		#region Variables.
		private bool _disposed = false;				// Flag to indicate that the buffer was disposed.
		private D3D.Buffer _buffer = null;			// Buffer to use.
		private GorgonDataStream _stream = null;	// Local buffer for constants.
		private GorgonGraphics _graphics = null;	// Graphics interface to use.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the data buffer.
		/// </summary>
		internal D3D.Buffer D3DBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the back end buffers.
		/// </summary>
		internal void CleanUp()
		{
			if (_buffer != null)
				_buffer.Dispose();
			_buffer = null;
		}

		/// <summary>
		/// Function to update a variable in the buffer.
		/// </summary>
		/// <param name="variable">Variable to update.</param>
		internal void UpdateVariable<T>(GorgonShaderVariable variable)
		{
			SharpDX.Direct3D11.Effect e;
			
			e.GetVariableByName().AsBlend().TypeInfo.
			_graphics.Context.UpdateSubresource(new DX.DataBox(_stream.Handle, 0, 0), _buffer);
			variable.HasChanged = false;
		}

		/// <summary>
		/// Function to initialize the buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		internal void Initialize(int size)
		{
			D3D.BufferDescription bufferDesc = new D3D.BufferDescription();

			bufferDesc.BindFlags = D3D.BindFlags.ConstantBuffer;
			bufferDesc.CpuAccessFlags = D3D.CpuAccessFlags.None;
			bufferDesc.OptionFlags = D3D.ResourceOptionFlags.None;
			bufferDesc.SizeInBytes = size;
			bufferDesc.Usage = D3D.ResourceUsage.Default;
			bufferDesc.StructureByteStride = 0;

			_buffer = new D3D.Buffer(_graphics.VideoDevice.D3DDevice, bufferDesc);
			_stream = new GorgonDataStream(size);
			DirectAccess.ZeroMemory(_stream.Handle, size);
			_graphics.Context.UpdateSubresource(new DX.DataBox(_stream.Handle, 0, 0), _buffer);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="graphics">Graphics interface.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonConstantBuffer(string name, GorgonGraphics graphics)
			: base(name)
		{
			_graphics = graphics;
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
					if (_stream != null)
						_stream.Dispose();
					if (_buffer != null)
						_buffer.Dispose();
				}

				_stream = null;
				_buffer = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
