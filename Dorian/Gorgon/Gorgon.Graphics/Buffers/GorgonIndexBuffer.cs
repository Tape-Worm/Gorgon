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
using GorgonLibrary.Diagnostics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold a set of indices.
	/// </summary>
	public class GorgonIndexBuffer
		: GorgonShaderBuffer
	{
		#region Variables.
		private DX.DataStream _lockStream;								// Stream used when locking.
		#endregion

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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a default shader resource view.
		/// </summary>
		private void CreateDefaultResourceView()
		{
			if ((!Settings.UseShaderView) || (Settings.Usage == BufferUsage.Staging))
			{
				return;
			}

			DefaultShaderView = CreateShaderView(0, Settings.ElementCount);
		}

        /// <summary>
        /// Function to retrieve the staging buffer for this buffer.
        /// </summary>
        /// <returns>
        /// The staging buffer for this buffer.
        /// </returns>
        protected override GorgonBuffer GetStagingBufferImpl()
        {
            GorgonBuffer result = Graphics.Buffers.CreateIndexBuffer(Name + " [Staging]",
                                                                         new GorgonIndexBufferSettings
                                                                             {
                                                                                 AllowUnorderedAccess = false,
                                                                                 SizeInBytes = Settings.SizeInBytes,
                                                                                 IsOutput = false,
                                                                                 Usage = BufferUsage.Staging,
                                                                                 Use32BitIndices =
                                                                                     Settings.Use32BitIndices,
                                                                                 UseShaderView = false
                                                                             });

            result.Copy(this);

            return result;
        }

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
		    if (Graphics.Input.IndexBuffer == this)
		    {
		        Graphics.Input.IndexBuffer = null;
		    }

		    if (IsLocked)
		    {
		        Unlock();
		    }

		    if (D3DResource == null)
		    {
		        return;
		    }

		    GorgonRenderStatistics.IndexBufferCount--;
		    GorgonRenderStatistics.IndexBufferSize -= D3DBuffer.Description.SizeInBytes;

		    D3DResource.Dispose();
		    D3DResource = null;

            Gorgon.Log.Print("Destroyed {0} {1}.", LoggingLevel.Verbose, GetType().FullName, Name);
		}

		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected override void InitializeImpl(GorgonDataStream data)
		{
			var desc = new D3D11.BufferDescription
			    {
			        BindFlags = D3D11.BindFlags.IndexBuffer,
			        CpuAccessFlags = D3DCPUAccessFlags,
			        OptionFlags = D3D11.ResourceOptionFlags.None,
			        SizeInBytes = SizeInBytes,
			        StructureByteStride = 0,
			        Usage = D3DUsage
			    };

			if (Settings.UseShaderView)
			{
				desc.BindFlags |= D3D11.BindFlags.ShaderResource;
			}

			if (Settings.AllowUnorderedAccess)
			{
				desc.BindFlags |= D3D11.BindFlags.UnorderedAccess;
			}

			if (Settings.IsOutput)
			{
				desc.BindFlags |= D3D11.BindFlags.StreamOutput;
			}

		    if (data == null)
		    {
		        D3DResource = new D3D11.Buffer(Graphics.D3DDevice, desc);
		    }
		    else
		    {
		        long position = data.Position;

		        using(var stream = new DX.DataStream(data.PositionPointer, data.Length - position, true, true))
		        {
		            D3DResource = new D3D11.Buffer(Graphics.D3DDevice, stream, desc);
		        }
		    }

			CreateDefaultResourceView();

		    GorgonRenderStatistics.IndexBufferCount++;
			GorgonRenderStatistics.IndexBufferSize += ((D3D11.Buffer)D3DResource).Description.SizeInBytes;
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
			var mapMode = D3D11.MapMode.Write;

#if DEBUG
			// Read is mutually exclusive.
		    if ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read)
		    {
		        throw new ArgumentException(Resources.GORGFX_BUFFER_WRITE_ONLY, "lockFlags");
		    }

		    if (lockFlags == BufferLockFlags.Write)
		    {
		        throw new ArgumentException(Resources.GORGFX_BUFFER_REQUIRES_NOOVERWRITE_DISCARD, "lockFlags");
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
		protected override void UnlockImpl()
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
			UpdateImpl(stream, offset, size);
		}

		/// <summary>
		/// Function to create a new shader view for the index buffer.
		/// </summary>
		/// <param name="startIndex">Starting index to map to the view.</param>
		/// <param name="count">Number of indices to map to the view.</param>
		/// <returns>A new shader view for the buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="startIndex"/> or <paramref name="count"/> parameters are less than 0 or 1, respectively.  Or if the total is larger than the buffer size.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer has a usage of Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow shader views.</para>
		/// </exception>
        /// <remarks>Use this to create additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>To create a shader view, the index buffer must have <see cref="GorgonLibrary.Graphics.GorgonIndexBufferSettings.UseShaderView">UseShaderView</see> in the settings set to TRUE.  Otherwise, an exception will be thrown.</para>
        /// <para>Views on index buffers will only use R32_Uint format if the buffer is a 32 bit index buffer or R16_Uint format if the buffer is a 16 bit buffer.</para>
        /// <para>The <paramref name="startIndex"/> and <paramref name="count"/> are elements in the buffer.  The size of each element is dependant upon whether the index buffer holds 32 bit indices or 16 bit indices.
        /// Consequently the number of elements in the buffer may be larger or smaller depending on the view format.  For example, a 48 byte buffer with 32bit indices will have an element count of 12.  Whereas the same buffer with 
        /// 16bit indices will have count of 24.</para>
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonBufferShaderView CreateShaderView(int startIndex, int count)
		{
			if (Settings.Usage == BufferUsage.Staging)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a shader resource view for a buffer that has a usage of Staging.");
			}

			if (!Settings.UseShaderView)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "The was created without shader view access.");
			}

			if ((startIndex + count > Settings.ElementCount) || (startIndex < 0) || (count < 1))
			{
				throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
			}

			return ViewCache.GetBufferView(Settings.Use32BitIndices ? BufferFormat.R32_UInt : BufferFormat.R16_UInt, startIndex,
			                               count, false);
		}

		/// <summary>
		/// Function to create an unordered access view for this buffer.
		/// </summary>
		/// <param name="startIndex">First element to map to the view.</param>
		/// <param name="count">The number of elements to map to the view.</param>
		/// <returns>A new unordered access view for the buffer.</returns>
		/// <remarks>Use this to create an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a <see cref="CreateShaderView">Shader View</see>, only one 
		/// unordered access view can be bound to the pipeline at any given time.
        /// <para>Views on index buffers will only use R32_Uint format if the buffer is a 32 bit index buffer or R16_Uint format if the buffer is a 16 bit buffer.</para>
        /// <para>The <paramref name="startIndex"/> and <paramref name="count"/> are elements in the buffer.  The size of each element is dependant upon whether the index buffer holds 32 bit indices or 16 bit indices.
        /// Consequently the number of elements in the buffer may be larger or smaller depending on the view format.  For example, a 48 byte buffer with 32bit indices will have an element count of 12. Whereas the same buffer with 
        /// 16bit indices will have count of 24.</para>
        /// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this buffer is set to Staging or Dynamic.
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="startIndex"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
		/// number of elements in the buffer.</exception>
		public GorgonBufferUnorderedAccessView CreateUnorderedAccessView(int startIndex, int count)
		{
			if (!Settings.AllowUnorderedAccess)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "The buffer does not allow unordered access.");
			}

			if (Settings.Usage == BufferUsage.Staging)
			{
				throw new GorgonException(GorgonResult.CannotBind, "Cannot bind an unordered access resource view to a buffer that has a usage of [Staging].");
			}

			if (Settings.Usage == BufferUsage.Dynamic)
			{
				throw new GorgonException(GorgonResult.CannotBind, "Cannot bind an unordered access resource view to a buffer that has a usage of [Dynamic].");
			}

			if ((startIndex + count > SizeInBytes) || (startIndex < 0) || (count < 1))
			{
				throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
			}

			var view = new GorgonBufferUnorderedAccessView(this,
													   Settings.Use32BitIndices ? BufferFormat.R32_UInt : BufferFormat.R16_UInt, 
			                                           startIndex, count);
			view.Initialize();

			return view;
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
