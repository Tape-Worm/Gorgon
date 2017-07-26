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
using Gorgon.Diagnostics;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A buffer for vertices.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use a vertex buffer to send vertices to the GPU. 
	/// </para>
	/// <para>
	/// To send vertices to the GPU using a vertex buffer, an application can upload a value type values, representing the vertices, to the buffer using one of the 
	/// <see cref="O:Gorgon.Graphics.GorgonVertexBuffer.Update{T}(ref T)">Update&lt;T&gt;</see> overloads. For best performance, it is recommended to upload vertex data only once, or rarely. However, in 
	/// some scenarios, and with the correct <see cref="IGorgonVertexBufferInfo.Usage"/> flag, vertex animation is possible by uploading data to a dynamic vertex buffer.
	/// </para>
	/// <para> 
	/// <example language="csharp">
	/// For example, to send a list of vertices to a vertex buffer:
	/// <code language="csharp">
	/// <![CDATA[
	/// // Our vertex, with a position and color component.
	/// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
	/// struct MyVertex
	/// {
	///		public Vector4 Position;
	///		public Vector4 Color;
	/// }
	/// 
	/// GorgonGraphics graphics;
	/// MyVertex[] _vertices = new MyVertex[100];
	/// GorgonVertexBuffer _vertexBuffer;
	/// 
	/// void InitializeVertexBuffer()
	/// {
	///		_vertices = ... // Fill your vertex array here.
	/// 
	///		// Create the vertex buffer large enough so that it'll hold all 100 vertices.
	///		_vertexBuffer = new GorgonVertexBuffer("MyVB", graphics, GorgonVertexBufferInfo.CreateFromType<MyVertex>(Usage.Default));
	/// 
	///		// Copy our data to the vertex buffer.
	///		_vertexBuffer.Update(_vertices);
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// </para>
	/// </remarks>
	public sealed class GorgonVertexBuffer
		: GorgonBufferCommon
	{
		#region Variables.
		// The information used to create the buffer.
		private readonly GorgonVertexBufferInfo _info;
		// The address returned by the lock on the buffer.
		private GorgonPointerAlias _lockAddress;
        #endregion

        #region Properties.
	    /// <summary>
	    /// Property to return the usage flags for the buffer.
	    /// </summary>
	    protected override D3D11.ResourceUsage Usage => _info.Usage;

		/// <summary>
        /// Property used to return the information used to create this buffer.
        /// </summary>
        public IGorgonVertexBufferInfo Info => _info;
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

			Log.Print($"{Name} Vertex Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

			D3D11.BindFlags bindFlags = D3D11.BindFlags.VertexBuffer;

			if (_info.IsStreamOut)
			{
				bindFlags |= D3D11.BindFlags.StreamOutput;
			}

			var desc  = new D3D11.BufferDescription
			{
				SizeInBytes = Info.SizeInBytes,
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

			SizeInBytes = Info.SizeInBytes;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the vertex buffer is locked when this method is called, it will automatically be unlocked and any lock pointer will be invalidated.
		/// </para>
		/// <para>
		/// Objects that override this method should be sure to call this base method or else a memory leak may occur.
		/// </para>
		/// </remarks>
		public override void Dispose()
		{
			// If we're locked, then unlock the buffer before destroying it.
			if ((IsLocked) && (_lockAddress != null) && (!_lockAddress.IsDisposed))
			{
				Unlock(ref _lockAddress);

				// Because the pointer is an alias, we don't really NEED to call this, but just for consistency we'll do so anyway.
				_lockAddress.Dispose();
			}

			base.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBuffer" /> class.
		/// </summary>
		/// <param name="name">Name of this buffer.</param>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
		/// <param name="info">Information used to create the buffer.</param>
		/// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
		/// <param name="log">[Optional] The log interface used for debug logging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is empty.</exception>
		public GorgonVertexBuffer(string name, GorgonGraphics graphics, IGorgonVertexBufferInfo info, IGorgonPointer initialData = null, IGorgonLog log = null)
			: base(graphics, name, log)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}
            
            BufferType = BufferType.Vertex;
			_info = new GorgonVertexBufferInfo(info);

			Initialize(initialData);
		}
		#endregion
	}
}
