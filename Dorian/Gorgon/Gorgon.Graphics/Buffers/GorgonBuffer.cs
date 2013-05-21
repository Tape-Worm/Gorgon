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
// Created: Monday, January 09, 2012 7:06:42 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Buffer usage types.
	/// </summary>
	public enum BufferUsage
	{
		/// <summary>
		/// Allows read/write access to the buffer from the GPU.
		/// </summary>
		Default = D3D11.ResourceUsage.Default,
		/// <summary>
		/// Can only be read by the GPU, cannot be written to or read from by the CPU, and cannot be written to by the GPU.
		/// </summary>
		/// <remarks>Pre-initialize any buffer created with this usage, or else you will not be able to after it's been created.</remarks>
		Immutable = D3D11.ResourceUsage.Immutable,
		/// <summary>
		/// Allows read access by the GPU and write access by the CPU.
		/// </summary>
		Dynamic = D3D11.ResourceUsage.Dynamic,
		/// <summary>
		/// Allows reading/writing by the CPU and can be copied to a GPU compatiable buffer (but not used directly by the GPU).
		/// </summary>
		Staging = D3D11.ResourceUsage.Staging
	}

	/// <summary>
	/// Flags used when locking the buffer for reading/writing.
	/// </summary>
	[Flags]
	public enum BufferLockFlags
	{
		/// <summary>
		/// Lock the buffer for reading.
		/// </summary>
		/// <remarks>This flag is mutually exclusive.</remarks>
		Read = 1,
		/// <summary>
		/// Lock the buffer for writing.
		/// </summary>
		Write = 2,
		/// <summary>
		/// Lock the buffer for writing, but guarantee that we will not overwrite a part of the buffer that's already in use.
		/// </summary>
		NoOverwrite = 4,
		/// <summary>
		/// Lock the buffer for writing, but mark its contents as invalid.
		/// </summary>
		Discard = 8
	}

	/// <summary>
	/// A generic buffer object.
	/// </summary>
	/// <remarks>This generic buffer object is unable to be bound with shaders, but can be bound with output streams to retrieve data from shaders that support output data streams.</remarks>
	public class GorgonBuffer
		: GorgonBaseBuffer
	{
		#region Properties.
		/// <summary>
		/// Property to return the settings for the buffer.
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
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
		    if (IsLocked)
		    {
		        Unlock();
		    }

		    if (D3DResource == null)
		    {
		        return;
		    }

		    GorgonRenderStatistics.BufferCount--;
		    GorgonRenderStatistics.BufferSize -= D3DBuffer.Description.SizeInBytes;

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
			if (D3DResource != null)
			{
				D3DResource.Dispose();
				D3DResource = null;
			}

		    var desc = new D3D11.BufferDescription
		        {
		            BindFlags = Settings.IsOutput ? D3D11.BindFlags.StreamOutput : D3D11.BindFlags.None,
		            CpuAccessFlags = D3DCPUAccessFlags,
		            OptionFlags = D3D11.ResourceOptionFlags.None,
		            SizeInBytes = SizeInBytes,
		            StructureByteStride = 0,
		            Usage = D3DUsage
		        };

		    if (data != null)
		    {
		        long position = data.Position;

		        using(var dxStream = new DX.DataStream(data.BasePointer, data.Length - position, true, true))
		        {
		            D3DResource = new D3D11.Buffer(Graphics.D3DDevice, dxStream, desc);
		        }
		    }
		    else
		    {
		        D3DResource = new D3D11.Buffer(Graphics.D3DDevice, desc);
		    }

		    GorgonRenderStatistics.BufferCount++;
			GorgonRenderStatistics.BufferSize += ((D3D11.Buffer)D3DResource).Description.SizeInBytes;
		}
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBuffer" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create this object.</param>
        /// <param name="name">Name of the buffer.</param>
        /// <param name="settings">Settings for the buffer.</param>
		internal GorgonBuffer(GorgonGraphics graphics, string name, IBufferSettings settings)
			: base(graphics, name, settings)
        {
		}
		#endregion
	}
}
