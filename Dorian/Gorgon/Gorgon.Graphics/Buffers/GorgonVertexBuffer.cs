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
// Created: Tuesday, January 03, 2012 8:22:34 AM
// 
#endregion

using System;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold a set of vertices.
	/// </summary>
	/// <remarks>The vertex buffer is the primary method of getting mesh vertices to the GPU for rendering.</remarks>
	public class GorgonVertexBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private DX.DataStream _lockStream;								// Stream used when locking.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of buffer.
		/// </summary>
		public override BufferType BufferType
		{
			get
			{
				return BufferType.Vertex;
			}
		}

		/// <summary>
		/// Property to return the settings for the vertex buffer.
		/// </summary>
		public new GorgonBufferSettings Settings
		{
			get
			{
				return (GorgonBufferSettings)base.Settings;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>
		/// A data stream containing the buffer data.
		/// </returns>
		protected override GorgonDataStream OnLock(BufferLockFlags lockFlags)
		{
			var mapMode = D3D11.MapMode.Write;

#if DEBUG
			// Read is mutually exclusive.
		    if ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read)
		    {
		        throw new ArgumentException(Resources.GORGFX_BUFFER_WRITE_ONLY, "lockFlags");
		    }

		    if (lockFlags == BufferLockFlags.Write)
		    {
		        throw new ArgumentException(Resources.GORGFX_BUFFER_LOCK_NOT_WRITE_DISCARD, "lockFlags");
		    }
#endif

		    if ((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
		    {
		        mapMode = D3D11.MapMode.WriteDiscard;
		    }

		    if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
		    {
		        mapMode = D3D11.MapMode.WriteNoOverwrite;
		    }

		    Graphics.Context.MapSubresource(D3DBuffer, mapMode, D3D11.MapFlags.None, out _lockStream);
			return new GorgonDataStream(_lockStream.DataPointer, (int)_lockStream.Length);
		}

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected override void OnUnlock()
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
		protected override void OnUpdate(GorgonDataStream stream, int offset, int size)
		{
			Graphics.Context.UpdateSubresource(
				new DX.DataBox
				    {
					DataPointer = stream.PositionPointer,
					RowPitch = size
				},
				D3DResource,
				0,
				new D3D11.ResourceRegion
				    {
					Left = offset,
					Right = offset + size,
					Top = 0,
					Bottom = 1,
					Front = 0,
					Back = 1
				});
		}

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
		/// <param name="size">The number of bytes to write.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>Please note that constant buffers don't use the <paramref name="offset"/> and <paramref name="size"/> parameters.</para>
		/// <para>This method will respect the <see cref="GorgonLibrary.IO.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(GorgonDataStream stream, int offset, int size)
		{
			OnUpdate(stream, offset, size);
		}
         
        /// <summary>
        /// Function to retrieve a new shader view for the index buffer.
        /// </summary>
        /// <param name="format">Format of the shader view.</param>
        /// <param name="startElement">Starting element to map to the view.</param>
        /// <param name="count">Number of elements to map to the view.</param>
		/// <param name="useRaw">TRUE to use a raw shader view, FALSE to use a normal view.</param>
        /// <returns>A shader view for the buffer.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="startElement"/> or <paramref name="count"/> parameters are less than 0 or 1, respectively.  Or if the total is larger than the buffer size.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
		/// <remarks>Use this to create additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
		/// <para>To use a shader view, the vertex buffer must have <see cref="GorgonLibrary.Graphics.GorgonBufferSettings.DefaultShaderViewFormat">DefaultShaderViewFormat</see> in the settings set to a value other than Unknown.  
        /// Otherwise, an exception will be thrown.</para>
        /// <para>The <paramref name="startElement"/> and <paramref name="count"/> are elements in the buffer.  The size of each element is dependant upon the format passed, and consequently the number of elements in the buffer 
        /// may be larger or smaller depending on the view format.  For example, a 48 byte buffer with a view of R32G32B32A32_Float will have an element count of 3 (48 / 16 bytes = 3).  Whereas the same buffer with a view of R8G8B8A8_Int will 
        /// have a count of 12 (48 / 4 bytes = 12).</para>
		/// <para>Raw views require that the buffer be created with the <see cref="GorgonLibrary.Graphics.GorgonBufferSettings.AllowRawViews">AllowRawViews</see> property set to TRUE in its settings.</para>
		/// <para>Raw views can only be used on SM5 video devices or better. </para>
		/// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonBufferShaderView GetShaderView(BufferFormat format, int startElement, int count, bool useRaw)
        {
	        return OnGetShaderView(format, startElement, count, useRaw);
        }

        /// <summary>
        /// Function to retrieve an unordered access view for this buffer.
        /// </summary>
        /// <param name="format">Format of the unordered access view.</param>
        /// <param name="startElement">First element to map to the view.</param>
        /// <param name="count">The number of elements to map to the view.</param>
		/// <param name="useRaw">TRUE to use a raw shader view, FALSE to use a normal view.</param>
        /// <returns>An unordered access view for the buffer.</returns>
        /// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a <see cref="GetShaderView">Shader View</see>, only one 
        /// unordered access view can be bound to the pipeline at any given time.
        /// <para>The <paramref name="startElement"/> and <paramref name="count"/> are elements in the buffer.  The size of each element is dependant upon the format passed, and consequently the number of elements in the buffer 
        /// may be larger or smaller depending on the view format.  For example, a 48 byte buffer with a view of R32G32B32A32_Float will have an element count of 3 (48 / 16 bytes = 3).  Whereas the same buffer with a view of R8G8B8A8_Int will 
        /// have a count of 12 (48 / 4 bytes = 12).</para>
		/// <para>Raw views require that the buffer be created with the <see cref="GorgonLibrary.Graphics.GorgonBufferSettings.AllowRawViews">AllowRawViews</see> property set to TRUE in its settings.</para>
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="startElement"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        public GorgonBufferUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int startElement, int count, bool useRaw)
        {
	        return OnGetUnorderedAccessView(format, startElement, count, useRaw, UnorderedAccessViewType.Standard);
        }
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="name">Name of the buffer.</param>
		/// <param name="settings">The settings for the vertex buffer.</param>
		internal GorgonVertexBuffer(GorgonGraphics graphics, string name, GorgonBufferSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
