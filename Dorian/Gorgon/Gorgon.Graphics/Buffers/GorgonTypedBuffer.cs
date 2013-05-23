#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, May 12, 2013 11:04:40 PM
// 
#endregion

using System;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold typed data elements.
	/// </summary>
	/// <typeparam name="T">Type of data in the buffer.</typeparam>
	/// <remarks>This buffer holds typed data and can be accessed in a shader through its shader view or an unordered access view.</remarks>
	public class GorgonTypedBuffer<T>
		: GorgonShaderBuffer
		where T : struct
	{
		#region Properties.
		/// <summary>
		/// Property to return the settings for the buffer.
		/// </summary>
		public new GorgonTypedBufferSettings<T> Settings
		{
			get
			{
				return (GorgonTypedBufferSettings<T>)base.Settings;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a default resource view object.
		/// </summary>
		private void CreateDefaultResourceView()
		{
		    if ((Settings.Usage == GorgonLibrary.Graphics.BufferUsage.Staging)
                || (Settings.ShaderViewFormat == BufferFormat.Unknown))
		    {
		        return;
		    }

		    DefaultShaderView = CreateShaderView(Settings.ShaderViewFormat, 0, Settings.ElementCount);
		}

        /// <summary>
        /// Function to retrieve the staging buffer for this buffer.
        /// </summary>
        /// <returns>
        /// The staging buffer for this buffer.
        /// </returns>
        protected override GorgonBaseBuffer GetStagingBufferImpl()
        {
            GorgonBaseBuffer result = Graphics.Buffers.CreateTypedBuffer(Name + " [Staging]",
                                                                         new GorgonTypedBufferSettings<T>
                                                                         {
                                                                             AllowUnorderedAccess = false,
                                                                             ElementCount = Settings.ElementCount,
                                                                             IsOutput = false,
                                                                             Usage = BufferUsage.Staging
                                                                         });

            result.Copy(this);

            return result;
        }

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			if (D3DResource != null)
			{
				GorgonRenderStatistics.TypedBufferCount--;
				GorgonRenderStatistics.TypedBufferSize -= D3DBuffer.Description.SizeInBytes;
			}

			base.CleanUpResource();
		}

		/// <summary>
		/// Function to initialize the buffer.
		/// </summary>
		/// <param name="value">Value used to initialize the buffer.</param>
		protected override void InitializeImpl(GorgonDataStream value)
		{
			if (D3DResource != null)
			{
				D3DResource.Dispose();
				D3DResource = null;
			}

		    var desc = new D3D.BufferDescription
		        {
		            BindFlags = Settings.Usage == BufferUsage.Staging ? D3D.BindFlags.None : D3D.BindFlags.ShaderResource,
		            CpuAccessFlags = D3DCPUAccessFlags,
		            OptionFlags = D3D.ResourceOptionFlags.None,
		            SizeInBytes = SizeInBytes,
		            Usage = D3DUsage
		        };

            if ((Settings.IsOutput)
                && (Settings.Usage != BufferUsage.Immutable)
                && (Settings.Usage != BufferUsage.Staging))
            {
                desc.BindFlags |= D3D.BindFlags.StreamOutput;
            }
            
            if ((Settings.Usage != BufferUsage.Staging) && (Settings.AllowUnorderedAccess))
			{
				desc.BindFlags |= D3D.BindFlags.UnorderedAccess;
			}

		    if (value != null)
		    {
		        long position = value.Position;

		        using(var dxStream = new DX.DataStream(value.BasePointer, value.Length - position, true, true))
		        {
		            D3DResource = new D3D.Buffer(Graphics.D3DDevice, dxStream, desc);
		        }
		    }
		    else
		    {
		        D3DResource = new D3D.Buffer(Graphics.D3DDevice, desc);
		    }

		    GorgonRenderStatistics.TypedBufferCount++;
			GorgonRenderStatistics.TypedBufferSize += ((D3D.Buffer)D3DResource).Description.SizeInBytes;

            CreateDefaultResourceView();
		}

		/// <summary>
		/// Function to create an unordered access view for this buffer.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="start">First element to map to the view.</param>
		/// <param name="count">The number of elements to map to the view.</param>
		/// <returns>A new unordered access view for the buffer.</returns>
		/// <remarks>Use this to create an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a <see cref="CreateShaderView">Shader View</see>, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this buffer is set to Staging or Dynamic.
		/// <para>-or-</para>
		/// <para>Thrown when the video device feature level is not SM_5 or better.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
		/// number of elements in the buffer.</exception>
		public GorgonBufferUnorderedAccessView CreateUnorderedAccessView(BufferFormat format, int start, int count)
		{
			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Unordered access views are only available on video devices that support SM_5 or better.");
			}

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
            
            if (format == BufferFormat.Unknown)
			{
				throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "format");
			}

			if ((start + count > Settings.ElementCount) || (start < 0) || (count < 1))
			{
				throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
			}

			// Ensure the size of the data type fits the requested format.
			var info = GorgonBufferFormatInfo.GetInfo(format);

			if (info.SizeInBytes != Settings.ElementSize)
			{
				throw new ArgumentException(
					string.Format(
						"The size of the format: {0} bytes, does not match the size of the data type: {1} bytes.",
						info.SizeInBytes,
						Settings.ElementSize),
					"format");
			}

			var view = new GorgonBufferUnorderedAccessView(this, format, start, count);

			view.Initialize();

			return view;
		}
		
		/// <summary>
        /// Function to create a new shader view for the buffer.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="start">Starting element.</param>
        /// <param name="count">Element count.</param>
        /// <returns>A new shader view for the buffer.</returns>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this buffer is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        /// <remarks>Use this to create additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonBufferShaderView CreateShaderView(BufferFormat format, int start, int count)
        {
            if (Settings.Usage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a shader resource view for a buffer that has a usage of Staging.");
            }

            if ((start + count > Settings.ElementCount) || (start < 0) || (count < 1))
            {
                throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
            }

            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "format");
            }

			// Ensure the size of the data type fits the requested format.
			var info = GorgonBufferFormatInfo.GetInfo(format);
				
			if (info.SizeInBytes != Settings.ElementSize)
		    {
			    throw new ArgumentException(
				    string.Format(
					    "The size of the format: {0} bytes, does not match the size of the data type: {1} bytes.",
					    info.SizeInBytes,
					    Settings.ElementSize),
				    "format");
		    }

	        return ViewCache.GetBufferView(format, start, count, false);
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTypedBuffer{T}" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="name">Name of the buffer.</param>
		/// <param name="settings">The settings to apply to the typed buffer.</param>
		internal GorgonTypedBuffer(GorgonGraphics graphics, string name, GorgonTypedBufferSettings<T> settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
