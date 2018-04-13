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
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;

namespace Gorgon.Graphics.Core
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
	public sealed class GorgonConstantBuffer
		: GorgonBufferBase
	{
		#region Variables.
		// The information used to create the buffer.
		private readonly GorgonConstantBufferInfo _info;
        #endregion

        #region Properties.
	    /// <summary>
	    /// Property to return whether or not the resource can be bound as a shader resource.
	    /// </summary>
	    protected internal override bool IsShaderResource => false;

        /// <summary>
        /// Property to return whether or not the resource can be used in an unordered access view.
        /// </summary>
        protected internal override bool IsUavResource => false;

	    /// <summary>
        /// Property to return the usage flags for the buffer.
        /// </summary>
        internal override ResourceUsage Usage => _info.Usage;

	    /// <summary>
	    /// Property to return whether or not the user has requested that the buffer be readable from the CPU.
	    /// </summary>
	    internal override bool RequestedCpuReadable => false;

	    /// <summary>
        /// Property used to return the information used to create this buffer.
        /// </summary>
        public IGorgonConstantBufferInfo Info => _info;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the buffer data.
		/// </summary>
		/// <param name="initialData">The initial data used to populate the buffer.</param>
		private void Initialize(IGorgonPointer initialData)
		{
			// If the buffer is not aligned to 16 bytes, then increase its size.
			SizeInBytes = (Info.SizeInBytes + 15) & ~15;

			if (SizeInBytes > Graphics.VideoAdapter.MaxConstantBufferSize)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CONSTANT_BUFFER_TOO_LARGE, SizeInBytes, Graphics.VideoAdapter.MaxConstantBufferSize));
			}
			
			D3D11.CpuAccessFlags cpuFlags = GetCpuFlags(false, D3D11.BindFlags.ConstantBuffer);

			Log.Print($"{Name} Constant Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

			D3D11.BufferDescription desc  = new D3D11.BufferDescription
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
			    D3DResource = NativeBuffer = new D3D11.Buffer(Graphics.D3DDevice, new IntPtr(initialData.Address), desc)
			                              {
			                                  DebugName = Name
			                              };
			}
			else
			{
			    D3DResource = NativeBuffer = new D3D11.Buffer(Graphics.D3DDevice, desc)
			                              {
			                                  DebugName = Name
			                              };
			}
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
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
		/// <exception cref="GorgonException">
		/// Thrown when the size of the constant buffer exceeds the maximum constant buffer size. See <see cref="IGorgonVideoAdapterInfo.MaxConstantBufferSize"/> to determine the maximum size of a constant buffer.
		/// </exception>
		public GorgonConstantBuffer(string name, GorgonGraphics graphics, IGorgonConstantBufferInfo info, IGorgonPointer initialData = null, IGorgonLog log = null)
			: base(name, graphics, log)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (info.SizeInBytes < 16)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_SIZE_TOO_SMALL, 16));
			}

            BufferType = BufferType.Constant;

			_info = new GorgonConstantBufferInfo(info);

			Initialize(initialData);
		}
		#endregion
	}
}
