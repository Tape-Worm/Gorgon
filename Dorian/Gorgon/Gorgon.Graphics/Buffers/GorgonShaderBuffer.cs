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
// Created: Monday, November 5, 2012 8:58:47 PM
// 
#endregion

using System;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer for shaders.
	/// </summary>
	/// <remarks>This is a base object for buffers that can be bound to a shader.</remarks>
	public abstract class GorgonShaderBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private DX.DataStream _lockStream;							// Lock stream.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the binding flags for the buffer.
		/// </summary>
		internal virtual D3D.BindFlags BindingFlags
		{
			get
			{
				if (IsUnorderedAccess)
					return D3D.BindFlags.ShaderResource | D3D.BindFlags.UnorderedAccess;
				else
					return D3D.BindFlags.ShaderResource;
			}
		}

		/// <summary>
		/// Property to return whether there's unordered access to the default view of the buffer.
		/// </summary>
		/// <remarks>Unordered access is only available to video devices that support SM5 or better.</remarks>
		public bool IsUnorderedAccess
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to set or return the view for the resource.
		/// </summary>
		public override GorgonResourceView View
		{
			get
			{
				return base.View;
			}
			set
			{
				if (BufferUsage != BufferUsage.Staging)
				{
					base.View = value;
				}
				else
				{
					base.View = null;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			// If we're bound with a pixel or vertex shader, then unbind.
			Graphics.Shaders.VertexShader.Resources.Unbind(this);
			Graphics.Shaders.PixelShader.Resources.Unbind(this);

			if (IsLocked)
				Unlock();

			if (View != null)
			{
				View = null;
			}

			if (DefaultView != null)
			{
				Gorgon.Log.Print("Gorgon default resource view {0}: Destroying default resource view.", Diagnostics.LoggingLevel.Verbose, DefaultView.Name); 
				DefaultView.Dispose();
				DefaultView = null;
			}

			if (D3DResource != null)
			{
				D3DResource.Dispose();
				D3DResource = null;
			}
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
				throw new ArgumentException("A structured buffer must be locked with the Write and Discard flags.", "lockFlags");

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
				D3DResource);
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
		/// Initializes a new instance of the <see cref="GorgonShaderBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU write access to the buffer, FALSE to disallow.</param>
		/// <param name="isUnorderedAccess">TRUE to allow unordered access to the buffer, FALSE to disallow.</param>
		/// <param name="size">Size of the buffer, in bytes.</param>
		protected GorgonShaderBuffer(GorgonGraphics graphics, bool allowCPUWrite, bool isUnorderedAccess, int size)
			: base(graphics, (allowCPUWrite ? BufferUsage.Dynamic : BufferUsage.Default), size)
		{
			if ((isUnorderedAccess) && (graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5))
				throw new ArgumentException("Cannot use unordered access buffers with video devices that are not SM5 or better.", "isUnorderedAccess");

			IsUnorderedAccess = isUnorderedAccess;
		}
		#endregion
	}
}
