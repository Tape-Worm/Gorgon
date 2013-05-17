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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Native;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer to hold generic byte data.
	/// </summary>
	/// <typeparam name="T">Type of data in the buffer.</typeparam>
	/// <remarks>This buffer holds typed data and can be accessed in a shader through its shader view or an unordered access view.</remarks>
	public class GorgonTypedBuffer<T>
		: GorgonShaderBuffer
		where T : struct
	{
		#region Properties.
        /// <summary>
        /// Property to return the size of an element, in bytes.
        /// </summary>
        public int ElementSize
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the settings for the buffer.
		/// </summary>
		public new GorgonTypedBufferSettings Settings
		{
			get
			{
				return (GorgonTypedBufferSettings)base.Settings;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a default resource view object.
		/// </summary>
		private void CreateDefaultResourceView()
		{
		    if ((BufferUsage == GorgonLibrary.Graphics.BufferUsage.Staging)
                || (Settings.ShaderViewFormat == BufferFormat.Unknown))
		    {
		        return;
		    }

		    DefaultShaderView = CreateShaderView(Settings.ShaderViewFormat, 0, Settings.ElementCount);
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
		            BindFlags = BufferUsage == BufferUsage.Staging ? D3D.BindFlags.None : D3D.BindFlags.ShaderResource,
		            CpuAccessFlags = D3DCPUAccessFlags,
		            OptionFlags = Settings.IsRaw ? D3D.ResourceOptionFlags.BufferAllowRawViews : D3D.ResourceOptionFlags.None,
		            SizeInBytes = SizeInBytes,
		            Usage = D3DUsage
		        };

			if ((BufferUsage != BufferUsage.Staging) && (Settings.UnorderedAccessViewFormat != BufferFormat.Unknown))
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

#if DEBUG
			D3DResource.DebugName = "Gorgon Typed Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonTypedBuffer<T>>().Count;
#endif
			CreateDefaultResourceView();
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
            if (BufferUsage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a shader resource view for a buffer that has a usage of Staging.");
            }

            if ((start + count > Settings.ElementCount) || (start < 0) || (count < 0))
            {
                throw new ArgumentException("The start and count must be 0 or greater and less than the number of elements in the buffer.");
            }

            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "format");
            }

            // Ensure the size of the data type fits the requested format.
            var info = GorgonBufferFormatInfo.GetInfo(format);

            if (info.SizeInBytes != ElementSize)
            {
                throw new ArgumentException(
                    string.Format(
                        "The size of the format: {0} bytes, does not match the size of the data type: {1} bytes.",
                        info.SizeInBytes,
                        ElementSize),
                    "format");
            }

            if ((Settings.IsRaw)
                && (info.Group != BufferFormat.R32))
            {
                throw new ArgumentException("Cannot view a raw buffer unless the view format is set to R32_Int, R32_UInt or R32_Float.", "format");
            }

            return ViewCache.GetBufferView(format, start, count);
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTypedBuffer{T}" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="settings">The settings to apply to the typed buffer.</param>
		internal GorgonTypedBuffer(GorgonGraphics graphics, GorgonTypedBufferSettings settings)
			: base(graphics, settings, settings.ElementCount * DirectAccess.SizeOf<T>())
		{
		    ElementSize = DirectAccess.SizeOf<T>();
		}
		#endregion
	}
}
