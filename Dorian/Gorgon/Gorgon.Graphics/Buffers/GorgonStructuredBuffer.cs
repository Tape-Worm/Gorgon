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
// Created: Monday, November 5, 2012 8:58:47 PM
// 
#endregion

using System;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The type of structured buffer.
	/// </summary>
	public enum StructuredBufferType
	{
		/// <summary>
		/// A standard structured buffer.
		/// </summary>
		Standard = 0,
		/// <summary>
		/// An append/consume buffer.
		/// </summary>
		AppendConsume = 1,
		/// <summary>
		/// A counter buffer.
		/// </summary>
		Counter = 2
	}

	/// <summary>
	/// A structured buffer for shaders.
	/// </summary>
	/// <remarks>Structured buffers are similar to <see cref="GorgonLibrary.Graphics.GorgonConstantBuffer">constant buffers</see> in that they're used to convey data to 
	/// a shader.  However, unlike constant buffers, these buffers allow for unordered access and are meant as containers for structured data (hence, structured buffers).
	/// <para>Structured buffers are only available to SM5 and above.</para>
	/// </remarks>
	public class GorgonStructuredBuffer
		: GorgonShaderBuffer
	{
		#region Properties.
		/// <summary>
		/// Property to return the settings for a structured shader buffer.
		/// </summary>
		public new GorgonStructuredBufferSettings Settings
		{
			get
			{
				return (GorgonStructuredBufferSettings)base.Settings;
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

		    DefaultShaderView = CreateShaderView(0, Settings.ElementCount);
		}

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			if (D3DResource != null)
			{
				GorgonRenderStatistics.StructuredBufferCount--;
				GorgonRenderStatistics.StructuredBufferSize -= D3DBuffer.Description.SizeInBytes;
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
		            OptionFlags = D3D.ResourceOptionFlags.BufferStructured,
		            SizeInBytes = SizeInBytes,
		            StructureByteStride = Settings.ElementSize,
		            Usage = D3DUsage
		        };

			if ((BufferUsage != BufferUsage.Staging) && (Settings.UseUnorderedAccessView))
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

		    GorgonRenderStatistics.StructuredBufferCount++;
			GorgonRenderStatistics.StructuredBufferSize += ((D3D.Buffer)D3DResource).Description.SizeInBytes;

#if DEBUG
			D3DResource.DebugName = "Gorgon Structured Buffer #" + Graphics.GetGraphicsObjectOfType<GorgonStructuredBuffer>().Count;
#endif
			CreateDefaultResourceView();
		}

        /// <summary>
        /// Function to create a new shader view for the buffer.
        /// </summary>
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

            return ViewCache.GetBufferView(BufferFormat.Unknown, start, count);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="settings">The settings for the structured buffer.</param>
		internal GorgonStructuredBuffer(GorgonGraphics graphics, GorgonStructuredBufferSettings settings)
			: base(graphics, settings, settings.ElementCount * settings.ElementSize)
		{
		}
		#endregion
	}
}
