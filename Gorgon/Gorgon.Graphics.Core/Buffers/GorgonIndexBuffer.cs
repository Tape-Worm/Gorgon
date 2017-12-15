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
        protected internal override ResourceUsage Usage => _info.Usage;

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
				case ResourceUsage.Staging:
					cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
					break;
				case ResourceUsage.Dynamic:
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
				Usage = (D3D11.ResourceUsage)_info.Usage,
				BindFlags = bindFlags,
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

			SizeInBytes = desc.SizeInBytes;
		}

	    /// <summary>
	    /// Function to create a new <see cref="GorgonIndexBufferUav"/> for this buffer.
	    /// </summary>
	    /// <param name="startElement">[Optional] The first element to start viewing from.</param>
	    /// <param name="elementCount">[Optional] The number of elements to view.</param>
	    /// <returns>A <see cref="GorgonIndexBufferUav"/> used to bind the buffer to a shader.</returns>
	    /// <exception cref="GorgonException">Thrown if the video adapter does not support feature set 11 or better.
	    /// <para>-or-</para>
	    /// <para>Thrown when this buffer does not have a <see cref="VertexIndexBufferBinding"/> of <see cref="VertexIndexBufferBinding.UnorderedAccess"/>.</para>
	    /// <para>-or-</para>
	    /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
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
	    /// This method requires a video adapter capable of supporting feature set 11 or better. If the current video adapter does not support feature set 11, an exception will be thrown.
	    /// </para>
	    /// </note>
	    /// </para>
	    /// </remarks>
	    public GorgonIndexBufferUav GetUnorderedAccessView(int startElement = 0, int elementCount = 0)
	    {
	        if (Graphics.RequestedFeatureSet < FeatureSet.Level_12_0)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_UAV_REQUIRES_SM5);
	        }

	        if ((Info.Usage == ResourceUsage.Staging)
	            || ((Info.Binding & VertexIndexBufferBinding.UnorderedAccess) != VertexIndexBufferBinding.UnorderedAccess))
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
	        }

	        if (!Graphics.FormatSupport.TryGetValue(IndexFormat, out GorgonFormatSupportInfo support))
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, IndexFormat));
	        }

            if ((support.FormatSupport & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView)
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
