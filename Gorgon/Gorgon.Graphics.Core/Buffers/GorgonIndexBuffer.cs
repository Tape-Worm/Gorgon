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
using Gorgon.Core;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A buffer for indices used to look up vertices within a <see cref="GorgonVertexBuffer"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use a index buffer to send indices to the GPU. These indices are used for allowing smaller vertex buffers and providing a faster means of finding vertices to draw on the GPU.
	/// </para>
	/// <para>
	/// To send indices to the GPU using a index buffer, an application can upload a value type values, representing the indices, to the buffer using one of the 
	/// <see cref="O:Gorgon.Graphics.GorgonIndexBuffer.Update{T}(ref T)">Update&lt;T&gt;</see> overloads. For best performance, it is recommended to upload index data only once, or rarely. However, in 
	/// some scenarios, and with the correct <see cref="IGorgonIndexBufferInfo.Usage"/> flag, indices can be updated regularly for things like dynamic tesselation of surface.
	/// </para>
	/// <para> 
	/// <example language="csharp">
	/// For example, to send a list of indices to a index buffer:
	/// <code language="csharp">
	/// <![CDATA[
	/// GorgonGraphics graphics;
	/// ushort[] _indices = new ushort[100];
	/// GorgonIndexBuffer _indexBuffer;
	/// 
	/// void InitializeIndexBuffer()
	/// {
	///		_indices = ... // Fill your index array here.
	/// 
	///		// Create the index buffer large enough so that it'll hold all 100 indices.
	///     // Unlike other buffers, we're passing the number of indices instead of bytes. 
	///     // This is because we can determine the number of bytes by whether we're using 
	///     // 16 bit indices (we are) and the index count.
	///		_indexBuffer = new GorgonIndexBuffer("MyIB", graphics, new GorgonIndexBufferInfo
	///	                                                               {
	///		                                                              IndexCount = _indices.Length
	///                                                                });
	/// 
	///		// Copy our data to the index buffer.
	///		_indexBuffer.Update(_indices);
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// </para>
	/// </remarks>
	public sealed class GorgonIndexBuffer
        : GorgonBufferBase
	{
		#region Variables.
		// The information used to create the buffer.
		private readonly GorgonIndexBufferInfo _info;
		// The size of an individual index.
		private readonly int _indexSize;
        #endregion

        #region Properties.
	    /// <summary>
	    /// Property to return whether or not the resource can be bound as a shader resource.
	    /// </summary>
	    protected internal override bool IsShaderResource => false;

	    /// <summary>
        /// Property to return whether or not the resource can be used in an unordered access view.
        /// </summary>
        protected internal override bool IsUavResource => (_info.Binding & VertexIndexBufferBinding.UnorderedAccess) == VertexIndexBufferBinding.UnorderedAccess;

        /// <summary>
        /// Property to return the usage flags for the buffer.
        /// </summary>
        protected override D3D11.ResourceUsage Usage => _info.Usage;

        /// <summary>
        /// Property to return the format of the buffer data when binding.
        /// </summary>
        internal BufferFormat IndexFormat => Info.Use16BitIndices ? BufferFormat.R16_UInt : BufferFormat.R32_UInt;

		/// <summary>
		/// Property used to return the information used to create this buffer.
		/// </summary>
		public IGorgonIndexBufferInfo Info => _info;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the buffer data.
		/// </summary>
		/// <param name="initialData">The initial data used to populate the buffer.</param>
		private void Initialize(IGorgonPointer initialData)
		{
			D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

			switch (_info.Usage)
			{
				case D3D11.ResourceUsage.Staging:
					cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
					break;
				case D3D11.ResourceUsage.Dynamic:
					cpuFlags = D3D11.CpuAccessFlags.Write;
					break;
			}

			Log.Print($"{Name} Index Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

		    D3D11.BindFlags bindFlags = D3D11.BindFlags.IndexBuffer;

		    if ((_info.Binding & VertexIndexBufferBinding.StreamOut) == VertexIndexBufferBinding.StreamOut)
		    {
		        bindFlags |= D3D11.BindFlags.StreamOutput;
		    }

		    if ((_info.Binding & VertexIndexBufferBinding.UnorderedAccess) == VertexIndexBufferBinding.UnorderedAccess)
		    {
		        bindFlags |= D3D11.BindFlags.UnorderedAccess;
		    }

            ValidateBufferBindings(_info.Usage, bindFlags);

            D3D11.BufferDescription desc  = new D3D11.BufferDescription
			{
				SizeInBytes = Info.IndexCount * _indexSize,
				Usage = _info.Usage,
				BindFlags = bindFlags,
				OptionFlags = D3D11.ResourceOptionFlags.None,
				CpuAccessFlags = cpuFlags,
				StructureByteStride = 0
			};

			if ((initialData != null) && (initialData.Size > 0))
			{
			    D3DResource = NativeBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), new IntPtr(initialData.Address), desc)
			                              {
			                                  DebugName = Name
			                              };
			}
			else
			{
			    D3DResource = NativeBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), desc)
			                              {
			                                  DebugName = Name
			                              };
			}

			SizeInBytes = desc.SizeInBytes;
		}

	    /// <summary>
	    /// Function to create a new <see cref="GorgonIndexBufferUav"/> for this buffer.
	    /// </summary>
	    /// <param name="startElement">[Optional] The first element to start viewing from.</param>
	    /// <param name="elementCount">[Optional] The number of elements to view.</param>
	    /// <returns>A <see cref="GorgonIndexBufferUav"/> used to bind the buffer to a shader.</returns>
	    /// <exception cref="GorgonException">Thrown if the video adapter does not support feature level 11 or better.
	    /// <para>-or-</para>
	    /// <para>Thrown when this buffer does not have a <see cref="VertexIndexBufferBinding"/> of <see cref="VertexIndexBufferBinding.UnorderedAccess"/>.</para>
	    /// <para>-or-</para>
	    /// <para>Thrown when this buffer has a usage of <c>Staging</c>.</para>
	    /// </exception>
	    /// <remarks>
	    /// <para>
	    /// This will create a unordered access view that makes a buffer accessible to compute shaders (or pixel shaders) using unordered access to the data. This allows viewing of the buffer data in a 
	    /// different format, or even a subsection of the buffer from within the shader.
	    /// </para>
	    /// <para>
	    /// The format of the view is based on whether the buffer uses 16 bit indices or 32 bit indices.
	    /// </para>
	    /// <para>
	    /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
	    /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
	    /// </para>
	    /// <para>
	    /// To determine how many elements are in a buffer, use the <see cref="IGorgonIndexBufferInfo.IndexCount"/> property on the <see cref="Info"/> property for this buffer.
	    /// </para>
	    /// <para>
	    /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
	    /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
	    /// </para>
	    /// <para>
	    /// <note type="important">
	    /// <para>
	    /// This method requires a video adapter capable of supporting feature level 11 or better. If the current video adapter does not support feature level 11, an exception will be thrown.
	    /// </para>
	    /// </note>
	    /// </para>
	    /// </remarks>
	    public GorgonIndexBufferUav GetUnorderedAccessView(int startElement = 0, int elementCount = 0)
	    {
	        if (Graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_UAV_REQUIRES_SM5);
	        }

	        if ((Info.Usage == D3D11.ResourceUsage.Staging)
	            || ((Info.Binding & VertexIndexBufferBinding.UnorderedAccess) != VertexIndexBufferBinding.UnorderedAccess))
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
	        }

            if ((Graphics.VideoDevice.GetBufferFormatSupport(IndexFormat) & BufferFormatSupport.TypedUnorderedAccessView) !=
	            BufferFormatSupport.TypedUnorderedAccessView)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, IndexFormat));
	        }

	        // Ensure the size of the data type fits the requested format.
	        GorgonFormatInfo info = new GorgonFormatInfo(IndexFormat);

	        startElement = startElement.Min(_info.IndexCount - 1).Max(0);

	        if (elementCount <= 0)
	        {
	            elementCount = _info.IndexCount - startElement;
	        }

	        elementCount = elementCount.Min(_info.IndexCount - startElement).Max(1);

	        BufferShaderViewKey key = new BufferShaderViewKey(startElement, elementCount, IndexFormat);

	        if (GetUav(key) is GorgonIndexBufferUav result)
	        {
	            return result;
	        }

	        result = new GorgonIndexBufferUav(this, startElement, elementCount, IndexFormat, info, Log);
	        result.CreateNativeView();
	        RegisterUav(key, result);

	        return result;
	    }

        /// <summary>
        /// Function to update the constant buffer data with data from native memory.
        /// </summary>
        /// <param name="data">The <see cref="GorgonPointer"/> to the native memory holding the data to copy into the buffer.</param>
        /// <param name="bufferOffset">[Optional] The number of bytes within this buffer to start writing at.</param>
		/// <param name="offset">[Optional] The offset, in bytes, to start copying from.</param>
        /// <param name="size">[Optional] The size, in bytes, to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/>, or the <paramref name="bufferOffset"/> plus the size of the data in <paramref name="data"/> exceed the size of this buffer.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter is larger than the total <see cref="GorgonGraphicsResource.SizeInBytes"/> of the buffer.
        /// <para>-or-</para>
        /// <para>The <paramref name="size"/> parameter is less than 1, or larger than the buffer size.</para>
        /// <para>-or-</para>
        /// <para>The <paramref name="offset"/> or the <paramref name="bufferOffset"/> parameter is less than 0.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonIndexBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to send a blob of byte data to the buffer. This allows for fine grained control over what gets sent to the buffer. 
        /// </para>
        /// <para>
        /// Because this is using native, unmanaged, memory, special care must be taken to ensure that the application does not attempt to read/write out of bounds of that memory region. Particular care must be 
        /// taken to ensure that <paramref name="offset"/>, <paramref name="bufferOffset"/> and <paramref name="size"/> do not exceed the bounds of the memory region.
        /// </para>
        /// <para>
        /// If the <paramref name="size"/> parameter is omitted (<b>null</b>), then the entire buffer size is used minus the <paramref name="offset"/>.
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a <see cref="IGorgonIndexBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void UpdateFromPointer(IGorgonPointer data, int bufferOffset = 0, int offset = 0, int? size = null)
		{
			data.ValidateObject(nameof(data));

		    if (size == null)
		    {
		        size = ((int)data.Size) - offset;
		    }

#if DEBUG
            if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_CANT_UPDATE_IMMUTABLE_OR_DYNAMIC);
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}

			if (bufferOffset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferOffset));
			}

			if (offset + size > data.Size)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, size));
			}

			if (bufferOffset + size > data.Size)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, offset, size));
			}
#endif

			Graphics.D3DDeviceContext.UpdateSubresource(new DX.DataBox
			                                            {
				                                            DataPointer = new IntPtr(data.Address + offset),
				                                            SlicePitch = 0,
				                                            RowPitch = size.Value
			                                            },
			                                            D3DResource, 
														0,
														new D3D11.ResourceRegion
														{
															Left = bufferOffset,
															Right = bufferOffset + size.Value,
															Top = 0,
															Front = 0,
															Back = 1,
															Bottom = 1
														});
		}

        /// <summary>
        /// Function to update the buffer with 16 bit index data.
        /// </summary>
        /// <param name="data">The array of index data to populate the buffer with.</param>
        /// <param name="bufferOffset">[Optional] The number of bytes within this buffer to start writing at.</param>
        /// <param name="startIndex">[Optional] The offset within the array to start copying from.</param>
        /// <param name="count">[Optional] The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/>, or the <paramref name="bufferOffset"/> plus the <paramref name="count"/> exceeds the number of elements in the <paramref name="data"/> parameter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter, multiplied by the number of items to copy is larger than the total <see cref="GorgonGraphicsResource.SizeInBytes"/> of the buffer.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="startIndex"/>, or the <paramref name="bufferOffset"/> is less than 0, or the <paramref name="count"/> is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the size of an index multiplied by the count (minus the offset) is larger than the buffer size.</para>
        /// </exception> 
        /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonIndexBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.
        /// <para>-or-</para>
        /// <para>The buffer was created as a 16 bit index buffer, use the <see cref="Update(int[],int,int,int?)"/> method instead.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will send 32 bit indices to the index buffer. If the <see cref="IGorgonIndexBufferInfo.Use16BitIndices"/> is <b>true</b>, then this method will throw an exception.
        /// </para>
        /// <para>
        /// If the <paramref name="count"/> parameter is omitted (<b>null</b>), then the length of the <paramref name="data"/> parameter is used minus the <paramref name="startIndex"/>.
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a <see cref="IGorgonIndexBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Update(short[] data, int bufferOffset = 0, int startIndex = 0, int? count = null)
        {
            data.ValidateObject(nameof(data));

            if (count == null)
            {
                count = data.Length - startIndex;
            }

#if DEBUG
            if (!Info.Use16BitIndices)
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_INDEX_BUFFER_TYPE_MISMATCH, 32, 16));
            }

            if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_CANT_UPDATE_IMMUTABLE_OR_DYNAMIC);
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (bufferOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferOffset));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if ((startIndex + count) > data.Length)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, startIndex, count));
            }

            if ((bufferOffset + count * _indexSize) > SizeInBytes)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, startIndex, count));
            }
#endif

            Graphics.D3DDeviceContext.UpdateSubresource(data,
                                                        D3DResource,
                                                        0,
                                                        0,
                                                        0,
                                                        new D3D11.ResourceRegion
                                                        {
                                                            Left = bufferOffset,
                                                            Right = bufferOffset + (count.Value * _indexSize),
                                                            Top = 0,
                                                            Front = 0,
                                                            Bottom = 1,
                                                            Back = 1
                                                        });
        }

        /// <summary>
        /// Function to update the buffer with 32 bit index data.
        /// </summary>
        /// <param name="data">The array of index data to populate the buffer with.</param>
        /// <param name="bufferOffset">[Optional] The number of bytes within this buffer to start writing at.</param>
		/// <param name="startIndex">[Optional] The offset within the array to start copying from.</param>
        /// <param name="count">[Optional] The number of elements to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/>, or the <paramref name="bufferOffset"/> plus the <paramref name="count"/> exceeds the number of elements in the <paramref name="data"/> parameter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the size, in bytes, of the <paramref name="data"/> parameter, multiplied by the number of items to copy is larger than the total <see cref="GorgonGraphicsResource.SizeInBytes"/> of the buffer.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="startIndex"/>, or the <paramref name="bufferOffset"/> is less than 0, or the <paramref name="count"/> is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the size of an index multiplied by the count (minus the offset) is larger than the buffer size.</para>
        /// </exception> 
        /// <exception cref="NotSupportedException">Thrown when the <see cref="IGorgonIndexBufferInfo.Usage"/> is either <c>Immutable</c> or <c>Dynamic</c>.
        /// <para>-or-</para>
        /// <para>The buffer was created as a 16 bit index buffer, use the <see cref="Update(short[],int,int,int?)"/> method instead.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will send 32 bit indices to the index buffer. If the <see cref="IGorgonIndexBufferInfo.Use16BitIndices"/> is <b>true</b>, then this method will throw an exception.
        /// </para>
        /// <para>
        /// If the <paramref name="count"/> parameter is omitted (<b>null</b>), then the length of the <paramref name="data"/> parameter is used minus the <paramref name="startIndex"/>.
        /// </para>
        /// <para>
        /// This method will throw an exception when the buffer is created with a <see cref="IGorgonIndexBufferInfo.Usage"/> of <c>Immutable</c> or <c>Dynamic</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, exceptions raised by this method will only be done so when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Update(int[] data, int bufferOffset = 0, int startIndex = 0, int? count = null)
		{
			data.ValidateObject(nameof(data));

		    if (count == null)
		    {
		        count = data.Length - startIndex;
		    }

#if DEBUG
		    if (Info.Use16BitIndices)
		    {
		        throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_INDEX_BUFFER_TYPE_MISMATCH, 16, 32));
		    }

			if ((Info.Usage == D3D11.ResourceUsage.Dynamic) || (Info.Usage == D3D11.ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_BUFFER_CANT_UPDATE_IMMUTABLE_OR_DYNAMIC);
			}

			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (bufferOffset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferOffset));
			}

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if ((startIndex + count) > data.Length)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, startIndex, count));
			}

			if ((bufferOffset + count * _indexSize) > SizeInBytes)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DATA_OFFSET_COUNT_IS_TOO_LARGE, startIndex, count));
			}
#endif

			Graphics.D3DDeviceContext.UpdateSubresource(data,
			                                            D3DResource,
			                                            0,
			                                            0,
			                                            0,
			                                            new D3D11.ResourceRegion
			                                            {
				                                            Left = bufferOffset,
				                                            Right = bufferOffset + (count.Value * _indexSize),
				                                            Top = 0,
				                                            Front = 0,
				                                            Bottom = 1,
				                                            Back = 1
			                                            });
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class.
		/// </summary>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
		/// <param name="info">Information used to create the buffer.</param>
		/// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
		public GorgonIndexBuffer(string name, GorgonGraphics graphics, IGorgonIndexBufferInfo info, IGorgonPointer initialData = null, IGorgonLog log = null)
			: base(graphics, name, log)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

            BufferType = BufferType.Index;
		    
			_info = new GorgonIndexBufferInfo(info);
			_indexSize = _info.Use16BitIndices ? sizeof(ushort) : sizeof(uint);

			Initialize(initialData);
		}
		#endregion
	}
}
