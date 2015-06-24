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

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A buffer to hold a set of indices.
	/// </summary>
	/// <remarks>Use this to define the layout order of a set of vertices within a buffer.  Using an index buffer can reduce the number of required vertices for a mesh, and thus perform better.</remarks>
	public class GorgonIndexBuffer
		: GorgonBaseBuffer
	{
		#region Properties.
		/// <summary>
		/// Property to return the index buffer settings.
		/// </summary>
		public new GorgonIndexBufferSettings Settings
		{
			get
			{
				return (GorgonIndexBufferSettings)base.Settings;
			}
		}

        /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public override BufferType BufferType
        {
            get
            {
                return BufferType.Index;
            }
        }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to clean up the resource object.
        /// </summary>
        protected override void CleanUpResource()
        {
            if (Graphics.Input.IndexBuffer == this)
            {
                Graphics.Input.IndexBuffer = null;
            }
            
            base.CleanUpResource();
        }

        /// <summary>
        /// Function used to lock the underlying buffer for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used when locking the buffer.</param>
        /// <param name="context">A graphics context to use when locking the buffer.</param>
        /// <returns>
        /// A data stream containing the buffer data.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// lockFlags
        /// or
        /// lockFlags
        /// </exception>
        /// <remarks>
        /// Use the <paramref name="context" /> parameter to determine the context in which the buffer should be updated. This is necessary to use that context
        /// to update the buffer because 2 threads may not access the same resource at the same time.
        /// </remarks>
		protected override GorgonDataStream OnLock(BufferLockFlags lockFlags, GorgonGraphics context)
        {
            DX.DataStream lockStream;

			context.Context.MapSubresource(D3DBuffer, GetMapMode(lockFlags), D3D11.MapFlags.None, out lockStream);

            return new GorgonDataStream(lockStream.DataPointer, (int)lockStream.Length);
		}

        /// <summary>
        /// Function to update the buffer.
        /// </summary>
        /// <param name="stream">Stream containing the data used to update the buffer.</param>
        /// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
        /// <param name="size">The number of bytes to write.</param>
        /// <param name="context">A graphics context to use when updating the buffer.</param>
        /// <remarks>
        /// Use the <paramref name="context" /> parameter to determine the context in which the buffer should be updated. This is necessary to use that context
        /// to update the buffer because 2 threads may not access the same resource at the same time.
        /// </remarks>
		protected override void OnUpdate(GorgonDataStream stream, int offset, int size, GorgonGraphics context)
		{
			stream.ValidateObject("stream");

#if DEBUG
			if (Settings.Usage != BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif
	
			context.Context.UpdateSubresource(
				new DX.DataBox
				    {
					DataPointer = stream.PositionIntPtr,
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
		/// Function to update the buffer with data.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		/// <param name="offset">Offset in the buffer, in bytes, to write at.</param>
		/// <param name="deferred">[Optional] The deferred context used to update the buffer.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it 
		/// will use the specified deferred context.
		/// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(ref int value, int offset, GorgonGraphics deferred = null)
		{
#if DEBUG
			if (Settings.Usage != BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif
			if (deferred == null)
			{
				deferred = Graphics;
			}

			deferred.Context.UpdateSubresource(ref value, D3DResource, 0, 0, 0, new D3D11.ResourceRegion
			{
				Left = offset,
				Right = offset + (Settings.Use32BitIndices ? sizeof(int) : sizeof(short)),
				Top = 0,
				Bottom = 1,
				Front = 0,
				Back = 1
			});
		}

		/// <summary>
		/// Function to update the buffer with data.
		/// </summary>
		/// <param name="values">Values to write to the buffer.</param>
		/// <param name="offset">Offset in the buffer, in bytes, to write at.</param>
		/// <param name="deferred">[Optional] The deferred context used to update the buffer.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it 
		/// will use the specified deferred context.
		/// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="values"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the buffer usage is not set to default.</exception>
		public void Update(int[] values, int offset, GorgonGraphics deferred = null)
		{
			values.ValidateObject("values");

#if DEBUG
			if (Settings.Usage != BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif
			if (deferred == null)
			{
				deferred = Graphics;
			}

			deferred.Context.UpdateSubresource(values, D3DResource, 0, 0, 0, new D3D11.ResourceRegion
			{
				Left = offset,
				Right = offset + (Settings.Use32BitIndices ? sizeof(int) : sizeof(short)) * values.Length,
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
		/// <param name="deferred">[Optional] The deferred context used to update the buffer.</param>
		/// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
		/// <para>Please note that constant buffers don't use the <paramref name="offset"/> and <paramref name="size"/> parameters.</para>
		/// <para>This method will respect the <see cref="Gorgon.IO.GorgonDataStream.Position">Position</see> property of the data stream.  
		/// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
		/// to 0.</para>
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context.
        /// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the buffer usage is not set to default.</exception>
        public void Update(GorgonDataStream stream, int offset, int size, GorgonGraphics deferred = null)
		{
            if (deferred == null)
            {
                deferred = Graphics;
            }

			OnUpdate(stream, offset, size, deferred);
		}

		/// <summary>
		/// Function to retrieve a new shader view for the index buffer.
		/// </summary>
		/// <param name="format">The format to cast the index data into.</param>
		/// <param name="startIndex">Starting index to map to the view.</param>
		/// <param name="count">Number of indices to map to the view.</param>
		/// <param name="useRaw"><b>true</b> to use a raw shader view, <b>false</b> to use a normal view.</param>
		/// <returns>A shader view for the buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="startIndex"/> or <paramref name="count"/> parameters are less than 0 or 1, respectively.  Or if the total is larger than the buffer size.</exception>
		/// <exception cref="GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
        /// <remarks>Use this to create/retrieve additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>To use a shader view, the index buffer must have <see cref="Gorgon.Graphics.GorgonIndexBufferSettings.AllowShaderViews">AllowShaderViews</see> in the settings set to <b>true</b>.  Otherwise, an exception will be thrown.</para>
        /// <para>The <paramref name="startIndex"/> and <paramref name="count"/> are elements in the buffer.  The size of each element is dependent upon the size, in bytes, of the format specified in the <paramref name="format"/> parameter.</para>
		/// <para>Raw views require that the buffer be created with the <see cref="Gorgon.Graphics.GorgonIndexBufferSettings.AllowRawViews">AllowRawViews</see> property set to <b>true</b> in its settings.</para>
		/// <para>Raw views can only be used on SM5 video devices or better. </para>
		/// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonBufferShaderView GetShaderView(BufferFormat format, int startIndex, int count, bool useRaw)
		{
		    return OnGetShaderView(format, startIndex, count, useRaw);
		}

		/// <summary>
		/// Function to retrieve an unordered access view for this buffer.
		/// </summary>
        /// <param name="format">The format to cast the index data into.</param>
		/// <param name="startIndex">First element to map to the view.</param>
		/// <param name="count">The number of elements to map to the view.</param>
        /// <param name="useRaw"><b>true</b> to use a raw shader view, <b>false</b> to use a normal view.</param>
		/// <returns>An unordered access view for the buffer.</returns>
		/// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a <see cref="GetShaderView">Shader View</see>, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>To use an unordered access view, the index buffer must have <see cref="Gorgon.Graphics.GorgonIndexBufferSettings.AllowUnorderedAccessViews">AllowUnorderedAccessViews</see> in the settings set to <b>true</b>.  Otherwise, an exception will be thrown.</para>
		/// <para>The <paramref name="startIndex"/> and <paramref name="count"/> are elements in the buffer.  The size of each element is dependent upon the size, in bytes, of the format specified in the <paramref name="format"/> parameter.</para>
		/// <para>Raw views require that the buffer be created with the <see cref="Gorgon.Graphics.GorgonIndexBufferSettings.AllowRawViews">AllowRawViews</see> property set to <b>true</b> in its settings.</para>
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="startIndex"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
		/// number of elements in the buffer.</exception>
		public GorgonBufferUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int startIndex, int count, bool useRaw)
		{
		    return OnGetUnorderedAccessView(format, startIndex, count, useRaw, UnorderedAccessViewType.Standard);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonIndexBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="name">Name for this index buffer.</param>
		/// <param name="settings">Settings for the buffer.</param>
		internal GorgonIndexBuffer(GorgonGraphics graphics, string name, GorgonIndexBufferSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
