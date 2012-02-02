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

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A constant buffer for shaders.
	/// </summary>
	/// <remarks>Constant buffers are used to send groups of scalar values to a shader.  The buffer is just a block of allocated memory that is written to by one of the various Write methods.
	/// <para>Typically, the user will define a value type that matches a constant buffer layout.  Then, if the value type uses nothing but blittable types, the user can then write the entire 
	/// value type structure to the constant buffer.  If the value type contains more complex types, such as arrays, then the user can write each item in the value type to a variable in the constant 
	/// buffer.  Please note that the names for the variables in the value type and the shader do -not- have to match, although, for the sake of clarity, it is a good idea that they do.</para>
	/// <para>In order to write to a constant buffer, the user must <see cref="M:GorgonLibrary.Graphics.GorgonBaseBuffer.Lock">lock</see> the buffer beforehand, and unlock it when done.  Failure to do so will result in an exception.</para>
	/// <para>Constant buffers follow very specific rules, which are explained at http://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx </para>
	/// <para>When passing a value type to the constant buffer, ensure that the type has a System.Runtime.InteropServices.StructLayout attribute assigned to it, and that the layout is explicit.  Also, the size of the 
	/// value type must be a multiple of 16, so padding variables may be required.</para>
	/// </remarks>
	public class GorgonConstantBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the buffer was disposed.
		private DX.DataStream _lockStream = null;							// Lock stream.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the buffer.
		/// </summary>
		/// <param name="value">Value used to initialize the buffer.</param>
		protected internal override void Initialize(GorgonDataStream value)
		{
			if (D3DBuffer != null)
				D3DBuffer.Dispose();

			D3D.BufferDescription desc = new D3D.BufferDescription()
			{
				BindFlags = D3D.BindFlags.ConstantBuffer,
				CpuAccessFlags = D3DCPUAccessFlags,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SizeInBytes = Size,
				StructureByteStride = 0,
				Usage = D3DUsage
			};

			if (value != null)
			{
				long position = value.Position;

				using (DX.DataStream dxStream = new DX.DataStream(value.BasePointer, value.Length - position, true, true))
					D3DBuffer = new D3D.Buffer(Graphics.D3DDevice, dxStream, desc);
			}
			else
				D3DBuffer = new D3D.Buffer(Graphics.D3DDevice, desc);

#if DEBUG
			D3DBuffer.DebugName = "Gorgon Constant Buffer #" + Graphics.TrackedObjects.Count(item => item is GorgonConstantBuffer).ToString();
#endif
		}

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>
		/// A data stream containing the buffer data.
		/// </returns>
		protected override GorgonDataStream LockImpl(BufferLockFlags lockFlags)
		{
			if (((lockFlags & BufferLockFlags.Discard) != BufferLockFlags.Discard) || ((lockFlags & BufferLockFlags.Write) != BufferLockFlags.Write))
				throw new ArgumentException("A constant buffer must be locked with the Write and Discard flags.", "lockFlags");

			Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None, out _lockStream);

			return new GorgonDataStream(_lockStream.DataPointer, (int)_lockStream.Length);
		}

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal override void UnlockImpl()
		{
			Graphics.Context.UnmapSubresource(D3DBuffer, 0);
			_lockStream.Dispose();
			_lockStream = null;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// If we're bound with a pixel or vertex shader, then unbind.
					Graphics.Shaders.VertexShader.ConstantBuffers.Unbind(this);
					Graphics.Shaders.PixelShader.ConstantBuffers.Unbind(this);

					if (IsLocked)
						Unlock();

					if (D3DBuffer != null)
						D3DBuffer.Dispose();
				}

				D3DBuffer = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		protected override void UpdateImpl(GorgonDataStream stream, int offset, int size)
		{
			Graphics.Context.UpdateSubresource(
				new DX.DataBox()
				{
					DataPointer = stream.PositionPointer,
					RowPitch = 0,
					SlicePitch = 0
				}, 
				D3DBuffer);
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer usage is not set to default.</exception>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public new void Update(GorgonDataStream stream, int offset, int size)
		{
			base.Update(stream, 0, 0);
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>This method will respect the <see cref="P:GorgonLibrary.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream)
		{
			Update(stream, 0, 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU write access to the buffer, FALSE to disallow.</param>
		internal GorgonConstantBuffer(GorgonGraphics graphics, int size, bool allowCPUWrite)
			: base(graphics, (allowCPUWrite ? BufferUsage.Dynamic : BufferUsage.Default), size)
		{
			if ((Size % 16) != 0)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the constant buffer.  The buffer size (" + size.ToString() + " bytes) need be evenly divisible by 16.");
		}
		#endregion
	}
}
