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
using System.Runtime.CompilerServices;
using DX = SharpDX;
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
	/// To send changing data to a shader from you application to a constant buffer, an application can upload a value type (or primitive) value to the buffer using one of the 
	/// <see cref="O:Gorgon.Graphics.GorgonBufferCommon.SetData{T}"/> methods. This allows an application to update the state of a shader to reflect changes in the application. Things like animation or setup 
	/// information can easily be sent to modify the state of a shader (hence somewhat making the term <i>constant</i> a bit of a misnomer).
	/// </para>
	/// <para>
	/// A constant buffer must be a minimum of 16 bytes in size (4 float values), and be aligned to 16 bytes. The maximum size of the buffer is limited to 134,217,728 elements (one element = 4x 32 bit float
	/// values). However, the GPU can only address 4096 (64K) of these elements at a time. As such the buffer will have to be bound to the GPU using a range via the <see cref="TODO No idea yet"/> if its size
	/// exceeds 4096 elements. 
	/// </para>
	/// <para>
	/// If the <see cref="IGorgonConstantBufferInfo"/> <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> value is less than 16 bytes, or is not aligned to 16 bytes, it will be adjusted before buffer
	/// creation to ensure the buffer is adequate.
	/// </para>
	/// <para>
	/// Constant buffers are bound to a finite number of slots in the shader. Typically these are declared as follows:
	/// <pre>
	/// cbuffer ViewMatrix : register(b0)
	/// {
	///	   Matrix4x4 viewMatrix;
	///    Matrix4x4 other;
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
	///		_viewMatrixBuffer.SetData<Matrix>(ref viewMatrix);
	/// 
	///		// Send again to the shader, but this time, at the fourth float value index.
	///     // This would skip the first 4 float values in viewMatrix, and write into
	///     // the remaining 60 float values, and the first 4 float values in "other".
	///		_viewMatrixBuffer.SetData<Matrix>(ref viewMatrix, 4 * sizeof(float));
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// </para>
	/// </remarks>
	/// <seealso cref="O:Gorgon.Graphics.GorgonBufferCommon.SetData{T}"/>
	public sealed class GorgonConstantBuffer
		: GorgonBufferCommon, IGorgonConstantBufferInfo
	{
        #region Constants.
	    /// <summary>
	    /// The prefix to assign to a default name.
	    /// </summary>
	    internal const string NamePrefix = nameof(GorgonConstantBuffer);
        #endregion

		#region Variables.
		// The information used to create the buffer.
		private readonly GorgonConstantBufferInfo _info;
        #endregion

        #region Properties.
	    /// <summary>
	    /// Property to return whether or not the user has requested that the buffer be readable from the CPU.
	    /// </summary>
	    public override bool IsCpuReadable => Usage == ResourceUsage.Staging;

	    /// <summary>
	    /// Property to return the usage for the resource.
	    /// </summary>
	    public override ResourceUsage Usage => _info.Usage;

	    /// <summary>
	    /// Property to return the size, in bytes, of the resource.
	    /// </summary>
	    public override int SizeInBytes => _info.SizeInBytes;

	    /// <summary>
	    /// Property to return the name of this object.
	    /// </summary>
	    public override string Name => _info.Name;

        /// <summary>
        /// Property to return the number of floating point values that can be stored in this buffer.
        /// </summary>
	    public int FloatElementCount
	    {
	        get;
	        private set;
	    }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the buffer data.
        /// </summary>
        /// <param name="initialData">The initial data used to populate the buffer.</param>
        private void Initialize(GorgonNativeBuffer<byte> initialData)
		{
			// If the buffer is not aligned to 16 bytes, then pad the size.
			_info.SizeInBytes = (_info.SizeInBytes + 15) & ~15;

		    FloatElementCount = _info.SizeInBytes / sizeof(float);

			D3D11.CpuAccessFlags cpuFlags = GetCpuFlags(false, D3D11.BindFlags.ConstantBuffer);

			Log.Print($"{Name} Constant Buffer: Creating D3D11 buffer. Size: {_info.SizeInBytes} bytes", LoggingLevel.Simple);

			D3D11.BufferDescription desc  = new D3D11.BufferDescription
			{
				SizeInBytes = _info.SizeInBytes,
				Usage = (D3D11.ResourceUsage)_info.Usage,
				BindFlags = D3D11.BindFlags.ConstantBuffer,
				OptionFlags = D3D11.ResourceOptionFlags.None,
				CpuAccessFlags = cpuFlags,
				StructureByteStride = 0
			};

			if ((initialData != null) && (initialData.Length > 0))
			{
			    unsafe
			    {
			        D3DResource = Native = new D3D11.Buffer(Graphics.D3DDevice, new IntPtr((void*)initialData), desc)
			                               {
			                                   DebugName = Name
			                               };
			    }
			}
			else
			{
			    D3DResource = Native = new D3D11.Buffer(Graphics.D3DDevice, desc)
			                           {
			                               DebugName = Name
			                           };
			}
		}

	    /// <summary>
	    /// Function to retrieve a copy of this buffer as a staging resource.
	    /// </summary>
	    /// <returns>The staging buffer to retrieve.</returns>
	    protected override GorgonBufferCommon GetStagingInternal()
	    {
	        return GetStaging();
	    }

	    /// <summary>
	    /// Function to copy the memory pointed at by a pointer to this buffer.
	    /// </summary>
	    /// <param name="ptr">The pointer to the memory to read.</param>
	    /// <param name="offset">The offset, in bytes, in the buffer to start writing into.</param>
	    /// <param name="typeSize">The size of an element of data.</param>
	    /// <param name="count">The number of elements to copy.</param>
	    /// <param name="copyMode">The type of copying.</param>
	    private unsafe void WritePtr(byte* ptr, int offset, int typeSize, int count, CopyMode copyMode)
	    {
	        int totalSize = count * typeSize;

	        if (Usage == ResourceUsage.Default)
	        {
	            Graphics.D3DDeviceContext.UpdateSubresource1(Native,
	                                                         0,
	                                                         new D3D11.ResourceRegion
	                                                         {
	                                                             Left = offset,
	                                                             Top = 0,
	                                                             Front = 0,
	                                                             Right = offset + totalSize,
	                                                             Bottom = 1,
	                                                             Back = 1
	                                                         },
	                                                         new IntPtr(ptr),
	                                                         totalSize,
	                                                         totalSize,
	                                                         (int)copyMode);
	            return;
	        }

	        D3D11.MapMode mapMode = D3D11.MapMode.Write;

	        if (Usage == ResourceUsage.Dynamic)
	        {
	            switch (copyMode)
	            {
	                case CopyMode.NoOverwrite:
	                    mapMode = D3D11.MapMode.WriteNoOverwrite;
	                    break;
	                default:
	                    mapMode = D3D11.MapMode.WriteDiscard;
	                    break;
	            }
	        }

	        DX.DataBox mapData = Graphics.D3DDeviceContext.MapSubresource(Native, 0, mapMode, D3D11.MapFlags.None);
	        try
	        {
	            byte* destPtr = (byte*)mapData.DataPointer + offset;
	            Unsafe.CopyBlock(destPtr, ptr, (uint)totalSize);
	        }
	        finally
	        {
	            Graphics.D3DDeviceContext.UnmapSubresource(Native, 0);
	        }
	    }

	    /// <summary>
	    /// Function to retrieve a copy of this buffer as a staging resource.
	    /// </summary>
	    /// <returns>The staging buffer to retrieve.</returns>
	    public GorgonConstantBuffer GetStaging()
	    {
	        GorgonConstantBuffer buffer = new GorgonConstantBuffer(Graphics,
	                                                               new GorgonConstantBufferInfo(_info, $"{Name}_Staging")
	                                                               {
	                                                                   Usage = ResourceUsage.Staging
	                                                               });

	        CopyTo(buffer);

	        return buffer;
	    }
	    #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConstantBuffer" /> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="info">Information used to create the buffer.</param>
        /// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
        /// <exception cref="GorgonException">
        /// Thrown when the size of the constant buffer exceeds the maximum constant buffer size. See <see cref="IGorgonVideoAdapterInfo.MaxConstantBufferSize"/> to determine the maximum size of a constant buffer.
        /// </exception>
        public GorgonConstantBuffer(GorgonGraphics graphics, IGorgonConstantBufferInfo info, GorgonNativeBuffer<byte> initialData = null)
			: base(graphics)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			_info = new GorgonConstantBufferInfo(info);

			Initialize(initialData);
		}
		#endregion
	}
}
