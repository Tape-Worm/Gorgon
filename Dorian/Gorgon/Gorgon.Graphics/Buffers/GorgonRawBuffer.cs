﻿#region MIT.
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
using GorgonLibrary.Graphics.Properties;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold generic byte data.
	/// </summary>
	/// <remarks>This buffer holds raw byte data and can be accessed in a shader through its shader view or an unordered access view.  The buffer size must be a multiple of 4 or an exception will be thrown when creating the buffer.</remarks>
	public class GorgonRawBuffer
		: GorgonShaderBuffer
	{
		#region Properties.
		/// <summary>
		/// Property to return the settings for the buffer.
		/// </summary>
		public new GorgonRawBufferSettings Settings
		{
			get
			{
				return (GorgonRawBufferSettings)base.Settings;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a default resource view object.
		/// </summary>
		private void CreateDefaultResourceView()
		{
		    if (BufferUsage == GorgonLibrary.Graphics.BufferUsage.Staging)
		    {
		        return;
		    }

		    DefaultShaderView = CreateShaderView(0, Settings.ElementCount / 4);
		}

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			if (D3DResource != null)
			{
				GorgonRenderStatistics.RawBufferCount--;
				GorgonRenderStatistics.RawBufferSize -= D3DBuffer.Description.SizeInBytes;
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
		            BindFlags = BufferUsage == BufferUsage.Staging ? D3D.BindFlags.None : D3D.BindFlags.ShaderResource,
		            CpuAccessFlags = D3DCPUAccessFlags,
		            OptionFlags = D3D.ResourceOptionFlags.BufferAllowRawViews,
		            SizeInBytes = SizeInBytes,
		            Usage = D3DUsage
		        };

			if ((BufferUsage != BufferUsage.Staging) && (Settings.CreateUnderedAccessView))
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

		    GorgonRenderStatistics.RawBufferCount++;
			GorgonRenderStatistics.RawBufferSize += ((D3D.Buffer)D3DResource).Description.SizeInBytes;

#if DEBUG
			D3DResource.DebugName = "Gorgon Raw Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonRawBuffer>().Count;
#endif
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
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this buffer is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the video device feature level is not SM_5 or better.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
		/// number of elements in the buffer.</exception>
		public GorgonBufferUnorderAccessView CreateUnorderedAccessView(BufferFormat format, int start, int count)
		{
			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Unordered access views are only available on video devices that support SM_5 or better.");
			}

			if (!Settings.AllowUnorderedAccess)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "The buffer does not allow unordered access.");
			}

			if (BufferUsage == BufferUsage.Staging)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create an unordered access resource view for a buffer that has a usage of Staging.");
			}

			if (format == BufferFormat.Unknown)
			{
				throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "format");
			}

			if ((start + count > Settings.ElementCount) || (start < 0) || (count < 0))
			{
				throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
			}

			// Ensure the size of the data type fits the requested format.
			var info = GorgonBufferFormatInfo.GetInfo(format);

			if (info.SizeInBytes != 4)
			{
				throw new ArgumentException(
					string.Format(
						"The size of the format: {0} bytes, does not match the size of the data type: 4 bytes.",
						info.SizeInBytes),
					"format");
			}

			return new GorgonBufferUnorderAccessView(this, format, start, count, false);
		}
		
		/// <summary>
        /// Function to create a new shader view for the buffer.
        /// </summary>
        /// <param name="start">Starting element.</param>
        /// <param name="count">Element count. This value must the number of elements, not the number of bytes in the resource.</param>
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
        public GorgonBufferShaderView CreateShaderView(int start, int count)
        {
            if (BufferUsage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a shader resource view for a buffer that has a usage of Staging.");
            }

            if ((start + count > Settings.ElementCount) || (start < 0) || (count < 0))
            {
                throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
            }

	        return ViewCache.GetBufferView(BufferFormat.R32, start, count, true);
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="settings">The settings to apply to the typed buffer.</param>
		internal GorgonRawBuffer(GorgonGraphics graphics, GorgonRawBufferSettings settings)
			: base(graphics, settings, settings.ElementCount)
		{
		}
		#endregion
	}
}
