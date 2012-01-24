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
	/// <para>In order to write to a constant buffer, the user must <see cref="GorgonLibrary.Graphics.GorgonConstantBuffer.GetBuffer">lock</see> the buffer beforehand, and unlock it when done.  Failure to do so will result in an exception.</para>
	/// <para>Constant buffers follow very specific rules, which are explained at http://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx </para>
	/// <para>When passing a value type to the constant buffer, ensure that the type has a System.Runtime.InteropServices.StructLayout attribute assigned to it, and that the layout is explicit.  Also, the size of the 
	/// value type must be a multiple of 16, so padding variables may be required.</para>
	/// </remarks>
	public class GorgonConstantBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the buffer was disposed.
		private GorgonBufferStream<GorgonConstantBuffer> _data = null;		// Constant buffer data stream.
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
					D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, dxStream, desc);
			}
			else
				D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, desc);

#if DEBUG
			D3DBuffer.DebugName = "Gorgon Constant Buffer #" + Graphics.TrackedObjects.Count(item => item is GorgonConstantBuffer).ToString();
#endif
		}

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>A data stream containing the buffer data.</returns>		
		protected override void LockBuffer(BufferLockFlags lockFlags)
		{
			if (_data != null)
			{
				_data.IsPersistent = false;
				_data.Dispose();
				_data = null;
			}

			DX.DataStream dxStream = null;

			Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None, out dxStream);
			_data = new GorgonBufferStream<GorgonConstantBuffer>(this, dxStream);
			_data.IsPersistent = false;

			return;
		}

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal override void UnlockBuffer()
		{
			Graphics.Context.UnmapSubresource(D3DBuffer, 0);
			IsLocked = false;			
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
					if (IsLocked)
					{
						if (_data != null)
						{
							_data.IsPersistent = false;
							_data.Dispose();
						}
					}

					if (D3DBuffer != null)
						D3DBuffer.Dispose();
				}

				_data = null;
				D3DBuffer = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="destIndex">Index in the destination buffer to start writing at.</param>
		/// <param name="range2D">2D constraints for the buffer.</param>
		/// <param name="rangeDepth">Depth constraints for the buffer.</param>
		protected override void UpdateBuffer(GorgonDataStream stream, int destIndex, System.Drawing.Rectangle range2D, GorgonLibrary.Math.GorgonMinMax rangeDepth)
		{
			Graphics.Context.UpdateSubresource(new DX.DataBox() { DataPointer = stream.BasePointer, RowPitch = 0, SlicePitch = 0 }, D3DBuffer, 0);
		}

		/// <summary>
		/// Function to update data in the buffer.
		/// </summary>
		/// <param name="stream">Stream used to update the data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> is NULL (or Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown if the constant buffer is accessible by the CPU.</exception>
		/// <remarks>Use this to update the entire buffer.</remarks>
		public void UpdateData(GorgonDataStream stream)
		{
			GorgonDebug.AssertNull<GorgonDataStream>(stream, "stream");

			if (BufferUsage == GorgonLibrary.Graphics.BufferUsage.Dynamic)
				throw new InvalidOperationException("Cannot update a constant buffer that is accessible by the CPU.");
			UpdateBuffer(stream, 0, System.Drawing.Rectangle.Empty, GorgonLibrary.Math.GorgonMinMax.Empty);
		}

		/// <summary>
		/// Function to prepare the buffer for writing.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer is already locked.
		/// <para>-or-</para>
		/// <para>Thrown when the buffer is not accessible to the CPU.</para>
		/// </exception>
		/// <returns>Returns a constant buffer stream used to write into the buffer.</returns>
		/// <remarks>Once done with writing to the buffer, ensure that the buffer is disposed so that the data can be uploaded to the video device.</remarks>		
		public GorgonBufferStream<GorgonConstantBuffer> GetBuffer()
		{
			if (IsLocked)
				throw new InvalidOperationException("The buffer is already locked.");
			if (BufferUsage == GorgonLibrary.Graphics.BufferUsage.Default)
				throw new InvalidOperationException("Cannot update a constant buffer that is not accessible by the CPU.");

			LockBuffer(BufferLockFlags.Discard | BufferLockFlags.Write);
			IsLocked = true;

			return _data;
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
			: base(graphics, (allowCPUWrite ? BufferUsage.Dynamic : BufferUsage.Default))
		{
			if ((Size % 16) != 0)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the constant buffer.  The buffer size (" + size.ToString() + " bytes) need be evenly divisible by 16.");
			Size = size;
		}
		#endregion
	}
}
